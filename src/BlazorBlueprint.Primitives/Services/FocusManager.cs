using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Primitives.Services;

/// <summary>
/// Implementation of focus management service using JavaScript interop.
/// </summary>
public class FocusManager : IFocusManager, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FocusManager"/> class.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime for invoking focus management functions.</param>
    public FocusManager(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    // The bundle is shared across the whole circuit, so the cache and the lock that used to live
    // here now live in PrimitiveModules. This service must not dispose what it gets back.
    private Task<IJSObjectReference> GetModuleAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(FocusManager));
        return PrimitiveModules.GetAsync(_jsRuntime);
    }

    /// <inheritdoc />
    public Task<IAsyncDisposable> TrapFocus(ElementReference container)
        => TrapFocus(container, FocusTrapInitialFocus.FirstFocusable);

    /// <inheritdoc />
    public async Task<IAsyncDisposable> TrapFocus(
        ElementReference container,
        FocusTrapInitialFocus initialFocus,
        ElementReference? initialFocusElement = null)
    {
        var module = await GetModuleAsync();
        var cleanupFunction = await module.InvokeAsync<IJSObjectReference>(
            "focusTrap.createFocusTrap", container, ToJsMode(initialFocus), initialFocusElement);
        return new FocusTrapHandle(cleanupFunction);
    }

    private static string ToJsMode(FocusTrapInitialFocus initialFocus) => initialFocus switch
    {
        FocusTrapInitialFocus.Container => "container",
        FocusTrapInitialFocus.None => "none",
        _ => "first",
    };

    /// <inheritdoc />
    public async Task RestoreFocus(ElementReference? previousElement)
    {
        if (previousElement.HasValue)
        {
            try
            {
                // Use Blazor's built-in FocusAsync instead of eval for security
                await previousElement.Value.FocusAsync();
            }
            catch
            {
                // Element may no longer exist, ignore
            }
        }
    }

    /// <inheritdoc />
    public async Task FocusFirst(ElementReference container)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("focusTrap.focusFirst", container);
    }

    /// <inheritdoc />
    public async Task FocusLast(ElementReference container)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("focusTrap.focusLast", container);
    }

    /// <summary>
    /// Disposes the focus manager, releasing JavaScript module resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        GC.SuppressFinalize(this);
        _disposed = true;

        // Nothing to release. The module reference belongs to PrimitiveModules and is shared with
        // every other component on the circuit; Blazor frees it when the circuit ends.
        await Task.CompletedTask;
    }

    private sealed class FocusTrapHandle : IAsyncDisposable
    {
        private readonly IJSObjectReference _cleanupFunction;

        public FocusTrapHandle(IJSObjectReference cleanupFunction)
        {
            _cleanupFunction = cleanupFunction;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await _cleanupFunction.InvokeVoidAsync("apply");
            }
            catch
            {
                // Cleanup function may already be disposed
            }
        }
    }
}

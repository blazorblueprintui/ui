using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Primitives.Services;

/// <summary>
/// Implementation of focus management service using JavaScript interop.
/// </summary>
public class FocusManager : IFocusManager, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    /// <summary>
    /// Initializes a new instance of the <see cref="FocusManager"/> class.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime for invoking focus management functions.</param>
    public FocusManager(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        if (_module == null)
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorBlueprint.Primitives/js/primitives/focus-trap.js");
        }
        return _module;
    }

    /// <inheritdoc />
    public async Task<IAsyncDisposable> TrapFocus(ElementReference container)
    {
        var module = await GetModuleAsync();
        var cleanupFunction = await module.InvokeAsync<IJSObjectReference>("createFocusTrap", container);
        return new FocusTrapHandle(cleanupFunction);
    }

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
        await module.InvokeVoidAsync("focusFirst", container);
    }

    /// <inheritdoc />
    public async Task FocusLast(ElementReference container)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("focusLast", container);
    }

    /// <summary>
    /// Disposes the focus manager, releasing JavaScript module resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (_module != null)
        {
            await _module.DisposeAsync();
        }
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

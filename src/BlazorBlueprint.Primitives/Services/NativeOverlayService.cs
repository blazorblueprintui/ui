using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Primitives.Services;

/// <summary>
/// Default implementation of <see cref="INativeOverlayService"/> backed by the
/// <c>native-dialog.js</c> module.
/// </summary>
public class NativeOverlayService : INativeOverlayService, IAsyncDisposable
{
    private readonly IJSRuntime jsRuntime;
    private readonly OverlayRenderingOptions options;
    private bool? dialogSupported;
    private bool disposed;

    public NativeOverlayService(IJSRuntime jsRuntime, OverlayRenderingOptions options)
    {
        this.jsRuntime = jsRuntime;
        this.options = options;
    }

    // The bundle is shared across the whole circuit, so the cache and the lock that used to live
    // here now live in PrimitiveModules. This service must not dispose what it gets back.
    private Task<IJSObjectReference> GetModuleAsync()
    {
        ObjectDisposedException.ThrowIf(disposed, nameof(NativeOverlayService));
        return PrimitiveModules.GetAsync(jsRuntime);
    }

    /// <inheritdoc />
    public async Task<bool> IsDialogSupportedAsync()
    {
        // Cache only a positive result. A false result may be a transient failure while JS
        // interop is unavailable (prerendering / disconnect); caching it would make the
        // "browser does not support" warning fire forever in a browser that supports it.
        if (dialogSupported == true)
        {
            return true;
        }

        try
        {
            var objectReference = await GetModuleAsync();
            var supported = await objectReference.InvokeAsync<bool>("nativeDialog.supportsNativeDialog");
            if (supported)
            {
                dialogSupported = true;
            }
            return supported;
        }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException or InvalidOperationException)
        {
            // JS interop unavailable (prerendering / disconnect). Assume unsupported for now.
            return false;
        }
    }

    /// <inheritdoc />
    public OverlayRenderingStrategy ResolveStrategy(OverlayRenderingStrategy? requested)
        => requested ?? options.DefaultStrategy;

    /// <inheritdoc />
    public async Task ShowDialogAsync(ElementReference element)
    {
        try
        {
            var objectReference = await GetModuleAsync();
            await objectReference.InvokeVoidAsync("nativeDialog.showModal", element);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException or InvalidOperationException)
        {
            // Expected during prerendering / disconnect.
        }
    }

    /// <inheritdoc />
    public async Task CloseDialogAsync(ElementReference element, string? returnValue = null)
    {
        try
        {
            var objectReference = await GetModuleAsync();
            await objectReference.InvokeVoidAsync("nativeDialog.closeDialog", element, returnValue);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException or InvalidOperationException)
        {
            // Expected during prerendering / disconnect.
        }
    }

    /// <inheritdoc />
    public async Task FocusDialogAsync(ElementReference element)
    {
        try
        {
            var objectReference = await GetModuleAsync();
            await objectReference.InvokeVoidAsync("nativeDialog.focusDialog", element);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException or InvalidOperationException)
        {
            // Expected during prerendering / disconnect.
        }
    }

    /// <inheritdoc />
    public async Task FocusTriggerAsync(ElementReference element)
    {
        try
        {
            var objectReference = await GetModuleAsync();
            await objectReference.InvokeVoidAsync("nativeDialog.focusElement", element);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException or InvalidOperationException)
        {
            // Expected during prerendering / disconnect.
        }
    }

    /// <inheritdoc />
    public async Task<IAsyncDisposable> SetupDialogAsync(ElementReference element, object dotNetRef)
    {
        var objectReference = await GetModuleAsync();
        var cleanup = await objectReference.InvokeAsync<IJSObjectReference>("nativeDialog.setupDialog", element, dotNetRef);
        return new NativeDialogHandle(cleanup);
    }

    private sealed class NativeDialogHandle : IAsyncDisposable
    {
        private readonly IJSObjectReference cleanup;

        public NativeDialogHandle(IJSObjectReference cleanup)
        {
            this.cleanup = cleanup;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await cleanup.InvokeVoidAsync("dispose");
                await cleanup.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException)
            {
                // Cleanup may already be disposed or circuit disconnected.
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return;
        }

        GC.SuppressFinalize(this);
        disposed = true;

        // Nothing to release. The module reference belongs to PrimitiveModules and is shared with
        // every other component on the circuit; Blazor frees it when the circuit ends.
        await Task.CompletedTask;
    }
}

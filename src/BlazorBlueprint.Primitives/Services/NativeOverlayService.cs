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
    private readonly SemaphoreSlim moduleLock = new(1, 1);
    private IJSObjectReference? module;
    private bool? dialogSupported;
    private bool disposed;

    public NativeOverlayService(IJSRuntime jsRuntime, OverlayRenderingOptions options)
    {
        this.jsRuntime = jsRuntime;
        this.options = options;
    }

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        ObjectDisposedException.ThrowIf(disposed, nameof(NativeOverlayService));

        await moduleLock.WaitAsync();
        try
        {
            if (module == null)
            {
                module = await jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/BlazorBlueprint.Primitives/js/primitives/native-dialog.js");
            }
            return module;
        }
        finally
        {
            moduleLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsDialogSupportedAsync()
    {
        if (dialogSupported.HasValue)
        {
            return dialogSupported.Value;
        }

        try
        {
            var objectReference = await GetModuleAsync();
            dialogSupported = await objectReference.InvokeAsync<bool>("supportsNativeDialog");
        }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException or InvalidOperationException)
        {
            // JS interop unavailable (prerendering / disconnect). Assume unsupported for now.
            dialogSupported = false;
        }

        return dialogSupported.Value;
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
            await objectReference.InvokeVoidAsync("showModal", element);
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
            await objectReference.InvokeVoidAsync("closeDialog", element, returnValue);
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
            await objectReference.InvokeVoidAsync("focusDialog", element);
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
            await objectReference.InvokeVoidAsync("focusElement", element);
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
        var cleanup = await objectReference.InvokeAsync<IJSObjectReference>("setupDialog", element, dotNetRef);
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

        if (module != null)
        {
            try
            {
                await module.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect.
            }
        }

        moduleLock.Dispose();
    }
}

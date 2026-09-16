using BlazorBlueprint.Primitives.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

internal sealed class MotionInterop(IJSRuntime js) : IAsyncDisposable
{
    private IJSObjectReference? module;
    private ElementReference element;
    private bool disposed;

    internal async Task InvokeAsync(string method, ElementReference target, params object?[] options)
    {
        if (disposed) { return; }
        try
        {
            element = target;
            module ??= await JsModules.GetAsync(js, JsModules.Versioned("./_content/BlazorBlueprint.Components/js/motion.js", typeof(MotionInterop).Assembly));
            if (disposed) { return; }
            await module.InvokeVoidAsync(method, new object?[] { target }.Concat(options).ToArray());
        }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException or InvalidOperationException) { }
    }

    public async ValueTask DisposeAsync()
    {
        disposed = true;
        if (module is not null)
        {
            try { await module.InvokeVoidAsync("dispose", element); }
            catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException) { }
        }
    }
}

using BlazorBlueprint.Primitives.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

// Owns the actual scroll element inside either portal presentation.
internal sealed class SelectOptionsViewport : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback OnLoadMore { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    private ElementReference element;
    private DotNetObjectReference<SelectOptionsViewport>? reference;
    private IJSObjectReference? module;
    private int observer;
    private bool disposed;
    private bool observing;
    private bool loading;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", Class);
        builder.AddElementReferenceCapture(2, value => element = value);
        builder.AddContent(3, ChildContent);
        builder.CloseElement();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (observer != 0 || observing || !OnLoadMore.HasDelegate || disposed) { return; }
        observing = true;
        try
        {
            module = await PrimitiveModules.GetAsync(JS);
            if (disposed) { return; }
            reference ??= DotNetObjectReference.Create(this);
            observer = await module.InvokeAsync<int>("elementUtils.observeNearBottom", element, reference, nameof(JsOnNearBottom), 80.0);
            if (disposed && observer != 0)
            {
                await module.InvokeVoidAsync("elementUtils.unobserveNearBottom", observer);
                observer = 0;
            }
        }
        catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException or InvalidOperationException) { }
        finally { observing = false; }
    }

    [JSInvokable]
    public async Task JsOnNearBottom()
    {
        if (disposed || IsLoading || loading || !OnLoadMore.HasDelegate) { return; }
        loading = true;
        try { await OnLoadMore.InvokeAsync(); }
        finally { loading = false; }
    }

    public async ValueTask DisposeAsync()
    {
        disposed = true;
        if (module is not null && observer != 0)
        {
            try { await module.InvokeVoidAsync("elementUtils.unobserveNearBottom", observer); }
            catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException) { }
        }
        reference?.Dispose();
    }
}

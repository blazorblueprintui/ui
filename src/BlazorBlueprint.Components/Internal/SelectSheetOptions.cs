using BlazorBlueprint.Primitives.Select;
using BlazorBlueprint.Primitives.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

// Lives inside the sheet's portal, so OnAfterRender runs only once the listbox exists.
internal sealed class SelectSheetOptions<TValue> : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Parameter] public SelectContext<TValue> Context { get; set; } = default!;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    private ElementReference element;
    private DotNetObjectReference<SelectSheetOptions<TValue>>? reference;
    private IJSObjectReference? module;
    private bool disposed;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<SelectContext<TValue>>>(0);
        builder.AddAttribute(1, "Value", Context);
        builder.AddAttribute(2, "ChildContent", (RenderFragment)(content =>
        {
            content.OpenElement(0, "div");
            content.AddAttribute(1, "id", Context.ContentId);
            content.AddAttribute(2, "role", "listbox");
            content.AddAttribute(3, "aria-labelledby", Context.TriggerId);
            content.AddAttribute(4, "tabindex", "-1");
            content.AddAttribute(5, "data-autofocus", true);
            content.AddAttribute(6, "data-select-content", "true");
            content.AddElementReferenceCapture(7, value => element = value);
            content.AddContent(8, ChildContent);
            content.CloseElement();
        }));
        builder.CloseComponent();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) { return; }
        try
        {
            module = await JsModules.GetAsync(JS, JsModules.Versioned("./_content/BlazorBlueprint.Components/js/select-sheet.js", typeof(SelectSheetOptions<TValue>).Assembly));
            if (disposed) { return; }
            reference = DotNetObjectReference.Create(this);
            await module.InvokeVoidAsync("initialize", element, reference, Context.State.Value?.ToString(), Context.State.TriggerElement);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException or InvalidOperationException) { }
    }

    [JSInvokable]
    public void JsOnEscapeKey()
    {
        if (!disposed) { Context.Close(restoreFocus: true); }
    }

    [JSInvokable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Instance callback required by the shared JavaScript listbox handler.")]
    public void JsOnTabKey()
    {
        // The modal sheet's focus trap owns Tab; it must not behave like a floating list.
    }

    public async ValueTask DisposeAsync()
    {
        disposed = true;
        if (module is not null)
        {
            try { await module.InvokeVoidAsync("dispose", element, Context.State.RestoreFocusOnClose ? Context.State.TriggerElement : null); }
            catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException) { }
        }
        reference?.Dispose();
    }
}

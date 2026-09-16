namespace BlazorBlueprint.Components;

internal sealed class SidebarStateSubscription(Action changed) : IDisposable
{
    private SidebarContext? context;
    internal void Observe(SidebarContext? value)
    {
        if (ReferenceEquals(context, value)) { return; }
        Dispose(); context = value;
        context?.StateChanged += OnChanged;
    }
    private void OnChanged(object? sender, EventArgs args) => changed();
    public void Dispose() { context?.StateChanged -= OnChanged; context = null; }
}

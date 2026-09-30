namespace BlazorBlueprint.Components;

/// <summary>
/// Provides control over the toast a component is shown in.
/// This interface is cascaded to everything rendered inside a toast, including components shown with
/// <see cref="ToastService.Show{TComponent}(Dictionary{string, object?}?, ToastData?)"/>.
/// </summary>
public interface IToastReference
{
    /// <summary>
    /// Dismisses the toast.
    /// </summary>
    public Task DismissAsync();
}

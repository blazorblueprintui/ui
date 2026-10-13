using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Service for managing toast notifications.
/// Registered as a scoped service by <c>AddBlazorBlueprintComponents()</c>.
/// </summary>
/// <remarks>
/// Keep it scoped. On Blazor Server a singleton would share every toast, and the render
/// fragments a toast holds, between all connected users.
/// </remarks>
public class ToastService
{
    private readonly List<ToastData> toasts = new();
    private readonly object sync = new();

    /// <summary>
    /// Event fired when the toast collection changes.
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    /// Gets the current list of toasts.
    /// </summary>
    public IReadOnlyList<ToastData> Toasts
    {
        get
        {
            lock (sync)
            {
                return toasts.ToArray();
            }
        }
    }

    /// <summary>
    /// Gets or sets the runtime toast position override.
    /// When set, this takes priority over the ToastProvider's Position parameter.
    /// Set to null to revert to the provider's default.
    /// </summary>
    public ToastPosition? Position { get; private set; }

    /// <summary>
    /// Sets the toast position at runtime.
    /// </summary>
    /// <param name="position">The position to display toasts.</param>
    public void SetPosition(ToastPosition position)
    {
        Position = position;
        OnChange?.Invoke();
    }

    /// <summary>
    /// Resets the toast position to the ToastProvider's default.
    /// </summary>
    public void ResetPosition()
    {
        Position = null;
        OnChange?.Invoke();
    }

    /// <summary>
    /// Shows a toast notification.
    /// </summary>
    /// <param name="toast">The toast data to display.</param>
    public void Show(ToastData toast)
    {
        lock (sync)
        {
            toasts.Add(toast);
        }
        OnChange?.Invoke();
    }

    /// <summary>
    /// Shows a toast whose body is a component, so its content is written as ordinary Razor markup.
    /// </summary>
    /// <remarks>
    /// The component renders in the toast's <see cref="ToastData.Content"/> and receives an
    /// <see cref="IToastReference"/> as a cascading parameter, so it can dismiss its own toast.
    /// </remarks>
    /// <typeparam name="TComponent">The component to show.</typeparam>
    /// <param name="parameters">Parameter values for the component, keyed by parameter name.</param>
    /// <param name="toast">
    /// The toast to show it in, for its variant, duration, position and other settings. Its
    /// <see cref="ToastData.Content"/> is replaced by the component.
    /// </param>
    public void Show<TComponent>(Dictionary<string, object?>? parameters = null, ToastData? toast = null)
        where TComponent : IComponent
    {
        // A copy, so a caller reusing the dictionary can't change a toast that is already showing.
        var values = parameters?.ToArray() ?? [];

        toast ??= new ToastData();
        toast.Content = builder =>
        {
            builder.OpenComponent<TComponent>(0);
            foreach (var (name, value) in values)
            {
                builder.AddComponentParameter(1, name, value);
            }
            builder.CloseComponent();
        };

        Show(toast);
    }

    /// <summary>
    /// Shows a simple toast with a message.
    /// </summary>
    /// <param name="description">The message to display.</param>
    /// <param name="title">Optional title.</param>
    /// <param name="variant">The visual variant.</param>
    /// <param name="duration">Duration in milliseconds (default 5000).</param>
    public void Show(string description, string? title = null, ToastVariant variant = ToastVariant.Default, int duration = 5000)
    {
        Show(new ToastData
        {
            Title = title,
            Description = description,
            Variant = variant,
            Duration = duration
        });
    }

    /// <summary>
    /// Shows a success toast with green accent and circle-check icon.
    /// </summary>
    /// <param name="description">The message to display.</param>
    /// <param name="title">Optional title.</param>
    public void Success(string description, string? title = null) =>
        Show(description, title, ToastVariant.Success);

    /// <summary>
    /// Shows an info toast with blue accent and info icon.
    /// </summary>
    /// <param name="description">The message to display.</param>
    /// <param name="title">Optional title.</param>
    public void Info(string description, string? title = null) =>
        Show(description, title, ToastVariant.Info);

    /// <summary>
    /// Shows a warning toast with amber accent and triangle-alert icon.
    /// </summary>
    /// <param name="description">The message to display.</param>
    /// <param name="title">Optional title.</param>
    public void Warning(string description, string? title = null) =>
        Show(description, title, ToastVariant.Warning);

    /// <summary>
    /// Shows an error toast (destructive variant).
    /// </summary>
    /// <param name="description">The message to display.</param>
    /// <param name="title">Optional title.</param>
    public void Error(string description, string? title = null) =>
        Show(description, title, ToastVariant.Destructive);

    /// <summary>
    /// Dismisses a specific toast by ID.
    /// </summary>
    /// <param name="id">The toast ID to dismiss.</param>
    public void Dismiss(string id)
    {
        bool removed;
        lock (sync)
        {
            var toast = toasts.FirstOrDefault(t => t.Id == id);
            removed = toast != null && toasts.Remove(toast);
        }
        if (removed)
        {
            OnChange?.Invoke();
        }
    }

    /// <summary>
    /// Dismisses all toasts.
    /// </summary>
    public void DismissAll()
    {
        lock (sync)
        {
            toasts.Clear();
        }
        OnChange?.Invoke();
    }
}

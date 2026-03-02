using System.ComponentModel;

namespace BlazorBlueprint.Components;

/// <summary>
/// Service for showing programmatic confirm dialogs.
/// Register as scoped in DI for Blazor Server user isolation.
/// </summary>
public class DialogService
{
    private readonly List<DialogData> dialogs = new();

    /// <summary>
    /// Event fired when the dialog collection changes.
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    /// Gets the current list of pending dialogs.
    /// </summary>
    public IReadOnlyList<DialogData> Dialogs => dialogs.AsReadOnly();

    /// <summary>
    /// Shows an alert dialog with a single acknowledgment button.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="description">Optional dialog message.</param>
    /// <param name="options">Optional customization options.</param>
    /// <returns>A task that completes when the user acknowledges the dialog.</returns>
    public Task Alert(string title, string? description = null, AlertDialogOptions? options = null)
    {
        var data = new AlertDialogData
        {
            Title = title,
            Description = description,
            Options = options ?? new AlertDialogOptions()
        };

        dialogs.Add(data);
        OnChange?.Invoke();

        return data.Completion;
    }

    /// <summary>
    /// Shows a confirm dialog and returns true if confirmed, false if cancelled.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="description">The dialog description/message.</param>
    /// <param name="options">Optional customization options for button labels and variant.</param>
    /// <returns>True if the user clicked confirm, false if cancelled.</returns>
    public Task<bool> Confirm(string title, string? description = null, ConfirmDialogOptions? options = null)
    {
        var data = new ConfirmDialogData
        {
            Title = title,
            Description = description,
            Options = options ?? new ConfirmDialogOptions()
        };

        dialogs.Add(data);
        OnChange?.Invoke();

        return data.Tcs.Task;
    }

    /// <summary>
    /// Opens a custom component inside a dialog.
    /// </summary>
    public Task<DialogResult> Open<TComponent>(DialogOpenOptions? options = null)
        where TComponent : IComponent
        => Open<TComponent>(new Dictionary<string, object?>(), options);

    /// <summary>
    /// Opens a custom component inside a dialog with parameters.
    /// </summary>
    public Task<DialogResult> Open<TComponent>(
        Dictionary<string, object?> parameters,
        DialogOpenOptions? options = null)
        where TComponent : IComponent
    {
        var data = new ComponentDialogData
        {
            Title = options?.Title ?? string.Empty,
            ComponentType = typeof(TComponent),
            Parameters = parameters,
            Options = options ?? new DialogOpenOptions()
        };

        dialogs.Add(data);
        OnChange?.Invoke();

        return data.Tcs.Task;
    }

    public Task<string?> Prompt(string title, string? description = null, PromptDialogOptions? options = null)
    {
        var data = new PromptDialogData
        {
            Title = title,
            Description = description,
            Options = options ?? new PromptDialogOptions()
        };

        dialogs.Add(data);
        OnChange?.Invoke();

        return data.Tcs.Task;
    }

    /// <summary>
    /// Cancels all pending dialogs, resolving each with false.
    /// </summary>
    internal void CancelAll()
    {
        foreach (var dialog in dialogs.ToList())
        {
            dialog.SetResult(null);
        }

        dialogs.Clear();
        OnChange?.Invoke();
    }

    /// <summary>
    /// Resolves a dialog with the given result and removes it from the list.
    /// Called by DialogProvider when the user clicks confirm or cancel.
    /// </summary>
    /// <param name="id">The dialog ID to resolve.</param>
    /// <param name="result">True for confirm, false for cancel.</param>
    internal void Resolve<TResult>(string id, TResult? result)
    {
        var dialog = dialogs.FirstOrDefault(d => d.Id == id);
        if (dialog != null)
        {
            dialogs.Remove(dialog);
            dialog.SetResult(result);
            OnChange?.Invoke();
        }
    }
}


using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Provides functionality for showing and managing dialogs.
/// </summary>
/// <remarks>
/// Register as scoped in DI for proper isolation in Blazor Server applications.
/// All dialog operations resolve with a <see cref="DialogResult"/>.
/// </remarks>
public class DialogService
{
    private readonly List<DialogData> dialogs = new();

    /// <summary>
    /// Occurs when the dialog collection changes.
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    /// Gets the current list of pending dialogs.
    /// </summary>
    public IReadOnlyList<DialogData> Dialogs => dialogs.AsReadOnly();

    /// <summary>
    /// Shows an alert dialog with a single acknowledgment button.
    /// </summary>
    /// <returns>
    /// A task that completes when the user dismisses the dialog.
    /// </returns>
    public Task<DialogResult> AlertAsync(
        string title,
        string? description = null,
        AlertDialogOptions? options = null)
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
    /// Shows a confirm dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="description">Optional dialog description or message.</param>
    /// <param name="options">Optional customization options.</param>
    /// <returns>
    /// A task that resolves to a <see cref="ConfirmDialogResult"/>
    /// indicating whether the user confirmed the action.
    /// </returns>
    public async Task<ConfirmDialogResult> ConfirmAsync(
        string title,
        string? description = null,
        ConfirmDialogOptions? options = null)
    {
        var data = new ConfirmDialogData
        {
            Title = title,
            Description = description,
            Options = options ?? new ConfirmDialogOptions()
        };

        dialogs.Add(data);
        OnChange?.Invoke();

        var result = await data.Completion;
        return new ConfirmDialogResult(result);
    }


    /// <summary>
    /// Shows a confirm dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="description">Optional dialog description or message.</param>
    /// <param name="options">Optional customization options.</param>
    /// <returns>
    /// A task that resolves to <see langword="true"/> if the user confirmed, <see langword="false"/> otherwise.
    /// </returns>
    [Obsolete("Use ConfirmAsync instead, which returns a ConfirmDialogResult with richer information.")]
    public async Task<bool> Confirm(
        string title,
        string? description = null,
        ConfirmDialogOptions? options = null)
    {
        var result = await ConfirmAsync(title, description, options);
        return result.Confirmed;
    }

    /// <summary>
    /// Shows a prompt dialog that collects text input from the user.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="description">Optional dialog description or message.</param>
    /// <param name="options">Optional customization options.</param>
    /// <returns>
    /// A task that resolves to a <see cref="PromptDialogResult"/>
    /// containing the entered value when confirmed.
    /// </returns>
    public async Task<PromptDialogResult> PromptAsync(
        string title,
        string? description = null,
        PromptDialogOptions? options = null)
    {
        var data = new PromptDialogData
        {
            Title = title,
            Description = description,
            Options = options ?? new PromptDialogOptions()
        };

        dialogs.Add(data);
        OnChange?.Invoke();

        var result = await data.Completion;
        return new PromptDialogResult(result);
    }

    /// <summary>
    /// Opens a custom component inside a dialog.
    /// </summary>
    public Task<DialogResult> OpenAsync<TComponent>(
        DialogOpenOptions? options = null)
        where TComponent : IComponent
        => OpenAsync<TComponent>(new Dictionary<string, object?>(), options);

    /// <summary>
    /// Opens a custom component inside a dialog with the specified parameters.
    /// </summary>
    public Task<DialogResult> OpenAsync<TComponent>(
        Dictionary<string, object?> parameters,
        DialogOpenOptions? options = null)
        where TComponent : IComponent
    {
        var id = Guid.NewGuid().ToString();
        var data = new ComponentDialogData
        {
            Id = id,
            Title = options?.Title ?? string.Empty,
            Description = options?.Description,
            ComponentType = typeof(TComponent),
            Parameters = parameters,
            Options = options ?? new DialogOpenOptions(),
            CloseCallback = result =>
            {
                Resolve(id, result);
                return Task.CompletedTask;
            }
        };

        dialogs.Add(data);
        OnChange?.Invoke();

        return data.Completion;
    }

    /// <summary>
    /// Resolves a dialog with the specified result and removes it from the collection.
    /// </summary>
    internal void Resolve(string id, DialogResult result)
    {
        var dialog = dialogs.FirstOrDefault(d => d.Id == id);
        if (dialog is null)
        {
            return;
        }

        dialogs.Remove(dialog);
        dialog.SetResult(result);
        OnChange?.Invoke();
    }

    /// <summary>
    /// Cancels all pending dialogs.
    /// </summary>
    internal void CancelAll()
    {
        foreach (var dialog in dialogs.ToList())
        {
            dialog.SetResult(DialogResult.Cancel());
        }

        dialogs.Clear();
        OnChange?.Invoke();
    }
}

using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Primitives.Services;

/// <summary>
/// Service for registering and managing global keyboard shortcuts.
/// Shortcuts work application-wide and fire even when menus are closed.
/// </summary>
public interface IKeyboardShortcutService : IAsyncDisposable
{
    /// <summary>
    /// Registers a keyboard shortcut that invokes a callback when pressed.
    /// </summary>
    /// <param name="shortcut">The shortcut string (e.g., "Ctrl+N", "Shift+Delete", "Ctrl+Shift+S").</param>
    /// <param name="callback">The async callback to invoke when the shortcut is pressed.</param>
    /// <returns>A disposable handle to unregister the shortcut.</returns>
    /// <example>
    /// <code>
    /// var registration = await KeyboardShortcuts.RegisterAsync("Ctrl+S", async () => await SaveAsync());
    /// // Later, to unregister:
    /// registration.Dispose();
    /// </code>
    /// </example>
    public Task<IDisposable> RegisterAsync(string shortcut, Func<Task> callback);

    /// <summary>
    /// Registers a keyboard shortcut with a unique identifier for later unregistration.
    /// </summary>
    /// <param name="shortcut">The shortcut string (e.g., "Ctrl+N").</param>
    /// <param name="callback">The async callback to invoke when the shortcut is pressed.</param>
    /// <param name="id">A unique identifier for this registration.</param>
    /// <returns>A disposable handle to unregister the shortcut.</returns>
    public Task<IDisposable> RegisterAsync(string shortcut, Func<Task> callback, string id);

    /// <summary>
    /// Unregisters a shortcut by its unique identifier.
    /// </summary>
    /// <param name="id">The identifier used when registering the shortcut.</param>
    public void Unregister(string id);

    /// <summary>
    /// Temporarily suspends all keyboard shortcuts.
    /// Useful when opening modal dialogs or other scenarios where shortcuts should be disabled.
    /// </summary>
    public void Suspend();

    /// <summary>
    /// Resumes keyboard shortcuts after suspension.
    /// </summary>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Resume is the correct domain term")]
    public void Resume();

    /// <summary>
    /// Gets whether shortcuts are currently suspended.
    /// </summary>
    public bool IsSuspended { get; }
}

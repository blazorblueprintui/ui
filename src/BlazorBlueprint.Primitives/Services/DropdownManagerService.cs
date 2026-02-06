namespace BlazorBlueprint.Primitives.Services;

/// <summary>
/// Interface for managing dropdown components globally to ensure only one can be open at a time.
/// </summary>
public interface IDropdownManagerService
{
    /// <summary>
    /// Registers a dropdown as open. If another dropdown is already open, it will be closed first.
    /// </summary>
    /// <param name="dropdownId">Unique identifier for the dropdown (should be stable across renders)</param>
    /// <param name="closeCallback">Callback to invoke when this dropdown needs to be closed by the manager</param>
    public void RegisterOpen(string dropdownId, Action closeCallback);

    /// <summary>
    /// Unregisters a dropdown when it closes naturally (via user action or click-outside).
    /// </summary>
    /// <param name="dropdownId">The dropdown ID to unregister</param>
    public void Unregister(string dropdownId);

    /// <summary>
    /// Gets whether a specific dropdown is currently registered as open.
    /// </summary>
    /// <param name="dropdownId">The dropdown ID to check</param>
    /// <returns>True if the dropdown is currently registered as open</returns>
    public bool IsOpen(string dropdownId);
}

/// <summary>
/// Manages dropdown components globally to ensure only one can be open at a time.
/// </summary>
/// <remarks>
/// This service should be registered as scoped in the DI container.
/// When a dropdown (Select, Combobox, DropdownMenu, Popover, etc.) opens,
/// it registers itself with this service. If another dropdown is already open,
/// the service automatically closes it before registering the new one.
///
/// Thread-safe for use in Blazor Server where multiple async operations may access the service concurrently.
/// </remarks>
public class DropdownManagerService : IDropdownManagerService
{
    private readonly object _lock = new object();
    private string? _currentDropdownId;
    private Action? _currentCloseCallback;

    /// <summary>
    /// Registers a dropdown as open. If another dropdown is already open, it will be closed first.
    /// </summary>
    /// <param name="dropdownId">Unique identifier for the dropdown (should be stable across renders)</param>
    /// <param name="closeCallback">Callback to invoke when this dropdown needs to be closed by the manager</param>
    /// <exception cref="ArgumentException">Thrown when dropdownId is null, empty, or contains invalid characters.</exception>
    public void RegisterOpen(string dropdownId, Action closeCallback)
    {
        // Validate input to prevent state tampering
        if (string.IsNullOrWhiteSpace(dropdownId))
        {
            throw new ArgumentException("Dropdown ID cannot be null or empty.", nameof(dropdownId));
        }

        ArgumentNullException.ThrowIfNull(closeCallback);

        // Basic sanitization - IDs should only contain safe characters
        if (!IsValidDropdownId(dropdownId))
        {
            throw new ArgumentException("Dropdown ID contains invalid characters.", nameof(dropdownId));
        }

        lock (_lock)
        {
            // Close the currently open dropdown if it's different from the one being registered
            if (_currentDropdownId != null && _currentDropdownId != dropdownId)
            {
                _currentCloseCallback?.Invoke();
            }

            _currentDropdownId = dropdownId;
            _currentCloseCallback = closeCallback;
        }
    }

    /// <summary>
    /// Validates that a dropdown ID contains only safe characters.
    /// </summary>
    private static bool IsValidDropdownId(string id)
    {
        // Allow alphanumeric, hyphens, underscores, and colons (common in generated IDs)
        foreach (var c in id)
        {
            if (!char.IsLetterOrDigit(c) && c != '-' && c != '_' && c != ':')
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Unregisters a dropdown when it closes naturally (via user action or click-outside).
    /// </summary>
    /// <param name="dropdownId">The dropdown ID to unregister</param>
    public void Unregister(string dropdownId)
    {
        lock (_lock)
        {
            if (_currentDropdownId == dropdownId)
            {
                _currentDropdownId = null;
                _currentCloseCallback = null;
            }
        }
    }

    /// <summary>
    /// Gets whether a specific dropdown is currently registered as open.
    /// </summary>
    /// <param name="dropdownId">The dropdown ID to check</param>
    /// <returns>True if the dropdown is currently registered as open</returns>
    public bool IsOpen(string dropdownId)
    {
        lock (_lock)
        {
            return _currentDropdownId == dropdownId;
        }
    }
}

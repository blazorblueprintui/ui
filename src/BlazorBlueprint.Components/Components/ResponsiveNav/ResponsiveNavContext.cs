namespace BlazorBlueprint.Components.ResponsiveNav;

/// <summary>
/// State container for the responsive navigation component.
/// </summary>
public class ResponsiveNavState
{
    /// <summary>
    /// Whether the mobile menu is open.
    /// </summary>
    public bool OpenMobile { get; set; }

    /// <summary>
    /// Whether the current viewport is mobile.
    /// </summary>
    public bool IsMobile { get; set; }
}

/// <summary>
/// Context for managing responsive navigation state.
/// Provides state management and responsive behavior.
/// </summary>
public class ResponsiveNavContext
{
    private ResponsiveNavState _state = new();

    /// <summary>
    /// Gets the current navigation state.
    /// </summary>
    public ResponsiveNavState State => _state;

    /// <summary>
    /// Gets whether the mobile menu is currently open.
    /// </summary>
    public bool OpenMobile => _state.OpenMobile;

    /// <summary>
    /// Gets whether the current viewport is mobile.
    /// </summary>
    public bool IsMobile => _state.IsMobile;

    /// <summary>
    /// Event raised when the navigation state changes.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// Toggles the mobile menu open/closed state.
    /// </summary>
    public void Toggle() =>
        SetOpenMobile(!_state.OpenMobile);

    /// <summary>
    /// Sets the mobile menu open state.
    /// </summary>
    public void SetOpenMobile(bool open)
    {
        if (_state.OpenMobile != open)
        {
            _state.OpenMobile = open;
            OnStateChanged();
        }
    }

    /// <summary>
    /// Sets whether the viewport is mobile.
    /// Called by ResponsiveNavProvider via JS interop.
    /// </summary>
    public void SetIsMobile(bool isMobile)
    {
        if (_state.IsMobile != isMobile)
        {
            _state.IsMobile = isMobile;
            // Close mobile menu when switching to desktop
            if (!isMobile && _state.OpenMobile)
            {
                _state.OpenMobile = false;
            }
            OnStateChanged();
        }
    }

    private void OnStateChanged() =>
        StateChanged?.Invoke(this, EventArgs.Empty);
}

using BlazorBlueprint.Primitives.Contexts;

namespace BlazorBlueprint.Primitives.Tabs;

/// <summary>
/// State for the Tabs primitive context.
/// </summary>
public class TabsState
{
    /// <summary>
    /// Gets or sets the currently active tab value.
    /// </summary>
    public string? ActiveValue { get; set; }

    /// <summary>
    /// Gets or sets the orientation of the tabs.
    /// </summary>
    public TabsOrientation Orientation { get; set; } = TabsOrientation.Horizontal;

    /// <summary>
    /// Gets or sets whether tab activation is automatic on focus or manual on click.
    /// </summary>
    public TabsActivationMode ActivationMode { get; set; } = TabsActivationMode.Automatic;
}

/// <summary>
/// Orientation of the tabs list.
/// </summary>
public enum TabsOrientation
{
    /// <summary>
    /// Tabs are arranged horizontally.
    /// </summary>
    Horizontal,

    /// <summary>
    /// Tabs are arranged vertically.
    /// </summary>
    Vertical
}

/// <summary>
/// Activation mode for tabs.
/// </summary>
public enum TabsActivationMode
{
    /// <summary>
    /// Tabs activate automatically when focused with arrow keys.
    /// </summary>
    Automatic,

    /// <summary>
    /// Tabs must be clicked or Enter/Space pressed to activate.
    /// </summary>
    Manual
}

/// <summary>
/// Context for Tabs primitive component and its children.
/// Manages tab state and provides IDs for ARIA attributes.
/// </summary>
public class TabsContext : PrimitiveContextWithEvents<TabsState>
{
    /// <summary>
    /// Initializes a new instance of the TabsContext.
    /// </summary>
    public TabsContext() : base(new TabsState(), "tabs")
    {
    }

    /// <summary>
    /// Gets the ID for the tabs list container.
    /// </summary>
    public string ListId => GetScopedId("list");

    /// <summary>
    /// Gets the ID for a specific tab trigger.
    /// </summary>
    public string GetTriggerId(string value) => GetScopedId($"trigger-{value}");

    /// <summary>
    /// Gets the ID for a specific tab content panel.
    /// </summary>
    public string GetContentId(string value) => GetScopedId($"content-{value}");

    /// <summary>
    /// Gets the currently active tab value.
    /// </summary>
    public string? ActiveValue => State.ActiveValue;

    /// <summary>
    /// Gets the orientation of the tabs.
    /// </summary>
    public TabsOrientation Orientation => State.Orientation;

    /// <summary>
    /// Gets the activation mode for tabs.
    /// </summary>
    public TabsActivationMode ActivationMode => State.ActivationMode;

    /// <summary>
    /// Sets the active tab.
    /// </summary>
    /// <param name="value">The value of the tab to activate.</param>
    public void SetActiveTab(string value) => UpdateState(state => state.ActiveValue = value);

    /// <summary>
    /// Checks if the specified tab is currently active.
    /// </summary>
    /// <param name="value">The tab value to check.</param>
    /// <returns>True if the tab is active, otherwise false.</returns>
    public bool IsTabActive(string value) => State.ActiveValue == value;
}

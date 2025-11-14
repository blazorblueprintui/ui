using BlazorUI.Primitives.Contexts;

namespace BlazorUI.Primitives.Accordion;

/// <summary>
/// State for the Accordion primitive context.
/// </summary>
public class AccordionState
{
    /// <summary>
    /// Gets or sets the currently open item value (single mode) or values (multiple mode).
    /// </summary>
    public HashSet<string> OpenValues { get; set; } = new();

    /// <summary>
    /// Gets or sets the accordion type (single or multiple).
    /// </summary>
    public AccordionType Type { get; set; } = AccordionType.Single;

    /// <summary>
    /// Gets or sets whether items can be collapsed in single mode.
    /// </summary>
    public bool Collapsible { get; set; } = false;
}

/// <summary>
/// Type of accordion behavior.
/// </summary>
public enum AccordionType
{
    /// <summary>
    /// Only one item can be open at a time.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple items can be open simultaneously.
    /// </summary>
    Multiple
}

/// <summary>
/// Context for Accordion primitive component and its children.
/// Manages accordion state and provides IDs for ARIA attributes.
/// </summary>
public class AccordionContext : PrimitiveContextWithEvents<AccordionState>
{
    /// <summary>
    /// Initializes a new instance of the AccordionContext.
    /// </summary>
    public AccordionContext() : base(new AccordionState(), "accordion")
    {
    }

    /// <summary>
    /// Gets the ID for a specific accordion item header.
    /// </summary>
    public string GetHeaderId(string value) => GetScopedId($"header-{value}");

    /// <summary>
    /// Gets the ID for a specific accordion item trigger.
    /// </summary>
    public string GetTriggerId(string value) => GetScopedId($"trigger-{value}");

    /// <summary>
    /// Gets the ID for a specific accordion item content region.
    /// </summary>
    public string GetContentId(string value) => GetScopedId($"content-{value}");

    /// <summary>
    /// Gets the accordion type.
    /// </summary>
    public AccordionType Type => State.Type;

    /// <summary>
    /// Gets whether the accordion is collapsible in single mode.
    /// </summary>
    public bool Collapsible => State.Collapsible;

    /// <summary>
    /// Checks if the specified item is currently open.
    /// </summary>
    /// <param name="value">The item value to check.</param>
    /// <returns>True if the item is open, otherwise false.</returns>
    public bool IsItemOpen(string value)
    {
        return State.OpenValues.Contains(value);
    }

    /// <summary>
    /// Toggles an accordion item open or closed.
    /// </summary>
    /// <param name="value">The value of the item to toggle.</param>
    public void ToggleItem(string value)
    {
        UpdateState(state =>
        {
            if (state.Type == AccordionType.Single)
            {
                // Single mode: close others and toggle this one
                if (state.OpenValues.Contains(value))
                {
                    // Only allow collapsing if Collapsible is true
                    if (state.Collapsible)
                    {
                        state.OpenValues.Clear();
                    }
                }
                else
                {
                    state.OpenValues.Clear();
                    state.OpenValues.Add(value);
                }
            }
            else
            {
                // Multiple mode: toggle independently
                if (state.OpenValues.Contains(value))
                {
                    state.OpenValues.Remove(value);
                }
                else
                {
                    state.OpenValues.Add(value);
                }
            }
        });
    }

    /// <summary>
    /// Sets the open items. Used for controlled state.
    /// </summary>
    /// <param name="values">The set of open item values.</param>
    public void SetOpenItems(HashSet<string> values)
    {
        UpdateState(state =>
        {
            state.OpenValues = new HashSet<string>(values);
        });
    }
}

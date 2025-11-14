namespace BlazorUI.Primitives.Utilities;

/// <summary>
/// Fluent builder for constructing ARIA attributes for accessible components.
/// Provides a type-safe way to add ARIA attributes to Blazor components.
/// </summary>
public class AriaBuilder
{
    private readonly Dictionary<string, object?> _attributes = new();

    /// <summary>
    /// Sets the ARIA role attribute.
    /// </summary>
    /// <param name="role">The role value (e.g., "dialog", "button", "menu").</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder Role(string role)
    {
        return Set("role", role);
    }

    /// <summary>
    /// Sets the aria-label attribute.
    /// </summary>
    /// <param name="label">The accessible label text.</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder Label(string? label)
    {
        return Set("aria-label", label);
    }

    /// <summary>
    /// Sets the aria-labelledby attribute.
    /// </summary>
    /// <param name="id">The ID of the labeling element.</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder LabelledBy(string? id)
    {
        return Set("aria-labelledby", id);
    }

    /// <summary>
    /// Sets the aria-describedby attribute.
    /// </summary>
    /// <param name="id">The ID of the describing element.</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder DescribedBy(string? id)
    {
        return Set("aria-describedby", id);
    }

    /// <summary>
    /// Sets the aria-expanded attribute (for expandable elements).
    /// </summary>
    /// <param name="expanded">Whether the element is expanded.</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder Expanded(bool? expanded)
    {
        return Set("aria-expanded", expanded?.ToString().ToLower());
    }

    /// <summary>
    /// Sets the aria-hidden attribute.
    /// </summary>
    /// <param name="hidden">Whether the element is hidden from screen readers.</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder Hidden(bool? hidden)
    {
        return Set("aria-hidden", hidden?.ToString().ToLower());
    }

    /// <summary>
    /// Sets the aria-modal attribute (for modal dialogs).
    /// </summary>
    /// <param name="modal">Whether the element is a modal.</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder Modal(bool modal)
    {
        return Set("aria-modal", modal.ToString().ToLower());
    }

    /// <summary>
    /// Sets the aria-disabled attribute.
    /// </summary>
    /// <param name="disabled">Whether the element is disabled.</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder Disabled(bool? disabled)
    {
        return Set("aria-disabled", disabled?.ToString().ToLower());
    }

    /// <summary>
    /// Sets the aria-checked attribute (for checkboxes and radio buttons).
    /// </summary>
    /// <param name="checked">The checked state: true, false, or "mixed".</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder Checked(bool? @checked)
    {
        return Set("aria-checked", @checked?.ToString().ToLower());
    }

    /// <summary>
    /// Sets the aria-checked attribute to "mixed" for indeterminate state.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder CheckedMixed()
    {
        return Set("aria-checked", "mixed");
    }

    /// <summary>
    /// Sets the aria-selected attribute (for selectable items).
    /// </summary>
    /// <param name="selected">Whether the element is selected.</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder Selected(bool? selected)
    {
        return Set("aria-selected", selected?.ToString().ToLower());
    }

    /// <summary>
    /// Sets the aria-current attribute (for navigation items).
    /// </summary>
    /// <param name="current">The current state (e.g., "page", "step", "location", "true", "false").</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder Current(string? current)
    {
        return Set("aria-current", current);
    }

    /// <summary>
    /// Sets the aria-haspopup attribute.
    /// </summary>
    /// <param name="hasPopup">The type of popup (e.g., "true", "menu", "listbox", "tree", "grid", "dialog").</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder HasPopup(string? hasPopup)
    {
        return Set("aria-haspopup", hasPopup);
    }

    /// <summary>
    /// Sets the aria-controls attribute.
    /// </summary>
    /// <param name="id">The ID of the controlled element.</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder Controls(string? id)
    {
        return Set("aria-controls", id);
    }

    /// <summary>
    /// Sets the aria-live attribute (for live regions).
    /// </summary>
    /// <param name="live">The live region politeness: "off", "polite", or "assertive".</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder Live(string? live)
    {
        return Set("aria-live", live);
    }

    /// <summary>
    /// Sets the aria-orientation attribute.
    /// </summary>
    /// <param name="orientation">The orientation: "horizontal" or "vertical".</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder Orientation(string? orientation)
    {
        return Set("aria-orientation", orientation);
    }

    /// <summary>
    /// Sets the aria-valuemin attribute (for range widgets).
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder ValueMin(double? min)
    {
        return Set("aria-valuemin", min?.ToString());
    }

    /// <summary>
    /// Sets the aria-valuemax attribute (for range widgets).
    /// </summary>
    /// <param name="max">The maximum value.</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder ValueMax(double? max)
    {
        return Set("aria-valuemax", max?.ToString());
    }

    /// <summary>
    /// Sets the aria-valuenow attribute (for range widgets).
    /// </summary>
    /// <param name="now">The current value.</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder ValueNow(double? now)
    {
        return Set("aria-valuenow", now?.ToString());
    }

    /// <summary>
    /// Sets the aria-valuetext attribute (for range widgets).
    /// </summary>
    /// <param name="text">Human-readable text alternative for the value.</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder ValueText(string? text)
    {
        return Set("aria-valuetext", text);
    }

    /// <summary>
    /// Sets a custom ARIA or data attribute.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">The attribute value.</param>
    /// <returns>The builder for chaining.</returns>
    public AriaBuilder Set(string name, object? value)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _attributes[name] = value;
        }
        return this;
    }

    /// <summary>
    /// Builds the final dictionary of attributes, excluding null values.
    /// </summary>
    /// <returns>Dictionary suitable for @attributes in Blazor components.</returns>
    public Dictionary<string, object> Build()
    {
        return _attributes
            .Where(x => x.Value != null)
            .ToDictionary(x => x.Key, x => x.Value!);
    }

    /// <summary>
    /// Builds the attributes and merges with additional attributes.
    /// </summary>
    /// <param name="additionalAttributes">Additional attributes to merge.</param>
    /// <returns>Merged dictionary of attributes.</returns>
    public Dictionary<string, object> Build(IDictionary<string, object>? additionalAttributes)
    {
        var result = Build();

        if (additionalAttributes != null)
        {
            foreach (var kvp in additionalAttributes)
            {
                // Additional attributes take precedence
                result[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }
}

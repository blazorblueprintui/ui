using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A child component for MultiSelect compositional mode that renders a selectable checkbox item
/// with rich content support. Must be used within a MultiSelect component.
/// </summary>
/// <typeparam name="TValue">The type of the item value, matching the parent MultiSelect.</typeparam>
public partial class BbMultiSelectItem<TValue> : ComponentBase, IDisposable
{
    [CascadingParameter]
    private BbMultiSelect<TValue>? Parent { get; set; }

    /// <summary>
    /// Gets or sets the value of this item.
    /// </summary>
    [Parameter, EditorRequired]
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets the display text for this item.
    /// Used for search filtering and badge display text lookup.
    /// </summary>
    [Parameter, EditorRequired]
    public string Text { get; set; } = default!;

    /// <summary>
    /// Gets or sets whether this item is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the child content for custom item rendering.
    /// When null, renders the Text parameter as plain text.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private bool IsSelected => Parent?.IsValueSelected(Value) ?? false;

    // CSS class accessors — avoid namespace/class collision in .razor template
    private static string ItemCssClassValue => BbMultiSelect<TValue>.ItemCssClass;
    private static string CheckboxCssClassValue => BbMultiSelect<TValue>.CheckboxCssClass;

    private bool IsVisible
    {
        get
        {
            if (Parent is null)
            {
                return false;
            }

            // When external filtering is active, the consumer controls which items
            // are rendered — show all of them without internal text filtering.
            if (Parent.IsExternalFiltering)
            {
                return true;
            }

            var query = Parent.SearchQuery;
            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }

            return Text.Contains(query, StringComparison.OrdinalIgnoreCase);
        }
    }

    protected override void OnInitialized()
    {
        if (Parent is null)
        {
            throw new InvalidOperationException(
                $"{nameof(BbMultiSelectItem<TValue>)} must be used within a {nameof(BbMultiSelect<TValue>)} component.");
        }

        Parent.RegisterItem(Value, Text);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        Parent?.RegisterItem(Value, Text);
    }

    private async Task HandleClick()
    {
        if (Parent is not null && !Disabled)
        {
            await Parent.ToggleItemValue(Value);
        }
    }

    // Do NOT unregister from the text registry here.
    // Items live inside a portal that unmounts when the popover closes,
    // but the registry is needed for badge display text while closed.
    // Items re-register on next open via OnInitialized.
    public void Dispose() => GC.SuppressFinalize(this);
}

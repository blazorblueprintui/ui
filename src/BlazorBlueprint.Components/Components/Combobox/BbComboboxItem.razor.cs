using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A child component for Combobox compositional mode that renders a selectable item
/// with rich content support. Must be used within a Combobox component.
/// </summary>
/// <typeparam name="TValue">The type of the item value, matching the parent Combobox.</typeparam>
public partial class BbComboboxItem<TValue> : ComponentBase, IDisposable
{
    [CascadingParameter]
    private BbCombobox<TValue>? Parent { get; set; }

    /// <summary>
    /// When true, this item is part of the parent's hidden registration pass: it registers
    /// its display text with the parent but renders no DOM. The parent renders the items a
    /// second time, eagerly and invisibly, so their captions are known on initial load —
    /// before the popover (and therefore the real, interactive items) has ever mounted.
    /// </summary>
    [CascadingParameter(Name = BbComboboxConstants.RegistrationScopeName)]
    private bool RegistrationOnly { get; set; }

    /// <summary>
    /// Gets or sets the value of this item.
    /// </summary>
    [Parameter, EditorRequired]
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets the display text for this item.
    /// Used for search filtering and trigger display text lookup.
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

    protected override void OnInitialized()
    {
        if (Parent is null)
        {
            throw new InvalidOperationException(
                $"{nameof(BbComboboxItem<TValue>)} must be used within a {nameof(BbCombobox<TValue>)} component.");
        }

        Parent.RegisterItem(Value, Text);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        Parent?.RegisterItem(Value, Text);
    }

    private async Task HandleSelect() =>
        await Parent!.HandleItemSelected(Value, Text);

    // Do NOT unregister from the text registry here.
    // Items live inside a portal that unmounts when the popover closes,
    // but the registry is needed for trigger display text while closed.
    // Items re-register on next open via OnInitialized.
    public void Dispose() => GC.SuppressFinalize(this);
}

using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// An individual checkbox item within a CheckboxGroup.
/// </summary>
public partial class BbCheckboxGroupItem<TValue> : ComponentBase, IDisposable
{
    [CascadingParameter]
    internal CheckboxGroupContext<TValue>? Context { get; set; }

    /// <summary>
    /// Gets or sets the value this item represents.
    /// </summary>
    [Parameter, EditorRequired]
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets an optional label to display next to the checkbox.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets whether this individual item is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets custom content to render instead of the Label parameter.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private readonly string itemId = $"cbg-{Guid.NewGuid():N}";
    private bool IsChecked => Context?.SelectedValues.Contains(Value) ?? false;
    private bool IsDisabled => Disabled || (Context?.IsDisabled ?? false);

    private string CssClass => ClassNames.cn("flex items-center gap-2", Class);

    protected override void OnInitialized() => Context?.RegisterItem(Value);

    private async Task HandleCheckedChanged(bool isChecked)
    {
        if (Context != null)
        {
            await Context.ToggleItem(Value, isChecked);
        }
    }

    public void Dispose()
    {
        Context?.UnregisterItem(Value);
        GC.SuppressFinalize(this);
    }
}

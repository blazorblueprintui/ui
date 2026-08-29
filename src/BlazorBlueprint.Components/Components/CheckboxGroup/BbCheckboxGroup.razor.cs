using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A group component that manages a collection of checkboxes with optional select-all support.
/// </summary>
public partial class BbCheckboxGroup<TValue> : ComponentBase
{
    /// <summary>
    /// Gets or sets the currently selected values.
    /// </summary>
    [Parameter]
    public IReadOnlyCollection<TValue> Values { get; set; } = Array.Empty<TValue>();

    /// <summary>
    /// Gets or sets the callback invoked when selected values change.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyCollection<TValue>> ValuesChanged { get; set; }

    /// <summary>
    /// Gets or sets whether to show a "Select all" checkbox.
    /// </summary>
    [Parameter]
    public bool ShowSelectAll { get; set; }

    /// <summary>
    /// Gets or sets the label for the select-all checkbox.
    /// </summary>
    [Parameter]
    public string SelectAllLabel { get; set; } = "Select all";

    /// <summary>
    /// Gets or sets whether all items in the group are disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes for the group container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the child content (CheckboxGroupItem components).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private CheckboxGroupContext<TValue> context = default!;
    private readonly HashSet<TValue> registeredItems = new();

    private string CssClass => ClassNames.cn("grid gap-3", Class);

    private bool IsAllSelected => registeredItems.Count > 0
        && Values.Count >= registeredItems.Count
        && registeredItems.All(v => Values.Contains(v));

    private bool IsIndeterminate => Values.Count > 0 && !IsAllSelected;

    protected override void OnParametersSet()
    {
        context = new CheckboxGroupContext<TValue>
        {
            SelectedValues = Values,
            IsDisabled = Disabled,
            ToggleItem = ToggleItemAsync,
            RegisterItem = RegisterItem,
            UnregisterItem = UnregisterItem
        };
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && ShowSelectAll && registeredItems.Count > 0)
        {
            StateHasChanged();
        }
    }

    private void RegisterItem(TValue value) => registeredItems.Add(value);

    private void UnregisterItem(TValue value) => registeredItems.Remove(value);

    private async Task ToggleItemAsync(TValue value, bool isChecked)
    {
        var newValues = new List<TValue>(Values);
        if (isChecked)
        {
            if (!newValues.Contains(value))
            {
                newValues.Add(value);
            }
        }
        else
        {
            newValues.Remove(value);
        }
        await ValuesChanged.InvokeAsync(newValues.AsReadOnly());
    }

    private async Task HandleSelectAllChanged(bool isChecked)
    {
        if (isChecked)
        {
            await ValuesChanged.InvokeAsync(registeredItems.ToList().AsReadOnly());
        }
        else
        {
            await ValuesChanged.InvokeAsync(Array.Empty<TValue>());
        }
    }
}

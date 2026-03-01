using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Container component for building data filter expressions.
/// Manages a tree of filter conditions and groups with AND/OR logic.
/// Supports two-way binding via <c>@bind-Filter</c>.
/// </summary>
public partial class BbFilterBuilder : ComponentBase, IDisposable
{
    private FilterBuilderContext? context;
    private Timer? debounceTimer;
    private int debounceVersion;
    private bool disposed;

    /// <summary>
    /// Gets or sets the current filter state. Supports two-way binding via <c>@bind-Filter</c>.
    /// </summary>
    [Parameter]
    public FilterDefinition Filter { get; set; } = new();

    /// <summary>
    /// Gets or sets the callback invoked when the filter changes (two-way binding).
    /// </summary>
    [Parameter]
    public EventCallback<FilterDefinition> FilterChanged { get; set; }

    /// <summary>
    /// Gets or sets the available fields to filter on.
    /// </summary>
    [Parameter]
    public IEnumerable<FilterField> Fields { get; set; } = Enumerable.Empty<FilterField>();

    /// <summary>
    /// Gets or sets the callback invoked when the filter changes (for side effects like applying filters).
    /// </summary>
    [Parameter]
    public EventCallback<FilterDefinition> OnFilterChanged { get; set; }

    /// <summary>
    /// Gets or sets the maximum nesting depth for filter groups. Default is 3.
    /// </summary>
    [Parameter]
    public int MaxDepth { get; set; } = 3;

    /// <summary>
    /// Gets or sets the maximum total conditions allowed. Null for unlimited.
    /// </summary>
    [Parameter]
    public int? MaxConditions { get; set; }

    /// <summary>
    /// Gets or sets the default logical operator for new groups. Default is <see cref="LogicalOperator.And"/>.
    /// </summary>
    [Parameter]
    public LogicalOperator DefaultOperator { get; set; } = LogicalOperator.And;

    /// <summary>
    /// Gets or sets whether to show an explicit Apply button instead of auto-applying on change.
    /// Default is false (auto-apply with debounce).
    /// </summary>
    [Parameter]
    public bool ShowApplyButton { get; set; }

    /// <summary>
    /// Gets or sets whether to use compact layout for inline/toolbar placement.
    /// </summary>
    [Parameter]
    public bool Compact { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes for the container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    protected override void OnParametersSet()
    {
        context = new FilterBuilderContext
        {
            Fields = Fields,
            MaxDepth = MaxDepth,
            MaxConditions = MaxConditions,
            DefaultOperator = DefaultOperator,
            Compact = Compact,
            RootFilter = Filter,
            NotifyChanged = OnFilterTreeChanged
        };
    }

    private void OnFilterTreeChanged()
    {
        if (ShowApplyButton)
        {
            StateHasChanged();
            return;
        }

        debounceTimer?.Dispose();
        var capturedVersion = ++debounceVersion;
        debounceTimer = new Timer(
            async _ =>
            {
                if (disposed || capturedVersion != debounceVersion)
                {
                    return;
                }
                await InvokeAsync(async () =>
                {
                    if (capturedVersion != debounceVersion)
                    {
                        return;
                    }
                    await NotifyFilterChanged();
                    StateHasChanged();
                });
            },
            null,
            300,
            Timeout.Infinite);
    }

    private async Task HandleApply() =>
        await NotifyFilterChanged();

    private async Task HandleClear()
    {
        Filter.Conditions.Clear();
        Filter.Groups.Clear();
        Filter.Operator = DefaultOperator;
        await NotifyFilterChanged();
    }

    private async Task NotifyFilterChanged()
    {
        if (FilterChanged.HasDelegate)
        {
            await FilterChanged.InvokeAsync(Filter);
        }
        if (OnFilterChanged.HasDelegate)
        {
            await OnFilterChanged.InvokeAsync(Filter);
        }
    }

    private string ContainerCssClass => ClassNames.cn(
        "space-y-3",
        Compact ? "text-sm" : null,
        Class
    );

    private static string ActionsCssClass => "flex items-center gap-2 pt-2";

    public void Dispose()
    {
        if (!disposed)
        {
            debounceTimer?.Dispose();
            disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

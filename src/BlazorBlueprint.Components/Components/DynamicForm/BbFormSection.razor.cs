using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Groups related form fields with a heading, description, and optional collapsible behavior.
/// Can be used standalone or within a <see cref="BbDynamicForm"/>.
/// </summary>
public partial class BbFormSection : ComponentBase
{
    private bool isExpanded;

    /// <summary>
    /// Gets or sets the section heading text.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the section description displayed below the heading.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the content of the section.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets whether this section can be collapsed.
    /// </summary>
    [Parameter]
    public bool Collapsible { get; set; }

    /// <summary>
    /// Gets or sets whether this section is initially expanded when <see cref="Collapsible"/> is true.
    /// </summary>
    [Parameter]
    public bool DefaultExpanded { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show a separator line above the section.
    /// </summary>
    [Parameter]
    public bool ShowSeparator { get; set; } = true;

    /// <summary>
    /// Gets or sets additional CSS classes applied to the section container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        isExpanded = DefaultExpanded;
    }

    private string SectionCssClass => ClassNames.cn("space-y-4", Class);
}

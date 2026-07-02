using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A form field wrapper for <see cref="BbTagInput"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
public partial class BbFormFieldTagInput : FormFieldBase
{
    /// <summary>
    /// Gets or sets the list of tags.
    /// </summary>
    [Parameter]
    public IReadOnlyList<string>? Tags { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the tag list changes.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyList<string>?> TagsChanged { get; set; }

    /// <summary>
    /// Gets or sets the expression for the Tags property, used for EditForm validation.
    /// </summary>
    [Parameter]
    public Expression<Func<IReadOnlyList<string>?>>? TagsExpression { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of tags allowed.
    /// </summary>
    [Parameter]
    public int MaxTags { get; set; } = int.MaxValue;

    /// <summary>
    /// Gets or sets the maximum character length for a single tag.
    /// </summary>
    [Parameter]
    public int MaxTagLength { get; set; } = 50;

    /// <summary>
    /// Gets or sets whether duplicate tags are allowed.
    /// </summary>
    [Parameter]
    public bool AllowDuplicates { get; set; }

    /// <summary>
    /// Gets or sets which keys trigger tag creation.
    /// </summary>
    [Parameter]
    public TagInputTrigger AddTrigger { get; set; } = TagInputTrigger.Enter | TagInputTrigger.Comma;

    /// <summary>
    /// Gets or sets an optional validation function.
    /// </summary>
    [Parameter]
    public Func<string, bool>? Validate { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a tag is rejected.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnTagRejected { get; set; }

    /// <summary>
    /// Gets or sets a static list of suggestions.
    /// </summary>
    [Parameter]
    public IEnumerable<string>? Suggestions { get; set; }

    /// <summary>
    /// Gets or sets an async function to load suggestions.
    /// </summary>
    [Parameter]
    public Func<string, CancellationToken, Task<IEnumerable<string>>>? OnSearchSuggestions { get; set; }

    /// <summary>
    /// Gets or sets the debounce interval for suggestion search in milliseconds.
    /// </summary>
    [Parameter]
    public int SuggestionDebounceMs { get; set; } = 300;

    /// <summary>
    /// Gets or sets the visual variant for rendered tags.
    /// </summary>
    [Parameter]
    public TagInputVariant Variant { get; set; } = TagInputVariant.Default;

    /// <summary>
    /// Gets or sets whether to show a clear-all button.
    /// </summary>
    [Parameter]
    public bool Clearable { get; set; }

    /// <summary>
    /// Gets or sets whether the component is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets an optional custom template for rendering each tag.
    /// </summary>
    [Parameter]
    public RenderFragment<string>? TagTemplate { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner TagInput container.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => TagsExpression;

    private async Task HandleTagsChanged(IReadOnlyList<string>? tags)
    {
        Tags = tags;
        await TagsChanged.InvokeAsync(tags);
        NotifyFieldChanged();
    }
}

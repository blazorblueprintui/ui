using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Linq.Expressions;

namespace BlazorBlueprint.Components;

/// <summary>
/// A textarea component that follows the shadcn/ui design system.
/// </summary>
/// <remarks>
/// <para>
/// The Textarea component provides a customizable multi-line text input that
/// supports various states and features. It includes ARIA attributes
/// and integrates with Blazor's data binding system.
/// </para>
/// <para>
/// Features:
/// - Multi-line text input with automatic content sizing
/// - Two-way data binding with Value/ValueChanged
/// - Character limit support via MaxLength parameter
/// - Error state visualization via aria-invalid attribute
/// - Smooth color and shadow transitions for state changes
/// - Disabled and required states
/// - Placeholder text support
/// - Includes ARIA attributes
/// - RTL (Right-to-Left) support
/// - Dark mode compatible via CSS variables
/// - JavaScript-first event handling for optimal performance in Blazor Server/Auto
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Textarea @bind-Value="description" Placeholder="Enter your description" /&gt;
///
/// &lt;Textarea Value="@comment" ValueChanged="HandleCommentChange" MaxLength="500" Required="true" AriaInvalid="@hasError" /&gt;
/// </code>
/// </example>
public partial class BbTextarea : ComponentBase
{
    private ElementReference inputRef;
    private IJSObjectReference? jsModule;
    private DotNetObjectReference<BbTextarea>? dotNetRef;
    private string instanceId = Guid.NewGuid().ToString("N");
    private string? generatedId;
    private bool jsInitialized;
    private bool disposed;
    private readonly InputValidationBehavior validation = new();

    /// <summary>
    /// Gets or sets the cascaded EditContext from a parent EditForm.
    /// </summary>
    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    [CascadingParameter(Name = "FieldIsInvalid")]
    private bool? FieldIsInvalid { get; set; }

    /// <summary>
    /// Gets or sets the current value of the textarea.
    /// </summary>
    /// <remarks>
    /// Supports two-way binding via @bind-Value syntax.
    /// </remarks>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the textarea value changes.
    /// </summary>
    /// <remarks>
    /// When <see cref="UpdateTiming"/> is <see cref="Components.UpdateTiming.OnChange"/> (default),
    /// fires only on blur or Enter. Use <see cref="Components.UpdateTiming.Immediate"/> for per-keystroke updates.
    /// </remarks>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text displayed when the textarea is empty.
    /// </summary>
    /// <remarks>
    /// Provides a hint to the user about what to enter.
    /// Should not be used as a replacement for a label.
    /// </remarks>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets whether the textarea is disabled.
    /// </summary>
    /// <remarks>
    /// When disabled:
    /// - Textarea cannot be focused or edited
    /// - Cursor is set to not-allowed
    /// - Opacity is reduced for visual feedback
    /// </remarks>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the textarea is required.
    /// </summary>
    /// <remarks>
    /// When true, the HTML5 required attribute is set.
    /// Works with form validation and :invalid CSS pseudo-class.
    /// </remarks>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of characters allowed in the textarea.
    /// </summary>
    /// <remarks>
    /// When set, the HTML5 maxlength attribute is applied.
    /// Browser will prevent users from entering more than this many characters.
    /// </remarks>
    [Parameter]
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets whether to display a character count below the textarea.
    /// </summary>
    /// <remarks>
    /// When true and <see cref="MaxLength"/> is set, displays a "{current}/{max}" counter
    /// below the textarea. When <see cref="MaxLength"/> is not set, displays only the current count.
    /// </remarks>
    [Parameter]
    public bool ShowCharacterCount { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the textarea.
    /// </summary>
    /// <remarks>
    /// Custom classes are appended after the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the HTML id attribute for the textarea element.
    /// </summary>
    /// <remarks>
    /// Used to associate the textarea with a label element via the label's 'for' attribute.
    /// This is essential for accessibility and allows clicking the label to focus the textarea.
    /// </remarks>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the textarea.
    /// </summary>
    /// <remarks>
    /// Provides an accessible name for screen readers.
    /// Use when there is no visible label element.
    /// </remarks>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the ID of the element that describes the textarea.
    /// </summary>
    /// <remarks>
    /// References the id of an element containing help text or error messages.
    /// Associates descriptive text with the textarea via aria-describedby.
    /// </remarks>
    [Parameter]
    public string? AriaDescribedBy { get; set; }

    /// <summary>
    /// Gets or sets whether the textarea value is invalid.
    /// </summary>
    /// <remarks>
    /// When true, aria-invalid="true" is set.
    /// Should be set based on validation state.
    /// Triggers destructive color styling for error states.
    /// </remarks>
    [Parameter]
    public bool? AriaInvalid { get; set; }

    /// <summary>
    /// Gets or sets the HTML name attribute for the textarea element.
    /// </summary>
    /// <remarks>
    /// When inside an EditForm and not explicitly set, the name is automatically
    /// derived from the ValueExpression (FieldIdentifier) to support SSR form postback.
    /// </remarks>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value.
    /// </summary>
    [Parameter]
    public Expression<Func<string?>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets when <see cref="ValueChanged"/> fires.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><see cref="UpdateTiming.Immediate"/> — every keystroke (batched via requestAnimationFrame).</item>
    /// <item><see cref="UpdateTiming.OnChange"/> — only on blur / Enter (default).</item>
    /// <item><see cref="UpdateTiming.Debounced"/> — after typing pauses for <see cref="DebounceInterval"/> ms.</item>
    /// </list>
    /// </remarks>
    [Parameter]
    public UpdateTiming UpdateTiming { get; set; } = UpdateTiming.OnChange;

    /// <summary>
    /// Gets or sets the debounce delay in milliseconds when <see cref="UpdateTiming"/> is <see cref="UpdateTiming.Debounced"/>.
    /// </summary>
    /// <remarks>
    /// Ignored when <see cref="UpdateTiming"/> is not <see cref="UpdateTiming.Debounced"/>. Default is 500 ms.
    /// </remarks>
    [Parameter]
    public int DebounceInterval { get; set; } = 500;

    /// <summary>
    /// Gets or sets additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the effective name attribute, falling back to the FieldIdentifier name when inside an EditForm.
    /// </summary>
    private string? EffectiveName => validation.GetEffectiveName(Name);

    /// <summary>
    /// Gets the effective aria-invalid value combining manual AriaInvalid and EditContext validation.
    /// </summary>
    private string? EffectiveAriaInvalid => validation.GetEffectiveAriaInvalid(AriaInvalid, FieldIsInvalid);

    private string EffectiveId => Id ?? (generatedId ??= $"textarea-{Guid.NewGuid().ToString("N")[..8]}");

    /// <summary>
    /// Gets the character count display text.
    /// </summary>
    private string CharacterCountText => MaxLength.HasValue
        ? $"{Value?.Length ?? 0}/{MaxLength}"
        : $"{Value?.Length ?? 0}";

    /// <summary>
    /// Gets the computed CSS classes for the textarea element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        // Base textarea styles (from shadcn/ui v4)
        "flex field-sizing-content min-h-16 w-full rounded-md border border-input",
        "bg-transparent dark:bg-input/30 px-3 py-2 text-base shadow-xs",
        "placeholder:text-muted-foreground",
        // Focus states
        "outline-none focus-visible:border-ring",
        // Error states (aria-invalid)
        "aria-[invalid=true]:border-destructive",
        // Disabled state
        "disabled:cursor-not-allowed disabled:opacity-50",
        // Smooth transitions
        "transition-[color,box-shadow]",
        // Responsive text sizing
        "md:text-sm",
        // Custom classes (if provided)
        Class
    );

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        validation.Update(CascadedEditContext, ValueExpression);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/BlazorBlueprint.Components/js/text-input.js");
                dotNetRef = DotNetObjectReference.Create(this);
                await jsModule.InvokeVoidAsync("initialize", inputRef, dotNetRef, instanceId, GetJsConfig());
                jsInitialized = true;
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect
            }
            catch (InvalidOperationException)
            {
                // JS interop not available during prerendering
            }
        }
    }

    /// <summary>
    /// Builds the JS configuration object from current parameters.
    /// </summary>
    private object GetJsConfig() => new
    {
        mode = UpdateTiming switch
        {
            UpdateTiming.Immediate => "immediate",
            UpdateTiming.OnChange => "onchange",
            UpdateTiming.Debounced => "debounced",
            _ => "onchange"
        },
        debounceMs = DebounceInterval,
        hasCharacterCount = ShowCharacterCount,
        characterCountSelector = ShowCharacterCount ? "[data-character-count]" : null,
        maxLength = MaxLength
    };

    /// <summary>
    /// Called from JavaScript during typing (Immediate/Debounced modes only).
    /// </summary>
    [JSInvokable]
    public async Task JsOnInput(string? value)
    {
        if (disposed) { return; }

        if (string.IsNullOrWhiteSpace(value))
        {
            value = null;
        }

        Value = value;

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(value);
        }

        validation.NotifyFieldChanged();

        StateHasChanged();
    }

    /// <summary>
    /// Called from JavaScript on blur/Enter (all modes).
    /// </summary>
    [JSInvokable]
    public async Task JsOnChange(string? value)
    {
        if (disposed) { return; }

        if (string.IsNullOrWhiteSpace(value))
        {
            value = null;
        }

        Value = value;

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(value);
        }

        validation.NotifyFieldChanged();

        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        disposed = true;

        if (jsModule != null && jsInitialized)
        {
            try
            {
                await jsModule.InvokeVoidAsync("dispose", instanceId);
                await jsModule.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect
            }
            catch (InvalidOperationException)
            {
                // JS interop not available
            }
        }

        dotNetRef?.Dispose();
        GC.SuppressFinalize(this);
    }
}

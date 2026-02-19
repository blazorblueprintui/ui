using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Linq.Expressions;

namespace BlazorBlueprint.Components;

/// <summary>
/// A textarea component optimized for use within InputGroup.
/// </summary>
/// <remarks>
/// <para>
/// InputGroupTextarea is a specialized textarea designed to work seamlessly within
/// an InputGroup container. It removes standalone styling since the parent provides
/// the visual container, border, and focus management.
/// </para>
/// <para>
/// Features:
/// - Transparent background for seamless integration
/// - No border or focus ring (parent handles these)
/// - Flexible height based on content
/// - Resize control options
/// - Automatic marking for parent detection
/// - JavaScript-first event handling for optimal performance in Blazor Server/Auto
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;InputGroup&gt;
///     &lt;InputGroupTextarea Rows="4" Placeholder="Write a comment..." /&gt;
///     &lt;InputGroupAddon Align="InputGroupAlign.BlockEnd"&gt;
///         &lt;InputGroupText&gt;0 / 280&lt;/InputGroupText&gt;
///     &lt;/InputGroupAddon&gt;
/// &lt;/InputGroup&gt;
/// </code>
/// </example>
public partial class BbInputGroupTextarea : ComponentBase
{
    private ElementReference inputRef;
    private IJSObjectReference? jsModule;
    private DotNetObjectReference<BbInputGroupTextarea>? dotNetRef;
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
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the textarea value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the number of visible text rows.
    /// </summary>
    /// <remarks>
    /// Default is 3 rows. The textarea can grow beyond this if resize is enabled.
    /// </remarks>
    [Parameter]
    public int Rows { get; set; } = 3;

    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets whether the textarea is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the textarea is required.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the HTML id attribute.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the ARIA described-by attribute.
    /// </summary>
    [Parameter]
    public string? AriaDescribedBy { get; set; }

    /// <summary>
    /// Gets or sets whether the textarea value is invalid.
    /// </summary>
    [Parameter]
    public bool? AriaInvalid { get; set; }

    /// <summary>
    /// Gets or sets the HTML name attribute for the textarea element.
    /// </summary>
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
    [Parameter]
    public UpdateTiming UpdateTiming { get; set; } = UpdateTiming.OnChange;

    /// <summary>
    /// Gets or sets the debounce delay in milliseconds when <see cref="UpdateTiming"/> is <see cref="UpdateTiming.Debounced"/>.
    /// </summary>
    [Parameter]
    public int DebounceInterval { get; set; } = 500;

    private string? EffectiveAriaInvalid => validation.GetEffectiveAriaInvalid(AriaInvalid, FieldIsInvalid);

    /// <summary>
    /// Gets the effective name attribute, falling back to the FieldIdentifier name when inside an EditForm.
    /// </summary>
    private string? EffectiveName => validation.GetEffectiveName(Name);

    private string EffectiveId => Id ?? (generatedId ??= $"textarea-{Guid.NewGuid().ToString("N")[..8]}");

    /// <summary>
    /// Gets or sets additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the textarea element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        // Base styles - minimal for group context
        "flex-1 bg-transparent px-3 py-2 text-base min-h-[60px]",
        "border-0 rounded-none", // No border or radius for seamless integration
        "placeholder:text-muted-foreground",
        "focus-visible:outline-none",
        "disabled:cursor-not-allowed disabled:opacity-50",
        "resize-none", // Prevent resize for cleaner appearance
        // Medium screens and up: smaller text
        "md:text-sm",
        // Custom classes
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
        debounceMs = DebounceInterval
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

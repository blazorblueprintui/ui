using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Represents an individual step within a <see cref="BbFormWizard"/>.
/// Holds step metadata (title, icon, validation) and conditionally renders
/// its content when the step is active.
/// </summary>
public partial class BbWizardStep : ComponentBase, IDisposable
{
    private int index = -1;

    /// <summary>
    /// The parent wizard component, received via cascading parameter.
    /// </summary>
    [CascadingParameter]
    public BbFormWizard? Wizard { get; set; }

    /// <summary>
    /// The title displayed in the step indicator.
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional description displayed below the title in the step indicator.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Optional Lucide icon name displayed in the step indicator circle.
    /// When not set, the step number is displayed instead.
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>
    /// Whether this step can be skipped. Optional steps show a "Skip" option
    /// in the navigation and do not require validation to proceed.
    /// </summary>
    [Parameter]
    public bool IsOptional { get; set; }

    /// <summary>
    /// Whether the step is disabled and cannot be navigated to.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Names of model properties validated for this step.
    /// When provided, clicking "Next" validates only these fields against the
    /// cascaded <c>EditContext</c>. Use top-level property names of the form model.
    /// </summary>
    [Parameter]
    public string[]? FieldNames { get; set; }

    /// <summary>
    /// Custom validation function for this step. When provided, must return
    /// <c>true</c> for the step to be considered valid. Works independently
    /// of or in combination with <see cref="FieldNames"/>.
    /// </summary>
    [Parameter]
    public Func<bool>? Validator { get; set; }

    /// <summary>
    /// The content to render when this step is active.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional CSS classes for the step content wrapper.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets the zero-based index of this step within the wizard.
    /// </summary>
    internal int Index => index;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (Wizard is null)
        {
            throw new InvalidOperationException(
                "BbWizardStep must be used within a BbFormWizard component.");
        }

        index = Wizard.RegisterStep(this, new WizardStepInfo
        {
            Title = Title,
            Description = Description,
            Icon = Icon,
            IsOptional = IsOptional,
            Disabled = Disabled,
            FieldNames = FieldNames,
            Validator = Validator
        });
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Wizard?.UnregisterStep(this);
        GC.SuppressFinalize(this);
    }

    private string? CssClass => ClassNames.cn(Class);
}

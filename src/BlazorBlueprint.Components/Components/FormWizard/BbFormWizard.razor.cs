using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBlueprint.Components;

/// <summary>
/// A multi-step form wizard component that manages step navigation, progress indication,
/// and per-step validation. Compose with <see cref="BbWizardStep"/> children to define
/// each step in the wizard.
/// </summary>
/// <remarks>
/// <para>
/// The wizard supports both controlled (<c>@bind-CurrentStep</c>) and uncontrolled modes.
/// Place inside an <c>EditForm</c> to enable per-step validation via the
/// <see cref="BbWizardStep.FieldNames"/> parameter on each step.
/// </para>
/// <para>
/// Features:
/// - Horizontal and vertical step indicator layouts
/// - Per-step validation with EditContext integration
/// - Optional/skippable steps
/// - Built-in or custom navigation buttons
/// - Visual step states (pending, active, completed, invalid)
/// </para>
/// </remarks>
public partial class BbFormWizard : ComponentBase
{
    // --- Registration (Timeline pattern: reset counter each render) ---
    private readonly List<WizardStepInfo> steps = new();
    private int registrationIndex;

    // --- State ---
    private int currentStep;
    private readonly HashSet<int> visitedSteps = new() { 0 };
    private readonly Dictionary<int, bool> stepValidationState = new();

    // --- Parameters ---

    /// <summary>
    /// The content to render inside the wizard. Should contain <see cref="BbWizardStep"/> components.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The zero-based index of the current step. Supports two-way binding with <c>@bind-CurrentStep</c>.
    /// </summary>
    [Parameter]
    public int CurrentStep { get; set; }

    /// <summary>
    /// Event callback invoked when the current step changes.
    /// Use with <c>@bind-CurrentStep</c> for two-way binding.
    /// </summary>
    [Parameter]
    public EventCallback<int> CurrentStepChanged { get; set; }

    /// <summary>
    /// Event callback invoked when the user completes the wizard (clicks the final button on the last step).
    /// </summary>
    [Parameter]
    public EventCallback OnComplete { get; set; }

    /// <summary>
    /// Event callback invoked when the active step changes, receiving the new step index.
    /// Useful for analytics or side effects on navigation.
    /// </summary>
    [Parameter]
    public EventCallback<int> OnStepChanged { get; set; }

    /// <summary>
    /// The layout orientation of the step indicator.
    /// <c>Horizontal</c> (default) shows the indicator across the top.
    /// <c>Vertical</c> shows it along the left side.
    /// </summary>
    [Parameter]
    public WizardLayout Layout { get; set; } = WizardLayout.Horizontal;

    /// <summary>
    /// Whether to show the built-in Back/Next/Complete navigation buttons.
    /// Default is <c>true</c>. Set to <c>false</c> to hide them or use <see cref="NavigationTemplate"/>.
    /// </summary>
    [Parameter]
    public bool ShowNavigation { get; set; } = true;

    /// <summary>
    /// Optional custom navigation template that replaces the default Back/Next/Complete buttons.
    /// The wizard instance is accessible via the cascaded <c>BbFormWizard</c> parameter.
    /// </summary>
    [Parameter]
    public RenderFragment? NavigationTemplate { get; set; }

    /// <summary>
    /// Whether users can click on future (unvisited) steps in the step indicator to skip ahead.
    /// Default is <c>false</c> (users can only click visited steps).
    /// </summary>
    [Parameter]
    public bool AllowSkipAhead { get; set; }

    /// <summary>
    /// Additional CSS classes for the root wizard container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Additional CSS classes for the step indicator nav element.
    /// </summary>
    [Parameter]
    public string? IndicatorClass { get; set; }

    /// <summary>
    /// Additional CSS classes for the step content area.
    /// </summary>
    [Parameter]
    public string? ContentClass { get; set; }

    /// <summary>
    /// Additional CSS classes for the navigation button area.
    /// </summary>
    [Parameter]
    public string? NavigationClass { get; set; }

    /// <summary>
    /// Custom label for the "Next" button. Default is "Next".
    /// </summary>
    [Parameter]
    public string? NextLabel { get; set; }

    /// <summary>
    /// Custom label for the "Back" button. Default is "Back".
    /// </summary>
    [Parameter]
    public string? BackLabel { get; set; }

    /// <summary>
    /// Custom label for the "Complete" button shown on the last step. Default is "Complete".
    /// </summary>
    [Parameter]
    public string? CompleteLabel { get; set; }

    /// <summary>
    /// Additional HTML attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [CascadingParameter]
    private EditContext? EditContext { get; set; }

    // --- Internal API (accessed by BbWizardStep via CascadingParameter) ---

    /// <summary>
    /// Gets the zero-based index of the currently active step.
    /// </summary>
    internal int ActiveStepIndex => currentStep;

    /// <summary>
    /// Gets the total number of registered steps.
    /// </summary>
    internal int StepCount => steps.Count;

    /// <summary>
    /// Gets whether the current step is the first step.
    /// </summary>
    internal bool IsFirstStep => currentStep == 0;

    /// <summary>
    /// Gets whether the current step is the last step.
    /// </summary>
    internal bool IsLastStep => currentStep == steps.Count - 1;

    /// <summary>
    /// Gets whether backward navigation is possible.
    /// </summary>
    internal bool CanGoBack => currentStep > 0;

    /// <summary>
    /// Gets whether forward navigation is possible.
    /// </summary>
    internal bool CanGoNext => currentStep < steps.Count - 1;

    // --- Step Registration ---

    /// <summary>
    /// Registers a step with the wizard and returns its index.
    /// Called by each <see cref="BbWizardStep"/> during <c>OnParametersSet</c>.
    /// </summary>
    internal int RegisterStep(WizardStepInfo info)
    {
        var idx = registrationIndex++;
        if (idx < steps.Count)
        {
            steps[idx] = info;
        }
        else
        {
            steps.Add(info);
        }
        return idx;
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        // Reset registration counter each render cycle (Timeline pattern)
        registrationIndex = 0;

        // Sync controlled state
        if (CurrentStepChanged.HasDelegate && CurrentStep != currentStep)
        {
            currentStep = CurrentStep;
            visitedSteps.Add(currentStep);
        }
    }

    // --- Navigation ---

    /// <summary>
    /// Navigates to the next step after validating the current step.
    /// </summary>
    internal async Task GoToNextAsync()
    {
        if (!CanGoNext)
        {
            return;
        }

        if (!ValidateCurrentStep())
        {
            StateHasChanged();
            return;
        }

        stepValidationState[currentStep] = true;

        var nextIndex = FindNextEnabledStep(currentStep);
        if (nextIndex >= 0)
        {
            await SetCurrentStepAsync(nextIndex);
        }
    }

    /// <summary>
    /// Navigates to the previous step without validation.
    /// </summary>
    internal async Task GoToBackAsync()
    {
        if (!CanGoBack)
        {
            return;
        }

        var prevIndex = FindPreviousEnabledStep(currentStep);
        if (prevIndex >= 0)
        {
            await SetCurrentStepAsync(prevIndex);
        }
    }

    /// <summary>
    /// Navigates directly to a specific step by index.
    /// Only allowed if the step has been visited or <see cref="AllowSkipAhead"/> is true.
    /// </summary>
    internal async Task GoToStepAsync(int index)
    {
        if (index < 0 || index >= steps.Count || index == currentStep)
        {
            return;
        }

        if (steps[index].Disabled)
        {
            return;
        }

        if (!AllowSkipAhead && !visitedSteps.Contains(index))
        {
            return;
        }

        await SetCurrentStepAsync(index);
    }

    /// <summary>
    /// Skips the current step (only valid for optional steps) and navigates forward.
    /// </summary>
    internal async Task SkipCurrentStepAsync()
    {
        if (!CanGoNext)
        {
            return;
        }

        if (currentStep < steps.Count && steps[currentStep].IsOptional)
        {
            stepValidationState[currentStep] = true;
            var nextIndex = FindNextEnabledStep(currentStep);
            if (nextIndex >= 0)
            {
                await SetCurrentStepAsync(nextIndex);
            }
        }
    }

    /// <summary>
    /// Completes the wizard. Validates the current step and invokes <see cref="OnComplete"/>.
    /// </summary>
    internal async Task CompleteAsync()
    {
        if (!ValidateCurrentStep())
        {
            StateHasChanged();
            return;
        }

        stepValidationState[currentStep] = true;

        if (OnComplete.HasDelegate)
        {
            await OnComplete.InvokeAsync();
        }
    }

    // --- Validation ---

    private bool ValidateCurrentStep()
    {
        if (currentStep >= steps.Count)
        {
            return true;
        }

        var step = steps[currentStep];

        // Check EditContext-based field validation
        if (step.FieldNames is { Length: > 0 } && EditContext is not null)
        {
            EditContext.Validate();

            foreach (var fieldName in step.FieldNames)
            {
                var fieldIdentifier = new FieldIdentifier(EditContext.Model, fieldName);
                if (EditContext.GetValidationMessages(fieldIdentifier).Any())
                {
                    stepValidationState[currentStep] = false;
                    return false;
                }
            }
        }

        // Check custom validator
        if (step.Validator is not null && !step.Validator())
        {
            stepValidationState[currentStep] = false;
            return false;
        }

        return true;
    }

    // --- Step State ---

    /// <summary>
    /// Gets the visual state of a step at the given index.
    /// </summary>
    internal WizardStepState GetStepState(int index)
    {
        if (index == currentStep)
        {
            return WizardStepState.Active;
        }

        if (steps.Count > index && steps[index].Disabled)
        {
            return WizardStepState.Pending;
        }

        if (stepValidationState.TryGetValue(index, out var isValid))
        {
            return isValid ? WizardStepState.Completed : WizardStepState.Invalid;
        }

        return WizardStepState.Pending;
    }

    /// <summary>
    /// Returns whether a step indicator can be clicked to navigate.
    /// </summary>
    internal bool CanNavigateToStep(int index)
    {
        if (index == currentStep)
        {
            return false;
        }

        if (index < 0 || index >= steps.Count)
        {
            return false;
        }

        if (steps[index].Disabled)
        {
            return false;
        }

        return AllowSkipAhead || visitedSteps.Contains(index);
    }

    // --- Helpers ---

    private async Task SetCurrentStepAsync(int index)
    {
        currentStep = index;
        visitedSteps.Add(index);

        if (CurrentStepChanged.HasDelegate)
        {
            await CurrentStepChanged.InvokeAsync(currentStep);
        }

        if (OnStepChanged.HasDelegate)
        {
            await OnStepChanged.InvokeAsync(currentStep);
        }

        StateHasChanged();
    }

    private int FindNextEnabledStep(int fromIndex)
    {
        for (int i = fromIndex + 1; i < steps.Count; i++)
        {
            if (!steps[i].Disabled)
            {
                return i;
            }
        }
        return -1;
    }

    private int FindPreviousEnabledStep(int fromIndex)
    {
        for (int i = fromIndex - 1; i >= 0; i--)
        {
            if (!steps[i].Disabled)
            {
                return i;
            }
        }
        return -1;
    }

    // --- CSS ---

    private string CssClass => ClassNames.cn(
        Layout == WizardLayout.Vertical ? "flex gap-8" : "flex flex-col",
        Class
    );

    private string IndicatorCssClass => ClassNames.cn(
        Layout == WizardLayout.Horizontal
            ? "flex items-center w-full"
            : "flex flex-col shrink-0",
        IndicatorClass
    );

    private string ContentWrapperCssClass => ClassNames.cn(
        Layout == WizardLayout.Vertical ? "flex-1 min-w-0 flex flex-col" : null
    );

    private string ContentCssClass => ClassNames.cn(
        Layout == WizardLayout.Horizontal ? "mt-6" : "flex-1",
        ContentClass
    );

    private string NavigationCssClass => ClassNames.cn(
        "flex items-center justify-between pt-6",
        NavigationClass
    );

    private static string GetStepCircleClass(WizardStepState state) => ClassNames.cn(
        "flex h-8 w-8 shrink-0 items-center justify-center rounded-full border-2 text-sm font-medium transition-colors",
        state switch
        {
            WizardStepState.Active => "border-primary bg-primary text-primary-foreground",
            WizardStepState.Completed => "border-primary bg-primary text-primary-foreground",
            WizardStepState.Invalid => "border-destructive bg-destructive text-destructive-foreground",
            WizardStepState.Skipped => "border-muted-foreground/50 text-muted-foreground",
            _ => "border-muted-foreground/25 text-muted-foreground"
        }
    );

    private string GetStepButtonClass(int index, WizardStepState state) => ClassNames.cn(
        "flex items-center gap-2 group cursor-default",
        Layout == WizardLayout.Vertical ? "flex-row" : "flex-col",
        CanNavigateToStep(index) ? "cursor-pointer" : null
    );

    private static string GetStepTitleClass(WizardStepState state) => ClassNames.cn(
        "text-sm font-medium transition-colors whitespace-nowrap",
        state == WizardStepState.Active ? "text-foreground" : "text-muted-foreground"
    );

    private static string GetConnectorClass(WizardStepState state, WizardLayout layout)
    {
        if (layout == WizardLayout.Horizontal)
        {
            return ClassNames.cn(
                "flex-1 mx-2 transition-colors",
                state == WizardStepState.Completed ? "h-0.5 bg-primary" : "h-0.5 bg-border"
            );
        }

        return ClassNames.cn(
            "transition-colors ml-[15px]",
            state == WizardStepState.Completed ? "w-0.5 h-8 bg-primary" : "w-0.5 h-8 bg-border"
        );
    }
}

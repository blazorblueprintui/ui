using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorBlueprint.Components;

/// <summary>
/// Renders a complete form from a <see cref="FormSchema"/> definition, automatically selecting
/// the appropriate BlazorBlueprint input component for each field.
/// </summary>
public partial class BbDynamicForm : ComponentBase
{
    private Dictionary<string, string?> fieldErrors = new();
    private bool isSubmitting;

    /// <summary>
    /// Gets or sets the schema definition for the form.
    /// </summary>
    [Parameter]
    public FormSchema? Schema { get; set; }

    /// <summary>
    /// Gets or sets the current form values dictionary. Use <c>@bind-Values</c> for two-way binding.
    /// </summary>
    [Parameter]
    public Dictionary<string, object?> Values { get; set; } = new();

    /// <summary>
    /// Gets or sets the callback invoked when the values dictionary changes.
    /// </summary>
    [Parameter]
    public EventCallback<Dictionary<string, object?>> ValuesChanged { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked on form submission (regardless of validation).
    /// </summary>
    [Parameter]
    public EventCallback<Dictionary<string, object?>> OnSubmit { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked on form submission when all validation passes.
    /// </summary>
    [Parameter]
    public EventCallback<Dictionary<string, object?>> OnValidSubmit { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when any field value changes.
    /// </summary>
    [Parameter]
    public EventCallback<FormFieldChangedEventArgs> OnFieldChanged { get; set; }

    /// <summary>
    /// Gets or sets the form layout mode.
    /// </summary>
    [Parameter]
    public FormLayout Layout { get; set; } = FormLayout.Vertical;

    /// <summary>
    /// Gets or sets the number of columns for the field grid layout.
    /// </summary>
    [Parameter]
    public int Columns { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether all fields are rendered as read-only.
    /// </summary>
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Gets or sets whether all fields are disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the text for the built-in submit button.
    /// </summary>
    [Parameter]
    public string SubmitText { get; set; } = "Submit";

    /// <summary>
    /// Gets or sets whether to show the built-in submit button.
    /// </summary>
    [Parameter]
    public bool ShowSubmitButton { get; set; } = true;

    /// <summary>
    /// Gets or sets additional CSS classes applied to the form element.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets custom field renderers keyed by field name.
    /// Used to provide custom rendering for <see cref="FieldType.Custom"/> fields.
    /// </summary>
    [Parameter]
    public Dictionary<string, RenderFragment<DynamicFieldRenderContext>>? FieldRenderers { get; set; }

    /// <summary>
    /// Gets or sets additional content to render inside the form (e.g., extra buttons).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        ApplyDefaultValues();
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        ApplyDefaultValues();
    }

    private void ApplyDefaultValues()
    {
        if (Schema is null)
        {
            return;
        }

        foreach (var field in GetAllFields())
        {
            if (field.DefaultValue is not null && !Values.ContainsKey(field.Name))
            {
                Values[field.Name] = field.DefaultValue;
            }
        }
    }

    private IEnumerable<FormFieldDefinition> GetAllFields()
    {
        if (Schema is null)
        {
            return [];
        }

        if (Schema.Sections.Count > 0)
        {
            return Schema.Sections.SelectMany(s => s.Fields);
        }

        return Schema.Fields;
    }

    private List<FormFieldDefinition> GetOrderedFields(List<FormFieldDefinition> fields)
    {
        return fields
            .Select((f, i) => (field: f, index: i))
            .OrderBy(x => x.field.Order ?? x.index)
            .Select(x => x.field)
            .ToList();
    }

    private bool IsFieldVisible(FormFieldDefinition field)
    {
        if (string.IsNullOrEmpty(field.VisibleWhen))
        {
            return true;
        }

        try
        {
            return VisibilityExpression.Evaluate(field.VisibleWhen, Values);
        }
        catch
        {
            return true;
        }
    }

    private bool IsSectionVisible(FormSectionDefinition section)
    {
        if (string.IsNullOrEmpty(section.VisibleWhen))
        {
            return true;
        }

        try
        {
            return VisibilityExpression.Evaluate(section.VisibleWhen, Values);
        }
        catch
        {
            return true;
        }
    }

    private int GetEffectiveColumns(int? sectionOverride)
    {
        return sectionOverride ?? Schema?.Columns ?? Columns;
    }

    private string GetGridClass(int columns)
    {
        return columns switch
        {
            1 => "grid grid-cols-1 gap-4",
            2 => "grid grid-cols-1 md:grid-cols-2 gap-4",
            3 => "grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4",
            4 => "grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4",
            _ => $"grid grid-cols-1 md:grid-cols-2 lg:grid-cols-{columns} gap-4"
        };
    }

    private string? GetColSpanStyle(FormFieldDefinition field, int maxColumns)
    {
        if (field.ColSpan > 1 && maxColumns > 1)
        {
            var span = Math.Min(field.ColSpan, maxColumns);
            return $"grid-column: span {span}";
        }

        return null;
    }

    // ── Value Change Handling ────────────────────────────────────────

    private async Task OnFieldValueChanged(string fieldName, object? newValue)
    {
        var oldValue = Values.TryGetValue(fieldName, out var v) ? v : null;
        Values[fieldName] = newValue;

        // Clear field error on change
        fieldErrors.Remove(fieldName);

        await ValuesChanged.InvokeAsync(Values);

        if (OnFieldChanged.HasDelegate)
        {
            await OnFieldChanged.InvokeAsync(new FormFieldChangedEventArgs(fieldName, oldValue, newValue));
        }

        StateHasChanged();
    }

    private Func<object?, Task> CreateFieldCallback(string fieldName)
    {
        return newValue => OnFieldValueChanged(fieldName, newValue);
    }

    // ── Validation ──────────────────────────────────────────────────

    private bool ValidateForm()
    {
        fieldErrors.Clear();

        foreach (var field in GetAllFields())
        {
            if (!IsFieldVisible(field))
            {
                continue;
            }

            Values.TryGetValue(field.Name, out var value);
            var error = ValidateField(field, value);
            if (error is not null)
            {
                fieldErrors[field.Name] = error;
            }
        }

        return fieldErrors.Count == 0;
    }

    private static string? ValidateField(FormFieldDefinition field, object? value)
    {
        // Required check
        if (field.Required && IsEmpty(value))
        {
            return $"{field.Label ?? field.Name} is required.";
        }

        // Skip further validation if empty and not required
        if (IsEmpty(value))
        {
            return null;
        }

        if (field.Validations is null)
        {
            return null;
        }

        foreach (var validation in field.Validations)
        {
            var error = EvaluateValidation(field, validation, value);
            if (error is not null)
            {
                return error;
            }
        }

        return null;
    }

    private static string? EvaluateValidation(FormFieldDefinition field, FieldValidation validation, object? value)
    {
        var label = field.Label ?? field.Name;
        var str = value?.ToString() ?? "";

        switch (validation.Type)
        {
            case ValidationType.Required:
                if (IsEmpty(value))
                {
                    return validation.Message ?? $"{label} is required.";
                }

                break;

            case ValidationType.MinLength:
                if (validation.Value is not null)
                {
                    var min = Convert.ToInt32(validation.Value, CultureInfo.InvariantCulture);
                    if (str.Length < min)
                    {
                        return validation.Message ?? $"{label} must be at least {min} characters.";
                    }
                }

                break;

            case ValidationType.MaxLength:
                if (validation.Value is not null)
                {
                    var max = Convert.ToInt32(validation.Value, CultureInfo.InvariantCulture);
                    if (str.Length > max)
                    {
                        return validation.Message ?? $"{label} must be at most {max} characters.";
                    }
                }

                break;

            case ValidationType.Min:
                if (validation.Value is not null && double.TryParse(str, CultureInfo.InvariantCulture, out var minNum))
                {
                    var minVal = Convert.ToDouble(validation.Value, CultureInfo.InvariantCulture);
                    if (minNum < minVal)
                    {
                        return validation.Message ?? $"{label} must be at least {minVal}.";
                    }
                }

                break;

            case ValidationType.Max:
                if (validation.Value is not null && double.TryParse(str, CultureInfo.InvariantCulture, out var maxNum))
                {
                    var maxVal = Convert.ToDouble(validation.Value, CultureInfo.InvariantCulture);
                    if (maxNum > maxVal)
                    {
                        return validation.Message ?? $"{label} must be at most {maxVal}.";
                    }
                }

                break;

            case ValidationType.Pattern:
                if (validation.Value is string pattern && !Regex.IsMatch(str, pattern))
                {
                    return validation.Message ?? $"{label} does not match the required format.";
                }

                break;

            case ValidationType.Email:
                if (!Regex.IsMatch(str, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    return validation.Message ?? $"{label} must be a valid email address.";
                }

                break;

            case ValidationType.Url:
                if (!Uri.TryCreate(str, UriKind.Absolute, out _))
                {
                    return validation.Message ?? $"{label} must be a valid URL.";
                }

                break;

            case ValidationType.Phone:
                if (!Regex.IsMatch(str, @"^[\d\s\-\+\(\)]+$"))
                {
                    return validation.Message ?? $"{label} must be a valid phone number.";
                }

                break;
        }

        return null;
    }

    private static bool IsEmpty(object? value)
    {
        return value switch
        {
            null => true,
            string s => string.IsNullOrWhiteSpace(s),
            IEnumerable<string> list => !list.Any(),
            _ => false
        };
    }

    // ── Form Submission ─────────────────────────────────────────────

    private async Task HandleSubmit()
    {
        isSubmitting = true;
        StateHasChanged();

        try
        {
            if (OnSubmit.HasDelegate)
            {
                await OnSubmit.InvokeAsync(Values);
            }

            var isValid = ValidateForm();
            StateHasChanged();

            if (isValid && OnValidSubmit.HasDelegate)
            {
                await OnValidSubmit.InvokeAsync(Values);
            }
        }
        finally
        {
            isSubmitting = false;
            StateHasChanged();
        }
    }

    // ── Rendering ───────────────────────────────────────────────────

    private void RenderFieldsGrid(RenderTreeBuilder builder, List<FormFieldDefinition> fields, int columns)
    {
        var orderedFields = GetOrderedFields(fields);
        var seq = 0;

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "class", GetGridClass(columns));

        foreach (var field in orderedFields)
        {
            if (!IsFieldVisible(field))
            {
                continue;
            }

            var colSpanStyle = GetColSpanStyle(field, columns);
            builder.OpenElement(seq, "div");
            if (colSpanStyle is not null)
            {
                builder.AddAttribute(seq + 1, "style", colSpanStyle);
            }

            fieldErrors.TryGetValue(field.Name, out var errorText);
            Values.TryGetValue(field.Name, out var value);

            RenderFragment<DynamicFieldRenderContext>? customRenderer = null;
            if (field.Type == FieldType.Custom)
            {
                FieldRenderers?.TryGetValue(field.Name, out customRenderer);
            }

            DynamicFieldRenderer.RenderField(
                builder,
                seq + 10,
                field,
                value,
                CreateFieldCallback(field.Name),
                errorText,
                Disabled,
                ReadOnly,
                this,
                customRenderer);

            builder.CloseElement();
            seq += 100;
        }

        builder.CloseElement();
    }

    private string FormCssClass => ClassNames.cn(
        "space-y-6",
        Layout == FormLayout.Inline ? "flex flex-wrap items-end gap-4" : null,
        Class
    );

    private string? GetFieldErrorText(string fieldName)
    {
        return fieldErrors.TryGetValue(fieldName, out var error) ? error : null;
    }

    private bool HasErrors => fieldErrors.Count > 0;

    private IEnumerable<string> ErrorSummary => fieldErrors.Values.Where(e => e is not null).Cast<string>();
}

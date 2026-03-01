namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the schema for a dynamic form, including its fields and optional section grouping.
/// </summary>
public class FormSchema
{
    /// <summary>
    /// Gets or sets the form title displayed above the form.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets a description displayed below the title.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the form sections. Use this for grouped fields with headings and descriptions.
    /// When set, <see cref="Fields"/> is ignored.
    /// </summary>
    public List<FormSectionDefinition> Sections { get; set; } = [];

    /// <summary>
    /// Gets or sets a flat list of fields (no section grouping).
    /// Ignored when <see cref="Sections"/> is populated.
    /// </summary>
    public List<FormFieldDefinition> Fields { get; set; } = [];

    /// <summary>
    /// Gets or sets the default number of grid columns for field layout.
    /// Individual sections can override this value.
    /// </summary>
    public int Columns { get; set; } = 1;
}

/// <summary>
/// Defines a section within a dynamic form that groups related fields.
/// </summary>
public class FormSectionDefinition
{
    /// <summary>
    /// Gets or sets the section heading text.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the section description displayed below the heading.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the fields within this section.
    /// </summary>
    public List<FormFieldDefinition> Fields { get; set; } = [];

    /// <summary>
    /// Gets or sets an override for the form-level column count within this section.
    /// When null, the form-level <see cref="FormSchema.Columns"/> is used.
    /// </summary>
    public int? Columns { get; set; }

    /// <summary>
    /// Gets or sets a visibility expression that controls whether this section is shown.
    /// Evaluated against current form values. Example: <c>"Country == 'US'"</c>.
    /// </summary>
    public string? VisibleWhen { get; set; }

    /// <summary>
    /// Gets or sets whether this section can be collapsed.
    /// </summary>
    public bool Collapsible { get; set; }

    /// <summary>
    /// Gets or sets whether this section is initially expanded when <see cref="Collapsible"/> is true.
    /// </summary>
    public bool DefaultExpanded { get; set; } = true;
}

/// <summary>
/// Defines a single field within a dynamic form schema.
/// </summary>
public class FormFieldDefinition
{
    /// <summary>
    /// Gets or sets the field name. Used as the key in the values dictionary.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the label text displayed above or beside the field.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the description/helper text displayed below the field.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text displayed when the field is empty.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the field type, which determines which component is rendered.
    /// </summary>
    public FieldType Type { get; set; } = FieldType.Text;

    /// <summary>
    /// Gets or sets whether this field is required.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets whether this field is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether this field is read-only.
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the default value for this field.
    /// Applied when the field name is not present in the values dictionary.
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the display order. Fields with lower values appear first.
    /// When null, fields appear in their definition order.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    /// Gets or sets the number of grid columns this field spans.
    /// </summary>
    public int ColSpan { get; set; } = 1;

    /// <summary>
    /// Gets or sets a visibility expression that controls whether this field is shown.
    /// Evaluated against current form values. Example: <c>"Country == 'US'"</c>.
    /// </summary>
    public string? VisibleWhen { get; set; }

    /// <summary>
    /// Gets or sets the options for Select, Combobox, MultiSelect, RadioGroup, and CheckboxGroup fields.
    /// </summary>
    public List<SelectOption<string>>? Options { get; set; }

    /// <summary>
    /// Gets or sets the validation rules for this field.
    /// </summary>
    public List<FieldValidation>? Validations { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for field-type-specific configuration.
    /// For example: <c>{ "rows", 5 }</c> for Textarea or <c>{ "min", 0 }, { "max", 100 }</c> for Slider.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

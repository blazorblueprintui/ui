namespace BlazorBlueprint.Components;

/// <summary>
/// Context passed to custom field renderers in a dynamic form,
/// providing the field definition, current value, and a callback to update it.
/// </summary>
public class DynamicFieldRenderContext
{
    /// <summary>
    /// Gets the field definition for this field.
    /// </summary>
    public FormFieldDefinition Field { get; }

    /// <summary>
    /// Gets the current value of the field.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Gets the callback to invoke when the field value changes.
    /// </summary>
    public Func<object?, Task> ValueChanged { get; }

    /// <summary>
    /// Gets whether this field is disabled (from field definition or form-level override).
    /// </summary>
    public bool Disabled { get; }

    /// <summary>
    /// Gets whether this field is read-only (from field definition or form-level override).
    /// </summary>
    public bool ReadOnly { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="DynamicFieldRenderContext"/>.
    /// </summary>
    public DynamicFieldRenderContext(
        FormFieldDefinition field,
        object? value,
        Func<object?, Task> valueChanged,
        bool disabled,
        bool readOnly)
    {
        Field = field;
        Value = value;
        ValueChanged = valueChanged;
        Disabled = disabled;
        ReadOnly = readOnly;
    }
}

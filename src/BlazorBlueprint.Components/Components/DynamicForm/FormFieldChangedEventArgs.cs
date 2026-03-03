namespace BlazorBlueprint.Components;

/// <summary>
/// Event arguments for when a dynamic form field value changes.
/// </summary>
public class FormFieldChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the name of the field that changed.
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    /// Gets the previous value of the field.
    /// </summary>
    public object? OldValue { get; }

    /// <summary>
    /// Gets the new value of the field.
    /// </summary>
    public object? NewValue { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="FormFieldChangedEventArgs"/>.
    /// </summary>
    public FormFieldChangedEventArgs(string fieldName, object? oldValue, object? newValue)
    {
        FieldName = fieldName;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

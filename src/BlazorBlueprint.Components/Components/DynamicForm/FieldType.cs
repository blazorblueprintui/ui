namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the type of field for a dynamic form field definition.
/// Each type maps to a specific BlazorBlueprint input component.
/// </summary>
public enum FieldType
{
    /// <summary>Single-line text input.</summary>
    Text,

    /// <summary>Email address input with email keyboard on mobile.</summary>
    Email,

    /// <summary>Password input with obscured characters.</summary>
    Password,

    /// <summary>URL input with URL validation.</summary>
    Url,

    /// <summary>Phone number input with telephone keyboard on mobile.</summary>
    Phone,

    /// <summary>Numeric input with increment/decrement controls.</summary>
    Number,

    /// <summary>Currency-formatted numeric input.</summary>
    Currency,

    /// <summary>Multi-line text input.</summary>
    Textarea,

    /// <summary>Rich text editor with formatting toolbar.</summary>
    RichText,

    /// <summary>Styled dropdown select.</summary>
    Select,

    /// <summary>Searchable combobox with filtering.</summary>
    Combobox,

    /// <summary>Multi-value select dropdown.</summary>
    MultiSelect,

    /// <summary>Native HTML select element.</summary>
    NativeSelect,

    /// <summary>Single checkbox (boolean).</summary>
    Checkbox,

    /// <summary>Toggle switch (boolean).</summary>
    Switch,

    /// <summary>Group of checkboxes for multi-value selection.</summary>
    CheckboxGroup,

    /// <summary>Radio button group for single-value selection.</summary>
    RadioGroup,

    /// <summary>Date picker.</summary>
    Date,

    /// <summary>Date range picker with start and end dates.</summary>
    DateRange,

    /// <summary>Time picker.</summary>
    Time,

    /// <summary>Date and time picker.</summary>
    DateTime,

    /// <summary>Color picker.</summary>
    Color,

    /// <summary>File upload.</summary>
    File,

    /// <summary>One-time password input.</summary>
    OTP,

    /// <summary>Single-value slider.</summary>
    Slider,

    /// <summary>Range slider with two thumbs.</summary>
    RangeSlider,

    /// <summary>Tag/chip input for managing a list of strings.</summary>
    Tags,

    /// <summary>Custom field type that delegates to a registered renderer.</summary>
    Custom
}

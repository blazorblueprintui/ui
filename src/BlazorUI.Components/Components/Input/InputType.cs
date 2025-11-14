namespace BlazorUI.Components.Input;

/// <summary>
/// Defines the type of input for an Input component.
/// </summary>
/// <remarks>
/// Maps to the HTML input type attribute. Each type provides different
/// input behavior, validation, and keyboard layouts on mobile devices.
/// </remarks>
public enum InputType
{
    /// <summary>
    /// Single-line text input (default).
    /// Accepts any text characters.
    /// </summary>
    Text,

    /// <summary>
    /// Email address input.
    /// Provides email validation and @ key on mobile keyboards.
    /// </summary>
    Email,

    /// <summary>
    /// Password input with obscured characters.
    /// Text is hidden for security (displayed as dots or asterisks).
    /// </summary>
    Password,

    /// <summary>
    /// Numeric input.
    /// Shows numeric keyboard on mobile and allows spinner controls.
    /// </summary>
    Number,

    /// <summary>
    /// Telephone number input.
    /// Optimized for phone number entry with tel: keyboard layout.
    /// </summary>
    Tel,

    /// <summary>
    /// URL input.
    /// Provides URL validation and .com key on mobile keyboards.
    /// </summary>
    Url,

    /// <summary>
    /// Search query input.
    /// Displays search icon and may show recent searches.
    /// </summary>
    Search,

    /// <summary>
    /// Date picker input.
    /// Shows native date picker on supported browsers.
    /// </summary>
    Date,

    /// <summary>
    /// Time picker input.
    /// Shows native time picker on supported browsers.
    /// </summary>
    Time,

    /// <summary>
    /// File upload input.
    /// Allows users to select files from their device.
    /// </summary>
    File
}

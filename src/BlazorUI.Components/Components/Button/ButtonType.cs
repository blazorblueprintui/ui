namespace BlazorUI.Components.Button;

/// <summary>
/// Defines the HTML button type attribute value.
/// </summary>
/// <remarks>
/// Controls the behavior of the button element when used within HTML forms.
/// Maps directly to the HTML 'type' attribute on button elements.
/// See: https://developer.mozilla.org/en-US/docs/Web/HTML/Element/button#type
/// </remarks>
public enum ButtonType
{
    /// <summary>
    /// Submit button (type="submit").
    /// Submits the form data to the server when clicked.
    /// This is the default behavior for buttons inside forms.
    /// </summary>
    Submit,

    /// <summary>
    /// Reset button (type="reset").
    /// Resets all form controls to their initial values when clicked.
    /// Use sparingly as this can be confusing to users.
    /// </summary>
    Reset,

    /// <summary>
    /// Regular button (type="button").
    /// Does not submit or reset the form.
    /// Default for buttons outside forms or with custom click handlers.
    /// Prevents accidental form submission.
    /// </summary>
    Button
}

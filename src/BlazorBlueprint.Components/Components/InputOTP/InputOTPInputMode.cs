namespace BlazorBlueprint.Components;

/// <summary>
/// Specifies the types of characters accepted by the InputOTP component.
/// </summary>
public enum InputOTPInputMode
{
    /// <summary>
    /// Only numeric digits (0-9) are accepted.
    /// </summary>
    Numbers,

    /// <summary>
    /// Only letters (a-z, A-Z) are accepted.
    /// </summary>
    Letters,

    /// <summary>
    /// Both letters and numbers are accepted.
    /// </summary>
    LettersAndNumbers
}

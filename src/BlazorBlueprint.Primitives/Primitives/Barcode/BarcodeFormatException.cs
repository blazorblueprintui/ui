namespace BlazorBlueprint.Primitives.Barcode;

/// <summary>
/// A value a symbology cannot carry.
/// </summary>
/// <remarks>
/// <para>
/// An <see cref="ArgumentException"/>, so existing catch blocks keep working — but its
/// <see cref="Message"/> is exactly the sentence it was given, with nothing appended.
/// </para>
/// <para>
/// That is the whole reason it exists. <see cref="ArgumentException.Message"/> composes the
/// message with the parameter name, and the wording of that suffix comes from a framework
/// resource string. On WebAssembly those resources are routinely trimmed away, and what comes
/// back in their place is the resource key — so a barcode with a bad value printed
/// <c>Arg_ParamName_Name</c> on the page. These messages are written to be read by whoever typed
/// the value, so the message has to be ours alone and has to say the same thing on every host.
/// </para>
/// <para>
/// The parameter name is still carried on <see cref="ArgumentException.ParamName"/> for anything
/// that wants it. It is only kept out of the text.
/// </para>
/// <para>
/// <see cref="Error"/> and <see cref="Arguments"/> say the same thing in a form a program can read,
/// so a component can show its own translated sentence instead of the English
/// <see cref="Message"/>.
/// </para>
/// </remarks>
public sealed class BarcodeFormatException : ArgumentException
{
    private readonly string text;

    /// <summary>Creates the exception with the reason it was thrown.</summary>
    /// <param name="message">What is wrong, as a sentence to show someone.</param>
    /// <param name="paramName">The argument at fault.</param>
    /// <param name="error">What is wrong, for a program to read.</param>
    /// <param name="arguments">The values the message was formatted with.</param>
    internal BarcodeFormatException(string message, string? paramName, BarcodeFormatError error, params object[] arguments)
        : base(message, paramName)
    {
        text = message;
        Error = error;
        Arguments = arguments;
    }

    /// <summary>Creates the exception.</summary>
    /// <param name="message">What is wrong, as a sentence to show someone.</param>
    /// <param name="paramName">The argument at fault.</param>
    public BarcodeFormatException(string message, string? paramName)
        : base(message, paramName)
    {
        text = message;
    }

    /// <summary>Creates the exception.</summary>
    /// <param name="message">What is wrong, as a sentence to show someone.</param>
    public BarcodeFormatException(string message)
        : base(message)
    {
        text = message;
    }

    /// <summary>Creates the exception.</summary>
    public BarcodeFormatException()
    {
        text = base.Message;
    }

    /// <summary>Creates the exception.</summary>
    /// <param name="message">What is wrong, as a sentence to show someone.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public BarcodeFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
        text = message;
    }

    /// <summary>
    /// Gets what is wrong, for a program to read. <see cref="BarcodeFormatError.Unspecified"/> when
    /// the exception was created without a reason.
    /// </summary>
    public BarcodeFormatError Error { get; }

    /// <summary>
    /// Gets the values <see cref="Message"/> was formatted with, in the order
    /// <see cref="BarcodeFormatError"/> documents for each reason. Empty when there are none.
    /// </summary>
    public IReadOnlyList<object> Arguments { get; } = [];

    /// <summary>Gets what is wrong, as a sentence and nothing else.</summary>
    public override string Message => text;
}

namespace BlazorBlueprint.Primitives.DataMatrix;

/// <summary>
/// A value a Data Matrix symbol cannot carry, or GS1 notation that cannot be read.
/// </summary>
/// <remarks>
/// <para>
/// An <see cref="ArgumentException"/>, so existing catch blocks keep working, but its
/// <see cref="Message"/> is exactly the sentence it was given, with nothing appended. The
/// framework's composed message adds a parameter-name suffix from a resource string that is
/// routinely trimmed on WebAssembly, where the raw resource key shows instead.
/// </para>
/// <para>
/// <see cref="Error"/> and <see cref="Arguments"/> say the same thing in a form a program can read,
/// so a component can show its own translated sentence instead of the English
/// <see cref="Message"/>.
/// </para>
/// </remarks>
public sealed class DataMatrixFormatException : ArgumentException
{
    private readonly string text;

    /// <summary>Creates the exception with the reason it was thrown.</summary>
    /// <param name="message">What is wrong, as a sentence to show someone.</param>
    /// <param name="paramName">The argument at fault.</param>
    /// <param name="error">What is wrong, for a program to read.</param>
    /// <param name="arguments">The values the message was formatted with.</param>
    internal DataMatrixFormatException(string message, string? paramName, DataMatrixFormatError error, params object[] arguments)
        : base(message, paramName)
    {
        text = message;
        Error = error;
        Arguments = arguments;
    }

    /// <summary>Creates the exception.</summary>
    /// <param name="message">What is wrong, as a sentence to show someone.</param>
    /// <param name="paramName">The argument at fault.</param>
    public DataMatrixFormatException(string message, string? paramName)
        : base(message, paramName)
    {
        text = message;
    }

    /// <summary>Creates the exception.</summary>
    /// <param name="message">What is wrong, as a sentence to show someone.</param>
    public DataMatrixFormatException(string message)
        : base(message)
    {
        text = message;
    }

    /// <summary>Creates the exception.</summary>
    public DataMatrixFormatException()
    {
        text = base.Message;
    }

    /// <summary>Creates the exception.</summary>
    /// <param name="message">What is wrong, as a sentence to show someone.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public DataMatrixFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
        text = message;
    }

    /// <summary>
    /// Gets what is wrong, for a program to read. <see cref="DataMatrixFormatError.Unspecified"/>
    /// when the exception was created without a reason.
    /// </summary>
    public DataMatrixFormatError Error { get; }

    /// <summary>
    /// Gets the values <see cref="Message"/> was formatted with, in the order
    /// <see cref="DataMatrixFormatError"/> documents for each reason. Empty when there are none.
    /// </summary>
    public IReadOnlyList<object> Arguments { get; } = [];

    /// <summary>Gets what is wrong, as a sentence and nothing else.</summary>
    public override string Message => text;

    /// <summary>The English sentence for each reason; DefaultBbLocalizer holds the same text.</summary>
    private static readonly Dictionary<DataMatrixFormatError, string> Templates = new()
    {
        [DataMatrixFormatError.Empty] = "There is nothing to encode.",
        [DataMatrixFormatError.TooLong] = "This value is too long for a Data Matrix symbol of shape {0}, which holds at most {1} data codewords.",
        [DataMatrixFormatError.CharacterNotInMode] = "{0} encodation cannot carry '{1}'. Use Auto to choose a mode for each part of the value.",
        [DataMatrixFormatError.InvalidText] = "This value is not valid Unicode text.",
        [DataMatrixFormatError.Gs1MissingAi] = "GS1 notation must start with an Application Identifier in parentheses, such as (01).",
        [DataMatrixFormatError.Gs1EmptyElement] = "GS1 element ({0}) has no data.",
        [DataMatrixFormatError.Gs1SeparatorInNotation] = "GS1 element ({0}) contains a group separator. Use either (AI) notation or an element string, not both.",
        [DataMatrixFormatError.Gs1WrongLength] = "GS1 element ({0}) needs {1} data characters, and this value has {2}.",
        [DataMatrixFormatError.DoesNotFitSize] = "This value does not fit the {0} symbol, which holds {1} data codewords.",
        [DataMatrixFormatError.Gs1NotDigits] = "GS1 element ({0}) holds digits only.",
        [DataMatrixFormatError.Gs1WrongCheckDigit] = "GS1 element ({0}) ends in the wrong check digit: it should be {1}, not {2}.",
    };

    internal static DataMatrixFormatException Create(string paramName, DataMatrixFormatError error, params object[] arguments) =>
        new(string.Format(System.Globalization.CultureInfo.InvariantCulture, Templates[error], arguments),
            paramName, error, arguments);
}

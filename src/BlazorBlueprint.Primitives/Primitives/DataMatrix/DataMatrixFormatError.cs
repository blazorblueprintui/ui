namespace BlazorBlueprint.Primitives.DataMatrix;

/// <summary>
/// Why a value was refused, carried on <see cref="DataMatrixFormatException.Error"/>.
/// </summary>
/// <remarks>
/// Each member lists what <see cref="DataMatrixFormatException.Arguments"/> holds for it, in order.
/// A component uses the two to show the message in the reader's language.
/// </remarks>
public enum DataMatrixFormatError
{
    /// <summary>No reason was given. No arguments.</summary>
    Unspecified,

    /// <summary>The value is empty. No arguments.</summary>
    Empty,

    /// <summary>
    /// The value does not fit the largest symbol of the allowed shape. Arguments: the
    /// <see cref="DataMatrixSymbolShape"/> and the data codewords its largest symbol holds.
    /// </summary>
    TooLong,

    /// <summary>
    /// An explicit encodation mode cannot carry a character. Arguments: the
    /// <see cref="DataMatrixEncodingMode"/> and the character, as a string.
    /// </summary>
    CharacterNotInMode,

    /// <summary>The value is not valid Unicode text, such as an unpaired surrogate. No arguments.</summary>
    InvalidText,

    /// <summary>GS1 notation does not start with a parenthesized Application Identifier. No arguments.</summary>
    Gs1MissingAi,

    /// <summary>A GS1 element has no data. Arguments: the Application Identifier.</summary>
    Gs1EmptyElement,

    /// <summary>
    /// GS1 notation also contains a group separator (U+001D). Arguments: the Application Identifier
    /// of the element that contains it.
    /// </summary>
    Gs1SeparatorInNotation,

    /// <summary>
    /// A GS1 element with a predefined length has the wrong length. Arguments: the Application
    /// Identifier, the number of data characters it needs, and the number given.
    /// </summary>
    Gs1WrongLength,
}

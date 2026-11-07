namespace BlazorBlueprint.Primitives.Barcode;

/// <summary>
/// Why a value was refused, carried on <see cref="BarcodeFormatException.Error"/>.
/// </summary>
/// <remarks>
/// Each member lists what <see cref="BarcodeFormatException.Arguments"/> holds for it, in order.
/// A component uses the two to show the message in the reader's language.
/// </remarks>
public enum BarcodeFormatError
{
    /// <summary>No reason was given. No arguments.</summary>
    Unspecified,

    /// <summary>The value is empty. No arguments.</summary>
    Empty,

    /// <summary>
    /// A digits-only symbology was given something else. Arguments: the <see cref="BarcodeType"/>
    /// and the offending character.
    /// </summary>
    NotDigits,

    /// <summary>Code 128 was given a character beyond ASCII. Arguments: the character.</summary>
    Code128NotAscii,

    /// <summary>Code 39 was given a character outside its set. Arguments: the character.</summary>
    Code39InvalidCharacter,

    /// <summary>
    /// A retail code has the wrong number of digits. Arguments: the <see cref="BarcodeType"/>, the
    /// length without the check digit, the length with it, and the length given.
    /// </summary>
    WrongLength,

    /// <summary>
    /// A retail code ends in the wrong check digit. Arguments: the <see cref="BarcodeType"/>, the
    /// digits before the check digit, the check digit expected, and the check digit given.
    /// </summary>
    WrongCheckDigit,

    /// <summary>The value is not an ISBN. Arguments: the value.</summary>
    InvalidIsbn,

    /// <summary>The value is not an ISSN. Arguments: the value.</summary>
    InvalidIssn,

    /// <summary>Interleaved 2 of 5 was given no digits. No arguments.</summary>
    ItfTooShort,

    /// <summary>Codabar was given a character outside its set. Arguments: the character.</summary>
    CodabarInvalidCharacter,

    /// <summary>MSI was given no digits. No arguments.</summary>
    MsiEmpty,

    /// <summary>POSTNET was given the wrong number of digits. Arguments: the number given.</summary>
    PostnetWrongLength,

    /// <summary>
    /// The Royal Mail 4-state code was given a character outside its set. Arguments: the character.
    /// </summary>
    Rm4sccInvalidCharacter,

    /// <summary>The Royal Mail 4-state code was given no characters. No arguments.</summary>
    Rm4sccEmpty,

    /// <summary>Pharmacode was given something other than 3 to 131070. Arguments: the value.</summary>
    PharmacodeOutOfRange,

    /// <summary>Telepen was given a character beyond ASCII. Arguments: the character.</summary>
    TelepenNotAscii,
}

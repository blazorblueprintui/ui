namespace BlazorBlueprint.Primitives.Barcode;

/// <summary>
/// Turns a value into a <see cref="BarcodeSymbol"/> in any of the supported symbologies.
/// </summary>
/// <remarks>
/// <para>
/// Each symbology has its own alphabet, its own length rules and its own check digit, and the
/// encoder enforces all three. A value the chosen symbology cannot carry throws rather than
/// producing a symbol that no reader will accept.
/// </para>
/// <para>
/// Where a check digit is part of the standard — the retail codes, POSTNET, the Royal Mail code —
/// it is always worked out and appended, and <see cref="BarcodeSymbol.Value"/> reports the result.
/// Where the standard makes it optional, <c>addChecksum</c> decides, and it is off by default
/// because a reader that is not expecting one reports it as part of the data.
/// </para>
/// </remarks>
public static class BarcodeEncoder
{
    /// <summary>
    /// Encodes a value as a barcode.
    /// </summary>
    /// <param name="value">The value to encode. What counts as valid depends on <paramref name="type"/>.</param>
    /// <param name="type">The symbology to encode in.</param>
    /// <param name="addChecksum">
    /// Whether to add the optional check character, for the symbologies that have one:
    /// <see cref="BarcodeType.Code39"/>, <see cref="BarcodeType.Itf"/>,
    /// <see cref="BarcodeType.Codabar"/> and <see cref="BarcodeType.Msi"/>. Ignored elsewhere,
    /// where the check digit is either mandatory or built into the symbology.
    /// </param>
    /// <returns>The finished symbol.</returns>
    /// <exception cref="BarcodeFormatException">
    /// <paramref name="value"/> is empty, or it holds a character or a length the symbology cannot
    /// carry, or it ends in a check digit that does not match.
    /// </exception>
    public static BarcodeSymbol Encode(string value, BarcodeType type, bool addChecksum = false)
    {
        // A null is a mistake in the calling code and never reaches a reader, so the framework's
        // own exception is right for it.
        ArgumentNullException.ThrowIfNull(value);

        // An empty value is not. It is what a bound input holds before anyone has typed, and the
        // message goes on the page — so it is ours. ArgumentException.ThrowIfNullOrEmpty would
        // word it from a framework resource, and on WebAssembly those are routinely trimmed to
        // their keys.
        if (value.Length == 0)
        {
            throw new BarcodeFormatException("There is nothing to encode.", nameof(value), BarcodeFormatError.Empty);
        }

        return type switch
        {
            BarcodeType.Code128 => Code128Encoder.Encode(value),
            BarcodeType.Code39 => Code39Encoder.Encode(value, addChecksum),
            BarcodeType.Ean13 or BarcodeType.Ean8 or BarcodeType.UpcA or BarcodeType.Isbn or BarcodeType.Issn
                => EanUpcEncoder.Encode(value, type),
            BarcodeType.Itf => ItfCodabarMsiEncoder.EncodeItf(value, type, addChecksum),
            BarcodeType.Codabar => ItfCodabarMsiEncoder.EncodeCodabar(value, addChecksum),
            BarcodeType.Msi => ItfCodabarMsiEncoder.EncodeMsi(value, addChecksum),
            BarcodeType.Postnet => PostalEncoder.EncodePostnet(value),
            BarcodeType.Rm4scc => PostalEncoder.EncodeRm4scc(value),
            BarcodeType.Pharmacode => PostalEncoder.EncodePharmacode(value),
            BarcodeType.Telepen => TelepenEncoder.Encode(value),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown barcode symbology."),
        };
    }

    /// <summary>
    /// Gets a short description of what a symbology accepts, for an error message or a form hint.
    /// </summary>
    /// <param name="type">The symbology.</param>
    /// <returns>A sentence describing the valid values.</returns>
    public static string DescribeInput(BarcodeType type) => type switch
    {
        BarcodeType.Code128 => "Any ASCII text.",
        BarcodeType.Code39 => "Digits, letters and - . space $ / + %.",
        BarcodeType.Ean13 => "12 digits, or 13 with the check digit.",
        BarcodeType.Ean8 => "7 digits, or 8 with the check digit.",
        BarcodeType.UpcA => "11 digits, or 12 with the check digit.",
        BarcodeType.Itf => "Digits, an even number of them.",
        BarcodeType.Codabar => "Digits and - $ : / . +, optionally wrapped in a start and stop letter A to D.",
        BarcodeType.Postnet => "A 5, 9 or 11 digit ZIP code.",
        BarcodeType.Pharmacode => "A whole number from 3 to 131070.",
        BarcodeType.Rm4scc => "Digits and upper-case letters.",
        BarcodeType.Isbn => "A 10 or 13 digit ISBN, with or without hyphens.",
        BarcodeType.Issn => "An 8 character ISSN, with or without a hyphen.",
        BarcodeType.Msi => "Digits.",
        BarcodeType.Telepen => "Any ASCII text.",
        _ => string.Empty,
    };

    /// <summary>
    /// Strips spaces and hyphens and checks that what is left is all digits.
    /// </summary>
    /// <param name="value">The value as given.</param>
    /// <param name="type">The symbology, for the error message.</param>
    /// <returns>The digits.</returns>
    /// <exception cref="BarcodeFormatException">The value holds something other than digits.</exception>
    internal static string DigitsOnly(string value, BarcodeType type)
    {
        var digits = new char[value.Length];
        var count = 0;

        foreach (var c in value)
        {
            if (c is '-' or ' ')
            {
                continue;
            }

            if (!char.IsAsciiDigit(c))
            {
                throw new BarcodeFormatException(
                    $"{type} holds digits only, and this value contains '{c}'. {DescribeInput(type)}",
                    nameof(value),
                    BarcodeFormatError.NotDigits,
                    type,
                    c);
            }

            digits[count] = c;
            count++;
        }

        return new string(digits, 0, count);
    }
}

namespace BlazorBlueprint.Primitives.Barcode;

/// <summary>
/// Three digit-oriented symbologies that share nothing but their age: Interleaved 2 of 5, Codabar
/// and MSI.
/// </summary>
internal static class ItfCodabarMsiEncoder
{
    /// <summary>
    /// Five elements per digit, two of them wide. Interleaving puts one digit in the bars and the
    /// next in the spaces, which is where the density and the even-length rule both come from.
    /// </summary>
    private static readonly string[] ItfPatterns =
    [
        "00110", "10001", "01001", "11000", "00101",
        "10100", "01100", "00011", "10010", "01010",
    ];

    private const string CodabarCharset = "0123456789-$:/.+";
    private const string CodabarDelimiters = "ABCD";

    /// <summary>Seven elements per character, bar first and alternating; <c>1</c> is wide.</summary>
    private static readonly string[] CodabarPatterns =
    [
        "0000011", "0000110", "0001001", "1100000", "0010010",
        "1000010", "0100001", "0100100", "0110000", "1001000",
        "0001100", "0011000", "1000101", "1010001", "1010100", "0010101",
    ];

    private static readonly string[] CodabarDelimiterPatterns =
    [
        "0011010", "0101001", "0001011", "0001110",
    ];

    internal static BarcodeSymbol EncodeItf(string value, BarcodeType type, bool addChecksum)
    {
        var digits = BarcodeEncoder.DigitsOnly(value, type);

        if (addChecksum)
        {
            digits += EanUpcEncoder.CheckDigit(digits);
        }

        // Digits are consumed in pairs, so an odd count gets a leading zero rather than an error.
        if (digits.Length % 2 == 1)
        {
            digits = "0" + digits;
        }

        if (digits.Length == 0)
        {
            throw new BarcodeFormatException("Interleaved 2 of 5 needs at least two digits.", nameof(value), BarcodeFormatError.ItfTooShort);
        }

        var builder = new BarcodeBuilder();

        // Start: four narrow elements. Stop: a wide bar, a narrow space and a narrow bar.
        builder.NarrowWide("0000", 1, 3);

        for (var i = 0; i < digits.Length; i += 2)
        {
            var bars = ItfPatterns[digits[i] - '0'];
            var spaces = ItfPatterns[digits[i + 1] - '0'];

            for (var element = 0; element < 5; element++)
            {
                builder.Bar(bars[element] == '1' ? 3 : 1);
                builder.Space(spaces[element] == '1' ? 3 : 1);
            }
        }

        builder.Bar(3);
        builder.Space(1);
        builder.Bar(1);

        return new BarcodeSymbol(
            type,
            digits,
            builder.Position,
            10,
            builder.ToBars(),
            [new BarcodeTextGroup(digits, 0, builder.Position)]);
    }

    internal static BarcodeSymbol EncodeCodabar(string value, bool addChecksum)
    {
        var upper = value.ToUpperInvariant();

        // The start and stop letters are part of the value. Supply them or they are added.
        var start = 'A';
        var stop = 'A';
        var body = upper;

        if (upper.Length >= 2
            && CodabarDelimiters.Contains(upper[0], StringComparison.Ordinal)
            && CodabarDelimiters.Contains(upper[^1], StringComparison.Ordinal))
        {
            start = upper[0];
            stop = upper[^1];
            body = upper[1..^1];
        }

        foreach (var c in body)
        {
            if (!CodabarCharset.Contains(c, StringComparison.Ordinal))
            {
                throw new BarcodeFormatException(
                    $"Codabar holds digits and - $ : / . +, wrapped in a start and stop letter from A to D. This value contains '{c}'.",
                    nameof(value),
                    BarcodeFormatError.CodabarInvalidCharacter,
                    c);
            }
        }

        if (addChecksum)
        {
            var sum = body.Sum(c => CodabarCharset.IndexOf(c, StringComparison.Ordinal));
            body += CodabarCharset[(16 - (sum % 16)) % 16];
        }

        var builder = new BarcodeBuilder();
        builder.NarrowWide(CodabarDelimiterPatterns[CodabarDelimiters.IndexOf(start, StringComparison.Ordinal)], 1, 2);

        foreach (var c in body)
        {
            builder.Space(1);
            builder.NarrowWide(CodabarPatterns[CodabarCharset.IndexOf(c, StringComparison.Ordinal)], 1, 2);
        }

        builder.Space(1);
        builder.NarrowWide(CodabarDelimiterPatterns[CodabarDelimiters.IndexOf(stop, StringComparison.Ordinal)], 1, 2);

        var encoded = $"{start}{body}{stop}";

        return new BarcodeSymbol(
            BarcodeType.Codabar,
            encoded,
            builder.Position,
            10,
            builder.ToBars(),
            [new BarcodeTextGroup(encoded, 0, builder.Position)]);
    }

    internal static BarcodeSymbol EncodeMsi(string value, bool addChecksum)
    {
        var digits = BarcodeEncoder.DigitsOnly(value, BarcodeType.Msi);

        if (digits.Length == 0)
        {
            throw new BarcodeFormatException("MSI needs at least one digit.", nameof(value), BarcodeFormatError.MsiEmpty);
        }

        if (addChecksum)
        {
            digits += MsiCheckDigit(digits);
        }

        var builder = new BarcodeBuilder();

        // Start: a wide bar and a narrow space, the same shape as a set bit.
        builder.Bar(2);
        builder.Space(1);

        foreach (var digit in digits)
        {
            var bits = digit - '0';

            // Four bits, most significant first. A one is a wide bar and a narrow space; a zero is
            // the other way round, so every bit costs three modules whatever its value.
            for (var bit = 3; bit >= 0; bit--)
            {
                if (((bits >> bit) & 1) == 1)
                {
                    builder.Bar(2);
                    builder.Space(1);
                }
                else
                {
                    builder.Bar(1);
                    builder.Space(2);
                }
            }
        }

        // Stop: a narrow bar, a wide space and a narrow bar.
        builder.Bar(1);
        builder.Space(2);
        builder.Bar(1);

        return new BarcodeSymbol(
            BarcodeType.Msi,
            digits,
            builder.Position,
            12,
            builder.ToBars(),
            [new BarcodeTextGroup(digits, 0, builder.Position)]);
    }

    /// <summary>
    /// Works out the MSI modulo 10 check digit, doubling every second digit from the right and
    /// adding the digits of the result.
    /// </summary>
    private static char MsiCheckDigit(string digits)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            var value = digits[^(i + 1)] - '0';
            if (i % 2 == 0)
            {
                value *= 2;
                if (value > 9)
                {
                    value -= 9;
                }
            }

            sum += value;
        }

        return (char)('0' + ((10 - (sum % 10)) % 10));
    }
}

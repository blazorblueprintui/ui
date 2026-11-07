using System.Globalization;

namespace BlazorBlueprint.Primitives.Barcode;

/// <summary>
/// The retail family: EAN-13, EAN-8, UPC-A, and the ISBN and ISSN that are EAN-13 underneath.
/// </summary>
/// <remarks>
/// All of them are the same machine: guard bars at the ends and the middle, digits encoded seven
/// modules each, and a check digit worked out with alternating weights of one and three. EAN-13
/// adds one trick — it has thirteen digits but only room for twelve, so the first digit is carried
/// in the pattern of odd and even parities used for the left-hand six.
/// </remarks>
internal static class EanUpcEncoder
{
    // Seven modules per digit. L is odd parity, G is L reversed and inverted, R is L inverted.
    private static readonly string[] LeftOdd =
    [
        "0001101", "0011001", "0010011", "0111101", "0100011",
        "0110001", "0101111", "0111011", "0110111", "0001011",
    ];

    private static readonly string[] LeftEven =
    [
        "0100111", "0110011", "0011011", "0100001", "0011101",
        "0111001", "0000101", "0010001", "0001001", "0010111",
    ];

    private static readonly string[] Right =
    [
        "1110010", "1100110", "1101100", "1000010", "1011100",
        "1001110", "1010000", "1000100", "1001000", "1110100",
    ];

    // Which of the left-hand six digits use even parity, chosen by the first digit of an EAN-13.
    private static readonly string[] ParityPatterns =
    [
        "000000", "001011", "001101", "001110", "010011",
        "011001", "011100", "010101", "010110", "011010",
    ];

    private const string EndGuard = "101";
    private const string CentreGuard = "01010";

    /// <summary>The guard bars run past the digits, so the printed number sits between them.</summary>
    private const double DataBarBottom = 100.0 / 110.0;

    internal static BarcodeSymbol Encode(string value, BarcodeType type)
    {
        var digits = type switch
        {
            BarcodeType.Isbn => IsbnDigits(value),
            BarcodeType.Issn => IssnDigits(value),
            _ => BarcodeEncoder.DigitsOnly(value, type),
        };

        var length = type switch
        {
            BarcodeType.Ean8 => 8,
            BarcodeType.UpcA => 12,
            _ => 13,
        };

        digits = WithCheckDigit(digits, length, type);

        return type == BarcodeType.Ean8
            ? EncodeEan8(digits)
            : EncodeThirteen(digits, type, value);
    }

    /// <summary>
    /// Accepts the value with or without its final check digit, and verifies it when it is there.
    /// </summary>
    private static string WithCheckDigit(string digits, int length, BarcodeType type)
    {
        if (digits.Length == length - 1)
        {
            return digits + CheckDigit(digits);
        }

        if (digits.Length != length)
        {
            throw new BarcodeFormatException(
                $"{type} needs {length - 1} digits, or {length} with the check digit already on the end. This value has {digits.Length}.",
                nameof(digits),
                BarcodeFormatError.WrongLength,
                type,
                length - 1,
                length,
                digits.Length);
        }

        var expected = CheckDigit(digits[..^1]);
        if (digits[^1] != expected)
        {
            throw new BarcodeFormatException(
                $"The check digit is wrong: {type} of {digits[..^1]} ends in {expected}, not {digits[^1]}. Leave the check digit off and it is worked out.",
                nameof(digits),
                BarcodeFormatError.WrongCheckDigit,
                type,
                digits[..^1],
                expected,
                digits[^1]);
        }

        return digits;
    }

    /// <summary>
    /// Works out the check digit, weighting every second digit from the right by three.
    /// </summary>
    /// <param name="withoutCheck">The value without its check digit.</param>
    /// <returns>The check digit as a character.</returns>
    internal static char CheckDigit(string withoutCheck)
    {
        var sum = 0;
        for (var i = 0; i < withoutCheck.Length; i++)
        {
            // Counting from the right so the weights land the same way whatever the length.
            var weight = (withoutCheck.Length - i) % 2 == 1 ? 3 : 1;
            sum += (withoutCheck[i] - '0') * weight;
        }

        return (char)('0' + ((10 - (sum % 10)) % 10));
    }

    private static BarcodeSymbol EncodeThirteen(string digits, BarcodeType type, string original)
    {
        // A UPC-A is an EAN-13 whose first digit is zero; encoding it that way and printing twelve
        // digits is exactly what the standard says to do.
        var thirteen = type == BarcodeType.UpcA ? "0" + digits : digits;
        var parity = ParityPatterns[thirteen[0] - '0'];

        var builder = new BarcodeBuilder();
        builder.Modules(EndGuard);
        var leftStart = builder.Position;

        // On a UPC-A the first and last digits are printed outside the guard bars, so their bars
        // run full height too and the number reads as one row.
        var outerDescend = type == BarcodeType.UpcA;

        for (var i = 0; i < 6; i++)
        {
            var digit = thirteen[i + 1] - '0';
            var bottom = outerDescend && i == 0 ? 1 : DataBarBottom;
            builder.Modules(parity[i] == '1' ? LeftEven[digit] : LeftOdd[digit], bottom);
        }

        var leftEnd = builder.Position;
        builder.Modules(CentreGuard);
        var rightStart = builder.Position;

        for (var i = 0; i < 6; i++)
        {
            var bottom = outerDescend && i == 5 ? 1 : DataBarBottom;
            builder.Modules(Right[thirteen[i + 7] - '0'], bottom);
        }

        var rightEnd = builder.Position;
        builder.Modules(EndGuard);

        var text = type switch
        {
            BarcodeType.UpcA =>
            [
                new BarcodeTextGroup(digits[..1], -9, -1),
                new BarcodeTextGroup(digits[1..6], leftStart + 7, leftEnd),
                new BarcodeTextGroup(digits[6..11], rightStart, rightEnd - 7),
                new BarcodeTextGroup(digits[11..], builder.Position + 1, builder.Position + 9),
            ],
            _ => new List<BarcodeTextGroup>
            {
                new(thirteen[..1], -9, -1),
                new(thirteen[1..7], leftStart, leftEnd),
                new(thirteen[7..], rightStart, rightEnd),
            },
        };

        if (type is BarcodeType.Isbn or BarcodeType.Issn)
        {
            // The human-readable form of a book or serial number goes above the bars, because the
            // thirteen digits below it are the product code rather than the number people quote.
            text.Insert(0, new BarcodeTextGroup(
                type == BarcodeType.Isbn ? FormatIsbn(thirteen) : FormatIssn(original, thirteen),
                0,
                builder.Position));
        }

        return new BarcodeSymbol(type, digits, builder.Position, 11, builder.ToBars(), text);
    }

    private static BarcodeSymbol EncodeEan8(string digits)
    {
        var builder = new BarcodeBuilder();
        builder.Modules(EndGuard);
        var leftStart = builder.Position;

        for (var i = 0; i < 4; i++)
        {
            builder.Modules(LeftOdd[digits[i] - '0'], DataBarBottom);
        }

        var leftEnd = builder.Position;
        builder.Modules(CentreGuard);
        var rightStart = builder.Position;

        for (var i = 4; i < 8; i++)
        {
            builder.Modules(Right[digits[i] - '0'], DataBarBottom);
        }

        var rightEnd = builder.Position;
        builder.Modules(EndGuard);

        List<BarcodeTextGroup> text =
        [
            new(digits[..4], leftStart, leftEnd),
            new(digits[4..], rightStart, rightEnd),
        ];

        return new BarcodeSymbol(BarcodeType.Ean8, digits, builder.Position, 7, builder.ToBars(), text);
    }

    /// <summary>
    /// Turns an ISBN, of either length and with or without hyphens, into the twelve digits an
    /// EAN-13 needs.
    /// </summary>
    private static string IsbnDigits(string value)
    {
        var cleaned = Strip(value);

        // A 10-digit ISBN's last character can be X, and it is a different check digit from the
        // EAN one, so it is dropped rather than carried over.
        if (cleaned.Length == 10 && cleaned[..9].All(char.IsAsciiDigit))
        {
            return "978" + cleaned[..9];
        }

        if (cleaned.Length is 12 or 13
            && cleaned.All(char.IsAsciiDigit)
            && (cleaned.StartsWith("978", StringComparison.Ordinal) || cleaned.StartsWith("979", StringComparison.Ordinal)))
        {
            return cleaned;
        }

        throw new BarcodeFormatException(
            $"An ISBN is 10 digits, or 13 beginning 978 or 979, with or without hyphens. This value is \"{value}\".",
            nameof(value),
            BarcodeFormatError.InvalidIsbn,
            value);
    }

    /// <summary>
    /// Turns an eight-digit ISSN into the twelve digits an EAN-13 needs, under the 977 prefix.
    /// </summary>
    private static string IssnDigits(string value)
    {
        var cleaned = Strip(value);

        // The ISSN's own check digit is dropped: the EAN has its own, and the two middle zeros are
        // the variant code, which is zero unless a publisher says otherwise.
        if (cleaned.Length == 8 && cleaned[..7].All(char.IsAsciiDigit))
        {
            return "977" + cleaned[..7] + "00";
        }

        if (cleaned.Length is 12 or 13 && cleaned.All(char.IsAsciiDigit) && cleaned.StartsWith("977", StringComparison.Ordinal))
        {
            return cleaned;
        }

        throw new BarcodeFormatException(
            $"An ISSN is 8 characters, with or without a hyphen. This value is \"{value}\".",
            nameof(value),
            BarcodeFormatError.InvalidIssn,
            value);
    }

    private static string Strip(string value) =>
        new([.. value.Where(c => c is not ('-' or ' '))]);

    /// <summary>
    /// Hyphenates a 13-digit ISBN at the only two boundaries that never move.
    /// </summary>
    /// <remarks>
    /// The registration group and the publisher inside it are split by a table that runs to
    /// thousands of ranges and changes as new ranges are issued, so the parts between the prefix
    /// and the check digit are left as one run rather than guessed at.
    /// </remarks>
    private static string FormatIsbn(string thirteen) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"ISBN {thirteen[..3]}-{thirteen[3..^1]}-{thirteen[^1]}");

    private static string FormatIssn(string original, string thirteen)
    {
        var cleaned = Strip(original);
        var body = cleaned.Length == 8 ? cleaned : thirteen[3..10] + IssnCheckCharacter(thirteen[3..10]);

        return string.Create(CultureInfo.InvariantCulture, $"ISSN {body[..4]}-{body[4..]}");
    }

    /// <summary>
    /// Works out an ISSN's own check character, which is modulo 11 and so can be an X.
    /// </summary>
    private static char IssnCheckCharacter(string seven)
    {
        var sum = 0;
        for (var i = 0; i < 7; i++)
        {
            sum += (seven[i] - '0') * (8 - i);
        }

        var remainder = (11 - (sum % 11)) % 11;
        return remainder == 10 ? 'X' : (char)('0' + remainder);
    }
}

namespace BlazorBlueprint.Primitives.Barcode;

/// <summary>
/// The codes that carry their data in bar heights rather than bar widths: POSTNET, the Royal Mail
/// 4-state code, and Laetus Pharmacode alongside them.
/// </summary>
/// <remarks>
/// A postal sorter reads a moving envelope, so these put every bar on the same pitch and vary the
/// shape instead. That survives the smearing a conveyor causes far better than varying the widths
/// would. The practical consequence for a renderer is that squashing the height destroys the data.
/// </remarks>
internal static class PostalEncoder
{
    /// <summary>Five bars per digit, two of them tall.</summary>
    private static readonly string[] PostnetPatterns =
    [
        "11000", "00011", "00101", "00110", "01001",
        "01010", "01100", "10001", "10010", "10100",
    ];

    /// <summary>The short bars of a POSTNET symbol start halfway down.</summary>
    private const double PostnetShortTop = 0.5;

    /// <summary>
    /// Four bars per character. Each bar is one of four shapes, written here as the pair of bits it
    /// contributes: the first to the ascender half, the second to the descender half.
    /// </summary>
    private static readonly string Rm4sccCharset =
        "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>
    /// The four bar shapes of a 4-state code, in the order the table indexes them: tracker,
    /// ascender, descender, full.
    /// </summary>
    private static readonly (double Top, double Bottom)[] Rm4sccShapes =
    [
        (0.375, 0.625),
        (0, 0.625),
        (0.375, 1),
        (0, 1),
    ];

    /// <summary>The two-of-four values, in the order the standard numbers them one to six.</summary>
    private static readonly int[] TwoOfFour = [0b0011, 0b0101, 0b0110, 0b1001, 0b1010, 0b1100];

    /// <summary>
    /// Each character's four bars as shape indexes into <see cref="Rm4sccShapes"/>.
    /// </summary>
    /// <remarks>
    /// The table is built rather than typed out: the Royal Mail code numbers its characters in a
    /// six-by-six grid, and a character's bars are the grid row and column written in a two-of-four
    /// code, the row driving the ascenders and the column the descenders. Declared after
    /// <see cref="TwoOfFour"/> because static fields initialise in source order.
    /// </remarks>
    private static readonly int[][] Rm4sccPatterns = BuildRm4sccPatterns();

    internal static BarcodeSymbol EncodePostnet(string value)
    {
        var digits = BarcodeEncoder.DigitsOnly(value, BarcodeType.Postnet);

        if (digits.Length is not (5 or 9 or 11))
        {
            throw new BarcodeFormatException(
                $"POSTNET carries a 5-digit ZIP, a 9-digit ZIP+4 or an 11-digit delivery point code. This value has {digits.Length} digits.",
                nameof(value),
                BarcodeFormatError.PostnetWrongLength,
                digits.Length);
        }

        // The check digit brings the sum of every digit up to the next multiple of ten.
        var sum = digits.Sum(d => d - '0');
        digits += (char)('0' + ((10 - (sum % 10)) % 10));

        var builder = new BarcodeBuilder();
        AppendPostnetBar(builder, tall: true);

        foreach (var digit in digits)
        {
            foreach (var bar in PostnetPatterns[digit - '0'])
            {
                AppendPostnetBar(builder, bar == '1');
            }
        }

        AppendPostnetBar(builder, tall: true);

        return new BarcodeSymbol(
            BarcodeType.Postnet,
            digits,
            builder.Position - 1,
            9,
            builder.ToBars(),
            [new BarcodeTextGroup(digits, 0, builder.Position - 1)]);
    }

    private static void AppendPostnetBar(BarcodeBuilder builder, bool tall)
    {
        builder.Bar(1, tall ? 0 : PostnetShortTop, 1);

        // Bars sit on a two-module pitch: one of bar and one of gap.
        builder.Space(1);
    }

    internal static BarcodeSymbol EncodeRm4scc(string value)
    {
        var upper = value.ToUpperInvariant();

        foreach (var c in upper)
        {
            if (!Rm4sccCharset.Contains(c, StringComparison.Ordinal))
            {
                throw new BarcodeFormatException(
                    $"The Royal Mail 4-state code holds digits and upper-case letters, and this value contains '{c}'.",
                    nameof(value),
                    BarcodeFormatError.Rm4sccInvalidCharacter,
                    c);
            }
        }

        if (upper.Length == 0)
        {
            throw new BarcodeFormatException("The Royal Mail 4-state code needs at least one character.", nameof(value), BarcodeFormatError.Rm4sccEmpty);
        }

        var builder = new BarcodeBuilder();

        // The start bar is an ascender and the stop bar a full-height bar, which is what tells a
        // sorter which way round the envelope went through.
        AppendRm4sccBar(builder, 1);

        foreach (var c in upper)
        {
            AppendRm4sccCharacter(builder, Rm4sccPatterns[Rm4sccCharset.IndexOf(c, StringComparison.Ordinal)]);
        }

        var check = Rm4sccCheckCharacter(upper);
        AppendRm4sccCharacter(builder, Rm4sccPatterns[Rm4sccCharset.IndexOf(check, StringComparison.Ordinal)]);

        AppendRm4sccBar(builder, 3);

        return new BarcodeSymbol(
            BarcodeType.Rm4scc,
            upper + check,
            builder.Position - 1,
            6,
            builder.ToBars(),
            [new BarcodeTextGroup(upper + check, 0, builder.Position - 1)]);
    }

    private static void AppendRm4sccCharacter(BarcodeBuilder builder, int[] shapes)
    {
        foreach (var shape in shapes)
        {
            AppendRm4sccBar(builder, shape);
        }
    }

    private static void AppendRm4sccBar(BarcodeBuilder builder, int shape)
    {
        var (top, bottom) = Rm4sccShapes[shape];
        builder.Bar(1, top, bottom);
        builder.Space(1);
    }

    /// <summary>
    /// Works out the check character by summing the row and column numbers of every character.
    /// </summary>
    private static char Rm4sccCheckCharacter(string value)
    {
        var rowSum = 0;
        var columnSum = 0;

        foreach (var c in value)
        {
            var index = Rm4sccCharset.IndexOf(c, StringComparison.Ordinal);
            rowSum += (index / 6) + 1;
            columnSum += (index % 6) + 1;
        }

        var row = ((rowSum - 1) % 6) + 1;
        var column = ((columnSum - 1) % 6) + 1;

        return Rm4sccCharset[((row - 1) * 6) + (column - 1)];
    }

    private static int[][] BuildRm4sccPatterns()
    {
        var patterns = new int[36][];

        for (var index = 0; index < 36; index++)
        {
            var ascenders = TwoOfFour[index / 6];
            var descenders = TwoOfFour[index % 6];
            var bars = new int[4];

            for (var bar = 0; bar < 4; bar++)
            {
                // Most significant bit first, so bar zero is the leftmost.
                var shift = 3 - bar;
                var hasAscender = ((ascenders >> shift) & 1) == 1;
                var hasDescender = ((descenders >> shift) & 1) == 1;

                bars[bar] = (hasAscender, hasDescender) switch
                {
                    (false, false) => 0,
                    (true, false) => 1,
                    (false, true) => 2,
                    _ => 3,
                };
            }

            patterns[index] = bars;
        }

        return patterns;
    }

    internal static BarcodeSymbol EncodePharmacode(string value)
    {
        if (!int.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var number)
            || number is < 3 or > 131070)
        {
            throw new BarcodeFormatException(
                $"Pharmacode carries a whole number from 3 to 131070, and this value is \"{value}\".",
                nameof(value),
                BarcodeFormatError.PharmacodeOutOfRange,
                value);
        }

        // The number is read off in binary from the bottom up, so the bars come out backwards.
        var widths = new List<int>();
        var remaining = number;
        while (remaining > 0)
        {
            if (remaining % 2 == 0)
            {
                widths.Add(3);
                remaining = (remaining - 2) / 2;
            }
            else
            {
                widths.Add(1);
                remaining = (remaining - 1) / 2;
            }
        }

        widths.Reverse();

        var builder = new BarcodeBuilder();
        for (var i = 0; i < widths.Count; i++)
        {
            if (i > 0)
            {
                builder.Space(2);
            }

            builder.Bar(widths[i]);
        }

        return new BarcodeSymbol(
            BarcodeType.Pharmacode,
            number.ToString(System.Globalization.CultureInfo.InvariantCulture),
            builder.Position,
            6,
            builder.ToBars(),
            [new BarcodeTextGroup(number.ToString(System.Globalization.CultureInfo.InvariantCulture), 0, builder.Position)]);
    }
}

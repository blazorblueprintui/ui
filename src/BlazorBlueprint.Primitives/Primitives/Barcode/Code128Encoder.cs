namespace BlazorBlueprint.Primitives.Barcode;

/// <summary>
/// Code 128: any ASCII character, and the densest of the linear codes.
/// </summary>
/// <remarks>
/// <para>
/// Every symbol character is eleven modules of three bars and three spaces. What makes it dense is
/// code set C, which packs a pair of digits into one character, so a run of digits costs half what
/// it does anywhere else. The encoder switches between the sets as it goes and switches back when
/// the run of digits ends.
/// </para>
/// <para>
/// Set A covers the control characters and set B the lower-case ones. The encoder starts in
/// whichever the data needs and only spends a character on a switch when it has to.
/// </para>
/// </remarks>
internal static class Code128Encoder
{
    /// <summary>Element widths for symbol values 0 to 106, three bars and three spaces each.</summary>
    private static readonly string[] Patterns =
    [
        "212222", "222122", "222221", "121223", "121322", "131222", "122213", "122312", "132212", "221213",
        "221312", "231212", "112232", "122132", "122231", "113222", "123122", "123221", "223211", "221132",
        "221231", "213212", "223112", "312131", "311222", "321122", "321221", "312212", "322112", "322211",
        "212123", "212321", "232121", "111323", "131123", "131321", "112313", "132113", "132311", "211313",
        "231113", "231311", "112133", "112331", "132131", "113123", "113321", "133121", "313121", "211331",
        "231131", "213113", "213311", "213131", "311123", "311321", "331121", "312113", "312311", "332111",
        "314111", "221411", "431111", "111224", "111422", "121124", "121421", "141122", "141221", "112214",
        "112412", "122114", "122411", "142112", "142211", "241211", "221114", "413111", "241112", "134111",
        "111242", "121142", "121241", "114212", "124112", "124211", "411212", "421112", "421211", "212141",
        "214121", "412121", "111143", "111341", "131141", "114113", "114311", "411113", "411311", "113141",
        "114131", "311141", "411131", "211412", "211214", "211232", "2331112",
    ];

    private const int CodeC = 99;
    private const int CodeB = 100;
    private const int CodeA = 101;
    private const int StartA = 103;
    private const int StartB = 104;
    private const int StartC = 105;
    private const int Stop = 106;

    internal static BarcodeSymbol Encode(string value)
    {
        foreach (var c in value)
        {
            if (c > 127)
            {
                throw new BarcodeFormatException(
                    $"Code 128 holds ASCII only, and this value contains '{c}'. Use a QR code for anything beyond that.",
                    nameof(value),
                    BarcodeFormatError.Code128NotAscii,
                    c);
            }
        }

        var symbols = BuildSymbols(value);

        // The check character weights each symbol by its position, the start character counting once.
        var sum = symbols[0];
        for (var i = 1; i < symbols.Count; i++)
        {
            sum += symbols[i] * i;
        }

        symbols.Add(sum % 103);
        symbols.Add(Stop);

        var builder = new BarcodeBuilder();
        foreach (var symbol in symbols)
        {
            builder.Widths(Patterns[symbol]);
        }

        return new BarcodeSymbol(
            BarcodeType.Code128,
            value,
            builder.Position,
            10,
            builder.ToBars(),
            [new BarcodeTextGroup(value, 0, builder.Position)]);
    }

    private static List<int> BuildSymbols(string value)
    {
        var symbols = new List<int>();
        var position = 0;

        // Four or more digits pay for the switch into set C straight away; exactly two, and the
        // whole value fits in one character.
        var digitRun = DigitRun(value, 0);
        var mode = digitRun >= 4 || (digitRun == value.Length && digitRun == 2) ? CodeC
            : NeedsSetA(value, 0) ? CodeA
            : CodeB;

        symbols.Add(mode switch { CodeC => StartC, CodeA => StartA, _ => StartB });

        while (position < value.Length)
        {
            if (mode == CodeC)
            {
                if (position + 1 < value.Length && char.IsAsciiDigit(value[position]) && char.IsAsciiDigit(value[position + 1]))
                {
                    symbols.Add(int.Parse(value.AsSpan(position, 2), System.Globalization.CultureInfo.InvariantCulture));
                    position += 2;
                    continue;
                }

                mode = NeedsSetA(value, position) ? CodeA : CodeB;
                symbols.Add(mode);
                continue;
            }

            // Back into C only when the run of digits is long enough to earn the switch character.
            var run = DigitRun(value, position);
            if (run >= 6 || (run >= 2 && position + run == value.Length && run % 2 == 0))
            {
                // An odd-length run starts one character late so the pairs line up.
                if (run % 2 == 1)
                {
                    symbols.Add(SymbolFor(value[position], mode));
                    position++;
                }

                mode = CodeC;
                symbols.Add(CodeC);
                continue;
            }

            if (mode == CodeB && value[position] < ' ')
            {
                mode = CodeA;
                symbols.Add(CodeA);
                continue;
            }

            if (mode == CodeA && value[position] > '_')
            {
                mode = CodeB;
                symbols.Add(CodeB);
                continue;
            }

            symbols.Add(SymbolFor(value[position], mode));
            position++;
        }

        return symbols;
    }

    /// <summary>
    /// Gets whether the character at a position can only be written in set A.
    /// </summary>
    private static bool NeedsSetA(string value, int position) =>
        position < value.Length && value[position] < ' ';

    private static int DigitRun(string value, int start)
    {
        var run = 0;
        while (start + run < value.Length && char.IsAsciiDigit(value[start + run]))
        {
            run++;
        }

        return run;
    }

    /// <summary>
    /// Maps an ASCII character to its symbol value in set A or set B.
    /// </summary>
    private static int SymbolFor(char c, int mode)
    {
        if (mode == CodeA)
        {
            // Set A runs space to underscore, then the control characters from 64 upwards.
            return c < ' ' ? c + 64 : c - ' ';
        }

        return c - ' ';
    }
}

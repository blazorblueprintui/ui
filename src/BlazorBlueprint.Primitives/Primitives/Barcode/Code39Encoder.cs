namespace BlazorBlueprint.Primitives.Barcode;

/// <summary>
/// Code 39: digits, upper-case letters and a handful of symbols, wrapped in asterisks.
/// </summary>
/// <remarks>
/// Every character is nine elements — five bars and four spaces — of which exactly three are wide.
/// That self-checking shape is why it survives bad printing, and why it is about three times wider
/// than <see cref="Code128Encoder"/> for the same data. Its check character is optional and is
/// still off by default, because most readers are not configured to expect one.
/// </remarks>
internal static class Code39Encoder
{
    /// <summary>The character set, in the order the check character counts them.</summary>
    private const string Charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%";

    /// <summary>
    /// One flag per element, bar first and alternating: <c>1</c> is wide, <c>0</c> is narrow.
    /// </summary>
    private static readonly string[] Patterns =
    [
        "000110100", "100100001", "001100001", "101100000", "000110001",
        "100110000", "001110000", "000100101", "100100100", "001100100",
        "100001001", "001001001", "101001000", "000011001", "100011000",
        "001011000", "000001101", "100001100", "001001100", "000011100",
        "100000011", "001000011", "101000010", "000010011", "100010010",
        "001010010", "000000111", "100000110", "001000110", "000010110",
        "110000001", "011000001", "111000000", "010010001", "110010000",
        "011010000", "010000101", "110000100", "011000100", "010101000",
        "010100010", "010001010", "000101010",
    ];

    /// <summary>The asterisk that starts and stops every Code 39 symbol.</summary>
    private const string Delimiter = "010010100";

    // The standard permits anything from 2:1 to 3:1. Two is the common default and a quarter
    // narrower on the label; three tolerates worse printing.
    private const int Narrow = 1;
    private const int Wide = 2;

    internal static BarcodeSymbol Encode(string value, bool addChecksum)
    {
        var upper = value.ToUpperInvariant();
        var indexes = new int[upper.Length];

        for (var i = 0; i < upper.Length; i++)
        {
            var index = Charset.IndexOf(upper[i], StringComparison.Ordinal);
            if (index < 0)
            {
                throw new BarcodeFormatException(
                    $"Code 39 holds digits, letters and - . space $ / + %, and this value contains '{value[i]}'. Use Code 128 for anything else.",
                    nameof(value),
                    BarcodeFormatError.Code39InvalidCharacter,
                    value[i]);
            }

            indexes[i] = index;
        }

        var encoded = upper;
        if (addChecksum)
        {
            var check = Charset[indexes.Sum() % 43];
            encoded += check;
            indexes = [.. indexes, Charset.IndexOf(check, StringComparison.Ordinal)];
        }

        var builder = new BarcodeBuilder();
        builder.NarrowWide(Delimiter, Narrow, Wide);

        foreach (var index in indexes)
        {
            // Characters are separated by one narrow space, which belongs to neither of them.
            builder.Space(Narrow);
            builder.NarrowWide(Patterns[index], Narrow, Wide);
        }

        builder.Space(Narrow);
        builder.NarrowWide(Delimiter, Narrow, Wide);

        return new BarcodeSymbol(
            BarcodeType.Code39,
            encoded,
            builder.Position,
            10,
            builder.ToBars(),
            [new BarcodeTextGroup($"*{encoded}*", 0, builder.Position)]);
    }
}

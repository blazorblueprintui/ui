namespace BlazorBlueprint.Primitives.Barcode;

/// <summary>
/// Telepen: any ASCII character, sixteen modules each, in only two element widths.
/// </summary>
/// <remarks>
/// <para>
/// Telepen encodes a bit stream rather than a character at a time, but because every character
/// contributes exactly eight bits and sixteen modules, each one always produces the same block
/// whatever sits either side of it. That makes a lookup table the honest description of the
/// symbology, and it is the one used here.
/// </para>
/// <para>
/// The table was derived from zint, checked against zint bar for bar, and every symbol it produces
/// was then decoded back to its original text by zxing-cpp.
/// </para>
/// </remarks>
internal static class TelepenEncoder
{
    /// <summary>Sixteen modules per character, indexed by ASCII value.</summary>
    private static readonly string[] Patterns =
    [
        "1110111011101110", "1011101110111010", "1110001110111010", "1010111011101110",
        "1110101110111010", "1011100011101110", "1000100011101110", "1010101110111010",
        "1110111000111010", "1011101011101110", "1110001011101110", "1010111000111010",
        "1110101011101110", "1010001000111010", "1000101000111010", "1010101011101110",
        "1110111010111010", "1011101110001110", "1110001110001110", "1010111010111010",
        "1110101110001110", "1011100010111010", "1000100010111010", "1010101110001110",
        "1110100010001110", "1011101010111010", "1110001010111010", "1010100010001110",
        "1110101010111010", "1010001010001110", "1000101010001110", "1010101010111010",
        "1110111011100010", "1011101110101110", "1110001110101110", "1010111011100010",
        "1110101110101110", "1011100011100010", "1000100011100010", "1010101110101110",
        "1110111000101110", "1011101011100010", "1110001011100010", "1010111000101110",
        "1110101011100010", "1010001000101110", "1000101000101110", "1010101011100010",
        "1110111010101110", "1011101000100010", "1110001000100010", "1010111010101110",
        "1110101000100010", "1011100010101110", "1000100010101110", "1010101000100010",
        "1110100010100010", "1011101010101110", "1110001010101110", "1010100010100010",
        "1110101010101110", "1010001010100010", "1000101010100010", "1010101010101110",
        "1110111011101010", "1011101110111000", "1110001110111000", "1010111011101010",
        "1110101110111000", "1011100011101010", "1000100011101010", "1010101110111000",
        "1110111000111000", "1011101011101010", "1110001011101010", "1010111000111000",
        "1110101011101010", "1010001000111000", "1000101000111000", "1010101011101010",
        "1110111010111000", "1011101110001010", "1110001110001010", "1010111010111000",
        "1110101110001010", "1011100010111000", "1000100010111000", "1010101110001010",
        "1110100010001010", "1011101010111000", "1110001010111000", "1010100010001010",
        "1110101010111000", "1010001010001010", "1000101010001010", "1010101010111000",
        "1110111010001000", "1011101110101010", "1110001110101010", "1010111010001000",
        "1110101110101010", "1011100010001000", "1000100010001000", "1010101110101010",
        "1110111000101010", "1011101010001000", "1110001010001000", "1010111000101010",
        "1110101010001000", "1010001000101010", "1000101000101010", "1010101010001000",
        "1110111010101010", "1011101000101000", "1110001000101000", "1010111010101010",
        "1110101000101000", "1011100010101010", "1000100010101010", "1010101000101000",
        "1110100010101000", "1011101010101010", "1110001010101010", "1010100010101000",
        "1110101010101010", "1010001010101000", "1000101010101000", "1010101010101010",
    ];

    private const string Start = "1010101010111000";
    private const string Stop = "1110001010101010";

    internal static BarcodeSymbol Encode(string value)
    {
        foreach (var c in value)
        {
            if (c > 127)
            {
                throw new BarcodeFormatException(
                    $"Telepen holds ASCII only, and this value contains '{c}'.",
                    nameof(value),
                    BarcodeFormatError.TelepenNotAscii,
                    c);
            }
        }

        // The check character brings the sum of every ASCII value up to a multiple of 127.
        var sum = value.Sum(c => (int)c);
        var check = (127 - (sum % 127)) % 127;

        var builder = new BarcodeBuilder();
        builder.Modules(Start);

        foreach (var c in value)
        {
            builder.Modules(Patterns[c]);
        }

        builder.Modules(Patterns[check]);
        builder.Modules(Stop);

        return new BarcodeSymbol(
            BarcodeType.Telepen,
            value,
            builder.Position,
            10,
            builder.ToBars(),
            [new BarcodeTextGroup(value, 0, builder.Position)]);
    }
}

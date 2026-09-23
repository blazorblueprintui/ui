namespace BlazorBlueprint.Primitives.DataMatrix;

/// <summary>A finished Data Matrix ECC 200 symbol as a grid of dark and light modules.</summary>
/// <remarks>The grid excludes the quiet zone. Render at least one light module around it.</remarks>
public sealed class DataMatrixSymbol
{
    private readonly bool[] modules;

    internal DataMatrixSymbol(
        int width,
        int height,
        int dataCapacity,
        int errorCodewords,
        int regionWidth,
        int regionHeight,
        DataMatrixEncodingMode mode,
        IReadOnlyList<DataMatrixEncodingMode> modesUsed,
        DataMatrixSymbolShape shape,
        bool isGs1,
        bool usesUtf8Eci,
        bool[] modules)
    {
        Width = width;
        Height = height;
        DataCapacity = dataCapacity;
        ErrorCodewords = errorCodewords;
        RegionWidth = regionWidth;
        RegionHeight = regionHeight;
        Mode = mode;
        ModesUsed = modesUsed;
        Shape = shape;
        IsGs1 = isGs1;
        UsesUtf8Eci = usesUtf8Eci;
        this.modules = modules;
    }

    /// <summary>Gets the width in modules, excluding the quiet zone.</summary>
    public int Width { get; }

    /// <summary>Gets the height in modules, excluding the quiet zone.</summary>
    public int Height { get; }

    /// <summary>Gets the number of data codewords the symbol holds.</summary>
    public int DataCapacity { get; }

    /// <summary>Gets the number of error correction codewords.</summary>
    public int ErrorCodewords { get; }

    /// <summary>Gets the width of each data region in modules, excluding its border.</summary>
    public int RegionWidth { get; }

    /// <summary>Gets the height of each data region in modules, excluding its border.</summary>
    public int RegionHeight { get; }

    /// <summary>Gets the requested encoding mode. Auto may use several modes in one symbol.</summary>
    public DataMatrixEncodingMode Mode { get; }

    /// <summary>Gets the encodation modes actually used by the payload, in first-use order.</summary>
    public IReadOnlyList<DataMatrixEncodingMode> ModesUsed { get; }

    /// <summary>Gets the actual symbol shape.</summary>
    public DataMatrixSymbolShape Shape { get; }

    /// <summary>Gets whether the symbol starts with the GS1 FNC1 codeword.</summary>
    public bool IsGs1 { get; }

    /// <summary>Gets whether UTF-8 ECI 26 precedes the payload.</summary>
    public bool UsesUtf8Eci { get; }

    /// <summary>Gets whether the module at a position is dark. Out-of-range reads as light.</summary>
    /// <param name="x">The column, starting at zero.</param>
    /// <param name="y">The row, starting at zero.</param>
    /// <returns>True when the module is dark.</returns>
    public bool this[int x, int y] =>
        x >= 0 && x < Width && y >= 0 && y < Height && modules[(y * Width) + x];
}

namespace BlazorBlueprint.Primitives.DataMatrix;

/// <summary>ECC 200 symbol attributes for the classic sizes and the DMRE rectangles.</summary>
/// <remarks>
/// Classic region dimensions and capacities follow the ECC 200 symbol table in ISO/IEC 16022. The
/// largest classic entry has eight 156-codeword data blocks and two 155-codeword data blocks. The
/// DMRE (Data Matrix Rectangular Extension) entries follow ISO/IEC 21471; each is one row of
/// regions and one Reed-Solomon block.
/// </remarks>
internal sealed record DataMatrixSymbolInfo(
    int DataCapacity,
    int ErrorCodewords,
    int RegionWidth,
    int RegionHeight,
    int RegionColumns,
    int RegionRows,
    int BlockCount,
    bool IsDmre = false)
{
    internal int Width => (RegionWidth + 2) * RegionColumns;

    internal int Height => (RegionHeight + 2) * RegionRows;

    internal int ErrorPerBlock => ErrorCodewords / BlockCount;

    internal int DataLengthForBlock(int block) =>
        DataCapacity == 1558 ? block < 8 ? 156 : 155 : DataCapacity / BlockCount;

    internal static readonly DataMatrixSymbolInfo[] Classic =
    [
        new(3, 5, 8, 8, 1, 1, 1),
        new(5, 7, 10, 10, 1, 1, 1),
        new(5, 7, 16, 6, 1, 1, 1),
        new(8, 10, 12, 12, 1, 1, 1),
        new(10, 11, 14, 6, 2, 1, 1),
        new(12, 12, 14, 14, 1, 1, 1),
        new(16, 14, 24, 10, 1, 1, 1),
        new(18, 14, 16, 16, 1, 1, 1),
        new(22, 18, 18, 18, 1, 1, 1),
        new(22, 18, 16, 10, 2, 1, 1),
        new(30, 20, 20, 20, 1, 1, 1),
        new(32, 24, 16, 14, 2, 1, 1),
        new(36, 24, 22, 22, 1, 1, 1),
        new(44, 28, 24, 24, 1, 1, 1),
        new(49, 28, 22, 14, 2, 1, 1),
        new(62, 36, 14, 14, 2, 2, 1),
        new(86, 42, 16, 16, 2, 2, 1),
        new(114, 48, 18, 18, 2, 2, 1),
        new(144, 56, 20, 20, 2, 2, 1),
        new(174, 68, 22, 22, 2, 2, 1),
        new(204, 84, 24, 24, 2, 2, 2),
        new(280, 112, 14, 14, 4, 4, 2),
        new(368, 144, 16, 16, 4, 4, 4),
        new(456, 192, 18, 18, 4, 4, 4),
        new(576, 224, 20, 20, 4, 4, 4),
        new(696, 272, 22, 22, 4, 4, 4),
        new(816, 336, 24, 24, 4, 4, 6),
        new(1050, 408, 18, 18, 6, 6, 6),
        new(1304, 496, 20, 20, 6, 6, 8),
        new(1558, 620, 22, 22, 6, 6, 10),
    ];

    /// <summary>The 18 DMRE sizes, from 8x48 to 26x64 modules.</summary>
    internal static readonly DataMatrixSymbolInfo[] Dmre =
    [
        new(18, 15, 22, 6, 2, 1, 1, true),
        new(24, 18, 14, 6, 4, 1, 1, true),
        new(32, 22, 18, 6, 4, 1, 1, true),
        new(38, 28, 22, 6, 4, 1, 1, true),
        new(49, 32, 18, 6, 6, 1, 1, true),
        new(63, 36, 22, 6, 6, 1, 1, true),
        new(43, 27, 14, 10, 4, 1, 1, true),
        new(64, 36, 20, 10, 4, 1, 1, true),
        new(62, 36, 14, 14, 4, 1, 1, true),
        new(44, 28, 16, 18, 2, 1, 1, true),
        new(56, 34, 20, 18, 2, 1, 1, true),
        new(84, 42, 14, 18, 4, 1, 1, true),
        new(72, 38, 22, 20, 2, 1, 1, true),
        new(80, 41, 22, 22, 2, 1, 1, true),
        new(108, 46, 14, 22, 4, 1, 1, true),
        new(70, 38, 18, 24, 2, 1, 1, true),
        new(90, 42, 22, 24, 2, 1, 1, true),
        new(118, 50, 14, 24, 4, 1, 1, true),
    ];

    internal static readonly DataMatrixSymbolInfo[] All = [.. Classic, .. Dmre];

    internal static bool Allows(DataMatrixSymbolInfo info, DataMatrixSymbolShape shape) => shape switch
    {
        DataMatrixSymbolShape.Auto => !info.IsDmre,
        DataMatrixSymbolShape.Square => info.Width == info.Height,
        DataMatrixSymbolShape.Rectangle => !info.IsDmre && info.Width != info.Height,
        DataMatrixSymbolShape.RectangleExtended => info.Width != info.Height,
        _ => false,
    };

    internal static int MaximumCapacity(DataMatrixSymbolShape shape) =>
        All.Where(info => Allows(info, shape)).Max(info => info.DataCapacity);

    internal static DataMatrixSymbolInfo? Choose(int count, DataMatrixSymbolShape shape) =>
        All.Where(info => count <= info.DataCapacity && Allows(info, shape))
            .OrderBy(info => info.Width * info.Height)
            .ThenBy(info => Math.Max(info.Width, info.Height))
            .FirstOrDefault();
}

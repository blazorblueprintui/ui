namespace BlazorBlueprint.Primitives.DataMatrix;

/// <summary>Classic ECC 200 symbol attributes, excluding DMRE.</summary>
/// <remarks>
/// Region dimensions and capacities follow the ECC 200 symbol table in ISO/IEC 16022. The last
/// entry has eight 156-codeword data blocks and two 155-codeword data blocks.
/// </remarks>
internal sealed record DataMatrixSymbolInfo(
    int DataCapacity,
    int ErrorCodewords,
    int RegionWidth,
    int RegionHeight,
    int RegionColumns,
    int RegionRows,
    int BlockCount)
{
    internal int Width => (RegionWidth + 2) * RegionColumns;

    internal int Height => (RegionHeight + 2) * RegionRows;

    internal int ErrorPerBlock => ErrorCodewords / BlockCount;

    internal int DataLengthForBlock(int block) =>
        DataCapacity == 1558 ? block < 8 ? 156 : 155 : DataCapacity / BlockCount;

    internal static readonly DataMatrixSymbolInfo[] All =
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

    internal static DataMatrixSymbolInfo? Choose(int count, DataMatrixSymbolShape shape) =>
        All.Where(info => count <= info.DataCapacity
                && (shape switch
                {
                    DataMatrixSymbolShape.Auto => true,
                    DataMatrixSymbolShape.Square => info.Width == info.Height,
                    DataMatrixSymbolShape.Rectangle => info.Width != info.Height,
                    _ => false,
                }))
            .OrderBy(info => info.Width * info.Height)
            .ThenBy(info => Math.Max(info.Width, info.Height))
            .FirstOrDefault();
}

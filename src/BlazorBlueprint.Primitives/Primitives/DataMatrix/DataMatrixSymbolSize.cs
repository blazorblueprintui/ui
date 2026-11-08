namespace BlazorBlueprint.Primitives.DataMatrix;

/// <summary>
/// A fixed Data Matrix ECC 200 symbol size, named rows by columns as in ISO/IEC 16022 and
/// ISO/IEC 21471, excluding the quiet zone.
/// </summary>
/// <remarks>
/// <see cref="Auto"/> picks the smallest symbol that fits, within the allowed
/// <see cref="DataMatrixSymbolShape"/>. Any other value always produces that size, which keeps a
/// series of labels identical or fits a strip of fixed height, and refuses a value that does not
/// fit it. The Dmre sizes are the ISO/IEC 21471 rectangular extension, which older scanners may
/// not read.
/// </remarks>
public enum DataMatrixSymbolSize
{
    /// <summary>The smallest symbol that fits the value, within the allowed shape.</summary>
    Auto,

    /// <summary>10 x 10 modules, 3 data codewords.</summary>
    Square10,

    /// <summary>12 x 12 modules, 5 data codewords.</summary>
    Square12,

    /// <summary>8 x 18 modules, 5 data codewords.</summary>
    Rectangle8x18,

    /// <summary>14 x 14 modules, 8 data codewords.</summary>
    Square14,

    /// <summary>8 x 32 modules, 10 data codewords.</summary>
    Rectangle8x32,

    /// <summary>16 x 16 modules, 12 data codewords.</summary>
    Square16,

    /// <summary>12 x 26 modules, 16 data codewords.</summary>
    Rectangle12x26,

    /// <summary>18 x 18 modules, 18 data codewords.</summary>
    Square18,

    /// <summary>20 x 20 modules, 22 data codewords.</summary>
    Square20,

    /// <summary>12 x 36 modules, 22 data codewords.</summary>
    Rectangle12x36,

    /// <summary>22 x 22 modules, 30 data codewords.</summary>
    Square22,

    /// <summary>16 x 36 modules, 32 data codewords.</summary>
    Rectangle16x36,

    /// <summary>24 x 24 modules, 36 data codewords.</summary>
    Square24,

    /// <summary>26 x 26 modules, 44 data codewords.</summary>
    Square26,

    /// <summary>16 x 48 modules, 49 data codewords.</summary>
    Rectangle16x48,

    /// <summary>32 x 32 modules, 62 data codewords.</summary>
    Square32,

    /// <summary>36 x 36 modules, 86 data codewords.</summary>
    Square36,

    /// <summary>40 x 40 modules, 114 data codewords.</summary>
    Square40,

    /// <summary>44 x 44 modules, 144 data codewords.</summary>
    Square44,

    /// <summary>48 x 48 modules, 174 data codewords.</summary>
    Square48,

    /// <summary>52 x 52 modules, 204 data codewords.</summary>
    Square52,

    /// <summary>64 x 64 modules, 280 data codewords.</summary>
    Square64,

    /// <summary>72 x 72 modules, 368 data codewords.</summary>
    Square72,

    /// <summary>80 x 80 modules, 456 data codewords.</summary>
    Square80,

    /// <summary>88 x 88 modules, 576 data codewords.</summary>
    Square88,

    /// <summary>96 x 96 modules, 696 data codewords.</summary>
    Square96,

    /// <summary>104 x 104 modules, 816 data codewords.</summary>
    Square104,

    /// <summary>120 x 120 modules, 1050 data codewords.</summary>
    Square120,

    /// <summary>132 x 132 modules, 1304 data codewords.</summary>
    Square132,

    /// <summary>144 x 144 modules, 1558 data codewords.</summary>
    Square144,

    /// <summary>DMRE 8 x 48 modules, 18 data codewords.</summary>
    Dmre8x48,

    /// <summary>DMRE 8 x 64 modules, 24 data codewords.</summary>
    Dmre8x64,

    /// <summary>DMRE 8 x 80 modules, 32 data codewords.</summary>
    Dmre8x80,

    /// <summary>DMRE 8 x 96 modules, 38 data codewords.</summary>
    Dmre8x96,

    /// <summary>DMRE 8 x 120 modules, 49 data codewords.</summary>
    Dmre8x120,

    /// <summary>DMRE 8 x 144 modules, 63 data codewords.</summary>
    Dmre8x144,

    /// <summary>DMRE 12 x 64 modules, 43 data codewords.</summary>
    Dmre12x64,

    /// <summary>DMRE 12 x 88 modules, 64 data codewords.</summary>
    Dmre12x88,

    /// <summary>DMRE 16 x 64 modules, 62 data codewords.</summary>
    Dmre16x64,

    /// <summary>DMRE 20 x 36 modules, 44 data codewords.</summary>
    Dmre20x36,

    /// <summary>DMRE 20 x 44 modules, 56 data codewords.</summary>
    Dmre20x44,

    /// <summary>DMRE 20 x 64 modules, 84 data codewords.</summary>
    Dmre20x64,

    /// <summary>DMRE 22 x 48 modules, 72 data codewords.</summary>
    Dmre22x48,

    /// <summary>DMRE 24 x 48 modules, 80 data codewords.</summary>
    Dmre24x48,

    /// <summary>DMRE 24 x 64 modules, 108 data codewords.</summary>
    Dmre24x64,

    /// <summary>DMRE 26 x 40 modules, 70 data codewords.</summary>
    Dmre26x40,

    /// <summary>DMRE 26 x 48 modules, 90 data codewords.</summary>
    Dmre26x48,

    /// <summary>DMRE 26 x 64 modules, 118 data codewords.</summary>
    Dmre26x64,
}

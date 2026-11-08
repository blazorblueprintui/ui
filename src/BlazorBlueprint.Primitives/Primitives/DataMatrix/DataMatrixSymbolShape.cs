namespace BlazorBlueprint.Primitives.DataMatrix;

/// <summary>Allowed shapes for a Data Matrix ECC 200 symbol.</summary>
public enum DataMatrixSymbolShape
{
    /// <summary>Pick the smallest classic square or rectangle by area. DMRE sizes are not used.</summary>
    Auto,

    /// <summary>Use a square symbol.</summary>
    Square,

    /// <summary>Use one of the six classic rectangular symbols.</summary>
    Rectangle,

    /// <summary>
    /// Use the smallest rectangle from the six classic sizes and the 18 DMRE (ISO/IEC 21471) sizes,
    /// from 8x48 to 26x64 modules.
    /// </summary>
    /// <remarks>
    /// DMRE fits more data into a low, wide strip. Older scanners may not read it, so check the
    /// readers that will scan the label before choosing it.
    /// </remarks>
    RectangleExtended,
}

namespace BlazorBlueprint.Primitives.DataMatrix;

/// <summary>Allowed shapes for a classic Data Matrix ECC 200 symbol.</summary>
public enum DataMatrixSymbolShape
{
    /// <summary>Pick the smallest available symbol by area.</summary>
    Auto,

    /// <summary>Use a square symbol.</summary>
    Square,

    /// <summary>Use one of the six classic rectangular symbols.</summary>
    Rectangle,
}

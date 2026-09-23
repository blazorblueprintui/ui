namespace BlazorBlueprint.Primitives.DataMatrix;

/// <summary>How input is packed into Data Matrix ECC 200 data codewords.</summary>
public enum DataMatrixEncodingMode
{
    /// <summary>Choose compact segments automatically.</summary>
    Auto,

    /// <summary>Default ASCII encodation, including digit pairs and upper shift.</summary>
    Ascii,

    /// <summary>Upper-case oriented C40 encodation.</summary>
    C40,

    /// <summary>Lower-case oriented Text encodation.</summary>
    Text,

    /// <summary>ANSI X12 encodation.</summary>
    X12,

    /// <summary>EDIFACT encodation.</summary>
    Edifact,

    /// <summary>Binary Base 256 encodation.</summary>
    Base256,
}

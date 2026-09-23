using System.Globalization;
using System.Net;
using System.Text;
using BlazorBlueprint.Primitives.DataMatrix;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>A Data Matrix ECC 200 code drawn as SVG, encoded entirely in C#.</summary>
/// <remarks>
/// The colours remain dark on white in both light and dark themes, preserving scanner contrast.
/// Provide <see cref="ShowValue"/> or a meaningful <see cref="AriaLabel"/> when the payload is not
/// already available as text nearby.
/// </remarks>
public partial class BbDataMatrix : ComponentBase
{
    private const int StandaloneModulePixels = 8;
    private const int AriaLabelValueLimit = 60;

    private string? svg;
    private string? encodeError;
    private DataMatrixSymbol? symbol;

    /// <summary>Gets or sets the text to encode. Nothing renders while empty.</summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>Gets or sets the encodation mode. Auto chooses compact segments.</summary>
    [Parameter]
    public DataMatrixEncodingMode EncodingMode { get; set; } = DataMatrixEncodingMode.Auto;

    /// <summary>Gets or sets the symbol shape. Rectangle uses only classic ECC 200 rectangles.</summary>
    [Parameter]
    public DataMatrixSymbolShape Shape { get; set; } = DataMatrixSymbolShape.Auto;

    /// <summary>Gets or sets whether the payload is a GS1 element string.</summary>
    /// <remarks>
    /// This adds the leading FNC1 and turns U+001D separators into FNC1. Supply actual AI data
    /// without display parentheses; this component does not parse human-readable GS1 notation.
    /// </remarks>
    [Parameter]
    public bool IsGs1 { get; set; }

    /// <summary>Gets or sets the rendered SVG width as a CSS length.</summary>
    [Parameter]
    public string Size { get; set; } = "12rem";

    /// <summary>Gets or sets the dark-module colour.</summary>
    [Parameter]
    public string Foreground { get; set; } = "#18181b";

    /// <summary>Gets or sets the background and quiet-zone colour.</summary>
    [Parameter]
    public string Background { get; set; } = "#ffffff";

    /// <summary>Gets or sets the quiet zone width in modules. The default is one module.</summary>
    [Parameter]
    public int QuietZone { get; set; } = 1;

    /// <summary>Gets or sets whether the value appears as selectable text below the symbol.</summary>
    [Parameter]
    public bool ShowValue { get; set; }

    /// <summary>Gets or sets a label shown above the symbol.</summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>Gets or sets the accessible name of the symbol.</summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>Gets or sets the callback invoked when the rendered symbol changes.</summary>
    /// <remarks>The callback receives null when the value is empty or invalid.</remarks>
    [Parameter]
    public EventCallback<DataMatrixSymbol?> OnEncoded { get; set; }

    /// <summary>Gets or sets additional CSS classes for the root element.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Gets or sets additional attributes on the root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>Gets the encoded headless symbol, or null when there is none.</summary>
    /// <remarks>Use <see cref="OnEncoded"/> when reading from a parent's markup.</remarks>
    public DataMatrixSymbol? Symbol => symbol;

    private string RootClass => ClassNames.cn("bb:inline-flex bb:flex-col bb:items-center", Class);

    private int EffectiveQuietZone => Math.Clamp(QuietZone, 0, 16);

    private string? DisplayValue => IsGs1 ? Value?.Replace("\u001d", " ␝ ", StringComparison.Ordinal) : Value;

    private string EffectiveAriaLabel => AriaLabel
        ?? (Value is not null && Value.Length <= AriaLabelValueLimit
            ? Localizer["DataMatrix.AriaLabelWithValue", DisplayValue ?? string.Empty]
            : Localizer["DataMatrix.AriaLabel"]);

    /// <summary>Returns the rendered SVG as a standalone document, or null if nothing is drawn.</summary>
    /// <returns>SVG markup with its colours and intrinsic size.</returns>
    public string? GetSvg() => svg;

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        var previous = svg;
        svg = null;
        encodeError = null;
        symbol = null;

        if (!string.IsNullOrEmpty(Value))
        {
            try
            {
                symbol = DataMatrixEncoder.Encode(Value, EncodingMode, Shape, IsGs1);
                svg = BuildSvg(symbol);
            }
            catch (ArgumentException)
            {
                // Invalid input is content, so show an alert rather than breaking a Server circuit.
                encodeError = Localizer["DataMatrix.InvalidValue"];
            }
        }

        if (OnEncoded.HasDelegate && !string.Equals(previous, svg, StringComparison.Ordinal))
        {
            await OnEncoded.InvokeAsync(symbol);
        }
    }

    private string BuildSvg(DataMatrixSymbol code)
    {
        var quiet = EffectiveQuietZone;
        var width = code.Width + (quiet * 2);
        var height = code.Height + (quiet * 2);
        var builder = new StringBuilder(code.Width * code.Height / 2);

        builder.Append(CultureInfo.InvariantCulture, $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {width} {height}\"");
        builder.Append(CultureInfo.InvariantCulture, $" width=\"{width * StandaloneModulePixels}\" height=\"{height * StandaloneModulePixels}\" role=\"img\"");
        builder.Append(CultureInfo.InvariantCulture, $" aria-label=\"{WebUtility.HtmlEncode(XmlSafe(EffectiveAriaLabel))}\"");
        builder.Append(CultureInfo.InvariantCulture, $" style=\"width:{WebUtility.HtmlEncode(XmlSafe(Size))};height:auto;max-width:100%\">");
        builder.Append(CultureInfo.InvariantCulture, $"<rect width=\"{width}\" height=\"{height}\" fill=\"{WebUtility.HtmlEncode(XmlSafe(Background))}\"/>");
        builder.Append(CultureInfo.InvariantCulture, $"<path fill=\"{WebUtility.HtmlEncode(XmlSafe(Foreground))}\" shape-rendering=\"crispEdges\" d=\"");

        for (var y = 0; y < code.Height; y++)
        {
            for (var x = 0; x < code.Width;)
            {
                if (!code[x, y])
                {
                    x++;
                    continue;
                }

                var run = 1;
                while (x + run < code.Width && code[x + run, y])
                {
                    run++;
                }

                builder.Append(CultureInfo.InvariantCulture, $"M{x + quiet} {y + quiet}h{run}v1h-{run}z");
                x += run;
            }
        }

        builder.Append("\"/></svg>");
        return builder.ToString();
    }

    private static string XmlSafe(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var rune in value.EnumerateRunes())
        {
            var codePoint = rune.Value;
            if (codePoint is 9 or 10 or 13
                || codePoint is >= 32 and <= 0xD7FF
                || codePoint is >= 0xE000 and <= 0xFFFD
                || codePoint is >= 0x10000 and <= 0x10FFFF)
            {
                builder.Append(rune);
            }
        }

        return builder.ToString();
    }
}

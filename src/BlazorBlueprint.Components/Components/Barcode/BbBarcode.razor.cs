using System.Globalization;
using System.Net;
using System.Text;
using BlazorBlueprint.Primitives.Barcode;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A linear barcode drawn as SVG.
/// </summary>
/// <remarks>
/// <para>
/// The encoding happens in C# through <see cref="BarcodeEncoder"/> — no JavaScript, no image
/// request and no dependency — so the symbol is part of the rendered markup and prints, scales and
/// copies like any other vector. That matters more here than for a QR code: a barcode is usually
/// printed, and a vector stays sharp at whatever size the label turns out to be.
/// </para>
/// <para>
/// The colours are fixed dark on white rather than following the theme, and stay that way in a dark
/// theme. A scanner measures the contrast between bar and space, and an inverted symbol reads as no
/// symbol at all on most hardware. Override <see cref="Foreground"/> and <see cref="Background"/>
/// only where the result will not be scanned, or where you have tested the scanner that will read it.
/// </para>
/// <para>
/// <see cref="ShowValue"/> is on by default. The line of text under the bars is what a person reads
/// when the scan fails, and on the retail codes its position either side of the guard bars is part
/// of the standard rather than decoration.
/// </para>
/// </remarks>
public partial class BbBarcode : ComponentBase
{
    private const int MinQuietZone = 0;
    private const int MaxQuietZone = 40;

    /// <summary>The text line is sized against the module width so it scales with the symbol.</summary>
    private const double TextHeightModules = 8;

    private const double TextGapModules = 1.5;

    /// <summary>
    /// The height of the bar area in the on-page SVG's own units. Any value works, because
    /// preserveAspectRatio is none and CSS decides the rendered height.
    /// </summary>
    private const double BarAreaUnits = 100;

    /// <summary>The bar height a standalone file gets, in modules, which fixes its proportions.</summary>
    private const double StandaloneBarUnits = 50;

    /// <summary>Pixels per module in a standalone file, so it opens at a usable size.</summary>
    private const int StandalonePixelsPerModule = 3;

    private string? svg;
    private string? encodeError;
    private BarcodeSymbol? symbol;

    /// <summary>
    /// Gets or sets the value to encode. Nothing renders while this is empty.
    /// </summary>
    /// <remarks>
    /// What counts as valid depends on <see cref="Type"/>.
    /// <see cref="BarcodeEncoder.DescribeInput(BarcodeType)"/> returns a sentence describing it, for
    /// a form hint. A value the symbology cannot carry renders a message rather than a symbol.
    /// </remarks>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the symbology to encode in.
    /// </summary>
    /// <remarks>
    /// <see cref="BarcodeType.Code128"/> is the general-purpose choice: any ASCII text, and the
    /// densest of the linear codes. Use a retail or postal type only where the reader on the other
    /// end expects it.
    /// </remarks>
    [Parameter]
    public BarcodeType Type { get; set; } = BarcodeType.Code128;

    /// <summary>
    /// Gets or sets whether to add the optional check character, where the symbology has one.
    /// </summary>
    /// <remarks>
    /// Applies to <see cref="BarcodeType.Code39"/>, <see cref="BarcodeType.Itf"/>,
    /// <see cref="BarcodeType.Codabar"/> and <see cref="BarcodeType.Msi"/>. It is off by default
    /// because a reader that is not configured to expect a check character reports it as part of
    /// the data. Where the standard makes the check digit mandatory it is always added, whatever
    /// this says.
    /// </remarks>
    [Parameter]
    public bool AddChecksum { get; set; }

    /// <summary>
    /// Gets or sets the width of one module, as a CSS length.
    /// </summary>
    /// <remarks>
    /// This is what decides how wide the whole symbol is, because a barcode's width is fixed by its
    /// data. Printed at less than about 0.19mm a module falls below what most scanners resolve, so
    /// a long value needs either a wider label or a denser symbology.
    /// </remarks>
    [Parameter]
    public string ModuleWidth { get; set; } = "2px";

    /// <summary>
    /// Gets or sets the height of the bars, as a CSS length.
    /// </summary>
    /// <remarks>
    /// Height is only about how easy the symbol is to aim at — except on
    /// <see cref="BarcodeType.Postnet"/> and <see cref="BarcodeType.Rm4scc"/>, where the data is in
    /// the heights and squashing the symbol destroys it.
    /// </remarks>
    [Parameter]
    public string Height { get; set; } = "4rem";

    /// <summary>
    /// Gets or sets the colour of the bars as a CSS colour.
    /// </summary>
    [Parameter]
    public string Foreground { get; set; } = "#18181b";

    /// <summary>
    /// Gets or sets the colour behind the symbol as a CSS colour, including the quiet zone.
    /// </summary>
    [Parameter]
    public string Background { get; set; } = "#ffffff";

    /// <summary>
    /// Gets or sets the light margin each side, in modules. Defaults to what the symbology asks for.
    /// </summary>
    /// <remarks>
    /// A scanner needs this margin to find where the symbol starts and stops, and it is the most
    /// common reason a printed barcode will not read. Lower it only when the label around it is
    /// already blank.
    /// </remarks>
    [Parameter]
    public int? QuietZone { get; set; }

    /// <summary>
    /// Gets or sets whether the value is printed under the bars.
    /// </summary>
    [Parameter]
    public bool ShowValue { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the check character is included in the printed line.
    /// </summary>
    /// <remarks>
    /// Off by default: the check digit is for the scanner, and printing it can confuse someone
    /// keying the number in by hand. The retail codes ignore this and always print theirs, because
    /// there the full number is the product code.
    /// </remarks>
    [Parameter]
    public bool ShowChecksum { get; set; }

    /// <summary>
    /// Gets or sets a label shown above the barcode.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the accessible name of the symbol.
    /// </summary>
    /// <remarks>
    /// Defaults to the symbology and the encoded value. Set it to say what the code identifies —
    /// "Barcode for order 4051" beats the digits on their own.
    /// </remarks>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked whenever the encoded symbol changes.
    /// </summary>
    /// <remarks>
    /// Carries the new symbol, or <see langword="null"/> where the value is empty or invalid. Use
    /// this rather than <see cref="Symbol"/> to show the encoded value in markup: a parent builds
    /// its own markup before its children re-render, so reading <see cref="Symbol"/> there gives the
    /// previous symbol.
    /// </remarks>
    [Parameter]
    public EventCallback<BarcodeSymbol?> OnEncoded { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes for the root element.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional attributes splatted onto the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the encoded symbol, or <see langword="null"/> while the value is empty or invalid.
    /// </summary>
    /// <remarks>
    /// Read this from an event handler or from <c>OnAfterRender</c>, where it is current. Reading it
    /// from a parent's markup gives the previous symbol — use <see cref="OnEncoded"/> there.
    /// </remarks>
    public BarcodeSymbol? Symbol => symbol;

    private string RootClass => ClassNames.cn("bb:inline-flex bb:flex-col bb:items-start", Class);

    private int EffectiveQuietZone =>
        Math.Clamp(QuietZone ?? symbol?.QuietZoneModules ?? 10, MinQuietZone, MaxQuietZone);

    private string EffectiveAriaLabel =>
        AriaLabel ?? Localizer["Barcode.AriaLabel", Type.ToString(), symbol?.Value ?? string.Empty];

    /// <summary>
    /// Returns the rendered SVG markup, or <see langword="null"/> while nothing is drawn.
    /// </summary>
    /// <returns>The SVG document, ready to write to a file, print, or hand to a download.</returns>
    /// <remarks>
    /// This is not the markup on the page. A file has no CSS around it, so it gets fixed
    /// proportions and its printed line drawn inside rather than beside it.
    /// </remarks>
    public string? GetSvg() => symbol is null ? null : BuildStandaloneSvg(symbol);

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
                symbol = BarcodeEncoder.Encode(Value, Type, AddChecksum);
                svg = BuildSvg(symbol);
            }
            catch (ArgumentException error)
            {
                // A value the symbology cannot carry is a content problem, not a bug. Say what is
                // wrong in the markup rather than throwing, which on Blazor Server would take the
                // circuit down.
                //
                // Shown as it is. BarcodeFormatException.Message is the encoder's own sentence and
                // nothing else — this used to strip a parameter-name suffix off the framework's
                // composed message, which printed the raw resource key Arg_ParamName_Name on
                // WebAssembly, where that resource is trimmed away.
                //
                // A refusal that carries its reason is shown through the localizer instead, so
                // the sentence can be translated; the English keys hold the encoder's own text.
                encodeError = error is BarcodeFormatException { Error: not BarcodeFormatError.Unspecified } format
                    ? LocalizeError(format)
                    : error.Message;
            }
        }

        if (OnEncoded.HasDelegate && !string.Equals(previous, svg, StringComparison.Ordinal))
        {
            await OnEncoded.InvokeAsync(symbol);
        }
    }

    /// <summary>
    /// Words an encoder refusal through the localizer, under <c>Barcode.{reason}</c>.
    /// </summary>
    /// <remarks>
    /// <see cref="BarcodeFormatError.NotDigits"/> ends with what the symbology accepts, which
    /// comes from <c>Barcode.Input{type}</c> so that half is translated too.
    /// </remarks>
    private string LocalizeError(BarcodeFormatException error)
    {
        var arguments = error.Arguments.ToList();

        if (error.Error == BarcodeFormatError.NotDigits && arguments.Count > 0 && arguments[0] is BarcodeType type)
        {
            arguments.Add(Localizer[$"Barcode.Input{type}"]);
        }

        return Localizer[$"Barcode.{error.Error}", [.. arguments]];
    }

    /// <summary>
    /// Gets the printed run that belongs above the bars, where the symbology has one.
    /// </summary>
    /// <remarks>
    /// Only ISBN and ISSN do: the number people quote goes above, and the thirteen digits of the
    /// product code go below.
    /// </remarks>
    private BarcodeTextGroup? AboveGroup =>
        symbol is null || !ShowValue
            ? null
            : TextGroupsToDraw(symbol).Where(IsFullWidth).Cast<BarcodeTextGroup?>().FirstOrDefault();

    private IReadOnlyList<BarcodeTextGroup> BelowGroups =>
        symbol is null || !ShowValue
            ? []
            : [.. TextGroupsToDraw(symbol).Where(g => !IsFullWidth(g) || AboveGroup is null)];

    /// <summary>
    /// Gets whether a run spans the whole symbol, which is what marks the ISBN and ISSN line.
    /// </summary>
    private bool IsFullWidth(BarcodeTextGroup group) =>
        symbol is not null
        && symbol.Type is BarcodeType.Isbn or BarcodeType.Issn
        && group.Start <= 0
        && group.End >= symbol.ModuleCount;

    private int TotalWidth => (symbol?.ModuleCount ?? 0) + (EffectiveQuietZone * 2);

    private string SymbolWidthStyle =>
        FormattableString.Invariant($"calc({ModuleWidth} * {TotalWidth})");

    /// <summary>
    /// Gets the size of the printed line. It follows the module width so it grows with the symbol,
    /// but it is clamped at both ends: never too small to read, and never so large on a
    /// coarse-moduled postal code that it swamps the bars.
    /// </summary>
    private string TextSizeStyle =>
        FormattableString.Invariant($"clamp(0.625rem, calc({ModuleWidth} * 5), 1rem)");

    /// <summary>
    /// Turns a module position into a percentage across the whole symbol, quiet zone included.
    /// </summary>
    /// <param name="modules">The position, or the width when <paramref name="span"/> is set.</param>
    /// <param name="span">
    /// <see langword="true"/> to treat the value as a width rather than a position from the left
    /// edge of the symbol.
    /// </param>
    private string Percent(double modules, bool span = false)
    {
        if (TotalWidth == 0)
        {
            return "0%";
        }

        var value = span ? modules : modules + EffectiveQuietZone;
        return FormattableString.Invariant($"{value / TotalWidth * 100:0.###}%");
    }

    /// <summary>
    /// Builds the bars. The text is drawn as HTML around this, not inside it.
    /// </summary>
    private string BuildSvg(BarcodeSymbol code)
    {
        var quiet = EffectiveQuietZone;
        var width = code.ModuleCount + (quiet * 2);
        var builder = new StringBuilder(2048);

        // preserveAspectRatio is none because a barcode's width comes from its data and its height
        // is free: the bars should stretch to whatever Height says without letterboxing.
        builder.Append(CultureInfo.InvariantCulture, $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {width} {BarAreaUnits}\"");
        builder.Append(" preserveAspectRatio=\"none\" role=\"img\"");
        builder.Append(CultureInfo.InvariantCulture, $" aria-label=\"{WebUtility.HtmlEncode(EffectiveAriaLabel)}\"");
        builder.Append(CultureInfo.InvariantCulture, $" style=\"display:block;width:100%;height:{WebUtility.HtmlEncode(Height)}\">");

        builder.Append(CultureInfo.InvariantCulture, $"<rect width=\"{width}\" height=\"{BarAreaUnits}\" fill=\"{WebUtility.HtmlEncode(Background)}\"/>");

        builder.Append(CultureInfo.InvariantCulture, $"<g fill=\"{WebUtility.HtmlEncode(Foreground)}\" shape-rendering=\"crispEdges\">");
        foreach (var bar in code.Bars)
        {
            AppendBar(builder, bar, quiet, 0);
        }

        builder.Append("</g></svg>");
        return builder.ToString();
    }

    private static void AppendBar(StringBuilder builder, BarcodeBar bar, int quiet, double offset)
    {
        var top = offset + (bar.Top * BarAreaUnits);
        var height = (bar.Bottom - bar.Top) * BarAreaUnits;

        builder.Append(CultureInfo.InvariantCulture, $"<rect x=\"{bar.Start + quiet}\" y=\"{top.ToString("0.###", CultureInfo.InvariantCulture)}\" width=\"{bar.Width}\" height=\"{height.ToString("0.###", CultureInfo.InvariantCulture)}\"/>");
    }

    /// <summary>
    /// Builds a standalone document: bars and printed line together, in true proportions.
    /// </summary>
    /// <remarks>
    /// This is not the markup on the page. On the page the bars stretch to whatever
    /// <see cref="Height"/> says and the printed line is HTML beside them, which keeps the text
    /// legible at any aspect ratio. A file has no CSS around it, so it gets a fixed, undistorted
    /// shape and its text drawn inside.
    /// </remarks>
    private string BuildStandaloneSvg(BarcodeSymbol code)
    {
        var quiet = EffectiveQuietZone;
        var width = code.ModuleCount + (quiet * 2);
        var groups = ShowValue ? TextGroupsToDraw(code) : [];
        var above = groups.Any(IsFullWidth) ? TextHeightModules + TextGapModules : 0;
        var below = groups.Count > 0 ? TextHeightModules + TextGapModules : 0;
        var height = StandaloneBarUnits + above + below;

        var builder = new StringBuilder(2048);
        builder.Append(CultureInfo.InvariantCulture, $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {width} {Round(height)}\"");
        builder.Append(CultureInfo.InvariantCulture, $" width=\"{width * StandalonePixelsPerModule}\" height=\"{Round(height * StandalonePixelsPerModule)}\" role=\"img\"");
        builder.Append(CultureInfo.InvariantCulture, $" aria-label=\"{WebUtility.HtmlEncode(EffectiveAriaLabel)}\">");

        builder.Append(CultureInfo.InvariantCulture, $"<rect width=\"{width}\" height=\"{Round(height)}\" fill=\"{WebUtility.HtmlEncode(Background)}\"/>");

        builder.Append(CultureInfo.InvariantCulture, $"<g fill=\"{WebUtility.HtmlEncode(Foreground)}\" shape-rendering=\"crispEdges\">");
        foreach (var bar in code.Bars)
        {
            var top = above + (bar.Top * StandaloneBarUnits);
            var barHeight = (bar.Bottom - bar.Top) * StandaloneBarUnits;
            builder.Append(CultureInfo.InvariantCulture, $"<rect x=\"{bar.Start + quiet}\" y=\"{Round(top)}\" width=\"{bar.Width}\" height=\"{Round(barHeight)}\"/>");
        }

        builder.Append("</g>");

        if (groups.Count > 0)
        {
            builder.Append(CultureInfo.InvariantCulture, $"<g fill=\"{WebUtility.HtmlEncode(Foreground)}\" font-family=\"ui-monospace, SFMono-Regular, Menlo, monospace\" font-size=\"{Round(TextHeightModules)}\" text-anchor=\"middle\">");

            foreach (var group in groups)
            {
                var isAbove = above > 0 && IsFullWidth(group);
                var baseline = isAbove
                    ? TextHeightModules
                    : above + StandaloneBarUnits + TextGapModules + TextHeightModules;
                var centre = ((group.Start + group.End) / 2) + quiet;

                builder.Append(CultureInfo.InvariantCulture, $"<text x=\"{Round(centre)}\" y=\"{Round(baseline)}\">{WebUtility.HtmlEncode(group.Text)}</text>");
            }

            builder.Append("</g>");
        }

        builder.Append("</svg>");
        return builder.ToString();
    }

    private static string Round(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    /// <summary>
    /// Drops the check character from the printed line where it was only added for the scanner.
    /// </summary>
    private IReadOnlyList<BarcodeTextGroup> TextGroupsToDraw(BarcodeSymbol code)
    {
        var groups = code.TextGroups;

        if (ShowChecksum || !AddChecksum || groups.Count != 1)
        {
            return groups;
        }

        var text = groups[0].Text;

        // Code 39 prints between asterisks, so the check character is the one before the last.
        var trimmed = code.Type == BarcodeType.Code39 && text.Length > 2
            ? text[..^2] + text[^1]
            : text.Length > 1 ? text[..^1] : text;

        return [groups[0] with { Text = trimmed }];
    }
}

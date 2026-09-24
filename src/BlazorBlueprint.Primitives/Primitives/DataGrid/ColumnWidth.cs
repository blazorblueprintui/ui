using System.Globalization;

namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// The one place that decides whether a column width value is a width.
/// </summary>
/// <remarks>
/// A pixel width whose rounded value is not positive is not a width, it is the absence of one: a
/// resize drag reports every column's measured width, and a column with nothing to measure — a
/// control column whose header is screen-reader text only — measures zero. Zero of any other unit
/// is no width either. Positive percentages and the lengths this type cannot evaluate —
/// <c>auto</c>, <c>clamp(...)</c>, <c>var(...)</c> — are left to the browser.
/// </remarks>
public static class ColumnWidth
{
    /// <summary>
    /// The smallest pixel width that survives being rounded to whole pixels.
    /// </summary>
    public const double MinimumUsablePixels = 0.5;

    private static readonly string[] LengthUnits =
    [
        "%", "px", "em", "rem", "ex", "ch", "cap", "ic", "lh", "rlh",
        "vw", "vh", "vmin", "vmax", "dvw", "dvh", "dvmin", "dvmax",
        "svw", "svh", "svmin", "svmax", "lvw", "lvh", "lvmin", "lvmax",
        "cqw", "cqh", "cqi", "cqb", "cqmin", "cqmax",
        "cm", "mm", "q", "in", "pt", "pc"
    ];

    /// <summary>
    /// Determines whether a measured pixel width is a width at all.
    /// </summary>
    /// <param name="pixels">The measured width, in pixels.</param>
    /// <returns>
    /// True when the width is a real number that rounds to at least one pixel. False for zero,
    /// for negatives, and for the non-finite values a measurement can produce.
    /// </returns>
    public static bool IsUsablePixels(double pixels) =>
        double.IsFinite(pixels) && pixels >= MinimumUsablePixels;

    /// <summary>
    /// Formats a measured pixel width the way the column state stores it.
    /// </summary>
    /// <param name="pixels">An accepted measurement, in pixels.</param>
    /// <returns>The width as an invariant pixel string, rounded so it can never become zero.</returns>
    public static string FormatPixels(double pixels) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"{Math.Round(pixels, MidpointRounding.AwayFromZero)}px");

    /// <summary>
    /// Parses a pixel width, such as <c>150px</c>.
    /// </summary>
    /// <param name="width">The width value to parse.</param>
    /// <param name="pixels">
    /// The parsed number of pixels, or zero when the value is not a pixel width.
    /// </param>
    /// <returns>True when the value is a pixel width, whatever its magnitude.</returns>
    public static bool TryParsePixels(string? width, out double pixels)
    {
        pixels = 0;

        if (string.IsNullOrWhiteSpace(width))
        {
            return false;
        }

        var text = width.Trim();
        if (!text.EndsWith("px", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return double.TryParse(
            text.AsSpan(0, text.Length - 2),
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out pixels);
    }

    /// <summary>
    /// Determines whether a width value can be rendered as a width.
    /// </summary>
    /// <param name="width">The width value, such as <c>150px</c>, <c>20%</c> or <c>auto</c>.</param>
    /// <returns>
    /// False for a missing, empty, zero, negative or unitless width, and true for everything else —
    /// including any length this type cannot evaluate, which is left to the browser rather than
    /// second-guessed.
    /// </returns>
    public static bool IsRenderable(string? width)
    {
        if (string.IsNullOrWhiteSpace(width))
        {
            return false;
        }

        var text = width.Trim();

        if (TryParsePixels(text, out var pixels))
        {
            return IsUsablePixels(pixels);
        }

        if (TryParseLength(text, out var length))
        {
            return double.IsFinite(length) && length > 0;
        }

        if (IsNumberWithoutLengthUnit(text))
        {
            return false;
        }

        return true;
    }

    private static bool TryParseLength(string text, out double length)
    {
        length = 0;

        foreach (var unit in LengthUnits)
        {
            if (!text.EndsWith(unit, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var number = text.AsSpan(0, text.Length - unit.Length);
            if (!number.IsEmpty
                && double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out length))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNumberWithoutLengthUnit(string text)
    {
        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
        {
            return true;
        }

        var start = text[0] is '+' or '-' ? 1 : 0;

        return start < text.Length && (char.IsAsciiDigit(text[start]) || text[start] == '.');
    }

    /// <summary>
    /// Reduces a width value to what belongs in the column state: the value itself, or null when
    /// it is not a width.
    /// </summary>
    /// <param name="width">The width value to normalize.</param>
    /// <returns>The width, or null when the value is not one worth keeping.</returns>
    public static string? Normalize(string? width) => IsRenderable(width) ? width : null;
}

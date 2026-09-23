using System.Globalization;

namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// The one place that decides whether a column width value is a width.
/// </summary>
/// <remarks>
/// A pixel width whose rounded value is not positive is not a width, it is the absence of one: a
/// resize drag reports every column's measured width, and a column with nothing to measure — a
/// control column whose header is screen-reader text only — measures zero. Non-pixel lengths are
/// left alone, since <c>20%</c>, <c>auto</c> and <c>clamp(...)</c> are not ours to evaluate.
/// </remarks>
public static class ColumnWidth
{
    /// <summary>
    /// The smallest pixel width that survives being rounded to whole pixels.
    /// </summary>
    public const double MinimumUsablePixels = 0.5;

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
    /// False for a missing, empty or zero width, and true for everything else — including any
    /// length this type cannot evaluate, which is left to the browser rather than second-guessed.
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

        if (text.EndsWith('%')
            && double.TryParse(
                text.AsSpan(0, text.Length - 1),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var percent))
        {
            return percent > 0;
        }

        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var bare))
        {
            return bare > 0;
        }

        // Not a number to judge — auto, fit-content, clamp(...).
        return true;
    }

    /// <summary>
    /// Reduces a width value to what belongs in the column state: the value itself, or null when
    /// it is not a width.
    /// </summary>
    /// <param name="width">The width value to normalize.</param>
    /// <returns>The width, or null when the value is not one worth keeping.</returns>
    public static string? Normalize(string? width) => IsRenderable(width) ? width : null;
}

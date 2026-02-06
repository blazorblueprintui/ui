using System.Globalization;
using System.Text.RegularExpressions;

namespace BlazorBlueprint.Components.ColorPicker;

/// <summary>
/// Utility methods for color conversions.
/// </summary>
public static partial class ColorUtils
{
    /// <summary>
    /// Converts RGB to HSL.
    /// </summary>
    public static (double H, double S, double L) RgbToHsl(int r, int g, int b)
    {
        var rf = r / 255.0;
        var gf = g / 255.0;
        var bf = b / 255.0;

        var max = Math.Max(rf, Math.Max(gf, bf));
        var min = Math.Min(rf, Math.Min(gf, bf));
        var l = (max + min) / 2.0;

        if (max == min)
        {
            return (0, 0, l);
        }

        var d = max - min;
        var s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

        double h;
        if (max == rf)
        {
            h = (((gf - bf) / d) + (gf < bf ? 6 : 0)) / 6.0;
        }
        else if (max == gf)
        {
            h = (((bf - rf) / d) + 2) / 6.0;
        }
        else
        {
            h = (((rf - gf) / d) + 4) / 6.0;
        }

        return (h * 360, s, l);
    }

    /// <summary>
    /// Converts HSL to RGB.
    /// </summary>
    public static (int R, int G, int B) HslToRgb(double h, double s, double l)
    {
        h = h / 360.0;

        if (s == 0)
        {
            var v = (int)Math.Round(l * 255);
            return (v, v, v);
        }

        var q = l < 0.5 ? l * (1 + s) : l + s - (l * s);
        var p = (2 * l) - q;

        var r = HueToRgb(p, q, h + (1.0 / 3));
        var g = HueToRgb(p, q, h);
        var b = HueToRgb(p, q, h - (1.0 / 3));

        return ((int)Math.Round(r * 255), (int)Math.Round(g * 255), (int)Math.Round(b * 255));
    }

    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0)
        {
            t += 1;
        }

        if (t > 1)
        {
            t -= 1;
        }

        if (t < (1.0 / 6))
        {
            return p + ((q - p) * 6 * t);
        }

        if (t < (1.0 / 2))
        {
            return q;
        }

        if (t < (2.0 / 3))
        {
            return p + ((q - p) * ((2.0 / 3) - t) * 6);
        }

        return p;
    }

    /// <summary>
    /// Converts HSV to RGB.
    /// </summary>
    public static (int R, int G, int B) HsvToRgb(double h, double s, double v)
    {
        h = h / 360.0;

        var i = (int)Math.Floor(h * 6);
        var f = (h * 6) - i;
        var p = v * (1 - s);
        var q = v * (1 - (f * s));
        var t = v * (1 - ((1 - f) * s));

        double r, g, b;
        switch (i % 6)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            default: r = v; g = p; b = q; break;
        }

        return ((int)Math.Round(r * 255), (int)Math.Round(g * 255), (int)Math.Round(b * 255));
    }

    /// <summary>
    /// Converts RGB to HSV.
    /// </summary>
    public static (double H, double S, double V) RgbToHsv(int r, int g, int b)
    {
        var rf = r / 255.0;
        var gf = g / 255.0;
        var bf = b / 255.0;

        var max = Math.Max(rf, Math.Max(gf, bf));
        var min = Math.Min(rf, Math.Min(gf, bf));
        var d = max - min;

        var v = max;
        var s = max == 0 ? 0 : d / max;

        double h;
        if (max == min)
        {
            h = 0;
        }
        else if (max == rf)
        {
            h = (((gf - bf) / d) + (gf < bf ? 6 : 0)) / 6.0;
        }
        else if (max == gf)
        {
            h = (((bf - rf) / d) + 2) / 6.0;
        }
        else
        {
            h = (((rf - gf) / d) + 4) / 6.0;
        }

        return (h * 360, s, v);
    }

    /// <summary>
    /// Converts RGB to hex string.
    /// </summary>
    public static string ToHex(int r, int g, int b, int? a = null)
    {
        if (a.HasValue && a.Value < 255)
        {
            return $"#{r:X2}{g:X2}{b:X2}{a.Value:X2}";
        }
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    /// <summary>
    /// Parses a hex color string.
    /// </summary>
    public static (int R, int G, int B, int? A) ParseHex(string hex)
    {
        hex = hex.TrimStart('#');

        // Handle short hex (#RGB)
        if (hex.Length == 3)
        {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        }
        // Handle short hex with alpha (#RGBA)
        else if (hex.Length == 4)
        {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}";
        }

        if (hex.Length >= 6)
        {
            var r = int.Parse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var g = int.Parse(hex[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var b = int.Parse(hex[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            int? a = null;
            if (hex.Length >= 8)
            {
                a = int.Parse(hex[6..8], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return (r, g, b, a);
        }

        return (0, 0, 0, null);
    }

    /// <summary>
    /// Validates a hex color string.
    /// </summary>
    public static bool IsValidHex(string? hex)
    {
        if (string.IsNullOrEmpty(hex))
        {
            return false;
        }

        hex = hex.TrimStart('#');
        return (hex.Length is 3 or 4 or 6 or 8) && HexRegex().IsMatch(hex);
    }

    [GeneratedRegex("^[0-9A-Fa-f]+$")]
    private static partial Regex HexRegex();

    /// <summary>
    /// Formats a color as RGB string.
    /// </summary>
    public static string ToRgbString(int r, int g, int b, int? a = null)
    {
        if (a.HasValue && a.Value < 255)
        {
            return $"rgba({r}, {g}, {b}, {a.Value / 255.0:F2})";
        }
        return $"rgb({r}, {g}, {b})";
    }

    /// <summary>
    /// Formats a color as HSL string.
    /// </summary>
    public static string ToHslString(double h, double s, double l, int? a = null)
    {
        if (a.HasValue && a.Value < 255)
        {
            return $"hsla({h:F0}, {s * 100:F0}%, {l * 100:F0}%, {a.Value / 255.0:F2})";
        }
        return $"hsl({h:F0}, {s * 100:F0}%, {l * 100:F0}%)";
    }
}

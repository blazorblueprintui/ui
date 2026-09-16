using System.Globalization;

namespace BlazorBlueprint.Components;

internal static class DateTimeSegments
{
    internal static string[] DateOrder(CultureInfo culture)
    {
        var result = new List<string>();
        var quote = '\0';
        var escaped = false;
        foreach (var character in culture.DateTimeFormat.ShortDatePattern)
        {
            if (escaped)
            {
                escaped = false;
                continue;
            }
            if (character == '\\')
            {
                escaped = true;
                continue;
            }
            if (quote != '\0')
            {
                if (character == quote) { quote = '\0'; }
                continue;
            }
            if (character is '\'' or '"')
            {
                quote = character;
                continue;
            }
            var part = character switch { 'd' => "Day", 'M' => "Month", 'y' => "Year", _ => null };
            if (part != null && !result.Contains(part)) { result.Add(part); }
        }
        return result.Count == 3 ? result.ToArray() : ["Month", "Day", "Year"];
    }

    internal static int? Number(string? text) => int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value) ? value : null;

    internal static DateTime? Date(string year, string month, string day, DateTime? min, DateTime? max)
    {
        if (year.Length != 4 || Number(year) is not int y || Number(month) is not int m || Number(day) is not int d
            || y is < 1 or > 9999 || m is < 1 or > 12 || d < 1 || d > DateTime.DaysInMonth(y, m))
        {
            return null;
        }
        var value = new DateTime(y, m, d);
        return (min.HasValue && value < min.Value.Date) || (max.HasValue && value > max.Value.Date) ? null : value;
    }

    internal static TimeSpan? Time(string hour, string minute, string second, bool showSeconds, bool hour12, bool pm, TimeSpan? min, TimeSpan? max)
    {
        var h = Number(hour);
        var m = Number(minute);
        var s = showSeconds ? Number(second) : 0;
        if (h == null || m is null or < 0 or > 59 || s is null or < 0 or > 59
            || (hour12 ? h is < 1 or > 12 : h is < 0 or > 23))
        {
            return null;
        }
        var value = new TimeSpan(hour12 ? (h.Value % 12) + (pm ? 12 : 0) : h.Value, m.Value, s.Value);
        return (min.HasValue && value < min) || (max.HasValue && value > max) ? null : value;
    }
}

using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace BlazorBlueprint.Components;

internal static class DataExtractor
{
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> PropertyCache = new();

    internal static List<object?> ExtractValues(object? data, string dataKey)
    {
        var values = new List<object?>();

        if (data is not IEnumerable enumerable)
        {
            return values;
        }

        foreach (var item in enumerable)
        {
            if (item == null)
            {
                values.Add(null);
                continue;
            }

            var property = GetProperty(item.GetType(), dataKey);
            if (property != null)
            {
                values.Add(property.GetValue(item));
            }
            else
            {
                values.Add(null);
            }
        }

        return values;
    }

    internal static List<string> ExtractStringValues(object? data, string dataKey)
    {
        var rawValues = ExtractValues(data, dataKey);
        var strings = new List<string>(rawValues.Count);

        foreach (var value in rawValues)
        {
            strings.Add(value?.ToString() ?? string.Empty);
        }

        return strings;
    }

    internal static List<string> ExtractDistinctStringValues(object? data, string dataKey)
    {
        var rawValues = ExtractValues(data, dataKey);
        var seen = new HashSet<string>();
        var strings = new List<string>();

        foreach (var value in rawValues)
        {
            var str = value?.ToString() ?? string.Empty;
            if (seen.Add(str))
            {
                strings.Add(str);
            }
        }

        return strings;
    }

    internal static List<Dictionary<string, object?>> ExtractNameValuePairs(object? data, string nameKey, string valueKey)
    {
        var pairs = new List<Dictionary<string, object?>>();

        if (data is not IEnumerable enumerable)
        {
            return pairs;
        }

        foreach (var item in enumerable)
        {
            if (item == null)
            {
                continue;
            }

            var nameProperty = GetProperty(item.GetType(), nameKey);
            var valueProperty = GetProperty(item.GetType(), valueKey);

            var pair = new Dictionary<string, object?>
            {
                ["name"] = nameProperty?.GetValue(item)?.ToString() ?? string.Empty,
                ["value"] = valueProperty?.GetValue(item)
            };

            pairs.Add(pair);
        }

        return pairs;
    }

    private static PropertyInfo? GetProperty(Type type, string propertyName)
    {
        return PropertyCache.GetOrAdd((type, propertyName), key =>
            key.Item1.GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase));
    }
}

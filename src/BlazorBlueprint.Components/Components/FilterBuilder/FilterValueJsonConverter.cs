using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorBlueprint.Components;

/// <summary>
/// Custom JSON converter for the polymorphic <c>Value</c> and <c>ValueEnd</c> properties
/// on <see cref="FilterCondition"/>. Handles string, double, bool, DateTime, string[], int,
/// and <see cref="InLastPeriod"/> values.
/// </summary>
public class FilterValueJsonConverter : JsonConverter<object?>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var stringValue = reader.GetString();
                // Only parse the ISO 8601 round-trip format written by Write() to avoid
                // coercing plain strings that happen to look like dates.
                if (DateTime.TryParseExact(stringValue, "O", CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out var dateValue))
                {
                    return dateValue;
                }
                // InLastPeriod values are kept as strings here; callers interpret
                // them based on the filter operator when needed.
                return stringValue;

            case JsonTokenType.Number:
                if (reader.TryGetInt32(out var intValue))
                {
                    return intValue;
                }
                return reader.GetDouble();

            case JsonTokenType.True:
                return true;

            case JsonTokenType.False:
                return false;

            case JsonTokenType.Null:
                return null;

            case JsonTokenType.StartArray:
                var list = new List<string>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        list.Add(reader.GetString() ?? "");
                    }
                }
                return list.ToArray();

            default:
                throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                break;
            case string s:
                writer.WriteStringValue(s);
                break;
            case int i:
                writer.WriteNumberValue(i);
                break;
            case double d:
                writer.WriteNumberValue(d);
                break;
            case float f:
                writer.WriteNumberValue(f);
                break;
            case decimal m:
                writer.WriteNumberValue(m);
                break;
            case bool b:
                writer.WriteBooleanValue(b);
                break;
            case DateTime dt:
                writer.WriteStringValue(dt.ToString("O"));
                break;
            case InLastPeriod period:
                writer.WriteStringValue(period.ToString());
                break;
            case string[] arr:
                writer.WriteStartArray();
                foreach (var item in arr)
                {
                    writer.WriteStringValue(item);
                }
                writer.WriteEndArray();
                break;
            case IEnumerable<string> enumerable:
                writer.WriteStartArray();
                foreach (var item in enumerable)
                {
                    writer.WriteStringValue(item);
                }
                writer.WriteEndArray();
                break;
            default:
                JsonSerializer.Serialize(writer, value, value.GetType(), options);
                break;
        }
    }
}

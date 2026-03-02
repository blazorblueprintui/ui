using System.Globalization;
using System.Text.Json;

namespace BlazorBlueprint.Components;

/// <summary>
/// Parses JSON into <see cref="FormSchema"/> definitions.
/// Supports two formats: native FormSchema JSON and JSON Schema (subset).
/// </summary>
public static class FormSchemaParser
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Parses a JSON string into a <see cref="FormSchema"/>.
    /// Auto-detects whether the input is a native FormSchema JSON or a JSON Schema.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>A <see cref="FormSchema"/> instance.</returns>
    /// <exception cref="JsonException">The JSON is invalid or cannot be parsed.</exception>
    public static FormSchema FromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Detect JSON Schema format: has "type": "object" and "properties"
        if (root.TryGetProperty("type", out var typeElement) &&
            typeElement.GetString() == "object" &&
            root.TryGetProperty("properties", out _))
        {
            return ParseJsonSchema(root);
        }

        // Native FormSchema format
        return ParseNativeSchema(json);
    }

    // ── Native FormSchema Parsing ────────────────────────────────────

    private static FormSchema ParseNativeSchema(string json)
    {
        // Use a DTO to handle the different casing/format possibilities
        var dto = JsonSerializer.Deserialize<NativeSchemaDto>(json, jsonOptions)
            ?? throw new JsonException("Failed to deserialize FormSchema from JSON.");

        var schema = new FormSchema
        {
            Title = dto.Title,
            Description = dto.Description,
            Columns = dto.Columns
        };

        if (dto.Sections is not null)
        {
            foreach (var sectionDto in dto.Sections)
            {
                schema.Sections.Add(ParseNativeSection(sectionDto));
            }
        }

        if (dto.Fields is not null)
        {
            foreach (var fieldDto in dto.Fields)
            {
                schema.Fields.Add(ParseNativeField(fieldDto));
            }
        }

        return schema;
    }

    private static FormSectionDefinition ParseNativeSection(NativeSectionDto dto)
    {
        var section = new FormSectionDefinition
        {
            Title = dto.Title,
            Description = dto.Description,
            Columns = dto.Columns,
            VisibleWhen = dto.VisibleWhen,
            Collapsible = dto.Collapsible ?? false,
            DefaultExpanded = dto.DefaultExpanded ?? true
        };

        if (dto.Fields is not null)
        {
            foreach (var fieldDto in dto.Fields)
            {
                section.Fields.Add(ParseNativeField(fieldDto));
            }
        }

        return section;
    }

    private static FormFieldDefinition ParseNativeField(NativeFieldDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new JsonException("Field 'name' is required and cannot be empty.");
        }

        var field = new FormFieldDefinition
        {
            Name = dto.Name,
            Label = dto.Label,
            Description = dto.Description,
            Placeholder = dto.Placeholder,
            Required = dto.Required ?? false,
            Disabled = dto.Disabled ?? false,
            ReadOnly = dto.ReadOnly ?? false,
            Order = dto.Order,
            ColSpan = dto.ColSpan ?? 1,
            VisibleWhen = dto.VisibleWhen
        };

        // Parse type from string
        if (dto.Type is not null && Enum.TryParse<FieldType>(dto.Type, ignoreCase: true, out var fieldType))
        {
            field.Type = fieldType;
        }

        // Parse default value
        if (dto.DefaultValue.HasValue)
        {
            field.DefaultValue = ParseJsonValue(dto.DefaultValue.Value);
        }

        // Parse options
        if (dto.Options is not null)
        {
            field.Options = new List<SelectOption<string>>();
            foreach (var optionElement in dto.Options)
            {
                if (optionElement.ValueKind == JsonValueKind.Object)
                {
                    var optValue = optionElement.TryGetProperty("value", out var v) ? v.GetString() ?? "" : "";
                    var optText = optionElement.TryGetProperty("text", out var t) ? t.GetString() ?? optValue : optValue;
                    field.Options.Add(new SelectOption<string>(optValue, optText));
                }
                else if (optionElement.ValueKind == JsonValueKind.String)
                {
                    var str = optionElement.GetString() ?? "";
                    field.Options.Add(new SelectOption<string>(str, str));
                }
            }
        }

        // Parse validations
        if (dto.Validations is not null)
        {
            field.Validations = new List<FieldValidation>();
            foreach (var validationElement in dto.Validations)
            {
                var validation = new FieldValidation();
                if (validationElement.TryGetProperty("type", out var vType) &&
                    Enum.TryParse<ValidationType>(vType.GetString(), ignoreCase: true, out var valType))
                {
                    validation.Type = valType;
                }

                if (validationElement.TryGetProperty("value", out var vVal))
                {
                    validation.Value = ParseJsonValue(vVal);
                }

                if (validationElement.TryGetProperty("message", out var vMsg))
                {
                    validation.Message = vMsg.GetString();
                }

                field.Validations.Add(validation);
            }
        }

        // Parse metadata
        if (dto.Metadata is not null)
        {
            field.Metadata = new Dictionary<string, object>();
            foreach (var prop in dto.Metadata.Value.EnumerateObject())
            {
                var val = ParseJsonValue(prop.Value);
                if (val is not null)
                {
                    field.Metadata[prop.Name] = val;
                }
            }
        }

        return field;
    }

    // ── JSON Schema Parsing ──────────────────────────────────────────

    private static FormSchema ParseJsonSchema(JsonElement root)
    {
        var schema = new FormSchema();

        if (root.TryGetProperty("title", out var title))
        {
            schema.Title = title.GetString();
        }

        if (root.TryGetProperty("description", out var desc))
        {
            schema.Description = desc.GetString();
        }

        // Get required fields
        var requiredFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (root.TryGetProperty("required", out var required) && required.ValueKind == JsonValueKind.Array)
        {
            foreach (var req in required.EnumerateArray())
            {
                var name = req.GetString();
                if (name is not null)
                {
                    requiredFields.Add(name);
                }
            }
        }

        // Parse properties
        if (root.TryGetProperty("properties", out var properties))
        {
            var order = 0;
            foreach (var prop in properties.EnumerateObject())
            {
                var field = ParseJsonSchemaProperty(prop.Name, prop.Value, requiredFields);
                field.Order = order++;
                schema.Fields.Add(field);
            }
        }

        return schema;
    }

    private static FormFieldDefinition ParseJsonSchemaProperty(string name, JsonElement property, HashSet<string> requiredFields)
    {
        var field = new FormFieldDefinition
        {
            Name = name,
            Required = requiredFields.Contains(name)
        };

        // Title → Label
        if (property.TryGetProperty("title", out var title))
        {
            field.Label = title.GetString();
        }
        else
        {
            // Auto-generate label from property name
            field.Label = PascalCaseToTitle(name);
        }

        // Description
        if (property.TryGetProperty("description", out var desc))
        {
            field.Description = desc.GetString();
        }

        // Default value
        if (property.TryGetProperty("default", out var defaultVal))
        {
            field.DefaultValue = ParseJsonValue(defaultVal);
        }

        // Determine field type
        var jsonType = property.TryGetProperty("type", out var typeEl) ? typeEl.GetString() : null;
        var format = property.TryGetProperty("format", out var formatEl) ? formatEl.GetString() : null;
        var hasEnum = property.TryGetProperty("enum", out var enumEl);

        if (hasEnum)
        {
            field.Type = FieldType.Select;
            field.Options = new List<SelectOption<string>>();
            foreach (var item in enumEl.EnumerateArray())
            {
                var val = item.GetString() ?? item.ToString();
                field.Options.Add(new SelectOption<string>(val, val));
            }
        }
        else
        {
            field.Type = (jsonType, format) switch
            {
                ("string", "email") => FieldType.Email,
                ("string", "uri") or ("string", "url") => FieldType.Url,
                ("string", "date") => FieldType.Date,
                ("string", "date-time") => FieldType.DateTime,
                ("string", "time") => FieldType.Time,
                ("string", "password") => FieldType.Password,
                ("string", _) => FieldType.Text,
                ("integer", _) or ("number", _) => FieldType.Number,
                ("boolean", _) => FieldType.Checkbox,
                ("array", _) => FieldType.Tags,
                _ => FieldType.Text
            };
        }

        // Validation rules from JSON Schema keywords
        field.Validations = new List<FieldValidation>();

        if (property.TryGetProperty("minLength", out var minLen))
        {
            field.Validations.Add(new FieldValidation
            {
                Type = ValidationType.MinLength,
                Value = minLen.GetInt32()
            });
        }

        if (property.TryGetProperty("maxLength", out var maxLen))
        {
            field.Validations.Add(new FieldValidation
            {
                Type = ValidationType.MaxLength,
                Value = maxLen.GetInt32()
            });
        }

        if (property.TryGetProperty("minimum", out var min))
        {
            field.Validations.Add(new FieldValidation
            {
                Type = ValidationType.Min,
                Value = min.GetDouble()
            });
        }

        if (property.TryGetProperty("maximum", out var max))
        {
            field.Validations.Add(new FieldValidation
            {
                Type = ValidationType.Max,
                Value = max.GetDouble()
            });
        }

        if (property.TryGetProperty("pattern", out var pattern))
        {
            field.Validations.Add(new FieldValidation
            {
                Type = ValidationType.Pattern,
                Value = pattern.GetString()
            });
        }

        if (format == "email")
        {
            field.Validations.Add(new FieldValidation { Type = ValidationType.Email });
        }

        if (format is "uri" or "url")
        {
            field.Validations.Add(new FieldValidation { Type = ValidationType.Url });
        }

        // Remove empty validations list
        if (field.Validations.Count == 0)
        {
            field.Validations = null;
        }

        return field;
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static object? ParseJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }

    private static string PascalCaseToTitle(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        var chars = new List<char> { char.ToUpper(name[0], CultureInfo.InvariantCulture) };
        for (var i = 1; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0 && !char.IsUpper(name[i - 1]))
            {
                chars.Add(' ');
            }

            chars.Add(name[i]);
        }

        return new string(chars.ToArray());
    }

    // ── Native Schema DTOs ───────────────────────────────────────────

    private sealed class NativeSchemaDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? Columns { get; set; }
        public List<NativeSectionDto>? Sections { get; set; }
        public List<NativeFieldDto>? Fields { get; set; }
    }

    private sealed class NativeSectionDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? Columns { get; set; }
        public string? VisibleWhen { get; set; }
        public bool? Collapsible { get; set; }
        public bool? DefaultExpanded { get; set; }
        public List<NativeFieldDto>? Fields { get; set; }
    }

    private sealed class NativeFieldDto
    {
        public string? Name { get; set; }
        public string? Label { get; set; }
        public string? Description { get; set; }
        public string? Placeholder { get; set; }
        public string? Type { get; set; }
        public bool? Required { get; set; }
        public bool? Disabled { get; set; }
        public bool? ReadOnly { get; set; }
        public JsonElement? DefaultValue { get; set; }
        public int? Order { get; set; }
        public int? ColSpan { get; set; }
        public string? VisibleWhen { get; set; }
        public List<JsonElement>? Options { get; set; }
        public List<JsonElement>? Validations { get; set; }
        public JsonElement? Metadata { get; set; }
    }
}

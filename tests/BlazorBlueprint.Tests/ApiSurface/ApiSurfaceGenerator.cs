using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Tests.ApiSurface;

/// <summary>
/// Generates a deterministic text representation of the public API surface
/// for a given assembly. Used with Verify snapshot testing to detect
/// unintended breaking changes to component parameters, enums, and services.
/// </summary>
public static class ApiSurfaceGenerator
{
    /// <summary>
    /// Generates the full API surface text for the given assembly.
    /// The output is sorted deterministically so that snapshot comparisons
    /// are stable regardless of reflection ordering.
    /// </summary>
    public static string Generate(Assembly assembly)
    {
        var sb = new StringBuilder();
        var assemblyName = assembly.GetName().Name;

        sb.AppendLine($"# {assemblyName} - API Surface");
        sb.AppendLine();

        var exportedTypes = assembly.GetExportedTypes()
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToList();

        WriteComponents(sb, exportedTypes);
        WriteEnums(sb, exportedTypes);
        WriteServiceInterfaces(sb, exportedTypes);

        return sb.ToString().TrimEnd();
    }

    private static void WriteComponents(StringBuilder sb, List<Type> types)
    {
        var componentTypes = types
            .Where(t => typeof(ComponentBase).IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();

        if (componentTypes.Count == 0)
            return;

        sb.AppendLine("## Components");
        sb.AppendLine();

        foreach (var type in componentTypes)
        {
            sb.AppendLine($"### {type.Name} ({type.Namespace})");

            var parameters = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<ParameterAttribute>() != null)
                .OrderBy(p => p.Name, StringComparer.Ordinal)
                .ToList();

            if (parameters.Count > 0)
            {
                foreach (var prop in parameters)
                {
                    var paramAttr = prop.GetCustomAttribute<ParameterAttribute>()!;
                    var suffix = paramAttr.CaptureUnmatchedValues
                        ? " [CaptureUnmatchedValues]"
                        : "";
                    sb.AppendLine($"  - {prop.Name} : {FormatTypeName(prop.PropertyType)}{suffix}");
                }
            }

            sb.AppendLine();
        }
    }

    private static void WriteEnums(StringBuilder sb, List<Type> types)
    {
        var enumTypes = types
            .Where(t => t.IsEnum)
            .ToList();

        if (enumTypes.Count == 0)
            return;

        sb.AppendLine("## Enums");
        sb.AppendLine();

        foreach (var type in enumTypes)
        {
            sb.AppendLine($"### {type.Name} ({type.Namespace})");

            foreach (var name in Enum.GetNames(type))
            {
                var value = Convert.ToInt32(Enum.Parse(type, name));
                sb.AppendLine($"  - {name} = {value}");
            }

            sb.AppendLine();
        }
    }

    private static void WriteServiceInterfaces(StringBuilder sb, List<Type> types)
    {
        var interfaces = types
            .Where(t => t.IsInterface)
            .ToList();

        if (interfaces.Count == 0)
            return;

        sb.AppendLine("## Interfaces");
        sb.AppendLine();

        foreach (var type in interfaces)
        {
            var displayName = type.IsGenericType
                ? FormatTypeName(type)
                : type.Name;

            sb.AppendLine($"### {displayName} ({type.Namespace})");

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(p => p.Name, StringComparer.Ordinal)
                .ToList();

            foreach (var prop in properties)
            {
                var accessors = new List<string>();
                if (prop.GetMethod != null) accessors.Add("get");
                if (prop.SetMethod != null) accessors.Add("set");
                var accessorStr = accessors.Count > 0 ? $" {{ {string.Join("; ", accessors)}; }}" : "";
                sb.AppendLine($"  - {prop.Name} : {FormatTypeName(prop.PropertyType)}{accessorStr}");
            }

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName) // Exclude property getters/setters
                .OrderBy(m => m.Name, StringComparer.Ordinal)
                .ThenBy(m => m.GetParameters().Length)
                .ToList();

            foreach (var method in methods)
            {
                var paramList = string.Join(", ", method.GetParameters()
                    .Select(p => $"{FormatTypeName(p.ParameterType)} {p.Name}"));
                sb.AppendLine($"  - {method.Name}({paramList}) : {FormatTypeName(method.ReturnType)}");
            }

            sb.AppendLine();
        }
    }

    /// <summary>
    /// Formats a type name into a readable string representation.
    /// Handles generics, nullables, and nested generic types.
    /// </summary>
    public static string FormatTypeName(Type type)
    {
        // Handle nullable value types: Nullable<bool> -> Boolean?
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying != null)
            return FormatTypeName(underlying) + "?";

        // Handle generic types: EventCallback<MouseEventArgs>, Dictionary<String, Object>, etc.
        if (type.IsGenericType)
        {
            var name = type.Name.Split('`')[0];
            var args = string.Join(", ", type.GetGenericArguments().Select(FormatTypeName));
            return $"{name}<{args}>";
        }

        // Handle arrays
        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            return FormatTypeName(elementType) + "[]";
        }

        // Handle by-ref types (out/ref parameters)
        if (type.IsByRef)
        {
            var elementType = type.GetElementType()!;
            return FormatTypeName(elementType);
        }

        // Use the CLR type name (String, Boolean, Int32, etc.)
        return type.Name;
    }
}

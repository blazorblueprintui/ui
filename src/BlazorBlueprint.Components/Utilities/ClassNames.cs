using System.Collections;
using System.Text.RegularExpressions;
using TailwindMerge;

namespace BlazorBlueprint.Components;

/// <summary>
/// Provides a utility for combining CSS class names intelligently.
/// Equivalent to shadcn's cn() utility (clsx + tailwind-merge).
/// </summary>
/// <example>
/// Basic usage:
/// <code>
/// ClassNames.cn("px-4", "py-2") // "px-4 py-2"
/// </code>
///
/// Conditional classes:
/// <code>
/// ClassNames.cn("btn", isActive &amp;&amp; "btn-active") // "btn btn-active" or "btn"
/// ClassNames.cn("btn", isActive ? "active" : "inactive") // "btn active" or "btn inactive"
/// </code>
///
/// Tailwind conflict resolution:
/// <code>
/// ClassNames.cn("px-4", "px-2") // "px-2" (later value wins)
/// ClassNames.cn("p-4", "pr-10") // "p-4 pr-10" (longhand refines shorthand)
/// ClassNames.cn("pr-10", "p-4") // "p-4" (shorthand overrides earlier longhands)
/// ClassNames.cn("md:p-4", "md:p-8") // "md:p-8" (conflicts scoped per variant)
/// ClassNames.cn("text-red-500", "text-primary-foreground") // "text-primary-foreground" (semantic colors supported)
/// </code>
///
/// Array support:
/// <code>
/// ClassNames.cn(new[] { "a", "b" }) // "a b"
/// </code>
///
/// Null handling:
/// <code>
/// ClassNames.cn("a", null, "b") // "a b"
/// ClassNames.cn("a", false &amp;&amp; "b", "c") // "a c"
/// </code>
/// </example>
public static class ClassNames
{
    private static readonly char[] WhitespaceSeparators = [' ', '\t', '\n', '\r'];
    private static readonly TwMerge twMerge = new();

    // Regex to validate CSS class names - allows alphanumeric, hyphens, underscores, colons, slashes, brackets, dots, percentages, and CSS combinator characters
    // This covers Tailwind classes like "w-1/2", "hover:bg-blue-500", "data-[state=open]:block", "text-[14px]", "[&>svg]:absolute"
    private static readonly Regex ValidClassNameRegex = new(@"^[a-zA-Z0-9_\-:/.[\]()%!@#&>+~=*,' ]+$", RegexOptions.Compiled);

    /// <summary>
    /// Validates that a CSS class name contains only safe characters.
    /// Rejects classes that could be used for CSS injection attacks.
    /// </summary>
    private static bool IsValidClassName(string className)
    {
        if (string.IsNullOrWhiteSpace(className) || className.Length > 200)
        {
            return false;
        }

        // Check for potentially dangerous patterns
        if (className.Contains("expression", StringComparison.OrdinalIgnoreCase) ||
            className.Contains("javascript", StringComparison.OrdinalIgnoreCase) ||
            className.Contains("url(", StringComparison.OrdinalIgnoreCase) ||
            className.Contains("import", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return ValidClassNameRegex.IsMatch(className);
    }

    /// <summary>
    /// Combines multiple class names intelligently, handling Tailwind CSS conflicts.
    /// Equivalent to shadcn's cn() utility.
    /// </summary>
    /// <param name="inputs">
    /// Class names as strings, booleans, arrays, or conditional expressions.
    /// - Strings are included as-is
    /// - true/false booleans are converted to "true"/"false" strings (useful for data attributes)
    /// - null values are ignored
    /// - Arrays and IEnumerables are recursively processed
    /// - Conditional expressions like (condition &amp;&amp; "class") work naturally (false is ignored)
    /// </param>
    /// <returns>Merged class string with Tailwind conflicts resolved</returns>
    /// <example>
    /// <code>
    /// cn("btn", isActive &amp;&amp; "active", "px-4", "px-2") // "btn active px-2"
    /// </code>
    /// </example>
    public static string cn(params object?[] inputs)
    {
        if (inputs == null || inputs.Length == 0)
        {
            return string.Empty;
        }

        var classes = new List<string>();

        foreach (var input in inputs)
        {
            ProcessInput(input, classes);
        }

        if (classes.Count == 0)
        {
            return string.Empty;
        }

        // Use TailwindMerge.NET to resolve conflicts
        return twMerge.Merge(string.Join(" ", classes)) ?? string.Empty;
    }

    /// <summary>
    /// Recursively processes input values and extracts class names.
    /// </summary>
    private static void ProcessInput(object? input, List<string> classes)
    {
        if (input == null)
        {
            return;
        }

        // Handle strings (most common case)
        if (input is string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                // Split on whitespace to handle multi-class strings
                var parts = str.Split(WhitespaceSeparators, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    if (IsValidClassName(part))
                    {
                        classes.Add(part);
                    }
                }
            }
            return;
        }

        // Handle booleans (from conditional expressions)
        // Note: In C#, `condition && "class"` returns "class" or false, not true/false
        // But if someone explicitly passes true/false, we ignore it
        if (input is bool)
        {
            // Ignore standalone boolean values
            return;
        }

        // Handle arrays and IEnumerables (recursive processing)
        if (input is IEnumerable enumerable and not string)
        {
            foreach (var item in enumerable)
            {
                ProcessInput(item, classes);
            }
            return;
        }

        // Handle other types by converting to string
        // This covers cases like numbers, enums, etc.
        var strValue = input.ToString();
        if (!string.IsNullOrWhiteSpace(strValue) && IsValidClassName(strValue))
        {
            classes.Add(strValue);
        }
    }

    /// <summary>
    /// Returns the class string if the condition is true, otherwise returns null.
    /// Useful for conditional classes in cn().
    /// </summary>
    /// <param name="condition">The condition to evaluate</param>
    /// <param name="className">The class string to return if condition is true</param>
    /// <returns>The class string if true, null if false</returns>
    /// <example>
    /// <code>
    /// cn("btn", when(isActive, "btn-active"), "px-4")
    /// // Instead of:
    /// cn("btn", isActive ? "btn-active" : null, "px-4")
    /// </code>
    /// </example>
    public static string? when(bool condition, string className) =>
        condition ? className : null;
}

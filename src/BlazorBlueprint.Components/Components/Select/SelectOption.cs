using System.Diagnostics.CodeAnalysis;

#pragma warning disable CA1716 // Namespace matches existing Select component family
namespace BlazorBlueprint.Components;
#pragma warning restore CA1716

/// <summary>
/// Represents a value/label pair for use with the Select component's
/// <c>Options</c> parameter. Use this when the bound value differs
/// from the display text (e.g. binding to an ID while showing a name).
/// </summary>
/// <typeparam name="TValue">The type of the option value.</typeparam>
/// <param name="Value">The value bound to the Select when this option is selected.</param>
/// <param name="Text">The display text shown in the dropdown and trigger.</param>
public record SelectOption<TValue>(TValue Value, string Text);

/// <summary>
/// Factory methods for creating <see cref="SelectOption{TValue}"/> instances.
/// </summary>
public static class SelectOption
{
    /// <summary>
    /// Creates a <see cref="SelectOption{TValue}"/> where the display text
    /// is derived from <c>ToString()</c>. Useful when value and label are the same.
    /// </summary>
    public static SelectOption<TValue> FromValue<TValue>(TValue value) =>
        new(value, value?.ToString() ?? "");
}

/// <summary>
/// Extension methods for looking up display text from <see cref="SelectOption{TValue}"/> collections.
/// Eliminates the need to write per-collection helper functions.
/// </summary>
public static class SelectOptionExtensions
{
    /// <summary>
    /// Gets the display text for a single value.
    /// Returns the matching option's <see cref="SelectOption{TValue}.Text"/>,
    /// or falls back to <c>value.ToString()</c>.
    /// </summary>
    public static string GetText<TValue>(this IEnumerable<SelectOption<TValue>> options, TValue? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        return options
            .FirstOrDefault(o => EqualityComparer<TValue>.Default.Equals(o.Value, value))?.Text
            ?? value.ToString()
            ?? string.Empty;
    }

    /// <summary>
    /// Gets the display texts for multiple values.
    /// Returns the matching option's <see cref="SelectOption{TValue}.Text"/> for each value.
    /// </summary>
    public static IEnumerable<string> GetTexts<TValue>(this IEnumerable<SelectOption<TValue>> options, IEnumerable<TValue>? values)
    {
        if (values is null)
        {
            return [];
        }

        return values.Select(v => options.GetText(v));
    }
}

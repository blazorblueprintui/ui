using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace BlazorBlueprint.Components;

/// <summary>
/// Provides type conversion for <see cref="BbInputField{TValue}"/> components.
/// </summary>
/// <remarks>
/// <para>
/// Converts between string representations (for HTML input elements) and typed values.
/// Uses a three-tier resolution chain: Instance → Global → Built-in default.
/// </para>
/// <para>
/// Built-in defaults cover: string, int, long, float, double, decimal, bool,
/// DateTime, DateTimeOffset, DateOnly, TimeOnly, Guid — plus all nullable variants.
/// </para>
/// </remarks>
/// <typeparam name="TValue">The target value type.</typeparam>
/// <example>
/// <code>
/// // Global registration (in Program.cs)
/// InputConverter&lt;int?&gt;.GlobalGetFunc = input =&gt;
///     string.IsNullOrEmpty(input) ? null : int.Parse(input);
///
/// // Per-component instance
/// var converter = new InputConverter&lt;decimal&gt;
/// {
///     GetFunc = input =&gt; decimal.Parse(input, CultureInfo.CurrentCulture),
///     SetFunc = value =&gt; value.ToString("C", CultureInfo.CurrentCulture)
/// };
/// </code>
/// </example>
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types",
    Justification = "Global converter registration requires per-type-argument static state (e.g., InputConverter<int>.GlobalGetFunc).")]
public class InputConverter<TValue>
{
    private static volatile Func<string, TValue>? _globalGetFunc;
    private static volatile Func<TValue, string?>? _globalSetFunc;

    /// <summary>
    /// Gets or sets the culture used for parsing and formatting.
    /// Resolution: Culture → GlobalCulture → CultureInfo.CurrentCulture.
    /// </summary>
    public CultureInfo? Culture { get; set; }

    private static volatile CultureInfo? _globalCulture;

    /// <summary>
    /// Gets or sets the global (app-wide) culture for parsing and formatting.
    /// </summary>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types",
        Justification = "Global culture setting requires per-type-argument static state.")]
    public static CultureInfo? GlobalCulture
    {
        get => _globalCulture;
        set => _globalCulture = value;
    }

    /// <summary>
    /// Gets or sets the global (app-wide) parse function for converting a string to <typeparamref name="TValue"/>.
    /// </summary>
    /// <remarks>
    /// Set once at startup in Program.cs. Applies to all <see cref="BbInputField{TValue}"/>
    /// components unless overridden by an instance-level <see cref="GetFunc"/>.
    /// </remarks>
    public static Func<string, TValue>? GlobalGetFunc
    {
        get => _globalGetFunc;
        set => _globalGetFunc = value;
    }

    /// <summary>
    /// Gets or sets the global (app-wide) format function for converting <typeparamref name="TValue"/> to a string.
    /// </summary>
    /// <remarks>
    /// Set once at startup in Program.cs. Applies to all <see cref="BbInputField{TValue}"/>
    /// components unless overridden by an instance-level <see cref="SetFunc"/>.
    /// </remarks>
    public static Func<TValue, string?>? GlobalSetFunc
    {
        get => _globalSetFunc;
        set => _globalSetFunc = value;
    }

    /// <summary>
    /// Gets or sets the instance-level parse function for converting a string to <typeparamref name="TValue"/>.
    /// </summary>
    /// <remarks>
    /// Takes highest priority in the resolution chain. Overrides both <see cref="GlobalGetFunc"/>
    /// and built-in defaults.
    /// </remarks>
    public Func<string, TValue>? GetFunc { get; set; }

    /// <summary>
    /// Gets or sets the instance-level format function for converting <typeparamref name="TValue"/> to a string.
    /// </summary>
    /// <remarks>
    /// Takes highest priority in the resolution chain. Overrides both <see cref="GlobalSetFunc"/>
    /// and built-in defaults.
    /// </remarks>
    public Func<TValue, string?>? SetFunc { get; set; }

    private CultureInfo ResolvedCulture => Culture ?? GlobalCulture ?? CultureInfo.CurrentCulture;

    /// <summary>
    /// Converts a string input to <typeparamref name="TValue"/>.
    /// </summary>
    /// <remarks>
    /// Resolution order: <see cref="GetFunc"/> → <see cref="GlobalGetFunc"/> → built-in default.
    /// Throws <see cref="InvalidOperationException"/> if no converter is available for the type.
    /// </remarks>
    /// <param name="input">The string value from the input element.</param>
    /// <returns>The parsed <typeparamref name="TValue"/>.</returns>
    /// <exception cref="FormatException">The input string is not in a valid format for the target type.</exception>
    /// <exception cref="InvalidOperationException">No converter is registered for <typeparamref name="TValue"/>.</exception>
    public TValue Get(string input)
    {
        if (GetFunc is not null)
        {
            return GetFunc(input);
        }

        if (GlobalGetFunc is not null)
        {
            return GlobalGetFunc(input);
        }

        return DefaultGet(input);
    }

    /// <summary>
    /// Converts a <typeparamref name="TValue"/> to its string representation.
    /// </summary>
    /// <remarks>
    /// Resolution order: <see cref="SetFunc"/> → <see cref="GlobalSetFunc"/> → built-in default.
    /// </remarks>
    /// <param name="value">The typed value to format.</param>
    /// <returns>The string representation, or null for null/default values.</returns>
    public string? Set(TValue value)
    {
        if (SetFunc is not null)
        {
            return SetFunc(value);
        }

        if (GlobalSetFunc is not null)
        {
            return GlobalSetFunc(value);
        }

        return DefaultSet(value);
    }

    /// <summary>
    /// Converts a <typeparamref name="TValue"/> to its string representation using a format string.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="IFormattable.ToString(string, IFormatProvider)"/> when the value implements
    /// <see cref="IFormattable"/>. Falls back to <see cref="Set(TValue)"/> if the value is null
    /// or does not implement <see cref="IFormattable"/>.
    /// Format only affects display (not parsing).
    /// </remarks>
    /// <param name="value">The typed value to format.</param>
    /// <param name="format">The format string (e.g., "yyyy-MM-dd", "N2", "C").</param>
    /// <returns>The formatted string representation.</returns>
    public string? SetWithFormat(TValue value, string format)
    {
        if (value is null)
        {
            return null;
        }

        if (value is IFormattable formattable)
        {
            return formattable.ToString(format, ResolvedCulture);
        }

        return Set(value);
    }

    private TValue DefaultGet(string input)
    {
        var targetType = typeof(TValue);
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        var isNullable = underlyingType is not null;
        var parseType = underlyingType ?? targetType;

        if (string.IsNullOrWhiteSpace(input))
        {
            return default!;
        }

        var culture = ResolvedCulture;

        object result = parseType switch
        {
            Type t when t == typeof(string) => input,
            Type t when t == typeof(int) => int.Parse(input, culture),
            Type t when t == typeof(long) => long.Parse(input, culture),
            Type t when t == typeof(float) => float.Parse(input, culture),
            Type t when t == typeof(double) => double.Parse(input, culture),
            Type t when t == typeof(decimal) => decimal.Parse(input, culture),
            Type t when t == typeof(bool) => bool.Parse(input),
            Type t when t == typeof(DateTime) => DateTime.Parse(input, culture),
            Type t when t == typeof(DateTimeOffset) => DateTimeOffset.Parse(input, culture),
            Type t when t == typeof(DateOnly) => DateOnly.Parse(input, culture),
            Type t when t == typeof(TimeOnly) => TimeOnly.Parse(input, culture),
            Type t when t == typeof(Guid) => Guid.Parse(input),
            _ => throw new InvalidOperationException(
                $"No converter registered for type '{targetType.FullName}'. " +
                $"Register a converter via InputConverter<{targetType.Name}>.GlobalGetFunc or " +
                $"provide an InputConverter<{targetType.Name}> instance with GetFunc set.")
        };

        if (isNullable)
        {
            return (TValue)result;
        }

        return (TValue)result;
    }

    private string? DefaultSet(TValue value)
    {
        if (value is null)
        {
            return null;
        }

        var targetType = typeof(TValue);
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        var parseType = underlyingType ?? targetType;

        if (parseType == typeof(string))
        {
            return value as string;
        }

        if (value is IFormattable formattable)
        {
            return formattable.ToString(null, ResolvedCulture);
        }

        return value.ToString();
    }
}

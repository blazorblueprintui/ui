namespace BlazorBlueprint.Components;

/// <summary>
/// Represents a quick-pick preset for the date range picker.
/// Can be a built-in preset (with automatic localization) or a custom entry.
/// </summary>
public class DateRangeQuickPick
{
    /// <summary>
    /// The built-in preset type, if this is a built-in preset. Null for custom presets.
    /// </summary>
    internal DateRangePreset? Preset { get; }

    /// <summary>
    /// The display label for custom presets. Null for built-in presets (label resolved via localization).
    /// </summary>
    public string? Label { get; }

    /// <summary>
    /// Factory function that produces the date range for custom presets. Null for built-in presets.
    /// </summary>
    public Func<DateRange>? RangeFactory { get; }

    private DateRangeQuickPick(DateRangePreset preset)
    {
        Preset = preset;
    }

    private DateRangeQuickPick(string label, Func<DateRange> rangeFactory)
    {
        Label = label;
        RangeFactory = rangeFactory;
    }

    /// <summary>
    /// Creates a quick-pick from a built-in preset. The label is resolved automatically via localization.
    /// </summary>
    public static DateRangeQuickPick FromPreset(DateRangePreset preset) => new(preset);

    /// <summary>
    /// Creates a fully custom quick-pick with a user-supplied label and range factory.
    /// </summary>
    public static DateRangeQuickPick Custom(string label, Func<DateRange> rangeFactory) => new(label, rangeFactory);

    /// <summary>
    /// Implicit conversion from <see cref="DateRangePreset"/> for ergonomic usage.
    /// </summary>
    public static implicit operator DateRangeQuickPick(DateRangePreset preset) => FromPreset(preset);
}

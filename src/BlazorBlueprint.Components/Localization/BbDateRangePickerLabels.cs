using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the DateRangePicker component.
/// </summary>
public sealed class BbDateRangePickerLabels
{
    [DisallowNull] public string Placeholder { get; set; } = "Select date range";
    [DisallowNull] public string QuickSelect { get; set; } = "Quick Select";
    [DisallowNull] public string SelectEndDate { get; set; } = "Select end date";
    [DisallowNull] public Func<int, string> DaysSelected { get; set; } = days => $"{days} day(s) selected";
    [DisallowNull] public string Clear { get; set; } = "Clear";
    [DisallowNull] public string Apply { get; set; } = "Apply";

    // Preset labels
    [DisallowNull] public string Today { get; set; } = "Today";
    [DisallowNull] public string Yesterday { get; set; } = "Yesterday";
    [DisallowNull] public string Last7Days { get; set; } = "Last 7 days";
    [DisallowNull] public string Last30Days { get; set; } = "Last 30 days";
    [DisallowNull] public string ThisMonth { get; set; } = "This month";
    [DisallowNull] public string LastMonth { get; set; } = "Last month";
    [DisallowNull] public string ThisYear { get; set; } = "This year";
    [DisallowNull] public string Custom { get; set; } = "Custom";
}

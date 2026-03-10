namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the DateRangePicker component.
/// </summary>
public class BbDateRangePickerLabels
{
    public string Placeholder { get; set; } = "Select date range";
    public string QuickSelect { get; set; } = "Quick Select";
    public string SelectEndDate { get; set; } = "Select end date";
    public Func<int, string> DaysSelected { get; set; } = days => $"{days} day(s) selected";
    public string Clear { get; set; } = "Clear";
    public string Apply { get; set; } = "Apply";

    // Preset labels
    public string Today { get; set; } = "Today";
    public string Yesterday { get; set; } = "Yesterday";
    public string Last7Days { get; set; } = "Last 7 days";
    public string Last30Days { get; set; } = "Last 30 days";
    public string ThisMonth { get; set; } = "This month";
    public string LastMonth { get; set; } = "Last month";
    public string ThisYear { get; set; } = "This year";
    public string Custom { get; set; } = "Custom";
}

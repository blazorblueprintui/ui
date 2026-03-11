using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the FilterBuilder and FilterCondition components.
/// </summary>
public sealed class BbFilterBuilderLabels
{
    [DisallowNull] public string FilterBuilderAriaLabel { get; set; } = "Filter builder";
    [DisallowNull] public string SelectField { get; set; } = "Select field...";
    [DisallowNull] public string RemoveCondition { get; set; } = "Remove condition";
    [DisallowNull] public string RemoveGroup { get; set; } = "Remove group";
    [DisallowNull] public string AddCondition { get; set; } = "Add condition";
    [DisallowNull] public string AddGroup { get; set; } = "Add group";
    [DisallowNull] public string FilterCondition { get; set; } = "Filter condition";
    [DisallowNull] public string EnterValue { get; set; } = "Enter value...";
    [DisallowNull] public string Min { get; set; } = "Min";
    [DisallowNull] public string And { get; set; } = "and";
    [DisallowNull] public string Max { get; set; } = "Max";
    [DisallowNull] public string Amount { get; set; } = "Amount";
    [DisallowNull] public string PickDate { get; set; } = "Pick a date";
    [DisallowNull] public string SelectValues { get; set; } = "Select values...";
    [DisallowNull] public string SelectValue { get; set; } = "Select value...";

    // Date preset labels
    [DisallowNull] public string Today { get; set; } = "today";
    [DisallowNull] public string Yesterday { get; set; } = "yesterday";
    [DisallowNull] public string Tomorrow { get; set; } = "tomorrow";
    [DisallowNull] public string ThisWeek { get; set; } = "this week";
    [DisallowNull] public string LastWeek { get; set; } = "last week";
    [DisallowNull] public string NextWeek { get; set; } = "next week";
    [DisallowNull] public string ThisMonth { get; set; } = "this month";
    [DisallowNull] public string LastMonth { get; set; } = "last month";
    [DisallowNull] public string NextMonth { get; set; } = "next month";
    [DisallowNull] public string ThisQuarter { get; set; } = "this quarter";
    [DisallowNull] public string LastQuarter { get; set; } = "last quarter";
    [DisallowNull] public string ThisYear { get; set; } = "this year";
    [DisallowNull] public string LastYear { get; set; } = "last year";

    // Period labels
    [DisallowNull] public string Days { get; set; } = "days";
    [DisallowNull] public string Weeks { get; set; } = "weeks";
    [DisallowNull] public string Months { get; set; } = "months";
    [DisallowNull] public string Hours { get; set; } = "hours";
    [DisallowNull] public string Minutes { get; set; } = "minutes";
    [DisallowNull] public string Seconds { get; set; } = "seconds";
}

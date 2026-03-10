namespace BlazorBlueprint.Primitives.Filtering;

/// <summary>
/// Defines the time period unit for the <see cref="FilterOperator.InLast"/> operator.
/// </summary>
public enum InLastPeriod
{
    /// <summary>
    /// Number of days.
    /// </summary>
    Days,

    /// <summary>
    /// Number of weeks.
    /// </summary>
    Weeks,

    /// <summary>
    /// Number of months.
    /// </summary>
    Months,

    /// <summary>
    /// Number of hours.
    /// </summary>
    Hours,

    /// <summary>
    /// Number of minutes.
    /// </summary>
    Minutes,

    /// <summary>
    /// Number of seconds.
    /// </summary>
    Seconds
}

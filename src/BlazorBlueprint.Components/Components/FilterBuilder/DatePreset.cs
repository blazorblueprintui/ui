namespace BlazorBlueprint.Components;

/// <summary>
/// Defines dynamic date presets for the <see cref="FilterOperator.DateIs"/>
/// and <see cref="FilterOperator.DateIsNot"/> operators.
/// Each preset resolves to a date range at evaluation time.
/// </summary>
public enum DatePreset
{
    /// <summary>
    /// The current day.
    /// </summary>
    Today,

    /// <summary>
    /// The previous day.
    /// </summary>
    Yesterday,

    /// <summary>
    /// The next day.
    /// </summary>
    Tomorrow,

    /// <summary>
    /// The current ISO week (Monday through Sunday).
    /// </summary>
    ThisWeek,

    /// <summary>
    /// The previous ISO week (Monday through Sunday).
    /// </summary>
    LastWeek,

    /// <summary>
    /// The next ISO week (Monday through Sunday).
    /// </summary>
    NextWeek,

    /// <summary>
    /// The current calendar month.
    /// </summary>
    ThisMonth,

    /// <summary>
    /// The previous calendar month.
    /// </summary>
    LastMonth,

    /// <summary>
    /// The next calendar month.
    /// </summary>
    NextMonth,

    /// <summary>
    /// The current calendar quarter (Q1: Jan–Mar, Q2: Apr–Jun, Q3: Jul–Sep, Q4: Oct–Dec).
    /// </summary>
    ThisQuarter,

    /// <summary>
    /// The previous calendar quarter.
    /// </summary>
    LastQuarter,

    /// <summary>
    /// The current calendar year.
    /// </summary>
    ThisYear,

    /// <summary>
    /// The previous calendar year.
    /// </summary>
    LastYear
}

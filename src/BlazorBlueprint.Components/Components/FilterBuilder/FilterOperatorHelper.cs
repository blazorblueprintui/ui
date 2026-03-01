namespace BlazorBlueprint.Components;

/// <summary>
/// Provides mappings between <see cref="FilterFieldType"/> and available <see cref="FilterOperator"/> values,
/// as well as display labels for operators.
/// </summary>
public static class FilterOperatorHelper
{
    private static readonly Dictionary<FilterFieldType, FilterOperator[]> OperatorsByType = new()
    {
        [FilterFieldType.Text] = new[]
        {
            FilterOperator.Equals,
            FilterOperator.NotEquals,
            FilterOperator.Contains,
            FilterOperator.NotContains,
            FilterOperator.StartsWith,
            FilterOperator.EndsWith,
            FilterOperator.IsEmpty,
            FilterOperator.IsNotEmpty
        },
        [FilterFieldType.Number] = new[]
        {
            FilterOperator.Equals,
            FilterOperator.NotEquals,
            FilterOperator.GreaterThan,
            FilterOperator.LessThan,
            FilterOperator.GreaterOrEqual,
            FilterOperator.LessOrEqual,
            FilterOperator.Between,
            FilterOperator.IsEmpty,
            FilterOperator.IsNotEmpty
        },
        [FilterFieldType.Date] = new[]
        {
            FilterOperator.Equals,
            FilterOperator.NotEquals,
            FilterOperator.GreaterThan,
            FilterOperator.LessThan,
            FilterOperator.Between,
            FilterOperator.InLast,
            FilterOperator.InNext,
            FilterOperator.DateIs,
            FilterOperator.DateIsNot,
            FilterOperator.IsEmpty,
            FilterOperator.IsNotEmpty
        },
        [FilterFieldType.DateTime] = new[]
        {
            FilterOperator.Equals,
            FilterOperator.NotEquals,
            FilterOperator.GreaterThan,
            FilterOperator.LessThan,
            FilterOperator.Between,
            FilterOperator.InLast,
            FilterOperator.InNext,
            FilterOperator.DateIs,
            FilterOperator.DateIsNot,
            FilterOperator.IsEmpty,
            FilterOperator.IsNotEmpty
        },
        [FilterFieldType.Boolean] = new[]
        {
            FilterOperator.IsTrue,
            FilterOperator.IsFalse
        },
        [FilterFieldType.Enum] = new[]
        {
            FilterOperator.Equals,
            FilterOperator.NotEquals,
            FilterOperator.In,
            FilterOperator.NotIn,
            FilterOperator.IsEmpty,
            FilterOperator.IsNotEmpty
        }
    };

    private static readonly Dictionary<FilterOperator, string> OperatorLabels = new()
    {
        [FilterOperator.Equals] = "equals",
        [FilterOperator.NotEquals] = "not equals",
        [FilterOperator.IsEmpty] = "is empty",
        [FilterOperator.IsNotEmpty] = "is not empty",
        [FilterOperator.Contains] = "contains",
        [FilterOperator.NotContains] = "not contains",
        [FilterOperator.StartsWith] = "starts with",
        [FilterOperator.EndsWith] = "ends with",
        [FilterOperator.GreaterThan] = "greater than",
        [FilterOperator.LessThan] = "less than",
        [FilterOperator.GreaterOrEqual] = "greater or equal",
        [FilterOperator.LessOrEqual] = "less or equal",
        [FilterOperator.Between] = "between",
        [FilterOperator.InLast] = "in the last",
        [FilterOperator.InNext] = "in the next",
        [FilterOperator.In] = "is any of",
        [FilterOperator.NotIn] = "is none of",
        [FilterOperator.IsTrue] = "is true",
        [FilterOperator.IsFalse] = "is false",
        [FilterOperator.DateIs] = "is",
        [FilterOperator.DateIsNot] = "is not"
    };

    private static readonly Dictionary<FilterOperator, string> DateOperatorLabels = new()
    {
        [FilterOperator.GreaterThan] = "is after",
        [FilterOperator.LessThan] = "is before",
    };

    /// <summary>
    /// Gets the available operators for a given field type.
    /// </summary>
    public static FilterOperator[] GetOperatorsForType(FilterFieldType type) =>
        OperatorsByType.TryGetValue(type, out var operators) ? operators : Array.Empty<FilterOperator>();

    /// <summary>
    /// Gets the display label for an operator, optionally adjusted for a specific field type.
    /// </summary>
    public static string GetOperatorLabel(FilterOperator op, FilterFieldType? fieldType = null)
    {
        if (fieldType is FilterFieldType.Date or FilterFieldType.DateTime
            && DateOperatorLabels.TryGetValue(op, out var dateLabel))
        {
            return dateLabel;
        }

        return OperatorLabels.TryGetValue(op, out var label) ? label : op.ToString();
    }

    /// <summary>
    /// Gets <see cref="SelectOption{TValue}"/> items for the operators of a given field type.
    /// </summary>
    public static IEnumerable<SelectOption<FilterOperator>> GetOperatorOptions(FilterFieldType type)
    {
        return GetOperatorsForType(type)
            .Select(op => new SelectOption<FilterOperator>(op, GetOperatorLabel(op, type)));
    }

    /// <summary>
    /// Returns true if the operator requires no value input (the operator itself implies the value).
    /// </summary>
    public static bool IsValuelessOperator(FilterOperator op)
    {
        return op is FilterOperator.IsEmpty
            or FilterOperator.IsNotEmpty
            or FilterOperator.IsTrue
            or FilterOperator.IsFalse;
    }

    /// <summary>
    /// Returns true if the operator requires two values (e.g. Between).
    /// </summary>
    public static bool IsRangeOperator(FilterOperator op) =>
        op is FilterOperator.Between;

    /// <summary>
    /// Returns true if the operator uses a <see cref="DatePreset"/> dropdown for its value input.
    /// </summary>
    public static bool IsDatePresetOperator(FilterOperator op) =>
        op is FilterOperator.DateIs or FilterOperator.DateIsNot;
}

namespace BlazorBlueprint.Components;

/// <summary>
/// Provides mappings between <see cref="FilterFieldType"/> and available <see cref="FilterOperator"/> values,
/// as well as display labels for operators.
/// </summary>
public static class FilterOperatorHelper
{
    private static readonly Dictionary<FilterFieldType, FilterOperator[]> OperatorsByType = new()
    {
        [FilterFieldType.String] = new[]
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
        [FilterOperator.NotEquals] = "does not equal",
        [FilterOperator.IsEmpty] = "is empty",
        [FilterOperator.IsNotEmpty] = "is not empty",
        [FilterOperator.Contains] = "contains",
        [FilterOperator.NotContains] = "does not contain",
        [FilterOperator.StartsWith] = "starts with",
        [FilterOperator.EndsWith] = "ends with",
        [FilterOperator.GreaterThan] = "is greater than",
        [FilterOperator.LessThan] = "is less than",
        [FilterOperator.GreaterOrEqual] = "is greater than or equal to",
        [FilterOperator.LessOrEqual] = "is less than or equal to",
        [FilterOperator.Between] = "is between",
        [FilterOperator.InLast] = "is in the last",
        [FilterOperator.In] = "is any of",
        [FilterOperator.NotIn] = "is none of",
        [FilterOperator.IsTrue] = "is true",
        [FilterOperator.IsFalse] = "is false"
    };

    private static readonly Dictionary<FilterOperator, string> DateOperatorLabels = new()
    {
        [FilterOperator.GreaterThan] = "is after",
        [FilterOperator.LessThan] = "is before",
    };

    /// <summary>
    /// Gets the available operators for a given field type.
    /// </summary>
    public static FilterOperator[] GetOperatorsForType(FilterFieldType type)
    {
        return OperatorsByType.TryGetValue(type, out var operators) ? operators : Array.Empty<FilterOperator>();
    }

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
    public static bool IsRangeOperator(FilterOperator op)
    {
        return op is FilterOperator.Between;
    }
}

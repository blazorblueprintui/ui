using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorBlueprint.Components;

/// <summary>
/// Extension methods for <see cref="FilterDefinition"/> providing filter evaluation,
/// LINQ expression building, and JSON serialization.
/// </summary>
public static class FilterDefinitionExtensions
{
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> PropertyCache = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(),
            new FilterValueJsonConverter()
        }
    };

    /// <summary>
    /// Compiles the filter into a <see cref="Func{T, TResult}"/> predicate for client-side in-memory filtering.
    /// </summary>
    /// <typeparam name="T">The type of objects to filter.</typeparam>
    /// <param name="filter">The filter definition.</param>
    /// <param name="fields">The field definitions providing type metadata.</param>
    /// <returns>A predicate that returns true if the item matches the filter.</returns>
    public static Func<T, bool> ToFunc<T>(this FilterDefinition filter, IEnumerable<FilterField> fields)
    {
        if (filter.IsEmpty)
        {
            return _ => true;
        }

        var itemType = typeof(T);
        return item => EvaluateGroup(item!, filter, itemType);
    }

    /// <summary>
    /// Builds a LINQ <see cref="Expression{TDelegate}"/> for use with EF Core or other IQueryable providers.
    /// </summary>
    /// <typeparam name="T">The type of objects to filter.</typeparam>
    /// <param name="filter">The filter definition.</param>
    /// <param name="fields">The field definitions providing type metadata.</param>
    /// <returns>An expression tree representing the filter.</returns>
    public static Expression<Func<T, bool>> ToExpression<T>(this FilterDefinition filter, IEnumerable<FilterField> fields)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        if (filter.IsEmpty)
        {
            return Expression.Lambda<Func<T, bool>>(Expression.Constant(true), parameter);
        }

        var body = BuildGroupExpression(parameter, filter);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    /// <summary>
    /// Serializes the filter definition to a JSON string.
    /// </summary>
    public static string ToJson(this FilterDefinition filter) =>
        JsonSerializer.Serialize(filter, JsonOptions);

    /// <summary>
    /// Deserializes a filter definition from a JSON string.
    /// </summary>
    public static FilterDefinition FromJson(string json) =>
        JsonSerializer.Deserialize<FilterDefinition>(json, JsonOptions) ?? new FilterDefinition();

    #region ToFunc implementation

    private static bool EvaluateGroup<T>(T item, FilterDefinition group, Type itemType)
    {
        var results = new List<bool>();

        foreach (var condition in group.Conditions)
        {
            if (string.IsNullOrEmpty(condition.Field))
            {
                continue;
            }
            results.Add(EvaluateCondition(item, condition, itemType));
        }

        foreach (var nestedGroup in group.Groups)
        {
            if (!nestedGroup.IsEmpty)
            {
                results.Add(EvaluateGroup(item, nestedGroup, itemType));
            }
        }

        if (results.Count == 0)
        {
            return true;
        }

        return group.Operator == LogicalOperator.And
            ? results.All(r => r)
            : results.Any(r => r);
    }

    private static bool EvaluateCondition<T>(T item, FilterCondition condition, Type itemType)
    {
        var prop = GetProperty(itemType, condition.Field);
        if (prop == null)
        {
            return false;
        }

        var rawValue = prop.GetValue(item);

        // Incomplete condition: user hasn't entered a value yet — ignore (match all)
        // to align with ToExpression behavior and avoid filtering out all rows mid-edit.
        if (IsIncompleteCondition(condition))
        {
            return true;
        }

        return condition.Operator switch
        {
            FilterOperator.IsEmpty => IsEmpty(rawValue),
            FilterOperator.IsNotEmpty => !IsEmpty(rawValue),
            FilterOperator.IsTrue => rawValue is true,
            FilterOperator.IsFalse => rawValue is false or null,
            FilterOperator.Equals => AreEqual(rawValue, condition.Value),
            FilterOperator.NotEquals => !AreEqual(rawValue, condition.Value),
            FilterOperator.Contains => StringOp(rawValue, condition.Value, (s, v) => s.Contains(v, StringComparison.OrdinalIgnoreCase)),
            FilterOperator.NotContains => StringOp(rawValue, condition.Value, (s, v) => !s.Contains(v, StringComparison.OrdinalIgnoreCase)),
            FilterOperator.StartsWith => StringOp(rawValue, condition.Value, (s, v) => s.StartsWith(v, StringComparison.OrdinalIgnoreCase)),
            FilterOperator.EndsWith => StringOp(rawValue, condition.Value, (s, v) => s.EndsWith(v, StringComparison.OrdinalIgnoreCase)),
            FilterOperator.GreaterThan => Compare(rawValue, condition.Value) is > 0,
            FilterOperator.LessThan => Compare(rawValue, condition.Value) is < 0,
            FilterOperator.GreaterOrEqual => Compare(rawValue, condition.Value) is >= 0,
            FilterOperator.LessOrEqual => Compare(rawValue, condition.Value) is <= 0,
            FilterOperator.Between => Compare(rawValue, condition.Value) is >= 0 && Compare(rawValue, condition.ValueEnd) is <= 0,
            FilterOperator.InLast => EvaluateInLast(rawValue, condition),
            FilterOperator.InNext => EvaluateInNext(rawValue, condition),
            FilterOperator.DateIs => EvaluateDatePreset(rawValue, condition, negate: false),
            FilterOperator.DateIsNot => EvaluateDatePreset(rawValue, condition, negate: true),
            FilterOperator.In => EvaluateIn(rawValue, condition.Value, contains: true),
            FilterOperator.NotIn => EvaluateIn(rawValue, condition.Value, contains: false),
            _ => true
        };
    }

    private static bool IsIncompleteCondition(FilterCondition condition)
    {
        // Valueless operators (IsEmpty, IsNotEmpty, IsTrue, IsFalse) never need a value.
        // Date preset operators store preset enum, not a free-form value.
        if (FilterOperatorHelper.IsValuelessOperator(condition.Operator)
            || FilterOperatorHelper.IsDatePresetOperator(condition.Operator))
        {
            return false;
        }

        // Range operators (Between, InLast, InNext) need both Value and ValueEnd.
        if (condition.Operator is FilterOperator.Between)
        {
            return condition.Value is null || condition.ValueEnd is null;
        }

        // All other operators require a primary value.
        return condition.Value is null;
    }

    private static bool IsEmpty(object? value) =>
        value is null or "" || (value is string s && string.IsNullOrWhiteSpace(s));

    private static bool AreEqual(object? a, object? b)
    {
        if (a is null && b is null)
        {
            return true;
        }
        if (a is null || b is null)
        {
            return false;
        }

        if (IsNumeric(a) && IsNumeric(b))
        {
            return ToDouble(a).CompareTo(ToDouble(b)) == 0;
        }

        try
        {
            var comparable = ConvertToComparable(a);
            var comparableB = ConvertToComparable(b);
            if (comparable != null && comparableB != null)
            {
                return comparable.CompareTo(comparableB) == 0;
            }
        }
        catch
        {
            // Fall through to Equals
        }

        return a.Equals(b) || a.ToString() == b.ToString();
    }

    private static bool StringOp(object? rawValue, object? filterValue, Func<string, string, bool> op)
    {
        var s = rawValue?.ToString();
        var v = filterValue?.ToString();
        if (s is null || v is null)
        {
            return false;
        }
        return op(s, v);
    }

    private static int? Compare(object? a, object? b)
    {
        if (a is null || b is null)
        {
            return null;
        }

        if (IsNumeric(a) && IsNumeric(b))
        {
            return ToDouble(a).CompareTo(ToDouble(b));
        }

        try
        {
            var comparableA = ConvertToComparable(a);
            var comparableB = ConvertToComparable(b);
            if (comparableA != null && comparableB != null)
            {
                return comparableA.CompareTo(comparableB);
            }
        }
        catch
        {
            // Fall through
        }

        return null;
    }

    private static bool IsNumeric(object value) =>
        value is int or long or float or double or decimal or short or byte or sbyte or ushort or uint or ulong;

    private static double ToDouble(object value) =>
        Convert.ToDouble(value, CultureInfo.InvariantCulture);

    private static IComparable? ConvertToComparable(object? value)
    {
        return value switch
        {
            IComparable c => c,
            _ => null
        };
    }

    private static bool EvaluateInLast(object? rawValue, FilterCondition condition)
    {
        if (rawValue is not DateTime dateValue)
        {
            return false;
        }

        var amount = condition.Value switch
        {
            int i => i,
            double d => (int)d,
            _ => 0
        };

        var period = condition.ValueEnd switch
        {
            InLastPeriod p => p,
            int i when Enum.IsDefined(typeof(InLastPeriod), i) => (InLastPeriod)i,
            string s when Enum.TryParse<InLastPeriod>(s, out var p) => p,
            _ => InLastPeriod.Days
        };

        var now = DateTime.Now;
        var cutoff = period switch
        {
            InLastPeriod.Days => now.AddDays(-amount),
            InLastPeriod.Weeks => now.AddDays(-amount * 7),
            InLastPeriod.Months => now.AddMonths(-amount),
            _ => now
        };

        return dateValue >= cutoff;
    }

    private static bool EvaluateInNext(object? rawValue, FilterCondition condition)
    {
        if (rawValue is not DateTime dateValue)
        {
            return false;
        }

        var amount = condition.Value switch
        {
            int i => i,
            double d => (int)d,
            _ => 0
        };

        var period = condition.ValueEnd switch
        {
            InLastPeriod p => p,
            int i when Enum.IsDefined(typeof(InLastPeriod), i) => (InLastPeriod)i,
            string s when Enum.TryParse<InLastPeriod>(s, out var p) => p,
            _ => InLastPeriod.Days
        };

        var cutoff = period switch
        {
            InLastPeriod.Days => DateTime.Now.AddDays(amount),
            InLastPeriod.Weeks => DateTime.Now.AddDays(amount * 7),
            InLastPeriod.Months => DateTime.Now.AddMonths(amount),
            _ => DateTime.Now
        };

        return dateValue <= cutoff && dateValue >= DateTime.Now;
    }

    private static bool EvaluateDatePreset(object? rawValue, FilterCondition condition, bool negate)
    {
        if (rawValue is not DateTime dateValue)
        {
            return false;
        }

        var preset = condition.Value switch
        {
            DatePreset p => p,
            int i when Enum.IsDefined(typeof(DatePreset), i) => (DatePreset)i,
            string s when Enum.TryParse<DatePreset>(s, out var p) => p,
            _ => DatePreset.Today
        };

        var (start, end) = ResolveDatePresetRange(preset);
        var inRange = dateValue >= start && dateValue < end;
        return negate ? !inRange : inRange;
    }

    private static (DateTime Start, DateTime End) ResolveDatePresetRange(DatePreset preset)
    {
        var today = DateTime.Today;

        return preset switch
        {
            DatePreset.Today => (today, today.AddDays(1)),
            DatePreset.Yesterday => (today.AddDays(-1), today),
            DatePreset.Tomorrow => (today.AddDays(1), today.AddDays(2)),
            DatePreset.ThisWeek => (StartOfWeek(today), StartOfWeek(today).AddDays(7)),
            DatePreset.LastWeek => (StartOfWeek(today).AddDays(-7), StartOfWeek(today)),
            DatePreset.NextWeek => (StartOfWeek(today).AddDays(7), StartOfWeek(today).AddDays(14)),
            DatePreset.ThisMonth => (new DateTime(today.Year, today.Month, 1),
                                     new DateTime(today.Year, today.Month, 1).AddMonths(1)),
            DatePreset.LastMonth => (new DateTime(today.Year, today.Month, 1).AddMonths(-1),
                                     new DateTime(today.Year, today.Month, 1)),
            DatePreset.NextMonth => (new DateTime(today.Year, today.Month, 1).AddMonths(1),
                                     new DateTime(today.Year, today.Month, 1).AddMonths(2)),
            DatePreset.ThisQuarter => (StartOfQuarter(today), StartOfQuarter(today).AddMonths(3)),
            DatePreset.LastQuarter => (StartOfQuarter(today).AddMonths(-3), StartOfQuarter(today)),
            DatePreset.ThisYear => (new DateTime(today.Year, 1, 1), new DateTime(today.Year + 1, 1, 1)),
            DatePreset.LastYear => (new DateTime(today.Year - 1, 1, 1), new DateTime(today.Year, 1, 1)),
            _ => (today, today.AddDays(1))
        };
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }

    private static DateTime StartOfQuarter(DateTime date)
    {
        var quarter = (date.Month - 1) / 3;
        return new DateTime(date.Year, (quarter * 3) + 1, 1);
    }

    private static bool EvaluateIn(object? rawValue, object? filterValue, bool contains)
    {
        if (filterValue is not IEnumerable<string> values)
        {
            return contains; // no filter values = match all (In) or nothing (NotIn)
        }

        var valueList = values as IList<string> ?? values.ToList();
        if (valueList.Count == 0)
        {
            return contains; // empty list = match all (In) or nothing (NotIn), aligns with ToExpression
        }

        var itemValue = rawValue?.ToString() ?? "";
        var isIn = valueList.Any(v => string.Equals(v, itemValue, StringComparison.OrdinalIgnoreCase));
        return contains ? isIn : !isIn;
    }

    private static PropertyInfo? GetProperty(Type type, string propertyName)
    {
        var key = (type, propertyName);
        return PropertyCache.GetOrAdd(key, k =>
            k.Item1.GetProperty(k.Item2, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase));
    }

    #endregion

    #region ToExpression implementation

    private static Expression BuildGroupExpression(
        ParameterExpression parameter,
        FilterDefinition group)
    {
        var expressions = new List<Expression>();

        foreach (var condition in group.Conditions)
        {
            if (string.IsNullOrEmpty(condition.Field))
            {
                continue;
            }

            var condExpr = BuildConditionExpression(parameter, condition);
            if (condExpr != null)
            {
                expressions.Add(condExpr);
            }
        }

        foreach (var nestedGroup in group.Groups)
        {
            if (!nestedGroup.IsEmpty)
            {
                expressions.Add(BuildGroupExpression(parameter, nestedGroup));
            }
        }

        if (expressions.Count == 0)
        {
            return Expression.Constant(true);
        }

        return group.Operator == LogicalOperator.And
            ? expressions.Aggregate(Expression.AndAlso)
            : expressions.Aggregate(Expression.OrElse);
    }

    private static Expression? BuildConditionExpression(
        ParameterExpression parameter,
        FilterCondition condition)
    {
        var property = parameter.Type.GetProperty(condition.Field,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (property == null)
        {
            return Expression.Constant(false);
        }

        var propAccess = Expression.Property(parameter, property);
        var propType = property.PropertyType;
        var isNullable = Nullable.GetUnderlyingType(propType) != null || !propType.IsValueType;

        return condition.Operator switch
        {
            FilterOperator.IsEmpty => BuildIsEmptyExpression(propAccess, propType, isNullable),
            FilterOperator.IsNotEmpty => Expression.Not(BuildIsEmptyExpression(propAccess, propType, isNullable)),
            FilterOperator.IsTrue => BuildBoolExpression(propAccess, propType, true),
            FilterOperator.IsFalse => BuildBoolExpression(propAccess, propType, false),
            FilterOperator.Equals => BuildComparisonExpression(propAccess, propType, condition.Value, ExpressionType.Equal),
            FilterOperator.NotEquals => BuildComparisonExpression(propAccess, propType, condition.Value, ExpressionType.NotEqual),
            FilterOperator.GreaterThan => BuildComparisonExpression(propAccess, propType, condition.Value, ExpressionType.GreaterThan),
            FilterOperator.LessThan => BuildComparisonExpression(propAccess, propType, condition.Value, ExpressionType.LessThan),
            FilterOperator.GreaterOrEqual => BuildComparisonExpression(propAccess, propType, condition.Value, ExpressionType.GreaterThanOrEqual),
            FilterOperator.LessOrEqual => BuildComparisonExpression(propAccess, propType, condition.Value, ExpressionType.LessThanOrEqual),
            FilterOperator.Contains => BuildStringMethodExpression(propAccess, propType, condition.Value, "Contains"),
            FilterOperator.NotContains => Expression.Not(BuildStringMethodExpression(propAccess, propType, condition.Value, "Contains")),
            FilterOperator.StartsWith => BuildStringMethodExpression(propAccess, propType, condition.Value, "StartsWith"),
            FilterOperator.EndsWith => BuildStringMethodExpression(propAccess, propType, condition.Value, "EndsWith"),
            FilterOperator.Between => BuildBetweenExpression(propAccess, propType, condition.Value, condition.ValueEnd),
            FilterOperator.InLast => BuildInLastExpression(propAccess, propType, condition),
            FilterOperator.InNext => BuildInNextExpression(propAccess, propType, condition),
            FilterOperator.DateIs => BuildDatePresetExpression(propAccess, propType, condition, negate: false),
            FilterOperator.DateIsNot => BuildDatePresetExpression(propAccess, propType, condition, negate: true),
            FilterOperator.In => BuildInExpression(propAccess, propType, condition.Value, negate: false),
            FilterOperator.NotIn => BuildInExpression(propAccess, propType, condition.Value, negate: true),
            _ => Expression.Constant(true)
        };
    }

    private static Expression BuildIsEmptyExpression(MemberExpression propAccess, Type propType, bool isNullable)
    {
        if (propType == typeof(string))
        {
            var isNullOrWhiteSpace = typeof(string).GetMethod(nameof(string.IsNullOrWhiteSpace), new[] { typeof(string) })!;
            return Expression.Call(isNullOrWhiteSpace, propAccess);
        }

        if (isNullable)
        {
            return Expression.Equal(propAccess, Expression.Constant(null, propType));
        }

        return Expression.Constant(false);
    }

    private static Expression BuildBoolExpression(MemberExpression propAccess, Type propType, bool expected)
    {
        if (propType == typeof(bool))
        {
            return expected
                ? (Expression)propAccess
                : Expression.Not(propAccess);
        }

        if (propType == typeof(bool?))
        {
            if (expected)
            {
                // IsTrue: value must be exactly true
                return Expression.Equal(propAccess, Expression.Constant((bool?)true, typeof(bool?)));
            }

            // IsFalse: value is false or null (matches ToFunc semantics: rawValue is false or null)
            return Expression.NotEqual(propAccess, Expression.Constant((bool?)true, typeof(bool?)));
        }

        return Expression.Constant(expected);
    }

    private static Expression BuildComparisonExpression(
        MemberExpression propAccess, Type propType, object? value, ExpressionType comparison)
    {
        var underlyingType = Nullable.GetUnderlyingType(propType) ?? propType;
        var convertedValue = ConvertValue(value, underlyingType);

        // Incomplete condition (user hasn't entered a value yet) — ignore it
        if (convertedValue is null && underlyingType.IsValueType)
        {
            return Expression.Constant(true);
        }

        // For nullable types, build the constant with the underlying type then lift to nullable
        Expression constant;
        if (Nullable.GetUnderlyingType(propType) != null)
        {
            var underlyingConstant = Expression.Constant(convertedValue, underlyingType);
            constant = Expression.Convert(underlyingConstant, propType);
        }
        else
        {
            constant = Expression.Constant(convertedValue, propType);
        }

        return Expression.MakeBinary(comparison, propAccess, constant);
    }

    private static Expression BuildStringMethodExpression(
        MemberExpression propAccess, Type propType, object? value, string methodName)
    {
        // Use ToLower(CultureInfo.InvariantCulture) + single-param overloads for EF Core/IQueryable
        // translation compatibility. The StringComparison overloads are not translatable to SQL.
        // Both sides use the same ToLower(CultureInfo.InvariantCulture) to avoid culture mismatch.
        var normalizedValue = (value?.ToString() ?? "").ToLower(CultureInfo.InvariantCulture);
        var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), new[] { typeof(CultureInfo) })!;
        var invariantCulture = Expression.Constant(CultureInfo.InvariantCulture);
        var comparisonMethod = typeof(string).GetMethod(methodName, new[] { typeof(string) })!;

        if (propType == typeof(string))
        {
            var nullCheck = Expression.NotEqual(propAccess, Expression.Constant(null, typeof(string)));
            var loweredProp = Expression.Call(propAccess, toLowerMethod, invariantCulture);
            var methodCall = Expression.Call(loweredProp, comparisonMethod, Expression.Constant(normalizedValue));
            return Expression.AndAlso(nullCheck, methodCall);
        }

        // For non-string properties: call type-specific ToString() to avoid boxing,
        // which improves EF Core/IQueryable translatability.
        var underlyingNullableType = Nullable.GetUnderlyingType(propType);

        if (underlyingNullableType != null)
        {
            // Nullable value types: check HasValue, then operate on Value.
            var toStringMethod = underlyingNullableType.GetMethod(nameof(ToString), Type.EmptyTypes)
                                 ?? typeof(object).GetMethod(nameof(ToString))!;
            var hasValue = Expression.Property(propAccess, "HasValue");
            var valueProperty = Expression.Property(propAccess, "Value");
            var toStringCallExpr = Expression.Call(valueProperty, toStringMethod);
            var lowered = Expression.Call(toStringCallExpr, toLowerMethod, invariantCulture);
            var methodCall = Expression.Call(lowered, comparisonMethod, Expression.Constant(normalizedValue));
            return Expression.AndAlso(hasValue, methodCall);
        }

        if (!propType.IsValueType)
        {
            // Reference types (non-string): null-check, then call ToString().
            var toStringMethod = propType.GetMethod(nameof(ToString), Type.EmptyTypes)
                                 ?? typeof(object).GetMethod(nameof(ToString))!;
            var nullCheck = Expression.NotEqual(propAccess, Expression.Constant(null, propType));
            var toStringCallExpr = Expression.Call(propAccess, toStringMethod);
            var lowered = Expression.Call(toStringCallExpr, toLowerMethod, invariantCulture);
            var methodCall = Expression.Call(lowered, comparisonMethod, Expression.Constant(normalizedValue));
            return Expression.AndAlso(nullCheck, methodCall);
        }

        // Non-nullable value types: call ToString() directly.
        {
            var toStringMethod = propType.GetMethod(nameof(ToString), Type.EmptyTypes)
                                 ?? typeof(object).GetMethod(nameof(ToString))!;
            var toStringCallExpr = Expression.Call(propAccess, toStringMethod);
            var lowered = Expression.Call(toStringCallExpr, toLowerMethod, invariantCulture);
            return Expression.Call(lowered, comparisonMethod, Expression.Constant(normalizedValue));
        }
    }

    private static BinaryExpression BuildBetweenExpression(
        MemberExpression propAccess, Type propType, object? valueStart, object? valueEnd)
    {
        var gte = BuildComparisonExpression(propAccess, propType, valueStart, ExpressionType.GreaterThanOrEqual);
        var lte = BuildComparisonExpression(propAccess, propType, valueEnd, ExpressionType.LessThanOrEqual);
        return Expression.AndAlso(gte, lte);
    }

    private static Expression BuildInLastExpression(
        MemberExpression propAccess, Type propType, FilterCondition condition)
    {
        if (propType != typeof(DateTime) && propType != typeof(DateTime?))
        {
            return Expression.Constant(true);
        }

        var amount = condition.Value switch
        {
            int i => i,
            double d => (int)d,
            _ => 0
        };

        var period = condition.ValueEnd switch
        {
            InLastPeriod p => p,
            int i when Enum.IsDefined(typeof(InLastPeriod), i) => (InLastPeriod)i,
            string s when Enum.TryParse<InLastPeriod>(s, out var p) => p,
            _ => InLastPeriod.Days
        };

        var cutoff = period switch
        {
            InLastPeriod.Days => DateTime.Now.AddDays(-amount),
            InLastPeriod.Weeks => DateTime.Now.AddDays(-amount * 7),
            InLastPeriod.Months => DateTime.Now.AddMonths(-amount),
            _ => DateTime.Now
        };

        Expression target = propType == typeof(DateTime?)
            ? Expression.Property(propAccess, "Value")
            : propAccess;

        var gte = Expression.GreaterThanOrEqual(target, Expression.Constant(cutoff));

        if (propType == typeof(DateTime?))
        {
            var hasValue = Expression.Property(propAccess, "HasValue");
            return Expression.AndAlso(hasValue, gte);
        }

        return gte;
    }

    private static Expression BuildInNextExpression(
        MemberExpression propAccess, Type propType, FilterCondition condition)
    {
        if (propType != typeof(DateTime) && propType != typeof(DateTime?))
        {
            return Expression.Constant(true);
        }

        var amount = condition.Value switch
        {
            int i => i,
            double d => (int)d,
            _ => 0
        };

        var period = condition.ValueEnd switch
        {
            InLastPeriod p => p,
            int i when Enum.IsDefined(typeof(InLastPeriod), i) => (InLastPeriod)i,
            string s when Enum.TryParse<InLastPeriod>(s, out var p) => p,
            _ => InLastPeriod.Days
        };

        var now = DateTime.Now;
        var cutoff = period switch
        {
            InLastPeriod.Days => now.AddDays(amount),
            InLastPeriod.Weeks => now.AddDays(amount * 7),
            InLastPeriod.Months => now.AddMonths(amount),
            _ => now
        };

        Expression target = propType == typeof(DateTime?)
            ? Expression.Property(propAccess, "Value")
            : propAccess;

        // target >= now AND target <= cutoff
        var gte = Expression.GreaterThanOrEqual(target, Expression.Constant(now));
        var lte = Expression.LessThanOrEqual(target, Expression.Constant(cutoff));
        Expression inRange = Expression.AndAlso(gte, lte);

        if (propType == typeof(DateTime?))
        {
            var hasValue = Expression.Property(propAccess, "HasValue");
            return Expression.AndAlso(hasValue, inRange);
        }

        return inRange;
    }

    private static Expression BuildDatePresetExpression(
        MemberExpression propAccess, Type propType, FilterCondition condition, bool negate)
    {
        if (propType != typeof(DateTime) && propType != typeof(DateTime?))
        {
            return Expression.Constant(true);
        }

        var preset = condition.Value switch
        {
            DatePreset p => p,
            int i when Enum.IsDefined(typeof(DatePreset), i) => (DatePreset)i,
            string s when Enum.TryParse<DatePreset>(s, out var p) => p,
            _ => DatePreset.Today
        };

        var (start, end) = ResolveDatePresetRange(preset);

        Expression target = propType == typeof(DateTime?)
            ? Expression.Property(propAccess, "Value")
            : propAccess;

        var gte = Expression.GreaterThanOrEqual(target, Expression.Constant(start));
        var lt = Expression.LessThan(target, Expression.Constant(end));
        Expression inRange = Expression.AndAlso(gte, lt);

        if (negate)
        {
            inRange = Expression.Not(inRange);
        }

        if (propType == typeof(DateTime?))
        {
            var hasValue = Expression.Property(propAccess, "HasValue");
            return negate
                ? Expression.OrElse(Expression.Not(hasValue), Expression.AndAlso(hasValue, inRange))
                : Expression.AndAlso(hasValue, inRange);
        }

        return inRange;
    }

    private static Expression BuildInExpression(
        MemberExpression propAccess, Type propType, object? filterValue, bool negate)
    {
        if (filterValue is not IEnumerable<string> values)
        {
            return Expression.Constant(!negate);
        }

        var valueList = values.ToList();
        if (valueList.Count == 0)
        {
            return Expression.Constant(!negate);
        }

        // Use ToLower(CultureInfo.InvariantCulture) + equality for EF Core/IQueryable translation compatibility.
        var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), new[] { typeof(CultureInfo) })!;
        var invariantCulture = Expression.Constant(CultureInfo.InvariantCulture);

        // For string properties use directly; for others call type-specific ToString()
        // to avoid boxing and improve EF Core/IQueryable translatability.
        Expression stringExpr;
        if (propType == typeof(string))
        {
            stringExpr = Expression.Coalesce(propAccess, Expression.Constant(""));
        }
        else
        {
            var underlyingNullableType = Nullable.GetUnderlyingType(propType);
            if (underlyingNullableType != null)
            {
                // Nullable value types: return "" when !HasValue, else Value.ToString()
                var toStringMethod = underlyingNullableType.GetMethod(nameof(ToString), Type.EmptyTypes)
                                     ?? typeof(object).GetMethod(nameof(ToString))!;
                var hasValue = Expression.Property(propAccess, "HasValue");
                var valueProperty = Expression.Property(propAccess, "Value");
                var toStringCall = Expression.Call(valueProperty, toStringMethod);
                stringExpr = Expression.Condition(hasValue, toStringCall, Expression.Constant(""));
            }
            else if (!propType.IsValueType)
            {
                // Reference types: return "" when null, else ToString()
                var toStringMethod = propType.GetMethod(nameof(ToString), Type.EmptyTypes)
                                     ?? typeof(object).GetMethod(nameof(ToString))!;
                var toStringCall = Expression.Call(propAccess, toStringMethod);
                stringExpr = Expression.Condition(
                    Expression.NotEqual(propAccess, Expression.Constant(null, propType)),
                    toStringCall,
                    Expression.Constant(""));
            }
            else
            {
                // Non-nullable value types: call ToString() directly
                var toStringMethod = propType.GetMethod(nameof(ToString), Type.EmptyTypes)
                                     ?? typeof(object).GetMethod(nameof(ToString))!;
                stringExpr = Expression.Call(propAccess, toStringMethod);
            }
        }

        var loweredProp = Expression.Call(stringExpr, toLowerMethod, invariantCulture);

        // Build individual equality checks combined with OR
        Expression? combined = null;
        foreach (var v in valueList)
        {
            var loweredValue = v.ToLower(CultureInfo.InvariantCulture);
            var check = Expression.Equal(loweredProp, Expression.Constant(loweredValue));
            combined = combined == null ? check : Expression.OrElse(combined, check);
        }

        if (combined == null)
        {
            return Expression.Constant(!negate);
        }

        return negate ? Expression.Not(combined) : combined;
    }

    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value is null)
        {
            return null;
        }

        try
        {
            if (targetType == typeof(string))
            {
                return value.ToString();
            }

            if (targetType == typeof(DateTime) && value is string dateStr)
            {
                return DateTime.Parse(dateStr, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(int))
            {
                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(double))
            {
                return Convert.ToDouble(value, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(decimal))
            {
                return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(float))
            {
                return Convert.ToSingle(value, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(long))
            {
                return Convert.ToInt64(value, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(bool))
            {
                return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
            }

            if (targetType.IsEnum)
            {
                if (value is string enumStr)
                {
                    return Enum.Parse(targetType, enumStr, ignoreCase: true);
                }
                return Enum.ToObject(targetType, Convert.ToInt64(value, CultureInfo.InvariantCulture));
            }

            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    #endregion
}

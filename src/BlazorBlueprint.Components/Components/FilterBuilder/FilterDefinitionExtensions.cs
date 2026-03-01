using System.Collections.Concurrent;
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

        var fieldMap = fields.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);
        return item => EvaluateGroup(item!, filter, fieldMap, typeof(T));
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

        var fieldMap = fields.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);
        var body = BuildGroupExpression(parameter, filter, fieldMap);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    /// <summary>
    /// Serializes the filter definition to a JSON string.
    /// </summary>
    public static string ToJson(this FilterDefinition filter)
    {
        return JsonSerializer.Serialize(filter, JsonOptions);
    }

    /// <summary>
    /// Deserializes a filter definition from a JSON string.
    /// </summary>
    public static FilterDefinition FromJson(string json)
    {
        return JsonSerializer.Deserialize<FilterDefinition>(json, JsonOptions) ?? new FilterDefinition();
    }

    #region ToFunc implementation

    private static bool EvaluateGroup<T>(T item, FilterDefinition group, Dictionary<string, FilterField> fieldMap, Type itemType)
    {
        var results = new List<bool>();

        foreach (var condition in group.Conditions)
        {
            if (string.IsNullOrEmpty(condition.Field))
            {
                continue;
            }
            results.Add(EvaluateCondition(item, condition, fieldMap, itemType));
        }

        foreach (var nestedGroup in group.Groups)
        {
            if (!nestedGroup.IsEmpty)
            {
                results.Add(EvaluateGroup(item, nestedGroup, fieldMap, itemType));
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

    private static bool EvaluateCondition<T>(T item, FilterCondition condition, Dictionary<string, FilterField> fieldMap, Type itemType)
    {
        var prop = GetProperty(itemType, condition.Field);
        if (prop == null)
        {
            return true;
        }

        var rawValue = prop.GetValue(item);
        fieldMap.TryGetValue(condition.Field, out var fieldDef);

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
            FilterOperator.GreaterThan => Compare(rawValue, condition.Value) > 0,
            FilterOperator.LessThan => Compare(rawValue, condition.Value) < 0,
            FilterOperator.GreaterOrEqual => Compare(rawValue, condition.Value) >= 0,
            FilterOperator.LessOrEqual => Compare(rawValue, condition.Value) <= 0,
            FilterOperator.Between => Compare(rawValue, condition.Value) >= 0 && Compare(rawValue, condition.ValueEnd) <= 0,
            FilterOperator.InLast => EvaluateInLast(rawValue, condition),
            FilterOperator.In => EvaluateIn(rawValue, condition.Value, contains: true),
            FilterOperator.NotIn => EvaluateIn(rawValue, condition.Value, contains: false),
            _ => true
        };
    }

    private static bool IsEmpty(object? value)
    {
        return value is null or "" || (value is string s && string.IsNullOrWhiteSpace(s));
    }

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

    private static int Compare(object? a, object? b)
    {
        if (a is null || b is null)
        {
            return 0;
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

        return 0;
    }

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

        var cutoff = period switch
        {
            InLastPeriod.Days => DateTime.Now.AddDays(-amount),
            InLastPeriod.Weeks => DateTime.Now.AddDays(-amount * 7),
            InLastPeriod.Months => DateTime.Now.AddMonths(-amount),
            _ => DateTime.Now
        };

        return dateValue >= cutoff;
    }

    private static bool EvaluateIn(object? rawValue, object? filterValue, bool contains)
    {
        if (filterValue is not IEnumerable<string> values)
        {
            return contains; // no filter values = match all (In) or nothing (NotIn)
        }

        var itemValue = rawValue?.ToString() ?? "";
        var isIn = values.Any(v => string.Equals(v, itemValue, StringComparison.OrdinalIgnoreCase));
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
        FilterDefinition group,
        Dictionary<string, FilterField> fieldMap)
    {
        var expressions = new List<Expression>();

        foreach (var condition in group.Conditions)
        {
            if (string.IsNullOrEmpty(condition.Field))
            {
                continue;
            }

            var condExpr = BuildConditionExpression(parameter, condition, fieldMap);
            if (condExpr != null)
            {
                expressions.Add(condExpr);
            }
        }

        foreach (var nestedGroup in group.Groups)
        {
            if (!nestedGroup.IsEmpty)
            {
                expressions.Add(BuildGroupExpression(parameter, nestedGroup, fieldMap));
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
        FilterCondition condition,
        Dictionary<string, FilterField> fieldMap)
    {
        var property = parameter.Type.GetProperty(condition.Field,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (property == null)
        {
            return Expression.Constant(true);
        }

        var propAccess = Expression.Property(parameter, property);
        var propType = property.PropertyType;
        var underlyingType = Nullable.GetUnderlyingType(propType) ?? propType;
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
            return Expression.Equal(propAccess, Expression.Constant((bool?)expected, typeof(bool?)));
        }

        return Expression.Constant(expected);
    }

    private static Expression BuildComparisonExpression(
        MemberExpression propAccess, Type propType, object? value, ExpressionType comparison)
    {
        var underlyingType = Nullable.GetUnderlyingType(propType) ?? propType;
        var convertedValue = ConvertValue(value, underlyingType);
        var constant = Expression.Constant(convertedValue, propType);

        if (Nullable.GetUnderlyingType(propType) != null)
        {
            constant = Expression.Constant(convertedValue == null ? null : convertedValue, propType);
        }

        return Expression.MakeBinary(comparison, propAccess, constant);
    }

    private static Expression BuildStringMethodExpression(
        MemberExpression propAccess, Type propType, object? value, string methodName)
    {
        var stringValue = value?.ToString() ?? "";
        var method = typeof(string).GetMethod(methodName, new[] { typeof(string), typeof(StringComparison) })!;

        Expression target = propType == typeof(string)
            ? propAccess
            : Expression.Call(propAccess, typeof(object).GetMethod(nameof(object.ToString))!);

        var nullCheck = Expression.NotEqual(target, Expression.Constant(null, typeof(string)));
        var methodCall = Expression.Call(target, method, Expression.Constant(stringValue), Expression.Constant(StringComparison.OrdinalIgnoreCase));

        return Expression.AndAlso(nullCheck, methodCall);
    }

    private static Expression BuildBetweenExpression(
        MemberExpression propAccess, Type propType, object? valueStart, object? valueEnd)
    {
        var gte = BuildComparisonExpression(propAccess, propType, valueStart, ExpressionType.GreaterThanOrEqual);
        var lte = BuildComparisonExpression(propAccess, propType, valueEnd, ExpressionType.LessThanOrEqual);
        return Expression.AndAlso(gte, lte);
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
                return DateTime.Parse(dateStr);
            }

            if (targetType == typeof(int))
            {
                return Convert.ToInt32(value);
            }

            if (targetType == typeof(double))
            {
                return Convert.ToDouble(value);
            }

            if (targetType == typeof(decimal))
            {
                return Convert.ToDecimal(value);
            }

            if (targetType == typeof(float))
            {
                return Convert.ToSingle(value);
            }

            if (targetType == typeof(long))
            {
                return Convert.ToInt64(value);
            }

            if (targetType == typeof(bool))
            {
                return Convert.ToBoolean(value);
            }

            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return value;
        }
    }

    #endregion
}

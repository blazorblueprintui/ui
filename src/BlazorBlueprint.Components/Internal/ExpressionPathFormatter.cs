using System.Linq.Expressions;
using System.Reflection;

namespace BlazorBlueprint.Components;

/// <summary>
/// Formats a value expression (e.g. <c>() =&gt; Input.Username</c>) into the dotted member
/// path used for the HTML name attribute ("Input.Username"), matching the behavior of
/// Blazor's built-in <c>InputBase&lt;T&gt;</c> so that <c>[SupplyParameterFromForm]</c>
/// model binding works with enhanced/SSR form posts.
/// </summary>
internal static class ExpressionPathFormatter
{
    /// <summary>
    /// Formats the member path of a value expression, or returns null when the
    /// expression shape is not a plain member/indexer chain (callers should fall
    /// back to <see cref="Microsoft.AspNetCore.Components.Forms.FieldIdentifier.FieldName"/>).
    /// </summary>
    public static string? FormatLambda(LambdaExpression? expression)
    {
        if (expression is null)
        {
            return null;
        }

        // Collected inner-most first, then reversed. Indexer segments render as "[n]"
        // and attach to the preceding member without a dot separator.
        var segments = new List<string>();
        var node = expression.Body;

        while (node is not null)
        {
            switch (node)
            {
                case MemberExpression member:
                    segments.Add(member.Member.Name);
                    node = member.Expression;
                    break;

                case BinaryExpression { NodeType: ExpressionType.ArrayIndex } arrayIndex:
                    if (!TryEvaluateIndex(arrayIndex.Right, out var arrayIdx))
                    {
                        return null;
                    }
                    segments.Add($"[{arrayIdx}]");
                    node = arrayIndex.Left;
                    break;

                case MethodCallExpression { Method.Name: "get_Item", Arguments.Count: 1 } call when call.Object is not null:
                    if (!TryEvaluateIndex(call.Arguments[0], out var itemIdx))
                    {
                        return null;
                    }
                    segments.Add($"[{itemIdx}]");
                    node = call.Object;
                    break;

                case ConstantExpression:
                    // Root of the chain: the component instance or a closure class.
                    node = null;
                    break;

                default:
                    // Unsupported shape (casts, method calls, etc.) — let callers fall back.
                    return null;
            }
        }

        if (segments.Count == 0)
        {
            return null;
        }

        segments.Reverse();

        var result = new System.Text.StringBuilder();
        foreach (var segment in segments)
        {
            if (result.Length > 0 && segment[0] != '[')
            {
                result.Append('.');
            }
            result.Append(segment);
        }

        return result.ToString();
    }

    private static bool TryEvaluateIndex(Expression indexExpression, out object? value)
    {
        switch (indexExpression)
        {
            case ConstantExpression constant:
                value = constant.Value;
                return true;

            // Captured loop variable: a field access on a closure constant.
            case MemberExpression { Expression: ConstantExpression closure, Member: FieldInfo field }:
                value = field.GetValue(closure.Value);
                return true;

            default:
                value = null;
                return false;
        }
    }
}

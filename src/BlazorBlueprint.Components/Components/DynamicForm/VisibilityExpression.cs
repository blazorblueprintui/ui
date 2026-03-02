using System.Collections.Concurrent;
using System.Globalization;

namespace BlazorBlueprint.Components;

/// <summary>
/// Parses and evaluates visibility expressions for dynamic form fields.
/// Supports operators: <c>==</c>, <c>!=</c>, <c>&amp;&amp;</c>, <c>||</c>, <c>!</c>
/// and literals: strings (<c>'value'</c>), booleans (<c>true</c>/<c>false</c>),
/// <c>null</c>, numbers, and field name identifiers.
/// </summary>
/// <example>
/// <code>
/// VisibilityExpression.Evaluate("Country == 'US'", values);
/// VisibilityExpression.Evaluate("Country == 'US' &amp;&amp; State != null", values);
/// VisibilityExpression.Evaluate("Notifications == true", values);
/// </code>
/// </example>
public static class VisibilityExpression
{
    private static readonly ConcurrentDictionary<string, Func<Dictionary<string, object?>, bool>> cache = new();

    /// <summary>
    /// Parses a visibility expression string into a reusable delegate.
    /// Results are cached for repeated evaluation.
    /// </summary>
    /// <param name="expression">The visibility expression to parse.</param>
    /// <returns>A function that evaluates the expression against a values dictionary.</returns>
    public static Func<Dictionary<string, object?>, bool> Parse(string expression)
    {
        return cache.GetOrAdd(expression, static expr =>
        {
            var tokens = Tokenize(expr);
            var parser = new Parser(tokens);
            var node = parser.ParseExpression();
            parser.ExpectEnd();
            return values => node.Evaluate(values);
        });
    }

    /// <summary>
    /// Evaluates a visibility expression against a dictionary of form values.
    /// </summary>
    /// <param name="expression">The visibility expression string.</param>
    /// <param name="values">The current form field values.</param>
    /// <returns><c>true</c> if the expression evaluates to true.</returns>
    public static bool Evaluate(string expression, Dictionary<string, object?> values)
    {
        var func = Parse(expression);
        return func(values);
    }

    // ── Tokenizer ────────────────────────────────────────────────────

    private enum TokenType
    {
        Identifier,
        StringLiteral,
        NumberLiteral,
        BooleanLiteral,
        NullLiteral,
        Equals,        // ==
        NotEquals,     // !=
        And,           // &&
        Or,            // ||
        Not,           // !
        LeftParen,     // (
        RightParen,    // )
        End
    }

    private readonly record struct Token(TokenType Type, string Value);

    private static List<Token> Tokenize(string input)
    {
        var tokens = new List<Token>();
        var i = 0;

        while (i < input.Length)
        {
            var c = input[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (c == '(' )
            {
                tokens.Add(new Token(TokenType.LeftParen, "("));
                i++;
                continue;
            }

            if (c == ')')
            {
                tokens.Add(new Token(TokenType.RightParen, ")"));
                i++;
                continue;
            }

            if (c == '=' && i + 1 < input.Length && input[i + 1] == '=')
            {
                tokens.Add(new Token(TokenType.Equals, "=="));
                i += 2;
                continue;
            }

            if (c == '!' && i + 1 < input.Length && input[i + 1] == '=')
            {
                tokens.Add(new Token(TokenType.NotEquals, "!="));
                i += 2;
                continue;
            }

            if (c == '!')
            {
                tokens.Add(new Token(TokenType.Not, "!"));
                i++;
                continue;
            }

            if (c == '&' && i + 1 < input.Length && input[i + 1] == '&')
            {
                tokens.Add(new Token(TokenType.And, "&&"));
                i += 2;
                continue;
            }

            if (c == '|' && i + 1 < input.Length && input[i + 1] == '|')
            {
                tokens.Add(new Token(TokenType.Or, "||"));
                i += 2;
                continue;
            }

            if (c == '\'' || c == '"')
            {
                var quote = c;
                i++;
                var start = i;
                while (i < input.Length && input[i] != quote)
                {
                    i++;
                }

                if (i >= input.Length)
                {
                    throw new FormatException($"Unterminated string literal in visibility expression at position {start - 1}.");
                }

                tokens.Add(new Token(TokenType.StringLiteral, input[start..i]));
                i++;
                continue;
            }

            if (char.IsDigit(c) || (c == '-' && i + 1 < input.Length && char.IsDigit(input[i + 1])))
            {
                var start = i;
                if (c == '-')
                {
                    i++;
                }

                while (i < input.Length && (char.IsDigit(input[i]) || input[i] == '.'))
                {
                    i++;
                }

                tokens.Add(new Token(TokenType.NumberLiteral, input[start..i]));
                continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                var start = i;
                while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_' || input[i] == '.'))
                {
                    i++;
                }

                var word = input[start..i];
                if (word is "true" or "false")
                {
                    tokens.Add(new Token(TokenType.BooleanLiteral, word));
                }
                else if (word == "null")
                {
                    tokens.Add(new Token(TokenType.NullLiteral, word));
                }
                else
                {
                    tokens.Add(new Token(TokenType.Identifier, word));
                }

                continue;
            }

            throw new FormatException($"Unexpected character '{c}' in visibility expression at position {i}.");
        }

        tokens.Add(new Token(TokenType.End, ""));
        return tokens;
    }

    // ── AST Nodes ────────────────────────────────────────────────────

    private interface INode
    {
        public bool Evaluate(Dictionary<string, object?> values);
    }

    private sealed class ComparisonNode : INode
    {
        private readonly IValueNode left;
        private readonly IValueNode right;
        private readonly bool negated;

        public ComparisonNode(IValueNode left, IValueNode right, bool negated)
        {
            this.left = left;
            this.right = right;
            this.negated = negated;
        }

        public bool Evaluate(Dictionary<string, object?> values)
        {
            var leftVal = left.GetValue(values);
            var rightVal = right.GetValue(values);
            var equal = ValuesEqual(leftVal, rightVal);
            return negated ? !equal : equal;
        }
    }

    private sealed class AndNode : INode
    {
        private readonly INode left;
        private readonly INode right;

        public AndNode(INode left, INode right)
        {
            this.left = left;
            this.right = right;
        }

        public bool Evaluate(Dictionary<string, object?> values) =>
            left.Evaluate(values) && right.Evaluate(values);
    }

    private sealed class OrNode : INode
    {
        private readonly INode left;
        private readonly INode right;

        public OrNode(INode left, INode right)
        {
            this.left = left;
            this.right = right;
        }

        public bool Evaluate(Dictionary<string, object?> values) =>
            left.Evaluate(values) || right.Evaluate(values);
    }

    private sealed class NotNode : INode
    {
        private readonly INode inner;

        public NotNode(INode inner)
        {
            this.inner = inner;
        }

        public bool Evaluate(Dictionary<string, object?> values) =>
            !inner.Evaluate(values);
    }

    private sealed class TruthyNode : INode
    {
        private readonly IValueNode valueNode;

        public TruthyNode(IValueNode valueNode)
        {
            this.valueNode = valueNode;
        }

        public bool Evaluate(Dictionary<string, object?> values) =>
            IsTruthy(valueNode.GetValue(values));
    }

    // ── Value Nodes ──────────────────────────────────────────────────

    private interface IValueNode
    {
        public object? GetValue(Dictionary<string, object?> values);
    }

    private sealed class IdentifierNode : IValueNode
    {
        private readonly string name;

        public IdentifierNode(string name)
        {
            this.name = name;
        }

        public object? GetValue(Dictionary<string, object?> values) =>
            values.TryGetValue(name, out var value) ? value : null;
    }

    private sealed class LiteralNode : IValueNode
    {
        private readonly object? value;

        public LiteralNode(object? value)
        {
            this.value = value;
        }

        public object? GetValue(Dictionary<string, object?> values) => value;
    }

    // ── Parser ───────────────────────────────────────────────────────

    private sealed class Parser
    {
        private readonly List<Token> tokens;
        private int position;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        private Token Current => tokens[position];

        private Token Advance()
        {
            var token = tokens[position];
            position++;
            return token;
        }

        public void ExpectEnd()
        {
            if (Current.Type != TokenType.End)
            {
                throw new FormatException($"Unexpected token '{Current.Value}' at end of visibility expression.");
            }
        }

        // expression → or_expr
        public INode ParseExpression() => ParseOr();

        // or_expr → and_expr ("||" and_expr)*
        private INode ParseOr()
        {
            var left = ParseAnd();
            while (Current.Type == TokenType.Or)
            {
                Advance();
                var right = ParseAnd();
                left = new OrNode(left, right);
            }

            return left;
        }

        // and_expr → unary_expr ("&&" unary_expr)*
        private INode ParseAnd()
        {
            var left = ParseUnary();
            while (Current.Type == TokenType.And)
            {
                Advance();
                var right = ParseUnary();
                left = new AndNode(left, right);
            }

            return left;
        }

        // unary_expr → "!" unary_expr | comparison
        private INode ParseUnary()
        {
            if (Current.Type == TokenType.Not)
            {
                Advance();
                var inner = ParseUnary();
                return new NotNode(inner);
            }

            return ParseComparison();
        }

        // comparison → primary (("==" | "!=") primary)?
        // If no comparison operator, treat as truthy check
        private INode ParseComparison()
        {
            if (Current.Type == TokenType.LeftParen)
            {
                Advance();
                var inner = ParseExpression();
                if (Current.Type != TokenType.RightParen)
                {
                    throw new FormatException("Missing closing parenthesis in visibility expression.");
                }

                Advance();

                if (Current.Type is TokenType.Equals or TokenType.NotEquals)
                {
                    throw new FormatException("Comparison operators cannot follow a parenthesized group in visibility expressions.");
                }

                return inner;
            }

            var left = ParseValue();

            if (Current.Type == TokenType.Equals)
            {
                Advance();
                var right = ParseValue();
                return new ComparisonNode(left, right, negated: false);
            }

            if (Current.Type == TokenType.NotEquals)
            {
                Advance();
                var right = ParseValue();
                return new ComparisonNode(left, right, negated: true);
            }

            // Bare identifier or literal → truthy check
            return new TruthyNode(left);
        }

        // primary → identifier | string_literal | number_literal | boolean_literal | null
        private IValueNode ParseValue()
        {
            var token = Current;

            switch (token.Type)
            {
                case TokenType.Identifier:
                    Advance();
                    return new IdentifierNode(token.Value);

                case TokenType.StringLiteral:
                    Advance();
                    return new LiteralNode(token.Value);

                case TokenType.NumberLiteral:
                    Advance();
                    if (double.TryParse(token.Value, CultureInfo.InvariantCulture, out var num))
                    {
                        return new LiteralNode(num);
                    }

                    throw new FormatException($"Invalid number '{token.Value}' in visibility expression.");

                case TokenType.BooleanLiteral:
                    Advance();
                    return new LiteralNode(token.Value == "true");

                case TokenType.NullLiteral:
                    Advance();
                    return new LiteralNode(null);

                default:
                    throw new FormatException($"Unexpected token '{token.Value}' in visibility expression, expected a value.");
            }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static bool ValuesEqual(object? left, object? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        // Both are strings → case-insensitive comparison
        if (left is string leftStr && right is string rightStr)
        {
            return string.Equals(leftStr, rightStr, StringComparison.OrdinalIgnoreCase);
        }

        // Boolean comparison with conversion
        if (left is bool || right is bool)
        {
            return IsTruthy(left) == IsTruthy(right);
        }

        // Numeric comparison
        if (IsNumeric(left) && IsNumeric(right))
        {
            return Convert.ToDouble(left, CultureInfo.InvariantCulture) ==
                   Convert.ToDouble(right, CultureInfo.InvariantCulture);
        }

        // String representation comparison (e.g., enum values stored as string in dict, compared to string literal)
        var leftString = left.ToString();
        var rightString = right.ToString();
        return string.Equals(leftString, rightString, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTruthy(object? value)
    {
        return value switch
        {
            null => false,
            bool b => b,
            string s => !string.IsNullOrEmpty(s),
            int i => i != 0,
            double d => d != 0,
            decimal dec => dec != 0,
            _ => true
        };
    }

    private static bool IsNumeric(object? value) =>
        value is int or long or float or double or decimal or short or byte;
}

using System.Text;

namespace BlazorBlueprint.Primitives.DataMatrix;

/// <summary>Converts GS1 human-readable notation into the element string a symbol carries.</summary>
/// <remarks>
/// <para>
/// The parentheses in <c>(01)09501101530003(17)270101(10)ABC123(21)XYZ</c> are display notation
/// only and are never encoded. The element string concatenates each Application Identifier (AI)
/// with its data, and ends every element whose length is not predefined by its AI with a group
/// separator (U+001D) when another element follows. A Data Matrix encoder writes that separator
/// as FNC1.
/// </para>
/// <para>
/// Elements with a predefined length (AIs starting 00-04, 11-20, 31-36 and 41, per the GS1 General
/// Specifications) are checked for that length. The identifiers that end in a GS1 check digit,
/// SSCC (00), GTIN (01, 02) and GLN (410-417), are checked to be digits with the right check
/// digit. Other AI values are not validated. Data that itself contains a parenthesized two to four
/// digit number is ambiguous in this notation; pass the element string instead.
/// </para>
/// </remarks>
public static class Gs1ElementString
{
    private const char GroupSeparator = '\u001D';

    /// <summary>Total element length, AI included, keyed by the first two AI digits.</summary>
    private static readonly Dictionary<string, int> PredefinedLengths = new(StringComparer.Ordinal)
    {
        ["00"] = 20, ["01"] = 16, ["02"] = 16, ["03"] = 16, ["04"] = 18,
        ["11"] = 8, ["12"] = 8, ["13"] = 8, ["14"] = 8, ["15"] = 8, ["16"] = 8, ["17"] = 8,
        ["18"] = 8, ["19"] = 8, ["20"] = 4,
        ["31"] = 10, ["32"] = 10, ["33"] = 10, ["34"] = 10, ["35"] = 10, ["36"] = 10,
        ["41"] = 16,
    };

    /// <summary>Converts human-readable GS1 notation into a GS1 element string.</summary>
    /// <param name="text">Notation such as <c>(01)09501101530003(10)ABC123(21)XYZ</c>.</param>
    /// <returns>The element string, with U+001D after variable-length elements that are followed by another.</returns>
    /// <exception cref="ArgumentNullException">The text is null.</exception>
    /// <exception cref="DataMatrixFormatException">
    /// The text does not start with a two to four digit AI in parentheses, an element has no data,
    /// an element contains U+001D, or an element with a predefined length has the wrong length.
    /// <see cref="DataMatrixFormatException.Error"/> says which.
    /// </exception>
    public static string FromHumanReadable(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (!TryReadAi(text, 0, out var ai, out var position))
        {
            throw DataMatrixFormatException.Create(nameof(text), DataMatrixFormatError.Gs1MissingAi);
        }

        var builder = new StringBuilder(text.Length);
        while (true)
        {
            var dataStart = position;
            string? nextAi = null;
            var nextPosition = text.Length;
            var end = dataStart;
            for (; end < text.Length; end++)
            {
                if (text[end] == '(' && TryReadAi(text, end, out var candidate, out var after))
                {
                    nextAi = candidate;
                    nextPosition = after;
                    break;
                }
            }

            var data = text[dataStart..end];
            if (data.Length == 0)
            {
                throw DataMatrixFormatException.Create(nameof(text), DataMatrixFormatError.Gs1EmptyElement, ai);
            }

            if (data.Contains(GroupSeparator, StringComparison.Ordinal))
            {
                throw DataMatrixFormatException.Create(nameof(text), DataMatrixFormatError.Gs1SeparatorInNotation, ai);
            }

            var isPredefined = PredefinedLengths.TryGetValue(ai[..2], out var totalLength);
            if (isPredefined && ai.Length + data.Length != totalLength)
            {
                throw DataMatrixFormatException.Create(nameof(text), DataMatrixFormatError.Gs1WrongLength,
                    ai, totalLength - ai.Length, data.Length);
            }

            if (HasCheckDigit(ai))
            {
                if (!data.All(char.IsAsciiDigit))
                {
                    throw DataMatrixFormatException.Create(nameof(text), DataMatrixFormatError.Gs1NotDigits, ai);
                }

                var expected = CheckDigit(data.AsSpan(0, data.Length - 1));
                if (data[^1] != expected)
                {
                    throw DataMatrixFormatException.Create(nameof(text), DataMatrixFormatError.Gs1WrongCheckDigit,
                        ai, expected.ToString(), data[^1].ToString());
                }
            }

            builder.Append(ai).Append(data);
            if (nextAi is null)
            {
                return builder.ToString();
            }

            if (!isPredefined)
            {
                builder.Append(GroupSeparator);
            }

            ai = nextAi;
            position = nextPosition;
        }
    }

    /// <summary>SSCC (00), GTIN (01, 02) and GLN (410-417) end in a GS1 mod 10 check digit.</summary>
    private static bool HasCheckDigit(string ai) =>
        ai is "00" or "01" or "02" || (ai.Length == 3 && ai[0] == '4' && ai[1] == '1' && ai[2] is >= '0' and <= '7');

    /// <summary>
    /// The GS1 mod 10 check digit: weights 3 and 1 alternate from the rightmost digit, and the
    /// check digit brings the sum up to a multiple of ten.
    /// </summary>
    internal static char CheckDigit(ReadOnlySpan<char> digits)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            var weight = i % 2 == 0 ? 3 : 1;
            sum += (digits[digits.Length - 1 - i] - '0') * weight;
        }

        return (char)('0' + ((10 - (sum % 10)) % 10));
    }

    /// <summary>Reads "(" + two to four digits + ")" at <paramref name="start"/>.</summary>
    private static bool TryReadAi(string text, int start, out string ai, out int after)
    {
        ai = string.Empty;
        after = start;
        if (start >= text.Length || text[start] != '(')
        {
            return false;
        }

        var close = start + 1;
        while (close < text.Length && char.IsAsciiDigit(text[close]) && close - start <= 4)
        {
            close++;
        }

        var digits = close - start - 1;
        if (digits is < 2 or > 4 || close >= text.Length || text[close] != ')')
        {
            return false;
        }

        ai = text[(start + 1)..close];
        after = close + 1;
        return true;
    }
}

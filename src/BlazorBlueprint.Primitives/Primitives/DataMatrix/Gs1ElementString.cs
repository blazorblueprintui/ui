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
/// Every element is checked against the stable rules of the GS1 General Specifications: its data
/// uses only GS1 character set 82; elements with a predefined length (AIs starting 00-04, 11-20,
/// 31-36 and 41) have that length and hold digits only; SSCC (00), GTIN (01, 02) and GLN
/// (410-417) end in the right check digit; and the dates in AIs 11, 12, 13, 15, 16 and 17
/// (YYMMDD) are real dates, where day 00 means the end of the month.
/// </para>
/// <para>
/// The AI dictionary itself is not checked: an AI GS1 has not assigned, or a variable-length value
/// in the wrong format for its AI, passes. Data that itself contains a parenthesized two to four
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
    /// The text does not start with a two to four digit AI in parentheses, or an element has no
    /// data, contains U+001D or a character outside GS1 character set 82, has the wrong predefined
    /// length, holds something other than digits where only digits belong, ends in the wrong check
    /// digit, or is not a real date. <see cref="DataMatrixFormatException.Error"/> says which.
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

            foreach (var c in data)
            {
                if (!IsCharacterSet82(c))
                {
                    throw DataMatrixFormatException.Create(nameof(text), DataMatrixFormatError.Gs1InvalidCharacter,
                        ai, DataMatrixEncoder.Describe(c));
                }
            }

            // Every predefined-length element is numeric: identifiers, dates, measures and counts.
            if (isPredefined && !data.All(char.IsAsciiDigit))
            {
                throw DataMatrixFormatException.Create(nameof(text), DataMatrixFormatError.Gs1NotDigits, ai);
            }

            if (IsDate(ai) && !IsRealDate(data))
            {
                throw DataMatrixFormatException.Create(nameof(text), DataMatrixFormatError.Gs1InvalidDate, ai, data);
            }

            if (HasCheckDigit(ai))
            {
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

    /// <summary>
    /// GS1 character set 82: the digits, both cases of A-Z, and ! " % &amp; ' ( ) * + , - . / : ; &lt; = &gt; ? _
    /// </summary>
    private static bool IsCharacterSet82(char c) =>
        char.IsAsciiLetterOrDigit(c) || c is '!' or '"' or '%' or '&' or '\'' or '(' or ')' or '*' or '+'
            or ',' or '-' or '.' or '/' or ':' or ';' or '<' or '=' or '>' or '?' or '_';

    /// <summary>Production (11), due (12), packaging (13), best before (15), sell by (16) and expiry (17) dates.</summary>
    private static bool IsDate(string ai) => ai is "11" or "12" or "13" or "15" or "16" or "17";

    /// <summary>
    /// Whether six digits YYMMDD name a real date. Day 00 means the end of the month. GS1 resolves
    /// the century within a sliding window around the current year, which today spans 1977-2076, so
    /// a year divisible by four is a leap year.
    /// </summary>
    private static bool IsRealDate(string yymmdd)
    {
        var year = ((yymmdd[0] - '0') * 10) + (yymmdd[1] - '0');
        var month = ((yymmdd[2] - '0') * 10) + (yymmdd[3] - '0');
        var day = ((yymmdd[4] - '0') * 10) + (yymmdd[5] - '0');
        if (month is < 1 or > 12)
        {
            return false;
        }

        return day == 0 || day <= DateTime.DaysInMonth(year % 4 == 0 ? 2000 : 2001, month);
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

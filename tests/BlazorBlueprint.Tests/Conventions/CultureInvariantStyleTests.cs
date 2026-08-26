using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BlazorBlueprint.Tests.Conventions;

/// <summary>
/// Guards against numeric values reaching an inline <c>style</c> attribute through the current
/// culture.
/// <para>
/// Razor renders a bare <c>@someDouble</c> with <see cref="System.Globalization.CultureInfo.CurrentCulture"/>,
/// so in a locale that writes decimals with a comma the markup becomes <c>left: 33,33%</c>. That is
/// a CSS syntax error, and the browser drops the whole declaration silently — no exception, no
/// console warning, just an element that does not move.
/// </para>
/// <para>
/// This has shipped twice: <c>BbColorPicker</c> (#436) and <c>BbRangeSlider</c> (#443), a fortnight
/// apart. The second was found only by sweeping the tree by hand after fixing the first, which is
/// precisely the kind of check worth automating.
/// </para>
/// </summary>
public class CultureInvariantStyleTests
{
    /// <summary>
    /// An interpolation sitting immediately before a CSS unit — <c>@(x * 100)%</c>, <c>@Foo px</c>.
    /// The unit must not be followed by a word character, or <c>@ItemContainerStyle</c> matches as
    /// <c>@It</c> + the <c>em</c> unit.
    /// </summary>
    private const string Unit = @"(?:%|px|deg|em|rem|vh|vw|fr)(?![\w-])";

    private static readonly Regex Interpolation = new(
        @"@\((?<expr>[^()]*(?:\([^()]*\)[^()]*)*)\)" + Unit + @"|@(?<expr2>[A-Za-z_][\w.]*)" + Unit,
        RegexOptions.Compiled);

    [Fact]
    public void NumericValuesInStyleAttributesAreCultureInvariant()
    {
        var violations = new List<string>();

        foreach (var file in SourceTree.ComponentSources.Where(f => f.Extension == ".razor"))
        {
            var lines = File.ReadAllLines(file.FullName);

            for (var i = 0; i < lines.Length; i++)
            {
                // Only inline styles matter. The same value in a class attribute or as text content
                // is not parsed as CSS, so a comma there is harmless.
                if (!lines[i].Contains("style=", StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (Match match in Interpolation.Matches(lines[i]))
                {
                    var expression = match.Groups["expr"].Success
                        ? match.Groups["expr"].Value
                        : match.Groups["expr2"].Value;

                    if (expression.Contains("InvariantCulture", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    violations.Add(
                        $"{SourceTree.RelativePath(file)}:{i + 1}  ->  @{expression.Trim()}");
                }
            }
        }

        Assert.True(violations.Count == 0, BuildMessage(violations));
    }

    private static string BuildMessage(IReadOnlyCollection<string> violations)
    {
        var message = new StringBuilder()
            .AppendLine(CultureInfo.InvariantCulture,
                $"{violations.Count} numeric value(s) reach an inline style through the current culture.")
            .AppendLine()
            .AppendLine("Razor renders a bare @value with CurrentCulture, so a comma-decimal locale")
            .AppendLine("produces invalid CSS (left: 33,33%) which the browser discards in silence.")
            .AppendLine()
            .AppendLine("Format through InvariantCulture instead:")
            .AppendLine("  style=\"left: @(pct.ToString(\"0.##\", CultureInfo.InvariantCulture))%\"")
            .AppendLine()
            .AppendLine("Prefer \"0.##\" over the default \"G\" format: G switches to scientific")
            .AppendLine("notation below 1e-5, which is invalid CSS by a different route.")
            .AppendLine();

        foreach (var violation in violations)
        {
            message.AppendLine("  " + violation);
        }

        return message.ToString();
    }
}

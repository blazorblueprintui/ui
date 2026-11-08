using System.Reflection;
using System.Xml.Linq;
using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives.DataMatrix;
using Xunit;

namespace BlazorBlueprint.Tests.DataMatrix;

public class DataMatrixComponentTests
{
    [Fact]
    public async Task GetSvgReturnsStandaloneScannerContrastAndOneMergedPath()
    {
        var component = Create();
        SetParameter(component, nameof(BbDataMatrix.Value), "ORDER-4051");
        SetParameter(component, nameof(BbDataMatrix.AriaLabel), "Data Matrix for order 4051");
        await component.EncodeAsync();

        var markup = Assert.IsType<string>(component.GetSvg());
        var document = XDocument.Parse(markup);
        XNamespace svg = "http://www.w3.org/2000/svg";
        var root = Assert.IsType<XElement>(document.Root);

        Assert.Equal(svg + "svg", root.Name);
        Assert.Equal("Data Matrix for order 4051", root.Attribute("aria-label")?.Value);
        Assert.Equal("#ffffff", root.Element(svg + "rect")?.Attribute("fill")?.Value);
        Assert.Equal("#18181b", Assert.Single(root.Elements(svg + "path")).Attribute("fill")?.Value);
        Assert.NotNull(component.Symbol);
    }

    [Fact]
    public async Task Gs1SeparatorDoesNotMakeStandaloneSvgInvalidXml()
    {
        var component = Create();
        SetParameter(component, nameof(BbDataMatrix.Value), "010950110153000310ABC\u001d17270101");
        SetParameter(component, nameof(BbDataMatrix.IsGs1), true);
        SetParameter(component, nameof(BbDataMatrix.ShowValue), true);

        await component.EncodeAsync();

        var markup = Assert.IsType<string>(component.GetSvg());
        var document = XDocument.Parse(markup);
        Assert.Contains("␝", document.Root?.Attribute("aria-label")?.Value);
        Assert.Equal(-1, markup.IndexOf('\u001d'));
    }

    [Fact]
    public async Task Gs1NotationEncodesAsElementStringAndKeepsItsReadableLabel()
    {
        var component = Create();
        SetParameter(component, nameof(BbDataMatrix.Value), "(01)09501101530003(10)ABC123(21)XYZ");
        SetParameter(component, nameof(BbDataMatrix.IsGs1), true);

        await component.EncodeAsync();

        var symbol = Assert.IsType<DataMatrixSymbol>(component.Symbol);
        Assert.True(symbol.IsGs1);
        var root = XDocument.Parse(Assert.IsType<string>(component.GetSvg())).Root;
        Assert.Equal("Data Matrix code for (01)09501101530003(10)ABC123(21)XYZ",
            root?.Attribute("aria-label")?.Value);
    }

    [Fact]
    public async Task MalformedGs1NotationSetsErrorStateWithoutThrowing()
    {
        var component = Create();
        SetParameter(component, nameof(BbDataMatrix.Value), "(01)123");
        SetParameter(component, nameof(BbDataMatrix.IsGs1), true);

        await component.EncodeAsync();

        Assert.Null(component.Symbol);
        Assert.Equal("GS1 element (01) needs 14 data characters, and this value has 3.", EncodeError(component));
    }

    [Fact]
    public async Task RefusalIsWordedThroughTheLocalizer()
    {
        var localizer = new DefaultBbLocalizer();
        localizer.Set("DataMatrix.Gs1WrongLength", "Element GS1 ({0}) wymaga {1} znaków danych, a ma {2}.");
        var component = Create(localizer);
        SetParameter(component, nameof(BbDataMatrix.Value), "(01)123");
        SetParameter(component, nameof(BbDataMatrix.IsGs1), true);

        await component.EncodeAsync();

        Assert.Equal("Element GS1 (01) wymaga 14 znaków danych, a ma 3.", EncodeError(component));
    }

    [Fact]
    public async Task ExplicitModeRefusalNamesTheModeAndCharacter()
    {
        var component = Create();
        SetParameter(component, nameof(BbDataMatrix.Value), "ABCd");
        SetParameter(component, nameof(BbDataMatrix.EncodingMode), DataMatrixEncodingMode.X12);

        await component.EncodeAsync();

        Assert.Equal("X12 encodation cannot carry 'd'. Use Auto to choose a mode for each part of the value.",
            EncodeError(component));
    }

    [Fact]
    public async Task RectangleExtendedShapeRendersADmreSymbol()
    {
        var component = Create();
        SetParameter(component, nameof(BbDataMatrix.Value), new string('1', 36));
        SetParameter(component, nameof(BbDataMatrix.Shape), DataMatrixSymbolShape.RectangleExtended);

        await component.EncodeAsync();

        var symbol = Assert.IsType<DataMatrixSymbol>(component.Symbol);
        Assert.Equal((48, 8, DataMatrixSymbolShape.RectangleExtended), (symbol.Width, symbol.Height, symbol.Shape));
        Assert.Equal("0 0 50 10", XDocument.Parse(component.GetSvg()!).Root?.Attribute("viewBox")?.Value);
    }

    [Fact]
    public async Task SymbolSizeFixesTheGridAndSaysWhenAValueDoesNotFit()
    {
        var component = Create();
        SetParameter(component, nameof(BbDataMatrix.Value), "LOT-42");
        SetParameter(component, nameof(BbDataMatrix.SymbolSize), DataMatrixSymbolSize.Dmre8x80);

        await component.EncodeAsync();

        Assert.Equal("0 0 82 10", XDocument.Parse(component.GetSvg()!).Root?.Attribute("viewBox")?.Value);

        SetParameter(component, nameof(BbDataMatrix.Value), "ABCDEFG");
        SetParameter(component, nameof(BbDataMatrix.SymbolSize), DataMatrixSymbolSize.Square10);
        await component.EncodeAsync();

        Assert.Null(component.Symbol);
        Assert.Equal("This value does not fit the Square10 symbol, which holds 3 data codewords.", EncodeError(component));
    }

    [Fact]
    public void EveryRefusalReasonHasALocalizerKeyMatchingTheEncoderSentence()
    {
        // A missing key would show the raw "DataMatrix.{reason}" in the alert, because the
        // localizer returns the key itself when it has no text for it.
        object[] arguments = ["A", "B", "C"];
        foreach (var reason in Enum.GetValues<DataMatrixFormatError>().Where(r => r != DataMatrixFormatError.Unspecified))
        {
            var key = $"DataMatrix.{reason}";
            Assert.True(DefaultBbLocalizer.EnglishDefaults.TryGetValue(key, out var english), $"Missing {key}");
            Assert.Equal(DataMatrixFormatException.Create("value", reason, arguments).Message,
                string.Format(System.Globalization.CultureInfo.InvariantCulture, english, arguments));
        }
    }

    [Fact]
    public async Task OversizeInputSetsErrorStateWithoutThrowing()
    {
        var component = Create();
        SetParameter(component, nameof(BbDataMatrix.Value), new string('A', 1600));
        SetParameter(component, nameof(BbDataMatrix.EncodingMode), DataMatrixEncodingMode.Base256);

        await component.EncodeAsync();

        Assert.Null(component.GetSvg());
        Assert.Null(component.Symbol);
        Assert.Equal("This value is too long for a Data Matrix symbol of shape Auto, which holds at most 1558 data codewords.",
            EncodeError(component));
    }

    private static TestDataMatrix Create(DefaultBbLocalizer? localizer = null)
    {
        var component = new TestDataMatrix();
        typeof(BbDataMatrix).GetProperty("Localizer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(component, localizer ?? new DefaultBbLocalizer());
        return component;
    }

    private static string? EncodeError(TestDataMatrix component) =>
        (string?)typeof(BbDataMatrix).GetField("encodeError", BindingFlags.Instance | BindingFlags.NonPublic)?
            .GetValue(component);

    private static void SetParameter(TestDataMatrix component, string name, object value) =>
        typeof(BbDataMatrix).GetProperty(name)!.SetValue(component, value);

    private sealed class TestDataMatrix : BbDataMatrix
    {
        public Task EncodeAsync() => OnParametersSetAsync();
    }
}

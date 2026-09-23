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
    public async Task OversizeInputSetsErrorStateWithoutThrowing()
    {
        var component = Create();
        SetParameter(component, nameof(BbDataMatrix.Value), new string('A', 1600));
        SetParameter(component, nameof(BbDataMatrix.EncodingMode), DataMatrixEncodingMode.Base256);

        await component.EncodeAsync();

        Assert.Null(component.GetSvg());
        Assert.Null(component.Symbol);
        Assert.Equal("This value cannot be encoded in the selected Data Matrix symbol.",
            typeof(BbDataMatrix).GetField("encodeError", BindingFlags.Instance | BindingFlags.NonPublic)?
                .GetValue(component));
    }

    private static TestDataMatrix Create()
    {
        var component = new TestDataMatrix();
        typeof(BbDataMatrix).GetProperty("Localizer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(component, new DefaultBbLocalizer());
        return component;
    }

    private static void SetParameter(TestDataMatrix component, string name, object value) =>
        typeof(BbDataMatrix).GetProperty(name)!.SetValue(component, value);

    private sealed class TestDataMatrix : BbDataMatrix
    {
        public Task EncodeAsync() => OnParametersSetAsync();
    }
}

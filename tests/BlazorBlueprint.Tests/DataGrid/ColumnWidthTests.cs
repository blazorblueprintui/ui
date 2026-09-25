using BlazorBlueprint.Primitives.DataGrid;

namespace BlazorBlueprint.Tests.DataGrid;

/// <summary>
/// The rule that decides which width values are widths. Tested here rather than only through a
/// grid, because it is the one thing the drag report, the column state and the renderer agree on.
/// </summary>
public class ColumnWidthTests
{
    [Theory]
    [InlineData("150px")]
    [InlineData("150.5px")]
    [InlineData("150PX")]
    [InlineData("0.5px")]
    [InlineData("20%")]
    [InlineData("2.5rem")]
    [InlineData("1e3px")]
    [InlineData("12ch")]
    [InlineData("100cqw")]
    [InlineData("10vi")]
    [InlineData("10vb")]
    [InlineData("10svb")]
    [InlineData("auto")]
    [InlineData("fit-content")]
    [InlineData("min-content")]
    [InlineData("-webkit-fill-available")]
    [InlineData("var(--column-width)")]
    [InlineData("clamp(4rem, 20%, 10rem)")]
    [InlineData("calc(100% - 2rem)")]
    public void AWidthIsRenderable(string width) => Assert.True(ColumnWidth.IsRenderable(width));

    [Fact]
    public void AUnitThisTypeHasNotHeardOfIsStillALength()
    {
        // No list of known units: guessing wrong there throws away a width someone asked for, which
        // is how 10vi went missing. Only a number wearing no unit at all is refused.
        Assert.True(ColumnWidth.IsRenderable("10vi"));
        Assert.True(ColumnWidth.IsRenderable("1foo"));
        Assert.False(ColumnWidth.IsRenderable("1"));
    }

    [Theory]
    [InlineData("0px")]
    [InlineData("0rem")]
    [InlineData("0vw")]
    [InlineData("0vi")]
    [InlineData("0ch")]
    [InlineData("0")]
    [InlineData("0%")]
    [InlineData("-10px")]
    [InlineData("-1em")]
    [InlineData("-2.5rem")]
    [InlineData("-1%")]
    [InlineData("0.4px")]
    [InlineData("NaNpx")]
    [InlineData("Infinitypx")]
    [InlineData("NaN%")]
    [InlineData("Infinity%")]
    [InlineData("Infinity")]
    [InlineData("1")]
    [InlineData("-1")]
    [InlineData("1e3")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SomethingThatIsNotAWidthIsNotRenderable(string? width) =>
        Assert.False(ColumnWidth.IsRenderable(width));

    [Fact]
    public void NormalizingKeepsAWidthAndDropsEverythingElse()
    {
        Assert.Equal("20%", ColumnWidth.Normalize("20%"));
        Assert.Null(ColumnWidth.Normalize("0px"));
    }

    [Fact]
    public void AWidthWrittenStraightOntoATrackedEntryIsStillNormalized()
    {
        var columns = new DataGridColumnState();
        columns.SetWidth("name", "140px");

        columns.Entries.Single().Width = "0px";

        Assert.Null(columns.GetWidth("name"));
    }

    [Fact]
    public void AnEntryKeepsAWidthItWasBuiltWith()
    {
        var entry = new ColumnStateEntry { ColumnId = "name", Width = "20%" };

        Assert.Equal("20%", entry.Width);
    }

    [Theory]
    [InlineData(220.4, true)]
    [InlineData(0.5, true)]
    [InlineData(0.4, false)]
    [InlineData(0, false)]
    [InlineData(-10, false)]
    [InlineData(double.NaN, false)]
    [InlineData(double.PositiveInfinity, false)]
    public void OnlyAMeasurableWidthIsUsable(double pixels, bool expected) =>
        Assert.Equal(expected, ColumnWidth.IsUsablePixels(pixels));

    [Theory]
    [InlineData(220.4, "220px")]
    [InlineData(220.6, "221px")]
    [InlineData(0.5, "1px")]
    public void AnAcceptedMeasurementFormatsAsWholePixels(double pixels, string expected) =>
        Assert.Equal(expected, ColumnWidth.FormatPixels(pixels));
}

using BlazorBlueprint.Primitives.DataGrid;

namespace BlazorBlueprint.Tests.DataGrid;

/// <summary>
/// What the primitive grid tells a consumer that asked it to set a width: the width the column
/// state accepted, not the one it was handed.
/// </summary>
public class ColumnWidthCallbackTests
{
    [Fact]
    public void AWidthThatIsNotAWidthIsNeitherStoredNorReported()
    {
        var reported = new List<(string ColumnId, string? Width)>();
        var context = CreateContext(reported);

        context.SetColumnWidth("name", "0px");

        Assert.Null(context.State.Columns.GetWidth("name"));
        Assert.Equal(("name", (string?)null), reported.Single());
    }

    [Fact]
    public void AWidthIsStoredAndReportedAsGiven()
    {
        var reported = new List<(string ColumnId, string? Width)>();
        var context = CreateContext(reported);

        context.SetColumnWidth("name", "140px");

        Assert.Equal("140px", context.State.Columns.GetWidth("name"));
        Assert.Equal(("name", "140px"), reported.Single());
    }

    private static DataGridContext<Row> CreateContext(List<(string ColumnId, string? Width)> reported) =>
        new(new DataGridState<Row>())
        {
            OnColumnResize = (columnId, width) => reported.Add((columnId, width))
        };

    private sealed class Row
    {
        public string Name { get; init; } = string.Empty;
    }
}

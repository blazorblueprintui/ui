using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives.DataGrid;
using Xunit;

namespace BlazorBlueprint.Tests.DataGrid;

/// <summary>
/// The grid styles every header through <see cref="IDataGridColumn{TData}.HeaderClass"/>. The expand
/// column used to answer <c>null</c> there, so its header could not be styled to match the rest (#597).
/// </summary>
public class ExpandColumnTests
{
    private sealed class Row;

    [Fact]
    public void HeaderClassReachesTheGrid()
    {
#pragma warning disable BL0005 // Set directly to check the column interface without mounting a grid.
        IDataGridColumn<Row> column = new BbDataGridExpandColumn<Row> { HeaderClass = "bg-primary" };
#pragma warning restore BL0005

        Assert.Equal("bg-primary", column.HeaderClass);
    }

    [Fact]
    public void HeaderClassIsNullByDefault()
    {
        IDataGridColumn<Row> column = new BbDataGridExpandColumn<Row>();

        Assert.Null(column.HeaderClass);
    }
}

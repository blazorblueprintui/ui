using System.Linq.Expressions;
using AngleSharp.Dom;
using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using BlazorBlueprint.Tests.Components;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using StyledDataGrid = BlazorBlueprint.Components.BbDataGrid<BlazorBlueprint.Tests.DataGrid.ControlColumnClassTests.Row>;

namespace BlazorBlueprint.Tests.DataGrid;

/// <summary>
/// The expand and edit columns take HeaderClass and CellClass like every other column type,
/// so their header and body cells can be styled to match the rest of the grid.
/// </summary>
public class ControlColumnClassTests
{
    [Fact]
    public async Task ExpandColumnClassesLandOnItsCells()
    {
        await using var context = CreateContext();

        var cut = RenderGrid(context, builder =>
        {
            builder.OpenComponent<BbDataGridExpandColumn<Row>>(0);
            builder.AddAttribute(1, nameof(BbDataGridExpandColumn<Row>.HeaderClass), "expand-header");
            builder.AddAttribute(2, nameof(BbDataGridExpandColumn<Row>.CellClass), "expand-cell");
            builder.CloseComponent();
            AddNameColumn(builder, 3);
        });

        Assert.Contains("expand-header", HeaderCell(cut, "__expand").ClassList);
        Assert.Contains("expand-cell", BodyCell(cut, 0).ClassList);
    }

    [Fact]
    public async Task EditColumnClassesLandOnItsCells()
    {
        await using var context = CreateContext();

        var cut = RenderGrid(context, builder =>
        {
            AddNameColumn(builder, 0);
            builder.OpenComponent<BbDataGridEditColumn<Row>>(1);
            builder.AddAttribute(2, nameof(BbDataGridEditColumn<Row>.HeaderClass), "edit-header");
            builder.AddAttribute(3, nameof(BbDataGridEditColumn<Row>.CellClass), "edit-cell");
            builder.CloseComponent();
        });

        Assert.Contains("edit-header", HeaderCell(cut, "__edit").ClassList);
        Assert.Contains("edit-cell", BodyCell(cut, 1).ClassList);
    }

    private static IRenderedComponent<StyledDataGrid> RenderGrid(ComponentBunitContext context, RenderFragment columns) =>
        context.Render<StyledDataGrid>(parameters => parameters
            .Add(p => p.Items, new[] { new Row { Id = 1, Name = "One" } })
            .Add(p => p.ItemKey, (Func<Row, object>)(r => r.Id))
            .Add(p => p.EditMode, DataGridEditMode.Row)
            .Add(p => p.Columns, columns));

    private static void AddNameColumn(RenderTreeBuilder builder, int sequence)
    {
        builder.OpenComponent<BbDataGridPropertyColumn<Row, string>>(sequence);
        builder.AddAttribute(sequence + 1, nameof(BbDataGridPropertyColumn<Row, string>.Property),
            (Expression<Func<Row, string>>)(r => r.Name));
        builder.CloseComponent();
    }

    private static IElement HeaderCell(IRenderedComponent<StyledDataGrid> cut, string columnId) =>
        cut.Find($"th[data-column-id='{columnId}']");

    private static IElement BodyCell(IRenderedComponent<StyledDataGrid> cut, int index) =>
        cut.FindAll("tbody tr")[0].QuerySelectorAll("td")[index];

    private static ComponentBunitContext CreateContext()
    {
        var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    public sealed class Row
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
    }
}

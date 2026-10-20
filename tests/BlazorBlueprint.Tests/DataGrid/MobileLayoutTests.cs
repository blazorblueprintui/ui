using System.Linq.Expressions;
using AngleSharp.Dom;
using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using BlazorBlueprint.Tests.Components;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using StyledDataGrid = BlazorBlueprint.Components.BbDataGrid<BlazorBlueprint.Tests.DataGrid.MobileLayoutTests.Row>;

namespace BlazorBlueprint.Tests.DataGrid;

/// <summary>
/// MobileLayout="Stacked" makes the grid write each body cell's label and the breakpoint into
/// the markup, for the CSS to stack rows on a small screen (#616). A grid left at the default
/// writes none of it.
/// </summary>
public class MobileLayoutTests
{
    [Fact]
    public async Task DefaultLayoutWritesNoStackedMarkup()
    {
        await using var context = CreateContext();

        var cut = RenderGrid(context, DataGridMobileLayout.None, null, AllColumns);

        Assert.Null(Root(cut).GetAttribute("data-mobile-stacked"));
        Assert.Null(cut.Find("thead").GetAttribute("role"));
        Assert.Null(cut.Find("tbody").GetAttribute("role"));
        Assert.All(BodyCells(cut), cell =>
        {
            Assert.False(cell.HasAttribute("data-label"));
            Assert.False(cell.HasAttribute("data-hide-on-mobile"));
        });
    }

    [Theory]
    [InlineData(null, "sm")]
    [InlineData(DataGridBreakpoint.Sm, "sm")]
    [InlineData(DataGridBreakpoint.Md, "md")]
    [InlineData(DataGridBreakpoint.Lg, "lg")]
    public async Task StackedLayoutWritesItsBreakpointOnTheRoot(DataGridBreakpoint? breakpoint, string expected)
    {
        await using var context = CreateContext();

        var cut = RenderGrid(context, DataGridMobileLayout.Stacked, breakpoint, AllColumns);

        Assert.Equal(expected, Root(cut).GetAttribute("data-mobile-stacked"));
    }

    [Fact]
    public async Task StackedLayoutLabelsEachCell()
    {
        await using var context = CreateContext();

        var cut = RenderGrid(context, DataGridMobileLayout.Stacked, null, AllColumns);
        var cells = BodyCells(cut);

        // Select column: no title, so no label.
        Assert.False(cells[0].HasAttribute("data-label"));
        // Title, inferred from the property.
        Assert.Equal("Name", cells[1].GetAttribute("data-label"));
        // MobileLabel replaces the title.
        Assert.Equal("Dept.", cells[2].GetAttribute("data-label"));
        // An empty MobileLabel shows no label.
        Assert.False(cells[3].HasAttribute("data-label"));
    }

    [Fact]
    public async Task StackedLayoutMarksHiddenColumns()
    {
        await using var context = CreateContext();

        var cut = RenderGrid(context, DataGridMobileLayout.Stacked, null, builder =>
        {
            builder.OpenComponent<BbDataGridSelectColumn<Row>>(0);
            builder.AddAttribute(1, nameof(BbDataGridSelectColumn<Row>.HideOnMobile), true);
            builder.CloseComponent();
            AddPropertyColumn(builder, 10, r => r.Name);
            AddPropertyColumn(builder, 20, r => r.Department, hideOnMobile: true);
        });
        var cells = BodyCells(cut);

        Assert.Equal("true", cells[0].GetAttribute("data-hide-on-mobile"));
        Assert.False(cells[1].HasAttribute("data-hide-on-mobile"));
        Assert.Equal("true", cells[2].GetAttribute("data-hide-on-mobile"));
    }

    [Fact]
    public async Task StackedLayoutGivesRowGroupsTheirRole()
    {
        await using var context = CreateContext();

        var cut = RenderGrid(context, DataGridMobileLayout.Stacked, null, AllColumns);

        Assert.Equal("grid", cut.Find("table").GetAttribute("role"));
        Assert.Equal("rowgroup", cut.Find("thead").GetAttribute("role"));
        Assert.Equal("rowgroup", cut.Find("tbody").GetAttribute("role"));
        Assert.Equal("row", cut.Find("tbody tr").GetAttribute("role"));
        Assert.All(BodyCells(cut), cell => Assert.Equal("gridcell", cell.GetAttribute("role")));
    }

    [Fact]
    public async Task PinnedColumnIsLabelledLikeAnyOther()
    {
        await using var context = CreateContext();

        var cut = RenderGrid(context, DataGridMobileLayout.Stacked, null, builder =>
        {
            builder.OpenComponent<BbDataGridPropertyColumn<Row, string>>(0);
            builder.AddAttribute(1, nameof(BbDataGridPropertyColumn<Row, string>.Property),
                (Expression<Func<Row, string>>)(r => r.Name));
            builder.AddAttribute(2, nameof(BbDataGridPropertyColumn<Row, string>.Pinned),
                BlazorBlueprint.Primitives.DataGrid.ColumnPinning.Left);
            builder.CloseComponent();
        });
        var cell = BodyCells(cut)[0];

        Assert.Equal("true", cell.GetAttribute("data-pinned"));
        Assert.Equal("Name", cell.GetAttribute("data-label"));
    }

    private static readonly RenderFragment AllColumns = builder =>
    {
        builder.OpenComponent<BbDataGridSelectColumn<Row>>(0);
        builder.CloseComponent();
        AddPropertyColumn(builder, 10, r => r.Name);
        AddPropertyColumn(builder, 20, r => r.Department, mobileLabel: "Dept.");
        builder.OpenComponent<BbDataGridTemplateColumn<Row>>(30);
        builder.AddAttribute(31, nameof(BbDataGridTemplateColumn<Row>.Title), "Actions");
        builder.AddAttribute(32, nameof(BbDataGridTemplateColumn<Row>.MobileLabel), "");
        builder.AddAttribute(33, nameof(BbDataGridTemplateColumn<Row>.ChildContent),
            (RenderFragment<Row>)(_ => b => b.AddContent(0, "Edit")));
        builder.CloseComponent();
    };

    private static IRenderedComponent<StyledDataGrid> RenderGrid(ComponentBunitContext context,
        DataGridMobileLayout layout, DataGridBreakpoint? breakpoint, RenderFragment columns) =>
        context.Render<StyledDataGrid>(parameters =>
        {
            parameters
                .Add(p => p.Items, new[] { new Row { Id = 1, Name = "One", Department = "Sales" } })
                .Add(p => p.SelectionMode, DataTableSelectionMode.Multiple)
                .Add(p => p.MobileLayout, layout)
                .Add(p => p.Columns, columns);
            if (breakpoint != null)
            {
                parameters.Add(p => p.MobileBreakpoint, breakpoint.Value);
            }
        });

    private static void AddPropertyColumn(RenderTreeBuilder builder, int sequence,
        Expression<Func<Row, string>> property, string? mobileLabel = null, bool hideOnMobile = false)
    {
        builder.OpenComponent<BbDataGridPropertyColumn<Row, string>>(sequence);
        builder.AddAttribute(sequence + 1, nameof(BbDataGridPropertyColumn<Row, string>.Property), property);
        if (mobileLabel != null)
        {
            builder.AddAttribute(sequence + 2, nameof(BbDataGridPropertyColumn<Row, string>.MobileLabel), mobileLabel);
        }

        if (hideOnMobile)
        {
            builder.AddAttribute(sequence + 3, nameof(BbDataGridPropertyColumn<Row, string>.HideOnMobile), true);
        }

        builder.CloseComponent();
    }

    private static IElement Root(IRenderedComponent<StyledDataGrid> cut) => cut.Find("div");

    private static List<IElement> BodyCells(IRenderedComponent<StyledDataGrid> cut) =>
        cut.FindAll("tbody tr")[0].QuerySelectorAll("td").ToList();

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

        public string Department { get; set; } = "";
    }
}

using System.Linq.Expressions;
using AngleSharp.Dom;
using BlazorBlueprint.Components;
using BlazorBlueprint.Tests.Components;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Xunit;
using StyledDataGrid = BlazorBlueprint.Components.BbDataGrid<BlazorBlueprint.Tests.Performance.DataGridHeaderPopupTests.Row>;

namespace BlazorBlueprint.Tests.Performance;

/// <summary>
/// Opening a column's filter popover or column menu re-rendered every row of the grid, because the
/// open state lived on the grid. It now lives in a scope around the popup, so opening or closing one
/// costs the same at any row count (#655), while the popup still sees every open and close (#437).
/// </summary>
public class DataGridHeaderPopupTests
{
    private static readonly Row[] Rows = Enumerable.Range(1, 30).Select(i => new Row { Id = i, City = $"City {i}" }).ToArray();

    [Fact]
    public async Task OpeningAndClosingAFilterDoesNotRerenderRows()
    {
        await using var context = CreateContext();
        var counter = new CellCounter();
        var cut = RenderGrid(context, counter, filterable: true, groupable: false);

        counter.Count = 0;
        await FilterButton(cut).ClickAsync(new());

        Assert.Equal("true", FilterButton(cut).GetAttribute("aria-expanded"));
        Assert.Equal(0, counter.Count);

        // The trigger only opens; dismissal (outside click, Escape) reports through OpenChanged.
        var popover = cut.FindComponent<BbPopover>();
        await cut.InvokeAsync(() => popover.Instance.OpenChanged.InvokeAsync(false));

        Assert.Equal("false", FilterButton(cut).GetAttribute("aria-expanded"));
        Assert.Equal(0, counter.Count);

        // Reopening still works: the popup keeps receiving its controlled value (#437).
        await FilterButton(cut).ClickAsync(new());
        Assert.Equal("true", FilterButton(cut).GetAttribute("aria-expanded"));
    }

    [Fact]
    public async Task OpeningAColumnMenuDoesNotRerenderRows()
    {
        await using var context = CreateContext();
        var counter = new CellCounter();
        var cut = RenderGrid(context, counter, filterable: false, groupable: true);

        counter.Count = 0;
        await MenuButton(cut).ClickAsync(new());

        Assert.Equal("true", MenuButton(cut).GetAttribute("aria-expanded"));
        Assert.Equal(0, counter.Count);
    }

    [Fact]
    public async Task AGridRenderClosesAnOpenPopupOnlyWhenAskedTo()
    {
        await using var context = CreateContext();
        var counter = new CellCounter();
        var cut = RenderGrid(context, counter, filterable: true, groupable: false);
        await FilterButton(cut).ClickAsync(new());

        // A re-render for some other reason leaves the popup open.
        await cut.InvokeAsync(() => cut.Instance.RefreshDataAsync());
        Assert.Equal("true", FilterButton(cut).GetAttribute("aria-expanded"));
    }

    private static IElement FilterButton(IRenderedComponent<StyledDataGrid> cut) =>
        cut.Find("button[aria-label='Filter City']");

    private static IElement MenuButton(IRenderedComponent<StyledDataGrid> cut) =>
        cut.FindAll("th[data-column-id='city'] button").Single(b => b.GetAttribute("aria-haspopup") != null);

    private static IRenderedComponent<StyledDataGrid> RenderGrid(ComponentBunitContext context, CellCounter counter, bool filterable, bool groupable)
    {
        RenderFragment columns = builder =>
        {
            builder.OpenComponent<BbDataGridPropertyColumn<Row, string>>(0);
            builder.AddAttribute(1, nameof(BbDataGridPropertyColumn<Row, string>.Property), (Expression<Func<Row, string>>)(r => r.City));
            builder.AddAttribute(2, nameof(BbDataGridPropertyColumn<Row, string>.Filterable), filterable);
            builder.AddAttribute(3, nameof(BbDataGridPropertyColumn<Row, string>.Groupable), groupable);
            builder.CloseComponent();
            builder.OpenComponent<BbDataGridTemplateColumn<Row>>(10);
            builder.AddAttribute(11, nameof(BbDataGridTemplateColumn<Row>.Title), "Counted");
            builder.AddAttribute(12, nameof(BbDataGridTemplateColumn<Row>.ChildContent), (RenderFragment<Row>)(row => b =>
            {
                counter.Count++;
                b.AddContent(0, row.Id);
            }));
            builder.CloseComponent();
        };

        return context.Render<StyledDataGrid>(p => p
            .Add(x => x.Items, Rows)
            .Add(x => x.InitialPageSize, 20)
            .Add(x => x.Columns, columns));
    }

    private static ComponentBunitContext CreateContext()
    {
        var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    public sealed class Row
    {
        public int Id { get; init; }

        public string City { get; init; } = "";
    }

    private sealed class CellCounter
    {
        public int Count { get; set; }
    }
}

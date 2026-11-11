using System.Linq.Expressions;
using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.DataGrid;
using BlazorBlueprint.Tests.Components;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Xunit;
using StyledDataGrid = BlazorBlueprint.Components.BbDataGrid<BlazorBlueprint.Tests.Performance.DataGridRenderCountTests.Row>;

namespace BlazorBlueprint.Tests.Performance;

/// <summary>
/// A page change rendered every body cell twice, and a sort click three or four times (#655).
/// Each extra pass rebuilds every row and cell, so the cost of a page change grew with the
/// number of passes as well as the number of rows. These checks count how often a cell is
/// rendered per interaction rather than timing the machine.
/// </summary>
public class DataGridRenderCountTests
{
    private const int PageSize = 20;

    private static readonly List<Row> Rows =
        Enumerable.Range(1, 200).Select(i => new Row { Id = i, Name = $"Row {i:D3}", Age = 200 - i }).ToList();

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PageChangeRendersEachRowOnce(bool bindState)
    {
        await using var context = CreateContext();
        var counter = new CellCounter();
        var cut = RenderGrid(context, counter, bindState);

        counter.Count = 0;
        await cut.FindAll("button").First(b => b.TextContent.Trim() == "Next").ClickAsync(new());

        Assert.Equal("Row 021", FirstName(cut));
        Assert.Equal(PageSize, counter.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SortClickRendersEachRowOnce(bool bindState)
    {
        await using var context = CreateContext();
        var counter = new CellCounter();
        var cut = RenderGrid(context, counter, bindState);

        counter.Count = 0;
        await cut.Find("th[data-column-id='age']").ClickAsync(new());

        Assert.Equal("Row 200", FirstName(cut));
        Assert.Equal("ascending", cut.Find("th[data-column-id='age']").GetAttribute("aria-sort"));
        Assert.Equal(PageSize, counter.Count);
    }

    [Fact]
    public async Task RowSelectionStillRepaintsTheRow()
    {
        // The primitive now leaves its render to the styled grid after a state change; a click
        // that only changes selection must still be drawn.
        await using var context = CreateContext();
        var cut = RenderGrid(context, new CellCounter(), bindState: false, selectable: true);

        await cut.FindAll("tbody tr")[2].ClickAsync(new());

        Assert.Equal("true", cut.FindAll("tbody tr")[2].GetAttribute("aria-selected"));
        Assert.Equal("false", cut.FindAll("tbody tr")[1].GetAttribute("aria-selected"));
    }

    private static string FirstName(IRenderedComponent<HostPage> cut) =>
        cut.FindAll("tbody tr")[0].QuerySelectorAll("td")[1].TextContent.Trim();

    private static IRenderedComponent<HostPage> RenderGrid(ComponentBunitContext context, CellCounter counter, bool bindState, bool selectable = false)
    {
        RenderFragment columns = builder =>
        {
            AddColumn(builder, 0, (Expression<Func<Row, int>>)(r => r.Id), sortable: false);
            AddColumn(builder, 10, (Expression<Func<Row, string>>)(r => r.Name), sortable: false);
            AddColumn(builder, 20, (Expression<Func<Row, int>>)(r => r.Age), sortable: true);
            builder.OpenComponent<BbDataGridTemplateColumn<Row>>(30);
            builder.AddAttribute(31, nameof(BbDataGridTemplateColumn<Row>.Title), "Counted");
            builder.AddAttribute(32, nameof(BbDataGridTemplateColumn<Row>.ChildContent), (RenderFragment<Row>)(row => b =>
            {
                counter.Count++;
                b.AddContent(0, row.Id);
            }));
            builder.CloseComponent();
        };

        return context.Render<HostPage>(p => p
            .Add(x => x.Columns, columns)
            .Add(x => x.BindState, bindState)
            .Add(x => x.Selectable, selectable));
    }

    private static void AddColumn<TProp>(RenderTreeBuilder builder, int sequence, Expression<Func<Row, TProp>> property, bool sortable)
    {
        builder.OpenComponent<BbDataGridPropertyColumn<Row, TProp>>(sequence);
        builder.AddAttribute(sequence + 1, nameof(BbDataGridPropertyColumn<Row, TProp>.Property), property);
        builder.AddAttribute(sequence + 2, nameof(BbDataGridPropertyColumn<Row, TProp>.Sortable), sortable);
        builder.CloseComponent();
    }

    private static ValueTask<DataGridResult<Row>> Provide(DataGridRequest request)
    {
        IEnumerable<Row> query = Rows;
        foreach (var sort in request.SortDefinitions)
        {
            query = sort.Direction == SortDirection.Descending
                ? query.OrderByDescending(r => r.Age)
                : query.OrderBy(r => r.Age);
        }

        var items = query.Skip(request.StartIndex).Take(request.Count ?? Rows.Count).ToList();
        return ValueTask.FromResult(new DataGridResult<Row> { Items = items, TotalItemCount = Rows.Count });
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

        public string Name { get; init; } = "";

        public int Age { get; init; }
    }

    private sealed class CellCounter
    {
        public int Count { get; set; }

    }

    /// <summary>
    /// A routed page holding the grid: with <see cref="BindState"/> it binds State the way
    /// <c>@bind-State</c> does, so every StateChanged re-renders the page and the grid with it.
    /// </summary>
    public sealed class HostPage : ComponentBase
    {
        private DataGridState<Row> state = new() { Pagination = { PageSize = PageSize } };

        [Parameter]
        public RenderFragment? Columns { get; set; }

        [Parameter]
        public bool BindState { get; set; }

        [Parameter]
        public bool Selectable { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<StyledDataGrid>(0);
            builder.AddAttribute(1, nameof(StyledDataGrid.ItemsProvider), (DataGridItemsProvider<Row>)Provide);
            builder.AddAttribute(2, nameof(StyledDataGrid.InitialPageSize), PageSize);
            builder.AddAttribute(3, nameof(StyledDataGrid.Columns), Columns);
            if (Selectable)
            {
                builder.AddAttribute(6, nameof(StyledDataGrid.SelectionMode), DataTableSelectionMode.Single);
            }
            if (BindState)
            {
                builder.AddAttribute(4, nameof(StyledDataGrid.State), state);
                builder.AddAttribute(5, nameof(StyledDataGrid.StateChanged),
                    EventCallback.Factory.Create<DataGridState<Row>>(this, s => state = s));
            }
            builder.CloseComponent();
        }
    }
}

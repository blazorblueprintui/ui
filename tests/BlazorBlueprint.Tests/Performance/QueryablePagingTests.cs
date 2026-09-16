using System.Collections;
using System.Linq.Expressions;
using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives.DataGrid;
using Xunit;

namespace BlazorBlueprint.Tests.Performance;

#pragma warning disable BL0005 // Parameters are supplied directly to isolate the data pipeline.
public class QueryablePagingTests
{
    private sealed record Row(int Value);

    [Fact]
    public async Task FetchesOnlyTheRequestedPageAndDefersTheFullExport()
    {
        var source = Enumerable.Range(0, 10_000).Select(i => new Row(i)).AsQueryable()
            .Where(row => row.Value >= 500).OrderByDescending(row => row.Value);
        var provider = new RecordingProvider(source.Provider);
        var grid = new BlazorBlueprint.Components.BbDataGrid<Row> { Items = new RecordingQuery<Row>(provider, source), InitialPageSize = 25 };
        ComponentProbe.Call(grid, "OnInitialized");
        var state = ComponentProbe.Field<DataGridState<Row>>(grid, "_gridState");
        state.Pagination.CurrentPage = 2;

        await (Task)ComponentProbe.Call(grid, "ProcessDataAsync")!;

        Assert.Equal(9500, state.Pagination.TotalItems);
        Assert.Equal(1, provider.CountQueries);
        Assert.Equal(25, provider.MaterializedRows);
        Assert.Contains("Skip(25).Take(25)", provider.LastReadExpression, StringComparison.Ordinal);
        var page = ComponentProbe.Field<IEnumerable<Row>>(grid, "_processedData").ToArray();
        Assert.Equal(9974, page[0].Value);
        Assert.Equal(9950, page[^1].Value);

        grid.ExportScope = DataGridExportScope.CurrentPage;
        var exportPage = (IReadOnlyList<Row>)ComponentProbe.Call(grid, "GetExportRows")!;
        Assert.Equal(25, exportPage.Count);
        Assert.Equal(25, provider.MaterializedRows);

        grid.ExportScope = DataGridExportScope.FilteredRows;
        var allRows = (IReadOnlyList<Row>)ComponentProbe.Call(grid, "GetExportRows")!;
        Assert.Equal(9500, allRows.Count);
        Assert.Equal(9999, allRows[0].Value);
        Assert.Equal(9525, provider.MaterializedRows);
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(false, "search")]
    public async Task KeepsTheFullDatasetWhenVirtualizationOrClientSearchRequiresIt(bool virtualize, string? search)
    {
        var source = Enumerable.Range(0, 100).Select(i => new Row(i)).AsQueryable();
        var provider = new RecordingProvider(source.Provider);
        var grid = new BlazorBlueprint.Components.BbDataGrid<Row>
        {
            Items = new RecordingQuery<Row>(provider, source), Virtualize = virtualize, SearchText = search ?? ""
        };
        await (Task)ComponentProbe.Call(grid, "ProcessDataAsync")!;
        Assert.Equal(100, provider.MaterializedRows);
        Assert.Equal(0, provider.CountQueries);
    }

    [Fact]
    public async Task SwitchingSourcesDoesNotExportTheOldQuery()
    {
        var grid = new BlazorBlueprint.Components.BbDataGrid<Row> { Items = new[] { new Row(1) }.AsQueryable() };
        await (Task)ComponentProbe.Call(grid, "ProcessDataAsync")!;
        grid.Items = new[] { new Row(2) };
        await (Task)ComponentProbe.Call(grid, "ProcessDataAsync")!;
        var rows = (IReadOnlyList<Row>)ComponentProbe.Call(grid, "GetExportRows")!;
        Assert.Equal(2, Assert.Single(rows).Value);
    }

    private sealed class RecordingProvider(IQueryProvider inner) : IQueryProvider
    {
        public int CountQueries { get; private set; }
        public int MaterializedRows { get; private set; }
        public string LastReadExpression { get; private set; } = "";

        public IQueryable CreateQuery(Expression expression) => throw new NotSupportedException();
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
            new RecordingQuery<TElement>(this, inner.CreateQuery<TElement>(expression));
        public object? Execute(Expression expression) => inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression)
        {
            if (expression is MethodCallExpression { Method.Name: nameof(Queryable.Count) })
            {
                CountQueries++;
            }
            return inner.Execute<TResult>(expression);
        }
        public IEnumerable<TElement> Read<TElement>(IQueryable<TElement> query)
        {
            LastReadExpression = query.Expression.ToString();
            foreach (var item in query)
            {
                MaterializedRows++;
                yield return item;
            }
        }
    }

    private sealed class RecordingQuery<T>(RecordingProvider provider, IQueryable<T> query) : IOrderedQueryable<T>
    {
        public Type ElementType => typeof(T);
        public Expression Expression => query.Expression;
        public IQueryProvider Provider => provider;
        public IEnumerator<T> GetEnumerator() => provider.Read(query).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
#pragma warning restore BL0005

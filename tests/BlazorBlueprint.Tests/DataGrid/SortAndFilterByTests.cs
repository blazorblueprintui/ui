using System.Linq.Expressions;
using BlazorBlueprint.Primitives.DataGrid;
using BlazorBlueprint.Primitives.Filtering;
using BlazorBlueprint.Primitives;
using Xunit;

namespace BlazorBlueprint.Tests.DataGrid;

/// <summary>
/// Covers a column that sorts and filters on a value it does not store — the case behind
/// <c>SortAndFilterBy</c>, where a cell template resolves a key to a label and the grid must
/// order and filter by the label the user reads rather than by the key.
/// </summary>
public class SortAndFilterByTests
{
    private sealed class Ticket
    {
        public int StatusId { get; init; }

        public string Title { get; init; } = "";

        public DateTime Raised { get; init; }
    }

    private static readonly int[] ActiveCancelledPendingOrder = { 2, 3, 1 };

    private static readonly int[] PendingCancelledActiveOrder = { 1, 3, 2 };

    private static readonly int[] CancelledOnly = { 3 };

    private static readonly int[] ActiveOnly = { 2 };

    private static readonly int[] CancelledThenActive = { 3, 2 };

    private static readonly string[] TitlesAscending = { "Laptop slow", "Printer jammed", "VPN drops" };

    private static readonly Dictionary<int, string> StatusNames = new()
    {
        [1] = "Pending",
        [2] = "Active",
        [3] = "Cancelled"
    };

    /// <summary>
    /// A column whose sort and filter value is a projection rather than its own property.
    /// Mirrors what <c>BbDataGridPropertyColumn.SortAndFilterBy</c> produces.
    /// </summary>
    private sealed class ProjectedColumn : IDataGridColumn<Ticket>
    {
        private readonly Expression<Func<Ticket, object?>>? selector;
        private readonly Func<Ticket, object?>? compiled;

        public ProjectedColumn(string columnId, Expression<Func<Ticket, object?>>? selector = null)
        {
            ColumnId = columnId;
            this.selector = selector;
            compiled = selector?.Compile();
        }

        public string ColumnId { get; }

        public string? Title => ColumnId;

        public bool Sortable => true;

        public bool Filterable => true;

        public bool Visible => true;

        public string? Width => null;

        public bool Hideable => true;

        public bool Resizable => true;

        public bool Reorderable => true;

        public ColumnPinning Pinned => ColumnPinning.None;

        public Microsoft.AspNetCore.Components.RenderFragment<DataGridCellContext<Ticket>>? CellTemplate => null;

        public Microsoft.AspNetCore.Components.RenderFragment<DataGridHeaderContext<Ticket>>? HeaderTemplate => null;

        public string? CellClass => null;

        public string? HeaderClass => null;

        public bool NoWrap => false;

        public AggregateFunction Aggregate => AggregateFunction.None;

        public object? GetValue(Ticket item) => item.StatusId;

        public object? GetRawValue(Ticket item) => item.StatusId;

        public object? GetSortAndFilterValue(Ticket item) =>
            compiled != null ? compiled(item) : GetRawValue(item);

        public LambdaExpression? GetSortAndFilterExpression() => selector;

        public int Compare(Ticket x, Ticket y)
        {
            if (selector == null)
            {
                return x.StatusId.CompareTo(y.StatusId);
            }

            return Comparer<string>.Default.Compare(
                GetSortAndFilterValue(x)?.ToString(),
                GetSortAndFilterValue(y)?.ToString());
        }

        public LambdaExpression? GetSortExpression() => selector;

        public LambdaExpression? GetFilterExpression() => selector;
    }

    private static List<Ticket> CreateTickets() => new()
    {
        new Ticket { StatusId = 1, Title = "Printer jammed", Raised = new DateTime(2026, 1, 5, 9, 30, 0) },
        new Ticket { StatusId = 3, Title = "Laptop slow", Raised = new DateTime(2026, 1, 6, 14, 0, 0) },
        new Ticket { StatusId = 2, Title = "VPN drops", Raised = new DateTime(2026, 1, 7, 8, 15, 0) }
    };

    // --- Defaults ------------------------------------------------------------------

    [Fact]
    public void GetSortAndFilterValueFallsBackToRawValueWhenNoSelectorIsSet()
    {
        var column = new ProjectedColumn("status");
        var ticket = new Ticket { StatusId = 3 };

        Assert.Equal(3, column.GetSortAndFilterValue(ticket));
        Assert.Null(column.GetSortAndFilterExpression());
    }

    // --- Sorting -------------------------------------------------------------------

    [Fact]
    public void InMemorySortOrdersByTheProjectedLabelNotTheKey()
    {
        var column = new ProjectedColumn("status", x => StatusNames[x.StatusId]);
        var sortDefinitions = new List<SortDefinition>
        {
            new() { ColumnId = "status", Direction = SortDirection.Ascending }
        };

        var sorted = CreateTickets()
            .ApplyMultiSort(sortDefinitions, new IDataGridColumn<Ticket>[] { column })
            .ToList();

        // Label order is Active, Cancelled, Pending — key order would be 1, 2, 3.
        Assert.Equal(ActiveCancelledPendingOrder, sorted.Select(t => t.StatusId));
    }

    [Fact]
    public void InMemorySortHonoursDescendingDirection()
    {
        var column = new ProjectedColumn("status", x => StatusNames[x.StatusId]);
        var sortDefinitions = new List<SortDefinition>
        {
            new() { ColumnId = "status", Direction = SortDirection.Descending }
        };

        var sorted = CreateTickets()
            .ApplyMultiSort(sortDefinitions, new IDataGridColumn<Ticket>[] { column })
            .ToList();

        Assert.Equal(PendingCancelledActiveOrder, sorted.Select(t => t.StatusId));
    }

    [Fact]
    public void QueryableSortOrdersByTheProjectedLabelNotTheKey()
    {
        // The selector returns a string, so no boxing conversion is involved. This proves the
        // queryable path reads the selector at all.
        var column = new ProjectedColumn("title", x => x.Title);
        var sortDefinitions = new List<SortDefinition>
        {
            new() { ColumnId = "title", Direction = SortDirection.Ascending }
        };

        var sorted = CreateTickets().AsQueryable()
            .ApplyMultiSort(sortDefinitions, new IDataGridColumn<Ticket>[] { column })
            .ToList();

        Assert.Equal(TitlesAscending, sorted.Select(t => t.Title));
    }

    [Fact]
    public void QueryableSortStripsTheBoxingConversionSoValueTypesStillOrder()
    {
        // Declaring the selector as Func<Ticket, object?> boxes the int through a Convert node.
        // Ordering by object would compare boxed references; the grid must strip that node.
        var column = new ProjectedColumn("raised", x => x.Raised);
        var sortDefinitions = new List<SortDefinition>
        {
            new() { ColumnId = "raised", Direction = SortDirection.Descending }
        };

        var sorted = CreateTickets().AsQueryable()
            .ApplyMultiSort(sortDefinitions, new IDataGridColumn<Ticket>[] { column })
            .ToList();

        Assert.Equal(ActiveCancelledPendingOrder, sorted.Select(t => t.StatusId));
    }

    // --- Filtering, in memory ------------------------------------------------------

    [Fact]
    public void MatchesValueComparesAgainstTheProjectedValue()
    {
        var column = new ProjectedColumn("status", x => StatusNames[x.StatusId]);
        var condition = new FilterCondition
        {
            Field = "status",
            Operator = FilterOperator.Equals,
            Value = "Cancelled"
        };

        var matched = CreateTickets()
            .Where(t => condition.MatchesValue(column.GetSortAndFilterValue(t)))
            .ToList();

        Assert.Equal(CancelledOnly, matched.Select(t => t.StatusId));
    }

    [Fact]
    public void MatchesValueSupportsContains()
    {
        var column = new ProjectedColumn("status", x => StatusNames[x.StatusId]);
        var condition = new FilterCondition
        {
            Field = "status",
            Operator = FilterOperator.Contains,
            Value = "cel"
        };

        var matched = CreateTickets()
            .Where(t => condition.MatchesValue(column.GetSortAndFilterValue(t)))
            .ToList();

        Assert.Equal(CancelledOnly, matched.Select(t => t.StatusId));
    }

    [Fact]
    public void MatchesValueTreatsAnIncompleteConditionAsMatchingEverything()
    {
        var condition = new FilterCondition
        {
            Field = "status",
            Operator = FilterOperator.Equals,
            Value = null
        };

        Assert.True(condition.MatchesValue("Pending"));
    }

    [Fact]
    public void MatchesValueUsesWholeDaySemanticsForDateFields()
    {
        var condition = new FilterCondition
        {
            Field = "raised",
            Operator = FilterOperator.Equals,
            Value = new DateTime(2026, 1, 6, 0, 0, 0)
        };

        // 14:00 on the 6th must match a filter for "the 6th", despite the time component.
        Assert.True(condition.MatchesValue(new DateTime(2026, 1, 6, 14, 0, 0), FilterFieldType.Date));
        Assert.False(condition.MatchesValue(new DateTime(2026, 1, 7, 0, 0, 0), FilterFieldType.Date));
    }

    // --- Filtering, queryable ------------------------------------------------------

    [Fact]
    public void ToExpressionForSelectorFiltersOnTheProjectedValue()
    {
        Expression<Func<Ticket, object?>> selector = x => x.Title;
        var condition = new FilterCondition
        {
            Field = "ignored",
            Operator = FilterOperator.StartsWith,
            Value = "VPN"
        };

        var matched = CreateTickets().AsQueryable()
            .Where(condition.ToExpressionForSelector<Ticket>(selector))
            .ToList();

        Assert.Equal(ActiveOnly, matched.Select(t => t.StatusId));
    }

    [Fact]
    public void ToExpressionForSelectorStripsTheBoxingConversionForValueTypes()
    {
        Expression<Func<Ticket, object?>> selector = x => x.StatusId;
        var condition = new FilterCondition
        {
            Field = "ignored",
            Operator = FilterOperator.GreaterThan,
            Value = 1
        };

        var matched = CreateTickets().AsQueryable()
            .Where(condition.ToExpressionForSelector<Ticket>(selector))
            .ToList();

        Assert.Equal(CancelledThenActive, matched.Select(t => t.StatusId));
    }

    [Fact]
    public void ToExpressionForSelectorIgnoresTheConditionFieldName()
    {
        // The field name identifies the column, not a property. A selector-backed column has no
        // property of that name, and the filter must still apply.
        Expression<Func<Ticket, object?>> selector = x => x.Title;
        var condition = new FilterCondition
        {
            Field = "no-such-property",
            Operator = FilterOperator.Contains,
            Value = "slow"
        };

        var matched = CreateTickets().AsQueryable()
            .Where(condition.ToExpressionForSelector<Ticket>(selector))
            .ToList();

        Assert.Single(matched);
    }

    [Fact]
    public void ToExpressionForSelectorRejectsALambdaWithTheWrongParameterCount()
    {
        Expression<Func<Ticket, int, object?>> twoParameters = (x, i) => x.StatusId + i;
        var condition = new FilterCondition { Field = "status", Operator = FilterOperator.Equals, Value = 1 };

        Assert.Throws<ArgumentException>(() => condition.ToExpressionForSelector<Ticket>(twoParameters));
    }

    [Fact]
    public void ToExpressionForSelectorAndMatchesValueAgreeOnTheSameData()
    {
        Expression<Func<Ticket, object?>> selector = x => StatusNames[x.StatusId];
        var compiled = selector.Compile();
        var condition = new FilterCondition
        {
            Field = "status",
            Operator = FilterOperator.NotEquals,
            Value = "Active"
        };

        var tickets = CreateTickets();
        var viaValue = tickets.Where(t => condition.MatchesValue(compiled(t))).Select(t => t.StatusId).ToList();
        var viaExpression = tickets.AsQueryable()
            .Where(condition.ToExpressionForSelector<Ticket>(selector))
            .Select(t => t.StatusId)
            .ToList();

        Assert.Equal(viaValue, viaExpression);
    }
}

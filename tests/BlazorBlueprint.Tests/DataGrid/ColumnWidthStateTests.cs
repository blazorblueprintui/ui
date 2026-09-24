using System.Text.RegularExpressions;
using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives.DataGrid;
using BlazorBlueprint.Tests.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using StyledDataGrid = BlazorBlueprint.Components.BbDataGrid<BlazorBlueprint.Tests.DataGrid.ColumnWidthStateTests.Row>;

namespace BlazorBlueprint.Tests.DataGrid;

/// <summary>
/// What a column is drawn at, and what a resize drag is allowed to put in the column state.
/// </summary>
public class ColumnWidthStateTests
{
    private const string Name = "name";
    private const string Actions = "actions";
    private const string Status = "status";
    private const string DeclaredWidth = "80px";

    // The control column: a width it declares, and a header a drag cannot measure.
    private static readonly ColumnSpec[] DefaultColumns =
    [
        new(Name),
        new(Actions, DeclaredWidth, ColumnPinning.Right)
    ];

    [Fact]
    public async Task AZeroStateWidthFallsBackToTheDeclaredWidth()
    {
        await WithGridAsync(DefaultColumns, async (grid, state, renderer) =>
        {
            state.Columns.SetWidth(Actions, "0px");
            await ReRenderAsync(grid, state, renderer);

            Assert.Equal($"width: {DeclaredWidth}", ColStyle(renderer.Markup(), Actions));
        });
    }

    [Fact]
    public async Task ANegativeStateWidthFallsBackToTheDeclaredWidth()
    {
        await WithGridAsync(DefaultColumns, async (grid, state, renderer) =>
        {
            state.Columns.SetWidth(Actions, "-10px");
            await ReRenderAsync(grid, state, renderer);

            Assert.Equal($"width: {DeclaredWidth}", ColStyle(renderer.Markup(), Actions));
        });
    }

    [Fact]
    public async Task AStateWidthFromAResizeStillWins()
    {
        // The declared width is what a zero falls back to, not what replaces every resize: a width
        // the user dragged to has to survive a reload, which is the point of the state.
        await WithGridAsync(DefaultColumns, async (grid, state, renderer) =>
        {
            state.Columns.SetWidth(Actions, "140px");
            await ReRenderAsync(grid, state, renderer);

            Assert.Equal("width: 140px", ColStyle(renderer.Markup(), Actions));
        });
    }

    [Fact]
    public async Task ANonPixelStateWidthIsLeftAlone()
    {
        await WithGridAsync(DefaultColumns, async (grid, state, renderer) =>
        {
            state.Columns.SetWidth(Actions, "20%");
            await ReRenderAsync(grid, state, renderer);

            Assert.Equal("width: 20%", ColStyle(renderer.Markup(), Actions));
        });
    }

    [Fact]
    public async Task ANonPixelDeclaredWidthAnswersForAZeroStateWidth()
    {
        ColumnSpec[] columns = [new(Name), new(Actions, "clamp(4rem, 20%, 10rem)", ColumnPinning.Right)];

        await WithGridAsync(columns, async (grid, state, renderer) =>
        {
            state.Columns.SetWidth(Actions, "0px");
            await ReRenderAsync(grid, state, renderer, columns);

            Assert.Equal("width: clamp(4rem, 20%, 10rem)", ColStyle(renderer.Markup(), Actions));
        });
    }

    [Fact]
    public async Task AZeroStateWidthLeavesAColumnThatDeclaresNoWidthUnstyled()
    {
        await WithGridAsync(DefaultColumns, async (grid, state, renderer) =>
        {
            state.Columns.SetWidth(Name, "0px");
            await ReRenderAsync(grid, state, renderer);

            var markup = renderer.Markup();

            Assert.Null(ColStyle(markup, Name));
            Assert.DoesNotContain("width: 0px", markup, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task AResizeThatReportsNothingUsableLeavesTheColumnAlone()
    {
        await WithGridAsync(DefaultColumns, async (grid, state, _) =>
        {
            await grid.OnResizeCompleted(Name, new Dictionary<string, double>
            {
                [Name] = 220,
                [Actions] = 0
            });

            Assert.Equal("220px", state.Columns.GetWidth(Name));
            Assert.Null(state.Columns.GetWidth(Actions));
        });
    }

    [Fact]
    public async Task ASubPixelResizeIsRefusedRatherThanStoredAsZero()
    {
        // Half a pixel is the smallest width that survives being rounded, and rounding agrees with
        // the check: anything this refuses cannot be stored as "0px" by the rounding either.
        await WithGridAsync(DefaultColumns, async (grid, state, _) =>
        {
            await grid.OnResizeCompleted(Name, new Dictionary<string, double> { [Name] = 0.4 });

            Assert.Null(state.Columns.GetWidth(Name));
        });
    }

    [Fact]
    public async Task ANonNumericResizeIsRefused()
    {
        await WithGridAsync(DefaultColumns, async (grid, state, _) =>
        {
            await grid.OnResizeCompleted(Name, new Dictionary<string, double> { [Name] = double.NaN });

            Assert.Null(state.Columns.GetWidth(Name));
        });
    }

    [Fact]
    public async Task AFractionalResizeIsStoredRounded()
    {
        await WithGridAsync(DefaultColumns, async (grid, state, _) =>
        {
            await grid.OnResizeCompleted(Name, new Dictionary<string, double> { [Name] = 220.6 });

            Assert.Equal("221px", state.Columns.GetWidth(Name));
        });
    }

    [Fact]
    public async Task ARestoredSnapshotKeepsAWidthAndDropsOneThatIsNotAWidth()
    {
        // What a page that saved a poisoned state gets back on its next load.
        await WithGridAsync(DefaultColumns, async (grid, state, renderer) =>
        {
            state.Columns.SetWidth(Actions, "140px");
            state.Columns.SetWidth(Name, "180px");

            var snapshot = state.Save();
            snapshot.ColumnStates.Single(c => c.ColumnId == Name).Width = "0px";
            state.Restore(snapshot);
            await ReRenderAsync(grid, state, renderer);

            Assert.Equal("140px", state.Columns.GetWidth(Actions));
            Assert.Null(state.Columns.GetWidth(Name));
            Assert.Equal("width: 140px", ColStyle(renderer.Markup(), Actions));
        });
    }

    [Fact]
    public async Task APinnedColumnCountsAPoisonedNeighbourAtItsDeclaredWidth()
    {
        // The sticky offset is the other reader of a column's width: a zero there shifts the pinned
        // column on top of its neighbour.
        ColumnSpec[] columns =
        [
            new(Name),
            new(Actions, DeclaredWidth, ColumnPinning.Right),
            new(Status, "100px", ColumnPinning.Right)
        ];

        await WithGridAsync(columns, async (grid, state, renderer) =>
        {
            state.Columns.SetWidth(Status, "0px");
            await ReRenderAsync(grid, state, renderer, columns);

            var headerStyle = HeaderStyle(renderer.Markup(), Actions);

            Assert.NotNull(headerStyle);
            Assert.Contains("right: 100px", headerStyle, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task APinnedColumnDoesNotOffsetByANegativeDeclaredWidth()
    {
        // A negative width is invalid CSS, so the browser drops it and the column is not actually
        // that wide: offsetting its neighbour backwards is worse than the 150px fallback.
        ColumnSpec[] columns =
        [
            new(Name),
            new(Actions, DeclaredWidth, ColumnPinning.Right),
            new(Status, "-10px", ColumnPinning.Right)
        ];

        await WithGridAsync(columns, async (grid, state, renderer) =>
        {
            await ReRenderAsync(grid, state, renderer, columns);

            var headerStyle = HeaderStyle(renderer.Markup(), Actions);

            Assert.NotNull(headerStyle);
            Assert.Contains("right: 150px", headerStyle, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task AResizeCallbackHearsWhatTheStateAccepted()
    {
        var reported = new List<(string ColumnId, string Width)>();

        await WithGridAsync(DefaultColumns, async (grid, state, _) =>
        {
            await grid.OnResizeCompleted(Name, new Dictionary<string, double> { [Name] = 220 });
            Assert.Equal(new[] { (Name, "220px") }, reported);

            // A refused width must not reach a consumer that persists it.
            state.Columns.SetWidth(Name, "180px");
            await grid.OnResizeCompleted(Name, new Dictionary<string, double> { [Name] = 0 });
            Assert.Equal(new[] { (Name, "220px"), (Name, "180px") }, reported);
        }, reported.Add);
    }

    private static async Task ReRenderAsync(
        StyledDataGrid grid,
        DataGridState<Row> state,
        ComponentTestRenderer renderer,
        ColumnSpec[]? columns = null) =>
        await renderer.Dispatcher.InvokeAsync(() =>
            grid.SetParametersAsync(ParameterView.FromDictionary(Parameters(state, columns ?? DefaultColumns))));

    private static async Task WithGridAsync(
        ColumnSpec[] columns,
        Func<StyledDataGrid, DataGridState<Row>, ComponentTestRenderer, Task> test,
        Action<(string ColumnId, string Width)>? onResize = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<IJSRuntime, NoopJavaScript>();
        services.AddBlazorBlueprintComponents();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, provider.GetRequiredService<ILoggerFactory>());

        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var state = new DataGridState<Row>();

            var grid = await renderer.MountAsync<StyledDataGrid>(Parameters(state, columns, onResize));

            // The grid registers its columns from their own OnInitialized, so the state has an entry
            // for every one of them by the time it has rendered. Everything below leans on that.
            Assert.Contains(state.Columns.Entries, e => e.ColumnId == columns[0].Id);

            await test(grid, state, renderer);
        });
    }

    private static Dictionary<string, object?> Parameters(
        DataGridState<Row> state,
        ColumnSpec[] columns,
        Action<(string ColumnId, string Width)>? onResize = null)
    {
        var parameters = new Dictionary<string, object?>
        {
            [nameof(StyledDataGrid.Items)] = new[] { new Row { Id = 1, Name = "One" } },
            [nameof(StyledDataGrid.State)] = state,
            [nameof(StyledDataGrid.ItemKey)] = (Func<Row, object>)(r => r.Id),
            [nameof(StyledDataGrid.Columns)] = BuildColumns(columns)
        };

        if (onResize != null)
        {
            parameters[nameof(StyledDataGrid.OnColumnResize)] =
                EventCallback.Factory.Create<(string ColumnId, string Width)>(new object(), onResize);
        }

        return parameters;
    }

    private static RenderFragment BuildColumns(ColumnSpec[] columns) => builder =>
    {
        var sequence = 0;
        foreach (var column in columns)
        {
            builder.OpenComponent<BbDataGridTemplateColumn<Row>>(sequence++);
            builder.AddAttribute(sequence++, "Id", column.Id);
            builder.AddAttribute(sequence++, "Title", column.Id);
            builder.AddAttribute(sequence++, "HeaderTemplate", (RenderFragment)(b => b.AddContent(0, column.Id)));

            if (column.Width != null)
            {
                builder.AddAttribute(sequence++, "Width", column.Width);
            }

            if (column.Pinned != ColumnPinning.None)
            {
                builder.AddAttribute(sequence++, "Pinned", column.Pinned);
            }

            builder.CloseComponent();
        }
    };

    /// <summary>The style written on a column's &lt;col&gt;, which is where its width lands.</summary>
    private static string? ColStyle(string markup, string columnId) => StyleOf(markup, "<col ", columnId);

    /// <summary>The style written on a column's header cell, where its pinned offset lands.</summary>
    private static string? HeaderStyle(string markup, string columnId) => StyleOf(markup, "<th ", columnId);

    private static string? StyleOf(string markup, string tag, string columnId)
    {
        var element = Regex.Match(
            markup,
            $"{Regex.Escape(tag)}[^>]*data-column-id=\"{Regex.Escape(columnId)}\"[^>]*>");

        if (!element.Success)
        {
            return null;
        }

        var style = Regex.Match(element.Value, "style=\"([^\"]*)\"");

        return style.Success ? style.Groups[1].Value : null;
    }

    private sealed record ColumnSpec(string Id, string? Width = null, ColumnPinning Pinned = ColumnPinning.None);

    public sealed class Row
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}

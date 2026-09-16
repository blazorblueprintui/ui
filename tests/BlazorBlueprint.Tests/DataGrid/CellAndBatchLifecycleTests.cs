using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using BlazorBlueprint.Tests.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Xunit;

namespace BlazorBlueprint.Tests.DataGrid;

public class CellAndBatchLifecycleTests
{
    [Fact]
    public async Task InvalidAndRejectedCellSavesRetainDraftAndCancelRestoresSource()
    {
        await WithGridAsync(DataGridEditMode.Cell, async grid =>
        {
            var row = grid.Items!.First();
            await grid.StartCellEditAsync(row, "name");
            var draft = Assert.Single(grid.PendingChanges).Item;
            draft.Name = "";
            Assert.False(await grid.CommitEditAsync());
            Assert.Equal("Original", row.Name);
            draft.Name = "Changed";
            Assert.False(await grid.CommitEditAsync());
            Assert.Equal("Original", row.Name);
            Assert.Equal("Changed", Assert.Single(grid.PendingChanges).Item.Name);
            await grid.CancelEditAsync();
            Assert.Empty(grid.PendingChanges);
            Assert.Null(grid.EditingItem);
            Assert.Equal("Original", row.Name);
        }, reject: true);
    }

    [Fact]
    public async Task BatchCellCancelPreservesPreviousStagedEditsAndSaveAppliesThem()
    {
        await WithGridAsync(DataGridEditMode.Batch, async grid =>
        {
            var row = grid.Items!.First();
            await grid.StartCellEditAsync(row, "name");
            Assert.Single(grid.PendingChanges).Item.Name = "Staged";
            Assert.True(await grid.CommitEditAsync());
            Assert.Equal("Original", row.Name);
            await grid.StartCellEditAsync(row, "hours");
            Assert.Single(grid.PendingChanges).Item.Hours = 50;
            await grid.CancelEditAsync();
            Assert.Equal(10, Assert.Single(grid.PendingChanges).Item.Hours);
            Assert.Equal("Staged", Assert.Single(grid.PendingChanges).Item.Name);
            Assert.True(await grid.CommitBatchAsync());
            Assert.Equal("Staged", row.Name);
            Assert.Equal(10, row.Hours);
            Assert.Empty(grid.PendingChanges);
        });
    }

    [Fact]
    public async Task RejectedBatchRetainsAllRowsAndCanBeDiscarded()
    {
        await WithGridAsync(DataGridEditMode.Batch, async grid =>
        {
            foreach (var row in grid.Items!)
            {
                await grid.StartCellEditAsync(row, "name");
                grid.PendingChanges.Single(c => c.OriginalItem == row).Item.Name = "Changed";
                Assert.True(await grid.CommitEditAsync());
            }
            Assert.False(await grid.CommitBatchAsync());
            Assert.Equal(2, grid.PendingChanges.Count);
            Assert.All(grid.Items!, r => Assert.Equal("Original", r.Name));
            await grid.CancelBatchAsync();
            Assert.Empty(grid.PendingChanges);
        }, reject: true);
    }

    [Fact]
    public async Task ChangedKeyCannotBeCommitted()
    {
        await WithGridAsync(DataGridEditMode.Cell, async grid =>
        {
            var row = grid.Items!.First();
            await grid.StartCellEditAsync(row, "name");
            Assert.Single(grid.PendingChanges).Item.Id = 100;
            Assert.False(await grid.CommitEditAsync());
            Assert.Equal(1, row.Id);
        });
    }

    private static async Task WithGridAsync(DataGridEditMode mode, Func<BbDataGrid<Row>, Task> test, bool reject = false)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger<>));
        services.AddSingleton<IJSRuntime, NoopJavaScript>();
        services.AddBlazorBlueprintComponents();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var grid = await renderer.MountAsync<BbDataGrid<Row>>(new()
            {
                [nameof(BbDataGrid<Row>.Items)] = new[] { new Row { Id = 1 }, new Row { Id = 2 } },
                [nameof(BbDataGrid<Row>.ItemKey)] = (Func<Row, object>)(r => r.Id),
                [nameof(BbDataGrid<Row>.EditMode)] = mode,
                [nameof(BbDataGrid<Row>.EditItemFactory)] = (Func<Row, Row>)(r => new Row { Id = r.Id, Name = r.Name, Hours = r.Hours }),
                [nameof(BbDataGrid<Row>.OnRowCommit)] = EventCallback.Factory.Create<DataGridRowCommitContext<Row>>(new object(), context => context.Cancel = reject),
                [nameof(BbDataGrid<Row>.OnBatchCommit)] = EventCallback.Factory.Create<DataGridBatchCommitContext<Row>>(new object(), context => context.Cancel = reject),
                [nameof(BbDataGrid<Row>.Columns)] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent<BbDataGridPropertyColumn<Row, string>>(0);
                    builder.AddAttribute(1, "Property", (Expression<Func<Row, string>>)(r => r.Name));
                    builder.AddAttribute(2, "EditTemplate", (RenderFragment<Row>)(r => b => b.AddContent(0, r.Name)));
                    builder.CloseComponent();
                    builder.OpenComponent<BbDataGridPropertyColumn<Row, int>>(3);
                    builder.AddAttribute(4, "Property", (Expression<Func<Row, int>>)(r => r.Hours));
                    builder.AddAttribute(5, "EditTemplate", (RenderFragment<Row>)(r => b => b.AddContent(0, r.Hours)));
                    builder.CloseComponent();
                })
            });
            await test(grid);
        });
    }

    public sealed class Row
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = "Original";
        public int Hours { get; set; } = 10;
    }
}

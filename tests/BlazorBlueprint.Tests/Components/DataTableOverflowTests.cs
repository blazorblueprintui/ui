using BlazorBlueprint.Components;
using Bunit;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// A table wider than its parent scrolls inside the bordered box instead of spilling past it.
/// </summary>
public class DataTableOverflowTests
{
    [Fact]
    public async Task BorderedTableContainerScrollsHorizontally()
    {
        await using var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = context.Render<BbDataTable<Row>>(parameters => parameters
            .Add(p => p.Data, new[] { new Row { Name = "One" } })
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<BbDataTableColumn<Row, string>>(0);
                builder.AddAttribute(1, nameof(BbDataTableColumn<Row, string>.Property),
                    (Func<Row, string?>)(r => r.Name));
                builder.AddAttribute(2, nameof(BbDataTableColumn<Row, string>.Header), "Name");
                builder.CloseComponent();
            }));

        var container = cut.Find("table").ParentElement!;
        Assert.Contains("bb:border", container.ClassList);
        Assert.Contains("bb:overflow-x-auto", container.ClassList);
    }

    public sealed class Row
    {
        public string Name { get; set; } = "";
    }
}

using BlazorBlueprint.Primitives.DataGrid;
using Xunit;

namespace BlazorBlueprint.Tests.DataGrid;

public class BufferedEditingTests
{
    [Fact]
    public void DraftsAreIsolatedAndReusedByStableKey()
    {
        var item = new Row { Id = 1, Name = "Original" };
        var buffer = new DataGridEditBuffer<Row>(r => new Row { Id = r.Id, Name = r.Name }, r => r.Id);
        var draft = buffer.Begin(item);
        draft.Name = "Edited";
        Assert.Equal("Original", item.Name);
        Assert.Same(draft, buffer.Begin(new Row { Id = 1 }));
        Assert.Same(draft, buffer.GetDisplayItem(item));
        Assert.Single(buffer.Changes);
        buffer.Discard(item);
        Assert.Same(item, buffer.GetDisplayItem(item));
        Assert.Empty(buffer.Changes);
    }

    [Fact]
    public void ReferenceIdentityIsUsedWithoutAKey()
    {
        var buffer = new DataGridEditBuffer<Row>(r => new Row { Id = r.Id });
        buffer.Begin(new Row { Id = 1 });
        buffer.Begin(new Row { Id = 1 });
        Assert.Equal(2, buffer.Count);
        buffer.Clear();
        Assert.Empty(buffer.Changes);
    }

    [Fact]
    public void FactoryCannotReturnSource() =>
        Assert.Throws<InvalidOperationException>(() => new DataGridEditBuffer<Row>(r => r).Begin(new Row()));

    [Fact]
    public void CapturedDraftCanBeAppliedAfterAcceptance()
    {
        var source = new Row { Id = 1, Name = "Old" };
        var draft = new Row { Id = 1, Name = "New" };
        DataGridRowSnapshot<Row>.Capture(draft).ApplyTo(source);
        Assert.Equal("New", source.Name);
        Assert.NotSame(source, draft);
    }

    private sealed class Row
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}

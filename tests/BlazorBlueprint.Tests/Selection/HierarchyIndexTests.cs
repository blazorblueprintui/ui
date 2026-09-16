using BlazorBlueprint.Components;
using Xunit;

namespace BlazorBlueprint.Tests.Selection;

public class HierarchyIndexTests
{
    [Fact]
    public void PathsUseKeysEvenWhenLabelsAreRepeated()
    {
        var index = Build([new("a", "Same", [new("b", "Same", [])]), new("c", "Same", [])]);
        Assert.Equal(["a", "b"], index.Path("b").Select(n => n.Value));
        Assert.Equal(["a", "c"], index.Roots);
        Assert.Empty(index.Path("missing"));
    }

    [Fact]
    public void DuplicateAndCyclicKeysAreRejected()
    {
        Assert.Throws<ArgumentException>(() => Build([new("a", "A", []), new("a", "B", [])]));
        var node = new Node("a", "A", []);
        node.Children.Add(node);
        Assert.Throws<ArgumentException>(() => Build([node]));
    }

    private static HierarchyIndex<Node> Build(IEnumerable<Node> nodes) => new(nodes, n => n.Id, n => n.Text, n => n.Children);
    private sealed record Node(string Id, string Text, List<Node> Children);
}

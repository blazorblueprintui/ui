using BlazorBlueprint.Components;
using Xunit;

namespace BlazorBlueprint.Tests.Performance;

#pragma warning disable BL0005
public class TreeSearchTests
{
    private sealed record Node(string Id, string Text, Node[] Children);

    [Theory]
    [InlineData(500)]
    [InlineData(1000)]
    public void MatchingSiblingsShareAnAncestorLookup(int count)
    {
        var children = Enumerable.Range(0, count).Select(i => new Node(i.ToString(System.Globalization.CultureInfo.InvariantCulture), "match", [])).ToArray();
        var root = new Node("root", "root", children);
        var keys = 0;
        var labels = 0;
        var tree = new BbTreeView<Node>
        {
            Items = new[] { root }, ValueField = n => { keys++; return n.Id; },
            TextField = n => { labels++; return n.Text; }, ChildrenProperty = n => n.Children,
            SearchText = "match"
        };
        ComponentProbe.Call(tree, "BuildItemIndex");
        keys = labels = 0;
        ComponentProbe.Call(tree, "ApplySearch");
        Assert.Equal(count + 1, labels);
        Assert.True(keys <= count + 1, $"Search read {keys} keys for {count + 1} nodes.");
        Assert.Equal("root", Assert.Single(ComponentProbe.Field<HashSet<string>>(tree, "searchExpandedValues")));
        Assert.False((bool)ComponentProbe.Call(tree, "IsFilteredOut", root)!);
        foreach (var child in children)
        {
            Assert.False((bool)ComponentProbe.Call(tree, "IsFilteredOut", child)!);
        }
        Assert.Equal(count + 1, labels); // Rendering never re-runs descendant searches.
    }

    [Fact]
    public void SearchShowsOnlyMatchesAndTheirAncestorsAndRefreshesAfterSourceChanges()
    {
        var match = new Node("match", "needle", []);
        var hidden = new Node("hidden", "other", []);
        var branch = new Node("branch", "branch", [match, hidden]);
        var root = new Node("root", "root", [branch]);
        var tree = new BbTreeView<Node>
        {
            Items = new[] { root }, ValueField = n => n.Id, TextField = n => n.Text,
            ChildrenProperty = n => n.Children, SearchText = "needle"
        };
        ComponentProbe.Call(tree, "BuildItemIndex");
        Assert.False((bool)ComponentProbe.Call(tree, "IsFilteredOut", root)!);
        Assert.False((bool)ComponentProbe.Call(tree, "IsFilteredOut", branch)!);
        Assert.True((bool)ComponentProbe.Call(tree, "IsFilteredOut", hidden)!);

        tree.Items = new[] { hidden };
        ComponentProbe.Call(tree, "BuildItemIndex");
        Assert.Empty(ComponentProbe.Field<HashSet<string>>(tree, "searchExpandedValues"));
        Assert.True((bool)ComponentProbe.Call(tree, "IsFilteredOut", hidden)!);
        tree.SearchText = "";
        Assert.False((bool)ComponentProbe.Call(tree, "IsFilteredOut", hidden)!);
    }

    [Fact]
    public void FlatSearchUsesParentLinksAndRestoresExpansionWhenCleared()
    {
        var root = new Node("root", "root", []);
        var child = new Node("child", "needle", []);
        var tree = new BbTreeView<Node>
        {
            Items = new[] { root, child }, ValueField = n => n.Id, TextField = n => n.Text,
            ParentField = n => n.Id == "root" ? null : "root", SearchText = "needle",
            ExpandedValues = new HashSet<string> { "previous" }
        };
        ComponentProbe.Call(tree, "BuildItemIndex");
        ComponentProbe.Call(tree, "ApplySearch");
        Assert.False((bool)ComponentProbe.Call(tree, "IsFilteredOut", root)!);
        Assert.Equal("root", Assert.Single(ComponentProbe.Field<HashSet<string>>(tree, "searchExpandedValues")));
        tree.SearchText = "";
        ComponentProbe.Call(tree, "ApplySearch");
        Assert.Equal("previous", Assert.Single(ComponentProbe.Field<HashSet<string>>(tree, "managedExpandedValues")));
    }

    [Fact]
    public void CachedLazyChildrenRemainSearchableAfterAParentRerender()
    {
        var root = new Node("root", "root", []);
        var child = new Node("child", "needle", []);
        var tree = new BbTreeView<Node>
        {
            Items = new[] { root }, ValueField = n => n.Id, TextField = n => n.Text,
            ChildrenProperty = n => n.Children, SearchText = "needle"
        };
        ComponentProbe.Field<Dictionary<string, List<Node>>>(tree, "loadedChildren")["root"] = [child];
        ComponentProbe.Call(tree, "BuildItemIndex");
        Assert.False((bool)ComponentProbe.Call(tree, "IsFilteredOut", root)!);
        Assert.False((bool)ComponentProbe.Call(tree, "IsFilteredOut", child)!);
        Assert.Equal("root", Assert.Single(ComponentProbe.Field<HashSet<string>>(tree, "searchExpandedValues")));
    }
}
#pragma warning restore BL0005

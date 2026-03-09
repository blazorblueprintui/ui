using BlazorBlueprint.Primitives.Utilities;
using Xunit;

namespace BlazorBlueprint.Tests.Utilities;

public class HierarchyManagerTests
{
    private sealed class TreeNode
    {
        public string Id { get; init; } = "";
        public string Name { get; init; } = "";
        public string? ParentId { get; init; }
        public List<TreeNode>? Children { get; init; }
    }

    private static HierarchyManager<TreeNode> CreateManager() =>
        new(n => n.Id);

    private static List<TreeNode> CreateNestedData()
    {
        return new List<TreeNode>
        {
            new()
            {
                Id = "1", Name = "Root A",
                Children = new List<TreeNode>
                {
                    new()
                    {
                        Id = "1.1", Name = "Child A1",
                        Children = new List<TreeNode>
                        {
                            new() { Id = "1.1.1", Name = "Grandchild A1a" },
                            new() { Id = "1.1.2", Name = "Grandchild A1b" }
                        }
                    },
                    new() { Id = "1.2", Name = "Child A2" }
                }
            },
            new()
            {
                Id = "2", Name = "Root B",
                Children = new List<TreeNode>
                {
                    new() { Id = "2.1", Name = "Child B1" }
                }
            },
            new() { Id = "3", Name = "Root C" }
        };
    }

    private static List<TreeNode> CreateFlatData()
    {
        return new List<TreeNode>
        {
            new() { Id = "1", Name = "Root A", ParentId = null },
            new() { Id = "2", Name = "Root B", ParentId = null },
            new() { Id = "3", Name = "Root C", ParentId = null },
            new() { Id = "1.1", Name = "Child A1", ParentId = "1" },
            new() { Id = "1.2", Name = "Child A2", ParentId = "1" },
            new() { Id = "2.1", Name = "Child B1", ParentId = "2" },
            new() { Id = "1.1.1", Name = "Grandchild A1a", ParentId = "1.1" },
            new() { Id = "1.1.2", Name = "Grandchild A1b", ParentId = "1.1" }
        };
    }

    [Fact]
    public void BuildFromNestedIndexesAllItems()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        Assert.True(manager.IsRegistered("1"));
        Assert.True(manager.IsRegistered("1.1"));
        Assert.True(manager.IsRegistered("1.1.1"));
        Assert.True(manager.IsRegistered("1.1.2"));
        Assert.True(manager.IsRegistered("1.2"));
        Assert.True(manager.IsRegistered("2"));
        Assert.True(manager.IsRegistered("2.1"));
        Assert.True(manager.IsRegistered("3"));
    }

    [Fact]
    public void BuildFromNestedSetsCorrectDepths()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        Assert.Equal(0, manager.GetDepth("1"));
        Assert.Equal(1, manager.GetDepth("1.1"));
        Assert.Equal(2, manager.GetDepth("1.1.1"));
        Assert.Equal(0, manager.GetDepth("3"));
    }

    [Fact]
    public void BuildFromNestedSetsCorrectParents()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        Assert.Null(manager.GetParentValue("1"));
        Assert.Equal("1", manager.GetParentValue("1.1"));
        Assert.Equal("1.1", manager.GetParentValue("1.1.1"));
        Assert.Equal("2", manager.GetParentValue("2.1"));
    }

    [Fact]
    public void BuildFromFlatIndexesAllItems()
    {
        var manager = CreateManager();
        manager.BuildFromFlat(CreateFlatData(), n => n.ParentId);

        Assert.True(manager.IsRegistered("1"));
        Assert.True(manager.IsRegistered("1.1"));
        Assert.True(manager.IsRegistered("1.1.1"));
    }

    [Fact]
    public void BuildFromFlatComputesCorrectDepths()
    {
        var manager = CreateManager();
        manager.BuildFromFlat(CreateFlatData(), n => n.ParentId);

        Assert.Equal(0, manager.GetDepth("1"));
        Assert.Equal(1, manager.GetDepth("1.1"));
        Assert.Equal(2, manager.GetDepth("1.1.1"));
    }

    [Fact]
    public void BuildFromFlatSetsCorrectParents()
    {
        var manager = CreateManager();
        manager.BuildFromFlat(CreateFlatData(), n => n.ParentId);

        Assert.Null(manager.GetParentValue("1"));
        Assert.Equal("1", manager.GetParentValue("1.1"));
        Assert.Equal("1.1", manager.GetParentValue("1.1.1"));
    }

    [Fact]
    public void GetRootItemsReturnsOnlyRoots()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        var roots = manager.GetRootItems();
        Assert.Equal(3, roots.Count);
        Assert.Equal("1", roots[0].Id);
        Assert.Equal("2", roots[1].Id);
        Assert.Equal("3", roots[2].Id);
    }

    [Fact]
    public void GetChildrenReturnsDirectChildren()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        var children = manager.GetChildren("1");
        Assert.Equal(2, children.Count);
        Assert.Equal("1.1", children[0].Id);
        Assert.Equal("1.2", children[1].Id);
    }

    [Fact]
    public void GetChildrenReturnsEmptyForLeaf()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        Assert.Empty(manager.GetChildren("3"));
    }

    [Fact]
    public void HasChildrenReturnsTrueForParent()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        Assert.True(manager.HasChildren("1"));
        Assert.True(manager.HasChildren("1.1"));
        Assert.False(manager.HasChildren("3"));
        Assert.False(manager.HasChildren("1.2"));
    }

    [Fact]
    public void GetChildCountReturnsCorrectCount()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        Assert.Equal(2, manager.GetChildCount("1"));
        Assert.Equal(2, manager.GetChildCount("1.1"));
        Assert.Equal(1, manager.GetChildCount("2"));
        Assert.Equal(0, manager.GetChildCount("3"));
    }

    [Fact]
    public void GetItemByValueReturnsCorrectItem()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        var item = manager.GetItemByValue("1.1");
        Assert.NotNull(item);
        Assert.Equal("Child A1", item!.Name);
    }

    [Fact]
    public void GetItemByValueReturnsNullForUnknown()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        Assert.Null(manager.GetItemByValue("nonexistent"));
    }

    [Fact]
    public void GetAllDescendantValuesReturnsAllDescendants()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        var descendants = manager.GetAllDescendantValues("1");
        Assert.Equal(4, descendants.Count);
        Assert.Contains("1.1", descendants);
        Assert.Contains("1.2", descendants);
        Assert.Contains("1.1.1", descendants);
        Assert.Contains("1.1.2", descendants);
    }

    [Fact]
    public void GetAllDescendantValuesReturnsEmptyForLeaf()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        Assert.Empty(manager.GetAllDescendantValues("3"));
    }

    [Fact]
    public void GetAncestorValuesReturnsBottomUp()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        var ancestors = manager.GetAncestorValues("1.1.1");
        Assert.Equal(2, ancestors.Count);
        Assert.Equal("1.1", ancestors[0]);
        Assert.Equal("1", ancestors[1]);
    }

    [Fact]
    public void GetAncestorValuesReturnsEmptyForRoot()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        Assert.Empty(manager.GetAncestorValues("1"));
    }

    [Fact]
    public void GetSiblingValuesExcludesSelf()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        var siblings = manager.GetSiblingValues("1.1");
        Assert.Single(siblings);
        Assert.Equal("1.2", siblings[0]);
    }

    [Fact]
    public void GetSetSizeReturnsCorrectCount()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        Assert.Equal(3, manager.GetSetSize("1"));
        Assert.Equal(2, manager.GetSetSize("1.1"));
        Assert.Equal(2, manager.GetSetSize("1.1.1"));
    }

    [Fact]
    public void GetPosInSetReturns1Based()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        Assert.Equal(1, manager.GetPosInSet("1"));
        Assert.Equal(2, manager.GetPosInSet("2"));
        Assert.Equal(3, manager.GetPosInSet("3"));
        Assert.Equal(1, manager.GetPosInSet("1.1"));
        Assert.Equal(2, manager.GetPosInSet("1.2"));
    }

    [Fact]
    public void FlattenAllCollapsedReturnsOnlyRoots()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        var result = manager.Flatten(new HashSet<string>());

        Assert.Equal(3, result.Count);
        Assert.Equal("1", result[0].Item.Id);
        Assert.True(result[0].HasChildren);
        Assert.False(result[0].IsExpanded);
        Assert.Equal(0, result[0].Depth);
        Assert.Equal("2", result[1].Item.Id);
        Assert.Equal("3", result[2].Item.Id);
        Assert.False(result[2].HasChildren);
    }

    [Fact]
    public void FlattenExpandedRootShowsChildren()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        var expanded = new HashSet<string> { "1" };
        var result = manager.Flatten(expanded);

        Assert.Equal(5, result.Count);
        Assert.Equal("1", result[0].Item.Id);
        Assert.True(result[0].IsExpanded);
        Assert.Equal("1.1", result[1].Item.Id);
        Assert.Equal(1, result[1].Depth);
        Assert.True(result[1].HasChildren);
        Assert.False(result[1].IsExpanded);
        Assert.Equal("1.2", result[2].Item.Id);
        Assert.Equal(1, result[2].Depth);
        Assert.True(result[2].IsLastChild);
        Assert.Equal("2", result[3].Item.Id);
        Assert.Equal("3", result[4].Item.Id);
    }

    [Fact]
    public void FlattenFullyExpandedShowsAllItems()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        var expanded = manager.GetExpandAllValues();
        var result = manager.Flatten(expanded);

        Assert.Equal(8, result.Count);
        Assert.Equal("1", result[0].Item.Id);
        Assert.Equal("1.1", result[1].Item.Id);
        Assert.Equal("1.1.1", result[2].Item.Id);
        Assert.Equal(2, result[2].Depth);
        Assert.Equal("1.1.2", result[3].Item.Id);
        Assert.Equal("1.2", result[4].Item.Id);
        Assert.Equal("2", result[5].Item.Id);
        Assert.Equal("2.1", result[6].Item.Id);
        Assert.Equal("3", result[7].Item.Id);
    }

    [Fact]
    public void FlattenWithSortingSortsSiblingsPerLevel()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        var expanded = manager.GetExpandAllValues();
        var result = manager.Flatten(expanded,
            sortComparison: (a, b) => string.Compare(b.Name, a.Name, StringComparison.Ordinal));

        Assert.Equal("3", result[0].Item.Id);
        Assert.Equal("2", result[1].Item.Id);
        Assert.Equal("2.1", result[2].Item.Id);
        Assert.Equal("1", result[3].Item.Id);
        Assert.Equal("1.2", result[4].Item.Id);
        Assert.Equal("1.1", result[5].Item.Id);
        Assert.Equal("1.1.2", result[6].Item.Id);
        Assert.Equal("1.1.1", result[7].Item.Id);
    }

    [Fact]
    public void FlattenWithFilterShowsMatchesAndAncestors()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        var expanded = manager.GetExpandAllValues();
        var result = manager.Flatten(expanded,
            filter: n => n.Name.Contains("Grandchild"));

        Assert.Equal(4, result.Count);
        Assert.Equal("1", result[0].Item.Id);
        Assert.False(result[0].MatchesFilter);
        Assert.Equal("1.1", result[1].Item.Id);
        Assert.False(result[1].MatchesFilter);
        Assert.Equal("1.1.1", result[2].Item.Id);
        Assert.True(result[2].MatchesFilter);
        Assert.Equal("1.1.2", result[3].Item.Id);
        Assert.True(result[3].MatchesFilter);
    }

    [Fact]
    public void FlattenWithFilterHidesNonMatchingBranches()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        var expanded = manager.GetExpandAllValues();
        var result = manager.Flatten(expanded,
            filter: n => n.Name == "Child B1");

        Assert.Equal(2, result.Count);
        Assert.Equal("2", result[0].Item.Id);
        Assert.False(result[0].MatchesFilter);
        Assert.Equal("2.1", result[1].Item.Id);
        Assert.True(result[1].MatchesFilter);
    }

    [Fact]
    public void FlattenWithSortAndFilterAppliesBoth()
    {
        var manager = CreateManager();
        var data = new List<TreeNode>
        {
            new()
            {
                Id = "1", Name = "Z-Parent",
                Children = new List<TreeNode>
                {
                    new() { Id = "1.1", Name = "Z-Child" },
                    new() { Id = "1.2", Name = "A-Match" }
                }
            },
            new()
            {
                Id = "2", Name = "A-Parent",
                Children = new List<TreeNode>
                {
                    new() { Id = "2.1", Name = "B-Match" }
                }
            }
        };
        manager.BuildFromNested(data, n => n.Children);

        var expanded = manager.GetExpandAllValues();
        var result = manager.Flatten(expanded,
            sortComparison: (a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal),
            filter: n => n.Name.Contains("Match"));

        Assert.Equal(4, result.Count);
        Assert.Equal("2", result[0].Item.Id);
        Assert.Equal("2.1", result[1].Item.Id);
        Assert.Equal("1", result[2].Item.Id);
        Assert.Equal("1.2", result[3].Item.Id);
    }

    [Fact]
    public void FlattenWithChildPageSizePaginatesChildren()
    {
        var manager = CreateManager();
        var children = Enumerable.Range(1, 12).Select(i =>
            new TreeNode { Id = $"1.{i}", Name = $"Child {i}" }).ToList();
        var data = new List<TreeNode>
        {
            new() { Id = "1", Name = "Parent", Children = children }
        };
        manager.BuildFromNested(data, n => n.Children);

        var expanded = new HashSet<string> { "1" };
        var result = manager.Flatten(expanded, childPageSize: 5);

        Assert.Equal(7, result.Count);
        Assert.Equal("1", result[0].Item.Id);
        Assert.Equal("1.1", result[1].Item.Id);
        Assert.Equal("1.5", result[5].Item.Id);
        Assert.True(result[6].IsChildPagerRow);
        Assert.Equal("1", result[6].ParentValue);
        Assert.Equal(0, result[6].ChildPageIndex);
        Assert.Equal(12, result[6].TotalChildren);
    }

    [Fact]
    public void FlattenWithChildPageIndexShowsCorrectPage()
    {
        var manager = CreateManager();
        var children = Enumerable.Range(1, 12).Select(i =>
            new TreeNode { Id = $"1.{i}", Name = $"Child {i}" }).ToList();
        var data = new List<TreeNode>
        {
            new() { Id = "1", Name = "Parent", Children = children }
        };
        manager.BuildFromNested(data, n => n.Children);

        var expanded = new HashSet<string> { "1" };
        var pageIndexes = new Dictionary<string, int> { { "1", 2 } };
        var result = manager.Flatten(expanded, childPageSize: 5, childPageIndexes: pageIndexes);

        Assert.Equal(4, result.Count);
        Assert.Equal("1", result[0].Item.Id);
        Assert.Equal("1.11", result[1].Item.Id);
        Assert.Equal("1.12", result[2].Item.Id);
        Assert.True(result[3].IsChildPagerRow);
        Assert.Equal(2, result[3].ChildPageIndex);
    }

    [Fact]
    public void FlattenChildrenBelowPageSizeNoPagerRow()
    {
        var manager = CreateManager();
        var data = new List<TreeNode>
        {
            new()
            {
                Id = "1", Name = "Parent",
                Children = new List<TreeNode>
                {
                    new() { Id = "1.1", Name = "Child 1" },
                    new() { Id = "1.2", Name = "Child 2" }
                }
            }
        };
        manager.BuildFromNested(data, n => n.Children);

        var expanded = new HashSet<string> { "1" };
        var result = manager.Flatten(expanded, childPageSize: 5);

        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, r => r.IsChildPagerRow);
    }

    [Fact]
    public void GetExpandAllValuesReturnsAllParents()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        var expandAll = manager.GetExpandAllValues();

        Assert.Contains("1", expandAll);
        Assert.Contains("1.1", expandAll);
        Assert.Contains("2", expandAll);
        Assert.DoesNotContain("3", expandAll);
        Assert.DoesNotContain("1.2", expandAll);
    }

    [Fact]
    public void GetExpandToDepthValuesReturnsCorrectNodes()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        var expand0 = manager.GetExpandToDepthValues(1);
        Assert.Contains("1", expand0);
        Assert.Contains("2", expand0);
        Assert.DoesNotContain("1.1", expand0);

        var expand1 = manager.GetExpandToDepthValues(2);
        Assert.Contains("1", expand1);
        Assert.Contains("1.1", expand1);
        Assert.Contains("2", expand1);
    }

    [Fact]
    public void ExpandAncestorsOfAddsAllAncestors()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        var expanded = new HashSet<string>();
        manager.ExpandAncestorsOf("1.1.1", expanded);

        Assert.Contains("1.1", expanded);
        Assert.Contains("1", expanded);
        Assert.Equal(2, expanded.Count);
    }

    [Fact]
    public void ClearRemovesAllData()
    {
        var manager = CreateManager();
        manager.BuildFromNested(CreateNestedData(), n => n.Children);

        manager.Clear();

        Assert.False(manager.IsRegistered("1"));
        Assert.Empty(manager.GetRootItems());
    }

    [Fact]
    public void AddChildrenAddsToExistingParent()
    {
        var manager = CreateManager();
        manager.BuildFromNested(new List<TreeNode>
        {
            new() { Id = "1", Name = "Root" }
        }, n => n.Children);

        Assert.False(manager.HasChildren("1"));

        manager.AddChildren("1", new List<TreeNode>
        {
            new() { Id = "1.1", Name = "Lazy Child 1" },
            new() { Id = "1.2", Name = "Lazy Child 2" }
        });

        Assert.True(manager.HasChildren("1"));
        Assert.Equal(2, manager.GetChildCount("1"));
        Assert.Equal(1, manager.GetDepth("1.1"));
    }

    [Fact]
    public void AddChildrenReplacesExistingChildren()
    {
        var manager = CreateManager();
        manager.BuildFromNested(new List<TreeNode>
        {
            new()
            {
                Id = "1", Name = "Root",
                Children = new List<TreeNode>
                {
                    new() { Id = "1.1", Name = "Old Child" }
                }
            }
        }, n => n.Children);

        manager.AddChildren("1", new List<TreeNode>
        {
            new() { Id = "1.new", Name = "New Child" }
        });

        Assert.False(manager.IsRegistered("1.1"));
        Assert.True(manager.IsRegistered("1.new"));
        Assert.Equal(1, manager.GetChildCount("1"));
    }

    [Fact]
    public void FlattenWithHasChildrenPredicateIndicatesExpandability()
    {
        var manager = CreateManager();
        var data = new List<TreeNode>
        {
            new() { Id = "1", Name = "Root" }
        };
        manager.BuildFromNested(data, n => n.Children);

        var result = manager.Flatten(
            new HashSet<string>(),
            hasChildrenPredicate: n => n.Name == "Root");

        Assert.Single(result);
        Assert.True(result[0].HasChildren);
    }
}

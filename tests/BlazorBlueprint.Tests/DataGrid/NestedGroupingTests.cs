using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.DataGrid;
using Xunit;

namespace BlazorBlueprint.Tests.DataGrid;

/// <summary>
/// Covers the state behind multi-level grouping: paths that identify a nested group without
/// colliding, and the level list that decides the nesting order.
/// </summary>
public class NestedGroupingTests
{
    private static readonly string[] DeptThenStatus = { "dept", "status" };

    private static readonly string[] StatusThenDept = { "status", "dept" };

    private static readonly string[] DeptThenYear = { "dept", "year" };

    private static readonly string[] StatusYearDept = { "status", "year", "dept" };

    private static readonly string[] YearOnly = { "year" };

    // --- GroupPath ------------------------------------------------------------------

    [Fact]
    public void TwoPathsWithTheSameKeysAreEqual()
    {
        var left = new GroupPath("Sales", "Active");
        var right = new GroupPath("Sales", "Active");

        Assert.Equal(left, right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void TheSameLeafKeyUnderDifferentParentsIsADifferentPath()
    {
        // This is the whole reason paths exist: a flat set of keys would collapse the "Active"
        // group under every department at once.
        var underSales = new GroupPath("Sales", "Active");
        var underSupport = new GroupPath("Support", "Active");

        Assert.NotEqual(underSales, underSupport);
    }

    [Fact]
    public void APathTracksItsOwnDepthAndLeafKey()
    {
        var path = new GroupPath("Sales", "Active", "2026");

        Assert.Equal(2, path.Depth);
        Assert.Equal("2026", path.Key);
        Assert.Equal(new GroupPath("Sales", "Active"), path.Parent());
    }

    [Fact]
    public void AppendReturnsAChildPathAndLeavesTheOriginalAlone()
    {
        var parent = new GroupPath("Sales");
        var child = parent.Append("Active");

        Assert.Equal(new GroupPath("Sales", "Active"), child);
        Assert.Equal(new GroupPath("Sales"), parent);
    }

    [Fact]
    public void ANullKeyIsALegitimateSegment()
    {
        var withNull = new GroupPath("Sales", null);

        Assert.Equal(new GroupPath("Sales", null), withNull);
        Assert.NotEqual(new GroupPath("Sales"), withNull);
    }

    [Fact]
    public void RootIsAnAncestorOfEveryPath()
    {
        Assert.True(GroupPath.Root.IsAncestorOfOrSelf(new GroupPath("Sales", "Active")));
        Assert.Equal(GroupPath.Root, new GroupPath("Sales").Parent());
    }

    [Fact]
    public void AncestryOnlyMatchesAPrefix()
    {
        var sales = new GroupPath("Sales");

        Assert.True(sales.IsAncestorOfOrSelf(new GroupPath("Sales", "Active")));
        Assert.True(sales.IsAncestorOfOrSelf(sales));
        Assert.False(sales.IsAncestorOfOrSelf(new GroupPath("Support", "Active")));
        Assert.False(new GroupPath("Sales", "Active").IsAncestorOfOrSelf(sales));
    }

    // --- Levels ---------------------------------------------------------------------

    [Fact]
    public void SetGroupsKeepsTheGivenOrder()
    {
        var state = new DataGridGroupState();

        state.SetGroups(new[] { Level("dept"), Level("status") });

        Assert.Equal(DeptThenStatus, state.ActiveGroups.Select(g => g.ColumnId));
        Assert.Equal(2, state.Depth);
        Assert.Equal("dept", state.ActiveGroup!.ColumnId);
    }

    [Fact]
    public void SetGroupsDropsARepeatedColumn()
    {
        // Grouping by the same column twice would produce a level where every group holds
        // exactly one child group.
        var state = new DataGridGroupState();

        state.SetGroups(new[] { Level("dept"), Level("status"), Level("dept") });

        Assert.Equal(DeptThenStatus, state.ActiveGroups.Select(g => g.ColumnId));
    }

    [Fact]
    public void AddGroupNestsInsideTheExistingLevels()
    {
        var state = new DataGridGroupState();
        state.SetGroup(Level("dept"));

        Assert.True(state.AddGroup(Level("status")));

        Assert.Equal(DeptThenStatus, state.ActiveGroups.Select(g => g.ColumnId));
    }

    [Fact]
    public void AddGroupIgnoresAColumnThatAlreadyGroups()
    {
        var state = new DataGridGroupState();
        state.SetGroup(Level("dept"));

        Assert.False(state.AddGroup(Level("dept")));
        Assert.Single(state.ActiveGroups);
    }

    [Fact]
    public void RemoveGroupKeepsTheOtherLevelsInOrder()
    {
        var state = new DataGridGroupState();
        state.SetGroups(new[] { Level("dept"), Level("status"), Level("year") });

        Assert.True(state.RemoveGroup("status"));

        Assert.Equal(DeptThenYear, state.ActiveGroups.Select(g => g.ColumnId));
    }

    [Fact]
    public void MoveGroupChangesWhichColumnGroupsOutermost()
    {
        var state = new DataGridGroupState();
        state.SetGroups(new[] { Level("dept"), Level("status") });

        Assert.True(state.MoveGroup("status", 0));

        Assert.Equal(StatusThenDept, state.ActiveGroups.Select(g => g.ColumnId));
    }

    [Fact]
    public void MoveGroupClampsAnIndexPastTheEnd()
    {
        var state = new DataGridGroupState();
        state.SetGroups(new[] { Level("dept"), Level("status"), Level("year") });

        state.MoveGroup("dept", 99);

        Assert.Equal(StatusYearDept, state.ActiveGroups.Select(g => g.ColumnId));
    }

    [Fact]
    public void GetLevelReportsWhereAColumnGroups()
    {
        var state = new DataGridGroupState();
        state.SetGroups(new[] { Level("dept"), Level("status") });

        Assert.Equal(0, state.GetLevel("dept"));
        Assert.Equal(1, state.GetLevel("status"));
        Assert.Equal(-1, state.GetLevel("salary"));
        Assert.True(state.IsGroupedBy("status"));
        Assert.False(state.IsGroupedBy("salary"));
    }

    [Fact]
    public void ChangingTheLevelsClearsCollapsedState()
    {
        // Every stored path is one level short of identifying a group once a level is added, so
        // keeping them would collapse the wrong groups.
        var state = new DataGridGroupState();
        state.SetGroup(Level("dept"));
        state.Toggle(new GroupPath("Sales"));
        Assert.True(state.IsCollapsed(new GroupPath("Sales")));

        state.AddGroup(Level("status"));

        Assert.Empty(state.CollapsedPaths);
    }

    // --- Collapse -------------------------------------------------------------------

    [Fact]
    public void CollapsingOneNestedGroupLeavesItsSiblingUnderAnotherParentAlone()
    {
        var state = new DataGridGroupState();
        state.SetGroups(new[] { Level("dept"), Level("status") });

        state.Toggle(new GroupPath("Sales", "Active"));

        Assert.True(state.IsCollapsed(new GroupPath("Sales", "Active")));
        Assert.False(state.IsCollapsed(new GroupPath("Support", "Active")));
    }

    [Fact]
    public void AGroupInsideACollapsedParentIsHiddenButNotItselfCollapsed()
    {
        // Keeping the two apart is what lets a subtree remember its own expansion while the
        // parent is shut.
        var state = new DataGridGroupState();
        state.SetGroups(new[] { Level("dept"), Level("status") });

        state.Toggle(new GroupPath("Sales"));

        Assert.False(state.IsCollapsed(new GroupPath("Sales", "Active")));
        Assert.True(state.IsHiddenByCollapse(new GroupPath("Sales", "Active")));
        Assert.False(state.IsHiddenByCollapse(new GroupPath("Support", "Active")));
    }

    [Fact]
    public void AnAncestorAtAnyDepthHidesADescendant()
    {
        var state = new DataGridGroupState();
        state.SetGroups(new[] { Level("dept"), Level("status"), Level("year") });

        state.Toggle(new GroupPath("Sales"));

        Assert.True(state.IsHiddenByCollapse(new GroupPath("Sales", "Active", "2026")));
    }

    [Fact]
    public void ATopLevelGroupIsNeverHiddenByCollapse()
    {
        var state = new DataGridGroupState();
        state.SetGroup(Level("dept"));
        state.Toggle(new GroupPath("Sales"));

        Assert.False(state.IsHiddenByCollapse(new GroupPath("Sales")));
    }

    [Fact]
    public void ExpandAllClearsEveryLevel()
    {
        var state = new DataGridGroupState();
        state.SetGroups(new[] { Level("dept"), Level("status") });
        state.CollapseAll(new[] { new GroupPath("Sales"), new GroupPath("Sales", "Active") });

        state.ExpandAll();

        Assert.Empty(state.CollapsedPaths);
    }

    // --- Single-level compatibility -------------------------------------------------

    [Fact]
    public void TheKeyBasedApiStillCollapsesATopLevelGroup()
    {
        var state = new DataGridGroupState();
        state.SetGroup(Level("dept"));

        state.Toggle("Sales");

        Assert.True(state.IsCollapsed("Sales"));
        Assert.True(state.IsCollapsed(new GroupPath("Sales")));
        Assert.Contains("Sales", state.CollapsedKeys);
    }

    [Fact]
    public void SetGroupReplacesEveryLevel()
    {
        var state = new DataGridGroupState();
        state.SetGroups(new[] { Level("dept"), Level("status") });

        state.SetGroup(Level("year"));

        Assert.Equal(YearOnly, state.ActiveGroups.Select(g => g.ColumnId));
    }

    [Fact]
    public void VersionAdvancesWheneverTheLevelsChange()
    {
        var state = new DataGridGroupState();
        var start = state.Version;

        state.SetGroup(Level("dept"));
        var afterSet = state.Version;
        state.AddGroup(Level("status"));
        var afterAdd = state.Version;
        state.RemoveGroup("status");

        Assert.True(afterSet > start);
        Assert.True(afterAdd > afterSet);
        Assert.True(state.Version > afterAdd);
    }

    [Fact]
    public void TogglingCollapseDoesNotAdvanceTheVersion()
    {
        // The version tells the grid its grouping shape changed. A collapse changes what is
        // rendered, not the shape, and advancing it would force a needless data refresh.
        var state = new DataGridGroupState();
        state.SetGroup(Level("dept"));
        var version = state.Version;

        state.Toggle(new GroupPath("Sales"));

        Assert.Equal(version, state.Version);
    }

    private static GroupDefinition Level(string columnId) =>
        new() { ColumnId = columnId, GroupSortDirection = SortDirection.Ascending };
}

using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.DataGrid;
using BlazorBlueprint.Primitives.Table;
using Xunit;

namespace BlazorBlueprint.Tests.DataGrid;

/// <summary>
/// Covers <see cref="DataGridSelectionBehavior.Replace"/>: the click, Shift+Click and Ctrl+Click
/// conventions a user brings from a file explorer, and the anchor row that Shift+Click extends
/// from.
/// </summary>
public class SelectionBehaviorTests
{
    private sealed class Row
    {
        public required string Name { get; init; }

        public override string ToString() => Name;
    }

    private static readonly int[] OneThenThree = { 1, 3 };

    private static List<Row> CreateRows() => new()
    {
        new Row { Name = "a" },
        new Row { Name = "b" },
        new Row { Name = "c" },
        new Row { Name = "d" },
        new Row { Name = "e" }
    };

    private static DataGridContext<Row> CreateContext(
        List<Row> rows,
        DataGridSelectionBehavior behavior,
        SelectionMode mode = SelectionMode.Multiple)
    {
        var state = new DataGridState<Row>();
        state.Selection.Mode = mode;

        return new DataGridContext<Row>(state)
        {
            ProcessedData = rows,
            SelectionMode = mode,
            SelectionBehavior = behavior
        };
    }

    private static string SelectedNames(DataGridContext<Row> context) =>
        string.Concat(context.State.Selection.SelectedItems
            .Select(r => r.Name)
            .OrderBy(n => n, StringComparer.Ordinal));

    // --- Toggle keeps today's behaviour --------------------------------------------

    [Fact]
    public void ToggleBehaviourFlipsOneRowAndLeavesTheRestAlone()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Toggle);

        context.ApplyRowSelectionInput(rows[0], RowSelectionModifiers.None);
        context.ApplyRowSelectionInput(rows[2], RowSelectionModifiers.None);

        Assert.Equal("ac", SelectedNames(context));

        context.ApplyRowSelectionInput(rows[0], RowSelectionModifiers.None);

        Assert.Equal("c", SelectedNames(context));
    }

    [Fact]
    public void ToggleBehaviourIgnoresModifierKeys()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Toggle);

        context.ApplyRowSelectionInput(rows[0], RowSelectionModifiers.None);
        context.ApplyRowSelectionInput(rows[3], new RowSelectionModifiers(Extend: true, Additive: false));

        // A range would have selected a, b, c and d. Toggle just adds d.
        Assert.Equal("ad", SelectedNames(context));
    }

    // --- Replace: plain click ------------------------------------------------------

    [Fact]
    public void PlainClickReplacesTheWholeSelection()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Replace);

        context.ApplyRowSelectionInput(rows[0], RowSelectionModifiers.None);
        context.ApplyRowSelectionInput(rows[3], new RowSelectionModifiers(Extend: true, Additive: false));
        Assert.Equal("abcd", SelectedNames(context));

        context.ApplyRowSelectionInput(rows[2], RowSelectionModifiers.None);

        Assert.Equal("c", SelectedNames(context));
    }

    [Fact]
    public void PlainClickOnTheOnlySelectedRowClearsTheSelection()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Replace);

        context.ApplyRowSelectionInput(rows[1], RowSelectionModifiers.None);
        Assert.Equal("b", SelectedNames(context));

        context.ApplyRowSelectionInput(rows[1], RowSelectionModifiers.None);

        Assert.Equal("", SelectedNames(context));
    }

    [Fact]
    public void PlainClickOnASelectedRowInAWiderSelectionCollapsesToThatRow()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Replace);

        context.ApplyRowSelectionInput(rows[0], RowSelectionModifiers.None);
        context.ApplyRowSelectionInput(rows[2], new RowSelectionModifiers(Extend: true, Additive: false));
        Assert.Equal("abc", SelectedNames(context));

        // b is already selected, but it is not the only selection, so this collapses rather
        // than clears.
        context.ApplyRowSelectionInput(rows[1], RowSelectionModifiers.None);

        Assert.Equal("b", SelectedNames(context));
    }

    // --- Replace: Shift+Click ------------------------------------------------------

    [Fact]
    public void ShiftClickSelectsTheRangeFromTheAnchor()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Replace);

        context.ApplyRowSelectionInput(rows[1], RowSelectionModifiers.None);
        context.ApplyRowSelectionInput(rows[3], new RowSelectionModifiers(Extend: true, Additive: false));

        Assert.Equal("bcd", SelectedNames(context));
    }

    [Fact]
    public void ShiftClickSelectsTheRangeWhenClickingUpwards()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Replace);

        context.ApplyRowSelectionInput(rows[3], RowSelectionModifiers.None);
        context.ApplyRowSelectionInput(rows[1], new RowSelectionModifiers(Extend: true, Additive: false));

        Assert.Equal("bcd", SelectedNames(context));
    }

    [Fact]
    public void RepeatedShiftClicksReExtendFromTheSameAnchor()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Replace);

        context.ApplyRowSelectionInput(rows[0], RowSelectionModifiers.None);
        context.ApplyRowSelectionInput(rows[3], new RowSelectionModifiers(Extend: true, Additive: false));
        Assert.Equal("abcd", SelectedNames(context));

        // The anchor stays at a, so shrinking the range works rather than walking it along.
        context.ApplyRowSelectionInput(rows[1], new RowSelectionModifiers(Extend: true, Additive: false));

        Assert.Equal("ab", SelectedNames(context));
    }

    [Fact]
    public void ShiftClickWithNoAnchorSelectsOnlyTheClickedRow()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Replace);

        context.ApplyRowSelectionInput(rows[2], new RowSelectionModifiers(Extend: true, Additive: false));

        Assert.Equal("c", SelectedNames(context));
    }

    [Fact]
    public void ShiftClickAfterAStaleAnchorSelectsOnlyTheClickedRow()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Replace);

        context.ApplyRowSelectionInput(rows[0], RowSelectionModifiers.None);

        // The page changed under the selection: the anchor is no longer among the visible rows.
        context.ProcessedData = rows.Skip(2).ToList();

        context.ApplyRowSelectionInput(rows[3], new RowSelectionModifiers(Extend: true, Additive: false));

        Assert.Equal("d", SelectedNames(context));
    }

    // --- Replace: Ctrl+Click -------------------------------------------------------

    [Fact]
    public void CtrlClickAddsARowWithoutDisturbingTheRest()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Replace);

        context.ApplyRowSelectionInput(rows[0], RowSelectionModifiers.None);
        context.ApplyRowSelectionInput(rows[3], new RowSelectionModifiers(Extend: false, Additive: true));

        Assert.Equal("ad", SelectedNames(context));
    }

    [Fact]
    public void CtrlClickRemovesAnAlreadySelectedRow()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Replace);

        context.ApplyRowSelectionInput(rows[0], RowSelectionModifiers.None);
        context.ApplyRowSelectionInput(rows[2], new RowSelectionModifiers(Extend: true, Additive: false));
        Assert.Equal("abc", SelectedNames(context));

        context.ApplyRowSelectionInput(rows[1], new RowSelectionModifiers(Extend: false, Additive: true));

        Assert.Equal("ac", SelectedNames(context));
    }

    [Fact]
    public void CtrlClickMovesTheAnchorSoAFollowingShiftClickExtendsFromIt()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Replace);

        context.ApplyRowSelectionInput(rows[0], RowSelectionModifiers.None);
        context.ApplyRowSelectionInput(rows[2], new RowSelectionModifiers(Extend: false, Additive: true));
        Assert.Equal("ac", SelectedNames(context));

        // The anchor is c, not a. Extending to e must give c, d, e — not a through e.
        context.ApplyRowSelectionInput(rows[4], new RowSelectionModifiers(Extend: true, Additive: false));

        Assert.Equal("cde", SelectedNames(context));
    }

    // --- Modes ---------------------------------------------------------------------

    [Fact]
    public void SingleModeTreatsEveryClickAsAPlainClick()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Replace, SelectionMode.Single);

        context.ApplyRowSelectionInput(rows[0], RowSelectionModifiers.None);
        context.ApplyRowSelectionInput(rows[3], new RowSelectionModifiers(Extend: true, Additive: true));

        Assert.Equal("d", SelectedNames(context));
    }

    [Fact]
    public void NoneModeSelectsNothing()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Replace, SelectionMode.None);

        context.ApplyRowSelectionInput(rows[0], RowSelectionModifiers.None);

        Assert.Equal("", SelectedNames(context));
    }

    // --- Checkboxes stay toggle ----------------------------------------------------

    [Fact]
    public void ToggleRowSelectionStaysToggleUnderReplaceSoCheckboxesKeepWorking()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Replace);

        context.ToggleRowSelection(rows[0]);
        context.ToggleRowSelection(rows[2]);

        Assert.Equal("ac", SelectedNames(context));
    }

    // --- Callbacks -----------------------------------------------------------------

    [Fact]
    public void SelectionChangeIsReportedOncePerClick()
    {
        var rows = CreateRows();
        var context = CreateContext(rows, DataGridSelectionBehavior.Replace);
        var reported = new List<int>();
        context.OnSelectionChange = items => reported.Add(items.Count);

        context.ApplyRowSelectionInput(rows[0], RowSelectionModifiers.None);
        context.ApplyRowSelectionInput(rows[2], new RowSelectionModifiers(Extend: true, Additive: false));

        Assert.Equal(OneThenThree, reported);
    }
}

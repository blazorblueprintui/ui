using System.Globalization;
using BlazorBlueprint.Primitives.Gantt;
using Xunit;

namespace BlazorBlueprint.Tests.Gantt;

/// <summary>
/// The chart engine: the tree, rolled-up summaries, the timeline, and the arrows between bars.
/// </summary>
public class GanttBuilderTests
{
    private static readonly TimeSpan Utc = TimeSpan.Zero;

    private sealed record Task(string Id, string? Parent, string Text, DateTimeOffset Start, DateTimeOffset End, double Progress = 0);

    private static DateTimeOffset On(int year, int month, int day, int hour = 0) =>
        new(new DateTime(year, month, day, hour, 0, 0, DateTimeKind.Unspecified), Utc);

    private static GanttSource<Task> Source(IEnumerable<Task> tasks) => new()
    {
        Items = tasks,
        Id = t => t.Id,
        ParentId = t => t.Parent,
        Text = t => t.Text,
        Start = t => t.Start,
        End = t => t.End,
        Progress = t => t.Progress,
        Labels = new GanttLabels { Culture = CultureInfo.InvariantCulture },
    };

    private static readonly Task[] Plan =
    [
        new("design", null, "Design", On(2026, 3, 2), On(2026, 3, 6)),
        new("wire", "design", "Wireframes", On(2026, 3, 2), On(2026, 3, 4), 1.0),
        new("visual", "design", "Visuals", On(2026, 3, 4), On(2026, 3, 6), 0.5),
        new("build", null, "Build", On(2026, 3, 9), On(2026, 3, 20), 0.25),
        new("ship", null, "Ship", On(2026, 3, 20), On(2026, 3, 20)),
    ];

    // ─── Tree ───────────────────────────────────────────────────────────────

    [Fact]
    public void NestsChildrenUnderTheirParentInDeclarationOrder()
    {
        var chart = GanttBuilder.Build(Source(Plan));

        Assert.Equal(["design", "wire", "visual", "build", "ship"], chart.Rows.Select(r => r.Id));
        Assert.Equal([0, 1, 1, 0, 0], chart.Rows.Select(r => r.Depth));
    }

    [Fact]
    public void MarksATaskWithChildrenAsASummary()
    {
        var chart = GanttBuilder.Build(Source(Plan));

        Assert.True(chart.Rows[0].IsSummary);
        Assert.Equal(2, chart.Rows[0].ChildCount);
        Assert.False(chart.Rows[1].IsSummary);
    }

    [Fact]
    public void TreatsAZeroLengthLeafAsAMilestone()
    {
        var chart = GanttBuilder.Build(Source(Plan));

        Assert.True(chart.Rows.Single(r => r.Id == "ship").IsMilestone);
        Assert.False(chart.Rows.Single(r => r.Id == "build").IsMilestone);
    }

    [Fact]
    public void TreatsAParentThatIsNotInTheSetAsNoParent()
    {
        var chart = GanttBuilder.Build(Source([new Task("orphan", "gone", "Orphan", On(2026, 3, 2), On(2026, 3, 3))]));

        Assert.Equal(0, chart.Rows[0].Depth);
    }

    [Fact]
    public void RejectsTwoTasksSharingAnIdentifier()
    {
        var duplicate = new[]
        {
            new Task("a", null, "One", On(2026, 3, 2), On(2026, 3, 3)),
            new Task("a", null, "Two", On(2026, 3, 2), On(2026, 3, 3)),
        };

        Assert.Throws<GanttBuildException>(() => GanttBuilder.Build(Source(duplicate)));
    }

    [Fact]
    public void RejectsALoopInTheParentIdentifiers()
    {
        var loop = new[]
        {
            new Task("a", "b", "A", On(2026, 3, 2), On(2026, 3, 3)),
            new Task("b", "a", "B", On(2026, 3, 2), On(2026, 3, 3)),
        };

        Assert.Throws<GanttBuildException>(() => GanttBuilder.Build(Source(loop)));
    }

    [Fact]
    public void SortsSiblingsWithoutMovingThemOutOfTheirBranch()
    {
        var sorted = new GanttSource<Task>
        {
            Items = Plan,
            Id = t => t.Id,
            ParentId = t => t.Parent,
            Text = t => t.Text,
            Start = t => t.Start,
            End = t => t.End,
            Order = Comparer<Task>.Create((a, b) => string.CompareOrdinal(a.Text, b.Text)),
        };

        var chart = GanttBuilder.Build(sorted);

        // Build, Design, Ship at the top; Visuals and Wireframes stay under Design.
        Assert.Equal(["build", "design", "visual", "wire", "ship"], chart.Rows.Select(r => r.Id));
    }

    // ─── Roll-up ────────────────────────────────────────────────────────────

    [Fact]
    public void TakesASummarySpanFromItsChildren()
    {
        var chart = GanttBuilder.Build(Source(Plan));
        var design = chart.Rows[0];

        Assert.Equal(On(2026, 3, 2), design.Start);
        Assert.Equal(On(2026, 3, 6), design.End);
    }

    [Fact]
    public void WeightsASummaryProgressByHowLongEachChildRuns()
    {
        var weighted = new[]
        {
            new Task("top", null, "Top", On(2026, 3, 2), On(2026, 3, 2)),
            new Task("long", "top", "Long", On(2026, 3, 2), On(2026, 3, 12), 0.5),
            new Task("short", "top", "Short", On(2026, 3, 2), On(2026, 3, 3), 1.0),
        };

        var chart = GanttBuilder.Build(Source(weighted));

        // Ten days half done plus one day finished is six of eleven.
        Assert.Equal(6.0 / 11.0, chart.Rows[0].Progress, 6);
    }

    [Fact]
    public void AveragesASummaryOfMilestonesEqually()
    {
        var milestones = new[]
        {
            new Task("top", null, "Top", On(2026, 3, 2), On(2026, 3, 2)),
            new Task("a", "top", "A", On(2026, 3, 2), On(2026, 3, 2), 1.0),
            new Task("b", "top", "B", On(2026, 3, 4), On(2026, 3, 4), 0.0),
        };

        var chart = GanttBuilder.Build(Source(milestones));

        Assert.Equal(0.5, chart.Rows[0].Progress, 6);
    }

    [Fact]
    public void LeavesASummaryAloneWhenRollUpIsOff()
    {
        var source = new GanttSource<Task>
        {
            Items = Plan,
            Id = t => t.Id,
            ParentId = t => t.Parent,
            Text = t => t.Text,
            Start = t => t.Start,
            End = t => t.End,
            Progress = t => t.Progress,
            RollUpSummaries = false,
        };

        var chart = GanttBuilder.Build(source);

        Assert.Equal(On(2026, 3, 2), chart.Rows[0].Start);
        Assert.Equal(On(2026, 3, 6), chart.Rows[0].End);
        Assert.Equal(0, chart.Rows[0].Progress);
    }

    [Fact]
    public void ClampsProgressToTheZeroToOneRange()
    {
        var chart = GanttBuilder.Build(Source([new Task("a", null, "A", On(2026, 3, 2), On(2026, 3, 3), 4.0)]));

        Assert.Equal(1, chart.Rows[0].Progress);
    }

    [Fact]
    public void DrawsATaskThatFinishesBeforeItStartsAsAMilestone()
    {
        var chart = GanttBuilder.Build(Source([new Task("a", null, "A", On(2026, 3, 5), On(2026, 3, 1))]));

        Assert.True(chart.Rows[0].IsMilestone);
        Assert.Equal(0, chart.Rows[0].LengthSlots, 6);
    }

    // ─── Collapsing ─────────────────────────────────────────────────────────

    [Fact]
    public void HidesTheChildrenOfACollapsedTask()
    {
        var collapsed = new GanttSource<Task>
        {
            Items = Plan,
            Id = t => t.Id,
            ParentId = t => t.Parent,
            Text = t => t.Text,
            Start = t => t.Start,
            End = t => t.End,
            Collapsed = new HashSet<string> { "design" },
        };

        var chart = GanttBuilder.Build(collapsed);

        Assert.Equal(["design", "build", "ship"], chart.Rows.Select(r => r.Id));
        Assert.False(chart.Rows[0].IsExpanded);
        Assert.Equal(5, chart.TaskCount);
    }

    // ─── Axis ───────────────────────────────────────────────────────────────

    [Fact]
    public void WidensTheRangeToWholeMonthsAtDayZoom()
    {
        var chart = GanttBuilder.Build(Source(Plan));

        Assert.Equal(On(2026, 3, 1), chart.Axis.Start);
        Assert.Equal(On(2026, 4, 1), chart.Axis.End);
        Assert.Equal(31, chart.Axis.SlotCount);
        Assert.Equal("Mar", chart.Axis.Major.Single().Label);
        Assert.Equal(31, chart.Axis.Major.Single().Span);
    }

    [Fact]
    public void KeepsTheRangeExactWhenNothingIsSnapped()
    {
        var source = new GanttSource<Task>
        {
            Items = Plan,
            Id = t => t.Id,
            ParentId = t => t.Parent,
            Text = t => t.Text,
            Start = t => t.Start,
            End = t => t.End,
            Snap = GanttRangeSnap.None,
        };

        var chart = GanttBuilder.Build(source);

        Assert.Equal(On(2026, 3, 2), chart.Axis.Start);
        Assert.Equal(On(2026, 3, 20), chart.Axis.End);
    }

    [Fact]
    public void ShadesSaturdayAndSundayAtDayZoom()
    {
        var chart = GanttBuilder.Build(Source(Plan));
        var shaded = chart.Axis.Minor.Where(s => s.IsNonWorking).Select(s => s.Start.Day).ToArray();

        // March 2026 starts on a Sunday.
        Assert.Equal([1, 7, 8, 14, 15, 21, 22, 28, 29], shaded);
    }

    [Fact]
    public void ShadesNoSlotWhereASlotIsLongerThanADay()
    {
        var chart = Zoomed(GanttZoom.Week);

        Assert.DoesNotContain(chart.Axis.Minor, s => s.IsNonWorking);
    }

    [Theory]
    [InlineData(GanttZoom.Hour, 432)]
    [InlineData(GanttZoom.Day, 31)]
    [InlineData(GanttZoom.Week, 5)]
    [InlineData(GanttZoom.Month, 1)]
    [InlineData(GanttZoom.Quarter, 1)]
    [InlineData(GanttZoom.Year, 1)]
    public void CutsTheTimelineIntoTheSlotsTheZoomAsksFor(GanttZoom zoom, int expected)
    {
        var chart = Zoomed(zoom);

        Assert.Equal(expected, chart.Axis.SlotCount);
    }

    [Fact]
    public void WidensToWholeMonthsRatherThanAWholeYearAtMonthZoom()
    {
        var axis = Zoomed(GanttZoom.Month).Axis;

        // Snapping a three-week plan out to a whole year would leave it one slot in twelve.
        Assert.Equal(On(2026, 3, 1), axis.Start);
        Assert.Equal(On(2026, 4, 1), axis.End);
        Assert.Equal("2026", axis.Major.Single().Label);
    }

    [Fact]
    public void GivesTheYearZoomNoUpperTier() => Assert.Empty(Zoomed(GanttZoom.Year).Axis.Major);

    [Fact]
    public void CoversEveryMinorSlotExactlyOnceWithTheUpperTier()
    {
        foreach (var zoom in Enum.GetValues<GanttZoom>())
        {
            var axis = Zoomed(zoom).Axis;
            if (axis.Major.Count == 0)
            {
                continue;
            }

            Assert.Equal(axis.SlotCount, axis.Major.Sum(m => m.Span));
        }
    }

    [Fact]
    public void NumbersAWeekSlotByItsIsoWeek()
    {
        var axis = Zoomed(GanttZoom.Week).Axis;

        Assert.Equal("W10", axis.Minor[0].Label);
    }

    [Fact]
    public void NamesAQuarterSlotByItsNumber()
    {
        var axis = Zoomed(GanttZoom.Quarter).Axis;

        Assert.Equal(["Q1"], axis.Minor.Select(s => s.Label));
    }

    // ─── Positions ──────────────────────────────────────────────────────────

    [Fact]
    public void MeasuresABarInSlotsFromTheLeftEdge()
    {
        var chart = GanttBuilder.Build(Source(Plan));
        var build = chart.Rows.Single(r => r.Id == "build");

        // The axis opens on 1 March, so the ninth is eight days in and runs for eleven.
        Assert.Equal(8, build.OffsetSlots, 6);
        Assert.Equal(11, build.LengthSlots, 6);
    }

    [Fact]
    public void GivesEverySlotTheSameWidthEvenWhereTheMonthsDiffer()
    {
        var source = new GanttSource<Task>
        {
            Items = [new Task("a", null, "A", On(2026, 2, 1), On(2026, 4, 1))],
            Id = t => t.Id,
            Text = t => t.Text,
            Start = t => t.Start,
            End = t => t.End,
            Zoom = GanttZoom.Month,
        };

        var chart = GanttBuilder.Build(source);

        // February is three days shorter than March and still exactly one slot wide.
        Assert.Equal(1, chart.Axis.Position(On(2026, 3, 1)) - chart.Axis.Position(On(2026, 2, 1)), 6);
        Assert.Equal(1, chart.Axis.Position(On(2026, 4, 1)) - chart.Axis.Position(On(2026, 3, 1)), 6);
    }

    [Fact]
    public void ReadsAPositionInsideASlotAsAFractionOfIt()
    {
        var chart = GanttBuilder.Build(Source(Plan));

        Assert.Equal(1.5, chart.Axis.Position(On(2026, 3, 2, 12)), 6);
    }

    [Fact]
    public void ClampsAPositionOutsideTheRangeToItsEdge()
    {
        var chart = GanttBuilder.Build(Source(Plan));

        Assert.Equal(0, chart.Axis.Position(On(2020, 1, 1)), 6);
        Assert.Equal(31, chart.Axis.Position(On(2030, 1, 1)), 6);
    }

    // ─── Dependencies ───────────────────────────────────────────────────────

    [Fact]
    public void TiesAFinishToStartArrowToTheRightEndsOfTheTwoBars()
    {
        var chart = Linked(new GanttDependency("wire", "visual"));
        var link = Assert.Single(chart.Links);

        Assert.Equal(GanttAnchor.End, link.FromAnchor);
        Assert.Equal(GanttAnchor.Start, link.ToAnchor);
        Assert.Equal(3, link.FromSlots, 6);
        Assert.Equal(3, link.ToSlots, 6);
    }

    [Theory]
    [InlineData(GanttDependencyType.StartToStart, GanttAnchor.Start, GanttAnchor.Start)]
    [InlineData(GanttDependencyType.FinishToFinish, GanttAnchor.End, GanttAnchor.End)]
    [InlineData(GanttDependencyType.StartToFinish, GanttAnchor.Start, GanttAnchor.End)]
    public void TiesEachDependencyTypeToTheEndsItsNameSays(
        GanttDependencyType type,
        GanttAnchor from,
        GanttAnchor to)
    {
        var link = Assert.Single(Linked(new GanttDependency("wire", "build", type)).Links);

        Assert.Equal(from, link.FromAnchor);
        Assert.Equal(to, link.ToAnchor);
    }

    [Fact]
    public void ReTiesAnArrowFromAHiddenTaskToTheSummaryThatHidesIt()
    {
        var source = new GanttSource<Task>
        {
            Items = Plan,
            Id = t => t.Id,
            ParentId = t => t.Parent,
            Text = t => t.Text,
            Start = t => t.Start,
            End = t => t.End,
            Collapsed = new HashSet<string> { "design" },
            Dependencies = [new GanttDependency("visual", "build")],
        };

        var chart = GanttBuilder.Build(source);
        var link = Assert.Single(chart.Links);

        Assert.Equal(chart.Rows.Single(r => r.Id == "design").Index, link.FromRow);
    }

    [Fact]
    public void DropsAnArrowWhoseTwoEndsFoldIntoTheSameRow()
    {
        var source = new GanttSource<Task>
        {
            Items = Plan,
            Id = t => t.Id,
            ParentId = t => t.Parent,
            Text = t => t.Text,
            Start = t => t.Start,
            End = t => t.End,
            Collapsed = new HashSet<string> { "design" },
            Dependencies = [new GanttDependency("wire", "visual")],
        };

        Assert.Empty(GanttBuilder.Build(source).Links);
    }

    [Fact]
    public void DropsAnArrowNamingATaskThatIsNotThere() =>
        Assert.Empty(Linked(new GanttDependency("wire", "nothing")).Links);

    // ─── Empty ──────────────────────────────────────────────────────────────

    [Fact]
    public void BuildsAnEmptyChartWithoutThrowing()
    {
        var chart = GanttBuilder.Build(Source([]));

        Assert.True(chart.IsEmpty);
        Assert.Equal(0, chart.TaskCount);
        Assert.NotEmpty(chart.Axis.Minor);
    }

    private static GanttChart<Task> Zoomed(GanttZoom zoom) => GanttBuilder.Build(new GanttSource<Task>
    {
        Items = Plan,
        Id = t => t.Id,
        ParentId = t => t.Parent,
        Text = t => t.Text,
        Start = t => t.Start,
        End = t => t.End,
        Zoom = zoom,
        Labels = new GanttLabels { Culture = CultureInfo.InvariantCulture },
    });

    private static GanttChart<Task> Linked(params GanttDependency[] dependencies) => GanttBuilder.Build(new GanttSource<Task>
    {
        Items = Plan,
        Id = t => t.Id,
        ParentId = t => t.Parent,
        Text = t => t.Text,
        Start = t => t.Start,
        End = t => t.End,
        Dependencies = dependencies,
        Labels = new GanttLabels { Culture = CultureInfo.InvariantCulture },
    });
}

using BlazorBlueprint.Components;
using Xunit;

namespace BlazorBlueprint.Tests.Scheduler;

public class SchedulerEngineTests
{
    [Fact]
    public void WeeklySeriesPreservesLocalTimeAcrossDst()
    {
        var item = Appointment(new DateTime(2026, 3, 1, 9, 0, 0), "America/New_York");
        item.RecurrenceRule = "FREQ=WEEKLY;COUNT=3";
        var occurrences = SchedulerEngine.Expand([item], Utc(2026, 3, 1), Utc(2026, 3, 20));
        Assert.Equal(3, occurrences.Count);
        Assert.Equal([14, 13, 13], occurrences.Select(o => o.Start.UtcDateTime.Hour));
        Assert.All(occurrences, o => Assert.Equal(TimeSpan.FromHours(1), o.End - o.Start));
    }

    [Fact]
    public void MissingDstTimeIsRejectedAndRepeatedTimeHasTwoExplicitChoices()
    {
        Assert.Throws<ArgumentException>(() => SchedulerEngine.ToInstant(new DateTime(2026, 3, 8, 2, 30, 0), "America/New_York"));
        var local = new DateTime(2026, 11, 1, 1, 30, 0);
        var earlier = SchedulerEngine.ToInstant(local, "America/New_York");
        var later = SchedulerEngine.ToInstant(local, "America/New_York", SchedulerAmbiguousTimeResolution.Later);
        Assert.Equal(TimeSpan.FromHours(1), later - earlier);
    }

    [Fact]
    public void MonthlyThirtyFirstSkipsMonthsWithoutThatDay()
    {
        var item = Appointment(new DateTime(2026, 1, 31, 9, 0, 0));
        item.RecurrenceRule = "FREQ=MONTHLY;COUNT=3";
        var results = SchedulerEngine.Expand([item], Utc(2026, 1, 1), Utc(2026, 7, 1));
        Assert.Equal([1, 3, 5], results.Select(o => o.Start.Month));
    }

    [Fact]
    public void OverlapAndExclusiveEndAreRespected()
    {
        var item = Appointment(new DateTime(2026, 9, 16, 23, 0, 0));
        item.End = item.Start.AddHours(3);
        Assert.Single(SchedulerEngine.Expand([item], Utc(2026, 9, 17), Utc(2026, 9, 18)));
        Assert.Empty(SchedulerEngine.Expand([item], Utc(2026, 9, 16), item.Start));
    }

    [Fact]
    public void EditingAnOccurrenceCreatesExceptionAndIndependentReplacement()
    {
        var series = Appointment(new DateTime(2026, 9, 16, 9, 0, 0));
        series.RecurrenceRule = "FREQ=DAILY;COUNT=3";
        var originalStart = series.Start.AddDays(1);
        var edited = series.Clone();
        edited.Title = "Rescheduled";
        edited.Start = originalStart.AddHours(2);
        edited.End = edited.Start.AddHours(1);
        var events = SchedulerEngine.ApplyChange([series], edited, SchedulerChangeKind.Update, SchedulerEditScope.Occurrence, originalStart);
        Assert.Empty(series.ExcludedStarts);
        Assert.Equal(2, events.Count);
        var results = SchedulerEngine.Expand(events, Utc(2026, 9, 16), Utc(2026, 9, 20));
        Assert.Equal(3, results.Count);
        Assert.DoesNotContain(results, o => o.Start == originalStart);
        Assert.Single(results, o => o.Event.Title == "Rescheduled");
        Assert.Empty(SchedulerEngine.ApplyChange(events, series, SchedulerChangeKind.Delete));
    }

    [Fact]
    public void DeleteOccurrenceDoesNotDeleteSeries()
    {
        var series = Appointment(new DateTime(2026, 9, 16, 9, 0, 0));
        series.RecurrenceRule = "FREQ=DAILY;COUNT=3";
        var events = SchedulerEngine.ApplyChange([series], series, SchedulerChangeKind.Delete, SchedulerEditScope.Occurrence, series.Start);
        Assert.Equal(2, SchedulerEngine.Expand(events, Utc(2026, 9, 16), Utc(2026, 9, 20)).Count);
    }

    [Fact]
    public void UnboundedRecurrenceIsLimitedByVisibleRangeAndSafetyCap()
    {
        var item = Appointment(new DateTime(2026, 1, 1, 9, 0, 0));
        item.RecurrenceRule = "FREQ=DAILY";
        Assert.Equal(7, SchedulerEngine.Expand([item], Utc(2026, 9, 1), Utc(2026, 9, 8)).Count);
        Assert.Throws<InvalidOperationException>(() => SchedulerEngine.Expand([item], Utc(2026, 9, 1), Utc(2026, 9, 8), 2));
    }

    [Fact]
    public void UntilAndWeekdayFiltersAreInclusiveOfTheLastMatchingInstant()
    {
        var item = Appointment(new DateTime(2026, 9, 14, 9, 0, 0));
        item.RecurrenceRule = "FREQ=WEEKLY;BYDAY=MO,WE;UNTIL=20260923T090000Z";
        var results = SchedulerEngine.Expand([item], Utc(2026, 9, 1), Utc(2026, 10, 1));
        Assert.Equal([14, 16, 21, 23], results.Select(o => o.Start.Day));
    }

    [Fact]
    public void InvalidFrequencyAndDuplicateIdsAreRejected()
    {
        var item = Appointment(new DateTime(2026, 9, 14, 9, 0, 0));
        item.RecurrenceRule = "FREQ=SECONDLY";
        Assert.Throws<ArgumentException>(() => SchedulerEngine.Validate(item));
        item.RecurrenceRule = null;
        Assert.Throws<ArgumentException>(() => SchedulerEngine.Expand([item, item.Clone()], Utc(2026, 9, 1), Utc(2026, 10, 1)));
    }

    private static SchedulerEvent Appointment(DateTime local, string zone = "UTC")
    {
        var start = SchedulerEngine.ToInstant(local, zone);
        return new SchedulerEvent { Id = "series", Title = "Meeting", Start = start, End = start.AddHours(1), TimeZoneId = zone };
    }
    private static DateTimeOffset Utc(int year, int month, int day) => new(year, month, day, 0, 0, 0, TimeSpan.Zero);
}

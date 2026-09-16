using System.Globalization;
using BlazorBlueprint.Components;
using BlazorBlueprint.Tests.Performance;
using BlazorBlueprint.Tests.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Xunit;

namespace BlazorBlueprint.Tests.Scheduler;

public class SchedulerLifecycleTests
{
    [Theory]
    [InlineData(15, "move", 9, 15, 10, 15)]
    [InlineData(30, "start", 8, 30, 10, 0)]
    [InlineData(60, "end", 9, 0, 11, 0)]
    public async Task GesturesSaveSnappedTimesWithoutMutatingSource(int slots, string action, int startHour, int startMinute, int endHour, int endMinute)
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            await scheduler.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(BbScheduler.SlotMinutes)] = slots }));
            await Gesture(scheduler, original, At(startHour, startMinute), At(endHour, endMinute), action);
            Assert.Equal(At(startHour, startMinute), scheduler.Events[0].Start);
            Assert.Equal(At(endHour, endMinute), scheduler.Events[0].End);
            Assert.Equal(At(9), original.Start);
            Assert.Equal(At(10), original.End);
            Assert.False(ComponentProbe.Field<bool>(scheduler, "editorOpen"));
        });
    }

    [Fact]
    public async Task OvernightMovePreservesDurationBeyondVisibleHours()
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            original.End = At(9).AddHours(20);
            await scheduler.SetParametersAsync(ParameterView.Empty);
            await Gesture(scheduler, original, At(10), At(10).AddHours(20), "move");
            Assert.Equal(At(10), scheduler.Events[0].Start);
            Assert.Equal(TimeSpan.FromHours(20), scheduler.Events[0].End - scheduler.Events[0].Start);
        });
    }

    [Fact]
    public async Task MoveAcrossResourcesPreservesOtherAssignmentsAndRecurringSeries()
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            original.ResourceIds = ["a", "b"];
            original.RecurrenceRule = "FREQ=DAILY;COUNT=3";
            await scheduler.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(BbScheduler.Resources)] = new SchedulerResource[] { new("a", "A"), new("b", "B"), new("c", "C") }
            }));
            await Gesture(scheduler, original, At(10), At(11), "move", target: 2);
            var series = scheduler.Events.Single(e => e.Id == original.Id);
            var moved = scheduler.Events.Single(e => e.SeriesId == original.Id);
            Assert.Contains(At(9), series.ExcludedStarts);
            Assert.Contains("b", moved.ResourceIds);
            Assert.Contains("c", moved.ResourceIds);
            Assert.DoesNotContain("a", moved.ResourceIds);
            Assert.Equal(At(10), moved.Start);
            Assert.Empty(original.ExcludedStarts);
            Assert.Contains("a", original.ResourceIds);
        });
    }

    [Fact]
    public async Task RejectedGestureRetainsOriginalAndOpensDraftForRetry()
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            var reject = true;
            await scheduler.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(BbScheduler.OnEventChange)] = EventCallback.Factory.Create<SchedulerChangeContext>(this, context => context.Cancel = reject)
            }));
            await Gesture(scheduler, original, At(10), At(11), "move");
            Assert.Same(original, scheduler.Events[0]);
            Assert.True(ComponentProbe.Field<bool>(scheduler, "editorOpen"));
            Assert.Equal(At(10), ComponentProbe.Field<SchedulerEvent>(scheduler, "draft").Start);
            Assert.NotNull(ComponentProbe.Field<string>(scheduler, "editorError"));
            reject = false;
            await (Task)ComponentProbe.Call(scheduler, "SaveAsync", false)!;
            Assert.Equal(At(10), scheduler.Events[0].Start);
        });
    }

    [Fact]
    public async Task ReadOnlyDisabledStaleAndInvalidGesturesDoNotSave()
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            await Gesture(scheduler, original, At(9, 10), At(10, 10), "move"); // off the slot grid
            await Gesture(scheduler, original, At(9), At(9), "end");
            await Gesture(scheduler, original, At(7), At(8), "move");
            await scheduler.CommitInteractionAsync(-1, original.Id, original.Start.ToUnixTimeMilliseconds(), 0, 0, At(10).ToUnixTimeMilliseconds(), At(11).ToUnixTimeMilliseconds(), "move");
            await scheduler.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(BbScheduler.AllowDrag)] = false, [nameof(BbScheduler.AllowResize)] = false }));
            await Gesture(scheduler, original, At(10), At(11), "move");
            await Gesture(scheduler, original, At(9), At(11), "end");
            await scheduler.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(BbScheduler.ReadOnly)] = true, [nameof(BbScheduler.AllowDrag)] = true }));
            await Gesture(scheduler, original, At(10), At(11), "move");
            Assert.Same(original, scheduler.Events[0]);
        });
    }

    [Fact]
    public async Task DeleteRequiresConfirmationCancelRetainsEventAndRejectionCanRetry()
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            var reject = true;
            await scheduler.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(BbScheduler.OnEventChange)] = EventCallback.Factory.Create<SchedulerChangeContext>(this, context => context.Cancel = reject)
            }));
            scheduler.EditEvent(new(original, original.Start, original.End));
            await (Task)ComponentProbe.Call(scheduler, "ConfirmDeleteAsync")!;
            Assert.Single(scheduler.Events);
            ComponentProbe.Call(scheduler, "RequestDelete");
            Assert.True(ComponentProbe.Field<bool>(scheduler, "deleteConfirmationOpen"));
            ComponentProbe.Call(scheduler, "SetDeleteConfirmationOpen", false);
            Assert.Single(scheduler.Events);
            Assert.True(ComponentProbe.Field<bool>(scheduler, "editorOpen"));
            ComponentProbe.Call(scheduler, "RequestDelete");
            await (Task)ComponentProbe.Call(scheduler, "ConfirmDeleteAsync")!;
            Assert.Single(scheduler.Events);
            Assert.True(ComponentProbe.Field<bool>(scheduler, "deleteConfirmationOpen"));
            reject = false;
            await (Task)ComponentProbe.Call(scheduler, "ConfirmDeleteAsync")!;
            Assert.Empty(scheduler.Events);
            Assert.False(ComponentProbe.Field<bool>(scheduler, "deleteConfirmationOpen"));
        });
    }

    [Fact]
    public async Task SingleZoneEditorUsesDisplayZoneAndPreservesStoredInstantsAndRecurrenceZone()
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            original.TimeZoneId = "America/New_York";
            await scheduler.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(BbScheduler.EnableTimeZones)] = false,
                [nameof(BbScheduler.TimeZoneId)] = "Asia/Singapore"
            }));
            scheduler.EditEvent(new(original, original.Start, original.End));
            Assert.Equal(At(17).DateTime, ComponentProbe.Field<DateTime?>(scheduler, "localStart"));
            await (Task)ComponentProbe.Call(scheduler, "SaveAsync", false)!;
            Assert.Equal(original.Start, scheduler.Events[0].Start);
            Assert.Equal("America/New_York", scheduler.Events[0].TimeZoneId);
        });
    }

    [Theory]
    [InlineData(SchedulerView.Week, DayOfWeek.Monday, "2026-09-16", "2026-09-14", 7)]
    [InlineData(SchedulerView.Week, DayOfWeek.Sunday, "2026-09-16", "2026-09-13", 7)]
    [InlineData(SchedulerView.WorkWeek, DayOfWeek.Sunday, "2026-09-16", "2026-09-14", 5)]
    [InlineData(SchedulerView.WorkWeek, DayOfWeek.Monday, "2026-09-20", "2026-09-14", 5)]
    [InlineData(SchedulerView.Week, DayOfWeek.Sunday, "2027-01-01", "2026-12-27", 7)]
    [InlineData(SchedulerView.WorkWeek, DayOfWeek.Monday, "2027-01-01", "2026-12-28", 5)]
    public async Task WeekViewsRenderTheCorrectDates(SchedulerView view, DayOfWeek firstDay, string date, string firstDate, int days)
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            await scheduler.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(BbScheduler.View)] = view,
                [nameof(BbScheduler.FirstDayOfWeek)] = firstDay,
                [nameof(BbScheduler.Date)] = DateOnly.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture)
            }));
            var expected = DateOnly.ParseExact(firstDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            Assert.Equal(Enumerable.Range(0, days).Select(expected.AddDays), LaneDates(scheduler));
        });
    }

    [Theory]
    [InlineData(SchedulerView.Day, 1)]
    [InlineData(SchedulerView.Week, 7)]
    [InlineData(SchedulerView.WorkWeek, 7)]
    public async Task ViewNavigationUsesWholePeriodsAndReportsDateChanges(SchedulerView view, int step)
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            var notifications = new List<DateOnly>();
            var date = new DateOnly(2026, 12, 30);
            await scheduler.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(BbScheduler.Date)] = date,
                [nameof(BbScheduler.View)] = view,
                [nameof(BbScheduler.DateChanged)] = EventCallback.Factory.Create<DateOnly>(this, notifications.Add)
            }));
            var initialLanes = LaneDates(scheduler);
            await (Task)ComponentProbe.Call(scheduler, "NavigateAsync", 1)!;
            Assert.Equal(date.AddDays(step), scheduler.Date);
            Assert.Equal(initialLanes.Select(d => d.AddDays(step)), LaneDates(scheduler));
            await (Task)ComponentProbe.Call(scheduler, "NavigateAsync", -1)!;
            Assert.Equal(initialLanes, LaneDates(scheduler));
            Assert.Equal(date, scheduler.Date);
            Assert.Equal(new[] { date.AddDays(step), date }, notifications);
        });
    }

    [Fact]
    public async Task WeekStartBindingAndViewChangesRetainTheWeekPreference()
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            var starts = new List<DayOfWeek>();
            var views = new List<SchedulerView>();
            await scheduler.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(BbScheduler.View)] = SchedulerView.Week,
                [nameof(BbScheduler.FirstDayOfWeekChanged)] = EventCallback.Factory.Create<DayOfWeek>(this, starts.Add),
                [nameof(BbScheduler.ViewChanged)] = EventCallback.Factory.Create<SchedulerView>(this, views.Add)
            }));
            await (Task)ComponentProbe.Call(scheduler, "SetFirstDayOfWeekAsync", DayOfWeek.Sunday)!;
            Assert.Equal(DayOfWeek.Sunday, LaneDates(scheduler)[0].DayOfWeek);
            await (Task)ComponentProbe.Call(scheduler, "SetViewAsync", SchedulerView.WorkWeek)!;
            Assert.Equal(DayOfWeek.Monday, LaneDates(scheduler)[0].DayOfWeek);
            Assert.Equal(DayOfWeek.Friday, LaneDates(scheduler)[^1].DayOfWeek);
            Assert.Equal(DayOfWeek.Sunday, scheduler.FirstDayOfWeek);
            await (Task)ComponentProbe.Call(scheduler, "SetViewAsync", SchedulerView.Week)!;
            Assert.Equal(DayOfWeek.Sunday, LaneDates(scheduler)[0].DayOfWeek);
            Assert.Equal(DayOfWeek.Sunday, Assert.Single(starts));
            Assert.Equal(SchedulerView.WorkWeek, views[0]);
            Assert.Equal(SchedulerView.Week, views[1]);
        });
    }

    [Fact]
    public async Task WorkWeekOmitsWeekendRecurrencesAndKeepsResourceLanes()
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            original.Start = At(9).AddDays(-1); // Sunday
            original.End = At(10).AddDays(-1);
            original.RecurrenceRule = "FREQ=DAILY;COUNT=7";
            original.ResourceIds = ["a", "b"];
            await scheduler.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(BbScheduler.View)] = SchedulerView.WorkWeek,
                [nameof(BbScheduler.FirstDayOfWeek)] = DayOfWeek.Sunday,
                [nameof(BbScheduler.Resources)] = new SchedulerResource[] { new("a", "A"), new("b", "B") }
            }));
            var dates = LaneDates(scheduler);
            Assert.Equal(10, dates.Length);
            Assert.All(dates.GroupBy(d => d), group => Assert.Equal(2, group.Count()));
            var occurrences = ComponentProbe.Field<IReadOnlyList<SchedulerOccurrence>>(scheduler, "occurrences");
            Assert.Equal(5, occurrences.Count);
            Assert.All(occurrences, occurrence => Assert.InRange(occurrence.Start.DayOfWeek, DayOfWeek.Monday, DayOfWeek.Friday));
        });
    }

    [Theory]
    [InlineData(SchedulerView.Day, "Pacific/Kiritimati", 14)]
    [InlineData(SchedulerView.Week, "Pacific/Kiritimati", 14)]
    [InlineData(SchedulerView.WorkWeek, "Pacific/Kiritimati", 14)]
    [InlineData(SchedulerView.Day, "Pacific/Pago_Pago", -11)]
    [InlineData(SchedulerView.Week, "Pacific/Pago_Pago", -11)]
    [InlineData(SchedulerView.WorkWeek, "Pacific/Pago_Pago", -11)]
    public async Task TodayUsesTheScheduleZoneAndRetainsViewAndWeekStart(SchedulerView view, string zone, int offsetHours)
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            var notifications = new List<DateOnly>();
            await scheduler.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(BbScheduler.Date)] = new DateOnly(2000, 1, 1),
                [nameof(BbScheduler.View)] = view,
                [nameof(BbScheduler.FirstDayOfWeek)] = DayOfWeek.Sunday,
                [nameof(BbScheduler.TimeZoneId)] = zone,
                [nameof(BbScheduler.EnableTimeZones)] = false,
                [nameof(BbScheduler.ReadOnly)] = true,
                [nameof(BbScheduler.DateChanged)] = EventCallback.Factory.Create<DateOnly>(this, notifications.Add)
            }));
            var before = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(offsetHours)).DateTime);
            await (Task)ComponentProbe.Call(scheduler, "NavigateToTodayAsync")!;
            var after = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(offsetHours)).DateTime);
            Assert.InRange(scheduler.Date, before, after);
            Assert.Equal(new[] { scheduler.Date }, notifications);
            Assert.Equal(view, scheduler.View);
            Assert.Equal(DayOfWeek.Sunday, scheduler.FirstDayOfWeek);
            var weekStart = view == SchedulerView.WorkWeek ? DayOfWeek.Monday : DayOfWeek.Sunday;
            var first = view == SchedulerView.Day ? scheduler.Date
                : scheduler.Date.AddDays(-(((int)scheduler.Date.DayOfWeek - (int)weekStart + 7) % 7));
            var days = view == SchedulerView.Day ? 1 : view == SchedulerView.WorkWeek ? 5 : 7;
            Assert.Equal(Enumerable.Range(0, days).Select(first.AddDays), LaneDates(scheduler));
            Assert.Same(original, Assert.Single(scheduler.Events));
        });
    }

    [Theory]
    [InlineData("DAILY")]
    [InlineData("MONTHLY")]
    [InlineData("YEARLY")]
    public async Task SimpleRepeatPresetsSaveAndReopenWithoutAnInterval(string frequency)
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            scheduler.EditEvent(new(original, original.Start, original.End), SchedulerEditScope.Series);
            ComponentProbe.Call(scheduler, "FrequencyChanged", frequency);
            await (Task)ComponentProbe.Call(scheduler, "SaveAsync", false)!;
            var saved = Assert.Single(scheduler.Events);
            Assert.Equal($"FREQ={frequency}", saved.RecurrenceRule);
            scheduler.EditEvent(new(saved, saved.Start, saved.End), SchedulerEditScope.Series);
            Assert.Equal(frequency, ComponentProbe.Field<string>(scheduler, "recurrenceFrequency"));
            Assert.Null(original.RecurrenceRule);
        });
    }

    [Fact]
    public async Task WeeklyCheckboxesSaveSelectedDaysAndRestoreThemWhenReopened()
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            scheduler.EditEvent(new(original, original.Start, original.End), SchedulerEditScope.Series);
            ComponentProbe.Call(scheduler, "FrequencyChanged", "WEEKLY");
            Assert.Equal([DayOfWeek.Monday], ComponentProbe.Field<HashSet<DayOfWeek>>(scheduler, "recurrenceDays"));
            ComponentProbe.Call(scheduler, "ToggleRecurrenceDay", DayOfWeek.Wednesday, true);
            ComponentProbe.Call(scheduler, "ToggleRecurrenceDay", DayOfWeek.Friday, true);
            await (Task)ComponentProbe.Call(scheduler, "SaveAsync", false)!;
            var saved = Assert.Single(scheduler.Events);
            Assert.Equal("FREQ=WEEKLY;BYDAY=MO,WE,FR", saved.RecurrenceRule);
            Assert.Equal([14, 16, 18, 21, 23, 25], SchedulerEngine.Expand([saved], At(0), At(0).AddDays(14)).Select(item => item.Start.Day));
            scheduler.EditEvent(new(saved, saved.Start, saved.End), SchedulerEditScope.Series);
            Assert.Equal("WEEKLY", ComponentProbe.Field<string>(scheduler, "recurrenceFrequency"));
            Assert.Equal([DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday], ComponentProbe.Field<HashSet<DayOfWeek>>(scheduler, "recurrenceDays").Order());
        });
    }

    [Theory]
    [InlineData(SchedulerEditScope.Series)]
    [InlineData(SchedulerEditScope.Occurrence)]
    public async Task WeeklyRepeatRequiresADayAndAllowsRetryWithoutDiscardingTheDraft(SchedulerEditScope scope)
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            scheduler.EditEvent(new(original, original.Start, original.End), scope);
            ComponentProbe.Call(scheduler, "FrequencyChanged", "WEEKLY");
            ComponentProbe.Call(scheduler, "ToggleRecurrenceDay", DayOfWeek.Monday, false);
            await (Task)ComponentProbe.Call(scheduler, "SaveAsync", false)!;
            Assert.Same(original, Assert.Single(scheduler.Events));
            Assert.True(ComponentProbe.Field<bool>(scheduler, "editorOpen"));
            Assert.Contains("at least one day", ComponentProbe.Field<string>(scheduler, "editorError"));
            ComponentProbe.Call(scheduler, "ToggleRecurrenceDay", DayOfWeek.Tuesday, true);
            await (Task)ComponentProbe.Call(scheduler, "SaveAsync", false)!;
            Assert.Equal("FREQ=WEEKLY;BYDAY=TU", Assert.Single(scheduler.Events).RecurrenceRule);
        });
    }

    [Fact]
    public async Task WeeklyPresetReadsExistingDaysAndCountBeforeChangingOneDay()
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            original.RecurrenceRule = "FREQ=WEEKLY;INTERVAL=1;BYDAY=MO,WE;COUNT=7";
            scheduler.EditEvent(new(original, original.Start, original.End), SchedulerEditScope.Series);
            Assert.True(ComponentProbe.Field<bool>(scheduler, "limitRecurrence"));
            Assert.Equal(7, ComponentProbe.Field<int?>(scheduler, "recurrenceCount"));
            ComponentProbe.Call(scheduler, "ToggleRecurrenceDay", DayOfWeek.Wednesday, false);
            ComponentProbe.Call(scheduler, "ToggleRecurrenceDay", DayOfWeek.Friday, true);
            await (Task)ComponentProbe.Call(scheduler, "SaveAsync", false)!;
            Assert.Equal("FREQ=WEEKLY;BYDAY=MO,FR;COUNT=7", Assert.Single(scheduler.Events).RecurrenceRule);
        });
    }

    [Theory]
    [InlineData("FREQ=WEEKLY;INTERVAL=2;BYDAY=MO,WE")]
    [InlineData("FREQ=DAILY;UNTIL=20261001T090000Z")]
    [InlineData("FREQ=MONTHLY;BYDAY=2MO")]
    public async Task AdvancedApplicationRulesArePreservedUnlessExplicitlyReplaced(string rule)
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            original.RecurrenceRule = rule;
            scheduler.EditEvent(new(original, original.Start, original.End), SchedulerEditScope.Series);
            Assert.Equal("EXISTING", ComponentProbe.Field<string>(scheduler, "recurrenceFrequency"));
            ComponentProbe.Field<SchedulerEvent>(scheduler, "draft").Title = "Updated title";
            await (Task)ComponentProbe.Call(scheduler, "SaveAsync", false)!;
            var saved = Assert.Single(scheduler.Events);
            Assert.Equal(rule, saved.RecurrenceRule);
            Assert.Equal("Updated title", saved.Title);
            scheduler.EditEvent(new(saved, saved.Start, saved.End), SchedulerEditScope.Series);
            ComponentProbe.Call(scheduler, "FrequencyChanged", "DAILY");
            await (Task)ComponentProbe.Call(scheduler, "SaveAsync", false)!;
            Assert.Equal("FREQ=DAILY", Assert.Single(scheduler.Events).RecurrenceRule);
        });
    }

    [Theory]
    [InlineData("2026-09-14", "UTC", 0, 24, 8, 640)]
    [InlineData("2026-09-14", "UTC", 8, 18, 0, 0)]
    [InlineData("2026-09-14", "UTC", 8, 18, 23, 800)]
    [InlineData("2026-03-08", "America/New_York", 0, 24, 8, 560)]
    [InlineData("2026-11-01", "America/New_York", 0, 24, 8, 720)]
    public async Task InitialScrollUsesDisplayZoneElapsedTimeAndClampsToTheRenderedRange(
        string date, string zone, int startHour, int endHour, int initialHour, double expectedTop)
    {
        await RunAsync(async (renderer, scheduler, original) =>
        {
            Assert.Null(ComponentProbe.Call(scheduler, "GetInitialScrollTop"));
            await scheduler.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(BbScheduler.Date)] = DateOnly.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                [nameof(BbScheduler.TimeZoneId)] = zone,
                [nameof(BbScheduler.StartHour)] = startHour,
                [nameof(BbScheduler.EndHour)] = endHour,
                [nameof(BbScheduler.InitialScrollHour)] = initialHour
            }));
            Assert.Equal(expectedTop, (double)ComponentProbe.Call(scheduler, "GetInitialScrollTop")!);
            Assert.Null(ComponentProbe.Field<string?>(scheduler, "loadError"));
        });
    }

    private static DateOnly[] LaneDates(BbScheduler scheduler) => ComponentProbe.Field<IEnumerable<object>>(scheduler, "lanes")
        .Select(lane => (DateOnly)lane.GetType().GetProperty("Date")!.GetValue(lane)!).ToArray();

    private static Task Gesture(BbScheduler scheduler, SchedulerEvent original, DateTimeOffset start, DateTimeOffset end, string action, int target = 0) =>
        scheduler.CommitInteractionAsync(ComponentProbe.Field<int>(scheduler, "revision"), original.Id, original.Start.ToUnixTimeMilliseconds(), 0, target,
            start.ToUnixTimeMilliseconds(), end.ToUnixTimeMilliseconds(), action);
    private static DateTimeOffset At(int hour, int minute = 0) => new(2026, 9, 14, hour, minute, 0, TimeSpan.Zero);

    private static async Task RunAsync(Func<ComponentTestRenderer, BbScheduler, SchedulerEvent, Task> test)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<IJSRuntime, NoopJavaScript>();
        services.AddBlazorBlueprintComponents();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await renderer.MountAsync<BlazorBlueprint.Primitives.Services.BbPortalHost>(new());
            var original = new SchedulerEvent { Id = "event", Title = "Workshop", Start = At(9), End = At(10) };
            var scheduler = await renderer.MountAsync<BbScheduler>(new()
            {
                [nameof(BbScheduler.Date)] = new DateOnly(2026, 9, 14),
                [nameof(BbScheduler.View)] = SchedulerView.Day,
                [nameof(BbScheduler.Events)] = new SchedulerEvent[] { original }
            });
            await test(renderer, scheduler, original);
        });
    }
}

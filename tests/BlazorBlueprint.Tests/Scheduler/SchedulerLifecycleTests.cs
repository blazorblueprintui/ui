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

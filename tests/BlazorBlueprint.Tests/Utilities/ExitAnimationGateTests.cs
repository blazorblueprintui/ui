using BlazorBlueprint.Primitives.Utilities;
using Xunit;

namespace BlazorBlueprint.Tests.Utilities;

/// <summary>
/// The shared exit lifecycle: a closed overlay stays present until its registered animation ends,
/// and a reopen or a newer close supersedes an exit that is still in flight.
/// </summary>
public class ExitAnimationGateTests
{
    [Fact]
    public void ClosedGateIsNotPresent()
    {
        var gate = new ExitAnimationGate();

        Assert.False(gate.IsAnimatingOut);
        Assert.False(gate.IsPresent(isOpen: false));
        Assert.True(gate.IsPresent(isOpen: true));
    }

    [Fact]
    public void BeginWithoutARegisteredAnimationLeavesTheOverlayUnmounted()
    {
        var gate = new ExitAnimationGate();
        var completed = 0;

        Assert.False(gate.Begin(() => completed++));

        Assert.False(gate.IsAnimatingOut);
        Assert.False(gate.IsPresent(isOpen: false));
        Assert.Equal(0, completed);
    }

    [Fact]
    public async Task BeginKeepsAClosedOverlayPresentUntilItsAnimationEnds()
    {
        var gate = new ExitAnimationGate();
        var animation = new TaskCompletionSource();
        var completed = 0;
        gate.ExitAnimation = _ => animation.Task;

        Assert.True(gate.Begin(() => completed++));

        Assert.True(gate.IsAnimatingOut);
        Assert.True(gate.IsPresent(isOpen: false));
        Assert.Equal(0, completed);

        animation.SetResult();

        await WaitUntilAsync(() => completed == 1);
        Assert.False(gate.IsAnimatingOut);
        Assert.False(gate.IsPresent(isOpen: false));
    }

    [Fact]
    public async Task BeginWhileAnExitIsInFlightDoesNotRestartTheWait()
    {
        var gate = new ExitAnimationGate();
        var animation = new TaskCompletionSource();
        var completed = 0;
        gate.ExitAnimation = _ => animation.Task;

        Assert.True(gate.Begin(() => completed++));
        Assert.False(gate.Begin(() => completed++));

        animation.SetResult();

        await WaitUntilAsync(() => completed == 1);
    }

    [Fact]
    public async Task CancelAbandonsAnExitInFlight()
    {
        var gate = new ExitAnimationGate();
        var animation = new TaskCompletionSource();
        var completed = 0;
        var sawCurrent = true;
        gate.ExitAnimation = isCurrent => ObserveAsync(isCurrent, animation.Task, current => sawCurrent = current);

        Assert.True(gate.Begin(() => completed++));

        gate.Cancel();

        Assert.False(gate.IsAnimatingOut);
        Assert.False(gate.IsPresent(isOpen: false));

        animation.SetResult();

        await WaitUntilAsync(() => !sawCurrent);
        Assert.Equal(0, completed);
    }

    [Fact]
    public void AnExitThatEndsAtOnceClearsPresenceBeforeTheOwnerCloses()
    {
        var gate = new ExitAnimationGate();
        var completed = 0;
        gate.ExitAnimation = _ => Task.CompletedTask;

        // Simulates a context that starts the exit while its open state is still set, exactly as
        // DialogContext.Close does; presence must clear despite isOpen still being true.
        Assert.True(gate.Begin(() => completed++));

        Assert.Equal(1, completed);
        Assert.False(gate.IsAnimatingOut);
        Assert.False(gate.IsPresent(isOpen: false));
    }

    [Fact]
    public async Task GateCanBeReusedForASecondClose()
    {
        var gate = new ExitAnimationGate();
        var animation = new TaskCompletionSource();
        var completed = 0;
        gate.ExitAnimation = _ => animation.Task;

        Assert.True(gate.Begin(() => completed++));
        animation.SetResult();
        await WaitUntilAsync(() => completed == 1);

        var second = new TaskCompletionSource();
        gate.ExitAnimation = _ => second.Task;
        Assert.True(gate.Begin(() => completed++));
        Assert.True(gate.IsAnimatingOut);

        second.SetResult();
        await WaitUntilAsync(() => completed == 2);
    }

    private static async Task ObserveAsync(Func<bool> isCurrent, Task animation, Action<bool> observe)
    {
        observe(isCurrent());
        await animation;
        observe(isCurrent());
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }

        Assert.True(condition());
    }
}

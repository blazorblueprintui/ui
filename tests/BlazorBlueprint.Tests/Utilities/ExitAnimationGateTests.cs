using BlazorBlueprint.Primitives.Utilities;
using Xunit;

namespace BlazorBlueprint.Tests.Utilities;

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
    public void BeginKeepsClosedOverlayPresent()
    {
        var gate = new ExitAnimationGate();

        gate.Begin();

        Assert.True(gate.IsAnimatingOut);
        Assert.True(gate.IsPresent(isOpen: false));
    }

    [Fact]
    public void CompleteCloseReportsWhetherToUnmount()
    {
        var gate = new ExitAnimationGate();
        gate.Begin();

        Assert.True(gate.CompleteClose(isOpen: false));
        Assert.False(gate.IsAnimatingOut);
        Assert.False(gate.IsPresent(isOpen: false));
    }

    [Fact]
    public void CompleteCloseDoesNothingWithoutAnExitInFlight()
    {
        var gate = new ExitAnimationGate();

        Assert.False(gate.CompleteClose(isOpen: false));
    }

    [Fact]
    public void ReopenDuringExitKeepsOverlayMountedAndLeavesGateSet()
    {
        var gate = new ExitAnimationGate();
        gate.Begin();

        Assert.False(gate.CompleteClose(isOpen: true));

        // Still animating out: the reopen wins, so the content stays mounted and the gate is left
        // for whoever closes it again.
        Assert.True(gate.IsAnimatingOut);
    }

    [Fact]
    public void CancelAbandonsAnExitInFlight()
    {
        var gate = new ExitAnimationGate();
        gate.Begin();

        gate.Cancel();

        Assert.False(gate.IsAnimatingOut);
        Assert.False(gate.CompleteClose(isOpen: false));
    }

    [Fact]
    public void BeginIsIdempotent()
    {
        var gate = new ExitAnimationGate();

        gate.Begin();
        gate.Begin();

        Assert.True(gate.IsAnimatingOut);

        // A repeated close must not restart the wait, so the single CompleteClose still ends it.
        Assert.True(gate.CompleteClose(isOpen: false));
    }

    [Fact]
    public void GateCanBeReusedForASecondClose()
    {
        var gate = new ExitAnimationGate();

        gate.Begin();
        Assert.True(gate.CompleteClose(isOpen: false));

        gate.Begin();
        Assert.True(gate.IsAnimatingOut);
        Assert.True(gate.CompleteClose(isOpen: false));
    }
}
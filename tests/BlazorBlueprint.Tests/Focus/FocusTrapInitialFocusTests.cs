using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Xunit;

namespace BlazorBlueprint.Tests.Focus;

/// <summary>
/// Guards the contract <see cref="FocusManager"/> hands to <c>focus-trap.js</c>.
///
/// The JS decides the initial focus target, so what is testable here is that the mode and the
/// explicit element arrive intact. That is where a silent regression would live: the trap would
/// still work, still trap Tab, and quietly go back to focusing the first tabbable child — which
/// is exactly the reported bug (#504), and it produces no error.
/// </summary>
public class FocusTrapInitialFocusTests
{
    [Fact]
    public async Task TrapFocusDefaultsToTheFirstTabbableDescendant()
    {
        var module = new RecordingModule();
        var manager = new FocusManager(new StubJsRuntime(module));

        await manager.TrapFocus(default);

        Assert.Equal("createFocusTrap", module.LastIdentifier);
        Assert.Equal("first", module.LastArgs![1]);
        Assert.Null(module.LastArgs[2]);
    }

    [Theory]
    [InlineData(FocusTrapInitialFocus.FirstFocusable, "first")]
    [InlineData(FocusTrapInitialFocus.Container, "container")]
    [InlineData(FocusTrapInitialFocus.None, "none")]
    public async Task TrapFocusSendsTheModeTheCallerAskedFor(
        FocusTrapInitialFocus initialFocus, string expected)
    {
        var module = new RecordingModule();
        var manager = new FocusManager(new StubJsRuntime(module));

        await manager.TrapFocus(default, initialFocus);

        Assert.Equal(expected, module.LastArgs![1]);
    }

    [Fact]
    public async Task TrapFocusForwardsAnExplicitElement()
    {
        var module = new RecordingModule();
        var manager = new FocusManager(new StubJsRuntime(module));
        var target = new ElementReference("the-target");

        await manager.TrapFocus(default, FocusTrapInitialFocus.Container, target);

        // The mode still travels: JS falls back to it when the explicit element turns out not to
        // be inside the container.
        Assert.Equal("container", module.LastArgs![1]);
        Assert.Equal(target, Assert.IsType<ElementReference>(module.LastArgs[2]));
    }

    /// <summary>Hands out the one module; anything else is a call the manager should not make.</summary>
    private sealed class StubJsRuntime(RecordingModule module) : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier != "import")
            {
                throw new InvalidOperationException(
                    $"FocusManager should only call 'import' on IJSRuntime, not '{identifier}'.");
            }

            return ValueTask.FromResult((TValue)(object)module);
        }
    }

    /// <summary>Records the last call so a test can assert on the arguments that reached JS.</summary>
    private sealed class RecordingModule : IJSObjectReference
    {
        public string? LastIdentifier { get; private set; }

        public object?[]? LastArgs { get; private set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            LastIdentifier = identifier;
            LastArgs = args;

            // The cleanup handle the real module returns.
            if (typeof(TValue) == typeof(IJSObjectReference))
            {
                return ValueTask.FromResult((TValue)(object)this);
            }

            return ValueTask.FromResult(default(TValue)!);
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

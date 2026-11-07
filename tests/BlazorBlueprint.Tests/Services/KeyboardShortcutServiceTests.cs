using BlazorBlueprint.Primitives.Services;
using Microsoft.JSInterop;
using Xunit;

namespace BlazorBlueprint.Tests.Services;

public class KeyboardShortcutServiceTests
{
    [Fact]
    public async Task DisposingAReplacedRegistrationKeepsTheNewOne()
    {
        var module = new RecordingModule();
        await using var service = new KeyboardShortcutService(new StubJsRuntime(module));
        var calls = new List<string>();

        // A page leaving and the next page arriving, both using Ctrl+S: the new page registers first.
        var oldPage = await service.RegisterAsync("Ctrl+S", () => { calls.Add("old"); return Task.CompletedTask; });
        var newPage = await service.RegisterAsync("Ctrl+S", () => { calls.Add("new"); return Task.CompletedTask; });
        oldPage.Dispose();

        await service.HandleShortcutAsync("ctrl+s");

        Assert.Equal(["new"], calls);
        Assert.DoesNotContain("keyboardShortcuts.unregisterShortcut", module.Calls);

        // The newer handle still removes its own registration.
        newPage.Dispose();
        await service.HandleShortcutAsync("ctrl+s");

        Assert.Equal(["new"], calls);
        Assert.Contains("keyboardShortcuts.unregisterShortcut", module.Calls);
    }

    [Fact]
    public async Task DisposingTheCurrentRegistrationRemovesIt()
    {
        var module = new RecordingModule();
        await using var service = new KeyboardShortcutService(new StubJsRuntime(module));
        var calls = 0;

        var handle = await service.RegisterAsync("Ctrl+S", () => { calls++; return Task.CompletedTask; });
        handle.Dispose();

        await service.HandleShortcutAsync("ctrl+s");

        Assert.Equal(0, calls);
        Assert.Contains("keyboardShortcuts.unregisterShortcut", module.Calls);
    }

    private sealed class StubJsRuntime(IJSObjectReference module) : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            ValueTask.FromResult((TValue)module);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            ValueTask.FromResult((TValue)module);
    }

    private sealed class RecordingModule : IJSObjectReference
    {
        public List<string> Calls { get; } = [];

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            Calls.Add(identifier);
            return ValueTask.FromResult(default(TValue)!);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            InvokeAsync<TValue>(identifier, args);

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

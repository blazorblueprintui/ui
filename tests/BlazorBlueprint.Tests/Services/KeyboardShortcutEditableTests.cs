using BlazorBlueprint.Primitives.Services;
using Microsoft.JSInterop;
using Xunit;

namespace BlazorBlueprint.Tests.Services;

public class KeyboardShortcutEditableTests
{
    [Theory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    [InlineData(false, "save")]
    [InlineData(true, "save")]
    public async Task RegistrationPassesEditableFlagToJavaScript(bool allowInEditable, string? id)
    {
        var runtime = new RecordingRuntime();
        await using var service = new KeyboardShortcutService(runtime);
        using var registration = await service.RegisterAsync("Ctrl+S", () => Task.CompletedTask, allowInEditable, id);

        var call = Assert.Single(runtime.Module.Calls, call => call.Identifier == "keyboardShortcuts.registerShortcut");
        Assert.Equal(new object?[] { "ctrl+s", allowInEditable }, call.Args);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExistingOverloadsKeepEditableShortcutsDisabled(bool withId)
    {
        var runtime = new RecordingRuntime();
        await using var service = new KeyboardShortcutService(runtime);
        using var registration = withId
            ? await service.RegisterAsync("Ctrl+S", () => Task.CompletedTask, "save")
            : await service.RegisterAsync("Ctrl+S", () => Task.CompletedTask);

        var call = Assert.Single(runtime.Module.Calls, call => call.Identifier == "keyboardShortcuts.registerShortcut");
        Assert.Equal(new object?[] { "ctrl+s", false }, call.Args);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("save")]
    public async Task DefaultInterfaceImplementationSupportsExistingServices(string? id)
    {
        var legacy = new LegacyService();
        IKeyboardShortcutService service = legacy;
        using var registration = await service.RegisterAsync("Ctrl+S", () => Task.CompletedTask, false, id);
        Assert.Equal(id, legacy.RegisteredId);
        Assert.Equal(1, legacy.Registrations);

        await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await service.RegisterAsync("Ctrl+S", () => Task.CompletedTask, true, id));
        Assert.Equal(1, legacy.Registrations);
    }

    private sealed class LegacyService : IKeyboardShortcutService
    {
        public string? RegisteredId { get; private set; }
        public int Registrations { get; private set; }
        public bool IsSuspended => false;
        public Task<IDisposable> RegisterAsync(string shortcut, Func<Task> callback) => RegisterAsync(shortcut, callback, null!);
        public Task<IDisposable> RegisterAsync(string shortcut, Func<Task> callback, string id)
        {
            RegisteredId = id;
            Registrations++;
            return Task.FromResult<IDisposable>(new Handle());
        }
        public void Unregister(string id) { }
        public void Suspend() { }
        public void Resume() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        private sealed class Handle : IDisposable
        {
            public void Dispose() { }
        }
    }

    private sealed class RecordingRuntime : IJSRuntime
    {
        public RecordingModule Module { get; } = new();
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            ValueTask.FromResult((TValue)(object)Module);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            InvokeAsync<TValue>(identifier, args);
    }

    private sealed class RecordingModule : IJSObjectReference
    {
        public List<(string Identifier, object?[]? Args)> Calls { get; } = [];
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            Calls.Add((identifier, args));
            return ValueTask.FromResult(default(TValue)!);
        }
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            InvokeAsync<TValue>(identifier, args);
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

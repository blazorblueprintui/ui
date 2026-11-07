using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Tests.Rendering;

public class MenubarShortcutTests
{
    [Fact]
    public async Task InitiallyDisabledItemCanBeEnabledAndDisabledAgain()
    {
        var javaScript = new ShortcutJavaScript();
        await RunAsync(async (renderer, shortcuts) =>
        {
            var clicks = 0;
            var item = await renderer.MountAsync<BbMenubarItem>(new()
            {
                [nameof(BbMenubarItem.Shortcut)] = "Ctrl+N",
                [nameof(BbMenubarItem.Disabled)] = true,
                [nameof(BbMenubarItem.OnClick)] = EventCallback.Factory.Create(this, () => clicks++)
            });

            Assert.Equal(1, javaScript.Registrations);
            await shortcuts.HandleShortcutAsync("ctrl+n");
            Assert.Equal(0, clicks);

            await SetParametersAsync(item, new() { [nameof(BbMenubarItem.Disabled)] = false });
            await shortcuts.HandleShortcutAsync("ctrl+n");
            Assert.Equal(1, clicks);

            await SetParametersAsync(item, new() { [nameof(BbMenubarItem.Disabled)] = true });
            await shortcuts.HandleShortcutAsync("ctrl+n");
            Assert.Equal(1, clicks);
            Assert.Equal(1, javaScript.Registrations);
        }, javaScript);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Ctrl+N")]
    public async Task ChangingShortcutRegistersTheNewKeyAndReleasesTheOldOne(string? initialShortcut)
    {
        await RunAsync(async (renderer, shortcuts) =>
        {
            var clicks = 0;
            var item = await renderer.MountAsync<BbMenubarItem>(new()
            {
                [nameof(BbMenubarItem.Shortcut)] = initialShortcut,
                [nameof(BbMenubarItem.OnClick)] = EventCallback.Factory.Create(this, () => clicks++)
            });

            await SetParametersAsync(item, new() { [nameof(BbMenubarItem.Shortcut)] = "Ctrl+M" });
            await shortcuts.HandleShortcutAsync("ctrl+n");
            Assert.Equal(0, clicks);
            await shortcuts.HandleShortcutAsync("ctrl+m");
            Assert.Equal(1, clicks);
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ClearingShortcutUnregistersIt(string? shortcut)
    {
        await RunAsync(async (renderer, shortcuts) =>
        {
            var clicks = 0;
            var item = await renderer.MountAsync<BbMenubarItem>(new()
            {
                [nameof(BbMenubarItem.Shortcut)] = "Ctrl+N",
                [nameof(BbMenubarItem.OnClick)] = EventCallback.Factory.Create(this, () => clicks++)
            });

            await SetParametersAsync(item, new() { [nameof(BbMenubarItem.Shortcut)] = shortcut });
            await shortcuts.HandleShortcutAsync("ctrl+n");
            Assert.Equal(0, clicks);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DisposingItemUnregistersItsShortcut(bool asyncDispose)
    {
        await RunAsync(async (renderer, shortcuts) =>
        {
            var clicks = 0;
            var item = await renderer.MountAsync<BbMenubarItem>(new()
            {
                [nameof(BbMenubarItem.Shortcut)] = "Ctrl+N",
                [nameof(BbMenubarItem.OnClick)] = EventCallback.Factory.Create(this, () => clicks++)
            });

            await shortcuts.HandleShortcutAsync("ctrl+n");
            Assert.Equal(1, clicks);

            if (asyncDispose)
            {
                await item.DisposeAsync();
            }
            else
            {
                item.Dispose();
            }

            await shortcuts.HandleShortcutAsync("ctrl+n");
            Assert.Equal(1, clicks);
        });
    }

    [Fact]
    public async Task ParameterChangesDuringRegistrationUseTheLatestShortcut()
    {
        var pending = new TaskCompletionSource();
        var javaScript = new ShortcutJavaScript { PendingRegistration = pending.Task };
        await RunAsync(async (renderer, shortcuts) =>
        {
            var clicks = 0;
            var item = await renderer.MountAsync<BbMenubarItem>(new()
            {
                [nameof(BbMenubarItem.Shortcut)] = "Ctrl+N",
                [nameof(BbMenubarItem.OnClick)] = EventCallback.Factory.Create(this, () => clicks++)
            });

            await SetParametersAsync(item, new() { [nameof(BbMenubarItem.Shortcut)] = "Ctrl+M" });
            Assert.Equal(1, javaScript.Registrations);
            pending.SetResult();

            await shortcuts.HandleShortcutAsync("ctrl+n");
            Assert.Equal(0, clicks);
            await shortcuts.HandleShortcutAsync("ctrl+m");
            Assert.Equal(1, clicks);
            Assert.Equal(2, javaScript.Registrations);
        }, javaScript);
    }

    [Fact]
    public async Task DisposalDuringRegistrationReleasesThePendingHandle()
    {
        var pending = new TaskCompletionSource();
        var javaScript = new ShortcutJavaScript { PendingRegistration = pending.Task };
        await RunAsync(async (renderer, shortcuts) =>
        {
            var clicks = 0;
            var item = await renderer.MountAsync<BbMenubarItem>(new()
            {
                [nameof(BbMenubarItem.Shortcut)] = "Ctrl+N",
                [nameof(BbMenubarItem.OnClick)] = EventCallback.Factory.Create(this, () => clicks++)
            });

            await item.DisposeAsync();
            await shortcuts.HandleShortcutAsync("ctrl+n");
            Assert.Equal(0, clicks);
            pending.SetResult();

            Assert.Equal(1, javaScript.Unregistrations);
            await shortcuts.HandleShortcutAsync("ctrl+n");
            Assert.Equal(0, clicks);
        }, javaScript);
    }

    private static Task SetParametersAsync(BbMenubarItem item, Dictionary<string, object?> parameters) =>
        item.SetParametersAsync(ParameterView.FromDictionary(parameters));

    private static async Task RunAsync(
        Func<ComponentTestRenderer, KeyboardShortcutService, Task> test, IJSRuntime? javaScript = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton<IJSRuntime>(javaScript ?? new NoopJavaScript());
        services.AddBlazorBlueprintComponents();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        var shortcuts = (KeyboardShortcutService)provider.GetRequiredService<IKeyboardShortcutService>();

        await renderer.Dispatcher.InvokeAsync(() => test(renderer, shortcuts));
    }

    private sealed class ShortcutJavaScript : IJSRuntime, IJSObjectReference
    {
        public int Registrations { get; private set; }
        public int Unregistrations { get; private set; }
        public Task? PendingRegistration { get; init; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public async ValueTask<TValue> InvokeAsync<TValue>(
            string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier == "keyboardShortcuts.registerShortcut")
            {
                Registrations++;
                if (PendingRegistration != null)
                {
                    await PendingRegistration;
                }
            }
            else if (identifier == "keyboardShortcuts.unregisterShortcut")
            {
                Unregistrations++;
            }

            return typeof(TValue) == typeof(IJSObjectReference) ? (TValue)(object)this : default!;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

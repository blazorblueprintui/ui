using System.Text.Json;
using BlazorBlueprint.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Tests.Theme;

/// <summary>
/// Guards <see cref="ThemeOptions.PersistToLocalStorage"/> against reading a theme it promised not
/// to write.
/// <para>
/// <c>InitializeAsync</c> used to call <c>loadTheme</c> unconditionally. Anyone who ran once with
/// persistence on and then turned it off kept getting the stored theme instead of their configured
/// defaults, with no way out short of clearing site data by hand (#481).
/// </para>
/// </summary>
public class ThemePersistenceTests
{
    /// <summary>
    /// Deliberately the opposite of the defaults asserted below on every axis, so a value leaking
    /// out of localStorage fails the test rather than coinciding with the expected answer.
    /// </summary>
    private const string StoredTheme =
        """{"isDarkMode":true,"baseColor":"Slate","primaryColor":"Blue","radius":1.0}""";

    [Fact]
    public async Task PersistenceDisabledDoesNotReadTheStoredTheme()
    {
        var module = new RecordingModule { StoredThemeJson = StoredTheme };
        var service = new ThemeService(
            new StubJsRuntime(module),
            new ThemeOptions { PersistToLocalStorage = false });

        await service.InitializeAsync();

        Assert.DoesNotContain("loadTheme", module.Calls);
    }

    [Fact]
    public async Task PersistenceDisabledClearsAnyThemeLeftByAnEarlierRun()
    {
        var module = new RecordingModule { StoredThemeJson = StoredTheme };
        var service = new ThemeService(
            new StubJsRuntime(module),
            new ThemeOptions { PersistToLocalStorage = false });

        await service.InitializeAsync();

        Assert.Contains("clearTheme", module.Calls);
    }

    [Fact]
    public async Task PersistenceDisabledKeepsTheConfiguredDefaults()
    {
        var module = new RecordingModule { StoredThemeJson = StoredTheme };
        var service = new ThemeService(
            new StubJsRuntime(module),
            new ThemeOptions
            {
                PersistToLocalStorage = false,
                DetectSystemPreference = false,
                DefaultDarkMode = false,
                DefaultBaseColor = BaseColor.Zinc,
                DefaultPrimaryColor = PrimaryColor.Default,
                DefaultRadius = 0.5,
            });

        await service.InitializeAsync();

        Assert.False(service.IsDarkMode);
        Assert.Equal(BaseColor.Zinc, service.BaseColor);
        Assert.Equal(PrimaryColor.Default, service.PrimaryColor);
        Assert.Equal(0.5, service.Radius);
    }

    [Fact]
    public async Task PersistenceEnabledStillRestoresTheStoredTheme()
    {
        var module = new RecordingModule { StoredThemeJson = StoredTheme };
        var service = new ThemeService(
            new StubJsRuntime(module),
            new ThemeOptions { PersistToLocalStorage = true });

        await service.InitializeAsync();

        Assert.Contains("loadTheme", module.Calls);
        Assert.DoesNotContain("clearTheme", module.Calls);
        Assert.True(service.IsDarkMode);
        Assert.Equal(BaseColor.Slate, service.BaseColor);
        Assert.Equal(PrimaryColor.Blue, service.PrimaryColor);
        Assert.Equal(1.0, service.Radius);
    }

    /// <summary>Hands out the one module. Anything else is a call the service should not be making.</summary>
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
                    $"ThemeService should only call 'import' on IJSRuntime, not '{identifier}'.");
            }

            return ValueTask.FromResult((TValue)(object)module);
        }
    }

    /// <summary>
    /// Records every call so a test can assert on what the service did and did not ask for.
    /// <c>loadTheme</c> answers by deserializing JSON, matching how the real interop materializes
    /// the payload — so the test exercises the same contract rather than a hand-built instance
    /// (which the service's private state type would not allow anyway).
    /// </summary>
    private sealed class RecordingModule : IJSObjectReference
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public List<string> Calls { get; } = [];

        /// <summary>What <c>loadTheme</c> returns; <c>null</c> means localStorage holds nothing.</summary>
        public string? StoredThemeJson { get; set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Calls.Add(identifier);

            if (identifier == "loadTheme" && StoredThemeJson is not null)
            {
                return ValueTask.FromResult(
                    JsonSerializer.Deserialize<TValue>(StoredThemeJson, JsonOptions)!);
            }

            if (identifier == "getPrefersDark")
            {
                return ValueTask.FromResult((TValue)(object)false);
            }

            return ValueTask.FromResult(default(TValue)!);
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

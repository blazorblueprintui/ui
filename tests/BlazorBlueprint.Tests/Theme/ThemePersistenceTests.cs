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
/// <para>
/// The decision itself now happens in <c>theme.initialize</c>, because reading localStorage,
/// clearing a stale entry and asking the OS for its dark-mode preference were four separate
/// awaited calls and so four circuit round trips on every page load. What C# still owns — and what
/// these tests cover — is the configuration it hands over, and its handling of the answer. That
/// localStorage really is left alone is browser behaviour, verified by driving the demo.
/// </para>
/// </summary>
public class ThemePersistenceTests
{
    /// <summary>
    /// Deliberately the opposite of the configured defaults on every axis, so a value leaking out
    /// of storage fails the test rather than coinciding with the expected answer.
    /// </summary>
    private const string RestoredTheme =
        """{"isDarkMode":true,"baseColor":"Slate","primaryColor":"Blue","radius":1.0}""";

    [Fact]
    public async Task PersistenceDisabledTellsTheBrowserNotToRead()
    {
        var module = new RecordingModule();
        var service = new ThemeService(
            new StubJsRuntime(module),
            new ThemeOptions { PersistToLocalStorage = false });

        await service.InitializeAsync();

        Assert.False(module.Config.GetProperty("persist").GetBoolean());
    }

    [Fact]
    public async Task PersistenceEnabledTellsTheBrowserToRead()
    {
        var module = new RecordingModule();
        var service = new ThemeService(
            new StubJsRuntime(module),
            new ThemeOptions { PersistToLocalStorage = true });

        await service.InitializeAsync();

        Assert.True(module.Config.GetProperty("persist").GetBoolean());
    }

    [Fact]
    public async Task ConfiguredDefaultsAreSentAsTheFallback()
    {
        var module = new RecordingModule();
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

        var defaults = module.Config.GetProperty("defaults");
        Assert.False(module.Config.GetProperty("detectSystemPreference").GetBoolean());
        Assert.False(defaults.GetProperty("isDarkMode").GetBoolean());
        Assert.Equal("zinc", defaults.GetProperty("baseColor").GetString());
        Assert.Equal("default", defaults.GetProperty("primaryColor").GetString());
        Assert.Equal(0.5, defaults.GetProperty("radius").GetDouble());
    }

    /// <summary>
    /// The browser validates a stored colour name before applying it, so the list of names it may
    /// accept has to travel with the request — otherwise a value this build no longer knows would
    /// be applied and then corrected a round trip later.
    /// </summary>
    [Fact]
    public async Task ValidColourNamesAreSentForTheBrowserToCheckAgainst()
    {
        var module = new RecordingModule();
        var service = new ThemeService(new StubJsRuntime(module), new ThemeOptions());

        await service.InitializeAsync();

        var baseColors = module.Config.GetProperty("validBaseColors")
            .EnumerateArray().Select(v => v.GetString()).ToList();

        Assert.Equal(Enum.GetNames<BaseColor>().Length, baseColors.Count);
        Assert.Contains("zinc", baseColors);
        Assert.All(baseColors, name => Assert.Equal(name, name!.ToLowerInvariant()));
    }

    [Fact]
    public async Task TheStateTheBrowserAppliedBecomesTheServiceState()
    {
        var module = new RecordingModule { AppliedThemeJson = RestoredTheme };
        var service = new ThemeService(
            new StubJsRuntime(module),
            new ThemeOptions { PersistToLocalStorage = true });

        await service.InitializeAsync();

        Assert.True(service.IsDarkMode);
        Assert.Equal(BaseColor.Slate, service.BaseColor);
        Assert.Equal(PrimaryColor.Blue, service.PrimaryColor);
        Assert.Equal(1.0, service.Radius);
    }

    /// <summary>
    /// Prerendering and a stubbed runtime both answer nothing. The configured defaults are already
    /// in place by then, so the service must keep them rather than fall over.
    /// </summary>
    [Fact]
    public async Task NoAnswerFromTheBrowserLeavesTheConfiguredDefaults()
    {
        var module = new RecordingModule { AppliedThemeJson = null };
        var service = new ThemeService(
            new StubJsRuntime(module),
            new ThemeOptions { DefaultBaseColor = BaseColor.Zinc, DefaultRadius = 0.5 });

        await service.InitializeAsync();

        Assert.Equal(BaseColor.Zinc, service.BaseColor);
        Assert.Equal(0.5, service.Radius);
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
    /// Captures the configuration handed to <c>theme.initialize</c> and answers with a state, the
    /// way the browser would. The config is round-tripped through JSON so the test sees the same
    /// shape the real interop serializes, rather than the anonymous type.
    /// </summary>
    private sealed class RecordingModule : IJSObjectReference
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public List<string> Calls { get; } = [];

        /// <summary>The configuration passed to <c>theme.initialize</c>.</summary>
        public JsonElement Config { get; private set; }

        /// <summary>What the browser reports it applied; <c>null</c> means it could not answer.</summary>
        public string? AppliedThemeJson { get; set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Calls.Add(identifier);

            if (identifier == "theme.initialize")
            {
                Config = JsonSerializer.SerializeToElement(args?[0], JsonOptions);

                if (AppliedThemeJson is not null)
                {
                    return ValueTask.FromResult(
                        JsonSerializer.Deserialize<TValue>(AppliedThemeJson, JsonOptions)!);
                }
            }

            return ValueTask.FromResult(default(TValue)!);
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

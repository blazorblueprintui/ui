using Microsoft.JSInterop;
using BlazorBlueprint.Primitives.Services;

namespace BlazorBlueprint.Components;

/// <summary>
/// Manages the application's visual theme — dark mode, base color, and primary color.
/// Persists preferences to <c>localStorage</c> and applies them to the DOM via a JS module.
/// </summary>
/// <remarks>
/// <para>
/// Register via <see cref="ServiceCollectionExtensions.AddBlazorBlueprintComponents(Microsoft.Extensions.DependencyInjection.IServiceCollection, Action{DefaultBbLocalizer}?, Action{ThemeOptions}?)"/>
/// with an optional <see cref="ThemeOptions"/> configuration action.
/// </para>
/// <para>
/// Call <see cref="InitializeAsync"/> once after the first interactive render (typically in
/// <c>OnAfterRenderAsync(firstRender)</c>) to load saved preferences and apply the theme.
/// </para>
/// </remarks>
public class ThemeService : IAsyncDisposable
{
    private readonly IJSRuntime jsRuntime;
    private readonly ThemeOptions options;
    private IJSObjectReference? module;
    private bool isDarkMode;
    private BaseColor baseColor;
    private PrimaryColor primaryColor;
    private double radius;
    private bool isInitialized;
    private ThemeDesign design;

    /// <summary>The full current configuration.</summary>
    public ThemePreset Preset => new() { DarkMode = isDarkMode, BaseColor = baseColor, PrimaryColor = primaryColor, Radius = radius, Design = design };

    /// <summary>
    /// Raised after any theme property changes. Subscribe to trigger <c>StateHasChanged</c> in consuming components.
    /// </summary>
    public event Action? OnThemeChanged;

    /// <summary>
    /// Gets whether dark mode is currently active.
    /// </summary>
    public bool IsDarkMode => isDarkMode;

    /// <summary>
    /// Gets the current base (gray scale) color.
    /// </summary>
    public BaseColor BaseColor => baseColor;

    /// <summary>
    /// Gets the current primary accent color.
    /// </summary>
    public PrimaryColor PrimaryColor => primaryColor;

    /// <summary>
    /// Gets the current border radius in rem.
    /// </summary>
    public double Radius => radius;

    /// <summary>
    /// Gets whether the service has been initialized via <see cref="InitializeAsync"/>.
    /// </summary>
    public bool IsInitialized => isInitialized;

    /// <summary>
    /// Creates a new <see cref="ThemeService"/> instance.
    /// </summary>
    /// <param name="jsRuntime">The Blazor JS interop runtime.</param>
    /// <param name="options">Theme configuration options.</param>
    public ThemeService(IJSRuntime jsRuntime, ThemeOptions options)
    {
        this.jsRuntime = jsRuntime;
        this.options = options;
        var preset = options.DefaultPreset ?? new ThemePreset
        {
            DarkMode = options.DefaultDarkMode, BaseColor = options.DefaultBaseColor,
            PrimaryColor = options.DefaultPrimaryColor, Radius = options.DefaultRadius
        };
        preset.Validate();
        isDarkMode = preset.DarkMode;
        baseColor = preset.BaseColor;
        primaryColor = preset.PrimaryColor;
        radius = preset.Radius;
        design = preset.Design;
    }

    /// <summary>
    /// Loads saved preferences from <c>localStorage</c> (if enabled), detects system color scheme
    /// preference, and applies the theme to the DOM. Safe to call multiple times — only the first
    /// call performs initialization.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (isInitialized)
        {
            return;
        }

        try
        {
            module = await ComponentModules.GetCoreAsync(jsRuntime);
        }
        catch (JSDisconnectedException)
        {
            isInitialized = true;
            return;
        }
        catch (InvalidOperationException)
        {
            // JS interop not available (SSR prerender) — keep defaults
            isInitialized = true;
            return;
        }

        try
        {
            // One call, not four. Reading localStorage, clearing a stale entry, asking the OS for
            // its dark-mode preference and applying the result are all browser-side decisions, and
            // on Blazor Server each separately awaited call was a circuit round trip on every page
            // load. The valid colour names go with it so that a corrupted entry falls back to the
            // configured default in the browser, exactly as ParseEnum would here, rather than being
            // applied and corrected a round trip later.
            var applied = await module.InvokeAsync<ThemeState?>("theme.initialize", new
            {
                persist = options.PersistToLocalStorage,
                detectSystemPreference = options.DetectSystemPreference,
                defaults = new
                {
                    isDarkMode,
                    baseColor = baseColor.ToString().ToLowerInvariant(),
                    primaryColor = primaryColor.ToString().ToLowerInvariant(),
                    radius,
                    design = design.ToJs()
                },
                validBaseColors = Enum.GetNames<BaseColor>().Select(n => n.ToLowerInvariant()).ToArray(),
                validPrimaryColors = Enum.GetNames<PrimaryColor>().Select(n => n.ToLowerInvariant()).ToArray()
            });

            // Null when interop is stubbed or the browser could not answer. The configured
            // defaults are already in the fields, so leaving them alone is the right outcome.
            if (applied is null)
            {
                isInitialized = true;
                return;
            }

            isDarkMode = applied.IsDarkMode;
            baseColor = ParseEnum(applied.BaseColor, baseColor);
            primaryColor = ParseEnum(applied.PrimaryColor, primaryColor);
            radius = applied.Radius is >= 0 and <= 4 ? applied.Radius.Value : radius;
            if (applied.Design is { } restored)
            {
                design = new ThemeDesign
                {
                    Density = ParseEnum(restored.Density, design.Density),
                    Font = ParseEnum(restored.Font, design.Font),
                    Surface = ParseEnum(restored.Surface, design.Surface),
                    MenuColor = ParseEnum(restored.MenuColor, design.MenuColor),
                    MenuAccent = ParseEnum(restored.MenuAccent, design.MenuAccent)
                };
            }
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected during init
        }

        isInitialized = true;
    }

    /// <summary>
    /// Toggles between dark and light mode.
    /// </summary>
    public async Task ToggleDarkModeAsync() =>
        await SetDarkModeAsync(!isDarkMode);

    /// <summary>
    /// Sets dark mode to the specified value.
    /// </summary>
    /// <param name="value"><c>true</c> for dark mode, <c>false</c> for light mode.</param>
    public async Task SetDarkModeAsync(bool value)
    {
        if (isDarkMode == value)
        {
            return;
        }

        isDarkMode = value;
        await ApplyDarkModeAsync();
        await SaveAsync();
        OnThemeChanged?.Invoke();
    }

    /// <summary>
    /// Sets the base (gray scale) color palette.
    /// </summary>
    /// <param name="color">The base color to apply.</param>
    public async Task SetBaseColorAsync(BaseColor color)
    {
        if (baseColor == color)
        {
            return;
        }

        baseColor = color;
        await ApplyBaseColorAsync();
        await SaveAsync();
        OnThemeChanged?.Invoke();
    }

    /// <summary>
    /// Sets the primary accent color.
    /// </summary>
    /// <param name="color">The primary color to apply.</param>
    public async Task SetPrimaryColorAsync(PrimaryColor color)
    {
        if (primaryColor == color)
        {
            return;
        }

        primaryColor = color;
        await ApplyPrimaryColorAsync();
        await SaveAsync();
        OnThemeChanged?.Invoke();
    }

    /// <summary>
    /// Sets the border radius.
    /// </summary>
    /// <param name="value">The radius in rem (e.g., 0, 0.3, 0.5, 0.75, 1.0).</param>
    public async Task SetRadiusAsync(double value)
    {
        if (!double.IsFinite(value) || value < 0 || value > 4)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Radius must be between 0 and 4 rem.");
        }
        if (Math.Abs(radius - value) < 0.001)
        {
            return;
        }

        radius = value;
        await ApplyRadiusAsync();
        await SaveAsync();
        OnThemeChanged?.Invoke();
    }

    /// <summary>Applies and persists a complete preset in one update.</summary>
    public async Task SetPresetAsync(ThemePreset preset)
    {
        ArgumentNullException.ThrowIfNull(preset);
        preset.Validate();
        await InitializeAsync();
        isDarkMode = preset.DarkMode;
        baseColor = preset.BaseColor;
        primaryColor = preset.PrimaryColor;
        radius = preset.Radius;
        design = preset.Design;
        await ApplyAllAsync();
        await SaveAsync();
        OnThemeChanged?.Invoke();
    }

    /// <summary>Changes appearance while preserving the current colors, radius and dark mode.</summary>
    public async Task SetDesignAsync(ThemeDesign value)
    {
        ArgumentNullException.ThrowIfNull(value);
        value.Validate();
        await InitializeAsync();
        await SetPresetAsync(Preset with { Design = value });
    }

    private async Task ApplyAllAsync()
    {
        if (module is null)
        {
            return;
        }

        try
        {
            await module.InvokeVoidAsync("theme.applyTheme",
                isDarkMode,
                baseColor.ToString().ToLowerInvariant(),
                primaryColor.ToString().ToLowerInvariant(),
                radius, design.ToJs());
        }
        catch
        {
            // Ignore if JS is unavailable
        }
    }

    private async Task ApplyDarkModeAsync()
    {
        if (module is null)
        {
            return;
        }

        try
        {
            await module.InvokeVoidAsync("theme.applyDarkMode", isDarkMode);
        }
        catch
        {
            // Ignore if JS is unavailable
        }
    }

    private async Task ApplyBaseColorAsync()
    {
        if (module is null)
        {
            return;
        }

        try
        {
            await module.InvokeVoidAsync("theme.applyBaseColor", baseColor.ToString().ToLowerInvariant());
        }
        catch
        {
            // Ignore if JS is unavailable
        }
    }

    private async Task ApplyPrimaryColorAsync()
    {
        if (module is null)
        {
            return;
        }

        try
        {
            await module.InvokeVoidAsync("theme.applyPrimaryColor", primaryColor.ToString().ToLowerInvariant());
        }
        catch
        {
            // Ignore if JS is unavailable
        }
    }

    private async Task ApplyRadiusAsync()
    {
        if (module is null)
        {
            return;
        }

        try
        {
            await module.InvokeVoidAsync("theme.applyRadius", radius);
        }
        catch
        {
            // Ignore if JS is unavailable
        }
    }

    private async Task SaveAsync()
    {
        if (module is null || !options.PersistToLocalStorage)
        {
            return;
        }

        try
        {
            await module.InvokeVoidAsync("theme.saveTheme",
                isDarkMode,
                baseColor.ToString().ToLowerInvariant(),
                primaryColor.ToString().ToLowerInvariant(),
                radius, design.ToJs());
        }
        catch
        {
            // Ignore if localStorage is unavailable
        }
    }

    private static TEnum ParseEnum<TEnum>(string? value, TEnum fallback) where TEnum : struct, Enum
    {
        if (string.IsNullOrEmpty(value))
        {
            return fallback;
        }

        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var result) && Enum.IsDefined(result) ? result : fallback;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        // Nothing to release: the theme module is shared and owned by JsModules.
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Represents saved theme state from <c>localStorage</c>.
    /// </summary>
    private sealed class ThemeState
    {
        /// <summary>Gets or sets whether dark mode is enabled.</summary>
        public bool IsDarkMode { get; set; }

        /// <summary>Gets or sets the base color name.</summary>
        public string? BaseColor { get; set; }

        /// <summary>Gets or sets the primary color name.</summary>
        public string? PrimaryColor { get; set; }

        /// <summary>Gets or sets the border radius in rem.</summary>
        public double? Radius { get; set; }
        public ThemeDesignState? Design { get; set; }
    }

    private sealed class ThemeDesignState
    {
        public string? Density { get; set; }
        public string? Font { get; set; }
        public string? Surface { get; set; }
        public string? MenuColor { get; set; }
        public string? MenuAccent { get; set; }
    }
}

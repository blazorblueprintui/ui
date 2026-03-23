using Microsoft.JSInterop;

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
        isDarkMode = options.DefaultDarkMode;
        baseColor = options.DefaultBaseColor;
        primaryColor = options.DefaultPrimaryColor;
        radius = options.DefaultRadius;
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
            module = await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorBlueprint.Components/js/theme.js");
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
            var saved = await module.InvokeAsync<ThemeState?>("loadTheme");
            if (saved is not null)
            {
                isDarkMode = saved.IsDarkMode;
                baseColor = ParseEnum(saved.BaseColor, options.DefaultBaseColor);
                primaryColor = ParseEnum(saved.PrimaryColor, options.DefaultPrimaryColor);
                radius = saved.Radius ?? options.DefaultRadius;
            }
            else if (options.DetectSystemPreference)
            {
                isDarkMode = await module.InvokeAsync<bool>("getPrefersDark");
            }

            await ApplyAllAsync();
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
        if (Math.Abs(radius - value) < 0.001)
        {
            return;
        }

        radius = value;
        await ApplyRadiusAsync();
        await SaveAsync();
        OnThemeChanged?.Invoke();
    }

    private async Task ApplyAllAsync()
    {
        if (module is null)
        {
            return;
        }

        try
        {
            await module.InvokeVoidAsync("applyTheme",
                isDarkMode,
                baseColor.ToString().ToLowerInvariant(),
                primaryColor.ToString().ToLowerInvariant(),
                radius);
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
            await module.InvokeVoidAsync("applyDarkMode", isDarkMode);
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
            await module.InvokeVoidAsync("applyBaseColor", baseColor.ToString().ToLowerInvariant());
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
            await module.InvokeVoidAsync("applyPrimaryColor", primaryColor.ToString().ToLowerInvariant());
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
            await module.InvokeVoidAsync("applyRadius", radius);
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
            await module.InvokeVoidAsync("saveTheme",
                isDarkMode,
                baseColor.ToString().ToLowerInvariant(),
                primaryColor.ToString().ToLowerInvariant(),
                radius);
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

        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var result) ? result : fallback;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (module is not null)
        {
            try
            {
                await module.DisposeAsync();
            }
            catch
            {
                // Ignore disposal errors during circuit disconnect
            }

            module = null;
        }

        GC.SuppressFinalize(this);
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
    }
}

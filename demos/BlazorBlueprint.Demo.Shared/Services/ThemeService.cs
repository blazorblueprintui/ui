using Microsoft.JSInterop;

namespace BlazorBlueprint.Demo.Services;

/// <summary>
/// Service for managing dark mode theme state.
/// Handles toggling between light and dark themes with localStorage persistence.
/// </summary>
public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _isDarkMode;
    private bool _isInitialized;

    /// <summary>
    /// Event raised when the theme changes.
    /// </summary>
    public event Action? OnThemeChanged;

    /// <summary>
    /// Gets whether dark mode is currently enabled.
    /// </summary>
    public bool IsDarkMode => _isDarkMode;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Initializes the theme service by loading the saved preference from localStorage.
    /// Should be called once during application startup.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        try
        {
            // Try to load saved preference from localStorage
            var savedTheme = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "theme");

            _isDarkMode = savedTheme == "dark";
            await ApplyThemeAsync(_isDarkMode);

            _isInitialized = true;
        }
        catch
        {
            // If localStorage is not available (SSR), default to light mode
            _isDarkMode = false;
            _isInitialized = true;
        }
    }

    /// <summary>
    /// Toggles between light and dark mode.
    /// </summary>
    public async Task ToggleThemeAsync()
    {
        _isDarkMode = !_isDarkMode;
        await ApplyThemeAsync(_isDarkMode);
        await SaveThemeAsync(_isDarkMode);

        OnThemeChanged?.Invoke();
    }

    /// <summary>
    /// Sets the theme to a specific mode.
    /// </summary>
    /// <param name="isDark">True for dark mode, false for light mode.</param>
    public async Task SetThemeAsync(bool isDark)
    {
        if (_isDarkMode == isDark)
        {
            return;
        }

        _isDarkMode = isDark;
        await ApplyThemeAsync(_isDarkMode);
        await SaveThemeAsync(_isDarkMode);

        OnThemeChanged?.Invoke();
    }

    /// <summary>
    /// Applies the theme by adding or removing the 'dark' class on the HTML element.
    /// </summary>
    private async Task ApplyThemeAsync(bool isDark)
    {
        try
        {
            if (isDark)
            {
                await _jsRuntime.InvokeVoidAsync("eval",
                    "document.documentElement.classList.add('dark')");
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("eval",
                    "document.documentElement.classList.remove('dark')");
            }
        }
        catch
        {
            // Ignore errors during SSR
        }
    }

    /// <summary>
    /// Saves the theme preference to localStorage.
    /// </summary>
    private async Task SaveThemeAsync(bool isDark)
    {
        try
        {
            var theme = isDark ? "dark" : "light";
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme", theme);
        }
        catch
        {
            // Ignore errors if localStorage is not available
        }
    }
}

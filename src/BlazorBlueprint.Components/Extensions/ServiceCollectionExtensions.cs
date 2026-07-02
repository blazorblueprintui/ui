using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using BlazorBlueprint.Primitives.Extensions;

namespace BlazorBlueprint.Components;

/// <summary>
/// Extension methods for registering BlazorBlueprint.Components services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all BlazorBlueprint services (Primitives + Components) to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureLocalizer">Optional action to configure localization keys via <see cref="DefaultBbLocalizer.Set"/>.</param>
    /// <param name="configureTheme">Optional action to configure the theme system via <see cref="ThemeOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// The localizer is registered as singleton by default via <c>TryAddSingleton</c>.
    /// To enable dynamic culture switching (e.g., per-circuit in Blazor Server), register
    /// your own <see cref="IBbLocalizer"/> implementation as scoped before calling this method.
    /// </para>
    /// <para>
    /// The <see cref="ThemeService"/> is always registered (scoped). Use <paramref name="configureTheme"/>
    /// to set defaults for dark mode, base color, primary color, and persistence behavior.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddBlazorBlueprintComponents(
        this IServiceCollection services,
        Action<DefaultBbLocalizer>? configureLocalizer = null,
        Action<ThemeOptions>? configureTheme = null)
    {
        // Register all primitive services (portal, focus, positioning, dropdown manager, keyboard shortcuts)
        services.AddBlazorBlueprintPrimitives();

        // Register ToastService as scoped for user isolation in Blazor Server
        // Each user session gets its own toast notification state
        services.AddScoped<ToastService>();

        // Register DialogService as scoped for programmatic confirm dialogs
        services.AddScoped<DialogService>();

        // Register ThemeService as scoped (each circuit/session gets its own theme state)
        var themeOptions = new ThemeOptions();
        configureTheme?.Invoke(themeOptions);
        services.TryAddSingleton(themeOptions);
        services.TryAddScoped<ThemeService>();

        // Register localizer (singleton by default; consumers can pre-register IBbLocalizer as scoped)
        var localizer = new DefaultBbLocalizer();
        configureLocalizer?.Invoke(localizer);
        services.TryAddSingleton<IBbLocalizer>(localizer);

        return services;
    }
}

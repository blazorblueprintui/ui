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
    /// <param name="configureLocalization">Optional action to configure localization labels.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// Localization options are registered as singleton by default. If you need dynamic
    /// culture switching (e.g., per-circuit in Blazor Server), register
    /// <see cref="BbLocalizationOptions"/> as scoped <b>before</b> calling this method —
    /// <c>TryAddSingleton</c> will not overwrite an existing registration.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddBlazorBlueprintComponents(
        this IServiceCollection services,
        Action<BbLocalizationOptions>? configureLocalization = null)
    {
        // Register all primitive services (portal, focus, positioning, dropdown manager, keyboard shortcuts)
        services.AddBlazorBlueprintPrimitives();

        // Register ToastService as scoped for user isolation in Blazor Server
        // Each user session gets its own toast notification state
        services.AddScoped<ToastService>();

        // Register DialogService as scoped for programmatic confirm dialogs
        services.AddScoped<DialogService>();

        // Register localization options (singleton by default; consumers can pre-register as scoped for dynamic cultures)
        var options = new BbLocalizationOptions();
        configureLocalization?.Invoke(options);
        services.TryAddSingleton(options);

        return services;
    }
}

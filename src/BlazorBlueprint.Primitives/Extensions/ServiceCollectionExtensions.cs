using Microsoft.Extensions.DependencyInjection;
using BlazorBlueprint.Primitives.Services;

namespace BlazorBlueprint.Primitives.Extensions;

/// <summary>
/// Extension methods for registering BlazorBlueprint.Primitives services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all BlazorBlueprint.Primitives primitive services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOverlays">Optional action to configure overlay rendering options
    /// (e.g. opt the whole app into native <c>&lt;dialog&gt;</c> rendering).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBlazorBlueprintPrimitives(
        this IServiceCollection services,
        Action<OverlayRenderingOptions>? configureOverlays = null)
    {
        // Overlay rendering options (global default strategy). Registered as singleton so the
        // resolved default is consistent across all render-mode scopes.
        var overlayOptions = new OverlayRenderingOptions();
        configureOverlays?.Invoke(overlayOptions);
        services.AddSingleton(overlayOptions);

        // Native overlay service (capability detection + native <dialog> driving).
        services.AddScoped<INativeOverlayService, NativeOverlayService>();

        // Register PortalService as scoped for user isolation in Blazor Server
        // Each user session gets its own portal registry
        services.AddScoped<IPortalService, PortalService>();

        // Register FocusManager as scoped (component-specific state)
        services.AddScoped<IFocusManager, FocusManager>();

        // Register PositioningService as scoped (component-specific state)
        services.AddScoped<IPositioningService, PositioningService>();

        // Register DropdownManagerService as scoped (ensures only one dropdown open at a time per user session)
        services.AddScoped<DropdownManagerService>();

        // Register KeyboardShortcutService as scoped (per user session for global keyboard shortcuts)
        services.AddScoped<IKeyboardShortcutService, KeyboardShortcutService>();

        return services;
    }
}

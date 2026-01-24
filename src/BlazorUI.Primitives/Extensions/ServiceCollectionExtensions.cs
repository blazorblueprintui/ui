using Microsoft.Extensions.DependencyInjection;
using BlazorUI.Primitives.Services;

namespace BlazorUI.Primitives.Extensions;

/// <summary>
/// Extension methods for registering BlazorUI.Primitives services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all BlazorUI.Primitives primitive services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBlazorUIPrimitives(this IServiceCollection services)
    {
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

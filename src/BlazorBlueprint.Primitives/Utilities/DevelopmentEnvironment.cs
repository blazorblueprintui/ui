using System.Reflection;

namespace BlazorBlueprint.Primitives.Utilities;

/// <summary>
/// Detects whether the host application is running in the Development environment,
/// so diagnostics that only help while building an app can be suppressed everywhere else.
/// </summary>
/// <remarks>
/// The environment abstraction differs per render mode and neither type is referenced by
/// this package: Blazor Server (and any generic host) registers
/// <c>Microsoft.Extensions.Hosting.IHostEnvironment</c>, while Blazor WebAssembly registers
/// <c>Microsoft.AspNetCore.Components.WebAssembly.Hosting.IWebAssemblyHostEnvironment</c>.
/// Both are resolved by name against the app's own service provider rather than taking a
/// package dependency on either. The answer is fixed for the lifetime of the process, so it
/// is resolved once and cached; every subsequent check is a field read.
/// <para>
/// When neither type can be found — a trimmed publish, or a host that registers neither
/// service — the result is <c>false</c>, so diagnostics stay silent. That is the safe
/// direction: a missed development warning is an inconvenience, a warning logged in
/// production is noise in someone else's telemetry.
/// </para>
/// </remarks>
internal static class DevelopmentEnvironment
{
    private const string DevelopmentEnvironmentName = "Development";

    private const string HostEnvironmentTypeName =
        "Microsoft.Extensions.Hosting.IHostEnvironment, Microsoft.Extensions.Hosting.Abstractions";

    private const string WebAssemblyHostEnvironmentTypeName =
        "Microsoft.AspNetCore.Components.WebAssembly.Hosting.IWebAssemblyHostEnvironment, Microsoft.AspNetCore.Components.WebAssembly";

    private static bool? cachedIsDevelopment;

    /// <summary>
    /// Returns true when the host application's environment is "Development".
    /// </summary>
    /// <param name="services">The application's service provider.</param>
    public static bool IsDevelopment(IServiceProvider services)
    {
        if (cachedIsDevelopment.HasValue)
        {
            return cachedIsDevelopment.Value;
        }

        var environmentName =
            ReadEnvironmentName(services, HostEnvironmentTypeName, "EnvironmentName")
            ?? ReadEnvironmentName(services, WebAssemblyHostEnvironmentTypeName, "Environment");

        var isDevelopment = string.Equals(environmentName, DevelopmentEnvironmentName, StringComparison.OrdinalIgnoreCase);
        cachedIsDevelopment = isDevelopment;
        return isDevelopment;
    }

    private static string? ReadEnvironmentName(IServiceProvider services, string assemblyQualifiedTypeName, string propertyName)
    {
        var environmentType = Type.GetType(assemblyQualifiedTypeName, throwOnError: false);
        if (environmentType is null)
        {
            return null;
        }

        var environment = services.GetService(environmentType);
        if (environment is null)
        {
            return null;
        }

        var property = environmentType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return property?.GetValue(environment) as string;
    }
}

using BlazorBlueprint.Primitives.Services;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

/// <summary>
/// Shared, cached loader for the BlazorBlueprint components core bundle — the five modules that
/// load on nearly every page.
/// </summary>
/// <remarks>
/// <para>
/// One place for the path, where there were ten copies of the string literal. And one place for
/// the version query that stops a browser or CDN serving a stale copy of the bundle against a
/// newer build; see <see cref="PrimitiveModules.ModuleUrl"/> for why that matters.
/// </para>
/// <para>
/// Functions are addressed by a dotted identifier of <c>namespace.function</c>, where the
/// namespace is the module's file name in camelCase: <c>sidebarInset.scrollToTop</c>,
/// <c>theme.initialize</c>.
/// </para>
/// </remarks>
public static class ComponentModules
{
    /// <summary>
    /// The path of the core bundle, as imported from C#.
    /// </summary>
    public const string CorePath = "./_content/BlazorBlueprint.Components/js/bb-components-core.js";

    /// <summary>
    /// The URL actually imported: <see cref="CorePath"/> with the library version as a query.
    /// </summary>
    // Revise both the entry and its dependency URLs for local builds whose assembly
    // informational version is unchanged, as well as for released package upgrades.
    public static string CoreUrl { get; } = JsModules.Versioned($"{CorePath}?assets=2", typeof(ComponentModules).Assembly);

    /// <summary>
    /// Gets the shared core bundle for the given runtime, importing it on first use.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime of the current circuit or application.</param>
    /// <returns>The shared, non-owning module reference.</returns>
    public static Task<IJSObjectReference> GetCoreAsync(IJSRuntime jsRuntime)
        => JsModules.GetAsync(jsRuntime, CoreUrl);

    /// <summary>
    /// Gets the shared core bundle without awaiting, or returns false if it is not imported yet.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime of the current circuit or application.</param>
    /// <param name="module">The shared, non-owning module reference, when one is loaded.</param>
    /// <returns><c>true</c> when the bundle is loaded and <paramref name="module"/> is set.</returns>
    public static bool TryGetCoreLoaded(IJSRuntime jsRuntime, out IJSObjectReference module)
        => JsModules.TryGetLoaded(jsRuntime, CoreUrl, out module);
}

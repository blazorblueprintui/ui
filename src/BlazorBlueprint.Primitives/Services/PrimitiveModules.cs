using Microsoft.JSInterop;

namespace BlazorBlueprint.Primitives.Services;

/// <summary>
/// Shared, cached loader for the BlazorBlueprint primitive JavaScript bundle.
/// </summary>
/// <remarks>
/// <para>
/// In Blazor Server an <c>import(...)</c> issued from C# is an instruction posted to the browser
/// that the server then awaits, so every lazily-imported module costs a full circuit round trip.
/// That cost is paid per module and per page load even when the file is already in the browser's
/// HTTP cache, and it scales with the user's latency rather than with the size of the file.
/// </para>
/// <para>
/// Every primitive module is therefore re-exported from a single bundle, and this type caches the
/// one reference to it per <see cref="IJSRuntime"/> — which means per circuit on Server and per
/// application on WebAssembly. The first caller pays one round trip and every later caller, in any
/// component, gets the same reference for free.
/// </para>
/// <para>
/// Functions are addressed by a dotted identifier of <c>namespace.function</c>, where the namespace
/// is the module's file name in camelCase:
/// <code>
/// var module = await PrimitiveModules.GetAsync(JSRuntime);
/// await module.InvokeVoidAsync("elementUtils.scrollIntoView", elementId);
/// </code>
/// </para>
/// <para>
/// The returned reference is non-owning: its <c>DisposeAsync</c> does nothing. One component
/// disposing it would otherwise break every other component on the circuit, and the components
/// that hold it have no way to know whether they are the last one. Blazor releases the underlying
/// module when the circuit ends.
/// </para>
/// </remarks>
public static class PrimitiveModules
{
    /// <summary>
    /// The path of the primitive bundle, as imported from C#.
    /// </summary>
    public const string ModulePath = "./_content/BlazorBlueprint.Primitives/js/primitives/bb-primitives.js";

    /// <summary>
    /// The URL actually imported: <see cref="ModulePath"/> with the library version as a query.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The query is what stops a browser, or a CDN in front of it, from serving last week's bundle
    /// against this week's C#. Static web assets carry no fingerprint on .NET 8, so without it a
    /// cached <c>bb-primitives.js</c> is a valid response for as long as the cache says — and a
    /// bundle that predates a renamed export fails with <c>Could not find 'x.y'</c> at the first
    /// call, which on Blazor Server has taken whole circuits down.
    /// </para>
    /// <para>
    /// It busts the entry file only. The modules it imports are fetched by the browser relative to
    /// it, without the query, so a stale one of those can still be served from cache; the bundle
    /// checks for that itself as it loads and fails with a message that names the file.
    /// </para>
    /// </remarks>
    // Asset revision 2 updates the entry URL along with the revised tree-keyboard import,
    // including local builds that retain the same informational assembly version.
    public static string ModuleUrl { get; } = JsModules.Versioned($"{ModulePath}?assets=2", typeof(PrimitiveModules).Assembly);

    /// <summary>
    /// Gets the shared primitive bundle for the given runtime, importing it on first use.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime of the current circuit or application.</param>
    /// <returns>The shared, non-owning module reference.</returns>
    public static Task<IJSObjectReference> GetAsync(IJSRuntime jsRuntime)
        => JsModules.GetAsync(jsRuntime, ModuleUrl);

    /// <summary>
    /// Gets the shared primitive bundle without awaiting, or returns false if it is not imported
    /// yet. For callers that must issue interop from a synchronous pass.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime of the current circuit or application.</param>
    /// <param name="module">The shared, non-owning module reference, when one is loaded.</param>
    /// <returns><c>true</c> when the bundle is loaded and <paramref name="module"/> is set.</returns>
    public static bool TryGetLoaded(IJSRuntime jsRuntime, out IJSObjectReference module)
        => JsModules.TryGetLoaded(jsRuntime, ModuleUrl, out module);
}

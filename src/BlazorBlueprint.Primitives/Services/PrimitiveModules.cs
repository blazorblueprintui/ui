using Microsoft.JSInterop;
using System.Runtime.CompilerServices;

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

    private static readonly ConditionalWeakTable<IJSRuntime, Handle> Cache = new();

    /// <summary>
    /// Gets the shared primitive bundle for the given runtime, importing it on first use.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime of the current circuit or application.</param>
    /// <returns>The shared, non-owning module reference.</returns>
    public static Task<IJSObjectReference> GetAsync(IJSRuntime jsRuntime)
    {
        ArgumentNullException.ThrowIfNull(jsRuntime);
        return Cache.GetValue(jsRuntime, static _ => new Handle()).GetAsync(jsRuntime);
    }

    /// <summary>
    /// Forwards every call to the real module but ignores disposal, so that a component tearing
    /// itself down cannot take the shared module away from the components that are still running.
    /// </summary>
    private sealed class NonOwningReference : IJSObjectReference
    {
        private readonly IJSObjectReference inner;

        public NonOwningReference(IJSObjectReference inner) => this.inner = inner;

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => inner.InvokeAsync<TValue>(identifier, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            => inner.InvokeAsync<TValue>(identifier, cancellationToken, args);

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class Handle
    {
        private readonly object sync = new();
        private Task<IJSObjectReference>? inFlight;

        public Task<IJSObjectReference> GetAsync(IJSRuntime jsRuntime)
        {
            lock (sync)
            {
                // Concurrent callers share one in-flight import, so the circuit pays for exactly
                // one round trip however many components ask at once.
                //
                // A failed import is never kept. Prerendering and a disconnecting circuit both fail
                // here, and both are states the app recovers from — caching the failure would leave
                // every overlay on the page permanently broken instead.
                if (inFlight is null || (inFlight.IsCompleted && !inFlight.IsCompletedSuccessfully))
                {
                    inFlight = ImportAsync(jsRuntime);
                }

                return inFlight;
            }
        }

        private static async Task<IJSObjectReference> ImportAsync(IJSRuntime jsRuntime)
        {
            var imported = await jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);
            return new NonOwningReference(imported);
        }
    }
}

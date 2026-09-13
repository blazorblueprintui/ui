using Microsoft.JSInterop;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace BlazorBlueprint.Primitives.Services;

/// <summary>
/// Imports a JavaScript module once per <see cref="IJSRuntime"/> and hands the same reference to
/// every caller.
/// </summary>
/// <remarks>
/// <para>
/// On Blazor Server an <c>import(...)</c> issued from C# is an instruction posted to the browser
/// that the server then awaits, so it costs a circuit round trip — every time, even though the
/// browser has had the file in its module registry since the first one. A component that caches
/// the reference in an instance field therefore pays that round trip <em>per instance</em>: a form
/// with thirteen text inputs imported the same file thirteen times, which measured as roughly four
/// extra client-to-server messages per input at page load.
/// </para>
/// <para>
/// Caching against the runtime means once per circuit on Server, and once per application on
/// WebAssembly. The returned reference is non-owning: its <c>DisposeAsync</c> does nothing,
/// because a component tearing itself down must not take the module away from the components still
/// using it. Blazor releases the real module when the circuit ends.
/// </para>
/// </remarks>
public static class JsModules
{
    private static readonly ConditionalWeakTable<IJSRuntime, ConcurrentDictionary<string, Handle>> Cache = new();

    /// <summary>
    /// Gets the shared reference to a module, importing it on first use.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime of the current circuit or application.</param>
    /// <param name="modulePath">The module path, as passed to <c>import</c>.</param>
    /// <returns>The shared, non-owning module reference.</returns>
    public static Task<IJSObjectReference> GetAsync(IJSRuntime jsRuntime, string modulePath)
    {
        ArgumentNullException.ThrowIfNull(jsRuntime);
        ArgumentException.ThrowIfNullOrWhiteSpace(modulePath);

        var modules = Cache.GetValue(jsRuntime, static _ => new ConcurrentDictionary<string, Handle>(StringComparer.Ordinal));
        return modules.GetOrAdd(modulePath, static _ => new Handle()).GetAsync(jsRuntime, modulePath);
    }

    /// <summary>
    /// Forwards every call to the real module but ignores disposal, so that a component tearing
    /// itself down cannot take a shared module away from the components that are still running.
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

        public Task<IJSObjectReference> GetAsync(IJSRuntime jsRuntime, string modulePath)
        {
            lock (sync)
            {
                // Concurrent callers share one in-flight import, so the circuit pays for exactly
                // one round trip however many components ask at once.
                //
                // A failed import is never kept. Prerendering and a disconnecting circuit both fail
                // here, and both are states the app recovers from — caching the failure would leave
                // every component that needs this module permanently broken instead.
                if (inFlight is null || (inFlight.IsCompleted && !inFlight.IsCompletedSuccessfully))
                {
                    inFlight = ImportAsync(jsRuntime, modulePath);
                }

                return inFlight;
            }
        }

        private static async Task<IJSObjectReference> ImportAsync(IJSRuntime jsRuntime, string modulePath)
        {
            var imported = await jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath);
            return new NonOwningReference(imported);
        }
    }
}

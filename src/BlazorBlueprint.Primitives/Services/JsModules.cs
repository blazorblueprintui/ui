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
    /// Appends the assembly's version to a module path as a <c>v</c> query, so a new release is a
    /// new URL and a cached copy of the old one is never a valid answer for it.
    /// </summary>
    /// <param name="modulePath">The module path, as passed to <c>import</c>.</param>
    /// <param name="assembly">The assembly whose version stamps the URL.</param>
    /// <returns>The path with <c>?v=&lt;version&gt;</c> appended, URL-encoded.</returns>
    public static string Versioned(string modulePath, System.Reflection.Assembly assembly)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modulePath);
        ArgumentNullException.ThrowIfNull(assembly);

        // The informational version carries the prerelease tag and, from MinVer, the commit — so
        // every build is a different URL. Falls back to the four-part version, which is always set.
        var version = assembly
            .GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
            .OfType<System.Reflection.AssemblyInformationalVersionAttribute>()
            .FirstOrDefault()?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(version))
        {
            version = assembly.GetName().Version?.ToString() ?? "0";
        }

        var separator = modulePath.Contains('?') ? '&' : '?';
        return $"{modulePath}{separator}v={Uri.EscapeDataString(version)}";
    }

    /// <summary>
    /// Gets an already-imported module without awaiting, or returns false if it is not loaded yet.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For callers that must issue their interop from a synchronous pass, where awaiting is not
    /// available at all. An overlay dispatches its open from <c>OnParametersSet</c> so the call
    /// rides out in the same flush as the render batch carrying its content; awaiting the import
    /// there would put the call a round trip behind the render and defeat the point.
    /// </para>
    /// <para>
    /// Returning false is not an error, only "not yet". The caller falls back to the awaited path,
    /// which pays one round trip for the import and warms this cache for every open after it.
    /// </para>
    /// </remarks>
    /// <param name="jsRuntime">The JavaScript runtime of the current circuit or application.</param>
    /// <param name="modulePath">The module path, as passed to <c>import</c>.</param>
    /// <param name="module">The shared, non-owning module reference, when one is loaded.</param>
    /// <returns><c>true</c> when the module is loaded and <paramref name="module"/> is set.</returns>
    public static bool TryGetLoaded(IJSRuntime jsRuntime, string modulePath, out IJSObjectReference module)
    {
        module = null!;

        if (jsRuntime is null || string.IsNullOrWhiteSpace(modulePath))
        {
            return false;
        }

        if (!Cache.TryGetValue(jsRuntime, out var modules) || !modules.TryGetValue(modulePath, out var handle))
        {
            return false;
        }

        var inFlight = handle.Loaded;
        if (inFlight is null || !inFlight.IsCompletedSuccessfully)
        {
            return false;
        }

        module = inFlight.Result;
        return true;
    }

    /// <summary>
    /// Forwards every call to the real module but ignores disposal, so that a component tearing
    /// itself down cannot take a shared module away from the components that are still running.
    /// </summary>
    private sealed class NonOwningReference : IJSObjectReference
    {
        private readonly IJSObjectReference inner;

        public NonOwningReference(IJSObjectReference inner)
        {
            this.inner = inner;
        }

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

        /// <summary>The import task, for a caller that can only look and not wait.</summary>
        public Task<IJSObjectReference>? Loaded
        {
            get
            {
                lock (sync)
                {
                    return inFlight;
                }
            }
        }

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

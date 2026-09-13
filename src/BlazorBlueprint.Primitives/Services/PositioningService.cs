using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;

namespace BlazorBlueprint.Primitives.Services;

/// <summary>
/// Implementation of positioning service using Floating UI library.
/// </summary>
public class PositioningService : IPositioningService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PositioningService"/> class.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime for invoking positioning functions.</param>
    public PositioningService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    // The bundle is shared across the whole circuit, so the cache and the lock that used to live
    // here now live in PrimitiveModules. This service must not dispose what it gets back.
    private Task<IJSObjectReference> GetModuleAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(PositioningService));
        return PrimitiveModules.GetAsync(_jsRuntime);
    }

    /// <inheritdoc />
    public async Task<PositionResult> ComputePositionAsync(
        ElementReference reference,
        ElementReference floating,
        PositioningOptions? options = null)
    {
        var module = await GetModuleAsync();
        options ??= new PositioningOptions();

        var jsOptions = new
        {
            placement = options.Placement,
            offset = options.Offset,
            flip = options.Flip,
            shift = options.Shift,
            padding = options.Padding,
            strategy = options.Strategy.ToValue(),
            matchReferenceWidth = options.MatchReferenceWidth
        };

        var result = await module.InvokeAsync<JsonElement>(
            "positioning.computePosition", reference, floating, jsOptions);

        return new PositionResult
        {
            X = result.GetProperty("x").GetDouble(),
            Y = result.GetProperty("y").GetDouble(),
            Placement = result.GetProperty("placement").GetString() ?? options.Placement,
            TransformOrigin = result.TryGetProperty("transformOrigin", out var origin)
                ? origin.GetString()
                : null,
            Strategy = result.TryGetProperty("strategy", out var strategy)
                ? strategy.GetString() ?? options.Strategy.ToValue()
                : options.Strategy.ToValue()
        };
    }

    /// <inheritdoc />
    public async Task ApplyPositionAsync(ElementReference floating, PositionResult position, bool makeVisible = false)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("positioning.applyPosition", floating, position, makeVisible);
    }

    /// <inheritdoc />
    public async Task HidePositionAsync(ElementReference floating)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("positioning.hidePosition", floating);
    }

    /// <inheritdoc />
    public async Task<IAsyncDisposable> AutoUpdateAsync(
        ElementReference reference,
        ElementReference floating,
        PositioningOptions? options = null)
    {
        var module = await GetModuleAsync();
        options ??= new PositioningOptions();

        var jsOptions = new
        {
            placement = options.Placement,
            offset = options.Offset,
            flip = options.Flip,
            shift = options.Shift,
            padding = options.Padding,
            strategy = options.Strategy.ToValue(),
            matchReferenceWidth = options.MatchReferenceWidth
        };

        var cleanup = await module.InvokeAsync<IJSObjectReference>(
            "positioning.autoUpdate", reference, floating, jsOptions);

        return new AutoUpdateHandle(cleanup);
    }

    /// <summary>
    /// Disposes the positioning service, releasing JavaScript module resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        GC.SuppressFinalize(this);
        _disposed = true;

        // Nothing to release. The module reference belongs to PrimitiveModules and is shared with
        // every other component on the circuit; Blazor frees it when the circuit ends.
        await Task.CompletedTask;
    }

    private sealed class AutoUpdateHandle : IAsyncDisposable
    {
        private readonly IJSObjectReference _cleanup;

        public AutoUpdateHandle(IJSObjectReference cleanup)
        {
            _cleanup = cleanup;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await _cleanup.InvokeVoidAsync("apply");
            }
            catch
            {
                // Cleanup may already be disposed
            }
        }
    }
}

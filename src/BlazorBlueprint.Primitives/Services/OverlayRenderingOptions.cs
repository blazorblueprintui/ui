namespace BlazorBlueprint.Primitives.Services;

/// <summary>
/// Global options controlling how BlazorBlueprint overlays render by default.
/// Supplied via <c>AddBlazorBlueprintPrimitives(configure)</c> and read at run time.
/// </summary>
public class OverlayRenderingOptions
{
    /// <summary>
    /// The default <see cref="OverlayRenderingStrategy"/> used by overlay components when
    /// they do not specify one themselves. Defaults to <see cref="OverlayRenderingStrategy.JavaScript"/>,
    /// preserving existing behaviour. Set to <see cref="OverlayRenderingStrategy.Native"/> to opt
    /// the whole app into native rendering (with automatic fallback when a browser lacks support).
    /// </summary>
    public OverlayRenderingStrategy DefaultStrategy { get; set; } = OverlayRenderingStrategy.JavaScript;
}

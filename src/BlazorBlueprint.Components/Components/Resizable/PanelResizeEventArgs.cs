using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Event arguments for a completed panel group resize.
/// </summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Type represents event arguments")]
public class PanelResizeEventArgs
{
    /// <summary>
    /// The panel sizes as percentages, in the order the panels were declared.
    /// </summary>
    /// <remarks>
    /// These are the sizes the group committed, after its own minimum and maximum clamping, so they
    /// are the values the panels are actually rendered at rather than the raw drag result.
    /// </remarks>
    public IReadOnlyList<double> Sizes { get; init; } = Array.Empty<double>();
}

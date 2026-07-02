using System.Collections.Generic;

namespace BlazorBlueprint.Components;

/// <summary>
/// Orientation of a dock split node.
/// </summary>
internal enum DockOrientation
{
    /// <summary>Children are arranged left-to-right.</summary>
    Horizontal,

    /// <summary>Children are arranged top-to-bottom.</summary>
    Vertical
}

/// <summary>
/// Base type for a node in the dock layout tree. A node is either a
/// <see cref="DockSplitNode"/> (a horizontal/vertical split) or a
/// <see cref="DockTabGroupNode"/> (a set of tabbed panels).
/// </summary>
internal abstract class DockNode
{
    /// <summary>Stable identity for the node, used for diffing and keying.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
}

/// <summary>
/// A split container that arranges two or more child nodes along an axis.
/// </summary>
internal sealed class DockSplitNode : DockNode
{
    /// <summary>The axis along which children are arranged.</summary>
    public DockOrientation Orientation { get; set; }

    /// <summary>The ordered child nodes.</summary>
    public List<DockNode> Children { get; set; } = new();

    /// <summary>The size of each child as a percentage of the split (parallel to <see cref="Children"/>).</summary>
    public List<double> Sizes { get; set; } = new();

    /// <summary>Returns a structural signature that changes whenever the split's shape changes.</summary>
    public string Signature => $"{Orientation}:{string.Join(',', Children.Select(c => c.Id))}";

    /// <summary>Ensures the <see cref="Sizes"/> list matches the number of children, distributing evenly.</summary>
    public void NormalizeSizes()
    {
        if (Children.Count == 0)
        {
            Sizes.Clear();
            return;
        }

        if (Sizes.Count != Children.Count)
        {
            var even = 100.0 / Children.Count;
            Sizes = Enumerable.Repeat(even, Children.Count).ToList();
            return;
        }

        var total = Sizes.Sum();
        if (total <= 0)
        {
            var even = 100.0 / Children.Count;
            Sizes = Enumerable.Repeat(even, Children.Count).ToList();
            return;
        }

        // Re-scale to total 100 so flex percentages stay sensible.
        for (var i = 0; i < Sizes.Count; i++)
        {
            Sizes[i] = Sizes[i] / total * 100.0;
        }
    }
}

/// <summary>
/// A group of panels rendered as tabs. Exactly one panel is active at a time.
/// </summary>
internal sealed class DockTabGroupNode : DockNode
{
    /// <summary>The ordered panel identifiers shown as tabs.</summary>
    public List<string> PanelIds { get; set; } = new();

    /// <summary>The identifier of the currently active panel, if any.</summary>
    public string? ActivePanelId { get; set; }

    /// <summary>Ensures <see cref="ActivePanelId"/> references a panel that is still present.</summary>
    public void EnsureActive()
    {
        if (PanelIds.Count == 0)
        {
            ActivePanelId = null;
            return;
        }

        if (ActivePanelId is null || !PanelIds.Contains(ActivePanelId))
        {
            ActivePanelId = PanelIds[0];
        }
    }
}

/// <summary>
/// A free-floating window that hosts a tab group detached from the docked layout.
/// </summary>
internal sealed class DockFloatingWindow
{
    /// <summary>The default width (pixels) a floating window is created with, before constraints.</summary>
    public const double DefaultWidth = 360;

    /// <summary>The default height (pixels) a floating window is created with, before constraints.</summary>
    public const double DefaultHeight = 260;

    /// <summary>The smallest width (pixels) a floating window may be, regardless of panel constraints.</summary>
    public const double MinFloatingWidth = 180;

    /// <summary>The smallest height (pixels) a floating window may be, regardless of panel constraints.</summary>
    public const double MinFloatingHeight = 120;

    /// <summary>Stable identity for the window.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>The tab group hosted by this window.</summary>
    public DockTabGroupNode Group { get; set; } = new();

    /// <summary>Horizontal offset (pixels) within the dock surface.</summary>
    public double X { get; set; }

    /// <summary>Vertical offset (pixels) within the dock surface.</summary>
    public double Y { get; set; }

    /// <summary>Window width in pixels.</summary>
    public double Width { get; set; } = DefaultWidth;

    /// <summary>Window height in pixels.</summary>
    public double Height { get; set; } = DefaultHeight;
}

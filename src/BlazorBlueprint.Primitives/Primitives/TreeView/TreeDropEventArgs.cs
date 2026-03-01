using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Primitives;

/// <summary>
/// Event arguments for a tree node drop operation.
/// </summary>
/// <typeparam name="TItem">The type of the tree data item.</typeparam>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Type represents event arguments")]
public class TreeDropEventArgs<TItem>
{
    /// <summary>
    /// Gets the node that was dragged.
    /// </summary>
    public TItem SourceNode { get; }

    /// <summary>
    /// Gets the node that was dropped onto.
    /// </summary>
    public TItem TargetNode { get; }

    /// <summary>
    /// Gets the drop position relative to the target node.
    /// </summary>
    public TreeDropPosition Position { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeDropEventArgs{TItem}"/> class.
    /// </summary>
    public TreeDropEventArgs(TItem sourceNode, TItem targetNode, TreeDropPosition position)
    {
        SourceNode = sourceNode;
        TargetNode = targetNode;
        Position = position;
    }
}

namespace BlazorBlueprint.Primitives.Sortable;

/// <summary>Describes a proposed reorder before the consumer's collection is changed.</summary>
/// <param name="Item">The item being moved.</param>
/// <param name="OldIndex">Its original zero-based position.</param>
/// <param name="NewIndex">Its proposed zero-based position.</param>
public sealed record SortableMoveContext<TItem>(TItem Item, int OldIndex, int NewIndex);

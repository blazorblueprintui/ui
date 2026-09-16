namespace BlazorBlueprint.Primitives.Sortable;

/// <summary>A proposed cross-list drop, evaluated by the source before either list callback runs.</summary>
public sealed record SortableDropContext<TItem>(TItem Item, string SourceId, string TargetId, int OldIndex, int NewIndex, bool IsClone);

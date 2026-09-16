namespace BlazorBlueprint.Components;

/// <summary>A group of currently visible/loaded items, supplied to the group header template.</summary>
public sealed record DataViewGroupContext<TItem>(string? Key, IReadOnlyList<TItem> Items) where TItem : class;

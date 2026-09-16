namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>Isolates pending edits from source records, keyed by stable row identity.</summary>
/// <typeparam name="TData">The row type.</typeparam>
public sealed class DataGridEditBuffer<TData> where TData : class
{
    private readonly Func<TData, TData> clone;
    private readonly Func<TData, object> key;
    private readonly Dictionary<object, DataGridEditChange<TData>> changes;

    /// <summary>Creates a buffer. The factory must return an independent editable copy.</summary>
    /// <param name="editItemFactory">Clones a row, including editable nested objects.</param>
    /// <param name="itemKey">Stable row identity, or null for reference identity.</param>
    public DataGridEditBuffer(Func<TData, TData> editItemFactory, Func<TData, object>? itemKey = null)
    {
        ArgumentNullException.ThrowIfNull(editItemFactory);
        clone = editItemFactory;
        key = itemKey ?? (item => item);
        changes = new(itemKey == null ? ReferenceEqualityComparer.Instance : EqualityComparer<object>.Default);
    }

    /// <summary>Gets the staged rows in insertion order.</summary>
    public IReadOnlyList<DataGridEditChange<TData>> Changes => changes.Values.ToArray();

    /// <summary>Gets the number of staged rows.</summary>
    public int Count => changes.Count;

    /// <summary>Gets an existing draft or creates an independent draft for this row.</summary>
    public TData Begin(TData item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var identity = key(item);
        if (changes.TryGetValue(identity, out var existing))
        {
            return existing.Item;
        }

        var draft = clone(item);
        if (draft == null || ReferenceEquals(draft, item))
        {
            throw new InvalidOperationException("EditItemFactory must return an independent, non-null copy of the row.");
        }

        changes.Add(identity, new DataGridEditChange<TData> { OriginalItem = item, Item = draft });
        return draft;
    }

    /// <summary>Gets a staged draft, or the source row if it has not been edited.</summary>
    public TData GetDisplayItem(TData item) => changes.TryGetValue(key(item), out var change) ? change.Item : item;

    /// <summary>Discards the draft for a row without modifying its source.</summary>
    public void Discard(TData item) => changes.Remove(key(item));

    /// <summary>Discards all drafts without modifying their sources.</summary>
    public void Clear() => changes.Clear();
}

/// <summary>A source record and its isolated proposed replacement.</summary>
/// <typeparam name="TData">The row type.</typeparam>
public sealed class DataGridEditChange<TData> where TData : class
{
    /// <summary>Gets the source record as it was when editing began.</summary>
    public required TData OriginalItem { get; init; }

    /// <summary>Gets the editable draft. It is not applied until a commit succeeds.</summary>
    public required TData Item { get; init; }
}

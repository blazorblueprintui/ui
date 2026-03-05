namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// An equality comparer that delegates to a user-supplied key function.
/// Used to give stable identity to data items so that selection and expansion
/// state survives data re-fetches (where object references change but the
/// logical identity remains the same).
/// </summary>
internal sealed class KeyBasedEqualityComparer<TData> : IEqualityComparer<TData> where TData : class
{
    private readonly Func<TData, object> keySelector;

    public KeyBasedEqualityComparer(Func<TData, object> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        this.keySelector = keySelector;
    }

    public bool Equals(TData? x, TData? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return Equals(keySelector(x), keySelector(y));
    }

    public int GetHashCode(TData obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return keySelector(obj)?.GetHashCode() ?? 0;
    }
}

using System.Globalization;

namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Identifies one group in a nested grouping by the ordered keys leading to it, from the
/// outermost level inward.
/// </summary>
/// <remarks>
/// A raw key is not enough to identify a nested group: grouping by Department then Status puts a
/// group keyed <c>Active</c> under every department, and a flat set of collapsed keys would
/// collapse all of them together. The full path disambiguates them.
/// <para>
/// Equality is by value over the whole path, so a path rebuilt on the next render matches the one
/// stored in the collapsed set. Keys are compared with <see cref="object.Equals(object?)"/>, and a
/// null key is a legitimate path segment — rows whose grouped column is null form their own group.
/// </para>
/// </remarks>
public sealed class GroupPath : IEquatable<GroupPath>
{
    private readonly object?[] keys;
    private readonly int hash;

    /// <summary>
    /// Creates a path from an ordered set of keys, outermost first.
    /// </summary>
    /// <param name="keys">The keys, outermost group first.</param>
    /// <exception cref="ArgumentNullException">Thrown when keys is null.</exception>
    public GroupPath(params object?[] keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        this.keys = (object?[])keys.Clone();

        var accumulated = new HashCode();
        foreach (var key in this.keys)
        {
            accumulated.Add(key);
        }

        hash = accumulated.ToHashCode();
    }

    /// <summary>
    /// Creates a path from an ordered sequence of keys, outermost first.
    /// </summary>
    /// <param name="keys">The keys, outermost group first.</param>
    public GroupPath(IEnumerable<object?> keys)
        : this((keys ?? throw new ArgumentNullException(nameof(keys))).ToArray())
    {
    }

    /// <summary>
    /// Gets the keys in this path, outermost group first.
    /// </summary>
    public IReadOnlyList<object?> Keys => keys;

    /// <summary>
    /// Gets how deep this group sits, zero-based. The outermost level is depth 0.
    /// </summary>
    public int Depth => keys.Length - 1;

    /// <summary>
    /// Gets the key of this group itself — the last segment of the path.
    /// </summary>
    public object? Key => keys.Length > 0 ? keys[^1] : null;

    /// <summary>
    /// Gets an empty path, which is the parent of every outermost group.
    /// </summary>
    public static GroupPath Root { get; } = new();

    /// <summary>
    /// Returns a new path one level deeper, ending in <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key of the child group.</param>
    /// <returns>The child path.</returns>
    public GroupPath Append(object? key)
    {
        var extended = new object?[keys.Length + 1];
        Array.Copy(keys, extended, keys.Length);
        extended[^1] = key;
        return new GroupPath(extended);
    }

    /// <summary>
    /// Returns the path of this group's parent, or <see cref="Root"/> when this is an outermost
    /// group.
    /// </summary>
    /// <returns>The parent path.</returns>
    public GroupPath Parent()
    {
        if (keys.Length == 0)
        {
            return Root;
        }

        return new GroupPath(keys[..^1]);
    }

    /// <summary>
    /// Gets whether <paramref name="other"/> is this path or sits under it.
    /// </summary>
    /// <param name="other">The path to test.</param>
    /// <returns>True when other starts with this path.</returns>
    public bool IsAncestorOfOrSelf(GroupPath other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (other.keys.Length < keys.Length)
        {
            return false;
        }

        for (var i = 0; i < keys.Length; i++)
        {
            if (!Equals(keys[i], other.keys[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public bool Equals(GroupPath? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (hash != other.hash || keys.Length != other.keys.Length)
        {
            return false;
        }

        for (var i = 0; i < keys.Length; i++)
        {
            if (!Equals(keys[i], other.keys[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as GroupPath);

    /// <inheritdoc />
    public override int GetHashCode() => hash;

    /// <inheritdoc />
    public override string ToString() =>
        string.Join(" › ", keys.Select(k => Convert.ToString(k, CultureInfo.CurrentCulture) ?? "(none)"));
}

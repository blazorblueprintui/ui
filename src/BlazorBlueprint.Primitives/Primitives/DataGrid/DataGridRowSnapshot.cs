using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Records the values of an item's public writable properties, so an edit can be undone.
/// </summary>
/// <remarks>
/// Row editing binds straight to the item, which is what lets an edit template use an ordinary
/// <c>@bind-Value</c>. That means a cancel has to put the old values back, and this is what holds
/// them. Only public instance properties that can be both read and written are captured — a
/// computed property has nothing to restore, and a private field is not something an edit template
/// can have changed.
/// </remarks>
/// <typeparam name="TData">The type of data items.</typeparam>
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types",
    Justification = "Capture is the only way to make a snapshot, and the type argument is already " +
                    "implied by the item being passed in.")]
public sealed class DataGridRowSnapshot<TData> where TData : class
{
    private static readonly PropertyInfo[] Properties = typeof(TData)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
        .ToArray();

    private readonly Dictionary<string, object?> values;

    private DataGridRowSnapshot(TData item, Dictionary<string, object?> values)
    {
        Item = item;
        this.values = values;
    }

    /// <summary>
    /// Gets the item the snapshot was taken from.
    /// </summary>
    public TData Item { get; }

    /// <summary>
    /// Takes a snapshot of an item's current values.
    /// </summary>
    /// <param name="item">The item to record.</param>
    /// <returns>The snapshot.</returns>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    public static DataGridRowSnapshot<TData> Capture(TData item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var values = new Dictionary<string, object?>(Properties.Length, StringComparer.Ordinal);
        foreach (var property in Properties)
        {
            try
            {
                values[property.Name] = property.GetValue(item);
            }
            catch (TargetInvocationException)
            {
                // A property that throws when read has no value worth restoring. Skipping it is
                // better than failing the edit before the user has typed anything.
            }
        }

        return new DataGridRowSnapshot<TData>(item, values);
    }

    /// <summary>
    /// Puts the recorded values back on the item.
    /// </summary>
    /// <remarks>
    /// Restores onto the same instance the snapshot came from, so anything else holding a
    /// reference to that row sees the values revert too.
    /// </remarks>
    public void Restore()
    {
        foreach (var property in Properties)
        {
            if (!values.TryGetValue(property.Name, out var value))
            {
                continue;
            }

            try
            {
                property.SetValue(Item, value);
            }
            catch (TargetInvocationException)
            {
                // A setter that rejects a value it previously held leaves that property as the
                // user left it. Better than abandoning the rest of the restore half-done.
            }
        }
    }

    /// <summary>
    /// Gets whether any recorded value differs from the item's current value.
    /// </summary>
    /// <returns>True when the item has been changed since the snapshot.</returns>
    public bool HasChanges()
    {
        foreach (var property in Properties)
        {
            if (!values.TryGetValue(property.Name, out var original))
            {
                continue;
            }

            object? current;
            try
            {
                current = property.GetValue(Item);
            }
            catch (TargetInvocationException)
            {
                continue;
            }

            if (!Equals(original, current))
            {
                return true;
            }
        }

        return false;
    }
}

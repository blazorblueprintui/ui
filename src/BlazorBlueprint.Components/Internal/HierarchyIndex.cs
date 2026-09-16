namespace BlazorBlueprint.Components;

internal sealed class HierarchyIndex<TItem>
{
    internal sealed record Node(TItem Item, string Value, string Text, string? Parent, List<string> Children);
    internal Dictionary<string, Node> Nodes { get; } = new(StringComparer.Ordinal);
    internal List<string> Roots { get; } = [];

    internal HierarchyIndex(IEnumerable<TItem> items, Func<TItem, string> valueField,
        Func<TItem, string> textField, Func<TItem, IEnumerable<TItem>?> childrenProperty)
    {
        foreach (var item in items)
        {
            Roots.Add(Add(item, null, 0));
        }

        string Add(TItem item, string? parent, int depth)
        {
            if (depth > 256)
            {
                throw new ArgumentException("The hierarchy exceeds 256 levels or contains a cycle.", nameof(items));
            }
            var value = valueField(item);
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            var node = new Node(item, value, textField(item), parent, []);
            if (!Nodes.TryAdd(value, node))
            {
                throw new ArgumentException($"Hierarchy keys must be unique; duplicate or cyclic key '{value}'.", nameof(items));
            }
            foreach (var child in childrenProperty(item) ?? [])
            {
                node.Children.Add(Add(child, value, depth + 1));
            }
            return value;
        }
    }

    internal IReadOnlyList<Node> Path(string? value)
    {
        var result = new List<Node>();
        while (value != null && Nodes.TryGetValue(value, out var node))
        {
            result.Add(node);
            value = node.Parent;
        }
        result.Reverse();
        return result;
    }
}

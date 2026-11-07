namespace BlazorBlueprint.Primitives.Gantt;

/// <summary>
/// Turns a flat list of tasks into a chart: a tree, rolled-up summaries, a timeline, and the
/// arrows between the bars.
/// </summary>
/// <remarks>
/// The engine on its own, with no markup, so a plan can be drawn somewhere that is not a screen.
/// </remarks>
public static class GanttBuilder
{
    /// <summary>
    /// Builds the chart.
    /// </summary>
    /// <typeparam name="TItem">The type of the source items.</typeparam>
    /// <param name="source">Where the tasks come from and how the timeline is cut up.</param>
    /// <returns>The chart.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <exception cref="GanttBuildException">
    /// A task has no identifier, two tasks share an identifier, or the parent identifiers form a loop.
    /// </exception>
    public static GanttChart<TItem> Build<TItem>(GanttSource<TItem> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var nodes = Collect(source);
        var roots = Link(nodes, source);

        if (source.Order is { } order)
        {
            Sort(roots, nodes, order);
        }

        foreach (var root in roots)
        {
            RollUp(root, source.RollUpSummaries);
        }

        var axis = BuildAxis(nodes, source);
        var rows = Flatten(roots, source.Collapsed, axis);
        var links = Resolve(nodes, rows, source.Dependencies);

        return new GanttChart<TItem>(rows, axis, links, nodes.Count);
    }

    private static Dictionary<string, Node<TItem>> Collect<TItem>(GanttSource<TItem> source)
    {
        var nodes = new Dictionary<string, Node<TItem>>(StringComparer.Ordinal);

        foreach (var item in source.Items)
        {
            var id = source.Id(item);
            if (string.IsNullOrEmpty(id))
            {
                throw new GanttBuildException("Every task needs an identifier.", nameof(source), GanttBuildError.MissingId);
            }

            var start = source.Start(item);
            var end = source.End(item);

            var node = new Node<TItem>
            {
                Item = item,
                Id = id,
                ParentId = source.ParentId?.Invoke(item),
                Text = source.Text(item) ?? string.Empty,
                Start = start,
                // A task cannot finish before it starts; treating that as zero length draws a
                // milestone rather than a bar running backwards.
                End = end < start ? start : end,
                Progress = Math.Clamp(source.Progress?.Invoke(item) ?? 0, 0, 1),
            };

            if (!nodes.TryAdd(id, node))
            {
                throw new GanttBuildException($"Two tasks share the identifier '{id}'.", nameof(source), GanttBuildError.DuplicateId, id);
            }
        }

        return nodes;
    }

    private static List<Node<TItem>> Link<TItem>(Dictionary<string, Node<TItem>> nodes, GanttSource<TItem> source)
    {
        var roots = new List<Node<TItem>>();

        foreach (var node in nodes.Values)
        {
            // A parent that is not in the set is no parent: a filtered slice of a plan still draws.
            if (node.ParentId is { Length: > 0 } parentId && nodes.TryGetValue(parentId, out var parent) && parent != node)
            {
                node.Parent = parent;
                parent.Children.Add(node);
            }
            else
            {
                roots.Add(node);
            }
        }

        // Anything not reachable from a root is in a loop, which would hang the walk below.
        var reachable = 0;
        var stack = new Stack<Node<TItem>>(roots);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            reachable++;
            foreach (var child in node.Children)
            {
                stack.Push(child);
            }
        }

        if (reachable != nodes.Count)
        {
            throw new GanttBuildException("The parent identifiers form a loop.", nameof(source), GanttBuildError.ParentLoop);
        }

        return roots;
    }

    private static void Sort<TItem>(List<Node<TItem>> roots, Dictionary<string, Node<TItem>> nodes, IComparer<TItem> order)
    {
        // OrderBy rather than List.Sort, which is not stable: equal siblings have to stay in the
        // order they arrived, or re-sorting on a column full of ties shuffles the plan.
        Reorder(roots);
        foreach (var node in nodes.Values)
        {
            Reorder(node.Children);
        }

        void Reorder(List<Node<TItem>> siblings)
        {
            if (siblings.Count < 2)
            {
                return;
            }

            var sorted = siblings.OrderBy(n => n.Item, order).ToList();
            siblings.Clear();
            siblings.AddRange(sorted);
        }
    }

    private static void RollUp<TItem>(Node<TItem> node, bool rollUp)
    {
        foreach (var child in node.Children)
        {
            RollUp(child, rollUp);
        }

        if (node.Children.Count == 0 || !rollUp)
        {
            return;
        }

        node.Start = node.Children.Min(c => c.Start);
        node.End = node.Children.Max(c => c.End);

        // Weighted by length, so a fortnight half done outweighs an afternoon finished. With
        // nothing but milestones under it there is no length to weigh, so count them equally.
        double total = 0;
        double done = 0;
        foreach (var child in node.Children)
        {
            var length = (child.End - child.Start).TotalMinutes;
            total += length;
            done += child.Progress * length;
        }

        node.Progress = total > 0 ? done / total : node.Children.Average(c => c.Progress);
    }

    private static GanttTimeAxis BuildAxis<TItem>(Dictionary<string, Node<TItem>> nodes, GanttSource<TItem> source)
    {
        DateTimeOffset start;
        DateTimeOffset end;

        if (nodes.Count == 0)
        {
            start = source.RangeStart ?? DateTimeOffset.Now;
            end = source.RangeEnd ?? start;
        }
        else
        {
            start = source.RangeStart ?? nodes.Values.Min(n => n.Start);
            end = source.RangeEnd ?? nodes.Values.Max(n => n.End);
        }

        return GanttTimeAxis.Build(start, end, source.Zoom, source.Snap, source.NonWorkingDays, source.Labels);
    }

    private static List<GanttRow<TItem>> Flatten<TItem>(
        List<Node<TItem>> roots,
        IReadOnlySet<string>? collapsed,
        GanttTimeAxis axis)
    {
        var rows = new List<GanttRow<TItem>>();

        foreach (var root in roots)
        {
            Walk(root, 0);
        }

        return rows;

        void Walk(Node<TItem> node, int depth)
        {
            var expanded = collapsed is null || !collapsed.Contains(node.Id);
            var offset = axis.Position(node.Start);

            var row = new GanttRow<TItem>(node.Item, node.Id, node.ParentId, node.Text, depth)
            {
                Index = rows.Count,
                Start = node.Start,
                End = node.End,
                Progress = node.Progress,
                IsSummary = node.Children.Count > 0,
                IsMilestone = node.Children.Count == 0 && node.End == node.Start,
                IsExpanded = expanded,
                ChildCount = node.Children.Count,
                OffsetSlots = offset,
                LengthSlots = axis.Position(node.End) - offset,
            };

            node.Row = row;
            rows.Add(row);

            if (!expanded)
            {
                return;
            }

            foreach (var child in node.Children)
            {
                Walk(child, depth + 1);
            }
        }
    }

    private static List<GanttLink> Resolve<TItem>(
        Dictionary<string, Node<TItem>> nodes,
        List<GanttRow<TItem>> rows,
        IEnumerable<GanttDependency>? dependencies)
    {
        var links = new List<GanttLink>();
        if (dependencies is null)
        {
            return links;
        }

        var drawn = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            drawn.Add(row.Id);
        }

        foreach (var dependency in dependencies)
        {
            var from = Visible(dependency.FromId);
            var to = Visible(dependency.ToId);

            // Both ends folded into the same summary leaves an arrow from a bar to itself.
            if (from is null || to is null || ReferenceEquals(from, to))
            {
                continue;
            }

            var (fromAnchor, toAnchor) = dependency.Type switch
            {
                GanttDependencyType.StartToStart => (GanttAnchor.Start, GanttAnchor.Start),
                GanttDependencyType.FinishToFinish => (GanttAnchor.End, GanttAnchor.End),
                GanttDependencyType.StartToFinish => (GanttAnchor.Start, GanttAnchor.End),
                _ => (GanttAnchor.End, GanttAnchor.Start),
            };

            links.Add(new GanttLink(
                dependency,
                from.Index,
                to.Index,
                fromAnchor == GanttAnchor.Start ? from.OffsetSlots : from.OffsetSlots + from.LengthSlots,
                toAnchor == GanttAnchor.Start ? to.OffsetSlots : to.OffsetSlots + to.LengthSlots,
                fromAnchor,
                toAnchor));
        }

        return links;

        GanttRow<TItem>? Visible(string id)
        {
            // Walk up to whatever is still on screen, so folding a branch keeps the arrows that
            // crossed it rather than silently dropping them.
            var node = nodes.GetValueOrDefault(id);
            while (node is not null && !drawn.Contains(node.Id))
            {
                node = node.Parent;
            }

            return node?.Row;
        }
    }

    private sealed class Node<TItem>
    {
        public required TItem Item { get; init; }

        public required string Id { get; init; }

        public required string? ParentId { get; init; }

        public required string Text { get; init; }

        public DateTimeOffset Start { get; set; }

        public DateTimeOffset End { get; set; }

        public double Progress { get; set; }

        public Node<TItem>? Parent { get; set; }

        public List<Node<TItem>> Children { get; } = [];

        public GanttRow<TItem>? Row { get; set; }
    }
}

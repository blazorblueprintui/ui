using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

/// <summary>
/// An IDE-style docking container. Panels declared with <see cref="BbDockPanel"/> can be
/// dragged by their tabs and re-docked as tabs or splits, detached into floating windows,
/// maximized, closed and reopened — similar to the docking experience in Visual Studio,
/// VS Code or dockspawn.
/// </summary>
public partial class BbDock : ComponentBase, IAsyncDisposable
{
    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    /// <summary>
    /// The <see cref="BbDockPanel"/> declarations hosted by this dock.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Optional content shown when no panels are open.
    /// </summary>
    [Parameter]
    public RenderFragment? EmptyContent { get; set; }

    /// <summary>
    /// Additional CSS classes applied to the dock surface.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Invoked after the layout changes (docking, floating, closing, reopening or reordering).
    /// </summary>
    [Parameter]
    public EventCallback OnLayoutChanged { get; set; }

    /// <summary>
    /// Invoked when a panel is closed by the user, with the panel's identifier.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnPanelClosed { get; set; }

    private readonly Dictionary<string, BbDockPanel> panels = new();
    private readonly List<string> registrationOrder = new();
    private readonly HashSet<string> closedPanels = new();
    private readonly HashSet<string> pinnedPanels = new();
    private readonly List<DockFloatingWindow> floatingWindows = new();

    private DockNode? layout;
    private string? maximizedGroupId;

    private ElementReference rootRef;
    private readonly string dockId = Guid.NewGuid().ToString("N");
    private IJSObjectReference? jsModule;
    private DotNetObjectReference<BbDock>? dotNetRef;
    private bool jsInitialized;
    private bool layoutBuilt;
    private bool disposed;

    private string? draggingPanelId;

    private DockTabGroupNode? MaximizedGroup =>
        maximizedGroupId is null ? null : FindGroup(maximizedGroupId);

    private string CssClass => ClassNames.cn(
        "relative h-full w-full select-none overflow-hidden bg-muted/20 text-foreground",
        Class);

    // ----------------------------------------------------------------- Registration

    internal void RegisterPanel(BbDockPanel panel)
    {
        panels[panel.Id] = panel;
        if (!registrationOrder.Contains(panel.Id))
        {
            registrationOrder.Add(panel.Id);
        }

        // A panel added after the initial layout was built (e.g. conditionally rendered)
        // is docked into a sensible default location.
        if (layoutBuilt && !closedPanels.Contains(panel.Id) && FindGroupContaining(panel.Id) is null)
        {
            AddPanelToDefaultLocation(panel.Id);
            InvokeAsync(StateHasChanged);
        }
    }

    internal void UnregisterPanel(BbDockPanel panel)
    {
        if (disposed)
        {
            return;
        }

        panels.Remove(panel.Id);
        registrationOrder.Remove(panel.Id);

        if (FindGroupContaining(panel.Id) is not null)
        {
            RemovePanelFromLayout(panel.Id);
            InvokeAsync(StateHasChanged);
        }
    }

    internal BbDockPanel? GetPanel(string id) => panels.TryGetValue(id, out var p) ? p : null;

    // ----------------------------------------------------------------- Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            BuildInitialLayout();
            layoutBuilt = true;
            await InitializeJsAsync();
            StateHasChanged();
        }
    }

    private async Task InitializeJsAsync()
    {
        try
        {
            jsModule = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorBlueprint.Components/js/dock.js");
            dotNetRef = DotNetObjectReference.Create(this);
            await jsModule.InvokeVoidAsync("initializeDock", dockId, rootRef, dotNetRef);
            jsInitialized = true;
        }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
        {
            // Expected during circuit disconnect / prerender.
        }
        catch (InvalidOperationException)
        {
            // JS interop not available during prerendering.
        }
    }

    // ----------------------------------------------------------------- Initial layout

    private void BuildInitialLayout()
    {
        DockTabGroupNode? MakeGroup(DockZone region)
        {
            var ids = registrationOrder
                .Where(id => panels.TryGetValue(id, out var p)
                             && !closedPanels.Contains(id)
                             && p.Region == region)
                .OrderBy(id => panels[id].Order)
                .ToList();

            if (ids.Count == 0)
            {
                return null;
            }

            var group = new DockTabGroupNode { PanelIds = ids };
            group.EnsureActive();
            return group;
        }

        var left = MakeGroup(DockZone.Left);
        var right = MakeGroup(DockZone.Right);
        var top = MakeGroup(DockZone.Top);
        var center = MakeGroup(DockZone.Center);
        var bottom = MakeGroup(DockZone.Bottom);

        var middleParts = new List<(DockNode Node, double Weight)>();
        if (top is not null)
        {
            middleParts.Add((top, 25));
        }
        if (center is not null)
        {
            middleParts.Add((center, 50));
        }
        if (bottom is not null)
        {
            middleParts.Add((bottom, 25));
        }

        var middle = Combine(middleParts, DockOrientation.Vertical);

        var rowParts = new List<(DockNode Node, double Weight)>();
        if (left is not null)
        {
            rowParts.Add((left, 20));
        }
        if (middle is not null)
        {
            rowParts.Add((middle, 60));
        }
        if (right is not null)
        {
            rowParts.Add((right, 20));
        }

        layout = Combine(rowParts, DockOrientation.Horizontal);
    }

    private static DockNode? Combine(List<(DockNode Node, double Weight)> parts, DockOrientation orientation)
    {
        if (parts.Count == 0)
        {
            return null;
        }

        if (parts.Count == 1)
        {
            return parts[0].Node;
        }

        var split = new DockSplitNode
        {
            Orientation = orientation,
            Children = parts.Select(p => p.Node).ToList(),
            Sizes = parts.Select(p => p.Weight).ToList()
        };
        split.NormalizeSizes();
        return split;
    }

    // ----------------------------------------------------------------- Tree traversal

    private IEnumerable<DockTabGroupNode> AllGroups()
    {
        if (layout is not null)
        {
            foreach (var g in GroupsIn(layout))
            {
                yield return g;
            }
        }

        foreach (var win in floatingWindows)
        {
            yield return win.Group;
        }
    }

    private static IEnumerable<DockTabGroupNode> GroupsIn(DockNode node)
    {
        switch (node)
        {
            case DockTabGroupNode group:
                yield return group;
                break;
            case DockSplitNode split:
                foreach (var child in split.Children)
                {
                    foreach (var g in GroupsIn(child))
                    {
                        yield return g;
                    }
                }
                break;
        }
    }

    private DockTabGroupNode? FindGroup(string id) => AllGroups().FirstOrDefault(g => g.Id == id);

    private DockTabGroupNode? FindGroupContaining(string panelId) =>
        AllGroups().FirstOrDefault(g => g.PanelIds.Contains(panelId));

    private DockSplitNode? FindParent(DockNode node)
    {
        if (layout is null)
        {
            return null;
        }

        return FindParentIn(layout, node);
    }

    private static DockSplitNode? FindParentIn(DockNode current, DockNode target)
    {
        if (current is DockSplitNode split)
        {
            if (split.Children.Contains(target))
            {
                return split;
            }

            foreach (var child in split.Children)
            {
                var found = FindParentIn(child, target);
                if (found is not null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    private void ReplaceInParent(DockNode oldNode, DockNode newNode)
    {
        if (ReferenceEquals(layout, oldNode))
        {
            layout = newNode;
            return;
        }

        var parent = FindParent(oldNode);
        if (parent is null)
        {
            return;
        }

        var index = parent.Children.IndexOf(oldNode);
        if (index >= 0)
        {
            parent.Children[index] = newNode;
        }
    }

    // ----------------------------------------------------------------- Removal / pruning

    private void RemovePanelFromLayout(string panelId)
    {
        var group = FindGroupContaining(panelId);
        if (group is null)
        {
            return;
        }

        group.PanelIds.Remove(panelId);
        group.EnsureActive();

        if (group.PanelIds.Count == 0)
        {
            RemoveEmptyGroup(group);
        }
    }

    private void RemoveEmptyGroup(DockTabGroupNode group)
    {
        if (maximizedGroupId == group.Id)
        {
            maximizedGroupId = null;
        }

        var win = floatingWindows.FirstOrDefault(w => ReferenceEquals(w.Group, group));
        if (win is not null)
        {
            floatingWindows.Remove(win);
            return;
        }

        if (ReferenceEquals(layout, group))
        {
            layout = null;
            return;
        }

        var parent = FindParent(group);
        if (parent is null)
        {
            return;
        }

        parent.Children.Remove(group);
        CollapseSplit(parent);
    }

    private void CollapseSplit(DockSplitNode split)
    {
        if (split.Children.Count == 1)
        {
            var only = split.Children[0];
            ReplaceInParent(split, only);
        }
        else if (split.Children.Count == 0)
        {
            if (ReferenceEquals(layout, split))
            {
                layout = null;
            }
            else
            {
                var parent = FindParent(split);
                if (parent is not null)
                {
                    parent.Children.Remove(split);
                    CollapseSplit(parent);
                }
            }
        }
        else
        {
            split.NormalizeSizes();
        }
    }

    // ----------------------------------------------------------------- Drop / docking

    private void SplitAroundTarget(DockTabGroupNode target, DockNode newNode, DockZone zone)
    {
        var orientation = zone is DockZone.Left or DockZone.Right
            ? DockOrientation.Horizontal
            : DockOrientation.Vertical;
        var newFirst = zone is DockZone.Left or DockZone.Top;

        var split = new DockSplitNode
        {
            Orientation = orientation,
            Children = newFirst ? new() { newNode, target } : new() { target, newNode },
            Sizes = new() { 50, 50 }
        };

        ReplaceInParent(target, split);
    }

    private void SplitAtRoot(DockNode newNode, DockZone zone)
    {
        if (layout is null)
        {
            layout = newNode;
            return;
        }

        var orientation = zone is DockZone.Left or DockZone.Right
            ? DockOrientation.Horizontal
            : DockOrientation.Vertical;
        var newFirst = zone is DockZone.Left or DockZone.Top;

        var split = new DockSplitNode
        {
            Orientation = orientation,
            Children = newFirst ? new() { newNode, layout } : new() { layout, newNode },
            Sizes = newFirst ? new() { 28, 72 } : new() { 72, 28 }
        };
        layout = split;
    }

    private void ReorderPanel(string panelId, DockTabGroupNode target, int index)
    {
        var source = FindGroupContaining(panelId);
        if (source is null)
        {
            return;
        }

        if (ReferenceEquals(source, target))
        {
            // Move within the same group: account for the gap left by removing the panel.
            var old = target.PanelIds.IndexOf(panelId);
            if (old < 0)
            {
                return;
            }

            target.PanelIds.RemoveAt(old);
            var insert = index;
            if (old < insert)
            {
                insert--;
            }
            insert = Math.Clamp(insert, 0, target.PanelIds.Count);
            target.PanelIds.Insert(insert, panelId);
        }
        else
        {
            // Move into another group's strip at a precise slot.
            RemovePanelFromLayout(panelId);
            var insert = Math.Clamp(index, 0, target.PanelIds.Count);
            target.PanelIds.Insert(insert, panelId);
        }

        // Pinned tabs occupy the front of the strip: a pinned tab cannot be reordered
        // behind an unpinned one, and an unpinned tab cannot jump ahead of a pinned one.
        // The drop index above is honoured within whichever region the tab belongs to.
        EnforcePinOrder(target);

        target.ActivePanelId = panelId;
    }

    // ----------------------------------------------------------------- Pinning

    internal bool IsPinned(string panelId) => pinnedPanels.Contains(panelId);

    /// <summary>
    /// Stable-partitions a group's tabs so pinned tabs sit at the front in pin order,
    /// followed by unpinned tabs in their existing order.
    /// </summary>
    private void EnforcePinOrder(DockTabGroupNode group)
    {
        if (group.PanelIds.Count < 2)
        {
            return;
        }

        var pinnedHere = group.PanelIds.Where(pinnedPanels.Contains).ToList();
        if (pinnedHere.Count == 0)
        {
            return;
        }

        var unpinnedHere = group.PanelIds.Where(id => !pinnedPanels.Contains(id));
        group.PanelIds = pinnedHere.Concat(unpinnedHere).ToList();
    }

    /// <summary>
    /// Toggles the pinned state of a panel. Pinning moves the tab to the back of the pinned
    /// queue at the front of its group; unpinning returns it to the front of the unpinned tabs.
    /// </summary>
    internal async Task TogglePinAsync(string panelId)
    {
        var group = FindGroupContaining(panelId);
        if (group is null)
        {
            return;
        }

        if (!pinnedPanels.Remove(panelId))
        {
            pinnedPanels.Add(panelId);
        }

        EnforcePinOrder(group);
        await AfterMutateAsync();
    }

    private void AddPanelToDefaultLocation(string panelId)
    {
        var existing = AllGroups().FirstOrDefault();
        if (existing is not null)
        {
            existing.PanelIds.Add(panelId);
            EnforcePinOrder(existing);
            existing.ActivePanelId = panelId;
        }
        else
        {
            var group = new DockTabGroupNode { PanelIds = { panelId }, ActivePanelId = panelId };
            layout = group;
        }
    }

    [JSInvokable]
    public async Task OnTabDropped(string targetType, string? groupId, string zone, double floatX, double floatY, int index)
    {
        if (disposed)
        {
            return;
        }

        var panelId = draggingPanelId;
        draggingPanelId = null;
        if (panelId is null || !panels.TryGetValue(panelId, out var panel))
        {
            return;
        }

        // Re-validate server-side: JS input cannot move a locked panel regardless of what
        // drop target it reports. The float case additionally requires CanDetach below.
        if (!panel.CanMove)
        {
            return;
        }

        var dropZone = ParseZone(zone);
        maximizedGroupId = null;

        switch (targetType)
        {
            case "reorder":
            {
                var target = groupId is null ? null : FindGroup(groupId);
                if (target is null)
                {
                    return;
                }

                // Reordering a lone panel within its own group is a no-op.
                if (target.PanelIds.Count == 1 && target.PanelIds[0] == panelId)
                {
                    target.ActivePanelId = panelId;
                    break;
                }

                ReorderPanel(panelId, target, index);
                break;
            }

            case "group":
            {
                var target = groupId is null ? null : FindGroup(groupId);
                if (target is null)
                {
                    return;
                }

                // Dropping a single-panel group onto itself is a no-op.
                if (target.PanelIds.Count == 1 && target.PanelIds[0] == panelId)
                {
                    target.ActivePanelId = panelId;
                    break;
                }

                if (dropZone == DockZone.Center)
                {
                    if (!target.PanelIds.Contains(panelId))
                    {
                        RemovePanelFromLayout(panelId);
                        target.PanelIds.Add(panelId);
                        EnforcePinOrder(target);
                    }
                    target.ActivePanelId = panelId;
                }
                else
                {
                    RemovePanelFromLayout(panelId);
                    var newGroup = new DockTabGroupNode { PanelIds = { panelId }, ActivePanelId = panelId };
                    SplitAroundTarget(target, newGroup, dropZone);
                }
                break;
            }

            case "root":
            {
                RemovePanelFromLayout(panelId);
                var newGroup = new DockTabGroupNode { PanelIds = { panelId }, ActivePanelId = panelId };
                SplitAtRoot(newGroup, dropZone);
                break;
            }

            case "float":
            {
                // A locked or non-floatable panel cannot be detached: leave it where it is.
                if (!panel.CanDetach)
                {
                    return;
                }

                RemovePanelFromLayout(panelId);
                var floatGroup = new DockTabGroupNode { PanelIds = { panelId }, ActivePanelId = panelId };
                var (minW, minH, maxW, maxH) = GroupSizeBounds(floatGroup);
                floatingWindows.Add(new DockFloatingWindow
                {
                    Group = floatGroup,
                    X = Math.Max(0, floatX),
                    Y = Math.Max(0, floatY),
                    Width = Math.Clamp(DockFloatingWindow.DefaultWidth, minW, maxW),
                    Height = Math.Clamp(DockFloatingWindow.DefaultHeight, minH, maxH)
                });
                break;
            }

            default:
                return;
        }

        await AfterMutateAsync();
    }

    // ----------------------------------------------------------------- Group/tab interactions

    internal void ActivatePanel(DockTabGroupNode group, string panelId)
    {
        group.ActivePanelId = panelId;
        StateHasChanged();
    }

    internal async Task ClosePanelAsync(string panelId)
    {
        if (!panels.TryGetValue(panelId, out var panel) || !panel.CanClose)
        {
            return;
        }

        RemovePanelFromLayout(panelId);
        pinnedPanels.Remove(panelId);
        closedPanels.Add(panelId);

        if (OnPanelClosed.HasDelegate)
        {
            await OnPanelClosed.InvokeAsync(panelId);
        }

        await AfterMutateAsync();
    }

    /// <summary>
    /// Closes every closable panel that shares a tab group with the given panel.
    /// </summary>
    internal Task CloseAllPanelsInGroupAsync(string panelId)
    {
        var group = FindGroupContaining(panelId);
        return group is null
            ? Task.CompletedTask
            : CloseManyAsync(group.PanelIds.ToList());
    }

    /// <summary>
    /// Closes every closable, unpinned panel that shares a tab group with the given panel.
    /// </summary>
    internal Task CloseAllButPinnedPanelsInGroupAsync(string panelId)
    {
        var group = FindGroupContaining(panelId);
        return group is null
            ? Task.CompletedTask
            : CloseManyAsync(group.PanelIds.Where(id => !pinnedPanels.Contains(id)).ToList());
    }

    /// <summary>
    /// Closes every closable panel in the given panel's tab group except the panel itself.
    /// </summary>
    internal Task CloseOtherPanelsInGroupAsync(string panelId)
    {
        var group = FindGroupContaining(panelId);
        return group is null
            ? Task.CompletedTask
            : CloseManyAsync(group.PanelIds.Where(id => id != panelId).ToList());
    }

    private async Task CloseManyAsync(IReadOnlyList<string> panelIds)
    {
        var closedAny = false;

        foreach (var id in panelIds)
        {
            if (!panels.TryGetValue(id, out var panel) || !panel.CanClose)
            {
                continue;
            }

            RemovePanelFromLayout(id);
            pinnedPanels.Remove(id);
            closedPanels.Add(id);
            closedAny = true;

            if (OnPanelClosed.HasDelegate)
            {
                await OnPanelClosed.InvokeAsync(id);
            }
        }

        if (closedAny)
        {
            await AfterMutateAsync();
        }
    }

    /// <summary>
    /// Reopens a previously closed panel, docking it into a sensible default location.
    /// </summary>
    /// <param name="panelId">The identifier of the panel to reopen.</param>
    public async Task OpenPanelAsync(string panelId)
    {
        if (!panels.ContainsKey(panelId))
        {
            return;
        }

        closedPanels.Remove(panelId);

        if (FindGroupContaining(panelId) is null)
        {
            AddPanelToDefaultLocation(panelId);
        }

        maximizedGroupId = null;
        await AfterMutateAsync();
    }

    /// <summary>
    /// Gets the identifiers of panels that are currently closed and can be reopened.
    /// </summary>
    /// <returns>The closed panel identifiers in registration order.</returns>
    public IReadOnlyList<string> GetClosedPanelIds() =>
        registrationOrder.Where(id => closedPanels.Contains(id)).ToList();

    internal bool IsMaximized(DockTabGroupNode group) => maximizedGroupId == group.Id;

    internal void ToggleMaximize(DockTabGroupNode group)
    {
        maximizedGroupId = maximizedGroupId == group.Id ? null : group.Id;
        StateHasChanged();
    }

    internal async Task StartTabDragAsync(string panelId, PointerEventArgs e)
    {
        draggingPanelId = panelId;
        var title = GetPanel(panelId)?.Title ?? string.Empty;

        if (jsModule is not null && jsInitialized)
        {
            await jsModule.InvokeVoidAsync("startTabDrag", dockId, title, e.ClientX, e.ClientY, e.PointerId);
        }
    }

    internal async Task StartWindowDragAsync(string windowId, PointerEventArgs e)
    {
        if (jsModule is not null && jsInitialized)
        {
            await jsModule.InvokeVoidAsync("startWindowDrag", dockId, windowId, e.ClientX, e.ClientY, e.PointerId);
        }
    }

    private async Task StartWindowResize(string windowId, PointerEventArgs e)
    {
        if (jsModule is null || !jsInitialized)
        {
            return;
        }

        var win = floatingWindows.FirstOrDefault(w => w.Id == windowId);
        if (win is null)
        {
            return;
        }

        // Feed the live JS resize the same bounds we enforce server-side so the grip stops at
        // the limit instead of snapping back on release. A max of 0 means "no maximum".
        var (minW, minH, maxW, maxH) = GroupSizeBounds(win.Group);
        var maxWArg = double.IsPositiveInfinity(maxW) ? 0 : maxW;
        var maxHArg = double.IsPositiveInfinity(maxH) ? 0 : maxH;

        await jsModule.InvokeVoidAsync(
            "startWindowResize", dockId, windowId, e.ClientX, e.ClientY, e.PointerId, minW, minH, maxWArg, maxHArg);
    }

    [JSInvokable]
    public void OnWindowMoved(string windowId, double x, double y)
    {
        if (disposed)
        {
            return;
        }

        var win = floatingWindows.FirstOrDefault(w => w.Id == windowId);
        if (win is not null)
        {
            win.X = Math.Max(0, x);
            win.Y = Math.Max(0, y);
            StateHasChanged();
        }
    }

    [JSInvokable]
    public void OnWindowResized(string windowId, double width, double height)
    {
        if (disposed)
        {
            return;
        }

        var win = floatingWindows.FirstOrDefault(w => w.Id == windowId);
        if (win is not null)
        {
            var (minW, minH, maxW, maxH) = GroupSizeBounds(win.Group);
            win.Width = Math.Clamp(width, minW, maxW);
            win.Height = Math.Clamp(height, minH, maxH);
            StateHasChanged();
        }
    }

    private async Task AfterMutateAsync()
    {
        foreach (var group in AllGroups())
        {
            group.EnsureActive();
        }

        StateHasChanged();

        if (OnLayoutChanged.HasDelegate)
        {
            await OnLayoutChanged.InvokeAsync();
        }
    }

    private static DockZone ParseZone(string zone) => zone switch
    {
        "left" => DockZone.Left,
        "right" => DockZone.Right,
        "top" => DockZone.Top,
        "bottom" => DockZone.Bottom,
        _ => DockZone.Center
    };

    private string FloatingStyle(DockFloatingWindow win)
    {
        var (minW, minH, maxW, maxH) = GroupSizeBounds(win.Group);
        var x = win.X.ToString("F0", CultureInfo.InvariantCulture);
        var y = win.Y.ToString("F0", CultureInfo.InvariantCulture);
        var w = Math.Clamp(win.Width, minW, maxW).ToString("F0", CultureInfo.InvariantCulture);
        var h = Math.Clamp(win.Height, minH, maxH).ToString("F0", CultureInfo.InvariantCulture);

        var style = $"left:{x}px;top:{y}px;width:{w}px;height:{h}px;" +
                    $"min-width:{minW.ToString("F0", CultureInfo.InvariantCulture)}px;" +
                    $"min-height:{minH.ToString("F0", CultureInfo.InvariantCulture)}px;";
        if (!double.IsPositiveInfinity(maxW))
        {
            style += $"max-width:{maxW.ToString("F0", CultureInfo.InvariantCulture)}px;";
        }
        if (!double.IsPositiveInfinity(maxH))
        {
            style += $"max-height:{maxH.ToString("F0", CultureInfo.InvariantCulture)}px;";
        }
        return style;
    }

    /// <summary>
    /// Computes the effective pixel size bounds for a floating window hosting the given group.
    /// Mins are the largest of the panels' minimums (floored at the usable window minimum); maxes
    /// are the smallest of the panels' maximums, reconciled so that <c>max &gt;= min</c>.
    /// </summary>
    private (double MinW, double MinH, double MaxW, double MaxH) GroupSizeBounds(DockTabGroupNode group)
    {
        var minW = DockFloatingWindow.MinFloatingWidth;
        var minH = DockFloatingWindow.MinFloatingHeight;
        var maxW = double.PositiveInfinity;
        var maxH = double.PositiveInfinity;

        foreach (var id in group.PanelIds)
        {
            var panel = GetPanel(id);
            if (panel is null)
            {
                continue;
            }

            if (panel.MinWidth is int pMinW)
            {
                minW = Math.Max(minW, pMinW);
            }
            if (panel.MinHeight is int pMinH)
            {
                minH = Math.Max(minH, pMinH);
            }
            if (panel.MaxWidth is int pMaxW)
            {
                maxW = Math.Min(maxW, pMaxW);
            }
            if (panel.MaxHeight is int pMaxH)
            {
                maxH = Math.Min(maxH, pMaxH);
            }
        }

        // A minimum larger than the requested maximum wins, so the window stays usable.
        maxW = Math.Max(maxW, minW);
        maxH = Math.Max(maxH, minH);
        return (minW, minH, maxW, maxH);
    }

    /// <summary>
    /// Builds the inline CSS min/max size constraints for a docked tab group from the panels it
    /// hosts, or <c>null</c> when no panel in the group declares any size constraint.
    /// </summary>
    internal string? GroupConstraintStyle(DockTabGroupNode group)
    {
        double minW = 0, minH = 0;
        var maxW = double.PositiveInfinity;
        var maxH = double.PositiveInfinity;

        foreach (var id in group.PanelIds)
        {
            var panel = GetPanel(id);
            if (panel is null)
            {
                continue;
            }

            if (panel.MinWidth is int pMinW)
            {
                minW = Math.Max(minW, pMinW);
            }
            if (panel.MinHeight is int pMinH)
            {
                minH = Math.Max(minH, pMinH);
            }
            if (panel.MaxWidth is int pMaxW)
            {
                maxW = Math.Min(maxW, pMaxW);
            }
            if (panel.MaxHeight is int pMaxH)
            {
                maxH = Math.Min(maxH, pMaxH);
            }
        }

        var parts = new List<string>();
        if (minW > 0)
        {
            parts.Add($"min-width:{minW.ToString("F0", CultureInfo.InvariantCulture)}px");
        }
        if (minH > 0)
        {
            parts.Add($"min-height:{minH.ToString("F0", CultureInfo.InvariantCulture)}px");
        }
        if (!double.IsPositiveInfinity(maxW))
        {
            parts.Add($"max-width:{Math.Max(maxW, minW).ToString("F0", CultureInfo.InvariantCulture)}px");
        }
        if (!double.IsPositiveInfinity(maxH))
        {
            parts.Add($"max-height:{Math.Max(maxH, minH).ToString("F0", CultureInfo.InvariantCulture)}px");
        }

        return parts.Count > 0 ? string.Join(';', parts) + ";" : null;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        disposed = true;

        if (jsModule is not null && jsInitialized)
        {
            try
            {
                await jsModule.InvokeVoidAsync("disposeDock", dockId);
            }
            catch
            {
                // Ignore — circuit may already be gone.
            }

            try
            {
                await jsModule.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect.
            }
        }

        dotNetRef?.Dispose();
    }
}

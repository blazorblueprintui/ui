using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

/// <summary>
/// Renders a single dock tab group: a tab strip plus the active panel's content.
/// Used internally by <see cref="BbDock"/>; not intended for direct consumption.
/// </summary>
public partial class BbDockTabGroup : ComponentBase, IAsyncDisposable
{
    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    [CascadingParameter]
    private BbDock Dock { get; set; } = null!;

    /// <summary>
    /// The tab group node to render. Typed as <see cref="object"/> so the internal layout
    /// model stays out of the public API; it is always an internal tab group node.
    /// </summary>
    [Parameter, EditorRequired]
    public object Group { get; set; } = null!;

    /// <summary>
    /// When set, this group is the root of a floating window with the given identifier.
    /// The tab strip then acts as the window's drag handle.
    /// </summary>
    [Parameter]
    public string? FloatingWindowId { get; set; }

    private DockTabGroupNode GroupNode => (DockTabGroupNode)Group;

    private BbContextMenu? tabMenu;
    private BbDockPanel? contextPanel;

    private readonly Dictionary<string, ElementReference> tabRefs = new();
    private ElementReference stripRef;
    private IJSObjectReference? jsModule;
    private DotNetObjectReference<BbDockTabGroup>? dotNetRef;
    private string? observedGroupId;
    private string? lastTabSignature;
    private List<string> overflowPanelIds = new();
    private bool disposed;

    private bool IsFloating => FloatingWindowId is not null;

    private bool IsMaximized => Dock is not null && Dock.IsMaximized(GroupNode);

    // Changes whenever the set, order or active state of tabs changes, so the strip is
    // re-measured for overflow even when its own size did not change.
    private string TabSignature => $"{string.Join('|', GroupNode.PanelIds)}#{GroupNode.ActivePanelId}";

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (Dock is null)
        {
            throw new InvalidOperationException($"{nameof(BbDockTabGroup)} must be used within a {nameof(BbDock)}.");
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (disposed)
        {
            return;
        }

        try
        {
            jsModule ??= await JS.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorBlueprint.Components/js/dock.js");

            // (Re)attach the overflow observer whenever this instance starts rendering a
            // different tab group (Blazor reuses component instances across layout changes).
            if (observedGroupId != GroupNode.Id)
            {
                if (observedGroupId is not null)
                {
                    await jsModule.InvokeVoidAsync("disposeTabOverflow", observedGroupId);
                }

                dotNetRef ??= DotNetObjectReference.Create(this);
                observedGroupId = GroupNode.Id;
                lastTabSignature = TabSignature;
                await jsModule.InvokeVoidAsync("initTabOverflow", GroupNode.Id, stripRef, dotNetRef);
            }
            else if (lastTabSignature != TabSignature)
            {
                // Tabs were added, removed or reordered: re-measure without a size change.
                lastTabSignature = TabSignature;
                await jsModule.InvokeVoidAsync("remeasureTabOverflow", GroupNode.Id);
            }
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

    /// <summary>
    /// Invoked from JS with the ids of tabs that no longer fit in the strip. These tabs are
    /// hidden and surfaced through the overflow dropdown instead.
    /// </summary>
    /// <param name="ids">The overflowing panel ids, in strip order.</param>
    [JSInvokable]
    public void OnTabOverflowChanged(string[] ids)
    {
        if (disposed)
        {
            return;
        }

        var next = ids.Where(id => GroupNode.PanelIds.Contains(id)).ToList();
        if (!next.SequenceEqual(overflowPanelIds))
        {
            overflowPanelIds = next;
            StateHasChanged();
        }
    }

    // Stable DOM ids so each tab and its panel can be linked with aria-controls /
    // aria-labelledby per the WAI-ARIA tabs pattern.
    private string TabDomId(string panelId) => $"bb-dock-tab-{GroupNode.Id}-{panelId}";

    private string PanelDomId(string panelId) => $"bb-dock-panel-{GroupNode.Id}-{panelId}";

    // The tabs currently reachable with the keyboard, in strip order. Overflowed tabs are
    // display:none and surfaced through the overflow dropdown instead, so they are skipped.
    private List<string> VisibleTabIds() =>
        GroupNode.PanelIds
            .Where(id => !overflowPanelIds.Contains(id) && Dock.GetPanel(id) is not null)
            .ToList();

    private async Task HandleTabKeyDown(BbDockPanel panel, KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "Enter" or " ":
                Dock.ActivatePanel(GroupNode, panel.Id);
                break;

            case "ArrowLeft":
                await FocusAdjacentTabAsync(panel.Id, -1);
                break;

            case "ArrowRight":
                await FocusAdjacentTabAsync(panel.Id, 1);
                break;

            case "Home":
            {
                var visible = VisibleTabIds();
                if (visible.Count > 0)
                {
                    await FocusTabAsync(visible[0]);
                }
                break;
            }

            case "End":
            {
                var visible = VisibleTabIds();
                if (visible.Count > 0)
                {
                    await FocusTabAsync(visible[^1]);
                }
                break;
            }
        }
    }

    private async Task FocusAdjacentTabAsync(string panelId, int direction)
    {
        var visible = VisibleTabIds();
        var current = visible.IndexOf(panelId);
        if (current < 0 || visible.Count < 2)
        {
            return;
        }

        var next = (current + direction + visible.Count) % visible.Count;
        await FocusTabAsync(visible[next]);
    }

    private async Task FocusTabAsync(string panelId)
    {
        if (!tabRefs.TryGetValue(panelId, out var el))
        {
            return;
        }

        try
        {
            await el.FocusAsync();
        }
        catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException)
        {
            // Element may not be rendered yet or the circuit disconnected.
        }
    }

    private async Task HandleTabPointerDown(BbDockPanel panel, PointerEventArgs e)
    {
        // Primary button only; let the click handler perform activation.
        if (e.Button != 0)
        {
            return;
        }

        // A locked panel cannot be dragged to move, reorder, re-dock or float.
        if (!panel.CanMove)
        {
            return;
        }

        await Dock.StartTabDragAsync(panel.Id, e);
    }

    private async Task HandleTabContextMenu(BbDockPanel panel, MouseEventArgs e)
    {
        contextPanel = panel;

        if (tabMenu is not null)
        {
            await tabMenu.OpenAt(e.ClientX, e.ClientY);
        }
    }

    private async Task HandleStripPointerDown(PointerEventArgs e)
    {
        if (IsFloating && e.Button == 0 && FloatingWindowId is not null)
        {
            await Dock.StartWindowDragAsync(FloatingWindowId, e);
        }
    }

    private string RootClass => ClassNames.cn(
        "flex h-full w-full min-w-0 flex-col overflow-hidden bg-background",
        // Floating windows already have a bordered wrapper; docked groups draw their own
        // outline so adjacent panels read as distinct, framed surfaces (VS-style).
        IsFloating ? null : "border border-border/70");

    // Pixel min/max size constraints declared by the group's panels. The floating window wrapper
    // already sizes itself from these, so the constraint style only applies to docked groups.
    private string? ConstraintStyle => IsFloating ? null : Dock.GroupConstraintStyle(GroupNode);

    private string StripClass => ClassNames.cn(
        "flex h-8 shrink-0 items-stretch border-b border-border/60 bg-muted/50",
        IsFloating ? "cursor-move" : null);

    private static string TabClass(bool isActive, bool isHidden) => ClassNames.cn(
        "group/tab relative flex h-full min-w-[88px] max-w-[200px] cursor-default items-center gap-1.5 border-r border-border/40 px-2.5 text-xs transition-colors",
        isHidden ? "hidden" : null,
        isActive
            ? "z-10 -mb-px border-b border-background bg-background text-foreground after:absolute after:inset-x-0 after:top-0 after:h-[2px] after:bg-primary"
            : "bg-transparent text-muted-foreground hover:bg-background/50 hover:text-foreground");

    private static string CloseClass() => ClassNames.cn(
        "ml-auto inline-flex h-4 w-4 shrink-0 items-center justify-center rounded-sm opacity-60 transition-opacity hover:bg-foreground/10 hover:!opacity-100");

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        disposed = true;

        if (jsModule is not null)
        {
            try
            {
                if (observedGroupId is not null)
                {
                    await jsModule.InvokeVoidAsync("disposeTabOverflow", observedGroupId);
                }

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

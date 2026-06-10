using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorBlueprint.Components;

/// <summary>
/// Renders a single dock tab group: a tab strip plus the active panel's content.
/// Used internally by <see cref="BbDock"/>; not intended for direct consumption.
/// </summary>
public partial class BbDockTabGroup : ComponentBase
{
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

    private DockTabGroupNode group => (DockTabGroupNode)Group;

    private BbContextMenu? tabMenu;
    private BbDockPanel? contextPanel;

    private bool IsFloating => FloatingWindowId is not null;

    private bool IsMaximized => Dock is not null && Dock.IsMaximized(group);

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (Dock is null)
        {
            throw new InvalidOperationException($"{nameof(BbDockTabGroup)} must be used within a {nameof(BbDock)}.");
        }
    }

    private async Task HandleTabPointerDown(BbDockPanel panel, PointerEventArgs e)
    {
        // Primary button only; let the click handler perform activation.
        if (e.Button != 0)
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

    private string StripClass => ClassNames.cn(
        "flex h-8 shrink-0 items-stretch border-b border-border/60 bg-muted/50",
        IsFloating ? "cursor-move" : null);

    private static string TabClass(bool isActive) => ClassNames.cn(
        "group/tab relative flex h-full min-w-[88px] max-w-[200px] cursor-default items-center gap-1.5 border-r border-border/40 px-2.5 text-xs transition-colors",
        isActive
            ? "z-10 -mb-px border-b border-background bg-background text-foreground after:absolute after:inset-x-0 after:top-0 after:h-[2px] after:bg-primary"
            : "bg-transparent text-muted-foreground hover:bg-background/50 hover:text-foreground");

    private static string CloseClass(bool isActive) => ClassNames.cn(
        "ml-auto inline-flex h-4 w-4 shrink-0 items-center justify-center rounded-sm transition-opacity hover:bg-foreground/10 hover:!opacity-100",
        isActive ? "opacity-60" : "opacity-0 group-hover/tab:opacity-60");
}

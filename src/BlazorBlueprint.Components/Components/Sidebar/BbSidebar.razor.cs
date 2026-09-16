using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.Sheet;
using Microsoft.AspNetCore.Components;
using System;

namespace BlazorBlueprint.Components;

public partial class BbSidebar : IDisposable
{
    [CascadingParameter]
    private SidebarContext? Context { get; set; }

    private SidebarContext? _subscribedContext;

    /// <summary>
    /// The content to render inside the sidebar.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the sidebar.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Collapsible behavior: icon-only when collapsed, full width when expanded.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool Collapsible { get; set; } = true;

    /// <summary>
    /// Additional attributes to apply to the sidebar element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private bool MobileOpen
    {
        get => Context?.OpenMobile ?? false;
        set => Context?.SetOpenMobile(value);
    }

    private SheetSide GetSheetSide()
    {
        return Context?.Side == SidebarSide.Right
            ? SheetSide.Right
            : SheetSide.Left;
    }

    private string GetDesktopClasses()
    {
        // hidden md:flex prevents the desktop sidebar from flashing on mobile screens
        // before JS detects the breakpoint and switches to the Sheet renderer.
        var baseClasses = "bb:group bb:peer bb:hidden bb:md:flex bb:flex-col bb:text-sidebar-foreground bb:shrink-0";

        // Variant-specific classes
        var variantClasses = Context?.Variant switch
        {
            SidebarVariant.Floating => "bb:bg-sidebar bb:border bb:border-sidebar-border bb:rounded-lg bb:shadow-lg bb:data-[state=closed]:border-0 bb:data-[state=closed]:shadow-none",
            SidebarVariant.Inset => "bb:bg-sidebar",
            _ => "bb:bg-sidebar bb:border-r bb:border-sidebar-border bb:data-[state=closed]:border-0"
        };

        // Side-specific positioning
        var sideClasses = Context?.Side == SidebarSide.Right
            ? "bb:border-r-0 bb:border-l bb:data-[state=closed]:border-0"
            : "";

        // Width and transition classes
        var widthClasses = Collapsible && Context?.CollapsedMode != SidebarCollapsedMode.Pill
            ? "bb:w-[var(--sidebar-width)] bb:transition-[width] bb:duration-200 bb:ease-linear bb:data-[state=collapsed]:w-[var(--sidebar-width-icon)]"
            : "bb:w-[var(--sidebar-width)] bb:transition-[width,opacity] bb:duration-200 bb:ease-linear bb:data-[state=closed]:w-0 bb:data-[state=closed]:opacity-0 bb:overflow-hidden";

        // Variant-specific layout classes
        var layoutClasses = Context?.Variant switch
        {
            SidebarVariant.Floating => "bb:fixed bb:top-2 bb:bottom-2 bb:z-10",
            SidebarVariant.Inset => "bb:relative bb:h-full",
            _ => "bb:sticky bb:top-0 bb:min-h-full"
        };

        // Add left/right positioning for floating/default variants
        if (Context?.Variant != SidebarVariant.Inset)
        {
            if (Context?.Variant == SidebarVariant.Floating)
            {
                layoutClasses += Context?.Side == SidebarSide.Right ? " bb:right-2" : " bb:left-2";
            }
            else
            {
                layoutClasses += Context?.Side == SidebarSide.Right ? " bb:right-0" : " bb:left-0";
            }
        }

        return ClassNames.cn(
            baseClasses,
            variantClasses,
            sideClasses,
            widthClasses,
            layoutClasses,
            "bb:motion-reduce:transition-none",
            Class
        );
    }

    private string GetMobileClasses()
    {
        return ClassNames.cn(
            "bb:w-[var(--sidebar-width)] bb:bg-sidebar bb:p-0 bb:flex bb:flex-col",
            "bb:[&>button]:hidden", // Hide the default Sheet close button
            Class
        );
    }

    private string GetDataState()
    {
        if (Context == null)
        {
            return "collapsed";
        }

        if (Context.Open)
        {
            return "expanded";
        }

        // When not open: return "collapsed" if Collapsible (shows icons), "closed" if not Collapsible (fully hidden)
        return Collapsible && Context.CollapsedMode != SidebarCollapsedMode.Pill ? "collapsed" : "closed";
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Only resubscribe if context reference changed
        if (Context != _subscribedContext)
        {
            // Unsubscribe from old context
            _subscribedContext?.StateChanged -= OnContextStateChanged;

            // Subscribe to new context
            Context?.StateChanged += OnContextStateChanged;

            _subscribedContext = Context;
        }
    }

    private void OnContextStateChanged(object? sender, EventArgs e) =>
        // Force re-render when sidebar state changes
        StateHasChanged();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _subscribedContext?.StateChanged -= OnContextStateChanged;
        _subscribedContext = null;
    }
}

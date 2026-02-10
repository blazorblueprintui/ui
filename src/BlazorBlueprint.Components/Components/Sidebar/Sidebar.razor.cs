using BlazorBlueprint.Primitives.Sheet;
using Microsoft.AspNetCore.Components;
using System;

namespace BlazorBlueprint.Components.Sidebar;

public partial class Sidebar : IDisposable
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
        var baseClasses = "group peer flex flex-col text-sidebar-foreground shrink-0";

        // Variant-specific classes
        var variantClasses = Context?.Variant switch
        {
            SidebarVariant.Floating => "bg-sidebar border border-sidebar-border rounded-lg shadow-lg data-[state=closed]:border-0 data-[state=closed]:shadow-none",
            SidebarVariant.Inset => "bg-sidebar",
            _ => "bg-sidebar border-r border-sidebar-border data-[state=closed]:border-0"
        };

        // Side-specific positioning
        var sideClasses = Context?.Side == SidebarSide.Right
            ? "border-r-0 border-l data-[state=closed]:border-0"
            : "";

        // Width and transition classes
        var widthClasses = Collapsible
            ? "w-[var(--sidebar-width)] transition-[width] duration-200 ease-linear data-[state=collapsed]:w-[var(--sidebar-width-icon)]"
            : "w-[var(--sidebar-width)] transition-[width,opacity] duration-200 ease-linear data-[state=closed]:w-0 data-[state=closed]:opacity-0 overflow-hidden";

        // Variant-specific layout classes
        var layoutClasses = Context?.Variant switch
        {
            SidebarVariant.Floating => "fixed top-2 bottom-2 z-10",
            SidebarVariant.Inset => "relative h-full",
            _ => "sticky top-0 min-h-full"
        };

        // Add left/right positioning for floating/default variants
        if (Context?.Variant != SidebarVariant.Inset)
        {
            if (Context?.Variant == SidebarVariant.Floating)
            {
                layoutClasses += Context?.Side == SidebarSide.Right ? " right-2" : " left-2";
            }
            else
            {
                layoutClasses += Context?.Side == SidebarSide.Right ? " right-0" : " left-0";
            }
        }

        return Utilities.ClassNames.cn(
            baseClasses,
            variantClasses,
            sideClasses,
            widthClasses,
            layoutClasses,
            Class
        );
    }

    private string GetMobileClasses()
    {
        return Utilities.ClassNames.cn(
            "w-[var(--sidebar-width)] bg-sidebar p-0 flex flex-col",
            "[&>button]:hidden", // Hide the default Sheet close button
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
        return Collapsible ? "collapsed" : "closed";
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Only resubscribe if context reference changed
        if (Context != _subscribedContext)
        {
            // Unsubscribe from old context
            if (_subscribedContext != null)
            {
                _subscribedContext.StateChanged -= OnContextStateChanged;
            }

            // Subscribe to new context
            if (Context != null)
            {
                Context.StateChanged += OnContextStateChanged;
            }

            _subscribedContext = Context;
        }
    }

    private void OnContextStateChanged(object? sender, EventArgs e) =>
        // Force re-render when sidebar state changes
        StateHasChanged();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_subscribedContext != null)
        {
            _subscribedContext.StateChanged -= OnContextStateChanged;
            _subscribedContext = null;
        }
    }
}

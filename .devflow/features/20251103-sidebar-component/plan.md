# Technical Implementation Plan: Sidebar Component

**Feature ID:** 20251103-sidebar-component
**Status:** PLAN
**Created:** 2025-11-03
**Complexity:** Very High
**Estimated Tasks:** 120-150 tasks
**Timeline:** 12+ weeks

---

## Executive Summary

This plan outlines the technical implementation of a comprehensive sidebar component system for BlazorUI, consisting of 25 total components across 4 implementation phases. The sidebar system follows shadcn/ui's design patterns while adapting to Blazor's component model and state management paradigm.

**Key Challenges:**
- Large scope (25 components) requiring phased delivery
- Complex state management across Blazor hosting models
- JavaScript interop for tooltips, keyboard shortcuts, and persistence
- RTL/i18n support with CSS logical properties
- Maintaining visual and behavioral parity with shadcn

**Architecture Impact:** Minor - Adds new components following existing patterns, extends CSS variable system

---

## Technical Approach

### Component Architecture Pattern

We'll follow the established BlazorUI component pattern with enhancements for the sidebar's complex state management needs:

```csharp
// Base pattern for all components
public partial class ComponentName : ShadcnComponentBase
{
    // Parameters
    [Parameter] public ComponentVariant Variant { get; set; }
    [Parameter] public Size Size { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    // CSS class building
    private string CssClass => new CssClassBuilder()
        .Add("base-class")
        .Add(GetVariantClass())
        .Add(Class)
        .Build();
}
```

### State Management Architecture

#### SidebarContext Design

```csharp
// Models/SidebarContext.cs
public class SidebarContext
{
    // Core state
    public bool Open { get; set; } = true;
    public bool OpenMobile { get; set; } = false;
    public SidebarVariant Variant { get; set; } = SidebarVariant.Sidebar;
    public SidebarSide Side { get; set; } = SidebarSide.Left;
    public CollapseMode CollapseMode { get; set; } = CollapseMode.Offcanvas;

    // State change events
    public event Action<bool>? OnOpenChanged;
    public event Action<bool>? OnOpenMobileChanged;

    // Toggle methods
    public void Toggle() => SetOpen(!Open);
    public void ToggleMobile() => SetOpenMobile(!OpenMobile);

    public void SetOpen(bool value)
    {
        if (Open != value)
        {
            Open = value;
            OnOpenChanged?.Invoke(value);
        }
    }

    public void SetOpenMobile(bool value)
    {
        if (OpenMobile != value)
        {
            OpenMobile = value;
            OnOpenMobileChanged?.Invoke(value);
        }
    }
}
```

#### CascadingValue Provider Pattern

```csharp
// Components/Sidebar/SidebarProvider.razor
@implements IAsyncDisposable

<CascadingValue Value="Context" IsFixed="false">
    @ChildContent
</CascadingValue>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public bool DefaultOpen { get; set; } = true;
    [Parameter] public SidebarVariant Variant { get; set; } = SidebarVariant.Sidebar;
    [Parameter] public bool EnablePersistence { get; set; } = true;

    private SidebarContext Context { get; set; } = new();
    private IJSObjectReference? module;

    protected override async Task OnInitializedAsync()
    {
        // Load JS module for persistence
        if (EnablePersistence)
        {
            module = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./scripts/sidebar.js");

            // Restore state
            var savedState = await module.InvokeAsync<string?>("getSidebarState");
            if (savedState != null)
            {
                Context.Open = JsonSerializer.Deserialize<bool>(savedState);
            }
        }

        // Subscribe to state changes
        Context.OnOpenChanged += HandleOpenChanged;
    }

    private async void HandleOpenChanged(bool open)
    {
        if (EnablePersistence && module != null)
        {
            await module.InvokeVoidAsync("setSidebarState", open);
        }
        await InvokeAsync(StateHasChanged);
    }
}
```

### CSS Variables Extension

Add sidebar-specific CSS variables to the theme system:

```css
:root {
    /* Sidebar colors */
    --sidebar-background: 0 0% 98%;
    --sidebar-foreground: 240 5.3% 26.1%;
    --sidebar-primary: 240 5.9% 10%;
    --sidebar-primary-foreground: 0 0% 98%;
    --sidebar-accent: 240 4.8% 95.9%;
    --sidebar-accent-foreground: 240 5.9% 10%;
    --sidebar-border: 220 13% 91%;
    --sidebar-ring: 217.2 91.2% 59.8%;

    /* Sidebar dimensions */
    --sidebar-width: 16rem;
    --sidebar-width-mobile: 18rem;
    --sidebar-width-icon: 3rem;
}

.dark {
    --sidebar-background: 240 5.9% 10%;
    --sidebar-foreground: 240 4.8% 95.9%;
    /* ... dark mode overrides ... */
}
```

### JavaScript Interop Strategy

#### Modular JS Architecture

```javascript
// wwwroot/scripts/sidebar.js
export function initializeSidebar(dotNetRef, options) {
    // Keyboard shortcuts
    document.addEventListener('keydown', (e) => {
        if ((e.metaKey || e.ctrlKey) && e.key === 'b') {
            e.preventDefault();
            dotNetRef.invokeMethodAsync('ToggleSidebar');
        }
    });

    // State persistence
    const storageKey = `sidebar-state-${options.id}`;

    return {
        getSidebarState: () => localStorage.getItem(storageKey),
        setSidebarState: (state) => localStorage.setItem(storageKey, JSON.stringify(state)),
        clearSidebarState: () => localStorage.removeItem(storageKey)
    };
}

// Tooltip positioning
export function positionTooltip(triggerEl, tooltipEl, placement = 'top') {
    const triggerRect = triggerEl.getBoundingClientRect();
    const tooltipRect = tooltipEl.getBoundingClientRect();

    let top = 0, left = 0;

    switch (placement) {
        case 'top':
            top = triggerRect.top - tooltipRect.height - 8;
            left = triggerRect.left + (triggerRect.width - tooltipRect.width) / 2;
            break;
        case 'bottom':
            top = triggerRect.bottom + 8;
            left = triggerRect.left + (triggerRect.width - tooltipRect.width) / 2;
            break;
        case 'left':
            top = triggerRect.top + (triggerRect.height - tooltipRect.height) / 2;
            left = triggerRect.left - tooltipRect.width - 8;
            break;
        case 'right':
            top = triggerRect.top + (triggerRect.height - tooltipRect.height) / 2;
            left = triggerRect.right + 8;
            break;
    }

    // Boundary checking
    const viewport = {
        width: window.innerWidth,
        height: window.innerHeight
    };

    // Adjust if out of bounds
    if (left < 0) left = 8;
    if (left + tooltipRect.width > viewport.width) {
        left = viewport.width - tooltipRect.width - 8;
    }
    if (top < 0) top = 8;
    if (top + tooltipRect.height > viewport.height) {
        top = viewport.height - tooltipRect.height - 8;
    }

    return { top: `${top}px`, left: `${left}px` };
}
```

---

## Component Breakdown

### Phase 1: Foundation Components

#### 1. Separator Component
**Location:** `/Components/Separator/`
**Dependencies:** None
**Complexity:** Low

```csharp
public partial class Separator : ShadcnComponentBase
{
    [Parameter] public Orientation Orientation { get; set; } = Orientation.Horizontal;
    [Parameter] public bool Decorative { get; set; } = false;

    private string CssClass => new CssClassBuilder()
        .Add("shrink-0 bg-border")
        .Add(Orientation == Orientation.Horizontal ? "h-[1px] w-full" : "h-full w-[1px]")
        .Add(Class)
        .Build();

    private string? AriaOrientation => !Decorative ? Orientation.ToString().ToLower() : null;
}
```

#### 2. Input Component
**Location:** `/Components/Input/`
**Dependencies:** None
**Complexity:** Medium

```csharp
public partial class Input : ShadcnComponentBase
{
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public InputType Type { get; set; } = InputType.Text;
    [Parameter] public Size Size { get; set; } = Size.Medium;
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Required { get; set; }

    private string CssClass => new CssClassBuilder()
        .Add("flex w-full rounded-md border border-input bg-background")
        .Add("text-sm ring-offset-background")
        .Add("placeholder:text-muted-foreground")
        .Add("focus-visible:outline-none focus-visible:ring-2")
        .Add("focus-visible:ring-ring focus-visible:ring-offset-2")
        .Add("disabled:cursor-not-allowed disabled:opacity-50")
        .Add(GetSizeClass())
        .Add(Class)
        .Build();
}
```

#### 3. Badge Component
**Location:** `/Components/Badge/`
**Dependencies:** None
**Complexity:** Low

```csharp
public partial class Badge : ShadcnComponentBase
{
    [Parameter] public BadgeVariant Variant { get; set; } = BadgeVariant.Default;
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string CssClass => new CssClassBuilder()
        .Add("inline-flex items-center rounded-full px-2.5 py-0.5")
        .Add("text-xs font-semibold transition-colors")
        .Add("focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2")
        .Add(GetVariantClass())
        .Add(Class)
        .Build();
}
```

#### 4. Skeleton Component
**Location:** `/Components/Skeleton/`
**Dependencies:** None
**Complexity:** Low

```csharp
public partial class Skeleton : ShadcnComponentBase
{
    [Parameter] public string? Width { get; set; }
    [Parameter] public string? Height { get; set; }
    [Parameter] public SkeletonShape Shape { get; set; } = SkeletonShape.Rectangle;

    private string CssClass => new CssClassBuilder()
        .Add("animate-pulse bg-muted")
        .Add(Shape == SkeletonShape.Circle ? "rounded-full" : "rounded-md")
        .Add(Class)
        .Build();

    private string Style => $"width: {Width ?? "100%"}; height: {Height ?? "20px"}";
}
```

#### 5. Label Component
**Location:** `/Components/Label/`
**Dependencies:** None
**Complexity:** Low

```csharp
public partial class Label : ShadcnComponentBase
{
    [Parameter] public string? For { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string CssClass => new CssClassBuilder()
        .Add("text-sm font-medium leading-none")
        .Add("peer-disabled:cursor-not-allowed peer-disabled:opacity-70")
        .Add(Class)
        .Build();
}
```

### Phase 2: Complex Dependencies

#### 6. Collapsible Component System
**Location:** `/Components/Collapsible/`
**Dependencies:** None
**Complexity:** High

```csharp
// Collapsible.razor.cs
public partial class Collapsible : ShadcnComponentBase
{
    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private CollapsibleContext Context { get; set; }

    protected override void OnInitialized()
    {
        Context = new CollapsibleContext
        {
            Open = Open,
            Disabled = Disabled,
            Toggle = async () => await ToggleAsync()
        };
    }

    private async Task ToggleAsync()
    {
        if (!Disabled)
        {
            Open = !Open;
            Context.Open = Open;
            await OpenChanged.InvokeAsync(Open);
        }
    }
}

// CollapsibleTrigger.razor.cs
public partial class CollapsibleTrigger : ShadcnComponentBase
{
    [CascadingParameter] public CollapsibleContext Context { get; set; } = null!;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public bool AsChild { get; set; }

    private async Task HandleClick()
    {
        await Context.Toggle();
    }
}

// CollapsibleContent.razor.cs
public partial class CollapsibleContent : ShadcnComponentBase
{
    [CascadingParameter] public CollapsibleContext Context { get; set; } = null!;
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string CssClass => new CssClassBuilder()
        .Add("overflow-hidden transition-all duration-200")
        .Add(Context.Open ? "animate-accordion-down" : "animate-accordion-up")
        .Add(Class)
        .Build();
}
```

#### 7. Tooltip Component System
**Location:** `/Components/Tooltip/`
**Dependencies:** JavaScript interop
**Complexity:** Very High

```csharp
// TooltipProvider.razor.cs
public partial class TooltipProvider : ShadcnComponentBase
{
    [Parameter] public int DelayDuration { get; set; } = 700;
    [Parameter] public int SkipDelayDuration { get; set; } = 300;
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private TooltipProviderContext Context { get; set; }

    protected override void OnInitialized()
    {
        Context = new TooltipProviderContext
        {
            DelayDuration = DelayDuration,
            SkipDelayDuration = SkipDelayDuration
        };
    }
}

// Tooltip.razor.cs
public partial class Tooltip : ShadcnComponentBase, IAsyncDisposable
{
    [CascadingParameter] public TooltipProviderContext Provider { get; set; } = null!;
    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private TooltipContext Context { get; set; }
    private IJSObjectReference? module;
    private Timer? delayTimer;

    protected override async Task OnInitializedAsync()
    {
        Context = new TooltipContext
        {
            Open = Open,
            SetOpen = SetOpen
        };

        module = await JS.InvokeAsync<IJSObjectReference>(
            "import", "./scripts/tooltip.js");
    }

    private async void SetOpen(bool value)
    {
        Open = value;
        Context.Open = value;
        await OpenChanged.InvokeAsync(value);
        await InvokeAsync(StateHasChanged);
    }
}
```

#### 8. Avatar Component System
**Location:** `/Components/Avatar/`
**Dependencies:** None
**Complexity:** Medium

```csharp
// Avatar.razor.cs
public partial class Avatar : ShadcnComponentBase
{
    [Parameter] public Size Size { get; set; } = Size.Medium;
    [Parameter] public AvatarShape Shape { get; set; } = AvatarShape.Circle;
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private AvatarContext Context { get; set; } = new();

    private string CssClass => new CssClassBuilder()
        .Add("relative flex shrink-0 overflow-hidden")
        .Add(Shape == AvatarShape.Circle ? "rounded-full" : "rounded-md")
        .Add(GetSizeClass())
        .Add(Class)
        .Build();
}

// AvatarImage.razor.cs
public partial class AvatarImage : ShadcnComponentBase
{
    [CascadingParameter] public AvatarContext Context { get; set; } = null!;
    [Parameter] public string? Src { get; set; }
    [Parameter] public string? Alt { get; set; }

    private bool ImageLoaded { get; set; }
    private bool ImageError { get; set; }

    private async Task HandleImageLoad()
    {
        ImageLoaded = true;
        Context.HasImage = true;
        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleImageError()
    {
        ImageError = true;
        Context.HasImage = false;
        await InvokeAsync(StateHasChanged);
    }
}
```

### Phase 3: Sidebar Core Components

#### 9. SidebarProvider (Already detailed above)

#### 10. Sidebar Container
**Location:** `/Components/Sidebar/`
**Dependencies:** SidebarProvider context
**Complexity:** High

```csharp
public partial class Sidebar : ShadcnComponentBase
{
    [CascadingParameter] public SidebarContext Context { get; set; } = null!;
    [Parameter] public SidebarVariant Variant { get; set; } = SidebarVariant.Sidebar;
    [Parameter] public SidebarSide Side { get; set; } = SidebarSide.Left;
    [Parameter] public CollapseMode CollapseMode { get; set; } = CollapseMode.Offcanvas;
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string CssClass => new CssClassBuilder()
        .Add("group/sidebar relative flex h-full flex-col")
        .Add("bg-sidebar text-sidebar-foreground")
        .Add(GetVariantClass())
        .Add(GetSideClass())
        .Add(GetCollapseClass())
        .Add(Class)
        .Build();

    private string GetVariantClass() => Variant switch
    {
        SidebarVariant.Sidebar => "border-e",
        SidebarVariant.Floating => "rounded-lg border shadow-lg",
        SidebarVariant.Inset => "border-e bg-background",
        _ => ""
    };

    private string GetCollapseClass()
    {
        if (!Context.Open && CollapseMode == CollapseMode.Icon)
            return "w-[--sidebar-width-icon]";
        return Context.Open ? "w-[--sidebar-width]" : "w-0";
    }
}
```

#### 11-13. Layout Components (Header, Content, Footer)
**Location:** `/Components/Sidebar/`
**Dependencies:** Sidebar context
**Complexity:** Low

```csharp
// SidebarHeader.razor.cs
public partial class SidebarHeader : ShadcnComponentBase
{
    private string CssClass => new CssClassBuilder()
        .Add("flex flex-col gap-2 p-2")
        .Add(Class)
        .Build();
}

// SidebarContent.razor.cs
public partial class SidebarContent : ShadcnComponentBase
{
    private string CssClass => new CssClassBuilder()
        .Add("flex flex-1 flex-col gap-2 overflow-auto p-2")
        .Add("group-data-[collapsible=icon]:overflow-hidden")
        .Add(Class)
        .Build();
}

// SidebarFooter.razor.cs
public partial class SidebarFooter : ShadcnComponentBase
{
    private string CssClass => new CssClassBuilder()
        .Add("flex flex-col gap-2 p-2 mt-auto")
        .Add(Class)
        .Build();
}
```

#### 14-16. Group Components
**Location:** `/Components/Sidebar/`
**Dependencies:** Collapsible, Sidebar context
**Complexity:** Medium

```csharp
// SidebarGroup.razor.cs
public partial class SidebarGroup : ShadcnComponentBase
{
    [CascadingParameter] public SidebarContext SidebarContext { get; set; } = null!;
    [Parameter] public bool Collapsible { get; set; }
    [Parameter] public bool DefaultOpen { get; set; } = true;
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private bool Open { get; set; }

    protected override void OnInitialized()
    {
        Open = DefaultOpen;
    }
}
```

#### 17-20. Menu Components
**Location:** `/Components/Sidebar/`
**Dependencies:** Sidebar context
**Complexity:** Medium-High

```csharp
// SidebarMenu.razor.cs
public partial class SidebarMenu : ShadcnComponentBase
{
    private string CssClass => new CssClassBuilder()
        .Add("flex w-full min-w-0 flex-col gap-1")
        .Add(Class)
        .Build();
}

// SidebarMenuItem.razor.cs
public partial class SidebarMenuItem : ShadcnComponentBase
{
    [Parameter] public bool Active { get; set; }

    private string CssClass => new CssClassBuilder()
        .Add("group/menu-item relative")
        .Add(Class)
        .Build();
}

// SidebarMenuButton.razor.cs
public partial class SidebarMenuButton : ShadcnComponentBase
{
    [CascadingParameter] public SidebarContext Context { get; set; } = null!;
    [Parameter] public bool Active { get; set; }
    [Parameter] public bool AsChild { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public RenderFragment? Icon { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private bool ShowTooltip => Context.Open == false &&
                                Context.CollapseMode == CollapseMode.Icon;

    private string CssClass => new CssClassBuilder()
        .Add("flex w-full items-center gap-2 rounded-md px-2 py-1.5")
        .Add("text-sm font-medium transition-colors")
        .Add("hover:bg-sidebar-accent hover:text-sidebar-accent-foreground")
        .Add("data-[active=true]:bg-sidebar-accent")
        .Add("group-data-[collapsible=icon]:justify-center")
        .Add(Class)
        .Build();
}
```

### Phase 4: Advanced Features

#### 21. SidebarTrigger with Keyboard Shortcuts
**Location:** `/Components/Sidebar/`
**Dependencies:** JavaScript interop, Sidebar context
**Complexity:** High

```csharp
public partial class SidebarTrigger : ShadcnComponentBase, IAsyncDisposable
{
    [CascadingParameter] public SidebarContext Context { get; set; } = null!;
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private IJSObjectReference? module;
    private DotNetObjectReference<SidebarTrigger>? dotNetRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            dotNetRef = DotNetObjectReference.Create(this);
            module = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./scripts/sidebar.js");
            await module.InvokeVoidAsync("initializeKeyboardShortcuts", dotNetRef);
        }
    }

    [JSInvokable]
    public void ToggleSidebar()
    {
        Context.Toggle();
        InvokeAsync(StateHasChanged);
    }
}
```

#### 22. State Persistence Implementation
**Location:** Built into SidebarProvider
**Dependencies:** JavaScript interop
**Complexity:** Medium

#### 23. Mobile Support (Optional Sheet Component)
**Location:** `/Components/Sheet/`
**Dependencies:** Dialog pattern adaptation
**Complexity:** Very High

```csharp
// Sheet.razor.cs (Mobile drawer implementation)
public partial class Sheet : ShadcnComponentBase
{
    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public SheetSide Side { get; set; } = SheetSide.Right;
    [Parameter] public RenderFragment? ChildContent { get; set; }

    // Portal rendering for overlay
    // Touch gesture handling
    // Focus trap implementation
}
```

---

## Data Models

### Enumerations

```csharp
// Enums/SidebarEnums.cs
public enum SidebarVariant
{
    Sidebar,   // Default, overlays content
    Floating,  // Appears above content with shadow
    Inset      // Pushes content to the side
}

public enum SidebarSide
{
    Left,
    Right
}

public enum CollapseMode
{
    Offcanvas,  // Slide off screen
    Icon,       // Collapse to icon-only
    None        // No collapse behavior
}

public enum Orientation
{
    Horizontal,
    Vertical
}

public enum BadgeVariant
{
    Default,
    Secondary,
    Destructive,
    Outline
}

public enum InputType
{
    Text,
    Email,
    Password,
    Number,
    Tel,
    Url,
    Search,
    Date,
    Time,
    File
}

public enum AvatarShape
{
    Circle,
    Square
}

public enum SkeletonShape
{
    Rectangle,
    Circle
}
```

### Context Models

```csharp
// Models/Contexts.cs
public class CollapsibleContext
{
    public bool Open { get; set; }
    public bool Disabled { get; set; }
    public Func<Task> Toggle { get; set; } = () => Task.CompletedTask;
}

public class TooltipProviderContext
{
    public int DelayDuration { get; set; } = 700;
    public int SkipDelayDuration { get; set; } = 300;
}

public class TooltipContext
{
    public bool Open { get; set; }
    public Action<bool> SetOpen { get; set; } = _ => { };
}

public class AvatarContext
{
    public bool HasImage { get; set; }
}
```

---

## API Design

### Component Parameter APIs

All components follow consistent parameter patterns:

```csharp
// Common parameters across components
[Parameter] public string? Id { get; set; }
[Parameter] public string? Class { get; set; }
[Parameter] public RenderFragment? ChildContent { get; set; }

// Variant parameters
[Parameter] public TVariant Variant { get; set; }
[Parameter] public Size Size { get; set; }

// State parameters
[Parameter] public bool Disabled { get; set; }
[Parameter] public bool Required { get; set; }

// Event callbacks
[Parameter] public EventCallback<T> OnClick { get; set; }
[Parameter] public EventCallback<T> ValueChanged { get; set; }

// Accessibility
[Parameter] public string? AriaLabel { get; set; }
[Parameter] public string? AriaDescribedBy { get; set; }
```

### Validation Patterns

```csharp
// Input validation
public partial class Input
{
    [Parameter] public string? Pattern { get; set; }
    [Parameter] public int? MaxLength { get; set; }
    [Parameter] public int? MinLength { get; set; }
    [Parameter] public string? Min { get; set; }
    [Parameter] public string? Max { get; set; }

    private bool IsValid()
    {
        if (Required && string.IsNullOrWhiteSpace(Value))
            return false;

        if (Pattern != null && !Regex.IsMatch(Value ?? "", Pattern))
            return false;

        return true;
    }
}
```

---

## Business Logic

### Sidebar State Management Flow

```
1. User interacts with trigger/button
   ↓
2. Context.Toggle() called
   ↓
3. State updated in context
   ↓
4. OnOpenChanged event fired
   ↓
5. SidebarProvider handles persistence
   ↓
6. All consuming components re-render via CascadingValue
   ↓
7. CSS classes update based on new state
   ↓
8. Transition animations play
```

### Collapsible Logic Flow

```
1. User clicks CollapsibleTrigger
   ↓
2. Context.Toggle() invoked
   ↓
3. Open state toggles
   ↓
4. OpenChanged callback invoked
   ↓
5. CollapsibleContent re-renders
   ↓
6. Height animation triggered via CSS
   ↓
7. ARIA attributes updated
```

### Tooltip Positioning Logic

```
1. Mouse enters TooltipTrigger
   ↓
2. Delay timer starts (700ms default)
   ↓
3. Timer completes → Open = true
   ↓
4. TooltipContent renders in portal
   ↓
5. JS calculates optimal position
   ↓
6. Position applied via style
   ↓
7. Boundary checking ensures visibility
   ↓
8. Mouse leaves → Hide after delay
```

---

## Integration Points

### CSS Variable Integration

Components integrate with the existing theme system and add new variables:

```css
/* Add to variables.css */
:root {
    /* Existing variables remain */

    /* New sidebar variables */
    --sidebar-background: hsl(var(--background));
    --sidebar-foreground: hsl(var(--foreground));
    --sidebar-primary: hsl(var(--primary));
    --sidebar-primary-foreground: hsl(var(--primary-foreground));
    --sidebar-accent: hsl(var(--accent));
    --sidebar-accent-foreground: hsl(var(--accent-foreground));
    --sidebar-border: hsl(var(--border));
    --sidebar-ring: hsl(var(--ring));

    /* Dimensions */
    --sidebar-width: 16rem;
    --sidebar-width-mobile: 18rem;
    --sidebar-width-icon: 3rem;
}
```

### Tailwind Configuration Updates

```javascript
// Extend tailwind.config.js
module.exports = {
    theme: {
        extend: {
            colors: {
                'sidebar': {
                    DEFAULT: 'hsl(var(--sidebar-background))',
                    foreground: 'hsl(var(--sidebar-foreground))',
                    primary: 'hsl(var(--sidebar-primary))',
                    'primary-foreground': 'hsl(var(--sidebar-primary-foreground))',
                    accent: 'hsl(var(--sidebar-accent))',
                    'accent-foreground': 'hsl(var(--sidebar-accent-foreground))',
                    border: 'hsl(var(--sidebar-border))',
                    ring: 'hsl(var(--sidebar-ring))',
                }
            },
            width: {
                'sidebar': 'var(--sidebar-width)',
                'sidebar-mobile': 'var(--sidebar-width-mobile)',
                'sidebar-icon': 'var(--sidebar-width-icon)',
            },
            keyframes: {
                'accordion-down': {
                    from: { height: '0' },
                    to: { height: 'var(--radix-accordion-content-height)' }
                },
                'accordion-up': {
                    from: { height: 'var(--radix-accordion-content-height)' },
                    to: { height: '0' }
                }
            },
            animation: {
                'accordion-down': 'accordion-down 0.2s ease-out',
                'accordion-up': 'accordion-up 0.2s ease-out',
            }
        }
    }
}
```

### JavaScript Module System

```javascript
// wwwroot/scripts/index.js - Main entry point
export { initializeSidebar, positionTooltip } from './sidebar.js';
export { initializeCollapsible } from './collapsible.js';
```

---

## Cross-Cutting Concerns

### 1. Theming System Integration

All components integrate with CSS variables:

```csharp
// Every component uses theme variables
private string CssClass => new CssClassBuilder()
    .Add("bg-sidebar text-sidebar-foreground")
    .Add("border-sidebar-border")
    .Add("hover:bg-sidebar-accent hover:text-sidebar-accent-foreground")
    .Build();
```

**Dark mode support:**
- All color variables have dark mode overrides
- Components automatically adapt when `.dark` class is applied
- No component-level dark mode logic needed

### 2. Internationalization (RTL Support)

All components use CSS logical properties for RTL support:

```css
/* Use logical properties instead of physical */
.sidebar {
    border-inline-end: 1px solid var(--sidebar-border); /* not border-right */
    padding-inline-start: 1rem; /* not padding-left */
    margin-inline-end: 0.5rem; /* not margin-right */
    text-align: start; /* not text-align: left */
}

/* Tailwind RTL utilities */
.sidebar-menu-item {
    @apply ps-2 me-2; /* start/end padding instead of left/right */
}
```

**Implementation pattern:**
```csharp
// Support RTL in component logic
[Parameter] public bool Rtl { get; set; }

private string GetDirectionalClass() => Rtl ?
    "border-s-0 border-e" :
    "border-e-0 border-s";
```

### 3. Accessibility Patterns

All components implement WCAG 2.1 AA standards:

```csharp
// Sidebar navigation accessibility
<nav role="navigation" aria-label="@AriaLabel">
    <button aria-expanded="@Context.Open"
            aria-controls="sidebar-content">
        Toggle Sidebar
    </button>

    <div id="sidebar-content"
         aria-hidden="@(!Context.Open)">
        @ChildContent
    </div>
</nav>

// Keyboard navigation
@onkeydown="HandleKeyDown"

private async Task HandleKeyDown(KeyboardEventArgs e)
{
    switch (e.Key)
    {
        case "Escape":
            await Context.SetOpen(false);
            break;
        case "ArrowDown":
            await FocusNextMenuItem();
            break;
        case "ArrowUp":
            await FocusPreviousMenuItem();
            break;
    }
}
```

### 4. Performance Optimization

```csharp
// Use ShouldRender to prevent unnecessary re-renders
protected override bool ShouldRender()
{
    // Only re-render if state actually changed
    return previousOpen != Context.Open ||
           previousVariant != Context.Variant;
}

// Lazy load heavy components
@if (ShowTooltip)
{
    <Tooltip>
        @TooltipContent
    </Tooltip>
}

// Virtualize long menu lists
<Virtualize Items="MenuItems" Context="item">
    <SidebarMenuItem>@item.Label</SidebarMenuItem>
</Virtualize>
```

---

## Security & Performance

### Security Considerations

1. **XSS Prevention**
   - Use Blazor's built-in encoding (`@` syntax)
   - Never use `MarkupString` for user content
   - Sanitize any HTML content if needed

2. **Input Validation**
   - Validate all form inputs server-side
   - Use parameter validation attributes
   - Implement rate limiting for state changes

3. **JavaScript Interop Safety**
   - Validate all data passed to/from JS
   - Use structured objects, not raw strings
   - Handle JS errors gracefully

### Performance Optimizations

1. **Rendering Optimization**
   ```csharp
   // Minimize re-renders
   protected override bool ShouldRender() => HasStateChanged;

   // Use virtualization for long lists
   <Virtualize Items="@LargeList" ItemsProvider="LoadItems">
       <ItemContent>
           <SidebarMenuItem Item="@context" />
       </ItemContent>
   </Virtualize>
   ```

2. **CSS Optimization**
   - Use Tailwind purge to remove unused styles
   - Minimize CSS-in-JS usage
   - Leverage browser caching for static assets

3. **JavaScript Bundle Optimization**
   - Lazy load JS modules
   - Minimize JS payload
   - Use tree-shaking for unused code

4. **State Management Efficiency**
   - Use `IsFixed="true"` for stable CascadingValues
   - Minimize cascading parameter depth
   - Cache computed values

---

## Testing Strategy

### Phase 1: Foundation Components Testing
**Duration:** 3 days

1. **Visual Testing**
   - Compare each component to shadcn reference
   - Test all size variants
   - Verify dark mode appearance
   - Check responsive behavior

2. **Functional Testing**
   - Input validation and events
   - Badge variant display
   - Skeleton animation
   - Label association with inputs

3. **Accessibility Testing**
   - Keyboard navigation
   - Screen reader announcements
   - ARIA attributes
   - Focus management

### Phase 2: Complex Components Testing
**Duration:** 4 days

1. **Collapsible Testing**
   - Open/close animations
   - Nested collapsibles
   - Keyboard shortcuts (Space, Enter)
   - State persistence

2. **Tooltip Testing**
   - Positioning in all directions
   - Viewport boundary detection
   - Delay timing
   - Touch device behavior

3. **Avatar Testing**
   - Image loading states
   - Fallback display
   - Different shapes and sizes

### Phase 3: Sidebar Core Testing
**Duration:** 5 days

1. **State Management**
   - Context propagation
   - State persistence across refreshes
   - Multiple sidebar instances
   - State sync across components

2. **Layout Testing**
   - All variant behaviors
   - Collapse modes
   - Mobile responsive behavior
   - RTL layout

3. **Animation Testing**
   - Smooth transitions
   - Reduced motion support
   - Performance on low-end devices

### Phase 4: Advanced Features Testing
**Duration:** 3 days

1. **Keyboard Shortcuts**
   - Cmd+B / Ctrl+B toggle
   - Custom key bindings
   - Conflict resolution

2. **Mobile Testing**
   - Touch gestures
   - Viewport management
   - Sheet/drawer behavior

3. **Cross-Browser Testing**
   - Chrome, Firefox, Safari, Edge
   - Mobile browsers
   - Different viewport sizes

### Test Coverage Requirements

```
Component Coverage:
- Visual parity: 100% match with shadcn
- Functionality: All props and events tested
- Accessibility: WCAG 2.1 AA compliance
- Browser support: Latest 2 versions

Hosting Model Coverage:
- Blazor Server: Full functionality
- Blazor WebAssembly: Full functionality
- Blazor Hybrid: Full functionality
```

---

## Architecture Impact

### Assessment: MINOR

This feature adds new components following existing patterns without changing core architecture.

**What stays the same:**
- Component organization pattern (feature-based folders)
- CSS variable theming approach
- Tailwind utility class usage
- Parameter and event callback patterns
- Accessibility implementation approach

**What's new (but non-breaking):**
- Additional CSS variables for sidebar theming
- New JavaScript modules for interop
- CascadingValue pattern for complex state (already used elsewhere)
- Portal rendering pattern for tooltips (isolated to tooltip component)

**No changes to:**
- Build process or configuration
- Existing components
- Project structure
- Deployment strategy
- Testing approach

---

## Implementation Order

### Critical Path Dependencies

```
Phase 1 (Foundation) - 3 weeks
├── Separator ──┐
├── Input       ├──> Used by multiple components
├── Badge       │
├── Skeleton    │
└── Label ──────┘

Phase 2 (Complex) - 3 weeks
├── Collapsible ──> Required by SidebarGroup
├── Tooltip ──────> Required by icon-only mode
└── Avatar ───────> Common in headers/footers

Phase 3 (Core) - 4 weeks
├── CSS Variables ──> Foundation for all sidebar components
├── SidebarProvider ─┐
├── Sidebar ─────────├──> Core container components
├── Layout (H/C/F) ──┤
├── SidebarSeparator ┤
├── SidebarGroup ────┤
└── Menu Components ─┘

Phase 4 (Advanced) - 2 weeks
├── Enhanced Menu ──> Extends Phase 3 menu
├── SidebarTrigger ─> Keyboard shortcuts
├── SidebarRail ────> Visual enhancement
├── Persistence ────> State management
└── Mobile Sheet ───> Optional mobile support
```

### Task Grouping Strategy

**Group 1: Foundation Setup (Week 1)**
- Create component folders
- Set up base classes
- Configure CSS variables
- Implement Separator, Label

**Group 2: Input Components (Week 2)**
- Input component with variants
- Badge with variants
- Skeleton with animation

**Group 3: Collapsible System (Week 3-4)**
- Collapsible context and state
- Trigger and content components
- Animation implementation

**Group 4: Tooltip System (Week 4-5)**
- JS interop setup
- Positioning logic
- Provider and context

**Group 5: Avatar System (Week 5-6)**
- Image loading logic
- Fallback handling
- Size variants

**Group 6: Sidebar Foundation (Week 7-8)**
- Provider and context
- Container component
- Layout components

**Group 7: Sidebar Groups (Week 8-9)**
- Group components
- Integration with Collapsible
- Label and actions

**Group 8: Menu System (Week 9-10)**
- Menu container
- Menu items and buttons
- Active state management

**Group 9: Advanced Features (Week 11-12)**
- Keyboard shortcuts
- State persistence
- Mobile support
- Final testing

---

## Key Challenges & Mitigation

### 1. State Management Complexity
**Challenge:** Managing state across multiple Blazor hosting models
**Mitigation:**
- Use CascadingValue with careful lifecycle management
- Test thoroughly in all hosting models
- Provide fallbacks for state persistence failures

### 2. JavaScript Interop Performance
**Challenge:** JS interop can be slow in Blazor Server
**Mitigation:**
- Batch JS calls when possible
- Use lazy loading for JS modules
- Cache JS object references
- Provide pure CSS fallbacks where feasible

### 3. Animation Performance
**Challenge:** Smooth animations on all devices
**Mitigation:**
- Use CSS transforms (GPU accelerated)
- Implement `prefers-reduced-motion` support
- Test on low-end devices
- Provide animation-free mode

### 4. RTL Complexity
**Challenge:** Ensuring all components work correctly in RTL
**Mitigation:**
- Use CSS logical properties exclusively
- Test every component in RTL mode
- Document RTL-specific behavior
- Provide RTL examples in demos

### 5. Tooltip Positioning Edge Cases
**Challenge:** Complex positioning logic with viewport boundaries
**Mitigation:**
- Implement comprehensive boundary checking
- Provide fallback positions
- Test at various viewport sizes
- Handle scrollable containers

### 6. Context Token Management
**Challenge:** Large feature may exceed context limits
**Mitigation:**
- Use DevFlow snapshots between phases
- Break work into smaller sessions
- Keep documentation references minimal
- Focus on one phase at a time

---

## Decision Points

### Decision 1: State Persistence Approach

**Options Considered:**
1. localStorage only
2. Cookies only
3. Hybrid (localStorage with cookie fallback)

**Decision:** Hybrid approach
**Rationale:** localStorage is simpler but may be blocked; cookies work everywhere but have size limits

### Decision 2: Tooltip Implementation

**Options Considered:**
1. Pure CSS tooltips (limited positioning)
2. Full JS positioning (complex but flexible)
3. Hybrid CSS with JS enhancement

**Decision:** Full JS positioning
**Rationale:** Matches shadcn behavior exactly, provides best UX with smart positioning

### Decision 3: Mobile Sidebar Approach

**Options Considered:**
1. Same component with responsive CSS
2. Separate Sheet component
3. Conditional rendering based on viewport

**Decision:** Separate Sheet component (Phase 4, optional)
**Rationale:** Cleaner separation of concerns, can defer if timeline tight

### Decision 4: Animation Technique

**Options Considered:**
1. CSS transitions only
2. JavaScript animation library
3. Blazor animation component

**Decision:** CSS transitions only
**Rationale:** Best performance, simpler implementation, matches shadcn approach

---

## Summary

### Complexity Assessment
**Overall: VERY HIGH**
- 25 components with complex interdependencies
- State management across multiple contexts
- JavaScript interop requirements
- Animation and positioning logic
- RTL and accessibility requirements

### Estimated Task Breakdown
- **Phase 1:** 25-30 tasks
- **Phase 2:** 30-35 tasks
- **Phase 3:** 40-50 tasks
- **Phase 4:** 25-30 tasks
- **Total:** 120-150 tasks

### Risk Level: HIGH
- Large scope with long timeline
- Complex state management requirements
- Multiple JavaScript interop points
- Performance optimization needs
- Extensive testing burden

### Architecture Impact: MINOR
- Follows existing patterns
- Extends CSS variable system
- No breaking changes
- Isolated component implementations

### User Review Required: NO
- Follows established patterns
- No major architectural decisions
- Standard implementation approach
- Well-defined in specification

---

**Next Steps:**
1. Review and approve this plan
2. Run `/devflow:tasks` to generate detailed task breakdown
3. Begin Phase 1 implementation with `/devflow:execute`
4. Create snapshots between phases for context management
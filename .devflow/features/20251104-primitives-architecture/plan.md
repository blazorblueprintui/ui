# Technical Implementation Plan: Radix UI-Style Primitives Architecture

**Feature:** 20251104-primitives-architecture
**Status:** Planning
**Architect:** Claude Opus 4.1
**Created:** 2025-11-04

## 1. Architecture Overview

### Two-Tier Architecture Pattern

The implementation will follow a strict separation between behavior (primitives) and presentation (styled components), mirroring the proven Radix UI + shadcn/ui pattern:

```
┌──────────────────────────────────────────────────────┐
│                 PRESENTATION LAYER                     │
│         Components/ (Styled shadcn Components)         │
│  - Applies Tailwind classes & CSS variables           │
│  - Provides default shadcn styling                    │
│  - Zero behavioral logic                              │
└──────────────────────────────────────────────────────┘
                           ▲
                    Wraps & Styles
                           │
┌──────────────────────────────────────────────────────┐
│                   BEHAVIOR LAYER                       │
│          Primitives/ (Headless Components)            │
│  - All interaction logic                              │
│  - Accessibility (ARIA, keyboard nav)                 │
│  - State management                                   │
│  - JavaScript interop                                 │
│  - Zero styling (headless)                           │
└──────────────────────────────────────────────────────┘
                           ▲
                        Utilizes
                           │
┌──────────────────────────────────────────────────────┐
│                INFRASTRUCTURE LAYER                    │
│               Shared/Services & Utilities             │
│  - PortalService (render outside DOM hierarchy)       │
│  - FocusManager (trap, restore focus)                │
│  - PositioningService (Floating UI integration)      │
│  - IdGenerator (unique IDs for ARIA)                 │
│  - UseControllableState (controlled/uncontrolled)    │
└──────────────────────────────────────────────────────┘
```

### Alignment with shadcn/ui Architecture

This directly parallels how shadcn/ui layers on top of Radix UI:
- **Radix UI (our Primitives/):** Provides unstyled, accessible, keyboard-navigable components
- **shadcn/ui (our Components/):** Adds Tailwind styling and visual design
- **Separation of Concerns:** Behavior is completely decoupled from presentation

## 2. Technical Approach

### Infrastructure Services Design

#### PortalService
```csharp
public interface IPortalService
{
    void RegisterPortal(string id, RenderFragment content);
    void UnregisterPortal(string id);
    void UpdatePortalContent(string id, RenderFragment content);
}

public class PortalService : IPortalService
{
    private readonly Dictionary<string, RenderFragment> portals = new();
    public event Action? OnPortalsChanged;

    // Renders content at document body level
    // Handles z-index stacking contexts
    // Manages cleanup on component disposal
}
```

#### FocusManager
```csharp
public interface IFocusManager
{
    Task TrapFocus(ElementReference container);
    Task RestoreFocus(ElementReference? previousElement);
    Task FocusFirst(ElementReference container);
    Task FocusLast(ElementReference container);
}

public class FocusManager : IFocusManager
{
    private readonly IJSRuntime jsRuntime;

    // Uses focus-trap.js for robust focus management
    // Handles tab cycling within containers
    // Restores focus on dialog close
}
```

#### PositioningService
```csharp
public interface IPositioningService
{
    Task<Position> ComputePosition(ElementReference reference, ElementReference floating, PositionOptions options);
    Task SetPosition(ElementReference element, Position position);
}

public class PositioningService : IPositioningService
{
    // Wraps @floating-ui/dom for positioning
    // Handles collision detection
    // Supports auto-placement
    // Manages viewport constraints
}
```

#### IdGenerator
```csharp
public static class IdGenerator
{
    private static int counter = 0;

    public static string GenerateId(string prefix = "shadcn")
    {
        return $"{prefix}-{Interlocked.Increment(ref counter)}";
    }
}
```

### Utility Classes Design

#### UseControllableState<T>
```csharp
public class UseControllableState<T>
{
    private T? uncontrolledValue;

    public T Value => IsControlled ? ControlledValue! : uncontrolledValue!;
    public bool IsControlled => ControlledValue != null;

    public T? ControlledValue { get; set; }
    public EventCallback<T> OnValueChanged { get; set; }

    public async Task SetValue(T value)
    {
        if (!IsControlled)
        {
            uncontrolledValue = value;
        }
        await OnValueChanged.InvokeAsync(value);
    }
}
```

#### AriaBuilder
```csharp
public class AriaBuilder
{
    private readonly Dictionary<string, object?> attributes = new();

    public AriaBuilder Role(string role) => Set("role", role);
    public AriaBuilder Label(string? label) => Set("aria-label", label);
    public AriaBuilder LabelledBy(string? id) => Set("aria-labelledby", id);
    public AriaBuilder DescribedBy(string? id) => Set("aria-describedby", id);
    public AriaBuilder Expanded(bool? expanded) => Set("aria-expanded", expanded?.ToString()?.ToLower());
    public AriaBuilder Hidden(bool? hidden) => Set("aria-hidden", hidden?.ToString()?.ToLower());
    public AriaBuilder Modal(bool modal) => Set("aria-modal", modal.ToString().ToLower());

    public Dictionary<string, object> Build() => attributes.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value!);
}
```

#### KeyboardNavigator
```csharp
public class KeyboardNavigator
{
    public bool HandleArrowNavigation(KeyboardEventArgs e, NavigationOptions options)
    {
        return e.Key switch
        {
            "ArrowUp" => options.Orientation != Orientation.Horizontal && MoveSelection(-1, options),
            "ArrowDown" => options.Orientation != Orientation.Horizontal && MoveSelection(1, options),
            "ArrowLeft" => options.Orientation != Orientation.Vertical && MoveSelection(options.IsRtl ? 1 : -1, options),
            "ArrowRight" => options.Orientation != Orientation.Vertical && MoveSelection(options.IsRtl ? -1 : 1, options),
            "Home" => MoveToFirst(options),
            "End" => MoveToLast(options),
            _ => false
        };
    }
}
```

### JavaScript Interop Strategy

Create focused, minimal JavaScript modules:

```javascript
// wwwroot/js/primitives/focus-trap.js
export function createFocusTrap(element, options) {
    const focusableSelectors = 'a[href], button:not([disabled]), input:not([disabled])...';
    let firstFocusable, lastFocusable;

    function handleKeyDown(e) {
        if (e.key !== 'Tab') return;
        // Trap focus logic
    }

    element.addEventListener('keydown', handleKeyDown);
    return () => element.removeEventListener('keydown', handleKeyDown);
}

// wwwroot/js/primitives/click-outside.js
export function onClickOutside(element, dotNetRef) {
    function handleClick(e) {
        if (!element.contains(e.target)) {
            dotNetRef.invokeMethodAsync('HandleClickOutside');
        }
    }

    document.addEventListener('click', handleClick);
    return () => document.removeEventListener('click', handleClick);
}
```

### CascadingValue Pattern for Context

```csharp
// DialogContext.cs
public class DialogContext
{
    public string DialogId { get; set; } = IdGenerator.GenerateId("dialog");
    public bool IsOpen { get; set; }
    public EventCallback<bool> OnOpenChanged { get; set; }

    public string TriggerId => $"{DialogId}-trigger";
    public string ContentId => $"{DialogId}-content";
    public string TitleId => $"{DialogId}-title";
    public string DescriptionId => $"{DialogId}-description";
}

// Dialog.razor
<CascadingValue Value="context" IsFixed="true">
    @ChildContent
</CascadingValue>

// DialogTrigger.razor
[CascadingParameter] private DialogContext Context { get; set; } = null!;
```

### EventCallback Pattern for State Management

```csharp
// Support both controlled and uncontrolled patterns
[Parameter] public bool? Open { get; set; }  // Controlled if not null
[Parameter] public EventCallback<bool> OpenChanged { get; set; }

// Two-way binding support
[Parameter] public string? Value { get; set; }
[Parameter] public EventCallback<string> ValueChanged { get; set; }

// Enable @bind-Value syntax
private async Task UpdateValue(string newValue)
{
    Value = newValue;
    await ValueChanged.InvokeAsync(newValue);
}
```

## 3. Component Design Patterns

### Controlled vs Uncontrolled State Pattern

Every primitive supports both patterns:

```csharp
// Uncontrolled (manages own state)
<DialogPrimitive>
    <DialogTrigger>Open</DialogTrigger>
    <DialogContent>Content</DialogContent>
</DialogPrimitive>

// Controlled (parent manages state)
<DialogPrimitive @bind-Open="isDialogOpen">
    <DialogTrigger>Open</DialogTrigger>
    <DialogContent>Content</DialogContent>
</DialogPrimitive>

@code {
    private bool isDialogOpen;
}
```

### Composition via RenderFragment

Primitives use composition for flexibility:

```csharp
[Parameter] public RenderFragment? ChildContent { get; set; }
[Parameter] public RenderFragment? Trigger { get; set; }
[Parameter] public RenderFragment? Content { get; set; }
[Parameter] public RenderFragment<T>? ItemTemplate { get; set; }
```

### Portal Rendering Approach

```csharp
// DialogPortal.razor
@inject IPortalService PortalService

@code {
    protected override void OnInitialized()
    {
        if (ShouldRenderInPortal)
        {
            PortalService.RegisterPortal(portalId, ChildContent);
        }
    }

    public void Dispose()
    {
        PortalService.UnregisterPortal(portalId);
    }
}
```

### Focus Management Strategy

```csharp
public class DialogContent : ComponentBase
{
    @Inject private IFocusManager FocusManager { get; set; } = null!;
    private ElementReference? previousActiveElement;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && IsOpen)
        {
            previousActiveElement = await GetActiveElement();
            await FocusManager.TrapFocus(contentRef);
        }
    }

    private async Task HandleClose()
    {
        await FocusManager.RestoreFocus(previousActiveElement);
    }
}
```

### Keyboard Navigation Framework

```csharp
public abstract class NavigableListPrimitive : ComponentBase
{
    protected KeyboardNavigator Navigator { get; } = new();

    protected virtual async Task HandleKeyDown(KeyboardEventArgs e)
    {
        var handled = e.Key switch
        {
            "Enter" or " " => await SelectCurrentItem(),
            "Escape" => await Cancel(),
            _ => Navigator.HandleArrowNavigation(e, GetNavigationOptions())
        };

        if (handled) e.PreventDefault();
    }
}
```

## 4. Implementation Phases

### Phase 1: Infrastructure Foundation (Complexity: High)
**Goal:** Establish core services and utilities that all primitives will depend on

**Components/Files to Create:**
```
Shared/
├── Services/
│   ├── PortalService.cs
│   ├── FocusManager.cs
│   ├── PositioningService.cs
│   └── IdGenerator.cs
├── Utilities/
│   ├── UseControllableState.cs
│   ├── AriaBuilder.cs
│   └── KeyboardNavigator.cs
├── Contexts/
│   └── PrimitiveContext.cs
└── Extensions/
    └── ServiceCollectionExtensions.cs

wwwroot/js/primitives/
├── focus-trap.js
├── positioning.js
├── portal.js
└── click-outside.js
```

**Key Technical Decisions:**
- Use dependency injection for services
- Implement disposal patterns for JS interop cleanup
- Create base classes for common primitive patterns
- Establish naming conventions (Primitive suffix for headless components)

**Patterns to Establish:**
- Service registration in DI container
- JS module loading and disposal
- Event callback patterns for state updates
- Context sharing via CascadingValue

**Dependencies:**
- Install @floating-ui/dom via npm
- Create JS build pipeline for modules
- Configure static file serving for JS

### Phase 2: Dialog Primitive - Reference Implementation (Complexity: Very High)
**Goal:** Create the most complex primitive as a reference for all others

**Components/Files to Create:**
```
Primitives/Dialog/
├── DialogPrimitive.razor
├── DialogTrigger.razor
├── DialogPortal.razor
├── DialogOverlay.razor
├── DialogContent.razor
├── DialogTitle.razor
├── DialogDescription.razor
├── DialogClose.razor
├── DialogContext.cs
└── DialogState.cs

Components/Dialog/
├── Dialog.razor (styled wrapper)
├── DialogHeader.razor
├── DialogFooter.razor
└── Dialog.razor.css
```

**Key Technical Decisions:**
- Portal rendering at body level for z-index management
- Focus trap implementation with tab cycling
- Body scroll lock when open
- Escape key handling
- Click outside to close (configurable)

**Patterns to Establish:**
- Context pattern for component communication
- Controlled/uncontrolled state handling
- ARIA attribute application
- Animation hooks (for future)

**Dependencies:**
- PortalService must be complete
- FocusManager must be complete
- JS interop modules must be loaded

### Phase 3: Refactor Existing Components (Complexity: High)
**Goal:** Migrate existing styled components to primitive architecture

**Components to Refactor (Priority Order):**
1. **Popover** → PopoverPrimitive + Popover
   - Uses PositioningService
   - Portal rendering
   - Click outside handling

2. **Dropdown Menu** → DropdownMenuPrimitive + DropdownMenu
   - Complex keyboard navigation
   - Nested menu support
   - Uses PositioningService

3. **Tooltip** → TooltipPrimitive + Tooltip
   - Hover/focus triggers
   - Delay configuration
   - Uses PositioningService

4. **Collapsible** → CollapsiblePrimitive + Collapsible
   - Simple open/close state
   - Animation ready

5. **Checkbox** → CheckboxPrimitive + Checkbox
   - Indeterminate state
   - Form integration

6. **Radio Group** → RadioGroupPrimitive + RadioGroup
   - Arrow key navigation
   - Group state management

7. **Switch** → SwitchPrimitive + Switch
   - Toggle state
   - ARIA switch role

8. **Label** → LabelPrimitive + Label
   - For attribute handling
   - Click to focus

9. **Select** → SelectPrimitive + Select
   - Complex dropdown
   - Search/filter ready
   - Virtual scrolling ready

**Key Technical Decisions:**
- Maintain backward compatibility where possible
- Keep existing public API if not problematic
- Add deprecation warnings for breaking changes

**Patterns to Establish:**
- Migration pattern from styled to primitive
- Backward compatibility approach
- Testing strategy for regressions

**Dependencies:**
- Phase 1 infrastructure complete
- Dialog primitive as reference

### Phase 4: New Priority Primitives (Complexity: Medium)
**Goal:** Add high-value primitives not yet implemented

**Tabs Primitive:**
```
Primitives/Tabs/
├── TabsPrimitive.razor
├── TabsList.razor
├── TabsTrigger.razor
├── TabsContent.razor
└── TabsContext.cs

Components/Tabs/
└── Tabs.razor (styled)
```
- Arrow key navigation
- Automatic/manual activation
- Vertical/horizontal orientation
- ARIA tabs pattern

**Accordion Primitive:**
```
Primitives/Accordion/
├── AccordionPrimitive.razor
├── AccordionItem.razor
├── AccordionTrigger.razor
├── AccordionContent.razor
└── AccordionContext.cs

Components/Accordion/
└── Accordion.razor (styled)
```
- Single/multiple expansion
- Arrow key navigation
- Collapse animation ready
- ARIA accordion pattern

**Hover Card Primitive:**
```
Primitives/HoverCard/
├── HoverCardPrimitive.razor
├── HoverCardTrigger.razor
├── HoverCardContent.razor
└── HoverCardContext.cs

Components/HoverCard/
└── HoverCard.razor (styled)
```
- Hover with delay
- Portal rendering
- Uses PositioningService
- Keyboard accessible

**Key Technical Decisions:**
- Follow established patterns from Dialog
- Consistent keyboard navigation patterns
- Consistent ARIA implementation

**Dependencies:**
- Previous phases complete
- Patterns well established

### Phase 5: Demo Site Restructure (Complexity: Medium)
**Goal:** Showcase both primitive and styled layers effectively

**Structure Changes:**
```
Demo/Pages/
├── Index.razor (updated landing)
├── GettingStarted.razor (architecture explanation)
├── Primitives/
│   ├── DialogPrimitive.razor
│   ├── PopoverPrimitive.razor
│   └── ... (all primitives)
├── Components/
│   ├── DialogDemo.razor
│   ├── ButtonDemo.razor
│   └── ... (all styled)
└── Shared/
    ├── CodeBlock.razor
    ├── PropsTable.razor
    ├── ExampleSection.razor
    └── KeyboardShortcuts.razor
```

**Navigation Updates:**
- Add Primitives section
- Add Components section
- Architecture overview page
- Migration guide (internal use)

**Demo Features:**
- Show primitive with minimal styling
- Show styled component
- Show composition examples
- Show controlled/uncontrolled examples
- Display keyboard shortcuts
- Show ARIA attributes

**Dependencies:**
- Primitives implemented
- Components refactored

### Phase 6: Testing & Documentation (Complexity: High)
**Goal:** Ensure quality and accessibility standards

**Testing Approach:**
```csharp
// Use Playwright MCP for testing
- Navigate to demo pages
- Test keyboard navigation sequences
- Verify ARIA attributes present
- Test focus management
- Test portal rendering
- Cross-browser validation
```

**Documentation Updates:**
```
.devflow/architecture.md (update with primitives layer)
README.md (explain two-tier architecture)

Primitives/{Component}/README.md:
- API surface documentation
- Usage examples
- Composition patterns
- ARIA implementation notes
```

**Quality Checklist:**
- [ ] All primitives keyboard navigable
- [ ] ARIA attributes correct
- [ ] Focus management working
- [ ] RTL support verified
- [ ] Screen reader tested
- [ ] Cross-browser tested
- [ ] Blazor Server tested
- [ ] Blazor WASM tested

**Dependencies:**
- All implementation complete
- Demo site ready for testing

## 5. Cross-Cutting Concerns Integration

### Theming Support

**Primitives (Headless):**
```razor
<!-- No theme classes, pure behavior -->
<button @onclick="HandleClick"
        role="button"
        aria-label="@AriaLabel"
        @attributes="AdditionalAttributes">
    @ChildContent
</button>
```

**Styled Components:**
```razor
<!-- Applies theme via Tailwind + CSS variables -->
<ButtonPrimitive class="bg-primary text-primary-foreground hover:bg-primary/90
                       rounded-md px-4 py-2 transition-colors"
                @onclick="HandleClick">
    @ChildContent
</ButtonPrimitive>
```

Key Points:
- Primitives accept `class` and `style` parameters but don't apply defaults
- Styled components layer Tailwind classes using CSS variables
- Dark mode handled entirely at styled layer via `.dark` class

### Internationalization Support

**RTL Support in Primitives:**
```csharp
// KeyboardNavigator handles RTL
protected bool IsRtl => CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;

protected void HandleArrowKey(KeyboardEventArgs e)
{
    var direction = e.Key switch
    {
        "ArrowLeft" => IsRtl ? 1 : -1,
        "ArrowRight" => IsRtl ? -1 : 1,
        _ => 0
    };
}
```

**Logical Properties in Styled Components:**
```css
.dialog-content {
    margin-inline-start: auto;  /* Not margin-left */
    padding-inline: 1rem;       /* Not padding-left/right */
    text-align: start;          /* Not text-align: left */
}
```

**Parameterized Strings:**
```csharp
[Parameter] public string CloseLabel { get; set; } = "Close";
[Parameter] public string OpenLabel { get; set; } = "Open";
```

## 6. Testing Strategy

### Playwright MCP Integration Approach

```csharp
// Test keyboard navigation
await page.GotoAsync("/primitives/dialog");
await page.Keyboard.PressAsync("Tab");  // Focus trigger
await page.Keyboard.PressAsync("Enter"); // Open dialog
await page.Keyboard.PressAsync("Tab");   // Focus trap cycles
await page.Keyboard.PressAsync("Escape"); // Close dialog

// Verify ARIA
var dialog = await page.QuerySelectorAsync("[role='dialog']");
Assert.NotNull(dialog);
var ariaModal = await dialog.GetAttributeAsync("aria-modal");
Assert.Equal("true", ariaModal);
```

### Keyboard Navigation Testing

Test Matrix:
- Tab/Shift+Tab: Focus navigation
- Enter/Space: Activation
- Escape: Cancel/close
- Arrow keys: List navigation
- Home/End: Jump to first/last
- Type-ahead: Quick selection (future)

### Screen Reader Testing

Tools:
- NVDA (Windows)
- JAWS (Windows)
- VoiceOver (macOS)

Verify:
- Role announcements
- Label reading
- State changes announced
- Description reading
- Navigation instructions

### Cross-Browser Testing

Browsers:
- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

Test:
- Rendering consistency
- JS interop functionality
- Keyboard navigation
- Focus indicators

### Hosting Model Testing

**Blazor Server:**
- SignalR connection handling
- Latency impact on interactions
- Disposal of JS interop

**Blazor WebAssembly:**
- Bundle size impact
- Initial load performance
- JS interop performance

## 7. Risk Mitigation

### JavaScript Interop Complexity

**Risk:** Different behavior between Server/WASM
**Mitigation:**
- Abstract JS calls behind service interfaces
- Implement retry logic for Server mode
- Use `ElementReference` carefully
- Proper disposal patterns for event listeners

```csharp
public async ValueTask DisposeAsync()
{
    if (jsModule != null)
    {
        await jsModule.InvokeVoidAsync("dispose", elementRef);
        await jsModule.DisposeAsync();
    }
}
```

### Portal Rendering Challenges

**Risk:** Blazor lacks native portal support
**Mitigation:**
- Service-based approach with lifecycle management
- Clear ownership of portal content
- Automatic cleanup on disposal
- Z-index management strategy

```csharp
public class PortalService : IDisposable
{
    private readonly Dictionary<string, PortalEntry> portals = new();

    public void Dispose()
    {
        foreach (var portal in portals.Values)
        {
            portal.Dispose();
        }
    }
}
```

### Refactoring Regression Risks

**Risk:** Breaking existing functionality
**Mitigation:**
- Keep parallel implementations during transition
- Extensive manual testing with Playwright
- Maintain backward compatible APIs where possible
- Feature flag for primitive mode (if needed)

```csharp
// Temporary compatibility layer
[Obsolete("Use DialogPrimitive for new implementations")]
public partial class LegacyDialog : ComponentBase { }
```

## 8. Architecture Impact Assessment

### Rating: MAJOR

This represents a fundamental architectural shift in how components are structured:

1. **New abstraction layer** (Primitives) added to the architecture
2. **Separation of concerns** between behavior and presentation
3. **New service infrastructure** for portal, focus, positioning
4. **JavaScript interop strategy** formalized
5. **Component composition patterns** established
6. **State management patterns** (controlled/uncontrolled) standardized

### Justification

This is a MAJOR change because it:
- Fundamentally changes the component architecture from single-layer to two-tier
- Introduces new infrastructure services that all components will depend on
- Requires refactoring every existing component
- Establishes patterns that all future components must follow
- Changes the mental model for how components are built and used

### Benefits Despite Complexity

- **Better maintainability:** Behavior separated from presentation
- **Improved accessibility:** Centralized, consistent implementation
- **Greater flexibility:** Developers can use primitives or styled components
- **Easier testing:** Can test behavior without styling concerns
- **Following proven patterns:** Aligns with Radix UI + shadcn success

## 9. Complexity Estimation

### Overall Complexity: Very High

**Breakdown by Phase:**
- Phase 1 (Infrastructure): High - New services, JS interop setup
- Phase 2 (Dialog Primitive): Very High - Most complex component, establishes patterns
- Phase 3 (Refactoring): High - Many components, regression risk
- Phase 4 (New Primitives): Medium - Following established patterns
- Phase 5 (Demo Site): Medium - Straightforward but extensive
- Phase 6 (Testing): High - Comprehensive validation needed

### Estimated Number of Tasks: 75-85

**Task Distribution:**
- Infrastructure: 12-15 tasks
- Dialog Primitive: 15-18 tasks
- Refactoring (9 components): 25-30 tasks
- New Primitives (3): 10-12 tasks
- Demo Site: 8-10 tasks
- Testing & Documentation: 5-8 tasks

### Key Challenges

1. **JS Interop Complexity:**
   - Managing lifecycle across hosting models
   - Memory leak prevention
   - Event handler cleanup

2. **State Management Patterns:**
   - Supporting both controlled and uncontrolled
   - Complex state synchronization
   - Cascading context updates

3. **Portal Rendering:**
   - No native Blazor support
   - Z-index management
   - Scroll locking

4. **Backward Compatibility:**
   - Avoiding breaking changes where possible
   - Migration path for consumers

5. **Testing Coverage:**
   - No automated testing framework
   - Manual testing burden
   - Accessibility validation

## 10. Implementation Order

### Suggested Task Grouping

**Group 1: Foundation (Week 1)**
- Set up infrastructure services
- Implement JS interop modules
- Create base primitive classes
- Establish patterns

**Group 2: Reference Implementation (Week 2)**
- Build Dialog primitive completely
- Create styled Dialog wrapper
- Comprehensive testing
- Document patterns

**Group 3: Core Refactoring (Week 3-4)**
- Refactor Popover, Dropdown, Tooltip
- These share positioning needs
- Similar complexity level

**Group 4: Simple Refactoring (Week 4)**
- Refactor Checkbox, Radio, Switch, Label
- Simpler components
- Quick wins

**Group 5: Complex Refactoring (Week 5)**
- Refactor Select, Collapsible
- More complex state/interaction

**Group 6: New Primitives (Week 5-6)**
- Implement Tabs, Accordion, HoverCard
- Follow established patterns

**Group 7: Demo & Polish (Week 6)**
- Update demo site
- Documentation
- Final testing

## Summary

This architectural transformation will establish BlazorUI as a mature, production-ready component library with clear separation between behavioral primitives and styled components. While complex, the benefits of improved maintainability, accessibility, and flexibility justify the investment.

---

**Complexity:** Very High
**Estimated Tasks:** 75-85
**Risk Level:** High (mitigated through phased approach)
**Architecture Impact:** MAJOR
**Requires Review:** true

**Next Steps:**
1. Review and approve this plan
2. Run `/devflow:tasks` to break down into atomic tasks
3. Begin implementation with Phase 1 (Infrastructure)
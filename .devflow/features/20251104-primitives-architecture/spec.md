# Feature Specification: Radix UI-Style Primitives Architecture

**Status:** Pending
**Created:** 2025-11-04T00:00:00Z
**Feature ID:** 20251104-primitives-architecture

## Problem Statement

The current BlazorUI library mixes presentation (styling) with behavior (accessibility, keyboard navigation, state management) in a single layer. This makes it difficult to customize component appearance without modifying behavior, reuse complex interaction patterns across different styled components, maintain consistency in accessibility and keyboard navigation, and follow the proven shadcn/ui architecture that separates Radix UI primitives (headless behavior) from styled components.

## Goals and Objectives

- Provide unstyled, headless primitive components that handle all behavior, accessibility, and state management
- Enable developers to build custom styled components on top of primitives with full control over appearance
- Support component composition patterns to create complex UI interactions
- Offer both primitives (for full customization) and pre-styled shadcn components (for quick implementation)
- Deliver built-in accessibility features (ARIA, keyboard navigation, focus management) without manual implementation
- Support both controlled and uncontrolled component state patterns

## User Stories

1. **As a developer using BlazorUI**, I want headless primitive components, so that I can build custom-styled components without reimplementing complex behavior and accessibility features.

2. **As a developer**, I want pre-styled shadcn components built on primitives, so that I can quickly add beautiful UI without customization when I don't need it.

3. **As a developer**, I want clear separation between behavior (primitives) and styling (components), so that I can understand the architecture and maintain my code more easily.

4. **As a developer**, I want comprehensive demo pages showing both primitives and styled components, so that I can learn how to use each layer and see composition patterns.

5. **As an accessibility-focused developer**, I want all primitives to handle ARIA attributes, keyboard navigation, and focus management automatically, so that my applications are accessible by default.

## Acceptance Criteria

### Infrastructure Foundation
- [ ] Infrastructure services created (PortalService, FocusManager, PositioningService, IdGenerator)
- [ ] Utility classes implemented (UseControllableState<T>, AriaBuilder, KeyboardNavigator)
- [ ] JavaScript interop modules created (focus-trap.js, positioning.js, portal.js, click-outside.js)
- [ ] Base PrimitiveContext<TState> class created for shared context patterns

### Dialog Primitive (Reference Implementation)
- [ ] Dialog primitive fully implemented with all sub-components (Dialog, DialogTrigger, DialogPortal, DialogOverlay, DialogContent, DialogTitle, DialogDescription, DialogClose)
- [ ] Dialog primitive supports controlled and uncontrolled state patterns
- [ ] Dialog primitive implements focus trap with JS interop
- [ ] Dialog primitive handles escape key to close
- [ ] Dialog primitive supports click-outside-to-close (optional)
- [ ] Dialog primitive implements body scroll locking
- [ ] Dialog primitive has full ARIA support (role="dialog", aria-modal, aria-labelledby, aria-describedby)
- [ ] Dialog primitive restores focus to trigger on close
- [ ] Styled Dialog component created as wrapper around Dialog primitive with shadcn classes
- [ ] Dialog primitive demo page created showing unstyled behavior
- [ ] Dialog styled component demo page created with real-world examples

### Refactored Existing Components
- [ ] Popover refactored to primitive layer with PositioningService integration
- [ ] Dropdown Menu refactored to primitive layer with keyboard navigation
- [ ] Tooltip refactored to primitive layer
- [ ] Collapsible refactored to primitive layer
- [ ] Checkbox refactored to primitive layer
- [ ] Radio Group refactored to primitive layer
- [ ] Switch refactored to primitive layer
- [ ] Label refactored to primitive layer
- [ ] Select refactored to primitive layer
- [ ] Each refactored component has primitive demo page
- [ ] Each refactored component has styled component demo page

### New Priority Primitives
- [ ] Tabs primitive implemented (TabsList, TabsTrigger, TabsContent) with arrow key navigation
- [ ] Tabs primitive supports controlled/uncontrolled active tab
- [ ] Tabs primitive supports horizontal/vertical orientation
- [ ] Styled Tabs component created
- [ ] Accordion primitive implemented (AccordionItem, AccordionTrigger, AccordionContent)
- [ ] Accordion primitive supports single/multiple mode
- [ ] Accordion primitive has keyboard navigation
- [ ] Styled Accordion component created
- [ ] Hover Card primitive implemented with hover triggering and delay controls
- [ ] Styled Hover Card component created
- [ ] Tabs, Accordion, and Hover Card have demo pages (primitive + styled)

### Demo Site Restructure
- [ ] Demo navigation updated with Primitives and Components sections
- [ ] Demo site folder structure reorganized (Primitives/ and Components/ folders)
- [ ] Shared demo utilities created (CodeBlock, PropsTable, ExampleSection, KeyboardShortcuts)
- [ ] Landing page updated with architecture overview
- [ ] Getting Started page updated with primitives explanation

### Testing & Quality
- [ ] All primitives pass keyboard-only navigation testing
- [ ] All primitives tested in Blazor Server
- [ ] All primitives tested in Blazor WebAssembly
- [ ] Playwright MCP used for UI validation during development
- [ ] Screen reader testing performed (NVDA or JAWS)
- [ ] Cross-browser testing completed (Chrome, Firefox, Safari, Edge)

### Documentation
- [ ] Architecture documentation updated with primitives vs components separation
- [ ] API patterns documented (controlled/uncontrolled, composition, CascadingValue)
- [ ] JavaScript interop guidelines documented
- [ ] Each primitive has README with API surface and usage examples

## Technical Requirements

### Architecture
- **Two-tier structure:** `Primitives/` (headless behavior) + `Components/` (styled wrappers)
- **Context pattern:** Use Blazor's `CascadingValue` for component context communication
- **State management:** `EventCallback<T>` for state updates with `@bind-` two-way binding support
- **Composition:** `RenderFragment` parameters for flexible component composition
- **Service layer:** Shared services for Portal, Focus, Positioning, and ID generation

### Folder Structure
```
src/BlazorUI/
├── Primitives/                    # NEW: Headless primitives
│   ├── Dialog/
│   ├── Popover/
│   ├── Tabs/
│   └── ... (other primitives)
│
├── Components/                    # REFACTORED: Styled wrappers
│   ├── Dialog/
│   ├── Button/
│   └── ... (styled components)
│
├── Shared/
│   ├── Services/                  # NEW: Shared services
│   │   ├── PortalService.cs
│   │   ├── FocusManager.cs
│   │   ├── PositioningService.cs
│   │   └── IdGenerator.cs
│   ├── Utilities/                 # NEW: Helper classes
│   │   ├── UseControllableState.cs
│   │   ├── AriaBuilder.cs
│   │   └── KeyboardNavigator.cs
│   └── Contexts/                  # NEW: Base contexts
│       └── PrimitiveContext.cs
│
└── wwwroot/js/primitives/         # NEW: JS interop
    ├── focus-trap.js
    ├── positioning.js
    ├── portal.js
    └── click-outside.js
```

### JavaScript Interop
- **Floating UI:** Use `@floating-ui/dom` for positioning overlays (Popover, Dropdown, Tooltip)
- **Focus Management:** Custom or library-based focus trap for dialogs
- **Portal Rendering:** Service-based approach with optional JS assistance
- **Click Outside:** Document-level event listener with DotNetObjectReference callbacks

### Hosting Model Compatibility
- **Blazor Server:** Full support (primary testing target)
- **Blazor WebAssembly:** Full support (test for bundle size)
- **Blazor Hybrid (MAUI):** Best effort (portal rendering may need adaptation)

### Accessibility Standards
- **WCAG 2.1 AA compliance**
- **Full keyboard navigation** (Tab, Enter, Space, Escape, Arrow keys)
- **ARIA attributes** automatically applied (roles, labels, descriptions, states)
- **Focus management** with visual indicators
- **Screen reader compatibility** (test with NVDA or JAWS)

### Testing Strategy
- **Playwright MCP:** Use during development for UI validation
- **Manual keyboard testing:** All primitives must be fully keyboard-navigable
- **Manual screen reader testing:** Verify ARIA implementation
- **Cross-browser testing:** Chrome, Firefox, Safari, Edge
- **Hosting model testing:** Blazor Server and WebAssembly

## Dependencies

### External Dependencies
- **@floating-ui/dom** - JavaScript positioning library (via npm/CDN)
- **focus-trap** library or custom implementation - Focus management
- **Tailwind CSS** - Already in place
- **CSS Variables** - Already in place

### Internal Dependencies
- Existing component implementations (to be refactored)
- Existing demo site infrastructure
- Current theming system

### No Blocking Dependencies
This feature can proceed independently. No other features must be completed first.

## Cross-Cutting Concerns

### Theming System
**File:** `.devflow/domains/design/theming.md`

Primitives must support the existing CSS variable theming system. Styled components will apply shadcn theme classes on top of primitives.

**Impact:**
- Primitives should not apply theme classes (headless)
- Styled components use Tailwind classes with CSS variable tokens
- Both layers must respect light/dark mode via CSS variable overrides

### Internationalization (i18n)
**File:** `.devflow/domains/infrastructure/internationalization.md`

Primitives should use CSS logical properties for RTL layout support. Component structure should support localization of labels and messages.

**Impact:**
- Use `margin-inline-start` instead of `margin-left`
- Use `padding-block` instead of `padding-top/bottom`
- Parameterize all user-facing strings
- Support `dir="rtl"` attribute propagation

## Risks and Challenges

### Technical Risks
1. **JavaScript interop complexity**
   - Focus traps may behave differently in Blazor Server (SignalR) vs WebAssembly
   - Memory leaks if DotNetObjectReference not properly disposed
   - **Mitigation:** Test extensively in both hosting models, use proper disposal patterns

2. **Portal rendering challenges**
   - Blazor doesn't have native portal support like React
   - Service-based approach is novel and may have edge cases
   - **Mitigation:** Implement simple MVP first, iterate based on testing

3. **Refactoring regressions**
   - Moving existing components could break functionality
   - **Mitigation:** Use Playwright testing to validate each refactored component

### Architectural Risks
1. **Breaking API changes**
   - Component property names or structure may change
   - **Mitigation:** Project is pre-release, breaking changes acceptable

2. **Complexity for new developers**
   - Two-tier architecture adds learning curve
   - **Mitigation:** Comprehensive demo pages and documentation

3. **Visual parity with shadcn**
   - Refactoring may affect visual appearance
   - **Mitigation:** Use Playwright for visual regression testing

### Scope Risk
1. **Large implementation scope**
   - Many components to refactor, easy to get stuck
   - **Mitigation:** Implement Dialog first as reference, then iterate on patterns

## Out of Scope

### Explicitly NOT Included

**Advanced Primitives (v2.0+):**
- Context Menu primitive
- Navigation Menu primitive
- Menubar primitive
- Slider primitive (multiple thumbs)
- Advanced features like nested dialogs

**Advanced Features (Future Iterations):**
- Typeahead search in dropdowns
- Complex positioning options beyond basic Floating UI
- Animation/transition customization system
- Advanced portal rendering optimizations

**Testing Infrastructure:**
- Automated test suite with bUnit
- CI/CD integration for automated testing
- Visual regression testing automation

**Documentation:**
- Migration guide for external users (project is pre-release)
- Interactive playground beyond basic demos
- Video tutorials or walkthroughs

**Performance Optimizations:**
- Bundle size optimization beyond basic code splitting
- Advanced rendering optimizations
- Lazy loading strategies

### Future Considerations

These items are acknowledged but deferred to future work:
- Component variants system (beyond basic parameters)
- Advanced theme customization UI
- Component CLI for scaffolding
- Storybook or similar component documentation tool

---

**Next Steps:**
1. Run `/devflow:plan` to generate detailed technical implementation plan
2. Run `/devflow:tasks` to break plan into atomic, executable tasks
3. Run `/devflow:execute` to begin implementation with quality gates

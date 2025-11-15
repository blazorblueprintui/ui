# Feature Specification: Button Group Component

**Created:** 2025-11-13T00:00:00Z
**Feature ID:** 20251113-button-group
**Workflow:** Build Feature (Streamlined)

---

## Description

Create a ButtonGroup component that visually connects related buttons together following the Shadcn UI design pattern. The component will group buttons with shared borders and rounded corners only on the outer edges, supporting both horizontal and vertical orientations. This includes the core ButtonGroup component plus ButtonGroupText and ButtonGroupSeparator sub-components for advanced layouts.

---

## Rationale

Button groups are a common UI pattern for grouping related actions together (e.g., text formatting, toolbar actions, navigation). By providing a pre-styled ButtonGroup component that follows the shadcn/ui design system, users can quickly create professional-looking grouped buttons without manually managing borders and styling between adjacent buttons.

---

## Acceptance Criteria

- Core ButtonGroup component created with horizontal and vertical orientation support
- ButtonGroupText sub-component for text/label sections within groups
- ButtonGroupSeparator sub-component for visual dividers within groups
- Demo page with multiple examples:
  - Basic horizontal button group
  - Vertical button group
  - Nested button groups with gaps
  - Complex example with dropdown menu integration (similar to Shadcn demo)
  - Icon-only button groups
  - Mixed content groups using ButtonGroupText and ButtonGroupSeparator
- Component properly handles focus states (z-index management)
- Automatic border and rounded corner styling for connected appearance
- ButtonGroup added to Components index page (/components)
- Navigation link added to main menu (NavMenu.razor)
- All examples use Lucide icons from the existing icon library
- Component follows existing architecture patterns (similar to Button, InputGroup)
- Comprehensive XML documentation for all components

---

## Files Affected

**New Files:**
- `src/BlazorUI.Components/Components/ButtonGroup/ButtonGroup.razor`
- `src/BlazorUI.Components/Components/ButtonGroup/ButtonGroup.razor.cs`
- `src/BlazorUI.Components/Components/ButtonGroup/ButtonGroupText.razor`
- `src/BlazorUI.Components/Components/ButtonGroup/ButtonGroupText.razor.cs`
- `src/BlazorUI.Components/Components/ButtonGroup/ButtonGroupSeparator.razor`
- `src/BlazorUI.Components/Components/ButtonGroup/ButtonGroupSeparator.razor.cs`
- `demo/BlazorUI.Demo/Pages/Components/ButtonGroup.razor`

**Modified Files:**
- `demo/BlazorUI.Demo/Pages/Components/Index.razor` (add ButtonGroup link)
- `demo/BlazorUI.Demo/Shared/NavMenu.razor` (add navigation item)

---

## Dependencies

- Existing Button component (BlazorUI.Components.Button)
- Existing LucideIcon component (for demo examples)
- Existing Separator component (for ButtonGroupSeparator implementation)
- TailwindMerge utility (ClassNames.cn)
- Tailwind CSS (styling)

---

**Next Step:** Auto-generate tasks and begin implementation with `/execute`

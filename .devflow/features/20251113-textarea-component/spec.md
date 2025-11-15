# Feature Specification: Textarea Component

**Created:** 2025-11-13T00:00:00.000Z
**Feature ID:** 20251113-textarea-component
**Workflow:** Build Feature (Streamlined)

---

## Description

Create a Textarea component following the shadcn-ui design pattern with field-sizing-content, responsive text sizing, and aria-invalid states. The component will match the existing Input component architecture with full two-way binding support, MaxLength parameter, and comprehensive accessibility features.

---

## Rationale

Provides a multi-line text input component that complements the existing Input component, following consistent design patterns and accessibility standards from the shadcn-ui design system.

---

## Acceptance Criteria

- Textarea component created in `src/BlazorUI.Components/Components/Textarea/`
- Full two-way binding support with `@bind-Value` syntax
- MaxLength parameter for character limit validation
- Follows shadcn-ui styling:
  - `field-sizing-content` for auto-sizing
  - `min-h-16` for minimum height
  - `aria-invalid` state styling with destructive colors
  - Dark mode support via CSS variables
- Comprehensive ARIA attributes (aria-label, aria-describedby, aria-invalid)
- Disabled and required states
- Placeholder text support
- Demo page created with multiple usage examples
- Component follows existing codebase patterns (matches Input component structure)

---

## Files Affected

- `src/BlazorUI.Components/Components/Textarea/Textarea.razor` (new)
- `src/BlazorUI.Components/Components/Textarea/Textarea.razor.cs` (new)
- `demo/BlazorUI.Demo/Pages/Components/TextareaDemo.razor` (new)

---

## Dependencies

None - uses existing `BlazorUI.Components.Utilities` (ClassNames.cn) and Tailwind CSS

---

**Next Step:** Auto-generate tasks and begin implementation with `/execute`

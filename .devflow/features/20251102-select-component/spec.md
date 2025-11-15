# Feature Specification: Select Component

**Created:** 2025-11-02T11:22:00.000Z
**Feature ID:** 20251102-select-component
**Workflow:** Build Feature (Streamlined)

---

## Description

Create a composable Select (dropdown) component based on shadcn/ui design with support for generic TValue binding, cascading parameters for parent-child communication, JavaScript interop for smart positioning, and multiple sub-components (Select, SelectTrigger, SelectContent, SelectItem, SelectValue, SelectGroup, SelectLabel).

---

## Rationale

Provides an essential form input component for the BlazorUI library, allowing users to choose from a list of options with accessible keyboard navigation and ARIA support, following shadcn/ui's composable architecture pattern.

---

## Acceptance Criteria

- Generic Select<TValue> component with @bind-Value support
- Sub-components: SelectTrigger, SelectContent, SelectItem, SelectValue, SelectGroup, SelectLabel
- Cascading parameters for parent-child communication
- JavaScript interop for smart dropdown positioning (handles viewport edges)
- Keyboard navigation (arrow keys, enter, escape)
- ARIA attributes for accessibility
- Demo page with basic select, grouped options, and scrollable list examples
- Tailwind CSS styling matching OKLCH theme system

---

## Files Affected

**New Components:**
- src/BlazorUI/Components/Select/Select.razor
- src/BlazorUI/Components/Select/Select.razor.cs
- src/BlazorUI/Components/Select/SelectTrigger.razor
- src/BlazorUI/Components/Select/SelectContent.razor
- src/BlazorUI/Components/Select/SelectContent.razor.cs
- src/BlazorUI/Components/Select/SelectItem.razor
- src/BlazorUI/Components/Select/SelectItem.razor.cs
- src/BlazorUI/Components/Select/SelectValue.razor
- src/BlazorUI/Components/Select/SelectGroup.razor
- src/BlazorUI/Components/Select/SelectLabel.razor
- src/BlazorUI/Components/Select/select.js

**Demo:**
- demo/BlazorUI.Demo/Pages/SelectDemo.razor
- demo/BlazorUI.Demo/Shared/NavMenu.razor (modified)

---

## Dependencies

None (uses built-in Blazor features and JSInterop)

---

**Next Step:** Auto-generate tasks and begin implementation with `/execute`

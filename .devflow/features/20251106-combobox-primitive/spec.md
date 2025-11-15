# Feature Specification: Combobox Primitive

**Created:** 2025-01-06T12:00:00Z
**Feature ID:** 20251106-combobox-primitive
**Workflow:** Build Feature (Streamlined)

---

## Description

Create a new headless Combobox primitive in Blazix that handles state management, item registration, filtering, keyboard navigation, and selection for command palettes, autocomplete, and searchable select patterns. Then migrate the existing Command component to use this primitive, reducing complexity from ~264 lines to ~50 lines and establishing clean separation between headless behavior (Blazix) and styled presentation (BlazorUI).

---

## Rationale

The Command component currently contains 264+ lines of complex state management, custom item registration, filtering logic, and keyboard navigation implementation. This logic should be extracted into a reusable primitive that can power Command, Autocomplete, SearchableSelect, and other combobox-pattern components. The primitive pattern (already proven with Select, Dialog, Accordion, etc.) provides better separation of concerns, reusability, and maintainability.

---

## Acceptance Criteria

- [ ] Combobox primitive created in `src/Blazix/Primitives/Combobox/` with 6 components
- [ ] ComboboxContext handles state management, item registration, filtering, keyboard navigation
- [ ] Supports both controlled and uncontrolled search query modes (UseControllableState pattern)
- [ ] Customizable filtering via FilterFunction parameter (default: LINQ Contains)
- [ ] Reuses existing JS utilities (scrollItemIntoView from select.js, setupKeyboardNav from keyboard-nav.js)
- [ ] Command component migrated to thin wrappers over Combobox primitive
- [ ] Command.razor.cs and CommandItem.razor.cs deleted (logic moved to primitive)
- [ ] All keyboard navigation preserved (ArrowUp/Down, Home/End, Enter, Escape)
- [ ] All filtering/searching behavior preserved
- [ ] Combobox primitive demo page created
- [ ] Existing Command demo updated and working
- [ ] Build succeeds with no errors
- [ ] All ARIA attributes and accessibility features preserved

---

## Files Affected

**New Files (Blazix Primitive):**
- `src/Blazix/Primitives/Combobox/ComboboxPrimitive.razor`
- `src/Blazix/Primitives/Combobox/ComboboxContext.cs`
- `src/Blazix/Primitives/Combobox/ComboboxInput.razor`
- `src/Blazix/Primitives/Combobox/ComboboxContent.razor`
- `src/Blazix/Primitives/Combobox/ComboboxItem.razor`
- `src/Blazix/Primitives/Combobox/ComboboxEmpty.razor`

**Modified Files (Command Component):**
- `src/BlazorUI/Components/Command/Command.razor` (simplified to wrapper)
- `src/BlazorUI/Components/Command/CommandInput.razor` (wrapper over ComboboxInput)
- `src/BlazorUI/Components/Command/CommandList.razor` (wrapper over ComboboxContent)
- `src/BlazorUI/Components/Command/CommandItem.razor` (wrapper over ComboboxItem)
- `src/BlazorUI/Components/Command/CommandEmpty.razor` (wrapper over ComboboxEmpty)

**Deleted Files:**
- `src/BlazorUI/Components/Command/Command.razor.cs` (logic moves to ComboboxContext)
- `src/BlazorUI/Components/Command/CommandItem.razor.cs` (logic moves to ComboboxItem primitive)

**Demo Files:**
- `demo/BlazorUI.Demo/Pages/Primitives/ComboboxPrimitiveDemo.razor` (new)
- `demo/BlazorUI.Demo/Pages/Components/CommandDemo.razor` (verify still works)

---

## Dependencies

- Existing Blazix primitives pattern (PrimitiveContextWithEvents base class)
- Existing JS utilities: `select.js` (scrollItemIntoView), `keyboard-nav.js` (setupKeyboardNav)
- UseControllableState pattern from Select primitive
- BlazorUI.Utilities.ClassNames.cn() for styling

---

**Next Step:** Auto-generate tasks and begin implementation with `/execute`

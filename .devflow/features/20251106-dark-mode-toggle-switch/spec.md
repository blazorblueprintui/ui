# Feature Specification: Dark Mode Toggle Switch

**Created:** 2025-11-06T00:00:00Z
**Feature ID:** 20251106-dark-mode-toggle-switch
**Workflow:** Build Feature (Streamlined)

---

## Description

Replace the current "Toggle Dark Mode" button component in the demo application with a proper Switch component that provides visual feedback through sun/moon icons. The switch will use the existing BlazorUI Switch component (medium size) and integrate with the current dark mode JavaScript toggle functionality.

---

## Rationale

Improve user experience by providing a clear, intuitive visual indicator of the current dark mode state. A toggle switch with icon indicators is a more standard and recognizable UI pattern for theme switching compared to a plain button.

---

## Acceptance Criteria

- ✓ Replace button in `DarkModeToggle.razor` with Switch component
- ✓ Display sun icon when in light mode (dark mode off)
- ✓ Display moon icon when in dark mode (dark mode on)
- ✓ Use medium size (default) for the switch
- ✓ Switch toggles dark mode on/off when clicked
- ✓ Maintain current JavaScript integration (no localStorage, session only)
- ✓ Component tracks current dark mode state properly

---

## Files Affected

- `demo/BlazorUI.Demo/Shared/DarkModeToggle.razor` - Replace button with Switch component
- Possibly: Add icon assets or use existing icon library

---

## Dependencies

- Existing: `BlazorUI.Components.Switch`
- Existing: `toggleDarkMode` JavaScript function in `_Host.cshtml`
- Icons: Need to determine available icon solution (Lucide, SVG, or existing icons)

---

**Next Step:** Auto-generate tasks and begin implementation with `/execute`

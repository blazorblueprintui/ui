# Feature Specification: Theme Selector

**Created:** 2025-11-02T10:45:00.000Z
**Feature ID:** 20251102-theme-selector
**Workflow:** Build Feature (Streamlined)

---

## Description

Add a theme selector dropdown in the header bar (left of dark mode toggle) that allows users to switch between 4 distinct color themes. Themes use OKLCH color space following shadcn/ui theming standards. When a user selects a theme, CSS variables update immediately and the entire UI reflects the new color scheme.

---

## Rationale

Provides users with visual customization options beyond just light/dark mode. Demonstrates the flexibility of the CSS variable-based theming system and showcases how shadcn/ui's design tokens work across different color palettes. This will be valuable for users who want to brand their applications with different color schemes.

---

## Acceptance Criteria

- [ ] Theme selector dropdown appears in header bar, left of dark mode toggle
- [ ] Four themes available: Zinc (default), Claude (OKLCH-based), and 2 additional varied themes
- [ ] Each theme has both light and dark mode variants
- [ ] Selecting a theme immediately updates UI colors across all components
- [ ] Theme files follow standard shadcn OKLCH format with CSS variables
- [ ] Dropdown styled with Tailwind (standard HTML select for now)
- [ ] Theme change works seamlessly with existing dark mode toggle
- [ ] All existing components (Button, NavMenu, etc.) work correctly with all themes

---

## Files Affected

**New Files:**
- `demo/BlazorUI.Demo/Shared/ThemeSelector.razor` - Theme selector dropdown component
- `demo/BlazorUI.Demo/Shared/ThemeSelector.razor.cs` - Component logic
- `demo/BlazorUI.Demo/wwwroot/styles/themes/zinc.css` - Default Zinc theme (from existing)
- `demo/BlazorUI.Demo/wwwroot/styles/themes/claude.css` - Claude theme (OKLCH)
- `demo/BlazorUI.Demo/wwwroot/styles/themes/ocean.css` - Ocean theme (cool blues/teals)
- `demo/BlazorUI.Demo/wwwroot/styles/themes/sunset.css` - Sunset theme (warm oranges/reds)

**Modified Files:**
- `demo/BlazorUI.Demo/Shared/MainLayout.razor` - Add ThemeSelector to header
- `demo/BlazorUI.Demo/Pages/_Host.cshtml` - Import theme CSS files
- `demo/BlazorUI.Demo/wwwroot/styles/shadcn-base.css` - Refactor to work with theme system

---

## Dependencies

- Existing Tailwind CSS setup
- OKLCH color space support (modern browsers)
- Current dark mode toggle implementation

---

**Next Step:** Auto-generate tasks and begin implementation with `/execute`

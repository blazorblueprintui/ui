# Feature Specification: Shadcn Theme Compatibility

**Status:** Pending
**Created:** 2025-11-06T00:00:00Z
**Feature ID:** 20251106-shadcn-theme-compatibility

---

## Problem Statement

BlazorUI components currently cannot use modern Shadcn themes (from tweakcn.com, ui.shadcn.com/themes, etc.) without modification due to fundamental incompatibilities:

1. **Color space mismatch**: Current implementation mixes HSL (shadcn-base.css) with OKLCH (theme files), creating invalid CSS when Tailwind wraps OKLCH in `hsl()` functions
2. **Theme structure mismatch**: BlazorUI uses multi-theme system with `[data-theme]` selectors, while Shadcn uses single theme with `:root` and `.dark` selectors
3. **Inconsistent component styling**: Components use three different styling patterns (StringBuilder, cn() utility, manual concatenation), making maintenance difficult and diverging from Shadcn's approach
4. **Missing design tokens**: Font, shadow, and spacing tokens from modern Shadcn themes are not integrated into Tailwind configuration

This prevents BlazorUI from being truly "Shadcn for Blazor" and requires users to manually convert themes, reducing adoption and creating a poor developer experience.

---

## Goals and Objectives

1. **Direct theme compatibility**: Users can copy any modern Shadcn theme (OKLCH-based) from tweakcn.com or similar tools and paste it directly into BlazorUI without modification
2. **Consistent styling architecture**: All components use the same styling pattern (cn() utility), matching Shadcn React's approach
3. **Complete token support**: All design tokens (colors, fonts, shadows, spacing, radius) work seamlessly with Tailwind utilities
4. **Modern standards**: Use OKLCH color space and Tailwind v4 for future-proof, perceptually uniform theming
5. **Simplified theme management**: Remove multi-theme complexity in favor of standard `:root`/`.dark` pattern with clear dark mode toggle

---

## User Stories

1. **As a Blazor developer**, I want to copy a theme from tweakcn.com and use it in my BlazorUI project without modification, so that I can quickly customize my application's appearance

2. **As a designer**, I want to use modern OKLCH color tokens that provide perceptually uniform color variations, so that my theme looks consistent across all components

3. **As a component maintainer**, I want all components to use the same styling pattern (cn() utility), so that the codebase is consistent and easy to maintain

4. **As a developer using BlazorUI**, I want access to all design tokens (fonts, shadows, spacing) through Tailwind utilities, so that I can build custom layouts that match the component styling

5. **As a theme creator**, I want to define a single theme with light and dark variants using `:root` and `.dark` selectors, so that my themes work the same way as standard Shadcn themes

---

## Acceptance Criteria

### Theme System
- [ ] **AC1**: Single theme file uses `:root` for light mode and `.dark` for dark mode (no `[data-theme]` selectors)
- [ ] **AC2**: All color tokens use OKLCH format: `--background: oklch(1 0 0);`
- [ ] **AC3**: Theme includes all standard tokens: background, foreground, card, popover, primary, secondary, muted, accent, destructive, border, input, ring, chart-1 through chart-5, sidebar variants
- [ ] **AC4**: Theme includes font tokens: `--font-sans`, `--font-serif`, `--font-mono`
- [ ] **AC5**: Theme includes shadow tokens: `--shadow-2xs` through `--shadow-2xl`
- [ ] **AC6**: Theme includes radius and spacing tokens
- [ ] **AC7**: `@theme inline` block correctly maps all CSS variables to Tailwind custom properties

### Tailwind Integration
- [ ] **AC8**: `tailwind.config.js` configured for OKLCH color space (no `hsl()` wrapping)
- [ ] **AC9**: All color utilities work: `bg-primary`, `text-foreground`, `border-input`, etc.
- [ ] **AC10**: Font utilities available: `font-sans`, `font-serif`, `font-mono`
- [ ] **AC11**: Shadow utilities available: `shadow-xs`, `shadow-sm`, `shadow-md`, `shadow-lg`, etc.
- [ ] **AC12**: Radius utilities work: `rounded-sm`, `rounded-md`, `rounded-lg`, `rounded-xl`

### Component Styling
- [ ] **AC13**: Button component uses cn() utility pattern (migrated from StringBuilder)
- [ ] **AC14**: Input component uses cn() utility pattern (migrated from StringBuilder)
- [ ] **AC15**: Badge component uses cn() utility pattern (migrated from StringBuilder)
- [ ] **AC16**: Switch component uses cn() utility pattern (migrated from StringBuilder)
- [ ] **AC17**: AccordionTrigger uses cn() utility pattern (migrated from manual concat)
- [ ] **AC18**: DialogContent uses cn() utility pattern (migrated from manual concat)
- [ ] **AC19**: All components reference color tokens consistently through Tailwind utilities
- [ ] **AC20**: No components use hardcoded colors or inline styles

### Dark Mode
- [ ] **AC21**: ThemeService simplified to only handle dark mode toggle (`.dark` class on `<html>`)
- [ ] **AC22**: Dark mode preference persists to localStorage
- [ ] **AC23**: All components render correctly in both light and dark modes
- [ ] **AC24**: Multi-theme switching logic removed (no more `data-theme` attribute)

### Compatibility Testing
- [ ] **AC25**: Sample theme from tweakcn.com works without modification when pasted into app.css
- [ ] **AC26**: All existing components render correctly with new theme system
- [ ] **AC27**: Focus states (ring), hover states, and disabled states work correctly
- [ ] **AC28**: Chart components use chart-* tokens properly
- [ ] **AC29**: Sidebar components use sidebar-* tokens properly

### Documentation
- [ ] **AC30**: Theme usage guide explains how to copy themes from tweakcn.com
- [ ] **AC31**: Migration guide documents breaking changes from old multi-theme system
- [ ] **AC32**: OKLCH color space benefits explained
- [ ] **AC33**: Tailwind v4 requirements documented

---

## Technical Requirements

### Color Space
- **OKLCH (CSS Color Level 4)**: Modern, perceptually uniform color space
- All tokens defined as: `--token: oklch(L C H);` (e.g., `--primary: oklch(0.7686 0.1647 70.0804);`)
- No HSL wrapping in Tailwind config - direct OKLCH token references

### Tailwind Version
- **Require Tailwind v4** (currently in alpha)
- Use `@theme inline` directive for CSS variable mapping
- Configure for OKLCH color space support

### Component Architecture
- **Standardize on cn() utility**: All components must use `ClassNames.cn()` for class composition
- Pattern:
  ```csharp
  private string CssClass => ClassNames.cn(
      "base-classes",
      variantClasses,
      sizeClasses,
      Class  // user override
  );
  ```

### Design Tokens (Complete Set)
**Colors** (OKLCH):
- Core: background, foreground, card, card-foreground, popover, popover-foreground
- Semantic: primary, primary-foreground, secondary, secondary-foreground, muted, muted-foreground, accent, accent-foreground, destructive, destructive-foreground
- UI: border, input, ring
- Charts: chart-1, chart-2, chart-3, chart-4, chart-5
- Sidebar: sidebar, sidebar-foreground, sidebar-primary, sidebar-primary-foreground, sidebar-accent, sidebar-accent-foreground, sidebar-border, sidebar-ring

**Typography**:
- `--font-sans`: Sans-serif font stack
- `--font-serif`: Serif font stack
- `--font-mono`: Monospace font stack

**Shadows**:
- `--shadow-2xs`, `--shadow-xs`, `--shadow-sm`, `--shadow`, `--shadow-md`, `--shadow-lg`, `--shadow-xl`, `--shadow-2xl`

**Spacing/Radius**:
- `--radius`: Base radius value
- Calculated variants in `@theme inline`: `--radius-sm`, `--radius-md`, `--radius-lg`, `--radius-xl`

### File Structure Changes
**Remove**:
- `demo/BlazorUI.Demo/wwwroot/css/shadcn-base.css` (HSL-based)
- `demo/BlazorUI.Demo/wwwroot/css/themes/*.css` (multi-theme files)

**Create/Update**:
- `demo/BlazorUI.Demo/wwwroot/css/app.css` (single OKLCH theme with `:root` and `.dark`)
- Update `tailwind.config.js` for OKLCH
- Update `app-input.css` for Tailwind v4

**Refactor**:
- All component `.razor` and `.razor.cs` files to use cn() pattern

---

## Dependencies

### External
- **Tailwind CSS v4** (alpha) - Required for `@theme inline` and OKLCH support
- **Node.js/npm** - For Tailwind build process
- **Modern browsers** - OKLCH support requires Chrome 111+, Safari 15.4+, Firefox 113+

### Internal
- **ClassNames.cn() utility** - Already exists, ensure it's properly configured with tailwind-merge
- **ThemeService** - Needs refactoring to remove multi-theme logic

### Breaking Changes
- **Multi-theme system removal**: Existing apps using `[data-theme]` selectors will break
- **HSL to OKLCH migration**: Any custom themes in HSL format will need conversion
- **Component API**: While component parameters remain the same, CSS classes may shift slightly

---

## Cross-Cutting Concerns

- **Theming System** (design/theming.md) - Core concern, full refactor
- **Internationalization** (infrastructure/internationalization.md) - Verify RTL support works with new theme system

---

## Risks and Challenges

### Browser Compatibility
- **Risk**: OKLCH not supported in older browsers (pre-2023)
- **Mitigation**: Document minimum browser versions clearly. Consider fallback to HSL for older browsers in future if needed.

### Tailwind v4 Alpha
- **Risk**: Tailwind v4 is still in alpha, APIs may change
- **Mitigation**: Pin to specific v4 alpha version, monitor Tailwind releases, prepare to update when v4 goes stable

### Breaking Changes
- **Risk**: Existing BlazorUI users will experience breaking changes
- **Mitigation**: Version bump to v2.0, comprehensive migration guide, provide conversion script/tool for themes

### Component Refactoring Scope
- **Risk**: Refactoring 6+ components to cn() pattern is time-consuming and error-prone
- **Mitigation**: Test each component thoroughly after refactoring, maintain visual parity with before/after screenshots

### Token Completeness
- **Risk**: Missing tokens may cause Tailwind utilities to fail
- **Mitigation**: Audit all components for token usage, cross-reference with Shadcn React component library

### Dark Mode Edge Cases
- **Risk**: Some components may not properly support dark mode after theme system change
- **Mitigation**: Manual testing of every component in both light and dark modes, fix any contrast/visibility issues

---

## Out of Scope

### Explicitly NOT Included
1. **HSL theme support**: This refactor commits to OKLCH only. HSL themes not supported.
2. **Multi-theme switching**: Removed in favor of single theme + dark mode. Users cannot switch between multiple themes (Ocean, Default, etc.).
3. **Tailwind v3 compatibility**: This requires Tailwind v4. No backward compatibility with v3.
4. **Automatic theme conversion tool**: Users must manually convert non-OKLCH themes. Conversion tool could be separate feature.
5. **Component functionality changes**: This is a styling/theming refactor only. Component behavior and parameters remain unchanged.
6. **New components**: Focus is on refactoring existing components, not adding new ones.
7. **Automated testing**: Continue with manual testing approach per constitution.
8. **Legacy browser fallbacks**: No polyfills or fallbacks for browsers without OKLCH support.

### Future Enhancements (Post-Refactor)
- Theme marketplace/gallery with pre-built OKLCH themes
- CLI tool for theme conversion (HSL → OKLCH)
- Theme preview component for rapid iteration
- Extended design tokens (animations, transitions, more spacing options)

---

## Success Metrics

**Primary Metric**: A developer can copy a theme from tweakcn.com and use it in BlazorUI with zero modifications

**Secondary Metrics**:
- All 15+ components render correctly in new theme system
- Dark mode toggle works seamlessly
- Build time with Tailwind v4 remains acceptable (<5 seconds for full rebuild)
- Component bundle size doesn't increase significantly
- Developer feedback on ease of theming is positive

---

## Notes

This is a **foundational refactor** that transforms BlazorUI's theming architecture to align with modern Shadcn standards. While breaking changes are significant, the long-term benefits include:

- True Shadcn theme compatibility
- Future-proof color system (OKLCH)
- Consistent component styling (cn() pattern)
- Simplified maintenance
- Better developer experience

Version bump to **v2.0** is recommended given the scope of breaking changes.

---

**Generated by DevFlow** | Spec Phase Complete

# ADR-0001: Adopt OKLCH Color Space and Single Theme Pattern for Shadcn Compatibility

**Status:** Proposed
**Date:** 2025-11-06
**Decision Makers:** Architecture Team
**Related Feature:** 20251106-shadcn-theme-compatibility

---

## Context

BlazorUI was created to be "Shadcn for Blazor" - providing the same high-quality, customizable components that Shadcn offers for React. However, our current theming implementation has diverged from modern Shadcn standards, creating several critical issues:

### Current Problems

1. **Color Space Incompatibility**
   - BlazorUI uses HSL color space: `--primary: 222.2 47.4% 11.2%`
   - Modern Shadcn themes use OKLCH: `--primary: oklch(0.7686 0.1647 70.0804)`
   - Our Tailwind config wraps everything in `hsl()`: `hsl(var(--primary))`
   - Result: `hsl(oklch(0.7686 0.1647 70.0804))` = invalid CSS

2. **Theme Structure Mismatch**
   - BlazorUI uses multi-theme system with `[data-theme="ocean"]` selectors
   - Shadcn uses single theme with `:root` and `.dark` selectors
   - Users cannot copy themes from tweakcn.com or ui.shadcn.com/themes

3. **Component Styling Inconsistency**
   - Three different patterns across components:
     - StringBuilder pattern (Button, Input, Badge, Switch)
     - cn() utility pattern (Select - trial implementation)
     - Manual string concatenation (AccordionTrigger, DialogContent)
   - Shadcn React consistently uses cn() throughout

4. **Missing Design Tokens**
   - No font family tokens (`--font-sans`, `--font-serif`, `--font-mono`)
   - No shadow scale tokens (`--shadow-xs` through `--shadow-2xl`)
   - Missing chart colors and calculated spacing tokens

### Why OKLCH?

OKLCH (Oklab Lightness Chroma Hue) is a perceptually uniform color space that solves many issues with HSL:

- **Perceptual Uniformity**: Equal numeric changes produce equal visual changes
- **Better Gradients**: Smooth transitions without unexpected color shifts
- **Consistent Lightness**: L value accurately represents perceived brightness
- **Wide Gamut**: Can represent colors outside sRGB
- **Future Standard**: Part of CSS Color Level 4, adopted by modern design tools

### Why Single Theme Pattern?

The multi-theme pattern adds complexity without clear benefits:

- **Maintenance Burden**: Each theme duplicates all tokens (3x the CSS)
- **User Confusion**: Most apps need only light/dark modes, not multiple themes
- **Compatibility**: Shadcn ecosystem expects `:root`/`.dark` pattern
- **Simplicity**: Easier to understand and customize one theme

### Why Tailwind v4?

Tailwind v4 (currently in alpha) provides native OKLCH support:

- **@theme inline**: Direct CSS variable mapping without wrapper functions
- **OKLCH-aware**: Understands OKLCH color space natively
- **Smaller builds**: More efficient output
- **Future-proof**: v3 will eventually be deprecated

---

## Decision

We will adopt the following architectural changes:

### 1. OKLCH Color Space
- All color tokens will use OKLCH format: `--primary: oklch(0.7686 0.1647 70.0804)`
- No HSL colors in the theme system
- Full browser support requirement (Chrome 111+, Safari 15.4+, Firefox 113+)

### 2. Single Theme Pattern
- One theme file with `:root` (light) and `.dark` (dark) selectors
- Remove `[data-theme]` attribute system
- Dark mode via `.dark` class on `<html>` element only

### 3. Tailwind v4 Integration
- Upgrade to Tailwind v4 alpha
- Use `@theme inline` for CSS variable mapping
- Remove `hsl()` wrappers from configuration

### 4. Component Standardization
- All components use `ClassNames.cn()` utility
- Remove StringBuilder and manual concatenation patterns
- Consistent with Shadcn React implementation

### 5. Complete Token Set
Add missing tokens:
- Font families: `--font-sans`, `--font-serif`, `--font-mono`
- Shadow scale: `--shadow-2xs` through `--shadow-2xl`
- All chart colors: `--chart-1` through `--chart-5`
- Calculated radius/spacing variants

---

## Consequences

### Positive Consequences

1. **True Shadcn Compatibility**
   - Users can copy any theme from tweakcn.com without modification
   - Compatible with entire Shadcn theme ecosystem
   - Easier onboarding for React developers

2. **Better Color Consistency**
   - OKLCH provides perceptually uniform color adjustments
   - More predictable color variations
   - Better accessibility (consistent contrast ratios)

3. **Simplified Maintenance**
   - Single theme to maintain instead of three
   - Consistent component styling pattern
   - Less CSS to ship to users

4. **Future-Proof Architecture**
   - OKLCH is the future of CSS color
   - Tailwind v4 is the next major version
   - Aligned with modern web standards

5. **Improved Developer Experience**
   - Clear, consistent patterns
   - Better documentation
   - Easier customization

### Negative Consequences

1. **Breaking Changes**
   - All existing themes become incompatible
   - Users must migrate to OKLCH format
   - Multi-theme switching feature removed

2. **Browser Requirements**
   - Drops support for older browsers (pre-2023)
   - No fallback for browsers without OKLCH support
   - May exclude some enterprise users

3. **Tailwind v4 Alpha Risk**
   - Using alpha software in production
   - Potential API changes before stable release
   - Limited ecosystem support initially

4. **Migration Burden**
   - Existing users must update their implementations
   - Learning curve for OKLCH color space
   - Time investment for migration

5. **Feature Removal**
   - No more theme switching (Ocean, Claude themes)
   - Some users may rely on this feature
   - Reduces product differentiation

---

## Alternatives Considered

### Alternative 1: Keep HSL, Add OKLCH Conversion Layer

**Approach**: Maintain HSL internally, convert OKLCH themes on import

**Pros**:
- No breaking changes
- Maintains multi-theme system
- Works with Tailwind v3

**Cons**:
- Complex conversion logic
- Loss of color fidelity in conversion
- Doesn't solve structural incompatibility
- Technical debt increases

**Rejected because**: Adds complexity without solving core issues

### Alternative 2: Support Both HSL and OKLCH

**Approach**: Detect color format and handle both

**Pros**:
- Maximum compatibility
- Gradual migration path
- No breaking changes initially

**Cons**:
- Very complex implementation
- Doubles testing surface
- Confusing for users
- Maintenance nightmare

**Rejected because**: Complexity outweighs benefits

### Alternative 3: Fork Shadcn Themes Back to HSL

**Approach**: Convert Shadcn themes to HSL format

**Pros**:
- Stays on Tailwind v3
- No browser compatibility issues
- Familiar color format

**Cons**:
- Loses perceptual uniformity benefits
- Swimming against the current
- Conversion tools needed forever
- Not actually compatible with Shadcn

**Rejected because**: Goes against industry direction

### Alternative 4: Wait for Tailwind v4 Stable

**Approach**: Delay changes until Tailwind v4 is stable

**Pros**:
- Less risk
- More stable foundation
- Better documentation

**Cons**:
- Unknown timeline (could be months)
- Users can't use Shadcn themes now
- Falling behind Shadcn ecosystem

**Rejected because**: Urgent user need for theme compatibility

---

## Implementation Plan

### Phase 1: Documentation and Communication
- Create migration guide
- Document breaking changes
- Announce v2.0 plans

### Phase 2: Theme System Refactor
- Implement OKLCH theme structure
- Create theme conversion tools
- Test with popular Shadcn themes

### Phase 3: Tailwind v4 Integration
- Upgrade to Tailwind v4 alpha
- Configure @theme inline
- Test utility generation

### Phase 4: Component Migration
- Refactor all components to cn() pattern
- Test each component thoroughly
- Ensure visual parity

### Phase 5: Release
- Beta release for early adopters
- Gather feedback
- Fix issues
- v2.0.0 stable release

---

## Metrics for Success

1. **Compatibility**: 100% of tweakcn.com themes work without modification
2. **Performance**: Build time remains under 5 seconds
3. **Adoption**: 50% of users migrate within 6 months
4. **Quality**: No increase in bug reports
5. **Satisfaction**: Positive user feedback on theme system

---

## References

- [OKLCH Color Space](https://oklch.com/)
- [CSS Color Level 4 Specification](https://www.w3.org/TR/css-color-4/)
- [Tailwind v4 Alpha Announcement](https://tailwindcss.com/blog/tailwindcss-v4-alpha)
- [Shadcn Theming Documentation](https://ui.shadcn.com/docs/theming)
- [tweakcn Theme Generator](https://tweakcn.com/)

---

## Decision

**Status**: ✅ APPROVED (pending review)

This architectural change is necessary to maintain BlazorUI's position as the premier Shadcn implementation for Blazor. While the breaking changes are significant, the long-term benefits of alignment with modern web standards and the Shadcn ecosystem outweigh the short-term migration costs.

**Version Impact**: This will require a major version bump to v2.0.0

---

**Document created by**: DevFlow Architect Agent
**Date**: 2025-11-06
**Review required by**: Architecture Team
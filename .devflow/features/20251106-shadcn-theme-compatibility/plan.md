# Technical Plan: Shadcn Theme Compatibility

**Feature ID:** 20251106-shadcn-theme-compatibility
**Status:** Planning Complete
**Created:** 2025-11-06
**Complexity:** HIGH (15-20 tasks)
**Risk Level:** MEDIUM-HIGH
**Estimated Time:** 24-32 hours
**Requires Review:** true (Major architectural changes)

---

## Executive Summary

### What We're Building
Transforming BlazorUI's theming system from a multi-theme HSL-based architecture to a single-theme OKLCH-based system that is fully compatible with modern Shadcn themes. This enables developers to copy themes directly from tweakcn.com or other Shadcn theme generators without modification.

### Why This Change
- **Theme Incompatibility**: Current HSL/OKLCH mismatch prevents using modern Shadcn themes
- **Component Inconsistency**: Three different styling patterns across components (StringBuilder, cn(), manual concatenation)
- **Missing Tokens**: Font, shadow, and spacing tokens not integrated with Tailwind
- **Maintenance Burden**: Multi-theme system adds unnecessary complexity

### Major Changes Overview
1. **Color Space Migration**: HSL → OKLCH for perceptually uniform colors
2. **Theme Structure**: Multi-theme with `[data-theme]` → Single theme with `:root`/`.dark`
3. **Tailwind Upgrade**: v3 → v4 (alpha) for native OKLCH support
4. **Component Standardization**: All components use cn() utility pattern
5. **Service Simplification**: ThemeService reduced to dark mode toggle only

### Breaking Changes Summary
- **Theme Format**: Existing HSL themes incompatible, require OKLCH conversion
- **Multi-theme Removal**: No more theme switching between Default/Ocean/Claude
- **Tailwind v4 Requirement**: Projects must upgrade to Tailwind v4 alpha
- **CSS Variable Names**: Some token names changed to match Shadcn standard

---

## Current State Analysis

### Existing Theme System Issues

1. **Mixed Color Spaces**
   - `tailwind.config.js`: Wraps all colors in `hsl()` function
   - Base theme (`:root`): Uses HSL values (`222.2 84% 4.9%`)
   - Custom themes (`[data-theme]`): Use OKLCH values (`oklch(0.9818 0.0054 95.0986)`)
   - **Result**: Invalid CSS when Tailwind generates `hsl(oklch(0.9818 0.0054 95.0986))`

2. **Component Styling Inconsistencies**
   - **StringBuilder Pattern** (Button, Input, Badge, Switch): Manual class concatenation
   - **cn() Utility Pattern** (Select component - trial implementation): Uses ClassNames.cn()
   - **Manual Concatenation** (AccordionTrigger, DialogContent): Direct string concat
   - **Result**: Difficult to maintain, inconsistent with Shadcn React

3. **Token Gaps**
   - **Missing**: Font families, shadow scales, calculated spacing
   - **Incomplete**: Chart colors (chart-1 to chart-5) not in all themes
   - **Inconsistent**: Sidebar tokens use different naming convention

4. **Multi-theme Complexity**
   - Three themes: Default (HSL), Claude (OKLCH), Ocean (OKLCH)
   - Theme switching via `data-theme` attribute
   - Each theme duplicates all tokens
   - **Result**: 3x maintenance burden, confusion about which format to use

### Architecture Impact Assessment

**MAJOR CHANGES REQUIRED:**
- Complete theme file restructure
- Tailwind configuration overhaul
- Component refactoring across 6+ components
- Service layer simplification
- Documentation updates

**This qualifies as a MAJOR architectural change requiring ADR documentation.**

---

## Proposed Architecture

### New Theme Structure

#### Single Theme File Pattern
```css
/* app.css - OKLCH-based single theme */

@layer base {
  /* Light mode (default) */
  :root {
    /* Core colors */
    --background: oklch(0.9818 0.0054 95.0986);
    --foreground: oklch(0.3438 0.0269 95.7226);

    /* Component colors */
    --card: oklch(0.9818 0.0054 95.0986);
    --card-foreground: oklch(0.1908 0.0020 106.5859);
    --popover: oklch(1.0000 0 0);
    --popover-foreground: oklch(0.2671 0.0196 98.9390);

    /* Semantic colors */
    --primary: oklch(0.7686 0.1647 70.0804);
    --primary-foreground: oklch(1.0000 0 0);
    --secondary: oklch(0.9245 0.0138 92.9892);
    --secondary-foreground: oklch(0.4334 0.0177 98.6048);
    --muted: oklch(0.9341 0.0153 90.2390);
    --muted-foreground: oklch(0.6059 0.0075 97.4233);
    --accent: oklch(0.9245 0.0138 92.9892);
    --accent-foreground: oklch(0.2671 0.0196 98.9390);
    --destructive: oklch(0.6368 0.2078 25.3313);
    --destructive-foreground: oklch(1.0000 0 0);

    /* UI elements */
    --border: oklch(0.8847 0.0069 97.3627);
    --input: oklch(0.7621 0.0156 98.3528);
    --ring: oklch(0.6171 0.1375 39.0427);

    /* Chart colors */
    --chart-1: oklch(0.5583 0.1276 42.9956);
    --chart-2: oklch(0.6898 0.1581 290.4107);
    --chart-3: oklch(0.8816 0.0276 93.1280);
    --chart-4: oklch(0.8822 0.0403 298.1792);
    --chart-5: oklch(0.5608 0.1348 42.0584);

    /* Sidebar (optional) */
    --sidebar-background: oklch(0.9663 0.0080 98.8792);
    --sidebar-foreground: oklch(0.3590 0.0051 106.6524);
    --sidebar-primary: oklch(0.6171 0.1375 39.0427);
    --sidebar-primary-foreground: oklch(0.9881 0 0);
    --sidebar-accent: oklch(0.9245 0.0138 92.9892);
    --sidebar-accent-foreground: oklch(0.3250 0 0);
    --sidebar-border: oklch(0.9401 0 0);
    --sidebar-ring: oklch(0.7731 0 0);

    /* Typography */
    --font-sans: "Inter", system-ui, sans-serif;
    --font-serif: "Merriweather", serif;
    --font-mono: "JetBrains Mono", monospace;

    /* Shadows */
    --shadow-2xs: 0 1px 2px -1px oklch(0% 0 0 / 0.1);
    --shadow-xs: 0 2px 4px -2px oklch(0% 0 0 / 0.1);
    --shadow-sm: 0 4px 6px -4px oklch(0% 0 0 / 0.1);
    --shadow: 0 10px 15px -3px oklch(0% 0 0 / 0.1);
    --shadow-md: 0 20px 25px -5px oklch(0% 0 0 / 0.1);
    --shadow-lg: 0 25px 50px -12px oklch(0% 0 0 / 0.25);
    --shadow-xl: 0 35px 60px -15px oklch(0% 0 0 / 0.3);
    --shadow-2xl: 0 50px 100px -20px oklch(0% 0 0 / 0.35);

    /* Radius */
    --radius: 0.5rem;
  }

  /* Dark mode */
  .dark {
    --background: oklch(0.2679 0.0036 106.6427);
    --foreground: oklch(0.8074 0.0142 93.0137);
    /* ... all tokens with dark mode values ... */
  }
}
```

### Tailwind v4 Integration

#### New Configuration Structure
```javascript
// tailwind.config.js - Tailwind v4 format
export default {
  darkMode: ['class'],
  content: [
    './Pages/**/*.{razor,html,cs}',
    '../../src/BlazorUI/**/*.{razor,html,cs}',
  ],
  theme: {
    extend: {
      // No color definitions here - handled by @theme inline
    }
  }
}
```

#### @theme Inline Block
```css
/* app-input.css - Tailwind v4 input file */
@import "tailwindcss";

@theme inline {
  /* Map CSS variables to Tailwind utilities */
  --color-background: var(--background);
  --color-foreground: var(--foreground);
  --color-primary: var(--primary);
  --color-primary-foreground: var(--primary-foreground);
  /* ... all color mappings ... */

  --font-family-sans: var(--font-sans);
  --font-family-serif: var(--font-serif);
  --font-family-mono: var(--font-mono);

  --shadow-xs: var(--shadow-xs);
  --shadow-sm: var(--shadow-sm);
  /* ... all shadow mappings ... */

  --radius-sm: calc(var(--radius) - 4px);
  --radius-md: calc(var(--radius) - 2px);
  --radius-lg: var(--radius);
  --radius-xl: calc(var(--radius) + 4px);
}
```

### Component Styling Standard

#### cn() Pattern Implementation
All components will use the ClassNames.cn() utility for consistent styling:

```csharp
// Before (StringBuilder pattern in Button.razor.cs)
private string CssClass
{
    get
    {
        var builder = new StringBuilder();
        builder.Append("inline-flex items-center justify-center ");
        builder.Append(Variant switch { ... });
        builder.Append(Size switch { ... });
        if (!string.IsNullOrWhiteSpace(Class))
            builder.Append(Class);
        return builder.ToString().Trim();
    }
}

// After (cn() pattern)
private string CssClass => ClassNames.cn(
    "inline-flex items-center justify-center rounded-md text-sm font-medium",
    "transition-colors focus-visible:outline-none focus-visible:ring-2",
    "focus-visible:ring-ring focus-visible:ring-offset-2",
    "disabled:opacity-50 disabled:pointer-events-none",
    Variant switch
    {
        ButtonVariant.Default => "bg-primary text-primary-foreground hover:bg-primary/90",
        ButtonVariant.Destructive => "bg-destructive text-destructive-foreground hover:bg-destructive/90",
        ButtonVariant.Outline => "border border-input bg-background hover:bg-accent hover:text-accent-foreground",
        ButtonVariant.Secondary => "bg-secondary text-secondary-foreground hover:bg-secondary/80",
        ButtonVariant.Ghost => "hover:bg-accent hover:text-accent-foreground",
        ButtonVariant.Link => "text-primary underline-offset-4 hover:underline",
        _ => "bg-primary text-primary-foreground hover:bg-primary/90"
    },
    Size switch
    {
        ButtonSize.Small => "h-9 rounded-md px-3 text-xs",
        ButtonSize.Default => "h-10 px-4 py-2",
        ButtonSize.Large => "h-11 rounded-md px-8",
        ButtonSize.Icon => "h-10 w-10",
        _ => "h-10 px-4 py-2"
    },
    Class  // User's custom classes
);
```

---

## Implementation Approach

### Phase 1: Theme System Refactor (AC1-AC7)
**Goal**: Establish new OKLCH theme structure

1. **Backup existing theme files** (for migration guide)
2. **Create new app.css with OKLCH tokens**
   - Copy sample theme from tweakcn.com
   - Ensure all required tokens present
   - Add font, shadow, spacing tokens
3. **Remove old theme files**
   - Delete HSL-based theme files
   - Remove multi-theme CSS files
4. **Update base styles**
   - Convert body/border styles to use OKLCH
   - Ensure proper layering with @layer

### Phase 2: Tailwind Configuration (AC8-AC12)
**Goal**: Configure Tailwind v4 for OKLCH support

1. **Install Tailwind v4 alpha**
   ```bash
   npm install tailwindcss@next @tailwindcss/cli@next
   ```
2. **Update tailwind.config.js**
   - Remove HSL color wrappers
   - Simplify to v4 structure
3. **Create app-input.css with @theme inline**
   - Map all CSS variables to Tailwind utilities
   - No `hsl()` or `oklch()` wrappers - direct references
4. **Test utility generation**
   - Verify `bg-primary`, `text-foreground`, etc. work
   - Check shadow and font utilities

### Phase 3: Component Migration (AC13-AC20)
**Goal**: Standardize all components on cn() pattern

Components to migrate (in order of complexity):
1. **Badge** (simplest - good test case)
2. **Switch** (similar to Badge)
3. **Input** (form component)
4. **Button** (most used component)
5. **AccordionTrigger** (nested component)
6. **DialogContent** (complex modal)

For each component:
- Replace StringBuilder/manual concat with cn()
- Ensure all color references use Tailwind utilities
- Remove any hardcoded colors
- Test in both light/dark modes

### Phase 4: Service Simplification (AC21-AC24)
**Goal**: Simplify ThemeService to dark mode only

1. **Refactor ThemeSelector component**
   - Remove theme dropdown
   - Add dark mode toggle (using Switch component)
2. **Update ThemeService logic**
   - Remove `data-theme` attribute management
   - Add `.dark` class toggle on `<html>`
   - Persist dark mode preference to localStorage
3. **Remove multi-theme logic**
   - Clean up any theme switching code
   - Update demo pages

### Phase 5: Testing & Validation (AC25-AC29)
**Goal**: Ensure everything works correctly

1. **Theme compatibility test**
   - Copy fresh theme from tweakcn.com
   - Paste into app.css
   - Verify no modifications needed
2. **Component testing matrix**
   - All components in light mode
   - All components in dark mode
   - All variants and sizes
   - Focus/hover/disabled states
3. **Cross-browser testing**
   - Chrome 111+
   - Safari 15.4+
   - Firefox 113+
   - Edge (Chromium)
4. **Performance verification**
   - Build time < 5 seconds
   - Bundle size comparison

---

## Technical Details

### File Structure Changes

#### Files to Remove
```
demo/BlazorUI.Demo/wwwroot/css/
  ├── themes/
  │   ├── default.css     (DELETE)
  │   ├── ocean.css       (DELETE)
  │   └── claude.css      (DELETE)
  └── shadcn-base.css     (DELETE)
```

#### Files to Create/Update
```
demo/BlazorUI.Demo/
  ├── wwwroot/css/
  │   └── app.css          (UPDATE: Single OKLCH theme)
  ├── tailwind.config.js   (UPDATE: Tailwind v4 format)
  ├── app-input.css        (CREATE: Tailwind v4 input)
  └── package.json         (UPDATE: Tailwind v4 alpha)
```

#### Components to Refactor
```
src/BlazorUI/Components/
  ├── Button/Button.razor.cs      (StringBuilder → cn())
  ├── Input/Input.razor.cs        (StringBuilder → cn())
  ├── Badge/Badge.razor.cs        (StringBuilder → cn())
  ├── Switch/Switch.razor.cs      (StringBuilder → cn())
  ├── Accordion/AccordionTrigger.razor.cs (manual → cn())
  └── Dialog/DialogContent.razor.cs       (manual → cn())
```

### Code Patterns and Examples

#### Token Mapping Table
| Old Token (HSL) | New Token (OKLCH) | Tailwind Utility |
|-----------------|-------------------|------------------|
| `--background: 0 0% 100%` | `--background: oklch(1 0 0)` | `bg-background` |
| `--primary: 222.2 47.4% 11.2%` | `--primary: oklch(0.7686 0.1647 70.0804)` | `bg-primary` |
| `--border: 214.3 31.8% 91.4%` | `--border: oklch(0.8847 0.0069 97.3627)` | `border-border` |
| N/A | `--font-sans: "Inter", system-ui` | `font-sans` |
| N/A | `--shadow-md: 0 20px 25px...` | `shadow-md` |

#### Migration Paths for Breaking Changes

1. **Theme Conversion (HSL → OKLCH)**
   ```javascript
   // Conversion script (to be documented)
   function hslToOklch(h, s, l) {
     // Complex conversion via RGB intermediate
     // Provide online tool or script
   }
   ```

2. **Multi-theme to Single Theme**
   ```csharp
   // Old: Theme switching
   await JSRuntime.InvokeVoidAsync("eval",
     $"document.documentElement.setAttribute('data-theme', '{theme}')");

   // New: Dark mode only
   await JSRuntime.InvokeVoidAsync("eval",
     isDark ? "document.documentElement.classList.add('dark')"
           : "document.documentElement.classList.remove('dark')");
   ```

3. **Component Class Updates**
   ```csharp
   // Old: StringBuilder
   var builder = new StringBuilder();
   builder.Append("base-classes ");

   // New: cn() utility
   ClassNames.cn("base-classes", conditionalClass && "active");
   ```

---

## Cross-Cutting Concerns

### Theming System (PRIMARY CONCERN)
- **Impact**: Complete overhaul of theming architecture
- **Changes**: HSL → OKLCH, multi-theme → single theme
- **Testing**: Verify all components render correctly with new tokens
- **Documentation**: Update theming.md with new approach

### Internationalization (RTL Support)
- **Impact**: Verify RTL utilities still work with OKLCH colors
- **Testing**: Test Arabic/Hebrew layouts with new theme
- **Risk**: Low - RTL is orthogonal to color system

### Accessibility
- **Impact**: Ensure color contrast ratios maintained in OKLCH
- **Testing**: Run WCAG contrast checker on all color pairs
- **Focus**: Ring colors must remain visible in both modes

### Browser Compatibility
- **Requirement**: OKLCH support (2023+ browsers)
- **Affected Browsers**:
  - ✅ Chrome 111+ (March 2023)
  - ✅ Safari 15.4+ (March 2022)
  - ✅ Firefox 113+ (May 2023)
  - ✅ Edge (Chromium-based)
  - ❌ IE 11 (already unsupported)
- **Mitigation**: Document minimum browser versions

---

## Risk Assessment

### 1. Tailwind v4 Alpha Stability (HIGH)
- **Risk**: Alpha version may have bugs or API changes
- **Probability**: Medium
- **Impact**: High
- **Mitigation**:
  - Pin to specific alpha version
  - Test thoroughly before release
  - Monitor Tailwind releases
  - Prepare contingency to stay on alpha if needed

### 2. Browser Compatibility (MEDIUM)
- **Risk**: Users on older browsers cannot see colors correctly
- **Probability**: Low (most users on modern browsers)
- **Impact**: High (complete color failure)
- **Mitigation**:
  - Clear documentation of browser requirements
  - Consider PostCSS plugin for fallbacks in v2.1

### 3. Breaking Changes Impact (HIGH)
- **Risk**: Existing BlazorUI users face migration burden
- **Probability**: High (all users affected)
- **Impact**: Medium
- **Mitigation**:
  - Version bump to v2.0.0
  - Comprehensive migration guide
  - Theme conversion tool/script
  - Support period for v1.x

### 4. Component Refactoring Bugs (MEDIUM)
- **Risk**: cn() migration introduces styling bugs
- **Probability**: Medium
- **Impact**: Medium
- **Mitigation**:
  - Careful testing of each component
  - Side-by-side comparison screenshots
  - Gradual rollout (test with Badge first)

### 5. Missing Design Tokens (LOW)
- **Risk**: Some Shadcn themes use tokens we don't support
- **Probability**: Low
- **Impact**: Low
- **Mitigation**:
  - Audit popular themes from tweakcn.com
  - Add missing tokens before release
  - Document any limitations

---

## Testing Strategy

### Manual Testing Checklist

#### Per Component (6 components × 2 modes = 12 test sessions)
- [ ] Light mode appearance matches design
- [ ] Dark mode appearance matches design
- [ ] All variants render correctly
- [ ] All sizes render correctly
- [ ] Hover states work
- [ ] Focus states visible (ring)
- [ ] Disabled states properly styled
- [ ] RTL layout works (if applicable)

#### Theme Compatibility (AC25)
- [ ] Copy theme from tweakcn.com
- [ ] Paste into app.css without modification
- [ ] All components render correctly
- [ ] No console errors
- [ ] Dark mode works

#### Visual Regression Testing
- [ ] Screenshot all components (before refactor)
- [ ] Screenshot all components (after refactor)
- [ ] Compare for unintended changes
- [ ] Document any intentional improvements

#### Cross-Browser Testing
- [ ] Chrome 111+ (primary)
- [ ] Safari 15.4+
- [ ] Firefox 113+
- [ ] Edge (Chromium)
- [ ] Mobile Safari (iOS)
- [ ] Chrome Mobile (Android)

### Performance Benchmarks
- **Build Time**: < 5 seconds for full rebuild
- **Bundle Size**: Within 10% of current size
- **Theme Switch**: < 50ms for dark mode toggle
- **First Paint**: No regression from v1

---

## Migration Guide Outline

### For Existing BlazorUI v1.x Users

#### 1. Breaking Changes You Must Address
- Theme format change (HSL → OKLCH)
- Multi-theme removal (only dark mode toggle remains)
- Tailwind v4 requirement
- Some CSS variable name changes

#### 2. Step-by-Step Migration

**Step 1: Backup Your Themes**
```bash
cp wwwroot/css/app.css wwwroot/css/app.css.backup
```

**Step 2: Upgrade Dependencies**
```bash
npm uninstall tailwindcss
npm install tailwindcss@next @tailwindcss/cli@next
```

**Step 3: Convert Your Theme**
- Option A: Use default OKLCH theme provided
- Option B: Pick new theme from tweakcn.com
- Option C: Convert existing HSL theme using provided tool

**Step 4: Update Theme Implementation**
```csharp
// Remove theme switching
- <ThemeSelector />

// Add dark mode toggle
+ <DarkModeToggle />
```

**Step 5: Update Tailwind Config**
```javascript
// Remove HSL wrappers
- colors: {
-   primary: "hsl(var(--primary))",
- }

// Simplified v4 config
+ // Colors now handled by @theme inline
```

#### 3. Theme Conversion Tool
```javascript
// theme-converter.js (to be provided)
const convertHslToOklch = (hslTheme) => {
  // Parse HSL values
  // Convert to OKLCH
  // Return new theme CSS
};
```

---

## Success Criteria Verification

### Primary Metric
✅ **Developer can copy theme from tweakcn.com and use without modification**
- Test with 5 different themes from tweakcn.com
- Each theme should work with zero changes

### All 33 Acceptance Criteria
| AC | Description | Verification Method |
|----|-------------|-------------------|
| AC1-AC7 | Theme system with OKLCH | Inspect app.css for correct format |
| AC8-AC12 | Tailwind integration | Test utility classes in browser |
| AC13-AC20 | Component cn() pattern | Code review each component |
| AC21-AC24 | Dark mode only | Test toggle functionality |
| AC25-AC29 | Compatibility testing | Manual testing checklist |
| AC30-AC33 | Documentation | Review docs for completeness |

### Performance Metrics
- Build time: Measure with `time npm run build`
- Bundle size: Check output in dist folder
- Theme quality: Visual inspection
- Developer experience: Internal team feedback

---

## Architecture Decision Record

**See separate file**: `.devflow/decisions/ADR-0001-adopt-oklch-color-space-and-single-theme-pattern.md`

This ADR documents:
- Why we chose OKLCH over HSL
- Why we removed multi-theme support
- Why we require Tailwind v4
- Why we standardized on cn() utility
- Trade-offs and consequences

---

## Implementation Order

### Logical Task Grouping

#### Group 1: Foundation (4-6 hours)
1. Create ADR document
2. Backup existing themes
3. Install Tailwind v4 alpha
4. Create new OKLCH theme file

#### Group 2: Configuration (3-4 hours)
5. Update tailwind.config.js
6. Create @theme inline block
7. Test utility generation
8. Update build pipeline

#### Group 3: Simple Components (4-5 hours)
9. Migrate Badge to cn()
10. Migrate Switch to cn()
11. Test and verify both components

#### Group 4: Core Components (6-8 hours)
12. Migrate Button to cn()
13. Migrate Input to cn()
14. Migrate AccordionTrigger to cn()
15. Migrate DialogContent to cn()

#### Group 5: Service Layer (3-4 hours)
16. Simplify ThemeService
17. Create DarkModeToggle component
18. Update demo pages

#### Group 6: Testing & Documentation (4-6 hours)
19. Complete testing checklist
20. Create migration guide
21. Update theming.md documentation
22. Final validation

**Total Estimated Time**: 24-32 hours

---

## Key Challenges and Mitigations

### Challenge 1: Tailwind v4 Alpha Instability
**Risk**: Build breaks or unexpected behavior
**Mitigation**:
- Extensive testing before release
- Pin exact version
- Have rollback plan

### Challenge 2: Theme Conversion Complexity
**Risk**: Users struggle to migrate existing themes
**Mitigation**:
- Provide conversion tool
- Offer pre-converted themes
- Video tutorial for migration

### Challenge 3: Component Styling Regression
**Risk**: Subtle visual changes break layouts
**Mitigation**:
- Screenshot comparison testing
- Staged rollout
- Beta testing period

### Challenge 4: Dark Mode Edge Cases
**Risk**: Some components don't properly support dark mode
**Mitigation**:
- Test every component in both modes
- Fix issues before release
- Document any limitations

---

## Notes

This is a **foundational architectural change** that positions BlazorUI for long-term compatibility with the Shadcn ecosystem. While the breaking changes are significant, the benefits far outweigh the migration costs:

- **True Shadcn compatibility**: Copy-paste themes without modification
- **Modern color system**: OKLCH provides better color consistency
- **Simplified maintenance**: Single theme pattern reduces complexity
- **Consistent codebase**: cn() utility standardization
- **Future-proof**: Tailwind v4 and OKLCH are the future of web styling

**Recommended version**: v2.0.0 (major version bump due to breaking changes)

**Flag for review**: ✅ This plan requires architecture team review before implementation.

---

**Generated by DevFlow Architect Agent** | Plan Phase Complete
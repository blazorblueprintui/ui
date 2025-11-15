# Cross-Cutting Concerns Index

Quick reference to domain documentation. Load full docs on-demand when working on related features.

---

## Design

### Theming System
**File:** `design/theming.md`
**Summary:** CSS Variables-based theming following shadcn's approach. Supports light/dark modes, color customization via CSS custom properties, and Tailwind integration.
**Keywords:** theme, colors, CSS variables, dark mode, customization

---

## Infrastructure

### Internationalization (i18n)
**File:** `infrastructure/internationalization.md`
**Summary:** RTL layout support, localization-ready architecture, culture-aware formatting. Components use CSS logical properties and parameterizable strings.
**Keywords:** i18n, RTL, localization, translation, culture, Arabic, Hebrew

---

## Loading Strategy

**When to load full domain docs:**
- Feature spec mentions relevant keywords (theme, i18n, RTL, etc.)
- User explicitly tags concerns during `/spec`
- Implementation requires detailed guidance from domain doc

**Always load this index** for quick reference during planning.

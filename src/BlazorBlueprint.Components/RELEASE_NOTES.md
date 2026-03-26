## What's New in v3.8.0

### New Features

- **Sidebar CSS custom property theming** — All sidebar layout, sizing, and styling values are now driven by ~70 CSS custom properties with sensible defaults. Override variables on `:root` to fully theme the sidebar without `!important` or specificity battles.
- **BbSidebarMenuButton OnClick** — Added `OnClick` EventCallback for custom click handling alongside built-in collapsible toggle behavior.
- **Sidebar active state variables** — Active menu button appearance (background, color, shadow, font-weight) is now controlled via `--sidebar-menu-button-active-*` variables.
- **Sidebar badge theming** — Badge background and color are now configurable via `--sidebar-badge-bg` and `--sidebar-badge-color` variables.

### Bug Fixes

- **BbSidebarMenuButton data-active** — Fixed `data-active` attribute rendering boolean `"True"` instead of lowercase `"true"`.
- **BbSidebarMenuButton size variants** — Fixed size variant switch comparing against wrong string values (`"small"`/`"large"` instead of `"sm"`/`"lg"` from `ToValue()`).

### Improvements

- **Sidebar data attributes** — Added missing `data-sidebar` attributes to 10 sidebar components (`BbSidebarContent`, `BbSidebarFooter`, `BbSidebarHeader`, `BbSidebarHeaderContent`, `BbSidebarMenu`, `BbSidebarMenuBadge`, `BbSidebarMenuItem`, `BbSidebarMenuSub`, `BbSidebarMenuButton`, `BbSidebarMenuSubButton`) for consistent CSS targeting.
- **Sidebar data-size attributes** — Added `data-size` attributes to `BbSidebarMenuButton` and `BbSidebarMenuSubButton` for size-aware CSS styling.
- **Sidebar collapsible icon mode** — Icon-mode overrides for collapsed sidebar are now handled via CSS custom properties instead of inline Tailwind utilities.

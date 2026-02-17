## What's New in v3.0.0

### Breaking Changes
- All Razor components now use the `Bb` prefix (e.g., `Button` -> `BbButton`, `Dialog` -> `BbDialog`)
- Namespaces flattened to `BlazorBlueprint.Components` and `BlazorBlueprint.Primitives`
- Combobox and MultiSelect API redesigned: `TItem` replaced with `TValue`, simplified value binding
- Trigger and Close components default `AsChild` to `true`
- ApexCharts replaced with Apache ECharts using a new declarative composition API
- Portal system upgraded to two-layer architecture with category-scoped rendering

### New Features
- 7 new components added
- 30+ new usability parameters across existing components
- Toast semantic variants, compact size, auto icons, pause-on-hover, and per-toast positioning
- Auto-generated `id` attributes for all input components (accessibility)
- `DialogService` with programmatic `Confirm()` method
- `ScrollArea` `FillContainer` parameter and `SidebarInset` scroll reset on navigation
- `ActiveClass` parameter on Select, Combobox, MultiSelect, and DropdownMenu triggers
- `ForceMount` parameter on `FloatingPortal` for persistent portal content
- Custom `bb-theme-changed` event for theme change detection
- Switch `ThumbClass`, `ThumbCheckedClass`, `ThumbUncheckedClass`, and `ThumbContent` parameters

### Performance
- Menu keyboard navigation moved from C# to JavaScript
- Dialog, Sheet, and Drawer escape key handling moved to JavaScript
- Input components migrated to JavaScript-first event architecture
- Centralized input validation logic into shared `InputValidationBehavior`
- Select display text state change optimization to prevent unnecessary re-renders

### Bug Fixes
- Fixed `FloatingPortal` content staying visible one render behind on close
- Removed default focus ring from all input components

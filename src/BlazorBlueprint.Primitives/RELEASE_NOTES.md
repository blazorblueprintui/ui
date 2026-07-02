## What's New in v3.13.0

### New Features
- **DataGrid** — added `CellClassFunc` on `IDataGridColumn` to compute conditional per-cell CSS classes from the row's data item, combined with the static `CellClass`.

### Bug Fixes
- **Floating/Popover positioning** — portal coordinates now render with invariant culture, fixing invalid CSS in locales that use a decimal comma (e.g. de-DE).
- **Popover** — `AsChild` triggers now apply a pointer-events guard while open (via new `TriggerContext.SuppressPointerEventsWhenOpen`), so a single click can no longer close and immediately re-open the overlay.
- **JS interop disposal** — swallow `JSException` on dispose paths in **Sortable**, **TreeView**, **FocusManager**, and **PositioningService** to prevent errors during WebView2 reloads.

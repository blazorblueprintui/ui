## What's New in v3.3.0

### New Components

- **BbDataGrid** — Full-featured data grid with column filtering, expandable rows, context menus, row click events, pinned columns, column resize/reorder/visibility, multi-sort, virtualization, state persistence, and async loading
- **BbDynamicForm** — Schema-driven form rendering with JSON Schema support, conditional field visibility, multi-section and multi-column layouts, inline validation, and a comprehensive field type gallery
- **BbFilterBuilder** — Visual query builder for constructing filter expressions with nested groups, dynamic date presets, relative date operators, LINQ expression output, and DataTable integration
- **BbFormWizard** — Multi-step form wizard with step validation, controlled navigation, vertical layout, optional steps, step state retention, and icon support
- **BbTreeView** — Hierarchical tree component with checkable nodes, drag-and-drop reordering, lazy loading, search filtering, expand-on-click, custom templates, and flat data binding

### New Features

- **Charts**: Added Composite, Scatter, Candlestick, Heatmap, Gauge, and Funnel chart types
- **InputOTP**: Added Mask parameter for character masking and InputMode for mobile keyboard hints; added paste support
- **Combobox**: Added SearchQuery/SearchQueryChanged parameters for external async filtering
- **Dialog & Sheet**: Added CloseOnOverlayClick parameter to control overlay dismiss behavior
- **Recipes**: New Recipes section with a Filterable Data Grid recipe example

### Bug Fixes

- Fixed DataGrid click intercept, keyboard navigation, and row expansion issues
- Resolved chart display glitches in themed environments
- Fixed Select and Calendar performance issues and checkmark overlap
- Improved WASM rendering performance — fixed ColorPicker drag jitter, sidebar menu sluggishness, and CascadingValue overhead
- Fixed Sheet Modal parameter not being passed through context
- Fixed DropdownMenu keyboard navigation highlighting container instead of individual items when using Href

### Improvements

- Added accessibility and API reference sections to 26 demo pages
- Bumped BlazorBlueprint.Primitives dependency to 3.3.0

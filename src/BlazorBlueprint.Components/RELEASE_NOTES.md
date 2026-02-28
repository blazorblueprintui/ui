## What's New in v3.1.2

### New Components
- **DataView** (`BbDataView`) — Display collections in switchable list/grid layouts with built-in sorting, filtering, pagination, and infinite scroll support
- **TagInput** (`BbTagInput`) — Multi-value input for entering and managing tags

### New Features
- **Combobox** — Added `SearchQuery` and `SearchQueryChanged` parameters for external async data source filtering
- **Dialog / Sheet** — Added `CloseOnOverlayClick` parameter to control whether clicking the overlay dismisses the modal

### Bug Fixes
- **InputOTP** — Paste support now works via Ctrl+V and clipboard events
- **DropdownMenu** — Keyboard navigation now correctly highlights individual menu items instead of the parent container
- **Sheet** — Fixed `Modal` parameter not being wired through context, preventing dismissal correctly

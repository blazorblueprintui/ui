## What's New in v3.11.0

### New Features
- **BbDataView** — new `DataViewItemsProvider<TItem>` delegate (with `DataViewRequest`/`DataViewResult<TItem>`) for asynchronous server-side data fetching driven by pagination, sort, and search state.
- **BbPopover** — new `RestoreFocusOnClose` parameter that returns focus to the trigger when the popover is closed via the controlled `Open` binding.

### Bug Fixes
- **Collapsible / DropdownMenu** — Space/Enter on the trigger no longer double-toggles (opening then closing in a single press).
- **Body scroll lock** — now reference-counted so nested/stacked overlays correctly restore page scroll once the last overlay closes.
- **Sortable** — corrected the library filename casing so it loads on case-sensitive filesystems.

### Improvements
- **Select / Popover / DropdownMenu** — focus returns to the trigger on intentional close (Escape, item selection) so keyboard Tab navigation continues from the right place; click-outside dismissals leave focus where the user clicked.

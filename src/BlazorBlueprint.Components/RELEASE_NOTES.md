## What's New in v3.11.0

### New Features

- **BbDataView**: Added `ItemsProvider` for server-side / lazy data loading (#306).
- **BbCommandVirtualizedGroup**: Added `ItemsProvider` for server-side lazy loading (#345).
- **BbDataGridSelectColumn**: Added `CellClass` and `HeaderClass` parameters to style the selection cells and header (#346).
- **BbPopover**: Added `RestoreFocusOnClose` parameter to return focus to the trigger on controlled close (#349).

### Bug Fixes

- **Combobox / MultiSelect**: Restore focus to the trigger on close so Tab navigation continues (#349).
- **Combobox**: Show the selected item's label for pre-bound values in compositional mode (#337, #343).
- **BbSidebarMenuButton / BbSidebarMenuSubButton**: Set `data-active` on menu links during navigation (#324).
- **Spinner**: Keep spinners and pulse indicators animating under `bb-no-animate` (#330).

### Improvements

- Bumped the `BlazorBlueprint.Primitives` dependency to 3.11.0.

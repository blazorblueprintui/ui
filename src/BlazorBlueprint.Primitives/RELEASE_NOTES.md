## What's New in v3.14.0

### New Features
- **DataGrid** — added `Groupable` on `IDataGridColumn` so columns can opt into runtime grouping from the column header menu (defaults to `false`; the group key comes from the column's raw value).
- **DataGrid** — added `DataGridGroupState.Version`, a counter that increments whenever the active group definition changes, letting the grid detect grouping changes made directly against the state.

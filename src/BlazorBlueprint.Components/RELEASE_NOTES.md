## What's New in v3.7.1

### Bug Fixes

- **DataGrid** hierarchy filtering now paginates by visible rows instead of root count, fixing cases where a single root with thousands of descendants rendered on one page
- **DataGrid** no longer auto-expands the entire tree when a filter matches everything (e.g. incomplete FilterBuilder conditions with no value entered)

### Performance

- **DataGrid** hierarchy/grouped rendering now supports virtualization — only visible rows are in the DOM regardless of total item count
- **DataGrid** automatically switches to `ShowMatchedOnly` filter mode when the dataset exceeds `HierarchyLargeDatasetThreshold` (default 500) to prevent subtree explosion with large hierarchies

### Improvements

- Bumped **BlazorBlueprint.Primitives** dependency to 3.7.1

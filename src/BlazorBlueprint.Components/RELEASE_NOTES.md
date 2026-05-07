## What's New in v3.10.0

### Breaking Changes

- **BbDataTable**: Removed the placeholder Filter popover from the toolbar along with the `DataTable.Filter` and `DataTable.FilterColumns` localization keys. Use `BbDataGrid` for per-column filter UIs.

### Improvements

- **CSS cascade**: Component styles now ship inside a dedicated `@layer bb` cascade layer declared as the strongest layer, so component utilities win the cascade regardless of consumer stylesheet load order.

### Bug Fixes

- **BbTabs**: `class` and `AdditionalAttributes` (e.g. `id`, `data-*`) are now forwarded to the primitive root element; the redundant outer wrapper div has been dropped.
- **BbFilterBuilder**: Filter condition rows and inner range / InLast sub-rows now wrap onto multiple lines on small screens instead of overflowing the parent card.

## What's New in v3.1.1

### Bug Fixes

- **`BbDialogContent`** and **`BbSheetContent`**: Added `CloseOnOverlayClick` parameter (default: `true`) to control whether clicking the backdrop overlay dismisses the dialog or sheet. This surfaces the underlying primitive's `CloseOnClick` option that was previously inaccessible at the Components layer.
- **`BbAlertDialogContent`**: Fixed a bug where clicking the backdrop overlay incorrectly closed the dialog. Per `role="alertdialog"` accessibility semantics, the overlay is now non-dismissable by default — users must interact with an explicit action button.

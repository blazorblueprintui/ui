## What's New in v3.17.0

### New Features
- **Charts** — new `OnDataPointClick` and `OnChartClick` on every chart. `OnDataPointClick` passes a `ChartClickEventArgs` with the series, `DataIndex`, `Name` and value; `OnChartClick` fires only for clicks away from a data point. No interop is set up unless a handler is attached.
- **BbFileUpload** — new `AllowPaste` (default `true`) accepts files pasted while focus is inside the upload, including screenshots. Pasted files go through the same validation as dropped files.
- **Theme** — new `js/theme-init.js` applies the saved theme before the first paint, removing the flash of the default theme on prerendered pages. Load it as a classic, blocking script in `<head>`.
- **BbDialogContent**, **BbSheetContent**, **BbDrawerContent** — new `InitialFocus` and `InitialFocusElement` control what receives focus on open. The default is unchanged.

### Bug Fixes
- **Focus trap** — overlays no longer force focus onto their first tabbable child, so a child that acts on focus no longer fires on open.
- **BbHoverCardTrigger** — no longer opens on programmatic focus, such as focus moved by a focus trap. Tabbing to it still opens it.
- **BbCopyText** — the tooltip no longer opens, and stays open, when returning to a background browser tab.
- **BbInputGroupAddon** — removed a click handler that did nothing.

### Improvements
- **Accessibility** — the themed focus ring replaces the browser outline on 21 components, including `BbAccordionTrigger`, `BbCollapsibleTrigger`, `BbDialogTrigger`, `BbSheetTrigger`, `BbAlertDialogTrigger`, `BbPopoverTrigger`, `BbBreadcrumbLink`, `BbCarouselNext`/`BbCarouselPrevious`, `BbRating` and `BbThemeSwitcher`. `BbAttachmentTrigger` and `BbCommandInput` previously showed no focus indicator at all.
- **BbFileUpload** — the focus ring now shows on the visible dropzone instead of the hidden file input.
- Updated the `BlazorBlueprint.Primitives` dependency to 3.17.0.

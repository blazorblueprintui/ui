## What's New in v3.16.1

### New Features
- **Charts** — new `OnDataPointClick` and `OnChartClick` on `BbChartBase`, so bar, line, area, pie, scatter, candlestick and the rest all report clicks. `OnDataPointClick` carries a `ChartClickEventArgs` with `SeriesName`, `SeriesIndex`, `DataIndex`, `Name`, `ComponentType`, `Value` and `Values`; `DataIndex` maps straight back to the bound collection. `OnChartClick` fires only when the click landed away from a data point. No interop is set up unless one of the two has a handler.
- **BbFileUpload** — new `AllowPaste` (default `true`) accepts files pasted with Ctrl+V while focus is inside the upload, including screenshots. Pasted text is ignored, and the files run through the same validation, limits and `OnValidationError` path as a drop.
- **Theme** — new `js/theme-init.js` applies the saved theme to `<html>` before the first paint, removing the flash of the default theme on prerendered and statically rendered pages. Load it as a classic, blocking script in `<head>`; `data-default-dark` and `data-storage` configure it.
- **BbDialogContent**, **BbSheetContent**, **BbDrawerContent** — new `InitialFocus` and `InitialFocusElement` choose what the focus trap focuses on open. Resolution order is the explicit element, `[data-autofocus]`, the mode, then the first tabbable descendant. The default is unchanged.

### Bug Fixes
- **Focus trap** — an overlay no longer forces focus onto its first tabbable descendant unconditionally, so a first child that acts on focus no longer fires that side effect on open.
- **BbHoverCardTrigger** — the card no longer opens on a programmatic focus, such as one moved by a focus trap. A real Tab still opens it, on both `AsChild` branches.
- **BbCopyText** — the tooltip no longer opens when the browser restores focus on returning to a background tab, where nothing then closed it.
- **BbInputGroupAddon** — removed a click handler that did nothing but swallow the click. The addon is a presentational wrapper.

### Improvements
- **Accessibility** — the themed focus ring now replaces the browser's own outline on 21 components: `BbAccordionTrigger`, `BbAlertDialogTrigger`, `BbAttachmentTrigger`, `BbBreadcrumbLink`, `BbCarouselNext`, `BbCarouselPrevious`, `BbCollapsibleTrigger`, `BbColorPicker`, `BbCommandInput`, `BbCopyText`, `BbDateTimePicker`, `BbDialogTrigger`, `BbFileUpload`, `BbFormWizard`, `BbMarker`, `BbPopoverTrigger`, `BbRating`, `BbResponsiveNavItems`, `BbSheetTrigger`, `BbSidebarRail` and `BbThemeSwitcher`. `BbAttachmentTrigger` and `BbCommandInput` previously drew no focus indicator at all.
- **BbFileUpload** — the focus ring goes on the visible dropzone through `focus-within`, rather than on the transparent file input laid over it where it showed nothing.
- Bumped the `BlazorBlueprint.Primitives` dependency to 3.16.1.

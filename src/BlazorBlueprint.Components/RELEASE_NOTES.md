## What's New in v4.2.0

### Breaking Changes

- **Text parameter defaults**: these parameters now default to `null` instead of English text, and fall back to `IBbLocalizer` keys, so what renders is unchanged. Code that reads them before setting them now gets `null`.
  - `BbFormFieldCombobox`: `Placeholder`, `SearchPlaceholder`, `EmptyMessage`
  - `BbFormFieldMultiSelect`: `Placeholder`, `SearchPlaceholder`, `EmptyMessage`, `SelectAllLabel`, `ClearLabel`, `CloseLabel`
  - `BbCheckboxGroup`, `BbFormFieldCheckboxGroup`: `SelectAllLabel`
  - `BbTimePicker`, `BbFormFieldTimePicker`: `Placeholder`
  - `BbCommandInput.Placeholder`, `BbTreeView.SearchPlaceholder`, `BbDynamicForm.SubmitText`
  - `DialogOptions.ConfirmText`/`CancelText`, `AlertDialogOptions.ButtonText`
- **Input styling**: field boxes use shadcn v4's fill (`bg-transparent dark:bg-input/30`, `shadow-xs`), so a theme's `--input` colour now shows in dark mode. Fields look slightly different in dark mode.
- **BbToastProvider**: its stacks now sit inside a polite live region (`<section aria-live="polite">`). Only destructive toasts keep `role="alert"`.
- **Text inputs** (`BbInput`, `BbTextarea`, `BbInputGroupInput`, `BbInputGroupTextarea`): with `Immediate` or `Debounced` timing, blur no longer raises `ValueChanged` a second time with the value typing already raised.
- **Dependencies**: `Microsoft.AspNetCore.Components.Web` is now 10.0.14.

### New Features

- **BlazorBlueprint.PdfViewer**: a new opt-in package with `BbPdfViewer`, a PDF.js-based viewer with paging, zoom, fit-to-width and download, loading from a URL or from bytes and streams. It is a separate package because PDF.js is over 2 MB; load `blazorblueprint-pdfviewer.css` after `blazorblueprint.css`.
- **BbDialog** and **DialogOpenOptions**:
  - `Position` (`DialogPosition`, nine placements).
  - Enter and exit animations: `EnterPreset`, `ExitPreset`, `EnterKeyframes`, `ExitKeyframes`, `AnimationDuration`. A closing dialog stays mounted until its exit animation ends, and reduced motion skips them.
- **BbSheetContent**: `ShowOverlay="false"` gives a non-modal sheet (no overlay, `aria-modal="false"`, no focus trap or scroll lock), and `CloseOnInteractOutside` closes it on an outside pointer-down.
- **Overlays**: Sheet, Dialog, DialogService, AlertDialog and Drawer overlays carry a `data-slot` (`sheet-overlay`, `dialog-overlay`, `alert-dialog-overlay`, `drawer-overlay`).
- **BbDataGrid**:
  - `MobileLayout="Stacked"` with `MobileBreakpoint` turns rows into cards on small screens. Columns take `MobileLabel` and `HideOnMobile`.
  - The expand and edit columns gain `HeaderClass` and `CellClass`.
- **Toasts**:
  - `ToastService.Show<TComponent>()` shows a component in a toast. `IToastReference.DismissAsync()` lets it close itself.
  - `ToastData.Icon`, `Content` and `Actions` slots.
  - A toast with `Content` or `Actions` stops auto-dismissing once the user presses or moves focus inside it.
- **BbCombobox**, **BbMultiSelect**: `TriggerId`, `TriggerAttributes` and `IsInvalid`.
- **BbColorPicker**: `ActionButtons` adds Apply and Cancel.
- **BbMenubarItem**, **BbCommandDialog**: `AllowShortcutInEditable`.
- **Localization**:
  - `DefaultBbLocalizer.EnglishDefaults` lists every key with its English text.
  - About 90 new keys cover text that was hard-coded English: TimePicker, MarkdownEditor, RichTextEditor, Toast, Carousel, Rating, TreeView, DynamicForm, dialogs, and barcode, Gantt and form-field validation messages.
- **Theme**:
  - A `surface` colour token (`--bb-surface`). Cards, dashboard widgets, popovers and hover cards declare it, so sticky DataGrid, Gantt and Scheduler surfaces match their container.
  - Theme switcher radius buttons preview their corner radius.

### Bug Fixes

- **BbGauge**: `Color` and `ProgressColor` now colour the progress arc.
- **BbTreeView**: selection stays fast with thousands of siblings.
- **BbToastProvider**: toasts already showing time out after navigating to a page with another layout.
- **BbMenubar**:
  - One roving Tab stop.
  - Clicks in portaled submenus keep it open.
  - Item shortcuts follow parameter changes.
- **Form fields**: FormFieldSelect, FormFieldCombobox and FormFieldMultiSelect labels point at the trigger, and the invalid state reaches `aria-invalid`. Combobox `aria-controls` names the list that opens.
- **Comma-decimal cultures**: TreeView indentation, Progress, context menu position, Rating, and ARIA and `data-*` numbers are written invariantly. NumericInput keeps its decimals through focus and blur.
- **BbDataGrid**: a column width is never stored as zero, and sticky surfaces no longer show a darker box in dark cards.
- **BbDataTable**: wide tables scroll inside their border.
- **BbDateRangePicker**: only the picked preset is highlighted when presets share a range.
- **BbInputGroup**: one focus ring on the group.
- **BbMultiSelect**: Escape closes only the list inside a dialog.
- **Text inputs**: `UpdateTiming` and `DebounceInterval` changes apply after the first render.
- **BbColorPicker**: the presets label spacing applies.

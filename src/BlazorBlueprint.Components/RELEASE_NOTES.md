## What's New in v3.13.0

### New Components
- **BbDock** — IDE-style docking container whose panels can be dragged, re-docked as tabs or splits, detached into floating windows, maximized, pinned, closed and reopened, with min/max size constraints.
- **BbEventCalendar** — generic event calendar with month, week and agenda views, per-event coloring and templating, two-way bindable `View`/`CurrentDate`, built-in navigation, and event/date/range callbacks.
- **BbDateTimePicker** & **BbFormFieldDateTimePicker** — combined date and time picker with calendar plus hour/minute/second and AM/PM steppers, 12/24h formats, Now/Clear actions and EditForm integration.
- **BbCopyText** — copy-to-clipboard component with a hover tooltip that reflects copy state and an `OnCopied` callback.
- **BbMessage**, **BbBubble**, **BbAttachment** & **BbMarker** — chat and messaging component families for building conversation UIs (message groups, chat bubbles with reactions, file attachments, and markers).

### New Features
- **BbDatePicker** — manual date entry via `Editable` and `InputFormats`, letting users type dates that are parsed against configured formats (ISO always accepted) and reverted when invalid.
- **BbThemeSwitcher** — independent base and primary color selection via the new `ColorLayout` (`Split`/`Combined`); selecting a base color no longer resets the primary.
- **BbCalendar**, **BbDatePicker** & **BbDateRangePicker** — per-day customization through `DayTemplate` (custom day content via `CalendarDayContext`) and `DayClassFunc` (conditional per-day CSS).
- **BbDataGrid** — `CellClassFunc` on property, template and hierarchy columns for conditional per-cell styling from the row's data.
- **BbDateRangePicker** — `ShowButtons` and `AutoApply` parameters; `AutoApply` applies and closes the popover once a complete valid range is selected.
- **Input components** — public `FocusAsync()` method and `Element` reference on `BbInput`, `BbTextarea`, `BbInputField`, `BbInputGroupInput`, `BbInputGroupTextarea`, `BbNumericInput`, `BbCurrencyInput`, `BbMaskedInput`, `BbTagInput` and the FormField wrappers.
- **BbCheckbox** — new `Name` parameter (forwarded by `BbFormFieldCheckbox`) for form submission.

### Bug Fixes
- **Forms** — input components now emit the full model path (e.g. `Input.Username`) in the `name` attribute so `[SupplyParameterFromForm]` binds on SSR/enhanced form submissions; `BbCheckbox` now renders a hidden native checkbox so it posts a value.
- **BbPopover** — pointer-events open-guard is now applied to `AsChild` triggers.
- **BbSidebar** — menu buttons now fire `OnClick` when rendered as an anchor.
- **JS interop** — swallow `JSException` on dispose paths to avoid errors during WebView2 reloads.

## What's New in v3.15.0

### Breaking Changes
- **BbDataGrid** — the JS-invokable `OnColumnReordered` now takes `(columnId, targetColumnId, placeAfter)` instead of `(columnId, newIndex)`; the drop gesture is resolved to a position on the .NET side.

### New Features
- **BbSidebarProvider** — new `Open`/`OpenChanged` and `OpenMobile`/`OpenMobileChanged` for controlled (`@bind-Open`) sidebar state; cookie persistence is suppressed while the desktop state is bound.
- **BbCopyText** — new `ValueFunc` resolves the copied text at click time, for values that are derived or expensive to compute. `Value` is no longer `EditorRequired` and wins when non-empty.
- **BbNumericInput**, **BbCurrencyInput**, **BbFormFieldNumericInput**, **BbFormFieldCurrencyInput** — new opt-in `EnableWheelStep` steps the value from the mouse wheel while the input is focused, accumulating delta so a trackpad flick doesn't step wildly.
- **BbDataGridPropertyColumn**, **BbDataGridTemplateColumn**, **BbDataGridHierarchyColumn** — new `Order` sets an explicit column position, for columns produced by wrappers or async fragments where registration order doesn't match declaration order.
- **BbDataGridPropertyColumn** — new `HeaderTemplate` replaces the header title text while keeping the sort indicator, filter icon, pin icon, column menu and resize handle.
- **BbLine**, **BbArea**, **BbScatter** — new `XDataKey` plots each point at its own X coordinate on a value, time or log axis instead of at its ordinal position.
- **BbXAxis** — new `Scale` lets a value axis auto-fit to the data range rather than always including zero.
- **BbFileUpload** — new `Id` for associating an external `<label for>` with the file input, and unmatched attributes now splat onto the `<input type="file">`.

### Bug Fixes
- **BbDataGrid** — rightward column drags no longer land one position too far right.
- **BbDataGrid** — columns registering in a later render pass are merged into the column state instead of being silently dropped from the grid.
- **BbDataGrid** — a header cell with a `HeaderTemplate` is named from the column's `Title`, so an icon-only header is no longer unannounced.
- **BbCurrencyInput** — focus, sanitisation and parsing now run through the currency's culture; previously an invariant-formatted edit value parsed against a comma-decimal culture (or a Blazor Server host culture) could inflate the amount.
- **BbColorPicker** — hue, saturation, brightness and alpha are formatted with invariant culture and fixed-point precision, so comma-decimal locales produce valid CSS.
- **BbRangeSlider** — tick marks and thumb positions use invariant, fixed-point formatting.
- **BbCopyText** — the tooltip renders through the floating portal, so an ancestor with `overflow: hidden|auto` can no longer clip it.
- **BbDashboardGrid** — stale JS→.NET callbacks to a disposed grid are dropped rather than logged as errors, and observers that outlive the grid element dispose themselves.
- **BbNumericInput** — the stepper buttons size from the field rather than a fixed height, so a custom height such as `h-8` no longer leaves them overhanging the input.
- **BbTooltipTrigger**, **BbHoverCardTrigger** — an `AsChild` trigger whose child ignores `TriggerContext` now logs a warning in the Development environment instead of silently never opening.

### Improvements
- **Focus visibility** — **BbInput**, **BbTextarea**, **BbInputField**, **BbInputGroupInput**, **BbInputGroupTextarea**, **BbMaskedInput**, **BbNumericInput**, **BbCurrencyInput**, **BbInputOTP**, **BbNativeSelect**, **BbSelectTrigger**, **BbCombobox** and **BbMultiSelect** now render a visible focus ring, hugging the field border on inputs and offset on button-style triggers (**BbDatePickerInput** toggle, **BbDrawerItem**, **BbResponsiveNavTrigger**).
- **Dependencies** — `HtmlSanitizer` moved from the 9.0.x line to stable 9.1.982, which resolves AngleSharp 1.x and clears GHSA-pgww-w46g-26qg.
- Bumped the `BlazorBlueprint.Primitives` dependency to 3.15.0.

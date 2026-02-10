## What's New in v2.5.0

### New Features
- Control-specific FormField wrapper components: `FormFieldCheckbox`, `FormFieldCombobox`, `FormFieldMultiSelect`, `FormFieldRadioGroup`, `FormFieldSelect`, `FormFieldSwitch`, and `FormFieldInput` — pre-configured wrappers that simplify EditForm integration for non-text controls
- EditForm and EditContext integration for `InputField` and `FormField` components with cascading validation support
- API surface snapshot tests for Components and Primitives assemblies to detect unintentional public API changes

### Bug Fixes
- Popover-based components (DatePicker, ColorPicker, TimePicker, Select, Combobox, MultiSelect) now render correctly above Dialog, Sheet, and Drawer overlays
- Fixed infinite render loop when opening dropdown controls inside a Dialog
- Fixed portal render timeout when floating content is nested inside other portals
- Dialog and Sheet portal content now properly refreshes on re-render
- Desktop sidebar visibility restored — `hidden` class no longer overrides responsive `md:flex`
- Combobox selected item checkmark moved from left to right side for consistency
- `FormFieldCheckbox` supports `HorizontalEnd` and `VerticalEnd` label orientations

### Improvements
- Bumped BlazorBlueprint.Primitives dependency to 2.3.2

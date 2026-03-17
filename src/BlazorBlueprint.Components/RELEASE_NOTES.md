## What's New in v3.6.0

### New Components

- **BbSortable** — Drag-and-drop sortable list and grid component powered by Sortable.js with support for single lists, connected multi-lists, grid layouts, drag handles, and WAI-ARIA accessibility
- **BbFormFieldTextarea** — FormField wrapper for Textarea with label, description, error, and EditForm integration
- **BbFormFieldDatePicker** — FormField wrapper for DatePicker with label, description, error, and EditForm integration
- **BbFormFieldTimePicker** — FormField wrapper for TimePicker with label, description, error, and EditForm integration
- **BbFormFieldNumericInput** — FormField wrapper for NumericInput with label, description, error, and EditForm integration
- **BbFormFieldNativeSelect** — FormField wrapper for NativeSelect with label, description, error, and EditForm integration
- **BbFormFieldCurrencyInput** — FormField wrapper for CurrencyInput with label, description, error, and EditForm integration
- **BbFormFieldMaskedInput** — FormField wrapper for MaskedInput with label, description, error, and EditForm integration
- **BbFormFieldInputOTP** — FormField wrapper for InputOTP with label, description, error, and EditForm integration
- **BbFormFieldTagInput** — FormField wrapper for TagInput with label, description, error, and EditForm integration
- **BbFormFieldDateRangePicker** — FormField wrapper for DateRangePicker with label, description, and error display
- **BbFormFieldCheckboxGroup** — FormField wrapper for CheckboxGroup with label, description, and error display
- **BbFormFieldFileUpload** — FormField wrapper for FileUpload with label, description, and error display

### New Features

- **BbSortable** — Lazy loading for the Sortable.js library to reduce initial bundle size
- **BbSortable** — `SortableLayout` enum with `List` and `Grid` layout modes
- **BbSortable** — `OnAdd` event for tracking cross-list item transfers between connected lists

### Performance

- **BbSortable** — Sortable.js library is loaded on-demand only when a sortable component is first rendered

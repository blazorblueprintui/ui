# Changelog

All notable changes to Blazor Blueprint are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

---

## 2026-02-10

### Fixed
- `DialogPortal` not refreshing content on re-render — event handlers inside dialog content would update state but the UI wouldn't re-render until an unrelated browser event occurred (#118)
- `SheetPortal` using `UpdatePortalContent` instead of `RefreshPortal`, which unnecessarily replaced the RenderFragment delegate on every parameter update

---

## 2026-02-09

### Added
- Control-specific `FormField` wrapper components: `FormFieldCheckbox`, `FormFieldSwitch`, `FormFieldRadioGroup`, `FormFieldSelect`, `FormFieldCombobox`, and `FormFieldMultiSelect` — pre-configured FormField wrappers that simplify EditForm integration for non-text controls
- `FormFieldBase` base class providing shared EditForm/EditContext integration, validation, label, description, and error message rendering
- `FormFieldInput` component for typed text input with built-in form field layout (label, description, validation messages)
- Flag icon and avatar examples in `FormFieldSelect` demo
- Demo pages for each FormField wrapper in the Components sidebar section
- EditForm integration for `InputField` components with cascading `EditContext` support

### Fixed
- Build errors related to FormField component integration
- Added `string` type to `GetFriendlyTypeName` in FormField for proper type display
- DateRangePicker popover layout responsive on mobile devices (single-month view with horizontal preset scrolling)
- DateRangePicker day name headers now rotate correctly based on `FirstDayOfWeek` parameter
- DateRangePicker no longer highlights disabled dates within a selected range
- DateRangePicker selected day count excludes disabled dates
- DateRangePicker presets are ignored when all dates in the range are disabled

---

## 2026-02-08

### Added
- `CursorType` enum and `CursorExtensions.ToClass()` utility for mapping cursor types to Tailwind CSS classes
- Cursor and pointer-events conflict resolution groups in TailwindMerge
- Cursor behavior demo section on Button demo page
- `InputField<TValue>` component — generic typed input with automatic type conversion, formatting, validation, and parse error handling
- `InputConverter<TValue>` system with global, instance, and built-in default converter resolution for 15+ types
- `UpdateTiming` enum for Input and InputField — `Immediate` (default), `OnChange` (blur/Enter), and `Debounced` modes
- `DebounceInterval` parameter (default 500ms) for debounced value updates
- `UpdateTiming` and `DebounceInterval` parameters on `Textarea`, `InputGroupInput`, and `InputGroupTextarea` — matching the `Input` component pattern
- `DisableDebounce` and `DebounceInterval` parameters on `CommandInput`, `NumericInput<T>`, and `CurrencyInput` — debounce ON by default with selective immediate firing on blur/arrow keys
- Update Timing / Debounce demo sections with live value displays on Textarea, InputGroup, Command, NumericInput, and CurrencyInput demo pages
- InputField demo page with examples for all supported types and features
- `SetPosition()` and `ResetPosition()` methods on `ToastService` for runtime toast position control
- `TimelineConnectorFit` enum with `Spaced` and `Connected` options for controlling how connector lines fit between icons
- `ConnectorFit` parameter on `Timeline` component
- `ConnectorClass` parameter on `TimelineItem` for custom connector height overrides
- `Loading` parameter on `TimelineIcon` for isolated pulse animation
- Rich Content, Connector Fit, and Custom Connector Height demo sections for Timeline
- EditForm and EditContext integration for 14 input components (`Input`, `Textarea`, `InputGroupInput`, `InputGroupTextarea`, `NumericInput`, `CurrencyInput`, `MaskedInput`, `Select`, `Combobox`, `NativeSelect`, `DatePicker`, `TimePicker`, `ColorPicker`, `InputOTP`) — cascading `EditContext`, `ValueExpression`, field validation tracking, and merged `aria-invalid` state
- `Name` parameter on text input components with auto-fallback to `FieldIdentifier` for SSR form postback support
- Form Validation demo sections on 12 component demo pages showing EditForm with `DataAnnotationsValidator` and `ValidationMessage`

### Changed
- Button component now shows `cursor-pointer` by default and `disabled:cursor-not-allowed` when disabled, replacing `disabled:pointer-events-none` for better UX feedback
- Added Salary column with currency format (`C0`) to the DataTable demo Basic Table example
- Command palette keyboard shortcut changed from `Ctrl+K` to `Ctrl+I` across demo pages to avoid browser address bar conflict
- `CommandDemo` refactored to use `IKeyboardShortcutService` instead of raw JS interop for Ctrl+I shortcut registration

### Fixed
- DataTable global search now uses the column's `Format` string when converting values for search matching, consistent with how cell values are rendered
- Text input components (`Input`, `Textarea`, `InputGroupInput`, `InputGroupTextarea`) now normalize empty/whitespace strings to `null`, preventing inconsistent validation on nullable string properties (#99)
- Dropdown mispositioning when switching between multiple open MultiSelect components
- Stale portal content when interacting inside open popovers
- ContextMenu keyboard navigation breaking on repeated right-clicks on the same trigger
- Toast positioning demo now interactive with live position switching
- Timeline connector line no longer uses fixed height — dynamically stretches to match content height (#104)
- Timeline connector gap asymmetry between top and bottom icons resolved with symmetric spacing
- Timeline loading pulse animation no longer bleeds through icon ring onto connector line
- Timeline collapsible item chevrons now align consistently regardless of title length
- Menubar demo icon name corrected from `check-circle` to `circle-check`
- Timeline connector `z-index` changed from `z-10` to `z-[1]` to prevent overlap with overlays
- Timeline connector minimum height increased from `min-h-8` to `min-h-16` for better spacing

---

## 2026-02-07

### Added
- Timeline component with composable sub-components (Timeline, TimelineItem, TimelineIcon, TimelineConnector, TimelineContent, TimelineHeader, TimelineTitle, TimelineDescription, TimelineTime, TimelineEmpty)
  - Layout alignment options (Center, Left, Right, Alternate)
  - Icon variants (Solid, Outline) with status-based coloring
  - Connector line styles (Solid, Dashed, Dotted)
  - Loading state with pulse animation
  - Collapsible items support
  - Size presets (Small, Medium, Large)
- DataTableColumn `Format` parameter for custom formatting of cell values (e.g., date/number formats via `IFormattable`)
- DataTableColumn now supports nullable `TValue` types for columns with null values
- Custom variant/styling examples to Button, Badge, Alert, and Toast demo pages showing how to use the `Class` parameter for project-specific variants
- `Class` property on `ToastData` for custom toast styling via TailwindMerge
- SplitButton component — primary action button with a dropdown menu for secondary actions, composing Button + DropdownMenu with SplitButtonItem and SplitButtonSeparator sub-components

### Changed
- `blazorblueprint.css` removed from git tracking — now rebuilt automatically during build via MSBuild targets (Windows and Linux), eliminating merge conflicts across branches

### Fixed
- Infinite render loop when FloatingPortal is nested inside Dialog (e.g., Combobox in Dialog) — removed re-registration in `OnParametersSet` to match DialogPortal pattern
- Migrated all components to `ClassNames.cn()` for proper TailwindMerge support
- TailwindMerge incorrectly classified `border-dashed`, `border-dotted`, and `border-l-2` as border-color utilities, causing them to be dropped during class merging


---

## 2026-02-06

### Added
- Button component now supports `Href` and `Target` parameters, rendering as an `<a>` element for navigation links while preserving all visual styling
- ColorPicker alpha (A) input field alongside R/G/B for precise transparency control

### Changed
- Coding standards are now enforced at build time

### Fixed
- Resolved all build warnings and configured warnings-as-errors for stricter code quality
- ColorPicker alpha channel now displays clean 6-char hex with percentage instead of confusing 8-char hex format
- ColorPicker alpha precision no longer drifts on repeated interactions
- ColorPicker demo alpha preview CSS corrected for proper transparency rendering

---

## 2026-02-05

### Fixed
- Touch and drag interactions now work on mobile devices for Slider, RangeSlider, Resizable, and ColorPicker components (migrated from mouse events to Pointer Events)
- RangeSlider thumb z-index no longer appears above dialog overlays
- Removed duplicate sidebar.js script tags causing console errors in demos
- Fixed broken theme CSS and image asset paths in demo apps

---

## 2026-02-04

### Added
- Multi-hosting model demo support - demos now run in Auto, Server, and WebAssembly modes

---

## 2026-02-03

### Fixed
- ScrollArea overflow and viewport height constraints for proper scrolling
- Spinner custom color support restored
- Compiler warnings (CS1998, CS0414) resolved across components
- DropdownMenu components now use fully qualified names to avoid ambiguity

### Changed
- Renamed "BlazorBlueprint" to "Blazor Blueprint" in page titles and prose

---

## 2026-02-02

### Changed
- Project renamed from BlazorUI to BlazorBlueprint for v2.0.0
- Repository URL updated to blazorblueprintui/ui

### Fixed
- ApexCharts legend now uses theme-aware styles

---

## 2026-02-01

### Added
- Chart Components Library with 6 chart types built on Blazor-ApexCharts:
  - AreaChart (Default, Spline, Stacked, Stepline variants)
  - BarChart (Vertical, Horizontal, Stacked, StackedHorizontal, FullStacked, FullStackedHorizontal, Grouped variants)
  - LineChart (Default, Spline, Stepline, Dashed, Gradient variants)
  - PieChart (Pie, Donut, GradientDonut variants)
  - RadarChart (Default, PolygonFill, MultiSeries variants)
  - RadialChart (Default, SemiCircle, Gauge, Gradient variants)
- ChartContainer wrapper component for consistent card-like styling
- ChartConfig for series-to-label/color mapping with CSS variable integration

### Fixed
- Select dropdown scroll flash when opening with a selected item

### Docs
- Comprehensive theming guide (THEMING.md) covering CSS variables, dark mode, and customization
- Chart components README with architecture explanation and usage examples
- ApexCharts.js and Blazor-ApexCharts added to acknowledgments (MIT License)
- README updated with accurate component (65+) and primitive (15) counts

---

## 2026-01-31

### Added
- FloatingPortal component for unified floating content infrastructure (portal registration, positioning, lifecycle management)

### Changed
- PopoverContent, TooltipContent, HoverCardContent, and DropdownMenuContent primitives refactored to use FloatingPortal (~600 lines of duplicated code removed)

### Fixed
- Select dropdown positioning flash on open
- Controlled state support for all floating components (Popover, Tooltip, HoverCard, DropdownMenu)
- Tooltip positioning now correctly derives Side/Align from context Placement
- PopoverContent MatchTriggerWidth not working in Combobox and MultiSelect
- Duplicate margin in Select dropdown causing inconsistent spacing with other dropdowns

---

## 2026-01-30

### Added
- ResponsiveNav component set for mobile-friendly navigation:
  - ResponsiveNavProvider: Context provider with JS interop for mobile detection
  - ResponsiveNavTrigger: Hamburger menu button (visible only on mobile, below 768px)
  - ResponsiveNavContent: Mobile Sheet content wrapper with auto-close on navigate
- NavigationMenuDemo updated with responsive example showing search and notification icons

---

## 2026-01-29

### Added
- 9 new input and selection components:
  - ColorPicker: Visual color selection with hex/RGB input, preset colors, and optional alpha channel
  - CurrencyInput: Locale-aware currency input with 40+ currencies and automatic formatting
  - DateRangePicker: Date range selection with dual calendar view and preset ranges
  - FileUpload: Drag-and-drop file upload with image previews, validation, and progress tracking
  - MaskedInput: Input with structured formats (phone, SSN, credit card, custom masks)
  - NumericInput: Numeric input with increment/decrement buttons and min/max constraints
  - RangeSlider: Dual-handle slider for selecting value ranges
  - Rating: Star rating component with half-value support and custom icons
  - TimePicker: Time selection with 12/24-hour format and keyboard navigation

### Fixed
- TailwindMerge validation now supports CSS combinator characters (`>`, `+`, `~`)
- DataTable select-all checkbox no longer toggles before dropdown option is selected
- CurrencyInput and NumericInput focus ring moved to container level

### Performance
- DataTable selection optimized for large datasets

### Docs
- README updated with hero image and improved intro

---

## 2026-01-27

### Added
- DataTable select-all dropdown - when total items exceed current page, clicking select-all checkbox shows dropdown with options to "Select all on this page" or "Select all X items" across all pages
- EnableKeyboardNavigation parameter for Table and DataTable components
- CSS custom properties for typography (`--tracking-*` letter-spacing variables, `--font-sans/serif/mono` font family variables)
- New JS modules for better code organization: `element-utils.js`, `table-row-nav.js`, `virtualization-scroll.js`
- IDropdownManagerService interface for improved testability

### Changed
- Table Primitive refactored - removed unused FilteringState and ColumnFilter classes (filtering is now solely DataTable's responsibility)
- OnFilter callback signature changed from `EventCallback<(string?, Dictionary<string, string>)>` to `EventCallback<string?>` (BREAKING CHANGE)
- DataTable default PageSizes updated to include 5, InitialPageSize changed to 5

### Fixed
- **Security**: Replaced all `eval()` calls with proper JS modules across 6 components
- **Security**: Fixed cssText injection vulnerability in `match-trigger-width.js`
- **Security**: Added HtmlSanitizer to MarkdownEditor to prevent XSS attacks
- **Security**: Added input validation to DropdownManagerService, TailwindMerge, KeyboardShortcutService, Slider, and ResizablePanelGroup
- **Security**: Added CSS class pattern validation to reject dangerous patterns
- DataTable selection tracking and reference aliasing bug resolved
- DataTable checkbox double-toggle issue fixed with stopPropagation
- Table focus ring styles added for keyboard navigation visibility
- Table arrow keys no longer scroll page during row navigation
- Bare catch blocks replaced with specific exception types
- Null-forgiving operators replaced with null-coalescing fallbacks

### Performance
- PortalHost optimized with portal key change tracking to avoid unnecessary re-renders
- MultiSelect cached TriggerCssClass and added delegate caching for event handlers
- RichTextEditor added ShouldRender() optimization to prevent unnecessary renders
- RadioGroup cached enabled items list to avoid LINQ allocations
- DataTable optimized search filtering with extracted MatchesSearch() method
- Calendar cached JS module reference and implemented proper IAsyncDisposable
- TailwindMerge added ConcurrentDictionary cache for regex lookups, fixed double regex evaluation
- TableDataExtensions optimized pagination with GetRange() instead of Skip().Take()

---

## 2026-01-26

### Added
- CommandVirtualizedGroup component for efficient rendering of large item lists using Blazor's Virtualize
- EnableLazyLoading parameter for CommandVirtualizedGroup - loads items progressively as user scrolls
- Wrap-around keyboard navigation for virtualized groups (Up at first item jumps to last, Down at last jumps to first)
- ScrollToIndexAsync method for programmatic scrolling to any item in virtualized groups
- Global search feature in demo header with Ctrl+K keyboard shortcut
- OnKeyboardNavigationChanged event for coordinating focus state across components
- OnFocusChanged and OnSearchChanged targeted events for O(1) performance updates
- Calendar keyboard navigation (arrow keys, Home/End, PageUp/PageDown, Enter/Space to select)
- ShowMonthYearDropdowns parameter for Calendar - option to show simple text header instead of dropdowns

### Changed
- Command component now supports both regular CommandItem and virtualized groups
- Keyboard navigation unified across regular items and virtualized groups with proper wrapping
- CommandSearch updated to use lazy loading for all icon groups (Lucide, Heroicons, Feather)

### Fixed
- Command component performance improved from O(n) to O(1) re-renders on focus change
- CommandVirtualizedGroup virtualization fixed - removed overflow-hidden that clipped Virtualize spacer elements
- Hover/keyboard navigation conflict resolved - CSS hover suppressed during keyboard navigation
- CommandItem no longer causes page jump on keyboard navigation (affected Combobox)
- Scroll-into-view only triggers during keyboard navigation, not mouse hover
- Select dropdown scrollbar appearance fixed for Edge browser

---

## 2026-01-25

### Fixed
- Menubar now switches between menus with a single click (previously required double-click due to overlay blocking)

### Added
- DisplayTextSelector parameter for Select component - allows deriving display text from the selected value without waiting for dropdown items to render
- DayClass and CellClass parameters for Calendar component - allows custom styling of day buttons and cells

### Changed
- License changed from MIT to Apache 2.0 for BlazorBlueprint.Components and BlazorBlueprint.Primitives (icon libraries remain MIT)
- Added NOTICE file with attribution requirements for derivative works
- Combobox and MultiSelect focus mechanism replaced with event-based approach using OnContentReady
- MultiSelect search box styling now matches Combobox for visual consistency

### Fixed
- Calendar performance significantly improved through caching and elimination of redundant renders
- Select dropdown now scrolls to and centers the selected item when opened

---

## 2026-01-24

### Added
- KeyboardShortcutService for global keyboard shortcut management
- Functional keyboard shortcuts in DropdownMenu and Menubar demo pages
- KeyboardShortcut utility class for parsing shortcut strings (e.g., "Ctrl+Shift+N")

### Changed
- DropdownMenu demo updated to show functional shortcuts with visual feedback
- Menubar demo updated to show functional shortcuts with visual feedback

---

## 2026-01-23

### Changed
- RichTextEditor rewritten to use Quill.js v2 in headless mode with custom Blazor toolbar
- RichTextEditor now uses Blazor Blueprint components (NativeSelect, Dialog, Toggle, Button) instead of raw HTML elements
- Link insertion uses BlazorBlueprint Dialog instead of browser prompt for consistent UX

### Fixed
- RichTextEditor block format removal now preserves inline formatting (bold, italic, etc.)
- RichTextEditor uses Quill's getSemanticHTML() for normalized cross-browser HTML output
- RichTextEditor callback suppression prevents update loops during programmatic content changes

### Added
- RichTextEditor SetDeltaAsync/GetDeltaAsync methods for native Quill Delta format support
- RichTextEditor toolbar presets (None, Simple, Standard, Full, Custom)

---

## 2026-01-22

### Added
- Checkbox item support for DropdownMenu and Menubar components

### Changed
- DataTable now uses the enhanced Pagination component

### Fixed
- Select component shows display name instead of raw value
- Select component keyboard navigation reliability improved
- ContextMenu repositions correctly on right-click within trigger area

---

## 2026-01-21

### Added
- Calendar month/year selection grids for easier date navigation
- ShowOutsideDays parameter for Calendar component
- Alert redesign with semantic variants (default, destructive, warning, success, info)

### Fixed
- DatePicker nested portal click-outside issues resolved
- DatePicker month/year dropdown no longer closes unexpectedly during navigation

---

## 2026-01-20

### Added
- 26 new components achieving shadcn/ui parity:
  - **High priority**: Alert, AlertDialog, Progress, Spinner, Toast, Calendar, DatePicker, NavigationMenu
  - **Medium priority**: Breadcrumb, Carousel, ContextMenu, Drawer, Menubar, Pagination, ScrollArea, Slider, Toggle, ToggleGroup
  - **Low priority**: AspectRatio, Empty, InputOTP, Kbd, NativeSelect, Resizable, Typography

### Fixed
- Keyboard navigation improvements across components
- Various component bug fixes

---

## 2026-01-19

### Added
- AutoClose parameter for MultiSelect component

### Fixed
- MultiSelect rebuilt using primitives architecture
- MultiSelect click-outside behavior corrected

---

## 2026-01-15

### Changed
- Website moved to separate repository

### Fixed
- Nested portal infinite loop prevention

---

## 2025-12-16

### Fixed
- Portal synchronization improvements
- Scroll jump prevention in portal-based components

---

## 2025-12-10

### Changed
- Comprehensive README documentation updates

---

## 2025-12-08

### Added
- AsChild pattern for trigger and close components

### Fixed
- UI jump in portal-based components

---

## 2025-12-07

### Fixed
- MultiSelect stale callback bug
- Accordion and Collapsible animation issues

---

## 2025-12-06

### Added
- MultiSelect component

### Fixed
- Primitives package reference update

---

## 2025-11-16

### Added
- Icon libraries expansion (Heroicons, Feather, Lucide)
- Website infrastructure
- DevFlow workflow system
- LLM documentation structure

### Changed
- GitHub Actions updates

---

## 2025-11-15

### Added
- Initial release v1.0.0-beta.1

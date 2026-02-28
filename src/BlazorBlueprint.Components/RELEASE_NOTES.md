## What's New in v3.1.0

### New Components

- **BbDataView** — Flexible data presentation component with list/grid layout toggle,
  column definitions, built-in filtering and sorting, pagination, infinite scroll,
  a `ToolbarActions` slot for custom toolbar content, and a `ScrollHeight` parameter
  for fixed-height viewports.
- **BbTagInput** — Inline chip/tag input for managing free-form lists of string values.
  Supports configurable trigger keys, `MaxTags`/`MaxTagLength` limits, duplicate prevention,
  paste support, static and async suggestions with debouncing, three visual variants,
  a `Clearable` button, and `TagTemplate` for custom rendering.

### New Features

- **Component animations** — Smooth enter/exit animations for 17 overlay and interactive
  components via tw-animate-css, driven by `data-state` attribute toggling. Includes
  `bb-no-animate` global CSS class and `prefers-reduced-motion` support.
- **BbAlert** — New `AutoDismissAfter`, `PauseOnHover`, `ShowCountdown`, and `Actions`
  parameters for timed dismissal with a visual countdown bar and inline action buttons.
- **BbToast / BbToastProvider** — New `ShowCountdown` parameter adds a visual countdown
  progress bar to toasts. Individual toasts can override via `ToastData.ShowCountdown`.
- **BbCombobox** — New `SearchQuery` and `SearchQueryChanged` parameters expose the
  current search string for external async filtering.
- **BbInputOTP** — Clipboard paste now fills OTP slots automatically.
- **ForceMount** — New parameter on `BbPopoverContent`, `BbDropdownMenuContent`, and
  `BbCollapsibleContent` to keep content mounted in the DOM when closed, enabling CSS
  exit animations.

### Bug Fixes

- Fixed keyboard navigation in `BbDropdownMenu`, `BbMenubar`, and `BbNavigationMenuLink`
  incorrectly highlighting the item container instead of the focused element when using
  `Href` links.
- Fixed `BbAccordion` spacebar double-toggle and added arrow key navigation.
- Fixed `BbButtonGroup` styling broken by a tooltip wrapper div.
- Fixed `BbTimelineTitle` styling; added `As` parameter to heading components.
- Fixed `BbInputField` and `BbInputGroupInput` throwing `InvalidStateError` on file input
  selection due to incorrect value binding.
- Fixed `BbTooltipTrigger` and `BbHoverCardTrigger` losing click and ARIA behavior when
  nested inside `BbDialogTrigger`.

### Improvements

- `BbAlert` and `BbToastProvider` auto-dismiss refactored from polling to
  `Task.Delay` + `TaskCompletionSource` for exact dismiss timing.
- Migrated from `tailwind.config.js` to Tailwind v4 CSS-first configuration.

# Changelog

All notable changes to BlazorUI are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

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
- RichTextEditor now uses BlazorUI components (NativeSelect, Dialog, Toggle, Button) instead of raw HTML elements
- Link insertion uses BlazorUI Dialog instead of browser prompt for consistent UX

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

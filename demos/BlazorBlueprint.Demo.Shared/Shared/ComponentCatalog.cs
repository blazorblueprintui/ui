namespace BlazorBlueprint.Demo.Shared;

/// <summary>Component inventory plus grouped, deduplicated demo-page navigation.</summary>
internal static class ComponentCatalog
{
    internal sealed record Entry(
        string Name,
        string Url,
        string Description,
        string Icon,
        bool IsNew = false,
        string? Component = null);

    internal static IReadOnlyList<Entry> Items { get; } =
        new Entry[]
        {
            new("Accordion", "/components/accordion", "Collapsible content sections with smooth animations", "layers"),
            new("Alert", "/components/alert", "Callout for important messages that attract user attention", "circle-alert"),
            new("Alert Dialog", "/components/alert-dialog", "Modal dialog requiring user acknowledgement", "triangle-alert"),
            new("App Bar", "/components/app-bar", "Mobile title, back navigation, actions and safe-area padding.", "panel-top", IsNew: true, Component: "BbAppBar"),
            new("Aspect Ratio", "/components/aspect-ratio", "Maintains consistent width/height ratio for content", "ratio"),
            new("Attachment", "/components/attachment", "Displays files or images with metadata, upload states, actions, and optional trigger overlays.", "component"),
            new("Avatar", "/components/avatar", "User profile images with fallback initials and icons", "circle-user"),
            new("Badge", "/components/badge", "Labels for status, categories, and metadata", "badge"),
            new("Badge Icon", "/components/badge#badge-icon", "Decorative icons inside badges.", "component", IsNew: true, Component: "BbBadgeIcon"),
            new("Bottom Navigation", "/components/bottom-nav", "Mobile navigation with active state, links and safe-area padding.", "panel-bottom", IsNew: true, Component: "BbBottomNav"),
            new("Bottom Navigation Item", "/components/bottom-nav#bottom-nav-item", "A mobile navigation link or action with active state.", "component", IsNew: true, Component: "BbBottomNavItem"),
            new("Breadcrumb", "/components/breadcrumb", "Navigation trail showing hierarchical location", "chevrons-right"),
            new("Bubble", "/components/bubble", "Displays conversational content surfaces with variants, alignment, and reactions.", "component"),
            new("Button", "/components/button", "Interactive buttons with multiple variants and sizes", "mouse-pointer-click"),
            new("Button Group", "/components/button-group", "Visually group related buttons with connected styling", "group"),
            new("Calendar", "/components/calendar", "Date selection with month view and navigation", "calendar"),
            new("Card", "/components/card", "Container for grouped content with header and footer", "square"),
            new("Carousel", "/components/carousel", "Slideshow component for cycling through content", "gallery-horizontal"),
            new("Cascader", "/components/cascader", "Select nested options through columns with full-path search and keyboard navigation", "git-branch", IsNew: true),
            new("Checkbox", "/components/checkbox", "Binary selection control with indeterminate state", "square-check"),
            new("Checkbox Group", "/components/checkbox-group", "Grouped checkboxes with shared state and select-all support", "component"),
            new("Collapsible", "/components/collapsible", "Expandable content area with trigger control", "chevrons-up-down"),
            new("Color Picker", "/components/color-picker", "Visual color selection with hex/RGB input and presets", "component"),
            new("Combobox", "/components/combobox", "Autocomplete input with searchable dropdown", "text-cursor-input"),
            new("Command", "/components/command", "Command palette for quick actions and navigation", "command"),
            new("Context Menu", "/components/context-menu", "Right-click menu with customizable items", "menu"),
            new("Context Menu Checkbox Item", "/components/context-menu#context-menu-checkbox-item", "A checked, unchecked or indeterminate menu item.", "component", IsNew: true, Component: "BbContextMenuCheckboxItem"),
            new("Context Menu Radio Group", "/components/context-menu#context-menu-radio-group", "Coordinates mutually exclusive menu choices.", "component", IsNew: true, Component: "BbContextMenuRadioGroup"),
            new("Context Menu Radio Item", "/components/context-menu#context-menu-radio-item", "A selectable option in a menu radio group.", "component", IsNew: true, Component: "BbContextMenuRadioItem"),
            new("Context Menu Submenu", "/components/context-menu#context-menu-submenu", "Coordinates a nested menu and its open state.", "component", IsNew: true, Component: "BbContextMenuSub"),
            new("Context Menu Submenu Content", "/components/context-menu#context-menu-submenu-content", "The positioned panel for nested menu items.", "component", IsNew: true, Component: "BbContextMenuSubContent"),
            new("Context Menu Submenu Trigger", "/components/context-menu#context-menu-submenu-trigger", "Opens a nested menu using pointer or keyboard.", "component", IsNew: true, Component: "BbContextMenuSubTrigger"),
            new("Copy Text", "/components/copy-text", "Inline text that copies text to the clipboard when clicked", "component"),
            new("Currency Input", "/components/currency-input", "Locale-aware currency input with 40+ currencies", "component"),
            new("Dashboard Grid", "/components/dashboard-grid", "A drag-and-drop, resizable widget layout for composing dashboards. Built on CSS Grid with responsive breakpoints, state persistence, and full keyboard accessibility.", "component"),
            new("Data Grid", "/components/datagrid", "Enterprise data grid with sorting, filtering, cell and batch editing, and row expansion", "grid-3x3"),
            new("Data Grid Editing", "/components/datagrid-editing", "Cell and batch editing with validation, input templates and sorting after saves", "table-2"),
            new("Data Grid Hierarchy", "/components/datagrid-hierarchy", "Display hierarchical tree data within the DataGrid using a flat-row rendering approach, with expand/collapse, depth-based indentation, per-level sorting, and child pagination.", "component"),
            new("Data Grid Styling", "/components/datagrid-styling", "Customize cell content, headers, row appearance, and grid styling with Tailwind CSS utilities.", "component"),
            new("Data Table", "/components/datatable", "Powerful tables with sorting, filtering, pagination, and selection", "table-2"),
            new("Data View", "/components/dataview", "List and grid layouts with sorting, filtering, pagination, and infinite scroll", "layout-list"),
            new("Date Input", "/components/date-input", "Edit day, month and four-digit year separately, in your culture’s date order.", "calendar-clock", IsNew: true, Component: "BbDateInput"),
            new("Date Picker", "/components/date-picker", "Date selection with calendar popover", "calendar-days"),
            new("Date Range Picker", "/components/date-range-picker", "Select date ranges with two-month calendar view", "component"),
            new("Date Time Picker", "/components/date-time-picker", "Combined date and time selection with calendar popover", "calendar-clock"),
            new("Dialog", "/components/dialog", "Modal dialogs with backdrop and focus management", "app-window"),
            new("Dock", "/components/dock", "IDE-style docking workspace with draggable tabs, splits, and floating windows", "component"),
            new("Drawer", "/components/drawer", "Mobile-friendly panel sliding from screen edge", "panel-bottom"),
            new("Dropdown Menu", "/components/dropdown-menu", "Context menus with items, separators, and shortcuts", "chevron-down"),
            new("Dropdown Menu Radio Group", "/components/dropdown-menu#dropdown-menu-radio-group", "Coordinates mutually exclusive menu choices.", "component", IsNew: true, Component: "BbDropdownMenuRadioGroup"),
            new("Dropdown Menu Radio Item", "/components/dropdown-menu#dropdown-menu-radio-item", "A selectable option in a menu radio group.", "component", IsNew: true, Component: "BbDropdownMenuRadioItem"),
            new("Dropdown Menu Submenu", "/components/dropdown-menu#dropdown-menu-submenu", "Coordinates a nested menu and its open state.", "component", IsNew: true, Component: "BbDropdownMenuSub"),
            new("Dropdown Menu Submenu Content", "/components/dropdown-menu#dropdown-menu-submenu-content", "The positioned panel for nested menu items.", "component", IsNew: true, Component: "BbDropdownMenuSubContent"),
            new("Dropdown Menu Submenu Trigger", "/components/dropdown-menu#dropdown-menu-submenu-trigger", "Opens a nested menu using pointer or keyboard.", "component", IsNew: true, Component: "BbDropdownMenuSubTrigger"),
            new("Dynamic Form", "/components/dynamic-form", "Schema-driven form rendering from JSON or code definitions", "component"),
            new("Empty", "/components/empty", "Empty state placeholder with icon and message", "inbox"),
            new("Event Calendar", "/components/event-calendar", "Month, week, and agenda views for your own event type", "calendar-days"),
            new("Field", "/components/field", "Combine labels, controls, and help text for accessible forms", "pencil-line"),
            new("File Upload", "/components/file-upload", "Drag-and-drop uploads with previews, progress, cancellation and retry", "component"),
            new("Filter Builder", "/components/filterbuilder", "Visual query builder for data filter expressions with AND/OR logic and nested groups", "filter"),
            new("Form Field Checkbox", "/components/form-field-checkbox", "Checkbox with built-in label, helper text, and error messages", "component"),
            new("Form Field Checkbox Group", "/components/form-field-checkbox-group", "Checkbox group with built-in label, helper text, and error messages", "component"),
            new("Form Field Combobox", "/components/form-field-combobox", "Searchable combobox with built-in label, helper text, and error messages", "component"),
            new("Form Field Currency Input", "/components/form-field-currency-input", "Currency input with built-in label, helper text, and error messages", "component"),
            new("Form Field Date Picker", "/components/form-field-date-picker", "Date picker with built-in label, helper text, and error messages", "component"),
            new("Form Field Date Range Picker", "/components/form-field-date-range-picker", "Date range picker with built-in label, helper text, and error messages", "component"),
            new("Form Field Date Time Picker", "/components/form-field-date-time-picker", "Date time picker with built-in label, helper text, and error messages", "component"),
            new("Form Field File Upload", "/components/form-field-file-upload", "File upload with built-in label, helper text, and error messages", "component"),
            new("Form Field Input", "/components/form-field-input", "Typed text input with built-in label, validation, and error messages", "component"),
            new("Form Field Input OTP", "/components/form-field-input-otp", "OTP verification input with built-in label, helper text, and error messages", "component"),
            new("Form Field Masked Input", "/components/form-field-masked-input", "Masked input (phone, SSN, etc.) with built-in label, helper text, and error messages", "component"),
            new("Form Field Multi Select", "/components/form-field-multi-select", "Multi-select with built-in label, helper text, and error messages", "component"),
            new("Form Field Native Select", "/components/form-field-native-select", "Native select with built-in label, helper text, and error messages", "component"),
            new("Form Field Numeric Input", "/components/form-field-numeric-input", "Numeric input with built-in label, helper text, and error messages", "component"),
            new("Form Field Radio Group", "/components/form-field-radio-group", "Radio group with built-in label, helper text, and error messages", "component"),
            new("Form Field Select", "/components/form-field-select", "Select dropdown with built-in label, helper text, and error messages", "component"),
            new("Form Field Switch", "/components/form-field-switch", "Toggle switch with built-in label, helper text, and error messages", "component"),
            new("Form Field Tag Input", "/components/form-field-tag-input", "Tag input with built-in label, helper text, and error messages", "component"),
            new("Form Field Textarea", "/components/form-field-textarea", "Textarea with built-in label, helper text, and error messages", "component"),
            new("Form Field Time Picker", "/components/form-field-time-picker", "Time picker with built-in label, helper text, and error messages", "component"),
            new("Form Wizard", "/components/form-wizard", "Multi-step form with progress tracking and validation", "component"),
            new("Height Animation", "/components/height-animation", "Animate expansion and dynamic content resizing.", "move-vertical", IsNew: true, Component: "BbHeightAnimation"),
            new("Hover Card", "/components/hovercard", "Rich preview cards on hover with delay control", "square-mouse-pointer"),
            new("Input", "/components/input", "Text input fields with multiple types and sizes", "text-cursor"),
            new("Input Field", "/components/input-field", "Typed input with automatic type conversion and validation", "component"),
            new("Input Group", "/components/input-group", "Enhanced inputs with icons, buttons, and addons", "layout-list"),
            new("Input OTP", "/components/input-otp", "One-time password input with individual digit fields", "key-round"),
            new("Item", "/components/item", "Flexible list items with media, content, and actions", "list"),
            new("Kbd", "/components/kbd", "Display keyboard shortcuts and key combinations", "keyboard"),
            new("Label", "/components/label", "Accessible labels for form controls", "tag"),
            new("Markdown Editor", "/components/markdown-editor", "Rich text editor with toolbar formatting and live preview", "file-text"),
            new("Marker", "/components/marker", "Displays inline status notes, bordered rows, and labeled separators in conversations.", "component"),
            new("Masked Input", "/components/masked-input", "Input with structured formats like phone, SSN, and credit card", "component"),
            new("Menubar", "/components/menubar", "Desktop application-style menu bar", "square-menu"),
            new("Menubar Radio Group", "/components/menubar#menubar-radio-group", "Coordinates mutually exclusive menu choices.", "component", IsNew: true, Component: "BbMenubarRadioGroup"),
            new("Menubar Radio Item", "/components/menubar#menubar-radio-item", "A selectable option in a menu radio group.", "component", IsNew: true, Component: "BbMenubarRadioItem"),
            new("Menubar Submenu", "/components/menubar#menubar-submenu", "Coordinates a nested menu and its open state.", "component", IsNew: true, Component: "BbMenubarSub"),
            new("Menubar Submenu Content", "/components/menubar#menubar-submenu-content", "The positioned panel for nested menu items.", "component", IsNew: true, Component: "BbMenubarSubContent"),
            new("Menubar Submenu Trigger", "/components/menubar#menubar-submenu-trigger", "Opens a nested menu using pointer or keyboard.", "component", IsNew: true, Component: "BbMenubarSubTrigger"),
            new("Message", "/components/message", "Lays out avatar, content, header, and footer for conversational message rows.", "component"),
            new("Motion", "/components/motion", "Animation presets, triggers and reduced-motion support.", "wand-sparkles", IsNew: true, Component: "BbMotion"),
            new("Multi Select", "/components/multiselect", "Searchable multi-selection with tags and checkboxes", "list-checks"),
            new("Native Select", "/components/native-select", "Browser native select with consistent styling", "square-chevron-down"),
            new("Navigation Menu", "/components/navigation-menu", "Site navigation with dropdown support", "navigation"),
            new("Notification Badge", "/components/notification-badge", "Count and dot overlays with accessible overflow counts.", "bell", IsNew: true, Component: "BbNotificationBadge"),
            new("Numeric Input", "/components/numeric-input", "Numeric input with increment/decrement and validation", "component"),
            new("Page Transition", "/components/page-transition", "Incoming-page transitions on navigation.", "route", IsNew: true, Component: "BbPageTransition"),
            new("Pagination", "/components/pagination", "Page navigation with previous/next controls", "arrow-left-right"),
            new("Popover", "/components/popover", "Floating panels for additional content and actions", "message-square"),
            new("Progress", "/components/progress", "Progress indicator for task completion", "loader"),
            new("Quantity Stepper", "/components/quantity-stepper", "Quantity controls with bounds, validation and remove-at-minimum actions.", "minus", IsNew: true, Component: "BbQuantityStepper"),
            new("Radio Group", "/components/radio-group", "Mutually exclusive options with keyboard navigation", "circle-dot"),
            new("Range Slider", "/components/range-slider", "Dual-handle slider for selecting value ranges", "component"),
            new("Rating", "/components/rating", "Star rating component with half-value and custom icons", "component"),
            new("Render State Provider", "/components/render-state-provider", "Share prerender and interactive rendering state.", "monitor", IsNew: true, Component: "BbRenderStateProvider"),
            new("Resizable", "/components/resizable", "Resizable panels with drag handles for adjustable layouts", "move"),
            new("Responsive Nav", "/components/responsive-nav", "Navigation bar that adapts to screen size with overflow menu", "component"),
            new("Rich Text Editor", "/components/rich-text-editor", "WYSIWYG editor with formatting toolbar and HTML output", "file-type"),
            new("Scheduler", "/components/scheduler", "Time-slot scheduling with event editing, recurrence, resources and time zones", "calendar-clock", IsNew: true),
            new("Screen Transition", "/components/screen-transition", "Animate screen changes using an application-owned key.", "panels-top-left", IsNew: true, Component: "BbScreenTransition"),
            new("Scroll Area", "/components/scroll-area", "Custom scrollable area with styled scrollbar", "scroll"),
            new("Section Header", "/components/section-header", "Section heading, supporting text and trailing actions.", "heading", IsNew: true, Component: "BbSectionHeader"),
            new("Select", "/components/select", "Dropdown selection with groups and labels", "list"),
            new("Selection Indicator", "/components/selection-indicator", "Animated active, hover and focus feedback.", "square-dashed", IsNew: true, Component: "BbSelectionIndicator"),
            new("Separator", "/components/separator", "Visual dividers for content sections", "minus"),
            new("Sheet", "/components/sheet", "Side panels that slide in from viewport edges", "panel-right"),
            new("Sidebar", "/components/sidebar", "Responsive navigation sidebar with collapsible menus", "panel-left"),
            new("Sidebar Pill Inset", "/components/sidebar#sidebar-pill-inset", "Content spacing that follows pill collapse state.", "component", IsNew: true, Component: "BbSidebarPillInset"),
            new("Sidebar Pill Navigation", "/components/sidebar#sidebar-pill-nav", "Compact navigation when the sidebar collapses.", "component", IsNew: true, Component: "BbSidebarPillNav"),
            new("Sidebar Pill Navigation Item", "/components/sidebar#sidebar-pill-nav-item", "An active link or action inside pill navigation.", "component", IsNew: true, Component: "BbSidebarPillNavItem"),
            new("Sidebar Selection Indicator", "/components/sidebar#sidebar-selection-indicator", "Animated active-item feedback for sidebar navigation.", "component", IsNew: true, Component: "BbSidebarSelectionIndicator"),
            new("Skeleton", "/components/skeleton", "Loading placeholders for content and images", "box"),
            new("Slider", "/components/slider", "Range input for selecting numeric values", "sliders-horizontal"),
            new("Sortable", "/components/sortable", "Drag-and-drop sortable lists, grids, and Kanban boards", "component"),
            new("Sortable Handle", "/components/sortable#sortable-handle", "A focusable handle for pointer and keyboard sorting.", "component", IsNew: true, Component: "BbSortableHandle"),
            new("Spinner", "/components/spinner", "Loading indicator for async operations", "loader"),
            new("Split Button", "/components/split-button", "Primary action button with a dropdown for secondary actions", "chevrons-down"),
            new("Switch", "/components/switch", "Toggle control for on/off states", "toggle-left"),
            new("Tabs", "/components/tabs", "Tabbed interface for organizing related content", "folder"),
            new("Tag Input", "/components/tag-input", "Inline chip/tag input for managing free-form text lists", "component"),
            new("Textarea", "/components/textarea", "Multi-line text input with automatic content sizing", "align-left"),
            new("Theme", "/components/theme", "Persistent presets, density, typography, surfaces, menus and scoped appearance.", "component"),
            new("Theme Scope", "/components/theme#theme-scope", "Local typography, density, surface and menu settings.", "component", IsNew: true, Component: "BbThemeScope"),
            new("Time Input", "/components/time-input", "Edit hours, minutes and optional seconds with culture-aware 12/24-hour display.", "calendar-clock", IsNew: true, Component: "BbTimeInput"),
            new("Time Picker", "/components/time-picker", "Time selection with 12/24-hour format and minute stepping", "component"),
            new("Timeline", "/components/timeline", "Chronological display of events with icons and connectors", "git-commit-horizontal"),
            new("Toast", "/components/toast", "Temporary notifications for user feedback", "bell"),
            new("Toggle", "/components/toggle", "Two-state button for toggleable options", "toggle-right"),
            new("Toggle Group", "/components/toggle-group", "Group of toggles with single or multiple selection", "layout-grid"),
            new("Tooltip", "/components/tooltip", "Brief informational popups on hover or focus", "info"),
            new("Tree View", "/components/tree-view", "Hierarchical data display with expand/collapse, selection, and keyboard navigation", "list-tree"),
            new("TreeSelect", "/components/tree-select", "Searchable single and multiple tree selection with cascading checkboxes", "list-tree", IsNew: true),
            new("Typography", "/components/typography", "Text styling utilities for headings, paragraphs, and more", "type"),
        }
        .OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    internal sealed record NavigationGroup(Entry Entry, IReadOnlyList<Entry> Children);

    internal static IReadOnlyList<NavigationGroup> NavigationGroups { get; } = BuildNavigationGroups();

    // Page navigation is shared by the sidebar, component homepage and command search.
    internal static IReadOnlyList<Entry> DemoPages { get; } = NavigationGroups
        .SelectMany(group => new[] { group.Entry }.Concat(group.Children)).ToArray();

    internal static string NavigationLabel(Entry parent, Entry child)
    {
        if (child.Url == "/components/datagrid-editing")
        {
            return "Cell & Batch Editing";
        }
        var prefix = parent.Url == "/components/field" ? "Form Field " : parent.Name + " ";
        return child.Name.StartsWith(prefix, StringComparison.Ordinal)
            ? child.Name[prefix.Length..] : child.Name;
    }

    private static NavigationGroup[] BuildNavigationGroups()
    {
        // Composition helpers are documented within their owning component page.
        var pages = Items.Where(entry => !entry.Url.Contains('#')).ToArray();
        var families = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["button"] = ["button-group", "split-button"],
            ["checkbox"] = ["checkbox-group"],
            ["datagrid"] = ["datagrid-editing", "datagrid-hierarchy", "datagrid-styling"],
            ["date-picker"] = ["date-input", "date-range-picker", "date-time-picker"],
            ["field"] = pages.Where(entry => entry.Url.StartsWith("/components/form-field-", StringComparison.Ordinal))
                .Select(entry => entry.Url["/components/".Length..]).ToArray(),
            ["input"] = ["currency-input", "input-field", "input-group", "input-otp", "masked-input", "numeric-input", "tag-input", "textarea"],
            ["motion"] = ["height-animation", "selection-indicator", "page-transition", "screen-transition", "render-state-provider"],
            ["select"] = ["multiselect", "native-select"],
            ["slider"] = ["range-slider"],
            ["time-picker"] = ["time-input"],
            ["toggle"] = ["toggle-group"],
        };
        var childUrls = families.Values.SelectMany(routes => routes).Select(route => "/components/" + route)
            .ToHashSet(StringComparer.Ordinal);
        return pages.Where(entry => !childUrls.Contains(entry.Url))
            .Select(entry => new NavigationGroup(entry,
                families.TryGetValue(entry.Url["/components/".Length..], out var routes)
                    ? pages.Where(child => routes.Contains(child.Url["/components/".Length..], StringComparer.Ordinal))
                        .OrderBy(child => NavigationLabel(entry, child), StringComparer.OrdinalIgnoreCase).ToArray()
                    : []))
            .OrderBy(group => group.Entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}

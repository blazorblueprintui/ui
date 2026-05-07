namespace BlazorBlueprint.Components;

/// <summary>
/// Default <see cref="IBbLocalizer"/> implementation with English defaults for all BlazorBlueprint component strings.
/// </summary>
/// <remarks>
/// <para>
/// This class is registered automatically by <c>AddBlazorBlueprintComponents()</c>.
/// Override individual keys at startup using <see cref="Set"/>, or subclass this to integrate
/// with <c>IStringLocalizer</c> for dynamic culture switching.
/// </para>
/// <para>
/// Keys use dot notation: <c>ComponentName.PropertyName</c> (e.g., <c>"DataGrid.Loading"</c>).
/// Format strings use standard <see cref="string.Format(string, object[])"/> placeholders
/// (e.g., <c>"Showing {0}–{1} of {2}"</c>).
/// </para>
/// </remarks>
public class DefaultBbLocalizer : IBbLocalizer
{
    private readonly Dictionary<string, string> defaults = new(StringComparer.Ordinal)
    {
        // Alert
        ["Alert.Dismiss"] = "Dismiss",

        // Breadcrumb
        ["Breadcrumb.Breadcrumb"] = "breadcrumb",
        ["Breadcrumb.More"] = "More",

        // Calendar
        ["Calendar.GoToPreviousMonth"] = "Go to previous month",
        ["Calendar.GoToNextMonth"] = "Go to next month",

        // Carousel
        ["Carousel.NextSlide"] = "Next slide",
        ["Carousel.PreviousSlide"] = "Previous slide",

        // Combobox
        ["Combobox.EmptyMessage"] = "No results found.",
        ["Combobox.Placeholder"] = "Select an option...",
        ["Combobox.SearchPlaceholder"] = "Search...",

        // Command
        ["Command.CommandMenu"] = "Command menu",
        ["Command.CommandList"] = "Command list",

        // DashboardGrid
        ["DashboardGrid.Loading"] = "Loading dashboard",
        ["DashboardGrid.NoWidgets"] = "No widgets to display",
        ["DashboardGrid.NoWidgetsDescription"] = "Get started by adding your first widget.",
        ["DashboardGrid.AddWidget"] = "Add Widget",
        ["DashboardGrid.RemoveWidget"] = "Remove widget",
        ["DashboardGrid.ResizeWidget"] = "Resize widget",

        // DataGrid
        ["DataGrid.Loading"] = "Loading...",
        ["DataGrid.NoResultsFound"] = "No results found",
        ["DataGrid.NoResultsFilterDescription"] = "Try adjusting or clearing your filters.",
        ["DataGrid.PreviousPage"] = "Previous page",
        ["DataGrid.NextPage"] = "Next page",
        ["DataGrid.ExpandAll"] = "Expand all",
        ["DataGrid.CollapseAll"] = "Collapse all",
        ["DataGrid.SelectAllOnPage"] = "Select all on this page ({0} items)",
        ["DataGrid.SelectAllItems"] = "Select all {0} items",
        ["DataGrid.ClearSelection"] = "Clear selection",
        ["DataGrid.SelectRowsAriaLabel"] = "Select rows - click to see options",
        ["DataGrid.SelectAllRows"] = "Select all rows",
        ["DataGrid.SelectThisRow"] = "Select this row",
        ["DataGrid.ExpandRow"] = "Expand row",
        ["DataGrid.CollapseRow"] = "Collapse row",
        ["DataGrid.Expand"] = "Expand",
        ["DataGrid.Collapse"] = "Collapse",
        ["DataGrid.ExpandGroup"] = "Expand group",
        ["DataGrid.CollapseGroup"] = "Collapse group",
        ["DataGrid.FilterPlaceholder"] = "Filter {0}",
        ["DataGrid.PinnedColumnTooltip"] = "This column is pinned and cannot be moved",
        ["DataGrid.ActiveFilters"] = "{0} active filter(s)",
        ["DataGrid.ClearAll"] = "Clear all",
        ["DataGrid.ShowingRange"] = "Showing {0}\u2013{1} of {2}",
        ["DataGrid.RowsSelected"] = "{0} of {1} row(s) selected",
        ["DataGrid.GroupItemCount"] = "({0} items)",
        ["DataGrid.CountLabel"] = "Count",
        ["DataGrid.SumLabel"] = "Sum",
        ["DataGrid.AverageLabel"] = "Avg",
        ["DataGrid.MinLabel"] = "Min",
        ["DataGrid.MaxLabel"] = "Max",
        ["DataGrid.FilterColumnEnterValue"] = "Enter value...",
        ["DataGrid.FilterColumnMin"] = "Min",
        ["DataGrid.FilterColumnAnd"] = "and",
        ["DataGrid.FilterColumnMax"] = "Max",
        ["DataGrid.FilterColumnAmount"] = "Amount",
        ["DataGrid.FilterColumnPickDate"] = "Pick a date",
        ["DataGrid.FilterColumnSelectValues"] = "Select values...",
        ["DataGrid.FilterColumnSelectValue"] = "Select value...",
        ["DataGrid.FilterColumnClear"] = "Clear",
        ["DataGrid.FilterColumnApply"] = "Apply",
        ["DataGrid.SearchPlaceholder"] = "Search...",

        // DataTable
        ["DataTable.Loading"] = "Loading...",
        ["DataTable.NoResultsFound"] = "No results found",
        ["DataTable.SelectRowsAriaLabel"] = "Select rows - click to see options",
        ["DataTable.SelectAllOnPage"] = "Select all on this page ({0} items)",
        ["DataTable.SelectAllItems"] = "Select all {0} items",
        ["DataTable.ClearSelection"] = "Clear selection",
        ["DataTable.SelectAllRows"] = "Select all rows",
        ["DataTable.SelectThisRow"] = "Select this row",
        ["DataTable.Search"] = "Search...",
        ["DataTable.Columns"] = "Columns",
        ["DataTable.ToggleColumns"] = "Toggle columns",

        // DataView
        ["DataView.SearchPlaceholder"] = "Search...",
        ["DataView.NoResultsFound"] = "No results found",
        ["DataView.Loading"] = "Loading...",
        ["DataView.LoadingMore"] = "Loading more...",
        ["DataView.LoadMore"] = "Load more",
        ["DataView.ListView"] = "List view",
        ["DataView.GridView"] = "Grid view",
        ["DataView.Sort"] = "Sort",

        // DatePicker
        ["DatePicker.Placeholder"] = "Pick a date",

        // DateRangePicker
        ["DateRangePicker.Placeholder"] = "Select date range",
        ["DateRangePicker.QuickSelect"] = "Quick Select",
        ["DateRangePicker.SelectEndDate"] = "Select end date",
        ["DateRangePicker.DaysSelected"] = "{0} day(s) selected",
        ["DateRangePicker.Clear"] = "Clear",
        ["DateRangePicker.Apply"] = "Apply",
        ["DateRangePicker.Today"] = "Today",
        ["DateRangePicker.Yesterday"] = "Yesterday",
        ["DateRangePicker.Last7Days"] = "Last 7 days",
        ["DateRangePicker.Last30Days"] = "Last 30 days",
        ["DateRangePicker.ThisMonth"] = "This month",
        ["DateRangePicker.LastMonth"] = "Last month",
        ["DateRangePicker.ThisYear"] = "This year",
        ["DateRangePicker.Custom"] = "Custom",

        // Dialog
        ["Dialog.Close"] = "Close",

        // FilterBuilder
        ["FilterBuilder.FilterBuilderAriaLabel"] = "Filter builder",
        ["FilterBuilder.SelectField"] = "Select field...",
        ["FilterBuilder.RemoveCondition"] = "Remove condition",
        ["FilterBuilder.RemoveGroup"] = "Remove group",
        ["FilterBuilder.AddCondition"] = "Add condition",
        ["FilterBuilder.AddGroup"] = "Add group",
        ["FilterBuilder.FilterCondition"] = "Filter condition",
        ["FilterBuilder.EnterValue"] = "Enter value...",
        ["FilterBuilder.Min"] = "Min",
        ["FilterBuilder.And"] = "and",
        ["FilterBuilder.Max"] = "Max",
        ["FilterBuilder.Amount"] = "Amount",
        ["FilterBuilder.PickDate"] = "Pick a date",
        ["FilterBuilder.SelectValues"] = "Select values...",
        ["FilterBuilder.SelectValue"] = "Select value...",
        ["FilterBuilder.Today"] = "today",
        ["FilterBuilder.Yesterday"] = "yesterday",
        ["FilterBuilder.Tomorrow"] = "tomorrow",
        ["FilterBuilder.ThisWeek"] = "this week",
        ["FilterBuilder.LastWeek"] = "last week",
        ["FilterBuilder.NextWeek"] = "next week",
        ["FilterBuilder.ThisMonth"] = "this month",
        ["FilterBuilder.LastMonth"] = "last month",
        ["FilterBuilder.NextMonth"] = "next month",
        ["FilterBuilder.ThisQuarter"] = "this quarter",
        ["FilterBuilder.LastQuarter"] = "last quarter",
        ["FilterBuilder.ThisYear"] = "this year",
        ["FilterBuilder.LastYear"] = "last year",
        ["FilterBuilder.Days"] = "days",
        ["FilterBuilder.Weeks"] = "weeks",
        ["FilterBuilder.Months"] = "months",
        ["FilterBuilder.Hours"] = "hours",
        ["FilterBuilder.Minutes"] = "minutes",
        ["FilterBuilder.Seconds"] = "seconds",

        // FormWizard
        ["FormWizard.WizardProgress"] = "Wizard progress",
        ["FormWizard.Back"] = "Back",
        ["FormWizard.Next"] = "Next",
        ["FormWizard.Skip"] = "Skip",
        ["FormWizard.Complete"] = "Complete",

        // MarkdownEditor
        ["MarkdownEditor.SelectHeadingLevel"] = "Select heading level",
        ["MarkdownEditor.Bold"] = "Bold (Ctrl+B)",
        ["MarkdownEditor.Italic"] = "Italic (Ctrl+I)",
        ["MarkdownEditor.Underline"] = "Underline (Ctrl+U)",
        ["MarkdownEditor.BulletList"] = "Bullet list",
        ["MarkdownEditor.NumberedList"] = "Numbered list",

        // MultiSelect
        ["MultiSelect.EmptyMessage"] = "No results found.",
        ["MultiSelect.Placeholder"] = "Select items...",
        ["MultiSelect.SearchPlaceholder"] = "Search...",
        ["MultiSelect.SelectAll"] = "Select All",
        ["MultiSelect.Clear"] = "Clear",
        ["MultiSelect.Close"] = "Close",

        // NumericInput
        ["NumericInput.IncreaseValue"] = "Increase value",
        ["NumericInput.DecreaseValue"] = "Decrease value",

        // Pagination
        ["Pagination.Pagination"] = "Pagination",
        ["Pagination.Previous"] = "Previous",
        ["Pagination.Next"] = "Next",
        ["Pagination.MorePages"] = "More pages",
        ["Pagination.GoToFirstPage"] = "Go to first page",
        ["Pagination.GoToLastPage"] = "Go to last page",
        ["Pagination.RowsPerPage"] = "Rows per page",
        ["Pagination.ShowingFormat"] = "Showing {0}-{1} of {2}",
        ["Pagination.PageFormat"] = "Page {0} of {1}",
        ["Pagination.NoItems"] = "No items",

        // Rating
        ["Rating.Rating"] = "Rating",

        // ResponsiveNav
        ["ResponsiveNav.ToggleMenu"] = "Toggle Menu",

        // RichTextEditor
        ["RichTextEditor.Normal"] = "Normal",
        ["RichTextEditor.Heading1"] = "Heading 1",
        ["RichTextEditor.Heading2"] = "Heading 2",
        ["RichTextEditor.Heading3"] = "Heading 3",
        ["RichTextEditor.Bold"] = "Bold (Ctrl+B)",
        ["RichTextEditor.Italic"] = "Italic (Ctrl+I)",
        ["RichTextEditor.Underline"] = "Underline (Ctrl+U)",
        ["RichTextEditor.Strikethrough"] = "Strikethrough",
        ["RichTextEditor.BulletList"] = "Bullet List",
        ["RichTextEditor.NumberedList"] = "Numbered List",
        ["RichTextEditor.InsertLink"] = "Insert Link",
        ["RichTextEditor.Blockquote"] = "Blockquote",
        ["RichTextEditor.CodeBlock"] = "Code Block",
        ["RichTextEditor.EditLink"] = "Edit Link",
        ["RichTextEditor.InsertLinkTitle"] = "Insert Link",
        ["RichTextEditor.EditLinkDescription"] = "Update the URL or remove the link.",
        ["RichTextEditor.InsertLinkDescription"] = "Enter the URL for the selected text.",
        ["RichTextEditor.RemoveLink"] = "Remove Link",
        ["RichTextEditor.Cancel"] = "Cancel",
        ["RichTextEditor.Update"] = "Update",
        ["RichTextEditor.Insert"] = "Insert",

        // Sheet
        ["Sheet.Close"] = "Close",

        // Sidebar
        ["Sidebar.ToggleSidebar"] = "Toggle Sidebar",

        // TagInput
        ["TagInput.Placeholder"] = "Add tag...",
        ["TagInput.RemoveTag"] = "Remove {0}",
        ["TagInput.ClearAllTags"] = "Clear all tags",
        ["TagInput.TagSuggestions"] = "Tag suggestions",

        // Theme
        ["Theme.Switcher.Label"] = "Customize theme",
        ["Theme.Switcher.Title"] = "Customize",
        ["Theme.Switcher.Description"] = "Pick a color and radius for your components.",
        ["Theme.Color"] = "Color",
        ["Theme.Radius"] = "Radius",
        ["Theme.Mode"] = "Mode",
        ["Theme.Light"] = "Light",
        ["Theme.Dark"] = "Dark",
        ["Theme.SwitchToLight"] = "Switch to light mode",
        ["Theme.SwitchToDark"] = "Switch to dark mode",

        // Timeline
        ["Timeline.Timeline"] = "Timeline",
    };

    /// <inheritdoc />
    public virtual string this[string key] =>
        defaults.TryGetValue(key, out var value) ? value : key;

    /// <inheritdoc />
    public virtual string this[string key, params object[] arguments] =>
        defaults.TryGetValue(key, out var value) ? string.Format(System.Globalization.CultureInfo.CurrentCulture, value, arguments) : key;

    /// <summary>
    /// Sets a localization key to a custom value. Use this during startup to override English defaults.
    /// </summary>
    /// <param name="key">The localization key (e.g., <c>"DataGrid.Loading"</c>).</param>
    /// <param name="value">The localized string value.</param>
    public void Set(string key, string value) =>
        defaults[key] = value;
}

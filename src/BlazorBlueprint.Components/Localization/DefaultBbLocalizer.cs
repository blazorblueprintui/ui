using System.Collections.Frozen;

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
    private static readonly FrozenDictionary<string, string> englishDefaults = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Attachment.Open"] = "Open attachment",
        ["Sortable.Handle"] = "Reorder item",
        ["FileUpload.Drop"] = "Drop files here",
        ["FileUpload.Drag"] = "Drag and drop files here",
        ["FileUpload.Browse"] = "Browse to upload",
        ["FileUpload.Accepted"] = "Accepted: {0}",
        ["FileUpload.MaxSize"] = "Max size: {0}",
        ["FileUpload.MaxFiles"] = "Max files: {0}",
        ["FileUpload.Remove"] = "Remove {0}",
        ["FileUpload.TooMany"] = "Maximum {0} files allowed",
        ["FileUpload.TooLarge"] = "{0} exceeds {1} limit",
        ["FileUpload.InvalidType"] = "{0} is not an accepted file type",
        ["MultiSelect.More"] = "+{0} more",
        ["MultiSelect.Remove"] = "Remove {0}",
        ["DateTimePicker.AM"] = "AM",
        ["DateTimePicker.PM"] = "PM",
        ["Sortable.Cancelled"] = "Move cancelled.",
        ["Sortable.Unchanged"] = "Item dropped in its original position.",
        ["Sortable.MoveFailed"] = "Unable to move the item. Try again.",
        ["Sortable.Transferred"] = "Item transferred to the connected list.",
        ["Sortable.TransferRejected"] = "The transfer was not allowed or no connected list is available.",
        ["Sortable.TransferFailed"] = "Unable to transfer the item. Try again.",
        ["Sortable.PickedUp"] = "Picked up item {0} of {1}. Use arrow keys to move.",
        ["Sortable.Disabled"] = "Reordering is disabled. Control plus Left or Right transfers to a connected list.",
        ["Sortable.Position"] = "Position {0} of {1}. Press Space or Enter to drop.",

        // Alert
        ["Alert.Dismiss"] = "Dismiss",

        // AlertDialog (DialogService.AlertAsync)
        ["AlertDialog.Button"] = "OK",

        // AppBar
        ["AppBar.Back"] = "Go back",

        // Barcode
        ["Barcode.AriaLabel"] = "{0} barcode for {1}",
        ["Barcode.Empty"] = "There is nothing to encode.",
        ["Barcode.NotDigits"] = "{0} holds digits only, and this value contains '{1}'. {2}",
        ["Barcode.Code128NotAscii"] = "Code 128 holds ASCII only, and this value contains '{0}'. Use a QR code for anything beyond that.",
        ["Barcode.Code39InvalidCharacter"] = "Code 39 holds digits, letters and - . space $ / + %, and this value contains '{0}'. Use Code 128 for anything else.",
        ["Barcode.WrongLength"] = "{0} needs {1} digits, or {2} with the check digit already on the end. This value has {3}.",
        ["Barcode.WrongCheckDigit"] = "The check digit is wrong: {0} of {1} ends in {2}, not {3}. Leave the check digit off and it is worked out.",
        ["Barcode.InvalidIsbn"] = "An ISBN is 10 digits, or 13 beginning 978 or 979, with or without hyphens. This value is \"{0}\".",
        ["Barcode.InvalidIssn"] = "An ISSN is 8 characters, with or without a hyphen. This value is \"{0}\".",
        ["Barcode.ItfTooShort"] = "Interleaved 2 of 5 needs at least two digits.",
        ["Barcode.CodabarInvalidCharacter"] = "Codabar holds digits and - $ : / . +, wrapped in a start and stop letter from A to D. This value contains '{0}'.",
        ["Barcode.MsiEmpty"] = "MSI needs at least one digit.",
        ["Barcode.PostnetWrongLength"] = "POSTNET carries a 5-digit ZIP, a 9-digit ZIP+4 or an 11-digit delivery point code. This value has {0} digits.",
        ["Barcode.Rm4sccInvalidCharacter"] = "The Royal Mail 4-state code holds digits and upper-case letters, and this value contains '{0}'.",
        ["Barcode.Rm4sccEmpty"] = "The Royal Mail 4-state code needs at least one character.",
        ["Barcode.PharmacodeOutOfRange"] = "Pharmacode carries a whole number from 3 to 131070, and this value is \"{0}\".",
        ["Barcode.TelepenNotAscii"] = "Telepen holds ASCII only, and this value contains '{0}'.",
        ["Barcode.InputCode128"] = "Any ASCII text.",
        ["Barcode.InputCode39"] = "Digits, letters and - . space $ / + %.",
        ["Barcode.InputEan13"] = "12 digits, or 13 with the check digit.",
        ["Barcode.InputEan8"] = "7 digits, or 8 with the check digit.",
        ["Barcode.InputUpcA"] = "11 digits, or 12 with the check digit.",
        ["Barcode.InputItf"] = "Digits, an even number of them.",
        ["Barcode.InputCodabar"] = "Digits and - $ : / . +, optionally wrapped in a start and stop letter A to D.",
        ["Barcode.InputPostnet"] = "A 5, 9 or 11 digit ZIP code.",
        ["Barcode.InputPharmacode"] = "A whole number from 3 to 131070.",
        ["Barcode.InputRm4scc"] = "Digits and upper-case letters.",
        ["Barcode.InputIsbn"] = "A 10 or 13 digit ISBN, with or without hyphens.",
        ["Barcode.InputIssn"] = "An 8 character ISSN, with or without a hyphen.",
        ["Barcode.InputMsi"] = "Digits.",
        ["Barcode.InputTelepen"] = "Any ASCII text.",

        // DataMatrix
        ["DataMatrix.AriaLabel"] = "Data Matrix code",
        ["DataMatrix.AriaLabelWithValue"] = "Data Matrix code for {0}",
        ["DataMatrix.InvalidValue"] = "This value cannot be encoded in the selected Data Matrix symbol.",
        ["DataMatrix.Empty"] = "There is nothing to encode.",
        ["DataMatrix.TooLong"] = "This value is too long for a Data Matrix symbol of shape {0}, which holds at most {1} data codewords.",
        ["DataMatrix.CharacterNotInMode"] = "{0} encodation cannot carry '{1}'. Use Auto to choose a mode for each part of the value.",
        ["DataMatrix.InvalidText"] = "This value is not valid Unicode text.",
        ["DataMatrix.Gs1MissingAi"] = "GS1 notation must start with an Application Identifier in parentheses, such as (01).",
        ["DataMatrix.Gs1EmptyElement"] = "GS1 element ({0}) has no data.",
        ["DataMatrix.Gs1SeparatorInNotation"] = "GS1 element ({0}) contains a group separator. Use either (AI) notation or an element string, not both.",
        ["DataMatrix.Gs1WrongLength"] = "GS1 element ({0}) needs {1} data characters, and this value has {2}.",
        ["DataMatrix.DoesNotFitSize"] = "This value does not fit the {0} symbol, which holds {1} data codewords.",
        ["DataMatrix.Gs1NotDigits"] = "GS1 element ({0}) holds digits only.",
        ["DataMatrix.Gs1WrongCheckDigit"] = "GS1 element ({0}) ends in the wrong check digit: it should be {1}, not {2}.",

        // BottomNav
        ["BottomNav.Label"] = "Primary navigation",

        // Breadcrumb
        ["Breadcrumb.Breadcrumb"] = "breadcrumb",
        ["Breadcrumb.More"] = "More",

        // Calendar
        ["Calendar.GoToPreviousMonth"] = "Go to previous month",
        ["Calendar.GoToNextMonth"] = "Go to next month",

        // Carousel
        ["Carousel.ChooseSlide"] = "Choose a slide",
        ["Carousel.Slide"] = "Slide",
        ["Carousel.StartRotation"] = "Start automatic rotation",
        ["Carousel.StopRotation"] = "Stop automatic rotation",
        ["Carousel.NextSlide"] = "Next slide",
        ["Carousel.PreviousSlide"] = "Previous slide",
        ["Carousel.RoleDescription"] = "carousel",
        ["Carousel.SlideRoleDescription"] = "slide",

        // Cascader
        ["Cascader.Label"] = "Select a category",
        ["Cascader.Placeholder"] = "Select a category…",
        ["Cascader.Search"] = "Search paths…",
        ["Cascader.Empty"] = "No matching paths",
        ["Cascader.Clear"] = "Clear selection",
        ["Cascader.Level"] = "Level {0}",

        // CheckboxGroup
        ["CheckboxGroup.SelectAll"] = "Select all",

        // Chip
        ["Chip.Dismiss"] = "Remove",

        // Color Picker
        ["ColorPicker.Presets"] = "Presets",
        ["ColorPicker.Cancel"] = "Cancel",
        ["ColorPicker.Apply"] = "Apply",

        // Combobox
        ["Combobox.EmptyMessage"] = "No results found.",
        ["Combobox.Placeholder"] = "Select an option...",
        ["Combobox.SearchPlaceholder"] = "Search...",
        ["Combobox.Selected"] = "Selected",

        // Command
        ["Command.CommandMenu"] = "Command menu",
        ["Command.CommandList"] = "Command list",
        ["Command.Placeholder"] = "Type a command or search...",
        ["Command.Loading"] = "Loading...",

        // ConfirmDialog (DialogService.ConfirmAsync)
        ["ConfirmDialog.Confirm"] = "Continue",
        ["ConfirmDialog.Cancel"] = "Cancel",

        // CopyText
        ["CopyText.Copied"] = "Copied!",
        ["CopyText.ClickToCopy"] = "Click to copy",

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
        ["DataGrid.FilterColumn"] = "Filter {0}",
        ["DataGrid.Columns"] = "Columns",
        ["DataGrid.ToggleColumns"] = "Toggle columns",
        ["DataGrid.PinnedColumn"] = "Pinned",
        ["DataGrid.ColumnMenu"] = "{0} column options",
        ["DataGrid.GroupByColumn"] = "Group by {0}",
        ["DataGrid.AddGroupByColumn"] = "Then group by {0}",
        ["DataGrid.GroupingBreadcrumbLabel"] = "Grouped by",
        ["DataGrid.RemoveGroupLevel"] = "Stop grouping by {0}",
        ["DataGrid.MoveGroupLevelOut"] = "Move {0} out one level",
        ["DataGrid.ClearGrouping"] = "Clear grouping",
        ["DataGrid.UngroupColumn"] = "Remove grouping",
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
        ["DataGrid.Export"] = "Export",
        ["DataGrid.EditRow"] = "Edit row",
        ["DataGrid.BatchEditing"] = "Batch editing",
        ["DataGrid.SaveChanges"] = "Save changes",
        ["DataGrid.DiscardChanges"] = "Discard changes",
        ["DataGrid.PendingRows"] = "{0} pending rows",
        ["DataGrid.EditCell"] = "Edit {0}",
        ["DataGrid.ImmutableKey"] = "The row key cannot be changed.",
        ["DataGrid.SaveRejected"] = "The save was rejected. Your changes have been retained.",
        ["DataGrid.SaveFailed"] = "Unable to save. Your changes have been retained; please try again.",
        ["DataGrid.InvalidBatch"] = "Correct invalid values before saving the batch.",
        ["DataGrid.SaveRow"] = "Save",
        ["DataGrid.CancelRowEdit"] = "Cancel",
        ["DataGrid.ExportAriaLabel"] = "Export rows to CSV",

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
        ["DataView.SelectItem"] = "Select item",
        ["DataView.OtherGroup"] = "Other",
        ["DataView.SearchPlaceholder"] = "Search...",
        ["DataView.NoResultsFound"] = "No results found",
        ["DataView.Loading"] = "Loading...",
        ["DataView.LoadingMore"] = "Loading more...",
        ["DataView.LoadMore"] = "Load more",
        ["DataView.ListView"] = "List view",
        ["DataView.GridView"] = "Grid view",
        ["DataView.Sort"] = "Sort",
        ["DataView.SortAndFilter"] = "Sort and filter",
        ["DataView.SortAndFilterDescription"] = "Changes update the results immediately. Select a sort field again to change direction.",
        ["DataView.Done"] = "Done",
        ["DataView.ItemsPerPage"] = "Items per page",

        // DateInput
        ["DateInput.Label"] = "Date",
        ["DateInput.Year"] = "Year",
        ["DateInput.Month"] = "Month",
        ["DateInput.Day"] = "Day",
        ["DateInput.Invalid"] = "Enter a complete, valid date within the allowed range.",

        // DatePicker
        ["DatePicker.Placeholder"] = "Pick a date",
        ["DatePicker.OpenCalendar"] = "Open calendar",

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

        // DateTimePicker
        ["DateTimePicker.Placeholder"] = "Pick a date and time",
        ["DateTimePicker.Hour"] = "Hour",
        ["DateTimePicker.Minute"] = "Min",
        ["DateTimePicker.Second"] = "Sec",
        ["DateTimePicker.IncrementHour"] = "Increment hour",
        ["DateTimePicker.DecrementHour"] = "Decrement hour",
        ["DateTimePicker.IncrementMinute"] = "Increment minute",
        ["DateTimePicker.DecrementMinute"] = "Decrement minute",
        ["DateTimePicker.IncrementSecond"] = "Increment second",
        ["DateTimePicker.DecrementSecond"] = "Decrement second",
        ["DateTimePicker.Now"] = "Now",
        ["DateTimePicker.Clear"] = "Clear",

        // Dialog
        ["Dialog.Close"] = "Close",

        // Dock
        ["Dock.Close"] = "Close",
        ["Dock.CloseTab"] = "Close {0}",
        ["Dock.CloseOtherTabs"] = "Close Other Tabs",
        ["Dock.CloseAllButPinned"] = "Close All But Pinned",
        ["Dock.CloseAllTabs"] = "Close All Tabs",
        ["Dock.PinTab"] = "Pin Tab",
        ["Dock.UnpinTab"] = "Unpin Tab",
        ["Dock.ShowHiddenTabs"] = "Show {0} hidden tab(s)",
        ["Dock.MaximizePanelGroup"] = "Maximize panel group",
        ["Dock.RestorePanelGroup"] = "Restore panel group",
        ["Dock.NoPanelsOpen"] = "No panels are open.",

        // Drawer
        ["Drawer.Resize"] = "Resize drawer",

        // DynamicForm
        ["DynamicForm.Submit"] = "Submit",
        ["DynamicForm.ValidationErrors"] = "Validation Errors",
        ["DynamicForm.Required"] = "{0} is required.",
        ["DynamicForm.MinLength"] = "{0} must be at least {1} characters.",
        ["DynamicForm.MaxLength"] = "{0} must be at most {1} characters.",
        ["DynamicForm.Min"] = "{0} must be at least {1}.",
        ["DynamicForm.Max"] = "{0} must be at most {1}.",
        ["DynamicForm.Pattern"] = "{0} does not match the required format.",
        ["DynamicForm.Email"] = "{0} must be a valid email address.",
        ["DynamicForm.Url"] = "{0} must be a valid URL.",
        ["DynamicForm.Phone"] = "{0} must be a valid phone number.",

        // EventCalendar
        ["EventCalendar.Today"] = "Today",
        ["EventCalendar.Month"] = "Month",
        ["EventCalendar.Week"] = "Week",
        ["EventCalendar.Agenda"] = "Agenda",
        ["EventCalendar.PreviousPeriod"] = "Go to previous period",
        ["EventCalendar.NextPeriod"] = "Go to next period",
        ["EventCalendar.ViewSwitcher"] = "Calendar view",
        ["EventCalendar.MoreEvents"] = "+{0} more",
        ["EventCalendar.NoEvents"] = "No events to display.",
        ["EventCalendar.AllDay"] = "All day",

        // FileUpload
        ["FileUpload.Progress"] = "Upload progress for {0}",
        ["FileUpload.Selected"] = "Ready to upload",
        ["FileUpload.Queued"] = "Waiting to upload",
        ["FileUpload.Uploading"] = "Uploading…",
        ["FileUpload.Succeeded"] = "Uploaded",
        ["FileUpload.Canceled"] = "Upload canceled",
        ["FileUpload.Failed"] = "Upload failed. You can retry.",
        ["FileUpload.Cancel"] = "Cancel",
        ["FileUpload.Upload"] = "Upload",
        ["FileUpload.Retry"] = "Retry",

        // FilterBuilder
        ["FilterBuilder.FilterBuilderAriaLabel"] = "Filter builder",
        ["FilterBuilder.Apply"] = "Apply Filter",
        ["FilterBuilder.Clear"] = "Clear",
        ["FilterBuilder.RootGroup"] = "Root filter group",
        ["FilterBuilder.NestedGroup"] = "Nested filter group at depth {0}",
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
        ["FilterBuilder.Presets"] = "Filter presets",
        ["FilterBuilder.Where"] = "Where",
        ["FilterBuilder.OperatorAnd"] = "AND",
        ["FilterBuilder.OperatorOr"] = "OR",
        ["FilterBuilder.OperatorEquals"] = "equals",
        ["FilterBuilder.OperatorNotEquals"] = "not equals",
        ["FilterBuilder.OperatorIsEmpty"] = "is empty",
        ["FilterBuilder.OperatorIsNotEmpty"] = "is not empty",
        ["FilterBuilder.OperatorContains"] = "contains",
        ["FilterBuilder.OperatorNotContains"] = "not contains",
        ["FilterBuilder.OperatorStartsWith"] = "starts with",
        ["FilterBuilder.OperatorEndsWith"] = "ends with",
        ["FilterBuilder.OperatorGreaterThan"] = "greater than",
        ["FilterBuilder.OperatorLessThan"] = "less than",
        ["FilterBuilder.OperatorGreaterOrEqual"] = "greater or equal",
        ["FilterBuilder.OperatorLessOrEqual"] = "less or equal",
        ["FilterBuilder.OperatorBetween"] = "between",
        ["FilterBuilder.OperatorInLast"] = "in the last",
        ["FilterBuilder.OperatorInNext"] = "in the next",
        ["FilterBuilder.OperatorIn"] = "is any of",
        ["FilterBuilder.OperatorNotIn"] = "is none of",
        ["FilterBuilder.OperatorIsTrue"] = "is true",
        ["FilterBuilder.OperatorIsFalse"] = "is false",
        ["FilterBuilder.OperatorDateIs"] = "is",
        ["FilterBuilder.OperatorDateIsNot"] = "is not",
        ["FilterBuilder.OperatorGreaterThanDate"] = "is after",
        ["FilterBuilder.OperatorLessThanDate"] = "is before",

        // FormFieldInput
        ["FormField.ParseError"] = "'{0}' is not a valid {1}.",
        ["FormField.PatternError"] = "'{0}' does not match the required format.",
        ["FormField.ValueError"] = "The entered value is not valid.",
        ["FormField.InvalidInput"] = "Invalid input.",
        ["FormField.TypeText"] = "text value",
        ["FormField.TypeWholeNumber"] = "whole number",
        ["FormField.TypeNumber"] = "number",
        ["FormField.TypeDate"] = "date",
        ["FormField.TypeTime"] = "time",
        ["FormField.TypeGuid"] = "GUID",
        ["FormField.TypeBoolean"] = "true/false value",

        // FormSection
        ["FormSection.Expand"] = "Expand section",
        ["FormSection.Collapse"] = "Collapse section",

        // FormWizard
        ["FormWizard.WizardProgress"] = "Wizard progress",
        ["FormWizard.Back"] = "Back",
        ["FormWizard.Next"] = "Next",
        ["FormWizard.Skip"] = "Skip",
        ["FormWizard.Complete"] = "Complete",

        // Gantt
        ["Gantt.Task"] = "Task",
        ["Gantt.Today"] = "Today",
        ["Gantt.ExpandAll"] = "Expand all",
        ["Gantt.CollapseAll"] = "Collapse all",
        ["Gantt.Expand"] = "Show the tasks under {0}",
        ["Gantt.Collapse"] = "Hide the tasks under {0}",
        ["Gantt.Zoom"] = "Zoom",
        ["Gantt.ZoomHour"] = "Hour",
        ["Gantt.ZoomDay"] = "Day",
        ["Gantt.ZoomWeek"] = "Week",
        ["Gantt.ZoomMonth"] = "Month",
        ["Gantt.ZoomQuarter"] = "Quarter",
        ["Gantt.ZoomYear"] = "Year",
        ["Gantt.Week"] = "W{0}",
        ["Gantt.Quarter"] = "Q{0}",
        ["Gantt.Loading"] = "Loading...",
        ["Gantt.Empty"] = "Nothing to schedule",
        ["Gantt.Dependencies"] = "Task dependencies",
        ["Gantt.FinishToStart"] = "{0} finishes before {1} starts",
        ["Gantt.StartToStart"] = "{0} starts when {1} starts",
        ["Gantt.FinishToFinish"] = "{0} finishes when {1} finishes",
        ["Gantt.StartToFinish"] = "{0} starts before {1} finishes",
        ["Gantt.BarLabel"] = "{0}, {1} to {2}, {3}% done",
        ["Gantt.MilestoneLabel"] = "{0}, milestone on {1}",
        ["Gantt.ProgressHandle"] = "Drag to set how far along {0} is",
        ["Gantt.LinkFromStart"] = "Draw a dependency from the start of {0}",
        ["Gantt.LinkFromEnd"] = "Draw a dependency from the end of {0}",
        ["Gantt.DragRow"] = "Drag to move {0}",
        ["Gantt.TooltipRange"] = "{0} \u2013 {1}",
        ["Gantt.TooltipDays"] = "{0} days",
        ["Gantt.TooltipHours"] = "{0} hours",
        ["Gantt.TooltipDone"] = "{0}% done",
        ["Gantt.Legend"] = "What the shapes mean",
        ["Gantt.LegendTask"] = "Task",
        ["Gantt.LegendDone"] = "Done",
        ["Gantt.LegendSummary"] = "Summary, rolled up from its children",
        ["Gantt.LegendMilestone"] = "Milestone",
        ["Gantt.LegendToday"] = "Today",
        ["Gantt.LegendNonWorking"] = "Non-working day",
        ["Gantt.MissingId"] = "Every task needs an identifier.",
        ["Gantt.DuplicateId"] = "Two tasks share the identifier '{0}'.",
        ["Gantt.ParentLoop"] = "The parent identifiers form a loop.",

        // Link
        ["Link.OpensInNewTab"] = "(opens in a new tab)",

        // Scroll to top
        ["ScrollToTop.Label"] = "Scroll back to top",

        // Exit prompt
        ["ExitPrompt.Title"] = "Leave without saving?",
        ["ExitPrompt.Message"] = "Your changes have not been saved. If you leave now, they are lost.",
        ["ExitPrompt.Stay"] = "Stay on this page",
        ["ExitPrompt.Leave"] = "Leave and discard",

        // Stepper
        ["Stepper.Progress"] = "Progress",
        ["Stepper.Optional"] = "Optional",
        ["Stepper.StepPosition"] = "Step {0} of {1}",
        ["Stepper.Completed"] = "Completed",
        ["Stepper.Error"] = "Error",
        ["Stepper.Skipped"] = "Skipped",

        // MarkdownEditor
        ["MarkdownEditor.SelectHeadingLevel"] = "Select heading level",
        ["MarkdownEditor.Bold"] = "Bold (Ctrl+B)",
        ["MarkdownEditor.Italic"] = "Italic (Ctrl+I)",
        ["MarkdownEditor.Underline"] = "Underline (Ctrl+U)",
        ["MarkdownEditor.BulletList"] = "Bullet list",
        ["MarkdownEditor.NumberedList"] = "Numbered list",
        ["MarkdownEditor.Write"] = "Write",
        ["MarkdownEditor.Preview"] = "Preview",
        ["MarkdownEditor.TextStyle"] = "Text Style",

        // MultiSelect
        ["MultiSelect.EmptyMessage"] = "No results found.",
        ["MultiSelect.Placeholder"] = "Select items...",
        ["MultiSelect.SearchPlaceholder"] = "Search...",
        ["MultiSelect.SelectAll"] = "Select All",
        ["MultiSelect.Clear"] = "Clear",
        ["MultiSelect.Close"] = "Close",

        // NotificationBadge
        ["NotificationBadge.Unread"] = "Unread notifications",
        ["NotificationBadge.Count"] = "{0} notifications",

        // NumericInput
        ["NumericInput.IncreaseValue"] = "Increase value",
        ["NumericInput.DecreaseValue"] = "Decrease value",

        // PivotDataGrid
        ["PivotDataGrid.Total"] = "Total",
        ["PivotDataGrid.Blank"] = "(blank)",
        ["PivotDataGrid.Empty"] = "Nothing to cross-tabulate",
        ["PivotDataGrid.Loading"] = "Loading...",
        ["PivotDataGrid.Fields"] = "Fields",
        ["PivotDataGrid.RowFields"] = "Rows",
        ["PivotDataGrid.ColumnFields"] = "Columns",
        ["PivotDataGrid.ValueFields"] = "Values",
        ["PivotDataGrid.PageOf"] = "Page {0} of {1}",
        ["PivotDataGrid.PreviousPage"] = "Previous page",
        ["PivotDataGrid.NextPage"] = "Next page",

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

        // PromptDialog (DialogService.PromptAsync)
        ["PromptDialog.Confirm"] = "OK",
        ["PromptDialog.Cancel"] = "Cancel",
        ["PromptDialog.Required"] = "The value is required",
        ["PromptDialog.MaxLength"] = "Value must be at most {0} characters.",

        // QuantityStepper
        ["QuantityStepper.Label"] = "Quantity",
        ["QuantityStepper.Remove"] = "Remove item",
        ["QuantityStepper.Increase"] = "Increase quantity",
        ["QuantityStepper.Decrease"] = "Decrease quantity",

        // QrCode
        ["QrCode.AriaLabel"] = "QR code",
        ["QrCode.AriaLabelWithValue"] = "QR code for {0}",
        ["QrCode.TooLong"] = "This value is too long to fit in a QR code.",

        // RangeSlider
        ["RangeSlider.RangeStart"] = "Range start",
        ["RangeSlider.RangeEnd"] = "Range end",

        // Rating
        ["Rating.Rating"] = "Rating",
        ["Rating.ItemLabel"] = "{0} out of {1}",

        // ResponsiveNav
        ["ResponsiveNav.ToggleMenu"] = "Toggle Menu",

        // Resizable
        ["Resizable.ResizeHandle"] = "Resize panels",

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
        ["RichTextEditor.Undo"] = "Undo (Ctrl+Z)",
        ["RichTextEditor.Redo"] = "Redo (Ctrl+Shift+Z)",
        ["RichTextEditor.InlineCode"] = "Inline Code",
        ["RichTextEditor.CheckList"] = "Checklist",
        ["RichTextEditor.Align"] = "Text Alignment",
        ["RichTextEditor.AlignLeft"] = "Align Left",
        ["RichTextEditor.AlignCenter"] = "Align Center",
        ["RichTextEditor.AlignRight"] = "Align Right",
        ["RichTextEditor.AlignJustify"] = "Justify",
        ["RichTextEditor.TextColor"] = "Text Color",
        ["RichTextEditor.Highlight"] = "Highlight",
        ["RichTextEditor.RemoveColor"] = "Remove Color",
        ["RichTextEditor.InsertImage"] = "Insert Image",
        ["RichTextEditor.Table"] = "Table",
        ["RichTextEditor.InsertTable"] = "Insert Table",
        ["RichTextEditor.TableSize"] = "{0} \u00d7 {1}",
        ["RichTextEditor.InsertRowAbove"] = "Insert Row Above",
        ["RichTextEditor.InsertRowBelow"] = "Insert Row Below",
        ["RichTextEditor.InsertColumnLeft"] = "Insert Column Left",
        ["RichTextEditor.InsertColumnRight"] = "Insert Column Right",
        ["RichTextEditor.DeleteRow"] = "Delete Row",
        ["RichTextEditor.DeleteColumn"] = "Delete Column",
        ["RichTextEditor.DeleteTable"] = "Delete Table",
        ["RichTextEditor.LinkUrl"] = "URL",
        ["RichTextEditor.LinkUrlPlaceholder"] = "https://example.com",
        ["RichTextEditor.InvalidUrl"] = "Please enter a valid URL",

        // Scheduler
        ["Scheduler.Repeat"] = "Repeat",
        ["Scheduler.Repeat.NONE"] = "Does not repeat",
        ["Scheduler.Repeat.DAILY"] = "Daily",
        ["Scheduler.Repeat.WEEKLY"] = "Weekly",
        ["Scheduler.Repeat.MONTHLY"] = "Monthly",
        ["Scheduler.Repeat.YEARLY"] = "Yearly",
        ["Scheduler.Repeat.EXISTING"] = "Existing repeat schedule",
        ["Scheduler.ExistingRepeatHelp"] = "The existing repeat schedule will be kept. Choose another repeat option to replace it.",
        ["Scheduler.RepeatOn"] = "Repeat on",
        ["Scheduler.ChooseRepeatDay"] = "Choose at least one day for a weekly event.",
        ["Scheduler.MonthlyOn"] = "Repeats on day {0} of each month. Months without this date are skipped.",
        ["Scheduler.YearlyOn"] = "Repeats every year on {0}. Years without this date are skipped.",
        ["Scheduler.LimitOccurrences"] = "End after a number of occurrences",
        ["Scheduler.Count"] = "Number of occurrences",
        ["Scheduler.Label"] = "Schedule",
        ["Scheduler.TimeSlots"] = "Schedule time slots",
        ["Scheduler.Previous"] = "Previous period",
        ["Scheduler.Next"] = "Next period",
        ["Scheduler.Today"] = "Today",
        ["Scheduler.Day"] = "Day",
        ["Scheduler.Week"] = "Week",
        ["Scheduler.WorkWeek"] = "Work week",
        ["Scheduler.Month"] = "Month",
        ["Scheduler.MoreEvents"] = "+{0} more",
        ["Scheduler.DayCell"] = "{0}. Press Enter to create an event.",
        ["Scheduler.WeekStartsOn"] = "Week starts on",
        ["Scheduler.WeekStartDay"] = "{0} start",
        ["Scheduler.Unassigned"] = "Unassigned",
        ["Scheduler.AllDay"] = "All day",
        ["Scheduler.AllDayRegion"] = "All-day events",
        ["Scheduler.AllDayLabel"] = "{0}, all day on {1}",
        ["Scheduler.AllDayRangeLabel"] = "{0}, all day from {1} to {2}",
        ["Scheduler.MoreAllDay"] = "+{0} more",
        ["Scheduler.Resources.Filter"] = "Resources shown",
        ["Scheduler.Resources.FilterPlaceholder"] = "All resources",
        ["Scheduler.Resources.FilterSearch"] = "Search resources...",
        ["Scheduler.Resources.FilterEmpty"] = "No resource found.",
        ["Scheduler.NoResourcesSelected"] = "No resources are selected. Choose at least one resource to see the schedule.",
        ["Scheduler.CreateAt"] = "Create an event at {0} for {1}",
        ["Scheduler.SlotLabel"] = "{0}. Press Enter to create an event.",
        ["Scheduler.SlotLabelForResource"] = "{0} for {1}. Press Enter to create an event.",
        ["Scheduler.OutsideActiveHours"] = "Outside the usual hours.",
        ["Scheduler.Create"] = "New event",
        ["Scheduler.Edit"] = "Edit event",
        ["Scheduler.EditorDescription"] = "Times use the event time zone. Changes are saved when you choose Save.",
        ["Scheduler.SingleZoneDescription"] = "Times use the schedule's time zone. Changes are saved when you choose Save.",
        ["Scheduler.ResizeStart"] = "Drag to change the start time",
        ["Scheduler.ResizeEnd"] = "Drag to change the end time",
        ["Scheduler.ConfirmDelete"] = "Delete event?",
        ["Scheduler.DeleteDescription"] = "Delete “{0}”? This cannot be undone.",
        ["Scheduler.DeleteOccurrenceDescription"] = "Delete this occurrence of “{0}”? Other occurrences will be kept.",
        ["Scheduler.DeleteSeriesDescription"] = "Delete the entire “{0}” series, including edited occurrences? This cannot be undone.",
        ["Scheduler.Scope"] = "Apply changes to",
        ["Scheduler.Occurrence"] = "This occurrence",
        ["Scheduler.Series"] = "Entire series",
        ["Scheduler.Title"] = "Title",
        ["Scheduler.Start"] = "Start",
        ["Scheduler.End"] = "End",
        ["Scheduler.LastDay"] = "Last day",
        ["Scheduler.TimeZone"] = "Event time zone",
        ["Scheduler.StartRepeatedTime"] = "If the clocks repeat the start time",
        ["Scheduler.EndRepeatedTime"] = "If the clocks repeat the end time",
        ["Scheduler.EarlierOffset"] = "Use the first occurrence",
        ["Scheduler.LaterOffset"] = "Use the second occurrence",
        ["Scheduler.Resources"] = "Resources",
        ["Scheduler.Delete"] = "Delete",
        ["Scheduler.Menu.NewEvent"] = "New event",
        ["Scheduler.Menu.EditEvent"] = "Edit event",
        ["Scheduler.Menu.DeleteEvent"] = "Delete event",
        ["Scheduler.Cancel"] = "Cancel",
        ["Scheduler.Save"] = "Save",
        ["Scheduler.InvalidEvents"] = "The schedule contains invalid events, time zones or recurrence rules.",
        ["Scheduler.InvalidEdit"] = "Check the title, recurrence, time zone, and end time. Times skipped by daylight saving cannot be selected.",
        ["Scheduler.SaveRejected"] = "These changes were rejected. Your edits have been retained.",
        ["Scheduler.SaveFailed"] = "Unable to save. Your edits have been retained; please try again.",

        // ListBox
        ["ListBox.SearchPlaceholder"] = "Search...",
        ["ListBox.SelectAll"] = "Select all",
        ["ListBox.Empty"] = "No options",
        ["ListBox.NoMatches"] = "No matches",
        ["ListBox.SelectedCount"] = "{0} selected",

        // PickList
        ["PickList.Available"] = "Available",
        ["PickList.Selected"] = "Selected",
        ["PickList.MoveSelectedToTarget"] = "Move selected",
        ["PickList.MoveAllToTarget"] = "Move all",
        ["PickList.MoveSelectedToSource"] = "Remove selected",
        ["PickList.MoveAllToSource"] = "Remove all",

        // Select
        ["Select.ChooseOption"] = "Choose an option",

        // Sheet
        ["Sheet.Close"] = "Close",

        // Signature
        ["Signature.Draw"] = "Draw",
        ["Signature.Type"] = "Type",
        ["Signature.DrawHint"] = "Sign here",
        ["Signature.TypeHint"] = "Type your full name",
        ["Signature.Clear"] = "Clear",
        ["Signature.Undo"] = "Undo last stroke",
        ["Signature.Canvas"] = "Signature drawing area",
        ["Signature.MethodLabel"] = "How to sign",

        // Sidebar
        ["Sidebar.PillNavigation"] = "Primary navigation",
        ["Sidebar.ExpandNavigation"] = "Expand navigation",
        ["Sidebar.ToggleSidebar"] = "Toggle Sidebar",

        // Sortable
        ["Sortable.KeyboardInstructions"] = "Press Space or Enter to pick up. Use arrows to reorder, Control plus Left or Right to transfer to a connected list, Space or Enter to drop, or Escape to cancel.",
        ["Sortable.MoveRejected"] = "The move was not allowed.",
        ["Sortable.DropRejected"] = "The drop was not allowed.",
        ["Sortable.Moved"] = "Item moved from position {0} to position {1}.",
        ["Sortable.Removed"] = "Item removed from position {0} and placed at position {1} in another list.",
        ["Sortable.Received"] = "Item received at position {0}.",

        // Spinner
        ["Spinner.Loading"] = "Loading",

        // SplitButton
        ["SplitButton.MoreOptions"] = "More options",

        // Tabs
        ["Tabs.Add"] = "New tab",
        ["Tabs.Close"] = "Close",
        ["Tabs.RenameLabel"] = "Rename {0}",

        // TagInput
        ["TagInput.Placeholder"] = "Add tag...",
        ["TagInput.RemoveTag"] = "Remove {0}",
        ["TagInput.ClearAllTags"] = "Clear all tags",
        ["TagInput.TagSuggestions"] = "Tag suggestions",
        ["TagInput.AddTag"] = "Add tag",

        // Theme
        ["Theme.Switcher.Label"] = "Customize theme",
        ["Theme.Switcher.Title"] = "Customize",
        ["Theme.Switcher.Description"] = "Pick a color and radius for your components.",
        ["Theme.Color"] = "Color",
        ["Theme.BaseColor"] = "Base color",
        ["Theme.PrimaryColor"] = "Primary color",
        ["Theme.Default"] = "Default",
        ["Theme.Density"] = "Density",
        ["Theme.Font"] = "Font",
        ["Theme.Surface"] = "Surfaces",
        ["Theme.MenuColor"] = "Menu color",
        ["Theme.MenuAccent"] = "Menu accent",
        ["Theme.Radius"] = "Radius",
        ["Theme.Mode"] = "Mode",
        ["Theme.Light"] = "Light",
        ["Theme.Dark"] = "Dark",
        ["Theme.SwitchToLight"] = "Switch to light mode",
        ["Theme.SwitchToDark"] = "Switch to dark mode",

        // TimeInput
        ["TimeInput.Label"] = "Time",
        ["TimeInput.Hour"] = "Hour",
        ["TimeInput.Minute"] = "Minute",
        ["TimeInput.Second"] = "Second",
        ["TimeInput.Period"] = "AM or PM",
        ["TimeInput.OpenPicker"] = "Open time picker",
        ["TimeInput.Invalid"] = "Enter a complete, valid time within the allowed range.",

        // Timeline
        ["Timeline.Timeline"] = "Timeline",
        ["Timeline.Empty"] = "No timeline items to display",

        // TimePicker
        ["TimePicker.Placeholder"] = "Select time",

        // Toast
        ["Toast.Region"] = "Notifications",
        ["Toast.Close"] = "Close",

        // TreeSelect
        ["TreeSelect.Label"] = "Select from tree",
        ["TreeSelect.Placeholder"] = "Select an item…",
        ["TreeSelect.Clear"] = "Clear selection",
        ["TreeSelect.Search"] = "Search…",

        // TreeView
        ["TreeView.SearchPlaceholder"] = "Search...",
        ["TreeView.FailedToLoad"] = "Failed to load",
        ["TreeView.Retry"] = "Retry",
    }.ToFrozenDictionary(StringComparer.Ordinal);

    private readonly Dictionary<string, string> defaults = new(englishDefaults, StringComparer.Ordinal);

    /// <summary>
    /// Gets every localization key with its built-in English text.
    /// </summary>
    /// <remarks>
    /// Overrides made with <see cref="Set"/> are not reflected here. Use this to list the available
    /// keys, or to export them as a starting point for a translation (for example as JSON).
    /// Values that take <see cref="string.Format(string, object[])"/> arguments contain
    /// <c>{0}</c>-style placeholders.
    /// </remarks>
    public static IReadOnlyDictionary<string, string> EnglishDefaults => englishDefaults;

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

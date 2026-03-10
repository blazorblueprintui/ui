namespace BlazorBlueprint.Components;

/// <summary>
/// Provides localization options for all BlazorBlueprint components.
/// Register via <c>AddBlazorBlueprintComponents(opts => { ... })</c>.
/// </summary>
/// <remarks>
/// <para>
/// All properties are pre-populated with English defaults. Override individual labels
/// at startup or use scoped registration for dynamic culture switching.
/// </para>
/// <example>
/// <code>
/// // Static configuration at startup
/// builder.Services.AddBlazorBlueprintComponents(opts =>
/// {
///     opts.DataGrid.NoResultsFound = "Keine Ergebnisse gefunden";
///     opts.Calendar.GoToPreviousMonth = "Zum vorherigen Monat";
/// });
///
/// // Dynamic with IStringLocalizer (register as scoped)
/// builder.Services.AddScoped(sp =>
/// {
///     var loc = sp.GetRequiredService&lt;IStringLocalizer&lt;SharedResources&gt;&gt;();
///     var options = new BbLocalizationOptions();
///     options.DataGrid.NoResultsFound = loc["NoResults"];
///     return options;
/// });
/// </code>
/// </example>
/// </remarks>
public class BbLocalizationOptions
{
    public BbCalendarLabels Calendar { get; set; } = new();
    public BbDataGridLabels DataGrid { get; set; } = new();
    public BbDataTableLabels DataTable { get; set; } = new();
    public BbPaginationLabels Pagination { get; set; } = new();
    public BbComboboxLabels Combobox { get; set; } = new();
    public BbMultiSelectLabels MultiSelect { get; set; } = new();
    public BbFilterBuilderLabels FilterBuilder { get; set; } = new();
    public BbSidebarLabels Sidebar { get; set; } = new();
    public BbCarouselLabels Carousel { get; set; } = new();
    public BbAlertLabels Alert { get; set; } = new();
    public BbDashboardGridLabels DashboardGrid { get; set; } = new();
    public BbDatePickerLabels DatePicker { get; set; } = new();
    public BbDateRangePickerLabels DateRangePicker { get; set; } = new();
    public BbNumericInputLabels NumericInput { get; set; } = new();
    public BbTagInputLabels TagInput { get; set; } = new();
    public BbCommandLabels Command { get; set; } = new();
    public BbDataViewLabels DataView { get; set; } = new();
    public BbDialogLabels Dialog { get; set; } = new();
    public BbSheetLabels Sheet { get; set; } = new();
    public BbFormWizardLabels FormWizard { get; set; } = new();
    public BbRatingLabels Rating { get; set; } = new();
    public BbBreadcrumbLabels Breadcrumb { get; set; } = new();
    public BbResponsiveNavLabels ResponsiveNav { get; set; } = new();
    public BbTimelineLabels Timeline { get; set; } = new();
}

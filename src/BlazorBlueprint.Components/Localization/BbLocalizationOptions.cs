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
/// <para>
/// <b>Thread safety:</b> Label properties are not thread-safe for concurrent write access.
/// Configure labels during startup in the <c>AddBlazorBlueprintComponents</c> callback.
/// For dynamic culture switching (e.g., Blazor Server per-circuit localization), register
/// <c>BbLocalizationOptions</c> as scoped instead of singleton — see the Localization Guide.
/// </para>
/// <para>
/// <b>Localization patterns:</b> Components use two access patterns depending on whether
/// a per-instance parameter override exists. Strings exposed as component <c>[Parameter]</c>
/// properties (e.g., <c>Placeholder</c>) use an <c>EffectiveX</c> fallback: the parameter
/// value wins if set, otherwise the DI label is used. Strings without a corresponding
/// parameter (e.g., internal button text, aria-labels) are read directly from the DI service.
/// Both patterns are intentional — the first supports instance-level overrides while the
/// second avoids API bloat for rarely-customized chrome strings.
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
    public BbCalendarLabels Calendar { get; } = new();
    public BbDataGridLabels DataGrid { get; } = new();
    public BbDataTableLabels DataTable { get; } = new();
    public BbPaginationLabels Pagination { get; } = new();
    public BbComboboxLabels Combobox { get; } = new();
    public BbMultiSelectLabels MultiSelect { get; } = new();
    public BbFilterBuilderLabels FilterBuilder { get; } = new();
    public BbSidebarLabels Sidebar { get; } = new();
    public BbCarouselLabels Carousel { get; } = new();
    public BbAlertLabels Alert { get; } = new();
    public BbDashboardGridLabels DashboardGrid { get; } = new();
    public BbDatePickerLabels DatePicker { get; } = new();
    public BbDateRangePickerLabels DateRangePicker { get; } = new();
    public BbNumericInputLabels NumericInput { get; } = new();
    public BbTagInputLabels TagInput { get; } = new();
    public BbCommandLabels Command { get; } = new();
    public BbDataViewLabels DataView { get; } = new();
    public BbDialogLabels Dialog { get; } = new();
    public BbSheetLabels Sheet { get; } = new();
    public BbFormWizardLabels FormWizard { get; } = new();
    public BbRatingLabels Rating { get; } = new();
    public BbBreadcrumbLabels Breadcrumb { get; } = new();
    public BbResponsiveNavLabels ResponsiveNav { get; } = new();
    public BbTimelineLabels Timeline { get; } = new();
    public BbMarkdownEditorLabels MarkdownEditor { get; } = new();
    public BbRichTextEditorLabels RichTextEditor { get; } = new();
}

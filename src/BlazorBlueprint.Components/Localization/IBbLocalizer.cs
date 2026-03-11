namespace BlazorBlueprint.Components;

/// <summary>
/// Provides localized strings for BlazorBlueprint components.
/// </summary>
/// <remarks>
/// <para>
/// Components resolve all UI chrome strings (placeholders, button labels, aria-labels, empty states)
/// through this interface using dot-notation keys (e.g., <c>"DataGrid.Loading"</c>, <c>"Combobox.Placeholder"</c>).
/// </para>
/// <para>
/// The library registers <see cref="DefaultBbLocalizer"/> by default, which provides English defaults
/// for all keys. To customize strings, either configure the default localizer at startup or replace
/// the registration with a custom implementation.
/// </para>
/// <example>
/// <code>
/// // Pattern A: Configure defaults at startup
/// builder.Services.AddBlazorBlueprintComponents(localizer =>
/// {
///     localizer.Set("DataGrid.Loading", "Laden...");
///     localizer.Set("Pagination.Next", "Weiter");
/// });
///
/// // Pattern B: Custom implementation with IStringLocalizer
/// public class AppBbLocalizer(IStringLocalizer&lt;SharedResources&gt; loc) : DefaultBbLocalizer
/// {
///     public override string this[string key] =>
///         loc[key] is { ResourceNotFound: false } found ? found.Value : base[key];
///
///     public override string this[string key, params object[] arguments] =>
///         loc[key, arguments] is { ResourceNotFound: false } found ? found.Value : base[key, arguments];
/// }
///
/// builder.Services.AddBlazorBlueprintComponents();
/// builder.Services.AddScoped&lt;IBbLocalizer, AppBbLocalizer&gt;();
/// </code>
/// </example>
/// </remarks>
public interface IBbLocalizer
{
    /// <summary>
    /// Gets the localized string for the specified key.
    /// </summary>
    /// <param name="key">The localization key in dot notation (e.g., <c>"DataGrid.Loading"</c>).</param>
    /// <returns>The localized string, or the key itself if not found.</returns>
    public string this[string key] { get; }

    /// <summary>
    /// Gets the localized string for the specified key, formatted with the provided arguments.
    /// </summary>
    /// <param name="key">The localization key in dot notation (e.g., <c>"DataGrid.ShowingRange"</c>).</param>
    /// <param name="arguments">The format arguments to substitute into the string.</param>
    /// <returns>The formatted localized string, or the key itself if not found.</returns>
    public string this[string key, params object[] arguments] { get; }
}

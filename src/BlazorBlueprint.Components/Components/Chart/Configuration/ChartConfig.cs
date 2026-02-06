using System.Collections;

namespace BlazorBlueprint.Components.Chart;

/// <summary>
/// Maps series names to their configuration (labels and colors) for chart components.
/// </summary>
/// <remarks>
/// <para>
/// ChartConfig follows the shadcn/ui pattern of mapping data keys to display
/// configuration. This separation allows data to use technical keys while
/// displaying user-friendly labels.
/// </para>
/// <para>
/// Features:
/// - Maps series keys to labels and colors
/// - Supports CSS custom properties for theming
/// - Provides default colors (--chart-1 through --chart-5)
/// - Implements IEnumerable for collection initializer syntax
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var config = ChartConfig.Create(
///     ("desktop", new ChartSeriesConfig { Label = "Desktop", Color = "var(--chart-1)" }),
///     ("mobile", new ChartSeriesConfig { Label = "Mobile", Color = "var(--chart-2)" })
/// );
///
/// // Or using collection initializer:
/// var config = new ChartConfig
/// {
///     { "desktop", new ChartSeriesConfig { Label = "Desktop", Color = "var(--chart-1)" } },
///     { "mobile", new ChartSeriesConfig { Label = "Mobile", Color = "var(--chart-2)" } }
/// };
/// </code>
/// </example>
public class ChartConfig : IEnumerable<KeyValuePair<string, ChartSeriesConfig>>
{
    private readonly Dictionary<string, ChartSeriesConfig> _configs = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the series configuration for the specified key.
    /// </summary>
    /// <param name="key">The series key (e.g., "desktop", "mobile").</param>
    /// <returns>The configuration for the series, or null if not found.</returns>
    public ChartSeriesConfig? this[string key]
    {
        get => _configs.TryGetValue(key, out var config) ? config : null;
        set
        {
            if (value != null)
            {
                _configs[key] = value;
            }
            else
            {
                _configs.Remove(key);
            }
        }
    }

    /// <summary>
    /// Adds a series configuration to the chart config.
    /// </summary>
    /// <param name="key">The series key that matches data property names.</param>
    /// <param name="config">The configuration for the series.</param>
    public void Add(string key, ChartSeriesConfig config) =>
        _configs[key] = config;

    /// <summary>
    /// Gets the label for a series key, returning the key itself if not configured.
    /// </summary>
    /// <param name="key">The series key.</param>
    /// <returns>The configured label or the key as a fallback.</returns>
    public string GetLabel(string key)
    {
        return _configs.TryGetValue(key, out var config) && !string.IsNullOrEmpty(config.Label)
            ? config.Label
            : key;
    }

    /// <summary>
    /// Gets the color for a series key, returning a default chart color if not configured.
    /// </summary>
    /// <param name="key">The series key.</param>
    /// <param name="index">The index to use for default color selection.</param>
    /// <returns>The configured color or a default chart color.</returns>
    public string GetColor(string key, int index = 0)
    {
        if (_configs.TryGetValue(key, out var config) && !string.IsNullOrEmpty(config.Color))
        {
            return config.Color;
        }

        return ChartColor.GetDefault(index);
    }

    /// <summary>
    /// Gets all configured series keys.
    /// </summary>
    public IEnumerable<string> Keys => _configs.Keys;

    /// <summary>
    /// Gets the number of configured series.
    /// </summary>
    public int Count => _configs.Count;

    /// <summary>
    /// Checks if a series key is configured.
    /// </summary>
    /// <param name="key">The series key to check.</param>
    /// <returns>True if the key is configured, false otherwise.</returns>
    public bool ContainsKey(string key) => _configs.ContainsKey(key);

    /// <summary>
    /// Creates a ChartConfig from a collection of key-value pairs.
    /// </summary>
    /// <param name="configs">Tuples of (key, config) pairs.</param>
    /// <returns>A new ChartConfig instance.</returns>
    /// <example>
    /// <code>
    /// var config = ChartConfig.Create(
    ///     ("desktop", new ChartSeriesConfig { Label = "Desktop", Color = "var(--chart-1)" }),
    ///     ("mobile", new ChartSeriesConfig { Label = "Mobile", Color = "var(--chart-2)" })
    /// );
    /// </code>
    /// </example>
    public static ChartConfig Create(params (string Key, ChartSeriesConfig Config)[] configs)
    {
        var chartConfig = new ChartConfig();
        foreach (var (key, config) in configs)
        {
            chartConfig.Add(key, config);
        }
        return chartConfig;
    }

    /// <summary>
    /// Creates a ChartConfig with default colors for the specified labels.
    /// </summary>
    /// <param name="seriesLabels">Labels for each series (keys will be generated).</param>
    /// <returns>A new ChartConfig with default chart colors.</returns>
    public static ChartConfig FromLabels(params string[] seriesLabels)
    {
        var chartConfig = new ChartConfig();
        for (var i = 0; i < seriesLabels.Length; i++)
        {
            var label = seriesLabels[i];
            var key = label.ToLowerInvariant().Replace(" ", "-");
            chartConfig.Add(key, new ChartSeriesConfig
            {
                Label = label,
                Color = ChartColor.GetDefault(i)
            });
        }
        return chartConfig;
    }

    public IEnumerator<KeyValuePair<string, ChartSeriesConfig>> GetEnumerator() =>
        _configs.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}

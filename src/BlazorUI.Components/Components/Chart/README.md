# BlazorUI Chart Components

A comprehensive charting library for BlazorUI built on [Blazor-ApexCharts](https://github.com/apexcharts/Blazor-ApexCharts), providing pre-configured chart components with shadcn/ui styling and theme integration.

## Overview

The chart components are **thin wrappers** around the ApexCharts library. They provide:

- Pre-configured options based on chart variants
- Theme integration via CSS custom properties
- Consistent styling matching the BlazorUI design system
- Simplified API for common use cases

Users still interact directly with ApexCharts for defining series data using `<ApexPointSeries>`.

## Architecture

```
Chart/
├── Configuration/
│   ├── ChartConfig.cs        # Series-to-label/color mapping
│   ├── ChartSeriesConfig.cs  # Individual series configuration
│   └── ChartColor.cs         # CSS variable color utilities
├── Core/
│   ├── ChartBase.cs          # Abstract base class for all charts
│   ├── ChartContainer.razor  # Styled container wrapper (Card-like)
│   └── ChartContainer.razor.cs
├── Enums/
│   ├── AreaChartVariant.cs
│   ├── BarChartVariant.cs
│   ├── LineChartVariant.cs
│   ├── PieChartVariant.cs
│   ├── RadarChartVariant.cs
│   ├── RadialChartVariant.cs
│   ├── LegendPosition.cs
│   └── TooltipIndicator.cs
└── Types/
    ├── AreaChart.razor/.cs
    ├── BarChart.razor/.cs
    ├── LineChart.razor/.cs
    ├── PieChart.razor/.cs
    ├── RadarChart.razor/.cs
    └── RadialChart.razor/.cs
```

### How It Works

Each chart component:

1. **Inherits from `ChartBase<TItem>`** - Provides common parameters and utilities
2. **Wraps `<ApexChart>`** - The underlying ApexCharts component
3. **Configures `ApexChartOptions`** - Based on the selected `Variant`
4. **Passes through `ChildContent`** - Where users define `<ApexPointSeries>` components

```razor
<!-- Inside BarChart.razor -->
<div class="@ContainerCssClass" data-slot="bar-chart">
    <ApexChart TItem="TItem"
               Options="@_options"
               Height="@Height"
               Width="@Width">
        @ChildContent  <!-- User-defined ApexPointSeries go here -->
    </ApexChart>
</div>
```

## Available Chart Types

### BarChart

Vertical and horizontal bars for comparing values across categories.

| Variant | Description |
|---------|-------------|
| `Vertical` | Default grouped vertical bars |
| `Horizontal` | Horizontal bars (good for long labels) |
| `Stacked` | Vertical stacked bars |
| `StackedHorizontal` | Horizontal stacked bars |
| `FullStacked` | 100% stacked vertical (percentage) |
| `FullStackedHorizontal` | 100% stacked horizontal |
| `Grouped` | Explicit grouped behavior |

### LineChart

Visualize trends and changes over time.

| Variant | Description |
|---------|-------------|
| `Default` | Straight lines connecting points |
| `Spline` | Smooth curved lines |
| `Stepline` | Step/staircase lines |
| `Dashed` | Dashed lines |
| `Gradient` | Lines with gradient fill beneath |

### AreaChart

Filled areas emphasizing volume and magnitude.

| Variant | Description |
|---------|-------------|
| `Default` | Standard area with gradient fill |
| `Spline` | Smooth curved area |
| `Stacked` | Multiple series stacked |
| `Stepline` | Step area chart |

### PieChart

Circular charts for proportions and part-to-whole relationships.

| Variant | Description |
|---------|-------------|
| `Pie` | Standard pie chart |
| `Donut` | Donut with hollow center |
| `GradientDonut` | Donut with gradient fills |

### RadarChart

Spider/web charts for comparing multiple variables.

| Variant | Description |
|---------|-------------|
| `Default` | Lines only |
| `PolygonFill` | Filled polygon area |
| `MultiSeries` | Multiple overlapping series |

### RadialChart

Circular gauges and progress indicators.

| Variant | Description |
|---------|-------------|
| `Default` | Full circle radial bar |
| `SemiCircle` | Half-circle gauge |
| `Gauge` | Speedometer style |
| `Gradient` | Radial bar with gradient |

## Usage

### Basic Example

```razor
@using ApexCharts
@using BlazorUI.Components.Chart

<BarChart TItem="SalesData"
          Items="@salesData"
          Variant="BarChartVariant.Stacked"
          Height="350px">
    <ApexPointSeries TItem="SalesData"
                     Items="@salesData"
                     Name="Desktop"
                     SeriesType="SeriesType.Bar"
                     XValue="@(item => item.Month)"
                     YValue="@(item => (decimal)item.Desktop)" />
    <ApexPointSeries TItem="SalesData"
                     Items="@salesData"
                     Name="Mobile"
                     SeriesType="SeriesType.Bar"
                     XValue="@(item => item.Month)"
                     YValue="@(item => (decimal)item.Mobile)" />
</BarChart>

@code {
    public class SalesData
    {
        public string Month { get; set; } = "";
        public int Desktop { get; set; }
        public int Mobile { get; set; }
    }

    private List<SalesData> salesData =
    [
        new() { Month = "Jan", Desktop = 186, Mobile = 80 },
        new() { Month = "Feb", Desktop = 305, Mobile = 200 },
        new() { Month = "Mar", Desktop = 237, Mobile = 120 }
    ];
}
```

### With ChartContainer

Use `ChartContainer` for a Card-like wrapper with consistent styling:

```razor
<ChartContainer>
    <div class="mb-4">
        <h3 class="text-lg font-medium">Monthly Sales</h3>
        <p class="text-sm text-muted-foreground">Desktop vs Mobile</p>
    </div>
    <BarChart TItem="SalesData" Items="@salesData" ...>
        ...
    </BarChart>
</ChartContainer>
```

### Pie/Donut Chart

```razor
<PieChart TItem="BrowserShare"
          Items="@browserData"
          Variant="PieChartVariant.Donut"
          CenterLabel="Total"
          CenterValue="100%"
          Height="350px">
    <ApexPointSeries TItem="BrowserShare"
                     Items="@browserData"
                     Name="Share"
                     SeriesType="SeriesType.Donut"
                     XValue="@(item => item.Browser)"
                     YValue="@(item => (decimal)item.Percentage)" />
</PieChart>
```

### Radial/Gauge Chart

```razor
<RadialChart TItem="ProgressData"
             Items="@progressData"
             Variant="RadialChartVariant.SemiCircle"
             CenterLabel="Complete"
             CenterValue="75%"
             Height="250px">
    <ApexPointSeries TItem="ProgressData"
                     Items="@progressData"
                     Name="Progress"
                     SeriesType="SeriesType.RadialBar"
                     XValue="@(item => item.Label)"
                     YValue="@(item => (decimal)item.Value)" />
</RadialChart>
```

## Common Parameters

All chart components inherit from `ChartBase<TItem>` and share these parameters:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Items` | `IEnumerable<TItem>?` | `null` | Data collection (passed to series) |
| `Height` | `string` | `"350px"` | Chart height (CSS value) |
| `Width` | `string` | `"100%"` | Chart width (CSS value) |
| `Title` | `string?` | `null` | Chart title |
| `ShowLegend` | `bool` | `true` | Display legend |
| `LegendPosition` | `LegendPosition` | `Bottom` | Legend placement |
| `ShowDataLabels` | `bool` | `false` | Show values on chart |
| `ShowTooltip` | `bool` | `true` | Enable tooltips |
| `EnableAnimations` | `bool` | `true` | Enable animations |
| `Config` | `ChartConfig?` | `null` | Series label/color mapping |
| `Class` | `string?` | `null` | Additional CSS classes |

### Chart-Specific Parameters

**BarChart:**
- `BorderRadius` (int, default: 4) - Corner radius for bars
- `BarWidth` (int, default: 70) - Bar width as percentage

**LineChart:**
- `StrokeWidth` (int, default: 2) - Line thickness
- `ShowMarkers` (bool, default: true) - Show data point markers
- `MarkerSize` (int, default: 4) - Marker size in pixels

**AreaChart:**
- `StrokeWidth` (int, default: 2) - Border line thickness
- `FillOpacity` (double, default: 0.4) - Area fill opacity

**PieChart:**
- `DonutSize` (int, default: 55) - Center hole size (%)
- `CenterLabel` (string?) - Text in donut center
- `CenterValue` (string?) - Value in donut center

**RadarChart:**
- `ShowMarkers` (bool, default: true) - Show vertex markers
- `MarkerSize` (int, default: 4) - Marker size
- `FillOpacity` (double, default: 0.25) - Fill opacity

**RadialChart:**
- `CenterLabel` (string?) - Center text label
- `CenterValue` (string?) - Center value display
- `StartAngle` (int?) - Custom start angle
- `EndAngle` (int?) - Custom end angle
- `HollowSize` (int, default: 55) - Center hollow size (%)
- `TrackBackground` (string) - Unfilled track color

## Theming

Charts integrate with BlazorUI's theme system via CSS custom properties:

```css
:root {
  --chart-1: oklch(0.646 0.222 41.116);   /* Coral/Orange */
  --chart-2: oklch(0.6 0.118 184.704);    /* Teal */
  --chart-3: oklch(0.398 0.07 227.392);   /* Dark Blue */
  --chart-4: oklch(0.828 0.189 84.429);   /* Gold */
  --chart-5: oklch(0.769 0.188 70.08);    /* Orange */
}

.dark {
  --chart-1: oklch(0.488 0.243 264.376);  /* Purple */
  --chart-2: oklch(0.696 0.17 162.48);    /* Green */
  --chart-3: oklch(0.769 0.188 70.08);    /* Orange */
  --chart-4: oklch(0.627 0.265 303.9);    /* Pink */
  --chart-5: oklch(0.645 0.246 16.439);   /* Red */
}
```

Charts automatically use these colors in order. To customize:

```razor
<BarChart TItem="SalesData" Items="@data" Config="@chartConfig">
    ...
</BarChart>

@code {
    private ChartConfig chartConfig = ChartConfig.Create(
        ("desktop", new ChartSeriesConfig
        {
            Label = "Desktop Users",
            Color = "var(--chart-1)"
        }),
        ("mobile", new ChartSeriesConfig
        {
            Label = "Mobile Users",
            Color = "var(--chart-2)"
        })
    );
}
```

## Series Type Mapping

When using `<ApexPointSeries>`, use the appropriate `SeriesType`:

| Chart Component | SeriesType |
|-----------------|------------|
| `BarChart` | `SeriesType.Bar` |
| `LineChart` | `SeriesType.Line` |
| `AreaChart` | `SeriesType.Area` |
| `PieChart` | `SeriesType.Pie` or `SeriesType.Donut` |
| `RadarChart` | `SeriesType.Radar` |
| `RadialChart` | `SeriesType.RadialBar` |

## Dependencies

- **Blazor-ApexCharts** (v4.0.0+) - The underlying charting library
- **ApexCharts.js** - Loaded automatically by Blazor-ApexCharts

## Limitations

1. **Direct ApexCharts API** - Users must understand `<ApexPointSeries>` for defining data
2. **No data binding abstraction** - Series are defined declaratively, not via a data model
3. **Limited customization via wrapper** - For advanced customization, access ApexCharts options directly

## Future Considerations

Potential enhancements:
- Higher-level data binding API (define series via parameters, not child content)
- Built-in series components (`<BarSeries>`, `<LineSeries>`, etc.)
- Chart templates for common dashboard patterns
- Export functionality (PNG, SVG, CSV)
- Real-time data streaming support

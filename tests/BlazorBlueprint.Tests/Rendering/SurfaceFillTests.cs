using System.Text.RegularExpressions;
using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives.DataGrid;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Xunit;
using StyledDataGrid = BlazorBlueprint.Components.BbDataGrid<BlazorBlueprint.Tests.Rendering.SurfaceFillTests.Row>;

namespace BlazorBlueprint.Tests.Rendering;

/// <summary>
/// Opaque fills behind sticky content take the colour of the surface they sit on.
/// <para>
/// A sticky header or a pinned column has to be opaque, or rows scroll through it. The grid used to
/// fill them with the page colour, so inside a card — whose colour differs from the page's in dark
/// mode — it showed as a darker box. Containers that paint their own background now declare it in
/// <c>--bb-surface</c>, and the grid fills with <c>bg-surface</c>, which reads it and falls back to
/// the page colour. The CSS itself needs a browser; what can break here is the markup: a container
/// that stops declaring the variable, or a grid fill that goes back to <c>bg-background</c>.
/// </para>
/// </summary>
public class SurfaceFillTests
{
    [Fact]
    public async Task ACardDeclaresItsColourAsTheSurface()
    {
        var markup = await RenderAsync<BbCard>(new()
        {
            [nameof(BbCard.ChildContent)] = (RenderFragment)(b => b.AddContent(0, "Body"))
        });

        // Also proves the class merge keeps an arbitrary-property class rather than dropping it.
        Assert.Contains("bb:[--bb-surface:var(--card)]", RootClass(markup), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ACardWithItsOwnClassesStillDeclaresTheSurface()
    {
        var markup = await RenderAsync<BbCard>(new()
        {
            [nameof(BbCard.Class)] = "p-6 shadow-none",
            [nameof(BbCard.ChildContent)] = (RenderFragment)(b => b.AddContent(0, "Body"))
        });

        Assert.Contains("bb:[--bb-surface:var(--card)]", RootClass(markup), StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheGridFillsItsHeaderBodyAndRowsWithTheSurface()
    {
        var markup = await RenderAsync<StyledDataGrid>(new()
        {
            [nameof(StyledDataGrid.Items)] = new[] { new Row(1, "One"), new Row(2, "Two") },
            [nameof(StyledDataGrid.ItemKey)] = (Func<Row, object>)(r => r.Id),
            [nameof(StyledDataGrid.StickyHeader)] = true,
            [nameof(StyledDataGrid.Columns)] = (RenderFragment)(builder =>
            {
                builder.OpenComponent<BbDataGridTemplateColumn<Row>>(0);
                builder.AddAttribute(1, "Id", "name");
                builder.AddAttribute(2, "Title", "Name");
                builder.AddAttribute(3, "Pinned", ColumnPinning.Left);
                builder.AddAttribute(4, "ChildContent", (RenderFragment<Row>)(row => b => b.AddContent(0, row.Name)));
                builder.CloseComponent();
            })
        });

        var fills = Regex.Matches(markup, @"<(thead|tbody|tr)\b[^>]*\bclass=""([^""]*)""")
            .Select(m => (Element: m.Groups[1].Value, Class: m.Groups[2].Value))
            .ToList();

        // The header, the body, the header row and both data rows.
        Assert.True(fills.Count >= 5, markup);
        Assert.All(fills, fill =>
        {
            Assert.Contains("bb:bg-surface", fill.Class, StringComparison.Ordinal);
            Assert.DoesNotContain("bb:bg-background", fill.Class, StringComparison.Ordinal);
        });
    }

    private static string RootClass(string markup)
    {
        var match = Regex.Match(markup, @"^\s*<div\b[^>]*\bclass=""([^""]*)""");
        Assert.True(match.Success, markup);
        return match.Groups[1].Value;
    }

    private static async Task<string> RenderAsync<T>(Dictionary<string, object?> parameters) where T : IComponent
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<IJSRuntime, NoopJavaScript>();
        services.AddBlazorBlueprintComponents();

        await using var provider = services.BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);

        await renderer.Dispatcher.InvokeAsync(async () => await renderer.MountAsync<T>(parameters));
        return renderer.Markup();
    }

    public sealed record Row(int Id, string Name);
}

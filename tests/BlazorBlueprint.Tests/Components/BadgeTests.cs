using BlazorBlueprint.Components;
using Bunit;

namespace BlazorBlueprint.Tests.Components;

public class BadgeTests
{
    [Fact]
    public void RendersContentVariantAndAdditionalAttributes()
    {
        using var context = new ComponentBunitContext();

        var badge = context.Render<BbBadge>(parameters => parameters
            .Add(p => p.Variant, BadgeVariant.Destructive)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                ["data-testid"] = "status"
            })
            .AddChildContent("Failed"));

        var element = badge.Find("[data-testid='status']");
        Assert.Equal("Failed", element.TextContent);
        Assert.Contains("bb:bg-destructive", element.ClassList);
    }

    [Fact]
    public void PlacesOptionalDotAtTheSelectedCorner()
    {
        using var context = new ComponentBunitContext();

        var badge = context.Render<BbBadge>(parameters => parameters
            .Add(p => p.ShowDot, true)
            .Add(p => p.DotPosition, BadgeDotPosition.BottomLeft)
            .AddChildContent("Updates"));

        Assert.Contains("bb:-bottom-1", badge.Find("span").ClassList);
        Assert.Contains("bb:-left-1", badge.Find("span").ClassList);
    }
}

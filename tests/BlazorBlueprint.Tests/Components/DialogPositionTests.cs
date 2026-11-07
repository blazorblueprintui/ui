using AngleSharp.Dom;
using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// A dialog sits in the centre unless it is told otherwise, and both <see cref="BbDialog.Position"/>
/// and <see cref="DialogOpenOptions.Position"/> move it (#612).
/// </summary>
public class DialogPositionTests
{
    private static readonly string[] Centred =
        ["bb:left-[50%]", "bb:top-[50%]", "bb:translate-x-[-50%]", "bb:translate-y-[-50%]", "bb:w-full"];

    private static readonly string[] OnTheRight =
        ["bb:left-auto", "bb:right-4", "bb:top-[50%]", "bb:translate-y-[-50%]", "bb:w-[calc(100%-2rem)]"];

    private static readonly string[] BottomLeftCorner =
        ["bb:left-4", "bb:top-auto", "bb:bottom-4", "bb:w-[calc(100%-2rem)]"];

    [Fact]
    public async Task ADialogWithNoPositionStaysCentred()
    {
        await using var context = CreateContext();

        var content = RenderOpenDialog(context, position: null);

        AssertClasses(content, Centred);
    }

    [Fact]
    public async Task ADialogMovesToThePositionItIsGiven()
    {
        await using var context = CreateContext();

        var content = RenderOpenDialog(context, DialogPosition.CenterRight);

        AssertClasses(content, OnTheRight);
        Assert.DoesNotContain("bb:translate-x-[-50%]", content.ClassList);
        Assert.DoesNotContain("bb:w-full", content.ClassList);
    }

    [Fact]
    public async Task AServiceDialogWithNoPositionStaysCentred()
    {
        await using var context = CreateContext();

        var container = OpenServiceDialog(context, new DialogOpenOptions());

        AssertClasses(container, Centred);
    }

    [Fact]
    public async Task AServiceDialogMovesToThePositionItIsGiven()
    {
        await using var context = CreateContext();

        var container = OpenServiceDialog(context, new DialogOpenOptions
        {
            Position = DialogPosition.BottomLeft,
            Size = DialogSize.Large
        });

        AssertClasses(container, BottomLeftCorner);
        Assert.Contains("bb:max-w-2xl", container.ClassList);
        Assert.DoesNotContain("bb:translate-y-[-50%]", container.ClassList);
    }

    private static IElement RenderOpenDialog(ComponentBunitContext context, DialogPosition? position)
    {
        var host = context.Render<BbPortalHost>();

        context.Render<BbDialog>(parameters =>
        {
            parameters.Add(p => p.DefaultOpen, true);
            if (position is { } value)
            {
                parameters.Add(p => p.Position, value);
            }

            parameters.AddChildContent<BbDialogContent>(content => content.AddChildContent("Body"));
        });

        return host.WaitForElement("[role='dialog']");
    }

    private static IElement OpenServiceDialog(ComponentBunitContext context, DialogOpenOptions options)
    {
        var provider = context.Render<BbDialogProvider>();
        var service = context.Services.GetRequiredService<DialogService>();

        provider.InvokeAsync(() => _ = service.OpenAsync<Body>(options));

        return provider.WaitForElement("[role='dialog']");
    }

    private static void AssertClasses(IElement element, string[] expected)
    {
        foreach (var name in expected)
        {
            Assert.Contains(name, element.ClassList);
        }
    }

    private static ComponentBunitContext CreateContext()
    {
        var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private sealed class Body : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, "Body");
    }
}

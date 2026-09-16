using BlazorBlueprint.Primitives.Menu;
using BlazorBlueprint.Tests.Performance;
using BlazorBlueprint.Tests.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace BlazorBlueprint.Tests.Menus;

public class MenuCompositionTests
{
    [Fact]
    public async Task RadioSelectionUpdatesOnceRespectsDisabledAndClosesTheRootOnRequest()
    {
        await using var provider = new ServiceCollection().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var closes = 0;
            var changes = new List<string>();
            var scope = new MenuScope(() => true, () => { closes++; return Task.CompletedTask; });
            RenderFragment content = b =>
            {
                b.OpenComponent<BbMenuRadioGroup<string>>(0);
                b.AddAttribute(1, "Value", "Comfortable");
                b.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(changes, changes.Add));
                b.AddAttribute(3, "ChildContent", (RenderFragment)(items =>
                {
                    items.OpenComponent<BbMenuRadioItem<string>>(0);
                    items.AddAttribute(1, "Value", "Compact");
                    items.AddAttribute(2, "CloseOnSelect", false);
                    items.CloseComponent();
                }));
                b.CloseComponent();
            };
            await MountScopeAsync(renderer, scope, content);
            var item = renderer.FindComponent<BbMenuRadioItem<string>>();
            var group = renderer.FindComponent<BbMenuRadioGroup<string>>();
            await (Task)ComponentProbe.Call(item, "SelectAsync")!;
            Assert.Equal(["Compact"], changes);
            Assert.Equal("Compact", group.Value);
            Assert.Equal(0, closes);
            await (Task)ComponentProbe.Call(item, "SelectAsync")!;
            Assert.Single(changes);
            await group.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(group.Disabled)] = true }));
            await item.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(item.Value)] = "Comfortable", [nameof(item.CloseOnSelect)] = true }));
            await (Task)ComponentProbe.Call(item, "SelectAsync")!;
            Assert.Single(changes);
            Assert.Equal(0, closes);
            await group.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(group.Disabled)] = false }));
            await item.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(item.CloseOnSelect)] = true }));
            await (Task)ComponentProbe.Call(item, "SelectAsync")!;
            Assert.Equal(1, closes);
        });
    }

    [Fact]
    public async Task OpeningASiblingClosesThePreviousSubmenuAndParentClosureResetsChildren()
    {
        await using var provider = new ServiceCollection().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var parentOpen = true;
            var scope = new MenuScope(() => parentOpen, () => Task.CompletedTask);
            BbMenuSub? first = null;
            BbMenuSub? second = null;
            RenderFragment content = b =>
            {
                b.OpenComponent<BbMenuSub>(0);
                b.AddComponentReferenceCapture(1, c => first = (BbMenuSub)c);
                b.CloseComponent();
                b.OpenComponent<BbMenuSub>(2);
                b.AddComponentReferenceCapture(3, c => second = (BbMenuSub)c);
                b.CloseComponent();
            };
            await MountScopeAsync(renderer, scope, content);
            first!.Open(true);
            Assert.True(first.IsOpen);
            second!.Open(false);
            Assert.False(first.IsOpen);
            Assert.False(first.RestoreFocus);
            Assert.True(second.IsOpen);
            second.Close(true);
            Assert.True(second.RestoreFocus);
            second.Open(true);
            parentOpen = false;
            await second.SetParametersAsync(ParameterView.Empty);
            parentOpen = true;
            Assert.False(second.IsOpen);
            await first.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(first.Disabled)] = true }));
            first.Open(true);
            Assert.False(first.IsOpen);
        });
    }

    private static Task<CascadingValue<MenuScope>> MountScopeAsync(ComponentTestRenderer renderer, MenuScope scope, RenderFragment content) =>
        renderer.MountAsync<CascadingValue<MenuScope>>(new()
        {
            [nameof(CascadingValue<MenuScope>.Value)] = scope,
            [nameof(CascadingValue<MenuScope>.ChildContent)] = content
        });
}

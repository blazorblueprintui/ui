using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using BlazorBlueprint.Components;
using BlazorBlueprint.Tests.Performance;
using BlazorBlueprint.Tests.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Xunit;

namespace BlazorBlueprint.Tests.Selection;

public class HierarchyPickerLifecycleTests
{
    [Theory]
    [InlineData(false, "root")]
    [InlineData(false, "leaf")]
    [InlineData(true, "leaf")]
    public async Task TreeKeyboardExpansionPreservesSelectionAndEnterCommitsAccordingToMode(bool multiple, string target)
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await renderer.MountAsync<BlazorBlueprint.Primitives.Services.BbPortalHost>(new());
            var picker = await renderer.MountAsync<BbTreeSelect<Node>>(new()
            {
                [nameof(BbTreeSelect<Node>.Items)] = Nodes,
                [nameof(BbTreeSelect<Node>.ValueField)] = (Func<Node, string>)(n => n.Id),
                [nameof(BbTreeSelect<Node>.TextField)] = (Func<Node, string>)(n => n.Text),
                [nameof(BbTreeSelect<Node>.ChildrenProperty)] = (Func<Node, IEnumerable<Node>>)(n => n.Children),
                [nameof(BbTreeSelect<Node>.Multiple)] = multiple,
                [nameof(BbTreeSelect<Node>.Value)] = "leaf"
            });
            ComponentProbe.Call(picker, "SetOpen", true);
            await picker.SetParametersAsync(ParameterView.Empty);
            var tree = renderer.FindComponent<BlazorBlueprint.Primitives.TreeView.BbTreeView>();
            Assert.Equal("true", tree.AdditionalAttributes!["data-tree-select"]);
            await tree.JsOnNodeExpand("root");
            await tree.JsOnNodeCollapse("root");
            await Task.Yield();
            Assert.True(ComponentProbe.Field<bool>(picker, "open"));
            Assert.Equal("leaf", picker.Value);
            Assert.Empty(picker.Values);

            if (multiple)
            {
                tree.JsOnNodeCheck(target);
                await Task.Yield();
                Assert.Contains(target, picker.Values);
                Assert.True(ComponentProbe.Field<bool>(picker, "open"));
                tree.JsOnNodeCheck(target);
                await Task.Yield();
                Assert.DoesNotContain(target, picker.Values);
                Assert.True(ComponentProbe.Field<bool>(picker, "open"));
            }
            else
            {
                await tree.JsOnNodeActivate(target, target == "root");
                await Task.Yield();
                Assert.Equal(target, picker.Value);
                Assert.False(ComponentProbe.Field<bool>(picker, "open"));
            }
        });
    }

    [Fact]
    public async Task TreeSearchStaysStableDuringCloseAndResetsOnReopen()
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var picker = await renderer.MountAsync<BbTreeSelect<Node>>(new()
            {
                [nameof(BbTreeSelect<Node>.Items)] = Nodes,
                [nameof(BbTreeSelect<Node>.ValueField)] = (Func<Node, string>)(n => n.Id),
                [nameof(BbTreeSelect<Node>.TextField)] = (Func<Node, string>)(n => n.Text),
                [nameof(BbTreeSelect<Node>.ChildrenProperty)] = (Func<Node, IEnumerable<Node>>)(n => n.Children)
            });
            ComponentProbe.Call(picker, "SetOpen", true);
            ComponentProbe.SetField(picker, "search", "Leaf");
            ComponentProbe.Call(picker, "SetOpen", false);
            Assert.False(ComponentProbe.Field<bool>(picker, "open"));
            Assert.Equal("Leaf", ComponentProbe.Field<string>(picker, "search"));
            ComponentProbe.Call(picker, "SetOpen", true);
            Assert.Empty(ComponentProbe.Field<string>(picker, "search"));
        });
    }

    [Theory]
    [InlineData(false, "")]
    [InlineData(true, "")]
    [InlineData(true, "Web")]
    public async Task ParentCheckboxCascadesAcrossCollapsedAndFilteredDescendants(bool leafOnly, string search)
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await renderer.MountAsync<BlazorBlueprint.Primitives.Services.BbPortalHost>(new());
            var changes = 0;
            var nodes = new Node[]
            {
                new("engineering", "Engineering", [new("platform", "Platform", [new("runtime", "Runtime", []), new("tools", "Tools", [])]), new("web", "Web", [])]),
                new("design", "Design", [new("brand", "Brand", [])])
            };
            var picker = await renderer.MountAsync<BbTreeSelect<Node>>(new()
            {
                [nameof(BbTreeSelect<Node>.Items)] = nodes,
                [nameof(BbTreeSelect<Node>.ValueField)] = (Func<Node, string>)(n => n.Id),
                [nameof(BbTreeSelect<Node>.TextField)] = (Func<Node, string>)(n => n.Text),
                [nameof(BbTreeSelect<Node>.ChildrenProperty)] = (Func<Node, IEnumerable<Node>>)(n => n.Children),
                [nameof(BbTreeSelect<Node>.Multiple)] = true,
                [nameof(BbTreeSelect<Node>.LeafOnly)] = leafOnly,
                [nameof(BbTreeSelect<Node>.Values)] = new HashSet<string> { "runtime", "brand" },
                [nameof(BbTreeSelect<Node>.ValuesChanged)] = EventCallback.Factory.Create<HashSet<string>>(new object(), _ => changes++)
            });
            ComponentProbe.Call(picker, "SetOpen", true);
            ComponentProbe.SetField(picker, "search", search);
            await picker.SetParametersAsync(ParameterView.Empty);
            var tree = renderer.FindComponent<BlazorBlueprint.Primitives.TreeView.BbTreeView>();
            var context = ComponentProbe.Field<BlazorBlueprint.Primitives.TreeView.TreeViewContext>(tree, "context");
            Assert.True(context.IsIndeterminate("engineering"));
            Assert.False(context.IsExpanded("platform"));
            var before = changes;

            tree.JsOnNodeCheck("engineering");
            await Task.Yield();
            Assert.Contains("runtime", picker.Values);
            Assert.Contains("tools", picker.Values);
            Assert.Contains("web", picker.Values);
            Assert.Contains("brand", picker.Values);
            Assert.Equal(!leafOnly, picker.Values.Contains("engineering"));
            Assert.True(context.IsChecked("platform"));
            Assert.True(context.IsChecked("engineering"));
            Assert.False(context.IsIndeterminate("engineering"));
            Assert.Equal(before + 1, changes);

            // Re-rendering the picker must preserve checked branch rollups in leaf-only mode.
            await picker.SetParametersAsync(ParameterView.Empty);
            tree.JsOnNodeCheck("web");
            await Task.Yield();
            Assert.DoesNotContain("web", picker.Values);
            Assert.Contains("runtime", picker.Values);
            Assert.True(context.IsIndeterminate("engineering"));
            tree.JsOnNodeCheck("engineering");
            await Task.Yield();
            tree.JsOnNodeCheck("engineering");
            await Task.Yield();
            Assert.DoesNotContain("runtime", picker.Values);
            Assert.DoesNotContain("tools", picker.Values);
            Assert.DoesNotContain("web", picker.Values);
            Assert.Contains("brand", picker.Values);
            Assert.False(context.IsIndeterminate("engineering"));
        });
    }

    [Fact]
    public async Task TreeSelectionEnforcesLeafOnlyDisabledAndReportsFieldChanges()
    {
        var services = Services();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var model = new Model();
            var editContext = new EditContext(model);
            using var validation = editContext.EnableDataAnnotationsValidation(provider);
            var changes = 0;
            var picker = await renderer.MountAsync<BbTreeSelect<Node>>(new()
            {
                [nameof(BbTreeSelect<Node>.Items)] = Nodes,
                [nameof(BbTreeSelect<Node>.ValueField)] = (Func<Node, string>)(n => n.Id),
                [nameof(BbTreeSelect<Node>.TextField)] = (Func<Node, string>)(n => n.Text),
                [nameof(BbTreeSelect<Node>.ChildrenProperty)] = (Func<Node, IEnumerable<Node>>)(n => n.Children),
                [nameof(BbTreeSelect<Node>.LeafOnly)] = true,
                [nameof(BbTreeSelect<Node>.ValueExpression)] = (Expression<Func<string?>>)(() => model.Value),
                [nameof(BbTreeSelect<Node>.ValueChanged)] = EventCallback.Factory.Create<string?>(new object(), value => { model.Value = value; changes++; })
            });
            typeof(BbTreeSelect<Node>).GetProperty("EditContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(picker, editContext);
            ComponentProbe.Call(picker, "OnParametersSet");
            await (Task)ComponentProbe.Call(picker, "SelectAsync", "root")!;
            Assert.Equal(0, changes);
            await (Task)ComponentProbe.Call(picker, "SelectAsync", "leaf")!;
            Assert.Equal("leaf", model.Value);
            Assert.True(editContext.IsModified());
            Assert.True(editContext.Validate());
            ComponentProbe.Call(picker, "SetOpen", true);
            var selectionChanges = changes;
            await (Task)ComponentProbe.Call(picker, "SelectAsync", "leaf")!;
            Assert.True(ComponentProbe.Field<bool>(picker, "open"));
            Assert.Equal(selectionChanges, changes);
            ComponentProbe.Call(picker, "NodeClicked", Nodes[0].Children[0]);
            Assert.False(ComponentProbe.Field<bool>(picker, "open"));
            await (Task)ComponentProbe.Call(picker, "ClearAsync")!;
            Assert.Null(model.Value);
            Assert.False(editContext.Validate());
            var before = changes;
            await picker.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(picker.Disabled)] = true }));
            await (Task)ComponentProbe.Call(picker, "SelectAsync", "leaf")!;
            Assert.Equal(before, changes);
        });
    }

    [Fact]
    public async Task CascaderDerivesPreselectedPathAndClearEmitsNull()
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var value = (string?)"leaf";
            var picker = await renderer.MountAsync<BbCascader<Node>>(new()
            {
                [nameof(BbCascader<Node>.Items)] = Nodes,
                [nameof(BbCascader<Node>.ValueField)] = (Func<Node, string>)(n => n.Id),
                [nameof(BbCascader<Node>.TextField)] = (Func<Node, string>)(n => n.Text),
                [nameof(BbCascader<Node>.ChildrenProperty)] = (Func<Node, IEnumerable<Node>>)(n => n.Children),
                [nameof(BbCascader<Node>.Value)] = value,
                [nameof(BbCascader<Node>.ValueChanged)] = EventCallback.Factory.Create<string?>(new object(), selected => value = selected)
            });
            Assert.Equal("Root / Leaf", ComponentProbe.Call(picker, "PathLabel", "leaf"));
            await (Task)ComponentProbe.Call(picker, "SelectAsync", null, true)!;
            Assert.Null(value);
            Assert.False(ComponentProbe.Field<bool>(picker, "open"));
        });
    }

    private static ServiceCollection Services()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<IJSRuntime, NoopJavaScript>();
        services.AddBlazorBlueprintComponents();
        return services;
    }

    private static readonly Node[] Nodes = [new("root", "Root", [new("leaf", "Leaf", [])])];
    public sealed record Node(string Id, string Text, Node[] Children);
    private sealed class Model
    {
        [Required] public string? Value { get; set; }
    }
}

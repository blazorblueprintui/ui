using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.TreeView;
using BlazorBlueprint.Tests.Components;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Xunit;
using StyledTreeItem = BlazorBlueprint.Components.BbTreeItem;
using StyledTreeView = BlazorBlueprint.Components.BbTreeView<string>;

namespace BlazorBlueprint.Tests.Performance;

/// <summary>
/// Selecting a node in a tree with thousands of siblings froze the page (#622): every node
/// re-rendered several times per click, and each render sorted its siblings to find its
/// position. These checks count renders and positions rather than timing the machine.
/// </summary>
public class TreeSelectionRenderTests
{
    private const int ChildCount = 200;

    [Fact]
    public void PositionsFollowSiblingOrderAfterRemovalAndReRegistration()
    {
        var context = new TreeViewContext();
        foreach (var value in new[] { "a", "b", "c", "d" })
        {
            context.RegisterNode(value, "root", 1, false);
        }

        context.UnregisterNode("b");
        Assert.Equal(1, context.GetPosInSet("a"));
        Assert.Equal(2, context.GetPosInSet("c"));
        Assert.Equal(3, context.GetPosInSet("d"));
        Assert.Equal(3, context.GetSetSize("c"));

        // Re-registering moves a node to the end, as the registration order always has.
        context.RegisterNode("a", "root", 1, false);
        context.RegisterNode("e", "root", 1, false);
        Assert.Equal(["c", "d", "a", "e"], context.GetChildValues("root"));
        Assert.Equal(1, context.GetPosInSet("c"));
        Assert.Equal(2, context.GetPosInSet("d"));
        Assert.Equal(3, context.GetPosInSet("a"));
        Assert.Equal(4, context.GetPosInSet("e"));
        Assert.Equal(0, context.GetPosInSet("b"));
    }

    [Fact]
    public void FirstEnabledRootSkipsDisabledRoots()
    {
        var context = new TreeViewContext();
        context.RegisterNode("r1", null, 0, true);
        context.RegisterNode("r2", null, 0, false);
        context.RegisterNode("r3", null, 0, false);

        Assert.True(context.IsFirstEnabledRoot("r2"));
        Assert.False(context.IsFirstEnabledRoot("r3"));

        context.UpdateNodeDisabled("r1", false);
        Assert.True(context.IsFirstEnabledRoot("r1"));
        Assert.False(context.IsFirstEnabledRoot("r2"));
    }

    [Fact]
    public async Task SelectingRedrawsOnlyTheNodesWhoseStateChanged()
    {
        using var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var rendered = new List<string>();

        var cut = context.Render<BbTreeView>(p => p
            .Add(x => x.SelectionMode, TreeSelectionMode.Single)
            .Add(x => x.DefaultExpandAll, true)
            .Add(x => x.ChildContent, PrimitiveTree(rendered)));
        var tree = cut.Instance;

        // Move focus off the root first: losing the roving tabindex re-renders the root's subtree.
        await cut.InvokeAsync(() => tree.JsOnNodeActivate("c1"));
        rendered.Clear();

        await cut.InvokeAsync(() => tree.JsOnNodeActivate("c5"));

        Assert.Equal(["c1", "c5"], rendered.Order());
        Assert.Equal("false", Node(cut, "c1").GetAttribute("aria-selected"));
        Assert.Equal("-1", Node(cut, "c1").GetAttribute("tabindex"));
        Assert.Equal("true", Node(cut, "c5").GetAttribute("aria-selected"));
        Assert.Equal("0", Node(cut, "c5").GetAttribute("tabindex"));
    }

    [Fact]
    public async Task BoundTreeRedrawsEachNodeInOnePassPerSelection()
    {
        using var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<BoundTreeHost>(p => p.Add(x => x.Count, ChildCount));
        var tree = cut.FindComponent<BbTreeView>().Instance;
        await cut.InvokeAsync(() => tree.JsOnNodeActivate("c1"));
        cut.Instance.Rendered.Clear();
        cut.Instance.ExpandedChanges = 0;

        await cut.InvokeAsync(() => tree.JsOnNodeActivate("c150"));

        Assert.Equal("c150", cut.Instance.Selected);
        Assert.Equal(0, cut.Instance.ExpandedChanges);
        // The bound SelectedValue re-renders the page, which redraws every node once; the tree
        // context's cascade can add a second render in the same batch. Before #622 each node
        // drew five times per click.
        Assert.Equal(ChildCount, cut.Instance.Rendered.Distinct().Count());
        Assert.All(cut.Instance.Rendered.GroupBy(v => v), g => Assert.InRange(g.Count(), 1, 2));
        Assert.Equal("true", Node(cut, "c150").GetAttribute("aria-selected"));
        Assert.Equal("false", Node(cut, "c1").GetAttribute("aria-selected"));
        Assert.Equal("151", Node(cut, "c150").GetAttribute("aria-posinset"));
        Assert.Equal(ChildCount.ToString(System.Globalization.CultureInfo.InvariantCulture), Node(cut, "c150").GetAttribute("aria-setsize"));
    }

    [Fact]
    public async Task ExpandingStillUpdatesTheBoundExpandedValues()
    {
        using var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<BoundTreeHost>(p => p.Add(x => x.Count, 3));
        var tree = cut.FindComponent<BbTreeView>().Instance;

        await cut.InvokeAsync(() => tree.JsOnNodeCollapse("root"));

        Assert.Equal(1, cut.Instance.ExpandedChanges);
        Assert.Empty(cut.Instance.Expanded);
        Assert.Equal("false", Node(cut, "root").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void RemovingASiblingRenumbersTheOnesAfterIt()
    {
        using var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<BoundTreeHost>(p => p.Add(x => x.Count, 4));
        Assert.Equal("3", Node(cut, "c2").GetAttribute("aria-posinset"));

        cut.Render(p => p.Add(x => x.Count, 4).Add(x => x.Skip, "c1"));

        Assert.Equal("2", Node(cut, "c2").GetAttribute("aria-posinset"));
        Assert.Equal("3", Node(cut, "c3").GetAttribute("aria-posinset"));
        Assert.Equal("3", Node(cut, "c3").GetAttribute("aria-setsize"));
    }

    private static AngleSharp.Dom.IElement Node<T>(IRenderedComponent<T> cut, string value) where T : IComponent =>
        cut.Find($"[role=treeitem][data-value='{value}']");

    private static RenderFragment PrimitiveTree(List<string> rendered) => builder =>
    {
        builder.OpenComponent<BbTreeItem>(0);
        builder.AddAttribute(1, nameof(BbTreeItem.Value), "root");
        builder.AddAttribute(2, nameof(BbTreeItem.ChildContent), (RenderFragment)(children =>
        {
            for (var i = 0; i < ChildCount; i++)
            {
                var value = "c" + i.ToString(System.Globalization.CultureInfo.InvariantCulture);
                children.OpenComponent<BbTreeItem>(0);
                children.SetKey(value);
                children.AddAttribute(1, nameof(BbTreeItem.Value), value);
                children.AddAttribute(2, nameof(BbTreeItem.NodeContent), (RenderFragment)(_ => rendered.Add(value)));
                children.CloseComponent();
            }
        }));
        builder.CloseComponent();
    };

    /// <summary>
    /// A page that binds SelectedValue and ExpandedValues on the styled tree, as #622's report does,
    /// and records which nodes drew their label.
    /// </summary>
    private sealed class BoundTreeHost : ComponentBase
    {
        [Parameter] public int Count { get; set; }
        [Parameter] public string? Skip { get; set; }

        public string? Selected { get; private set; }
        public HashSet<string> Expanded { get; private set; } = ["root"];
        public int ExpandedChanges { get; set; }
        public List<string> Rendered { get; } = [];

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<StyledTreeView>(0);
            builder.AddAttribute(1, nameof(StyledTreeView.SelectionMode), TreeSelectionMode.Single);
            builder.AddAttribute(2, nameof(StyledTreeView.SelectedValue), Selected);
            builder.AddAttribute(3, nameof(StyledTreeView.SelectedValueChanged), EventCallback.Factory.Create<string?>(this, v => Selected = v));
            builder.AddAttribute(4, nameof(StyledTreeView.ExpandedValues), Expanded);
            builder.AddAttribute(5, nameof(StyledTreeView.ExpandedValuesChanged), EventCallback.Factory.Create<HashSet<string>>(this, v =>
            {
                ExpandedChanges++;
                Expanded = v;
            }));
            builder.AddAttribute(6, nameof(StyledTreeView.ChildContent), (RenderFragment)(root =>
            {
                root.OpenComponent<StyledTreeItem>(0);
                root.AddAttribute(1, nameof(StyledTreeItem.Value), "root");
                root.AddAttribute(2, nameof(StyledTreeItem.Label), "root");
                root.AddAttribute(3, nameof(StyledTreeItem.ChildContent), (RenderFragment)(children =>
                {
                    for (var i = 0; i < Count; i++)
                    {
                        var value = "c" + i.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        if (value == Skip)
                        {
                            continue;
                        }

                        children.OpenComponent<StyledTreeItem>(0);
                        children.SetKey(value);
                        children.AddAttribute(1, nameof(StyledTreeItem.Value), value);
                        children.AddAttribute(2, nameof(StyledTreeItem.LabelTemplate), (RenderFragment)(_ => Rendered.Add(value)));
                        children.CloseComponent();
                    }
                }));
                root.CloseComponent();
            }));
            builder.CloseComponent();
        }
    }
}

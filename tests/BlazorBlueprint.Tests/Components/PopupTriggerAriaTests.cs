using AngleSharp.Dom;
using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// A trigger's <c>aria-controls</c> has to name an element that is there once its popup opens, and
/// <c>aria-haspopup</c> has to say what that element is. The popover trigger rendered its own
/// <c>aria-haspopup="true"</c> and <c>aria-controls</c> after the caller's attributes, so what
/// Combobox, MultiSelect, TreeSelect and Cascader asked for never reached the page. Combobox also
/// asked for <c>{Id}-listbox</c>, an id nothing rendered: its list takes the command's id.
/// </summary>
public class PopupTriggerAriaTests
{
    private static readonly SelectOption<string>[] Options = [new("a", "Alpha"), new("b", "Beta")];

    private static readonly Node[] Nodes = [new("root", "Root", [new("leaf", "Leaf", [])])];

    [Fact]
    public async Task ComboboxTriggerNamesTheListThatOpens()
    {
        await using var context = CreateContext();
        var host = context.Render<BbPortalHost>();

        var cut = context.Render<BbCombobox<string>>(parameters => parameters
            .Add(p => p.Options, Options)
            .Add(p => p.TriggerId, "fruit"));

        var list = OpenAndFindControlled(cut, host, "fruit", "listbox");
        Assert.Equal("listbox", list.GetAttribute("role"));
        Assert.Equal(Options.Length, list.QuerySelectorAll("[role='option']").Length);

        // The search input names the same list.
        Assert.Equal(list.Id, host.Find("input[role='combobox']").GetAttribute("aria-controls"));
    }

    [Fact]
    public async Task EachComboboxNamesItsOwnList()
    {
        await using var context = CreateContext();

        var first = context.Render<BbCombobox<string>>(parameters => parameters.Add(p => p.Options, Options));
        var second = context.Render<BbCombobox<string>>(parameters => parameters.Add(p => p.Options, Options));

        Assert.NotEqual(
            first.Find("button[role='combobox']").GetAttribute("aria-controls"),
            second.Find("button[role='combobox']").GetAttribute("aria-controls"));
    }

    [Fact]
    public async Task MultiSelectTriggerNamesTheListThatOpens()
    {
        await using var context = CreateContext();
        var host = context.Render<BbPortalHost>();

        var cut = context.Render<BbMultiSelect<string>>(parameters => parameters
            .Add(p => p.Options, Options)
            .Add(p => p.TriggerId, "fruits"));

        var list = OpenAndFindControlled(cut, host, "fruits", "listbox");
        Assert.Equal("listbox", list.GetAttribute("role"));
        Assert.Equal("true", list.GetAttribute("aria-multiselectable"));
    }

    [Fact]
    public async Task TreeSelectTriggerNamesTheTreeThatOpens()
    {
        await using var context = CreateContext();
        var host = context.Render<BbPortalHost>();

        var cut = context.Render<BbTreeSelect<Node>>(parameters => parameters
            .Add(p => p.Items, Nodes)
            .Add(p => p.ValueField, n => n.Id)
            .Add(p => p.TextField, n => n.Text)
            .Add(p => p.ChildrenProperty, n => n.Children)
            .Add(p => p.Id, "place"));

        var tree = OpenAndFindControlled(cut, host, "place", "tree");
        Assert.Equal("tree", tree.GetAttribute("role"));
    }

    [Fact]
    public async Task CascaderTriggerNamesTheDialogThatOpens()
    {
        await using var context = CreateContext();
        var host = context.Render<BbPortalHost>();

        var cut = context.Render<BbCascader<Node>>(parameters => parameters
            .Add(p => p.Items, Nodes)
            .Add(p => p.ValueField, n => n.Id)
            .Add(p => p.TextField, n => n.Text)
            .Add(p => p.ChildrenProperty, n => n.Children)
            .Add(p => p.Id, "region"));

        var dialog = OpenAndFindControlled(cut, host, "region", "dialog");
        Assert.Equal("dialog", dialog.GetAttribute("role"));
    }

    [Fact]
    public async Task APlainPopoverTriggerStillNamesItsContent()
    {
        await using var context = CreateContext();
        var host = context.Render<BbPortalHost>();

        var cut = context.Render<BbPopover>(parameters => parameters
            .Add(p => p.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenComponent<BbPopoverTrigger>(0);
                builder.AddComponentParameter(1, nameof(BbPopoverTrigger.AsChild), false);
                builder.AddComponentParameter(2, "id", "more");
                builder.CloseComponent();
                builder.OpenComponent<BbPopoverContent>(3);
                builder.CloseComponent();
            })));

        var content = OpenAndFindControlled(cut, host, "more", "true");
        Assert.Equal("dialog", content.GetAttribute("role"));
    }

    // The popover's portal loads its JS module on first render, open or not.
    private static ComponentBunitContext CreateContext()
    {
        var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    /// <summary>
    /// Opens the popup from the trigger with <paramref name="triggerId"/> and returns the one element
    /// its <c>aria-controls</c> names.
    /// </summary>
    private static IElement OpenAndFindControlled<TComponent>(
        IRenderedComponent<TComponent> cut, IRenderedComponent<BbPortalHost> host, string triggerId, string hasPopup)
        where TComponent : class, IComponent
    {
        var trigger = cut.Find($"#{triggerId}");
        Assert.Equal(hasPopup, trigger.GetAttribute("aria-haspopup"));
        var controlled = trigger.GetAttribute("aria-controls");
        Assert.False(string.IsNullOrEmpty(controlled));

        trigger.Click();

        var element = host.WaitForElement($"[id='{controlled}']");
        Assert.Single(host.FindAll($"[id='{controlled}']"));
        return element;
    }

    public sealed record Node(string Id, string Text, Node[] Children);
}

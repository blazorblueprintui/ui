using BlazorBlueprint.Components;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BlazorBlueprint.Tests.Performance;

#pragma warning disable BL0005
public class CommandFilteringTests
{
    [Theory]
    [InlineData(500)]
    [InlineData(1000)]
    public void SearchingEvaluatesEachItemOnce(int count)
    {
        var context = new CommandContext();
        var items = new List<BbCommandItem>();
        for (var i = 0; i < count; i++)
        {
            var index = context.RegisterItem($"item {i}", null, false, default);
            var component = new BbCommandItem { Context = context };
            ComponentProbe.SetField(component, "_index", index);
            items.Add(component);
        }
        var evaluations = 0;
        context.FilterFunction = (_, _) => { evaluations++; return true; };
        context.SetSearchQuery("item");
        foreach (var item in items)
        {
            ComponentProbe.Call(item, "UpdateVisibilityAndFilteredIndex");
        }
        // Parents recreate OnSelect closures while rendering. They do not change filtering.
        for (var i = 0; i < count; i++)
        {
            context.UpdateItem(i, $"item {i}", null, false,
                EventCallback.Factory.Create(new object(), () => { }));
            ComponentProbe.Call(items[i], "UpdateVisibilityAndFilteredIndex");
        }
        Assert.Equal(count, evaluations);
        Assert.Equal(count - 1, ComponentProbe.Field<int>(items[^1], "_filteredIndex"));
    }

    [Fact]
    public void InvalidatesOnSearchRegistrationMetadataAndFilterChanges()
    {
        var context = new CommandContext();
        var apple = context.RegisterItem("apple", null, false, default);
        context.SetSearchQuery("pear");
        Assert.Empty(context.GetFilteredItems());
        var pear = context.RegisterItem("pear", null, false, default);
        Assert.Equal("pear", Assert.Single(context.GetFilteredItems()).Value);
        context.UpdateItem(apple, "apple", "pear", false, default);
        Assert.Equal(2, context.GetFilteredItems().Count);
        context.UnregisterItem(pear);
        Assert.Equal("apple", Assert.Single(context.GetFilteredItems()).Value);
        context.SetSearchQuery("apple");
        Assert.Empty(context.GetFilteredItems());
        context.FilterFunction = (_, _) => true;
        Assert.Single(context.GetFilteredItems());
        context.FilterFunction = (_, _) => false;
        Assert.Empty(context.GetFilteredItems());
    }

    [Fact]
    public void PublicListsRemainCallerOwnedAndStableFilterDelegatesCanRefresh()
    {
        var context = new CommandContext();
        context.RegisterItem("a", null, false, default);
        context.GetFilteredItems().Clear();
        Assert.Single(context.GetFilteredItems());
        context.GetItemByIndex(0)!.SearchText = "updated";
        context.SetSearchQuery("updated");
        Assert.Single(context.GetFilteredItems());
        var include = true;
        Func<CommandItemMetadata, string, bool> filter = (_, _) => include;
        context.FilterFunction = filter;
        context.SetSearchQuery("x");
        Assert.Single(context.GetFilteredItems());
        include = false;
        context.FilterFunction = filter;
        Assert.Empty(context.GetFilteredItems());
    }
}
#pragma warning restore BL0005

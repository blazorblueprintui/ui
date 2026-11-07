using BlazorBlueprint.Components;
using Bunit;

namespace BlazorBlueprint.Tests.Components;

public class AccordionTests
{
    [Fact]
    public void TriggerTogglesItsPanelAndKeepsAriaReferencesValid()
    {
        using var context = new ComponentBunitContext();

        var cut = context.Render<BbAccordion>(parameters => parameters
            .Add(p => p.Collapsible, true)
            .AddChildContent<BbAccordionItem>(item => item
                .Add(p => p.Value, "details")
                .AddChildContent<BbAccordionTrigger>(trigger => trigger
                    .AddChildContent("Details"))
                .AddChildContent<BbAccordionContent>(content => content
                    .AddChildContent("Panel content"))));

        var triggerElement = cut.Find("button[aria-expanded]");
        var panel = cut.Find("[role='region']");
        Assert.Equal("false", triggerElement.GetAttribute("aria-expanded"));
        Assert.Equal(panel.Id, triggerElement.GetAttribute("aria-controls"));
        Assert.Equal(triggerElement.Id, panel.GetAttribute("aria-labelledby"));

        triggerElement.Click();

        Assert.Equal("true", cut.Find("button[aria-expanded]").GetAttribute("aria-expanded"));
        Assert.Equal("open", cut.Find("[role='region']").GetAttribute("data-state"));

        cut.Find("button[aria-expanded]").Click();

        Assert.Equal("false", cut.Find("button[aria-expanded]").GetAttribute("aria-expanded"));
        Assert.Equal("closed", cut.Find("[role='region']").GetAttribute("data-state"));
    }
}

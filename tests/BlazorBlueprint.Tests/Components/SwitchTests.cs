using BlazorBlueprint.Components;
using Bunit;

namespace BlazorBlueprint.Tests.Components;

public class SwitchTests
{
    [Fact]
    public void ClickUpdatesAriaStateAndReportsTheNewValue()
    {
        using var context = new ComponentBunitContext();
        var changes = new List<bool>();

        var cut = context.Render<BbSwitch>(parameters => parameters
            .Add(p => p.Checked, false)
            .Add(p => p.CheckedChanged, changes.Add)
            .Add(p => p.AriaLabel, "Notifications"));

        var button = cut.Find("button[role='switch']");
        Assert.Equal("false", button.GetAttribute("aria-checked"));
        Assert.Equal("Notifications", button.GetAttribute("aria-label"));

        button.Click();

        Assert.Equal([true], changes);
        Assert.Equal("true", cut.Find("button[role='switch']").GetAttribute("aria-checked"));
    }

    [Fact]
    public void DisabledSwitchDoesNotReportAChange()
    {
        using var context = new ComponentBunitContext();
        var changes = new List<bool>();

        var cut = context.Render<BbSwitch>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.CheckedChanged, changes.Add));

        var button = cut.Find("button[role='switch']");
        Assert.Equal("true", button.GetAttribute("aria-disabled"));
        Assert.True(button.HasAttribute("disabled"));

        button.Click();

        Assert.Empty(changes);
        Assert.Equal("false", cut.Find("button[role='switch']").GetAttribute("aria-checked"));
    }
}

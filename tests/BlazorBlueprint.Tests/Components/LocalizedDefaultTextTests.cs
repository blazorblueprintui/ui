using BlazorBlueprint.Components;
using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// Strings that used to be hard-coded English (or English parameter defaults) now resolve through
/// <see cref="IBbLocalizer"/>: the English output is unchanged by default and overriding a key changes it.
/// </summary>
public class LocalizedDefaultTextTests
{
    private static BunitContext CreateContext(Action<DefaultBbLocalizer>? configureLocalizer = null)
    {
        var context = new BunitContext();
        context.Services.AddBlazorBlueprintComponents(configureLocalizer);
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    [Fact]
    public void DefaultsRenderTheSameEnglishText()
    {
        using var context = CreateContext();

        var spinner = context.Render<BbSpinner>();
        Assert.Equal("Loading", spinner.Find("[role='status']").GetAttribute("aria-label"));

        var empty = context.Render<BbTimelineEmpty>();
        Assert.Equal("No timeline items to display", empty.Find("p").TextContent.Trim());

        var rating = context.Render<BbRating>(parameters => parameters.Add(p => p.Max, 5));
        Assert.Equal("2 out of 5", rating.FindAll("[role='radio']")[1].GetAttribute("aria-label"));

        var group = context.Render<BbCheckboxGroup<string>>(parameters => parameters
            .Add(p => p.ShowSelectAll, true));
        Assert.Contains("Select all", group.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void OverriddenKeysChangeTheRenderedText()
    {
        using var context = CreateContext(localizer =>
        {
            localizer.Set("Spinner.Loading", "Wird geladen");
            localizer.Set("Timeline.Empty", "Keine Einträge");
            localizer.Set("Rating.ItemLabel", "{0} von {1}");
            localizer.Set("CheckboxGroup.SelectAll", "Alle auswählen");
            localizer.Set("FormSection.Collapse", "Abschnitt einklappen");
        });

        var spinner = context.Render<BbSpinner>();
        Assert.Equal("Wird geladen", spinner.Find("[role='status']").GetAttribute("aria-label"));

        var empty = context.Render<BbTimelineEmpty>();
        Assert.Equal("Keine Einträge", empty.Find("p").TextContent.Trim());

        var rating = context.Render<BbRating>(parameters => parameters.Add(p => p.Max, 5));
        Assert.Equal("2 von 5", rating.FindAll("[role='radio']")[1].GetAttribute("aria-label"));

        var group = context.Render<BbCheckboxGroup<string>>(parameters => parameters
            .Add(p => p.ShowSelectAll, true));
        Assert.Contains("Alle auswählen", group.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Select all", group.Markup, StringComparison.Ordinal);

        var section = context.Render<BbFormSection>(parameters => parameters
            .Add(p => p.Collapsible, true)
            .Add(p => p.Title, "Details"));
        Assert.Contains("Abschnitt einklappen", section.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ExplicitParameterStillWinsOverLocalizedDefault()
    {
        using var context = CreateContext(localizer => localizer.Set("CheckboxGroup.SelectAll", "Alle auswählen"));

        var group = context.Render<BbCheckboxGroup<string>>(parameters => parameters
            .Add(p => p.ShowSelectAll, true)
            .Add(p => p.SelectAllLabel, "Select every permission"));

        Assert.Contains("Select every permission", group.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Alle auswählen", group.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ServiceDialogsRenderLocalizedButtonDefaults()
    {
        await using var context = CreateContext(localizer =>
        {
            localizer.Set("ConfirmDialog.Confirm", "Weiter");
            localizer.Set("PromptDialog.Cancel", "Abbrechen");
        });
        var provider = context.Render<BbDialogProvider>();
        var service = context.Services.GetRequiredService<DialogService>();

        _ = provider.InvokeAsync(() => _ = service.ConfirmAsync("Delete?"));
        provider.WaitForAssertion(() =>
        {
            var buttons = provider.FindAll("[role='alertdialog'] button, [role='dialog'] button")
                .Select(b => b.TextContent.Trim())
                .ToList();
            Assert.Contains("Weiter", buttons);
            Assert.Contains("Cancel", buttons);
        });
        _ = provider.InvokeAsync(() => service.CancelAll());

        _ = provider.InvokeAsync(() => _ = service.PromptAsync("Name?", options: new PromptDialogOptions { ConfirmText = "Save" }));
        provider.WaitForAssertion(() =>
        {
            var buttons = provider.FindAll("[role='alertdialog'] button, [role='dialog'] button")
                .Select(b => b.TextContent.Trim())
                .ToList();
            Assert.Contains("Save", buttons);
            Assert.Contains("Abbrechen", buttons);
        });
    }
}

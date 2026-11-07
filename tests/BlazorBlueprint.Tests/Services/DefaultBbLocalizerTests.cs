using BlazorBlueprint.Components;

namespace BlazorBlueprint.Tests.Services;

/// <summary>
/// <see cref="DefaultBbLocalizer.EnglishDefaults"/> lists every key with its built-in English text,
/// and overrides made with <see cref="DefaultBbLocalizer.Set"/> never leak into it (#638).
/// </summary>
public class DefaultBbLocalizerTests
{
    [Fact]
    public void EnglishDefaultsMatchWhatTheLocalizerReturns()
    {
        var localizer = new DefaultBbLocalizer();

        Assert.NotEmpty(DefaultBbLocalizer.EnglishDefaults);
        foreach (var (key, value) in DefaultBbLocalizer.EnglishDefaults)
        {
            Assert.Equal(value, localizer[key]);
        }
    }

    [Fact]
    public void SetOverridesOneInstanceWithoutChangingTheEnglishDefaults()
    {
        var english = DefaultBbLocalizer.EnglishDefaults["DataGrid.Loading"];
        var overridden = new DefaultBbLocalizer();

        overridden.Set("DataGrid.Loading", "Wird geladen…");

        Assert.Equal("Wird geladen…", overridden["DataGrid.Loading"]);
        Assert.Equal(english, DefaultBbLocalizer.EnglishDefaults["DataGrid.Loading"]);
        Assert.Equal(english, new DefaultBbLocalizer()["DataGrid.Loading"]);
    }

    [Fact]
    public void EnglishDefaultsCannotBeModified() =>
        Assert.Throws<NotSupportedException>(() =>
            ((IDictionary<string, string>)DefaultBbLocalizer.EnglishDefaults)["DataGrid.Loading"] = "changed");
}

using BlazorBlueprint.Components;
using Bunit;

namespace BlazorBlueprint.Tests.Components;

public class FormFieldCheckboxTests
{
    [Theory]
    [InlineData(false, "Choose an option")]
    [InlineData(true, "An option is required")]
    public void DescriptionPointsToRenderedHelperOrErrorText(bool invalid, string message)
    {
        using var context = new ComponentBunitContext();

        var cut = context.Render<BbFormFieldCheckbox>(parameters =>
        {
            if (invalid)
            {
                parameters.Add(p => p.ErrorText, message);
            }
            else
            {
                parameters.Add(p => p.HelperText, message);
            }
        });

        var describedBy = cut.Find("button[role='checkbox']").GetAttribute("aria-describedby");
        Assert.False(string.IsNullOrEmpty(describedBy));

        var description = cut.Find($"[id='{describedBy}']");
        Assert.Equal(message, description.TextContent);
    }
}

using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives.Barcode;
using BlazorBlueprint.Primitives.Gantt;
using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// Engine and validator messages that reach the page (#637 part c) are worded through
/// <see cref="IBbLocalizer"/>: the English is unchanged by default and overriding a key changes it.
/// </summary>
public class LocalizedEngineMessageTests
{
    private static BunitContext CreateContext(Action<DefaultBbLocalizer>? configureLocalizer = null)
    {
        var context = new BunitContext();
        context.Services.AddBlazorBlueprintComponents(configureLocalizer);
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    // ----- Barcode -----

    [Theory]
    [InlineData("", BarcodeType.Code128, BarcodeFormatError.Empty)]
    [InlineData("12a45", BarcodeType.Ean13, BarcodeFormatError.NotDigits)]
    [InlineData("12345", BarcodeType.Ean13, BarcodeFormatError.WrongLength)]
    [InlineData("5901234123459", BarcodeType.Ean13, BarcodeFormatError.WrongCheckDigit)]
    [InlineData("é", BarcodeType.Code128, BarcodeFormatError.Code128NotAscii)]
    [InlineData("a_b", BarcodeType.Code39, BarcodeFormatError.Code39InvalidCharacter)]
    [InlineData("not an isbn", BarcodeType.Isbn, BarcodeFormatError.InvalidIsbn)]
    [InlineData("1234", BarcodeType.Issn, BarcodeFormatError.InvalidIssn)]
    [InlineData("--", BarcodeType.Itf, BarcodeFormatError.ItfTooShort)]
    [InlineData("A12x4B", BarcodeType.Codabar, BarcodeFormatError.CodabarInvalidCharacter)]
    [InlineData("--", BarcodeType.Msi, BarcodeFormatError.MsiEmpty)]
    [InlineData("1234", BarcodeType.Postnet, BarcodeFormatError.PostnetWrongLength)]
    [InlineData("ab#", BarcodeType.Rm4scc, BarcodeFormatError.Rm4sccInvalidCharacter)]
    [InlineData("1", BarcodeType.Pharmacode, BarcodeFormatError.PharmacodeOutOfRange)]
    [InlineData("é", BarcodeType.Telepen, BarcodeFormatError.TelepenNotAscii)]
    public void EveryBarcodeRefusalCarriesItsReasonAndTheDefaultKeyRepeatsTheMessage(
        string value, BarcodeType type, BarcodeFormatError expected)
    {
        var error = Assert.Throws<BarcodeFormatException>(() => BarcodeEncoder.Encode(value, type));
        Assert.Equal(expected, error.Error);

        // The English key, formatted with the exception's arguments, is the exception's own text.
        var localizer = new DefaultBbLocalizer();
        var arguments = error.Arguments.ToList();
        if (expected == BarcodeFormatError.NotDigits)
        {
            arguments.Add(localizer[$"Barcode.Input{type}"]);
        }

        Assert.Equal(error.Message, localizer[$"Barcode.{expected}", [.. arguments]]);
    }

    [Fact]
    public void EveryBarcodeInputDescriptionHasAKeyMatchingDescribeInput()
    {
        var localizer = new DefaultBbLocalizer();
        foreach (var type in Enum.GetValues<BarcodeType>())
        {
            Assert.Equal(BarcodeEncoder.DescribeInput(type), localizer[$"Barcode.Input{type}"]);
        }
    }

    [Fact]
    public void EveryBarcodeReasonHasAKey()
    {
        foreach (var error in Enum.GetValues<BarcodeFormatError>().Where(e => e != BarcodeFormatError.Unspecified))
        {
            Assert.True(DefaultBbLocalizer.EnglishDefaults.ContainsKey($"Barcode.{error}"), error.ToString());
        }
    }

    [Fact]
    public void BarcodeShowsTheSameEnglishByDefault()
    {
        using var context = CreateContext();

        var cut = context.Render<BbBarcode>(parameters => parameters
            .Add(p => p.Value, "12a45")
            .Add(p => p.Type, BarcodeType.Ean13));

        Assert.Equal(
            "Ean13 holds digits only, and this value contains 'a'. 12 digits, or 13 with the check digit.",
            cut.Find("[role='alert']").TextContent);
    }

    [Fact]
    public void BarcodeShowsTheOverriddenKeys()
    {
        using var context = CreateContext(localizer =>
        {
            localizer.Set("Barcode.NotDigits", "{0} nimmt nur Ziffern, nicht '{1}'. {2}");
            localizer.Set("Barcode.InputEan13", "12 oder 13 Ziffern.");
            localizer.Set("Barcode.WrongLength", "{0}: {1} oder {2} Ziffern, nicht {3}.");
        });

        var cut = context.Render<BbBarcode>(parameters => parameters
            .Add(p => p.Value, "12a45")
            .Add(p => p.Type, BarcodeType.Ean13));
        Assert.Equal("Ean13 nimmt nur Ziffern, nicht 'a'. 12 oder 13 Ziffern.", cut.Find("[role='alert']").TextContent);

        cut.Render(parameters => parameters.Add(p => p.Value, "12345"));
        Assert.Equal("Ean13: 12 oder 13 Ziffern, nicht 5.", cut.Find("[role='alert']").TextContent);
    }

    // ----- Gantt -----

    private sealed record Job(string Id, string? Parent, string Name);

    private static IRenderedComponent<BbGantt<Job>> RenderGantt(BunitContext context, IEnumerable<Job> jobs)
    {
        var start = new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero);
        return context.Render<BbGantt<Job>>(parameters => parameters
            .Add(p => p.Data, jobs)
            .Add(p => p.IdSelector, j => j.Id)
            .Add(p => p.ParentIdSelector, j => j.Parent)
            .Add(p => p.TextSelector, j => j.Name)
            .Add(p => p.StartSelector, _ => start)
            .Add(p => p.EndSelector, _ => start.AddDays(2))
            .Add(p => p.ScrollToToday, false));
    }

    [Fact]
    public void GanttRefusalsCarryTheirReason()
    {
        var start = new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero);
        GanttSource<Job> Source(params Job[] jobs) => new()
        {
            Items = jobs,
            Id = j => j.Id,
            ParentId = j => j.Parent,
            Text = j => j.Name,
            Start = _ => start,
            End = _ => start.AddDays(1),
        };

        var missing = Assert.Throws<GanttBuildException>(() => GanttBuilder.Build(Source(new Job("", null, "A"))));
        Assert.Equal(GanttBuildError.MissingId, missing.Error);
        Assert.Empty(missing.Arguments);

        var duplicate = Assert.Throws<GanttBuildException>(
            () => GanttBuilder.Build(Source(new Job("a", null, "A"), new Job("a", null, "B"))));
        Assert.Equal(GanttBuildError.DuplicateId, duplicate.Error);
        Assert.Equal(["a"], duplicate.Arguments);
        Assert.StartsWith("Two tasks share the identifier 'a'.", duplicate.Message, StringComparison.Ordinal);

        var loop = Assert.Throws<GanttBuildException>(
            () => GanttBuilder.Build(Source(new Job("a", "b", "A"), new Job("b", "a", "B"))));
        Assert.Equal(GanttBuildError.ParentLoop, loop.Error);

        // Still an ArgumentException, so existing catch blocks hold.
        Assert.IsAssignableFrom<ArgumentException>(loop);
    }

    [Fact]
    public void GanttShowsTheSameEnglishByDefault()
    {
        using var context = CreateContext();

        var cut = RenderGantt(context, [new Job("a", null, "A"), new Job("a", null, "B")]);
        Assert.Equal("Two tasks share the identifier 'a'.", cut.Find("[role='alert']").TextContent);

        cut = RenderGantt(context, [new Job("a", "b", "A"), new Job("b", "a", "B")]);
        Assert.Equal("The parent identifiers form a loop.", cut.Find("[role='alert']").TextContent);
    }

    [Fact]
    public void GanttShowsTheOverriddenKeys()
    {
        using var context = CreateContext(localizer =>
        {
            localizer.Set("Gantt.DuplicateId", "Zwei Aufgaben teilen die Kennung '{0}'.");
            localizer.Set("Gantt.MissingId", "Jede Aufgabe braucht eine Kennung.");
        });

        var cut = RenderGantt(context, [new Job("a", null, "A"), new Job("a", null, "B")]);
        Assert.Equal("Zwei Aufgaben teilen die Kennung 'a'.", cut.Find("[role='alert']").TextContent);

        cut = RenderGantt(context, [new Job("", null, "A")]);
        Assert.Equal("Jede Aufgabe braucht eine Kennung.", cut.Find("[role='alert']").TextContent);
    }

    // ----- PivotDataGrid -----

    [Fact]
    public void PivotWithNothingToBuildShowsItsEmptyStateRatherThanTheBuilderMessage()
    {
        // BbPivotDataGrid checks for measures and fields before it calls PivotBuilder, so the
        // builder's two English refusals never reach the page: the localized empty state does.
        using var context = CreateContext(localizer => localizer.Set("PivotDataGrid.Empty", "Nichts"));

        var cut = context.Render<BbPivotDataGrid<Job>>(parameters => parameters
            .Add(p => p.Data, [new Job("a", null, "A")]));

        Assert.Empty(cut.FindAll("[role='alert']"));
        Assert.Contains("Nichts", cut.Markup, StringComparison.Ordinal);
    }

    // ----- FormFieldInput -----

    private static string ParseErrorText<TValue>(BunitContext context, string input, Action<ComponentParameterCollectionBuilder<BbFormFieldInput<TValue>>>? configure = null)
    {
        var cut = context.Render<BbFormFieldInput<TValue>>(parameters =>
        {
            parameters.Add(p => p.Label, "Field");
            configure?.Invoke(parameters);
        });

        var field = cut.FindComponent<BbInputField<TValue>>();
        cut.InvokeAsync(() => field.Instance.JsOnChange(input)).GetAwaiter().GetResult();

        return cut.Find("[data-slot='field-error']").TextContent.Trim();
    }

    [Fact]
    public void FormFieldInputShowsTheSameEnglishByDefault()
    {
        using var context = CreateContext();

        Assert.Equal("'abc' is not a valid whole number.", ParseErrorText<int>(context, "abc"));
        Assert.Equal("'abc' is not a valid date.", ParseErrorText<DateOnly>(context, "abc"));
        Assert.Equal(
            "'abc' does not match the required format.",
            ParseErrorText<string>(context, "abc", p => p.Add(x => x.ValidationPattern, "^[0-9]+$")));
        Assert.Equal(
            "The entered value is not valid.",
            ParseErrorText<int>(context, "5", p => p.Add(x => x.Validation, v => v > 10)));
    }

    [Fact]
    public void FormFieldInputShowsTheOverriddenKeys()
    {
        using var context = CreateContext(localizer =>
        {
            localizer.Set("FormField.ParseError", "'{0}' ist keine gültige {1}.");
            localizer.Set("FormField.TypeWholeNumber", "Ganzzahl");
            localizer.Set("FormField.ValueError", "Ungültiger Wert.");
        });

        Assert.Equal("'abc' ist keine gültige Ganzzahl.", ParseErrorText<int>(context, "abc"));
        Assert.Equal(
            "Ungültiger Wert.",
            ParseErrorText<int>(context, "5", p => p.Add(x => x.Validation, v => v > 10)));
    }

    [Fact]
    public void FormFieldInputErrorTextStillWins()
    {
        using var context = CreateContext(localizer => localizer.Set("FormField.ParseError", "übersetzt"));

        Assert.Equal("Custom", ParseErrorText<int>(context, "abc", p => p.Add(x => x.ErrorText, "Custom")));
    }

    // ----- DynamicForm -----

    private static FormSchema Schema(params FieldValidation[] validations) => new()
    {
        Fields =
        [
            new FormFieldDefinition { Name = "name", Label = "Name", Required = true },
            new FormFieldDefinition { Name = "code", Label = "Code", Validations = [.. validations] },
        ],
    };

    private static IReadOnlyList<string> SubmitErrors(BunitContext context, FormSchema schema, Dictionary<string, object?> values)
    {
        var cut = context.Render<BbDynamicForm>(parameters => parameters
            .Add(p => p.Schema, schema)
            .Add(p => p.Values, values)
            .Add(p => p.ShowValidationSummary, true));

        cut.Find("form").Submit();

        return [.. cut.FindAll("[role='alert'] li").Select(li => li.TextContent.Trim())];
    }

    [Fact]
    public void DynamicFormShowsTheSameEnglishByDefault()
    {
        using var context = CreateContext();

        var errors = SubmitErrors(
            context,
            Schema(new FieldValidation { Type = ValidationType.MinLength, Value = 5 }),
            new() { ["code"] = "abc" });

        Assert.Equal(["Name is required.", "Code must be at least 5 characters."], errors);

        errors = SubmitErrors(
            context,
            Schema(new FieldValidation { Type = ValidationType.Pattern, Value = "^[0-9]+$" }),
            new() { ["name"] = "Ada", ["code"] = "abc" });

        Assert.Equal(["Code does not match the required format."], errors);
    }

    [Fact]
    public void DynamicFormShowsTheOverriddenKeysAndARuleMessageStillWins()
    {
        using var context = CreateContext(localizer =>
        {
            localizer.Set("DynamicForm.Required", "{0} ist erforderlich.");
            localizer.Set("DynamicForm.MinLength", "{0} braucht mindestens {1} Zeichen.");
        });

        var errors = SubmitErrors(
            context,
            Schema(new FieldValidation { Type = ValidationType.MinLength, Value = 5 }),
            new() { ["code"] = "abc" });

        Assert.Equal(["Name ist erforderlich.", "Code braucht mindestens 5 Zeichen."], errors);

        errors = SubmitErrors(
            context,
            Schema(new FieldValidation { Type = ValidationType.MinLength, Value = 5, Message = "Too short" }),
            new() { ["name"] = "Ada", ["code"] = "abc" });

        Assert.Equal(["Too short"], errors);
    }
}

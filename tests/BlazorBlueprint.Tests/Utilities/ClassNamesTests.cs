using BlazorBlueprint.Components;
using Xunit;

namespace BlazorBlueprint.Tests.Utilities;

public class ClassNamesTests
{
    [Fact]
    public void BasicConcatenation() =>
        Assert.Equal("a b c", ClassNames.cn("a", "b", "c"));

    [Fact]
    public void NullHandling() =>
        Assert.Equal("a b c", ClassNames.cn("a", null, "b", null, "c"));

    [Fact]
    public void EmptyStringHandling() =>
        Assert.Equal("a b c", ClassNames.cn("a", "", "b", "  ", "c"));

    [Fact]
    public void ConditionalFalse() =>
        Assert.Equal("btn px-4", ClassNames.cn("btn", false ? "active" : null, "px-4"));

    [Fact]
    public void ConditionalTrue() =>
        Assert.Equal("btn active px-4", ClassNames.cn("btn", true ? "active" : null, "px-4"));

    [Fact]
    public void ArraySupport()
    {
        string[] abc = ["a", "b", "c"];
        Assert.Equal("a b c", ClassNames.cn(abc));
    }

    [Fact]
    public void MixedArraysAndStrings()
    {
        string[] bc = ["b", "c"];
        Assert.Equal("a b c d", ClassNames.cn("a", bc, "d"));
    }

    [Fact]
    public void TailwindConflictPadding() =>
        Assert.Equal("px-2", ClassNames.cn("px-4", "px-2"));

    [Fact]
    public void TailwindConflictLonghandRefinesShorthand() =>
        Assert.Equal("p-4 px-2 py-6", ClassNames.cn("p-4", "px-2", "py-6"));

    [Fact]
    public void TailwindConflictShorthandOverridesLonghands() =>
        Assert.Equal("p-4", ClassNames.cn("px-2", "py-6", "p-4"));

    [Fact]
    public void TailwindConflictTextColor() =>
        Assert.Equal("text-red-600", ClassNames.cn("text-blue-500", "text-red-600"));

    [Fact]
    public void TailwindConflictBackground() =>
        Assert.Equal("bg-gray-100", ClassNames.cn("bg-white", "bg-gray-100"));

    [Fact]
    public void NoConflictDifferentUtilities() =>
        Assert.Equal(
            "px-4 py-2 bg-white text-black",
            ClassNames.cn("px-4", "py-2", "bg-white", "text-black"));

    [Fact]
    public void ComplexRealWorldExample() =>
        Assert.Equal(
            "inline-flex items-center justify-center rounded-md text-sm font-medium py-2 bg-primary text-primary-foreground px-8",
            ClassNames.cn(
                "inline-flex items-center justify-center",
                "rounded-md text-sm font-medium",
                "px-4 py-2",
                true ? "bg-primary text-primary-foreground" : null,
                false ? "bg-secondary" : null,
                "px-8"));

    [Fact]
    public void DisplayConflicts() =>
        Assert.Equal("grid", ClassNames.cn("block", "flex", "inline-block", "grid"));

    [Fact]
    public void MultiWordClassesInString() =>
        Assert.Equal("py-2 bg-white px-8", ClassNames.cn("px-4 py-2", "bg-white", "px-8"));

    [Fact]
    public void ArbitraryFontSizeDoesNotConflictWithTextColor() =>
        Assert.Equal(
            "text-white text-[.5rem]",
            ClassNames.cn("text-primary-foreground", "text-white", "text-[.5rem]"));

    [Fact]
    public void ArbitraryColorValueConflictsWithTextColor() =>
        Assert.Equal("text-[#ff0]", ClassNames.cn("text-white", "text-[#ff0]"));

    [Fact]
    public void ArbitraryCalcValueIsNonColor() =>
        Assert.Equal(
            "text-red-500 text-[calc(1rem+2px)]",
            ClassNames.cn("text-red-500", "text-[calc(1rem+2px)]"));

    // Library tokens carry the `bb:` prefix (#496). A consumer's unprefixed token must still
    // replace the library's for the same property, and library tokens must still merge among
    // themselves exactly as unprefixed ones do.

    [Fact]
    public void ConsumerTokenReplacesPrefixedLibraryToken() =>
        Assert.Equal("bb:rounded-md p-6", ClassNames.cn("bb:p-4 bb:rounded-md", "p-6"));

    [Fact]
    public void ConsumerTokenReplacesPrefixedLibraryTokenMidString() =>
        Assert.Equal("bb:flex bb:items-center gap-4", ClassNames.cn("bb:flex bb:items-center bb:gap-2", "gap-4"));

    [Fact]
    public void PrefixedTokensMergeAmongThemselves() =>
        Assert.Equal("bb:p-8", ClassNames.cn("bb:p-4", "bb:p-8"));

    [Fact]
    public void PrefixedShorthandOverridesPrefixedLonghand() =>
        Assert.Equal("bb:p-2", ClassNames.cn("bb:px-4", "bb:p-2"));

    [Fact]
    public void PrefixedVariantsMergePerVariant() =>
        Assert.Equal("bb:sm:p-8", ClassNames.cn("bb:sm:p-4", "bb:sm:p-8"));

    [Fact]
    public void PrefixedTokenPassedAsClassReplacesPrefixedBase() =>
        Assert.Equal("bb:h-8", ClassNames.cn("bb:h-9", "bb:h-8"));

    [Fact]
    public void SameTokenOnBothSidesKeepsTheLaterForm() =>
        Assert.Equal("p-4", ClassNames.cn("bb:p-4", "p-4"));

    [Fact]
    public void PrefixedGroupMarkerSurvivesConsumerOverride() =>
        Assert.Equal("bb:group/row bg-muted", ClassNames.cn("bb:group/row bb:bg-background", "bg-muted"));

    [Fact]
    public void PrefixedArbitraryVariantIsKept() =>
        Assert.Equal(
            "bb:[&_svg]:shrink-0 bb:[&_svg]:size-4 text-sm",
            ClassNames.cn("bb:[&_svg]:shrink-0 bb:[&_svg]:size-4 bb:text-xs", "text-sm"));

    [Fact]
    public void PrefixedImportantTokensMerge() =>
        Assert.Equal("bb:!p-2", ClassNames.cn("bb:!p-0", "bb:!p-2"));

    [Fact]
    public void ConsumerOnlyInputIsUntouchedByPrefixHandling() =>
        Assert.Equal("btn px-2", ClassNames.cn("btn", "px-4", "px-2"));
}

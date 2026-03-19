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
}

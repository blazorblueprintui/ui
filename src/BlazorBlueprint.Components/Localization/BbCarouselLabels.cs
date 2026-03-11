using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for Carousel components.
/// </summary>
public sealed class BbCarouselLabels
{
    [DisallowNull] public string NextSlide { get; set; } = "Next slide";
    [DisallowNull] public string PreviousSlide { get; set; } = "Previous slide";
}

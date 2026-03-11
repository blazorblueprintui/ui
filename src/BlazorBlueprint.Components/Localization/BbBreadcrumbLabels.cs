using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for Breadcrumb components.
/// </summary>
public sealed class BbBreadcrumbLabels
{
    [DisallowNull] public string Breadcrumb { get; set; } = "breadcrumb";
    [DisallowNull] public string More { get; set; } = "More";
}

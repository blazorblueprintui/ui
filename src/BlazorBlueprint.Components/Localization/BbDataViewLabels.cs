using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the DataView component.
/// </summary>
public sealed class BbDataViewLabels
{
    [DisallowNull] public string SearchPlaceholder { get; set; } = "Search...";
    [DisallowNull] public string NoResultsFound { get; set; } = "No results found";
    [DisallowNull] public string Loading { get; set; } = "Loading...";
    [DisallowNull] public string LoadingMore { get; set; } = "Loading more...";
    [DisallowNull] public string LoadMore { get; set; } = "Load more";
    [DisallowNull] public string ListView { get; set; } = "List view";
    [DisallowNull] public string GridView { get; set; } = "Grid view";
    [DisallowNull] public string Sort { get; set; } = "Sort";
}

using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for DashboardGrid components.
/// </summary>
public sealed class BbDashboardGridLabels
{
    [DisallowNull] public string Loading { get; set; } = "Loading dashboard";
    [DisallowNull] public string NoWidgets { get; set; } = "No widgets to display";
    [DisallowNull] public string NoWidgetsDescription { get; set; } = "Get started by adding your first widget.";
    [DisallowNull] public string AddWidget { get; set; } = "Add Widget";
    [DisallowNull] public string RemoveWidget { get; set; } = "Remove widget";
    [DisallowNull] public string ResizeWidget { get; set; } = "Resize widget";
}

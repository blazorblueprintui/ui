using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for Command components.
/// </summary>
public sealed class BbCommandLabels
{
    [DisallowNull] public string CommandMenu { get; set; } = "Command menu";
    [DisallowNull] public string CommandList { get; set; } = "Command list";
}

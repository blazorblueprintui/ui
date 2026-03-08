using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Primitives.Toggle;

/// <summary>
/// Defines the selection mode of a toggle group.
/// </summary>
[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Single is a domain term for selection mode")]
public enum ToggleGroupType
{
    /// <summary>
    /// Only one toggle can be active at a time.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple toggles can be active simultaneously.
    /// </summary>
    Multiple
}

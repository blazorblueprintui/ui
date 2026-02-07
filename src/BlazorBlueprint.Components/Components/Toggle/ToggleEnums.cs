using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components.Toggle;

public enum ToggleVariant
{
    Default,
    Outline
}

public enum ToggleSize
{
    Small,
    Default,
    Large
}

[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Single is a domain term for selection mode")]
public enum ToggleGroupType
{
    Single,
    Multiple
}

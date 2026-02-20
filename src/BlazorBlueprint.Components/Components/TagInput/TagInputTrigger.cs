namespace BlazorBlueprint.Components;

/// <summary>
/// Defines which keys trigger tag creation in a BbTagInput component.
/// Multiple triggers can be combined using bitwise OR.
/// </summary>
[Flags]
public enum TagInputTrigger
{
    /// <summary>Enter key adds the current input as a tag.</summary>
    Enter = 1,

    /// <summary>Comma key adds the current input as a tag.</summary>
    Comma = 2,

    /// <summary>Space key adds the current input as a tag.</summary>
    Space = 4,

    /// <summary>Tab key adds the current input as a tag.</summary>
    Tab = 8,

    /// <summary>Semicolon key adds the current input as a tag.</summary>
    Semicolon = 16
}

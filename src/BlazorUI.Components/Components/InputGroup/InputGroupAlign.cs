namespace BlazorUI.Components.InputGroup;

/// <summary>
/// Defines the alignment positions for InputGroup addons.
/// </summary>
/// <remarks>
/// <para>
/// InputGroupAlign specifies where addon content should be positioned
/// relative to the input control within an InputGroup.
/// </para>
/// <para>
/// Alignment options:
/// - InlineStart: Positioned at the start (left in LTR, right in RTL) of the input
/// - InlineEnd: Positioned at the end (right in LTR, left in RTL) of the input
/// - BlockStart: Positioned above the input
/// - BlockEnd: Positioned below the input
/// </para>
/// </remarks>
public enum InputGroupAlign
{
    /// <summary>
    /// Position addon at the inline start (left in LTR, right in RTL).
    /// </summary>
    /// <remarks>
    /// Common use cases: search icons, currency symbols, URL protocols.
    /// </remarks>
    InlineStart,

    /// <summary>
    /// Position addon at the inline end (right in LTR, left in RTL).
    /// </summary>
    /// <remarks>
    /// Common use cases: action buttons, validation icons, clear buttons.
    /// </remarks>
    InlineEnd,

    /// <summary>
    /// Position addon above the input (block start).
    /// </summary>
    /// <remarks>
    /// Common use cases: labels, descriptions, field titles.
    /// </remarks>
    BlockStart,

    /// <summary>
    /// Position addon below the input (block end).
    /// </summary>
    /// <remarks>
    /// Common use cases: character counters, help text, validation messages.
    /// </remarks>
    BlockEnd
}

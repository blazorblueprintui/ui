namespace BlazorBlueprint.Components;

/// <summary>
/// Defines inline alignment options for bubble rows.
/// </summary>
public enum BubbleAlign
{
    /// <summary>
    /// Align bubble to the start.
    /// </summary>
    Start,

    /// <summary>
    /// Align bubble to the end.
    /// </summary>
    End
}

/// <summary>
/// Defines side anchoring options for bubble reactions.
/// </summary>
public enum BubbleReactionsSide
{
    /// <summary>
    /// Anchor reactions above bubble content.
    /// </summary>
    Top,

    /// <summary>
    /// Anchor reactions below bubble content.
    /// </summary>
    Bottom
}

/// <summary>
/// Defines horizontal alignment options for bubble reactions.
/// </summary>
public enum BubbleReactionsAlign
{
    /// <summary>
    /// Align reactions to the start edge.
    /// </summary>
    Start,

    /// <summary>
    /// Align reactions to the end edge.
    /// </summary>
    End
}

/// <summary>
/// Element type to render for <see cref="BbBubbleContent"/>.
/// </summary>
public enum BubbleContentElement
{
    /// <summary>
    /// Render as a div element for static content.
    /// </summary>
    Div,

    /// <summary>
    /// Render as an anchor element for navigation links.
    /// </summary>
    Anchor,

    /// <summary>
    /// Render as a button element for actions.
    /// </summary>
    Button
}

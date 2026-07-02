namespace BlazorBlueprint.Components;

/// <summary>
/// Defines upload lifecycle states for an attachment.
/// </summary>
public enum AttachmentState
{
    /// <summary>
    /// Attachment is idle and ready.
    /// </summary>
    Idle,

    /// <summary>
    /// Attachment is currently uploading.
    /// </summary>
    Uploading,

    /// <summary>
    /// Attachment is being processed after upload.
    /// </summary>
    Processing,

    /// <summary>
    /// Attachment upload or processing failed.
    /// </summary>
    Error,

    /// <summary>
    /// Attachment is fully uploaded and available.
    /// </summary>
    Done
}

/// <summary>
/// Defines size options for attachment cards.
/// </summary>
public enum AttachmentSize
{
    /// <summary>
    /// Default attachment size.
    /// </summary>
    Default,

    /// <summary>
    /// Small attachment size.
    /// </summary>
    Sm,

    /// <summary>
    /// Extra small attachment size.
    /// </summary>
    Xs
}

/// <summary>
/// Defines orientation of media and content in an attachment.
/// </summary>
public enum AttachmentOrientation
{
    /// <summary>
    /// Media and content are arranged horizontally.
    /// </summary>
    Horizontal,

    /// <summary>
    /// Media and content are stacked vertically.
    /// </summary>
    Vertical
}

/// <summary>
/// Defines media rendering modes for attachment media slot.
/// </summary>
public enum AttachmentMediaVariant
{
    /// <summary>
    /// Icon-style media container.
    /// </summary>
    Icon,

    /// <summary>
    /// Image preview media container.
    /// </summary>
    Image
}

/// <summary>
/// Element type to render for <see cref="BbAttachmentTrigger"/>.
/// </summary>
public enum AttachmentTriggerElement
{
    /// <summary>
    /// Render as a button element for actions.
    /// </summary>
    Button,

    /// <summary>
    /// Render as an anchor element for navigation links.
    /// </summary>
    Anchor
}

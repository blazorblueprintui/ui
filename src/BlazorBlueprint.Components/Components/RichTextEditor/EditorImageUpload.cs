namespace BlazorBlueprint.Components;

/// <summary>
/// An image the user added to a <see cref="BbRichTextEditor"/> — picked from the toolbar,
/// dropped onto the editor or pasted — handed to <see cref="BbRichTextEditor.ImageUploader"/>.
/// </summary>
/// <param name="FileName">The file name the browser reported, such as <c>photo.png</c>. Not trusted; use it for a display name or an extension at most.</param>
/// <param name="ContentType">The MIME type the browser reported, such as <c>image/png</c>.</param>
/// <param name="Size">The file size in bytes.</param>
/// <param name="Content">The image bytes. Read it fully inside the handler; it is disposed when the handler returns.</param>
public sealed record EditorImageUpload(string FileName, string ContentType, long Size, Stream Content);

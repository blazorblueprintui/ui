using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBlueprint.Components;

/// <summary>
/// Represents a file in the upload queue.
/// </summary>
public class FileUploadItem
{
    /// <summary>Current transport state. Selection alone does not start transport without UploadHandler.</summary>
    public FileUploadStatus Status { get; internal set; }
    public long BytesTransferred { get; internal set; }
    public double Progress => Size == 0 ? Status == FileUploadStatus.Succeeded ? 100 : 0 : 100.0 * BytesTransferred / Size;
    /// <summary>The last transport exception, for application diagnostics. The default UI shows a generic failure message.</summary>
    public Exception? UploadError { get; internal set; }

    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the browser file reference.
    /// </summary>
    public required IBrowserFile File { get; init; }

    /// <summary>
    /// Gets or sets the preview data URL (for images).
    /// </summary>
    public string? PreviewUrl { get; set; }

    /// <summary>
    /// Gets or sets whether the file is an image.
    /// </summary>
    public bool IsImage => File.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the file name.
    /// </summary>
    public string Name => File.Name;

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public long Size => File.Size;

    /// <summary>
    /// Gets the content type.
    /// </summary>
    public string ContentType => File.ContentType;

    /// <summary>
    /// Gets a formatted file size string.
    /// </summary>
    public string FormattedSize
    {
        get
        {
            if (Size < 1024)
            {
                return $"{Size} B";
            }

            if (Size < 1024 * 1024)
            {
                return $"{Size / 1024.0:F1} KB";
            }

            if (Size < 1024 * 1024 * 1024)
            {
                return $"{Size / (1024.0 * 1024):F1} MB";
            }
            return $"{Size / (1024.0 * 1024 * 1024):F1} GB";
        }
    }

    /// <summary>
    /// Gets the file extension.
    /// </summary>
    public string Extension => Path.GetExtension(File.Name).TrimStart('.').ToUpperInvariant();
}

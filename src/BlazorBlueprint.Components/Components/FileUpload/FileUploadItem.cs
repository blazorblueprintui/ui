using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBlueprint.Components.FileUpload;

/// <summary>
/// Represents a file in the upload queue.
/// </summary>
public class FileUploadItem
{
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

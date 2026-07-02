using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A form field wrapper for <see cref="BbFileUpload"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
public partial class BbFormFieldFileUpload : FormFieldBase
{
    /// <summary>
    /// Gets or sets the selected files.
    /// </summary>
    [Parameter]
    public IReadOnlyList<FileUploadItem>? Files { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when files change.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyList<FileUploadItem>> FilesChanged { get; set; }

    /// <summary>
    /// Gets or sets whether multiple files can be selected.
    /// </summary>
    [Parameter]
    public bool Multiple { get; set; }

    /// <summary>
    /// Gets or sets accepted file types (MIME types or extensions).
    /// </summary>
    [Parameter]
    public string? Accept { get; set; }

    /// <summary>
    /// Gets or sets the maximum file size in bytes.
    /// </summary>
    [Parameter]
    public long MaxFileSize { get; set; } = 10485760;

    /// <summary>
    /// Gets or sets the maximum number of files.
    /// </summary>
    [Parameter]
    public int MaxFileCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether to show image previews.
    /// </summary>
    [Parameter]
    public bool ShowPreview { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the upload is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the file upload is required.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the callback when validation errors occur.
    /// </summary>
    [Parameter]
    public EventCallback<FileValidationError> OnValidationError { get; set; }

    /// <summary>
    /// Gets or sets custom dropzone content.
    /// </summary>
    [Parameter]
    public RenderFragment? DropzoneContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner FileUpload.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => null;

    private BbFileUpload? fileUploadRef;

    /// <summary>
    /// Clears all files and validation errors, resetting the component to its initial state.
    /// </summary>
    public async Task ClearFiles()
    {
        if (fileUploadRef != null)
        {
            await fileUploadRef.ClearFiles();
        }
    }

    private async Task HandleFilesChanged(IReadOnlyList<FileUploadItem> files)
    {
        Files = files;
        await FilesChanged.InvokeAsync(files);
    }
}

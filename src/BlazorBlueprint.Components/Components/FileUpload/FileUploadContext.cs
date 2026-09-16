namespace BlazorBlueprint.Components;

/// <summary>Lifecycle state for a selected file. Retry starts a fresh stream and resets progress.</summary>
public enum FileUploadStatus { Selected, Queued, Uploading, Succeeded, Canceled, Failed }

/// <summary>A transport invocation. Report bytes actually sent; returning successfully completes the upload.</summary>
public sealed class FileUploadContext
{
    private readonly Func<long, Task> report;
    private readonly long maximumSize;
    private readonly CancellationToken cancellationToken;

    internal FileUploadContext(FileUploadItem item, long maximumSize, Func<long, Task> report, CancellationToken cancellationToken)
    {
        Item = item;
        this.maximumSize = maximumSize;
        this.cancellationToken = cancellationToken;
        this.report = report;
    }

    public FileUploadItem Item { get; }
    /// <summary>Opens a fresh browser stream bounded by the component's MaxFileSize and cancellation token.</summary>
    public Stream OpenReadStream() => Item.File.OpenReadStream(maximumSize, cancellationToken);
    /// <summary>Reports cumulative bytes sent in this attempt. Regressing and late reports are ignored.</summary>
    public Task ReportProgressAsync(long bytesTransferred) => report(bytesTransferred);
}

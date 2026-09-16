using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBlueprint.Components;

public partial class BbFileUpload
{
    private readonly Dictionary<string, FileUploadOperation> operations = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim uploadSlots = new(3);
    private readonly List<InputSelection> inputSelections = [new()];
    private bool disposed;

    /// <summary>Uploads a file using the application's transport. Null preserves selection-only behavior.</summary>
    [Parameter] public Func<FileUploadContext, CancellationToken, Task>? UploadHandler { get; set; }
    /// <summary>Starts uploads after selection when UploadHandler is supplied. At most three run concurrently.</summary>
    [Parameter] public bool AutoUpload { get; set; } = true;
    /// <summary>Raised after an attempt reaches Succeeded, Failed or Canceled.</summary>
    [Parameter] public EventCallback<FileUploadItem> OnUploadFinished { get; set; }

    /// <summary>Starts selected, canceled or failed files. Successful files are not uploaded twice.</summary>
    public async Task UploadFilesAsync()
    {
        var files = _files.Where(f => f.Status is FileUploadStatus.Selected or FileUploadStatus.Canceled or FileUploadStatus.Failed).ToArray();
        await Task.WhenAll(files.Select(UploadFileAsync));
    }

    /// <summary>Starts or retries one file, resetting its progress. The handler must support a fresh attempt.</summary>
    public async Task UploadFileAsync(FileUploadItem file)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (Disabled || disposed || UploadHandler == null || !_files.Contains(file)
            || file.Status is FileUploadStatus.Succeeded or FileUploadStatus.Queued or FileUploadStatus.Uploading)
        {
            return;
        }
        if (!operations.TryGetValue(file.Id, out var operation))
        {
            operation = new FileUploadOperation(file);
            operations.Add(file.Id, operation);
        }
        await operation.RunAsync(UploadHandler, MaxFileSize, uploadSlots, NotifyUploadChangedAsync);
        if (!disposed && _files.Contains(file))
        {
            await OnUploadFinished.InvokeAsync(file);
        }
    }

    /// <summary>Requests cancellation of a queued or active attempt. Cancellation completes when the handler returns.</summary>
    public void CancelUpload(FileUploadItem file)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (operations.TryGetValue(file.Id, out var operation))
        {
            operation.Cancel();
        }
    }

    private Task NotifyUploadChangedAsync() => disposed ? Task.CompletedTask : InvokeAsync(StateHasChanged);

    private void ReleaseFile(FileUploadItem file)
    {
        if (operations.Remove(file.Id, out var operation))
        {
            operation.Dispose();
        }
        foreach (var selection in inputSelections)
        {
            selection.FileIds.Remove(file.Id);
        }
        inputSelections.RemoveAll(s => s != inputSelections[^1] && s.FileIds.Count == 0);
    }

    // A fresh InputFile is rendered after every selection. Older inputs stay hidden until their
    // files are removed: Blazor invalidates its IBrowserFile references when that input changes.
    private sealed class InputSelection
    {
        internal string Id { get; } = Guid.NewGuid().ToString("N");
        internal InputFile? Input { get; set; }
        internal HashSet<string> FileIds { get; } = new(StringComparer.Ordinal);
    }
}

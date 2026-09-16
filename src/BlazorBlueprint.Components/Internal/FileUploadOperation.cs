namespace BlazorBlueprint.Components;

internal sealed class FileUploadOperation(FileUploadItem item) : IDisposable
{
    private CancellationTokenSource? cancellation;
    private int running;
    private int attempt;
    private bool disposed;

    internal void Cancel() => cancellation?.Cancel();

    internal async Task RunAsync(Func<FileUploadContext, CancellationToken, Task> handler, long maximumSize,
        SemaphoreSlim slots, Func<Task> changed)
    {
        if (disposed || Interlocked.CompareExchange(ref running, 1, 0) != 0)
        {
            return;
        }
        var generation = Interlocked.Increment(ref attempt);
        using var source = new CancellationTokenSource();
        cancellation = source;
        item.Status = FileUploadStatus.Queued;
        item.BytesTransferred = 0;
        item.UploadError = null;
        var entered = false;
        try
        {
            await changed();
            await slots.WaitAsync(source.Token);
            entered = true;
            source.Token.ThrowIfCancellationRequested();
            item.Status = FileUploadStatus.Uploading;
            await changed();
            var context = new FileUploadContext(item, maximumSize, async bytes =>
            {
                if (generation != attempt || disposed || source.IsCancellationRequested || item.Status != FileUploadStatus.Uploading)
                {
                    return;
                }
                item.BytesTransferred = Math.Max(item.BytesTransferred, Math.Clamp(bytes, 0, item.Size));
                await changed();
            }, source.Token);
            await handler(context, source.Token);
            source.Token.ThrowIfCancellationRequested();
            item.BytesTransferred = item.Size;
            item.Status = FileUploadStatus.Succeeded;
        }
        catch (Exception) when (source.IsCancellationRequested)
        {
            item.Status = FileUploadStatus.Canceled;
        }
        catch (Exception ex)
        {
            item.UploadError = ex;
            item.Status = FileUploadStatus.Failed;
        }
        finally
        {
            if (entered)
            {
                slots.Release();
            }
            cancellation = null;
            Interlocked.Exchange(ref running, 0);
            await changed();
        }
    }

    public void Dispose()
    {
        disposed = true;
        Cancel();
        GC.SuppressFinalize(this);
    }
}

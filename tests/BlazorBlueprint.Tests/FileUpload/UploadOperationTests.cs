using BlazorBlueprint.Components;
using Microsoft.AspNetCore.Components.Forms;
using Xunit;

namespace BlazorBlueprint.Tests.FileUpload;

public class UploadOperationTests
{
    [Fact]
    public async Task FailureRetainsFileAndRetryGetsFreshStreamAndProgress()
    {
        var browserFile = new TestFile();
        var item = new FileUploadItem { File = browserFile };
        using var operation = new FileUploadOperation(item);
        using var slots = new SemaphoreSlim(1);
        FileUploadContext? oldContext = null;
        await operation.RunAsync(async (context, _) =>
        {
            oldContext = context;
            using var stream = context.OpenReadStream();
            await context.ReportProgressAsync(50);
            throw new IOException("Transport failed");
        }, 100, slots, Changed);
        Assert.Equal(FileUploadStatus.Failed, item.Status);
        Assert.Equal(50, item.BytesTransferred);
        await operation.RunAsync(async (context, _) =>
        {
            using var stream = context.OpenReadStream();
            Assert.Equal(0, item.BytesTransferred);
            await context.ReportProgressAsync(20);
            await oldContext!.ReportProgressAsync(99);
            Assert.Equal(20, item.BytesTransferred);
            await context.ReportProgressAsync(10);
            Assert.Equal(20, item.BytesTransferred);
        }, 100, slots, Changed);
        Assert.Equal(2, browserFile.OpenCount);
        Assert.Equal(FileUploadStatus.Succeeded, item.Status);
        Assert.Equal(100, item.Progress);
        Assert.Null(item.UploadError);
    }

    [Fact]
    public async Task QueuedCancellationNeverCallsTransport()
    {
        var item = new FileUploadItem { File = new TestFile() };
        using var operation = new FileUploadOperation(item);
        using var slots = new SemaphoreSlim(0, 1);
        var called = false;
        var task = operation.RunAsync((_, _) => { called = true; return Task.CompletedTask; }, 100, slots, Changed);
        operation.Cancel();
        await task;
        Assert.False(called);
        Assert.Equal(FileUploadStatus.Canceled, item.Status);
    }

    [Fact]
    public async Task ActiveCancellationAndDuplicateStartAreHandled()
    {
        var item = new FileUploadItem { File = new TestFile() };
        using var operation = new FileUploadOperation(item);
        using var slots = new SemaphoreSlim(1);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var task = operation.RunAsync(async (_, token) =>
        {
            started.SetResult();
            await Task.Delay(Timeout.Infinite, token);
        }, 100, slots, Changed);
        await started.Task;
        await operation.RunAsync((_, _) => throw new InvalidOperationException("Duplicate run"), 100, slots, Changed);
        operation.Cancel();
        await task;
        Assert.Equal(FileUploadStatus.Canceled, item.Status);
        Assert.Equal(1, slots.CurrentCount);
    }

    private static Task Changed() => Task.CompletedTask;
    private sealed class TestFile : IBrowserFile
    {
        public int OpenCount { get; private set; }
        public string Name => "test.bin";
        public DateTimeOffset LastModified => DateTimeOffset.UnixEpoch;
        public long Size => 100;
        public string ContentType => "application/octet-stream";
        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            Assert.Equal(100, maxAllowedSize);
            OpenCount++;
            return new MemoryStream(new byte[100]);
        }
    }
}

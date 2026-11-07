using System.IO;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// An <see cref="IJSRuntime"/> whose <c>animation.waitForExit</c> stays pending until
/// <see cref="Release"/> is called, so a test can assert that an overlay stays mounted
/// (and its focus trap held) while its exit animation plays. All other interop calls
/// complete at once.
/// </summary>
internal sealed class HeldExitJavaScript : IJSRuntime, IJSObjectReference
{
    private readonly TaskCompletionSource _wait = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _waits;

    public int WaitCount => _waits;

    public void Release() => _wait.TrySetResult();

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
        InvokeAsync<TValue>(identifier, CancellationToken.None, args);

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        if (identifier == "import")
        {
            return ValueTask.FromResult((TValue)(object)this);
        }

        if (identifier == "animation.waitForExit")
        {
            Interlocked.Increment(ref _waits);
            var tcs = new TaskCompletionSource<IJSVoidResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            _wait.Task.ContinueWith(_ => tcs.TrySetResult(default!), TaskScheduler.Default);
            return new ValueTask<TValue>((Task<TValue>)(object)tcs.Task);
        }

        if (typeof(TValue) == typeof(IJSObjectReference))
        {
            return ValueTask.FromResult((TValue)(object)this);
        }

        return ValueTask.FromResult(default(TValue)!);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
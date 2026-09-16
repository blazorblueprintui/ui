using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Tests.Rendering;

// Keep renderer internals confined to this adapter; tests use the component's public methods.
#pragma warning disable BL0006
internal sealed class ComponentTestRenderer(IServiceProvider services, ILoggerFactory loggerFactory)
    : Renderer(services, loggerFactory)
{
    private readonly List<int> roots = [];
    public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();
    protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;
    protected override void HandleException(Exception exception) => System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();

    internal async Task<T> MountAsync<T>(Dictionary<string, object?> parameters) where T : IComponent
    {
        var component = (T)InstantiateComponent(typeof(T));
        var id = AssignRootComponentId(component);
        roots.Add(id);
        await RenderRootComponentAsync(id, ParameterView.FromDictionary(parameters));
        return component;
    }

    internal T FindComponent<T>() where T : IComponent
    {
        var pending = new Stack<int>(roots);
        while (pending.TryPop(out var id))
        {
            var frames = GetCurrentRenderTreeFrames(id);
            for (var i = 0; i < frames.Count; i++)
            {
                var frame = frames.Array[i];
                if (frame.FrameType == RenderTreeFrameType.Component)
                {
                    if (frame.Component is T match)
                    {
                        return match;
                    }
                    pending.Push(frame.ComponentId);
                }
            }
        }
        throw new InvalidOperationException($"No rendered {typeof(T).Name} component found.");
    }
}
#pragma warning restore BL0006

internal sealed class NoopJavaScript : IJSRuntime, IJSObjectReference
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
        ValueTask.FromResult(typeof(TValue) == typeof(IJSObjectReference) ? (TValue)(object)this : default!);
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

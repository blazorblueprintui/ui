using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

/// <summary>
/// Connects to a Server-Sent Events endpoint and dispatches events to child
/// <see cref="BbEventTemplate{TValue}"/> components for typed deserialization
/// and rendering.
/// </summary>
public partial class BbEventConsumer : ComponentBase, IAsyncDisposable
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    private readonly EventConsumerContext context = new();

    private IJSObjectReference? jsModule;
    private IJSObjectReference? jsHandle;
    private DotNetObjectReference<BbEventConsumer>? dotNetRef;

    private bool disposed;
    private bool connected;

    /// <summary>
    /// The URL of the Server-Sent Events endpoint.
    /// </summary>
    [Parameter, EditorRequired]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Child content containing <see cref="BbEventTemplate{TValue}"/> components.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Invoked when the EventSource connection is established.
    /// </summary>
    [Parameter]
    public EventCallback OnOpen { get; set; }

    /// <summary>
    /// Invoked when an unnamed (default) message is received.
    /// Receives the raw event data string.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnMessage { get; set; }

    /// <summary>
    /// Whether to open the connection automatically on first render.
    /// Default is <c>true</c>. Set to <c>false</c> and call <see cref="ConnectAsync"/>
    /// to connect manually.
    /// </summary>
    [Parameter]
    public bool AutoConnect { get; set; } = true;

    /// <summary>
    /// Global render mode applied to all child <see cref="BbEventTemplate{TValue}"/>
    /// components.
    /// </summary>
    [Parameter]
    public EventRenderMode? RenderMode { get; set; }

    /// <summary>
    /// Invoked whenever the connection state changes, so the parent can re-render
    /// and display the current state (e.g. Disconnected, Connecting, Open, Reconnecting, Closed).
    /// </summary>
    [Parameter]
    public EventCallback<EventSourceState> OnConnectionStateChanged { get; set; }

    /// <summary>
    /// Gets the current connection state of the EventSource.
    /// </summary>
    public EventSourceState ConnectionState => context.ConnectionState;

    protected override void OnParametersSet()
    {
        if (RenderMode is EventRenderMode renderMode)
        {
            context.RenderMode = renderMode;
        }
    }

    private async Task SetConnectionStateAsync(EventSourceState state)
    {
        context.ConnectionState = state;
        await InvokeAsync(StateHasChanged);
        if (OnConnectionStateChanged.HasDelegate)
        {
            await OnConnectionStateChanged.InvokeAsync(state);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && AutoConnect)
        {
            await ConnectAsync();
        }
    }

    /// <summary>
    /// Opens the EventSource connection.
    /// </summary>
    public async Task ConnectAsync()
    {
        if (connected || disposed)
        {
            return;
        }

        try
        {
            await SetConnectionStateAsync(EventSourceState.Connecting);

            jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorBlueprint.Components/js/event-source.js");

            dotNetRef = DotNetObjectReference.Create(this);

            var eventTypes = context.RegisteredEventTypes.ToArray();
            jsHandle = await jsModule.InvokeAsync<IJSObjectReference>(
                "connect", Url, dotNetRef, eventTypes);

            connected = true;
        }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
        {
            _ = SetConnectionStateAsync(EventSourceState.Closed);
        }
        catch (InvalidOperationException)
        {
            _ = SetConnectionStateAsync(EventSourceState.Disconnected);
        }
    }

    /// <summary>
    /// Closes the EventSource connection.
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (!connected)
        {
            return;
        }

        await CleanupConnectionAsync();

        connected = false;
        await SetConnectionStateAsync(EventSourceState.Disconnected);
    }

    [JSInvokable]
    public async Task OnJsOpen()
    {
        if (disposed) { return; }

        await SetConnectionStateAsync(EventSourceState.Open);

        if (OnOpen.HasDelegate)
        {
            await OnOpen.InvokeAsync();
        }
    }

    [JSInvokable]
    public async Task OnJsError(string state)
    {
        if (disposed) { return; }

        var newState = state == "closed"
            ? EventSourceState.Closed
            : EventSourceState.Reconnecting;
        await SetConnectionStateAsync(newState);
    }

    [JSInvokable]
    public async Task OnJsDefaultMessage(string data)
    {
        if (disposed) { return; }

        if (OnMessage.HasDelegate)
        {
            await OnMessage.InvokeAsync(data);
        }
    }

    [JSInvokable]
    public Task OnJsMessage(string eventType, string eventId, string data)
    {
        if (disposed) { return Task.CompletedTask; }

        context.DispatchEvent(eventType, eventId, data);
        return Task.CompletedTask;
    }

    private async Task CleanupConnectionAsync()
    {
        if (jsHandle != null)
        {
            try
            {
                await jsHandle.InvokeVoidAsync("dispose");
            }
            catch
            {
                // Connection may already be closed
            }

            try
            {
                await jsHandle.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect
            }

            jsHandle = null;
        }

        dotNetRef?.Dispose();
        dotNetRef = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        connected = false;
        GC.SuppressFinalize(this);

        await CleanupConnectionAsync();

        if (jsModule != null)
        {
            try
            {
                await jsModule.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect
            }

            jsModule = null;
        }
    }
}

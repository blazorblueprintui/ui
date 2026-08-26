using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

public partial class BbSidebarProvider
{
    private SidebarContext Context { get; set; } = new();
    private IJSObjectReference? _module;
    private DotNetObjectReference<BbSidebarProvider>? _dotNetRef;
    private bool lastToggleShortcutEnabled = true;
    private int instanceId;

    // Last values pushed to the parent, so a context change only raises a callback for the state
    // that actually moved. Toggling the desktop sidebar must not fire OpenMobileChanged.
    private bool lastNotifiedOpen;
    private bool lastNotifiedOpenMobile;
    private bool initialized;

    /// <summary>
    /// Whether the desktop open state is owned by the consumer rather than this provider.
    /// Both halves of the binding are required: a value with no callback could never be updated
    /// from inside, leaving the sidebar unable to respond to its own trigger.
    /// </summary>
    private bool IsOpenControlled => Open.HasValue && OpenChanged.HasDelegate;

    /// <summary>
    /// Whether the mobile drawer state is owned by the consumer. Independent of
    /// <see cref="IsOpenControlled"/> — one may be controlled without the other.
    /// </summary>
    private bool IsOpenMobileControlled => OpenMobile.HasValue && OpenMobileChanged.HasDelegate;

    /// <summary>
    /// Cookie persistence is suppressed while the desktop state is controlled: the consumer owns
    /// the value, so a restored cookie would fight the bound value on the next load.
    /// </summary>
    private bool ShouldPersist => !string.IsNullOrEmpty(CookieKey) && !IsOpenControlled;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    protected override void OnParametersSet()
    {
        // Update context when parameters change
        Context.SetVariant(Variant);
        Context.SetSide(Side);

        // Push controlled values down. SetOpen/SetOpenMobile no-op when the value is unchanged,
        // so the callback raised below cannot bounce back into an update loop.
        if (initialized)
        {
            if (IsOpenControlled && Open!.Value != Context.Open)
            {
                lastNotifiedOpen = Open.Value;
                Context.SetOpen(Open.Value);
            }

            if (IsOpenMobileControlled && OpenMobile!.Value != Context.OpenMobile)
            {
                lastNotifiedOpenMobile = OpenMobile.Value;
                Context.SetOpenMobile(OpenMobile.Value);
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                // Load the sidebar JavaScript module
                _module = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/BlazorBlueprint.Components/js/sidebar.js");

                // Create a reference to this component for JS callbacks
                _dotNetRef = DotNetObjectReference.Create(this);

                // Initialize sidebar state from cookie if persistence is enabled.
                // Skipped in controlled mode — the bound value wins, so reading a cookie here
                // would only produce a flash of the wrong state before the parent's value applies.
                bool? savedOpen = null;
                if (ShouldPersist)
                {
                    // Use JsonElement because JS returns bool|null and InvokeAsync<bool?> can't handle null
                    var result = await _module.InvokeAsync<JsonElement>("getSidebarState", CookieKey!);
                    savedOpen = result.ValueKind switch
                    {
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => null
                    };
                }

                // Initialize context: controlled value first, then cookie, then DefaultOpen
                InitializeContext(savedOpen ?? DefaultOpen);

                // Set up mobile detection and keyboard shortcuts
                lastToggleShortcutEnabled = EnableToggleShortcut;
                instanceId = await _module.InvokeAsync<int>("initializeSidebar", _dotNetRef, EnableToggleShortcut);

                StateHasChanged();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect in Blazor Server
                InitializeContext(DefaultOpen);
                StateHasChanged();
            }
            catch (InvalidOperationException)
            {
                // JS interop not available during prerendering
                InitializeContext(DefaultOpen);
                StateHasChanged();
            }
        }
        else if (_module != null && lastToggleShortcutEnabled != EnableToggleShortcut)
        {
            // Keep the shortcut in sync when the parameter changes after the first render
            lastToggleShortcutEnabled = EnableToggleShortcut;

            try
            {
                await _module.InvokeVoidAsync("setToggleShortcutEnabled", instanceId, EnableToggleShortcut);
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect in Blazor Server
            }
            catch (InvalidOperationException)
            {
                // JS interop not available
            }
        }
    }

    /// <summary>
    /// Seeds the context and subscribes for change notification. Called from every initialization
    /// path — including the prerender and disconnected-circuit fallbacks, which previously left the
    /// provider unsubscribed and so unable to persist state or raise the change callbacks.
    /// </summary>
    /// <param name="uncontrolledOpen">
    /// Desktop state to use when <see cref="Open"/> is not bound: the persisted cookie value if one
    /// was read, otherwise <see cref="DefaultOpen"/>. A bound value takes precedence over both.
    /// </param>
    private void InitializeContext(bool uncontrolledOpen)
    {
        var open = IsOpenControlled ? Open!.Value : uncontrolledOpen;

        Context.Initialize(open: open, variant: Variant, side: Side);

        if (IsOpenMobileControlled)
        {
            Context.SetOpenMobile(OpenMobile!.Value);
        }

        lastNotifiedOpen = Context.Open;
        lastNotifiedOpenMobile = Context.OpenMobile;

        if (!initialized)
        {
            Context.StateChanged += OnStateChanged;
            initialized = true;
        }
    }

    private async void OnStateChanged(object? sender, EventArgs e)
    {
        try
        {
            // Raise the binding callbacks before persisting, so a consumer sees the change at the
            // same point they would from any other component in the library. Each is compared
            // against the last value pushed, so toggling the desktop sidebar does not also raise
            // OpenMobileChanged, and a re-entrant update from the parent settles rather than loops.
            if (Context.Open != lastNotifiedOpen)
            {
                lastNotifiedOpen = Context.Open;

                if (OpenChanged.HasDelegate)
                {
                    await OpenChanged.InvokeAsync(Context.Open);
                }
            }

            if (Context.OpenMobile != lastNotifiedOpenMobile)
            {
                lastNotifiedOpenMobile = Context.OpenMobile;

                if (OpenMobileChanged.HasDelegate)
                {
                    await OpenMobileChanged.InvokeAsync(Context.OpenMobile);
                }
            }

            // Persist sidebar state to cookie when it changes
            if (_module != null && ShouldPersist)
            {
                try
                {
                    await _module.InvokeVoidAsync("saveSidebarState", CookieKey!, Context.Open);
                }
                catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
                {
                    // Expected during circuit disconnect
                }
                catch (InvalidOperationException)
                {
                    // JS interop not available during prerendering
                }
            }

            // Notify UI of state change
            await InvokeAsync(StateHasChanged);
        }
        catch (ObjectDisposedException)
        {
            // Component may be disposed during async operation
        }
    }

    /// <summary>
    /// Called from JavaScript when mobile state changes.
    /// </summary>
    [JSInvokable]
    public void OnMobileChange(bool isMobile) =>
        Context.SetIsMobile(isMobile);

    /// <summary>
    /// Called from JavaScript when keyboard shortcut (Ctrl/Cmd + B) is pressed.
    /// </summary>
    [JSInvokable]
    public void OnToggleShortcut()
    {
        Context.ToggleSidebar();
        StateHasChanged(); // Force re-render after toggle
    }

    public async ValueTask DisposeAsync()
    {
        if (Context != null)
        {
            Context.StateChanged -= OnStateChanged;
        }

        if (_module != null)
        {
            try
            {
                if (instanceId != 0)
                {
                    await _module.InvokeVoidAsync("cleanup", instanceId);
                }

                await _module.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException)
            {
                // Circuit disconnected, ignore
            }
            catch (InvalidOperationException)
            {
                // JS interop not available
            }
        }

        _dotNetRef?.Dispose();

        GC.SuppressFinalize(this);
    }
}

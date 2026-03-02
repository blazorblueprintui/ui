using Microsoft.JSInterop;

namespace BlazorBlueprint.Demo.Services;

/// <summary>
/// Service for persisting collapsible menu state in browser localStorage.
/// </summary>
public class CollapsibleStateService : IAsyncDisposable
{
    private readonly IJSRuntime jsRuntime;
    private const string LocalStoragePrefix = "blazorblueprint:collapsible:";
    private IJSObjectReference? utilsModule;

    public CollapsibleStateService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    private async ValueTask<IJSObjectReference> GetUtilsModuleAsync()
    {
        return utilsModule ??= await jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/BlazorBlueprint.Demo.Shared/js/demo-utils.js");
    }

    /// <summary>
    /// Get the saved state for a collapsible menu.
    /// </summary>
    /// <param name="key">Unique identifier for the collapsible (e.g., "sidebar-primitives-menu")</param>
    /// <param name="defaultValue">Default value if no state is saved</param>
    /// <returns>True if the menu should be open, false otherwise</returns>
    public async Task<bool> GetStateAsync(string key, bool defaultValue = false)
    {
        try
        {
            var storageKey = LocalStoragePrefix + key;
            var value = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", storageKey);

            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            return bool.TryParse(value, out var result) && result;
        }
        catch
        {
            // If localStorage is not available or there's an error, return default
            return defaultValue;
        }
    }

    /// <summary>
    /// Save the state for a collapsible menu.
    /// Fires and forgets the JS interop call to avoid blocking the UI thread.
    /// </summary>
    /// <param name="key">Unique identifier for the collapsible</param>
    /// <param name="isOpen">Whether the menu is open</param>
    public void SetState(string key, bool isOpen)
    {
        var storageKey = LocalStoragePrefix + key;
        _ = SetStateFireAndForgetAsync(storageKey, isOpen);
    }

    private async Task SetStateFireAndForgetAsync(string storageKey, bool isOpen)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", storageKey, isOpen.ToString());
        }
        catch
        {
            // Silently fail if localStorage is not available
        }
    }

    /// <summary>
    /// Clear the saved state for a collapsible menu.
    /// </summary>
    /// <param name="key">Unique identifier for the collapsible</param>
    public async Task ClearStateAsync(string key)
    {
        try
        {
            var storageKey = LocalStoragePrefix + key;
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", storageKey);
        }
        catch
        {
            // Silently fail if localStorage is not available
        }
    }

    /// <summary>
    /// Clear all saved collapsible states.
    /// </summary>
    public async Task ClearAllStatesAsync()
    {
        try
        {
            var module = await GetUtilsModuleAsync();
            var keys = await module.InvokeAsync<string[]>("getLocalStorageKeysByPrefix", LocalStoragePrefix);

            foreach (var key in keys)
            {
                await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            }
        }
        catch
        {
            // Silently fail if localStorage is not available
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (utilsModule != null)
        {
            try
            {
                await utilsModule.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Circuit disconnected, ignore
            }
        }

        GC.SuppressFinalize(this);
    }
}

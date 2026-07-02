using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Primitives.NavigationMenu;

/// <summary>
/// Root context for the navigation menu. Manages which item is open, close timers,
/// and trigger registration for keyboard navigation.
/// </summary>
public class NavigationMenuContext : IDisposable
{
    private readonly Action stateChanged;
    private readonly List<ElementReference> triggerRefs = new();
    private CancellationTokenSource? closeTimerCts;

    /// <summary>
    /// The value of the currently open menu item, or <c>null</c> if all are closed.
    /// </summary>
    public string? ActiveItem { get; private set; }

    /// <summary>
    /// Whether keyboard navigation within dropdowns is enabled.
    /// </summary>
    public bool EnableKeyboardNavigation { get; }

    /// <summary>
    /// Number of registered triggers.
    /// </summary>
    public int TriggerCount => triggerRefs.Count;

    /// <summary>
    /// Creates a new <see cref="NavigationMenuContext"/>.
    /// </summary>
    public NavigationMenuContext(Action stateChanged, bool enableKeyboardNavigation)
    {
        this.stateChanged = stateChanged;
        EnableKeyboardNavigation = enableKeyboardNavigation;
    }

    /// <summary>
    /// Sets the active menu item and triggers a re-render.
    /// </summary>
    public void SetActiveItem(string? value)
    {
        ActiveItem = value;
        stateChanged();
    }

    /// <summary>
    /// Registers a trigger element and returns its index.
    /// </summary>
    public int RegisterTrigger(ElementReference triggerRef)
    {
        triggerRefs.Add(triggerRef);
        return triggerRefs.Count - 1;
    }

    /// <summary>
    /// Updates the element reference at the given index (needed after first render).
    /// </summary>
    public void UpdateTriggerRef(int index, ElementReference triggerRef)
    {
        if (index >= 0 && index < triggerRefs.Count)
        {
            triggerRefs[index] = triggerRef;
        }
    }

    /// <summary>
    /// Gets the trigger element at the specified index.
    /// </summary>
    public ElementReference? GetTriggerAt(int index)
    {
        if (index >= 0 && index < triggerRefs.Count)
        {
            return triggerRefs[index];
        }

        return null;
    }

    /// <summary>
    /// Starts a shared close timer. After the delay, closes all menus.
    /// Cancel with <see cref="CancelCloseTimer"/>.
    /// </summary>
    public async void StartCloseTimer()
    {
        CancelCloseTimer();
        closeTimerCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(150, closeTimerCts.Token);
            SetActiveItem(null);
        }
        catch (TaskCanceledException)
        {
            // Timer was cancelled
        }
    }

    /// <summary>
    /// Cancels any pending close timer.
    /// </summary>
    public void CancelCloseTimer()
    {
        closeTimerCts?.Cancel();
        closeTimerCts?.Dispose();
        closeTimerCts = null;
    }

    /// <summary>
    /// Disposes the close timer resources.
    /// </summary>
    public void Dispose()
    {
        closeTimerCts?.Cancel();
        closeTimerCts?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Per-item context for a navigation menu item.
/// </summary>
public class NavigationMenuItemContext
{
    private readonly NavigationMenuContext parent;

    /// <summary>
    /// Unique value identifying this menu item.
    /// </summary>
    public string? Value { get; }

    /// <summary>
    /// Whether this item's dropdown is currently open.
    /// </summary>
    public bool IsOpen => parent.ActiveItem == Value;

    /// <summary>
    /// Creates a new <see cref="NavigationMenuItemContext"/>.
    /// </summary>
    public NavigationMenuItemContext(NavigationMenuContext parent, string? value)
    {
        this.parent = parent;
        Value = value;
    }

    /// <summary>Sets this item as open.</summary>
    public void Open() => parent.SetActiveItem(Value);

    /// <summary>Closes this item if open.</summary>
    public void Close()
    {
        if (IsOpen)
        {
            parent.SetActiveItem(null);
        }
    }

    /// <summary>Toggles open/closed.</summary>
    public void Toggle()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    /// <summary>Starts the parent's shared close timer.</summary>
    public void StartCloseTimer() => parent.StartCloseTimer();

    /// <summary>Cancels the parent's close timer.</summary>
    public void CancelCloseTimer() => parent.CancelCloseTimer();
}

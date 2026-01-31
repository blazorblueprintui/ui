using BlazorUI.Primitives.Utilities;

namespace BlazorUI.Primitives.Contexts;

/// <summary>
/// Base class for primitive component contexts.
/// Provides a pattern for sharing state between parent and child components
/// using Blazor's CascadingValue mechanism.
/// </summary>
/// <typeparam name="TState">The type of state this context manages.</typeparam>
public abstract class PrimitiveContext<TState> where TState : class
{
    /// <summary>
    /// Gets or sets the state for this context.
    /// </summary>
    public TState State { get; set; }

    /// <summary>
    /// The component ID, automatically generated.
    /// Used for coordinating ARIA attributes between components.
    /// </summary>
    public string Id { get; protected set; }

    /// <summary>
    /// Initializes a new primitive context with a state instance.
    /// </summary>
    /// <param name="state">The initial state.</param>
    /// <param name="idPrefix">Prefix for generated ID.</param>
    protected PrimitiveContext(TState state, string idPrefix = "primitive")
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
        Id = IdGenerator.GenerateId(idPrefix);
    }

    /// <summary>
    /// Generates a scoped ID for child components.
    /// Useful for creating unique IDs for ARIA attributes.
    /// </summary>
    /// <param name="suffix">The suffix to append to the component ID.</param>
    /// <returns>A unique scoped ID.</returns>
    public string GetScopedId(string suffix)
    {
        return $"{Id}-{suffix}";
    }
}

/// <summary>
/// Base class for primitive component contexts with event callbacks.
/// Extends PrimitiveContext to include common event handling patterns.
/// </summary>
/// <typeparam name="TState">The type of state this context manages.</typeparam>
public abstract class PrimitiveContextWithEvents<TState> : PrimitiveContext<TState>
    where TState : class
{
    /// <summary>
    /// Event that is raised when the state changes.
    /// Subscribers should call StateHasChanged when this event fires.
    /// </summary>
    public event Action? OnStateChanged;

    /// <summary>
    /// Initializes a new primitive context with events.
    /// </summary>
    /// <param name="state">The initial state.</param>
    /// <param name="idPrefix">Prefix for generated ID.</param>
    protected PrimitiveContextWithEvents(TState state, string idPrefix = "primitive")
        : base(state, idPrefix)
    {
    }

    /// <summary>
    /// Notifies subscribers that the state has changed.
    /// Child components should re-render when this is called.
    /// </summary>
    protected void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Updates the state and notifies subscribers.
    /// </summary>
    /// <param name="updateAction">Action that updates the state.</param>
    protected void UpdateState(Action<TState> updateAction)
    {
        updateAction(State);
        NotifyStateChanged();
    }

    /// <summary>
    /// Updates the state with optional notification.
    /// </summary>
    /// <param name="updateAction">Action that updates the state.</param>
    /// <param name="notifyChanged">Whether to notify subscribers of the change. Default is true.</param>
    protected void UpdateState(Action<TState> updateAction, bool notifyChanged)
    {
        updateAction(State);
        if (notifyChanged)
        {
            NotifyStateChanged();
        }
    }
}

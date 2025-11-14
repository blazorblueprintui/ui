using Microsoft.AspNetCore.Components;

namespace BlazorUI.Primitives.Utilities;

/// <summary>
/// Utility for managing controlled and uncontrolled component state.
/// Supports both patterns where the parent component controls the state,
/// or the component manages its own internal state.
/// </summary>
/// <typeparam name="T">The type of the state value.</typeparam>
public class UseControllableState<T>
{
    private T? _uncontrolledValue;
    private readonly T _defaultValue;

    /// <summary>
    /// Creates a new controllable state manager.
    /// </summary>
    /// <param name="defaultValue">The default value when uncontrolled.</param>
    public UseControllableState(T defaultValue)
    {
        _defaultValue = defaultValue;
        _uncontrolledValue = defaultValue;
    }

    /// <summary>
    /// The controlled value provided by the parent component.
    /// Set this to control the value from the parent.
    /// </summary>
    public T? ControlledValue { get; set; }

    /// <summary>
    /// Explicitly sets whether this state is controlled by the parent.
    /// Must be set by the component based on whether ControlledValue parameter was provided.
    /// </summary>
    public bool IsControlled { get; set; }

    /// <summary>
    /// Event callback for notifying parent of value changes.
    /// </summary>
    public EventCallback<T> OnValueChanged { get; set; }

    /// <summary>
    /// Gets the current value, either controlled or uncontrolled.
    /// </summary>
    public T Value => IsControlled ? ControlledValue! : _uncontrolledValue!;

    /// <summary>
    /// Sets the value, updating either controlled or uncontrolled state.
    /// </summary>
    /// <param name="value">The new value.</param>
    /// <returns>A task that completes when the value change is handled.</returns>
    public async Task SetValueAsync(T value)
    {
        if (!IsControlled)
        {
            _uncontrolledValue = value;
        }

        // Always notify parent of change (for both controlled and uncontrolled)
        if (OnValueChanged.HasDelegate)
        {
            await OnValueChanged.InvokeAsync(value);
        }
    }

    /// <summary>
    /// Resets the uncontrolled value to the default.
    /// Has no effect in controlled mode.
    /// </summary>
    public void Reset()
    {
        if (!IsControlled)
        {
            _uncontrolledValue = _defaultValue;
        }
    }
}

/// <summary>
/// Extension methods for working with controllable state.
/// </summary>
public static class UseControllableStateExtensions
{
    /// <summary>
    /// Creates a controllable state manager from component parameters.
    /// </summary>
    /// <typeparam name="T">The type of the state value.</typeparam>
    /// <param name="controlledValue">The controlled value parameter.</param>
    /// <param name="onValueChanged">The value changed callback.</param>
    /// <param name="defaultValue">The default value when uncontrolled.</param>
    /// <param name="isControlled">Whether the component is in controlled mode (typically: controlledValue != null).</param>
    /// <returns>A configured controllable state manager.</returns>
    public static UseControllableState<T> Create<T>(
        T? controlledValue,
        EventCallback<T> onValueChanged,
        T defaultValue,
        bool? isControlled = null)
    {
        return new UseControllableState<T>(defaultValue)
        {
            ControlledValue = controlledValue,
            OnValueChanged = onValueChanged,
            // If not explicitly specified, infer from whether callback has delegate
            IsControlled = isControlled ?? (onValueChanged.HasDelegate && controlledValue != null)
        };
    }
}

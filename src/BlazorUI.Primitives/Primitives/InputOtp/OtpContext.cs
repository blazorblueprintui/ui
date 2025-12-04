using BlazorUI.Primitives.Contexts;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Primitives.InputOtp;

/// <summary>
/// State for the OTP input primitive context.
/// </summary>
public class OtpState
{
    /// <summary>
    /// Gets or sets the array of characters representing each slot.
    /// </summary>
    public char[] Slots { get; set; } = Array.Empty<char>();

    /// <summary>
    /// Gets or sets the index of the currently focused slot.
    /// </summary>
    public int FocusedIndex { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether the OTP input is disabled.
    /// </summary>
    public bool IsDisabled { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the OTP input is in an invalid/error state.
    /// </summary>
    public bool IsInvalid { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the OTP input currently has focus.
    /// </summary>
    public bool HasFocus { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the current focus is from keyboard navigation.
    /// When true, focus should not be redirected to the first empty slot.
    /// </summary>
    public bool IsKeyboardNavigation { get; set; } = false;
}

/// <summary>
/// Context for OTP input primitive component and its children.
/// Manages slot values, focus state, and provides IDs for ARIA attributes.
/// </summary>
public class OtpContext : PrimitiveContextWithEvents<OtpState>
{
    private readonly List<ElementReference> _slotRefs = new();

    /// <summary>
    /// Initializes a new instance of the OtpContext with the specified length.
    /// </summary>
    /// <param name="length">The number of OTP slots.</param>
    public OtpContext(int length = 6) : base(new OtpState(), "otp")
    {
        State.Slots = new char[length];
        // Initialize with empty chars
        for (int i = 0; i < length; i++)
        {
            State.Slots[i] = '\0';
        }
    }

    /// <summary>
    /// Gets the ID for the OTP container.
    /// </summary>
    public string ContainerId => GetScopedId("container");

    /// <summary>
    /// Gets the ID for a specific slot.
    /// </summary>
    /// <param name="index">The slot index.</param>
    public string GetSlotId(int index) => GetScopedId($"slot-{index}");

    /// <summary>
    /// Gets the number of slots.
    /// </summary>
    public int Length => State.Slots.Length;

    /// <summary>
    /// Gets the currently focused slot index.
    /// </summary>
    public int FocusedIndex => State.FocusedIndex;

    /// <summary>
    /// Gets the current complete OTP value as a string.
    /// </summary>
    public string Value => new string(State.Slots).Replace("\0", "");

    /// <summary>
    /// Gets whether the OTP is complete (all slots filled).
    /// </summary>
    public bool IsComplete => !State.Slots.Contains('\0');

    /// <summary>
    /// Gets whether the OTP input is disabled.
    /// </summary>
    public bool IsDisabled => State.IsDisabled;

    /// <summary>
    /// Gets whether the OTP input is in an invalid/error state.
    /// </summary>
    public bool IsInvalid => State.IsInvalid;

    /// <summary>
    /// Gets whether the OTP input currently has focus.
    /// </summary>
    public bool HasFocus => State.HasFocus;

    /// <summary>
    /// Gets whether the current focus is from keyboard navigation.
    /// </summary>
    public bool IsKeyboardNavigation => State.IsKeyboardNavigation;

    /// <summary>
    /// Registers a slot element reference for focus management.
    /// </summary>
    /// <param name="index">The slot index.</param>
    /// <param name="element">The element reference.</param>
    public void RegisterSlot(int index, ElementReference element)
    {
        // Ensure the list is large enough
        while (_slotRefs.Count <= index)
        {
            _slotRefs.Add(default);
        }
        _slotRefs[index] = element;
    }

    /// <summary>
    /// Gets the element reference for a slot.
    /// </summary>
    /// <param name="index">The slot index.</param>
    public ElementReference? GetSlotRef(int index)
    {
        if (index >= 0 && index < _slotRefs.Count)
        {
            return _slotRefs[index];
        }
        return null;
    }

    /// <summary>
    /// Sets the value at the specified slot index.
    /// </summary>
    /// <param name="index">The slot index.</param>
    /// <param name="value">The character value.</param>
    public void SetSlotValue(int index, char value)
    {
        if (index >= 0 && index < State.Slots.Length)
        {
            UpdateState(state =>
            {
                state.Slots[index] = value;
            });
        }
    }

    /// <summary>
    /// Gets the value at the specified slot index.
    /// </summary>
    /// <param name="index">The slot index.</param>
    public char GetSlotValue(int index)
    {
        if (index >= 0 && index < State.Slots.Length)
        {
            return State.Slots[index];
        }
        return '\0';
    }

    /// <summary>
    /// Sets the focused slot index.
    /// </summary>
    /// <param name="index">The slot index to focus.</param>
    public void SetFocusedIndex(int index)
    {
        if (index >= 0 && index < State.Slots.Length)
        {
            UpdateState(state =>
            {
                state.FocusedIndex = index;
            });
        }
    }

    /// <summary>
    /// Moves focus to the next slot if available.
    /// </summary>
    public void FocusNext()
    {
        if (State.FocusedIndex < State.Slots.Length - 1)
        {
            SetFocusedIndex(State.FocusedIndex + 1);
        }
    }

    /// <summary>
    /// Moves focus to the previous slot if available.
    /// </summary>
    public void FocusPrevious()
    {
        if (State.FocusedIndex > 0)
        {
            SetFocusedIndex(State.FocusedIndex - 1);
        }
    }

    /// <summary>
    /// Clears all slot values.
    /// </summary>
    public void Clear()
    {
        UpdateState(state =>
        {
            for (int i = 0; i < state.Slots.Length; i++)
            {
                state.Slots[i] = '\0';
            }
            state.FocusedIndex = 0;
        });
    }

    /// <summary>
    /// Sets the complete OTP value from a string.
    /// Useful for paste operations.
    /// </summary>
    /// <param name="value">The value to set.</param>
    public void SetValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return;

        UpdateState(state =>
        {
            for (int i = 0; i < state.Slots.Length; i++)
            {
                state.Slots[i] = (i < value.Length) ? value[i] : '\0';
            }
            // Focus the next empty slot or the last slot
            state.FocusedIndex = Math.Min(value.Length, state.Slots.Length - 1);
        });
    }

    /// <summary>
    /// Sets the disabled state.
    /// </summary>
    /// <param name="disabled">Whether the input is disabled.</param>
    public void SetDisabled(bool disabled)
    {
        UpdateState(state =>
        {
            state.IsDisabled = disabled;
        });
    }

    /// <summary>
    /// Sets the invalid/error state.
    /// </summary>
    /// <param name="invalid">Whether the input is in an invalid state.</param>
    public void SetInvalid(bool invalid)
    {
        UpdateState(state =>
        {
            state.IsInvalid = invalid;
        });
    }

    /// <summary>
    /// Sets the focus state.
    /// </summary>
    /// <param name="hasFocus">Whether the input has focus.</param>
    public void SetHasFocus(bool hasFocus)
    {
        UpdateState(state =>
        {
            state.HasFocus = hasFocus;
        });
    }

    /// <summary>
    /// Sets the keyboard navigation state.
    /// </summary>
    /// <param name="isKeyboardNavigation">Whether focus is from keyboard navigation.</param>
    public void SetKeyboardNavigation(bool isKeyboardNavigation)
    {
        UpdateState(state =>
        {
            state.IsKeyboardNavigation = isKeyboardNavigation;
        });
    }
}

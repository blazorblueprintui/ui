using Microsoft.AspNetCore.Components;
using BlazorBlueprint.Primitives.Contexts;

namespace BlazorBlueprint.Primitives.Select;

/// <summary>
/// Non-generic interface exposing the display-only contract of a select context.
/// Used by components that need the selected display text without knowing TValue.
/// </summary>
public interface ISelectDisplayContext
{
    /// <summary>
    /// Gets the display text for the currently selected value.
    /// </summary>
    public string? DisplayText { get; }

    /// <summary>
    /// Event that is raised when the state changes.
    /// </summary>
    public event Action? OnStateChanged;
}

/// <summary>
/// State for the Select primitive context.
/// </summary>
/// <typeparam name="TValue">The type of the selected value.</typeparam>
public class SelectState<TValue>
{
    /// <summary>
    /// Gets or sets whether the select dropdown is currently open.
    /// </summary>
    public bool IsOpen { get; set; }

    /// <summary>
    /// Gets or sets the currently selected value.
    /// </summary>
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets the display text for the currently selected value.
    /// </summary>
    public string? DisplayText { get; set; }

    /// <summary>
    /// Gets or sets the element that triggers the select dropdown.
    /// Used for positioning.
    /// </summary>
    public ElementReference? TriggerElement { get; set; }

    /// <summary>
    /// Gets or sets the index of the currently focused item.
    /// Used for keyboard navigation.
    /// </summary>
    public int FocusedIndex { get; set; } = -1;

    /// <summary>
    /// Gets or sets whether the select is disabled.
    /// </summary>
    public bool Disabled { get; set; }
}

/// <summary>
/// Context for Select primitive component and its children.
/// Manages select state, provides IDs for ARIA attributes, and handles keyboard navigation.
/// </summary>
/// <typeparam name="TValue">The type of the selected value.</typeparam>
public class SelectContext<TValue> : PrimitiveContextWithEvents<SelectState<TValue>>, ISelectDisplayContext
{
    /// <summary>
    /// Gets or sets default CSS classes to apply to all items in the select.
    /// Cascaded from the parent Select's ItemClass parameter.
    /// </summary>
    public string? ItemClass { get; set; }

    /// <summary>
    /// Callback invoked when the selected value changes.
    /// </summary>
    public Action<TValue?>? OnValueChange { get; set; }

    /// <summary>
    /// Initializes a new instance of the SelectContext.
    /// </summary>
    public SelectContext() : base(new SelectState<TValue>(), "select")
    {
        TriggerId = GetScopedId("trigger");
    }

    /// <summary>
    /// Gets the ID for the select trigger button.
    /// Can be overridden via <see cref="SetTriggerId"/> when the trigger element
    /// uses a custom id (e.g. for label association in form fields).
    /// </summary>
    public string TriggerId { get; private set; }

    /// <summary>
    /// Gets the ID for the select content container.
    /// </summary>
    public string ContentId => GetScopedId("content");

    /// <summary>
    /// Gets the ID for the select value display.
    /// </summary>
    public string ValueId => GetScopedId("value");

    /// <summary>
    /// Gets whether the select is currently open.
    /// </summary>
    public bool IsOpen => State.IsOpen;

    /// <summary>
    /// Gets the currently focused item index.
    /// </summary>
    public int FocusedIndex => State.FocusedIndex;

    /// <summary>
    /// Gets the currently selected value.
    /// </summary>
    public TValue? Value => State.Value;

    /// <summary>
    /// Gets the display text for the currently selected value.
    /// </summary>
    public string? DisplayText => State.DisplayText;

    /// <summary>
    /// Gets whether the select is disabled.
    /// </summary>
    public bool Disabled => State.Disabled;

    /// <summary>
    /// Opens the select dropdown.
    /// </summary>
    /// <param name="triggerElement">Optional element that triggered the dropdown.</param>
    public void Open(ElementReference? triggerElement = null)
    {
        if (State.Disabled)
        {
            return;
        }

        // Clear registered items when opening — they'll re-register as the content mounts.
        // Required for ForceMount=false where items are destroyed on close and recreated on open.
        if (!State.IsOpen)
        {
            Items.Clear();
        }

        UpdateState(state =>
        {
            state.IsOpen = true;
            state.TriggerElement = triggerElement;
            state.FocusedIndex = -1; // Reset focus on open
        });
    }

    /// <summary>
    /// Closes the select dropdown.
    /// </summary>
    public void Close()
    {
        UpdateState(state =>
        {
            state.IsOpen = false;
            state.FocusedIndex = -1;
        });
    }

    /// <summary>
    /// Toggles the select dropdown open/closed state.
    /// </summary>
    /// <param name="triggerElement">Optional element that triggered the toggle.</param>
    public void Toggle(ElementReference? triggerElement = null)
    {
        if (State.Disabled)
        {
            return;
        }

        if (State.IsOpen)
        {
            Close();
        }
        else
        {
            Open(triggerElement);
        }
    }

    /// <summary>
    /// Selects a value and updates the display text.
    /// </summary>
    /// <param name="value">The value to select.</param>
    /// <param name="displayText">The display text for the selected value.</param>
    public void SelectValue(TValue? value, string? displayText)
    {
        UpdateState(state =>
        {
            state.Value = value;
            state.DisplayText = displayText;
            state.IsOpen = false; // Close after selection
            state.FocusedIndex = -1;
        });

        // Invoke value change callback
        OnValueChange?.Invoke(value);
    }

    /// <summary>
    /// Updates the display text for the currently selected value.
    /// Only fires a state change notification when the text actually changes.
    /// </summary>
    /// <param name="displayText">The new display text.</param>
    public void SetDisplayText(string? displayText)
    {
        if (State.DisplayText != displayText)
        {
            UpdateState(state => state.DisplayText = displayText);
        }
    }

    /// <summary>
    /// Sets the disabled state.
    /// </summary>
    /// <param name="disabled">Whether the select is disabled.</param>
    public void SetDisabled(bool disabled)
    {
        UpdateState(state =>
        {
            state.Disabled = disabled;
            if (disabled && state.IsOpen)
            {
                state.IsOpen = false;
            }
        });
    }

    /// <summary>
    /// Overrides the auto-generated trigger ID with a custom value.
    /// Used when the trigger element has a custom <c>id</c> attribute
    /// (e.g. for label association in form fields). Ensures click-outside
    /// detection and ARIA references use the correct ID.
    /// </summary>
    /// <param name="id">The custom trigger element ID.</param>
    public void SetTriggerId(string id) =>
        TriggerId = id;

    /// <summary>
    /// Sets the focused item index for keyboard navigation.
    /// </summary>
    /// <param name="index">The index of the item to focus.</param>
    public void SetFocusedIndex(int index) => UpdateState(state => state.FocusedIndex = index);

    /// <summary>
    /// Gets the list of registered items for keyboard navigation.
    /// </summary>
    private List<SelectItemMetadata<TValue>> Items { get; } = new List<SelectItemMetadata<TValue>>();

    /// <summary>
    /// Registers an item with the select context for keyboard navigation.
    /// </summary>
    /// <param name="value">The value of the item.</param>
    /// <param name="disabled">Whether the item is disabled.</param>
    /// <param name="displayText">The display text for the item.</param>
    /// <returns>A stable identifier for the registered item.</returns>
    public string RegisterItem(TValue? value, bool disabled, string? displayText = null)
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var metadata = new SelectItemMetadata<TValue>
        {
            Id = id,
            Value = value,
            Disabled = disabled,
            DisplayText = displayText
        };
        Items.Add(metadata);

        // If this item's value matches the currently selected value,
        // update the DisplayText to show the proper display name
        if (displayText != null && EqualityComparer<TValue>.Default.Equals(State.Value, value))
        {
            UpdateState(state => state.DisplayText = displayText);
        }

        return id;
    }

    /// <summary>
    /// Unregisters an item from the select context by its stable identifier.
    /// Safe to call during disposal — does not corrupt other items' indices.
    /// </summary>
    /// <param name="id">The stable identifier returned by <see cref="RegisterItem"/>.</param>
    public void UnregisterItem(string id)
    {
        var index = Items.FindIndex(i => i.Id == id);
        if (index >= 0)
        {
            Items.RemoveAt(index);

            // Adjust focused index if needed
            if (State.FocusedIndex >= Items.Count)
            {
                SetFocusedIndex(Items.Count - 1);
            }
        }
    }

    /// <summary>
    /// Gets the current index of an item by its stable identifier.
    /// Returns -1 if the item is not found.
    /// </summary>
    /// <param name="id">The stable identifier returned by <see cref="RegisterItem"/>.</param>
    /// <returns>The current index in the items list, or -1 if not found.</returns>
    public int GetItemIndex(string id) =>
        Items.FindIndex(i => i.Id == id);

    /// <summary>
    /// Gets the stable identifier of the currently focused item.
    /// Returns null if no item is focused.
    /// </summary>
    public string? GetFocusedItemId()
    {
        if (State.FocusedIndex >= 0 && State.FocusedIndex < Items.Count)
        {
            return Items[State.FocusedIndex].Id;
        }
        return null;
    }

    /// <summary>
    /// Moves focus to the next item that is not disabled.
    /// </summary>
    /// <param name="direction">1 for next, -1 for previous.</param>
    public void MoveFocus(int direction)
    {
        if (Items.Count == 0)
        {
            return;
        }

        var startIndex = State.FocusedIndex;
        var newIndex = startIndex;

        // Find next non-disabled item
        do
        {
            newIndex += direction;

            // Wrap around
            if (newIndex < 0)
            {
                newIndex = Items.Count - 1;
            }
            else if (newIndex >= Items.Count)
            {
                newIndex = 0;
            }


            // Avoid infinite loop if all items are disabled
            if (newIndex == startIndex)
            {
                break;
            }


        } while (newIndex >= 0 && newIndex < Items.Count && Items[newIndex].Disabled);

        SetFocusedIndex(newIndex);
    }

    /// <summary>
    /// Moves focus to the first enabled item.
    /// </summary>
    public void FocusFirst()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            if (!Items[i].Disabled)
            {
                SetFocusedIndex(i);
                return;
            }
        }
    }

    /// <summary>
    /// Moves focus to the currently selected item, or the first enabled item if none is selected.
    /// </summary>
    public void FocusSelectedOrFirst()
    {
        // Try to find and focus the selected item
        if (State.Value != null)
        {
            for (var i = 0; i < Items.Count; i++)
            {
                if (!Items[i].Disabled && EqualityComparer<TValue>.Default.Equals(Items[i].Value, State.Value))
                {
                    SetFocusedIndex(i);
                    return;
                }
            }
        }

        // Fall back to focusing the first item
        FocusFirst();
    }

    /// <summary>
    /// Moves focus to the last enabled item.
    /// </summary>
    public void FocusLast()
    {
        for (var i = Items.Count - 1; i >= 0; i--)
        {
            if (!Items[i].Disabled)
            {
                SetFocusedIndex(i);
                return;
            }
        }
    }

    /// <summary>
    /// Selects the currently focused item.
    /// </summary>
    public void SelectFocusedItem()
    {
        if (State.FocusedIndex >= 0 && State.FocusedIndex < Items.Count)
        {
            var item = Items[State.FocusedIndex];
            if (!item.Disabled)
            {
                SelectValue(item.Value, item.DisplayText);
            }
        }
    }

    /// <summary>
    /// Gets the display text for a given value by looking up registered items.
    /// </summary>
    /// <param name="value">The value to look up.</param>
    /// <returns>The display text if found, otherwise null.</returns>
    public string? GetDisplayTextForValue(TValue? value)
    {
        if (value == null)
        {
            return null;
        }


        var item = Items.FirstOrDefault(i => EqualityComparer<TValue>.Default.Equals(i.Value, value));
        return item?.DisplayText;
    }

}

/// <summary>
/// Metadata for a registered select item.
/// </summary>
/// <typeparam name="TValue">The type of the item value.</typeparam>
public class SelectItemMetadata<TValue>
{
    /// <summary>
    /// Gets or sets the stable identifier for this item registration.
    /// Used for safe unregistration without index corruption.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the select item.
    /// </summary>
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the display text for the item.
    /// </summary>
    public string? DisplayText { get; set; }
}

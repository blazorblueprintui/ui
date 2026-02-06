using Microsoft.AspNetCore.Components.Web;
using System.Globalization;

namespace BlazorBlueprint.Primitives.Utilities;

/// <summary>
/// Utility for handling keyboard navigation in list-based components.
/// Supports arrow keys, Home/End, and RTL text direction.
/// </summary>
public class KeyboardNavigator
{
    /// <summary>
    /// Handles arrow key navigation and returns the direction offset.
    /// </summary>
    /// <param name="e">The keyboard event.</param>
    /// <param name="options">Navigation options.</param>
    /// <returns>The index offset (-1, 0, or 1), or null if key not handled.</returns>
    public static int? HandleArrowNavigation(KeyboardEventArgs e, NavigationOptions options)
    {
        if (e == null)
        {
            return null;
        }

        return e.Key switch
        {
            "ArrowUp" when options.Orientation != Orientation.Horizontal => -1,
            "ArrowDown" when options.Orientation != Orientation.Horizontal => 1,
            "ArrowLeft" when options.Orientation != Orientation.Vertical =>
                options.IsRtl ? 1 : -1,
            "ArrowRight" when options.Orientation != Orientation.Vertical =>
                options.IsRtl ? -1 : 1,
            "Home" => int.MinValue, // Special value to indicate first item
            "End" => int.MaxValue,   // Special value to indicate last item
            _ => null
        };
    }

    /// <summary>
    /// Calculates the next index based on current index and navigation direction.
    /// </summary>
    /// <param name="currentIndex">The current selected index.</param>
    /// <param name="offset">The direction offset from HandleArrowNavigation.</param>
    /// <param name="itemCount">Total number of items.</param>
    /// <param name="loop">Whether to loop from end to start and vice versa.</param>
    /// <returns>The next index to select.</returns>
    public static int GetNextIndex(int currentIndex, int offset, int itemCount, bool loop = true)
    {
        if (itemCount == 0)
        {
            return -1;
        }

        // Handle Home key
        if (offset == int.MinValue)
        {
            return 0;
        }

        // Handle End key
        if (offset == int.MaxValue)
        {
            return itemCount - 1;
        }

        var nextIndex = currentIndex + offset;

        if (loop)
        {
            // Wrap around
            if (nextIndex < 0)
            {
                nextIndex = itemCount - 1;
            }

            if (nextIndex >= itemCount)
            {
                nextIndex = 0;
            }
        }
        else
        {
            // Clamp to bounds
            if (nextIndex < 0)
            {
                nextIndex = 0;
            }

            if (nextIndex >= itemCount)
            {
                nextIndex = itemCount - 1;
            }
        }

        return nextIndex;
    }

    /// <summary>
    /// Determines if the current culture uses right-to-left text direction.
    /// </summary>
    /// <returns>True if RTL, false otherwise.</returns>
    public static bool IsRtl() =>
        CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;

    /// <summary>
    /// Handles type-ahead search by matching key input to item text.
    /// </summary>
    /// <param name="key">The pressed key.</param>
    /// <param name="items">List of searchable item texts.</param>
    /// <param name="currentIndex">Current selected index.</param>
    /// <returns>The index of the matched item, or current index if no match.</returns>
    public static int? HandleTypeAhead(string key, IReadOnlyList<string> items, int currentIndex)
    {
        if (string.IsNullOrEmpty(key) || key.Length != 1)
        {
            return null;
        }

        var searchText = key;
        var startIndex = (currentIndex + 1) % items.Count;

        // Search from current position forward
        for (var i = 0; i < items.Count; i++)
        {
            var index = (startIndex + i) % items.Count;
            if (items[index].StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return null;
    }
}

/// <summary>
/// Options for keyboard navigation behavior.
/// </summary>
public class NavigationOptions
{
    /// <summary>
    /// The orientation of the navigation (horizontal, vertical, or both).
    /// </summary>
    public Orientation Orientation { get; set; } = Orientation.Both;

    /// <summary>
    /// Whether the component uses right-to-left text direction.
    /// </summary>
    public bool IsRtl { get; set; } = KeyboardNavigator.IsRtl();

    /// <summary>
    /// Whether navigation should loop from last to first item.
    /// </summary>
    public bool Loop { get; set; } = true;

    /// <summary>
    /// Whether to skip disabled items during navigation.
    /// </summary>
    public bool SkipDisabled { get; set; } = true;
}

/// <summary>
/// Navigation orientation for keyboard navigation.
/// </summary>
public enum Orientation
{
    /// <summary>
    /// Horizontal navigation only (Left/Right arrows).
    /// </summary>
    Horizontal,

    /// <summary>
    /// Vertical navigation only (Up/Down arrows).
    /// </summary>
    Vertical,

    /// <summary>
    /// Both horizontal and vertical navigation.
    /// </summary>
    Both
}

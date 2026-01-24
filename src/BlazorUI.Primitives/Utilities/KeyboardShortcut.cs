namespace BlazorUI.Primitives.Utilities;

/// <summary>
/// Represents a keyboard shortcut definition with modifier keys and a main key.
/// Provides parsing, normalization, and display formatting for keyboard shortcuts.
/// </summary>
public class KeyboardShortcut
{
    /// <summary>
    /// The main key (e.g., "T", "N", "Delete", "F1").
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Whether Ctrl (or Cmd on Mac) is required.
    /// </summary>
    public bool Ctrl { get; set; }

    /// <summary>
    /// Whether Shift is required.
    /// </summary>
    public bool Shift { get; set; }

    /// <summary>
    /// Whether Alt (Option on Mac) is required.
    /// </summary>
    public bool Alt { get; set; }

    /// <summary>
    /// Whether Meta (Windows key or Cmd on Mac) is required.
    /// Note: Ctrl and Meta are typically treated as equivalent for cross-platform shortcuts.
    /// </summary>
    public bool Meta { get; set; }

    /// <summary>
    /// Parses a shortcut string like "Ctrl+Shift+T" into a KeyboardShortcut.
    /// </summary>
    /// <param name="shortcutString">The shortcut string (e.g., "Ctrl+N", "Shift+Delete", "Ctrl+Shift+S").</param>
    /// <returns>A KeyboardShortcut instance.</returns>
    /// <exception cref="ArgumentException">Thrown if the shortcut string is invalid.</exception>
    public static KeyboardShortcut Parse(string shortcutString)
    {
        if (string.IsNullOrWhiteSpace(shortcutString))
        {
            throw new ArgumentException("Shortcut string cannot be null or empty.", nameof(shortcutString));
        }

        var shortcut = new KeyboardShortcut();
        var parts = shortcutString.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            throw new ArgumentException("Shortcut string must contain at least one key.", nameof(shortcutString));
        }

        foreach (var part in parts)
        {
            var lowerPart = part.ToLowerInvariant();

            switch (lowerPart)
            {
                case "ctrl":
                case "control":
                case "cmd":
                case "command":
                    shortcut.Ctrl = true;
                    break;
                case "shift":
                    shortcut.Shift = true;
                    break;
                case "alt":
                case "option":
                case "opt":
                    shortcut.Alt = true;
                    break;
                case "meta":
                case "win":
                case "windows":
                    shortcut.Meta = true;
                    break;
                default:
                    // This is the main key
                    if (!string.IsNullOrEmpty(shortcut.Key))
                    {
                        throw new ArgumentException(
                            $"Shortcut string can only have one main key. Found '{shortcut.Key}' and '{part}'.",
                            nameof(shortcutString));
                    }
                    shortcut.Key = NormalizeKey(part);
                    break;
            }
        }

        if (string.IsNullOrEmpty(shortcut.Key))
        {
            throw new ArgumentException("Shortcut string must contain a main key (not just modifiers).", nameof(shortcutString));
        }

        return shortcut;
    }

    /// <summary>
    /// Attempts to parse a shortcut string, returning false if parsing fails.
    /// </summary>
    /// <param name="shortcutString">The shortcut string to parse.</param>
    /// <param name="shortcut">The parsed shortcut, or null if parsing failed.</param>
    /// <returns>True if parsing succeeded, false otherwise.</returns>
    public static bool TryParse(string? shortcutString, out KeyboardShortcut? shortcut)
    {
        shortcut = null;
        if (string.IsNullOrWhiteSpace(shortcutString))
        {
            return false;
        }

        try
        {
            shortcut = Parse(shortcutString);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the normalized key combination string for comparison.
    /// Format: "ctrl+shift+alt+meta+key" (lowercase, consistent order).
    /// </summary>
    /// <returns>A normalized string representation of the shortcut.</returns>
    public string GetNormalizedKey()
    {
        var parts = new List<string>();

        if (Ctrl) parts.Add("ctrl");
        if (Shift) parts.Add("shift");
        if (Alt) parts.Add("alt");
        if (Meta) parts.Add("meta");

        parts.Add(Key.ToLowerInvariant());

        return string.Join("+", parts);
    }

    /// <summary>
    /// Returns a display string for the shortcut.
    /// </summary>
    /// <param name="useMacSymbols">If true, uses Mac-style symbols (⌘, ⇧, ⌥). Defaults to false.</param>
    /// <returns>A human-readable display string.</returns>
    public string ToDisplayString(bool useMacSymbols = false)
    {
        var parts = new List<string>();

        if (useMacSymbols)
        {
            if (Ctrl) parts.Add("⌘");
            if (Shift) parts.Add("⇧");
            if (Alt) parts.Add("⌥");
            if (Meta) parts.Add("⌃");
        }
        else
        {
            if (Ctrl) parts.Add("Ctrl");
            if (Shift) parts.Add("Shift");
            if (Alt) parts.Add("Alt");
            if (Meta) parts.Add("Win");
        }

        // Capitalize the key for display
        var displayKey = Key.Length == 1 ? Key.ToUpperInvariant() : CapitalizeFirst(Key);
        parts.Add(displayKey);

        return useMacSymbols ? string.Concat(parts) : string.Join("+", parts);
    }

    /// <summary>
    /// Checks if this shortcut matches a keyboard event.
    /// </summary>
    /// <param name="key">The key from the keyboard event.</param>
    /// <param name="ctrlKey">Whether Ctrl was pressed.</param>
    /// <param name="shiftKey">Whether Shift was pressed.</param>
    /// <param name="altKey">Whether Alt was pressed.</param>
    /// <param name="metaKey">Whether Meta was pressed.</param>
    /// <returns>True if the event matches this shortcut.</returns>
    public bool Matches(string key, bool ctrlKey, bool shiftKey, bool altKey, bool metaKey)
    {
        // Normalize Ctrl/Meta - treat them as equivalent for cross-platform
        var eventHasCtrlOrMeta = ctrlKey || metaKey;
        var shortcutNeedsCtrlOrMeta = Ctrl || Meta;

        if (shortcutNeedsCtrlOrMeta != eventHasCtrlOrMeta) return false;
        if (Shift != shiftKey) return false;
        if (Alt != altKey) return false;

        // Compare keys case-insensitively
        return string.Equals(Key, NormalizeKey(key), StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public override string ToString() => ToDisplayString();

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is KeyboardShortcut other)
        {
            return GetNormalizedKey() == other.GetNormalizedKey();
        }
        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode() => GetNormalizedKey().GetHashCode();

    private static string NormalizeKey(string key)
    {
        // Normalize common key names
        return key.ToLowerInvariant() switch
        {
            " " or "space" or "spacebar" => "space",
            "esc" or "escape" => "escape",
            "del" or "delete" => "delete",
            "ins" or "insert" => "insert",
            "pgup" or "pageup" => "pageup",
            "pgdn" or "pagedown" => "pagedown",
            "arrowup" or "up" => "arrowup",
            "arrowdown" or "down" => "arrowdown",
            "arrowleft" or "left" => "arrowleft",
            "arrowright" or "right" => "arrowright",
            "return" or "enter" => "enter",
            "backspace" or "back" => "backspace",
            "tab" => "tab",
            _ => key.ToLowerInvariant()
        };
    }

    private static string CapitalizeFirst(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();
    }
}

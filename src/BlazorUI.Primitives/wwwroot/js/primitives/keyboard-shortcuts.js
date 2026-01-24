// Global keyboard shortcut handler for BlazorUI
// Provides document-level keyboard event handling with Blazor interop

let dotNetRef = null;
let registeredShortcuts = new Set();
let keydownHandler = null;

/**
 * Initializes the keyboard shortcut listener.
 * @param {Object} dotNetReference - DotNet object reference for callbacks to C#
 */
export function initialize(dotNetReference) {
    dotNetRef = dotNetReference;

    keydownHandler = handleKeyDown;
    document.addEventListener('keydown', keydownHandler, true);
}

/**
 * Registers a shortcut key combination to listen for.
 * @param {string} normalizedKey - The normalized key combination (e.g., "ctrl+shift+t")
 */
export function registerShortcut(normalizedKey) {
    registeredShortcuts.add(normalizedKey.toLowerCase());
}

/**
 * Unregisters a shortcut key combination.
 * @param {string} normalizedKey - The normalized key combination to remove
 */
export function unregisterShortcut(normalizedKey) {
    registeredShortcuts.delete(normalizedKey.toLowerCase());
}

/**
 * Disposes the keyboard shortcut listener and cleans up all resources.
 */
export function dispose() {
    if (keydownHandler) {
        document.removeEventListener('keydown', keydownHandler, true);
        keydownHandler = null;
    }
    dotNetRef = null;
    registeredShortcuts.clear();
}

/**
 * Handles keydown events and invokes registered shortcuts.
 * @param {KeyboardEvent} event - The keyboard event
 */
function handleKeyDown(event) {
    // Skip if target is an input element (user is typing)
    if (shouldSkipEvent(event)) {
        return;
    }

    const normalizedKey = getNormalizedKey(event);

    if (registeredShortcuts.has(normalizedKey)) {
        event.preventDefault();
        event.stopPropagation();

        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('HandleShortcutAsync', normalizedKey)
                .catch(err => console.error('Error invoking shortcut handler:', err));
        }
    }
}

/**
 * Determines if the keyboard event should be skipped.
 * @param {KeyboardEvent} event - The keyboard event
 * @returns {boolean} True if the event should be skipped
 */
function shouldSkipEvent(event) {
    const target = event.target;

    // Skip if typing in input elements
    if (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA') {
        return true;
    }

    // Skip if in contenteditable
    if (target.isContentEditable) {
        return true;
    }

    // Skip if in a select element
    if (target.tagName === 'SELECT') {
        return true;
    }

    return false;
}

/**
 * Normalizes a keyboard event into a consistent key string.
 * @param {KeyboardEvent} event - The keyboard event
 * @returns {string} Normalized key string (e.g., "ctrl+shift+t")
 */
function getNormalizedKey(event) {
    const parts = [];

    // Ctrl or Meta (Cmd on Mac) - treat as equivalent
    if (event.ctrlKey || event.metaKey) {
        parts.push('ctrl');
    }

    if (event.shiftKey) {
        parts.push('shift');
    }

    if (event.altKey) {
        parts.push('alt');
    }

    // Normalize the main key
    let key = normalizeKeyName(event.key);
    parts.push(key);

    return parts.join('+');
}

/**
 * Normalizes key names for consistent comparison.
 * @param {string} key - The key name from the event
 * @returns {string} Normalized key name
 */
function normalizeKeyName(key) {
    const lowerKey = key.toLowerCase();

    // Handle special key names
    const keyMap = {
        ' ': 'space',
        'escape': 'escape',
        'enter': 'enter',
        'tab': 'tab',
        'backspace': 'backspace',
        'delete': 'delete',
        'insert': 'insert',
        'home': 'home',
        'end': 'end',
        'pageup': 'pageup',
        'pagedown': 'pagedown',
        'arrowup': 'arrowup',
        'arrowdown': 'arrowdown',
        'arrowleft': 'arrowleft',
        'arrowright': 'arrowright'
    };

    return keyMap[lowerKey] || lowerKey;
}

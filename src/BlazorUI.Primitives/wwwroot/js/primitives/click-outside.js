// Click-outside detection for dropdowns, popovers, and dialogs

// Store cleanup functions with unique IDs to avoid passing functions through JS interop
const cleanupRegistry = new Map();
let cleanupIdCounter = 0;

/**
 * Sets up click-outside detection for an element.
 * @param {HTMLElement} element - The element to monitor
 * @param {Object} dotNetRef - DotNet object reference for callback
 * @param {string} methodName - The method name to invoke on click outside
 * @param {HTMLElement} excludeElement - Optional element to exclude from outside detection (e.g., trigger button)
 * @returns {Object} Disposable object with dispose() method to remove listeners
 */
export function onClickOutside(element, dotNetRef, methodName = 'HandleClickOutside', excludeElement = null) {
    if (!element || !dotNetRef) {
        console.warn('click-outside: element or dotNetRef is null');
        return {
            _cleanupId: -1,
            dispose: function() {}
        };
    }

    let isMouseDownInside = false;

    const handleMouseDown = (e) => {
        isMouseDownInside = element.contains(e.target) || (excludeElement && excludeElement.contains(e.target));
    };

    const handleMouseUp = (e) => {
        // Only trigger if both mousedown and mouseup were outside (and not on excluded element)
        const isOutside = !element.contains(e.target) && !(excludeElement && excludeElement.contains(e.target));

        if (!isMouseDownInside && isOutside) {
            try {
                // Check if dotNetRef is still valid before invoking
                if (dotNetRef && !dotNetRef._disposed) {
                    dotNetRef.invokeMethodAsync(methodName);
                }
            } catch (error) {
                console.error('click-outside callback error:', error);
            }
        }
        isMouseDownInside = false;
    };

    // Use capture phase to ensure we catch events before they're stopped
    document.addEventListener('mousedown', handleMouseDown, true);
    document.addEventListener('mouseup', handleMouseUp, true);

    // Store cleanup function in registry with unique ID
    const cleanupFunc = () => {
        document.removeEventListener('mousedown', handleMouseDown, true);
        document.removeEventListener('mouseup', handleMouseUp, true);
    };

    const id = cleanupIdCounter++;
    cleanupRegistry.set(id, cleanupFunc);

    // Return disposable object with ID (no arrow functions in the object)
    return {
        _cleanupId: id,
        dispose: function() {
            const cleanup = cleanupRegistry.get(this._cleanupId);
            if (cleanup) {
                cleanup();
                cleanupRegistry.delete(this._cleanupId);
            }
        }
    };
}

/**
 * Sets up Escape key detection.
 * @param {Object} dotNetRef - DotNet object reference for callback
 * @param {string} methodName - The method name to invoke on Escape
 * @returns {Object} Disposable object with dispose() method to remove listener
 */
export function onEscapeKey(dotNetRef, methodName = 'HandleEscape') {
    if (!dotNetRef) {
        console.warn('escape-key: dotNetRef is null');
        return {
            _cleanupId: -1,
            dispose: function() {}
        };
    }

    const handleKeyDown = (e) => {
        if (e.key === 'Escape') {
            try {
                if (dotNetRef && !dotNetRef._disposed) {
                    dotNetRef.invokeMethodAsync(methodName);
                }
            } catch (error) {
                console.error('escape-key callback error:', error);
            }
        }
    };

    document.addEventListener('keydown', handleKeyDown);

    // Store cleanup function in registry
    const cleanupFunc = () => {
        document.removeEventListener('keydown', handleKeyDown);
    };

    const id = cleanupIdCounter++;
    cleanupRegistry.set(id, cleanupFunc);

    return {
        _cleanupId: id,
        dispose: function() {
            const cleanup = cleanupRegistry.get(this._cleanupId);
            if (cleanup) {
                cleanup();
                cleanupRegistry.delete(this._cleanupId);
            }
        }
    };
}

/**
 * Sets up focus-outside detection (when focus leaves an element).
 * @param {HTMLElement} element - The element to monitor
 * @param {Object} dotNetRef - DotNet object reference for callback
 * @param {string} methodName - The method name to invoke on focus outside
 * @returns {Object} Disposable object with dispose() method to remove listeners
 */
export function onFocusOutside(element, dotNetRef, methodName = 'HandleFocusOutside') {
    if (!element || !dotNetRef) {
        console.warn('focus-outside: element or dotNetRef is null');
        return {
            _cleanupId: -1,
            dispose: function() {}
        };
    }

    const handleFocusIn = (e) => {
        if (!element.contains(e.target)) {
            try {
                if (dotNetRef && !dotNetRef._disposed) {
                    dotNetRef.invokeMethodAsync(methodName);
                }
            } catch (error) {
                console.error('focus-outside callback error:', error);
            }
        }
    };

    document.addEventListener('focusin', handleFocusIn, true);

    // Store cleanup function in registry
    const cleanupFunc = () => {
        document.removeEventListener('focusin', handleFocusIn, true);
    };

    const id = cleanupIdCounter++;
    cleanupRegistry.set(id, cleanupFunc);

    return {
        _cleanupId: id,
        dispose: function() {
            const cleanup = cleanupRegistry.get(this._cleanupId);
            if (cleanup) {
                cleanup();
                cleanupRegistry.delete(this._cleanupId);
            }
        }
    };
}

/**
 * Combined interaction-outside detector (click + focus).
 * @param {HTMLElement} element - The element to monitor
 * @param {Object} dotNetRef - DotNet object reference for callback
 * @param {string} methodName - The method name to invoke
 * @returns {Object} Disposable object with dispose() method to remove all listeners
 */
export function onInteractOutside(element, dotNetRef, methodName = 'HandleInteractOutside') {
    const cleanupClick = onClickOutside(element, dotNetRef, methodName);
    const cleanupFocus = onFocusOutside(element, dotNetRef, methodName);

    // Store combined cleanup in registry
    const cleanupFunc = () => {
        cleanupClick.dispose();
        cleanupFocus.dispose();
    };

    const id = cleanupIdCounter++;
    cleanupRegistry.set(id, cleanupFunc);

    return {
        _cleanupId: id,
        dispose: function() {
            const cleanup = cleanupRegistry.get(this._cleanupId);
            if (cleanup) {
                cleanup();
                cleanupRegistry.delete(this._cleanupId);
            }
        }
    };
}

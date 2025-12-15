/**
 * Keyboard navigation helper for primitives
 * Prevents default scroll behavior for navigation keys
 */

/**
 * Waits for an element to appear in the DOM by ID.
 * @param {string} elementId - ID of the element to wait for
 * @param {number} maxWaitMs - Maximum time to wait in milliseconds
 * @param {number} intervalMs - Check interval in milliseconds
 * @returns {Promise<HTMLElement|null>} The element if found, null otherwise
 */
async function waitForElementById(elementId, maxWaitMs = 200, intervalMs = 10) {
    const startTime = Date.now();

    while (Date.now() - startTime < maxWaitMs) {
        const element = document.getElementById(elementId);
        if (element && document.body.contains(element)) {
            return element;
        }
        await new Promise(resolve => setTimeout(resolve, intervalMs));
    }

    return null;
}

export function setupKeyboardNav(element, dotNetRef) {
    if (!element) return;

    const handleKeyDown = (e) => {
        // Prevent default scroll behavior for navigation keys
        if (['ArrowDown', 'ArrowUp', 'Home', 'End', 'PageUp', 'PageDown'].includes(e.key)) {
            e.preventDefault();
        }
    };

    element.addEventListener('keydown', handleKeyDown);

    // Return cleanup function
    return {
        dispose: () => {
            element.removeEventListener('keydown', handleKeyDown);
        }
    };
}

/**
 * Sets up keyboard navigation using element ID. Waits for element to appear in DOM.
 * This decouples C# timing from portal rendering - JS handles all waiting.
 * @param {string} elementId - ID of the element to set up keyboard nav for
 * @param {Object} dotNetRef - DotNet object reference (optional, for future callback support)
 * @param {number} maxWaitMs - Maximum time to wait for element in milliseconds
 * @returns {Promise<Object>} Disposable object with dispose() method, or no-op if element not found
 */
export async function setupKeyboardNavById(elementId, dotNetRef = null, maxWaitMs = 200) {
    const element = await waitForElementById(elementId, maxWaitMs);
    if (!element) {
        console.warn(`setupKeyboardNavById: Element ${elementId} not found within ${maxWaitMs}ms`);
        return {
            dispose: function() {}
        };
    }

    return setupKeyboardNav(element, dotNetRef);
}

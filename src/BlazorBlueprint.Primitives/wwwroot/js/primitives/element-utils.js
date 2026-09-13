/**
 * Element utilities for common DOM operations
 * Provides reusable functions to replace eval() calls
 */

/**
 * Shows an element by setting opacity and pointer-events.
 * Used as fallback when positioning setup fails.
 * @param {string} elementId - The ID of the element to show
 */
export function showElement(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.style.opacity = '1';
        element.style.pointerEvents = 'auto';
    }
}

/**
 * Scrolls an element into view with configurable options.
 * @param {string} elementId - The ID of the element to scroll into view
 * @param {string} block - The block alignment ('nearest', 'start', 'center', 'end')
 * @param {string} behavior - The scroll behavior ('instant', 'smooth', 'auto')
 */
export function scrollIntoView(elementId, block = 'nearest', behavior = 'instant') {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({
            block: block,
            behavior: behavior
        });
    }
}

/**
 * Focuses an element by its ID.
 * @param {string} elementId - The ID of the element to focus
 */
export function focusElement(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.focus();
    }
}

/**
 * Returns true when the scrollable container element is within `threshold` pixels
 * of its bottom edge. Works for both flex-column lists and multi-column CSS grids
 * because the measurement is taken on the *scroll container*, not on individual items.
 *
 * @param {HTMLElement} element   - The scrollable container element.
 * @param {number}      threshold - Pixels from the bottom edge that count as "near bottom" (default 80).
 * @returns {boolean}
 */
export function isNearBottom(element, threshold = 80) {
    if (!element) return false;
    return element.scrollTop + element.clientHeight >= element.scrollHeight - threshold;
}

// ============================================================================
// Keyboard arrival tracking
//
// `:focus-visible` cannot answer "did the user tab here?". Chrome reports it as true for a
// programmatic .focus() on a div[tabindex="0"] even straight after a real mouse click - measured,
// not assumed - so a hover card gated on it still opened when a focus trap moved focus. The same
// limitation is recorded at Primitives/Table/BbTableRow.razor:58.
//
// Tab is how a keyboard user reaches a trigger, and nothing moves focus programmatically in
// response to Tab, so "a Tab keydown happened just before this focus" separates the two cases
// where the selector cannot.
// ============================================================================

let lastTabKeyAt = 0;
let keyboardArrivalListenerInitialized = false;

function initKeyboardArrivalListener() {
    if (keyboardArrivalListenerInitialized) return;

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Tab') {
            lastTabKeyAt = performance.now();
        }
    }, true);

    keyboardArrivalListenerInitialized = true;
}

initKeyboardArrivalListener();

/**
 * Reports whether an element was reached by a deliberate keyboard focus, rather than by focus
 * moved programmatically - by a focus trap, or an explicit .focus() call.
 *
 * Returns true when the check cannot run, so that an unsupported browser never withholds a
 * hover card from a keyboard user. WCAG 1.4.13 is why the focus path exists at all.
 *
 * @param {HTMLElement} element - The element to test
 * @param {number} [withinMs=500] - How recently a Tab keydown must have happened
 * @returns {boolean} True when focus arrived from the keyboard
 */
export function isKeyboardFocus(element, withinMs = 500) {
    if (!element) return false;

    // A text field is always focus-visible, and typing into one is unambiguous intent.
    try {
        if (element.matches('input:not([type="button"]):not([type="submit"]), textarea, select')) {
            return true;
        }
    } catch {
        return true;
    }

    return (performance.now() - lastTabKeyAt) <= withinMs;
}

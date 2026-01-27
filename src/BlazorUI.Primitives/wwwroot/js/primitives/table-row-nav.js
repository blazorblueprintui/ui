/**
 * Table row keyboard navigation utilities
 * Provides functions for accessible table row interaction
 */

/**
 * Prevents Space key from scrolling when a table row is focused.
 * Attaches a keydown listener in capture phase.
 * @param {HTMLElement} element - The row element to attach the handler to
 * @returns {Object} Object with dispose function for cleanup
 */
export function preventSpaceKeyScroll(element) {
    if (!element) return { dispose: () => {} };

    const handleKeyDown = (e) => {
        // Check both modern and legacy key identifiers
        if (e.key === ' ' || e.keyCode === 32) {
            e.preventDefault();
        }
    };

    element.addEventListener('keydown', handleKeyDown, { capture: true });

    return {
        dispose: () => {
            element.removeEventListener('keydown', handleKeyDown, { capture: true });
        }
    };
}

/**
 * Moves focus to the previous focusable row.
 * Skips rows with tabindex="-1".
 * @param {HTMLElement} element - The current row element
 */
export function moveFocusToPreviousRow(element) {
    if (!element) return;

    let prevRow = element.previousElementSibling;
    while (prevRow && prevRow.getAttribute('tabindex') === '-1') {
        prevRow = prevRow.previousElementSibling;
    }
    if (prevRow && prevRow.getAttribute('tabindex') === '0') {
        prevRow.focus();
    }
}

/**
 * Moves focus to the next focusable row.
 * Skips rows with tabindex="-1".
 * @param {HTMLElement} element - The current row element
 */
export function moveFocusToNextRow(element) {
    if (!element) return;

    let nextRow = element.nextElementSibling;
    while (nextRow && nextRow.getAttribute('tabindex') === '-1') {
        nextRow = nextRow.nextElementSibling;
    }
    if (nextRow && nextRow.getAttribute('tabindex') === '0') {
        nextRow.focus();
    }
}

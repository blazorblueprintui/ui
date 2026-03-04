/**
 * Table row navigation and click utilities
 * Provides functions for accessible table row interaction
 */

/**
 * Selector matching interactive elements whose clicks should NOT
 * bubble into row-level click / selection handlers.
 */
const INTERACTIVE_SELECTOR =
  'a[href],button,input,select,textarea,label[for],' +
  '[role="button"],[role="checkbox"],[role="switch"],' +
  '[role="menuitem"],[role="option"],[role="tab"]';

/**
 * Attaches a capture-phase click listener on the row that stops propagation
 * when the click originates from an interactive child element.
 * This prevents row selection / OnRowClick from firing when the user
 * clicks buttons, links, checkboxes, etc. inside a cell.
 *
 * @param {HTMLElement} rowElement - The <tr> element
 * @returns {{ dispose(): void }} Cleanup handle
 */
export function interceptInteractiveClicks(rowElement) {
  if (!rowElement) return { dispose: () => {} };

  const handler = (e) => {
    // Walk from the actual click target up to (but not including) the row.
    // If we hit an interactive element, swallow the event.
    const interactive = e.target.closest(INTERACTIVE_SELECTOR);
    if (interactive && rowElement.contains(interactive) && interactive !== rowElement) {
      e.stopPropagation();
    }
  };

  // Capture phase so we run before Blazor's bubble-phase @onclick binding.
  rowElement.addEventListener('click', handler, { capture: true });

  return {
    dispose: () => {
      rowElement.removeEventListener('click', handler, { capture: true });
    }
  };
}

/**
 * Prevents Space and Arrow keys from scrolling when a table row is focused.
 * Attaches a keydown listener in capture phase.
 * @param {HTMLElement} element - The row element to attach the handler to
 * @returns {Object} Object with dispose function for cleanup
 */
export function preventSpaceKeyScroll(element) {
    if (!element) return { dispose: () => {} };

    const handleKeyDown = (e) => {
        // Prevent Space, ArrowUp, and ArrowDown from scrolling
        if (e.key === ' ' || e.keyCode === 32 ||
            e.key === 'ArrowUp' || e.keyCode === 38 ||
            e.key === 'ArrowDown' || e.keyCode === 40) {
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

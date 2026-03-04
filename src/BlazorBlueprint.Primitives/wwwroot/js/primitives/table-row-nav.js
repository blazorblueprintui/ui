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
 * Attaches a capture-phase click listener on the row that flags clicks
 * originating from interactive child elements.  Instead of calling
 * stopPropagation (which would prevent Blazor's root-level event
 * delegation from seeing the event at all), we set a property on the
 * row element that the C# HandleClick can read via JS interop.
 *
 * Library-owned interactive elements (expand button, selection checkbox)
 * already use Blazor's @onclick:stopPropagation="true" to suppress the
 * row handler through Blazor's internal dispatch.  This interceptor
 * handles user-provided interactive content in cell templates.
 *
 * @param {HTMLElement} rowElement - The <tr> element
 * @returns {{ dispose(): void }} Cleanup handle
 */
export function interceptInteractiveClicks(rowElement) {
  if (!rowElement) return { dispose: () => {} };

  const handler = (e) => {
    const interactive = e.target.closest(INTERACTIVE_SELECTOR);
    rowElement._bbInteractiveClick = !!(interactive && rowElement.contains(interactive) && interactive !== rowElement);
  };

  // Capture phase so the flag is set before Blazor dispatches the row click.
  rowElement.addEventListener('click', handler, { capture: true });

  return {
    dispose: () => {
      rowElement.removeEventListener('click', handler, { capture: true });
    }
  };
}

/**
 * Returns true if the last click on this row targeted an interactive
 * child element, then resets the flag.
 *
 * @param {HTMLElement} rowElement - The <tr> element
 * @returns {boolean}
 */
export function consumeInteractiveClickFlag(rowElement) {
  if (!rowElement) return false;
  const flag = rowElement._bbInteractiveClick === true;
  rowElement._bbInteractiveClick = false;
  return flag;
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
    while (prevRow && prevRow.getAttribute('tabindex') !== '0') {
        prevRow = prevRow.previousElementSibling;
    }
    prevRow?.focus();
}

/**
 * Moves focus to the next focusable row.
 * Skips siblings without tabindex="0" (detail rows, non-navigable rows, etc.).
 * @param {HTMLElement} element - The current row element
 */
export function moveFocusToNextRow(element) {
    if (!element) return;

    let nextRow = element.nextElementSibling;
    while (nextRow && nextRow.getAttribute('tabindex') !== '0') {
        nextRow = nextRow.nextElementSibling;
    }
    nextRow?.focus();
}

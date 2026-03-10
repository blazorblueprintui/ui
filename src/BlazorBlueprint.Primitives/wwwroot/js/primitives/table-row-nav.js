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
 * Prevents Space and Arrow keys from scrolling when a table row is focused,
 * and sets a flag when the keydown originates from an interactive child so
 * the Blazor-side handler can skip row-level navigation.
 *
 * Skips prevention when the event originates from an interactive child element
 * (e.g. a Combobox dropdown or Popover inside a cell template) so those
 * elements retain normal keyboard behaviour.
 * Attaches a keydown listener in capture phase.
 * @param {HTMLElement} element - The row element to attach the handler to
 * @returns {Object} Object with dispose function for cleanup
 */
export function preventSpaceKeyScroll(element) {
    if (!element) return { dispose: () => {} };

    const handleKeyDown = (e) => {
        // Flag interactive child events so Blazor's HandleKeyDown can skip
        element._bbInteractiveKeyDown = isInteractiveTarget(e.target, element);

        // Let interactive child elements handle their own keys
        if (element._bbInteractiveKeyDown) return;

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
 * Returns true if the last keydown on this row targeted an interactive
 * child element, then resets the flag.
 *
 * @param {HTMLElement} rowElement - The <tr> element
 * @returns {boolean}
 */
export function consumeInteractiveKeyDownFlag(rowElement) {
    if (!rowElement) return false;
    const flag = rowElement._bbInteractiveKeyDown === true;
    rowElement._bbInteractiveKeyDown = false;
    return flag;
}

/**
 * Checks whether the event target is an interactive child of the row,
 * or is inside a portal-based overlay (popover, combobox dropdown, etc.)
 * that was triggered from within the row.
 * @param {HTMLElement} target - The event target
 * @param {HTMLElement} rowElement - The <tr> row element
 * @returns {boolean}
 */
function isInteractiveTarget(target, rowElement) {
    if (!target || target === rowElement) return false;

    // Check if the target is inside a portal overlay (rendered outside the row)
    if (!rowElement.contains(target)) return false;

    // Check if the target (or an ancestor within the row) is interactive
    const interactive = target.closest(INTERACTIVE_SELECTOR);
    return !!(interactive && rowElement.contains(interactive) && interactive !== rowElement);
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

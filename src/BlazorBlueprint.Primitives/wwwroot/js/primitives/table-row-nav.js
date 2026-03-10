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
 *
 * When the keydown originates from an interactive child element (e.g. a
 * Combobox trigger, Popover button, or any element matching
 * INTERACTIVE_SELECTOR), the handler:
 *   1. Skips preventDefault() so the child retains default browser behaviour.
 *   2. Calls stopPropagation() in bubble phase so Blazor's root-level event
 *      delegation never dispatches the event to the row's @onkeydown handler.
 *
 * This is entirely JS-based — no C# interop round-trip is needed.
 *
 * @param {HTMLElement} element - The row element to attach the handler to
 * @returns {Object} Object with dispose function for cleanup
 */
export function preventSpaceKeyScroll(element) {
    if (!element) return { dispose: () => {} };

    // Capture phase: runs before the event reaches the target.
    // - For interactive children: skip preventDefault so the child keeps
    //   normal behaviour, and set a flag for the bubble handler.
    // - For the row itself: preventDefault to stop page scroll.
    const captureHandler = (e) => {
        element._bbInteractiveKeyDown = isInteractiveTarget(e.target, element);

        if (element._bbInteractiveKeyDown) {
            return;
        }

        if (e.key === ' ' || e.keyCode === 32 ||
            e.key === 'ArrowUp' || e.keyCode === 38 ||
            e.key === 'ArrowDown' || e.keyCode === 40) {
            e.preventDefault();
        }
    };

    // Bubble phase: runs after the target has handled the event.
    // When the flag is set, stopPropagation prevents the event from
    // reaching Blazor's document-level event delegation, so the row's
    // C# HandleKeyDown is never invoked for interactive-child events.
    const bubbleHandler = (e) => {
        if (element._bbInteractiveKeyDown) {
            e.stopPropagation();
            element._bbInteractiveKeyDown = false;
        }
    };

    element.addEventListener('keydown', captureHandler, { capture: true });
    element.addEventListener('keydown', bubbleHandler, { capture: false });

    return {
        dispose: () => {
            element.removeEventListener('keydown', captureHandler, { capture: true });
            element.removeEventListener('keydown', bubbleHandler, { capture: false });
        }
    };
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

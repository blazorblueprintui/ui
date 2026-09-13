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
            element._bbInteractiveKeyDown = false;

            // A row being edited needs Enter and Escape to reach Blazor, because that is how the
            // edit is committed or discarded from inside an input. Stopping propagation here
            // would block every Blazor handler in the row, not just the row's own, because Blazor
            // listens at the document. The row's C# handler knows it is editing and does not run
            // its selection shortcuts for these keys.
            if (element.dataset.editing === 'true' && (e.key === 'Enter' || e.key === 'Escape')) {
                return;
            }

            e.stopPropagation();
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
 * Attaches the row key and click behaviour once for a whole grid, instead of once per row.
 *
 * Every row used to register its own listeners, which meant one interop call — and so one circuit
 * round trip on Blazor Server — per row. Measured on the demo, a 465-row grid sent 240
 * client-to-server messages on load against 23 for a page with no grid, and the count tracked the
 * row count.
 *
 * The listeners now live on the grid container and find the row at event time. Rows opt in by
 * rendering `data-bb-row-keys` and `data-bb-row-click`, so a row that wants neither is still
 * skipped — without anyone calling into JavaScript.
 *
 * Phases are unchanged relative to the event target: capture on an ancestor still runs before the
 * target, and bubble on an ancestor still runs after the target and before Blazor's document-level
 * dispatch, which is the ordering both handlers depend on.
 *
 * @param {HTMLElement} container - An ancestor of every row in the grid.
 * @returns {{ dispose(): void }} Cleanup handle.
 */
export function delegateRowBehaviour(container) {
    if (!container) return { dispose: () => {} };

    const rowFor = (target, attribute) => {
        if (!target || typeof target.closest !== 'function') return null;
        const row = target.closest(`[${attribute}]`);
        return row && container.contains(row) ? row : null;
    };

    const keyCapture = (e) => {
        const row = rowFor(e.target, 'data-bb-row-keys');
        if (!row) return;

        row._bbInteractiveKeyDown = isInteractiveTarget(e.target, row);

        if (row._bbInteractiveKeyDown) {
            return;
        }

        if (e.key === ' ' || e.keyCode === 32 ||
            e.key === 'ArrowUp' || e.keyCode === 38 ||
            e.key === 'ArrowDown' || e.keyCode === 40) {
            e.preventDefault();
        }
    };

    const keyBubble = (e) => {
        const row = rowFor(e.target, 'data-bb-row-keys');
        if (!row || !row._bbInteractiveKeyDown) return;

        row._bbInteractiveKeyDown = false;

        // A row being edited needs Enter and Escape to reach Blazor, because that is how the edit
        // is committed or discarded from inside an input. Stopping propagation here would block
        // every Blazor handler in the row, not just the row's own, because Blazor listens at the
        // document. The row's C# handler knows it is editing and does not run its selection
        // shortcuts for these keys.
        if (row.dataset.editing === 'true' && (e.key === 'Enter' || e.key === 'Escape')) {
            return;
        }

        e.stopPropagation();
    };

    const clickCapture = (e) => {
        const row = rowFor(e.target, 'data-bb-row-click');
        if (!row) return;

        const interactive = e.target.closest(INTERACTIVE_SELECTOR);
        row._bbInteractiveClick = !!(interactive && row.contains(interactive) && interactive !== row);
    };

    container.addEventListener('keydown', keyCapture, { capture: true });
    container.addEventListener('keydown', keyBubble, { capture: false });
    container.addEventListener('click', clickCapture, { capture: true });

    return {
        dispose: () => {
            container.removeEventListener('keydown', keyCapture, { capture: true });
            container.removeEventListener('keydown', keyBubble, { capture: false });
            container.removeEventListener('click', clickCapture, { capture: true });
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

/**
 * Blurs whatever is focused inside the row, so an input that only writes its value back on
 * change has done so before the row is committed.
 *
 * Pressing Enter runs the row's keydown handler before the browser fires the input's change
 * event, so committing straight away would drop whatever the user typed into the field they were
 * still in. Blurring first forces that change through.
 *
 * @param {HTMLElement} rowElement - The <tr> row element
 * @returns {boolean} True when something inside the row was focused and has been blurred.
 */
export function blurFocusedInput(rowElement) {
    const active = document.activeElement;
    if (!rowElement || !active || active === document.body || !rowElement.contains(active)) {
        return false;
    }

    active.blur();
    return true;
}

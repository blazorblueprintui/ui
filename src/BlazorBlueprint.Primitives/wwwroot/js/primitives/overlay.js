/**
 * Opening and closing a floating overlay, each in a single interop call.
 *
 * Why this module exists
 * ----------------------
 * Every `InvokeAsync` from C# on Blazor Server is a message the server posts to the browser and
 * then awaits, so it costs a network round trip. Opening an overlay used to spend three of them
 * back to back — compute the position, apply it and reveal the element, then start auto-update —
 * with a Blazor re-render for the resolved placement wedged in the middle, before the element was
 * visible. None of those steps needs the server's opinion: they are all decisions the browser can
 * make on its own, and C# only needs the answer at the end.
 *
 * `open()` does the lot in one hop and returns the resolved position, so the element is on screen
 * by the time C# hears back. `close()` is the mirror: stop auto-update and run the exit animation
 * in one hop rather than two.
 *
 * This is paid on every open, not just the first, which is what makes it worth the indirection.
 */

import * as clickOutside from './click-outside.js';
import * as escapeKeydown from './escape-keydown.js';
import * as positioning from './positioning.js';

/** portalId -> cleanup functions to run when the overlay closes. */
const openOverlays = new Map();

/**
 * Positions a floating element against its anchor, reveals it, and keeps it positioned.
 *
 * @param {string} portalId - Identifies the overlay, so close() can find its cleanups.
 * @param {HTMLElement} reference - The anchor element to position against.
 * @param {HTMLElement} floating - The portal content element to position.
 * @param {Object} options - Positioning options: placement, offset, flip, shift, padding,
 *   strategy, matchReferenceWidth. Set `autoUpdate: false` to skip the scroll/resize watcher.
 *   `dismiss` optionally asks for dismissal listeners: { contentId, triggerId,
 *   onOutsideInteraction, onEscapeKey }.
 * @param {Object} dismissRef - .NET reference the dismissal listeners call back into.
 * @returns {Promise<Object>} The resolved position — x, y, placement, transformOrigin, strategy.
 */
export async function open(portalId, reference, floating, options = {}, dismissRef = null) {
    // A reopen while the previous open is still wired would leak its auto-update listener.
    runCleanups(portalId);

    const position = await positioning.computePosition(reference, floating, options);
    positioning.applyPosition(floating, position, true);

    const cleanups = [];

    if (options.autoUpdate !== false) {
        // autoUpdate repositions on scroll and resize. Its own first update runs here and is
        // harmless — applyPosition without makeVisible writes coordinates only, never visibility.
        const handle = await positioning.autoUpdate(reference, floating, options);
        cleanups.push(() => handle.apply());
    }

    // Wired here rather than by the owner after the fact. Each listener the owner registered
    // itself cost another round trip, and until they landed the overlay was visible but did not
    // respond to a click outside it.
    const dismiss = options.dismiss;
    if (dismiss && dismissRef) {
        if (dismiss.onOutsideInteraction) {
            const handle = clickOutside.onClickOutsideByIds(
                dismiss.contentId, dismissRef, 'JsOnDismissOutside', dismiss.triggerId);
            cleanups.push(() => handle.dispose());
        }

        if (dismiss.onEscapeKey) {
            // The shared stack, not a listener of our own: Escape must dismiss the topmost overlay
            // only. A popover opened inside a dialog is above it and goes first; the dialog is
            // still there for the next press.
            escapeKeydown.initialize(dismissRef, portalId, 'JsOnDismissEscape');
            cleanups.push(() => escapeKeydown.dispose(portalId));
        }
    }

    openOverlays.set(portalId, cleanups);

    return position;
}

/**
 * Stops keeping the overlay positioned and returns it to its hidden state, waiting for any exit
 * animation on the way.
 *
 * @param {string} portalId - The overlay to close.
 * @param {HTMLElement} floating - The portal content element, or null to only drop the listeners.
 */
export async function close(portalId, floating) {
    runCleanups(portalId);

    if (floating) {
        await positioning.hidePosition(floating);
    }
}

/**
 * Whether this overlay is currently wired up. Lets C# skip a close for something never opened.
 *
 * @param {string} portalId - The overlay to check.
 * @returns {boolean} True if open() has run and close() has not.
 */
export function isOpen(portalId) {
    return openOverlays.has(portalId);
}

function runCleanups(portalId) {
    const cleanups = openOverlays.get(portalId);
    if (!cleanups) {
        return;
    }

    openOverlays.delete(portalId);

    for (const cleanup of cleanups) {
        try {
            cleanup();
        } catch (error) {
            // One failed listener must not strand the others, or the overlay leaks them.
            console.error('overlay: cleanup failed', error);
        }
    }
}

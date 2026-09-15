/**
 * Opening and closing a floating overlay, each in a single interop call that nothing awaits.
 *
 * Why this module exists
 * ----------------------
 * Every `InvokeAsync` from C# on Blazor Server is a message the server posts to the browser and
 * then awaits, so it costs a network round trip. Opening an overlay used to spend three of them
 * back to back — compute the position, apply it and reveal the element, then start auto-update —
 * with a Blazor re-render for the resolved placement wedged in the middle, before the element was
 * visible. None of those steps needs the server's opinion: they are all decisions the browser can
 * make on its own.
 *
 * Folding them into one call removed two of the three, and the remaining one still cost a round
 * trip because C# awaited the answer. It no longer does. `open()` is dispatched with
 * `InvokeVoidAsync` from the same synchronous pass that renders the overlay's content, so the
 * `BeginInvokeJS` message rides out in the same flush as the render batch and the server never
 * waits for a reply.
 *
 * What that costs, and how it is paid
 * -----------------------------------
 * Fire-and-forget means the message arrives *before* the browser has applied the render batch
 * carrying the content element, because the server sent it first. C# therefore cannot hand over an
 * `ElementReference` for content that does not exist yet — argument deserialisation would throw on
 * the client before this module's code ran. The content element is named instead, by the
 * `data-bb-portal` attribute the portal always renders, and resolved here with a wait. The wait is
 * client-side and costs nothing; it is over within a frame, because the render batch is the very
 * next message on the wire.
 *
 * Nothing awaits the result, so nothing reports a failure either. Every path in here logs.
 */

import * as clickOutside from './click-outside.js';
import * as escapeKeydown from './escape-keydown.js';
import * as positioning from './positioning.js';
import * as menuKeyboard from './menu-keyboard.js';
import * as select from './select.js';

/** portalId -> cleanup functions to run when the overlay closes. */
const openOverlays = new Map();

/**
 * portalId -> monotonic token for the most recent open/close.
 *
 * `open()` is asynchronous inside — it waits for the element, then measures it — while the C# side
 * has already moved on. A close arriving during that window used to hide an element that `open()`
 * then revealed a moment later, leaving an overlay on screen that the user had dismissed. Each
 * call takes a token on entry and abandons itself if a later call has taken one since.
 */
const sequence = new Map();

/** How long to wait for the render batch carrying the portal content to be applied. */
const CONTENT_WAIT_MS = 2000;

function nextToken(portalId) {
    const token = (sequence.get(portalId) ?? 0) + 1;
    sequence.set(portalId, token);
    return token;
}

function isCurrent(portalId, token) {
    return sequence.get(portalId) === token;
}

/**
 * Finds a portal's content element, waiting for the render batch that carries it.
 *
 * @param {string} portalId - The portal to find.
 * @param {number} timeoutMs - How long to wait before giving up.
 * @returns {Promise<HTMLElement|null>} The content element, or null if it never arrived.
 */
function waitForContent(portalId, timeoutMs = CONTENT_WAIT_MS) {
    const selector = `[data-bb-portal="${CSS.escape(portalId)}"]`;

    const found = document.querySelector(selector);
    if (found) {
        return Promise.resolve(found);
    }

    return new Promise(resolve => {
        let settled = false;

        const finish = element => {
            if (settled) return;
            settled = true;
            observer.disconnect();
            clearTimeout(timer);
            resolve(element);
        };

        // A MutationObserver rather than a poll: Blazor applies the whole render batch in one
        // synchronous pass, so the element appears in a single mutation and this resolves in the
        // same task. A 10ms poll would add up to 10ms of dead time to every open.
        const observer = new MutationObserver(() => {
            const element = document.querySelector(selector);
            if (element) {
                finish(element);
            }
        });

        observer.observe(document.body, { childList: true, subtree: true });

        const timer = setTimeout(() => {
            console.warn(`overlay: portal content '${portalId}' did not render within ${timeoutMs}ms`);
            finish(null);
        }, timeoutMs);

        // The element can land between the querySelector above and the observer starting.
        const late = document.querySelector(selector);
        if (late) {
            finish(late);
        }
    });
}

/**
 * Positions a floating element against its anchor, reveals it, and keeps it positioned.
 *
 * Call it and walk away — C# does not await this, and must not.
 *
 * @param {string} portalId - Identifies the overlay, so close() can find its cleanups.
 * @param {HTMLElement} reference - The anchor element to position against. Already on screen when
 *   the overlay opens, so this one can still travel as an ElementReference.
 * @param {Object} options - Positioning options: placement, offset, flip, shift, padding,
 *   strategy, matchReferenceWidth. Set `autoUpdate: false` to skip the scroll/resize watcher.
 *   `dismiss` optionally asks for dismissal listeners: { contentId, triggerId,
 *   onOutsideInteraction, onEscapeKey }. `sideElementId` names the element that carries
 *   `data-side`. `reportPlacement` asks for a `JsOnPlacementChanged` callback — a round trip,
 *   so only set it when C# genuinely reads the placement. `listbox` prepares select content:
 *   { kind, contentId, callbackRef, selectedValue, loop, mode }. `scrollToCurrent` scrolls a
 *   marked element into view before the reveal: { containerId, selector }. `autoFocusId` names
 *   an element to focus one frame after the reveal.
 * @param {Object} dotNetRef - .NET reference the listeners and callbacks call back into.
 * @returns {Promise<Object|null>} The resolved position, for the rare caller that awaits.
 */
export async function open(portalId, reference, options = {}, dotNetRef = null) {
    const token = nextToken(portalId);

    try {
        // A reopen while the previous open is still wired would leak its auto-update listener.
        runCleanups(portalId);

        const floating = await waitForContent(portalId);
        if (!floating || !isCurrent(portalId, token)) {
            return null;
        }

        if (!positioning.isElementReady(reference)) {
            console.warn(`overlay: anchor for '${portalId}' is not in the document`);
            return null;
        }

        // Before the reveal, not after. A list must appear already scrolled to the selected
        // option; scrolling it once it is on screen is a visible jump.
        if (options.keyboard) {
            prepareKeyboard(options.keyboard, dotNetRef);
        }

        // Same rule, for content that is not a listbox and marks its chosen item its own way.
        if (options.scrollToCurrent) {
            try {
                select.scrollMarkedIntoView(
                    options.scrollToCurrent.containerId, options.scrollToCurrent.selector);
            } catch (error) {
                console.error('overlay: failed to scroll to the current item', error);
            }
        }

        const position = await positioning.computePosition(reference, floating, options);
        if (!isCurrent(portalId, token)) {
            return null;
        }

        applySide(options.sideElementId, position.placement);
        positioning.applyPosition(floating, position, true);

        // Strictly after the reveal. applyPosition makes the element visible inside a
        // requestAnimationFrame, and focus() on a `visibility: hidden` element is a no-op that
        // throws nothing — so focusing any earlier leaves the listbox unfocused, every arrow key
        // going to the server as a trigger keydown, and the highlight never moving.
        //
        // Listbox only. A menu moves real focus between its items and decides for itself whether
        // to take focus on open, from its own initialFocus setting.
        //
        // autoFocusId is the same idea for any other content — a combobox's search box, say. Its
        // owner used to focus it from the portal's ready callback, which on Blazor Server runs a
        // round trip after the content renders, and then slept 50ms and paid another round trip
        // for FocusAsync. Here it is one frame after the reveal, for free.
        const focusTarget = options.keyboard && options.keyboard.kind === 'Listbox'
            ? () => select.focusListbox(options.keyboard.contentId)
            : options.autoFocusId
                ? () => document.getElementById(options.autoFocusId)?.focus({ preventScroll: true })
                : null;

        if (focusTarget) {
            requestAnimationFrame(() => requestAnimationFrame(() => {
                if (isCurrent(portalId, token)) {
                    focusTarget();
                }
            }));
        }

        const cleanups = [];

        if (options.autoUpdate !== false) {
            // autoUpdate repositions on scroll and resize. Its own first update runs here and is
            // harmless — applyPosition without makeVisible writes coordinates only, never
            // visibility. It also keeps data-side honest when a scroll flips the overlay.
            const handle = await positioning.autoUpdate(reference, floating, {
                ...options,
                onPlacement: placement => applySide(options.sideElementId, placement)
            });

            if (!isCurrent(portalId, token)) {
                handle.apply();
                return null;
            }

            cleanups.push(() => handle.apply());
        }

        // Wired here rather than by the owner after the fact. Each listener the owner registered
        // itself cost another round trip, and until they landed the overlay was visible but did
        // not respond to a click outside it.
        const dismiss = options.dismiss;
        if (dismiss && dotNetRef) {
            if (dismiss.onOutsideInteraction) {
                const handle = clickOutside.onClickOutsideByIds(
                    dismiss.contentId, dotNetRef, 'JsOnDismissOutside', dismiss.triggerId);
                cleanups.push(() => handle.dispose());
            }

            if (dismiss.onEscapeKey) {
                // The shared stack, not a listener of our own: Escape must dismiss the topmost
                // overlay only. A popover opened inside a dialog is above it and goes first; the
                // dialog is still there for the next press.
                escapeKeydown.initialize(dotNetRef, portalId, 'JsOnDismissEscape');
                cleanups.push(() => escapeKeydown.dispose(portalId));
            }
        }

        openOverlays.set(portalId, cleanups);

        // Deliberately last, and opt-in. This is a call back into .NET, which on Blazor Server is
        // the round trip the rest of this function exists to avoid. The overlay is already on
        // screen and usable by the time it goes out, so an owner that needs the placement in C#
        // pays for it without the user waiting on it.
        if (options.reportPlacement && dotNetRef) {
            dotNetRef.invokeMethodAsync('JsOnPlacementChanged', position.placement)
                .catch(error => console.error('overlay: placement callback failed', error));
        }

        return position;
    } catch (error) {
        // Nothing awaits this call, so an error that escapes here is silent — the overlay simply
        // never appears. Logging is the only report there is.
        console.error(`overlay: failed to open '${portalId}'`, error);
        return null;
    }
}

/**
 * Stops keeping the overlay positioned and returns it to its hidden state, waiting for any exit
 * animation on the way.
 *
 * Like open(), C# does not await this. The overlay is visually gone as soon as the exit animation
 * finishes, which is a browser-side cost and not a network one.
 *
 * @param {string} portalId - The overlay to close.
 * @param {Object} options - `notifyClosed` asks for a `JsOnClosed` callback once the element is
 *   hidden, for owners that unmount their content afterwards. `keyboard` names content whose key
 *   handlers should be released. `restoreFocusToId` names the element to focus once hidden.
 * @param {Object} dotNetRef - .NET reference for the `notifyClosed` callback.
 */
export async function close(portalId, options = {}, dotNetRef = null) {
    const token = nextToken(portalId);

    try {
        runCleanups(portalId);

        // Folded in rather than left to the owner. Releasing these handlers was its own awaited
        // interop call on the way out — a round trip spent removing an event listener from an
        // overlay the user had already dismissed.
        if (options.keyboard) {
            try {
                if (options.keyboard.kind === 'Menu') {
                    menuKeyboard.dispose(options.keyboard.contentId);
                } else {
                    select.cleanupKeyboardNavigation(options.keyboard.contentId);
                }
            } catch (error) {
                console.error('overlay: failed to release keyboard handlers', error);
            }
        }

        // Already gone, or never opened. Still worth telling the owner, which may be waiting for
        // this to unmount its content.
        const floating = document.querySelector(`[data-bb-portal="${CSS.escape(portalId)}"]`);

        if (floating) {
            await positioning.hidePosition(floating);
        }

        if (!isCurrent(portalId, token)) {
            // Reopened while the exit animation ran. The overlay is on screen again and its
            // content must stay mounted.
            return;
        }

        // Focus goes back to the trigger here, for a close the user asked for with Escape or a
        // selection. The owner used to do this from C# after its close render, as an awaited
        // FocusAsync — a round trip spent putting focus somewhere the browser could put it
        // itself. A click outside does not restore: focus stays where the user clicked.
        if (options.restoreFocusToId) {
            const target = document.getElementById(options.restoreFocusToId);
            if (target) {
                try {
                    target.focus({ preventScroll: true });
                } catch (error) {
                    // A trigger that is no longer focusable is not worth a console error.
                }
            }
        }

        if (options.notifyClosed && dotNetRef) {
            // Off the critical path by design: the overlay is already invisible, and this only
            // releases the DOM behind it.
            dotNetRef.invokeMethodAsync('JsOnClosed')
                .catch(error => console.error('overlay: close callback failed', error));
        }
    } catch (error) {
        console.error(`overlay: failed to close '${portalId}'`, error);
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

/**
 * Writes the resolved side onto the element that styles itself from it.
 *
 * C# used to own this attribute, and paid a render — a round trip on Server — to write it, wedged
 * between computing the position and revealing the element. The side is a browser measurement, so
 * the browser writes it.
 *
 * @param {string|null} elementId - The element carrying data-side, if the owner named one.
 * @param {string} placement - The resolved placement, e.g. "top-start".
 */
function applySide(elementId, placement) {
    if (!elementId || !placement) {
        return;
    }

    const element = document.getElementById(elementId);
    if (element) {
        element.setAttribute('data-side', placement.split('-')[0]);
    }
}

/**
 * Wires the content's keyboard behaviour, inside the call that opens the overlay.
 *
 * It was a separately awaited interop call from the owner, so on Blazor Server the overlay
 * appeared and only became keyboard-usable a round trip later.
 *
 * @param {Object} keyboard - { kind, contentId, callbackRef, selectedValue, loop, mode }.
 * @param {Object} dotNetRef - The portal's reference, used only if the owner named none.
 */
function prepareKeyboard(keyboard, dotNetRef) {
    // The content's own reference, falling back to the portal's. Escape on a listbox or a menu
    // calls the owner's methods, not the portal's.
    const ref = keyboard.callbackRef || dotNetRef;
    if (!ref) {
        return;
    }

    try {
        if (keyboard.kind === 'Menu') {
            const container = document.getElementById(keyboard.contentId);
            if (container) {
                menuKeyboard.initialize(container, ref, keyboard.contentId, {
                    mode: keyboard.mode || 'vertical',
                    loop: keyboard.loop !== false
                });
            }
            return;
        }

        // autoFocus off: the listbox is still parked and invisible here. open() focuses it once it
        // has revealed it.
        select.openListbox(keyboard.contentId, keyboard.selectedValue, true, ref);
    } catch (error) {
        console.error('overlay: failed to wire keyboard handling', error);
    }
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

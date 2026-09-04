// Native <dialog> rendering helper for Blazor components.
// Drives the browser's built-in <dialog> element (top layer, focus trap, Escape,
// ::backdrop) instead of the JS focus-trap/scroll-lock/portal handshake. This
// removes the scoped-service + render-handshake dependency that breaks portaled
// overlays across Blazor render-mode boundaries (e.g. InteractiveWebAssembly).
//
// NOTE: this module deliberately uses only function declarations and globalThis
// state — no top-level `let`/`const`/`class`. Blazor WebAssembly's dynamic
// `import()` can re-evaluate a module in a shared scope, and top-level lexical
// bindings then collide with "Identifier has already been declared". Function
// declarations and globalThis assignments survive that re-evaluation safely.

/**
 * Detects whether the browser supports the native <dialog> element with showModal().
 * Baseline: Chrome 37+, Firefox 98+, Safari 15.4+. Cached after the first call.
 * @returns {boolean}
 */
export function supportsNativeDialog() {
    if (globalThis.__bbSupportsNativeDialog !== undefined) {
        return globalThis.__bbSupportsNativeDialog;
    }
    globalThis.__bbSupportsNativeDialog = typeof HTMLDialogElement !== 'undefined'
        && typeof HTMLDialogElement.prototype.showModal === 'function';
    return globalThis.__bbSupportsNativeDialog;
}

/**
 * Opens a <dialog> element as a modal (top layer). No-op if already open.
 * @param {HTMLDialogElement} element
 */
export function showModal(element) {
    if (!element) {
        return;
    }
    if (supportsNativeDialog() && !element.open) {
        element.showModal();
    }
}

/**
 * Closes a <dialog> element. No-op if not open.
 * @param {HTMLDialogElement} element
 * @param {string|null} [returnValue]
 */
export function closeDialog(element, returnValue) {
    if (!element) {
        return;
    }
    if (supportsNativeDialog() && element.open) {
        element.close(returnValue ?? null);
    }
}

/**
 * Focuses the dialog itself (the browser's native focus trap already confines
 * Tab within a modal dialog). Falls back to the first focusable element.
 * @param {HTMLDialogElement} element
 */
export function focusDialog(element) {
    if (!element) {
        return;
    }
    if (document.activeElement === element) {
        return;
    }
    element.focus();
    if (document.activeElement !== element) {
        const focusable = element.querySelector(
            'button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), '
            + 'textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'
        );
        if (focusable) {
            focusable.focus();
        }
    }
}

/**
 * Focuses the element that opened the dialog, restoring the trigger's focus.
 * @param {HTMLElement} element
 */
export function focusElement(element) {
    if (element && typeof element.focus === 'function') {
        element.focus();
    }
}

// ============================================================================
// Native <dialog> event wiring
// Blazor cannot reliably distinguish a backdrop click from a content click, and
// Escape (cancel) must be preventable so the component can honour CloseOnEscape.
// These listeners forward the native events to .NET, which owns the close logic.
// ============================================================================

/**
 * Wires native dialog lifecycle events to a .NET instance.
 * Expects the .NET object to expose JSInvokable methods:
 *   JsOnNativeCancel()          - Escape pressed (cancel is prevented here)
 *   JsOnNativeClose()           - dialog closed (safety net)
 *   JsOnNativeBackdropClick()   - click landed on the dialog itself (backdrop)
 * @param {HTMLDialogElement} dialog
 * @param {object} dotNetRef - a DotNetObjectReference
 * @returns {{ dispose: () => void }}
 */
export function setupDialog(dialog, dotNetRef) {
    const onCancel = (e) => {
        e.preventDefault();
        dotNetRef.invokeMethodAsync('JsOnNativeCancel');
    };
    const onClose = () => {
        dotNetRef.invokeMethodAsync('JsOnNativeClose');
    };
    const onClick = (e) => {
        // A native modal dialog fills the top layer, so both a click on the ::backdrop
        // (outside the dialog) and a click on the dialog's own box (including its padding)
        // resolve to e.target === dialog. Only the former is a backdrop dismissal; a click
        // inside the dialog — padding included — must not close it. Compare the click point
        // against the dialog's border box to tell them apart (the JS path renders content and
        // overlay as separate elements, so it never conflates these).
        if (e.target !== dialog) {
            return;
        }
        const rect = dialog.getBoundingClientRect();
        const x = e.clientX;
        const y = e.clientY;
        const insideBox = x >= rect.left && x <= rect.right && y >= rect.top && y <= rect.bottom;
        if (!insideBox) {
            dotNetRef.invokeMethodAsync('JsOnNativeBackdropClick');
        }
    };

    dialog.addEventListener('cancel', onCancel);
    dialog.addEventListener('close', onClose);
    dialog.addEventListener('click', onClick);

    return {
        dispose: () => {
            dialog.removeEventListener('cancel', onCancel);
            dialog.removeEventListener('close', onClose);
            dialog.removeEventListener('click', onClick);
        }
    };
}

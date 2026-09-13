// Focus trap implementation for modal dialogs and overlays
// Based on accessibility best practices for focus management

const focusableSelectors = [
    'a[href]',
    'button:not([disabled])',
    'input:not([disabled])',
    'textarea:not([disabled])',
    'select:not([disabled])',
    '[tabindex]:not([tabindex="-1"])',
    'audio[controls]',
    'video[controls]'
].join(', ');

/**
 * Creates a focus trap within the specified container element.
 *
 * The initial focus target is resolved in this order, so that a caller can always win over
 * the default without having to inject a dummy tab stop:
 *   1. an explicit element passed as `initialFocusElement`
 *   2. `[data-autofocus]` within the container - the same convention portal.js already honours
 *   3. `mode`: 'container' focuses the container itself, 'none' leaves focus where it is
 *   4. the first tabbable descendant - the historical behaviour, and still the default
 *
 * @param {HTMLElement} container - The container element to trap focus within
 * @param {'first'|'container'|'none'} [mode='first'] - What to focus when no explicit target
 *   or [data-autofocus] element is present
 * @param {HTMLElement} [initialFocusElement=null] - Explicit element to focus, beating every
 *   other rule
 * @returns {Object} Object with an `apply` cleanup function, for IJSObjectReference
 */
export function createFocusTrap(container, mode = 'first', initialFocusElement = null) {
    if (!container) {
        console.warn('Focus trap: container is null or undefined');
        return () => {};
    }

    const getFocusableElements = () => {
        return Array.from(container.querySelectorAll(focusableSelectors))
            .filter(el => {
                const style = window.getComputedStyle(el);
                return style.display !== 'none' && style.visibility !== 'hidden';
            });
    };

    const handleKeyDown = (e) => {
        if (e.key !== 'Tab') return;

        const focusableElements = getFocusableElements();
        if (focusableElements.length === 0) return;

        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];

        // Shift + Tab on first element: focus last
        if (e.shiftKey && document.activeElement === firstElement) {
            e.preventDefault();
            lastElement.focus();
        }
        // Tab on last element: focus first
        else if (!e.shiftKey && document.activeElement === lastElement) {
            e.preventDefault();
            firstElement.focus();
        }
    };

    container.addEventListener('keydown', handleKeyDown);

    // Resolve the initial focus target. `container.contains` guards an explicit element that
    // is not actually inside the trap, which would put focus outside the thing being trapped.
    const resolveInitialFocus = () => {
        if (initialFocusElement && container.contains(initialFocusElement)) {
            return initialFocusElement;
        }

        const autofocusTarget = container.matches('[data-autofocus]')
            ? container
            : container.querySelector('[data-autofocus]');
        if (autofocusTarget) {
            return autofocusTarget;
        }

        if (mode === 'none') return null;
        if (mode === 'container') return container;

        return getFocusableElements()[0] ?? null;
    };

    const initialTarget = resolveInitialFocus();
    if (initialTarget) {
        initialTarget.focus();
    }

    // Return cleanup function wrapped in object for C# IJSObjectReference
    const cleanup = () => {
        container.removeEventListener('keydown', handleKeyDown);
    };

    return {
        apply: cleanup
    };
}

/**
 * Focuses the first focusable element in the container.
 * @param {HTMLElement} container - The container to search
 */
export function focusFirst(container) {
    if (!container) return;

    const focusableElements = Array.from(container.querySelectorAll(focusableSelectors))
        .filter(el => {
            const style = window.getComputedStyle(el);
            return style.display !== 'none' && style.visibility !== 'hidden';
        });

    if (focusableElements.length > 0) {
        focusableElements[0].focus();
    }
}

/**
 * Focuses the last focusable element in the container.
 * @param {HTMLElement} container - The container to search
 */
export function focusLast(container) {
    if (!container) return;

    const focusableElements = Array.from(container.querySelectorAll(focusableSelectors))
        .filter(el => {
            const style = window.getComputedStyle(el);
            return style.display !== 'none' && style.visibility !== 'hidden';
        });

    if (focusableElements.length > 0) {
        focusableElements[focusableElements.length - 1].focus();
    }
}

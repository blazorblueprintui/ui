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
 * @param {HTMLElement} container - The container element to trap focus within
 * @returns {Function} Cleanup function to remove the focus trap
 */
export function createFocusTrap(container) {
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

    // Focus first element on initialization
    const focusableElements = getFocusableElements();
    if (focusableElements.length > 0) {
        focusableElements[0].focus();
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

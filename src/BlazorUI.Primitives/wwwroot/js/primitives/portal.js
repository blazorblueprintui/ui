// Portal rendering helper for Blazor components
// Assists with rendering content at document body level

/**
 * Creates a portal container at document body level if it doesn't exist.
 * @param {string} id - The portal container ID
 * @returns {HTMLElement} The portal container element
 */
export function ensurePortalContainer(id = 'blazorui-portal-root') {
    let container = document.getElementById(id);

    if (!container) {
        container = document.createElement('div');
        container.id = id;
        container.style.position = 'fixed';
        container.style.zIndex = '9999';
        container.style.top = '0';
        container.style.left = '0';
        container.style.pointerEvents = 'none'; // Allow clicks to pass through empty areas
        document.body.appendChild(container);
    }

    return container;
}

/**
 * Removes a portal container if it's empty.
 * @param {string} id - The portal container ID
 */
export function cleanupPortalContainer(id = 'blazorui-portal-root') {
    const container = document.getElementById(id);

    if (container && container.children.length === 0) {
        container.remove();
    }
}

/**
 * Sets up portal rendering for an element.
 * Moves the element to document.body level to escape parent stacking contexts.
 * @param {HTMLElement} element - The element to portal
 * @param {string} portalId - The portal container ID
 * @returns {Object} Disposable object with dispose() method
 */
export function setupPortal(element, portalId = 'blazorui-portal-root') {
    if (!element) {
        console.warn('setupPortal: element is null');
        return { dispose: () => {} };
    }

    const originalParent = element.parentElement;
    const originalNextSibling = element.nextSibling;

    const portal = ensurePortalContainer(portalId);
    portal.appendChild(element);

    // Enable pointer events on the element
    element.style.pointerEvents = 'auto';

    return {
        dispose: () => {
            // Move element back to original location for proper Blazor disposal
            if (originalParent && document.body.contains(element)) {
                if (originalNextSibling && originalNextSibling.parentElement === originalParent) {
                    originalParent.insertBefore(element, originalNextSibling);
                } else {
                    originalParent.appendChild(element);
                }
            }

            // Clean up portal container if empty
            if (portal && portal.children.length === 0 && portal.parentElement) {
                portal.remove();
            }
        }
    };
}

/**
 * Gets the computed z-index for proper stacking.
 * @param {HTMLElement} element - The element to check
 * @returns {number} The computed z-index
 */
export function getComputedZIndex(element) {
    const computed = window.getComputedStyle(element);
    const zIndex = parseInt(computed.zIndex, 10);
    return isNaN(zIndex) ? 0 : zIndex;
}

/**
 * Locks body scroll (useful for modals).
 * @returns {Object} Object with cleanup method to restore scroll
 */
export function lockBodyScroll() {
    const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
    const originalOverflow = document.body.style.overflow;
    const originalPaddingRight = document.body.style.paddingRight;

    document.body.style.overflow = 'hidden';

    // Prevent layout shift by adding padding for scrollbar
    if (scrollbarWidth > 0) {
        document.body.style.paddingRight = `${scrollbarWidth}px`;
    }

    // Return cleanup function wrapped in object for C# interop
    const cleanup = () => {
        document.body.style.overflow = originalOverflow;
        document.body.style.paddingRight = originalPaddingRight;
    };

    return {
        apply: cleanup
    };
}

/**
 * Checks if an element is currently visible in the viewport.
 * @param {HTMLElement} element - The element to check
 * @returns {boolean} True if element is visible
 */
export function isElementInViewport(element) {
    const rect = element.getBoundingClientRect();

    return (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
        rect.right <= (window.innerWidth || document.documentElement.clientWidth)
    );
}

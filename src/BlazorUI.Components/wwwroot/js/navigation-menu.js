/**
 * Navigation Menu keyboard navigation helper
 * Handles arrow key navigation within the navigation menu dropdown content.
 * Unlike the primitives keyboard-nav.js, this targets focusable links (a[href])
 * rather than role="menuitem" elements, since navigation menus often contain
 * custom content with plain anchor tags.
 */

/**
 * Gets focusable elements in DOM order within a container.
 * Targets anchor tags with href attributes that are visible.
 * @param {HTMLElement} container - The menu container element
 * @returns {HTMLElement[]} Array of focusable elements in DOM order
 */
function getFocusableElements(container) {
    if (!container) return [];

    // Find all anchor tags with href and elements with role="menuitem"
    const elements = Array.from(container.querySelectorAll('a[href], [role="menuitem"]'));

    // Filter out hidden or disabled elements
    return elements.filter(el => {
        if (el.getAttribute('aria-disabled') === 'true') return false;
        if (el.hidden) return false;

        // Check if element is visible
        const style = window.getComputedStyle(el);
        if (style.display === 'none' || style.visibility === 'hidden') return false;

        return true;
    });
}

/**
 * Navigates to the next focusable element in DOM order.
 * @param {HTMLElement} container - The menu container element
 * @returns {HTMLElement|null} The focused element, or null if none
 */
export function navigateNext(container) {
    const items = getFocusableElements(container);
    if (items.length === 0) return null;

    const currentIndex = items.findIndex(item => item === document.activeElement);
    let nextIndex;

    if (currentIndex === -1) {
        nextIndex = 0;
    } else if (currentIndex === items.length - 1) {
        nextIndex = 0; // Loop to first
    } else {
        nextIndex = currentIndex + 1;
    }

    items[nextIndex]?.focus();
    return items[nextIndex] || null;
}

/**
 * Navigates to the previous focusable element in DOM order.
 * @param {HTMLElement} container - The menu container element
 * @returns {HTMLElement|null} The focused element, or null if none
 */
export function navigatePrevious(container) {
    const items = getFocusableElements(container);
    if (items.length === 0) return null;

    const currentIndex = items.findIndex(item => item === document.activeElement);
    let prevIndex;

    if (currentIndex === -1) {
        prevIndex = items.length - 1;
    } else if (currentIndex === 0) {
        prevIndex = items.length - 1; // Loop to last
    } else {
        prevIndex = currentIndex - 1;
    }

    items[prevIndex]?.focus();
    return items[prevIndex] || null;
}

/**
 * Navigates to the first focusable element.
 * @param {HTMLElement} container - The menu container element
 * @returns {HTMLElement|null} The focused element, or null if none
 */
export function navigateFirst(container) {
    const items = getFocusableElements(container);
    if (items.length === 0) return null;

    items[0]?.focus();
    return items[0] || null;
}

/**
 * Navigates to the last focusable element.
 * @param {HTMLElement} container - The menu container element
 * @returns {HTMLElement|null} The focused element, or null if none
 */
export function navigateLast(container) {
    const items = getFocusableElements(container);
    if (items.length === 0) return null;

    const lastIndex = items.length - 1;
    items[lastIndex]?.focus();
    return items[lastIndex] || null;
}

/**
 * Focuses the given element.
 * @param {HTMLElement} element - The element to focus
 */
export function focusElement(element) {
    if (element) {
        element.focus();
    }
}

/**
 * Sets up keyboard navigation on a container.
 * Uses element-level listener to avoid interfering with other components.
 * Returns a cleanup function.
 * @param {HTMLElement} container - The menu container element
 * @param {DotNetObjectReference} dotNetRef - Reference to call back into Blazor
 */
export function setupKeyboardNavigation(container, dotNetRef) {
    const handleKeyDown = (e) => {
        const items = getFocusableElements(container);
        const focusedItemIndex = items.indexOf(document.activeElement);
        const hasFocusedItem = focusedItemIndex !== -1;

        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                if (!hasFocusedItem) {
                    navigateFirst(container);
                } else {
                    navigateNext(container);
                }
                break;
            case 'ArrowUp':
                e.preventDefault();
                if (!hasFocusedItem) {
                    navigateLast(container);
                } else {
                    navigatePrevious(container);
                }
                break;
            case 'Home':
                e.preventDefault();
                navigateFirst(container);
                break;
            case 'End':
                e.preventDefault();
                navigateLast(container);
                break;
            case 'Escape':
                e.preventDefault();
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('HandleEscapeKey');
                }
                break;
        }
    };

    // Element-level listener - only fires when container or its children have focus
    container.addEventListener('keydown', handleKeyDown);

    // Return cleanup object
    return {
        dispose: () => {
            container.removeEventListener('keydown', handleKeyDown);
        }
    };
}

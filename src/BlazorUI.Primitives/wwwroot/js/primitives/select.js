// Select primitive utilities for keyboard navigation, scroll and focus management
// Uses JavaScript-based keyboard handling to avoid race conditions with Blazor rendering

/**
 * Storage for active keyboard handlers by content ID
 */
const activeHandlers = new Map();

/**
 * Gets select option items in DOM order within a container.
 * @param {HTMLElement} container - The select content container element
 * @returns {HTMLElement[]} Array of option elements in DOM order
 */
function getOptionsInDomOrder(container) {
    if (!container) return [];
    return Array.from(container.querySelectorAll('[role="option"]'));
}

/**
 * Gets enabled (not disabled) options in DOM order.
 * @param {HTMLElement} container - The select content container element
 * @returns {HTMLElement[]} Array of enabled option elements in DOM order
 */
function getEnabledOptions(container) {
    return getOptionsInDomOrder(container).filter(item =>
        item.getAttribute('data-disabled') !== 'true' &&
        item.getAttribute('aria-disabled') !== 'true'
    );
}

/**
 * Gets the currently focused option index.
 * @param {HTMLElement} container - The select content container
 * @returns {number} Index of focused option, or -1 if none
 */
function getFocusedIndex(container) {
    const items = getEnabledOptions(container);
    return items.findIndex(item => item.getAttribute('data-focused') === 'true');
}

/**
 * Scrolls an element into view within a scrollable container, without scrolling the page.
 * @param {HTMLElement} element - The element to scroll into view
 * @param {HTMLElement} container - The scroll container
 * @param {boolean} center - Whether to center the item in the container
 */
function scrollIntoContainerView(element, container, center = false) {
    if (!element || !container) return;

    const containerRect = container.getBoundingClientRect();
    const elementRect = element.getBoundingClientRect();

    // Calculate the element's position relative to the container's scroll position
    const elementTop = element.offsetTop;
    const elementHeight = element.offsetHeight;
    const containerScrollTop = container.scrollTop;
    const containerHeight = container.clientHeight;

    if (center) {
        // Center the element in the container
        const targetScrollTop = elementTop - (containerHeight / 2) + (elementHeight / 2);
        container.scrollTop = Math.max(0, targetScrollTop);
    } else {
        // Scroll only if element is outside visible area
        if (elementTop < containerScrollTop) {
            // Element is above visible area
            container.scrollTop = elementTop;
        } else if (elementTop + elementHeight > containerScrollTop + containerHeight) {
            // Element is below visible area
            container.scrollTop = elementTop + elementHeight - containerHeight;
        }
    }
}

/**
 * Sets focus visual indicator on an option.
 * @param {HTMLElement} container - The select content container
 * @param {number} index - Index of the option to focus
 * @param {boolean} center - Whether to center the item in the container (for initial selection)
 */
function setFocusedOption(container, index, center = false) {
    const items = getEnabledOptions(container);

    // Remove focus from all items
    items.forEach(item => item.setAttribute('data-focused', 'false'));

    // Set focus on target item
    if (index >= 0 && index < items.length) {
        items[index].setAttribute('data-focused', 'true');
        // Use container-aware scrolling to avoid scrolling the page
        scrollIntoContainerView(items[index], container, center);
    }
}

/**
 * Navigates to the next option.
 * @param {HTMLElement} container - The select content container
 * @param {boolean} loop - Whether to loop from last to first
 */
function navigateNext(container, loop = true) {
    const items = getEnabledOptions(container);
    if (items.length === 0) return;

    const currentIndex = getFocusedIndex(container);
    let nextIndex;

    if (currentIndex === -1) {
        nextIndex = 0;
    } else if (currentIndex === items.length - 1) {
        nextIndex = loop ? 0 : currentIndex;
    } else {
        nextIndex = currentIndex + 1;
    }

    setFocusedOption(container, nextIndex);
}

/**
 * Navigates to the previous option.
 * @param {HTMLElement} container - The select content container
 * @param {boolean} loop - Whether to loop from first to last
 */
function navigatePrevious(container, loop = true) {
    const items = getEnabledOptions(container);
    if (items.length === 0) return;

    const currentIndex = getFocusedIndex(container);
    let prevIndex;

    if (currentIndex === -1) {
        prevIndex = items.length - 1;
    } else if (currentIndex === 0) {
        prevIndex = loop ? items.length - 1 : 0;
    } else {
        prevIndex = currentIndex - 1;
    }

    setFocusedOption(container, prevIndex);
}

/**
 * Navigates to the first option.
 * @param {HTMLElement} container - The select content container
 */
function navigateFirst(container) {
    setFocusedOption(container, 0);
}

/**
 * Navigates to the last option.
 * @param {HTMLElement} container - The select content container
 */
function navigateLast(container) {
    const items = getEnabledOptions(container);
    setFocusedOption(container, items.length - 1);
}

/**
 * Selects the currently focused option by triggering its click handler.
 * @param {HTMLElement} container - The select content container
 */
function selectFocusedOption(container) {
    const items = getEnabledOptions(container);
    const focusedIndex = getFocusedIndex(container);

    if (focusedIndex >= 0 && focusedIndex < items.length) {
        items[focusedIndex].click();
    }
}

/**
 * Sets up keyboard navigation for a select content element.
 * This is called from Blazor when the select opens.
 * @param {string} contentId - The ID of the select content element
 * @param {object} dotNetRef - Reference to the Blazor component for callbacks
 * @returns {object} Cleanup object with dispose method
 */
export function setupKeyboardNavigation(contentId, dotNetRef) {
    const container = document.getElementById(contentId);
    if (!container) {
        return { dispose: () => {} };
    }

    // Clean up any existing handler for this content
    if (activeHandlers.has(contentId)) {
        activeHandlers.get(contentId).dispose();
    }

    const handleKeyDown = async (e) => {
        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                navigateNext(container, true);
                break;

            case 'ArrowUp':
                e.preventDefault();
                navigatePrevious(container, true);
                break;

            case 'Home':
                e.preventDefault();
                navigateFirst(container);
                break;

            case 'End':
                e.preventDefault();
                navigateLast(container);
                break;

            case 'Enter':
            case ' ':
                e.preventDefault();
                selectFocusedOption(container);
                break;

            case 'Escape':
                e.preventDefault();
                if (dotNetRef) {
                    await dotNetRef.invokeMethodAsync('HandleEscapeKey');
                }
                break;

            case 'Tab':
                if (dotNetRef) {
                    await dotNetRef.invokeMethodAsync('HandleTabKey');
                }
                break;
        }
    };

    // Attach the handler to the container
    container.addEventListener('keydown', handleKeyDown);

    // Focus the container so it receives keyboard events
    // Use requestAnimationFrame to ensure focus happens after Blazor's event handling completes
    const focusContainer = () => {
        container.focus({ preventScroll: true });

        // If focus didn't take, try again with a small delay
        if (document.activeElement !== container) {
            setTimeout(() => {
                container.focus({ preventScroll: true });
            }, 10);
        }
    };

    // Use double rAF to ensure we're past any pending Blazor updates
    requestAnimationFrame(() => {
        requestAnimationFrame(focusContainer);
    });

    const cleanup = {
        dispose: () => {
            container.removeEventListener('keydown', handleKeyDown);
            activeHandlers.delete(contentId);
        }
    };

    activeHandlers.set(contentId, cleanup);
    return cleanup;
}

/**
 * Cleans up keyboard navigation for a select content element.
 * @param {string} contentId - The ID of the select content element
 */
export function cleanupKeyboardNavigation(contentId) {
    if (activeHandlers.has(contentId)) {
        activeHandlers.get(contentId).dispose();
    }
}

/**
 * Focuses the select content element.
 * @param {string} contentId - The ID of the select content element
 */
export function focusContent(contentId) {
    const contentElement = document.getElementById(contentId);
    if (contentElement) {
        contentElement.focus({ preventScroll: true });
    }
}

/**
 * Scrolls an item into view within its scroll container (not the page).
 * @param {string} itemId - The ID of the item element
 * @param {boolean} instant - Whether to scroll instantly (no animation) - unused, kept for API compatibility
 * @param {boolean} center - Whether to center the item in the container
 */
export function scrollItemIntoView(itemId, instant = false, center = true) {
    const itemElement = document.getElementById(itemId);
    if (!itemElement) return;

    // Find the scroll container (the listbox or its scrollable parent)
    const container = itemElement.closest('[role="listbox"]') || itemElement.parentElement;
    if (container) {
        scrollIntoContainerView(itemElement, container, center);
    }
}

/**
 * Focuses an element with preventScroll option.
 * @param {HTMLElement} element - The element to focus
 */
export function focusElementWithPreventScroll(element) {
    if (element) {
        setTimeout(() => {
            element.focus({ preventScroll: true });
        }, 10);
    }
}

/**
 * Focuses the initially selected or first option.
 * @param {string} contentId - The ID of the select content element
 * @param {string} selectedValue - The currently selected value (optional)
 */
export function focusInitialOption(contentId, selectedValue) {
    const container = document.getElementById(contentId);
    if (!container) return;

    const items = getEnabledOptions(container);
    if (items.length === 0) return;

    // Try to find and focus the selected item by aria-selected attribute
    let targetIndex = 0;
    const selectedIndex = items.findIndex(item =>
        item.getAttribute('aria-selected') === 'true'
    );
    if (selectedIndex >= 0) {
        targetIndex = selectedIndex;
    }

    // Use center=true to show context around the selected item
    setFocusedOption(container, targetIndex, true);
}

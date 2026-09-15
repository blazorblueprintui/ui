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
 * Finds the nearest scrollable ancestor of an element.
 * @param {HTMLElement} element - The element to find the scroll parent for
 * @returns {HTMLElement|null} The nearest scrollable ancestor, or null
 */
function getScrollParent(element) {
    let parent = element.parentElement;
    while (parent) {
        const overflowY = getComputedStyle(parent).overflowY;
        if (overflowY === 'auto' || overflowY === 'scroll') {
            return parent;
        }
        parent = parent.parentElement;
    }
    return null;
}

/**
 * Scrolls an element into view within a scrollable container, without scrolling the page.
 * @param {HTMLElement} element - The element to scroll into view
 * @param {HTMLElement} container - The fallback scroll container
 * @param {boolean} center - Whether to center the item in the container
 */
function scrollIntoContainerView(element, container, center = false) {
    if (!element || !container) return;

    // Use the nearest scrollable ancestor if the container itself isn't scrollable
    const scrollContainer = getScrollParent(element) || container;

    // Calculate the element's position relative to the scroll container
    const elementTop = element.offsetTop;
    const elementHeight = element.offsetHeight;
    const containerScrollTop = scrollContainer.scrollTop;
    const containerHeight = scrollContainer.clientHeight;

    if (containerHeight <= 0 || elementHeight <= 0) return;

    if (center) {
        // Center the element in the container
        const targetScrollTop = elementTop - (containerHeight / 2) + (elementHeight / 2);
        scrollContainer.scrollTop = Math.max(0, targetScrollTop);
    } else {
        // Scroll only if element is outside visible area
        if (elementTop < containerScrollTop) {
            // Element is above visible area
            scrollContainer.scrollTop = elementTop;
        } else if (elementTop + elementHeight > containerScrollTop + containerHeight) {
            // Element is below visible area
            scrollContainer.scrollTop = elementTop + elementHeight - containerHeight;
        }
    }
}

/**
 * Sets focus visual indicator on an option.
 * @param {HTMLElement} container - The select content container
 * @param {number} index - Index of the option to focus
 * @param {boolean} center - Whether to center the item in the container (for initial selection)
 */
function setFocusedOption(container, index, center = false, scroll = true) {
    const items = getEnabledOptions(container);

    // Remove focus from all items
    items.forEach(item => item.setAttribute('data-focused', 'false'));

    // Set focus on target item
    if (index >= 0 && index < items.length) {
        items[index].setAttribute('data-focused', 'true');
        if (scroll) {
            // Use container-aware scrolling to avoid scrolling the page
            scrollIntoContainerView(items[index], container, center);
        }
        setActiveDescendant(container, items[index].id);
    } else {
        setActiveDescendant(container, null);
    }
}

/**
 * Points the listbox at its focused option for assistive technology.
 *
 * C# used to render this attribute, which meant every arrow key cost a render batch — a circuit
 * round trip on Blazor Server — to move a highlight the browser had already moved. `data-focused`
 * was written here and `aria-activedescendant` was written there, so the two could disagree for a
 * round trip. One writer owns both now.
 *
 * @param {HTMLElement} container - The listbox element.
 * @param {string|null} id - The focused option's element id, or null for none.
 */
function setActiveDescendant(container, id) {
    if (id) {
        container.setAttribute('aria-activedescendant', id);
    } else {
        container.removeAttribute('aria-activedescendant');
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
export function setupKeyboardNavigation(contentId, dotNetRef, autoFocus = true) {
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
                // Ours to handle, so it must not also reach the document-level escape stack —
                // otherwise a select inside a dialog closes both at once.
                e.stopPropagation();
                if (dotNetRef) {
                    await dotNetRef.invokeMethodAsync('JsOnEscapeKey');
                }
                break;

            case 'Tab':
                if (dotNetRef) {
                    await dotNetRef.invokeMethodAsync('JsOnTabKey');
                }
                break;
        }
    };

    // Hover moves the highlight, and it does so here rather than through an @onmouseenter on each
    // option. That handler called back into C# to set the focused index, which re-rendered every
    // item — a circuit round trip per option the pointer crossed, to move a highlight the browser
    // could move itself. Delegated to the container, so the cost does not scale with option count.
    const handleMouseOver = (e) => {
        const option = e.target.closest?.('[role="option"]');
        if (!option || !container.contains(option)) {
            return;
        }

        if (option.getAttribute('data-disabled') === 'true' ||
            option.getAttribute('aria-disabled') === 'true') {
            return;
        }

        const index = getEnabledOptions(container).indexOf(option);
        if (index >= 0 && index !== getFocusedIndex(container)) {
            // No scroll: the pointer is already on the option, and scrolling under it would move
            // the list out from beneath the cursor.
            setFocusedOption(container, index, false, false);
        }
    };

    // Attach the handler to the container
    container.addEventListener('keydown', handleKeyDown);
    container.addEventListener('mouseover', handleMouseOver);

    // Focus is the caller's business when it opts out. overlay.open does, because it attaches
    // the handlers before it reveals the listbox — and a `visibility: hidden` element cannot take
    // focus, so focusing here would silently do nothing and leave every key going to the trigger.
    if (autoFocus) {
        requestAnimationFrame(() => {
            requestAnimationFrame(() => focusListbox(contentId));
        });
    }

    const cleanup = {
        dispose: () => {
            container.removeEventListener('keydown', handleKeyDown);
            container.removeEventListener('mouseover', handleMouseOver);
            activeHandlers.delete(contentId);
        }
    };

    activeHandlers.set(contentId, cleanup);
    return cleanup;
}

/**
 * Scrolls the element marked as current into view inside its own scroll container.
 *
 * For content that shows its chosen value with an icon rather than with `aria-selected` — a
 * combobox reads `aria-selected` as "keyboard-focused", which is a different thing — so the owner
 * marks the chosen element and names the marker here.
 *
 * Runs before the overlay is revealed, which is why it can only be a browser-side call: a list
 * must appear already scrolled to its selection, and scrolling it once it is on screen is a jump.
 * A parked element still lays out, because it is hidden with `visibility` rather than `display`,
 * so every measurement this needs is already valid.
 *
 * @param {string} containerId - The element to search within.
 * @param {string} selector - CSS selector for the current item. Defaults to the select's own.
 */
export function scrollMarkedIntoView(containerId, selector = '[role="option"][aria-selected="true"]') {
    const container = document.getElementById(containerId);
    if (!container) return;

    const target = container.querySelector(selector);
    if (!target) return;

    scrollIntoContainerView(target, container, true);
}

/**
 * Puts keyboard focus on the listbox so it receives the arrow keys.
 *
 * Must run after the listbox is revealed. A hidden element cannot be focused, and the failure is
 * silent: focus stays on the trigger, every keystroke goes to the server as a DOM event, and the
 * highlight never moves.
 *
 * @param {string} contentId - The ID of the select content element
 */
export function focusListbox(contentId) {
    const container = document.getElementById(contentId);
    if (!container) return;

    container.focus({ preventScroll: true });

    // If focus didn't take, try again with a small delay
    if (document.activeElement !== container) {
        setTimeout(() => container.focus({ preventScroll: true }), 10);
    }
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
 * Prepares an open listbox for interaction, in one call.
 *
 * Scrolling the selected option into view and attaching the keyboard handler were two separately
 * awaited calls from C#, and on Blazor Server each one is a circuit round trip — paid on every
 * open, while the user is waiting for the list to be usable.
 *
 * The scroll still happens first, and now strictly first: it runs before anything else in this
 * task, so it lands ahead of the requestAnimationFrame that reveals the portal. Revealing a
 * listbox already scrolled to the selected option is the whole point of the ordering.
 *
 * @param {string} contentId - The listbox element id.
 * @param {string|null} selectedValue - The currently selected value, if any.
 * @param {boolean} attachKeyboard - Whether the keyboard handler still needs attaching.
 * @param {Object|null} dotNetRef - Callback target for the keyboard handler.
 */
export function openListbox(contentId, selectedValue, attachKeyboard, dotNetRef) {
    focusInitialOption(contentId, selectedValue);

    if (attachKeyboard && dotNetRef) {
        // autoFocus off: the listbox is still parked and invisible at this point. overlay.open
        // focuses it once it has revealed it.
        setupKeyboardNavigation(contentId, dotNetRef, false);
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

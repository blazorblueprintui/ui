/**
 * Smart popover positioning with auto-detection for viewport edges.
 * Supports all four sides (top, bottom, left, right) and three alignment options (start, center, end).
 * Automatically adjusts position when there's insufficient space on the preferred side.
 */

/**
 * Positions the popover content relative to its trigger element.
 * @param {HTMLElement} containerElement - The popover container element
 * @param {string} popoverId - The unique ID of the popover instance
 * @param {string} side - Preferred side: "top", "bottom", "left", "right", or "auto"
 * @param {string} align - Alignment: "start", "center", or "end"
 */
export function positionPopover(containerElement, popoverId, side, align) {
    if (!containerElement || !popoverId) return;

    // Find the popover content element (the popoverId IS the content element)
    const popoverContent = document.getElementById(popoverId);
    if (!popoverContent) return;

    // Find the trigger button
    const triggerButton = containerElement.querySelector('[role="button"]');
    if (!triggerButton) return;

    // Get viewport dimensions and trigger position
    const triggerRect = triggerButton.getBoundingClientRect();
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;

    // Get popover dimensions
    const popoverRect = popoverContent.getBoundingClientRect();
    const popoverWidth = popoverRect.width || 288; // Default w-72 (18rem = 288px)
    const popoverHeight = popoverRect.height || 200;

    // Spacing between trigger and popover
    const spacing = 8; // 8px gap

    // Calculate available space on each side
    const spaceAbove = triggerRect.top;
    const spaceBelow = viewportHeight - triggerRect.bottom;
    const spaceLeft = triggerRect.left;
    const spaceRight = viewportWidth - triggerRect.right;

    // Determine the best side if "auto" is specified
    let finalSide = side;
    if (side === "auto") {
        // Choose the side with the most available space
        const spaces = {
            bottom: spaceBelow,
            top: spaceAbove,
            right: spaceRight,
            left: spaceLeft
        };
        finalSide = Object.keys(spaces).reduce((a, b) => spaces[a] > spaces[b] ? a : b);
    } else {
        // Check if preferred side has enough space, otherwise use opposite
        if (side === "bottom" && spaceBelow < popoverHeight && spaceAbove > spaceBelow) {
            finalSide = "top";
        } else if (side === "top" && spaceAbove < popoverHeight && spaceBelow > spaceAbove) {
            finalSide = "bottom";
        } else if (side === "right" && spaceRight < popoverWidth && spaceLeft > spaceRight) {
            finalSide = "left";
        } else if (side === "left" && spaceLeft < popoverWidth && spaceRight > spaceLeft) {
            finalSide = "right";
        }
    }

    // Set data-side attribute for CSS animations
    popoverContent.setAttribute("data-side", finalSide);

    // Calculate position based on final side and alignment
    let top, left;

    switch (finalSide) {
        case "top":
            top = triggerRect.top - popoverHeight - spacing;
            left = calculateAlignmentPosition(triggerRect, popoverWidth, align, "horizontal");
            break;

        case "bottom":
            top = triggerRect.bottom + spacing;
            left = calculateAlignmentPosition(triggerRect, popoverWidth, align, "horizontal");
            break;

        case "left":
            left = triggerRect.left - popoverWidth - spacing;
            top = calculateAlignmentPosition(triggerRect, popoverHeight, align, "vertical");
            break;

        case "right":
            left = triggerRect.right + spacing;
            top = calculateAlignmentPosition(triggerRect, popoverHeight, align, "vertical");
            break;
    }

    // Ensure popover stays within viewport bounds
    top = Math.max(spacing, Math.min(top, viewportHeight - popoverHeight - spacing));
    left = Math.max(spacing, Math.min(left, viewportWidth - popoverWidth - spacing));

    // Apply position
    popoverContent.style.position = "fixed";
    popoverContent.style.top = `${top}px`;
    popoverContent.style.left = `${left}px`;

    // Make visible after positioning (remove CSS pre-hide classes)
    popoverContent.classList.remove('opacity-0', 'invisible');
    popoverContent.classList.add('opacity-100', 'visible');

    // Focus management for accessibility
    // If there's a combobox input inside, focus it directly instead of the container
    setTimeout(() => {
        if (popoverContent) {
            // Check if there's a combobox input that should receive focus
            const comboboxInput = popoverContent.querySelector('input[role="combobox"][type="text"]');

            if (comboboxInput) {
                // Focus the input directly for combobox pattern
                comboboxInput.focus({ preventScroll: true });
            } else {
                // Focus the container for other popovers
                popoverContent.focus({ preventScroll: true });
            }
        }
    }, 100); // Increased delay to let Blazor render complete
}

/**
 * Calculates the alignment position (start, center, end) for the popover.
 * @param {DOMRect} triggerRect - The bounding rect of the trigger element
 * @param {number} popoverSize - Width or height of the popover
 * @param {string} align - Alignment option: "start", "center", or "end"
 * @param {string} axis - "horizontal" or "vertical"
 * @returns {number} The calculated position value
 */
function calculateAlignmentPosition(triggerRect, popoverSize, align, axis) {
    const isHorizontal = axis === "horizontal";
    const triggerStart = isHorizontal ? triggerRect.left : triggerRect.top;
    const triggerSize = isHorizontal ? triggerRect.width : triggerRect.height;

    switch (align) {
        case "start":
            return triggerStart;

        case "end":
            return triggerStart + triggerSize - popoverSize;

        case "center":
        default:
            return triggerStart + (triggerSize / 2) - (popoverSize / 2);
    }
}

/**
 * Click-outside listeners storage
 */
const clickOutsideListeners = new Map();

/**
 * Scroll listeners storage
 */
const scrollListeners = new Map();

/**
 * Sets up a click-outside listener for a popover.
 * Closes the popover when clicking outside of it.
 * @param {string} popoverId - The unique ID of the popover instance
 * @param {object} dotNetHelper - DotNet object reference for callbacks
 */
export function setupClickOutside(popoverId, dotNetHelper) {
    // Remove any existing listener for this popover
    removeClickOutside(popoverId);

    const popoverContainer = document.getElementById(popoverId);
    if (!popoverContainer) return;

    // Create click handler
    const clickHandler = (event) => {
        // Check if click is outside the popover container
        if (!popoverContainer.contains(event.target)) {
            // Call the Blazor callback to close the popover
            dotNetHelper.invokeMethodAsync('CloseFromJavaScript');
        }
    };

    // Store the listener reference
    clickOutsideListeners.set(popoverId, clickHandler);

    // Add listener with a slight delay to avoid immediate closure
    setTimeout(() => {
        document.addEventListener('click', clickHandler);
    }, 10);
}

/**
 * Removes the click-outside listener for a popover.
 * @param {string} popoverId - The unique ID of the popover instance
 */
export function removeClickOutside(popoverId) {
    const clickHandler = clickOutsideListeners.get(popoverId);
    if (clickHandler) {
        document.removeEventListener('click', clickHandler);
        clickOutsideListeners.delete(popoverId);
    }
}

/**
 * Sets up a scroll listener for a popover to close it when scrolling.
 * @param {string} popoverId - The unique ID of the popover instance
 * @param {object} dotNetHelper - DotNet object reference for callbacks
 */
export function setupScrollListener(popoverId, dotNetHelper) {
    // Remove any existing listener for this popover
    removeScrollListener(popoverId);

    // Create scroll handler
    const closeOnScroll = () => {
        // Close popover when user scrolls
        dotNetHelper.invokeMethodAsync('CloseFromJavaScript');
    };

    // Store the listener reference
    scrollListeners.set(popoverId, closeOnScroll);

    // Listen to scroll events (capture phase to catch all scrolls)
    window.addEventListener('scroll', closeOnScroll, true);
    // Also listen to wheel events for better responsiveness
    window.addEventListener('wheel', closeOnScroll, { passive: true });
}

/**
 * Removes the scroll listener for a popover.
 * @param {string} popoverId - The unique ID of the popover instance
 */
export function removeScrollListener(popoverId) {
    const scrollHandler = scrollListeners.get(popoverId);
    if (scrollHandler) {
        window.removeEventListener('scroll', scrollHandler, true);
        window.removeEventListener('wheel', scrollHandler);
        scrollListeners.delete(popoverId);
    }
}

/**
 * Cleanup function for removing all event listeners.
 */
export function cleanup() {
    // Remove all click-outside listeners
    clickOutsideListeners.forEach((handler) => {
        document.removeEventListener('click', handler);
    });
    clickOutsideListeners.clear();

    // Remove all scroll listeners
    scrollListeners.forEach((handler) => {
        window.removeEventListener('scroll', handler, true);
        window.removeEventListener('wheel', handler);
    });
    scrollListeners.clear();
}

/**
 * Dispose function called when the module is being disposed.
 * Ensures all event listeners are removed.
 */
export function dispose() {
    cleanup();
}

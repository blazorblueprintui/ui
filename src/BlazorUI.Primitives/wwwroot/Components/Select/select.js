/**
 * Smart dropdown positioning for Select component.
 * Dynamically positions the dropdown to avoid viewport edges.
 */

export function positionDropdown(triggerElement, selectId) {
    if (!triggerElement || !selectId) return;

    // Find the dropdown content element
    const selectContainer = document.getElementById(selectId);
    if (!selectContainer) return;

    const dropdownContent = selectContainer.querySelector('[role="listbox"]');
    if (!dropdownContent) return;

    // Get trigger element dimensions and position
    const triggerRect = triggerElement.getBoundingClientRect();
    const viewportHeight = window.innerHeight;
    const viewportWidth = window.innerWidth;

    // Get dropdown dimensions
    const dropdownRect = dropdownContent.getBoundingClientRect();
    const dropdownHeight = dropdownRect.height || 240; // Default max height from CSS
    const dropdownWidth = dropdownRect.width || triggerRect.width;

    // Calculate available space above and below trigger
    const spaceBelow = viewportHeight - triggerRect.bottom;
    const spaceAbove = triggerRect.top;

    // Determine if dropdown should open upward or downward
    const shouldOpenUpward = spaceBelow < dropdownHeight && spaceAbove > spaceBelow;

    // Position the dropdown
    if (shouldOpenUpward) {
        // Open upward
        dropdownContent.style.bottom = `${triggerRect.height + 4}px`;
        dropdownContent.style.top = 'auto';
    } else {
        // Open downward (default)
        dropdownContent.style.top = `${triggerRect.height + 4}px`;
        dropdownContent.style.bottom = 'auto';
    }

    // Ensure dropdown doesn't overflow horizontally
    const triggerLeft = triggerRect.left;
    const triggerRight = viewportWidth - triggerRect.right;

    if (triggerLeft + dropdownWidth > viewportWidth) {
        // Align to right edge if overflow on right
        dropdownContent.style.right = '0';
        dropdownContent.style.left = 'auto';
    } else {
        // Default left alignment
        dropdownContent.style.left = '0';
        dropdownContent.style.right = 'auto';
    }

    // Set width to match trigger
    dropdownContent.style.minWidth = `${triggerRect.width}px`;
}

/**
 * Click-outside listeners storage
 */
const clickOutsideListeners = new Map();

/**
 * Sets up a click-outside listener for a select dropdown.
 * Closes the dropdown when clicking outside of it.
 */
export function setupClickOutside(selectId, dotNetHelper) {
    // Remove any existing listener for this select
    removeClickOutside(selectId);

    const selectContainer = document.getElementById(selectId);
    if (!selectContainer) return;

    // Create click handler
    const clickHandler = (event) => {
        // Check if click is outside the select container
        if (!selectContainer.contains(event.target)) {
            // Call the Blazor callback to close the dropdown
            dotNetHelper.invokeMethodAsync('CloseFromJavaScript');
        }
    };

    // Store the listener reference
    clickOutsideListeners.set(selectId, clickHandler);

    // Add listener with a slight delay to avoid immediate closure
    setTimeout(() => {
        document.addEventListener('click', clickHandler);
    }, 10);
}

/**
 * Removes the click-outside listener for a select dropdown.
 */
export function removeClickOutside(selectId) {
    const clickHandler = clickOutsideListeners.get(selectId);
    if (clickHandler) {
        document.removeEventListener('click', clickHandler);
        clickOutsideListeners.delete(selectId);
    }
}

/**
 * Cleanup function for removing event listeners.
 */
export function cleanup() {
    // Remove all click-outside listeners
    clickOutsideListeners.forEach((handler) => {
        document.removeEventListener('click', handler);
    });
    clickOutsideListeners.clear();
}

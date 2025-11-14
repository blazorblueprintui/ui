/**
 * Smart dropdown positioning for DropdownMenu component.
 * Handles vertical positioning (above/below) based on available viewport space.
 * Horizontal alignment is handled via CSS classes.
 */

export function positionDropdown(containerElement, dropdownId) {
    if (!containerElement || !dropdownId) return;

    // Find the dropdown content element
    const dropdownContainer = document.getElementById(dropdownId);
    if (!dropdownContainer) return;

    const dropdownContent = dropdownContainer.querySelector('[role="menu"]');
    if (!dropdownContent) return;

    // Find the trigger button to measure its height for vertical positioning
    const triggerButton = containerElement.querySelector('[role="button"]');
    if (!triggerButton) return;

    // Get viewport dimensions and trigger position
    const triggerRect = triggerButton.getBoundingClientRect();
    const viewportHeight = window.innerHeight;

    // Get dropdown dimensions
    const dropdownRect = dropdownContent.getBoundingClientRect();
    const dropdownHeight = dropdownRect.height || 240;

    // Calculate available space above and below trigger
    const spaceBelow = viewportHeight - triggerRect.bottom;
    const spaceAbove = triggerRect.top;

    // Determine if dropdown should open upward or downward
    const shouldOpenUpward = spaceBelow < dropdownHeight && spaceAbove > spaceBelow;

    // Position the dropdown vertically only
    // Horizontal alignment is handled by CSS classes in DropdownMenuContent
    if (shouldOpenUpward) {
        // Open upward
        dropdownContent.style.bottom = `${triggerRect.height + 4}px`;
        dropdownContent.style.top = 'auto';
    } else {
        // Open downward (default)
        dropdownContent.style.top = `${triggerRect.height + 4}px`;
        dropdownContent.style.bottom = 'auto';
    }

    // Focus the menu content (helps prevent unwanted page interactions)
    setTimeout(() => {
        if (dropdownContent) {
            dropdownContent.focus();
        }
    }, 0);
}

/**
 * Click-outside listeners storage
 */
const clickOutsideListeners = new Map();

/**
 * Sets up a click-outside listener for a dropdown menu.
 * Closes the dropdown when clicking outside of it.
 */
export function setupClickOutside(dropdownId, dotNetHelper) {
    // Remove any existing listener for this dropdown
    removeClickOutside(dropdownId);

    const dropdownContainer = document.getElementById(dropdownId);
    if (!dropdownContainer) return;

    // Create click handler
    const clickHandler = (event) => {
        // Check if click is outside the dropdown container
        if (!dropdownContainer.contains(event.target)) {
            // Call the Blazor callback to close the dropdown
            dotNetHelper.invokeMethodAsync('CloseFromJavaScript');
        }
    };

    // Store the listener reference
    clickOutsideListeners.set(dropdownId, clickHandler);

    // Add listener with a slight delay to avoid immediate closure
    setTimeout(() => {
        document.addEventListener('click', clickHandler);
    }, 10);
}

/**
 * Removes the click-outside listener for a dropdown menu.
 */
export function removeClickOutside(dropdownId) {
    const clickHandler = clickOutsideListeners.get(dropdownId);
    if (clickHandler) {
        document.removeEventListener('click', clickHandler);
        clickOutsideListeners.delete(dropdownId);
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

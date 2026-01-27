/**
 * Virtualization scroll utilities for Command component
 * Provides functions for scrolling in virtualized lists
 */

/**
 * Scrolls to a specific index in a virtualized group.
 * Calculates scroll position accounting for container offset, heading height, and item index.
 * @param {string} listId - The ID of the scrollable list container
 * @param {string} containerId - The ID of the group container
 * @param {number} index - The item index to scroll to
 * @param {number} headingHeight - Height of the group heading in pixels (default 32)
 * @param {number} itemHeight - Height of each item in pixels (default 32)
 */
export function scrollToVirtualizedIndex(listId, containerId, index, headingHeight = 32, itemHeight = 32) {
    const list = document.getElementById(listId);
    const container = document.getElementById(containerId);
    if (!list || !container) return;

    // Get the container's position relative to the list
    const containerTop = container.offsetTop - list.offsetTop;
    let targetScroll;

    if (index === 0) {
        // For first item, scroll to show the group heading
        targetScroll = containerTop;
    } else {
        // For other items, scroll to show the item
        targetScroll = containerTop + headingHeight + (index * itemHeight);
    }

    // Clamp to valid scroll range
    const maxScroll = list.scrollHeight - list.clientHeight;
    list.scrollTop = Math.max(0, Math.min(targetScroll, maxScroll));
}

/**
 * Scrolls an element into view with configurable block alignment.
 * @param {string} elementId - The ID of the element to scroll into view
 * @param {string} scrollBlock - The block alignment ('start', 'nearest', 'center', 'end')
 */
export function scrollElementIntoView(elementId, scrollBlock = 'nearest') {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({
            block: scrollBlock,
            behavior: 'instant'
        });
    }
}

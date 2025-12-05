// Context menu positioning helper
// Ensures context menu stays within viewport bounds

/**
 * Adjusts the position of a context menu to ensure it stays within the viewport.
 * Repositions the menu if it would overflow the viewport edges.
 * @param {HTMLElement} menuElement - The context menu element
 * @param {number} x - Initial X coordinate (mouse position)
 * @param {number} y - Initial Y coordinate (mouse position)
 * @param {number} padding - Padding from viewport edges (default: 8px)
 * @returns {Object} Adjusted position { x, y }
 */
export function adjustContextMenuPosition(menuElement, x, y, padding = 8) {
    if (!menuElement) {
        return { x, y };
    }

    // Get viewport dimensions
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;

    // Get menu dimensions
    const menuRect = menuElement.getBoundingClientRect();
    const menuWidth = menuRect.width;
    const menuHeight = menuRect.height;

    let adjustedX = x;
    let adjustedY = y;

    // Check right edge overflow
    if (x + menuWidth > viewportWidth - padding) {
        // Move menu to the left of the cursor
        adjustedX = Math.max(padding, x - menuWidth);
    }

    // Check bottom edge overflow
    if (y + menuHeight > viewportHeight - padding) {
        // Move menu above the cursor
        adjustedY = Math.max(padding, y - menuHeight);
    }

    // Ensure menu doesn't go off left edge
    if (adjustedX < padding) {
        adjustedX = padding;
    }

    // Ensure menu doesn't go off top edge
    if (adjustedY < padding) {
        adjustedY = padding;
    }

    return { x: adjustedX, y: adjustedY };
}

/**
 * Sets up context menu positioning and applies adjusted coordinates.
 * @param {HTMLElement} menuElement - The context menu element
 * @param {number} x - Initial X coordinate
 * @param {number} y - Initial Y coordinate
 * @param {number} padding - Padding from viewport edges
 * @returns {Object} Disposable object
 */
export function setupContextMenuPosition(menuElement, x, y, padding = 8) {
    if (!menuElement) {
        return { dispose: () => {} };
    }

    // Wait for next frame to ensure menu is rendered and has dimensions
    requestAnimationFrame(() => {
        const adjusted = adjustContextMenuPosition(menuElement, x, y, padding);
        
        // Apply adjusted position
        menuElement.style.left = `${adjusted.x}px`;
        menuElement.style.top = `${adjusted.y}px`;
    });

    return {
        dispose: () => {
            // No cleanup needed for one-time positioning
        }
    };
}

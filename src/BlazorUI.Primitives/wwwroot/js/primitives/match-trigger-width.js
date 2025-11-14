/**
 * Match content width to trigger width
 * @param {HTMLElement} triggerElement - The trigger element
 * @param {HTMLElement} contentElement - The content element to resize
 * @returns {Object} Cleanup function
 */
export function matchTriggerWidth(triggerElement, contentElement) {
    if (!triggerElement || !contentElement) {
        console.warn('matchTriggerWidth: Missing required elements');
        return { dispose: () => {} };
    }

    function updateWidth() {
        const triggerRect = triggerElement.getBoundingClientRect();
        // Set width with !important via cssText to override any class-based width
        contentElement.style.cssText += `; width: ${triggerRect.width}px !important`;
    }

    // Apply width immediately
    updateWidth();

    // Update on window resize
    const resizeObserver = new ResizeObserver(() => {
        updateWidth();
    });

    resizeObserver.observe(triggerElement);

    return {
        dispose: () => {
            resizeObserver.disconnect();
        }
    };
}

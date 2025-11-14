/**
 * Keyboard navigation helper for primitives
 * Prevents default scroll behavior for navigation keys
 */

export function setupKeyboardNav(element, dotNetRef) {
    if (!element) return;

    const handleKeyDown = (e) => {
        // Prevent default scroll behavior for navigation keys
        if (['ArrowDown', 'ArrowUp', 'Home', 'End', 'PageUp', 'PageDown'].includes(e.key)) {
            e.preventDefault();
        }
    };

    element.addEventListener('keydown', handleKeyDown);

    // Return cleanup function
    return {
        dispose: () => {
            element.removeEventListener('keydown', handleKeyDown);
        }
    };
}

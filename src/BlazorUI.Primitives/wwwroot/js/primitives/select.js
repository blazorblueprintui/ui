// Select primitive utilities for scroll and focus management
// Note: Click-outside detection has been moved to click-outside.js for unified handling

export function focusContent(contentId) {
    const contentElement = document.getElementById(contentId);
    if (contentElement) {
        contentElement.focus({ preventScroll: true });
    }
}

export function scrollItemIntoView(itemId, instant = false) {
    const itemElement = document.getElementById(itemId);
    if (itemElement) {
        itemElement.scrollIntoView({
            block: 'nearest',
            inline: 'nearest',
            behavior: instant ? 'instant' : 'smooth'
        });
    }
}

export function focusElementWithPreventScroll(element) {
    if (element) {
        // Small delay to ensure element is ready
        setTimeout(() => {
            element.focus({ preventScroll: true });
        }, 10);
    }
}

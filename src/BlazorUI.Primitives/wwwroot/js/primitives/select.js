// Click-outside handling for Select primitive
let clickOutsideHandlers = new Map();

export function setupClickOutside(elementRef, dotNetHelper, contentId) {
    const handler = (event) => {
        const contentElement = document.getElementById(contentId);
        if (contentElement && !contentElement.contains(event.target)) {
            // Check if click is also outside the trigger
            const triggerElement = event.target.closest('[role="combobox"]');
            if (!triggerElement) {
                dotNetHelper.invokeMethodAsync('HandleClickOutside');
            }
        }
    };

    // Small delay to avoid closing immediately after opening
    setTimeout(() => {
        document.addEventListener('click', handler, true);
        clickOutsideHandlers.set(contentId, handler);

        // Add keyboard handler to prevent scrolling on arrow keys
        const keyHandler = (e) => {
            if (['ArrowUp', 'ArrowDown', 'Home', 'End', ' '].includes(e.key)) {
                const content = document.getElementById(contentId);
                if (content && document.activeElement === content) {
                    e.preventDefault();
                }
            }
        };

        const contentElement = document.getElementById(contentId);
        if (contentElement) {
            contentElement.addEventListener('keydown', keyHandler);
            clickOutsideHandlers.set(contentId + '-key', keyHandler);

            // Focus the content element so keyboard events work
            contentElement.focus({ preventScroll: true });
        }
    }, 100);
}

export function focusContent(contentId) {
    const contentElement = document.getElementById(contentId);
    if (contentElement) {
        contentElement.focus({ preventScroll: true });
    }
}

export function scrollItemIntoView(itemId) {
    const itemElement = document.getElementById(itemId);
    if (itemElement) {
        itemElement.scrollIntoView({
            block: 'nearest',
            inline: 'nearest',
            behavior: 'smooth'
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

export function removeClickOutside(contentId) {
    const handler = clickOutsideHandlers.get(contentId);
    if (handler) {
        document.removeEventListener('click', handler, true);
        clickOutsideHandlers.delete(contentId);
    }

    const keyHandler = clickOutsideHandlers.get(contentId + '-key');
    if (keyHandler) {
        const contentElement = document.getElementById(contentId);
        if (contentElement) {
            contentElement.removeEventListener('keydown', keyHandler);
        }
        clickOutsideHandlers.delete(contentId + '-key');
    }
}

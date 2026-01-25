// MultiSelect keyboard navigation handler
// Handles navigation in JavaScript with direct DOM manipulation for immediate response
// Key differences from combobox:
// - Space toggles checkbox without closing
// - Enter toggles checkbox and closes
// - Preserves selection state on items

let multiSelectStates = new Map();

/**
 * Sets up keyboard event interception for a multiselect input element
 * @param {HTMLElement} inputElement - The input element to attach the handler to
 * @param {DotNetObject} dotNetRef - Reference to the Blazor component
 * @param {string} inputId - Unique identifier for the input element
 * @param {string} contentId - ID of the listbox content element
 */
export function setupMultiSelectInput(inputElement, dotNetRef, inputId, contentId) {
    if (!inputElement || !dotNetRef) {
        console.error('setupMultiSelectInput: missing required parameters');
        return;
    }

    // State for this multiselect instance
    const state = {
        inputElement,
        dotNetRef,
        contentId,
        focusedIndex: -1
    };

    // Get visible and enabled option elements
    const getVisibleOptions = () => {
        const content = document.getElementById(contentId);
        if (!content) return [];

        // Get all option elements that are visible and not disabled
        return Array.from(content.querySelectorAll('[role="option"]')).filter(el => {
            // Element must be visible and not disabled
            return el.offsetParent !== null && el.getAttribute('aria-disabled') !== 'true';
        });
    };

    // Update focused state in DOM (preserves selection state unlike combobox)
    const updateFocusedItem = (newIndex) => {
        const options = getVisibleOptions();

        // Remove focus from all items (but preserve selection state)
        options.forEach(el => {
            el.setAttribute('data-focused', 'false');
            // Don't change aria-selected - it reflects checkbox state in multiselect
        });

        // Add focus to new item
        if (newIndex >= 0 && newIndex < options.length) {
            const focusedEl = options[newIndex];
            focusedEl.setAttribute('data-focused', 'true');

            // Scroll into view if needed
            focusedEl.scrollIntoView({ block: 'nearest', behavior: 'smooth' });

            state.focusedIndex = newIndex;
        } else {
            state.focusedIndex = -1;
        }
    };

    // Handle keyboard navigation
    const keyHandler = (e) => {
        const options = getVisibleOptions();

        // Only handle keys if there are visible options
        if (options.length === 0 && !['Enter', 'Escape', ' '].includes(e.key)) {
            return;
        }

        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                {
                    const newIndex = state.focusedIndex < options.length - 1
                        ? state.focusedIndex + 1
                        : 0; // Wrap to first
                    updateFocusedItem(newIndex);
                }
                break;

            case 'ArrowUp':
                e.preventDefault();
                {
                    const newIndex = state.focusedIndex > 0
                        ? state.focusedIndex - 1
                        : options.length - 1; // Wrap to last
                    updateFocusedItem(newIndex);
                }
                break;

            case 'Home':
                e.preventDefault();
                updateFocusedItem(0);
                break;

            case 'End':
                e.preventDefault();
                updateFocusedItem(options.length - 1);
                break;

            case ' ': // Space - toggle checkbox, don't close
                e.preventDefault();
                if (state.focusedIndex >= 0 && state.focusedIndex < options.length) {
                    const focusedEl = options[state.focusedIndex];
                    if (focusedEl.getAttribute('aria-disabled') !== 'true') {
                        // Click the focused item to trigger toggle
                        focusedEl.click();
                        // Call Blazor to handle the toggle
                        dotNetRef.invokeMethodAsync('HandleSpace').catch(err => {
                            console.error('Error invoking HandleSpace:', err);
                        });
                    }
                }
                break;

            case 'Enter':
                e.preventDefault();
                if (state.focusedIndex >= 0 && state.focusedIndex < options.length) {
                    const focusedEl = options[state.focusedIndex];
                    if (focusedEl.getAttribute('aria-disabled') !== 'true') {
                        // Click the focused item to trigger toggle
                        focusedEl.click();
                    }
                }
                // Call Blazor to handle enter (toggle + close)
                dotNetRef.invokeMethodAsync('HandleEnter').catch(err => {
                    console.error('Error invoking HandleEnter:', err);
                });
                break;

            case 'Escape':
                e.preventDefault();
                // Close dropdown via Blazor
                dotNetRef.invokeMethodAsync('HandleEscape').catch(err => {
                    console.error('Error invoking HandleEscape:', err);
                });
                break;
        }
    };

    // Use capture phase (true) to run before browser's default behavior
    inputElement.addEventListener('keydown', keyHandler, true);

    // Reset focused index when search query changes
    const inputHandler = () => {
        // Reset focus when user types
        state.focusedIndex = -1;
        setTimeout(() => {
            // Clear all focused states after a brief delay to let Blazor re-render
            const options = getVisibleOptions();
            options.forEach(el => {
                el.setAttribute('data-focused', 'false');
            });
        }, 50);
    };
    inputElement.addEventListener('input', inputHandler);

    // Store state and handlers for cleanup
    multiSelectStates.set(inputId, {
        state,
        keyHandler,
        inputHandler,
        inputElement
    });

    // Note: Focus is handled by Blazor's OnContentReady event, not here
}

/**
 * Removes keyboard event handler from a multiselect input
 * @param {string} inputId - Unique identifier for the input element
 */
export function removeMultiSelectInput(inputId) {
    const stored = multiSelectStates.get(inputId);
    if (stored) {
        stored.inputElement.removeEventListener('keydown', stored.keyHandler, true);
        stored.inputElement.removeEventListener('input', stored.inputHandler);
        multiSelectStates.delete(inputId);
    }
}

/**
 * Cleans up all multiselect handlers (useful for debugging)
 */
export function disposeAll() {
    multiSelectStates.forEach((stored, inputId) => {
        stored.inputElement.removeEventListener('keydown', stored.keyHandler, true);
        stored.inputElement.removeEventListener('input', stored.inputHandler);
    });
    multiSelectStates.clear();
}

/**
 * Focuses an element with preventScroll option to avoid page jumping
 * @param {HTMLElement} element - The element to focus
 */
export function focusElementWithPreventScroll(element) {
    if (element) {
        setTimeout(() => {
            element.focus({ preventScroll: true });
        }, 10);
    }
}


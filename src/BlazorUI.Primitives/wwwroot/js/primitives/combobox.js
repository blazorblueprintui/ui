// Combobox keyboard navigation handler
// Handles navigation in JavaScript with direct DOM manipulation for immediate response

let comboboxStates = new Map();

/**
 * Sets up keyboard event interception for a combobox input element
 * @param {HTMLElement} inputElement - The input element to attach the handler to
 * @param {DotNetObject} dotNetRef - Reference to the Blazor component
 * @param {string} inputId - Unique identifier for the input element
 * @param {string} contentId - ID of the listbox content element
 */
export function setupComboboxInput(inputElement, dotNetRef, inputId, contentId) {
    if (!inputElement || !dotNetRef) {
        console.error('setupComboboxInput: missing required parameters');
        return;
    }

    // State for this combobox instance
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

    // Update focused state in DOM
    const updateFocusedItem = (newIndex) => {
        const options = getVisibleOptions();

        // Remove focus from all items
        options.forEach(el => {
            el.setAttribute('data-focused', 'false');
            el.setAttribute('aria-selected', 'false');
            // HEADLESS DESIGN: Use data attributes only, not CSS classes
        });

        // Add focus to new item
        if (newIndex >= 0 && newIndex < options.length) {
            const focusedEl = options[newIndex];
            focusedEl.setAttribute('data-focused', 'true');
            focusedEl.setAttribute('aria-selected', 'true');
            // HEADLESS DESIGN: Use data attributes only, not CSS classes

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
        if (options.length === 0 && !['Enter', 'Escape'].includes(e.key)) {
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

            case 'Enter':
                e.preventDefault();
                if (state.focusedIndex >= 0 && state.focusedIndex < options.length) {
                    const focusedEl = options[state.focusedIndex];
                    // Double-check the item is not disabled before clicking
                    if (focusedEl.getAttribute('aria-disabled') !== 'true') {
                        // Click the focused item to trigger Blazor's selection logic
                        focusedEl.click();
                    }
                }
                break;

            case 'Escape':
                e.preventDefault();
                // Clear search via Blazor
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
                el.setAttribute('aria-selected', 'false');
                // HEADLESS DESIGN: Use data attributes only, not CSS classes
            });
        }, 50);
    };
    inputElement.addEventListener('input', inputHandler);

    // Store state and handlers for cleanup
    comboboxStates.set(inputId, {
        state,
        keyHandler,
        inputHandler,
        inputElement
    });
}

/**
 * Removes keyboard event handler from a combobox input
 * @param {string} inputId - Unique identifier for the input element
 */
export function removeComboboxInput(inputId) {
    const stored = comboboxStates.get(inputId);
    if (stored) {
        stored.inputElement.removeEventListener('keydown', stored.keyHandler, true);
        stored.inputElement.removeEventListener('input', stored.inputHandler);
        comboboxStates.delete(inputId);
    }
}

/**
 * Cleans up all combobox handlers (useful for debugging)
 */
export function disposeAll() {
    comboboxStates.forEach((stored, inputId) => {
        stored.inputElement.removeEventListener('keydown', stored.keyHandler, true);
        stored.inputElement.removeEventListener('input', stored.inputHandler);
    });
    comboboxStates.clear();
}

/**
 * Focuses an element with preventScroll option to avoid page jumping
 * @param {HTMLElement} element - The element to focus
 */
export function focusElementWithPreventScroll(element) {
    if (element) {
        // Small delay to ensure element is ready
        setTimeout(() => {
            element.focus({ preventScroll: true });
        }, 10);
    }
}

/**
 * Focuses an element by ID with preventScroll option to avoid page jumping
 * Uses retry mechanism for Blazor Server-Side rendering delays
 * @param {string} elementId - The ID of the element to focus
 */
export function focusElementByIdWithPreventScroll(elementId) {
    let attempts = 0;
    const maxAttempts = 10;
    const retryDelay = 50; // Check every 50ms

    const tryFocus = () => {
        const element = document.getElementById(elementId);
        if (element) {
            element.focus({ preventScroll: true });
            return true;
        }

        attempts++;
        if (attempts < maxAttempts) {
            setTimeout(tryFocus, retryDelay);
        } else {
            console.warn(`focusElementByIdWithPreventScroll: Element with id "${elementId}" not found after ${maxAttempts} attempts`);
        }
    };

    // Start with a small initial delay
    setTimeout(tryFocus, 50);
}

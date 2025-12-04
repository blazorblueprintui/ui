// Positioning service using Floating UI
// CDN: https://cdn.jsdelivr.net/npm/@floating-ui/dom@1.5.3/+esm

let floatingUI = null;

// Store cleanup functions with unique IDs to avoid passing functions through JS interop
const cleanupRegistry = new Map();
let cleanupIdCounter = 0;

/**
 * Lazy loads Floating UI from CDN with local fallback
 */
async function loadFloatingUI() {
    if (floatingUI) return floatingUI;

    try {
        // Try CDN first
        floatingUI = await import('https://cdn.jsdelivr.net/npm/@floating-ui/dom@1.5.3/+esm');
        return floatingUI;
    } catch (cdnError) {
        console.warn('Failed to load Floating UI from CDN, trying local fallback:', cdnError);

        try {
            // Try unpkg as alternative CDN
            floatingUI = await import('https://unpkg.com/@floating-ui/dom@1.5.3/+esm');
            return floatingUI;
        } catch (fallbackError) {
            console.error('Failed to load Floating UI from all sources:', fallbackError);
            throw new Error('Floating UI library could not be loaded. Please check network connection or bundle the library locally.');
        }
    }
}

/**
 * Computes the optimal position for a floating element.
 * @param {HTMLElement} reference - Reference element
 * @param {HTMLElement} floating - Floating element to position
 * @param {Object} options - Positioning options
 * @returns {Promise<Object>} Position result with x, y, placement
 */
export async function computePosition(reference, floating, options = {}) {
    try {
        const lib = await loadFloatingUI();

    const {
        placement = 'bottom',
        offset: offsetValue = 8,
        flip = true,
        shift = true,
        padding = 8,
        strategy = 'absolute'
    } = options;

    const middleware = [
        lib.offset(offsetValue)
    ];

    if (flip) {
        middleware.push(lib.flip({ padding }));
    }

    if (shift) {
        middleware.push(lib.shift({ padding }));
    }

    // Add arrow middleware if arrow element provided
    if (options.arrow) {
        middleware.push(lib.arrow({ element: options.arrow }));
    }

    const result = await lib.computePosition(reference, floating, {
        placement,
        middleware,
        strategy
    });

        return {
            x: result.x,
            y: result.y,
            placement: result.placement,
            transformOrigin: getTransformOrigin(result.placement),
            strategy: result.strategy || strategy
        };
    } catch (error) {
        console.error('Failed to compute position:', error);
        throw error;  // Re-throw for caller to handle
    }
}

/**
 * Applies computed position to a floating element.
 * @param {HTMLElement} floating - The floating element
 * @param {Object} position - Position result from computePosition
 * @param {boolean} makeVisible - Whether to make the element visible after positioning
 */
export function applyPosition(floating, position, makeVisible = false) {
    if (!floating || !position) return;

    Object.assign(floating.style, {
        position: position.strategy || 'absolute',
        left: `${position.x}px`,
        top: `${position.y}px`,
        transformOrigin: position.transformOrigin || ''
    });

    // If makeVisible is true, show the element after positioning
    // Note: We don't set opacity here - let CSS animations (animate-in, fade-in) handle it
    if (makeVisible) {
        floating.style.visibility = 'visible';
        floating.style.pointerEvents = 'auto';
    }
}

/**
 * Sets up auto-update for dynamic positioning.
 * @param {HTMLElement} reference - Reference element
 * @param {HTMLElement} floating - Floating element
 * @param {Object} options - Positioning options
 * @returns {Object} Disposable object with id and apply() method for cleanup
 */
export async function autoUpdate(reference, floating, options = {}) {
    try {
        const lib = await loadFloatingUI();

    const update = async () => {
        const position = await computePosition(reference, floating, options);
        applyPosition(floating, position);
    };

    // Initial position
    await update();

        // Set up auto-update - this returns a cleanup function
        const cleanupFunc = lib.autoUpdate(reference, floating, update);

        // Store cleanup function in registry with unique ID
        const id = cleanupIdCounter++;
        cleanupRegistry.set(id, cleanupFunc);

        // Return disposable object with ID (no functions in the object)
        return {
            _cleanupId: id,
            apply: function() {
                const cleanup = cleanupRegistry.get(this._cleanupId);
                if (cleanup) {
                    cleanup();
                    cleanupRegistry.delete(this._cleanupId);
                }
            }
        };
    } catch (error) {
        console.error('Failed to set up auto-update:', error);
        throw error;  // Re-throw for caller to handle
    }
}

/**
 * Gets the transform-origin based on placement for animations.
 * @param {string} placement - The placement value
 * @returns {string} CSS transform-origin value
 */
function getTransformOrigin(placement) {
    const placements = {
        'top': 'bottom center',
        'top-start': 'bottom left',
        'top-end': 'bottom right',
        'bottom': 'top center',
        'bottom-start': 'top left',
        'bottom-end': 'top right',
        'left': 'center right',
        'left-start': 'top right',
        'left-end': 'bottom right',
        'right': 'center left',
        'right-start': 'top left',
        'right-end': 'bottom left'
    };

    return placements[placement] || 'center';
}

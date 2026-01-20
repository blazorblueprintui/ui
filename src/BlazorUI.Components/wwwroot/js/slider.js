// Slider drag handler
// Handles mouse drag events at document level for smooth slider interaction

let sliderStates = new Map();

/**
 * Initializes drag handling for a slider element
 * @param {HTMLElement} trackElement - The slider track element
 * @param {DotNetObject} dotNetRef - Reference to the Blazor component
 * @param {string} sliderId - Unique identifier for the slider
 */
export function initializeSlider(trackElement, dotNetRef, sliderId) {
    if (!trackElement || !dotNetRef) {
        console.error('initializeSlider: missing required parameters');
        return;
    }

    const state = {
        trackElement,
        dotNetRef,
        isDragging: false
    };

    const calculateValue = (clientX) => {
        const rect = trackElement.getBoundingClientRect();
        const percentage = Math.max(0, Math.min(1, (clientX - rect.left) / rect.width));
        return percentage;
    };

    const handleMouseMove = (e) => {
        if (!state.isDragging) return;
        e.preventDefault();

        const percentage = calculateValue(e.clientX);
        dotNetRef.invokeMethodAsync('UpdateValueFromPercentage', percentage).catch(err => {
            console.error('Error updating slider value:', err);
        });
    };

    const handleMouseUp = () => {
        if (state.isDragging) {
            state.isDragging = false;
            document.removeEventListener('mousemove', handleMouseMove);
            document.removeEventListener('mouseup', handleMouseUp);
            document.body.style.userSelect = '';
            document.body.style.cursor = '';
        }
    };

    const handleTrackMouseDown = (e) => {
        e.preventDefault();
        state.isDragging = true;

        // Prevent text selection while dragging
        document.body.style.userSelect = 'none';
        document.body.style.cursor = 'grabbing';

        // Calculate initial value from click position
        const percentage = calculateValue(e.clientX);
        dotNetRef.invokeMethodAsync('UpdateValueFromPercentage', percentage).catch(err => {
            console.error('Error updating slider value:', err);
        });

        // Add document-level listeners for drag
        document.addEventListener('mousemove', handleMouseMove);
        document.addEventListener('mouseup', handleMouseUp);
    };

    // Attach mousedown to the track element
    trackElement.addEventListener('mousedown', handleTrackMouseDown);

    // Store for cleanup
    sliderStates.set(sliderId, {
        state,
        handleTrackMouseDown,
        handleMouseMove,
        handleMouseUp,
        trackElement
    });
}

/**
 * Removes slider drag handling
 * @param {string} sliderId - Unique identifier for the slider
 */
export function disposeSlider(sliderId) {
    const stored = sliderStates.get(sliderId);
    if (stored) {
        stored.trackElement.removeEventListener('mousedown', stored.handleTrackMouseDown);
        document.removeEventListener('mousemove', stored.handleMouseMove);
        document.removeEventListener('mouseup', stored.handleMouseUp);
        sliderStates.delete(sliderId);
    }
}

/**
 * Gets the bounding rect of an element
 * @param {HTMLElement} element - The element
 * @returns {Object} The bounding rect
 */
export function getBoundingRect(element) {
    if (!element) return { left: 0, width: 100 };
    const rect = element.getBoundingClientRect();
    return { left: rect.left, width: rect.width };
}

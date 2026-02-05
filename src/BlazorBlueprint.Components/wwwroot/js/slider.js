// Slider drag handler
// Handles pointer drag events at document level for smooth slider interaction

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
        isDragging: false,
        pointerId: null
    };

    const calculateValue = (clientX) => {
        const rect = trackElement.getBoundingClientRect();
        const percentage = Math.max(0, Math.min(1, (clientX - rect.left) / rect.width));
        return percentage;
    };

    const handlePointerMove = (e) => {
        if (!state.isDragging || e.pointerId !== state.pointerId) return;
        e.preventDefault();

        const percentage = calculateValue(e.clientX);
        dotNetRef.invokeMethodAsync('UpdateValueFromPercentage', percentage).catch(err => {
            console.error('Error updating slider value:', err);
        });
    };

    const handlePointerUp = (e) => {
        if (e.pointerId !== state.pointerId) return;
        if (state.isDragging) {
            state.isDragging = false;
            state.pointerId = null;
            trackElement.releasePointerCapture(e.pointerId);
            document.body.style.userSelect = '';
            document.body.style.cursor = '';
        }
    };

    const handlePointerCancel = (e) => {
        if (e.pointerId !== state.pointerId) return;
        state.isDragging = false;
        state.pointerId = null;
        document.body.style.userSelect = '';
        document.body.style.cursor = '';
    };

    const handleTrackPointerDown = (e) => {
        if (state.isDragging) return; // Ignore secondary touches
        e.preventDefault();
        state.isDragging = true;
        state.pointerId = e.pointerId;
        trackElement.setPointerCapture(e.pointerId);

        // Prevent text selection while dragging
        document.body.style.userSelect = 'none';
        document.body.style.cursor = 'grabbing';

        // Calculate initial value from click/touch position
        const percentage = calculateValue(e.clientX);
        dotNetRef.invokeMethodAsync('UpdateValueFromPercentage', percentage).catch(err => {
            console.error('Error updating slider value:', err);
        });
    };

    // Attach pointer events to the track element
    trackElement.addEventListener('pointerdown', handleTrackPointerDown);
    trackElement.addEventListener('pointermove', handlePointerMove);
    trackElement.addEventListener('pointerup', handlePointerUp);
    trackElement.addEventListener('pointercancel', handlePointerCancel);

    // Store for cleanup
    sliderStates.set(sliderId, {
        state,
        handleTrackPointerDown,
        handlePointerMove,
        handlePointerUp,
        handlePointerCancel,
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
        stored.trackElement.removeEventListener('pointerdown', stored.handleTrackPointerDown);
        stored.trackElement.removeEventListener('pointermove', stored.handlePointerMove);
        stored.trackElement.removeEventListener('pointerup', stored.handlePointerUp);
        stored.trackElement.removeEventListener('pointercancel', stored.handlePointerCancel);
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

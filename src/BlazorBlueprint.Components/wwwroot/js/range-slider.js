// Range Slider drag handler
// Handles pointer drag events for dual-thumb slider

const rangeSliderStates = new Map();

/**
 * Initializes drag handling for a range slider
 * @param {HTMLElement} trackElement - The slider track element
 * @param {DotNetObject} dotNetRef - Reference to the Blazor component
 * @param {string} sliderId - Unique identifier for the slider
 */
export function initializeRangeSlider(trackElement, dotNetRef, sliderId) {
    if (!trackElement || !dotNetRef) {
        console.error('initializeRangeSlider: missing required parameters');
        return;
    }

    const state = {
        trackElement,
        dotNetRef,
        isDragging: false,
        activeThumb: null, // 'start' or 'end'
        pointerId: null
    };

    const calculatePercentage = (clientX) => {
        const rect = trackElement.getBoundingClientRect();
        return Math.max(0, Math.min(1, (clientX - rect.left) / rect.width));
    };

    const handlePointerMove = (e) => {
        if (!state.isDragging || !state.activeThumb || e.pointerId !== state.pointerId) return;
        e.preventDefault();

        const percentage = calculatePercentage(e.clientX);
        state.dotNetRef.invokeMethodAsync('UpdateValueFromPercentage', percentage, state.activeThumb).catch(err => {
            console.error('Error updating range slider value:', err);
        });
    };

    const handlePointerUp = (e) => {
        if (e.pointerId !== state.pointerId) return;
        if (state.isDragging) {
            state.isDragging = false;
            state.activeThumb = null;
            trackElement.releasePointerCapture(e.pointerId);
            state.pointerId = null;
            document.body.style.userSelect = '';
            document.body.style.cursor = '';
        }
    };

    const handlePointerCancel = (e) => {
        if (e.pointerId !== state.pointerId) return;
        state.isDragging = false;
        state.activeThumb = null;
        state.pointerId = null;
        document.body.style.userSelect = '';
        document.body.style.cursor = '';
    };

    const handleTrackPointerDown = (e) => {
        // Only handle clicks on the track, not on thumbs (thumbs handle their own events)
        if (e.target.hasAttribute('data-thumb')) return;
        if (state.isDragging) return; // Ignore secondary touches

        e.preventDefault();
        const percentage = calculatePercentage(e.clientX);

        // Get current percentages from the component
        state.dotNetRef.invokeMethodAsync('HandleTrackClick', percentage).catch(err => {
            console.error('Error handling track click:', err);
        });
    };

    // Attach pointer events to the track element
    trackElement.addEventListener('pointerdown', handleTrackPointerDown);
    trackElement.addEventListener('pointermove', handlePointerMove);
    trackElement.addEventListener('pointerup', handlePointerUp);
    trackElement.addEventListener('pointercancel', handlePointerCancel);

    // Store for cleanup and external access
    rangeSliderStates.set(sliderId, {
        state,
        handleTrackPointerDown,
        handlePointerMove,
        handlePointerUp,
        handlePointerCancel,
        trackElement
    });
}

/**
 * Starts dragging a specific thumb
 * @param {string} sliderId - Unique identifier for the slider
 * @param {string} thumb - Which thumb ('start' or 'end')
 * @param {number} pointerId - The pointer ID for this drag operation
 */
export function startDrag(sliderId, thumb, pointerId) {
    const stored = rangeSliderStates.get(sliderId);
    if (!stored) return;

    const { state, trackElement } = stored;

    if (state.isDragging) return; // Ignore if already dragging

    state.isDragging = true;
    state.activeThumb = thumb;
    state.pointerId = pointerId;

    document.body.style.userSelect = 'none';
    document.body.style.cursor = 'grabbing';

    // Set pointer capture on track element for reliable drag tracking
    trackElement.setPointerCapture(pointerId);
}

/**
 * Removes range slider handling
 * @param {string} sliderId - Unique identifier for the slider
 */
export function disposeRangeSlider(sliderId) {
    const stored = rangeSliderStates.get(sliderId);
    if (stored) {
        stored.trackElement.removeEventListener('pointerdown', stored.handleTrackPointerDown);
        stored.trackElement.removeEventListener('pointermove', stored.handlePointerMove);
        stored.trackElement.removeEventListener('pointerup', stored.handlePointerUp);
        stored.trackElement.removeEventListener('pointercancel', stored.handlePointerCancel);
        rangeSliderStates.delete(sliderId);
    }
}

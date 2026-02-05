// Resizable panel drag handler
// Handles pointer drag events at document level for smooth resize interaction

let resizableStates = new Map();

/**
 * Initializes resize handling for a panel group
 * @param {HTMLElement} groupElement - The panel group container element
 * @param {DotNetObject} dotNetRef - Reference to the Blazor component
 * @param {string} groupId - Unique identifier for the group
 * @param {boolean} isHorizontal - Whether the layout is horizontal
 */
export function initializeResizable(groupElement, dotNetRef, groupId, isHorizontal) {
    if (!groupElement || !dotNetRef) {
        console.error('initializeResizable: missing required parameters');
        return;
    }

    const state = {
        groupElement,
        dotNetRef,
        isHorizontal,
        isDragging: false,
        activeHandleIndex: -1,
        startPosition: 0,
        startSizes: [],
        pointerId: null
    };

    resizableStates.set(groupId, state);
}

/**
 * Starts a resize operation
 * @param {string} groupId - The group identifier
 * @param {number} handleIndex - Index of the handle being dragged
 * @param {number} clientX - Pointer X position
 * @param {number} clientY - Pointer Y position
 * @param {number[]} currentSizes - Current panel sizes as percentages
 * @param {number} pointerId - The pointer ID for this resize operation
 */
export function startResize(groupId, handleIndex, clientX, clientY, currentSizes, pointerId) {
    const state = resizableStates.get(groupId);
    if (!state) return;

    if (state.isDragging) return; // Ignore secondary touches

    state.isDragging = true;
    state.activeHandleIndex = handleIndex;
    state.startPosition = state.isHorizontal ? clientX : clientY;
    state.startSizes = [...currentSizes];
    state.pointerId = pointerId;

    // Prevent text selection while dragging
    document.body.style.userSelect = 'none';
    document.body.style.cursor = state.isHorizontal ? 'col-resize' : 'row-resize';

    // Add document-level listeners
    const handlePointerMove = (e) => onPointerMove(groupId, e);
    const handlePointerUp = (e) => onPointerUp(groupId, e);
    const handlePointerCancel = (e) => onPointerCancel(groupId, e);

    state.handlePointerMove = handlePointerMove;
    state.handlePointerUp = handlePointerUp;
    state.handlePointerCancel = handlePointerCancel;

    document.addEventListener('pointermove', handlePointerMove);
    document.addEventListener('pointerup', handlePointerUp);
    document.addEventListener('pointercancel', handlePointerCancel);

    // Set pointer capture on group element for reliable drag tracking
    state.groupElement.setPointerCapture(pointerId);
}

function onPointerMove(groupId, e) {
    const state = resizableStates.get(groupId);
    if (!state || !state.isDragging || e.pointerId !== state.pointerId) return;

    e.preventDefault();

    const rect = state.groupElement.getBoundingClientRect();
    const totalSize = state.isHorizontal ? rect.width : rect.height;
    const currentPosition = state.isHorizontal ? e.clientX : e.clientY;

    // Calculate delta as percentage of total size
    const deltaPixels = currentPosition - state.startPosition;
    const deltaPercent = (deltaPixels / totalSize) * 100;

    // Calculate new sizes for the two panels adjacent to the handle
    const panelIndex = state.activeHandleIndex;
    const newSizes = [...state.startSizes];

    // Adjust the panel before and after the handle
    const newSize1 = state.startSizes[panelIndex] + deltaPercent;
    const newSize2 = state.startSizes[panelIndex + 1] - deltaPercent;

    // Apply minimum size constraints (10% minimum)
    const minSize = 10;
    if (newSize1 >= minSize && newSize2 >= minSize) {
        newSizes[panelIndex] = newSize1;
        newSizes[panelIndex + 1] = newSize2;

        // Notify Blazor of the new sizes
        state.dotNetRef.invokeMethodAsync('UpdatePanelSizes', newSizes).catch(err => {
            console.error('Error updating panel sizes:', err);
        });
    }
}

function onPointerUp(groupId, e) {
    const state = resizableStates.get(groupId);
    if (!state || e.pointerId !== state.pointerId) return;

    cleanupResize(state, e.pointerId);
}

function onPointerCancel(groupId, e) {
    const state = resizableStates.get(groupId);
    if (!state || e.pointerId !== state.pointerId) return;

    cleanupResize(state, e.pointerId);
}

function cleanupResize(state, pointerId) {
    state.isDragging = false;
    state.activeHandleIndex = -1;
    state.pointerId = null;

    document.body.style.userSelect = '';
    document.body.style.cursor = '';

    if (state.handlePointerMove) {
        document.removeEventListener('pointermove', state.handlePointerMove);
    }
    if (state.handlePointerUp) {
        document.removeEventListener('pointerup', state.handlePointerUp);
    }
    if (state.handlePointerCancel) {
        document.removeEventListener('pointercancel', state.handlePointerCancel);
    }

    try {
        state.groupElement.releasePointerCapture(pointerId);
    } catch (err) {
        // Pointer capture may already be released
    }
}

/**
 * Disposes resize handling for a panel group
 * @param {string} groupId - The group identifier
 */
export function disposeResizable(groupId) {
    const state = resizableStates.get(groupId);
    if (state) {
        if (state.handlePointerMove) {
            document.removeEventListener('pointermove', state.handlePointerMove);
        }
        if (state.handlePointerUp) {
            document.removeEventListener('pointerup', state.handlePointerUp);
        }
        if (state.handlePointerCancel) {
            document.removeEventListener('pointercancel', state.handlePointerCancel);
        }
        resizableStates.delete(groupId);
    }
}

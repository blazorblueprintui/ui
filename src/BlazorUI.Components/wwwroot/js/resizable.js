// Resizable panel drag handler
// Handles mouse drag events at document level for smooth resize interaction

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
        startSizes: []
    };

    resizableStates.set(groupId, state);
}

/**
 * Starts a resize operation
 * @param {string} groupId - The group identifier
 * @param {number} handleIndex - Index of the handle being dragged
 * @param {number} clientX - Mouse X position
 * @param {number} clientY - Mouse Y position
 * @param {number[]} currentSizes - Current panel sizes as percentages
 */
export function startResize(groupId, handleIndex, clientX, clientY, currentSizes) {
    const state = resizableStates.get(groupId);
    if (!state) return;

    state.isDragging = true;
    state.activeHandleIndex = handleIndex;
    state.startPosition = state.isHorizontal ? clientX : clientY;
    state.startSizes = [...currentSizes];

    // Prevent text selection while dragging
    document.body.style.userSelect = 'none';
    document.body.style.cursor = state.isHorizontal ? 'col-resize' : 'row-resize';

    // Add document-level listeners
    const handleMouseMove = (e) => onMouseMove(groupId, e);
    const handleMouseUp = () => onMouseUp(groupId);

    state.handleMouseMove = handleMouseMove;
    state.handleMouseUp = handleMouseUp;

    document.addEventListener('mousemove', handleMouseMove);
    document.addEventListener('mouseup', handleMouseUp);
}

function onMouseMove(groupId, e) {
    const state = resizableStates.get(groupId);
    if (!state || !state.isDragging) return;

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

function onMouseUp(groupId) {
    const state = resizableStates.get(groupId);
    if (!state) return;

    state.isDragging = false;
    state.activeHandleIndex = -1;

    document.body.style.userSelect = '';
    document.body.style.cursor = '';

    if (state.handleMouseMove) {
        document.removeEventListener('mousemove', state.handleMouseMove);
    }
    if (state.handleMouseUp) {
        document.removeEventListener('mouseup', state.handleMouseUp);
    }
}

/**
 * Disposes resize handling for a panel group
 * @param {string} groupId - The group identifier
 */
export function disposeResizable(groupId) {
    const state = resizableStates.get(groupId);
    if (state) {
        if (state.handleMouseMove) {
            document.removeEventListener('mousemove', state.handleMouseMove);
        }
        if (state.handleMouseUp) {
            document.removeEventListener('mouseup', state.handleMouseUp);
        }
        resizableStates.delete(groupId);
    }
}

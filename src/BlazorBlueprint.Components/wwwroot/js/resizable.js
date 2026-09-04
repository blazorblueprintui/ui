// Resizable panel drag handler
// Handles pointer drag events at document level for smooth resize interaction.
//
// The drag loop writes flex-basis straight to the panel elements and does NOT call back into
// Blazor per pointermove. A round trip per frame is unusable on Blazor Server: interop is
// dispatched serially on the circuit's synchronisation context, so moves arriving faster than the
// server can service them queue up and the lag accumulates across the drag.
//
// C# is therefore deliberately stale for the duration of the drag and is synced exactly once, on
// pointerup/pointercancel. This relies on Blazor diffing against what it last rendered rather than
// against the DOM: while the rendered size is unchanged, re-renders leave our flex-basis alone.

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
        groupId,
        isHorizontal,
        isDragging: false,
        activeHandleIndex: -1,
        startPosition: 0,
        startSizes: [],
        currentSizes: [],
        pointerId: null,
        panelElements: [],
        useDomWrites: true,
        pendingPosition: 0,
        frameHandle: 0,
        dirty: false
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
 * @param {number[]} [minSizes] - Minimum sizes per panel as percentages
 * @param {number[]} [maxSizes] - Maximum sizes per panel as percentages
 * @param {boolean} [isHorizontal] - Current orientation, in case Direction changed since init
 */
export function startResize(groupId, handleIndex, clientX, clientY, currentSizes, pointerId, minSizes, maxSizes, isHorizontal) {
    const state = resizableStates.get(groupId);
    if (!state) return;

    if (state.isDragging) return; // Ignore secondary touches

    if (typeof isHorizontal === 'boolean') {
        state.isHorizontal = isHorizontal;
    }

    state.isDragging = true;
    state.activeHandleIndex = handleIndex;
    state.startPosition = state.isHorizontal ? clientX : clientY;
    state.pendingPosition = state.startPosition;
    state.startSizes = [...currentSizes];
    state.currentSizes = [...currentSizes];
    state.pointerId = pointerId;
    state.minSizes = minSizes || currentSizes.map(() => 10);
    state.maxSizes = maxSizes || currentSizes.map(() => 100);
    state.frameHandle = 0;
    state.dirty = false;

    // Resolve the panel elements once per drag so the move loop never touches the DOM tree.
    state.panelElements = resolvePanelElements(state);
    state.useDomWrites = !!(state.panelElements[handleIndex] && state.panelElements[handleIndex + 1]);

    if (!state.useDomWrites) {
        console.warn(
            `resizable: could not resolve the panel elements for handle ${handleIndex} in group ${groupId}. ` +
            'Falling back to per-frame interop, which is slow on Blazor Server.');
    }

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

/**
 * Finds this group's own panel elements, in declared order.
 *
 * Scoped by group id rather than by descendant position, because a nested panel group's panels are
 * also descendants of this group's element and must not be picked up.
 */
function resolvePanelElements(state) {
    const ordered = [];

    for (const element of state.groupElement.querySelectorAll('[data-bb-resizable-panel]')) {
        if (element.getAttribute('data-bb-resizable-group') !== state.groupId) continue;

        const index = Number.parseInt(element.getAttribute('data-bb-resizable-panel'), 10);
        if (Number.isInteger(index) && index >= 0) {
            ordered[index] = element;
        }
    }

    return ordered;
}

function onPointerMove(groupId, e) {
    const state = resizableStates.get(groupId);
    if (!state || !state.isDragging || e.pointerId !== state.pointerId) return;

    e.preventDefault();

    state.pendingPosition = state.isHorizontal ? e.clientX : e.clientY;

    // Coalesce to one layout write per frame; pointermove fires well above the display refresh rate.
    if (state.frameHandle) return;
    state.frameHandle = requestAnimationFrame(() => {
        state.frameHandle = 0;
        applyDrag(state);
    });
}

function applyDrag(state) {
    if (!state.isDragging) return;

    const rect = state.groupElement.getBoundingClientRect();
    const totalSize = state.isHorizontal ? rect.width : rect.height;
    if (!(totalSize > 0)) return;

    const index = state.activeHandleIndex;
    const start1 = state.startSizes[index];
    const start2 = state.startSizes[index + 1];
    if (start1 === undefined || start2 === undefined) return;

    const min1 = state.minSizes[index];
    const max1 = state.maxSizes[index];
    const min2 = state.minSizes[index + 1];
    const max2 = state.maxSizes[index + 1];

    // Clamp the delta to the range both panels can satisfy, so an overshooting drag pins at the
    // limit. The previous code rejected the whole move instead, which left the handle frozen at
    // wherever the last in-range frame happened to land.
    const lowerBound = Math.max(min1 - start1, start2 - max2);
    const upperBound = Math.min(max1 - start1, start2 - min2);
    if (lowerBound > upperBound) return; // Constraints cannot be satisfied together

    const deltaPixels = state.pendingPosition - state.startPosition;
    const deltaPercent = clamp((deltaPixels / totalSize) * 100, lowerBound, upperBound);

    // Round here rather than at render time so the value C# ends up formatting ("F2") is exactly
    // the one already applied to the DOM — otherwise the panels snap by a sub-pixel on drag end.
    const size1 = round2(start1 + deltaPercent);
    const size2 = round2(start1 + start2 - size1);

    if (size1 === state.currentSizes[index] && size2 === state.currentSizes[index + 1]) return;

    state.currentSizes[index] = size1;
    state.currentSizes[index + 1] = size2;
    state.dirty = true;

    if (state.useDomWrites) {
        state.panelElements[index].style.flexBasis = `${size1}%`;
        state.panelElements[index + 1].style.flexBasis = `${size2}%`;
    } else {
        notifyBlazor(state, 'UpdatePanelSizes');
    }
}

function clamp(value, min, max) {
    return Math.min(Math.max(value, min), max);
}

function round2(value) {
    return Math.round(value * 100) / 100;
}

function notifyBlazor(state, method) {
    state.dotNetRef.invokeMethodAsync(method, [...state.currentSizes]).catch(err => {
        console.error('Error updating panel sizes:', err);
    });
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
    // Flush a queued frame first, so the sizes committed to C# match what is on screen.
    if (state.frameHandle) {
        cancelAnimationFrame(state.frameHandle);
        state.frameHandle = 0;
        applyDrag(state);
    }

    // Only a drag that moved something is a resize; a press and release is not.
    const shouldCommit = state.dirty;

    state.isDragging = false;
    state.activeHandleIndex = -1;
    state.pointerId = null;
    state.dirty = false;

    document.body.style.userSelect = '';
    document.body.style.cursor = '';

    removeDocumentListeners(state);

    try {
        state.groupElement.releasePointerCapture(pointerId);
    } catch (err) {
        // Pointer capture may already be released
    }

    // The single interop call of the whole drag. On the fallback path this is one more call after
    // the per-frame ones, so that OnResizeEnd fires exactly once there too.
    if (shouldCommit) {
        notifyBlazor(state, 'CommitPanelSizes');
    }
}

function removeDocumentListeners(state) {
    if (state.handlePointerMove) {
        document.removeEventListener('pointermove', state.handlePointerMove);
        state.handlePointerMove = null;
    }
    if (state.handlePointerUp) {
        document.removeEventListener('pointerup', state.handlePointerUp);
        state.handlePointerUp = null;
    }
    if (state.handlePointerCancel) {
        document.removeEventListener('pointercancel', state.handlePointerCancel);
        state.handlePointerCancel = null;
    }
}

/**
 * Disposes resize handling for a panel group
 * @param {string} groupId - The group identifier
 */
export function disposeResizable(groupId) {
    const state = resizableStates.get(groupId);
    if (state) {
        if (state.frameHandle) {
            cancelAnimationFrame(state.frameHandle);
            state.frameHandle = 0;
        }

        // Disposing mid-drag would otherwise strand these on <body> for the rest of the page's life.
        if (state.isDragging) {
            document.body.style.userSelect = '';
            document.body.style.cursor = '';
        }

        state.isDragging = false;
        removeDocumentListeners(state);
        resizableStates.delete(groupId);
    }
}

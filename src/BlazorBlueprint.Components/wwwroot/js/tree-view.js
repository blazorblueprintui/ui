/**
 * Tree view drag and drop handler
 * Uses pointer events for cross-browser drag interaction.
 */

const instances = new Map();

/**
 * Initialize drag and drop for a tree view by element ID.
 * @param {string} elementId - The ID of the tree container element
 * @param {object} dotNetRef - DotNetObjectReference for C# callbacks
 * @param {string} instanceId - Unique instance ID
 */
export function initializeDragDropById(elementId, dotNetRef, instanceId) {
  const containerElement = document.getElementById(elementId);
  initializeDragDrop(containerElement, dotNetRef, instanceId);
}

/**
 * Initialize drag and drop for a tree view.
 * @param {HTMLElement} containerElement - The tree container element
 * @param {object} dotNetRef - DotNetObjectReference for C# callbacks
 * @param {string} instanceId - Unique instance ID
 */
export function initializeDragDrop(containerElement, dotNetRef, instanceId) {
  if (!containerElement || !dotNetRef) return;

  const DRAG_THRESHOLD = 5; // px of movement before drag starts

  const state = {
    containerElement,
    dotNetRef,
    isDragging: false,
    pendingDrag: false,
    startX: 0,
    startY: 0,
    sourceElement: null,
    sourceValue: null,
    indicator: null,
    ghostElement: null,
    pointerId: null
  };

  const getTreeItemFromPoint = (x, y) => {
    const elements = document.elementsFromPoint(x, y);
    for (const el of elements) {
      const item = el.closest('[role="treeitem"]');
      if (item && containerElement.contains(item) && item !== state.sourceElement) {
        return item;
      }
    }
    return null;
  };

  const getDropPosition = (targetEl, clientY) => {
    const rect = targetEl.querySelector('[data-tree-node]')?.getBoundingClientRect() || targetEl.getBoundingClientRect();
    const relativeY = clientY - rect.top;
    const height = rect.height;

    if (relativeY < height * 0.25) return 'before';
    if (relativeY > height * 0.75) return 'after';
    return 'inside';
  };

  const isDescendantOf = (childEl, parentEl) => {
    if (!childEl || !parentEl) return false;
    let el = childEl.parentElement;
    while (el) {
      if (el === parentEl) return true;
      el = el.parentElement;
    }
    return false;
  };

  const createIndicator = () => {
    const indicator = document.createElement('div');
    indicator.className = 'bb-tree-drop-indicator';
    indicator.style.cssText = 'position:absolute;left:0;right:0;height:2px;background:hsl(var(--primary));pointer-events:none;z-index:50;display:none;';
    containerElement.style.position = 'relative';
    containerElement.appendChild(indicator);
    return indicator;
  };

  const updateIndicator = (targetEl, position) => {
    if (!state.indicator) {
      state.indicator = createIndicator();
    }

    const nodeEl = targetEl.querySelector('[data-tree-node]') || targetEl;
    const containerRect = containerElement.getBoundingClientRect();
    const targetRect = nodeEl.getBoundingClientRect();

    state.indicator.style.display = 'block';
    state.indicator.style.left = `${targetRect.left - containerRect.left}px`;
    state.indicator.style.width = `${targetRect.width}px`;

    if (position === 'before') {
      state.indicator.style.top = `${targetRect.top - containerRect.top}px`;
      state.indicator.style.height = '2px';
      targetEl.classList.remove('bb-tree-drop-target');
    } else if (position === 'after') {
      state.indicator.style.top = `${targetRect.bottom - containerRect.top}px`;
      state.indicator.style.height = '2px';
      targetEl.classList.remove('bb-tree-drop-target');
    } else {
      state.indicator.style.display = 'none';
      targetEl.classList.add('bb-tree-drop-target');
    }
  };

  const clearIndicator = () => {
    if (state.indicator) {
      state.indicator.style.display = 'none';
    }
    containerElement.querySelectorAll('.bb-tree-drop-target').forEach(el => {
      el.classList.remove('bb-tree-drop-target');
    });
  };

  const startDrag = () => {
    state.pendingDrag = false;
    state.isDragging = true;

    // Set pointer capture on the container for smooth drag
    containerElement.setPointerCapture(state.pointerId);

    // Visual feedback: reduce opacity of source
    state.sourceElement.style.opacity = '0.5';

    document.body.style.userSelect = 'none';
    document.body.style.cursor = 'grabbing';
  };

  const handlePointerDown = (e) => {
    const draggableNode = e.target.closest('[data-draggable="true"]');
    if (!draggableNode) return;

    const treeItem = draggableNode.closest('[role="treeitem"]');
    if (!treeItem) return;

    // Don't start drag on toggle or checkbox clicks
    if (e.target.closest('[data-tree-toggle]') || e.target.closest('[data-tree-checkbox]')) return;

    // Record pending drag — actual drag starts after movement exceeds threshold
    state.pendingDrag = true;
    state.startX = e.clientX;
    state.startY = e.clientY;
    state.sourceElement = treeItem;
    state.sourceValue = treeItem.getAttribute('data-value');
    state.pointerId = e.pointerId;
  };

  const handlePointerMove = (e) => {
    if (!state.isDragging && !state.pendingDrag) return;

    // Check threshold before starting actual drag
    if (state.pendingDrag && !state.isDragging) {
      const dx = e.clientX - state.startX;
      const dy = e.clientY - state.startY;
      if (Math.abs(dx) < DRAG_THRESHOLD && Math.abs(dy) < DRAG_THRESHOLD) return;
      startDrag();
    }

    const targetItem = getTreeItemFromPoint(e.clientX, e.clientY);
    if (!targetItem) {
      clearIndicator();
      return;
    }

    // Prevent dropping onto descendants
    if (isDescendantOf(targetItem, state.sourceElement)) {
      clearIndicator();
      return;
    }

    const position = getDropPosition(targetItem, e.clientY);
    updateIndicator(targetItem, position);
  };

  const resetState = () => {
    if (state.sourceElement) {
      state.sourceElement.style.opacity = '';
    }
    clearIndicator();
    document.body.style.userSelect = '';
    document.body.style.cursor = '';

    if (state.isDragging && state.pointerId !== null) {
      try {
        containerElement.releasePointerCapture(state.pointerId);
      } catch {
        // Pointer capture may already be released
      }
    }

    state.isDragging = false;
    state.pendingDrag = false;
    state.sourceElement = null;
    state.sourceValue = null;
    state.pointerId = null;
  };

  const handlePointerUp = (e) => {
    if (!state.isDragging && !state.pendingDrag) return;

    // If drag never actually started (below threshold), just reset — let click fire normally
    if (!state.isDragging) {
      resetState();
      return;
    }

    const targetItem = getTreeItemFromPoint(e.clientX, e.clientY);

    if (targetItem && !isDescendantOf(targetItem, state.sourceElement)) {
      const targetValue = targetItem.getAttribute('data-value');
      const position = getDropPosition(targetItem, e.clientY);

      if (state.sourceValue && targetValue) {
        dotNetRef.invokeMethodAsync('JsOnNodeDrop', state.sourceValue, targetValue, position);
      }
    }

    resetState();
  };

  const handlePointerCancel = () => {
    resetState();
  };

  containerElement.addEventListener('pointerdown', handlePointerDown);
  containerElement.addEventListener('pointermove', handlePointerMove);
  containerElement.addEventListener('pointerup', handlePointerUp);
  containerElement.addEventListener('pointercancel', handlePointerCancel);

  state.handlePointerDown = handlePointerDown;
  state.handlePointerMove = handlePointerMove;
  state.handlePointerUp = handlePointerUp;
  state.handlePointerCancel = handlePointerCancel;

  instances.set(instanceId, state);
}

/**
 * Dispose drag and drop for a tree view instance.
 * @param {string} instanceId
 */
export function disposeDragDrop(instanceId) {
  const state = instances.get(instanceId);
  if (!state) return;

  if (state.containerElement) {
    state.containerElement.removeEventListener('pointerdown', state.handlePointerDown);
    state.containerElement.removeEventListener('pointermove', state.handlePointerMove);
    state.containerElement.removeEventListener('pointerup', state.handlePointerUp);
    state.containerElement.removeEventListener('pointercancel', state.handlePointerCancel);
  }

  if (state.indicator && state.indicator.parentElement) {
    state.indicator.parentElement.removeChild(state.indicator);
  }

  instances.delete(instanceId);
}

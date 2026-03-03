// Dashboard Grid - drag, resize, and responsive breakpoint handler
// Uses pointer events with capture for smooth drag/resize interactions

const instances = new Map();
const DRAG_THRESHOLD = 5;

export function initializeDashboardGrid(dotNetRef, instanceId, options) {
  if (!dotNetRef) return;

  const state = {
    dotNetRef,
    options,
    isDragging: false,
    isResizing: false,
    pendingDrag: false,
    pendingResize: false,
    startX: 0,
    startY: 0,
    pointerId: null,
    activeWidgetId: null,
    activeHandle: null,
    ghostElement: null,
    placeholderElement: null,
    originalWidget: null,
    startCol: 0,
    startRow: 0,
    startColSpan: 0,
    startRowSpan: 0,
  };

  instances.set(instanceId, state);
  setupResizeObserver(instanceId, state);
  setupEventListeners(instanceId, state);
}

export function updateGridOptions(instanceId, options) {
  const state = instances.get(instanceId);
  if (!state) return;
  state.options = options;
}

export function updateWidgetPositions(instanceId, positions) {
  // After a .NET state restore, sync DOM inline styles
  if (!positions) return;
  for (const pos of positions) {
    const widget = document.querySelector(`[data-widget-id="${pos.widgetId}"]`);
    if (widget) {
      widget.style.gridColumn = `${pos.column} / span ${pos.columnSpan}`;
      widget.style.gridRow = `${pos.row} / span ${pos.rowSpan}`;
    }
  }
}

export function setEditable(instanceId, editable) {
  const state = instances.get(instanceId);
  if (!state) return;
  state.options.editable = editable;
  state.options.allowDrag = editable && state.options.allowDrag;
  state.options.allowResize = editable && state.options.allowResize;
}

export function disposeDashboardGrid(instanceId) {
  const state = instances.get(instanceId);
  if (!state) return;

  if (state.resizeObserver) {
    state.resizeObserver.disconnect();
  }

  cleanupListeners(state);
  instances.delete(instanceId);
}

// --- Resize Observer for responsive breakpoints ---

function setupResizeObserver(instanceId, state) {
  const checkBreakpoint = () => {
    const width = window.innerWidth;
    let bp = 'large';
    if (width < state.options.smallBreakpoint) {
      bp = 'small';
    } else if (width < state.options.mediumBreakpoint) {
      bp = 'medium';
    }

    if (state.currentBreakpoint !== bp) {
      state.currentBreakpoint = bp;
      state.dotNetRef.invokeMethodAsync('JsOnBreakpointChanged', bp).catch(() => {});
    }
  };

  checkBreakpoint();
  state.resizeObserver = new ResizeObserver(() => checkBreakpoint());
  state.resizeObserver.observe(document.body);
}

// --- Event Listeners ---

function setupEventListeners(instanceId, state) {
  state.handlePointerDown = (e) => onPointerDown(instanceId, state, e);
  state.handlePointerMove = (e) => onPointerMove(instanceId, state, e);
  state.handlePointerUp = (e) => onPointerUp(instanceId, state, e);
  state.handlePointerCancel = (e) => onPointerCancel(instanceId, state, e);
  state.handleKeyDown = (e) => onKeyDown(instanceId, state, e);

  document.addEventListener('pointerdown', state.handlePointerDown);
  document.addEventListener('pointermove', state.handlePointerMove);
  document.addEventListener('pointerup', state.handlePointerUp);
  document.addEventListener('pointercancel', state.handlePointerCancel);
  document.addEventListener('keydown', state.handleKeyDown);
}

function cleanupListeners(state) {
  document.removeEventListener('pointerdown', state.handlePointerDown);
  document.removeEventListener('pointermove', state.handlePointerMove);
  document.removeEventListener('pointerup', state.handlePointerUp);
  document.removeEventListener('pointercancel', state.handlePointerCancel);
  document.removeEventListener('keydown', state.handleKeyDown);
}

// --- Pointer Down ---

function onPointerDown(instanceId, state, e) {
  if (!state.options.editable || e.button !== 0) return;

  // Check for drag handle
  const dragHandle = e.target.closest('[data-dashboard-drag-handle]');
  if (dragHandle && state.options.allowDrag) {
    const widgetId = dragHandle.getAttribute('data-widget-id');
    const widgetEl = document.querySelector(`[data-widget-id="${widgetId}"]`);
    if (!widgetEl) return;

    e.preventDefault();
    state.pendingDrag = true;
    state.activeWidgetId = widgetId;
    state.originalWidget = widgetEl;
    state.startX = e.clientX;
    state.startY = e.clientY;
    state.pointerId = e.pointerId;

    const style = widgetEl.style;
    state.startCol = parseGridValue(style.gridColumn, 'start');
    state.startRow = parseGridValue(style.gridRow, 'start');
    state.startColSpan = parseGridValue(style.gridColumn, 'span');
    state.startRowSpan = parseGridValue(style.gridRow, 'span');
    return;
  }

  // Check for resize handle
  const resizeHandle = e.target.closest('[data-dashboard-resize-handle]');
  if (resizeHandle && state.options.allowResize) {
    const widgetId = resizeHandle.getAttribute('data-widget-id');
    const widgetEl = document.querySelector(`[data-widget-id="${widgetId}"]`);
    if (!widgetEl) return;

    e.preventDefault();
    state.pendingResize = true;
    state.activeWidgetId = widgetId;
    state.activeHandle = resizeHandle.getAttribute('data-dashboard-resize-handle');
    state.originalWidget = widgetEl;
    state.startX = e.clientX;
    state.startY = e.clientY;
    state.pointerId = e.pointerId;

    const style = widgetEl.style;
    state.startCol = parseGridValue(style.gridColumn, 'start');
    state.startRow = parseGridValue(style.gridRow, 'start');
    state.startColSpan = parseGridValue(style.gridColumn, 'span');
    state.startRowSpan = parseGridValue(style.gridRow, 'span');
    return;
  }
}

// --- Pointer Move ---

function onPointerMove(instanceId, state, e) {
  if (e.pointerId !== state.pointerId) return;

  const dx = e.clientX - state.startX;
  const dy = e.clientY - state.startY;
  const distance = Math.sqrt(dx * dx + dy * dy);

  // Activate drag after threshold
  if (state.pendingDrag && !state.isDragging) {
    if (distance >= DRAG_THRESHOLD) {
      activateDrag(state);
    }
    return;
  }

  // Activate resize after threshold
  if (state.pendingResize && !state.isResizing) {
    if (distance >= DRAG_THRESHOLD) {
      activateResize(state);
    }
    return;
  }

  if (state.isDragging) {
    e.preventDefault();
    updateDrag(state, e);
  }

  if (state.isResizing) {
    e.preventDefault();
    updateResize(state, e);
  }
}

// --- Pointer Up ---

function onPointerUp(instanceId, state, e) {
  if (e.pointerId !== state.pointerId) return;

  if (state.isDragging) {
    finishDrag(state);
  } else if (state.isResizing) {
    finishResize(state);
  }

  resetState(state);
}

function onPointerCancel(instanceId, state, e) {
  if (e.pointerId !== state.pointerId) return;
  cancelInteraction(state);
}

// --- Drag Logic ---

function activateDrag(state) {
  state.isDragging = true;
  state.pendingDrag = false;

  // Capture all widget positions at drag start for stable swap detection
  const grid = state.originalWidget.closest('[role="region"][aria-roledescription="dashboard grid"]');
  if (grid) {
    state.dragGrid = grid;
    state.originalPositions = getWidgetPositions(grid, null);
  }

  document.body.style.userSelect = 'none';
  document.body.style.cursor = 'grabbing';

  state.originalWidget.setAttribute('data-dragging', 'true');
  state.originalWidget.style.opacity = '0.5';
  state.originalWidget.style.zIndex = '50';
}

function updateDrag(state, e) {
  const grid = state.dragGrid;
  if (!grid) return;

  const gridRect = grid.getBoundingClientRect();
  const cols = state.options.columns;
  const gap = state.options.gap;
  const cellWidth = (gridRect.width - (cols - 1) * gap) / cols;
  const rowHeight = state.options.rowHeight;

  // Calculate target column and row from pointer position
  const relX = e.clientX - gridRect.left;
  const relY = e.clientY - gridRect.top;

  let targetCol = Math.round(relX / (cellWidth + gap)) + 1;
  let targetRow = Math.round(relY / (rowHeight + gap)) + 1;

  // Clamp to grid bounds
  targetCol = Math.max(1, Math.min(targetCol, cols - state.startColSpan + 1));
  targetRow = Math.max(1, targetRow);

  // Skip if target hasn't changed
  if (targetCol === state.lastResolvedCol && targetRow === state.lastResolvedRow) return;

  // Build proposed layout from original positions with dragged widget at target
  const proposed = state.originalPositions.map(p => ({...p}));
  const dragged = proposed.find(p => p.id === state.activeWidgetId);
  if (!dragged) return;
  dragged.col = targetCol;
  dragged.row = targetRow;

  // Resolve all overlaps by pushing displaced widgets
  const resolved = resolveLayout(proposed, state.activeWidgetId,
    state.startCol, state.startRow, cols);

  if (!resolved) {
    // Couldn't resolve — restore original layout
    applyLayout(grid, state.originalPositions);
    state.originalWidget.style.opacity = '0.5';
    state.originalWidget.style.zIndex = '50';
    state.lastResolvedCol = undefined;
    state.lastResolvedRow = undefined;
    return;
  }

  // Apply resolved layout to DOM
  applyLayout(grid, resolved);
  state.originalWidget.style.opacity = '0.5';
  state.originalWidget.style.zIndex = '50';

  state.resolvedLayout = resolved;
  state.targetCol = targetCol;
  state.targetRow = targetRow;
  state.lastResolvedCol = targetCol;
  state.lastResolvedRow = targetRow;
}

function finishDrag(state) {
  state.originalWidget.style.opacity = '';
  state.originalWidget.style.zIndex = '';
  state.originalWidget.setAttribute('data-dragging', 'false');

  document.body.style.userSelect = '';
  document.body.style.cursor = '';

  if (state.resolvedLayout && state.originalPositions) {
    // Find all widgets that changed position
    const changes = [];
    for (const resolved of state.resolvedLayout) {
      const original = state.originalPositions.find(p => p.id === resolved.id);
      if (!original) continue;
      if (resolved.col !== original.col || resolved.row !== original.row) {
        changes.push({ id: resolved.id, col: resolved.col, row: resolved.row });
      }
    }

    if (changes.length > 0) {
      state.dotNetRef.invokeMethodAsync('JsOnLayoutResolved',
        state.activeWidgetId, changes).catch(() => {});
      announceChange(state, `Dashboard layout updated`);
    }
  }
}

// --- Resize Logic ---

function activateResize(state) {
  state.isResizing = true;
  state.pendingResize = false;

  document.body.style.userSelect = 'none';
  state.originalWidget.setAttribute('data-resizing', 'true');
}

function updateResize(state, e) {
  const grid = state.originalWidget.closest('[role="region"][aria-roledescription="dashboard grid"]');
  if (!grid) return;

  const gridRect = grid.getBoundingClientRect();
  const cols = state.options.columns;
  const gap = state.options.gap;
  const cellWidth = (gridRect.width - (cols - 1) * gap) / cols;
  const rowHeight = state.options.rowHeight;

  const dx = e.clientX - state.startX;
  const dy = e.clientY - state.startY;

  let newColSpan = state.startColSpan;
  let newRowSpan = state.startRowSpan;

  const handle = state.activeHandle;

  if (handle === 'se' || handle === 'e') {
    newColSpan = Math.max(1, Math.round((state.startColSpan * (cellWidth + gap) + dx) / (cellWidth + gap)));
  }

  if (handle === 'se' || handle === 's') {
    newRowSpan = Math.max(1, Math.round((state.startRowSpan * (rowHeight + gap) + dy) / (rowHeight + gap)));
  }

  // Clamp to grid max columns
  const maxCol = cols - state.startCol + 1;
  newColSpan = Math.min(newColSpan, maxCol);

  // Apply min/max from widget data attributes
  const widget = state.originalWidget;
  const minColSpan = parseInt(widget.getAttribute('data-min-col-span') || '1', 10);
  const minRowSpan = parseInt(widget.getAttribute('data-min-row-span') || '1', 10);
  const maxColSpan = parseInt(widget.getAttribute('data-max-col-span') || '0', 10);
  const maxRowSpan = parseInt(widget.getAttribute('data-max-row-span') || '0', 10);

  newColSpan = Math.max(minColSpan, newColSpan);
  newRowSpan = Math.max(minRowSpan, newRowSpan);
  if (maxColSpan > 0) newColSpan = Math.min(maxColSpan, newColSpan);
  if (maxRowSpan > 0) newRowSpan = Math.min(maxRowSpan, newRowSpan);

  // Collision detection — only resize if new size doesn't overlap other widgets
  const others = getWidgetPositions(grid, state.activeWidgetId);
  if (wouldOverlap(state.startCol, state.startRow, newColSpan, newRowSpan, others)) {
    return; // Keep current size, don't update
  }

  // Live update
  widget.style.gridColumn = `${state.startCol} / span ${newColSpan}`;
  widget.style.gridRow = `${state.startRow} / span ${newRowSpan}`;

  state.targetColSpan = newColSpan;
  state.targetRowSpan = newRowSpan;
}

function finishResize(state) {
  const colSpan = state.targetColSpan || state.startColSpan;
  const rowSpan = state.targetRowSpan || state.startRowSpan;

  state.originalWidget.setAttribute('data-resizing', 'false');
  document.body.style.userSelect = '';

  if (colSpan !== state.startColSpan || rowSpan !== state.startRowSpan) {
    state.dotNetRef.invokeMethodAsync('JsOnWidgetResizeEnd', state.activeWidgetId, colSpan, rowSpan).catch(() => {});

    announceChange(state, `Widget resized to ${colSpan} columns, ${rowSpan} rows`);
  }
}

// --- Keyboard Navigation ---

function onKeyDown(instanceId, state, e) {
  if (!state.options.editable) return;

  const target = e.target;

  // Handle drag handle keyboard (arrow keys to move)
  const dragHandle = target.closest('[data-dashboard-drag-handle]');
  if (dragHandle && state.options.allowDrag) {
    const widgetId = dragHandle.getAttribute('data-widget-id');
    const widget = document.querySelector(`[data-widget-id="${widgetId}"]`);
    if (!widget) return;

    const grid = widget.closest('[role="region"][aria-roledescription="dashboard grid"]');
    if (!grid) return;

    const style = widget.style;
    const col = parseGridValue(style.gridColumn, 'start');
    const row = parseGridValue(style.gridRow, 'start');
    const colSpan = parseGridValue(style.gridColumn, 'span');
    const rowSpan = parseGridValue(style.gridRow, 'span');
    const cols = state.options.columns;

    let newCol = col;
    let newRow = row;
    let handled = false;

    if (e.key === 'ArrowLeft') { newCol = Math.max(1, col - 1); handled = true; }
    else if (e.key === 'ArrowRight') { newCol = Math.min(cols - colSpan + 1, col + 1); handled = true; }
    else if (e.key === 'ArrowUp') { newRow = Math.max(1, row - 1); handled = true; }
    else if (e.key === 'ArrowDown') { newRow = row + 1; handled = true; }

    if (handled) {
      e.preventDefault();
      if (newCol !== col || newRow !== row) {
        const others = getWidgetPositions(grid, widgetId);
        if (wouldOverlap(newCol, newRow, colSpan, rowSpan, others)) { return; }
        state.dotNetRef.invokeMethodAsync('JsOnWidgetDragEnd', widgetId, newCol, newRow).catch(() => {});
        announceChange(state, `Widget moved to column ${newCol}, row ${newRow}`);
      }
    }
    return;
  }

  // Handle resize handle keyboard (shift+arrow keys to resize)
  const resizeHandle = target.closest('[data-dashboard-resize-handle]');
  if (resizeHandle && state.options.allowResize) {
    const widgetId = resizeHandle.getAttribute('data-widget-id');
    const widget = document.querySelector(`[data-widget-id="${widgetId}"]`);
    if (!widget) return;

    if (!e.shiftKey) return;

    const grid = widget.closest('[role="region"][aria-roledescription="dashboard grid"]');
    if (!grid) return;

    const style = widget.style;
    const col = parseGridValue(style.gridColumn, 'start');
    const row = parseGridValue(style.gridRow, 'start');
    const colSpan = parseGridValue(style.gridColumn, 'span');
    const rowSpan = parseGridValue(style.gridRow, 'span');
    const cols = state.options.columns;

    const minColSpan = parseInt(widget.getAttribute('data-min-col-span') || '1', 10);
    const minRowSpan = parseInt(widget.getAttribute('data-min-row-span') || '1', 10);
    const maxColSpan = parseInt(widget.getAttribute('data-max-col-span') || '0', 10);
    const maxRowSpan = parseInt(widget.getAttribute('data-max-row-span') || '0', 10);

    let newColSpan = colSpan;
    let newRowSpan = rowSpan;
    let handled = false;

    if (e.key === 'ArrowRight') { newColSpan = colSpan + 1; handled = true; }
    else if (e.key === 'ArrowLeft') { newColSpan = colSpan - 1; handled = true; }
    else if (e.key === 'ArrowDown') { newRowSpan = rowSpan + 1; handled = true; }
    else if (e.key === 'ArrowUp') { newRowSpan = rowSpan - 1; handled = true; }

    if (handled) {
      e.preventDefault();
      newColSpan = Math.max(minColSpan, Math.min(newColSpan, cols - col + 1));
      newRowSpan = Math.max(minRowSpan, newRowSpan);
      if (maxColSpan > 0) newColSpan = Math.min(maxColSpan, newColSpan);
      if (maxRowSpan > 0) newRowSpan = Math.min(maxRowSpan, newRowSpan);

      if (newColSpan !== colSpan || newRowSpan !== rowSpan) {
        const others = getWidgetPositions(grid, widgetId);
        if (wouldOverlap(col, row, newColSpan, newRowSpan, others)) { return; }
        state.dotNetRef.invokeMethodAsync('JsOnWidgetResizeEnd', widgetId, newColSpan, newRowSpan).catch(() => {});
        announceChange(state, `Widget resized to ${newColSpan} columns, ${newRowSpan} rows`);
      }
    }
    return;
  }

  // Escape cancels active drag/resize
  if (e.key === 'Escape' && (state.isDragging || state.isResizing)) {
    e.preventDefault();
    cancelInteraction(state);
  }
}

// --- Collision Detection & Layout Resolution ---

function getWidgetPositions(grid, excludeWidgetId) {
  // Use :scope > to select only direct child widgets, not nested
  // resize/drag handles which also carry data-widget-id attributes.
  const widgets = grid.querySelectorAll(':scope > [data-widget-id]');
  const positions = [];
  for (const w of widgets) {
    const id = w.getAttribute('data-widget-id');
    if (id === excludeWidgetId) continue;
    const col = parseGridValue(w.style.gridColumn, 'start');
    const row = parseGridValue(w.style.gridRow, 'start');
    const colSpan = parseGridValue(w.style.gridColumn, 'span');
    const rowSpan = parseGridValue(w.style.gridRow, 'span');
    positions.push({ id, col, row, colSpan, rowSpan });
  }
  return positions;
}

function wouldOverlap(col, row, colSpan, rowSpan, others) {
  for (const o of others) {
    if (
      col < o.col + o.colSpan &&
      o.col < col + colSpan &&
      row < o.row + o.rowSpan &&
      o.row < row + rowSpan
    ) {
      return true;
    }
  }
  return false;
}

function rectsOverlap(a, b) {
  return a.col < b.col + b.colSpan &&
    b.col < a.col + a.colSpan &&
    a.row < b.row + b.rowSpan &&
    b.row < a.row + a.rowSpan;
}

function findFirstOverlap(layout) {
  for (let i = 0; i < layout.length; i++) {
    for (let j = i + 1; j < layout.length; j++) {
      if (rectsOverlap(layout[i], layout[j])) {
        return [layout[i], layout[j]];
      }
    }
  }
  return null;
}

/**
 * Resolve all overlaps in the layout by pushing displaced widgets.
 * Strategy: displaced widgets try the dragged widget's vacated position first,
 * then get pushed below the widget that displaced them.
 */
function resolveLayout(layout, movedId, origCol, origRow, cols) {
  for (let iter = 0; iter < 100; iter++) {
    const pair = findFirstOverlap(layout);
    if (!pair) return layout; // no overlaps — resolved

    const [a, b] = pair;

    // The dragged widget always takes priority. For cascade pushes,
    // the widget at the higher/earlier position pushes the other.
    let pusher, pushed;
    if (a.id === movedId) { pusher = a; pushed = b; }
    else if (b.id === movedId) { pusher = b; pushed = a; }
    else {
      if (a.row < b.row || (a.row === b.row && a.col <= b.col)) {
        pusher = a; pushed = b;
      } else {
        pusher = b; pushed = a;
      }
    }

    // Strategy 1: For widgets directly displaced by the dragged widget,
    // try placing them at the dragged widget's vacated position.
    if (pusher.id === movedId) {
      const fits = origCol + pushed.colSpan - 1 <= cols &&
        !wouldOverlap(origCol, origRow, pushed.colSpan, pushed.rowSpan,
          layout.filter(w => w.id !== pushed.id));
      if (fits) {
        pushed.col = origCol;
        pushed.row = origRow;
        continue;
      }
    }

    // Strategy 2: Push below the pusher, keep same column.
    pushed.row = pusher.row + pusher.rowSpan;

    // If pushed off-grid horizontally, move to column 1
    if (pushed.col + pushed.colSpan - 1 > cols) {
      pushed.col = 1;
    }
  }

  return null; // couldn't resolve
}

function applyLayout(grid, layout) {
  for (const pos of layout) {
    const el = grid.querySelector(`:scope > [data-widget-id="${pos.id}"]`);
    if (el) {
      el.style.gridColumn = `${pos.col} / span ${pos.colSpan}`;
      el.style.gridRow = `${pos.row} / span ${pos.rowSpan}`;
    }
  }
}

// --- Helpers ---

function parseGridValue(gridProp, type) {
  if (!gridProp) return 1;
  // Format: "col / span colSpan" or "row / span rowSpan"
  const parts = gridProp.split('/').map(s => s.trim());
  if (type === 'start') {
    return parseInt(parts[0], 10) || 1;
  }
  if (type === 'span') {
    const spanPart = parts[1] || '';
    const match = spanPart.match(/span\s+(\d+)/);
    return match ? parseInt(match[1], 10) : 1;
  }
  return 1;
}

function cancelInteraction(state) {
  // Restore all widgets to their original positions
  if (state.dragGrid && state.originalPositions) {
    applyLayout(state.dragGrid, state.originalPositions);
  }

  if (state.originalWidget) {
    state.originalWidget.style.opacity = '';
    state.originalWidget.style.zIndex = '';
    state.originalWidget.setAttribute('data-dragging', 'false');
    state.originalWidget.setAttribute('data-resizing', 'false');
  }

  document.body.style.userSelect = '';
  document.body.style.cursor = '';

  resetState(state);
}

function resetState(state) {
  state.isDragging = false;
  state.isResizing = false;
  state.pendingDrag = false;
  state.pendingResize = false;
  state.activeWidgetId = null;
  state.activeHandle = null;
  state.originalWidget = null;
  state.pointerId = null;
  state.targetCol = undefined;
  state.targetRow = undefined;
  state.targetColSpan = undefined;
  state.targetRowSpan = undefined;
  state.dragGrid = null;
  state.originalPositions = null;
  state.resolvedLayout = null;
  state.lastResolvedCol = undefined;
  state.lastResolvedRow = undefined;
}

function announceChange(state, message) {
  // Find the live region associated with this instance
  const liveRegions = document.querySelectorAll('[aria-live="polite"]');
  for (const region of liveRegions) {
    if (region.classList.contains('sr-only')) {
      region.textContent = message;
      setTimeout(() => { region.textContent = ''; }, 1000);
      return;
    }
  }
}

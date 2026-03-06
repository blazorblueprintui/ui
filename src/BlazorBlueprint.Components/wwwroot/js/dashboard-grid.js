// Dashboard Grid - drag, resize, and responsive breakpoint handler
// Uses pointer events with capture for smooth drag/resize interactions

const instances = new Map();
const DRAG_THRESHOLD = 5;

export function initializeDashboardGrid(dotNetRef, instanceId, options) {
  if (!dotNetRef) return;

  const gridEl = document.querySelector(`[data-dashboard-id="${instanceId}"]`);
  if (!gridEl) return;

  const state = {
    dotNetRef,
    options,
    gridEl,
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

  // Preserve the configured drag/resize values so setEditable can restore them
  state.configuredAllowDrag = options.allowDrag;
  state.configuredAllowResize = options.allowResize;

  instances.set(instanceId, state);
  setupResizeObserver(instanceId, state);
  setupEventListeners(instanceId, state);
  setupMutationObserver(state);
  syncGridGuide(state);

  // Run initial compaction and reveal widgets
  runCompactAndReveal(state);
}

export function updateGridOptions(instanceId, options) {
  const state = instances.get(instanceId);
  if (!state) return;
  state.configuredAllowDrag = options.allowDrag;
  state.configuredAllowResize = options.allowResize;
  state.options = options;
  syncGridGuide(state);
}

export function updateWidgetPositions(instanceId, positions) {
  // After a .NET state restore, sync DOM inline styles
  if (!positions) return;
  const state = instances.get(instanceId);
  const container = state ? state.gridEl : null;
  for (const pos of positions) {
    const widget = container
      ? container.querySelector(`[data-widget-id="${pos.widgetId}"]`)
      : document.querySelector(`[data-widget-id="${pos.widgetId}"]`);
    if (widget) {
      widget.style.gridColumn = `${pos.column} / span ${pos.columnSpan}`;
      widget.style.gridRow = `${pos.row} / span ${pos.rowSpan}`;
    }
  }
}

export function requestCompact(instanceId) {
  const state = instances.get(instanceId);
  if (!state || state.isDragging || state.isResizing) return;
  runCompactAndReveal(state);
}

export function setEditable(instanceId, editable) {
  const state = instances.get(instanceId);
  if (!state) return;
  state.options.editable = editable;
  // Recompute from the original configured values (passed during init/update)
  // so toggling editable off/on restores the intended behavior.
  state.options.allowDrag = editable && (state.configuredAllowDrag ?? state.options.allowDrag);
  state.options.allowResize = editable && (state.configuredAllowResize ?? state.options.allowResize);
}

export function disposeDashboardGrid(instanceId) {
  const state = instances.get(instanceId);
  if (!state) return;

  if (state.resizeObserver) {
    state.resizeObserver.disconnect();
  }
  if (state.mutationObserver) {
    state.mutationObserver.disconnect();
  }

  cleanupListeners(state);
  instances.delete(instanceId);
}

// --- MutationObserver for widget add/remove ---

function setupMutationObserver(state) {
  state.mutationObserver = new MutationObserver((mutations) => {
    if (state.isDragging || state.isResizing) return;

    // Only react if widget elements were added or removed
    let widgetChanged = false;
    for (const mutation of mutations) {
      for (const node of mutation.addedNodes) {
        if (node.nodeType === 1 && node.hasAttribute('data-widget-id')) {
          widgetChanged = true;
          break;
        }
      }
      if (widgetChanged) break;
      for (const node of mutation.removedNodes) {
        if (node.nodeType === 1 && node.hasAttribute('data-widget-id')) {
          widgetChanged = true;
          break;
        }
      }
      if (widgetChanged) break;
    }

    if (widgetChanged) {
      runCompactAndReveal(state);
    }
  });

  state.mutationObserver.observe(state.gridEl, { childList: true });
}

function getActiveColumns(state) {
  return getColumnsForBreakpoint(state, state.currentBreakpoint || 'large');
}

function runCompactAndReveal(state) {
  const grid = state.gridEl;
  const positions = getWidgetPositions(grid, null);

  if (positions.length === 0) {
    grid.setAttribute('data-positioned', 'true');
    return;
  }

  const columns = getActiveColumns(state);

  // Clamp column spans to fit within the current column count
  for (const pos of positions) {
    if (pos.colSpan > columns) {
      pos.colSpan = columns;
    }
    if (pos.col + pos.colSpan - 1 > columns) {
      pos.col = Math.max(1, columns - pos.colSpan + 1);
    }
  }

  if (state.options.compact) {
    compact(positions, null, columns);
    applyLayout(grid, positions);
  }

  grid.setAttribute('data-positioned', 'true');

  // Send compacted positions to .NET state
  const dtos = positions.map(p => ({
    widgetId: p.id,
    col: p.col,
    row: p.row,
    colSpan: p.colSpan,
    rowSpan: p.rowSpan
  }));
  state.dotNetRef.invokeMethodAsync('JsOnCompactComplete', dtos)
    .catch(err => console.error('JsOnCompactComplete failed:', err));
}

// --- Resize Observer for responsive breakpoints ---

function getColumnsForBreakpoint(state, bp) {
  if (bp === 'small') return state.options.smallColumns;
  if (bp === 'medium') return state.options.mediumColumns;
  return state.options.columns;
}

// Measures the grid's actual rendered dimensions and generates a perfectly
// aligned SVG mask for the background squares. Called on init and resize.
function syncGridGuide(state) {
  const el = state.gridEl;
  if (!el) return;

  const cs = getComputedStyle(el);

  // Sync padding so the ::before pseudo covers exactly the content area
  el.style.setProperty('--bb-grid-pad-top', cs.paddingTop);
  el.style.setProperty('--bb-grid-pad-right', cs.paddingRight);
  el.style.setProperty('--bb-grid-pad-bottom', cs.paddingBottom);
  el.style.setProperty('--bb-grid-pad-left', cs.paddingLeft);

  // Read the browser's actual computed column width — no JS math drift
  const tracks = cs.gridTemplateColumns.split(/\s+/);
  const cellWidth = parseFloat(tracks[0]) || 0;
  const gap = state.options.gap;
  const rowHeight = state.options.rowHeight;

  const tileW = cellWidth + gap;
  const tileH = rowHeight + gap;

  // Generate SVG tile matching actual grid cell dimensions
  const svg = `<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 ${tileW} ${tileH}'>`
    + `<rect x='1' y='1' width='${cellWidth - 2}' height='${rowHeight - 2}' rx='3' ry='3' fill='none' stroke='white'/>`
    + `</svg>`;
  const encoded = encodeURIComponent(svg);

  el.style.setProperty('--bb-dashboard-guide-svg', `url("data:image/svg+xml,${encoded}")`);
  el.style.setProperty('--bb-dashboard-cell-w', `${tileW}px`);
  el.style.setProperty('--bb-dashboard-cell-h', `${tileH}px`);
}

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
      const isInitial = state.currentBreakpoint === undefined;
      const prevBp = state.currentBreakpoint;
      state.currentBreakpoint = bp;
      state.dotNetRef.invokeMethodAsync('JsOnBreakpointChanged', bp)
        .catch(err => console.error('JsOnBreakpointChanged failed:', err));

      // Re-compact widgets for the new column count (skip initial detection)
      if (!isInitial) {
        recompactForBreakpoint(state, prevBp, bp);
      }
    }
  };

  checkBreakpoint();
  state.resizeObserver = new ResizeObserver(() => {
    checkBreakpoint();
    syncGridGuide(state);
  });
  state.resizeObserver.observe(document.body);
}

function recompactForBreakpoint(state, prevBp, bp) {
  const grid = state.gridEl;
  if (!grid) return;

  const columns = getColumnsForBreakpoint(state, bp);

  // Save current positions keyed by the breakpoint we're leaving
  if (!state.savedPositions) {
    state.savedPositions = {};
  }
  state.savedPositions[prevBp] = getWidgetPositions(grid, null);

  // If we have saved positions for the target breakpoint, restore them
  if (state.savedPositions[bp]) {
    const positions = state.savedPositions[bp];
    delete state.savedPositions[bp];

    if (state.options.compact) {
      compact(positions, null, columns);
    }

    applyLayout(grid, positions);
    syncPositionsToNet(state, positions);
    return;
  }

  const positions = getWidgetPositions(grid, null);
  if (positions.length === 0) return;

  // Clamp column spans to fit within the new column count
  for (const pos of positions) {
    if (pos.colSpan > columns) {
      pos.colSpan = columns;
    }
    if (pos.col + pos.colSpan - 1 > columns) {
      pos.col = Math.max(1, columns - pos.colSpan + 1);
    }
  }

  if (state.options.compact) {
    compact(positions, null, columns);
  }

  applyLayout(grid, positions);
  syncPositionsToNet(state, positions);
}

function syncPositionsToNet(state, positions) {
  const dtos = positions.map(p => ({
    widgetId: p.id,
    col: p.col,
    row: p.row,
    colSpan: p.colSpan,
    rowSpan: p.rowSpan
  }));
  state.dotNetRef.invokeMethodAsync('JsOnCompactComplete', dtos)
    .catch(err => console.error('JsOnCompactComplete (breakpoint) failed:', err));
}

// --- Event Listeners ---

function setupEventListeners(instanceId, state) {
  state.handlePointerDown = (e) => onPointerDown(instanceId, state, e);
  state.handlePointerMove = (e) => onPointerMove(instanceId, state, e);
  state.handlePointerUp = (e) => onPointerUp(instanceId, state, e);
  state.handlePointerCancel = (e) => onPointerCancel(instanceId, state, e);
  state.handleKeyDown = (e) => onKeyDown(instanceId, state, e);
  state.handleFocusOut = (e) => onWidgetFocusOut(instanceId, state, e);

  state.gridEl.addEventListener('pointerdown', state.handlePointerDown);
  document.addEventListener('pointermove', state.handlePointerMove);
  document.addEventListener('pointerup', state.handlePointerUp);
  document.addEventListener('pointercancel', state.handlePointerCancel);
  state.gridEl.addEventListener('keydown', state.handleKeyDown);
  state.gridEl.addEventListener('focusout', state.handleFocusOut);
}

function cleanupListeners(state) {
  if (state.gridEl) {
    state.gridEl.removeEventListener('pointerdown', state.handlePointerDown);
    state.gridEl.removeEventListener('keydown', state.handleKeyDown);
    state.gridEl.removeEventListener('focusout', state.handleFocusOut);
  }
  document.removeEventListener('pointermove', state.handlePointerMove);
  document.removeEventListener('pointerup', state.handlePointerUp);
  document.removeEventListener('pointercancel', state.handlePointerCancel);
}

// --- Pointer Down ---

function onPointerDown(instanceId, state, e) {
  if (!state.options.editable || e.button !== 0) return;

  // Focus the clicked widget so keyboard navigation works immediately
  const clickedWidget = e.target.closest('[data-widget-id]');
  if (clickedWidget) {
    clickedWidget.focus();
  }

  // Check for resize handle first (takes priority over widget drag)
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

  // Check for widget drag (click anywhere on the widget)
  if (state.options.allowDrag) {
    const widgetEl = e.target.closest('[data-widget-id]');
    if (widgetEl && widgetEl.getAttribute('data-static') !== 'true') {
      const widgetId = widgetEl.getAttribute('data-widget-id');

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
  state.gridEl.setPointerCapture(state.pointerId);

  // Capture all widget positions at drag start for stable swap detection
  const grid = state.originalWidget.closest('[role="region"][aria-roledescription="dashboard grid"]');
  if (grid) {
    state.dragGrid = grid;
    state.originalPositions = getWidgetPositions(grid, null);
  }

  // Capture grab offset (pointer position relative to widget's top-left corner)
  const widgetRect = state.originalWidget.getBoundingClientRect();
  state.grabOffsetX = state.startX - widgetRect.left;
  state.grabOffsetY = state.startY - widgetRect.top;

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
  const cols = getActiveColumns(state);
  const gap = state.options.gap;
  const cellWidth = (gridRect.width - (cols - 1) * gap) / cols;
  const rowHeight = state.options.rowHeight;

  // Calculate target column and row from pointer position, adjusted for grab offset
  const relX = e.clientX - gridRect.left - (state.grabOffsetX || 0);
  const relY = e.clientY - gridRect.top - (state.grabOffsetY || 0);

  let targetCol = Math.round(relX / (cellWidth + gap)) + 1;
  let targetRow = Math.round(relY / (rowHeight + gap)) + 1;

  // Clamp to grid bounds
  targetCol = Math.max(1, Math.min(targetCol, cols - state.startColSpan + 1));
  targetRow = Math.max(1, targetRow);

  // Skip if target hasn't changed
  if (targetCol === state.lastResolvedCol && targetRow === state.lastResolvedRow) return;

  let resolved = resolveLayoutAfterDrag(
    state.originalPositions, state.activeWidgetId,
    targetCol, targetRow, state.startCol, state.startRow, cols
  );

  // If resolve returned null, reject this drag position (keep last valid layout)
  if (!resolved) {
    state.lastResolvedCol = targetCol;
    state.lastResolvedRow = targetRow;
    return;
  }

  if (state.options.compact) {
    compact(resolved, state.activeWidgetId, cols);
  }
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
  const widgetEl = state.originalWidget;
  widgetEl.style.opacity = '';
  widgetEl.style.zIndex = '';
  widgetEl.setAttribute('data-dragging', 'false');

  document.body.style.userSelect = '';
  document.body.style.cursor = '';

  // Re-focus the widget so keyboard navigation continues to work
  widgetEl.focus();

  if (state.resolvedLayout && state.originalPositions) {
    const finalLayout = state.resolvedLayout;
    const grid = state.dragGrid;

    // Compact after interaction completes
    if (state.options.compact && grid) {
      compact(finalLayout, null, getActiveColumns(state));
      applyLayout(grid, finalLayout);
    }

    // Find all widgets that changed position
    const changes = [];
    for (const resolved of finalLayout) {
      const original = state.originalPositions.find(p => p.id === resolved.id);
      if (!original) continue;
      if (resolved.col !== original.col || resolved.row !== original.row) {
        changes.push({ id: resolved.id, col: resolved.col, row: resolved.row });
      }
    }

    if (changes.length > 0) {
      state.dotNetRef.invokeMethodAsync('JsOnLayoutResolved',
        state.activeWidgetId, changes)
        .catch(err => console.error('JsOnLayoutResolved failed:', err));
      announceChange(state, `Dashboard layout updated`);
    }
  }
}

// --- Resize Logic ---

function activateResize(state) {
  state.isResizing = true;
  state.pendingResize = false;
  state.gridEl.setPointerCapture(state.pointerId);

  // Capture all widget positions at resize start for stable resolution
  const grid = state.originalWidget.closest('[role="region"][aria-roledescription="dashboard grid"]');
  if (grid) {
    state.resizeGrid = grid;
    state.originalResizePositions = getWidgetPositions(grid, null);
  }

  document.body.style.userSelect = 'none';
  state.originalWidget.setAttribute('data-resizing', 'true');
}

function updateResize(state, e) {
  const grid = state.resizeGrid;
  if (!grid || !state.originalResizePositions) return;

  const gridRect = grid.getBoundingClientRect();
  const cols = getActiveColumns(state);
  const gap = state.options.gap;
  const cellWidth = (gridRect.width - (cols - 1) * gap) / cols;
  const rowHeight = state.options.rowHeight;

  const dx = e.clientX - state.startX;
  const dy = e.clientY - state.startY;

  const handle = state.activeHandle;
  const resizeRight = handle === 'e' || handle === 'se' || handle === 'ne';
  const resizeLeft = handle === 'w' || handle === 'sw' || handle === 'nw';
  const resizeBottom = handle === 's' || handle === 'se' || handle === 'sw';
  const resizeTop = handle === 'n' || handle === 'ne' || handle === 'nw';

  let newCol = state.startCol;
  let newRow = state.startRow;
  let newColSpan = state.startColSpan;
  let newRowSpan = state.startRowSpan;

  // Right/bottom: grow span in drag direction
  if (resizeRight) {
    newColSpan = Math.round((state.startColSpan * (cellWidth + gap) + dx) / (cellWidth + gap));
  }
  if (resizeBottom) {
    newRowSpan = Math.round((state.startRowSpan * (rowHeight + gap) + dy) / (rowHeight + gap));
  }

  // Left/top: move position and adjust span inversely (opposite edge stays fixed)
  if (resizeLeft) {
    const colDelta = Math.round(dx / (cellWidth + gap));
    newCol = state.startCol + colDelta;
    newColSpan = state.startColSpan - colDelta;
  }
  if (resizeTop) {
    const rowDelta = Math.round(dy / (rowHeight + gap));
    newRow = state.startRow + rowDelta;
    newRowSpan = state.startRowSpan - rowDelta;
  }

  // Apply min/max from widget data attributes
  const widget = state.originalWidget;
  const minColSpan = parseInt(widget.getAttribute('data-min-col-span') || '1', 10);
  const minRowSpan = parseInt(widget.getAttribute('data-min-row-span') || '1', 10);
  const maxColSpan = parseInt(widget.getAttribute('data-max-col-span') || '0', 10);
  const maxRowSpan = parseInt(widget.getAttribute('data-max-row-span') || '0', 10);

  newColSpan = Math.max(minColSpan, newColSpan);
  newRowSpan = Math.max(minRowSpan, newRowSpan);
  if (maxColSpan > 0) { newColSpan = Math.min(maxColSpan, newColSpan); }
  if (maxRowSpan > 0) { newRowSpan = Math.min(maxRowSpan, newRowSpan); }

  // When resizing from left/top, recalculate position to keep opposite edge fixed
  if (resizeLeft) {
    newCol = state.startCol + state.startColSpan - newColSpan;
  }
  if (resizeTop) {
    newRow = state.startRow + state.startRowSpan - newRowSpan;
  }

  // Grid bounds
  newCol = Math.max(1, newCol);
  newRow = Math.max(1, newRow);
  newColSpan = Math.min(newColSpan, cols - newCol + 1);

  // Skip if nothing changed
  if (newCol === state.lastResizeCol && newRow === state.lastResizeRow &&
      newColSpan === state.lastResizeColSpan && newRowSpan === state.lastResizeRowSpan) return;

  let resolved = resolveLayoutAfterResize(
    state.originalResizePositions, state.activeWidgetId,
    newCol, newRow, newColSpan, newRowSpan,
    state.startCol, state.startRow, state.startColSpan, state.startRowSpan,
    cols
  );

  // If resolve returned null, reject this resize increment (keep last valid layout)
  if (!resolved) {
    state.lastResizeCol = newCol;
    state.lastResizeRow = newRow;
    state.lastResizeColSpan = newColSpan;
    state.lastResizeRowSpan = newRowSpan;
    return;
  }

  if (state.options.compact) {
    compact(resolved, state.activeWidgetId, cols);
  }
  applyLayout(grid, resolved);

  state.resolvedResizeLayout = resolved;
  state.targetCol = newCol;
  state.targetRow = newRow;
  state.targetColSpan = newColSpan;
  state.targetRowSpan = newRowSpan;
  state.lastResizeCol = newCol;
  state.lastResizeRow = newRow;
  state.lastResizeColSpan = newColSpan;
  state.lastResizeRowSpan = newRowSpan;
}

function finishResize(state) {
  state.originalWidget.setAttribute('data-resizing', 'false');
  document.body.style.userSelect = '';

  // Re-focus the widget so keyboard navigation continues to work
  state.originalWidget.focus();

  const finalLayout = state.resolvedResizeLayout;
  const originalPositions = state.originalResizePositions;

  if (!finalLayout || !originalPositions) return;

  // Compact after interaction completes
  const grid = state.resizeGrid;
  if (state.options.compact && grid) {
    compact(finalLayout, null, getActiveColumns(state));
    applyLayout(grid, finalLayout);
  }

  // Find the resized widget's final position
  const resized = finalLayout.find(p => p.id === state.activeWidgetId);
  if (!resized) return;

  const resizeChanged = resized.col !== state.startCol || resized.row !== state.startRow ||
      resized.colSpan !== state.startColSpan || resized.rowSpan !== state.startRowSpan;

  if (resizeChanged) {
    // Send resize event for the resized widget
    state.dotNetRef.invokeMethodAsync('JsOnWidgetResizeEnd',
      state.activeWidgetId, resized.col, resized.row, resized.colSpan, resized.rowSpan)
      .catch(err => console.error('JsOnWidgetResizeEnd failed:', err));

    // Send layout changes for all displaced widgets
    const changes = [];
    for (const final of finalLayout) {
      if (final.id === state.activeWidgetId) continue;
      const original = originalPositions.find(p => p.id === final.id);
      if (!original) continue;
      if (final.col !== original.col || final.row !== original.row) {
        changes.push({ id: final.id, col: final.col, row: final.row });
      }
    }
    if (changes.length > 0) {
      state.dotNetRef.invokeMethodAsync('JsOnLayoutResolved',
        state.activeWidgetId, changes)
        .catch(err => console.error('JsOnLayoutResolved (resize) failed:', err));
    }

    announceChange(state, `Widget resized to ${resized.colSpan} columns, ${resized.rowSpan} rows`);
  }
}

// --- Keyboard Navigation ---

function onKeyDown(instanceId, state, e) {
  if (!state.options.editable) return;

  const target = e.target;

  // Handle resize handle keyboard (shift+arrow keys to resize)
  // Must be checked before the widget move handler since resize handles
  // also carry data-widget-id and would match the move selector.
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
    const cols = getActiveColumns(state);

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
        const allPositions = getWidgetPositions(grid, null);

        let resolved = resolveLayoutAfterResize(
          allPositions, widgetId, col, row, newColSpan, newRowSpan,
          col, row, colSpan, rowSpan, cols
        );
        if (resolved && state.options.compact) {
          compact(resolved, widgetId, cols);
        }

        if (!resolved) return;
        applyLayout(grid, resolved);

        // Send resize for the resized widget
        state.dotNetRef.invokeMethodAsync('JsOnWidgetResizeEnd', widgetId, col, row, newColSpan, newRowSpan)
          .catch(err => console.error('JsOnWidgetResizeEnd (keyboard) failed:', err));

        // Send position changes for displaced widgets
        const changes = [];
        for (const r of resolved) {
          if (r.id === widgetId) continue;
          const orig = allPositions.find(p => p.id === r.id);
          if (!orig) continue;
          if (r.col !== orig.col || r.row !== orig.row) {
            changes.push({ id: r.id, col: r.col, row: r.row });
          }
        }
        if (changes.length > 0) {
          state.dotNetRef.invokeMethodAsync('JsOnLayoutResolved', widgetId, changes)
            .catch(err => console.error('JsOnLayoutResolved (keyboard resize) failed:', err));
        }
        announceChange(state, `Widget resized to ${newColSpan} columns, ${newRowSpan} rows`);
      }
    }
    return;
  }

  // Handle widget keyboard navigation (arrow keys to move)
  const widgetTarget = target.closest('[data-widget-id]');
  if (widgetTarget && state.options.allowDrag && widgetTarget.getAttribute('data-static') !== 'true') {
    const widgetId = widgetTarget.getAttribute('data-widget-id');
    const widget = widgetTarget;

    const grid = widget.closest('[role="region"][aria-roledescription="dashboard grid"]');
    if (!grid) return;

    const style = widget.style;
    const col = parseGridValue(style.gridColumn, 'start');
    const row = parseGridValue(style.gridRow, 'start');
    const colSpan = parseGridValue(style.gridColumn, 'span');
    const rowSpan = parseGridValue(style.gridRow, 'span');
    const cols = getActiveColumns(state);

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
        const allPositions = getWidgetPositions(grid, null);

        let resolved = resolveLayoutAfterDrag(allPositions, widgetId, newCol, newRow, col, row, cols);
        if (resolved && state.options.compact) {
          compact(resolved, widgetId, cols);
        }

        if (!resolved) return;
        applyLayout(grid, resolved);

        const changes = [];
        for (const r of resolved) {
          const orig = allPositions.find(p => p.id === r.id);
          if (!orig) continue;
          if (r.col !== orig.col || r.row !== orig.row) {
            changes.push({ id: r.id, col: r.col, row: r.row });
          }
        }
        if (changes.length > 0) {
          state.dotNetRef.invokeMethodAsync('JsOnLayoutResolved', widgetId, changes)
            .catch(err => console.error('JsOnLayoutResolved (keyboard) failed:', err));
        }
        announceChange(state, `Widget moved to column ${newCol}, row ${newRow}`);
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

// --- Focus Out (compact on blur) ---

function onWidgetFocusOut(instanceId, state, e) {
  if (!state.options.compact || !state.options.editable) return;
  if (state.isDragging || state.isResizing) return;

  // Only act when a widget loses focus
  const widget = e.target.closest('[data-widget-id]');
  if (!widget) return;

  // If focus is moving to another element inside the same widget, ignore
  if (e.relatedTarget && widget.contains(e.relatedTarget)) return;

  const grid = state.gridEl;
  if (!grid) return;

  const positions = getWidgetPositions(grid, null);
  if (positions.length === 0) return;

  // Take a snapshot before compacting to detect changes
  const before = positions.map(p => ({ ...p }));
  compact(positions, null, getActiveColumns(state));

  // Check if anything actually moved
  const changes = [];
  for (const pos of positions) {
    const orig = before.find(p => p.id === pos.id);
    if (!orig) continue;
    if (pos.col !== orig.col || pos.row !== orig.row) {
      changes.push({ id: pos.id, col: pos.col, row: pos.row });
    }
  }

  if (changes.length > 0) {
    applyLayout(grid, positions);
    state.dotNetRef.invokeMethodAsync('JsOnLayoutResolved', '', changes)
      .catch(err => console.error('JsOnLayoutResolved (compact on blur) failed:', err));
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
    const isStatic = w.getAttribute('data-static') === 'true';
    positions.push({ id, col, row, colSpan, rowSpan, isStatic });
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

/**
 * Full compaction: slide widgets left then up to close all gaps.
 * The fixedId widget (being dragged/resized) stays in place.
 */
function compact(positions, fixedId, columns) {
  const sorted = [...positions].sort((a, b) => a.row !== b.row ? a.row - b.row : a.col - b.col);
  const placed = [];

  // Pre-place fixed widget
  const fixed = sorted.find(p => p.id === fixedId);
  if (fixed) placed.push(fixed);

  // Pass 1: slide left on current row, overflow to next row if needed
  for (const widget of sorted) {
    if (widget.id === fixedId) continue;

    let found = false;
    for (let col = 1; col <= columns - widget.colSpan + 1; col++) {
      if (!wouldOverlap(col, widget.row, widget.colSpan, widget.rowSpan, placed)) {
        widget.col = col;
        found = true;
        break;
      }
    }
    // Search downward unbounded until a free slot is found
    for (let row = widget.row + 1; !found; row++) {
      for (let col = 1; col <= columns - widget.colSpan + 1; col++) {
        if (!wouldOverlap(col, row, widget.colSpan, widget.rowSpan, placed)) {
          widget.row = row;
          widget.col = col;
          found = true;
          break;
        }
      }
    }
    placed.push(widget);
  }

  // Pass 2: slide up without changing columns
  placed.length = 0;
  const sorted2 = [...positions].sort((a, b) => a.row !== b.row ? a.row - b.row : a.col - b.col);
  if (fixed) placed.push(fixed);

  for (const widget of sorted2) {
    if (widget.id === fixedId) continue;
    for (let row = 1; row <= widget.row; row++) {
      if (!wouldOverlap(widget.col, row, widget.colSpan, widget.rowSpan, placed)) {
        widget.row = row;
        break;
      }
    }
    placed.push(widget);
  }
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

/**
 * Checks if two rectangles overlap on the grid.
 */
function rectsOverlap(a, b) {
  return a.col < b.col + b.colSpan &&
    b.col < a.col + a.colSpan &&
    a.row < b.row + b.rowSpan &&
    b.row < a.row + a.rowSpan;
}

/**
 * Find all widgets that overlap with the given widget.
 */
function findCollisions(widget, allWidgets) {
  const collisions = [];
  for (const other of allWidgets) {
    if (other.id === widget.id) continue;
    if (rectsOverlap(widget, other)) {
      collisions.push(other);
    }
  }
  return collisions;
}

/**
 * Push a single widget out of the way of `pusher` along the given axis/direction.
 * Cascades recursively if the pushed widget now overlaps others.
 * Returns false if the push is impossible (canvas edge, static widget, cycle).
 */
function pushWidget(widget, pusher, axis, direction, allWidgets, columns, visited) {
  if (widget.isStatic) return false;
  if (visited.has(widget.id)) return false;
  visited.add(widget.id);

  // Save original position for rollback
  const origCol = widget.col;
  const origRow = widget.row;

  // Move widget to just outside pusher
  if (axis === 'col') {
    widget.col = direction > 0
      ? pusher.col + pusher.colSpan  // push right
      : pusher.col - widget.colSpan; // push left
  } else {
    widget.row = direction > 0
      ? pusher.row + pusher.rowSpan  // push down
      : pusher.row - widget.rowSpan; // push up
  }

  // Check grid bounds (horizontal only — vertical grows downward)
  if (widget.col < 1 || widget.col + widget.colSpan - 1 > columns || widget.row < 1) {
    widget.col = origCol;
    widget.row = origRow;
    visited.delete(widget.id);
    return false;
  }

  // Find new collisions caused by this push
  const newCollisions = findCollisions(widget, allWidgets);
  for (const collider of newCollisions) {
    if (collider.id === pusher.id) continue;
    if (!pushWidget(collider, widget, axis, direction, allWidgets, columns, visited)) {
      // Cascade failed — rollback
      widget.col = origCol;
      widget.row = origRow;
      visited.delete(widget.id);
      return false;
    }
  }

  return true;
}

/**
 * Resolve layout after dragging a widget to a new position.
 * Uses collision detection + local displacement instead of global compaction.
 * Returns the resolved positions array, or null if the move should be rejected.
 */
function resolveLayoutAfterDrag(originalPositions, draggedId, targetCol, targetRow, origCol, origRow, columns) {
  const positions = originalPositions.map(p => ({...p}));
  const dragged = positions.find(p => p.id === draggedId);
  if (!dragged) return null;

  // Place dragged widget at target (clamped)
  dragged.col = Math.max(1, Math.min(targetCol, columns - dragged.colSpan + 1));
  dragged.row = Math.max(1, targetRow);

  // Find overlapping widgets
  const collisions = findCollisions(dragged, positions);

  if (collisions.length === 0) {
    return positions;
  }

  // Try swap if exactly 1 collision
  if (collisions.length === 1) {
    const collider = collisions[0];
    if (!collider.isStatic) {
      // Try placing collider at dragged widget's original position
      const swapPositions = originalPositions.map(p => ({...p}));
      const swapDragged = swapPositions.find(p => p.id === draggedId);
      const swapCollider = swapPositions.find(p => p.id === collider.id);

      swapDragged.col = dragged.col;
      swapDragged.row = dragged.row;
      swapCollider.col = origCol;
      swapCollider.row = origRow;

      // Check if collider fits at original position without overlapping anyone else
      const swapConflicts = findCollisions(swapCollider, swapPositions);
      if (swapConflicts.length === 0) {
        return swapPositions;
      }
    }
  }

  // Push colliders along the drag direction
  const dragDirCol = targetCol - origCol;
  const dragDirRow = targetRow - origRow;

  // Determine primary push axis and direction from drag movement
  let pushAxis, pushDirection;
  if (Math.abs(dragDirCol) >= Math.abs(dragDirRow)) {
    pushAxis = 'col';
    pushDirection = dragDirCol >= 0 ? 1 : -1;
  } else {
    pushAxis = 'row';
    pushDirection = dragDirRow >= 0 ? 1 : -1;
  }

  for (const collider of collisions) {
    const visited = new Set([draggedId]);
    if (!pushWidget(collider, dragged, pushAxis, pushDirection, positions, columns, visited)) {
      // Primary direction failed — try secondary axis
      const altAxis = pushAxis === 'col' ? 'row' : 'col';
      const altDirection = pushAxis === 'col' ? (dragDirRow >= 0 ? 1 : -1) : (dragDirCol >= 0 ? 1 : -1);
      const visited2 = new Set([draggedId]);

      // Re-derive positions from original since previous push may have partially mutated
      const retryPositions = originalPositions.map(p => ({...p}));
      const retryDragged = retryPositions.find(p => p.id === draggedId);
      retryDragged.col = dragged.col;
      retryDragged.row = dragged.row;

      const retryCollisions = findCollisions(retryDragged, retryPositions);
      let allPushed = true;
      for (const rc of retryCollisions) {
        if (!pushWidget(rc, retryDragged, altAxis, altDirection, retryPositions, columns, visited2)) {
          // Try pushing down as last resort
          const visited3 = new Set([draggedId]);
          if (!pushWidget(rc, retryDragged, 'row', 1, retryPositions, columns, visited3)) {
            allPushed = false;
            break;
          }
        }
      }

      if (allPushed) {
        return retryPositions;
      }

      return null;
    }
  }

  return positions;
}

/**
 * Resolve layout after resizing a widget.
 * Pushes colliders along the direction of the growing edge.
 * Returns the resolved positions array, or null if the resize should be rejected.
 */
function resolveLayoutAfterResize(originalPositions, resizedId, newCol, newRow, newColSpan, newRowSpan, startCol, startRow, startColSpan, startRowSpan, columns) {
  const positions = originalPositions.map(p => ({...p}));
  const resized = positions.find(p => p.id === resizedId);
  if (!resized) return null;

  resized.col = newCol;
  resized.row = newRow;
  resized.colSpan = newColSpan;
  resized.rowSpan = newRowSpan;

  const collisions = findCollisions(resized, positions);

  if (collisions.length === 0) {
    return positions;
  }

  // Determine which edges grew
  const grewRight = (newCol + newColSpan) > (startCol + startColSpan);
  const grewLeft = newCol < startCol;
  const grewDown = (newRow + newRowSpan) > (startRow + startRowSpan);
  const grewUp = newRow < startRow;

  for (const collider of collisions) {
    let pushed = false;
    const visited = new Set([resizedId]);

    // Try pushing along the growing edge direction
    if (grewRight && !pushed) {
      pushed = pushWidget(collider, resized, 'col', 1, positions, columns, visited);
    }
    if (grewLeft && !pushed) {
      visited.clear(); visited.add(resizedId);
      pushed = pushWidget(collider, resized, 'col', -1, positions, columns, visited);
    }
    if (grewDown && !pushed) {
      visited.clear(); visited.add(resizedId);
      pushed = pushWidget(collider, resized, 'row', 1, positions, columns, visited);
    }
    if (grewUp && !pushed) {
      visited.clear(); visited.add(resizedId);
      pushed = pushWidget(collider, resized, 'row', -1, positions, columns, visited);
    }

    // Last resort: push down
    if (!pushed) {
      visited.clear(); visited.add(resizedId);
      pushed = pushWidget(collider, resized, 'row', 1, positions, columns, visited);
    }

    if (!pushed) {
      return null;
    }
  }

  return positions;
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
  if (state.resizeGrid && state.originalResizePositions) {
    applyLayout(state.resizeGrid, state.originalResizePositions);
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
  if (state.pointerId != null && state.gridEl) {
    try { state.gridEl.releasePointerCapture(state.pointerId); } catch {}
  }
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
  state.resizeGrid = null;
  state.originalResizePositions = null;
  state.resolvedResizeLayout = null;
  state.lastResizeCol = undefined;
  state.lastResizeRow = undefined;
  state.lastResizeColSpan = undefined;
  state.lastResizeRowSpan = undefined;
}

function announceChange(state, message) {
  // Find the live region scoped to this grid instance
  const grid = state.gridEl;
  if (!grid) return;
  const id = grid.getAttribute('data-dashboard-id');
  const region = id ? document.getElementById(`${id}-live`) : null;
  if (region) {
    region.textContent = message;
    setTimeout(() => { region.textContent = ''; }, 1000);
  }
}

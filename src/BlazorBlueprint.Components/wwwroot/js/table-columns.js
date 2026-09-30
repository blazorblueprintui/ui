// Column resize and reorder for any table whose columns carry data-column-id.
// Shared by BbDataGrid and BbGantt: both keep their widths in C# and both want the drag itself
// to happen without a Blazor round-trip, which is the whole of what this file is for.
// Resize: pure JS pointer-capture drag on resize handles.
// Reorder: HTML5 Drag and Drop API with event delegation on the table.

// ─── Shared state ───────────────────────────────────────────────────────────

const gridStates = new Map();

function getOrCreateState(gridId) {
  if (!gridStates.has(gridId)) {
    gridStates.set(gridId, {
      containerElement: null,
      dotNetRef: null,
      // Resize state
      resizeEnabled: false,
      isDragging: false,
      minWidth: 50,
      // Reorder state
      reorderEnabled: false,
      reorderDelegationSetup: false,
      reorderableIds: new Set(),
      dragColumnId: null,
      dragTh: null,
      dropIndicator: null
    });
  }
  return gridStates.get(gridId);
}

// ─── Column Resize ──────────────────────────────────────────────────────────

/**
 * Initialize column resize for a DataGrid.
 * @param {HTMLElement} containerElement - The grid root container
 * @param {DotNetObject} dotNetRef - Blazor component reference
 * @param {string} gridId - Unique grid identifier
 * @param {number} minWidth - Minimum column width in pixels
 */
export function initColumnResize(containerElement, dotNetRef, gridId, minWidth) {
  if (!containerElement || !dotNetRef) return;

  const state = getOrCreateState(gridId);
  state.containerElement = containerElement;
  state.dotNetRef = dotNetRef;
  state.resizeEnabled = true;
  state.minWidth = minWidth || 50;
}

/**
 * Setup resize handles for resizable columns.
 * Finds elements with [data-resize-handle] and attaches pointer event listeners.
 * Resize is handled entirely in JS for instant feedback — no Blazor round-trip.
 * @param {string} gridId - Grid identifier
 */
export function setupResizeHandles(gridId) {
  const state = gridStates.get(gridId);
  if (!state || !state.containerElement) return;

  const table = state.containerElement.querySelector('table');
  if (!table) return;

  const handles = table.querySelectorAll('[data-resize-handle]');
  for (const handle of handles) {
    if (handle._resizeSetup) continue;
    handle._resizeSetup = true;

    const columnId = handle.getAttribute('data-resize-handle');

    // Prevent click from reaching the th (which triggers sort)
    handle.addEventListener('click', (e) => {
      e.stopPropagation();
    });

    handle.addEventListener('pointerdown', (e) => {
      e.stopPropagation();
      e.preventDefault();

      if (state.isDragging) return;

      // Snapshot the managed column widths and freeze them on their <col> elements.
      // Matched by data-column-id rather than by position: a Gantt's colgroup also holds one
      // <col> per timeline slot, and those are not columns anyone can drag.
      // A column that measures zero is left alone: every column is measured, so a control column
      // with nothing to measure would otherwise be frozen at zero and stay that way.
      const ths = Array.from(table.querySelectorAll('thead th[data-column-id]'));
      const colFor = id => table.querySelector(`colgroup col[data-column-id="${CSS.escape(id)}"]`);

      const settled = new Map();

      ths.forEach(th => {
        const id = th.getAttribute('data-column-id');
        const col = id ? colFor(id) : null;
        const width = Math.round(th.getBoundingClientRect().width);
        if (col && width > 0) {
          col.style.width = width + 'px';
          settled.set(id, width);
        }
      });

      // Lock the table width to what it currently measures, including any columns this file does
      // not manage. Without the lock, table-fixed + width:100% scales every other column to make
      // room for the one being dragged.
      let totalWidth = Math.round(table.getBoundingClientRect().width);
      table.style.width = totalWidth + 'px';

      const activeTh = ths.find(th => th.getAttribute('data-column-id') === columnId);
      const startWidth = activeTh ? activeTh.getBoundingClientRect().width : 150;
      const activeCol = colFor(columnId);
      const startX = e.clientX;

      state.isDragging = true;

      document.body.style.userSelect = 'none';
      document.body.style.cursor = 'col-resize';

      const onMove = (moveEvt) => {
        if (moveEvt.pointerId !== e.pointerId) return;
        moveEvt.preventDefault();
        const delta = moveEvt.clientX - startX;
        const newWidth = Math.max(state.minWidth, Math.round(startWidth + delta));
        if (activeCol) {
          activeCol.style.width = newWidth + 'px';
          settled.set(columnId, newWidth);
          // Update table width to match the new total
          table.style.width = (totalWidth - startWidth + newWidth) + 'px';
        }
      };

      const onEnd = (endEvt) => {
        if (endEvt.pointerId !== e.pointerId) return;

        document.body.style.userSelect = '';
        document.body.style.cursor = '';
        document.removeEventListener('pointermove', onMove);
        document.removeEventListener('pointerup', onEnd);
        document.removeEventListener('pointercancel', onEnd);

        try { handle.releasePointerCapture(endEvt.pointerId); } catch {}

        state.isDragging = false;

        // Commit the widths the drag settled on to Blazor. A column the drag measured nothing for is
        // not in here at all: reporting the zero it measures is what put "0px" into the column
        // state, where it outranked the width the column declares.
        const widths = Object.fromEntries(settled);

        state.dotNetRef.invokeMethodAsync('OnResizeCompleted', columnId, widths)
          .catch(() => { /* component may be disposed */ });
      };

      document.addEventListener('pointermove', onMove);
      document.addEventListener('pointerup', onEnd);
      document.addEventListener('pointercancel', onEnd);

      try { handle.setPointerCapture(e.pointerId); } catch {}
    });
  }
}

// ─── Column Reorder ─────────────────────────────────────────────────────────

/**
 * Initialize column reorder for a DataGrid.
 * @param {HTMLElement} containerElement - The grid root container
 * @param {DotNetObject} dotNetRef - Blazor component reference
 * @param {string} gridId - Unique grid identifier
 */
export function initColumnReorder(containerElement, dotNetRef, gridId) {
  if (!containerElement || !dotNetRef) return;

  const state = getOrCreateState(gridId);
  state.containerElement = containerElement;
  state.dotNetRef = dotNetRef;
  state.reorderEnabled = true;

  // Create drop indicator element
  if (!state.dropIndicator) {
    const indicator = document.createElement('div');
    indicator.style.cssText =
      'position:absolute;width:2px;background:hsl(var(--primary));' +
      'top:0;bottom:0;pointer-events:none;z-index:50;display:none;';
    containerElement.style.position = 'relative';
    containerElement.appendChild(indicator);
    state.dropIndicator = indicator;
  }
}

/**
 * Setup drag handlers for reorderable header cells using event delegation.
 * Uses a single set of listeners on the <table> element rather than per-cell
 * listeners, so it works correctly even when Blazor patches/replaces th elements.
 * The draggable="true" attribute must be set by Blazor on the th elements.
 * @param {string} gridId - Grid identifier
 * @param {string[]} reorderableColumnIds - Column IDs that can be reordered
 */
export function setupDraggableHeaders(gridId, reorderableColumnIds) {
  const state = gridStates.get(gridId);
  if (!state || !state.containerElement) return;

  // Always update the set of reorderable column IDs
  state.reorderableIds = new Set(reorderableColumnIds);

  // Set up event delegation once on the table
  if (state.reorderDelegationSetup) return;

  const table = state.containerElement.querySelector('table');
  if (!table) return;

  state.reorderDelegationSetup = true;

  table.addEventListener('dragstart', (e) => {
    const th = e.target.closest('th[data-column-id]');
    if (!th) return;

    const columnId = th.getAttribute('data-column-id');
    if (!state.reorderableIds.has(columnId)) return;

    // Don't start drag if a resize is in progress
    if (state.isDragging) {
      e.preventDefault();
      return;
    }

    state.dragColumnId = columnId;
    state.dragTh = th;

    if (e.dataTransfer) {
      e.dataTransfer.effectAllowed = 'move';
      e.dataTransfer.setData('text/plain', columnId);

      // Create ghost drag image — positioned at the header cell location
      // so the browser can capture it correctly
      const rect = th.getBoundingClientRect();
      const ghost = document.createElement('div');
      ghost.textContent = th.textContent.trim();
      ghost.style.cssText =
        `position:fixed;` +
        `left:${rect.left}px;top:${rect.top}px;` +
        `width:${rect.width}px;height:${rect.height}px;` +
        `display:flex;align-items:center;padding:0 16px;` +
        `background:hsl(var(--background));` +
        `border:1px solid hsl(var(--border));border-radius:6px;` +
        `box-shadow:0 4px 12px rgba(0,0,0,0.15);` +
        `font-size:14px;font-weight:500;color:hsl(var(--foreground));` +
        `opacity:0.9;pointer-events:none;z-index:9999;`;
      document.body.appendChild(ghost);

      // Offset so the ghost aligns with where the user clicked
      const offsetX = e.clientX - rect.left;
      const offsetY = e.clientY - rect.top;
      e.dataTransfer.setDragImage(ghost, offsetX, offsetY);

      // Clean up ghost element after browser has captured it
      requestAnimationFrame(() => {
        requestAnimationFrame(() => {
          if (ghost.parentNode) {
            ghost.parentNode.removeChild(ghost);
          }
        });
      });
    }

    th.style.opacity = '0.4';
  });

  table.addEventListener('dragend', () => {
    if (state.dragTh) {
      state.dragTh.style.opacity = '';
    }
    state.dragColumnId = null;
    state.dragTh = null;
    if (state.dropIndicator) {
      state.dropIndicator.style.display = 'none';
    }
  });

  table.addEventListener('dragover', (e) => {
    if (!state.dragColumnId) return;

    const th = e.target.closest('th[data-column-id]');
    if (!th) return;

    const columnId = th.getAttribute('data-column-id');
    if (state.dragColumnId === columnId) return;

    // Do not show drop indicator over pinned columns
    if (th.getAttribute('data-pinned') === 'true') return;

    e.preventDefault();
    if (e.dataTransfer) {
      e.dataTransfer.dropEffect = 'move';
    }

    // Position drop indicator
    const rect = th.getBoundingClientRect();
    const containerRect = state.containerElement.getBoundingClientRect();
    const midX = rect.left + rect.width / 2;
    const indicatorX = e.clientX < midX
      ? rect.left - containerRect.left
      : rect.right - containerRect.left;

    if (state.dropIndicator) {
      state.dropIndicator.style.display = 'block';
      state.dropIndicator.style.left = indicatorX + 'px';
      state.dropIndicator.style.top = (rect.top - containerRect.top) + 'px';
      state.dropIndicator.style.height = rect.height + 'px';
    }
  });

  table.addEventListener('dragleave', (e) => {
    // Only hide indicator when leaving the table entirely
    if (!e.relatedTarget || !table.contains(e.relatedTarget)) {
      if (state.dropIndicator) {
        state.dropIndicator.style.display = 'none';
      }
    }
  });

  table.addEventListener('drop', (e) => {
    if (!state.dragColumnId) return;

    e.preventDefault();
    if (state.dropIndicator) {
      state.dropIndicator.style.display = 'none';
    }

    const th = e.target.closest('th[data-column-id]');
    if (!th) return;

    // Do not allow dropping onto a pinned column
    if (th.getAttribute('data-pinned') === 'true') return;

    const targetColumnId = th.getAttribute('data-column-id');
    if (!targetColumnId || targetColumnId === state.dragColumnId) return;

    // Report the drop as a *gesture* — "put the dragged column before / after
    // this column" — and let Blazor resolve it to a position in the column
    // state. Deliberately do NOT send a header-cell index: the header row is
    // not a 1:1 view of the column order (hidden columns are absent from the
    // DOM, pinned columns are re-partitioned to the edges of the row), and the
    // dragged column is still in the DOM here while the .NET side removes it
    // before re-inserting. Sending a raw index is what made rightward drags
    // overshoot by one. See BbDataGrid.OnColumnReordered for the resolution.
    const rect = th.getBoundingClientRect();
    const midX = rect.left + rect.width / 2;
    const placeAfter = e.clientX >= midX;

    state.dotNetRef.invokeMethodAsync('OnColumnReordered',
      state.dragColumnId, targetColumnId, placeAfter).catch(() => { });

    // Don't null dragColumnId/dragTh here — dragend always fires after drop
    // and handles cleanup + opacity reset.
  });
}

// ─── Disposal ───────────────────────────────────────────────────────────────

/**
 * Dispose all column management state for a grid.
 * @param {string} gridId - Grid identifier
 */
export function dispose(gridId) {
  const state = gridStates.get(gridId);
  if (!state) return;

  // Cleanup reorder indicator
  if (state.dropIndicator && state.dropIndicator.parentNode) {
    state.dropIndicator.parentNode.removeChild(state.dropIndicator);
  }

  gridStates.delete(gridId);
}

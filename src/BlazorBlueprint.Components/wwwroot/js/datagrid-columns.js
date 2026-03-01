// DataGrid column resize and reorder handler
// Resize: pure JS pointer-capture drag on resize handles (no Blazor round-trip)
// Reorder: HTML5 Drag and Drop API with event delegation on the table

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

      // Snapshot all column widths and freeze them on <col> elements
      const ths = Array.from(table.querySelectorAll('thead th[data-column-id]'));
      const cols = table.querySelectorAll('colgroup col');

      let totalWidth = 0;
      ths.forEach((th, i) => {
        const w = Math.round(th.getBoundingClientRect().width);
        if (cols[i]) {
          cols[i].style.width = w + 'px';
        }
        totalWidth += w;
      });

      // Lock the table width to the sum of column widths.
      // This prevents table-fixed + width:100% from proportionally
      // scaling other columns when one is resized.
      table.style.width = totalWidth + 'px';

      // Find the active column index
      const thIndex = ths.findIndex(th => th.getAttribute('data-column-id') === columnId);
      const startWidth = thIndex >= 0 && ths[thIndex]
        ? ths[thIndex].getBoundingClientRect().width
        : 150;
      const activeCol = thIndex >= 0 ? cols[thIndex] : null;
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

        // Commit all column widths to Blazor
        const widths = {};
        ths.forEach((th, i) => {
          const id = th.getAttribute('data-column-id');
          if (id && cols[i]) {
            widths[id] = parseFloat(cols[i].style.width) || th.getBoundingClientRect().width;
          }
        });

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

    // Determine target index from visible header cells
    const headerCells = Array.from(
      table.querySelectorAll('thead th[data-column-id]')
    );
    const targetIndex = headerCells.indexOf(th);

    // Adjust based on drop position (before or after the target)
    const rect = th.getBoundingClientRect();
    const midX = rect.left + rect.width / 2;
    const adjustedIndex = e.clientX < midX ? targetIndex : targetIndex + 1;

    state.dotNetRef.invokeMethodAsync('OnColumnReordered',
      state.dragColumnId, adjustedIndex).catch(() => { });

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

/** @type {Promise|null} */
let sortableLoadPromise = null;

/** @type {any} */
let sortableLib = null;

/** @type {Map<string, any>} */
const instances = new Map();

/**
 * Lazily load the Sortable ESM library.
 * Uses a single-flight pattern to prevent duplicate loads.
 * @returns {Promise<any>}
 */
async function loadSortable() {
  if (sortableLib) return sortableLib;

  // Check for globally loaded Sortable first (e.g., via <script> tag)
  if (window.Sortable) {
    sortableLib = window.Sortable;
    return sortableLib;
  }

  if (!sortableLoadPromise) {
    sortableLoadPromise = (async () => {
      // Resolve relative to this module's own URL
      const libPath = new URL('../../lib/sortable/Sortable.min.js', import.meta.url).href;
      const mod = await import(libPath);
      return mod;
    })();
  }

  sortableLib = await sortableLoadPromise;
  return sortableLib;
}

/**
 * Create a static visual clone of a container, positioned exactly over it,
 * so the real container can be reverted invisibly underneath.
 * @param {HTMLElement} container
 * @returns {HTMLElement} The overlay element — call .remove() to clean up.
 */
function freezeSnapshot(container) {
  const rect = container.getBoundingClientRect();
  const clone = container.cloneNode(true);
  clone.removeAttribute('id');
  clone.style.cssText = `
    position: fixed;
    top: ${rect.top}px;
    left: ${rect.left}px;
    width: ${rect.width}px;
    height: ${rect.height}px;
    z-index: 10000;
    pointer-events: none;
    margin: 0;
  `;
  document.body.appendChild(clone);
  container.style.visibility = 'hidden';
  return {
    remove() {
      container.style.visibility = '';
      clone.remove();
    }
  };
}

/**
 * Initialize a Sortable instance on a DOM element.
 * @param {string} id - Element ID
 * @param {string} group - Group name
 * @param {boolean|string} pull - Pull settings
 * @param {boolean|array} put - Put settings
 * @param {boolean} sort - Enable sorting
 * @param {string} handle - Handle selector
 * @param {string} filter - Filter selector
 * @param {object} component - .NET component reference
 * @param {boolean} forceFallback - Force fallback mode
 */
export async function init(id, group, pull, put, sort, handle, filter, component, forceFallback) {
  // Destroy existing instance if re-initializing
  destroy(id);

  const el = document.getElementById(id);
  if (!el) return;

  const mod = await loadSortable();
  const Sortable = mod.default || mod.Sortable || mod;
  // Debounce swap detection to prevent grid oscillation — when SortableJS
  // swaps two adjacent grid items, the cursor can end up over the swapped
  // item and trigger an immediate reverse swap, causing a visual "dance".
  let lastMoveTime = 0;

  const sortable = new Sortable(el, {
    animation: 150,
    group: {
      name: group,
      pull: pull ?? true,
      put: put
    },
    filter: filter || undefined,
    sort: sort,
    forceFallback: forceFallback,
    handle: handle || undefined,
    onMove: () => {
      const now = Date.now();
      if (now - lastMoveTime < 200) {
        return false;
      }
      lastMoveTime = now;
      return true;
    },
    onUpdate: (event) => {
      // Blazor tracks DOM nodes by reference, so we must revert SortableJS's
      // DOM mutation before Blazor re-renders. To prevent a visible flash,
      // we place a static clone (snapshot) over the container — the user sees
      // the correct post-drop state while the real container reverts and
      // Blazor re-renders underneath.
      const snapshot = freezeSnapshot(event.to);

      event.item.remove();
      event.to.insertBefore(event.item, event.to.children[event.oldIndex]);

      component.invokeMethodAsync('OnUpdateJS', event.oldDraggableIndex, event.newDraggableIndex)
        .then(() => { snapshot.remove(); });
    },
    onRemove: (event) => {
      if (event.pullMode === 'clone') {
        event.clone.remove();
      }

      const snapshot = freezeSnapshot(event.from);

      event.item.remove();
      event.from.insertBefore(event.item, event.from.childNodes[event.oldIndex]);

      component.invokeMethodAsync('OnRemoveJS', event.oldDraggableIndex, event.newDraggableIndex)
        .then(() => { snapshot.remove(); });
    },
    onAdd: (event) => {
      const snapshot = freezeSnapshot(event.to);

      event.item.remove();

      component.invokeMethodAsync('OnAddJS', event.oldDraggableIndex, event.newDraggableIndex)
        .then(() => { snapshot.remove(); });
    }
  });

  instances.set(id, sortable);
}

/**
 * Destroy a Sortable instance and clean up event listeners.
 * @param {string} id - Element ID
 */
export function destroy(id) {
  const sortable = instances.get(id);
  if (sortable) {
    sortable.destroy();
    instances.delete(id);
  }
}

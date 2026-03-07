/** @type {Promise|null} */
let sortableLoadPromise = null;

/** @type {any} */
let sortableLib = null;

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
      const libPath = new URL('../lib/sortable/Sortable.min.js', import.meta.url).href;
      const mod = await import(libPath);
      return mod;
    })();
  }

  sortableLib = await sortableLoadPromise;
  return sortableLib;
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
  const mod = await loadSortable();
  const Sortable = mod.default || mod.Sortable || mod;
  var sortable = new Sortable(document.getElementById(id), {
    animation: 200,
    group: {
      name: group,
      pull: pull || true,
      put: put
    },
    filter: filter || undefined,
    sort: sort,
    forceFallback: forceFallback,
    handle: handle || undefined,
    onUpdate: (event) => {
      // Revert the DOM to match the .NET state
      event.item.remove();
      event.to.insertBefore(event.item, event.to.children[event.oldIndex]);

      // Notify .NET to update its model and re-render
      component.invokeMethodAsync('OnUpdateJS', event.oldDraggableIndex, event.newDraggableIndex);
    },
    onRemove: (event) => {
      if (event.pullMode === 'clone') {
        // Remove the clone
        event.clone.remove();
      }

      event.item.remove();
      event.from.insertBefore(event.item, event.from.childNodes[event.oldIndex]);

      // Notify .NET to update its model and re-render
      component.invokeMethodAsync('OnRemoveJS', event.oldDraggableIndex, event.newDraggableIndex);
    },
    onAdd: (event) => {
      // The source list's onRemove handler already reverted the DOM.
      // Notify the target list so its .NET model can be updated.
      component.invokeMethodAsync('OnAddJS', event.oldDraggableIndex, event.newDraggableIndex);
    }
  });
}

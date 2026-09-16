import { attachKeyboardSorting } from '../sortable-keyboard.js';
import { performDrop } from '../sortable-transfer.js';
import { createDragOverlay } from '../sortable-overlay.js';

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
      const libPath = new URL('../../lib/sortable/sortable.min.js', import.meta.url).href;
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

  const resolvedHandle = handle || (el.querySelector('[data-bb-sortable-handle]') ? '[data-bb-sortable-handle]' : undefined);
  let cleanupOverlay = null;
  const sortable = new Sortable(el, {
    animation: 150,
    group: {
      name: group,
      pull: pull === 'false' ? false : pull === 'true' ? true : pull ?? true,
      put: put
    },
    filter: [filter, '[data-bb-sortable-handle]:disabled'].filter(Boolean).join(','),
    sort: sort,
    forceFallback: forceFallback,
    onStart: event => {
      cleanupOverlay?.();
      cleanupOverlay = createDragOverlay(event.item, event.originalEvent);
      if (cleanupOverlay && Sortable.ghost) Sortable.ghost.style.setProperty('opacity', '0', 'important');
    },
    onEnd: () => { cleanupOverlay?.(); cleanupOverlay = null; },
    handle: resolvedHandle,
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
        .finally(() => { snapshot.remove(); }).catch(() => {});
    },
    onRemove: (event) => {
      if (event.pullMode === 'clone') {
        event.clone.remove();
      }

      event.item.remove();
      event.from.insertBefore(event.item, event.from.children[event.oldIndex]);
      // A Bb target coordinates validation and callbacks after both DOM trees are restored.
      if (!instances.has(event.to.id)) {
        component.invokeMethodAsync('OnRemoveJS', event.oldDraggableIndex, event.newDraggableIndex).catch(() => {});
      }
    },
    onAdd: (event) => {
      const snapshots = [freezeSnapshot(event.to), freezeSnapshot(event.from)];
      event.item.remove();
      const source = instances.get(event.from.id)?.component;
      // Sortable dispatches onRemove synchronously after onAdd. The first await keeps model
      // callbacks behind that DOM restoration, including on WebAssembly.
      performDrop(source, component, event.oldDraggableIndex, event.newDraggableIndex, id, event.pullMode === 'clone')
        .finally(() => { snapshots.forEach(snapshot => snapshot.remove()); })
        .catch(error => console.error('sortable: cross-list drop failed', error));
    }
  });

  const transferByKeyboard = async (oldIndex, direction) => {
    if (pull === 'false') return false;
    const connected = [...instances.values()].filter(instance => instance.group === group && instance.el.isConnected && instance.el.getClientRects().length)
      .sort((a, b) => a.el.compareDocumentPosition(b.el) & 4 ? -1 : 1);
    const current = connected.findIndex(instance => instance.el === el);
    let target;
    for (let i = current + direction; i >= 0 && i < connected.length; i += direction) {
      if (connected[i].put && connected[i].el.dataset.keyboardSorting === 'true') { target = connected[i]; break; }
    }
    if (!target) return false;
    const newIndex = [...target.el.children].filter(child => child.hasAttribute('data-bb-sortable-item')).length;
    const allowed = await performDrop(component, target.component, oldIndex, newIndex, target.el.id, pull === 'clone');
    if (allowed) {
      await new Promise(resolve => requestAnimationFrame(resolve));
      const added = [...target.el.children].filter(child => child.hasAttribute('data-bb-sortable-item'))[newIndex];
      const focus = added?.querySelector(target.handle || '[data-bb-sortable-handle]') || added;
      focus?.focus();
    }
    return allowed;
  };
  const cleanupKeyboard = attachKeyboardSorting(el, handle, filter,
    (oldIndex, newIndex) => component.invokeMethodAsync('OnUpdateJS', oldIndex, newIndex), transferByKeyboard);
  const handleObserver = new MutationObserver(() => {
    if (!handle) sortable.option('handle', el.querySelector('[data-bb-sortable-handle]') ? '[data-bb-sortable-handle]' : null);
  });
  handleObserver.observe(el, { childList: true });
  instances.set(id, { sortable, cleanupKeyboard, component, el, group, put, handle, cleanup: () => { cleanupOverlay?.(); handleObserver.disconnect(); } });
}

/**
 * Destroy a Sortable instance and clean up event listeners.
 * @param {string} id - Element ID
 */
export function destroy(id) {
  const instance = instances.get(id);
  if (instance) {
    instance.cleanupKeyboard();
    instance.cleanup();
    instance.sortable.destroy();
    instances.delete(id);
  }
}

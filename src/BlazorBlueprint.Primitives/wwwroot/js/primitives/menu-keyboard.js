/**
 * Menu keyboard navigation module.
 * Handles all keyboard events for menu containers (DropdownMenu, ContextMenu, Menubar)
 * entirely in JavaScript. Only calls C# for Escape (close) and horizontal menu switching.
 *
 * Modes:
 *   "vertical" — DropdownMenu, ContextMenu (ArrowDown/ArrowUp/Home/End/Enter/Space/Escape)
 *   "menubar"  — Menubar content (same as vertical + ArrowLeft/ArrowRight for menu switching)
 */

const instances = new Map();

const menuItemSelector = '[role="menuitem"], [role="menuitemcheckbox"], [role="menuitemradio"]';

/**
 * Gets all enabled menu items in DOM order within a container.
 * Filters by aria-disabled !== 'true'.
 */
function getEnabledMenuItems(container) {
  if (!container) return [];
  return Array.from(container.querySelectorAll(menuItemSelector))
    .filter(item => item.getAttribute('aria-disabled') !== 'true');
}

/**
 * Focus a menu item reliably. Uses preventScroll and falls back to
 * temporarily setting tabindex="0" if the initial focus() call fails
 * (can happen with anchor elements in some browser/framework contexts).
 */
function focusMenuItem(item) {
  if (!item) return;
  item.focus({ preventScroll: true });
  if (document.activeElement !== item) {
    const prev = item.getAttribute('tabindex');
    item.setAttribute('tabindex', '0');
    item.focus({ preventScroll: true });
    item.setAttribute('tabindex', prev ?? '-1');
  }
}

/**
 * Navigate to the next enabled menu item.
 */
function navigateNext(container, loop) {
  const items = getEnabledMenuItems(container);
  if (items.length === 0) return;

  const currentIndex = items.findIndex(item => item === document.activeElement);
  let nextIndex;

  if (currentIndex === -1) {
    nextIndex = 0;
  } else if (currentIndex === items.length - 1) {
    nextIndex = loop ? 0 : currentIndex;
  } else {
    nextIndex = currentIndex + 1;
  }

  focusMenuItem(items[nextIndex]);
}

/**
 * Navigate to the previous enabled menu item.
 */
function navigatePrevious(container, loop) {
  const items = getEnabledMenuItems(container);
  if (items.length === 0) return;

  const currentIndex = items.findIndex(item => item === document.activeElement);
  let prevIndex;

  if (currentIndex === -1) {
    prevIndex = items.length - 1;
  } else if (currentIndex === 0) {
    prevIndex = loop ? items.length - 1 : 0;
  } else {
    prevIndex = currentIndex - 1;
  }

  focusMenuItem(items[prevIndex]);
}

/**
 * Navigate to the first enabled menu item.
 */
function navigateFirst(container) {
  const items = getEnabledMenuItems(container);
  if (items.length > 0) {
    focusMenuItem(items[0]);
  }
}

/**
 * Navigate to the last enabled menu item.
 */
function navigateLast(container) {
  const items = getEnabledMenuItems(container);
  if (items.length > 0) {
    focusMenuItem(items[items.length - 1]);
  }
}

/**
 * Focus an element using double requestAnimationFrame for reliable timing
 * after Blazor render cycles and portal mounts.
 */
function focusWithDoubleRaf(element) {
  requestAnimationFrame(() => {
    requestAnimationFrame(() => {
      if (element) {
        element.focus({ preventScroll: true });
      }
    });
  });
}

/**
 * Initialize keyboard handling for a menu container.
 * @param {HTMLElement} container - The [role="menu"] element
 * @param {DotNetObjectReference} dotNetRef - For Escape/menu-switch callbacks
 * @param {string} instanceId - Unique ID for this instance
 * @param {object} config
 * @param {string} config.mode - "vertical" (default) or "menubar"
 * @param {boolean} config.loop - Whether navigation wraps (default true)
 * @param {string|null} config.initialFocus - "first", "last", or null
 */
export function initialize(container, dotNetRef, instanceId, config) {
  if (!container || !dotNetRef) return;

  // Dispose existing instance if re-initializing
  dispose(instanceId);

  const mode = config?.mode || 'vertical';
  const loop = config?.loop !== false;
  const initialFocus = config?.initialFocus || null;

  const handleKeyDown = (e) => {
    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault();
        navigateNext(container, loop);
        break;

      case 'ArrowUp':
        e.preventDefault();
        navigatePrevious(container, loop);
        break;

      case 'Home':
        e.preventDefault();
        navigateFirst(container);
        break;

      case 'End':
        e.preventDefault();
        navigateLast(container);
        break;

      case 'Enter':
      case ' ':
        e.preventDefault();
        if (document.activeElement && container.contains(document.activeElement)) {
          document.activeElement.click();
        }
        break;

      case 'Escape':
        e.preventDefault();
        dotNetRef.invokeMethodAsync('JsOnEscapeKey').catch(() => {});
        break;

      case 'ArrowRight':
        if (mode === 'menubar') {
          e.preventDefault();
          dotNetRef.invokeMethodAsync('JsOnNextMenu').catch(() => {});
        }
        break;

      case 'ArrowLeft':
        if (mode === 'menubar') {
          e.preventDefault();
          dotNetRef.invokeMethodAsync('JsOnPreviousMenu').catch(() => {});
        }
        break;
    }
  };

  container.addEventListener('keydown', handleKeyDown);
  instances.set(instanceId, { container, handler: handleKeyDown });

  // Apply initial focus using double rAF for reliable timing
  if (initialFocus === 'first') {
    focusWithDoubleRaf(container);
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        navigateFirst(container);
      });
    });
  } else if (initialFocus === 'last') {
    focusWithDoubleRaf(container);
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        navigateLast(container);
      });
    });
  }
}

/**
 * Dispose a specific instance, removing the keydown listener.
 * @param {string} instanceId
 */
export function dispose(instanceId) {
  const stored = instances.get(instanceId);
  if (!stored) return;

  stored.container.removeEventListener('keydown', stored.handler);
  instances.delete(instanceId);
}

/**
 * Focus the first enabled menu item using double rAF.
 * Exported for external callers (e.g., trigger ArrowDown opens menu).
 * @param {HTMLElement} container
 */
export function focusFirstItem(container) {
  requestAnimationFrame(() => {
    requestAnimationFrame(() => {
      navigateFirst(container);
    });
  });
}

/**
 * Focus the last enabled menu item using double rAF.
 * Exported for external callers (e.g., trigger ArrowUp opens menu).
 * @param {HTMLElement} container
 */
export function focusLastItem(container) {
  requestAnimationFrame(() => {
    requestAnimationFrame(() => {
      navigateLast(container);
    });
  });
}

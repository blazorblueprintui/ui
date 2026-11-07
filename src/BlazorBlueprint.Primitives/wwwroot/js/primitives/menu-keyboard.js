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

/** Type-ahead search delay in milliseconds. Resets the search buffer after inactivity. */
const TYPE_AHEAD_DELAY = 350;

/**
 * Gets all enabled menu items in DOM order within a container.
 * Filters by aria-disabled !== 'true'.
 */
function getEnabledMenuItems(container) {
  if (!container) return [];
  return Array.from(container.querySelectorAll(menuItemSelector))
    .filter(item => item.getAttribute('aria-disabled') !== 'true' && item.closest('[role="menu"]') === container);
}

/**
 * Focus a menu item reliably and reveal it within its own panel.
 * Uses preventScroll and falls back to temporarily setting tabindex="0"
 * if the initial focus() call fails (can happen with anchor elements in
 * some browser/framework contexts).
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

  const menu = item.closest('[role="menu"]');
  if (!menu || menu.scrollHeight <= menu.clientHeight || !menu.offsetHeight) return;

  const panel = menu.getBoundingClientRect();
  const box = item.getBoundingClientRect();
  // Rects include the opening scale animation; scrollTop uses unscaled CSS pixels.
  const scale = panel.height / menu.offsetHeight || 1;
  const top = panel.top + menu.clientTop * scale;
  const bottom = top + menu.clientHeight * scale;
  if (box.top < top) menu.scrollTop -= (top - box.top) / scale;
  else if (box.bottom > bottom) menu.scrollTop += (box.bottom - bottom) / scale;
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
 * Handle type-ahead search: accumulates typed characters and focuses
 * the next matching menu item. Resets after TYPE_AHEAD_DELAY ms of inactivity.
 */
function handleTypeAhead(instance, container, key) {
  clearTimeout(instance.typeAheadTimer);
  instance.typeAheadBuffer = (instance.typeAheadBuffer || '') + key.toLowerCase();

  instance.typeAheadTimer = setTimeout(() => {
    instance.typeAheadBuffer = '';
  }, TYPE_AHEAD_DELAY);

  const items = getEnabledMenuItems(container);
  if (items.length === 0) return;

  const search = instance.typeAheadBuffer;
  const currentIndex = items.findIndex(item => item === document.activeElement);
  const startIndex = currentIndex >= 0 ? currentIndex + 1 : 0;

  // Search from current position forward, wrapping around
  for (let i = 0; i < items.length; i++) {
    const index = (startIndex + i) % items.length;
    const text = items[index].textContent?.trim().toLowerCase() || '';
    if (text.startsWith(search)) {
      focusMenuItem(items[index]);
      return;
    }
  }
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
  const instance = { container, handler: null, typeAheadBuffer: '', typeAheadTimer: null };

  const handleKeyDown = (e) => {
    if (e.defaultPrevented || e.isComposing || e.ctrlKey || e.metaKey || e.altKey) return;
    if (e.target?.closest('[role="menu"]') !== container) return;
    const active = document.activeElement;
    const rtl = getComputedStyle(container).direction === 'rtl';
    const forward = rtl ? 'ArrowLeft' : 'ArrowRight';
    const back = rtl ? 'ArrowRight' : 'ArrowLeft';
    const submenuTrigger = active?.hasAttribute('data-bb-submenu-trigger') && active.getAttribute('aria-disabled') !== 'true';
    if (submenuTrigger && (e.key === forward || e.key === 'Enter' || e.key === ' ')) {
      e.preventDefault();
      e.stopPropagation();
      const submenu = document.getElementById(active.getAttribute('aria-controls'));
      if (active.getAttribute('aria-expanded') === 'true' && submenu) {
        navigateFirst(submenu);
      } else {
        active.click();
      }
      return;
    }
    if (mode === 'submenu' && e.key === back) {
      e.preventDefault();
      e.stopPropagation();
      dotNetRef.invokeMethodAsync('JsOnEscapeKey').catch(() => {});
      return;
    }
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
        // Ours to handle, so it must not also reach the document-level escape stack — otherwise
        // a menu inside a dialog closes both at once.
        e.stopPropagation();
        dotNetRef.invokeMethodAsync('JsOnEscapeKey').catch(() => {});
        break;

      case 'ArrowRight':
        if (mode === 'menubar') {
          e.preventDefault();
          dotNetRef.invokeMethodAsync(rtl ? 'JsOnPreviousMenu' : 'JsOnNextMenu').catch(() => {});
        }
        break;

      case 'ArrowLeft':
        if (mode === 'menubar') {
          e.preventDefault();
          dotNetRef.invokeMethodAsync(rtl ? 'JsOnNextMenu' : 'JsOnPreviousMenu').catch(() => {});
        }
        break;

      default:
        // Type-ahead: single printable characters trigger search
        if (e.key.length === 1 && !e.ctrlKey && !e.metaKey && !e.altKey) {
          e.preventDefault();
          handleTypeAhead(instance, container, e.key);
        }
        break;
    }
  };

  // A hover-open submenu already exists when its trigger is clicked. Move focus without
  // waiting for another render or reopening the floating overlay.
  const handleClick = e => {
    const trigger = e.target?.closest('[data-bb-submenu-trigger]');
    if (trigger?.closest('[role="menu"]') !== container || trigger.getAttribute('aria-disabled') === 'true') return;
    if (trigger.getAttribute('aria-expanded') === 'true') {
      navigateFirst(document.getElementById(trigger.getAttribute('aria-controls')));
    }
  };
  instance.clickHandler = handleClick;
  container.addEventListener('click', handleClick);
  instance.handler = handleKeyDown;
  container.addEventListener('keydown', handleKeyDown);
  instances.set(instanceId, instance);

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
  } else if (initialFocus === 'container') {
    // Focus the container itself (enables keyboard events) without
    // navigating to any item. ArrowDown will move to the first item.
    focusWithDoubleRaf(container);
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
  stored.container.removeEventListener('click', stored.clickHandler);
  clearTimeout(stored.typeAheadTimer);
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

/** Applies menu focus after a floating overlay becomes visible. */
export function focusInitial(container, initialFocus) {
  if (!container) return;
  if (initialFocus === 'first') navigateFirst(container);
  else if (initialFocus === 'last') navigateLast(container);
  else if (initialFocus === 'container') container.focus({ preventScroll: true });
}

/**
 * Keeps a pointer-positioned menu inside the viewport.
 *
 * ContextMenu places its content at the raw click coordinates, so a right-click near the bottom
 * or right edge would otherwise render the menu partly off-screen with no way to reach the items.
 * Flips the menu back across the pointer when it does not fit, which is what native context menus
 * do, and falls back to clamping when it does not fit on either side.
 *
 * @param {HTMLElement} container The menu panel, already positioned at (x, y).
 * @param {number} x The pointer's clientX.
 * @param {number} y The pointer's clientY.
 * @param {number} margin Pixels to keep between the menu and the viewport edge.
 */
export function clampToViewport(container, x, y, margin = 8) {
  if (!container) return;
  const { offsetWidth: width, offsetHeight: height } = container;
  if (!width || !height) return;

  const limitX = window.innerWidth - margin;
  const limitY = window.innerHeight - margin;

  // Flip back across the pointer first; only clamp if the menu fits on neither side.
  let left = x + width > limitX ? x - width : x;
  let top = y + height > limitY ? y - height : y;

  left = Math.max(margin, Math.min(left, limitX - width));
  top = Math.max(margin, Math.min(top, limitY - height));

  container.style.left = `${left}px`;
  container.style.top = `${top}px`;
}

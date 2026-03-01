/**
 * Tree view keyboard navigation
 * Handles arrow key navigation, expand/collapse, selection, and checkbox toggling.
 */

const instances = new Map();

/**
 * Gets all visible tree items (role="treeitem") in DOM order within a container.
 * Only returns items that are actually visible (not inside collapsed groups).
 * @param {HTMLElement} container
 * @returns {HTMLElement[]}
 */
function getVisibleTreeItems(container) {
  if (!container) return [];
  const items = Array.from(container.querySelectorAll('[role="treeitem"]'));
  return items.filter(item => {
    // Check that all ancestor groups are visible (parent treeitem is expanded)
    let el = item.parentElement;
    while (el && el !== container) {
      if (el.getAttribute('role') === 'group') {
        const parentItem = el.parentElement;
        if (parentItem && parentItem.getAttribute('role') === 'treeitem') {
          if (parentItem.getAttribute('aria-expanded') !== 'true') {
            return false;
          }
        }
      }
      el = el.parentElement;
    }
    return true;
  });
}

/**
 * Gets the data-value attribute from a tree item element.
 * @param {HTMLElement} el
 * @returns {string|null}
 */
function getNodeValue(el) {
  return el ? el.getAttribute('data-value') : null;
}

/**
 * Checks if a tree item is disabled.
 * @param {HTMLElement} el
 * @returns {boolean}
 */
function isDisabled(el) {
  return el && el.getAttribute('aria-disabled') === 'true';
}

/**
 * Finds the closest treeitem element from the event target.
 * @param {Event} e
 * @returns {HTMLElement|null}
 */
function getTreeItemFromEvent(e) {
  return e.target.closest('[role="treeitem"]');
}

/**
 * Initialize keyboard navigation for a tree view instance.
 * @param {HTMLElement} containerElement
 * @param {object} dotNetRef - DotNetObjectReference for C# callbacks
 * @param {string} instanceId - Unique instance ID
 */
export function initialize(containerElement, dotNetRef, instanceId) {
  if (!containerElement || !dotNetRef) return;

  const state = { containerElement, dotNetRef, instanceId };

  const handleKeyDown = (e) => {
    const currentItem = getTreeItemFromEvent(e);
    if (!currentItem) return;

    const value = getNodeValue(currentItem);
    if (!value) return;

    const items = getVisibleTreeItems(containerElement);
    const currentIndex = items.indexOf(currentItem);

    switch (e.key) {
      case 'ArrowDown': {
        e.preventDefault();
        // Move focus to next visible node
        for (let i = currentIndex + 1; i < items.length; i++) {
          if (!isDisabled(items[i])) {
            items[i].focus();
            break;
          }
        }
        break;
      }

      case 'ArrowUp': {
        e.preventDefault();
        // Move focus to previous visible node
        for (let i = currentIndex - 1; i >= 0; i--) {
          if (!isDisabled(items[i])) {
            items[i].focus();
            break;
          }
        }
        break;
      }

      case 'ArrowRight': {
        e.preventDefault();
        const isExpanded = currentItem.getAttribute('aria-expanded') === 'true';
        const hasChildren = currentItem.getAttribute('data-has-children') === 'true';

        if (hasChildren && !isExpanded) {
          // Expand the node
          dotNetRef.invokeMethodAsync('JsOnNodeExpand', value);
        } else if (hasChildren && isExpanded) {
          // Move to first child
          const group = currentItem.querySelector('[role="group"]');
          if (group) {
            const firstChild = group.querySelector('[role="treeitem"]');
            if (firstChild && !isDisabled(firstChild)) {
              firstChild.focus();
            }
          }
        }
        break;
      }

      case 'ArrowLeft': {
        e.preventDefault();
        const isExpanded = currentItem.getAttribute('aria-expanded') === 'true';

        if (isExpanded) {
          // Collapse the node
          dotNetRef.invokeMethodAsync('JsOnNodeCollapse', value);
        } else {
          // Move to parent
          const group = currentItem.parentElement;
          if (group && group.getAttribute('role') === 'group') {
            const parentItem = group.parentElement;
            if (parentItem && parentItem.getAttribute('role') === 'treeitem' && !isDisabled(parentItem)) {
              parentItem.focus();
            }
          }
        }
        break;
      }

      case 'Enter': {
        e.preventDefault();
        if (!isDisabled(currentItem)) {
          const hasChildren = currentItem.getAttribute('data-has-children') === 'true';
          dotNetRef.invokeMethodAsync('JsOnNodeActivate', value, hasChildren);
        }
        break;
      }

      case ' ': {
        e.preventDefault();
        if (!isDisabled(currentItem)) {
          // If checkable, toggle checkbox; otherwise, activate/select
          const isCheckable = currentItem.getAttribute('aria-checked') !== null;
          if (isCheckable) {
            dotNetRef.invokeMethodAsync('JsOnNodeCheck', value);
          } else {
            const hasChildren = currentItem.getAttribute('data-has-children') === 'true';
            dotNetRef.invokeMethodAsync('JsOnNodeActivate', value, hasChildren);
          }
        }
        break;
      }

      case 'Home': {
        e.preventDefault();
        // Focus first visible node
        for (let i = 0; i < items.length; i++) {
          if (!isDisabled(items[i])) {
            items[i].focus();
            break;
          }
        }
        break;
      }

      case 'End': {
        e.preventDefault();
        // Focus last visible node
        for (let i = items.length - 1; i >= 0; i--) {
          if (!isDisabled(items[i])) {
            items[i].focus();
            break;
          }
        }
        break;
      }

      case '*': {
        e.preventDefault();
        // Expand all siblings of the focused node
        dotNetRef.invokeMethodAsync('JsOnExpandSiblings', value);
        break;
      }

      default:
        return; // Don't stop propagation for unhandled keys
    }
  };

  // Handle click to select/toggle
  const handleClick = (e) => {
    const treeItem = getTreeItemFromEvent(e);
    if (!treeItem || isDisabled(treeItem)) return;

    const value = getNodeValue(treeItem);
    if (!value) return;

    // Check if the click was on the expand toggle area
    const toggle = e.target.closest('[data-tree-toggle]');
    if (toggle) {
      const isExpanded = treeItem.getAttribute('aria-expanded') === 'true';
      if (isExpanded) {
        dotNetRef.invokeMethodAsync('JsOnNodeCollapse', value);
      } else {
        dotNetRef.invokeMethodAsync('JsOnNodeExpand', value);
      }
      return;
    }

    // Check if click was on a checkbox
    const checkbox = e.target.closest('[data-tree-checkbox]');
    if (checkbox) {
      dotNetRef.invokeMethodAsync('JsOnNodeCheck', value);
      return;
    }

    // Otherwise, select the node
    const hasChildren = treeItem.getAttribute('data-has-children') === 'true';
    dotNetRef.invokeMethodAsync('JsOnNodeActivate', value, hasChildren);

    // Make sure the clicked item gets focus
    treeItem.focus();
  };

  containerElement.addEventListener('keydown', handleKeyDown);
  containerElement.addEventListener('click', handleClick);

  state.handleKeyDown = handleKeyDown;
  state.handleClick = handleClick;
  instances.set(instanceId, state);
}

/**
 * Dispose keyboard navigation for a tree view instance.
 * @param {string} instanceId
 */
export function dispose(instanceId) {
  const state = instances.get(instanceId);
  if (!state) return;

  if (state.containerElement) {
    state.containerElement.removeEventListener('keydown', state.handleKeyDown);
    state.containerElement.removeEventListener('click', state.handleClick);
  }

  instances.delete(instanceId);
}

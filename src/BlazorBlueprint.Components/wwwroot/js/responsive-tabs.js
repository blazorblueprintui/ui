// Responsive tabs overflow detection
// Uses ResizeObserver to detect when tab buttons overflow their container

const states = new Map();

/**
 * Initializes overflow detection for a responsive tabs container.
 * @param {DotNetObject} dotNetRef - Reference to the Blazor component
 * @param {string} componentId - Unique identifier for this instance
 * @param {HTMLElement} containerElement - The wrapper element containing the tablist
 */
export function initialize(dotNetRef, componentId, containerElement) {
  if (!dotNetRef || !containerElement) {
    return;
  }

  const tablist = containerElement.querySelector('[role="tablist"]');
  if (!tablist) {
    return;
  }

  // Measure the natural width of all tabs once, before any hiding occurs.
  // This avoids the feedback loop where hiding tabs changes scrollWidth.
  let tabsNaturalWidth = tablist.scrollWidth;
  let lastOverflowing = null;

  const check = () => {
    const availableWidth = containerElement.clientWidth;
    const isOverflowing = tabsNaturalWidth > availableWidth;
    if (isOverflowing !== lastOverflowing) {
      lastOverflowing = isOverflowing;
      dotNetRef.invokeMethodAsync('OnOverflowChange', isOverflowing).catch(() => {});
    }
  };

  const observer = new ResizeObserver(() => check());
  observer.observe(containerElement);

  states.set(componentId, { observer, dotNetRef, tablist, remeasure });

  // Initial check
  check();

  /**
   * Re-measures the natural tab width. Called when tabs are added/removed.
   */
  function remeasure() {
    tabsNaturalWidth = tablist.scrollWidth;
    lastOverflowing = null;
    check();
  }
}

/**
 * Triggers a re-measurement of the natural tab width.
 * @param {string} componentId - Unique identifier for the instance
 */
export function remeasure(componentId) {
  const state = states.get(componentId);
  if (state) {
    state.remeasure();
  }
}

/**
 * Disposes overflow detection for a responsive tabs instance.
 * @param {string} componentId - Unique identifier for the instance
 */
export function dispose(componentId) {
  const state = states.get(componentId);
  if (state) {
    state.observer.disconnect();
    states.delete(componentId);
  }
}

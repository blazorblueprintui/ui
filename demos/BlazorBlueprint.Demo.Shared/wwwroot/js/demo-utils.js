/**
 * Demo site utility functions.
 * Replaces inline eval() calls with proper module exports.
 */

let globalSearchHandler = null;
let globalSearchDotNetRef = null;

/**
 * Scroll the main content area to the top.
 */
export function scrollMainContentToTop() {
  const el = document.getElementById('main-content');
  if (el) {
    el.scrollTo(0, 0);
  }
}

/**
 * Register a global Ctrl+K keyboard shortcut for search.
 * @param {object} dotNetRef - DotNetObjectReference for callbacks
 */
export function registerGlobalSearch(dotNetRef) {
  disposeGlobalSearch();
  globalSearchDotNetRef = dotNetRef;
  globalSearchHandler = function (e) {
    if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
      e.preventDefault();
      if (globalSearchDotNetRef) {
        globalSearchDotNetRef.invokeMethodAsync('OpenSearch');
      }
    }
  };
  document.addEventListener('keydown', globalSearchHandler);
}

/**
 * Remove the global search keyboard shortcut.
 */
export function disposeGlobalSearch() {
  if (globalSearchHandler) {
    document.removeEventListener('keydown', globalSearchHandler);
    globalSearchHandler = null;
  }
  globalSearchDotNetRef = null;
}

/**
 * Get all localStorage keys matching a prefix.
 * @param {string} prefix - The prefix to filter by
 * @returns {string[]} Matching keys
 */
export function getLocalStorageKeysByPrefix(prefix) {
  const keys = [];
  for (let i = 0; i < localStorage.length; i++) {
    const key = localStorage.key(i);
    if (key && key.startsWith(prefix)) {
      keys.push(key);
    }
  }
  return keys;
}

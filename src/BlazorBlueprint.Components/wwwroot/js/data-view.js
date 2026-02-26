/**
 * DataView scroll utilities.
 * Provides helpers for infinite scroll detection that work correctly
 * regardless of whether the content uses a flex list or a CSS grid layout
 * (including grids with multiple items per row).
 */

/**
 * Returns true when the scrollable container element is within `threshold` pixels
 * of its bottom edge.  Works for both flex-column lists and multi-column CSS grids
 * because the measurement is taken on the *scroll container*, not on individual items.
 *
 * @param {HTMLElement} element   - The scrollable container element.
 * @param {number}      threshold - Pixels from the bottom edge that count as "near bottom" (default 80).
 * @returns {boolean}
 */
export function isNearBottom(element, threshold = 80) {
  if (!element) return false;
  return element.scrollTop + element.clientHeight >= element.scrollHeight - threshold;
}

/**
 * IME composition guard.
 *
 * While an IME is composing (Korean 안녕, Japanese かんじ, Chinese pinyin) the browser
 * fires `input` and `keydown` for every intermediate step. Acting on those events
 * breaks composition in two distinct ways:
 *
 *   1. Assigning `element.value` mid-composition resets the IME's composition buffer —
 *      even when the assigned string is identical to what the element already holds.
 *      Blazor's renderer assigns `element.value` whenever a bound field changes, so any
 *      per-keystroke C# round-trip corrupts composition (typing 안녕 yields ㅇ안안ㄴ녕).
 *   2. Enter/Space/Arrow are how the user commits a composition or picks a candidate.
 *      Handlers bound to those keys fire while the IME still owns the keystroke.
 *
 * Both are fixed the same way: do nothing while composing, act once on `compositionend`.
 *
 * Two consumers, two entry points:
 *
 *   - A JS module that owns its own listeners passes `onFlush` and checks `isComposing`
 *     from inside its handlers.
 *   - A component whose handlers are Blazor's own (`@oninput`, `@onkeydown`, `@bind`)
 *     passes `suppress`. Blazor delegates bubbling events to a single document-level
 *     listener, so stopping propagation at the element keeps those handlers from firing
 *     without the component moving any logic into JS. This works for `input` and
 *     `keydown` specifically: Blazor registers its non-bubbling set (`change`, `blur`,
 *     `focus`, ...) with `capture: true` on `document`, which would run before us, but
 *     `input` and `keydown` are not in that set and are listened for on the bubble phase.
 */

const instances = new Map();

/**
 * Creates a composition guard bound to an element.
 * @param {HTMLElement} element - The element to guard.
 * @param {object} [options] - Configuration.
 * @param {Function} [options.onFlush] - Invoked once after composition ends. For callers
 *   that own their listeners; do not combine with a `suppress` of the same event.
 * @param {string[]} [options.suppress] - Event names to keep from reaching Blazor's
 *   document-level listeners while composing. Only `input` and `keydown` are meaningful.
 * @returns {{isComposing: boolean, dispose: Function}}
 */
export function createCompositionGuard(element, options = {}) {
  if (!element) {
    return { isComposing: false, dispose() {} };
  }

  const { onFlush, suppress = [] } = options;
  const state = { isComposing: false };

  const handleCompositionStart = () => {
    state.isComposing = true;
  };

  const handleCompositionEnd = () => {
    state.isComposing = false;

    // Browsers disagree on whether the final `input` fires before or after
    // `compositionend`, so the last one may have been suppressed above. Re-dispatching
    // guarantees Blazor sees exactly one post-composition `input` under either ordering;
    // a duplicate is harmless because the value is unchanged and the diff emits no edit.
    if (suppress.includes('input')) {
      element.dispatchEvent(new Event('input', { bubbles: true }));
    }

    if (onFlush) {
      onFlush();
    }
  };

  // A composition belongs to the focused element, so focus leaving it ends the composition
  // whether or not the IME said so. Without this, an interrupted composition (tab-switch is
  // the usual way) would latch isComposing and wedge the field for good. Normal blurs are
  // already preceded by compositionend, so this only fires in the pathological case.
  const handleBlur = () => {
    if (state.isComposing) {
      handleCompositionEnd();
    }
  };

  // keyCode 229 is the pre-`isComposing` signal for "this keystroke belongs to the IME".
  const shouldSuppress = (e) => state.isComposing || e.isComposing === true || e.keyCode === 229;

  const handleSuppressed = (e) => {
    if (shouldSuppress(e)) {
      e.stopPropagation();
    }
  };

  element.addEventListener('compositionstart', handleCompositionStart);
  element.addEventListener('compositionend', handleCompositionEnd);
  element.addEventListener('blur', handleBlur);

  for (const name of suppress) {
    element.addEventListener(name, handleSuppressed, true);
  }

  return {
    get isComposing() {
      return state.isComposing;
    },
    dispose() {
      element.removeEventListener('compositionstart', handleCompositionStart);
      element.removeEventListener('compositionend', handleCompositionEnd);
      element.removeEventListener('blur', handleBlur);
      for (const name of suppress) {
        element.removeEventListener(name, handleSuppressed, true);
      }
    }
  };
}

/**
 * Attaches a guard keyed by instance id, for components that have no JS module of their
 * own and drive this directly from C#.
 * @param {HTMLElement} element - The element to guard.
 * @param {string} instanceId - Unique ID for this instance.
 * @param {object} [options] - Same shape as createCompositionGuard's options.
 */
export function attach(element, instanceId, options = {}) {
  detach(instanceId);
  const guard = createCompositionGuard(element, options);
  instances.set(instanceId, guard);
}

/**
 * Removes a guard previously attached with attach().
 * @param {string} instanceId - The instance to detach.
 */
export function detach(instanceId) {
  const guard = instances.get(instanceId);
  if (guard) {
    guard.dispose();
    instances.delete(instanceId);
  }
}

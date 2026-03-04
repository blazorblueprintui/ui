/**
 * Numeric Input JavaScript interop module.
 * Handles input/blur/focus/keydown events in JS to minimize C# interop calls.
 *
 * Four C# callbacks:
 *   - JsOnInput(value)   — called during typing (immediate or after debounce)
 *   - JsOnBlur(value)    — called on blur (always)
 *   - JsOnFocus()        — called on focus (always)
 *   - JsOnKeyDown(key)   — called for step keys (ArrowUp/Down, PageUp/Down, Home/End)
 */

const instances = new Map();

/**
 * Initializes JS event handling for a numeric input element.
 * @param {HTMLElement} element - The input element.
 * @param {DotNetObject} dotNetRef - Reference to the Blazor component.
 * @param {string} instanceId - Unique ID for this instance.
 * @param {object} config - Configuration object.
 * @param {boolean} config.disableDebounce - When true, fire JsOnInput immediately.
 * @param {number} config.debounceMs - Debounce interval (when disableDebounce is false).
 * @param {string[]} config.stepKeys - Key names to intercept (e.g. ['ArrowUp', 'ArrowDown']).
 */
export function initialize(element, dotNetRef, instanceId, config) {
  if (!element || !dotNetRef) {
    return;
  }

  const state = {
    element,
    dotNetRef,
    config,
    debounceTimer: null
  };

  const stepKeySet = new Set(config.stepKeys || []);

  const cancelPending = () => {
    if (state.debounceTimer) {
      clearTimeout(state.debounceTimer);
      state.debounceTimer = null;
    }
  };

  const handleInput = () => {
    const value = element.value;

    if (state.config.disableDebounce) {
      dotNetRef.invokeMethodAsync('JsOnInput', value).catch(() => {});
    } else {
      cancelPending();
      state.debounceTimer = setTimeout(() => {
        state.debounceTimer = null;
        dotNetRef.invokeMethodAsync('JsOnInput', value).catch(() => {});
      }, state.config.debounceMs);
    }
  };

  const handleBlur = () => {
    cancelPending();
    dotNetRef.invokeMethodAsync('JsOnBlur', element.value).catch(() => {});
  };

  const handleFocus = () => {
    dotNetRef.invokeMethodAsync('JsOnFocus').catch(() => {});
  };

  const handleKeyDown = (e) => {
    if (stepKeySet.has(e.key)) {
      e.preventDefault();
      dotNetRef.invokeMethodAsync('JsOnKeyDown', e.key).catch(() => {});
    }
  };

  element.addEventListener('input', handleInput);
  element.addEventListener('blur', handleBlur);
  element.addEventListener('focus', handleFocus);
  element.addEventListener('keydown', handleKeyDown);

  instances.set(instanceId, {
    state,
    handleInput,
    handleBlur,
    handleFocus,
    handleKeyDown,
    element
  });
}

/**
 * Updates the configuration for an existing instance.
 * @param {string} instanceId - The instance to update.
 * @param {object} config - New configuration object.
 */
export function updateConfig(instanceId, config) {
  const stored = instances.get(instanceId);
  if (stored) {
    stored.state.config = config;
  }
}

/**
 * Removes event handlers and cleans up state.
 * @param {string} instanceId - The instance to dispose.
 */
export function dispose(instanceId) {
  const stored = instances.get(instanceId);
  if (!stored) {
    return;
  }

  stored.element.removeEventListener('input', stored.handleInput);
  stored.element.removeEventListener('blur', stored.handleBlur);
  stored.element.removeEventListener('focus', stored.handleFocus);
  stored.element.removeEventListener('keydown', stored.handleKeyDown);

  if (stored.state.debounceTimer) {
    clearTimeout(stored.state.debounceTimer);
  }

  instances.delete(instanceId);
}

/**
 * Text Input JavaScript interop module.
 * Handles input/change events in JS to minimize C# interop calls.
 *
 * Modes:
 *   - onchange:  JS only calls C# on the native change event (zero interop during typing).
 *                For inputs this fires on blur and Enter; for textareas it fires on blur only.
 *   - immediate: JS batches calls via requestAnimationFrame.
 *   - debounced: JS debounces calls via setTimeout.
 */

const instances = new Map();

/**
 * Initializes JS event handling for a text input or textarea element.
 * @param {HTMLElement} element - The input or textarea element.
 * @param {DotNetObject} dotNetRef - Reference to the Blazor component.
 * @param {string} instanceId - Unique ID for this instance.
 * @param {object} config - Configuration object.
 * @param {string} config.mode - 'onchange' | 'immediate' | 'debounced'
 * @param {number} config.debounceMs - Debounce interval (debounced mode only).
 * @param {boolean} [config.hasCharacterCount] - Whether to update a character count element.
 * @param {string} [config.characterCountSelector] - CSS selector for the counter element.
 * @param {number|null} [config.maxLength] - Max length for character count display.
 * @param {boolean} [config.notifyOnBlur] - When true, also fires JsOnChange on blur even if value didn't change.
 */
export function initialize(element, dotNetRef, instanceId, config) {
  if (!element || !dotNetRef) {
    return;
  }

  const state = {
    element,
    dotNetRef,
    config,
    debounceTimer: null,
    rafId: null,
    pendingValue: null
  };

  const callOnInput = (value) => {
    dotNetRef.invokeMethodAsync('JsOnInput', value).catch(() => {});
  };

  const callOnChange = (value) => {
    dotNetRef.invokeMethodAsync('JsOnChange', value).catch(() => {});
  };

  const updateCharacterCount = () => {
    if (!config.hasCharacterCount || !config.characterCountSelector) {
      return;
    }
    const wrapper = element.closest('[data-textarea-wrapper]');
    if (!wrapper) {
      return;
    }
    const counter = wrapper.querySelector(config.characterCountSelector);
    if (!counter) {
      return;
    }
    const len = element.value.length;
    counter.textContent = config.maxLength
      ? `${len}/${config.maxLength}`
      : `${len}`;
  };

  const cancelPending = () => {
    if (state.debounceTimer) {
      clearTimeout(state.debounceTimer);
      state.debounceTimer = null;
    }
    if (state.rafId) {
      cancelAnimationFrame(state.rafId);
      state.rafId = null;
    }
  };

  const handleInput = () => {
    const value = element.value;

    updateCharacterCount();

    if (config.mode === 'onchange') {
      return;
    }

    if (config.mode === 'immediate') {
      if (state.rafId) {
        cancelAnimationFrame(state.rafId);
      }
      state.pendingValue = value;
      state.rafId = requestAnimationFrame(() => {
        state.rafId = null;
        callOnInput(state.pendingValue);
      });
      return;
    }

    if (config.mode === 'debounced') {
      if (state.debounceTimer) {
        clearTimeout(state.debounceTimer);
      }
      state.debounceTimer = setTimeout(() => {
        state.debounceTimer = null;
        callOnInput(value);
      }, config.debounceMs);
    }
  };

  const handleChange = () => {
    cancelPending();
    callOnChange(element.value);
  };

  element.addEventListener('input', handleInput);
  element.addEventListener('change', handleChange);

  const stored = {
    state,
    handleInput,
    handleChange,
    element
  };

  // When notifyOnBlur is true, also listen for blur events so JsOnChange fires
  // even when the value hasn't changed (needed for editing/display toggle in InputField).
  // The change event fires before blur, so use a flag to prevent double-calling.
  if (config.notifyOnBlur) {
    let changeHandledBlur = false;

    const originalHandleChange = handleChange;
    const handleChangeForBlur = () => {
      changeHandledBlur = true;
      originalHandleChange();
    };

    const handleBlur = () => {
      if (!changeHandledBlur) {
        cancelPending();
        callOnChange(element.value);
      }
      changeHandledBlur = false;
    };

    // Replace change listener with dedup version
    element.removeEventListener('change', handleChange);
    element.addEventListener('change', handleChangeForBlur);
    element.addEventListener('blur', handleBlur);

    stored.handleChange = handleChangeForBlur;
    stored.handleBlur = handleBlur;
  }

  instances.set(instanceId, stored);
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
  stored.element.removeEventListener('change', stored.handleChange);

  if (stored.handleBlur) {
    stored.element.removeEventListener('blur', stored.handleBlur);
  }

  if (stored.state.debounceTimer) {
    clearTimeout(stored.state.debounceTimer);
  }
  if (stored.state.rafId) {
    cancelAnimationFrame(stored.state.rafId);
  }

  instances.delete(instanceId);
}

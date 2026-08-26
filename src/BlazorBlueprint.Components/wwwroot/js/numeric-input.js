/**
 * Numeric Input JavaScript interop module.
 * Handles input/blur/focus/keydown events in JS to minimize C# interop calls.
 *
 * Four C# callbacks:
 *   - JsOnInput(value)   — called during typing (immediate or after debounce)
 *   - JsOnBlur(value)    — called on blur (always)
 *   - JsOnFocus()        — called on focus (always)
 *   - JsOnKeyDown(key)   — called for step keys (ArrowUp/Down, PageUp/Down, Home/End)
 *
 * When config.enableWheelStep is true, wheel movement over the focused input is accumulated
 * and mapped to ArrowUp/Down through the same key callback. It is opt-in because stepping
 * requires preventDefault, which takes the scroll away from the page.
 *
 * Sanitization and interop are held back while an IME is composing, then flushed once on
 * compositionend. See composition-guard.js for why.
 */

import { createCompositionGuard } from './composition-guard.js';

const instances = new Map();

/**
 * Accumulated wheel distance, in pixels, that equals one step. Chrome, Edge and Safari
 * report one mouse-wheel detent as deltaY ±100, so a discrete notch still steps once
 * immediately; a trackpad, which emits dozens of small deltas per flick, no longer does.
 */
const WheelStepThreshold = 100;

/**
 * Idle gap after which the accumulator resets, so two deliberate flicks are not summed
 * into one step. Comfortably longer than the ~16ms spacing inside a momentum burst.
 */
const WheelIdleResetMs = 200;

/**
 * Ceiling on the steps a single wheel event may produce. Guards against an outsized delta
 * (a page-mode wheel, or a synthetic event) firing an unbounded burst of interop calls.
 */
const WheelMaxStepsPerEvent = 10;

/**
 * Converts a wheel event's delta to pixels so the threshold above means the same thing
 * everywhere. Firefox reports line mode (3 lines per detent) and, rarely, page mode.
 * @param {WheelEvent} e - The wheel event.
 * @returns {number} deltaY in pixels.
 */
const normalizeWheelDelta = (e) => {
  switch (e.deltaMode) {
    case 1: return e.deltaY * (WheelStepThreshold / 3);
    case 2: return e.deltaY * WheelStepThreshold;
    default: return e.deltaY;
  }
};

/**
 * Folds full-width forms to their ASCII equivalents, one character in one character out so
 * cursor offsets survive. A Japanese IME in 全角 mode emits ０-９ for the digit keys, which
 * would otherwise fail the ASCII range test below and be stripped as garbage.
 * @param {string} ch - A single character.
 * @returns {string} The ASCII equivalent, or the original character.
 */
const foldFullWidth = (ch) => {
  const code = ch.charCodeAt(0);
  if (code >= 0xff10 && code <= 0xff19) {
    return String.fromCharCode(code - 0xff10 + 0x30);
  }
  switch (code) {
    case 0xff0e: return '.';
    case 0xff0c: return ',';
    case 0xff0d: return '-';
    default: return ch;
  }
};

/**
 * Initializes JS event handling for a numeric input element.
 * @param {HTMLElement} element - The input element.
 * @param {DotNetObject} dotNetRef - Reference to the Blazor component.
 * @param {string} instanceId - Unique ID for this instance.
 * @param {object} config - Configuration object.
 * @param {boolean} config.disableDebounce - When true, fire JsOnInput immediately.
 * @param {number} config.debounceMs - Debounce interval (when disableDebounce is false).
 * @param {string[]} config.stepKeys - Key names to intercept (e.g. ['ArrowUp', 'ArrowDown']).
 * @param {boolean} config.allowDecimal - Whether decimal points are allowed.
 * @param {boolean} config.allowNegative - Whether negative sign is allowed.
 * @param {string} config.decimalSeparator - The decimal separator character used for input sanitization (e.g. '.').
 * @param {boolean} config.enableWheelStep - Whether the wheel steps the value while focused. Off by default.
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
    wheelAccumulator: 0,
    wheelLastEventAt: 0
  };

  const stepKeySet = new Set(config.stepKeys || []);

  /**
   * Strips non-numeric characters from input value, preserving cursor position.
   * Allows digits, at most one decimal point (if allowDecimal), and a leading minus (if allowNegative).
   */
  const sanitizeInput = () => {
    const cfg = state.config;
    const raw = element.value;
    const cursorPos = element.selectionStart ?? raw.length;

    let sanitized = '';
    let hasDecimal = false;
    let removed = 0;

    for (let i = 0; i < raw.length; i++) {
      const ch = foldFullWidth(raw[i]);
      const decSep = cfg.decimalSeparator || '.';
      if (ch >= '0' && ch <= '9') {
        sanitized += ch;
      } else if ((ch === '.' || ch === ',' || ch === decSep) && cfg.allowDecimal && !hasDecimal) {
        sanitized += decSep;
        hasDecimal = true;
      } else if (ch === '-' && cfg.allowNegative && sanitized.length === 0) {
        sanitized += ch;
      } else {
        if (i < cursorPos) {
          removed++;
        }
      }
    }

    if (sanitized !== raw) {
      element.value = sanitized;
      const newPos = Math.max(0, Math.min(cursorPos - removed, sanitized.length));
      try {
        element.setSelectionRange(newPos, newPos);
      } catch {
        // setSelectionRange not supported for this input type
      }
    }
  };

  const cancelPending = () => {
    if (state.debounceTimer) {
      clearTimeout(state.debounceTimer);
      state.debounceTimer = null;
    }
  };

  const handleInput = () => {
    // sanitizeInput writes element.value, which would reset the composition buffer;
    // guard.onFlush re-runs this once the IME commits.
    if (guard.isComposing) {
      return;
    }

    sanitizeInput();
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
    // Arrow/Home/End drive the IME candidate list while composing — stepping the value
    // here would steal them and preventDefault the IME's own handling.
    if (guard.isComposing || e.isComposing === true || e.keyCode === 229) {
      return;
    }

    if (stepKeySet.has(e.key)) {
      e.preventDefault();
      dotNetRef.invokeMethodAsync('JsOnKeyDown', e.key).catch(() => {});
    }
  };

  /**
   * Steps the value when the accumulated wheel distance crosses one detent, reusing the
   * keyboard step path so clamping, min/max and step all stay in one place in C#.
   */
  const handleWheel = (e) => {
    if (document.activeElement !== element || e.deltaY === 0) {
      return;
    }

    // The gesture is ours for as long as the input holds focus — let go of the accumulator
    // rather than the scroll, so a gesture never scrolls the page halfway through a step.
    e.preventDefault();

    const delta = normalizeWheelDelta(e);
    const now = Date.now();

    // A new gesture starts clean: an idle gap, or a reversal of direction.
    if (now - state.wheelLastEventAt > WheelIdleResetMs ||
        (state.wheelAccumulator !== 0 && Math.sign(delta) !== Math.sign(state.wheelAccumulator))) {
      state.wheelAccumulator = 0;
    }

    state.wheelLastEventAt = now;
    state.wheelAccumulator += delta;

    let steps = Math.trunc(state.wheelAccumulator / WheelStepThreshold);
    if (steps === 0) {
      return;
    }

    // Carry the remainder so a burst of sub-threshold events still adds up over time.
    state.wheelAccumulator -= steps * WheelStepThreshold;

    steps = Math.max(-WheelMaxStepsPerEvent, Math.min(WheelMaxStepsPerEvent, steps));

    const key = steps < 0 ? 'ArrowUp' : 'ArrowDown';
    for (let i = 0; i < Math.abs(steps); i++) {
      dotNetRef.invokeMethodAsync('JsOnKeyDown', key).catch(() => {});
    }
  };

  const guard = createCompositionGuard(element, { onFlush: handleInput });

  element.addEventListener('input', handleInput);
  element.addEventListener('blur', handleBlur);
  element.addEventListener('focus', handleFocus);
  element.addEventListener('keydown', handleKeyDown);

  const stored = {
    state,
    handleInput,
    handleBlur,
    handleFocus,
    handleKeyDown,
    handleWheel,
    wheelAttached: false,
    guard,
    element
  };

  instances.set(instanceId, stored);

  // Opt-in only: with wheel stepping off no listener exists at all, so nothing calls
  // preventDefault and page scrolling over the input is exactly as it was.
  setWheelStepEnabled(instanceId, config.enableWheelStep === true);
}

/**
 * Enables or disables wheel stepping after initialization, attaching or removing the
 * listener so the disabled state costs nothing and never intercepts a scroll.
 * @param {string} instanceId - The instance to update.
 * @param {boolean} enabled - Whether the wheel steps the value while the input is focused.
 */
export function setWheelStepEnabled(instanceId, enabled) {
  const stored = instances.get(instanceId);
  if (!stored) {
    return;
  }

  const shouldAttach = enabled === true;
  if (shouldAttach === stored.wheelAttached) {
    return;
  }

  if (shouldAttach) {
    stored.element.addEventListener('wheel', stored.handleWheel, { passive: false });
  } else {
    stored.element.removeEventListener('wheel', stored.handleWheel);
  }

  stored.wheelAttached = shouldAttach;
  stored.state.wheelAccumulator = 0;
  stored.state.wheelLastEventAt = 0;
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
  if (stored.wheelAttached) {
    stored.element.removeEventListener('wheel', stored.handleWheel);
  }
  stored.guard.dispose();

  if (stored.state.debounceTimer) {
    clearTimeout(stored.state.debounceTimer);
  }

  instances.delete(instanceId);
}

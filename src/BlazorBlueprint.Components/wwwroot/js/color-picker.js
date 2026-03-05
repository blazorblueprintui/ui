// Color picker drag handler
// Handles pointer capture and events natively for smooth drag interaction

const states = new Map();

/**
 * Initializes drag handling for the saturation/brightness area
 * @param {HTMLElement} element - The color area element
 * @param {DotNetObject} dotNetRef - Reference to the Blazor component
 * @param {string} pickerId - Unique identifier for the picker
 */
export function initializeArea(element, dotNetRef, pickerId) {
  if (!element || !dotNetRef) return;

  const state = getOrCreateState(pickerId);
  state.dotNetRef = dotNetRef;

  const calculatePosition = (e) => {
    const rect = element.getBoundingClientRect();
    const satPct = Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width));
    const brightPct = Math.max(0, Math.min(1, 1 - (e.clientY - rect.top) / rect.height));
    return { satPct, brightPct };
  };

  const onPointerDown = (e) => {
    if (state.areaPointerId != null) return;
    e.preventDefault();
    state.areaPointerId = e.pointerId;
    element.setPointerCapture(e.pointerId);
    document.body.style.userSelect = 'none';

    const { satPct, brightPct } = calculatePosition(e);
    dotNetRef.invokeMethodAsync('UpdateAreaFromJs', satPct, brightPct).catch(() => {});
  };

  const onPointerMove = (e) => {
    if (e.pointerId !== state.areaPointerId) return;
    e.preventDefault();

    const { satPct, brightPct } = calculatePosition(e);
    dotNetRef.invokeMethodAsync('UpdateAreaFromJs', satPct, brightPct).catch(() => {});
  };

  const onPointerUp = (e) => {
    if (e.pointerId !== state.areaPointerId) return;
    element.releasePointerCapture(e.pointerId);
    state.areaPointerId = null;
    document.body.style.userSelect = '';
  };

  const onPointerCancel = (e) => {
    if (e.pointerId !== state.areaPointerId) return;
    state.areaPointerId = null;
    document.body.style.userSelect = '';
  };

  element.addEventListener('pointerdown', onPointerDown);
  element.addEventListener('pointermove', onPointerMove);
  element.addEventListener('pointerup', onPointerUp);
  element.addEventListener('pointercancel', onPointerCancel);

  state.areaCleanup = () => {
    element.removeEventListener('pointerdown', onPointerDown);
    element.removeEventListener('pointermove', onPointerMove);
    element.removeEventListener('pointerup', onPointerUp);
    element.removeEventListener('pointercancel', onPointerCancel);
  };
}

/**
 * Initializes drag handling for a hue or alpha slider
 * @param {HTMLElement} element - The slider element
 * @param {DotNetObject} dotNetRef - Reference to the Blazor component
 * @param {string} pickerId - Unique identifier for the picker
 * @param {string} sliderType - 'hue' or 'alpha'
 */
export function initializeSlider(element, dotNetRef, pickerId, sliderType) {
  if (!element || !dotNetRef) return;

  const state = getOrCreateState(pickerId);
  state.dotNetRef = dotNetRef;
  const pointerKey = sliderType + 'PointerId';

  const calculatePct = (e) => {
    const rect = element.getBoundingClientRect();
    return Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width));
  };

  const onPointerDown = (e) => {
    if (state[pointerKey] != null) return;
    e.preventDefault();
    state[pointerKey] = e.pointerId;
    element.setPointerCapture(e.pointerId);
    document.body.style.userSelect = 'none';

    const pct = calculatePct(e);
    dotNetRef.invokeMethodAsync('UpdateSliderFromJs', sliderType, pct).catch(() => {});
  };

  const onPointerMove = (e) => {
    if (e.pointerId !== state[pointerKey]) return;
    e.preventDefault();

    const pct = calculatePct(e);
    dotNetRef.invokeMethodAsync('UpdateSliderFromJs', sliderType, pct).catch(() => {});
  };

  const onPointerUp = (e) => {
    if (e.pointerId !== state[pointerKey]) return;
    element.releasePointerCapture(e.pointerId);
    state[pointerKey] = null;
    document.body.style.userSelect = '';
  };

  const onPointerCancel = (e) => {
    if (e.pointerId !== state[pointerKey]) return;
    state[pointerKey] = null;
    document.body.style.userSelect = '';
  };

  element.addEventListener('pointerdown', onPointerDown);
  element.addEventListener('pointermove', onPointerMove);
  element.addEventListener('pointerup', onPointerUp);
  element.addEventListener('pointercancel', onPointerCancel);

  state[sliderType + 'Cleanup'] = () => {
    element.removeEventListener('pointerdown', onPointerDown);
    element.removeEventListener('pointermove', onPointerMove);
    element.removeEventListener('pointerup', onPointerUp);
    element.removeEventListener('pointercancel', onPointerCancel);
  };
}

/**
 * Disposes all listeners for a picker
 * @param {string} pickerId - Unique identifier for the picker
 */
export function dispose(pickerId) {
  const state = states.get(pickerId);
  if (!state) return;

  state.areaCleanup?.();
  state.hueCleanup?.();
  state.alphaCleanup?.();
  states.delete(pickerId);
}

function getOrCreateState(pickerId) {
  if (!states.has(pickerId)) {
    states.set(pickerId, {
      dotNetRef: null,
      areaPointerId: null,
      huePointerId: null,
      alphaPointerId: null,
      areaCleanup: null,
      hueCleanup: null,
      alphaCleanup: null
    });
  }
  return states.get(pickerId);
}

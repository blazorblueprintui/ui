import { createDragUpdates, releaseDragPreview } from '../../BlazorBlueprint.Primitives/js/drag-updates.js';

const states = new Map();

function bindDrag(element, dotNetRef, method, getArgs, preview, properties) {
  const root = element.closest('[data-bb-color-picker]');
  if (!root) return () => {};
  let pointerId = null;
  let generation = 0;
  let disposed = false;
  const updates = createDragUpdates((...args) => dotNetRef.invokeMethodAsync(method, ...args));
  const disabled = () => root.getAttribute('data-disabled') === 'true';
  const update = e => {
    if (disabled()) return;
    const rect = element.getBoundingClientRect();
    if (rect.width <= 0 || rect.height <= 0) return;
    const x = Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width));
    const y = Math.max(0, Math.min(1, 1 - (e.clientY - rect.top) / rect.height));
    preview(root, x, y);
    updates.push(getArgs(x, y));
  };
  const down = e => {
    if (pointerId != null || disabled() || e.button > 0) return;
    e.preventDefault();
    generation++;
    pointerId = e.pointerId;
    updates.begin();
    element.setPointerCapture(pointerId);
    document.body.style.userSelect = 'none';
    update(e);
  };
  const move = e => {
    if (e.pointerId !== pointerId) return;
    e.preventDefault();
    update(e);
  };
  const finish = e => {
    if (e.pointerId !== pointerId) return;
    if (e.type === 'pointerup') update(e);
    pointerId = null;
    if (element.hasPointerCapture(e.pointerId)) element.releasePointerCapture(e.pointerId);
    document.body.style.userSelect = '';
    const current = generation;
    releaseDragPreview(updates, root, properties, () => !disposed && generation === current && pointerId == null);
  };
  const listeners = { pointerdown: down, pointermove: move, pointerup: finish, pointercancel: finish, lostpointercapture: finish };
  Object.entries(listeners).forEach(([event, handler]) => element.addEventListener(event, handler));
  return () => {
    disposed = true;
    updates.dispose();
    Object.entries(listeners).forEach(([event, handler]) => element.removeEventListener(event, handler));
    if (pointerId != null) {
      if (element.hasPointerCapture(pointerId)) element.releasePointerCapture(pointerId);
      document.body.style.userSelect = '';
    }
    properties.forEach(property => root.style.removeProperty(property));
  };
}

export function initializeArea(element, dotNetRef, pickerId) {
  if (!element || !dotNetRef) return;
  const state = getOrCreateState(pickerId);
  state.area?.();
  state.area = bindDrag(element, dotNetRef, 'UpdateAreaFromJs', (x, y) => [x, y], (root, x, y) => {
    root.style.setProperty('--bb-color-saturation', `${x * 100}%`);
    root.style.setProperty('--bb-color-brightness', `${(1 - y) * 100}%`);
  }, ['--bb-color-saturation', '--bb-color-brightness']);
}

export function initializeSlider(element, dotNetRef, pickerId, sliderType) {
  if (!element || !dotNetRef || !['hue', 'alpha'].includes(sliderType)) return;
  const state = getOrCreateState(pickerId);
  state[sliderType]?.();
  const position = `--bb-color-${sliderType}-position`;
  const properties = sliderType === 'hue' ? [position, '--bb-color-hue'] : [position];
  state[sliderType] = bindDrag(element, dotNetRef, 'UpdateSliderFromJs', x => [sliderType, x], (root, x) => {
    root.style.setProperty(position, `${x * 100}%`);
    if (sliderType === 'hue') root.style.setProperty('--bb-color-hue', String(x * 360));
  }, properties);
}

export function dispose(pickerId) {
  const state = states.get(pickerId);
  if (!state) return;
  state.area?.();
  state.hue?.();
  state.alpha?.();
  states.delete(pickerId);
}

function getOrCreateState(pickerId) {
  if (!states.has(pickerId)) states.set(pickerId, {});
  return states.get(pickerId);
}

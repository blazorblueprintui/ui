import { createDragUpdates, releaseDragPreview, snapSliderValue } from '../../BlazorBlueprint.Primitives/js/drag-updates.js';

const rangeSliderStates = new Map();

export function initializeRangeSlider(trackElement, dotNetRef, sliderId) {
  if (!trackElement || !dotNetRef) return;
  disposeRangeSlider(sliderId);
  let pointerId = null;
  let thumb = null;
  let start = 0;
  let end = 100;
  let min = 0;
  let max = 100;
  let step = 1;
  let minRange = 0;
  let moved = false;
  let disposed = false;
  let generation = 0;
  const properties = ['--bb-range-start', '--bb-range-end', '--bb-range-width'];
  const updates = createDragUpdates((percentage, activeThumb) =>
    dotNetRef.invokeMethodAsync('UpdateValueFromPercentage', percentage, activeThumb));
  const disabled = () => trackElement.getAttribute('data-disabled') === 'true';
  const percentageAt = e => {
    const rect = trackElement.getBoundingClientRect();
    return rect.width > 0 ? Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width)) : 0;
  };
  const update = e => {
    if (disabled()) return;
    let value = snapSliderValue(percentageAt(e), min, max, step);
    if (thumb === 'start') {
      value = Math.max(min, Math.min(value, end - minRange));
      start = value;
    } else {
      value = Math.min(max, Math.max(value, start + minRange));
      end = value;
    }
    const span = max - min;
    const startPct = span > 0 ? (start - min) / span * 100 : 0;
    const endPct = span > 0 ? (end - min) / span * 100 : 0;
    trackElement.style.setProperty('--bb-range-start', `${startPct}%`);
    trackElement.style.setProperty('--bb-range-end', `${endPct}%`);
    trackElement.style.setProperty('--bb-range-width', `${endPct - startPct}%`);
    updates.push([span > 0 ? (value - min) / span : 0, thumb]);
  };
  const begin = (activeThumb, id) => {
    if (pointerId != null || disabled()) return false;
    generation++;
    pointerId = id;
    thumb = activeThumb;
    moved = false;
    min = Number(trackElement.getAttribute('data-min'));
    max = Number(trackElement.getAttribute('data-max'));
    step = Number(trackElement.getAttribute('data-step'));
    minRange = Number(trackElement.getAttribute('data-min-range'));
    start = Number(trackElement.getAttribute('data-start'));
    end = Number(trackElement.getAttribute('data-end'));
    updates.begin();
    // preventDefault suppresses the browser's usual focus-on-pointerdown behavior.
    // Explicitly focus the active thumb so keyboard input still works after dragging.
    trackElement.querySelector(`[data-thumb="${activeThumb}"]`)?.focus({ preventScroll: true });
    trackElement.setPointerCapture(id);
    document.body.style.userSelect = 'none';
    document.body.style.cursor = 'grabbing';
    return true;
  };
  const down = e => {
    if (pointerId != null || disabled() || e.button > 0) return;
    e.preventDefault();
    const target = e.target.closest('[data-thumb]');
    if (target && trackElement.contains(target)) {
      begin(target.getAttribute('data-thumb'), e.pointerId);
    } else {
      const lo = Number(trackElement.getAttribute('data-min'));
      const hi = Number(trackElement.getAttribute('data-max'));
      const value = lo + percentageAt(e) * (hi - lo);
      const fromStart = Math.abs(value - Number(trackElement.getAttribute('data-start')));
      const fromEnd = Math.abs(value - Number(trackElement.getAttribute('data-end')));
      if (begin(fromStart <= fromEnd ? 'start' : 'end', e.pointerId)) {
        moved = true;
        update(e);
      }
    }
  };
  const move = e => {
    if (e.pointerId !== pointerId) return;
    e.preventDefault();
    moved = true;
    update(e);
  };
  const finish = e => {
    if (e.pointerId !== pointerId) return;
    if (moved && e.type === 'pointerup') update(e);
    pointerId = null;
    if (trackElement.hasPointerCapture(e.pointerId)) trackElement.releasePointerCapture(e.pointerId);
    document.body.style.userSelect = '';
    document.body.style.cursor = '';
    const current = generation;
    releaseDragPreview(updates, trackElement, properties, () => !disposed && generation === current && pointerId == null);
  };
  const listeners = { pointerdown: down, pointermove: move, pointerup: finish, pointercancel: finish, lostpointercapture: finish };
  Object.entries(listeners).forEach(([event, handler]) => trackElement.addEventListener(event, handler));
  rangeSliderStates.set(sliderId, {
    begin,
    dispose() {
      disposed = true;
      updates.dispose();
      Object.entries(listeners).forEach(([event, handler]) => trackElement.removeEventListener(event, handler));
      if (pointerId != null) {
        if (trackElement.hasPointerCapture(pointerId)) trackElement.releasePointerCapture(pointerId);
        document.body.style.userSelect = '';
        document.body.style.cursor = '';
      }
      properties.forEach(property => trackElement.style.removeProperty(property));
    }
  });
}

// Kept for callers using the existing interop entry point; normal pointer capture is local.
export function startDrag(sliderId, thumb, pointerId) {
  rangeSliderStates.get(sliderId)?.begin(thumb, pointerId);
}

export function disposeRangeSlider(sliderId) {
  rangeSliderStates.get(sliderId)?.dispose();
  rangeSliderStates.delete(sliderId);
}

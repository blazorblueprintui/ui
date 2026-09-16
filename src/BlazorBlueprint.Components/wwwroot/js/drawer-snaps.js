const instances = new WeakMap();
export function nearestSnap(points, fraction, dismiss) {
  if (dismiss && fraction < points[0] / 2) return -1;
  let nearest = 0;
  for (let i = 1; i < points.length; i++) {
    if (Math.abs(points[i] - fraction) < Math.abs(points[nearest] - fraction)) nearest = i;
  }
  return nearest;
}
export function configure(element, dotNetRef, options) {
  dispose(element);
  const handle = element?.querySelector('[data-bb-drawer-handle]');
  if (!handle || !options.points?.length) return;
  const horizontal = options.direction === 'left' || options.direction === 'right';
  const dimension = horizontal ? 'width' : 'height';
  const axis = horizontal ? 'clientX' : 'clientY';
  const sign = options.direction === 'top' || options.direction === 'left' ? 1 : -1;
  const viewport = () => horizontal ? (window.visualViewport?.width ?? window.innerWidth) : (window.visualViewport?.height ?? window.innerHeight);
  let drag = null; let pending = false; let disposed = false;
  const paint = fraction => { element.style[dimension] = `${Math.max(0, fraction) * viewport()}px`; };
  const reset = () => paint(options.points[options.index]);
  const commit = async index => {
    if (pending || disposed) return;
    if (index >= 0) { options.index = index; reset(); }
    else paint(0);
    pending = true;
    try { await dotNetRef.invokeMethodAsync('JsOnSnapChanged', index); }
    catch { if (!disposed) reset(); }
    finally { pending = false; }
  };
  const release = () => {
    if (!drag) return;
    const id = drag.id; drag = null;
    if (handle.hasPointerCapture(id)) handle.releasePointerCapture(id);
  };
  const cancel = () => { release(); reset(); };
  const down = event => {
    if (pending || event.button !== 0) return;
    event.preventDefault();
    drag = { id: event.pointerId, start: event[axis], size: element.getBoundingClientRect()[dimension], fraction: options.points[options.index] };
    handle.setPointerCapture(event.pointerId); handle.focus({ preventScroll: true });
  };
  const move = event => {
    if (!drag || drag.id !== event.pointerId) return;
    drag.fraction = Math.max(0, Math.min(options.points.at(-1), (drag.size + ((event[axis] - drag.start) * sign)) / viewport()));
    paint(drag.fraction);
  };
  const up = event => {
    if (!drag || drag.id !== event.pointerId) return;
    const index = nearestSnap(options.points, drag.fraction, options.dismiss);
    release(); void commit(index);
  };
  const keydown = event => {
    if (event.key === 'Escape' && drag) { event.preventDefault(); event.stopPropagation(); cancel(); return; }
    if (pending || event.ctrlKey || event.metaKey || event.altKey) return;
    const delta = event.key === 'ArrowUp' || event.key === 'ArrowRight' ? 1 : event.key === 'ArrowDown' || event.key === 'ArrowLeft' ? -1 : 0;
    if (!delta && event.key !== 'Home' && event.key !== 'End') return;
    event.preventDefault();
    const index = event.key === 'Home' ? 0 : event.key === 'End' ? options.points.length - 1 : Math.max(0, Math.min(options.points.length - 1, options.index + delta));
    void commit(index);
  };
  const listeners = { pointerdown: down, pointermove: move, pointerup: up, pointercancel: cancel, lostpointercapture: () => { if (drag) cancel(); }, keydown };
  for (const [name, listener] of Object.entries(listeners)) handle.addEventListener(name, listener);
  const resize = () => { release(); reset(); };
  window.addEventListener('resize', resize);
  window.visualViewport?.addEventListener('resize', resize);
  instances.set(element, () => {
    disposed = true; release();
    for (const [name, listener] of Object.entries(listeners)) handle.removeEventListener(name, listener);
    window.removeEventListener('resize', resize);
    window.visualViewport?.removeEventListener('resize', resize);
  });
  reset();
}
export function dispose(element) {
  instances.get(element)?.();
  instances.delete(element);
}

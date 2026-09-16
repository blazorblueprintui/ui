// Drag feedback stays in the browser. Only the latest distinct value crosses interop,
// at most every 50 ms, with one callback in flight even on a slow Server circuit.
export function createDragUpdates(send, interval = 50) {
  let pending = null;
  let lastSent = null;
  let lastSentAt = -Infinity;
  let inFlight = false;
  let timer = null;
  let disposed = false;
  let flushing = false;
  let waiters = [];

  const same = (a, b) => a && b && a.length === b.length && a.every((v, i) => Object.is(v, b[i]));
  const settle = () => {
    if (!inFlight && !pending || disposed) {
      flushing = false;
      const current = waiters;
      waiters = [];
      current.forEach(resolve => resolve());
    }
  };
  const drain = () => {
    if (disposed || inFlight) return;
    if (!pending) { settle(); return; }
    const delay = flushing ? 0 : Math.max(0, interval - (performance.now() - lastSentAt));
    if (delay > 0) {
      timer ??= setTimeout(() => { timer = null; drain(); }, delay);
      return;
    }
    const args = pending;
    pending = null;
    lastSent = args;
    lastSentAt = performance.now();
    inFlight = true;
    Promise.resolve().then(() => { if (!disposed) return send(...args); })
      .catch(() => {}) // A disconnected circuit must not leave a drag queue running.
      .finally(() => { inFlight = false; drain(); settle(); });
  };
  return {
    begin() { lastSent = null; lastSentAt = -Infinity; },
    push(args) {
      if (disposed || same(args, pending ?? lastSent)) return;
      pending = same(args, lastSent) ? null : args;
      drain();
    },
    flush() {
      if (disposed) return Promise.resolve();
      if (timer != null) { clearTimeout(timer); timer = null; }
      flushing = true;
      const completion = new Promise(resolve => waiters.push(resolve));
      drain();
      settle();
      return completion;
    },
    dispose() {
      disposed = true;
      pending = null;
      if (timer != null) clearTimeout(timer);
      timer = null;
      settle();
    }
  };
}

// Let the final Blazor render arrive before handing visual ownership back. A new gesture or
// disposal invalidates this cleanup, so an older callback cannot erase a newer preview.
export function releaseDragPreview(updates, element, properties, isCurrent) {
  updates.flush().then(() => requestAnimationFrame(() => requestAnimationFrame(() => {
    if (isCurrent()) properties.forEach(property => element.style.removeProperty(property));
  })));
}

export function snapSliderValue(percentage, min, max, step) {
  let value = min + Math.max(0, Math.min(1, percentage)) * (max - min);
  if (step > 0 && Number.isFinite(step)) {
    const quotient = value / step;
    const floor = Math.floor(quotient);
    // Match Math.Round's midpoint-to-even behavior, including negative ranges.
    value = (quotient - floor === 0.5 ? floor + (Math.abs(floor % 2)) : Math.round(quotient)) * step;
  }
  return Math.max(min, Math.min(max, value));
}

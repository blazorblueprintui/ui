const registrations = new WeakMap();

// Focus and spinbutton keys stay local, including on a high-latency Server circuit.
// Normal input events carry the edited text to Blazor; Tab and text selection remain native.
export function initialize(root) {
  if (!root || registrations.has(root)) return;
  const segments = () => [...root.querySelectorAll('input[data-bb-segment]:not(:disabled)')];
  const focus = event => {
    if (event.target.matches('input[data-bb-segment]')) event.target.select();
  };
  const keydown = event => {
    const input = event.target;
    if (!input.matches('input[data-bb-segment]') || input.disabled || input.readOnly || event.isComposing
      || event.ctrlKey || event.metaKey || event.altKey || event.shiftKey) return;
    if (event.key === 'ArrowLeft' || event.key === 'ArrowRight') {
      const inputs = segments();
      const rtl = getComputedStyle(root).direction === 'rtl';
      const delta = (event.key === 'ArrowRight' ? 1 : -1) * (rtl ? -1 : 1);
      const next = inputs[inputs.indexOf(input) + delta];
      if (next) { event.preventDefault(); next.focus(); next.select(); }
      return;
    }
    if (!['ArrowUp', 'ArrowDown', 'Home', 'End'].includes(event.key)) return;
    event.preventDefault();
    const min = Number(input.dataset.min);
    const max = Number(input.dataset.max);
    const step = Number(input.dataset.step || 1);
    const current = input.value === '' ? min : Number(input.value);
    let next = event.key === 'Home' ? min : event.key === 'End' ? max
      : (Number.isFinite(current) ? current : min) + (event.key === 'ArrowUp' ? step : -step);
    next = Math.min(max, Math.max(min, next));
    input.value = String(next).padStart(Number(input.dataset.digits), '0');
    input.dispatchEvent(new Event('input', { bubbles: true }));
    input.select();
  };
  root.addEventListener('focusin', focus);
  root.addEventListener('keydown', keydown);
  registrations.set(root, { focus, keydown });
}

export function dispose(root) {
  const handlers = registrations.get(root);
  if (!handlers) return;
  root.removeEventListener('focusin', handlers.focus);
  root.removeEventListener('keydown', handlers.keydown);
  registrations.delete(root);
}

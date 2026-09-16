import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';
const source = await readFile(new URL('../../src/BlazorBlueprint.Components/wwwroot/js/scheduler.js', import.meta.url), 'utf8');
const { calculateChange } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);
const minute = 60_000;
const hour = 60 * minute;
const lane = { start: 8 * hour, end: 18 * hour };
const event = { start: 9 * hour, end: 10 * hour };

for (const slots of [15, 30, 60]) {
  test(`moving snaps to ${slots}-minute slots while preserving duration`, () => {
    assert.deepEqual(calculateChange('move', event, lane, lane, 40, slots), {
      start: event.start + slots * minute, end: event.end + slots * minute
    });
    assert.deepEqual(calculateChange('move', event, lane, lane, 10, slots), event);
  });
}
test('moving between days follows the destination lane even across a DST boundary', () => {
  const nextDay = { start: lane.start + 23 * hour, end: lane.end + 23 * hour };
  assert.deepEqual(calculateChange('move', event, lane, nextDay, 40, 30), { start: event.start + 23.5 * hour, end: event.end + 23.5 * hour });
});
test('moving clamps the visible start and preserves overnight duration', () => {
  assert.deepEqual(calculateChange('move', event, lane, lane, -9999, 30), { start: 8 * hour, end: 9 * hour });
  assert.deepEqual(calculateChange('move', event, lane, lane, 9999, 30), { start: 17.5 * hour, end: 18.5 * hour });
  assert.deepEqual(calculateChange('move', { start: 9 * hour, end: 29 * hour }, lane, lane, 40, 30), { start: 9.5 * hour, end: 29.5 * hour });
});
test('both resize edges snap and preserve at least one slot without moving the opposite edge', () => {
  assert.deepEqual(calculateChange('start', event, lane, lane, -40, 15), { start: 8.75 * hour, end: 10 * hour });
  assert.deepEqual(calculateChange('end', event, lane, lane, 40, 60), { start: 9 * hour, end: 11 * hour });
  assert.deepEqual(calculateChange('start', event, lane, lane, 9999, 30), { start: 9.5 * hour, end: 10 * hour });
  assert.deepEqual(calculateChange('end', event, lane, lane, -9999, 30), { start: 9 * hour, end: 9.5 * hour });
});
test('resizing a clipped multi-day appointment preserves the offscreen edge', () => {
  assert.deepEqual(calculateChange('end', { start: 0, end: 10 * hour }, lane, lane, 40, 30), { start: 0, end: 10.5 * hour });
  assert.deepEqual(calculateChange('start', { start: 17 * hour, end: 30 * hour }, lane, lane, -40, 30), { start: 16.5 * hour, end: 30 * hour });
});

// Exercise the pointer lifecycle as well as the time calculations.
const { initialize, dispose } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);
function pointerFixture() {
  const handlers = () => ({ listeners: new Map(),
    addEventListener(name, callback) { this.listeners.set(name, callback); },
    removeEventListener(name) { this.listeners.delete(name); }
  });
  const calls = [];
  const root = { ...handlers(), dataset: { allowDrag: 'true', allowResize: 'true', slotMinutes: '30', timeZone: 'UTC', revision: '1' } };
  const viewport = { scrollTop: 0, scrollLeft: 0, getBoundingClientRect: () => ({ left: 0, right: 500, top: 0, bottom: 800 }) };
  const sourceLane = { dataset: { start: lane.start, end: lane.end, schedulerLane: '0' }, getBoundingClientRect: () => ({ left: 0, top: 0, width: 500 }), closest: () => sourceLane };
  const card = { dataset: { start: event.start, end: event.end, schedulerEvent: 'test' }, isConnected: true,
    closest: selector => selector === '[data-scheduler-lane]' ? sourceLane : selector === '[data-scheduler-event]' ? card : null,
    setPointerCapture() { this.captured = true; }, hasPointerCapture() { return this.captured; }, releasePointerCapture() { this.captured = false; },
    cloneNode: () => ({ style: {}, removeAttribute() {}, setAttribute() {}, querySelectorAll: () => [], querySelector: () => null, remove() {} })
  };
  root.querySelector = () => viewport;
  root.contains = node => node === card || node === sourceLane;
  globalThis.document = { ...handlers(), body: { append() {} }, documentElement: {}, elementFromPoint: () => sourceLane };
  globalThis.window = handlers();
  globalThis.requestAnimationFrame = () => 1;
  globalThis.cancelAnimationFrame = () => {};
  globalThis.getComputedStyle = () => ({ fontSize: '16px', backgroundColor: '#fff' });
  initialize(root, { invokeMethodAsync: async (...args) => calls.push(args) });
  const fire = (name, y, target = card) => root.listeners.get(name)?.({ target, button: 0, pointerId: 1, clientX: 100, clientY: y, preventDefault() {}, stopImmediatePropagation() {} });
  return { root, card, calls, fire };
}

test('a gesture commits once on release, uses the final coordinate and preserves normal clicks', async () => {
  const f = pointerFixture();
  f.fire('pointerdown', 100);
  await f.fire('pointerup', 100);
  assert.equal(f.calls.length, 0);
  f.fire('pointerdown', 100);
  f.fire('pointermove', 140);
  assert.equal(f.calls.length, 0);
  await f.fire('pointerup', 180);
  assert.equal(f.calls.length, 1);
  assert.equal(f.calls[0][6], 10 * hour);
  assert.equal(f.calls[0][7], 11 * hour);
  assert.equal(f.card.captured, false);
  dispose(f.root);
  assert.equal(f.root.listeners.size, 0);
  assert.equal(document.listeners.size, 0);
  assert.equal(window.listeners.size, 0);
});

test('disabled, canceled and disposed gestures never save', async () => {
  const f = pointerFixture();
  f.root.dataset.allowDrag = 'false';
  f.fire('pointerdown', 100);
  f.fire('pointermove', 140);
  await f.fire('pointerup', 140);
  f.root.dataset.allowDrag = 'true';
  f.fire('pointerdown', 100);
  f.fire('pointermove', 140);
  document.listeners.get('keydown')({ key: 'Escape', preventDefault() {}, stopPropagation() {} });
  await f.fire('pointerup', 140);
  f.fire('pointerdown', 100);
  f.fire('pointermove', 140);
  f.fire('pointercancel', 140);
  await f.fire('pointerup', 140);
  f.fire('pointerdown', 100);
  f.fire('pointermove', 140);
  dispose(f.root);
  assert.equal(f.calls.length, 0);
  assert.equal(f.card.captured, false);
});

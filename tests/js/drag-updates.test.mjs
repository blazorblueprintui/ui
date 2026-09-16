import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

const repo = new URL('../../', import.meta.url);
const urlFor = source => `data:text/javascript;base64,${Buffer.from(source).toString('base64')}`;
const helperUrl = urlFor(await readFile(new URL('src/BlazorBlueprint.Primitives/wwwroot/js/drag-updates.js', repo), 'utf8'));
const { createDragUpdates, snapSliderValue } = await import(helperUrl);
async function load(path) {
  const source = (await readFile(new URL(path, repo), 'utf8'))
    .replace(/from '[^']*drag-updates\.js'/g, `from '${helperUrl}'`);
  return import(urlFor(source));
}
const slider = await load('src/BlazorBlueprint.Primitives/wwwroot/js/primitives/slider.js');
const range = await load('src/BlazorBlueprint.Components/wwwroot/js/range-slider.js');
const color = await load('src/BlazorBlueprint.Components/wwwroot/js/color-picker.js');

async function microtasks() { for (let i = 0; i < 15; i++) await Promise.resolve(); }
function clock(t) {
  const saved = Object.fromEntries(['performance', 'setTimeout', 'clearTimeout', 'requestAnimationFrame', 'document']
    .map(key => [key, Object.getOwnPropertyDescriptor(globalThis, key)]));
  let now = 0;
  let nextId = 0;
  const timers = new Map();
  Object.defineProperty(globalThis, 'performance', { configurable: true, value: { now: () => now } });
  globalThis.setTimeout = (fn, delay = 0) => { const id = ++nextId; timers.set(id, { fn, at: now + delay }); return id; };
  globalThis.clearTimeout = id => timers.delete(id);
  globalThis.requestAnimationFrame = fn => setTimeout(fn, 16);
  globalThis.document = { body: { style: {} } };
  t.after(() => {
    for (const [key, descriptor] of Object.entries(saved)) {
      if (descriptor) Object.defineProperty(globalThis, key, descriptor);
      else delete globalThis[key];
    }
  });
  return {
    async advance(ms) {
      const end = now + ms;
      for (;;) {
        const entry = [...timers].filter(([, timer]) => timer.at <= end).sort((a, b) => a[1].at - b[1].at)[0];
        if (!entry) break;
        const [id, timer] = entry;
        timers.delete(id);
        now = timer.at;
        timer.fn();
        await microtasks();
      }
      now = end;
      await microtasks();
    }
  };
}
function element(attributes = {}) {
  const attrs = new Map(Object.entries(attributes));
  const listeners = new Map();
  const styles = new Map();
  const captures = new Set();
  return {
    styles, attrs, listeners, parent: null, children: [],
    focus() { document.activeElement = this; },
    querySelector(selector) { return this.children.find(child => selector === `[data-thumb="${child.getAttribute('data-thumb')}"]`) ?? null; },
    style: { setProperty: (key, value) => styles.set(key, value), removeProperty: key => styles.delete(key) },
    getAttribute: key => attrs.get(key) ?? null,
    getBoundingClientRect: () => ({ left: 0, top: 0, width: 100, height: 100 }),
    addEventListener: (key, fn) => listeners.set(key, fn),
    removeEventListener: key => listeners.delete(key),
    setPointerCapture: id => captures.add(id),
    hasPointerCapture: id => captures.has(id),
    releasePointerCapture: id => captures.delete(id),
    contains(target) { return target === this || target.parent === this; },
    closest(selector) {
      if (selector === '[data-thumb]' && attrs.has('data-thumb')) return this;
      if (selector === '[data-bb-color-picker]' && attrs.has('data-bb-color-picker')) return this;
      return this.parent?.closest(selector) ?? null;
    },
    fire(type, options = {}) {
      listeners.get(type)?.({ type, target: this, pointerId: 1, button: 0, clientX: 50, clientY: 50, preventDefault() {}, ...options });
    }
  };
}

// Network-independent operation counts: the clock and server acknowledgements are controlled.
test('duplicate pointer values do not create duplicate interop calls', async t => {
  clock(t);
  const calls = [];
  const queue = createDragUpdates(value => { calls.push(value); });
  queue.begin();
  for (let i = 0; i < 500; i++) queue.push([0.5]);
  await queue.flush();
  assert.deepEqual(calls, [0.5]);
  queue.dispose();
});

test('live updates are throttled and pointer-up flushes the final value immediately', async t => {
  const time = clock(t);
  const calls = [];
  const queue = createDragUpdates(value => { calls.push(value); });
  queue.push([1]);
  await microtasks();
  queue.push([2]);
  await time.advance(49);
  assert.deepEqual(calls, [1]);
  await time.advance(1);
  assert.deepEqual(calls, [1, 2]);
  queue.push([3]);
  await queue.flush();
  assert.deepEqual(calls, [1, 2, 3]);
  queue.dispose();
});

test('slow circuits keep one callback in flight and deliver the latest final value', async t => {
  clock(t);
  const calls = [];
  const acknowledgements = [];
  const queue = createDragUpdates(value => { calls.push(value); return new Promise(resolve => acknowledgements.push(resolve)); });
  queue.push([0]);
  await microtasks();
  for (let i = 1; i <= 500; i++) queue.push([i]);
  const flushed = queue.flush();
  assert.deepEqual(calls, [0]);
  acknowledgements.shift()();
  await microtasks();
  assert.deepEqual(calls, [0, 500]);
  acknowledgements.shift()();
  await flushed;
  queue.dispose();
});

test('disposal cancels scheduled callbacks and resolves flush waiters', async t => {
  const time = clock(t);
  const calls = [];
  const queue = createDragUpdates(value => { calls.push(value); });
  queue.push([1]);
  await microtasks();
  queue.push([2]);
  queue.dispose();
  await time.advance(100);
  await queue.flush();
  assert.deepEqual(calls, [1]);
});

test('step snapping matches .NET midpoint-to-even and clamps negative ranges', () => {
  assert.equal(snapSliderValue(0.25, 0, 10, 1), 2);
  assert.equal(snapSliderValue(0.35, 0, 10, 1), 4);
  assert.equal(snapSliderValue(0.25, -10, 0, 1), -8);
  assert.equal(snapSliderValue(0, 3, 9, 2), 4);
  assert.equal(snapSliderValue(1, 3, 9, 2), 8);
});

test('slider moves locally, suppresses duplicate steps, and restores rendered styles after acknowledgement', async t => {
  const time = clock(t);
  const track = element({ 'data-min': '0', 'data-max': '100', 'data-step': '10', 'data-orientation': 'vertical' });
  const calls = [];
  slider.initialize(track, { invokeMethodAsync: (...args) => { calls.push(args); return Promise.resolve(); } }, 'slider');
  track.fire('pointerdown', { clientY: 25 });
  assert.equal(track.styles.get('--bb-slider-position'), '80%');
  for (let i = 0; i < 500; i++) track.fire('pointermove', { clientY: 25 });
  track.fire('pointerup', { clientY: 25 });
  await microtasks();
  assert.deepEqual(calls, [['JsUpdateValueFromPercentage', 0.8]]);
  await time.advance(32);
  assert.equal(track.styles.has('--bb-slider-position'), false);
  slider.dispose('slider');
  assert.equal(track.listeners.size, 0);
});

test('disabled slider ignores input and a disposed drag cannot send its pending value', async t => {
  const time = clock(t);
  const track = element({ 'data-disabled': 'true', 'data-min': '0', 'data-max': '100', 'data-step': '1' });
  const calls = [];
  slider.initialize(track, { invokeMethodAsync: (...args) => { calls.push(args); return Promise.resolve(); } }, 'disabled');
  track.fire('pointerdown');
  await microtasks();
  assert.equal(calls.length, 0);
  track.attrs.delete('data-disabled');
  track.fire('pointerdown');
  await microtasks();
  track.fire('pointermove', { clientX: 90 });
  slider.dispose('disabled');
  await time.advance(100);
  assert.equal(calls.length, 1);
  assert.equal(document.body.style.userSelect, '');
});

test('range captures a thumb locally, respects minimum range, and flushes on cancellation', async t => {
  clock(t);
  const track = element({ 'data-min': '0', 'data-max': '100', 'data-step': '1', 'data-min-range': '10', 'data-start': '20', 'data-end': '80' });
  const thumb = element({ 'data-thumb': 'start' });
  thumb.parent = track;
  track.children.push(thumb);
  const calls = [];
  range.initializeRangeSlider(track, { invokeMethodAsync: (...args) => { calls.push(args); return Promise.resolve(); } }, 'range');
  track.fire('pointerdown', { target: thumb, clientX: 20 });
  assert.equal(track.hasPointerCapture(1), true);
  assert.equal(document.activeElement, thumb);
  assert.equal(calls.length, 0);
  track.fire('pointermove', { clientX: 95 });
  assert.equal(track.styles.get('--bb-range-start'), '70%');
  assert.equal(track.styles.get('--bb-range-width'), '10%');
  track.fire('pointercancel');
  await microtasks();
  assert.deepEqual(calls, [['UpdateValueFromPercentage', 0.7, 'start']]);
  range.disposeRangeSlider('range');
});

test('color picker previews locally and coalesces area and hue moves', async t => {
  clock(t);
  const root = element({ 'data-bb-color-picker': '' });
  const area = element(); area.parent = root;
  const hue = element(); hue.parent = root;
  const calls = [];
  const dotnet = { invokeMethodAsync: (...args) => { calls.push(args); return Promise.resolve(); } };
  color.initializeArea(area, dotnet, 'color');
  color.initializeSlider(hue, dotnet, 'color', 'hue');
  area.fire('pointerdown', { clientX: 20, clientY: 75 });
  for (let i = 0; i < 500; i++) area.fire('pointermove', { clientX: 20, clientY: 75 });
  assert.equal(root.styles.get('--bb-color-saturation'), '20%');
  assert.equal(root.styles.get('--bb-color-brightness'), '75%');
  area.fire('pointerup', { clientX: 20, clientY: 75 });
  hue.fire('pointerdown', { clientX: 75 });
  assert.equal(root.styles.get('--bb-color-hue'), '270');
  hue.fire('pointerup', { clientX: 75 });
  await microtasks();
  assert.deepEqual(calls, [['UpdateAreaFromJs', 0.2, 0.25], ['UpdateSliderFromJs', 'hue', 0.75]]);
  color.dispose('color');
  assert.equal(area.listeners.size, 0);
  assert.equal(hue.listeners.size, 0);
});

test('an older release cannot clear a new drag preview', async t => {
  const time = clock(t);
  const track = element({ 'data-min': '0', 'data-max': '100', 'data-step': '1' });
  slider.initialize(track, { invokeMethodAsync: () => Promise.resolve() }, 'overlap');
  track.fire('pointerdown', { clientX: 20 });
  track.fire('pointerup', { clientX: 25 });
  await microtasks();
  track.fire('pointerdown', { clientX: 70 });
  await time.advance(32);
  assert.equal(track.styles.get('--bb-slider-position'), '70%');
  slider.dispose('overlap');
});

test('release samples its final coordinate even without a preceding move event', async t => {
  clock(t);
  const track = element({ 'data-min': '0', 'data-max': '100', 'data-step': '1' });
  const calls = [];
  slider.initialize(track, { invokeMethodAsync: (...args) => { calls.push(args); return Promise.resolve(); } }, 'final');
  track.fire('pointerdown', { clientX: 10 });
  track.fire('pointerup', { clientX: 90 });
  await microtasks();
  assert.deepEqual(calls.at(-1), ['JsUpdateValueFromPercentage', 0.9]);
  slider.dispose('final');
});

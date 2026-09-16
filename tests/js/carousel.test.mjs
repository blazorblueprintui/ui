import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';
const source = await readFile(new URL('../../src/BlazorBlueprint.Components/wwwroot/js/carousel.js', import.meta.url), 'utf8');
const { configure, dispose, maximumStart } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

test('measured navigation stops at the final full view, including fractional slide widths', () => {
  assert.equal(maximumStart([0, 100, 200, 300], 200), 2);
  assert.equal(maximumStart([0, 80, 160, 240], 180), 3);
  assert.equal(maximumStart([0], 0), 0);
});

function surface() {
  const events = new Map(), attributes = new Map();
  return { events, style: {}, inert: false, addEventListener: (key, fn) => events.set(key, fn), removeEventListener: key => events.delete(key),
    getAttribute: key => attributes.get(key) ?? null, setAttribute: (key, value) => attributes.set(key, value), removeAttribute: key => attributes.delete(key),
    hasAttribute: key => attributes.has(key), closest: () => null };
}
function fixture(t) {
  const timers = new Map(); let id = 0;
  const previous = { setTimeout: globalThis.setTimeout, clearTimeout: globalThis.clearTimeout };
  globalThis.setTimeout = fn => { timers.set(++id, fn); return id; };
  globalThis.clearTimeout = key => timers.delete(key);
  t.after(() => Object.assign(globalThis, previous));
  const root = surface(), viewport = surface(), track = surface(), media = surface(); media.matches = false;
  globalThis.matchMedia = () => media; globalThis.getComputedStyle = () => ({ direction: 'ltr' });
  globalThis.document = surface(); globalThis.document.hidden = false; globalThis.window = surface();
  let disconnects = 0;
  globalThis.ResizeObserver = class { observe() {} disconnect() { disconnects++; } };
  globalThis.MutationObserver = class { observe() {} disconnect() { disconnects++; } };
  root.querySelector = () => viewport; viewport.querySelector = () => track;
  viewport.getBoundingClientRect = () => ({ left: 0, right: 100, top: 0, bottom: 100 });
  track.clientWidth = 100; track.scrollWidth = 300;
  track.children = [0, 1, 2].map(index => {
    const slide = surface(); slide.setAttribute('data-carousel-slide', '');
    slide.getBoundingClientRect = () => { const x = Number(track.style.transform?.match(/translateX\((-?[\d.]+)px\)/)?.[1] ?? 0); return { left: index * 100 + x, right: (index + 1) * 100 + x, top: 0, bottom: 100 }; };
    return slide;
  });
  const calls = [];
  const config = { index: 0, vertical: false, draggable: true, autoplay: true, interval: 3000, paused: false, loop: true };
  configure(root, { invokeMethodAsync: async (...args) => { calls.push(args); } }, config);
  return { root, viewport, track, media, config, timers, calls, disconnects: () => disconnects };
}

test('autoplay suspends for hover, keyboard focus, reduced motion and cleans up on disposal', async t => {
  const f = fixture(t);
  assert.equal(f.timers.size, 1);
  f.root.events.get('pointerenter')({ pointerType: 'mouse' }); assert.equal(f.timers.size, 0);
  f.root.events.get('pointerleave')(); assert.equal(f.timers.size, 1);
  f.media.matches = true; f.media.events.get('change')(); assert.equal(f.timers.size, 0);
  f.media.matches = false; f.media.events.get('change')(); assert.equal(f.timers.size, 1);
  f.root.events.get('focusin')({ target: { closest: () => null } });
  assert.equal(f.timers.size, 0); assert.ok(f.calls.some(call => call[0] === 'JsOnPause'));
  assert.equal(f.track.children[1].inert, true);
  dispose(f.root);
  assert.equal(f.timers.size, 0); assert.equal(f.disconnects(), 2); assert.equal(f.root.events.size, 0);
  assert.equal(f.track.children[1].inert, false); assert.equal(f.track.children[1].getAttribute('aria-hidden'), null);
});

test('dragging changes a single slide; cancelling leaves the active index unchanged', async t => {
  const f = fixture(t); f.calls.length = 0;
  let captures = false;
  f.viewport.setPointerCapture = () => { captures = true; }; f.viewport.hasPointerCapture = () => captures; f.viewport.releasePointerCapture = () => { captures = false; };
  const event = x => ({ button: 0, pointerId: 1, clientX: x, clientY: 0, target: { closest: () => null }, preventDefault() {} });
  f.viewport.events.get('pointerdown')(event(100)); f.viewport.events.get('pointermove')(event(30));
  assert.equal(f.track.style.transform, 'translateX(-70px)'); assert.equal(f.calls.length, 0);
  f.viewport.events.get('pointercancel')(event(30)); await Promise.resolve();
  assert.equal(f.calls.length, 0);
  f.viewport.events.get('pointerdown')(event(100)); f.viewport.events.get('pointermove')(event(30)); f.viewport.events.get('pointerup')(event(30));
  await Promise.resolve(); await Promise.resolve();
  assert.deepEqual(f.calls, [['JsOnStep', 1]]);
  dispose(f.root);
});

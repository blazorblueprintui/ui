import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';
const source = await readFile(new URL('../../src/BlazorBlueprint.Components/wwwroot/js/motion.js', import.meta.url), 'utf8');
const { motion, height, play, dispose } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

function fixture(reduced = false) {
  const preferenceHandlers = new Map();
  const media = { matches: reduced, addEventListener: (name, fn) => preferenceHandlers.set(name, fn), removeEventListener: name => preferenceHandlers.delete(name) };
  globalThis.matchMedia = () => media;
  const animations = [], handlers = new Map();
  const element = {
    style: {}, hidden: false, addEventListener: (name, fn) => handlers.set(name, fn), removeEventListener: name => handlers.delete(name),
    animate(frames, options) { let resolve; const animation = { frames, options, cancelled: false, finished: new Promise(r => { resolve = r; }), finish: () => resolve(), cancel() { this.cancelled = true; } }; animations.push(animation); return animation; },
    contains: () => false
  };
  return { element, animations, media, preferenceHandlers, handlers };
}
const config = { visible: true, preset: 'Fade', duration: 200, trigger: 'Visibility', first: false };

test('prerendered content is stable until requested; reduced motion suppresses entrance and exit animation', () => {
  const f = fixture(true);
  motion(f.element, config); play(f.element);
  assert.equal(f.animations.length, 0); assert.equal(f.element.hidden, false);
  motion(f.element, { ...config, visible: false });
  assert.equal(f.element.hidden, true); assert.equal(f.animations.length, 0);
  dispose(f.element); assert.equal(f.preferenceHandlers.size, 0);
});

test('an interrupted exit cannot hide content that has re-entered', async () => {
  const f = fixture(); motion(f.element, config);
  assert.equal(f.animations.length, 0);
  motion(f.element, { ...config, visible: false });
  const exit = f.animations[0]; assert.equal(f.element.hidden, false);
  motion(f.element, config);
  assert.equal(exit.cancelled, true);
  exit.finish(); await Promise.resolve();
  assert.equal(f.element.hidden, false);
  f.animations[1].finish(); await Promise.resolve();
  assert.equal(f.element.hidden, false);
  dispose(f.element);
});

test('switching reduced-motion preference cancels active animations and releases trigger handlers', () => {
  const f = fixture(); motion(f.element, { ...config, trigger: 'Hover' });
  f.handlers.get('pointerenter')(); assert.equal(f.animations.length, 1);
  f.media.matches = true; f.preferenceHandlers.get('change')();
  assert.equal(f.animations[0].cancelled, true);
  dispose(f.element); assert.equal(f.handlers.size, 0); assert.equal(f.preferenceHandlers.size, 0);
});

test('dynamic height returns to auto, collapse hides only after animation and teardown disconnects resize', async () => {
  const f = fixture(); let size = 100, resizeCallback, disconnected = false;
  globalThis.ResizeObserver = class { constructor(fn) { resizeCallback = fn; } observe() {} disconnect() { disconnected = true; } };
  f.element.firstElementChild = { getBoundingClientRect: () => ({ height: size }) };
  f.element.getBoundingClientRect = () => ({ height: size });
  const options = { expanded: true, enabled: true, duration: 200 };
  height(f.element, options); assert.equal(f.element.style.height, 'auto');
  size = 180; resizeCallback(); assert.deepEqual(f.animations[0].frames, [{ height: '100px' }, { height: '180px' }]);
  f.animations[0].finish(); await Promise.resolve(); assert.equal(f.element.style.height, 'auto');
  height(f.element, { ...options, expanded: false }); assert.equal(f.element.hidden, false);
  f.animations[1].finish(); await Promise.resolve(); assert.equal(f.element.hidden, true);
  dispose(f.element); assert.equal(disconnected, true);
});

import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';
const source = await readFile(new URL('../../src/BlazorBlueprint.Components/wwwroot/js/motion.js', import.meta.url), 'utf8');
const { motion, height, play, dispose, transition } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

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

for (const trigger of ['Manual', 'Hover', 'Press', 'InView']) {
  test(`${trigger} does not animate visibility changes`, () => {
    const f = fixture();
    motion(f.element, { ...config, trigger });
    motion(f.element, { ...config, trigger, visible: false });
    assert.equal(f.element.hidden, true);
    motion(f.element, { ...config, trigger });
    assert.equal(f.element.hidden, false);
    assert.equal(f.animations.length, 0);
    play(f.element);
    assert.equal(f.animations.length, 1);
    dispose(f.element);
  });
}

const dialog = { preset: 'SlideUp', duration: 200, easing: 'ease-out', distance: 16, exiting: false };

test('a dialog exit plays the preset backwards and holds its last frame until the dialog is removed', async () => {
  const f = fixture();
  const exit = transition(f.element, { ...dialog, exiting: true });
  assert.deepEqual(f.animations[0].frames[0], { opacity: 1, transform: 'translate(0, 0)' });
  assert.deepEqual(f.animations[0].frames[1], { opacity: 0, transform: 'translate(0px, 16px)' });
  f.animations[0].finish(); await exit;
  assert.equal(f.animations[0].cancelled, false);
});

test('a dialog reopened mid-exit or after it drops the exit, and the enter lets go when done', async () => {
  const f = fixture();
  const exit = transition(f.element, { ...dialog, exiting: true });
  const enter = transition(f.element, dialog);
  await exit;
  assert.equal(f.animations[0].cancelled, true);
  f.animations[1].finish(); await enter;
  assert.equal(f.animations[1].cancelled, true);

  const held = transition(f.element, { ...dialog, exiting: true });
  f.animations[2].finish(); await held;
  await transition(f.element, { ...dialog, duration: 0 });
  assert.equal(f.animations[2].cancelled, true);
});

test('custom dialog exit keyframes play as written, and reduced motion skips dialog animation', async () => {
  const f = fixture();
  const keyframes = [{ opacity: 1 }, { opacity: 0, transform: 'translateX(40px)' }];
  const exit = transition(f.element, { ...dialog, exiting: true, keyframes });
  assert.deepEqual(f.animations[0].frames, keyframes);
  f.animations[0].finish(); await exit;

  const reduced = fixture(true);
  await transition(reduced.element, { ...dialog, exiting: true });
  assert.equal(reduced.animations.length, 0);
});

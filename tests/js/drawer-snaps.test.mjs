import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';
const source = await readFile(new URL('../../src/BlazorBlueprint.Components/wwwroot/js/drawer-snaps.js', import.meta.url), 'utf8');
const { configure, dispose, nearestSnap } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

test('snap selection respects dismissal configuration and bounds', () => {
  assert.equal(nearestSnap([0.3, 0.6, 0.9], 0.12, true), -1);
  assert.equal(nearestSnap([0.3, 0.6, 0.9], 0.12, false), 0);
  assert.equal(nearestSnap([0.3, 0.6, 0.9], 0.58, true), 1);
  assert.equal(nearestSnap([0.3, 0.6, 0.9], 1, false), 2);
});

test('pointer previews stay local until release; cancellation sends no callback', async () => {
  const handlers = new Map(); const calls = []; let captured = false;
  const handle = {
    addEventListener: (name, listener) => handlers.set(name, listener), removeEventListener: name => handlers.delete(name),
    setPointerCapture: () => { captured = true; }, hasPointerCapture: () => captured,
    releasePointerCapture: () => { captured = false; }, focus() {}
  };
  globalThis.window = { innerHeight: 1000, innerWidth: 500, addEventListener() {}, removeEventListener() {} };
  const element = { querySelector: () => handle, style: {}, getBoundingClientRect: () => ({ height: 300, width: 500 }) };
  configure(element, { invokeMethodAsync: async (...args) => calls.push(args) }, { direction: 'bottom', points: [0.3, 0.6, 0.9], index: 0, dismiss: true });
  const event = y => ({ button: 0, pointerId: 1, clientY: y, preventDefault() {} });
  handlers.get('pointerdown')(event(700)); handlers.get('pointermove')(event(420));
  assert.equal(element.style.height, '580px'); assert.deepEqual(calls, []);
  handlers.get('pointercancel')();
  assert.equal(element.style.height, '300px'); assert.deepEqual(calls, []);
  handlers.get('pointerdown')(event(700)); handlers.get('pointermove')(event(410)); handlers.get('pointerup')(event(410));
  await Promise.resolve();
  assert.deepEqual(calls, [['JsOnSnapChanged', 1]]);
  assert.equal(element.style.height, '600px');
  dispose(element); assert.equal(handlers.size, 0); assert.equal(captured, false);
});

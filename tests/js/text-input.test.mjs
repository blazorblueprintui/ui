import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

const base = '../../src/BlazorBlueprint.Components/wwwroot/js/';
const guard = await readFile(new URL(`${base}composition-guard.js`, import.meta.url), 'utf8');
const source = (await readFile(new URL(`${base}text-input.js`, import.meta.url), 'utf8'))
  .replace("'./composition-guard.js?assets=2'", `'data:text/javascript;base64,${Buffer.from(guard).toString('base64')}'`);
const { initialize, updateConfig, dispose } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

function fixture(t, config) {
  const frames = [];
  globalThis.requestAnimationFrame = fn => frames.push(fn);
  globalThis.cancelAnimationFrame = () => {};
  t.after(() => {
    delete globalThis.requestAnimationFrame;
    delete globalThis.cancelAnimationFrame;
  });
  const listeners = new Map();
  const element = {
    value: '',
    addEventListener: (name, handler) => listeners.set(name, handler),
    removeEventListener: name => listeners.delete(name)
  };
  const calls = [];
  const dotNetRef = { invokeMethodAsync: (method, value) => { calls.push([method, value]); return Promise.resolve(); } };
  const id = `text-input-${Math.random()}`;
  initialize(element, dotNetRef, id, config);
  t.after(() => dispose(id));
  const type = value => {
    element.value = value;
    listeners.get('input')();
  };
  const flushFrames = () => frames.splice(0).forEach(fn => fn());
  return { id, type, calls, flushFrames };
}

test('updateConfig from onchange to immediate raises JsOnInput while typing', t => {
  const { id, type, calls, flushFrames } = fixture(t, { mode: 'onchange', debounceMs: 0 });
  type('a');
  flushFrames();
  assert.deepEqual(calls, []);

  updateConfig(id, { mode: 'immediate', debounceMs: 0 });
  type('ab');
  flushFrames();
  assert.deepEqual(calls, [['JsOnInput', 'ab']]);
});

test('updateConfig from immediate to onchange stops JsOnInput while typing', t => {
  const { id, type, calls, flushFrames } = fixture(t, { mode: 'immediate', debounceMs: 0 });
  updateConfig(id, { mode: 'onchange', debounceMs: 0 });
  type('a');
  flushFrames();
  assert.deepEqual(calls, []);
});

test('updateConfig changes the debounce interval', t => {
  t.mock.timers.enable({ apis: ['setTimeout'] });
  const { id, type, calls } = fixture(t, { mode: 'debounced', debounceMs: 500 });
  updateConfig(id, { mode: 'debounced', debounceMs: 100 });
  type('a');
  t.mock.timers.tick(100);
  assert.deepEqual(calls, [['JsOnInput', 'a']]);
});

import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../../src/BlazorBlueprint.Components/wwwroot/js/segmented-input.js', import.meta.url), 'utf8');
const { initialize, dispose } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

function fixture(t, direction = 'ltr') {
  const original = globalThis.getComputedStyle;
  globalThis.getComputedStyle = () => ({ direction });
  t.after(() => { globalThis.getComputedStyle = original; });
  const listeners = new Map();
  const inputs = Array.from({ length: 3 }, () => ({
    value: '15', dataset: { min: '0', max: '59', digits: '2', step: '15' },
    disabled: false, readOnly: false, selected: false, focused: false, events: [],
    matches: () => true,
    select() { this.selected = true; },
    focus() { this.focused = true; },
    dispatchEvent(e) { this.events.push(e); }
  }));
  const root = {
    querySelectorAll: () => inputs.filter(i => !i.disabled),
    addEventListener: (name, handler) => listeners.set(name, handler),
    removeEventListener: name => listeners.delete(name)
  };
  initialize(root);
  const key = (input, key, options = {}) => {
    const event = { target: input, key, prevented: false, preventDefault() { this.prevented = true; }, ...options };
    listeners.get('keydown')?.(event);
    return event;
  };
  return { root, inputs, listeners, key };
}

test('spin keys apply increments and clamp to the segment bounds', t => {
  const { inputs: [input], key } = fixture(t);
  key(input, 'ArrowUp');
  assert.equal(input.value, '30');
  key(input, 'End');
  key(input, 'ArrowUp');
  assert.equal(input.value, '59');
  key(input, 'Home');
  key(input, 'ArrowDown');
  assert.equal(input.value, '00');
  assert.equal(input.events.length, 5);
  assert.ok(input.events.every(e => e.type === 'input' && e.bubbles));
});

test('horizontal arrows navigate in visual order and preserve native shortcuts', t => {
  const { inputs, key } = fixture(t, 'rtl');
  assert.equal(key(inputs[0], 'ArrowLeft').prevented, true);
  assert.equal(inputs[1].focused, true);
  assert.equal(inputs[1].selected, true);
  assert.equal(key(inputs[0], 'Tab').prevented, false);
  assert.equal(key(inputs[0], 'ArrowLeft', { shiftKey: true }).prevented, false);
});

test('disabled, readonly and composing segments are not changed; disposal removes handlers', t => {
  const { root, inputs: [input], key, listeners } = fixture(t);
  input.readOnly = true;
  key(input, 'ArrowUp');
  input.readOnly = false;
  input.disabled = true;
  key(input, 'ArrowUp');
  input.disabled = false;
  key(input, 'ArrowUp', { isComposing: true });
  assert.equal(input.value, '15');
  assert.equal(input.events.length, 0);
  dispose(root);
  assert.equal(listeners.size, 0);
});

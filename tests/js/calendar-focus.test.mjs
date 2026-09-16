import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../../src/BlazorBlueprint.Primitives/wwwroot/js/primitives/element-utils.js', import.meta.url), 'utf8');
globalThis.document = { addEventListener: () => {} };
const { focusElement } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

test('calendar focus waits for its popup to become visible', async () => {
  let visible = false;
  let focused = false;
  const element = { isConnected: true, getClientRects: () => [1], focus: () => { focused = true; } };
  globalThis.document = { getElementById: () => element };
  globalThis.window = { getComputedStyle: () => ({ visibility: visible ? 'visible' : 'hidden', display: 'block' }) };
  globalThis.requestAnimationFrame = callback => { assert.equal(focused, false); visible = true; callback(); };
  await focusElement('calendar-day');
  assert.equal(focused, true);
});

test('a calendar closed before reveal does not steal focus', async () => {
  let focused = false;
  const element = { isConnected: true, focus: () => { focused = true; } };
  globalThis.document = { getElementById: () => element };
  globalThis.window = { getComputedStyle: () => ({ visibility: 'hidden', display: 'block' }) };
  globalThis.requestAnimationFrame = callback => { element.isConnected = false; callback(); };
  await focusElement('calendar-day');
  assert.equal(focused, false);
});

import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../../src/BlazorBlueprint.Primitives/wwwroot/js/primitives/table-row-nav.js', import.meta.url), 'utf8');
const { blurFocusedInput, preventSpaceKeyScroll } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

test('editing rows leave arrow keys available to select and date controls', () => {
  const handlers = [];
  const control = { closest: () => control };
  const row = {
    dataset: { editing: 'true' }, contains: element => element === control,
    addEventListener: (_, handler) => handlers.push(handler), removeEventListener() {}
  };
  const cleanup = preventSpaceKeyScroll(row);
  let stopped = false;
  const event = { target: control, key: 'ArrowDown', stopPropagation: () => { stopped = true; }, preventDefault: () => assert.fail('The editor owns its arrow keys') };
  handlers.forEach(handler => handler(event));
  assert.equal(stopped, false);
  row.dataset.editing = 'false';
  handlers.forEach(handler => handler(event));
  assert.equal(stopped, true);
  cleanup.dispose();
});

test('Enter flushes a text or numeric editor before saving', () => {
  let blurred = false;
  const input = { closest: () => null, blur: () => { blurred = true; } };
  globalThis.document = { activeElement: input, body: {} };
  assert.equal(blurFocusedInput({ contains: element => element === input }), true);
  assert.equal(blurred, true);
});

test('Enter on picker triggers or Save/Cancel buttons does not also commit the row', () => {
  for (const control of ['button', 'select', '[role="combobox"]', '[role="checkbox"]']) {
    const input = { closest: selector => selector.includes(control) ? input : null, blur: () => assert.fail('Control must retain its activation behavior') };
    globalThis.document = { activeElement: input, body: {} };
    assert.equal(blurFocusedInput({ contains: () => true }), false);
  }
});

test('Enter inside a portalled date or select popup does not commit the owning row', () => {
  globalThis.document = { activeElement: { blur: () => assert.fail('Popup must retain focus') }, body: {} };
  assert.equal(blurFocusedInput({ contains: () => false }), false);
});

import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';
const base = '../../src/BlazorBlueprint.Primitives/wwwroot/js/';
const theme = await readFile(new URL(`${base}theme-scope.js`, import.meta.url), 'utf8');
const source = (await readFile(new URL(`${base}sortable-overlay.js`, import.meta.url), 'utf8')).replace("'./theme-scope.js'", `'data:text/javascript;base64,${Buffer.from(theme).toString('base64')}'`);
const { createDragOverlay } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

test('drag preview is inert, stripped of duplicate IDs, kept on screen and fully removed', () => {
  const handlers = new Map(); let removed = false, appended = false, duplicateRemoved = false;
  const attributes = new Map([['id', 'preview']]);
  const overlay = { style: {}, hidden: true, inert: false, offsetWidth: 80, offsetHeight: 40,
    setAttribute: (name, value) => attributes.set(name, value), removeAttribute: name => attributes.delete(name),
    querySelectorAll: () => [{ removeAttribute: () => { duplicateRemoved = true; } }], remove: () => { removed = true; } };
  const item = { querySelector: () => ({ cloneNode: () => overlay }) };
  globalThis.document = { body: { appendChild: () => { appended = true; } }, addEventListener: (name, fn) => handlers.set(name, fn), removeEventListener: name => handlers.delete(name) };
  globalThis.window = { innerWidth: 300, innerHeight: 200 };
  const cleanup = createDragOverlay(item, { clientX: 290, clientY: 190 });
  assert.equal(appended, true); assert.equal(overlay.hidden, false); assert.equal(overlay.inert, true);
  assert.equal(attributes.has('id'), false); assert.equal(duplicateRemoved, true);
  assert.equal(overlay.style.transform, 'translate(212px, 152px)');
  assert.equal(handlers.size, 2);
  cleanup(); assert.equal(removed, true); assert.equal(handlers.size, 0);
});

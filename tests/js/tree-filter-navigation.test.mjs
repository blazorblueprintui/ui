import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../../src/BlazorBlueprint.Primitives/wwwroot/js/primitives/tree-keyboard.js', import.meta.url), 'utf8');
const { initialize, dispose } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

test('filtered cascade nodes are skipped by keyboard navigation and keep a visible tab stop', () => {
  let observer;
  let disconnected = false;
  globalThis.MutationObserver = class {
    constructor(callback) { observer = callback; }
    observe() {}
    disconnect() { disconnected = true; }
  };
  let focused;
  class Node {
    hidden = false;
    attributes = new Map([['role', 'treeitem'], ['tabindex', '-1']]);
    constructor(value, parentElement) { this.parentElement = parentElement; this.attributes.set('data-value', value); }
    getAttribute(name) { return this.attributes.get(name) ?? null; }
    setAttribute(name, value) { this.attributes.set(name, value); }
    closest(selector) { return selector === '[role="treeitem"]' ? this : null; }
    focus() { focused = this; }
  }
  globalThis.Element = Node;
  const handlers = new Map();
  const container = { querySelectorAll: () => nodes, addEventListener: (name, fn) => handlers.set(name, fn), removeEventListener: name => handlers.delete(name) };
  const first = new Node('first', container);
  const hidden = new Node('hidden', container);
  hidden.hidden = true;
  const hiddenChild = new Node('hidden-child', hidden);
  const last = new Node('last', container);
  const nodes = [first, hidden, hiddenChild, last];
  initialize(container, { invokeMethodAsync: async () => {} }, 'filtered-tree');
  assert.equal(first.getAttribute('tabindex'), '0');
  handlers.get('keydown')({ target: first, key: 'ArrowDown', preventDefault() {} });
  assert.equal(focused, last);
  last.hidden = true;
  observer();
  assert.equal(first.getAttribute('tabindex'), '0');
  assert.equal(last.getAttribute('tabindex'), '-1');
  dispose('filtered-tree');
  assert.equal(disconnected, true);
  assert.equal(handlers.size, 0);
});

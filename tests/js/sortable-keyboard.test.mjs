import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../../src/BlazorBlueprint.Primitives/wwwroot/js/sortable-keyboard.js', import.meta.url), 'utf8');
const { attachKeyboardSorting } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

function setup(t, reject = false, transfer = null) {
  const saved = Object.fromEntries(['document', 'MutationObserver', 'getComputedStyle'].map(k => [k, globalThis[k]]));
  const region = { textContent: '' };
  globalThis.document = { getElementById: () => region };
  globalThis.MutationObserver = class { observe() {} disconnect() {} };
  globalThis.getComputedStyle = () => ({ direction: 'ltr' });
  t.after(() => Object.assign(globalThis, saved));
  const handlers = new Map();
  const root = {
    id: 'tasks', children: [], dataset: { keyboardSorting: 'true' },
    addEventListener: (name, callback) => handlers.set(name, callback),
    removeEventListener: name => handlers.delete(name),
    appendChild(child) { this.children = this.children.filter(n => n !== child); this.children.push(child); },
    insertBefore(child, next) {
      this.children = this.children.filter(n => n !== child);
      const index = next == null ? this.children.length : this.children.indexOf(next);
      this.children.splice(index, 0, child);
    }
  };
  const nodes = ['A', 'B', 'C'].map(name => {
    const attributes = new Map([['data-bb-sortable-item', '']]);
    return {
      name, dataset: {}, parentElement: root, isConnected: true, focused: false,
      hasAttribute: name => attributes.has(name), getAttribute: name => attributes.get(name) ?? null,
      setAttribute: (name, value) => attributes.set(name, value), removeAttribute: name => attributes.delete(name),
      contains(target) { return target === this; }, closest() { return this; },
      get nextSibling() { return root.children[root.children.indexOf(this) + 1] ?? null; },
      focus() { this.focused = true; }
    };
  });
  root.children = [...nodes];
  const calls = [];
  const cleanup = attachKeyboardSorting(root, '', null, async (oldIndex, newIndex) => {
    assert.deepEqual(root.children.map(n => n.name), ['A', 'B', 'C'], 'restore Blazor DOM before committing');
    calls.push([oldIndex, newIndex]);
    if (reject) throw new Error('save rejected');
  }, transfer);
  const key = (key, options = {}) => handlers.get('keydown')({ target: nodes[1], key, preventDefault() {}, ...options });
  return { root, nodes, calls, key, cleanup, handlers, region };
}

test('keyboard preview commits exactly once and restores the DOM before invoking Blazor', async t => {
  const { root, nodes, key, calls } = setup(t);
  key(' '); key('ArrowDown');
  assert.deepEqual(root.children.map(n => n.name), ['A', 'C', 'B']);
  assert.deepEqual(calls, []);
  key('Enter');
  await Promise.resolve(); await Promise.resolve();
  assert.deepEqual(calls, [[1, 2]]);
  assert.ok(nodes[1].focused);
});

test('Escape cancels a preview without changing the collection', t => {
  const { root, key, calls, region } = setup(t);
  key(' '); key('Home');
  assert.deepEqual(root.children.map(n => n.name), ['B', 'A', 'C']);
  key('Escape');
  assert.deepEqual(root.children.map(n => n.name), ['A', 'B', 'C']);
  assert.deepEqual(calls, []);
  assert.equal(region.textContent, 'Move cancelled.');
});

test('failed commits restore focus and disposal restores author attributes', async t => {
  const { nodes, key, cleanup, calls, handlers, region } = setup(t, true);
  assert.equal(nodes[1].getAttribute('tabindex'), '0');
  key(' '); key('End'); key('Enter');
  await Promise.resolve(); await Promise.resolve();
  assert.equal(calls.length, 1);
  assert.ok(nodes[1].focused);
  assert.match(region.textContent, /Unable to move/);
  cleanup();
  assert.equal(nodes[1].getAttribute('tabindex'), null);
  assert.equal(handlers.size, 0);
});


test('Control plus arrow transfers only after restoring the source preview', async t => {
  const transfers = [];
  const view = setup(t, false, async (index, direction) => {
    assert.deepEqual(view.root.children.map(n => n.name), ['A', 'B', 'C']);
    transfers.push([index, direction]);
    return true;
  });
  view.key(' '); view.key('Home'); view.key('ArrowRight', { ctrlKey: true });
  await Promise.resolve(); await Promise.resolve();
  assert.deepEqual(transfers, [[1, 1]]); assert.deepEqual(view.calls, []);
  assert.match(view.region.textContent, /transferred/);
  view.cleanup();
});

test('a transfer rejection restores focus; non-sortable sources still allow transfers', async t => {
  const view = setup(t, false, async () => false);
  view.root.dataset.sortableOrder = 'false';
  view.key(' '); view.key('ArrowDown');
  assert.deepEqual(view.root.children.map(n => n.name), ['A', 'B', 'C']);
  view.key('ArrowRight', { ctrlKey: true });
  await Promise.resolve(); await Promise.resolve();
  assert.ok(view.nodes[1].focused); assert.match(view.region.textContent, /not allowed/);
  view.cleanup();
});

import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../../src/BlazorBlueprint.Primitives/wwwroot/js/primitives/tree-keyboard.js', import.meta.url), 'utf8');
const { initialize, dispose } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);
let serial = 0;

function tree({ picker = true, checkable = false, branch = true, disabled = false } = {}) {
  const handlers = new Map(), calls = [];
  class ElementStub {
    constructor(attributes) { this.attributes = new Map(Object.entries(attributes)); }
    getAttribute(name) { return this.attributes.get(name) ?? null; }
    setAttribute(name, value) { this.attributes.set(name, value); }
    closest(selector) { return selector === '[role="treeitem"]' ? this : null; }
    querySelectorAll() { return []; }
    contains(element) { return element === this; }
    focus() { document.activeElement = this; }
  }
  globalThis.Element = ElementStub;
  globalThis.MutationObserver = class { observe() {} disconnect() {} };
  const container = new ElementStub({ 'data-tree-select': picker ? 'true' : 'false' });
  const item = new ElementStub({ role: 'treeitem', 'data-value': 'node', tabindex: '0',
    'data-has-children': String(branch), 'aria-disabled': String(disabled),
    ...(branch ? { 'aria-expanded': 'false' } : {}), ...(checkable ? { 'aria-checked': 'false' } : {}) });
  item.parentElement = container;
  container.querySelectorAll = () => [item];
  container.addEventListener = (name, handler) => handlers.set(name, handler);
  container.removeEventListener = name => handlers.delete(name);
  globalThis.document = { activeElement: item };
  const id = `tree-${serial++}`;
  initialize(container, { invokeMethodAsync: async (...args) => {
    calls.push(args);
    if (args[0] === 'JsOnNodeExpand') item.setAttribute('aria-expanded', 'true');
    if (args[0] === 'JsOnNodeCollapse') item.setAttribute('aria-expanded', 'false');
    if (args[0] === 'JsOnNodeCheck') item.setAttribute('aria-checked', String(item.getAttribute('aria-checked') !== 'true'));
  } }, id);
  return { item, container, calls,
    key(key, target = item) {
      const event = { key, target, preventDefault() { this.prevented = true; } };
      handlers.get('keydown')(event);
      return event;
    },
    end: () => dispose(id)
  };
}

test('TreeSelect Space toggles branches without selecting or checking and retains focus', () => {
  for (const checkable of [false, true]) {
    const t = tree({ checkable });
    assert.equal(t.key(' ').prevented, true);
    assert.equal(t.item.getAttribute('aria-expanded'), 'true');
    t.key(' ');
    assert.equal(t.item.getAttribute('aria-expanded'), 'false');
    assert.deepEqual(t.calls, [['JsOnNodeExpand', 'node'], ['JsOnNodeCollapse', 'node']]);
    assert.equal(document.activeElement, t.item);
    t.end();
  }
});

test('TreeSelect Space on a leaf prevents scrolling without changing selection', () => {
  const t = tree({ branch: false, checkable: true });
  assert.equal(t.key(' ').prevented, true);
  assert.deepEqual(t.calls, []);
  t.end();
});

test('TreeSelect Enter selects single nodes and toggles checkbox nodes', () => {
  const single = tree();
  single.key('Enter');
  assert.deepEqual(single.calls, [['JsOnNodeActivate', 'node', true]]);
  single.end();
  const multiple = tree({ checkable: true });
  multiple.key('Enter');
  assert.equal(multiple.item.getAttribute('aria-checked'), 'true');
  multiple.key('Enter');
  assert.equal(multiple.item.getAttribute('aria-checked'), 'false');
  assert.deepEqual(multiple.calls, [['JsOnNodeCheck', 'node'], ['JsOnNodeCheck', 'node']]);
  multiple.end();
});

test('TreeView retains Space selection/checking and Enter activation', () => {
  for (const checkable of [false, true]) {
    const t = tree({ picker: false, checkable });
    t.key(' '); t.key('Enter');
    assert.deepEqual(t.calls, [checkable ? ['JsOnNodeCheck', 'node'] : ['JsOnNodeActivate', 'node', true], ['JsOnNodeActivate', 'node', true]]);
    t.end();
  }
});

test('TreeSelect leaves disabled nodes and interactive children unchanged', () => {
  const disabled = tree({ disabled: true });
  disabled.key(' '); disabled.key('Enter');
  assert.deepEqual(disabled.calls, []); disabled.end();
  const t = tree();
  const input = new Element({});
  input.closest = selector => selector === '[role="treeitem"]' ? t.item : input;
  t.item.contains = element => element === input;
  assert.equal(t.key(' ', input).prevented, undefined);
  assert.equal(t.key('Enter', input).prevented, undefined);
  assert.deepEqual(t.calls, []);
  t.end();
});

import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../../src/BlazorBlueprint.Primitives/wwwroot/js/menu-triggers.js', import.meta.url), 'utf8');
const { initialize, dispose } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

function setup(t, navigation = false) {
  const previousDocument = Object.getOwnPropertyDescriptor(globalThis, 'document');
  const previousObserver = Object.getOwnPropertyDescriptor(globalThis, 'MutationObserver');
  const listeners = new Map();
  const elements = new Map();
  const calls = [];
  function element(parent = null, attrs = {}) {
    const el = {
      parent,
      contains(target) {
        for (; target; target = target.parent) if (target === el) return true;
        return false;
      },
      getAttribute: name => attrs[name] ?? null,
      hasAttribute: name => name in attrs,
      closest(selector) {
        for (let current = el; current; current = current.parent) {
          if (selector === '[role="menubar"]' && current === root) return root;
          if (selector === '[role="menu"][aria-labelledby]' && current.getAttribute('role') === 'menu' && current.hasAttribute('aria-labelledby')) return current;
        }
        return null;
      },
      addEventListener() {}, removeEventListener() {}
    };
    if (attrs.id) elements.set(attrs.id, el);
    return el;
  }
  const root = element();
  const trigger = element(root, { 'aria-expanded': 'true' });
  trigger.click = () => calls.push('close');
  root.querySelectorAll = () => [trigger];
  globalThis.document = {
    getElementById: id => elements.get(id) ?? null,
    addEventListener: (name, callback) => listeners.set(name, callback),
    removeEventListener: name => listeners.delete(name)
  };
  globalThis.MutationObserver = class { observe() {} disconnect() {} };
  initialize(root, navigation, true, { invokeMethodAsync: async name => calls.push(name) });
  t.after(() => {
    dispose(root);
    if (previousDocument) Object.defineProperty(globalThis, 'document', previousDocument);
    else delete globalThis.document;
    if (previousObserver) Object.defineProperty(globalThis, 'MutationObserver', previousObserver);
    else delete globalThis.MutationObserver;
  });
  function submenu(parent) {
    const id = `sub-${elements.size}`;
    const trigger = element(parent, { id, 'data-bb-submenu-trigger': '' });
    const panel = element(null, { role: 'menu', 'aria-labelledby': id });
    return { trigger, panel, item: element(panel) };
  }
  return { root, element, submenu, calls, listeners,
    pointerdown: target => listeners.get('pointerdown')({ target }),
    end: () => dispose(root) };
}

test('pointerdown in a portaled submenu leaves its menubar open', t => {
  const m = setup(t);
  const sub = m.submenu(m.root);
  m.pointerdown(sub.panel);
  m.pointerdown(sub.item);
  assert.deepEqual(m.calls, []);
  m.end();
});

test('pointerdown in a nested portaled submenu leaves its menubar open', t => {
  const m = setup(t);
  const sub = m.submenu(m.root);
  const nested = m.submenu(sub.panel);
  m.pointerdown(nested.item);
  assert.deepEqual(m.calls, []);
  m.end();
});

test('only submenus owned by this menubar count as inside', t => {
  const m = setup(t);
  m.pointerdown(m.element(m.root));
  assert.deepEqual(m.calls, []);
  m.pointerdown(m.submenu(m.element()).item);
  m.pointerdown(m.element());
  assert.deepEqual(m.calls, ['close', 'close']);
  m.end();
  assert.equal(m.listeners.has('pointerdown'), false);
});

test('non-Element pointer targets dismiss without throwing', t => {
  const m = setup(t);
  m.pointerdown(document);
  assert.deepEqual(m.calls, ['close']);
  m.end();
});

test('navigation menu outside dismissal is unchanged', t => {
  const m = setup(t, true);
  m.pointerdown(m.submenu(m.root).item);
  assert.deepEqual(m.calls, ['JsCloseMenus']);
  m.end();
});

import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../../src/BlazorBlueprint.Primitives/wwwroot/js/menu-triggers.js', import.meta.url), 'utf8');
const { initialize, dispose } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

function menubar(direction = 'ltr', navigation = false, focusEvents = true) {
  const savedGlobals = Object.fromEntries(['MutationObserver', 'document', 'getComputedStyle']
    .map(name => [name, Object.getOwnPropertyDescriptor(globalThis, name)]));
  const handlers = new Map();
  const items = [];
  const root = {
    querySelectorAll: () => items,
    contains: item => items.includes(item),
    addEventListener: (name, handler) => handlers.set(name, handler),
    removeEventListener: name => handlers.delete(name)
  };
  let mutate;
  let disconnected = false;
  globalThis.MutationObserver = class {
    constructor(callback) { mutate = callback; }
    observe() {}
    disconnect() { disconnected = true; }
  };
  globalThis.document = { activeElement: null, addEventListener() {}, removeEventListener() {} };
  globalThis.getComputedStyle = () => ({ direction });
  function item(label) {
    const attrs = new Map();
    const entry = {
      label, disabled: false,
      get tabIndex() { return attrs.has('tabindex') ? Number(attrs.get('tabindex')) : 0; },
      getAttribute: name => attrs.get(name) ?? null,
      setAttribute: (name, value) => attrs.set(name, String(value)),
      closest: selector => ['button[aria-haspopup="menu"]', '[data-nav-trigger]'].includes(selector) ? entry
        : ['nav', '[role="menu"], [role="menubar"]'].includes(selector) ? root : null,
      focus() {
        document.activeElement = entry;
        if (focusEvents) handlers.get('focusin')?.({ target: entry });
      },
      click() {}
    };
    items.push(entry);
    return entry;
  }
  return {
    root, items, item,
    start: () => initialize(root, navigation),
    mutate: () => mutate(),
    key(key) {
      const event = { key, target: document.activeElement, preventDefault() { this.prevented = true; } };
      handlers.get('keydown')(event);
      return event;
    },
    end() {
      dispose(root);
      for (const [name, descriptor] of Object.entries(savedGlobals)) {
        if (descriptor) Object.defineProperty(globalThis, name, descriptor);
        else delete globalThis[name];
      }
      assert.equal(disconnected, true);
      assert.equal(handlers.size, 0);
    }
  };
}

test('menubar has one Tab stop that follows arrows, Home/End and pointer focus in LTR and RTL', () => {
  for (const direction of ['ltr', 'rtl']) {
    const m = menubar(direction);
    const file = m.item('File'), edit = m.item('Edit'), view = m.item('View');
    m.start();
    assert.deepEqual(m.items.map(item => item.tabIndex), [0, -1, -1]);
    file.focus();
    m.key(direction === 'rtl' ? 'ArrowLeft' : 'ArrowRight');
    assert.equal(document.activeElement, edit);
    assert.deepEqual(m.items.map(item => item.tabIndex), [-1, 0, -1]);
    m.key('End');
    assert.equal(document.activeElement, view);
    assert.deepEqual(m.items.map(item => item.tabIndex), [-1, -1, 0]);
    m.key('Home');
    assert.equal(document.activeElement, file);
    m.key(direction === 'rtl' ? 'ArrowRight' : 'ArrowLeft');
    assert.equal(document.activeElement, view);
    file.focus();
    assert.deepEqual(m.items.map(item => item.tabIndex), [0, -1, -1]);
    view.focus();
    assert.deepEqual(m.items.map(item => item.tabIndex), [-1, -1, 0]);
    assert.equal(m.key('Tab').prevented, undefined);
    document.activeElement = null;
    m.start();
    assert.deepEqual(m.items.map(item => item.tabIndex), [-1, -1, 0]);
    m.end();
  }
});

test('menubar keeps one entry point when triggers change and excludes nested menus', () => {
  const m = menubar();
  const file = m.item('File'), edit = m.item('Edit');
  const nested = m.item('Nested');
  nested.closest = () => ({});
  nested.setAttribute('tabindex', '-1');
  m.start();
  edit.focus();
  const view = m.item('View');
  m.mutate();
  assert.deepEqual(m.items.map(item => item.tabIndex), [-1, 0, -1, -1]);
  document.activeElement = nested;
  m.mutate();
  assert.equal(edit.tabIndex, 0);
  edit.disabled = true;
  m.mutate();
  assert.deepEqual(m.items.map(item => item.tabIndex), [0, -1, -1, -1]);
  view.focus();
  m.items.splice(m.items.indexOf(view), 1);
  document.activeElement = null;
  m.mutate();
  assert.equal(file.tabIndex, 0);
  file.disabled = true;
  m.mutate();
  assert.deepEqual(m.items.map(item => item.tabIndex), [-1, -1, -1]);
  edit.disabled = false;
  m.mutate();
  assert.equal(edit.tabIndex, 0);
  m.end();
});

test('navigation menus retain their existing Tab stops', () => {
  const m = menubar('ltr', true);
  m.item('File'); m.item('Edit');
  m.start();
  m.items[0].focus();
  m.key('ArrowRight');
  assert.equal(document.activeElement, m.items[1]);
  m.mutate();
  assert.deepEqual(m.items.map(item => item.getAttribute('tabindex')), [null, null]);
  m.end();
});

test('arrow navigation updates the entry point even when the document is in the background', () => {
  const m = menubar('ltr', false, false);
  m.item('File'); m.item('Edit');
  m.start();
  m.items[0].focus();
  m.key('ArrowRight');
  assert.deepEqual(m.items.map(item => item.tabIndex), [-1, 0]);
  m.key('Home');
  assert.deepEqual(m.items.map(item => item.tabIndex), [0, -1]);
  m.end();
});

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
      setAttribute: (name, value) => { attrs[name] = String(value); },
      hasAttribute: name => name in attrs,
      closest(selector) {
        for (let current = el; current; current = current.parent) {
          if (['[role="menubar"]', '[role="menu"], [role="menubar"]'].includes(selector) && current === root) return root;
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

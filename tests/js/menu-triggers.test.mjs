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

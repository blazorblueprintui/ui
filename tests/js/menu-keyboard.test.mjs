import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../../src/BlazorBlueprint.Primitives/wwwroot/js/primitives/menu-keyboard.js', import.meta.url), 'utf8');
const { initialize, dispose, focusInitial } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);
let serial = 0;
function menu(direction = 'ltr') {
  const handlers = new Map();
  const items = [];
  const panel = {
    direction, items, querySelectorAll: () => items,
    contains: item => items.includes(item), closest: () => panel,
    addEventListener: (name, cb) => handlers.set(name, cb),
    removeEventListener: name => handlers.delete(name),
    focus: () => { document.activeElement = panel; }
  };
  function item(label, attrs = {}) {
    const attributes = new Map(Object.entries(attrs));
    const entry = {
      textContent: label, clicks: 0,
      getAttribute: name => attributes.get(name) ?? null,
      hasAttribute: name => attributes.has(name),
      setAttribute: (name, value) => attributes.set(name, value),
      closest: () => panel,
      focus: () => { document.activeElement = entry; },
      click: () => entry.clicks++
    };
    items.push(entry);
    return entry;
  }
  const calls = [];
  const id = `menu-${serial++}`;
  const api = { invokeMethodAsync: async name => calls.push(name) };
  globalThis.document = { activeElement: panel, getElementById: () => null };
  globalThis.getComputedStyle = element => ({ direction: element.direction });
  return { panel, item, calls,
    start: mode => initialize(panel, api, id, { mode }),
    key(key, overrides = {}) {
      const event = { key, target: document.activeElement, preventDefault() { this.prevented = true; }, stopPropagation() { this.stopped = true; }, ...overrides };
      handlers.get('keydown')(event);
      return event;
    },
    end: () => dispose(id)
  };
}

test('menu navigation skips disabled and nested items; radio activation uses the item click', () => {
  const m = menu();
  const one = m.item('Comfortable');
  m.item('Unavailable', { 'aria-disabled': 'true' });
  const nested = m.item('Nested'); nested.closest = () => ({});
  const two = m.item('Compact', { role: 'menuitemradio' });
  m.start('vertical');
  focusInitial(m.panel, 'first');
  assert.equal(document.activeElement, one);
  m.key('ArrowDown'); assert.equal(document.activeElement, two);
  m.key('Enter'); assert.equal(two.clicks, 1);
  m.key('ArrowDown'); assert.equal(document.activeElement, one);
  m.end();
});

test('forward arrow opens a submenu before switching menubar menus, and reuses an open submenu', () => {
  const child = menu();
  const leaf = child.item('Leaf');
  const m = menu();
  const trigger = m.item('Share', { 'data-bb-submenu-trigger': '', 'aria-controls': 'child' });
  m.start('menubar'); trigger.focus();
  const opened = m.key('ArrowRight');
  assert.equal(trigger.clicks, 1); assert.deepEqual(m.calls, []);
  assert.equal(opened.stopped, true);
  trigger.setAttribute('aria-expanded', 'true');
  document.getElementById = id => id === 'child' ? child.panel : null;
  m.key('ArrowRight'); assert.equal(document.activeElement, leaf);
  m.end();
});

test('submenu back and Escape close only the current submenu and reverse in RTL', () => {
  for (const direction of ['ltr', 'rtl']) {
    const m = menu(direction); m.item('Child').focus(); m.start('submenu');
    const back = m.key(direction === 'rtl' ? 'ArrowRight' : 'ArrowLeft');
    assert.equal(back.stopped, true);
    const escape = m.key('Escape'); assert.equal(escape.stopped, true);
    assert.deepEqual(m.calls, ['JsOnEscapeKey', 'JsOnEscapeKey']);
    m.end();
  }
});

test('handled events and modifier shortcuts are left alone', () => {
  const m = menu(); const item = m.item('Item'); m.start('vertical'); item.focus();
  m.key('Enter', { defaultPrevented: true });
  m.key('Enter', { ctrlKey: true });
  assert.equal(item.clicks, 0);
  m.end();
});

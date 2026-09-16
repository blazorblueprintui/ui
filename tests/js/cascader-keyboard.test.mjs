import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../../src/BlazorBlueprint.Components/wwwroot/js/cascader.js', import.meta.url), 'utf8');
const { initialize, connect, dispose } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

class Element {
  children = [];
  dataset = {};
  attributes = new Map();
  listeners = new Map();
  isConnected = true;
  tabIndex = 0;
  clicks = 0;
  constructor(parent, dataset = {}) { this.parentElement = parent; this.dataset = dataset; parent?.children.push(this); }
  getAttribute(name) { return this.attributes.get(name) ?? null; }
  hasAttribute(name) { return this.attributes.has(name); }
  matches(selector) {
    const key = selector.slice(1, -1).replace(/^data-/, '').replace(/-([a-z])/g, (_, c) => c.toUpperCase());
    return key in this.dataset;
  }
  closest(selector) { return this.matches(selector) ? this : this.parentElement?.closest(selector); }
  querySelectorAll(selector) { return this.children.flatMap(child => [...(child.matches(selector) ? [child] : []), ...child.querySelectorAll(selector)]); }
  querySelector(selector) { return this.querySelectorAll(selector)[0]; }
  addEventListener(name, handler) { this.listeners.set(name, handler); }
  removeEventListener(name) { this.listeners.delete(name); }
  focus() { document.activeElement = this; }
  scrollIntoView() {}
  click() { this.clicks++; this.onclick?.(); }
}

function fixture() {
  const observers = [];
  const frames = [];
  globalThis.requestAnimationFrame = callback => frames.push(callback);
  globalThis.MutationObserver = class {
    constructor(callback) { this.callback = callback; observers.push(this); }
    observe() {}
    disconnect() { this.disconnected = true; }
  };
  globalThis.getComputedStyle = () => ({ direction: 'ltr' });
  const root = new Element();
  const trigger = new Element(root, { cascaderTrigger: 'true' });
  const portal = new Element(null, { bbPortal: 'portal', state: 'open' });
  const popup = new Element(portal);
  const search = new Element(popup, { cascaderSearch: 'true' });
  const column = new Element(popup, { cascaderColumn: '0' });
  const engineering = new Element(column, { cascaderItem: 'engineering' });
  engineering.attributes.set('aria-expanded', 'false');
  const design = new Element(column, { cascaderItem: 'design' });
  design.attributes.set('aria-expanded', 'false');
  globalThis.document = { getElementById: () => popup, activeElement: search };
  initialize(root, 'picker');
  connect('picker');
  const flush = () => { while (frames.length) frames.shift()(); };
  const key = (target, key, extra = {}) => {
    const event = { target, key, preventDefault() { this.prevented = true; }, ...extra };
    (target === trigger ? trigger : popup).listeners.get('keydown')(event);
    return event;
  };
  const update = () => observers.filter(observer => !observer.disconnected).forEach(observer => observer.callback());
  return { root, popup, portal, search, trigger, engineering, design, observers, key, update, flush };
}

test('Cascader arrows, Home/End, and RTL navigate levels after asynchronous branch rendering', () => {
  const f = fixture();
  f.key(f.search, 'ArrowDown');
  assert.equal(document.activeElement, f.engineering);
  f.key(f.engineering, 'End');
  assert.equal(document.activeElement, f.design);
  f.key(f.design, 'Home');
  assert.equal(document.activeElement, f.engineering);
  f.key(f.engineering, 'ArrowRight');
  assert.equal(f.engineering.clicks, 1);
  assert.equal(document.activeElement, f.engineering);

  const children = new Element(f.popup, { cascaderColumn: '1', cascaderParent: 'engineering' });
  const platform = new Element(children, { cascaderItem: 'platform' });
  const web = new Element(children, { cascaderItem: 'web' });
  f.engineering.attributes.set('aria-expanded', 'true');
  f.update();
  assert.equal(document.activeElement, platform);
  f.key(platform, 'ArrowDown');
  assert.equal(document.activeElement, web);
  f.key(web, 'ArrowLeft');
  assert.equal(document.activeElement, f.engineering);
  globalThis.getComputedStyle = () => ({ direction: 'rtl' });
  f.key(f.engineering, 'ArrowLeft');
  assert.equal(document.activeElement, platform);
  f.key(platform, 'ArrowRight');
  assert.equal(document.activeElement, f.engineering);
  // Enter still activates an already expanded branch, including ChangeOnSelect behavior.
  f.key(f.engineering, 'Enter');
  assert.equal(f.engineering.clicks, 2);
  dispose('picker');
  assert.equal(f.trigger.listeners.size, 0);
  assert.equal(f.popup.listeners.size, 0);
  assert.ok(f.observers.every(observer => observer.disconnected));
});

test('search editing keys stay native; results support arrows and Enter without submitting the form', () => {
  const f = fixture();
  assert.equal(f.key(f.search, 'ArrowLeft').prevented, undefined);
  assert.equal(f.key(f.search, 'Home').prevented, undefined);
  f.popup.children = [f.search];
  const results = new Element(f.popup, { cascaderColumn: '0' });
  const runtime = new Element(results, { cascaderItem: 'runtime' });
  const web = new Element(results, { cascaderItem: 'web' });
  f.update();
  f.key(f.search, 'ArrowUp');
  assert.equal(document.activeElement, web);
  assert.equal(f.key(web, 'Enter').prevented, true);
  assert.equal(web.clicks, 1);
  assert.equal(f.key(f.search, 'Enter').prevented, true);
  assert.equal(runtime.clicks, 1);
  assert.equal(f.key(runtime, 'Escape').prevented, undefined);
  assert.equal(f.key(runtime, 'Tab').prevented, undefined);
  assert.equal(f.key(f.search, 'Enter', { isComposing: true }).prevented, undefined);
  dispose('picker');
});

test('arrow keys open from the trigger, repeated opens release old handlers, and closing prevents late focus', () => {
  const f = fixture();
  f.trigger.disabled = true;
  f.key(f.trigger, 'ArrowDown');
  assert.equal(f.trigger.clicks, 0);
  f.trigger.disabled = false;
  f.portal.dataset.state = 'closed';
  f.key(f.trigger, 'ArrowUp');
  assert.equal(f.trigger.clicks, 1);
  f.portal.dataset.state = 'open';
  connect('picker');
  f.flush();
  assert.equal(document.activeElement, f.design);
  assert.equal(f.design.tabIndex, 0);
  assert.equal(f.engineering.tabIndex, -1);
  assert.equal(f.observers[0].disconnected, true);
  f.portal.dataset.state = 'closed';
  f.key(f.trigger, 'ArrowDown');
  connect('picker');
  document.activeElement = f.trigger;
  f.flush();
  assert.equal(document.activeElement, f.trigger);
  dispose('picker');
});

test('opening a selected path and rendering deeper levels scrolls to the trailing edge without stealing focus', () => {
  const f = fixture();
  const viewport = new Element(f.popup, { cascaderScroll: '' });
  viewport.scrollWidth = 960;
  viewport.scrollLeft = 0;
  new Element(viewport, { cascaderColumn: '1', cascaderParent: 'engineering' });
  f.update();
  f.flush();
  assert.equal(viewport.scrollLeft, 960);
  assert.equal(document.activeElement, f.search);
  viewport.scrollWidth = 1280;
  new Element(viewport, { cascaderColumn: '2', cascaderParent: 'platform' });
  f.update();
  assert.equal(viewport.scrollLeft, 1280);
  // A manual scroll is respected until the selected path changes or the popup reopens.
  viewport.scrollLeft = 100;
  f.update();
  assert.equal(viewport.scrollLeft, 100);
  connect('picker');
  f.flush();
  assert.equal(viewport.scrollLeft, 1280);
  globalThis.getComputedStyle = () => ({ direction: 'rtl' });
  connect('picker');
  f.flush();
  assert.equal(viewport.scrollLeft, -1280);
  dispose('picker');
});

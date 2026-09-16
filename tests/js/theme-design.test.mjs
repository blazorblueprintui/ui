import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';
async function moduleAt(path) {
  const source = await readFile(new URL(path, import.meta.url), 'utf8');
  return import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);
}
const theme = await moduleAt('../../src/BlazorBlueprint.Components/wwwroot/js/theme.js');
const { inheritTheme } = await moduleAt('../../src/BlazorBlueprint.Primitives/wwwroot/js/theme-scope.js');
function element() {
  const attrs = new Map(); const styles = new Map(); const classes = new Set();
  return {
    getAttribute: name => attrs.get(name) ?? null,
    setAttribute: (name, value) => attrs.set(name, value), removeAttribute: name => attrs.delete(name),
    style: { getPropertyValue: name => styles.get(name) ?? '', setProperty: (name, value) => styles.set(name, value), removeProperty: name => styles.delete(name) },
    classList: { add: name => classes.add(name), remove: name => classes.delete(name), contains: name => classes.has(name) }
  };
}
function environment(saved) {
  const root = element(); const storage = new Map(saved ? [['bb-theme', JSON.stringify(saved)]] : []);
  globalThis.document = { documentElement: root };
  globalThis.localStorage = { getItem: key => storage.get(key), setItem: (key, value) => storage.set(key, value), removeItem: key => storage.delete(key) };
  globalThis.MutationObserver = class { observe() {} disconnect() {} };
  return { root, storage };
}
const config = {
  persist: true, detectSystemPreference: false,
  defaults: { isDarkMode: false, baseColor: 'zinc', primaryColor: 'default', radius: 0.5, design: { density: 'compact', font: 'inter' } },
  validBaseColors: ['zinc', 'slate'], validPrimaryColors: ['default', 'blue']
};
test('old saved themes retain configured design defaults; invalid stored design and radius fall back', () => {
  environment({ isDarkMode: true, baseColor: 'slate', primaryColor: 'blue', radius: -3, design: { density: 'unknown', font: 'mono' } });
  const state = theme.initialize(config);
  assert.equal(state.radius, 0.5);
  assert.equal(state.design.density, 'compact');
  assert.equal(state.design.font, 'mono');
  environment({ radius: 0.75 });
  assert.equal(theme.initialize(config).design.font, 'inter');
});
test('appearance persists and disabled persistence clears an earlier saved theme', () => {
  const { storage, root } = environment();
  theme.saveTheme(false, 'zinc', 'blue', 0.75, { density: 'dense', menuColor: 'inverse' });
  const state = theme.initialize(config);
  assert.equal(state.design.density, 'dense');
  assert.equal(root.getAttribute('data-bb-menu-color'), 'inverse');
  theme.initialize({ ...config, persist: false });
  assert.equal(storage.has('bb-theme'), false);
  assert.equal(root.getAttribute('data-bb-density'), 'compact');
});
test('a scoped menu inherits fonts and tokens across a portal and releases observers on close', () => {
  const scope = element(); const floating = element();
  scope.style.setProperty('--bb-spacing', '0.1875rem');
  scope.style.setProperty('--radius', '0rem');
  scope.style.fontFamily = 'monospace';
  let observer; let disconnected = false;
  globalThis.MutationObserver = class { constructor(callback) { observer = callback; } observe() {} disconnect() { disconnected = true; } };
  globalThis.getComputedStyle = node => node.style;
  const cleanup = inheritTheme({ closest: () => scope }, floating);
  assert.equal(floating.style.getPropertyValue('--bb-spacing'), '0.1875rem');
  assert.equal(floating.style.fontFamily, 'monospace');
  assert.equal(floating.getAttribute('data-bb-theme-scope'), '');
  scope.style.setProperty('--radius', '0.5rem'); observer();
  assert.equal(floating.style.getPropertyValue('--radius'), '0.5rem');
  cleanup();
  assert.equal(disconnected, true);
  assert.equal(floating.style.getPropertyValue('--radius'), '');
  assert.equal(floating.getAttribute('data-bb-theme-scope'), null);
});

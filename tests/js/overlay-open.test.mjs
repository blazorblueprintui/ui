import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

// Every module overlay.js imports is a stub the tests can steer, so open() runs on its own.
const stubs = globalThis.overlayStubs = {
  clickOutside: {}, themeScope: {}, escapeKeydown: {}, positioning: {}, menuKeyboard: {}, select: {}
};
const source = (await readFile(new URL('../../src/BlazorBlueprint.Primitives/wwwroot/js/primitives/overlay.js', import.meta.url), 'utf8'))
  .replace(/^import \* as (\w+) from .*;$/gm, 'const $1 = globalThis.overlayStubs.$1;');
const { open, close, isOpen } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);
globalThis.CSS = { escape: value => value };

const dismiss = { contentId: 'content', triggerId: 'trigger', onOutsideInteraction: true, onEscapeKey: true };
const dotNetRef = { invokeMethodAsync: async () => {} };

function fixture(autoUpdate) {
  const calls = [];
  globalThis.document = { querySelector: () => ({}) };
  Object.assign(stubs.themeScope, { inheritTheme: () => () => {} });
  Object.assign(stubs.positioning, {
    isElementReady: () => true,
    computePosition: async () => ({ placement: 'bottom' }),
    applyPosition: (floating, position, makeVisible) => calls.push(makeVisible ? 'revealed' : 'positioned'),
    hidePosition: async () => calls.push('hidden'),
    autoUpdate
  });
  Object.assign(stubs.clickOutside, {
    onClickOutsideByIds: (...args) => {
      calls.push(['outside', ...args]);
      return { dispose: () => calls.push('outside disposed') };
    }
  });
  Object.assign(stubs.escapeKeydown, {
    initialize: (...args) => calls.push(['escape', ...args]),
    dispose: id => calls.push(['escape disposed', id])
  });
  return calls;
}

async function quietly(run) {
  const logged = [];
  const error = console.error;
  console.error = (...args) => logged.push(args);
  try {
    await run();
  } finally {
    console.error = error;
  }
  return logged;
}

test('an overlay whose auto-update fails still wires Escape and outside dismissal', async () => {
  // What a plain-HTTP page hit: positioning's auto-update threw on a missing crypto.randomUUID.
  const calls = fixture(async () => { throw new TypeError('crypto.randomUUID is not a function'); });

  const logged = await quietly(() => open('popover', {}, { dismiss }, dotNetRef));

  assert.deepEqual(calls, [
    'revealed',
    ['outside', 'content', dotNetRef, 'JsOnDismissOutside', 'trigger'],
    ['escape', dotNetRef, 'popover', 'JsOnDismissEscape']
  ]);
  assert.equal(isOpen('popover'), true);
  assert.match(logged[0][0], /failed to keep 'popover' positioned/);

  await close('popover');
  assert.deepEqual(calls.slice(3), ['outside disposed', ['escape disposed', 'popover'], 'hidden']);
  assert.equal(isOpen('popover'), false);
});

test('one dismissal listener failing does not cost the overlay the other', async () => {
  const calls = fixture(async () => ({ apply: () => {} }));
  stubs.clickOutside.onClickOutsideByIds = () => { throw new Error('outside failed'); };

  const logged = await quietly(() => open('menu', {}, { dismiss }, dotNetRef));

  assert.deepEqual(calls, ['revealed', ['escape', dotNetRef, 'menu', 'JsOnDismissEscape']]);
  assert.equal(isOpen('menu'), true);
  assert.match(logged[0][0], /failed to wire outside dismissal for 'menu'/);
  await close('menu');
});

test('a close that lands while auto-update is failing leaves no listeners behind', async () => {
  let fail;
  const calls = fixture(() => new Promise((_, reject) => { fail = reject; }));

  const opening = open('select', {}, { dismiss }, dotNetRef);
  await new Promise(resolve => setImmediate(resolve));
  await close('select');
  await quietly(async () => {
    fail(new TypeError('crypto.randomUUID is not a function'));
    await opening;
  });

  assert.equal(calls.some(call => Array.isArray(call)), false);
  assert.equal(isOpen('select'), false);
});

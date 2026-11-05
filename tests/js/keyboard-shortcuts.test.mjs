import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../../src/BlazorBlueprint.Primitives/wwwroot/js/primitives/keyboard-shortcuts.js', import.meta.url), 'utf8');
const { initialize, registerShortcut, dispose } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

function shortcuts(t) {
  let handler;
  const calls = [];
  const previousDocument = globalThis.document;
  globalThis.document = {
    addEventListener: (_, callback) => { handler = callback; },
    removeEventListener() {}
  };
  initialize({ invokeMethodAsync: async (...args) => { calls.push(args); } });
  registerShortcut('ctrl+s');
  t.after(() => {
    dispose();
    globalThis.document = previousDocument;
  });
  return {
    calls,
    key(overrides = {}) {
      const event = {
        key: 's', ctrlKey: true, target: { tagName: 'DIV' },
        defaultPrevented: false, stopped: false,
        preventDefault() { this.defaultPrevented = true; },
        stopPropagation() { this.stopped = true; },
        ...overrides
      };
      handler(event);
      return event;
    }
  };
}

for (const composition of [{ isComposing: true }, { keyCode: 229 }]) {
  test(`composition ${JSON.stringify(composition)} leaves the event untouched`, t => {
    const s = shortcuts(t);
    const event = s.key(composition);
    assert.deepEqual(s.calls, []);
    assert.equal(event.defaultPrevented, false);
    assert.equal(event.stopped, false);
  });
}

test('registered shortcuts still fire after composition ends, including repeats', t => {
  const s = shortcuts(t);
  s.key({ isComposing: true, keyCode: 229 });
  for (const overrides of [{}, { repeat: true }, { ctrlKey: false, metaKey: true }]) {
    const event = s.key(overrides);
    assert.equal(event.defaultPrevented, true);
    assert.equal(event.stopped, true);
  }
  assert.deepEqual(s.calls, Array.from({ length: 3 }, () => ['HandleShortcutAsync', 'ctrl+s']));
});

test('unregistered keys and editable targets remain untouched', t => {
  const s = shortcuts(t);
  for (const overrides of [
    { key: 'x' },
    ...['INPUT', 'TEXTAREA', 'SELECT'].map(tagName => ({ target: { tagName } })),
    { target: { tagName: 'DIV', isContentEditable: true } }
  ]) {
    const event = s.key(overrides);
    assert.equal(event.defaultPrevented, false);
    assert.equal(event.stopped, false);
  }
  assert.deepEqual(s.calls, []);
});

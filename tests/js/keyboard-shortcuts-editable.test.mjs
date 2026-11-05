import assert from 'node:assert/strict';
import { after, test } from 'node:test';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../../src/BlazorBlueprint.Primitives/wwwroot/js/primitives/keyboard-shortcuts.js', import.meta.url), 'utf8');
const shortcuts = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);
const previousDocument = globalThis.document;
after(() => { globalThis.document = previousDocument; });

function harness() {
  let handler;
  const calls = [];
  globalThis.document = {
    addEventListener: (name, callback) => { handler = callback; },
    removeEventListener: () => { handler = null; }
  };
  shortcuts.initialize({ invokeMethodAsync: async (...args) => { calls.push(args); } });
  return {
    calls,
    key(target, key = 's') {
      const event = {
        target, key, ctrlKey: true,
        preventDefault() { this.prevented = true; },
        stopPropagation() { this.stopped = true; }
      };
      handler(event);
      return event;
    }
  };
}

const editableTargets = [
  { tagName: 'INPUT' },
  { tagName: 'TEXTAREA' },
  { tagName: 'SELECT' },
  { tagName: 'SPAN', isContentEditable: true }
];

test('shortcuts in editable elements require an opt-in for each key', () => {
  const h = harness();
  try {
    shortcuts.registerShortcut('ctrl+s', true);
    shortcuts.registerShortcut('ctrl+k');
    for (const target of editableTargets) {
      const save = h.key(target);
      assert.equal(save.prevented, true, `${target.tagName}: opted-in shortcut prevents browser action`);
      assert.equal(save.stopped, true);
      const palette = h.key(target, 'k');
      assert.equal(palette.prevented, undefined);
      assert.equal(palette.stopped, undefined);
    }
    assert.deepEqual(h.calls, editableTargets.map(() => ['HandleShortcutAsync', 'ctrl+s']));
    h.key({ tagName: 'BUTTON' }, 'k');
    assert.deepEqual(h.calls.at(-1), ['HandleShortcutAsync', 'ctrl+k']);
  } finally {
    shortcuts.dispose();
  }
});

test('replacing, unregistering and disposing clear the editable opt-in', () => {
  const h = harness();
  try {
    shortcuts.registerShortcut('CTRL+S', true);
    shortcuts.registerShortcut('ctrl+s');
    assert.equal(h.key(editableTargets[0]).prevented, undefined);
    shortcuts.registerShortcut('ctrl+s', true);
    shortcuts.unregisterShortcut('CTRL+S');
    assert.equal(h.key(editableTargets[0]).prevented, undefined);
    shortcuts.registerShortcut('ctrl+s', true);
    shortcuts.dispose();
    const fresh = harness();
    assert.equal(fresh.key(editableTargets[0]).prevented, undefined);
    assert.deepEqual(fresh.calls, []);
  } finally {
    shortcuts.dispose();
  }
});

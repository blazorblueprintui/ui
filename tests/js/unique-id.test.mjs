import assert from 'node:assert/strict';
import { test } from 'node:test';
import { createId } from '../../src/BlazorBlueprint.Primitives/wwwroot/js/unique-id.js';
import { onClickOutsideByIds } from '../../src/BlazorBlueprint.Primitives/wwwroot/js/primitives/click-outside.js';

const v4 = /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/;

// A plain-HTTP page on any host but localhost: the browser leaves randomUUID off its crypto object.
function withoutRandomUUID(run) {
  Object.defineProperty(crypto, 'randomUUID', { value: undefined, configurable: true });
  try {
    return run();
  } finally {
    delete crypto.randomUUID;
  }
}

test('ids are version 4 UUIDs from getRandomValues when randomUUID is missing', () => {
  withoutRandomUUID(() => {
    const ids = Array.from({ length: 100 }, createId);
    for (const id of ids) {
      assert.match(id, v4);
    }
    assert.equal(new Set(ids).size, ids.length);
  });
});

test('randomUUID is used when the page has it', () => {
  Object.defineProperty(crypto, 'randomUUID', { value: () => 'from-random-uuid', configurable: true });
  try {
    assert.equal(createId(), 'from-random-uuid');
  } finally {
    delete crypto.randomUUID;
  }
});

test('click-outside registers and releases its listeners without randomUUID', () => {
  const listeners = new Map();
  globalThis.document = {
    addEventListener: (name, handler) => listeners.set(name, handler),
    removeEventListener: name => listeners.delete(name)
  };

  withoutRandomUUID(() => {
    const handle = onClickOutsideByIds('content', { invokeMethodAsync: async () => {} }, 'JsOnDismissOutside', 'trigger');
    assert.match(handle._cleanupId, v4);
    assert.deepEqual([...listeners.keys()], ['pointerdown', 'pointerup']);
    handle.dispose();
  });

  assert.equal(listeners.size, 0);
});

import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';
const source = await readFile(new URL('../../src/BlazorBlueprint.Primitives/wwwroot/js/sortable-transfer.js', import.meta.url), 'utf8');
const { performDrop } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

test('rejected transfers notify neither list', async () => {
  const calls = [];
  const source = { invokeMethodAsync: async method => { calls.push(method); return false; } };
  const target = { invokeMethodAsync: async method => calls.push(method) };
  assert.equal(await performDrop(source, target, 1, 2, 'target', false), false);
  assert.deepEqual(calls, ['CanDropJS']);
});

test('accepted transfer waits for DOM restoration then awaits source completion before target notification', async () => {
  const calls = []; let domRestored = false; let release;
  const gate = new Promise(resolve => { release = resolve; });
  const source = { invokeMethodAsync: async method => {
    assert.equal(domRestored, true); calls.push(method);
    if (method === 'OnRemoveJS') await gate;
    return true;
  } };
  const target = { invokeMethodAsync: async method => calls.push(method) };
  const transfer = performDrop(source, target, 0, 2, 'target', true);
  domRestored = true;
  await Promise.resolve(); await Promise.resolve();
  assert.deepEqual(calls, ['CanDropJS', 'OnRemoveJS']);
  release(); await transfer;
  assert.deepEqual(calls, ['CanDropJS', 'OnRemoveJS', 'OnAddJS']);
});

import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../../src/BlazorBlueprint.Components/wwwroot/js/file-upload.js', import.meta.url), 'utf8');
const { initializeDropZone } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

class Element extends EventTarget {
  disabled = false;
  files = null;
}
function drop(zone, files) {
  const event = new Event('drop', { cancelable: true });
  event.dataTransfer = { files };
  zone.dispatchEvent(event);
}

test('drop locks a selected input before another drop can invalidate browser files', () => {
  const zone = new Element();
  const input = new Element();
  let changes = 0;
  input.addEventListener('change', () => changes++);
  const cleanup = initializeDropZone(zone, input);
  const first = [{ name: 'first.bin' }];
  drop(zone, first);
  assert.equal(input.disabled, true);
  drop(zone, [{ name: 'second.bin' }]);
  assert.equal(input.files, first);
  assert.equal(changes, 1);
  cleanup.dispose();
});

test('disabled drop zones do not replace selected files and listeners are cleaned up', () => {
  const zone = new Element();
  const input = new Element();
  input.disabled = true;
  const cleanup = initializeDropZone(zone, input);
  drop(zone, [{ name: 'ignored.bin' }]);
  assert.equal(input.files, null);
  cleanup.dispose();
  input.disabled = false;
  input.dispatchEvent(new Event('change'));
  assert.equal(input.disabled, false);
  drop(zone, [{ name: 'after-dispose.bin' }]);
  assert.equal(input.files, null);
});

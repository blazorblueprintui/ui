import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

const source = (await readFile(new URL('../../src/BlazorBlueprint.Primitives/wwwroot/js/primitives/positioning.js', import.meta.url), 'utf8'))
  .replace(/^import .*;$/gm, 'const floatingUIBundled = {};');
const { hidePosition } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

test('a popup hides when its exit ends without waiting for a longer tree transition or spinner', async () => {
  globalThis.requestAnimationFrame = callback => callback();
  const styles = new Map();
  let finishExit;
  const floating = {
    dataset: { state: 'closed' },
    style: { setProperty: (name, value) => styles.set(name, value) }
  };
  const content = { parentElement: floating, dataset: { state: 'closed' } };
  const child = { parentElement: content, dataset: {} };
  const exit = new Promise(resolve => { finishExit = resolve; });
  const animation = (target, finished, iterations = 1) => ({
    playState: 'running', effect: { target, getTiming: () => ({ iterations }) }, finished
  });
  floating.getAnimations = () => [
    animation(content, exit), animation(child, new Promise(() => {})),
    animation(child, new Promise(() => {}), Infinity)
  ];
  const hiding = hidePosition(floating);
  assert.equal(styles.get('pointer-events'), 'none');
  assert.equal(styles.has('visibility'), false);
  finishExit();
  // The longer child transition remains running. The popup must already be hidden.
  await Promise.race([hiding, new Promise((_, reject) => setTimeout(() => reject(new Error('Waited for a descendant animation')), 100))]);
  assert.equal(styles.get('visibility'), 'hidden');
});

test('reopening during an exit keeps the popup visible', async () => {
  globalThis.requestAnimationFrame = callback => callback();
  const styles = new Map();
  const floating = { dataset: { state: 'closed' }, style: { setProperty: (name, value) => styles.set(name, value) } };
  let finish;
  floating.getAnimations = () => [{ playState: 'running', effect: { target: floating }, finished: new Promise(resolve => { finish = resolve; }) }];
  const hiding = hidePosition(floating);
  await Promise.resolve();
  floating.dataset.state = 'open';
  finish();
  await hiding;
  assert.equal(styles.has('visibility'), false);
});

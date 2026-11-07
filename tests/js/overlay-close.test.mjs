import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFile } from 'node:fs/promises';

// The real hidePosition behind overlay.close(), with Floating UI stubbed out. On Blazor Server the
// close call reaches the browser before the render that flips data-state, and these tests keep
// data-state at what the browser actually holds at that moment.
const positioningSource = (await readFile(new URL('../../src/BlazorBlueprint.Primitives/wwwroot/js/primitives/positioning.js', import.meta.url), 'utf8'))
  .replace(/^import \* as floatingUIBundled from .*;$/m, 'const floatingUIBundled = {};')
  .replace(/^import \{ createId \} from .*;$/m, "const createId = () => 'id';");
const { hidePosition } = await import(`data:text/javascript;base64,${Buffer.from(positioningSource).toString('base64')}`);

const stubs = globalThis.overlayStubs = {
  clickOutside: {}, themeScope: { inheritTheme: () => () => {} }, escapeKeydown: {}, menuKeyboard: {}, select: {},
  positioning: {
    hidePosition,
    isElementReady: () => true,
    computePosition: async () => ({ placement: 'top' }),
    applyPosition: () => {}
  }
};
const overlaySource = (await readFile(new URL('../../src/BlazorBlueprint.Primitives/wwwroot/js/primitives/overlay.js', import.meta.url), 'utf8'))
  .replace(/^import \* as (\w+) from .*;$/gm, 'const $1 = globalThis.overlayStubs.$1;');
const { open, close } = await import(`data:text/javascript;base64,${Buffer.from(overlaySource).toString('base64')}`);

globalThis.CSS = { escape: value => value };
globalThis.requestAnimationFrame = callback => callback();

function tooltip(state, animations = []) {
  const styles = new Map();
  const floating = {
    dataset: { state },
    style: { setProperty: (name, value) => styles.set(name, value) },
    getAnimations: () => animations
  };
  globalThis.document = { querySelector: () => floating, getElementById: () => null };
  return { floating, styles };
}

test('a close that arrives before the closed render still hides the overlay (#608)', async () => {
  // Opened and shown; the render carrying data-state="closed" has not been applied yet.
  const { styles } = tooltip('open');
  styles.set('visibility', 'visible');

  await close('tooltip-608');

  assert.equal(styles.get('visibility'), 'hidden');
  assert.equal(styles.get('opacity'), '0');
});

test('a reopen during the exit animation keeps the overlay visible', async () => {
  let finishExit;
  const exit = { playState: 'running', effect: { target: null }, finished: new Promise(resolve => { finishExit = resolve; }) };
  const { floating, styles } = tooltip('closed', [exit]);
  exit.effect.target = floating;

  const closing = close('popover-reopen');
  await Promise.resolve();

  // The reopen's own render has not landed either, so data-state still says closed.
  await open('popover-reopen', {}, { autoUpdate: false });
  finishExit();
  await closing;

  assert.equal(styles.has('visibility'), false);
});

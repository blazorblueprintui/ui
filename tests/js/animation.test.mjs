import assert from 'node:assert/strict';
import { test } from 'node:test';
import { waitForExit } from '../../src/BlazorBlueprint.Primitives/wwwroot/js/primitives/animation.js';

const withAnimationFrame = () => {
  globalThis.requestAnimationFrame = callback => callback();
};

const animation = (target, finished, playState = 'running', iterations = 1) => ({
  playState,
  effect: { target, getTiming: () => ({ iterations }) },
  finished
});

test('an element with no animations resolves at once', async () => {
  withAnimationFrame();
  const element = { getAnimations: () => [] };

  await waitForExit(element);
});

test('an element without getAnimations resolves at once', async () => {
  withAnimationFrame();

  await waitForExit({});
});

test('a null element resolves at once', async () => {
  await waitForExit(null);
});

test('waits for the element exit animation to finish', async () => {
  withAnimationFrame();
  let finish;
  const element = {
    getAnimations: () => [animation(element, new Promise(resolve => { finish = resolve; }))]
  };

  let done = false;
  const waiting = waitForExit(element).then(() => { done = true; });

  await Promise.resolve();
  assert.equal(done, false);

  finish();
  await waiting;
  assert.equal(done, true);
});

test('a direct closed-state child is part of the exit', async () => {
  withAnimationFrame();
  let finish;
  const element = {};
  const content = { parentElement: element, dataset: { state: 'closed' } };
  element.getAnimations = () => [animation(content, new Promise(resolve => { finish = resolve; }))];
  let done = false;
  const waiting = waitForExit(element).then(() => { done = true; });

  await Promise.resolve();
  assert.equal(done, false);

  finish();
  await waiting;
  assert.equal(done, true);
});

test('a longer descendant animation is ignored', async () => {
  withAnimationFrame();
  const element = {};
  const content = { parentElement: element, dataset: { state: 'closed' } };
  const child = { parentElement: content, dataset: {} };
  element.getAnimations = () => [
    animation(content, Promise.resolve()),
    animation(child, new Promise(() => {}))
  ];

  await waitForExit(element);
});

test('an infinite animation is ignored', async () => {
  withAnimationFrame();
  const element = {};
  element.getAnimations = () => [animation(element, new Promise(() => {}), 'running', Infinity)];

  await waitForExit(element);
});

test('a paused animation is ignored', async () => {
  withAnimationFrame();
  const element = {};
  element.getAnimations = () => [animation(element, new Promise(() => {}), 'paused')];

  await waitForExit(element);
});

test('a cancelled animation still ends the wait', async () => {
  withAnimationFrame();
  const element = {};
  element.getAnimations = () => [animation(element, Promise.reject(new Error('cancelled')))];

  await waitForExit(element);
});

test('an open direct child is not part of the exit', async () => {
  withAnimationFrame();
  const element = {};
  const child = { parentElement: element, dataset: { state: 'open' } };
  element.getAnimations = () => [animation(child, new Promise(() => {}))];

  await waitForExit(element);
});

test('reduced motion (an already-finished 0.01ms animation) ends the wait at once', async () => {
  withAnimationFrame();
  const element = {};
  element.getAnimations = () => [animation(element, Promise.resolve(), 'running', 1)];

  await waitForExit(element);
});

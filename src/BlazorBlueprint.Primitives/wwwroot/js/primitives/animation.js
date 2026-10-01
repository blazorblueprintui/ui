/**
 * Shared exit-animation timing for overlay components that keep an element mounted
 * with data-state="closed" while its animate-out classes play.
 */

/** Longest an exit animation may hold the element visible before we unmount it regardless. */
const EXIT_ANIMATION_TIMEOUT_MS = 1000;

/**
 * Resolves once all of an element's exit animations have finished, or immediately if there
 * is none.
 *
 * The caller keeps the element mounted with data-state="closed" while awaiting this; blob's
 * completion is what flips a component's internal closing flag so the element can be removed.
 *
 * Only the element itself and its direct content elements are considered. A direct child in
 * the closed state is how animated overlays are composed (a `display: contents` portal wrapper
 * with an overlay and a content div under it). Descendant transitions (tree chevrons, hovered
 * rows, checkboxes) can outlast the exit fade just as a loading spinner can — waiting for them
 * lets the completed fade revert to full opacity, briefly revealing the overlay again.
 *
 * @param {HTMLElement} el - The element whose animations to wait on.
 * @returns {Promise<void>}
 */
export async function waitForExit(el) {
    if (!el) return;

    // getAnimations reports nothing until the browser has processed the class and attribute
    // change that starts the animation, so give it one frame to do that first.
    await new Promise(resolve => requestAnimationFrame(() => resolve()));

    if (typeof el.getAnimations !== 'function') return;

    const running = el.getAnimations({ subtree: true }).filter(a => {
        if (a.playState !== 'running') return false;
        const target = a.effect?.target;
        if (target !== el && !(target?.parentElement === el && target.dataset.state === 'closed')) return false;
        const iterations = a.effect?.getTiming?.().iterations;
        return iterations !== Infinity;
    });
    if (running.length === 0) return;

    // allSettled, not all: a cancelled animation rejects, and a cancelled exit animation still
    // means we are done waiting. The timeout is a backstop for an animation that never ends
    // (infinite iteration, a paused element) — without it the overlay would stay on screen.
    await Promise.race([
        Promise.allSettled(running.map(a => a.finished)),
        new Promise(resolve => setTimeout(resolve, EXIT_ANIMATION_TIMEOUT_MS)),
    ]);
}
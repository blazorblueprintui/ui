const states = new WeakMap();
const preference = () => globalThis.matchMedia?.('(prefers-reduced-motion: reduce)');

export function presetFrames(preset, distance = 16) {
    const fade = [{ opacity: 0 }, { opacity: 1 }];
    const slide = (x, y) => [{ opacity: 0, transform: `translate(${x}px, ${y}px)` }, { opacity: 1, transform: 'translate(0, 0)' }];
    switch (preset) {
        case 'SlideUp': return slide(0, distance);
        case 'SlideDown': return slide(0, -distance);
        case 'SlideLeft': return slide(distance, 0);
        case 'SlideRight': return slide(-distance, 0);
        case 'Scale': return [{ opacity: 0, transform: 'scale(.92)' }, { opacity: 1, transform: 'scale(1)' }];
        case 'Spring': return [{ opacity: 0, transform: 'scale(.8)' }, { opacity: 1, transform: 'scale(1.06)', offset: .65 }, { opacity: 1, transform: 'scale(1)' }];
        case 'Bounce': return [0, -distance, 0, -distance / 3, 0].map(y => ({ transform: `translateY(${y}px)` }));
        case 'Pulse': return [1, 1.05, 1].map(scale => ({ transform: `scale(${scale})` }));
        case 'ShakeX': return [0, -distance, distance, -distance / 2, distance / 2, 0].map(x => ({ transform: `translateX(${x}px)` }));
        case 'ShakeY': return [0, -distance, distance, -distance / 2, distance / 2, 0].map(y => ({ transform: `translateY(${y}px)` }));
        default: return fade;
    }
}

function createState(element, type, config) {
    dispose(element);
    const state = { type, config, cleanups: [], animation: null, token: 0, reduced: preference(), disposed: false };
    states.set(element, state);
    const stopForPreference = () => {
        if (state.reduced?.matches) { stop(state); state.settle?.(); }
    };
    state.reduced?.addEventListener('change', stopForPreference);
    state.cleanups.push(() => state.reduced?.removeEventListener('change', stopForPreference));
    return state;
}
function listen(state, element, name, handler) {
    element.addEventListener(name, handler);
    state.cleanups.push(() => element.removeEventListener(name, handler));
}
function stop(state) {
    state.token++;
    state.animation?.cancel();
    state.animation = null;
}
function animate(element, state, frames, done) {
    stop(state);
    const token = state.token;
    if (state.reduced?.matches || !state.config.duration || !element.animate) { done?.(); return; }
    let animation;
    try {
        animation = element.animate(frames, { duration: state.config.duration, delay: state.config.delay || 0, easing: state.config.easing || 'ease-out', fill: 'both' });
    } catch (error) {
        done?.();
        console.warn('BlazorBlueprint: invalid motion keyframes or easing.', error);
        return;
    }
    state.animation = animation;
    animation.finished.then(() => {
        if (state.disposed || token !== state.token) { return; }
        done?.();
        animation.cancel();
        state.animation = null;
    }, () => {});
}
function runMotion(element, state, exiting = false) {
    if (!exiting && !state.config.visible) { return; }
    element.hidden = false;
    const frames = state.config.keyframes?.length ? state.config.keyframes : presetFrames(state.config.preset, state.config.distance);
    // Reversing offsets as well as order keeps custom keyframe offsets valid.
    const used = exiting ? [...frames].reverse().map(frame => ({ ...frame, ...(frame.offset != null ? { offset: 1 - frame.offset } : {}) })) : frames;
    animate(element, state, used, () => { element.hidden = !state.config.visible; });
}
export function motion(element, config) {
    let state = states.get(element);
    const key = JSON.stringify(config);
    if (state?.type === 'motion' && state.key === key) { return; }
    const previous = state?.type === 'motion' ? state.config : null;
    state = createState(element, 'motion', config);
    state.key = key;
    state.settle = () => { element.hidden = !state.config.visible; };
    state.settle();
    if (previous && previous.visible !== config.visible) { runMotion(element, state, !config.visible); }
    else if (config.visible && config.trigger === 'Visibility' && config.first && !previous) { runMotion(element, state); }
    if (config.trigger === 'InView' && typeof IntersectionObserver !== 'undefined') {
        const observer = new IntersectionObserver(entries => {
            if (entries.some(entry => entry.isIntersecting && entry.intersectionRatio >= config.threshold)) {
                runMotion(element, state);
                if (config.once) { observer.disconnect(); }
            }
        }, { threshold: config.threshold });
        observer.observe(element);
        state.cleanups.push(() => observer.disconnect());
    } else if (config.trigger === 'Hover') {
        listen(state, element, 'pointerenter', () => runMotion(element, state));
        listen(state, element, 'focusin', event => { if (!element.contains(event.relatedTarget)) { runMotion(element, state); } });
    } else if (config.trigger === 'Press') {
        listen(state, element, 'pointerdown', event => { if (event.button === 0) { runMotion(element, state); } });
        listen(state, element, 'keydown', event => { if (!event.repeat && ['Enter', ' '].includes(event.key)) { runMotion(element, state); } });
    }
}
export function play(element) {
    const state = states.get(element);
    if (state?.type === 'motion') { runMotion(element, state); }
}

export function height(element, config) {
    let state = states.get(element);
    if (state?.type === 'height') {
        const changed = state.config.expanded !== config.expanded;
        state.config = config;
        if (changed) { state.resize(); }
        return;
    }
    state = createState(element, 'height', config);
    const inner = element.firstElementChild;
    if (!inner) { return; }
    state.lastHeight = config.expanded ? inner.getBoundingClientRect().height : 0;
    state.settle = () => { element.hidden = !state.config.expanded; element.style.height = state.config.expanded ? 'auto' : '0px'; };
    state.resize = () => {
        const expanded = state.config.expanded;
        const before = state.animation ? element.getBoundingClientRect().height : state.lastHeight;
        element.hidden = false;
        const next = expanded ? inner.getBoundingClientRect().height : 0;
        state.lastHeight = next;
        if (!state.config.enabled || before === next) { stop(state); state.settle(); return; }
        animate(element, state, [{ height: `${before}px` }, { height: `${next}px` }], state.settle);
    };
    state.settle();
    const observer = new ResizeObserver(() => {
        if (state.config.expanded && Math.abs(inner.getBoundingClientRect().height - state.lastHeight) > .5) { state.resize(); }
    });
    observer.observe(inner);
    state.cleanups.push(() => observer.disconnect());
}

export function indicator(element, config) {
    let state = states.get(element);
    if (state?.type === 'indicator') { state.config = config; state.update(); return; }
    state = createState(element, 'indicator', config);
    const parent = element.parentElement;
    if (!parent) { return; }
    let hoverTarget = null;
    let previous = null;
    let observedTarget = null;
    let frame = 0;
    const schedule = () => { if (!frame) { frame = requestAnimationFrame(() => { frame = 0; state.update(); }); } };
    const resize = new ResizeObserver(schedule);
    resize.observe(parent);
    state.update = () => {
        let target;
        try { target = hoverTarget?.isConnected ? hoverTarget : parent.querySelector(state.config.selector); }
        catch { element.hidden = true; return; }
        if (!target || !target.getClientRects().length) { element.hidden = true; previous = null; return; }
        if (observedTarget !== target) { if (observedTarget) { resize.unobserve(observedTarget); } resize.observe(target); observedTarget = target; }
        const box = target.getBoundingClientRect();
        const host = parent.getBoundingClientRect();
        const style = { transform: `translate(${box.left - host.left + parent.scrollLeft - parent.clientLeft}px, ${box.top - host.top + parent.scrollTop - parent.clientTop}px)`, width: `${box.width}px`, height: `${box.height}px` };
        if (previous && JSON.stringify(previous) === JSON.stringify(style)) { return; }
        const from = state.animation ? { transform: getComputedStyle(element).transform, width: getComputedStyle(element).width, height: getComputedStyle(element).height } : previous;
        element.hidden = false;
        Object.assign(element.style, style);
        previous = style;
        if (from) { animate(element, state, [from, style]); }
    };
    const mutation = new MutationObserver(records => { if (records.some(record => record.target !== element)) { schedule(); } });
    mutation.observe(parent, { attributes: true, childList: true, subtree: true });
    listen(state, parent, 'scroll', schedule);
    const hover = event => {
        if (!state.config.hoverSelector) { return; }
        try { hoverTarget = event.target.closest(state.config.hoverSelector); } catch { hoverTarget = null; }
        if (hoverTarget && !parent.contains(hoverTarget)) { hoverTarget = null; }
        schedule();
    };
    listen(state, parent, 'pointerover', hover);
    listen(state, parent, 'focusin', hover);
    listen(state, parent, 'pointerleave', () => { hoverTarget = null; schedule(); });
    listen(state, parent, 'focusout', () => { hoverTarget = null; schedule(); });
    state.cleanups.push(() => { resize.disconnect(); mutation.disconnect(); if (frame) { cancelAnimationFrame(frame); } });
    state.update();
}

export function dispose(element) {
    const state = states.get(element);
    if (!state) { return; }
    state.disposed = true;
    stop(state);
    for (const cleanup of state.cleanups) { cleanup(); }
    states.delete(element);
}

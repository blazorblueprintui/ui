const instances = new WeakMap();
const interactive = 'button,a,input,select,textarea,[contenteditable=true],[role=button]';
export function maximumStart(offsets, maximumDistance) {
    if (maximumDistance <= 1) { return 0; }
    const first = offsets.findIndex(offset => offset >= maximumDistance - 1);
    return first < 0 ? Math.max(0, offsets.length - 1) : first;
}
export function configure(root, dotnet, config) {
    let state = instances.get(root);
    if (state) {
        if (state.config.paused && !config.paused) { state.focusPaused = false; }
        state.config = config; state.layout(); state.schedule(); return;
    }
    const viewport = root.querySelector('[data-carousel-viewport]');
    const track = viewport?.querySelector('[data-carousel-track]');
    if (!viewport || !track) { return; }
    state = { config, cleanups: [], timer: null, pending: false, focusPaused: false, hover: false, disposed: false, maximum: -1, distance: 0, drag: null, didDrag: false, slides: new Map() };
    instances.set(root, state);
    const reduced = matchMedia('(prefers-reduced-motion: reduce)');
    const on = (element, name, fn, options) => { element.addEventListener(name, fn, options); state.cleanups.push(() => element.removeEventListener(name, fn, options)); };
    const invoke = async (method, ...args) => {
        if (state.disposed) { return; }
        try { await dotnet.invokeMethodAsync(method, ...args); }
        catch (error) { if (!state.disposed) { console.warn('BlazorBlueprint carousel callback failed.', error); } }
    };
    const step = async direction => {
        if (state.pending || state.disposed) { return; }
        state.pending = true;
        try { await invoke('JsOnStep', direction); }
        finally { state.pending = false; state.schedule(); }
    };
    state.schedule = () => {
        clearTimeout(state.timer); state.timer = null;
        if (state.disposed || !state.config.autoplay || state.config.paused || state.focusPaused || state.hover || state.drag || state.pending || reduced.matches || document.hidden || state.maximum <= 0 || (!state.config.loop && state.config.index >= state.maximum)) { return; }
        state.timer = setTimeout(() => step(1), state.config.interval);
    };
    state.layout = () => {
        if (state.disposed) { return; }
        const slides = [...track.children].filter(child => child.hasAttribute('data-carousel-slide'));
        if (!slides.length) { return; }
        const vertical = state.config.vertical;
        state.rtl = !vertical && getComputedStyle(root).direction === 'rtl';
        viewport.style.touchAction = state.config.draggable ? vertical ? 'pan-x pinch-zoom' : 'pan-y pinch-zoom' : '';
        const boxes = slides.map(slide => slide.getBoundingClientRect());
        const offsets = boxes.map(box => vertical ? box.top - boxes[0].top : state.rtl ? boxes[0].right - box.right : box.left - boxes[0].left);
        const maxDistance = Math.max(0, vertical ? track.scrollHeight - track.clientHeight : track.scrollWidth - track.clientWidth);
        const maximum = maximumStart(offsets, maxDistance);
        state.distance = Math.min(offsets[Math.min(state.config.index, slides.length - 1)] || 0, maxDistance);
        if (!state.drag) { track.style.transform = vertical ? `translateY(${-state.distance}px)` : `translateX(${state.rtl ? state.distance : -state.distance}px)`; }
        if (state.maximum !== maximum) { state.maximum = maximum; void invoke('JsOnLayout', maximum); }
        // Measure visibility after translation; offscreen slides should not receive keyboard focus.
        const bounds = viewport.getBoundingClientRect();
        slides.forEach((slide, index) => {
            if (!state.slides.has(slide)) { state.slides.set(slide, { inert: slide.inert, hidden: slide.getAttribute('aria-hidden'), label: slide.getAttribute('aria-label') }); }
            const box = slide.getBoundingClientRect();
            const visible = vertical ? box.bottom > bounds.top + 1 && box.top < bounds.bottom - 1 : box.right > bounds.left + 1 && box.left < bounds.right - 1;
            slide.inert = !visible;
            slide.setAttribute('aria-hidden', visible ? 'false' : 'true');
            if (!state.slides.get(slide).label) { slide.setAttribute('aria-label', `${index + 1} / ${slides.length}`); }
        });
        track.setAttribute('aria-live', state.config.autoplay && !state.config.paused && !state.focusPaused && !reduced.matches ? 'off' : 'polite');
        state.schedule();
    };
    const resize = new ResizeObserver(() => state.layout()); resize.observe(viewport); resize.observe(track);
    const changes = new MutationObserver(() => state.layout()); changes.observe(track, { childList: true });
    state.cleanups.push(() => { resize.disconnect(); changes.disconnect(); });
    on(window, 'resize', () => state.layout());
    on(document, 'visibilitychange', () => state.schedule());
    on(reduced, 'change', () => { state.layout(); state.schedule(); });
    on(root, 'pointerenter', event => { if (event.pointerType !== 'touch') { state.hover = true; state.schedule(); } });
    on(root, 'pointerleave', () => { state.hover = false; state.schedule(); });
    on(root, 'focusin', event => {
        if (event.target.closest('[data-carousel-rotation]')) { return; }
        state.focusPaused = true; state.schedule(); state.layout();
        if (state.config.autoplay && !state.config.paused) { void invoke('JsOnPause'); }
    });
    const finishDrag = async (event, cancelled = false) => {
        const drag = state.drag;
        if (!drag) { return; }
        state.drag = null;
        if (viewport.hasPointerCapture?.(drag.id)) { viewport.releasePointerCapture(drag.id); }
        track.style.transition = '';
        if (!cancelled && state.didDrag) {
            const delta = state.config.vertical ? event.clientY - drag.y : event.clientX - drag.x;
            if (Math.abs(delta) >= 35) { await step((delta < 0 ? 1 : -1) * (state.rtl ? -1 : 1)); }
        }
        state.layout(); state.schedule();
    };
    on(viewport, 'pointerdown', event => {
        if (!state.config.draggable || event.button !== 0 || event.target.closest(interactive)) { return; }
        state.didDrag = false;
        state.drag = { id: event.pointerId, x: event.clientX, y: event.clientY };
        state.schedule();
    });
    on(viewport, 'pointermove', event => {
        const drag = state.drag;
        if (!drag || drag.id !== event.pointerId) { return; }
        const delta = state.config.vertical ? event.clientY - drag.y : event.clientX - drag.x;
        const cross = state.config.vertical ? event.clientX - drag.x : event.clientY - drag.y;
        if (!state.didDrag && Math.abs(cross) > Math.abs(delta) && Math.abs(cross) > 6) { void finishDrag(event, true); return; }
        if (Math.abs(delta) < 6 && !state.didDrag) { return; }
        state.didDrag = true;
        viewport.setPointerCapture?.(drag.id);
        event.preventDefault();
        track.style.transition = 'none';
        track.style.transform = state.config.vertical ? `translateY(${-state.distance + delta}px)` : `translateX(${(state.rtl ? state.distance : -state.distance) + delta}px)`;
    }, { passive: false });
    on(viewport, 'pointerup', event => { void finishDrag(event); });
    on(viewport, 'pointercancel', event => { void finishDrag(event, true); });
    on(viewport, 'lostpointercapture', event => { if (state.drag) { void finishDrag(event, true); } });
    on(viewport, 'click', event => { if (state.didDrag) { event.preventDefault(); event.stopImmediatePropagation(); state.didDrag = false; } }, true);
    on(viewport, 'dragstart', event => { if (state.config.draggable && !event.target.closest(interactive)) { event.preventDefault(); } });
    on(root, 'keydown', event => {
        if (event.target.closest(interactive) || event.altKey || event.ctrlKey || event.metaKey) { return; }
        const next = state.config.vertical ? 'ArrowDown' : state.rtl ? 'ArrowLeft' : 'ArrowRight';
        const previous = state.config.vertical ? 'ArrowUp' : state.rtl ? 'ArrowRight' : 'ArrowLeft';
        if (event.key === next || event.key === previous) { event.preventDefault(); void step(event.key === next ? 1 : -1); }
    });
    on(track, 'transitionend', () => state.layout());
    state.cleanups.push(() => {
        if (state.drag && viewport.hasPointerCapture?.(state.drag.id)) { viewport.releasePointerCapture(state.drag.id); }
        viewport.style.touchAction = ''; track.style.transform = ''; track.style.transition = ''; track.removeAttribute('aria-live');
        for (const [slide, before] of state.slides) {
            slide.inert = before.inert;
            for (const [name, value] of [['aria-hidden', before.hidden], ['aria-label', before.label]]) { if (value === null) { slide.removeAttribute(name); } else { slide.setAttribute(name, value); } }
        }
    });
    state.layout();
}
export function dispose(root) {
    const state = instances.get(root);
    if (!state) { return; }
    state.disposed = true; clearTimeout(state.timer);
    for (const cleanup of state.cleanups) { cleanup(); }
    instances.delete(root);
}

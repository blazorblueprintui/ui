// Pointer previews stay in the browser; only a completed gesture crosses the Blazor circuit.
const instances = new WeakMap();
const minute = 60_000;

export function calculateChange(action, original, source, target, deltaY, slotMinutes) {
    const step = slotMinutes * minute;
    const snap = value => target.start + Math.round((value - target.start) / step) * step;
    const delta = deltaY / 40 * step;
    let start = original.start;
    let end = original.end;
    if (action === 'move') {
        const duration = end - start;
        start = snap(target.start + original.start - source.start + delta);
        // Preserve overnight duration and the hidden leading portion of a clipped event.
        const hiddenMinutes = Math.max(0, source.start - original.start);
        const earliest = target.start + Math.ceil(-hiddenMinutes / step) * step;
        const latest = target.end - step;
        start = Math.max(earliest, Math.min(latest, start));
        end = start + duration;
    } else if (action === 'start') {
        const latest = Math.min(target.end - step, target.start + Math.floor((end - step - target.start) / step) * step);
        if (latest < target.start) return null;
        start = Math.max(target.start, Math.min(latest, snap(start + delta)));
    } else if (action === 'end') {
        const earliest = Math.max(target.start + step, target.start + Math.ceil((start + step - target.start) / step) * step);
        if (earliest > target.end) return null;
        end = Math.min(target.end, Math.max(earliest, snap(end + delta)));
    } else return null;
    return { start, end };
}

export function initialize(root, dotNet) {
    dispose(root);
    let gesture = null;
    let pending = false;
    let suppressClickUntil = 0;
    let frame = 0;
    const viewport = root.querySelector('[data-scheduler-scroll]');
    if (viewport && root.dataset.initialScrollTop != null) {
        viewport.scrollTop = Number(root.dataset.initialScrollTop);
    }
    const allowed = action => root.dataset[action === 'move' ? 'allowDrag' : 'allowResize'] === 'true';
    const bounds = lane => ({ start: Number(lane.dataset.start), end: Number(lane.dataset.end) });
    const endPreview = () => {
        cancelAnimationFrame(frame);
        frame = 0;
        const previous = gesture;
        gesture = null;
        previous?.preview?.remove();
        if (previous && previous.card.hasPointerCapture?.(previous.pointerId)) previous.card.releasePointerCapture(previous.pointerId);
        return previous;
    };
    const cancel = () => {
        if (gesture?.moved) suppressClickUntil = Date.now() + 500;
        endPreview();
    };
    const laneAt = (x, y) => {
        const lane = document.elementFromPoint(x, y)?.closest('[data-scheduler-lane]');
        return lane && root.contains(lane) ? lane : null;
    };
    const paint = () => {
        const g = gesture;
        if (!g?.moved) return;
        if (!allowed(g.action) || g.revision !== Number(root.dataset.revision) || !g.card.isConnected) {
            cancel();
            return;
        }
        const lane = g.action === 'move' ? laneAt(g.x, g.y) : g.source;
        g.target = lane;
        const sourceRect = g.source.getBoundingClientRect();
        const deltaY = g.y - g.initialY + g.initialTop - sourceRect.top;
        g.change = lane ? calculateChange(g.action, g.original, bounds(g.source), bounds(lane), deltaY, g.slotMinutes) : null;
        if (!g.change) {
            if (g.preview) g.preview.style.display = 'none';
            return;
        }
        if (!g.preview) {
            g.preview = g.card.cloneNode(true);
            g.preview.removeAttribute('id');
            g.preview.querySelectorAll('[id]').forEach(element => element.removeAttribute('id'));
            g.preview.removeAttribute('data-scheduler-event');
            g.preview.setAttribute('aria-hidden', 'true');
            g.preview.inert = true;
            document.body.append(g.preview);
        }
        const rect = lane.getBoundingClientRect();
        const range = bounds(lane);
        const scale = 40 / (g.slotMinutes * minute);
        const from = Math.max(range.start, g.change.start);
        const to = Math.min(range.end, g.change.end);
        const gutter = 4.5 * parseFloat(getComputedStyle(document.documentElement).fontSize);
        Object.assign(g.preview.style, {
            display: '', position: 'fixed', pointerEvents: 'none', zIndex: '100', margin: '0',
            left: `${rect.left + gutter + 4}px`, width: `${Math.max(20, rect.width - gutter - 8)}px`,
            top: `${rect.top + (from - range.start) * scale + 2}px`, height: `${Math.max(16, (to - from) * scale - 4)}px`,
            opacity: '0.85', backgroundColor: getComputedStyle(root).backgroundColor === 'rgba(0, 0, 0, 0)' ? 'var(--background)' : getComputedStyle(root).backgroundColor
        });
        const label = g.preview.querySelector('[data-scheduler-event-time]');
        if (label) {
            const format = new Intl.DateTimeFormat(undefined, { timeStyle: 'short', timeZone: root.dataset.timeZone });
            label.textContent = `${format.format(g.change.start)}–${format.format(g.change.end)}`;
        }
    };
    const autoScroll = () => {
        if (!gesture?.moved) return;
        const rect = viewport.getBoundingClientRect();
        const { x, y } = gesture;
        if (x >= rect.left && x <= rect.right && y >= rect.top && y <= rect.bottom) {
            viewport.scrollTop += y > rect.bottom - 32 ? 10 : y < rect.top + 32 ? -10 : 0;
            viewport.scrollLeft += x > rect.right - 32 ? 10 : x < rect.left + 32 ? -10 : 0;
        }
        paint();
        if (gesture) frame = requestAnimationFrame(autoScroll);
    };
    const down = event => {
        if (event.button !== 0 || event.isPrimary === false || pending || gesture) return;
        const card = event.target.closest('[data-scheduler-event]');
        if (!card || !root.contains(card)) return;
        // Custom event templates may contain their own controls.
        if (event.target.closest('input,select,textarea,a,[contenteditable="true"]')) return;
        const action = event.target.closest('[data-scheduler-resize]')?.dataset.schedulerResize ?? 'move';
        if (!allowed(action)) return;
        const source = card.closest('[data-scheduler-lane]');
        gesture = {
            card, source, action, pointerId: event.pointerId, revision: Number(root.dataset.revision),
            original: bounds(card), slotMinutes: Number(root.dataset.slotMinutes),
            initialX: event.clientX, initialY: event.clientY, initialTop: source.getBoundingClientRect().top,
            x: event.clientX, y: event.clientY, moved: false
        };
        card.setPointerCapture(event.pointerId);
    };
    const move = event => {
        if (!gesture || gesture.pointerId !== event.pointerId) return;
        gesture.x = event.clientX;
        gesture.y = event.clientY;
        if (!gesture.moved && Math.hypot(gesture.x - gesture.initialX, gesture.y - gesture.initialY) < 5) return;
        event.preventDefault();
        if (!gesture.moved) {
            gesture.moved = true;
            frame = requestAnimationFrame(autoScroll);
        }
        paint();
    };
    const up = async event => {
        if (!gesture || gesture.pointerId !== event.pointerId) return;
        if (gesture.moved) {
            gesture.x = event.clientX;
            gesture.y = event.clientY;
            paint();
        }
        const g = endPreview();
        if (!g?.moved) return;
        suppressClickUntil = Date.now() + 500;
        event.preventDefault();
        if (!g.change || !g.target || !allowed(g.action)) return;
        if (g.change.start === g.original.start && g.change.end === g.original.end && g.source === g.target) return;
        pending = true;
        try {
            await dotNet.invokeMethodAsync('CommitInteractionAsync', g.revision, g.card.dataset.schedulerEvent, g.original.start,
                Number(g.source.dataset.schedulerLane), Number(g.target.dataset.schedulerLane), g.change.start, g.change.end, g.action);
        } catch {
            // A disconnected circuit cannot commit. The original DOM was never moved.
        } finally { pending = false; }
    };
    const click = event => {
        if (Date.now() < suppressClickUntil) { event.preventDefault(); event.stopImmediatePropagation(); }
    };
    const key = event => {
        if (event.key === 'Escape' && gesture) { event.preventDefault(); event.stopPropagation(); cancel(); }
    };
    const pointerCancel = event => { if (gesture?.pointerId === event.pointerId) cancel(); };
    const dragStart = event => { if (gesture) event.preventDefault(); };
    root.addEventListener('pointerdown', down);
    root.addEventListener('pointermove', move);
    root.addEventListener('pointerup', up);
    root.addEventListener('pointercancel', pointerCancel);
    root.addEventListener('lostpointercapture', pointerCancel);
    root.addEventListener('click', click, true);
    root.addEventListener('dragstart', dragStart);
    document.addEventListener('keydown', key, true);
    window.addEventListener('blur', cancel);
    instances.set(root, () => {
        cancel();
        root.removeEventListener('pointerdown', down);
        root.removeEventListener('pointermove', move);
        root.removeEventListener('pointerup', up);
        root.removeEventListener('pointercancel', pointerCancel);
        root.removeEventListener('lostpointercapture', pointerCancel);
        root.removeEventListener('click', click, true);
        root.removeEventListener('dragstart', dragStart);
        document.removeEventListener('keydown', key, true);
        window.removeEventListener('blur', cancel);
    });
}

export function dispose(root) {
    instances.get(root)?.();
    instances.delete(root);
}

export function restoreDeleteFocus(owner) {
    let attempts = 0;
    const restore = () => {
        const button = Array.from(document.querySelectorAll('[data-scheduler-delete]'))
            .find(candidate => candidate.dataset.schedulerDelete === owner);
        if (!button || button.disabled) return;
        // Nested modal cleanup may still have its parent inert during this render.
        if (button.closest('[inert], [aria-hidden="true"]')) {
            if (++attempts < 60) requestAnimationFrame(restore);
            return;
        }
        button.focus({ preventScroll: true });
    };
    requestAnimationFrame(() => requestAnimationFrame(restore));
}

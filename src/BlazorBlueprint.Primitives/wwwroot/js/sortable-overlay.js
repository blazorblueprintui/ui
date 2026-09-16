import { inheritTheme } from './theme-scope.js';

// The preview is decorative and never moves or mounts a Blazor-owned node.
export function createDragOverlay(item, initialEvent) {
    const template = item.querySelector('[data-bb-sortable-overlay]');
    if (!template) return null;
    const overlay = template.cloneNode(true);
    overlay.hidden = false;
    overlay.inert = true;
    overlay.setAttribute('aria-hidden', 'true');
    overlay.removeAttribute('id');
    overlay.querySelectorAll('[id]').forEach(element => element.removeAttribute('id'));
    Object.assign(overlay.style, { position: 'fixed', top: '0', left: '0', zIndex: '10001', pointerEvents: 'none', margin: '0', maxWidth: 'calc(100vw - 24px)' });
    document.body.appendChild(overlay);
    const restoreTheme = inheritTheme(item, overlay);
    const move = event => {
        const point = event?.touches?.[0] || event;
        if (point?.clientX == null) return;
        overlay.style.transform = `translate(${Math.max(0, Math.min(point.clientX + 12, window.innerWidth - overlay.offsetWidth - 8))}px, ${Math.max(0, Math.min(point.clientY + 12, window.innerHeight - overlay.offsetHeight - 8))}px)`;
    };
    document.addEventListener('pointermove', move, { passive: true });
    document.addEventListener('touchmove', move, { passive: true });
    move(initialEvent);
    return () => {
        document.removeEventListener('pointermove', move);
        document.removeEventListener('touchmove', move);
        restoreTheme();
        overlay.remove();
    };
}

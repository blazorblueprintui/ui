// Root-level keyboard navigation and outside dismissal for headless menus.
const registrations = new WeakMap();

function isInsideMenu(root, target) {
    const visited = new Set();
    while (target) {
        if (root.contains(target)) return true;
        // Portaled submenus are DOM siblings. Follow their labelled trigger back to the owner.
        const menu = target.closest?.('[role="menu"][aria-labelledby]');
        if (!menu || visited.has(menu)) return false;
        visited.add(menu);
        target = document.getElementById(menu.getAttribute('aria-labelledby'));
        if (!target?.hasAttribute('data-bb-submenu-trigger')) return false;
    }
    return false;
}

export function initialize(root, navigation = false, enabled = true, dotNetRef = null) {
    let state = registrations.get(root);
    if (state) { state.enabled = enabled; return; }
    state = { enabled, pending: null };
    const triggers = () => [...root.querySelectorAll(navigation ? '[data-nav-trigger]' : 'button[aria-haspopup="menu"]')]
        .filter(el => !el.disabled && el.closest(navigation ? 'nav' : '[role="menubar"]') === root);
    const panelFor = trigger => navigation ? trigger.closest('[data-nav-item]')?.querySelector('[role="menu"]') : null;
    const focusPending = () => {
        if (!state.pending) return;
        const panel = panelFor(state.pending.trigger);
        const links = panel ? [...panel.querySelectorAll('[role="menuitem"]')].filter(el => el.getAttribute('aria-disabled') !== 'true') : [];
        if (links.length) { (state.pending.last ? links.at(-1) : links[0]).focus(); state.pending = null; }
    };
    const keydown = event => {
        if (navigation && !state.enabled) return;
        const list = triggers();
        const trigger = event.target.closest(navigation ? '[data-nav-trigger]' : 'button[aria-haspopup="menu"]');
        if (trigger && list.includes(trigger)) {
            if (!navigation && event.key === 'Escape' && trigger.getAttribute('aria-expanded') === 'true') {
                event.preventDefault(); trigger.click(); trigger.focus(); return;
            }
            const index = list.indexOf(trigger);
            const rtl = getComputedStyle(root).direction === 'rtl';
            let next;
            if (event.key === 'ArrowRight') next = (index + (rtl ? -1 : 1) + list.length) % list.length;
            if (event.key === 'ArrowLeft') next = (index + (rtl ? 1 : -1) + list.length) % list.length;
            if (event.key === 'Home') next = 0;
            if (event.key === 'End') next = list.length - 1;
            if (next !== undefined) { event.preventDefault(); list[next].focus(); return; }
            if (['ArrowDown', 'ArrowUp'].includes(event.key)) {
                event.preventDefault();
                if (navigation) state.pending = { trigger, last: event.key === 'ArrowUp' };
                if (trigger.getAttribute('aria-expanded') !== 'true') trigger.click();
                focusPending();
            }
        }
        if (!navigation) return; // Menubar panels have their own keyboard controller.
        const item = event.target.closest('[data-nav-item]');
        const owner = item?.querySelector('[data-nav-trigger]');
        if (event.key === 'Escape' && owner) {
            event.preventDefault(); event.stopPropagation(); state.pending = null;
            owner.focus(); dotNetRef?.invokeMethodAsync('JsCloseMenus').catch(() => {});
        }
        const panel = event.target.closest('[role="menu"]');
        if (!panel) return;
        const links = [...panel.querySelectorAll('[role="menuitem"]')].filter(el => el.getAttribute('aria-disabled') !== 'true');
        const index = links.indexOf(event.target.closest('[role="menuitem"]'));
        let next;
        if (event.key === 'ArrowDown') next = (index + 1) % links.length;
        if (event.key === 'ArrowUp') next = (index - 1 + links.length) % links.length;
        if (event.key === 'Home') next = 0;
        if (event.key === 'End') next = links.length - 1;
        if (links[next]) { event.preventDefault(); links[next].focus(); }
    };
    const outside = event => {
        if (navigation ? root.contains(event.target) : isInsideMenu(root, event.target)) return;
        if (navigation) dotNetRef?.invokeMethodAsync('JsCloseMenus').catch(() => {});
        else triggers().find(el => el.getAttribute('aria-expanded') === 'true')?.click();
    };
    const observer = new MutationObserver(focusPending);
    observer.observe(root, { childList: true, subtree: true, attributes: true, attributeFilter: ['data-state'] });
    root.addEventListener('keydown', keydown);
    document.addEventListener('pointerdown', outside);
    state.dispose = () => { observer.disconnect(); root.removeEventListener('keydown', keydown); document.removeEventListener('pointerdown', outside); };
    registrations.set(root, state);
}
export function dispose(root) { registrations.get(root)?.dispose(); registrations.delete(root); }

// Arrow keys move focus locally; activating an item uses the component's normal click handler.
const instances = new Map();
const itemSelector = '[data-cascader-item]';
const columnSelector = '[data-cascader-column]';

function stateFor(id) {
    if (!instances.has(id)) instances.set(id, {});
    return instances.get(id);
}

function itemsIn(container) {
    return Array.from(container?.querySelectorAll(itemSelector) ?? []).filter(item => !item.disabled);
}

function isOpen(state) {
    return state.popup?.isConnected && state.popup.closest('[data-state]')?.dataset.state === 'open';
}

function focusItem(state, item) {
    if (!item || !isOpen(state)) return;
    for (const candidate of itemsIn(state.popup)) candidate.tabIndex = candidate === item ? 0 : -1;
    item.focus({ preventScroll: true });
    item.scrollIntoView({ block: 'nearest', inline: 'nearest' });
}

function syncItems(state) {
    if (!isOpen(state)) return;
    const items = itemsIn(state.popup);
    const active = items.includes(document.activeElement) ? document.activeElement : null;
    const entry = active ?? items.find(item => item.getAttribute('aria-current') === 'true') ?? items[0];
    for (const item of items) item.tabIndex = item === entry ? 0 : -1;

    // Opening a branch on Server renders its children later. Wait for the matching column,
    // rather than focusing stale children from the branch that previously occupied this level.
    if (state.pendingChild) {
        const columns = Array.from(state.popup.querySelectorAll(columnSelector));
        const child = columns.find(column => column.dataset.cascaderParent === state.pendingChild);
        const first = itemsIn(child)[0];
        if (first) {
            state.pendingChild = null;
            focusItem(state, first);
        }
    }
}

function enterBranch(state, item, activate = false) {
    state.pendingChild = item.dataset.cascaderItem;
    if (activate || item.getAttribute('aria-expanded') !== 'true') item.click();
    syncItems(state);
}

function focusEntry(state, last = false) {
    const columns = state.popup?.querySelectorAll(columnSelector);
    const items = itemsIn(columns?.[0]);
    focusItem(state, last ? items.at(-1) : items[0]);
}

function onKeyDown(state, event) {
    if (!isOpen(state) || event.altKey || event.ctrlKey || event.metaKey || event.isComposing) return;
    state.pendingChild = null;
    const search = event.target.closest('[data-cascader-search]');
    if (search) {
        if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
            event.preventDefault();
            focusEntry(state, event.key === 'ArrowUp');
        } else if (event.key === 'Enter') {
            // Do not submit an enclosing form while searching.
            event.preventDefault();
            const first = itemsIn(state.popup)[0];
            if (first?.hasAttribute('aria-expanded')) enterBranch(state, first, true);
            else first?.click();
        }
        return;
    }

    const item = event.target.closest(itemSelector);
    if (!item) return;
    const column = item.closest(columnSelector);
    const items = itemsIn(column);
    const current = items.indexOf(item);
    const rtl = getComputedStyle(state.popup).direction === 'rtl';
    const forward = rtl ? 'ArrowLeft' : 'ArrowRight';
    const back = rtl ? 'ArrowRight' : 'ArrowLeft';
    switch (event.key) {
        case 'ArrowDown':
        case 'ArrowUp': {
            event.preventDefault();
            const delta = event.key === 'ArrowDown' ? 1 : -1;
            focusItem(state, items[(current + delta + items.length) % items.length]);
            break;
        }
        case 'Home':
        case 'End':
            event.preventDefault();
            focusItem(state, event.key === 'Home' ? items[0] : items.at(-1));
            break;
        case forward:
            event.preventDefault();
            if (item.hasAttribute('aria-expanded')) enterBranch(state, item);
            break;
        case back: {
            event.preventDefault();
            const parent = itemsIn(state.popup).find(candidate => candidate.dataset.cascaderItem === column.dataset.cascaderParent);
            if (parent) focusItem(state, parent);
            else state.popup.querySelector('[data-cascader-search]')?.focus();
            break;
        }
        case 'Enter':
        case ' ':
            event.preventDefault();
            if (item.hasAttribute('aria-expanded')) enterBranch(state, item, true);
            else item.click();
            break;
        // Escape belongs to the overlay's shared dismissal stack; Tab leaves normally.
    }
}

export function initialize(container, popupId) {
    const state = stateFor(popupId);
    state.releaseTrigger?.();
    const trigger = container.querySelector('[data-cascader-trigger]');
    if (!trigger) return;
    const handler = event => {
        if (trigger.disabled || event.altKey || event.ctrlKey || event.metaKey || !['ArrowDown', 'ArrowUp'].includes(event.key)) return;
        event.preventDefault();
        state.initialLast = event.key === 'ArrowUp';
        if (isOpen(state)) {
            focusEntry(state, state.initialLast);
            state.initialLast = undefined;
        } else trigger.click();
    };
    trigger.addEventListener('keydown', handler);
    state.releaseTrigger = () => trigger.removeEventListener('keydown', handler);
}

export function connect(popupId) {
    const state = stateFor(popupId);
    state.releasePopup?.();
    const popup = document.getElementById(popupId);
    if (!popup) return;
    state.popup = popup;
    state.pendingChild = null;
    const keydown = event => onKeyDown(state, event);
    const focusin = event => {
        const item = event.target.closest(itemSelector);
        if (item) for (const candidate of itemsIn(popup)) candidate.tabIndex = candidate === item ? 0 : -1;
    };
    const focusInitial = () => requestAnimationFrame(() => requestAnimationFrame(() => {
        if (state.popup !== popup || !isOpen(state) || state.initialLast === undefined) return;
        focusEntry(state, state.initialLast);
        state.initialLast = undefined;
    }));
    const portal = popup.closest('[data-bb-portal]');
    popup.addEventListener('keydown', keydown);
    popup.addEventListener('focusin', focusin);
    portal?.addEventListener('blazorblueprint:visible', focusInitial);
    const observer = new MutationObserver(() => syncItems(state));
    observer.observe(popup, { childList: true, subtree: true, attributes: true, attributeFilter: ['aria-expanded', 'aria-current'] });
    state.releasePopup = () => {
        popup.removeEventListener('keydown', keydown);
        popup.removeEventListener('focusin', focusin);
        portal?.removeEventListener('blazorblueprint:visible', focusInitial);
        observer.disconnect();
    };
    syncItems(state);
    focusInitial();
}

export function dispose(popupId) {
    const state = instances.get(popupId);
    state?.releaseTrigger?.();
    state?.releasePopup?.();
    if (state) state.popup = null;
    instances.delete(popupId);
}

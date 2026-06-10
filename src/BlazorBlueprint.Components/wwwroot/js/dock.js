// Docking interactions for BbDock.
//
// Handles three pointer-driven gestures:
//   1. Dragging a panel tab to re-dock it (as a tab or a split), to a dock edge, or out to a
//      floating window. Draws a live drop indicator and reports the resolved drop to .NET.
//   2. Dragging a floating window by its tab strip to reposition it.
//   3. Dragging a floating window's resize grip to resize it.
//
// .NET owns the layout model; this module only computes drop targets and reports results.

const docks = new Map();
const DRAG_THRESHOLD = 4;
const ACCENT_BG = "rgba(59, 130, 246, 0.28)";
const ACCENT_BORDER = "2px solid rgba(59, 130, 246, 0.9)";

export function initializeDock(dockId, rootEl, dotNetRef) {
    if (!dockId || !rootEl || !dotNetRef) {
        return;
    }
    docks.set(dockId, { rootEl, dotNetRef });
}

export function disposeDock(dockId) {
    docks.delete(dockId);
}

// ---------------------------------------------------------------- shared pointer session

function attachSession(handlers) {
    const onMove = (e) => handlers.move(e);
    const onUp = (e) => {
        cleanup();
        handlers.up(e);
    };
    const cleanup = () => {
        document.removeEventListener("pointermove", onMove);
        document.removeEventListener("pointerup", onUp);
        document.removeEventListener("pointercancel", onUp);
        document.body.style.userSelect = "";
        document.body.style.cursor = "";
    };

    document.addEventListener("pointermove", onMove);
    document.addEventListener("pointerup", onUp);
    document.addEventListener("pointercancel", onUp);
    document.body.style.userSelect = "none";
    return cleanup;
}

// ---------------------------------------------------------------- tab drag → docking

export function startTabDrag(dockId, title, clientX, clientY, pointerId) {
    const dock = docks.get(dockId);
    if (!dock) {
        return;
    }

    const state = {
        dock,
        title: title || "Panel",
        pointerId,
        startX: clientX,
        startY: clientY,
        active: false,
        ghost: null,
        indicator: null,
        target: null
    };

    attachSession({
        move: (e) => {
            if (e.pointerId !== pointerId) {
                return;
            }
            onTabMove(state, e);
        },
        up: (e) => {
            if (e.pointerId !== pointerId) {
                return;
            }
            onTabUp(state, e);
        }
    });
}

function onTabMove(state, e) {
    if (!state.active) {
        const dx = e.clientX - state.startX;
        const dy = e.clientY - state.startY;
        if (Math.hypot(dx, dy) < DRAG_THRESHOLD) {
            return;
        }
        state.active = true;
        state.ghost = createGhost(state.title);
        document.body.style.cursor = "default";
    }

    moveGhost(state.ghost, e.clientX, e.clientY);
    state.target = computeTarget(state.dock, e.clientX, e.clientY);
    drawIndicator(state, state.target);
}

function onTabUp(state, e) {
    removeEl(state.ghost);
    removeEl(state.indicator);
    state.ghost = null;
    state.indicator = null;

    if (!state.active) {
        // No movement past the threshold — treat as a click, not a drag.
        return;
    }

    const t = state.target || { type: "none" };
    let floatX = 0;
    let floatY = 0;

    if (t.type === "float") {
        const r = state.dock.rootEl.getBoundingClientRect();
        floatX = e.clientX - r.left - 24;
        floatY = e.clientY - r.top - 12;
    }

    state.dock.dotNetRef
        .invokeMethodAsync("OnTabDropped", t.type, t.groupId || null, t.zone || "center", floatX, floatY, typeof t.index === "number" ? t.index : -1)
        .catch(() => { });
}

function computeTarget(dock, x, y) {
    const r = dock.rootEl.getBoundingClientRect();

    if (x < r.left || x > r.right || y < r.top || y > r.bottom) {
        return { type: "float" };
    }

    // Hovering a tab strip reorders within (or into) that group without changing the layout.
    const stripEl = findClosestAt(dock, x, y, "[data-dock-tabstrip]");
    if (stripEl) {
        return {
            type: "reorder",
            groupId: stripEl.getAttribute("data-dock-tabstrip"),
            index: computeInsertIndex(stripEl, x)
        };
    }

    // A band along the dock's outer border docks against the whole dock.
    const edge = 26;
    if (x - r.left < edge) {
        return { type: "root", zone: "left" };
    }
    if (r.right - x < edge) {
        return { type: "root", zone: "right" };
    }
    if (y - r.top < edge) {
        return { type: "root", zone: "top" };
    }
    if (r.bottom - y < edge) {
        return { type: "root", zone: "bottom" };
    }

    const groupEl = findGroupAt(dock, x, y);
    if (!groupEl) {
        return { type: "float" };
    }

    const gid = groupEl.getAttribute("data-dock-group");
    const zone = zoneWithin(groupEl.getBoundingClientRect(), x, y);
    return { type: "group", groupId: gid, zone };
}

function findGroupAt(dock, x, y) {
    return findClosestAt(dock, x, y, "[data-dock-group]");
}

function findClosestAt(dock, x, y, selector) {
    const els = document.elementsFromPoint(x, y);
    for (const el of els) {
        if (!el.closest) {
            continue;
        }
        const match = el.closest(selector);
        if (match && dock.rootEl.contains(match)) {
            return match;
        }
    }
    return null;
}

// The insertion slot (0..tabCount) for a pointer x over a tab strip, using each tab's midpoint.
function computeInsertIndex(stripEl, x) {
    const tabs = stripEl.querySelectorAll("[data-dock-tab]");
    for (let i = 0; i < tabs.length; i++) {
        const tr = tabs[i].getBoundingClientRect();
        if (x < tr.left + tr.width / 2) {
            return i;
        }
    }
    return tabs.length;
}

function zoneWithin(r, x, y) {
    const rx = (x - r.left) / r.width;
    const ry = (y - r.top) / r.height;
    const m = 0.22;
    const inMidX = rx > m && rx < 1 - m;
    const inMidY = ry > m && ry < 1 - m;

    if (rx < m && inMidY) {
        return "left";
    }
    if (rx > 1 - m && inMidY) {
        return "right";
    }
    if (ry < m && inMidX) {
        return "top";
    }
    if (ry > 1 - m && inMidX) {
        return "bottom";
    }
    return "center";
}

function drawIndicator(state, t) {
    if (!t || t.type === "none" || t.type === "float") {
        if (state.indicator) {
            state.indicator.style.display = "none";
        }
        return;
    }

    if (!state.indicator) {
        const ind = document.createElement("div");
        ind.style.cssText =
            "position:fixed;z-index:10001;pointer-events:none;border-radius:6px;box-sizing:border-box;" +
            "transition:left .07s ease,top .07s ease,width .07s ease,height .07s ease;";
        ind.style.background = ACCENT_BG;
        ind.style.border = ACCENT_BORDER;
        document.body.appendChild(ind);
        state.indicator = ind;
    }

    const ind = state.indicator;
    ind.style.display = "block";

    let box;
    if (t.type === "reorder") {
        const stripEl = state.dock.rootEl.querySelector(`[data-dock-tabstrip="${t.groupId}"]`);
        if (!stripEl) {
            ind.style.display = "none";
            return;
        }
        box = insertionLineBox(stripEl, t.index);
        // A solid caret line between tabs rather than a translucent fill.
        ind.style.background = "rgba(59, 130, 246, 0.95)";
        ind.style.border = "none";
    } else {
        ind.style.background = ACCENT_BG;
        ind.style.border = ACCENT_BORDER;
        if (t.type === "root") {
            box = zoneBox(state.dock.rootEl.getBoundingClientRect(), t.zone, 0.3);
        } else {
            const gEl = state.dock.rootEl.querySelector(`[data-dock-group="${t.groupId}"]`);
            if (!gEl) {
                ind.style.display = "none";
                return;
            }
            box = zoneBox(gEl.getBoundingClientRect(), t.zone, 0.5);
        }
    }

    ind.style.left = `${box.left}px`;
    ind.style.top = `${box.top}px`;
    ind.style.width = `${box.width}px`;
    ind.style.height = `${box.height}px`;
}

function insertionLineBox(stripEl, index) {
    const tabs = stripEl.querySelectorAll("[data-dock-tab]");
    const sr = stripEl.getBoundingClientRect();
    let lineX;
    if (tabs.length === 0) {
        lineX = sr.left;
    } else if (index >= tabs.length) {
        lineX = tabs[tabs.length - 1].getBoundingClientRect().right;
    } else {
        lineX = tabs[index].getBoundingClientRect().left;
    }
    return { left: lineX - 1, top: sr.top, width: 2, height: sr.height };
}

function zoneBox(r, zone, frac) {
    switch (zone) {
        case "left":
            return { left: r.left, top: r.top, width: r.width * frac, height: r.height };
        case "right":
            return { left: r.right - r.width * frac, top: r.top, width: r.width * frac, height: r.height };
        case "top":
            return { left: r.left, top: r.top, width: r.width, height: r.height * frac };
        case "bottom":
            return { left: r.left, top: r.bottom - r.height * frac, width: r.width, height: r.height * frac };
        default:
            return { left: r.left, top: r.top, width: r.width, height: r.height };
    }
}

function createGhost(title) {
    const g = document.createElement("div");
    g.textContent = title;
    g.style.cssText =
        "position:fixed;z-index:10002;pointer-events:none;padding:4px 10px;font-size:12px;font-weight:500;" +
        "border-radius:6px;background:#1f2937;color:#fff;box-shadow:0 6px 20px rgba(0,0,0,0.28);opacity:.92;" +
        "white-space:nowrap;transform:translate(10px,10px);";
    document.body.appendChild(g);
    return g;
}

function moveGhost(g, x, y) {
    if (g) {
        g.style.left = `${x}px`;
        g.style.top = `${y}px`;
    }
}

function removeEl(el) {
    if (el && el.parentNode) {
        el.parentNode.removeChild(el);
    }
}

// ---------------------------------------------------------------- floating window move

export function startWindowDrag(dockId, windowId, clientX, clientY, pointerId) {
    const dock = docks.get(dockId);
    if (!dock) {
        return;
    }

    const winEl = dock.rootEl.querySelector(`[data-dock-window="${windowId}"]`);
    if (!winEl) {
        return;
    }

    const rootRect = dock.rootEl.getBoundingClientRect();
    const startLeft = parseFloat(winEl.style.left) || 0;
    const startTop = parseFloat(winEl.style.top) || 0;
    const offsetX = clientX - (rootRect.left + startLeft);
    const offsetY = clientY - (rootRect.top + startTop);

    attachSession({
        move: (e) => {
            if (e.pointerId !== pointerId) {
                return;
            }
            const left = Math.max(0, e.clientX - rootRect.left - offsetX);
            const top = Math.max(0, e.clientY - rootRect.top - offsetY);
            winEl.style.left = `${left}px`;
            winEl.style.top = `${top}px`;
        },
        up: (e) => {
            if (e.pointerId !== pointerId) {
                return;
            }
            const left = parseFloat(winEl.style.left) || 0;
            const top = parseFloat(winEl.style.top) || 0;
            dock.dotNetRef.invokeMethodAsync("OnWindowMoved", windowId, left, top).catch(() => { });
        }
    });
    document.body.style.cursor = "move";
}

// ---------------------------------------------------------------- floating window resize

export function startWindowResize(dockId, windowId, clientX, clientY, pointerId) {
    const dock = docks.get(dockId);
    if (!dock) {
        return;
    }

    const winEl = dock.rootEl.querySelector(`[data-dock-window="${windowId}"]`);
    if (!winEl) {
        return;
    }

    const startWidth = winEl.offsetWidth;
    const startHeight = winEl.offsetHeight;
    const startX = clientX;
    const startY = clientY;

    attachSession({
        move: (e) => {
            if (e.pointerId !== pointerId) {
                return;
            }
            const width = Math.max(180, startWidth + (e.clientX - startX));
            const height = Math.max(120, startHeight + (e.clientY - startY));
            winEl.style.width = `${width}px`;
            winEl.style.height = `${height}px`;
        },
        up: (e) => {
            if (e.pointerId !== pointerId) {
                return;
            }
            dock.dotNetRef
                .invokeMethodAsync("OnWindowResized", windowId, winEl.offsetWidth, winEl.offsetHeight)
                .catch(() => { });
        }
    });
    document.body.style.cursor = "nwse-resize";
}

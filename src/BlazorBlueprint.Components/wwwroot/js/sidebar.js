/**
 * Sidebar JavaScript module
 * Handles mobile detection, keyboard shortcuts, and state persistence
 *
 * ES modules are singletons, so every SidebarProvider on the page shares this
 * module. State is therefore held per instance rather than at module level, and
 * the keydown listener and ResizeObserver are shared and fan out to instances.
 */

const MOBILE_BREAKPOINT = 768;

let nextInstanceId = 1;
const instances = new Map();

let resizeObserver = null;
let keyboardHandler = null;

/**
 * Initialize sidebar with mobile detection and keyboard shortcuts
 * @param {DotNetObject} componentRef - Reference to the SidebarProvider component
 * @param {boolean} enableToggleShortcut - Whether Ctrl/Cmd + B toggles the sidebar
 * @returns {number} Instance id, passed back to setToggleShortcutEnabled and cleanup
 */
export function initializeSidebar(componentRef, enableToggleShortcut) {
    const instanceId = nextInstanceId++;

    instances.set(instanceId, {
        dotNetRef: componentRef,
        shortcutEnabled: enableToggleShortcut !== false
    });

    attachSharedListeners();

    // Push the current mobile state to the new instance
    notifyMobile(componentRef);

    return instanceId;
}

/**
 * Enable or disable the toggle shortcut after initialization
 * @param {number} instanceId - Instance id returned by initializeSidebar
 * @param {boolean} enabled - Whether Ctrl/Cmd + B toggles the sidebar
 */
export function setToggleShortcutEnabled(instanceId, enabled) {
    const instance = instances.get(instanceId);
    if (instance) {
        instance.shortcutEnabled = enabled !== false;
    }
}

/**
 * Get sidebar state from cookie
 * @param {string} key - Cookie key
 * @returns {boolean|null} The saved state or null if not found
 */
export function getSidebarState(key) {
    const value = getCookie(key);
    if (value === 'true') return true;
    if (value === 'false') return false;
    return null;
}

/**
 * Save sidebar state to cookie
 * @param {string} key - Cookie key
 * @param {boolean} value - State to save
 */
export function saveSidebarState(key, value) {
    setCookie(key, value.toString(), 7); // 7 days expiration
}

/**
 * Attach the shared keydown listener and ResizeObserver on first use
 */
function attachSharedListeners() {
    if (!keyboardHandler) {
        keyboardHandler = (e) => {
            // Check for Ctrl+B or Cmd+B
            if (!((e.ctrlKey || e.metaKey) && e.key === 'b')) {
                return;
            }

            let handled = false;

            for (const instance of instances.values()) {
                if (!instance.shortcutEnabled) {
                    continue;
                }

                handled = true;
                invoke(instance.dotNetRef, 'OnToggleShortcut');
            }

            // Only swallow the key when something acted on it, so that a page
            // with the shortcut disabled still gets its native Ctrl/Cmd + B
            if (handled) {
                e.preventDefault();
            }
        };

        document.addEventListener('keydown', keyboardHandler);
    }

    if (!resizeObserver) {
        resizeObserver = new ResizeObserver(() => {
            for (const instance of instances.values()) {
                notifyMobile(instance.dotNetRef);
            }
        });

        resizeObserver.observe(document.body);
    }
}

/**
 * Send the current mobile state to a single instance
 * @param {DotNetObject} dotNetRef - Reference to the SidebarProvider component
 */
function notifyMobile(dotNetRef) {
    invoke(dotNetRef, 'OnMobileChange', window.innerWidth < MOBILE_BREAKPOINT);
}

/**
 * Invoke a .NET method, ignoring failures from a disposed component or circuit.
 * Without this a single torn-down instance would break the shared fan-out loop.
 * @param {DotNetObject} dotNetRef - Reference to the SidebarProvider component
 * @param {string} method - JSInvokable method name
 * @param {...any} args - Arguments to forward
 */
function invoke(dotNetRef, method, ...args) {
    try {
        const result = dotNetRef.invokeMethodAsync(method, ...args);
        if (result && typeof result.catch === 'function') {
            result.catch(() => { });
        }
    } catch {
        // Instance is gone; cleanup will remove it
    }
}

/**
 * Get cookie value
 * @param {string} name - Cookie name
 * @returns {string|null} Cookie value or null
 */
function getCookie(name) {
    const nameEQ = name + "=";
    const ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) === ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

/**
 * Set cookie value
 * @param {string} name - Cookie name
 * @param {string} value - Cookie value
 * @param {number} days - Expiration in days
 */
function setCookie(name, value, days) {
    let expires = "";
    if (days) {
        const date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/; SameSite=Lax";
}

/**
 * Remove an instance, tearing down the shared listeners once the last one goes
 * @param {number} instanceId - Instance id returned by initializeSidebar
 */
export function cleanup(instanceId) {
    instances.delete(instanceId);

    if (instances.size > 0) {
        return;
    }

    if (resizeObserver) {
        resizeObserver.disconnect();
        resizeObserver = null;
    }

    if (keyboardHandler) {
        document.removeEventListener('keydown', keyboardHandler);
        keyboardHandler = null;
    }
}

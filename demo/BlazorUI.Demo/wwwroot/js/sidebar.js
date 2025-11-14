/**
 * Sidebar JavaScript module
 * Handles mobile detection, keyboard shortcuts, and state persistence
 */

const MOBILE_BREAKPOINT = 768;

let dotNetRef = null;
let cookieKey = null;
let resizeObserver = null;
let keyboardHandler = null;

/**
 * Initialize sidebar with mobile detection and keyboard shortcuts
 * @param {DotNetObject} componentRef - Reference to the SidebarProvider component
 * @param {string} key - Cookie key for state persistence
 */
export function initializeSidebar(componentRef, key) {
    dotNetRef = componentRef;
    cookieKey = key;

    // Set up mobile detection
    setupMobileDetection();

    // Set up keyboard shortcuts
    setupKeyboardShortcuts();
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
 * Set up mobile detection using ResizeObserver
 */
function setupMobileDetection() {
    if (!dotNetRef) return;

    const checkMobile = () => {
        const isMobile = window.innerWidth < MOBILE_BREAKPOINT;
        dotNetRef.invokeMethodAsync('OnMobileChange', isMobile);
    };

    // Initial check
    checkMobile();

    // Listen for resize events
    resizeObserver = new ResizeObserver(() => {
        checkMobile();
    });

    resizeObserver.observe(document.body);
}

/**
 * Set up keyboard shortcuts for toggling sidebar
 */
function setupKeyboardShortcuts() {
    if (!dotNetRef) return;

    keyboardHandler = (e) => {
        // Check for Ctrl+B or Cmd+B
        if ((e.ctrlKey || e.metaKey) && e.key === 'b') {
            e.preventDefault();
            dotNetRef.invokeMethodAsync('OnToggleShortcut');
        }
    };

    document.addEventListener('keydown', keyboardHandler);
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
 * Cleanup event listeners and observers
 */
export function cleanup() {
    if (resizeObserver) {
        resizeObserver.disconnect();
        resizeObserver = null;
    }

    if (keyboardHandler) {
        document.removeEventListener('keydown', keyboardHandler);
        keyboardHandler = null;
    }

    dotNetRef = null;
    cookieKey = null;
}

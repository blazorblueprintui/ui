/**
 * Sidebar State Persistence Module
 * Handles saving and restoring sidebar state using localStorage with cookie fallback
 */

const STORAGE_KEY = 'shadcn-blazor:sidebar:state';
const COOKIE_NAME = 'shadcn-blazor-sidebar-state';
const COOKIE_MAX_AGE = 60 * 60 * 24 * 365; // 1 year in seconds

/**
 * Gets the sidebar state from storage
 * @param {string} key - Optional custom storage key
 * @returns {object|null} The sidebar state object or null if not found
 */
export function getSidebarState(key = STORAGE_KEY) {
    try {
        // Try localStorage first
        if (typeof localStorage !== 'undefined') {
            const stored = localStorage.getItem(key);
            if (stored) {
                return JSON.parse(stored);
            }
        }

        // Fallback to cookie
        const cookieValue = getCookie(COOKIE_NAME);
        if (cookieValue) {
            return JSON.parse(decodeURIComponent(cookieValue));
        }

        return null;
    } catch (error) {
        console.error('SidebarPersistence: Error reading state', error);
        return null;
    }
}

/**
 * Sets the sidebar state in storage
 * @param {object} state - The sidebar state object to save
 * @param {string} key - Optional custom storage key
 */
export function setSidebarState(state, key = STORAGE_KEY) {
    try {
        const stateJson = JSON.stringify(state);

        // Try localStorage first
        if (typeof localStorage !== 'undefined') {
            localStorage.setItem(key, stateJson);
        }

        // Also save to cookie as fallback
        setCookie(COOKIE_NAME, encodeURIComponent(stateJson), COOKIE_MAX_AGE);
    } catch (error) {
        console.error('SidebarPersistence: Error saving state', error);

        // If localStorage fails, try cookie only
        try {
            const stateJson = JSON.stringify(state);
            setCookie(COOKIE_NAME, encodeURIComponent(stateJson), COOKIE_MAX_AGE);
        } catch (cookieError) {
            console.error('SidebarPersistence: Error saving to cookie', cookieError);
        }
    }
}

/**
 * Clears the saved sidebar state
 * @param {string} key - Optional custom storage key
 */
export function clearSidebarState(key = STORAGE_KEY) {
    try {
        // Clear localStorage
        if (typeof localStorage !== 'undefined') {
            localStorage.removeItem(key);
        }

        // Clear cookie
        deleteCookie(COOKIE_NAME);
    } catch (error) {
        console.error('SidebarPersistence: Error clearing state', error);
    }
}

/**
 * Gets a cookie value by name
 * @param {string} name - Cookie name
 * @returns {string|null} Cookie value or null if not found
 */
function getCookie(name) {
    if (typeof document === 'undefined') {
        return null;
    }

    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);

    if (parts.length === 2) {
        return parts.pop().split(';').shift();
    }

    return null;
}

/**
 * Sets a cookie
 * @param {string} name - Cookie name
 * @param {string} value - Cookie value
 * @param {number} maxAge - Max age in seconds
 */
function setCookie(name, value, maxAge) {
    if (typeof document === 'undefined') {
        return;
    }

    let cookie = `${name}=${value}; path=/; max-age=${maxAge}; SameSite=Lax`;

    // Add Secure flag if on HTTPS
    if (window.location.protocol === 'https:') {
        cookie += '; Secure';
    }

    document.cookie = cookie;
}

/**
 * Deletes a cookie
 * @param {string} name - Cookie name
 */
function deleteCookie(name) {
    if (typeof document === 'undefined') {
        return;
    }

    document.cookie = `${name}=; path=/; max-age=0`;
}

/**
 * Checks if localStorage is available
 * @returns {boolean} True if localStorage is available
 */
export function isLocalStorageAvailable() {
    try {
        if (typeof localStorage === 'undefined') {
            return false;
        }

        const testKey = '__shadcn_blazor_test__';
        localStorage.setItem(testKey, 'test');
        localStorage.removeItem(testKey);
        return true;
    } catch (error) {
        return false;
    }
}

/**
 * Gets the storage type being used
 * @returns {string} 'localStorage', 'cookie', or 'none'
 */
export function getStorageType() {
    if (isLocalStorageAvailable()) {
        return 'localStorage';
    } else if (typeof document !== 'undefined') {
        return 'cookie';
    } else {
        return 'none';
    }
}

/**
 * Theme JavaScript module
 * CSP-compliant DOM manipulation for dark mode, base color, primary color, and radius.
 * No eval() — all operations use direct DOM APIs.
 */

const STORAGE_KEY = 'bb-theme';

/**
 * The theme state the document is meant to have.
 *
 * Everything below is applied to <html>, which the server never renders with these
 * attributes — the theme is only known client-side (localStorage). Blazor's enhanced
 * page refresh merges the freshly server-rendered document into the live DOM and
 * syncs <html>'s attributes, so it strips class/data-base-color/--radius wholesale.
 * `dotnet watch` triggers exactly that on every hot reload, which reset the theme to
 * light mid-session and left the C# ThemeService state disagreeing with the DOM (#400).
 *
 * Tracking the intended state lets us put it back when something external clears it.
 * @type {{ isDark: boolean, baseColor: string|null, primaryColor: string|null, radius: number|null } | null}
 */
let desired = null;

/** @type {MutationObserver | null} */
let guard = null;

/** Whether we are mid-write, so the guard ignores our own mutations. */
let writing = false;

/**
 * Write the desired state to the document.
 */
function writeToDom() {
  if (!desired) {
    return;
  }

  writing = true;
  try {
    const root = document.documentElement;

    if (desired.isDark) {
      root.classList.add('dark');
    } else {
      root.classList.remove('dark');
    }

    if (desired.baseColor !== null) {
      root.setAttribute('data-base-color', desired.baseColor);
    }

    if (desired.primaryColor !== null) {
      if (desired.primaryColor === 'default') {
        root.removeAttribute('data-primary-color');
      } else {
        root.setAttribute('data-primary-color', desired.primaryColor);
      }
    }

    if (desired.radius !== null) {
      root.style.setProperty('--radius', desired.radius + 'rem');
    }
  } finally {
    writing = false;
  }
}

/**
 * Whether the document has drifted from the desired state.
 * @returns {boolean}
 */
function hasDrifted() {
  if (!desired) {
    return false;
  }

  const root = document.documentElement;

  if (root.classList.contains('dark') !== desired.isDark) {
    return true;
  }

  if (desired.baseColor !== null && root.getAttribute('data-base-color') !== desired.baseColor) {
    return true;
  }

  if (desired.primaryColor !== null) {
    const current = root.getAttribute('data-primary-color');
    const expected = desired.primaryColor === 'default' ? null : desired.primaryColor;
    if (current !== expected) {
      return true;
    }
  }

  if (desired.radius !== null && root.style.getPropertyValue('--radius') !== desired.radius + 'rem') {
    return true;
  }

  return false;
}

/**
 * Watch <html> and restore the theme if something external resets it. Idempotent —
 * the observer is created once per document. Re-applying only happens on genuine
 * drift, so our own writes cannot cause a feedback loop.
 */
function startGuard() {
  if (guard || typeof MutationObserver === 'undefined') {
    return;
  }

  guard = new MutationObserver(() => {
    if (writing || !hasDrifted()) {
      return;
    }
    writeToDom();
  });

  guard.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ['class', 'data-base-color', 'data-primary-color', 'style']
  });
}

/**
 * Apply the full theme to the document.
 * @param {boolean} isDark
 * @param {string} baseColor
 * @param {string} primaryColor
 * @param {number} radius
 */
export function applyTheme(isDark, baseColor, primaryColor, radius) {
  desired = { isDark, baseColor, primaryColor, radius };
  writeToDom();
  startGuard();
}

/**
 * Apply or remove dark mode.
 * @param {boolean} isDark
 */
export function applyDarkMode(isDark) {
  desired = desired ?? { isDark, baseColor: null, primaryColor: null, radius: null };
  desired.isDark = isDark;
  writeToDom();
  startGuard();
}

/**
 * Set the base color data attribute on the document element.
 * @param {string} color - Lowercase base color name (e.g., "zinc", "slate").
 */
export function applyBaseColor(color) {
  desired = desired ?? { isDark: document.documentElement.classList.contains('dark'), baseColor: color, primaryColor: null, radius: null };
  desired.baseColor = color;
  writeToDom();
  startGuard();
}

/**
 * Set the primary color data attribute on the document element.
 * @param {string} color - Lowercase primary color name (e.g., "blue", "default").
 */
export function applyPrimaryColor(color) {
  desired = desired ?? { isDark: document.documentElement.classList.contains('dark'), baseColor: null, primaryColor: color, radius: null };
  desired.primaryColor = color;
  writeToDom();
  startGuard();
}

/**
 * Set the --radius CSS custom property on the document element.
 * @param {number} radius - Border radius in rem.
 */
export function applyRadius(radius) {
  desired = desired ?? { isDark: document.documentElement.classList.contains('dark'), baseColor: null, primaryColor: null, radius };
  desired.radius = radius;
  writeToDom();
  startGuard();
}

/**
 * Detect whether the user's OS prefers dark mode.
 * @returns {boolean}
 */
export function getPrefersDark() {
  return window.matchMedia('(prefers-color-scheme: dark)').matches;
}

/**
 * Load saved theme preferences from localStorage.
 * @returns {{ isDarkMode: boolean, baseColor: string, primaryColor: string, radius: number } | null}
 */
export function loadTheme() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return null;
    }
    return JSON.parse(raw);
  } catch {
    return null;
  }
}

/**
 * Resolves and applies the startup theme in one call.
 *
 * This used to be two to four separate calls from C# — loadTheme, then maybe clearTheme, then
 * maybe getPrefersDark, then applyTheme — and on Blazor Server each one is a circuit round trip,
 * paid on every page load before the page settles. None of the decisions between them need the
 * server: they are reads of localStorage and a media query.
 *
 * Colour names are validated here against the lists C# supplies, so a corrupted or outdated entry
 * in localStorage falls back to the configured default exactly as C#'s enum parse would, rather
 * than being applied and corrected a round trip later.
 *
 * @param {Object} config
 * @param {boolean} config.persist - Whether saved preferences should be honoured at all.
 * @param {boolean} config.detectSystemPreference - Whether to fall back to the OS dark-mode setting.
 * @param {Object} config.defaults - isDarkMode, baseColor, primaryColor, radius.
 * @param {string[]} config.validBaseColors - Accepted base colour names, lower case.
 * @param {string[]} config.validPrimaryColors - Accepted primary colour names, lower case.
 * @returns {{ isDarkMode: boolean, baseColor: string, primaryColor: string, radius: number }}
 *   The state that was applied.
 */
export function initialize(config) {
  const defaults = config.defaults;

  // Only read localStorage when persistence is enabled. Reading it regardless meant a theme saved
  // by an earlier run kept overriding the configured defaults after persistence was turned off,
  // with no way out short of the user clearing site data by hand (#481). Clearing the stale entry
  // closes the same hole for anyone who already has one.
  const saved = config.persist ? loadTheme() : null;
  if (!config.persist) {
    clearTheme();
  }

  const pick = (value, allowed, fallback) => {
    const name = typeof value === 'string' ? value.toLowerCase() : null;
    return name && allowed.includes(name) ? name : fallback;
  };

  const state = saved
    ? {
        isDarkMode: !!saved.isDarkMode,
        baseColor: pick(saved.baseColor, config.validBaseColors, defaults.baseColor),
        primaryColor: pick(saved.primaryColor, config.validPrimaryColors, defaults.primaryColor),
        radius: typeof saved.radius === 'number' ? saved.radius : defaults.radius,
      }
    : {
        isDarkMode: config.detectSystemPreference ? getPrefersDark() : defaults.isDarkMode,
        baseColor: defaults.baseColor,
        primaryColor: defaults.primaryColor,
        radius: defaults.radius,
      };

  applyTheme(state.isDarkMode, state.baseColor, state.primaryColor, state.radius);

  return state;
}

/**
 * Save theme preferences to localStorage.
 * @param {boolean} isDarkMode
 * @param {string} baseColor
 * @param {string} primaryColor
 * @param {number} radius
 */
export function saveTheme(isDarkMode, baseColor, primaryColor, radius) {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify({ isDarkMode, baseColor, primaryColor, radius }));
  } catch {
    // localStorage unavailable — silently ignore
  }
}

/**
 * Remove any saved theme preferences from localStorage.
 * Used when persistence is disabled, so a stale entry written by an earlier
 * run cannot keep overriding the configured defaults.
 */
export function clearTheme() {
  try {
    localStorage.removeItem(STORAGE_KEY);
  } catch {
    // localStorage unavailable — silently ignore
  }
}

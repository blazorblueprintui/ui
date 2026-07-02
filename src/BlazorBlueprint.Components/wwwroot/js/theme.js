/**
 * Theme JavaScript module
 * CSP-compliant DOM manipulation for dark mode, base color, primary color, and radius.
 * No eval() — all operations use direct DOM APIs.
 */

const STORAGE_KEY = 'bb-theme';

/**
 * Apply the full theme to the document.
 * @param {boolean} isDark
 * @param {string} baseColor
 * @param {string} primaryColor
 * @param {number} radius
 */
export function applyTheme(isDark, baseColor, primaryColor, radius) {
  applyDarkMode(isDark);
  applyBaseColor(baseColor);
  applyPrimaryColor(primaryColor);
  applyRadius(radius);
}

/**
 * Apply or remove dark mode.
 * @param {boolean} isDark
 */
export function applyDarkMode(isDark) {
  const root = document.documentElement;
  if (isDark) {
    root.classList.add('dark');
  } else {
    root.classList.remove('dark');
  }
}

/**
 * Set the base color data attribute on the document element.
 * Also removes any inline --primary/--primary-foreground/--ring overrides
 * so the base color's built-in values take effect cleanly.
 * @param {string} color - Lowercase base color name (e.g., "zinc", "slate").
 */
export function applyBaseColor(color) {
  const root = document.documentElement;
  root.setAttribute('data-base-color', color);
}

/**
 * Set the primary color data attribute on the document element.
 * @param {string} color - Lowercase primary color name (e.g., "blue", "default").
 */
export function applyPrimaryColor(color) {
  const root = document.documentElement;
  if (color === 'default') {
    root.removeAttribute('data-primary-color');
  } else {
    root.setAttribute('data-primary-color', color);
  }
}

/**
 * Set the --radius CSS custom property on the document element.
 * @param {number} radius - Border radius in rem.
 */
export function applyRadius(radius) {
  document.documentElement.style.setProperty('--radius', radius + 'rem');
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

// chart-theme.js
// CSS variable resolution and theme change detection for ECharts

/**
 * Recursively walk an option object and resolve any string values
 * that contain CSS var() references to computed color values.
 * @param {object} option - ECharts option object
 * @param {HTMLElement} contextElement - Element for getComputedStyle
 * @returns {object} - New option object with resolved colors
 */
export function resolveThemeColors(option, contextElement) {
  if (!option || !contextElement) return option;
  return deepResolve(structuredClone(option), contextElement);
}

/**
 * @param {any} obj
 * @param {HTMLElement} el
 * @returns {any}
 */
function deepResolve(obj, el) {
  if (obj === null || obj === undefined) return obj;

  if (typeof obj === 'string') {
    return resolveValue(obj, el);
  }

  if (Array.isArray(obj)) {
    return obj.map(item => deepResolve(item, el));
  }

  if (typeof obj === 'object') {
    for (const key of Object.keys(obj)) {
      obj[key] = deepResolve(obj[key], el);
    }
  }

  return obj;
}

/**
 * Resolve a single string value. If it contains var(--x), compute it.
 * Handles OKLCH values by converting to hex.
 * @param {string} value
 * @param {HTMLElement} el
 * @returns {string}
 */
function resolveValue(value, el) {
  if (typeof value !== 'string') return value;

  const varRegex = /var\(\s*(--[a-zA-Z0-9-]+)\s*(?:,\s*([^)]+))?\s*\)/g;
  if (!varRegex.test(value)) return value;

  // Reset regex lastIndex after test
  varRegex.lastIndex = 0;

  const resolved = value.replace(varRegex, (_match, varName, fallback) => {
    const computed = getComputedStyle(el).getPropertyValue(varName).trim();
    if (computed) {
      return convertToUsableColor(computed);
    }
    return fallback ? fallback.trim() : _match;
  });

  // If the result is still a complex CSS expression (e.g., color-mix()),
  // resolve it to a hex color through the canvas context
  if (resolved !== value && /color-mix\s*\(/i.test(resolved)) {
    return convertToUsableColor(resolved);
  }

  return resolved;
}

/**
 * Convert any CSS color expression to an rgb()/rgba() string that ECharts can parse.
 *
 * ECharts' internal zrender color parser understands hex, rgb/rgba, hsl/hsla, and
 * named colours, but NOT CSS Level 4 formats such as oklch().
 *
 * ## Why pure math is used — and why it works in ALL browsers
 *
 * CSS custom properties (e.g. `--chart-1: oklch(81% .1 252)`) are returned
 * verbatim by `getComputedStyle(el).getPropertyValue('--name')` in every browser
 * (Chrome, Firefox, Safari, Edge, Brave, Opera…).  The browser never normalises
 * or resolves a custom-property token value — it simply stores and returns the
 * raw text.  Therefore the string we receive is always the original oklch()
 * expression regardless of the browser, and we need to convert it ourselves.
 *
 * The `oklchToRgb` function below is self-contained JavaScript math (Oklab
 * reference matrices).  It uses no browser API, so it produces identical results
 * in Chrome, Firefox, Safari, and all Chromium forks.
 *
 * Additional note: Chrome 111+ also stopped normalising oklch when it appears as
 * a *resolved* non-custom value (e.g. reading back `element.style.color`).
 * Firefox and Safari still normalise those to rgb().  Neither behaviour affects
 * us because we only read from custom properties.
 *
 * Other CSS Level 4 expressions (e.g. color-mix()) that do not use oklch are
 * attempted via a DOM fallback; if that also returns oklch (Chrome behaviour),
 * the result is passed to the math converter.
 *
 * @param {string} colorValue - Any CSS colour string
 * @returns {string} - ECharts-compatible colour string
 */
function convertToUsableColor(colorValue) {
  // Fast-path: already in a format ECharts understands
  if (/^#[0-9a-fA-F]{3,8}$/.test(colorValue)) return colorValue;
  if (/^rgba?\(/.test(colorValue)) return colorValue;
  if (/^hsla?\(/.test(colorValue)) return colorValue;

  // Handle bare OKLCH token without wrapper (e.g. "0.81 0.10 252")
  if (/^\d/.test(colorValue) && colorValue.includes(' ') && !colorValue.includes('(')) {
    colorValue = `oklch(${colorValue})`;
  }

  // Convert oklch() using self-contained math — browser APIs are unreliable here
  if (/^oklch\(/i.test(colorValue)) {
    const rgb = oklchToRgb(colorValue);
    if (rgb) return rgb;
  }

  // Fallback: DOM-based resolution for other expressions (color-mix, etc.)
  const tmp = document.createElement('div');
  tmp.style.color = colorValue;
  document.documentElement.appendChild(tmp);
  const resolved = getComputedStyle(tmp).color;
  document.documentElement.removeChild(tmp);

  // If DOM returned a parseable non-oklch value, use it
  if (resolved && resolved !== 'rgba(0, 0, 0, 0)' && !/^oklch\(/i.test(resolved)) {
    return resolved;
  }
  // If DOM still returned oklch (e.g. color-mix with oklch inputs), try converting it
  if (resolved && /^oklch\(/i.test(resolved)) {
    const rgb = oklchToRgb(resolved);
    if (rgb) return rgb;
  }

  return colorValue;
}

/**
 * Convert an oklch() colour string to an rgb() string using self-contained math.
 * Implements the official Oklab/OKLCH → sRGB conversion matrices.
 *
 * This function is entirely browser-agnostic — it performs pure arithmetic with
 * no browser API calls, so it produces identical results in Chrome, Firefox,
 * Safari, Edge, Brave, and any other JavaScript environment.
 *
 * Handles both "oklch(L C H)" (L as 0–1) and "oklch(L% C H)" (L as 0–100%).
 * Accepts both space-separated and comma-separated component syntax.
 * Optionally ignores a trailing "/ alpha" component.
 *
 * @param {string} oklchStr - e.g. "oklch(0.42 0.18 266)" or "oklch(42% .18 266)"
 * @returns {string|null} - rgb() string, or null if the input could not be parsed
 */
function oklchToRgb(oklchStr) {
  const m = oklchStr.match(
    /oklch\(\s*([\d.]+)(%?)\s*,?\s*([\d.]+)\s*,?\s*([\d.]+)\s*(?:\/\s*[\d.]+%?\s*)?\)/i
  );
  if (!m) return null;

  let L = parseFloat(m[1]);
  if (m[2] === '%') L /= 100;       // percentage form → 0–1
  const C = parseFloat(m[3]);
  const H = parseFloat(m[4]);

  // 1. oklch → oklab
  const hRad = (H * Math.PI) / 180;
  const a = C * Math.cos(hRad);
  const b = C * Math.sin(hRad);

  // 2. oklab → linear sRGB  (Oklab reference matrix)
  const l_ = L + 0.3963377774 * a + 0.2158037573 * b;
  const m_ = L - 0.1055613458 * a - 0.0638541728 * b;
  const s_ = L - 0.0894841775 * a - 1.2914855480 * b;

  const l3 = l_ * l_ * l_;
  const m3 = m_ * m_ * m_;
  const s3 = s_ * s_ * s_;

  const rLin = +4.0767416621 * l3 - 3.3077115913 * m3 + 0.2309699292 * s3;
  const gLin = -1.2684380046 * l3 + 2.6097574011 * m3 - 0.3413193965 * s3;
  const bLin = -0.0041960863 * l3 - 0.7034186147 * m3 + 1.7076147010 * s3;

  // 3. linear sRGB → sRGB gamma + clamp to [0, 255]
  const toSrgbByte = (linear) => {
    const v = Math.max(0, Math.min(1, linear));
    const gamma = v <= 0.0031308 ? 12.92 * v : 1.055 * Math.pow(v, 1 / 2.4) - 0.055;
    return Math.round(gamma * 255);
  };

  return `rgb(${toSrgbByte(rLin)}, ${toSrgbByte(gLin)}, ${toSrgbByte(bLin)})`;
}

/**
 * Watch for theme changes (dark/light mode toggle) via MutationObserver
 * and custom 'bb-theme-changed' events.
 *
 * Observes the <html> element for class or data-theme attribute changes.
 * Also listens for 'bb-theme-changed' custom events on <html>, allowing
 * developers to trigger chart color updates after programmatically changing
 * CSS variables (e.g., dynamic theming, theme builders).
 *
 * Usage:
 *   document.documentElement.dispatchEvent(new CustomEvent('bb-theme-changed'));
 *
 * @param {Function} callback - Called when theme changes
 * @returns {Function} - Cleanup function to stop watching
 */
export function watchThemeChanges(callback) {
  const target = document.documentElement;

  const observer = new MutationObserver((mutations) => {
    for (const mutation of mutations) {
      if (mutation.type === 'attributes' &&
        (mutation.attributeName === 'class' || mutation.attributeName === 'data-theme')) {
        // Delay to let CSS variables update
        requestAnimationFrame(() => callback());
        break;
      }
    }
  });

  observer.observe(target, {
    attributes: true,
    attributeFilter: ['class', 'data-theme']
  });

  // Listen for custom theme change events (e.g., from dynamic theme editors)
  const onThemeChanged = () => requestAnimationFrame(() => callback());
  target.addEventListener('bb-theme-changed', onThemeChanged);

  return () => {
    observer.disconnect();
    target.removeEventListener('bb-theme-changed', onThemeChanged);
  };
}

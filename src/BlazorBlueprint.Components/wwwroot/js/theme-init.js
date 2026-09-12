/**
 * Applies the saved theme to <html> before the first paint.
 *
 * Without this the page renders with the default theme, and the saved one is applied once
 * Blazor has started — so a user who chose dark mode sees a flash of light first, on every
 * load. That is #477, and it cannot be fixed from C#: on a prerendered or statically
 * rendered page the server has no way to know the preference, which lives in localStorage.
 *
 * This must be loaded as a CLASSIC, BLOCKING script in <head>:
 *
 *   <script src="_content/BlazorBlueprint.Components/js/theme-init.js"></script>
 *
 * Not type="module" — modules are deferred until after the document is parsed, which is
 * after the first paint, which is the whole problem. Not inline either, so that a strict
 * Content-Security-Policy needs no 'unsafe-inline'.
 *
 * Configure it with data attributes on that same script tag:
 *
 *   data-default-dark="true"   use dark when nothing is saved, instead of the OS preference
 *   data-default-dark="false"  use light when nothing is saved
 *   data-storage="false"       ignore localStorage entirely; pair with
 *                              PersistToLocalStorage = false so a stale saved theme from an
 *                              earlier session cannot come back (#481)
 *
 * Deliberately duplicates the small part of theme.js that touches <html>. Importing shares
 * the code but reintroduces the defer, and a copy of four DOM writes is the cheaper trade.
 * If the attribute names here and in theme.js ever diverge, the theme will flash again.
 */
(function () {
    'use strict';

    var STORAGE_KEY = 'bb-theme';

    // document.currentScript is the tag being executed, which is how the data-* config is read
    // without an inline script.
    var script = document.currentScript;
    var useStorage = !script || script.getAttribute('data-storage') !== 'false';
    var defaultDark = script ? script.getAttribute('data-default-dark') : null;

    var saved = null;
    if (useStorage) {
        try {
            var raw = localStorage.getItem(STORAGE_KEY);
            if (raw) {
                saved = JSON.parse(raw);
            }
        } catch (e) {
            // localStorage blocked, or the stored value is not JSON. Fall through to the default;
            // a wrong theme for one frame beats an exception that stops the page parsing.
            saved = null;
        }
    }

    var root = document.documentElement;

    var isDark;
    if (saved && typeof saved.isDarkMode === 'boolean') {
        isDark = saved.isDarkMode;
    } else if (defaultDark === 'true') {
        isDark = true;
    } else if (defaultDark === 'false') {
        isDark = false;
    } else {
        try {
            isDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        } catch (e) {
            isDark = false;
        }
    }

    if (isDark) {
        root.classList.add('dark');
    } else {
        root.classList.remove('dark');
    }

    if (saved) {
        if (saved.baseColor) {
            root.setAttribute('data-base-color', saved.baseColor);
        }

        if (saved.primaryColor) {
            if (saved.primaryColor === 'default') {
                root.removeAttribute('data-primary-color');
            } else {
                root.setAttribute('data-primary-color', saved.primaryColor);
            }
        }

        if (typeof saved.radius === 'number') {
            root.style.setProperty('--radius', saved.radius + 'rem');
        }
    }
})();

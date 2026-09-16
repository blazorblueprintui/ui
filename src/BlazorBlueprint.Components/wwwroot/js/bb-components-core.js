/**
 * The JavaScript modules that load on nearly every page, in one entry point.
 *
 * Why only these
 * --------------
 * Each `import(...)` issued from C# is a circuit round trip on Blazor Server. `JsModules` already
 * reduced that to one per *module* rather than one per component instance; this file reduces the
 * common case to one per *page*.
 *
 * Measured, cold cache, across eight demo pages: `theme.js`, `sidebar.js` and `sidebar-inset.js`
 * load on every single one, and `text-input.js` with `composition-guard.js` on every page carrying
 * a form control. That is three to five round trips before anyone has clicked anything.
 *
 * The rest of the Components layer stays lazy on purpose. Bundling all thirty would be 56 KB
 * gzipped, and an app that shows one `BbInput` would download the dashboard grid, the dock, the
 * markdown editor and the ECharts adapter to get it. These five are 8 KB, and an app that renders
 * any page at all has already paid for them.
 *
 * Namespaces are the file name in camelCase, matching the primitives bundle, so a call site reads
 * `textInput.initialize` without anyone having to open this file.
 */

// Keep these dependency URLs in step with ComponentModules.CoreUrl. The query
// on the entry module is not inherited by relative imports in browser caches.
import * as compositionGuard from './composition-guard.js?assets=2';
import * as sidebar from './sidebar.js?assets=2';
import * as sidebarInset from './sidebar-inset.js?assets=2';
import * as textInput from './text-input.js?assets=2';
import * as theme from './theme.js?assets=2';

// Fails loudly when a module here is older than this bundle. See bb-primitives.js for the
// incident that made this necessary: a stale sidebar.js behind a CDN, and every circuit dead.
function assertFresh(fileName, module, exportName) {
    if (typeof module[exportName] !== 'function') {
        throw new Error(
            `BlazorBlueprint: ${fileName} is out of date — it has no '${exportName}', which this ` +
            `version of bb-components-core.js requires. The browser or a cache in front of it ` +
            `served an older copy. Hard-refresh to check, and make sure _content/BlazorBlueprint.*/js/ ` +
            `is not cached for longer than a deployment.`);
    }
}

assertFresh('composition-guard.js', compositionGuard, 'attach');
assertFresh('sidebar.js', sidebar, 'initialize');
assertFresh('sidebar-inset.js', sidebarInset, 'scrollToTop');
assertFresh('text-input.js', textInput, 'initialize');
assertFresh('theme.js', theme, 'initialize');

export {
    compositionGuard,
    sidebar,
    sidebarInset,
    textInput,
    theme
};

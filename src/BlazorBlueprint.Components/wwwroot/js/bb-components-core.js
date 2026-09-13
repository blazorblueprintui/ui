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

export * as compositionGuard from './composition-guard.js';
export * as sidebar from './sidebar.js';
export * as sidebarInset from './sidebar-inset.js';
export * as textInput from './text-input.js';
export * as theme from './theme.js';

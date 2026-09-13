/**
 * Single entry point for every BlazorBlueprint primitive module.
 *
 * Why this file exists
 * --------------------
 * In Blazor Server an `import(...)` issued from C# is an instruction posted to the browser that
 * the server then awaits, so every lazily-imported module costs a full circuit round trip — paid
 * once per module, per page load, whether or not the file is already in the HTTP cache. Opening
 * one overlay used to chain three of them (positioning, Floating UI, click-outside) and the cost
 * scaled with the user's latency, not with the size of the files.
 *
 * Importing this barrel instead resolves the whole primitive surface in one round trip: the
 * browser walks the static import graph itself, in parallel, without asking the server again.
 *
 * Each module is re-exported under its own namespace because several of them export the same
 * names (`initialize`, `dispose`, `focusElement`). Call them from C# with a dotted identifier:
 *
 *     await module.InvokeVoidAsync("clickOutside.onClickOutsideByIds", ...);
 *
 * The individual files remain importable on their own and are unchanged; a module is evaluated
 * once per document regardless of which route reaches it first, so mixing the two is safe.
 *
 * The namespace is always the file name in camelCase. Keep it that way — the rule is what makes
 * the call sites predictable.
 */

export * as clickOutside from './click-outside.js';
export * as elementUtils from './element-utils.js';
export * as escapeKeydown from './escape-keydown.js';
export * as focusTrap from './focus-trap.js';
export * as keyboardNav from './keyboard-nav.js';
export * as keyboardShortcuts from './keyboard-shortcuts.js';
export * as matchTriggerWidth from './match-trigger-width.js';
export * as menuKeyboard from './menu-keyboard.js';
export * as nativeDialog from './native-dialog.js';
export * as overlay from './overlay.js';
export * as portal from './portal.js';
export * as positioning from './positioning.js';
export * as scrollArea from './scroll-area.js';
export * as select from './select.js';
export * as slider from './slider.js';
export * as sortable from './sortable.js';
export * as tableRowNav from './table-row-nav.js';
export * as treeKeyboard from './tree-keyboard.js';

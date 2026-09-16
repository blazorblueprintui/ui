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

import * as clickOutside from './click-outside.js';
import * as elementUtils from './element-utils.js';
import * as escapeKeydown from './escape-keydown.js';
import * as focusTrap from './focus-trap.js';
import * as keyboardNav from './keyboard-nav.js';
import * as keyboardShortcuts from './keyboard-shortcuts.js';
import * as matchTriggerWidth from './match-trigger-width.js';
import * as menuKeyboard from './menu-keyboard.js';
import * as nativeDialog from './native-dialog.js';
import * as overlay from './overlay.js';
import * as portal from './portal.js';
import * as positioning from './positioning.js';
import * as scrollArea from './scroll-area.js';
import * as select from './select.js';
import * as slider from './slider.js';
import * as sortable from './sortable.js';
import * as tableRowNav from './table-row-nav.js';
// Revise the dependency URL when its keyboard contract changes: a versioned
// entry module does not invalidate relative imports already held by a browser.
import * as treeKeyboard from './tree-keyboard.js?rev=2';

/**
 * Fails loudly when a module in this bundle is older than the bundle itself.
 *
 * The C# import of this file carries the library version as a query, so a new release is a new
 * URL and the entry file is never served stale. The files it imports are fetched relative to it,
 * without the query, and a browser or CDN that still holds an older copy of one of them will hand
 * it over without complaint. A consumer behind a CDN with a long browser-cache TTL hit exactly
 * that: a new bundle importing a four-hour-old sidebar.js, `sidebar.initialize` not a function,
 * and every circuit dead at the first call — with nothing to say why.
 *
 * Each module is checked for an export that the current version of the bundle depends on. A
 * stale file fails here, at load, with the file named, instead of later, somewhere else, with a
 * message about a function.
 */
function assertFresh(fileName, module, exportName) {
    if (typeof module[exportName] !== 'function') {
        throw new Error(
            `BlazorBlueprint: ${fileName} is out of date — it has no '${exportName}', which this ` +
            `version of bb-primitives.js requires. The browser or a cache in front of it served an ` +
            `older copy. Hard-refresh to check, and make sure _content/BlazorBlueprint.*/js/ is not ` +
            `cached for longer than a deployment.`);
    }
}

assertFresh('click-outside.js', clickOutside, 'onClickOutsideByIds');
assertFresh('element-utils.js', elementUtils, 'observeNearBottom');
assertFresh('escape-keydown.js', escapeKeydown, 'initialize');
assertFresh('focus-trap.js', focusTrap, 'createFocusTrap');
assertFresh('keyboard-nav.js', keyboardNav, 'setupKeyboardNav');
assertFresh('keyboard-shortcuts.js', keyboardShortcuts, 'registerShortcut');
assertFresh('match-trigger-width.js', matchTriggerWidth, 'matchTriggerWidth');
assertFresh('menu-keyboard.js', menuKeyboard, 'initialize');
assertFresh('native-dialog.js', nativeDialog, 'setupDialog');
assertFresh('overlay.js', overlay, 'open');
assertFresh('portal.js', portal, 'setupPortal');
assertFresh('positioning.js', positioning, 'hidePosition');
assertFresh('scroll-area.js', scrollArea, 'initialize');
assertFresh('select.js', select, 'scrollMarkedIntoView');
assertFresh('slider.js', slider, 'initialize');
assertFresh('sortable.js', sortable, 'init');
assertFresh('table-row-nav.js', tableRowNav, 'delegateRowBehaviour');
assertFresh('tree-keyboard.js', treeKeyboard, 'initialize');

export {
    clickOutside,
    elementUtils,
    escapeKeydown,
    focusTrap,
    keyboardNav,
    keyboardShortcuts,
    matchTriggerWidth,
    menuKeyboard,
    nativeDialog,
    overlay,
    portal,
    positioning,
    scrollArea,
    select,
    slider,
    sortable,
    tableRowNav,
    treeKeyboard
};

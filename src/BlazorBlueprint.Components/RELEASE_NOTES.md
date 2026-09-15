## What's New in v4.0.0-beta.2

**This is a prerelease.** The API may still change before the stable v4.0.0 release.

### Breaking Changes
- **BbTooltipTrigger** — `AsChild` now defaults to `false`. A bare icon, plain text or arbitrary markup opens the tooltip with no opt-in. Add `AsChild="true"` where the child consumes the trigger context itself, such as a `BbButton`. The wrapper `<span>` uses `display: contents`, so layout is unaffected, but DOM-walking selectors and test hooks may need updating.
- **BbDrawerTrigger**, **BbDrawerClose** — now render a real `<button type="button">` instead of a bare `<div @onclick>`, so they are reachable by keyboard and announced as controls. Both gain `AsChild`; set it to `true` when the child is already a control, otherwise you get a button nested inside a button.
- **JavaScript modules** — every component now imports its JS module once per circuit through `JsModules.GetAsync` / `PrimitiveModules.GetAsync` and no longer disposes it. Primitive modules are addressed through the `bb-primitives.js` bundle under a namespace (`elementUtils.isNearBottom`, `portal.lockBodyScroll`, `escapeKeydown.initialize`). Custom code that imported the individual primitive files or called the unnamespaced exports must be updated.
- **Core bundle** — `theme.js`, `sidebar.js`, `sidebar-inset.js`, `text-input.js` and `composition-guard.js` ship as `bb-components-core.js` and are addressed under a namespace (`theme.initialize`, `sidebarInset.scrollToTop`). Custom code that imported the individual files must be updated.
- **BbPopoverContent**, **BbSelectContent**, **BbDropdownMenuContent** — dismissal and listbox keyboard handling are wired by `BbFloatingPortal` inside the call that opens the overlay. JavaScript now owns `data-side`, `data-focused` and `aria-activedescendant`; code that rendered these from C# must stop, or the two writers will conflict.
- Updated the `BlazorBlueprint.Primitives` dependency to 4.0.0-beta.2, which carries its own breaking changes. See the Primitives release notes and `V4-MIGRATION-GUIDE.md`.

### New Features
- **BbDialog** — new `RenderingStrategy` parameter. Set it to `OverlayRenderingStrategy.Native` to render through the browser's built-in `<dialog>` element via `showModal()`, which works across Blazor render-mode boundaries and needs no portal host. When null, the global default configured through `AddBlazorBlueprintPrimitives` applies.
- **BbDialogContent** — in native mode the JavaScript overlay is omitted and the `::backdrop` provides the scrim. `CloseOnOverlayClick` now also controls whether a backdrop click closes a native dialog. New stylesheet rules style `dialog[data-state]` and its backdrop to match the JavaScript path.
- **BbPopoverContent** — new `ScrollToSelected` and `ScrollToSelectedSelector` parameters. A popover-based list opens already scrolled to its chosen item, and the scroll runs inside the interop call that positions the popover, before it is revealed.
- **BbCopyText** — new `ValueFuncAsync` for text that has to be fetched or computed. The copy happens inside the real click or keydown gesture in JavaScript, and the browser is handed a promise, so the clipboard write survives however long the callback takes. `Value` still wins when non-empty, then `ValueFunc`, then `ValueFuncAsync`.
- **BbCopyText** — new `OnCopyFailed` callback with a `CopyTextFailure` of `Refused` or `NoValue`. A failed copy was previously invisible.

### Bug Fixes
- **BbSidebarInset** — client-side navigation no longer kills the circuit. The scroll-to-top call used the pre-bundle function name, so every link click threw `Could not find 'scrollToTop'` from an `async void` handler and dropped the connection. The call is now namespaced, and a `JSException` from it is swallowed, since scrolling a new page to the top is not worth a dead circuit. Present in 4.0.0-beta.1.
- **BbCombobox** — closing no longer discards a paged list. `SearchQueryChanged` fired with an empty string on every close, even when nothing had been typed, which an infinite-scroll consumer reads as "reload your first page". It now fires only when there is a search to clear, which also removes the flicker the reload caused mid-close-animation.
- **BbCombobox** — reopens scrolled to the chosen item, as `BbSelect` has always done. The chosen item is marked with `data-bb-current` in both `Options` and compositional modes, since `aria-selected` on a command item means "keyboard-focused" rather than "chosen".
- **BbCopyText** — the clipboard write is now made inside the user gesture, so Safari no longer refuses it. Enter and Space are handled in JavaScript, since a `span` with `role="button"` gets no native click from them. The Blazor handlers take over if the module fails to load or during prerendering.
- **BbCopyText** — the `execCommand` fallback now runs only for the insecure-context case it was written for, instead of on every failure, where it could report success while the clipboard stayed empty.
- **BbDrawerTrigger**, **BbDrawerClose** — now show the themed focus ring, since they are focusable for the first time.

### Improvements
- **BbCommandInput** — the focus ring moves from the `<input>` to its row on `focus-within`, inset and rounded at the top to match the popover. It was a third box drawn inside the bordered row inside the bordered popover, on screen the whole time a combobox was open. The indicator is larger, not smaller, so the accessibility fix it came from still holds. `BbCommand` used on its own gets the same treatment.
- **ThemeService** — invalid colour names in `localStorage` now fall back to the configured default in the browser instead of being applied and corrected a round trip later.

### Performance
- **BbSelect**, **BbPopover**, **BbDropdownMenu**, **BbCombobox** and other floating overlays — open and close in one interop call each instead of five, through the Primitives update. Median time from click to visible for a select dropped from 147ms to 81ms on a 20ms round trip, and the first open now costs the same as a reopen. Arrow keys and option hover in a listbox no longer send a message to the server.
- **JavaScript modules** — each module is imported once per circuit and shared across every component instance, instead of once per component. Thirteen inputs on a page previously issued thirteen import calls for the same file, each a round trip on Blazor Server.
- **Core bundle** — the five modules that load on every page, or on every page with a form control, ship as one file. Distinct module imports per page drop from 3–6 to 1–3. The remaining modules stay lazy.
- **ThemeService**, **BbSidebarProvider** — initialise in one interop call each instead of two to four. Reading storage, the OS dark-mode preference and applying the result now happen in the browser and return what was applied.
- **BbDataGrid** — key and click handlers are attached once to the grid container and delegated, instead of once per row. Rows opt in via `data-bb-row-keys` and `data-bb-row-click`. A 465-row grid went from 240 client-to-server messages on load to 111.

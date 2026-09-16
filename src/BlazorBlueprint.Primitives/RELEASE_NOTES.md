## What's New in v4.0.0-beta.5

**This is a prerelease.** The API may still change before the stable v4.0.0 release.

### Breaking Changes

- **BbPopoverContent**, **BbSelectContent**, **BbDropdownMenuContent**: the `JsOnClickOutside` and `JsOnEscapeKey` JSInvokable methods are removed. Dismissal now arrives through the new `BbFloatingPortal.OnDismiss` callback.
- **BbTableRow**, **BbDataGridRow**, **BbPopoverContent**, **BbHoverCardTrigger**, **BbCategoryPortalHost** no longer implement `IAsyncDisposable`. Code that awaited `DisposeAsync()` on these components must be updated.
- **BbFloatingPortal** opens and closes from `OnParametersSet` without awaiting interop. It no longer passes an `ElementReference` to JavaScript; the content is located by a `data-bb-portal` attribute instead.
- **BbFloatingPortal**: JavaScript now owns the resolved `data-side` attribute and a listbox's `data-focused` and `aria-activedescendant` state. Owners that rendered these from C# must stop, or the two writers will conflict.
- **JavaScript modules**: `overlay.open` and `overlay.close` have new signatures. `open(portalId, reference, options, dotNetRef)` no longer takes a floating element, and `close(portalId, options, dotNetRef)` replaces `close(portalId, floating)`.
- **JavaScript modules**: the `onClickOutside` and `onEscapeKey` exports are removed from `click-outside.js`. Use `overlay.open` with `FloatingDismissOptions` instead.
- **JavaScript modules**: all primitive modules are now bundled into `bb-primitives.js` and exposed under a namespace (`focusTrap.createFocusTrap`, `positioning.computePosition`, etc.). Custom code that imported the individual module files must load the bundle via `PrimitiveModules.GetAsync` and use the namespaced identifier.
- **escape-keydown.js**: `initialize` now takes an optional `methodName` argument; the default callback name changed from `HandleEscape` to `JsOnEscapeKey`.
- **PrimitiveModules** imports the bundle through the new versioned `ModuleUrl`, not `ModulePath`. Code that passed `ModulePath` to `JsModules.TryGetLoaded` must pass `ModuleUrl` instead.
- **bb-primitives.js** now throws at load when any module it imports is older than the bundle. A stale cached file that previously failed later with a missing-function error now fails immediately with the file named.
- **BbTableRow**, **BbDataGridRow**, **BbMenubarContent**, **BbSortable**: the few Tailwind utilities these primitives render themselves (row focus ring, menubar backdrop, sortable `sr-only` live region) are now `bb:`-prefixed and served from the `bb-utilities` layer in `blazorblueprint.css`. A consumer's own Tailwind build no longer emits them, so remove any `@source` that points at the library and update CSS or test selectors that matched the old unprefixed class names.

### New Features

- **Native dialog rendering**: **BbDialog** gains a `RenderingStrategy` parameter. Set it to `OverlayRenderingStrategy.Native` to render a browser `<dialog>` element driven by `showModal()`, which works across Blazor render-mode boundaries and does not need a portal host. Falls back to JavaScript rendering with a diagnostic when the browser lacks support.
- **OverlayRenderingOptions**: `AddBlazorBlueprintPrimitives` now accepts a configure callback to set a global `DefaultStrategy` for all overlays.
- **INativeOverlayService**: new scoped service that resolves the effective rendering strategy and drives the native `<dialog>` element (show, close, focus, and lifecycle events).
- **BbDialogContent** gains `CloseOnOverlayClick` to control whether a backdrop click closes a native dialog.
- **BbFloatingPortal** gains `Dismiss` (`FloatingDismissOptions`) and `OnDismiss` (`EventCallback<FloatingDismissReason>`). Owners declare which dismissal gestures to listen for and the portal wires them in the same call that opens the overlay.
- **BbFloatingPortal** gains `Keyboard` (`FloatingKeyboardOptions`). Listbox or menu keyboard handling is wired inside the open call, with `FloatingKeyboardKind` selecting the behaviour.
- **BbFloatingPortal** gains `SideElementId`, so JavaScript writes the resolved `data-side` attribute on a named element without a C# re-render.
- **BbFloatingPortal** gains `ScrollToCurrentIn` and `ScrollToCurrentSelector`, which scroll a chosen item into view before the overlay is revealed.
- **BbPopoverContent** gains `ScrollToSelected` and `ScrollToSelectedSelector`, so a popover-based list opens already scrolled to its current item.
- **FloatingDismissReason** enum reports whether an overlay was dismissed by an outside interaction or the Escape key.
- **JsModules.GetAsync** and **PrimitiveModules.GetAsync**: shared, per-circuit module references that any component can use without owning or disposing them. **JsModules.TryGetLoaded** and **PrimitiveModules.TryGetLoaded** return an already-loaded module synchronously.
- **BbFloatingPortal** gains `AutoFocusId`, which focuses a named element one frame after the reveal, inside the call that opens the overlay.
- **BbFloatingPortal** gains `RestoreFocusToId` and `RestoreFocusOnClose`, so the browser returns focus to the trigger inside the close call for an intentional close only.
- **BbPopoverContent** gains `AutoFocusId`, so a search box inside a popover takes focus without a round trip.
- **JsModules.Versioned** appends an assembly's informational version to a module path as a `v` query, so a new release is a new URL. **PrimitiveModules.ModuleUrl** exposes the versioned bundle URL.
- **elementUtils.observeNearBottom** and **observeHover**: new JavaScript observers that call .NET once when a list scrolls near its bottom or when the pointer moves onto a different item.

### Bug Fixes

- **Overlays**: Escape now closes only the topmost open overlay. A popover inside a dialog no longer closes the dialog on the first press.
- **Overlays**: the exit-animation wait ignores infinite animations, so an overlay with a spinner inside closes on time instead of reappearing at full opacity.
- **BbSelectContent**: hover and keyboard highlight are written by JavaScript only, so two options can no longer appear focused at once.
- **BbDropdownMenuContent**: clicking a nested portal inside an open menu no longer closes the menu. Outside-click detection now resolves elements by id per event, so it does not go stale after a re-render.
- **BbFloatingPortal**: removed the fixed 500ms deadline on the portal host render signal, which timed out on slow connections. The wait is now unbounded and cancelled on close or disposal.
- **BbFloatingPortal**: a listbox is focused only after the reveal, so arrow keys no longer reach the trigger while the overlay is still hidden.
- **JavaScript modules**: a browser or CDN that serves a stale copy of a bundled module no longer kills the circuit at the first call. The bundle fails at load with an error that names the file and says what to do.
- **NavigationMenuContext**: the close timer catches every exception, so an unexpected error in the fire-and-forget handler can no longer close the Blazor Server circuit.
- **BbPopoverContent** and **BbDropdownMenuContent** no longer render a second time on open. The duplicate render raised a portal refresh mid-cycle that the host had to defer by a round trip.

### Performance

- **Overlays** open and close in one circuit round trip each. The portal registers content, positions, reveals, starts auto-update, and wires dismissal and keyboard listeners in a single interop call that is not awaited. On a 20ms round trip, a select open went from 11 messages to 1 and a close from 16 to 1 compared with v3.
- **Overlays**: arrow keys and option hover in a listbox no longer send a message to the server.
- **JavaScript modules** are imported once per circuit and shared, instead of once per component instance. Pages with many inputs issue far fewer round trips on Blazor Server.
- **Floating UI** is statically imported by `positioning.js`, removing a hidden dynamic import on the first position computation.
- **BbDataGridRow** and **BbTableRow** no longer attach keyboard and click handlers per row. **BbDataGrid** and **BbTable** delegate a single handler from the container, and rows opt in via `data-bb-row-keys` and `data-bb-row-click`.
- **BbSelectContent** scrolls the selected option into view and attaches the keyboard handler in the same call that opens the overlay, so the list appears already scrolled to the selection.
- **BbDropdownMenuContent** drops a redundant width-matching interop call; `MatchAnchorWidth` already covers it.
- **BbTooltipContent** and **BbHoverCardContent** no longer request the portal's ready callback, which was empty and cost a round trip on Blazor Server.
- **BbSelectContent**, **BbPopoverContent** and **BbDropdownMenuContent** restore focus to the trigger inside `overlay.close` instead of awaiting `FocusAsync` after the close render, saving a round trip on every Escape and every selection.
- **BbPopoverContent** requests the portal's ready callback only when a consumer has set `OnContentReady`. Without one, the callback was a round trip spent notifying no one.
- **Overlays**: an element named by `AutoFocusId` is focused inside the open call, replacing a ready callback, a 50ms sleep, and a second round trip.
- **Scroll containers**: `observeNearBottom` checks the scroll position in the browser, coalesced to one check per frame, and calls .NET once on entering the near-bottom zone instead of once per scroll event.
- **Lists**: `observeHover` uses one delegated hover listener per list and reports only a genuine change of item, so pointer travel no longer sends a message per pixel.

### Improvements

- **README** documents the JavaScript bundle, the `overlay.open` pattern, and the rules for adding a primitive that needs JavaScript.

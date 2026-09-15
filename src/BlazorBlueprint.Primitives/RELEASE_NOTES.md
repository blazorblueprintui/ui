## What's New in v4.0.0-beta.1

**This is a prerelease.** The API may still change before the stable v4.0.0 release.

### Breaking Changes

- **BbPopoverContent**, **BbSelectContent**, **BbDropdownMenuContent**: the `JsOnClickOutside` and `JsOnEscapeKey` JSInvokable methods are removed. Dismissal now arrives through the new `BbFloatingPortal.OnDismiss` callback.
- **BbTableRow**, **BbDataGridRow**, **BbPopoverContent**, **BbHoverCardTrigger**, **BbCategoryPortalHost** no longer implement `IAsyncDisposable`. Code that awaited `DisposeAsync()` on these components must be updated.
- **JavaScript modules**: the `onClickOutside` and `onEscapeKey` exports are removed from `click-outside.js`. Use `overlay.open` with `FloatingDismissOptions` instead.
- **JavaScript modules**: all primitive modules are now bundled into `bb-primitives.js` and exposed under a namespace (`focusTrap.createFocusTrap`, `positioning.computePosition`, etc.). Custom code that imported the individual module files must load the bundle via `PrimitiveModules.GetAsync` and use the namespaced identifier.
- **escape-keydown.js**: `initialize` now takes an optional `methodName` argument; the default callback name changed from `HandleEscape` to `JsOnEscapeKey`.

### New Features

- **Native dialog rendering**: **BbDialog** gains a `RenderingStrategy` parameter. Set it to `OverlayRenderingStrategy.Native` to render a browser `<dialog>` element driven by `showModal()`, which works across Blazor render-mode boundaries and does not need a portal host. Falls back to JavaScript rendering with a diagnostic when the browser lacks support.
- **OverlayRenderingOptions**: `AddBlazorBlueprintPrimitives` now accepts a configure callback to set a global `DefaultStrategy` for all overlays.
- **INativeOverlayService**: new scoped service that resolves the effective rendering strategy and drives the native `<dialog>` element (show, close, focus, and lifecycle events).
- **BbDialogContent** gains `CloseOnOverlayClick` to control whether a backdrop click closes a native dialog.
- **BbFloatingPortal** gains `Dismiss` (`FloatingDismissOptions`) and `OnDismiss` (`EventCallback<FloatingDismissReason>`). Owners declare which dismissal gestures to listen for and the portal wires them in the same call that opens the overlay.
- **FloatingDismissReason** enum reports whether an overlay was dismissed by an outside interaction or the Escape key.
- **JsModules.GetAsync** and **PrimitiveModules.GetAsync**: shared, per-circuit module references that any component can use without owning or disposing them.

### Bug Fixes

- **Overlays**: Escape now closes only the topmost open overlay. A popover inside a dialog no longer closes the dialog on the first press.
- **BbDropdownMenuContent**: clicking a nested portal inside an open menu no longer closes the menu. Outside-click detection now resolves elements by id per event, so it does not go stale after a re-render.
- **BbFloatingPortal**: removed the fixed 500ms deadline on the portal host render signal, which timed out on slow connections. The wait is now unbounded and cancelled on close or disposal.

### Performance

- **Overlays** open in one interop call instead of five. Positioning, reveal, auto-update, and dismissal listeners are wired together; closing is a single call. Median time from click to visible for a select dropped from 147ms to 81ms on a 20ms round trip.
- **JavaScript modules** are imported once per circuit and shared, instead of once per component instance. Pages with many inputs issue far fewer round trips on Blazor Server.
- **Floating UI** is statically imported by `positioning.js`, removing a hidden dynamic import on the first position computation.
- **BbDataGridRow** and **BbTableRow** no longer attach keyboard and click handlers per row. **BbDataGrid** and **BbTable** delegate a single handler from the container, and rows opt in via `data-bb-row-keys` and `data-bb-row-click`.
- **BbSelectContent** scrolls the selected option into view and attaches the keyboard handler in one interop call, and the list appears already scrolled to the selection.

### Improvements

- **README** documents the JavaScript bundle, the `overlay.open` pattern, and the rules for adding a primitive that needs JavaScript.

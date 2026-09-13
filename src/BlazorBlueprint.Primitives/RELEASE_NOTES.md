## What's New in v3.17.0

### Breaking Changes
- **HoverCardTrigger** — now implements `IAsyncDisposable` instead of `IDisposable`, so it can release its JS module. Only code that calls `Dispose()` on the trigger directly is affected.

### New Features
- **FocusTrapInitialFocus** — new enum controlling what a focus trap focuses on open: `FirstFocusable` (the default and previous behaviour), `Container` or `None`.
- **DialogContent**, **SheetContent** — new `InitialFocus` and `InitialFocusElement` parameters, so a dialog whose first tabbable child acts on focus no longer needs a dummy `tabindex="0"` element to opt out.
- **IFocusManager** — new `TrapFocus(container, initialFocus, initialFocusElement)` overload. It is a default interface implementation, so existing implementers keep compiling and keep their current behaviour.
- **Focus trap** — an element carrying `data-autofocus` inside the container is now honoured, matching the convention `portal.js` already uses.

### Bug Fixes
- **HoverCardTrigger** — the card no longer opens when focus is moved programmatically, by a focus trap or an explicit `.focus()` call. A real Tab to the trigger, or focus landing in a text field, still opens it.
- **Focus trap** — `data-autofocus` and the trap's own initial focus could previously both fire and race inside a dialog; they now resolve to a single target.

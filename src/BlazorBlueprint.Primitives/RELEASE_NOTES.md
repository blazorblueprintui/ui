## What's New in v3.15.0

### Breaking Changes
- **IPositioningService** — added `HidePositionAsync`, which has no default implementation; custom implementations of the interface must add it.

### New Features
- **DataGrid** — added `Order` on `IDataGridColumn`, an explicit zero-based position among the grid's data columns (defaults to `null`, keeping registration order).
- **DataGrid** — added `DataGridColumnState.SyncColumns`, which adds entries for newly registered columns while preserving existing visibility, width, and user reordering.
- **DataGridHeaderCell** — added `AriaLabel` for headers whose content carries no text of its own, such as icon-only headers.
- **TriggerContext** — added `NotifyConsumed()`, so a custom `AsChild` trigger child that only touches the context inside event handlers can acknowledge it.
- **PositioningService** — added `HidePositionAsync`, returning a floating element to its hidden state through the same JS path that made it visible.

### Bug Fixes
- **Popover** — Escape is now watched at the document instead of on the content element, so it closes the popover even though focus stays on the trigger.
- **FloatingPortal** — overlays are now hidden through JS, matching how they were shown; previously a closed overlay whose markup was otherwise unchanged could stay visible.
- **Popover**, **Select**, **DropdownMenu** — a close racing a teardown no longer throws out of disposal and take down the Blazor Server circuit.
- **DataGrid** — rightward column drags no longer overshoot by one position.
- **DataGrid** — columns registering after initialization are kept and stay visible, rather than being dropped.
- **DataGrid** — a header cell supplying a `HeaderTemplate` can now be given an accessible name, so the column is not left unnamed for assistive technology.
- **PortalHost** — content-only portal updates arriving in the Blazor Server render-to-acknowledgement window are recovered instead of leaving stale content on screen.
- **PortalHost** — the deferred content flush now yields before re-rendering, avoiding a stack overflow on WebAssembly when a nested portal refreshes on every render.

### Improvements
- **TooltipTrigger**, **HoverCardTrigger** — log a warning in the Development environment when `AsChild="true"` is used with a child that never consumes the cascaded `TriggerContext`, which would otherwise leave the overlay silently unopenable.
- **TooltipTrigger**, **HoverCardTrigger** — expanded `AsChild` documentation covering what the child is responsible for and when to leave it `false`.

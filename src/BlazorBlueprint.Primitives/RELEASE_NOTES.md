## What's New in v4.2.0

### Breaking Changes

- **GanttBuilder**: refusals now throw `GanttBuildException`, which derives from `ArgumentException`. `catch (ArgumentException)` keeps working, but a check for that exact type (for example `Assert.Throws<ArgumentException>`) no longer matches.
- **Positioning**: floating content now takes a z-index above every raised ancestor of its trigger, instead of a fixed 50 (or 9999 for fixed positioning). Triggers in normal flow are unchanged.
- **Dependencies**: `Microsoft.AspNetCore.Components.Web` is now 10.0.14.

### New Features

- **BbSheetContent**: `NonModal` and `CloseOnInteractOutside`. A non-modal sheet is `aria-modal="false"`, doesn't trap focus or lock scroll, and can close on an outside pointer-down.
- **BbDialogContent**: `EnterAnimation` and `ExitAnimation` (`Func<ElementReference, Task>`). A dialog with an exit animation stays mounted with `data-state="closed"` until the animation ends, keeping its focus trap, scroll lock and Escape handling until then. Components' `BbDialog` animations are built on this.
- **IKeyboardShortcutService.RegisterAsync(shortcut, callback, allowInEditable, id)**: lets a shortcut fire while focus is in an input, textarea, select or contenteditable element. A default interface implementation keeps existing custom services compatible.
- **BarcodeFormatException**: `Error` (`BarcodeFormatError`) and `Arguments` give each refusal a machine-readable reason, so a UI can show it in another language. `Message` is unchanged.
- **GanttBuildException** and **GanttBuildError**: the same reason and argument pattern for `GanttBuilder`.
- **DataGridColumnWidth**: shared column width parsing (hidden from IntelliSense). Widths accept any CSS unit, and a zero width is never stored.
- **BbPopoverTrigger**: an `id` given to the trigger becomes `PopoverContext.TriggerId`, so focus restore and click-outside use the element actually rendered. A caller's `aria-haspopup` and `aria-controls` now win.

### Bug Fixes

- **Overlays**:
  - Overlays open and close on plain-HTTP sites, where `crypto.randomUUID` is missing.
  - Each dismissal listener is wired on its own, so one failure can't leave an overlay stuck open.
- **Tooltip and overlays**: hide on close even when the closed render arrives late on Blazor Server.
- **Keyboard shortcuts**:
  - Disposing an older handle no longer removes a newer registration for the same key.
  - IME composition events are ignored.
- **Menus**:
  - The menubar has one roving Tab stop.
  - Pointer presses inside portaled submenus count as inside.
  - Keyboard-focused items scroll into view within their panel.
- **BbTreeView**: a node re-renders only when its own state changes, and position lookups are constant time, so selection stays fast with thousands of siblings.
- **Comma-decimal cultures**: Progress writes `aria-value*` and `data-*` numbers invariantly. Column widths parse without thousands separators.

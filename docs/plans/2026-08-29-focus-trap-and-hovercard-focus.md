# Focus trap initial target, and HoverCard opening on programmatic focus

Drafted 2026-08-29 from an external report against 3.15.0. Both defects were re-confirmed
against `develop` at 3.16.0 before this plan was written; neither is already fixed.

| # | Step | Size | Breaking |
|---|---|---|---|
| 1 | `createFocusTrap` honours an initial-focus target | M | no |
| 2 | `InitialFocus` parameter on the four trapping components | S | no |
| 3 | HoverCard: do not open on programmatic focus | S | no |
| 4 | Decide whether `OpenDelay` applies to keyboard focus | S | see below |

---

## 1. The focus trap always focuses the first tabbable descendant

`focus-trap.js:57-61` ends `createFocusTrap` with an unconditional
`focusableElements[0].focus()`. `createFocusTrap(container)` takes one argument, and so does
`IFocusManager.TrapFocus(ElementReference container)`, so there is no override anywhere in the
chain.

The reporter's point about the container is correct. `Primitives/Dialog/BbDialogContent.razor`
renders its container with `@ref="_contentRef"` (line 14) and `tabindex="-1"` (line 20), and
passes that same reference to `TrapFocus` on line 104. The container is focusable and already in
hand; the trap reaches past it.

**Consequence.** Any dialog whose first tabbable descendant has an on-focus side effect fires it
on open. The only consumer workaround is a dummy `tabindex="0"` element, which adds a phantom tab
stop.

### The library already has an autofocus convention

Worth knowing before designing anything: `[data-autofocus]` is **already** honoured, but by a
different mechanism. `portal.js:168-190` installs a `blazorblueprint:visible` listener that
focuses `[data-autofocus]` within the element that became visible, and
`BbDropdownMenuContent.razor:165` relies on it.

Two things follow. The fix is cheaper than the report assumes — teaching the trap the same
attribute matches an existing convention rather than inventing one. And the two mechanisms can
**race** today: a consumer who puts `data-autofocus` inside a dialog has the portal listener and
`focusableElements[0].focus()` both firing, with the winner decided by event ordering. That race
is a live bug even before this work, and closing it is part of the value here.

### Proposed resolution order

Inside `createFocusTrap`, pick the initial focus target in this order:

1. an explicit element passed from C#
2. `[data-autofocus]` within the container
3. the container itself, **if** the caller asked for it
4. the first tabbable descendant — today's behaviour, and still the default

Steps 1-2 are additive. Step 4 stays the default so no existing dialog changes.

## 2. `InitialFocus` on the trapping components

`TrapFocus` gains an optional argument, and the parameter is plumbed through the four components
that call it:

- `Primitives/Dialog/BbDialogContent.razor:104`
- `Primitives/Sheet/BbSheetContent.razor:105`
- `Components/Drawer/BbDrawerContent.razor:80`
- `Components/Dialog/BbDialogProvider.razor:140`

`IFocusManager.TrapFocus` is public, so the new argument must be optional — adding a required
parameter to a public interface method breaks any consumer implementing it.

**Radix parity is a separate decision.** Radix focuses the container by default and exposes
`onOpenAutoFocus` to redirect. Matching that default would change behaviour for every existing
dialog, so it belongs behind the parameter (option 3 above) rather than as the new default. If it
is ever made the default, that is a major.

## 3. HoverCard opens on any focus, including programmatic

Confirmed in both branches:

| path | mouse | focus |
|---|---|---|
| `AsChild="false"` | `HandleMouseEnter:235` → `Timer(Context.OpenDelay)` | `HandleFocus:263` → `Context.Open` immediately |
| `AsChild` | `HandleMouseEnterForContext:120` → `Timer(Context.OpenDelay)` | `HandleFocusForContext:146` → `Context.Open` immediately |

The sharper half of the report is 2(b), not 2(a). Opening on *any* focus means a focus trap
moving focus onto the trigger — or any explicit `.focus()` — pops the card open with no user
intent behind it. Gating the focus path on `:focus-visible` fixes that, and keyboard users still
get the card because Tab sets `:focus-visible`.

**`:focus-visible` does not work. Measured, not assumed.** It was implemented and tested in the
running demo, and Chrome reports `:focus-visible` as **true** for a programmatic `.focus()` on the
trigger's `div[tabindex="0"]` — including directly after a real, trusted mouse click. The card
still opened. `BbTableRow.razor:58-59` already recorded the same limitation from the other
direction.

**What was shipped instead:** `element-utils.js` tracks the timestamp of the last `Tab` keydown at
the document, in the capture phase, and `isKeyboardFocus(element)` returns true only when a Tab
happened within 500 ms — or when the element is a text field, where focus is unambiguous. Tab is
how a keyboard user reaches a trigger, and nothing moves focus programmatically in response to
Tab, so it separates the two cases where the selector cannot. The check returns true whenever it
cannot run (prerender, module unavailable), because a keyboard user losing the card is a worse
regression than the bug.

## 4. Should `OpenDelay` apply to keyboard focus? — recommend **no**

The report asks for it on consistency grounds. I think that is wrong, and it is the one place
this plan does not follow the report.

`OpenDelay` defaults to **700 ms** (`BbHoverCard.razor:64`, `HoverCardContext.cs:19`). The delay
exists because a pointer sweeps across elements incidentally, so an immediate open would fire
cards the user never asked for. Tab does not do that — landing on a trigger is deliberate. So the
two input methods differ for a real reason, not an unstated one.

Applying the delay would make a keyboard user wait 700 ms for something a mouse user gets by
resting still. That is a straight accessibility regression, and WCAG 1.4.13 is the reason the
focus path exists at all.

Once step 3 lands, the reported symptom is gone: programmatic focus no longer opens the card.
What is left is a deliberate difference. **Recommendation: gate on `:focus-visible` and keep the
focus path immediate.** If consistency is still wanted afterwards, expose a separate
`FocusOpenDelay` defaulting to 0, rather than reusing `OpenDelay`.

## Verification

- A dialog whose first tabbable child has an on-focus side effect: the effect must not fire on
  open once `InitialFocus` or `[data-autofocus]` is set.
- The existing default path must be unchanged — a dialog with no `InitialFocus` still focuses the
  first tabbable descendant.
- `data-autofocus` inside a dialog must be deterministic, not a race.
- Tab onto a HoverCard trigger opens the card, in Chrome, Firefox and Safari.
- A programmatic `.focus()` on the trigger does **not** open it.
- Blur still closes; `CloseDelay` unchanged.

## Not included

- Making container-focus the default. Behaviour change; needs a major.
- `onOpenAutoFocus`-style callbacks. The parameter covers the reported need; a callback can be
  added later without breaking it.

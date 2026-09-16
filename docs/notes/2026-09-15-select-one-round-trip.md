# Task: cut BbSelect (and the shared overlay path) to one circuit round trip per open and one per close

## Why

On Blazor Server every awaited JS interop call, and every render batch whose ack gates the next
`OnAfterRenderAsync` step, is a full network round trip — sequentially. Measured on a real deployment
(Luma, nwb, ~90 ms circuit round trip) by hooking the circuit's WebSocket:

- **3.14.1:** a `BbSelect` open = 11 sequential round trips (7 awaited JS calls + ack-gated renders)
  → 1.6 s. On a 500 ms circuit the same open took 5.5 s. Close = 16 awaited JS calls.
- **4.0.0-beta.1:** open = 5 round trips → 0.5 s. Close = 3 round trips → 0.36 s. Awaited JS calls are
  now 2 per open (`overlay.open`, `openListbox`) and 2 per close (`cleanupKeyboardNavigation`,
  `overlay.close`).

Server time per event is 4–48 ms throughout; the cost is entirely round-trip count × latency.

**Target: open = 1 round trip, close = 1 round trip.**

## Measured wire sequence, 4.0.0-beta.1, open

```
client → DispatchEventAsync (click)
server ← RenderBatch: trigger aria-expanded / data-state              round trip 1
        (client acks → OnAfterRenderAsync on the server)
server ← RenderBatch: popup content into the portal (~3.5 KB)         round trip 2  ← content is a second, ack-gated render
        (ack → OnAfterRenderAsync)
server ← BeginInvokeJS overlay.open … client → EndInvokeJSFromDotNet  round trip 3  ← awaited, result unused
server ← RenderBatch (small)
        (ack → OnAfterRenderAsync)
server ← RenderBatch (small)                                            round trip 4  ← state flip after positioning
server ← BeginInvokeJS openListbox … client → result                    round trip 5  ← awaited
server ← RenderBatch ×4 (data-focused, aria-activedescendant, …)
```

Close: `cleanupKeyboardNavigation` awaited → render → `overlay.close` awaited → renders (3 round trips).

## Changes

1. **Render the popup content in the same render batch as the open state.** When `IsOpen` becomes
   true, the trigger attributes and the portal content must go out in one batch — no portal mount
   that waits for `OnAfterRenderAsync` to add the content.
2. **Do not await `overlay.open`.** Call it with `InvokeVoidAsync` and continue; positioning,
   trigger-width matching and reveal are already applied in JS. Remove the render that currently
   follows it.
3. **Fold `openListbox` (initial-option focus + keyboard-nav attach) into that same fire-and-forget
   call.** Pass the `DotNetObjectReference` for keyboard callbacks in the one call.
4. **Remove the `OnAfterRenderAsync`-chained re-render** after the content is acked (the
   positioned/visible flag). Visibility is a style JS sets; if .NET needs a flag, set it before the
   first render, not after an ack.
5. **Let JS own `data-focused` and `aria-activedescendant`** during open and keyboard navigation;
   call back into .NET only on selection (and Escape/Tab as today). No render batches for focus moves.
6. **Close:** one fire-and-forget `overlay.close` that also runs `cleanupKeyboardNavigation`, then a
   single render to unmount the content.
7. **Apply the same rule to every overlay sharing this path** (`BbPopover`, `BbDropdownMenu`,
   `BbTooltip`, `BbHoverCard`, `BbDialog`, `BbSheet`) and to `BbSidebarProvider`, which awaits
   `sidebar.initialize` in `OnAfterRenderAsync`.

   Rule: never `await` a JS call whose result .NET does not read; never advance open/close state
   through `OnAfterRenderAsync`.

## Constraints

- Keep 3.16/3.17 behaviour: exit animations (`hidePosition` waiting for the animation), the
  Tab-arrival focus check, `InitialFocus` / `[data-autofocus]` on dialogs, `matchReferenceWidth`.
- Keyboard accessibility unchanged: Escape, Tab, arrow navigation and `aria-activedescendant` still
  correct in the DOM — written by JS, not by a render.
- Public component API unchanged unless unavoidable; note any change in the changelog as breaking.
- Fire-and-forget calls must still handle a disconnected circuit (`JSDisconnectedException`) silently.

## Acceptance — measure, don't assume

Run the demo app (Blazor Server), open DevTools, paste this before clicking a `BbSelect` trigger:

```js
window.__ws={raw:[],socket:null,t0:0};const w=window.__ws,txt=d=>{const b=d instanceof ArrayBuffer?new Uint8Array(d):new TextEncoder().encode(String(d));let s='';for(const x of b)s+=(x>=32&&x<127)?String.fromCharCode(x):' ';return s.replace(/\s+/g,' ')};
const push=(dir,d)=>{d instanceof Blob?d.arrayBuffer().then(a=>w.raw.push({at:performance.now(),dir,text:txt(a)})):w.raw.push({at:performance.now(),dir,text:txt(d)})};
const send=WebSocket.prototype.send;WebSocket.prototype.send=function(d){if(!w.socket&&String(this.url).includes('/_blazor')){w.socket=this;this.addEventListener('message',e=>push('←',e.data))}if(this===w.socket)push('→',d);return send.call(this,d)};
addEventListener('pointerdown',()=>{w.raw=[];w.t0=performance.now()},true);
window.__report=()=>w.raw.sort((a,b)=>a.at-b.at).map(r=>{const t=r.text,k=/DispatchEventAsync/.test(t)?'event':/OnRenderCompleted/.test(t)?'ack':/EndInvokeJSFromDotNet/.test(t)?'js-result':/JS.RenderBatch/.test(t)?'render':/JS.BeginInvokeJS/.test(t)?'js-call':'other';return Math.round(r.at-w.t0)+r.dir+k}).filter(s=>!s.endsWith('ack')).join(' ');
```

Click the trigger, wait a second, run `__report()`. Done means, for an open:

- exactly one `render` before the popup is visible (open state + content together),
- zero `js-result` messages (no awaited JS call),
- no further `render` after the reveal until the user interacts,
- and the same for close: one `render`, zero `js-result`.

Test with the Chrome window in the foreground; with DevTools network throttling on, the popup must
still appear after a single round trip. Add a browser test that asserts the message shape so it
cannot regress.

## Also observed in the same deployment (not this task, but worth knowing)

- The 3.14.1 → 3.17.0 chain was: `computePosition`, `applyPosition`, `matchReferenceWidth`,
  `focusInitialOption`, a per-open module `import`, one unnamed call, `setupKeyboardNavigation` —
  each awaited. The beta's barrel modules and `overlay.open` fixed most of it; this task is the rest.
- A consumer behind Cloudflare with "Browser Cache TTL" set had 4-hour-old `sidebar.js` cached across
  the beta deploy; the new `bb-components-core.js` barrel imported it and `sidebar.initialize` was not
  a function, killing every circuit. Static imports between library modules mean a partially cached
  set is fatal — a version query on intra-library imports, or a startup check that the module set
  matches, would turn that into a clear error instead.

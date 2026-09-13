# Measuring overlay latency in a deployed app

The demo is not fast because it is a demo. Before assuming your app is slow for a reason the
library can fix, measure it — the same way, so the numbers are comparable.

## What we already ruled out

**Page weight is not the variable.** Opening the same overlay on `/components/datagrid`
(7,431 DOM nodes, 978 KB of HTML) and on `/components/select` (1,124 nodes, 215 KB) takes the
same time to the millisecond — 98ms to visible, 38ms from inserted to visible, at a 20ms round
trip. Blazor's diff is scoped to what changed, so a big static page does not slow an overlay.

If your deployed app is slower than the demo, the difference is one of these instead:

| Suspect | What it looks like | How to tell |
|---|---|---|
| Round-trip **tail**, not median | median fine, p90 bad | compare p50 and p90 of `visible` below |
| Server work per hop | every stage inflated evenly | server-side span per interaction |
| First use of an unbundled module | **first** interaction slow, later ones fine | a `.js` fetch in the trace below |
| Circuit contention | erratic, worse under load | correlate with concurrent users |
| Proxy buffering WebSocket frames | uniformly worse than raw RTT | compare RTT to WebSocket frame timing |

## The measurement

Paste into the DevTools console on the deployed page, then click the overlay trigger.

```js
(() => {
  const runs = [];
  window.__bbReport = () => {
    const f = k => runs.map(r => r[k]).filter(v => v != null).sort((a, b) => a - b);
    const pick = (a, p) => a.length ? a[Math.min(a.length - 1, Math.floor(a.length * p))] : null;
    for (const k of ['inserted', 'visible', 'insertedToVisible']) {
      const a = f(k);
      console.log(k.padEnd(18), 'p50', pick(a, 0.5), ' p90', pick(a, 0.9), ` (n=${a.length})`);
    }
    console.log('js fetched after click:', runs.flatMap(r => r.fetches));
    return runs;
  };

  const arm = () => {
    const m = { t0: null, inserted: null, visible: null, res0: performance.getEntriesByType('resource').length };
    const onDown = () => { if (m.t0 === null) m.t0 = performance.now(); };
    document.addEventListener('pointerdown', onDown, { capture: true, once: true });

    const obs = new MutationObserver(() => {
      if (m.t0 === null) return;
      const el = document.querySelector('[data-portal-content][data-state="open"]');
      if (!el) return;
      if (m.inserted === null) m.inserted = performance.now();
      if (m.visible === null && /visibility:\s*visible/.test(el.getAttribute('style') || '')) {
        m.visible = performance.now();
        obs.disconnect();
        runs.push({
          inserted: Math.round(m.inserted - m.t0),
          visible: Math.round(m.visible - m.t0),
          insertedToVisible: Math.round(m.visible - m.inserted),
          fetches: performance.getEntriesByType('resource').slice(m.res0)
            .filter(r => r.name.endsWith('.js')).map(r => r.name.split('/').pop())
        });
        console.log('open:', runs[runs.length - 1]);
        arm();
      }
    });
    obs.observe(document.body, { childList: true, subtree: true, attributes: true, attributeFilter: ['style', 'data-state'] });
  };

  arm();
  console.log('Armed. Open and close the overlay a dozen times, then run __bbReport()');
})();
```

Open and close the overlay a dozen times, then call `__bbReport()`.

## Reading the result

- **`insertedToVisible` near one round trip** — the library is doing its job; the time is
  elsewhere. Look at `inserted`, which is Blazor rendering your page, not the overlay.
- **`insertedToVisible` several round trips** — an overlay path that still issues serial interop
  calls. Worth reporting with the component name.
- **Any `.js` in `fetched after click`** — a module loading on first use. Each one is a round trip
  that the bundle was meant to remove; 27 modules in the Components layer still work this way.
- **p90 far above p50** — the network tail, multiplied by however many round trips the
  interaction costs. Reducing the count is the only thing that helps; the link will not get
  steadier.

## Counting round trips

The count matters more than any single hop, because they are serial. To see them, the WebSocket
has to be wrapped before the circuit connects — so paste this, then reload, then interact:

```js
sessionStorage.__bbCount = '1';
```

…and have the app's startup script check for it. There is no way to hook a circuit that has
already connected from the console alone.

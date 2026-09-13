# Quirks and learnings

Evergreen reference for gotchas that have cost real time in this repo. Not a changelog and not a
style guide — each entry is a trap that *looked* like something else, plus the tell that identifies
it next time.

Read this when a problem has a recognisable shape from the list. Don't read it front-to-back.

---

## Verification traps

These are about **believing a check that never happened**. Each produced a confident "verified"
that was false; two shipped or nearly shipped a broken build, and one invented a regression that
was not there.

### 1. Never `tail` a `dotnet build`

MSBuild prints its elapsed-time line on **failure as well as success**, so:

```bash
dotnet build 2>&1 | tail -2
```

looks essentially identical either way. A `ButtonSize.Sm` typo reached `develop` and broke every
demo host because the build output was read through `tail`.

**Do this instead** — check the exit code, and let the error lines through:

```bash
dotnet build 2>&1 | grep -E "error|warning|Build succeeded" ; echo "exit=${PIPESTATUS[0]}"
```

or simply `dotnet build > /tmp/build.log 2>&1 || tail -40 /tmp/build.log`.

**The tell:** a build "succeeded" but the change has no visible effect.

### 2. `dotnet run --no-build` will serve a stale assembly

`--no-build` does exactly what it says — it does not notice that the source has changed since the
last successful compile. Combined with trap (1) this produced a browser "verification" of markup
that had **never compiled**: the build had failed, the failure was invisible, and `--no-build`
happily served the previous assembly.

**The tell:** a DOM probe cannot find an element you just added. Suspect the build before you
suspect the code.

### 3. Editing a `.js` file does not invalidate the browser's ES module

Blazor's JS interop loads modules via `import("./_content/.../foo.js")`. The browser caches the
module for the lifetime of the page. After editing `positioning.js` the page kept the **old**
module — the new export simply did not exist, `InvokeVoidAsync` threw, and a
`catch (JSException)` swallowed it silently. The symptom was indistinguishable from "my fix
didn't work".

**Do this instead** — hard-reload the page after any `.js` change, and if you need proof the new
file is actually being served:

```js
await fetch('/_content/BlazorBlueprint.Components/js/foo.js', { cache: 'reload' })
    .then(r => r.text())
    .then(t => t.includes('myNewExport'));
```

**The tell:** a JS-backed feature behaves exactly as it did before the edit, with no console error
(because the interop exception was caught and ignored).

---

### 4. Playwright's `addInitScript` stacks, and every later measurement is wrong

`page.addInitScript(...)` registers the script on the **context**, not on the navigation. Calling it
once per measurement run against the same context installs N copies. A harness that wrapped
`WebSocket.send` to emulate latency therefore emulated `N × 10ms`, and logged every frame N times.

Five consecutive "after" measurements got monotonically worse — 214ms, 250ms, 298ms, 363ms, 414ms —
which read exactly like a performance regression in the change under test. It was the harness.

**Do this instead:** build a fresh context inside the run and close it at the end.

```js
const context = await page.context().browser().newContext();
try { await context.addInitScript(fn, arg); /* … */ } finally { await context.close(); }
```

**The tell:** repeated runs drift in one direction while staying stable *within* a batch. A real
regression is stable across batches; an accumulating harness is not. Restarting the server does not
help, which is the second tell — it points at the browser, not the app.

---

## Rendering and interop

### Blazor diffs against what it last rendered, not against the DOM

This is the single most load-bearing fact about mixing JS and Blazor in this codebase, and it cuts
both ways.

If JS writes to an element's `style` (or any attribute Blazor also renders), Blazor will **not**
correct it on the next render unless the value *it* renders has changed since *its* last render.
The DOM is never consulted.

- **It bit us** in `BbCopyText`: JS wrote `visibility`/`opacity` with `!important`, taking ownership
  of `style`. A closed portal's markup is byte-identical to its pre-open markup, so no update was
  emitted and the JS values survived — tooltips stuck visible forever. Fixed by making hiding go
  back through JS (`HidePositionAsync`) rather than hoping a re-render would undo it.
- **We rely on it** in `BbResizablePanelGroup`: during a drag, JS owns `flex-basis` and C# state is
  deliberately stale, so re-renders leave the dragged panels alone. C# is synced once on pointer-up,
  and only *then* does the rendered style string change.

**Rule of thumb:** whichever layer writes an attribute must own it for the whole interaction, and
must hand it back at a defined sync point. Do not interleave.

### An `import(...)` issued from C# is a circuit round trip, not a fetch

On Blazor Server, `IJSRuntime.InvokeAsync<IJSObjectReference>("import", path)` posts an instruction
to the browser and awaits the reply. The cost is the circuit, not the file — it is paid once per
module per page load whether or not the file is already in the browser's HTTP cache, and it scales
with the user's latency. Three chained module loads read as "1ms, 3ms, 2ms to fetch" in DevTools
with 30–90ms gaps between them, and the gaps are the whole story.

A nested `await import(...)` *inside* a module is worse, because nothing on the C# side can see it.
Floating UI was loaded that way from `positioning.js` for exactly this reason.

**Do this instead:** reach everything through `PrimitiveModules.GetAsync`, which imports one bundle
and caches it per circuit, and make intra-module dependencies static imports so the browser resolves
the graph itself. `PrimitiveJsModuleTests` fails the build if a new module escapes the bundle.

**The tell:** a slow interaction whose network panel shows small files and large gaps.

### Blazor resolves a dotted interop identifier by walking the object

`module.InvokeVoidAsync("clickOutside.onEscapeKey", …)` works: Blazor splits the identifier on `.`
and walks from the module object, so `export * as clickOutside from './click-outside.js'` is directly
addressable. This is what makes one bundle possible despite six modules all exporting `initialize`
and `dispose`.

A wrong identifier fails only at runtime, in the browser, as `Could not find 'x.y'` — and only on the
path that uses it. Neither the compiler nor the API surface snapshot can see it.

### Overlay content lives in a different subtree from its trigger

Overlays render through `<PortalHost />`, so once content is open the trigger and the content are
in **different DOM subtrees**. Keyboard handlers bound with `@onkeydown` on the *content* never fire
if focus never moved off the trigger — the key event bubbles nowhere near the handler.

`BbPopover`'s Escape key was broken this way. The fix was the document-level `onEscapeKey` helper in
`click-outside.js`.

**The tell:** a handler that is obviously bound and obviously correct simply never runs.

### `ForceMount` defaults to `true` on `BbFloatingPortal`

Which means most components take `HandleForceMountLifecycleAsync`, **not** the standard mount path.
Analysing the standard path when the component is actually on the force-mount path burns a lot of
time and produces confident, wrong conclusions.

Check which path the component is on before reasoning about portal lifecycle.

---

## Culture

### On Blazor Server, `CultureInfo.CurrentCulture` is the *server's* culture

Not the user's. This is why the `BbCurrencyInput` ×100 bug hit everyone regardless of their locale:
format/parse used the *currency's* culture, focus wrote *invariant*, and the JS sanitiser got the
*ambient* one. Three layers each individually defensible, disagreeing with each other.

Any component that formats or parses numbers needs one explicit culture decision applied at every
boundary — including the JS side, which has its own idea of what the locale is.

**Corollary:** culture-sensitive fixes verified on Server are **not** verified on WebAssembly, where
`CurrentCulture` really is the user's. They differ by construction.

---

## Process

### `Fixes #N` does not close issues here

PRs merge into `develop`, but the repository's default branch is `main`. GitHub only auto-closes
issues from the default branch, so closing keywords in a PR body **never fire**. Every issue has to
be closed by hand after the merge.

Assume nothing auto-closes. Same applies to the project board — status moves are manual.

### Coincidences that look like working code

Two `h-5` stepper buttons summed to the default `h-10` field height **by coincidence** in
`BbNumericInput`. The `ButtonClass` was `static`, so it could never see the instance's `Class` —
the layout was correct only for the default size and silently wrong for every other one.

**The tell:** a `static` member in a component that reads like it should be per-instance. If it
composes anything derived from a parameter, it is a bug waiting for someone to change the size.

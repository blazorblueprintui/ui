# Blazor Blueprint v4 Migration Guide

This guide helps you upgrade from Blazor Blueprint **v3** to **v4**. Breaking changes that require
code updates come first, followed by anything you can adopt at your own pace.

This guide is written as v4 is built, so it grows as changes land.

---

## Migration Checklist

| # | Breaking Change | Severity | Action Required |
|---|---|---|---|
| 1 | `BbDrawerTrigger` / `BbDrawerClose` render a real `<button>` | **Medium** | Add `AsChild="true"` where the child is already a control |
| 2 | `BbTooltipTrigger.AsChild` default → `false` | **Medium** | Add `AsChild="true"` where the child consumes the trigger context, such as a `BbButton` |
| 3 | Every utility in `blazorblueprint.css` is prefixed `bb:` | **Low** for most; **Medium** if you relied on the shipped utilities without your own Tailwind build | Nothing if you run Tailwind. Otherwise, see below |

---

## 1. `BbDrawerTrigger` and `BbDrawerClose` render a real `<button>`

**Issue:** [#507](https://github.com/blazorblueprintui/ui/issues/507)

Both were a bare `<div @onclick>` with no `tabindex`, no `role` and no keyboard handler. That
worked when the child happened to be focusable, and silently did not when it was not — plain text,
an icon or a `<span>` gave a trigger that no keyboard user could reach and no screen reader
announced as a control.

Both now render `<button type="button">` by default, with `AsChild="true"` for the case where the
child is already a control. This is the shape `BbDialogTrigger`, `BbSheetTrigger` and
`BbPopoverTrigger` have always had; Drawer was the only one that did not follow it.

### What to change

If you wrap a control, add `AsChild="true"`. Otherwise you get a `<button>` inside a `<button>`,
which is invalid HTML and which no browser renders reliably.

```razor
<!-- v3 -->
<BbDrawerTrigger>
    <BbButton Variant="ButtonVariant.Outline">Open</BbButton>
</BbDrawerTrigger>

<!-- v4 -->
<BbDrawerTrigger AsChild="true">
    <BbButton Variant="ButtonVariant.Outline">Open</BbButton>
</BbDrawerTrigger>
```

The same applies to `BbDrawerClose`.

**Plain content needs no change**, and now works by keyboard for the first time:

```razor
<BbDrawerTrigger>Open the drawer</BbDrawerTrigger>
```

### How to find every usage

Search for `<BbDrawerTrigger>` and `<BbDrawerClose>` with no `AsChild`, and check what the next
element is. If it is a component or a `<button>`, add `AsChild="true"`.

---

## 2. `BbTooltipTrigger.AsChild` now defaults to `false`

**Issue:** [#428](https://github.com/blazorblueprintui/ui/issues/428), deferred half of
[#425](https://github.com/blazorblueprintui/ui/issues/425)

In v3 this defaulted to `true`. In that mode the trigger renders no element and no handlers — it
only cascades a `TriggerContext`, and the child is required to consume it. `BbButton` does;
`LucideIcon` and plain markup do not. So the most natural thing to write silently did nothing:

```razor
<BbTooltipTrigger>
    <LucideIcon Name="house" />
</BbTooltipTrigger>
```

A default where the obvious usage fails, and the working usage requires knowing about an opt-out,
is the wrong way round. v3 moved trigger defaults to `true` across the family; for tooltip
specifically that turned out to be the wrong call, and v4 reverses it.

### What to change

Add `AsChild="true"` wherever the child consumes the trigger context itself:

```razor
<!-- v3 -->
<BbTooltipTrigger>
    <BbButton Variant="ButtonVariant.Outline">Hover me</BbButton>
</BbTooltipTrigger>

<!-- v4 -->
<BbTooltipTrigger AsChild="true">
    <BbButton Variant="ButtonVariant.Outline">Hover me</BbButton>
</BbTooltipTrigger>
```

A bare icon, plain text or arbitrary markup needs no change, and now works.

### What actually changes in the DOM

With `AsChild="false"` the trigger wraps its content in a `<span>` carrying the handlers. That span
uses `display: contents`, so **it generates no layout box** — spacing, flex and inline-block
behaviour are unaffected. The practical impact is on anything that walks the DOM: `:first-child`
selectors, `querySelector` paths, and test hooks that assume the child is a direct descendant.

### Only the styled wrapper changed

`BlazorBlueprint.Primitives.Tooltip.BbTooltipTrigger` already defaulted to `false`. The divergence
was in the Components layer, and this removes it.

### The warning is still there

An unconsumed trigger context is reported through `ILogger` in the Development environment, added
in [#425](https://github.com/blazorblueprintui/ui/issues/425). With the default flipped you should
see it far less often, but it still catches an `AsChild="true"` around a child that ignores the
context.

---

## Other `AsChild` triggers

[#428](https://github.com/blazorblueprintui/ui/issues/428) asked whether the same default question
applies to popover, dialog, sheet, dropdown menu, hover card and collapsible. It does not, and they
are **unchanged**:

Those triggers render a `<button>` in their non-`AsChild` branch and are opened by a click, which
any focusable child already delivers by bubbling. Tooltip is different because it opens on **hover
and focus**, which do not bubble usefully — so a trigger that renders nothing genuinely has nothing
listening. The asymmetry is in the interaction, not in the API.

---

## 3. Every utility in `blazorblueprint.css` is prefixed `bb:`

**Issue:** [#501](https://github.com/blazorblueprintui/ui/issues/501), fixing
[#496](https://github.com/blazorblueprintui/ui/issues/496)

`blazorblueprint.css` is a prebuilt Tailwind stylesheet. In v3 its utilities were unprefixed and
written into Tailwind's `utilities` cascade layer — the same layer your own Tailwind build writes
into. Layer names are global to the document, so two builds emitting the same class name into the
same layer were resolved by which `<link>` came second, not by Tailwind's sort order. Your
`sm:grid-cols-2 md:grid-cols-4` collapsed when Blazor Blueprint loaded after your stylesheet, and
the library's own `hidden sm:flex` collapsed when it loaded before. No load order fixed both.

In v4 every utility the library emits is prefixed — `.bb\:flex`, `.bb\:sm\:hidden`,
`.bb\:data-\[state\=open\]\:bg-accent` — and lives in a `bb-utilities` layer of its own. The two
builds can no longer produce the same class name, so nothing depends on load order any more.

### If you run your own Tailwind build

**Nothing changes in your markup.** `Class="p-6"` is still `p-6`; the library strips its prefix
when it merges, so your unprefixed class still replaces the library's for the same property:

```razor
<BbCard Class="p-6">        @* renders class="… p-6 …", with the library's bb:p-4 removed *@
```

Two things to check:

- **Remove any `@source` that points at the Blazor Blueprint package or sources.** It was never
  needed, and under v4 it finds `bb:flex`, does not recognise the `bb` variant, and emits nothing.
- **Load order no longer matters** for utilities. Keep your theme before `blazorblueprint.css` as
  before; put your Tailwind output wherever you like.

### If you do not run Tailwind

Some projects wrote Tailwind classes in their own markup and relied on `blazorblueprint.css`
happening to contain them. That was never supported — the file only ever held the classes the
components use — and in v4 those classes are all prefixed, so a bare `class="flex gap-4"` in your
page matches nothing.

You have two options:

- **Add a Tailwind build to your project.** This is the supported path for using utilities in your
  own markup. The [standalone CLI](https://tailwindcss.com/blog/standalone-cli) needs no Node.js.
- **Use the prefixed classes directly:** `class="bb:flex bb:gap-4"`. They work anywhere on the
  page, not only inside components. The set is whatever the components happen to use and may
  change between versions, so treat this as a stopgap rather than an API.

### Renamed: `shimmer` and `scroll-fade-x`

These two utilities were safelisted so consumers could apply them by name. They are now
`bb:shimmer` and `bb:scroll-fade-x`:

```razor
<!-- v3 -->
<BbMarkerContent Class="shimmer">…</BbMarkerContent>

<!-- v4 -->
<BbMarkerContent Class="bb:shimmer">…</BbMarkerContent>
```

### Internal class names are not an API

If you have CSS, JavaScript or tests that select the library's internal elements by utility class
(`.flex-col`, `.group\/row`, `.hidden`), those selectors now need the prefix. Prefer the `data-slot`
and other data attributes the components render; those are stable.

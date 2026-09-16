# Component expansion implementation

Work branch: `feat/component-expansion-v4`, targeting `v4`.

The user authorized proceeding in the order recommended in `docs/research/2026-09-16-neoui-parity-review.md`. This is implementation work in progress, not a claim of complete NeoUI Pro parity. Pro's private implementation remains unavailable for inspection.

## Current status

The open-source expansion adds 37 styled components, including 13 primary controls with focused demo pages. The sidebar and component homepage share 129 distinct destinations; composition helpers are documented on their owning pages. The sidebar uses its standard width, and badges distinguish new components from additions to existing APIs. The Mobile Shop composition is a recipe.

Scheduler demos cover all 24 hours and initially scroll to 8 AM. The editor preserves drafts on backdrop clicks, offers simple repeat choices and weekly weekday selection, and retains application-supplied advanced rules until explicitly replaced. TreeSelect uses Space for expansion and Enter for selection or checkbox toggling.

Final validation: full solution build with zero warnings/errors, 320 .NET tests, 70 JavaScript tests, 130 component-page documentation/render checks, and 10 static CSS checks passed. Targeted browser checks cover navigation, segmented inputs, TreeSelect, the shopping recipe, scheduler recurrence and Day/Week scrolling. Broader interaction coverage and the AI/SaaS roadmap below remain follow-up work.

The sections below record intermediate implementation and verification milestones; their earlier navigation layouts and test counts are superseded by this status and the final follow-up sections.

## Core additions implemented; runtime verification underway

- Implemented segmented DateInput/TimeInput: culture order, null/draft handling, bounds, keyboard increment/navigation, EditContext validation, optional calendar/time picker. Live demos and API docs added.
- Implemented Sortable keyboard pickup/move/drop/cancel, announcements, focus return, CanMove. Implemented source-side CanDrop with serialized cross-list callbacks and a reusable handle. Connected-list keyboard transfer uses Control+Left/Right, enforces the same drop permissions, and restores focus. Custom inert drag-overlay templates and cleanup are implemented.
- Implemented menu radio groups/items, context-menu checkbox items, nested submenus across DropdownMenu/ContextMenu/Menubar, root selection closure, sibling coordination and RTL keyboard behavior. Menus use shared primitives. Live demos and API docs added.
- Implemented aggregate theme presets (seven profiles), density/font/surface/menu settings, scoped appearance with portal inheritance, persistence and ThemeSwitcher controls. Scoped themes inherit document palettes/dark mode. Named fonts require app-supplied font assets.
- Implemented DataView key-based selection, grouping, list virtualization and live demo. Group headings stay mounted; rows virtualize separately within each group. Provider fetching remains page/batch based. Grid mode does not virtualize. Local ShowPagination=false now exposes all filtered data. Preserve user-selected layout across parent renders.
- Implemented MultiSelect FooterContent and dispatcher-safe CloseAsync; custom footer Escape behavior uses existing popover dismissal.
- Implemented FilterBuilder preset buttons/dropdown, searchable field picker and per-field value-editor templates. Presets clone into drafts and respect explicit Apply mode and depth/condition limits. Live demo added.

- Mobile controls implemented with live demos: AppBar, BottomNav, NotificationBadge, QuantityStepper, SectionHeader; drawer snap points (pointer + keyboard + optional dismiss); Select bottom-sheet; explicit MobileToolbar and FilterContent in DataView. All demo hosts build. Final interaction checks remain.
- Motion/polish implemented with live examples, snippets and API entries: motion presets and custom keyframes; height, selection and incoming navigation transitions; reduced-motion and prerender behavior; Carousel autoplay/pause/drag/measured bounds/indicators; semantic Badge, Required/Scrollable ToggleGroup, Separator line styles, and Sidebar pill navigation.

## Remaining roadmap

- Complete broader browser interaction checks for the Core additions. Verify consumer class overrides in a rendered scoped theme. API baselines have been reviewed and accepted, and changelog/catalog entries added.
- AI: provider-backed streaming/cancel/retry chat, message actions, attachments, tool/results/sources; inline generation acceptance; AI command integration.
- SaaS: authentication/session, tenant isolation, membership/roles, billing lifecycle, auditing, working open-source workflows reusing suitable blueprint patterns.
- Versioned blueprint catalog and repeatable coverage of existing Bb strengths.

User explicitly directed **everything to remain open source**, including AI components, adapters, SaaS starter and workflow examples. Do not put new work in the nested Pro repository. Recommended default stack: .NET 10 Blazor, ASP.NET Core Identity (cookie sessions, confirmation/reset, MFA/passkeys), EF Core 10 + Npgsql/PostgreSQL, tenant IDs + membership policies/query and write isolation, Stripe Checkout/Portal + verified/deduplicated webhooks, Microsoft.Extensions.AI IChatClient with a configurable initial OpenAI adapter. UI packages remain independent of backend adapters. No external account or paid resource creation authorized or required.

## Verification so far

- Full solution build succeeds with zero warnings/errors across Server, WebAssembly and Auto hosts.
- Full .NET suite: **296 tests passed**, including the reviewed API snapshots.
- JavaScript suite: **69 tests passed**, including keyboard Sortable transfer, drawer snapping, motion and Carousel behavior.
- Fixed a DataView demo parameter (`Filterable`, not `Searchable`) that caused HTTP 500. Mobile toolbar changes now invalidate the component's render cache.
- Browser verification is now available through native Chrome. Cascader renders correctly in light and dark mode; opening its nested popup and selecting Tools updates the bound value and returns focus.
- A user screenshot exposed missing demo styles. Browser requests accepting gzip received HTTP 200 with an empty body, while uncompressed CSS was present. Tailwind previously ran after static asset discovery/fingerprinting/compression. It now runs before `ResolveProjectStaticWebAssets`, including output absent during initial clean-checkout evaluation.
- Deleted only ignored generated CSS, then built the entire solution once to verify fresh generation. `scripts/check-static-css.py` passes **10 checks**: current metadata and gzip contents across all three hosts, plus identity/gzip delivery of both library and demo stylesheets from the running Server host.
- Stop demo processes before rebuilding and restart afterward: live processes retain asset maps. Current Server demo listens at `http://localhost:7172`. Verification logs: `/tmp/bb-css-final-build.log`, `/tmp/bb-parity-all-tests-8.log`, `/tmp/bb-parity-js-tests-9.log`. All 29 routes for the changed component demos return HTTP 200 with their expected page title and heading. These are render checks, not a substitute for browser interaction coverage.

These additions do not yet establish complete NeoUI Pro parity. The AI and SaaS implementation tracks remain outstanding; Pro's private feature inventory remains unverified.

## Navigation inventory and TreeSelect follow-up

- Renamed the working branch to `feat/component-expansion-v4`; use feature-based branch names without competitor names.
- README now lists every new styled component: 37 total, comprising 13 primary controls and 24 composition helpers. Six shared headless menu components are documented separately as supporting primitives.
- The 37 new Razor components span four new demo pages (Date Input, Time Input, Mobile Controls and Motion) and additions to existing demos. The homepage catalog and search retain the individual component inventory and v4 badges. Sidebar navigation links once per actual demo page: helper anchors are excluded, route aliases are consolidated, and related dedicated pages (including Data Grid Editing, Hierarchy and Styling) nest beneath their parent. Labels wrap within a 22rem sidebar. Families and children are alphabetized.
- TreeSelect Space toggles branch expansion without changing selection; Enter selects and closes in single mode, or checks/unchecks while remaining open in multiple mode. Standalone TreeView behavior and LeafOnly restrictions are preserved. Permanent demo descriptions, keyboard guidance and snippets are updated.
- Added five JavaScript keyboard regressions and three .NET integration cases, including unchanged selection during expansion and reselecting the current value. Full suites pass: 296 .NET and 69 JavaScript tests; all three hosts build with zero warnings/errors. Ten CSS delivery checks pass.
- Verified TreeSelect behavior in Chrome, including both directions of Space expansion, Enter selection/focus return, and repeated Enter checking/unchecking a cascading branch without closing. An existing browser retained the older relative JavaScript import; revised the bundle/dependency URLs and verified the fix in the regular browser after an ordinary reload.
- Current validation logs: `/tmp/bb-component-expansion-build.log`, `/tmp/bb-tree-keyboard-dotnet-tests.log`, `/tmp/bb-tree-keyboard-js-tests.log`. Local demo is running at `http://localhost:7172`.

## Scheduler and navigation follow-up

- Scheduler editor backdrop clicks retain the draft. The date heading is centered, with Previous / Today / Next grouped together. Today uses the configured display time zone and preserves the view and week-start preference.
- The sidebar distinguishes demo pages from component implementation pieces. Mobile Controls and Motion each have one link; helper-only links remain available in the component catalog and search.
- Sidebar demo examples are grouped with the other collapse/navigation examples; pill and selection-indicator APIs are in the final API section, after accessibility guidance. The selection indicator now has its own live example and matching snippet.
- Verified the rendered sidebar: 120 links map to 120 distinct Razor demo pages; no helper anchors or duplicate aliases. Families and their children are alphabetized. All four sidebar helper anchors still resolve after the page reorganization.
- Browser verification confirms Today navigation, centered heading and grouped controls, backdrop draft retention, wider nested navigation, and the reordered Sidebar examples. All three demo hosts build with zero warnings/errors; 302 .NET tests and all 10 CSS delivery checks pass. Latest logs: `/tmp/bb-scheduler-menu-build.log` and `/tmp/bb-scheduler-menu-tests.log`.

## Documentation and version badge follow-up

- Restored the original main navigation order; only Components families and their children remain alphabetized.
- Reviewed all 121 routed component demo pages. Each now has standard ARIA/keyboard documentation followed by one API Reference section; examples precede both. Missing references and stale API types were corrected, and SortableHandle is grouped with the rest of Sortable's APIs.
- Compared version labels against `components/v3.17.0`. Existing components such as Sortable no longer have a new-component sidebar badge. New API groups and individual new parameters, methods and enum values carry `v4` badges in their references.
- Time Input uses BbSelect for AM/PM inline and in the clock popup, with correctly displayed initial values and synchronized changes. The permanent demo, snippet and accessibility guidance match the implementation.
- Latest validation: full solution build with zero warnings/errors, 305 .NET tests, 121 rendered-page documentation checks and 10 CSS checks passed. Chrome confirmed the Date Input and Sortable documentation layouts and Time Input keyboard/popup behavior.
- Details: [Component demo documentation review](2026-09-16-demo-documentation-review.md). Latest logs: `/tmp/bb-demo-audit-build.log`, `/tmp/bb-demo-audit-tests.log` and `/tmp/bb-demo-audit-http.log`.

## Focused demos, recipes and recurrence follow-up

- The sidebar, component homepage and command search now use the same grouped list of actual demo pages. The rendered homepage and sidebar have 129 matching destinations in the same order, each backed by a distinct demo page. Composition helpers remain documented within their owning pages rather than appearing as duplicate cards or menu items.
- Split App Bar, Bottom Navigation, Notification Badge, Quantity Stepper and Section Header into focused demos. Split the six motion-related controls into individual demos grouped under Motion. Each page has matching snippets, accessibility guidance and API references.
- Moved the combined example to Recipes → Mobile Shop (`/recipes/mobile-shop`); `/components/mobile-controls` remains a compatibility alias. Home now browses two products, Cart manages quantities/removal/totals, and Account has sample preferences and demo-order history.
- Daily, monthly and yearly recurrence no longer asks for an interval. Weekly recurrence has labelled weekday checkboxes, restores saved selections and requires at least one day. The optional count limit remains. Advanced application-supplied rules stay unchanged unless the user explicitly chooses a replacement; the editor no longer exposes raw rule syntax.
- Restored the standard sidebar width (16rem desktop / 18rem mobile from the component defaults) and right-aligned version badges using the flexible label area. Long labels can still wrap.
- Revised the core JavaScript entry URL and dependency URLs after Safari reproduced a stale sidebar module error. An ordinary reload now restores interactive controls without manually clearing browser storage.
- Validation: full solution build has zero warnings/errors; all 315 .NET tests pass, including 10 recurrence cases added in this follow-up. All 130 routed component demos pass the documentation/render checks, the homepage and sidebar share 129 distinct destinations, and all 10 CSS checks pass.
- Browser checks confirm distinct shopping screens and cart totals/order flow; weekly weekday selection/save/reopening; simple daily, monthly and yearly repeat controls; grouped homepage cards; the standard sidebar width; and right-aligned version badges. Final Server log is clear of circuit failures. Current demo: `http://localhost:7172`.
- Logs: `/tmp/bb-recipes-scheduler-build.log`, `/tmp/bb-recipes-scheduler-tests.log`, `/tmp/bb-recipes-scheduler-demo.log`.

## Scheduler full-day scrolling

- The short demo ranges (8 AM–2 PM, or 8 AM–noon) omitted the rest of the day and fit inside the scroll viewport. All scheduler examples now render midnight through midnight with `StartHour=0`, `EndHour=24` and `InitialScrollHour=8`.
- Added the optional `InitialScrollHour` parameter, with timezone/DST-aware initial positioning and range clamping. It is applied on the first interactive render only, preserving later user scrolling. Reviewed and accepted the single new API snapshot entry.
- The time-slot viewport is capped at the smaller of 700px and 65vh, keeps day headings sticky, contains scrolling and is keyboard focusable. Updated live demos, snippets, accessibility guidance, API reference and changelog.
- Browser verification reached 12:00 AM and 11:30 PM by scrolling inside the grid, with the surrounding page stationary. Verified the initial 8 AM position and vertical scrolling in both Day and Week views.
- Full solution build passes with zero warnings/errors; all 320 .NET tests pass. New regression cases cover initial offsets on ordinary and DST days, bounds clamping, and preservation of manual scrolling during pointer interaction. Logs: `/tmp/bb-scheduler-scroll-build.log`, `/tmp/bb-scheduler-scroll-tests.log`, `/tmp/bb-scheduler-scroll-all-js-tests.log`.

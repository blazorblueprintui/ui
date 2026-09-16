**NeoUI Core / NeoUI Pro versus Blazor Blueprint — 16 September 2026**

Bb has substantial enterprise functionality beyond the inspected NeoUI Core tree. The remaining verified Core gaps concentrate on segmented inputs, mobile controls, reusable motion, richer theme configuration, and features inside existing components. Complete parity with NeoUI Pro cannot be certified from its public marketing page: its private implementation and a versioned feature catalog were not available for inspection.

**Scope and evidence**

- NeoUI source: `jimmyps/blazor-shadcn-ui`, `main`, commit `c828d5ca442c936fa101f00210240454606faca3`.
- Bb source: local `v4`, commit `f3703a36`, including the September 16 Scheduler, hierarchy-picker, grid-editing and upload work. These findings describe this checkout, not just the stable package.
- Bb Pro: local checkout at `d125ca0`; this is not a claim about a newer remote branch or published offering.
- NuGet was checked through its live package index and registration APIs. Bb's stable Components version was 3.17.0; the index also contained 4.0.0-beta.5. Do not present all v4 source capabilities as stable-package features.
- Method: source inventory, public APIs, selected implementations, repository history, package metadata, and official sites. No comparative runtime, accessibility or performance tests were performed. “Present” establishes implementation evidence, not complete behavioral equivalence.
- Names were normalized by function: e.g. NeoUI `Grid` contains `DataGrid`, and Bb's `Toggle` folder contains `BbToggleGroup`. Razor-file totals are not component totals.

NeoUI's [NOTICE](https://github.com/jimmyps/blazor-shadcn-ui/blob/main/NOTICE) explicitly acknowledges material derived from Blazor Blueprint. The repository not being marked as a GitHub fork does not negate that attribution.

**1. Latest updates**

| Item | Verified version/date | Evidence |
|---|---|---|
| Latest default-branch commit | August 24, 2026, 16:39:21 UTC; `c828d5ca44` | [Commit](https://github.com/jimmyps/blazor-shadcn-ui/commit/c828d5ca442c936fa101f00210240454606faca3) |
| Latest stable NeoUI.Blazor package | 4.1.38; August 24, 2026, 16:40:21 UTC — August 25, 00:40 Singapore time | [Package](https://www.nuget.org/packages/NeoUI.Blazor/4.1.38), [registration metadata](https://api.nuget.org/v3/registration5-semver1/neoui.blazor/4.1.38.json) |
| Latest Primitives package | 4.0.13; August 17, 2026 UTC | [Package](https://www.nuget.org/packages/NeoUI.Blazor.Primitives/4.0.13) |
| Latest Lucide package | 3.1.0; August 21, 2026 UTC | [Package](https://www.nuget.org/packages/NeoUI.Icons.Lucide/3.1.0) |
| Latest GitHub Release entry | components/v4.1.0; May 17, 2026 | [Release](https://github.com/jimmyps/blazor-shadcn-ui/releases/tag/components/v4.1.0) |
| NeoUI Pro | No dated public Pro changelog or release history established | [Public product page](https://neoui.pro/) |

Recent changes, summarized from the [changelog](https://github.com/jimmyps/blazor-shadcn-ui/blob/main/CHANGELOG.md):

- August 24: MultiSelect footer slot and programmatic close; follow-up fixes for focus and dispatcher use.
- August 17–21: DataTable root-level tree pagination, refresh/page-reset support, loading-state fixes and fewer redundant server fetches.
- August: Lucide expanded to 1,834 icons with old-name aliases; CSS shorthand merging and headerless Card padding fixed.
- August 8: infinite-loading controls stop when all items are loaded and prevent concurrent fetch bursts.

Earlier major additions were segmented DateInput/TimeInput and nested filter editing in [v4.1.0](https://github.com/jimmyps/blazor-shadcn-ui/releases/tag/components/v4.1.0), expanded theming in [v4.0.0](https://github.com/jimmyps/blazor-shadcn-ui/releases/tag/components/v4.0.0), and mobile controls in [v3.9.0](https://github.com/jimmyps/blazor-shadcn-ui/releases/tag/components/v3.9.0). The release page alone misses later package updates.

**2. Additional Core components and helpers**

The following are in NeoUI's public source. They are not Pro-only. “Missing” below means no equivalent reusable component/API was found in the inspected Bb core; developers can still compose some of these experiences manually.

| NeoUI component/family | Bb assessment | Capability to add |
|---|---|---|
| DateInput | Missing segmented control; DatePicker has a text-entry path | Separate culture-aware date segments with keyboard increment/navigation and optional calendar. [Source](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/DateInput) |
| TimeInput | Missing segmented control; TimePicker exists | Hour/minute/second/AM-PM segment editing, 12/24-hour behavior and optional picker. [Source](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/TimeInput) |
| AppBar | Missing dedicated component | Mobile top bar with title, back action and trailing actions. [Source](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/AppBar) |
| BottomNav / BottomNavItem | Missing dedicated components | Mobile bottom tabs, active state, badges and safe-area handling. [Source](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/BottomNav) |
| NotificationBadge | Partial: BbBadge supports a dot | Count/dot overlay around arbitrary content, maximum-count display and zero visibility. [Source](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/NotificationBadge) |
| QuantityStepper | Partial: NumericInput exists | Compact quantity control with a remove action at the minimum. [Source](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/QuantityStepper) |
| SectionHeader | Missing convenience component | Section title plus action/link and optional separator. Low implementation priority. [Source](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/SectionHeader) |
| Motion + presets | Missing general animation API; Bb already animates overlays | Reusable fade/slide/scale/spring/scroll/stagger effects, custom keyframes and reduced-motion handling. [Source](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/Motion) |
| PageTransition | Missing dedicated component | Navigation-triggered transitions with prerender awareness. [Source](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/PageTransition) |
| ScreenTransition | Missing dedicated component | Tab, push and pop transitions within an application shell. [Source](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/ScreenTransition) |
| HeightAnimation | Missing general helper | Animate arbitrary content-height changes, including command search results. [Source](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/HeightAnimation) |
| SelectionIndicator | Missing reusable component | Moving active/hover indicator across tabs, navigation or custom selections. [Source](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/SelectionIndicator) |
| RenderStateProvider | Missing reusable equivalent | Cascaded interactive/hydration/navigation state for animation coordination. Infrastructure helper, not another visible widget. [Source](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/RenderState) |

`AppProvider` also exists in NeoUI, primarily for theme propagation and initialization. Bb needs equivalent outcomes only if its theming is expanded; matching the wrapper's name or architecture is unnecessary.

**3. Function gaps within components Bb already has**

| Area | Verified difference and Bb implication |
|---|---|
| Theme system | NeoUI has seven style variants, font presets, radius presets, menu colors/accents, and an aggregate preset model. Bb exposes base/primary color, dark mode and radius, but not this broader public configuration model. Extend Bb's design-system API. [NeoUI styles](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/StyleVariant), [Bb theme service](../../src/BlazorBlueprint.Components/Components/Theme/ThemeService.cs) |
| Drawer | NeoUI exposes `SnapPoints` and bindable `SnapIndex`, with drag-to-snap behavior. Bb's Drawer has no matching snap-point API. [NeoUI](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/Drawer), [Bb](../../src/BlazorBlueprint.Components/Components/Drawer) |
| Select | NeoUI can present selection as a bottom sheet. Bb's Select lacks that presentation mode. [NeoUI](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/Select), [Bb](../../src/BlazorBlueprint.Components/Components/Select) |
| Sidebar | NeoUI adds pill collapse mode, pill navigation/layout helpers and an animated selection indicator. Bb's normal responsive/collapsible Sidebar exists, but these APIs do not. [NeoUI docs](https://demos.neoui.io/components/sidebar), [Bb](../../src/BlazorBlueprint.Components/Components/Sidebar) |
| DataView | NeoUI adds built-in single/multiple selection, grouped rendering, list virtualization and a mobile sort/filter sheet. Bb has list/grid layouts, async data providers and infinite loading, but lacks these matching APIs. Infinite loading is not DOM virtualization. NeoUI's virtualized mode is list-only. [NeoUI](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/DataView), [Bb](../../src/BlazorBlueprint.Components/Components/DataView) |
| Sortable | Both support reordering and cross-list transfer. NeoUI additionally implements keyboard pickup/move/drop/cancel, composable handle/overlay helpers and a drop-permission callback. Bb's inspected SortableJS wrapper lacks corresponding keyboard handling and typed drop-validation API. [NeoUI behavior](https://github.com/jimmyps/blazor-shadcn-ui/blob/main/src/NeoUI.Blazor.Primitives/wwwroot/js/primitives/sortable.js), [Bb behavior](../../src/BlazorBlueprint.Primitives/wwwroot/js/primitives/sortable.js) |
| Menus | NeoUI exposes submenu and radio-item families for DropdownMenu, ContextMenu and Menubar; ContextMenu also has checkbox items. Bb lacks the corresponding families in the inspected styled/headless implementation. Bb already has DropdownMenu and Menubar checkbox items. [NeoUI menu source](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/DropdownMenu), [Bb components](../../src/BlazorBlueprint.Components/Components) |
| Carousel | NeoUI has autoplay, drag control, indicator positions, slides-per-view, gap and slide-change callback. Bb's basic compositional carousel exposes orientation/loop and navigation, but lacks these public options. [NeoUI](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/Carousel), [Bb](../../src/BlazorBlueprint.Components/Components/Carousel/BbCarousel.razor) |
| FilterBuilder | Nested AND/OR groups and query expressions already exist in Bb. Remaining NeoUI conveniences include filter-preset tabs/dropdown, a searchable field picker and configurable chip editors. Do not count nested filters as absent in Bb. [NeoUI](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/Filter), [Bb](../../src/BlazorBlueprint.Components/Components/FilterBuilder) |
| MultiSelect | NeoUI exposes an option-list footer slot and `CloseAsync()`. Neither public facility was found on BbMultiSelect. [NeoUI](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/MultiSelect), [Bb](../../src/BlazorBlueprint.Components/Components/MultiSelect) |
| Small controls | NeoUI has semantic/soft Badge variants, a BadgeIcon helper, ToggleGroup `Required`/`Scrollable`, and Separator line styles. Bb has the base controls; these are incremental API/design additions. [Badge](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/Badge), [ToggleGroup](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/ToggleGroup), [Separator](https://github.com/jimmyps/blazor-shadcn-ui/tree/main/src/NeoUI.Blazor/Components/Separator) |

NeoUI's DataTable is richer than BbDataTable in isolation: hierarchy, virtualization, server data, pinning, resizing and reordering are exposed there. Bb largely covers these capabilities in **BbDataGrid**, so they should not all become new library-wide gaps. Choose whether to keep the simple-table/advanced-grid distinction rather than duplicating every capability across both controls.

NeoUI's public DataGrid wraps AG Grid and includes transaction/live-row APIs and content auto-sizing. Bb's native grid has a different API and implementation. Incremental add/update/remove operations, auto-sizing and feature combinations deserve a focused follow-up comparison. The mere presence of an AG Grid option does not establish implemented, tested parity for that feature.

NeoUI documents a free Blazor-managed server paging path, alongside an optional AG Grid Enterprise server-side path. Its own [server-side demo source](https://github.com/jimmyps/blazor-shadcn-ui/blob/main/demo/NeoUI.Demo.Shared/Pages/Components/Grid/ServerSide.razor) identifies the latter as requiring an AG Grid license. Do not conflate that third-party tier with NeoUI Pro or assume a Pro purchase includes it.

**4. Existing coverage and Bb advantages**

Bb already covers the common forms, overlays, navigation, tables, trees, charts, rich text/Markdown editors, uploads, dynamic forms, filtering, sorting and localization categories. Similar names do not establish complete API parity, but they are not missing component families.

Specific false gaps avoided during this audit:

- NeoUI's `ComposedChart` maps to [BbChart](../../src/BlazorBlueprint.Components/Components/Chart/Types/BbChart.cs), which already mixes Bar, Line, Area and other series. “12 chart types” versus “11” is misleading here.
- `LinkButton` maps to [BbButton.Href](../../src/BlazorBlueprint.Components/Components/Button/BbButton.razor.cs).
- `DialogHost` maps functionally to Bb's dialog provider/service system.
- `ToggleGroup` is present inside Bb's `Toggle` directory; `InputOtp` versus `InputOTP` is spelling, not a gap.
- Several NeoUI chart, empty-state and toast subcomponents are composition conveniences rather than whole missing capabilities.

Bb components not found as equivalent dedicated families in the inspected NeoUI Core source include **Scheduler, EventCalendar, DashboardGrid, Dock, TreeSelect, Cascader, DateTimePicker, FormWizard, CheckboxGroup, CopyText, Attachment, Bubble, Message and Marker**, plus the form-field wrapper suite and Font Awesome package. Bb's new grid cell/batch editing and upload progress/cancel/retry lifecycle are already present in the reviewed v4 source. See the [current README](../../README.md) and [changelog](../../CHANGELOG.md).

These are source-backed advantages over Core. They are not evidence that NeoUI's inaccessible Pro implementation lacks the same functionality.

**5. Open source versus Pro**

NeoUI Core contains the component library, including the advanced grid, charts, dynamic forms, filter builder, theme system, mobile controls and motion components discussed above. Its [Pro page](https://neoui.pro/) advertises an additional commercial layer: 150+ blocks; streaming chat, inline AI generation and an AI command palette; a SaaS starter with authentication, billing, multi-tenancy and team permissions; enterprise workflows such as audit/activity UI; and priority support. These are advertised capabilities, not inspected implementations.

No public component-by-component Pro API catalog or dated changelog was established. The [public blocks page](https://neoui.io/blocks) displayed 92 blocks, which does not independently verify the Pro claim. An exact Pro-only component inventory remains unverified.

Bb's local Pro checkout contains **61 blueprint Razor files excluding `_Imports.razor`**, spanning Settings, Dashboard, Content, Data/Tables, E-Commerce, Marketing, Authentication, Charts and Navigation. This is an on-disk count, not 61 tested/released production blocks, nor Bb's total public-plus-private block count. The 61 files include 10 Settings, 10 Dashboard, 8 Content, 7 Data/Tables, 7 E-Commerce, 6 Marketing, 5 Authentication, 5 Charts and 3 Navigation files.

The local `pro/src` and `pro/templates` trees contain no implementation files. Consequently, the [Pro README](../../pro/README.md) claims about AI and a complete SaaS starter are not established by this checkout. Existing login, billing and team-management blueprints provide UI/callbacks; they do not by themselves implement identity, subscription processing, tenant isolation or authorization. Bb's core Message/Bubble/Attachment controls are useful building blocks for an AI UI, but not a streaming chat product.

**6. Recommended parity roadmap**

These priorities are planning judgments based on the inspected gaps and the user's goal of meeting or exceeding Pro's capabilities.

| Priority | Work | Completion evidence |
|---|---|---|
| P1 | Segmented DateInput/TimeInput | Keyboard, culture, null/clear, bounds, EditContext validation and Server/WASM demos. Improve on NeoUI's documented hidden-input integration with first-class Bb form integration. |
| P1 | Keyboard Sortable and full menu composition | Keyboard-only reorder/cancel, focus restoration, announcements, nested menus and radio/checkbox semantics. |
| P1 | Theme expansion | Coherent style/density/font/menu presets, scoped defaults and persistence, including overlays and validation that user class overrides still win. |
| P1 | DataView and practical control gaps | Selection, grouping and virtualization; MultiSelect footer/close; mobile selection presentation; reusable filter presets. |
| P2 | Mobile control suite | AppBar, BottomNav, count badges, QuantityStepper, drawer snap points and mobile data toolbars working together in a sample shell. |
| P2 | Motion and polish | Reusable motion/height/selection/navigation transitions; reduced-motion and prerender behavior; carousel controls and small semantic variants. |
| Parallel product track | AI application components | Streaming/cancel/retry chat, message actions, attachments, tool/results and source display; inline generate/rewrite with accept/reject; AI command integration. Backed by a working provider adapter and demos. |
| Parallel product track | SaaS starter and complete workflows | Working auth/session flows, organization/tenant isolation, memberships/roles, billing lifecycle and audited actions. Reuse the existing blueprints as the UI layer. |
| P2 | Versioned blueprint catalog | Inventory public/private blocks together; show actual implemented flows, dependencies and release status. Compare workflow coverage before counts. |
| Continuous | Preserve and prove Bb's advantages | Exercise Scheduler recurrence/time zones, grid validation/editing, docking, uploads and hierarchical selection across supported hosts; publish limitations and repeatable checks. |

To claim complete Pro parity later, obtain a versioned Pro feature inventory and evaluate its concrete workflows against these acceptance criteria. Until then, the defensible claims are: verified Core gaps, verified Bb Core advantages, and explicitly unverified Pro capabilities. No library implementation was changed as part of this review.

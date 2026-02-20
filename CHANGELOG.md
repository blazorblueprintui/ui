# Changelog

All notable changes to Blazor Blueprint are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

---

## 2026-02-20

### Added

- Component animations powered by **tw-animate-css** — smooth enter/exit animations for 17 overlay and interactive components using `data-state` attribute toggling and Tailwind utility classes (`animate-in`, `animate-out`, `fade-in-0`, `zoom-in-95`, `slide-in-from-*`, etc.).
- `bb-no-animate` global CSS class — add to `<html>` to disable all animations and transitions site-wide, including portaled content.
- `prefers-reduced-motion` accessibility support — all animations and transitions are automatically suppressed when the user has "Reduce motion" enabled in their OS settings.
- `ForceMount` parameter on `BbPopoverContent` and `BbDropdownMenuContent` (Components layer, default: `true`) — keeps content mounted in the DOM when closed so CSS exit animations can play before removal.
- `ForceMount` parameter on Primitives `BbPopoverContent` and `BbDropdownMenuContent` (default: `false`) — opt-in mount persistence at the headless layer.
- `ForceMount` parameter on `BbCollapsibleContent` (Components layer, default: `true`) — exposed for developer control; previously hardcoded.
- `data-state="open"` attribute on `BbToast` — enables CSS animation classes on toast notifications.
- `AutoDismissAfter` parameter on `BbAlert` — timed alert dismissal with `PeriodicTimer`-based countdown (100ms ticks) and proper `IAsyncDisposable` cleanup.
- `PauseOnHover` parameter on `BbAlert` (default: `true`) — pauses auto-dismiss countdown on mouse hover.
- `ShowCountdown` parameter on `BbAlert` (default: `true` when `AutoDismissAfter` is set) — visual countdown progress bar.
- `Actions` RenderFragment on `BbAlert` — slot for inline action buttons below alert content.
- Animation toggle component in demo site header — toggles `bb-no-animate` on `<html>` with localStorage persistence.
- Animations guide page (`/guides/animations`) with live interactive examples for all 5 animation types: fade+zoom, slide from edge, fade+zoom+directional slide, slide in (toast), and expand/collapse.
- "Guides" collapsible sidebar section in demo site navigation.

### Fixed

- Dialog and AlertDialog animations simplified to fade+zoom only — removed slide animation that caused visual inconsistency with centered overlays.

---

## 2026-02-19

### Fixed

- `BbTooltipTrigger` and `BbHoverCardTrigger` now merge parent `TriggerContext` (e.g., from `BbDialogTrigger`) so click, ARIA, and keyboard behavior is preserved alongside hover/focus behavior (#131).
- `DisposeAsync` across all components now catches `JSDisconnectedException`, `TaskCanceledException`, and `ObjectDisposedException` independently per JS module, so one disposal failure doesn't prevent cleanup of others (#77).
- File inputs in `BbInputGroupInput` and `BbInputField` no longer attempt value binding, preventing `InvalidStateError` on file selection (#132).

### Internal

- Icon generation scripts (Lucide, Heroicons, Feather) centralized into `tools/icon-generation/` with a unified `build-icons.sh` runner. Added Node.js Lucide generator to replace the PowerShell script.
- Primitives and Components README files updated with correct `Bb`-prefixed component names, 45+ missing components, setup steps, and services documentation.

---

## 2026-02-17

### Added

- `ToastVariant.Success`, `ToastVariant.Info`, `ToastVariant.Warning` — 3 new semantic toast variants with themed colors matching the Alert component's CSS custom properties (`alert-success`, `alert-info`, `alert-warning`).
- `ToastSize` enum (`Default`, `Compact`) — compact size reduces padding (`p-3` vs `p-6`) and font sizes (`text-xs` vs `text-sm`) for dialog-friendly toasts.
- Auto variant icons on `BbToast` — semantic variants automatically display LucideIcons: Success → `circle-check`, Info → `info`, Warning → `triangle-alert`, Destructive → `circle-x`. Default variant has no icon. Opt-out via `ToastData.ShowIcon = false`.
- `PauseOnHover` parameter on `BbToastProvider` (default: `true`) — pauses the auto-dismiss timer when the mouse hovers over a toast, resumes with correct remaining duration on mouse leave.
- `ToastData.Position` (`ToastPosition?`) — per-toast position override. Toasts with a custom position render in a separate positioned container from the main toast stack.
- `ToastService.Info(description, title?)` and `ToastService.Warning(description, title?)` factory methods.
- Auto-generated `id` attributes for all input components — `BbInput`, `BbTextarea`, `BbNumericInput<TValue>`, `BbCurrencyInput`, `BbMaskedInput`, `BbInputField<TValue>`, `BbInputGroupInput`, `BbInputGroupTextarea`, `BbCheckbox`, `BbInputOTP`, and `BbNativeSelect<TValue>` now always render with an `id` attribute. A stable 8-character Guid-based ID (e.g., `input-a3f7c2d4`) is generated on first render when no `Id` parameter is provided. User-supplied `Id` always takes precedence.
- `Id` parameter on `BbInputOTP` and `BbNativeSelect<TValue>` — these components previously had no `Id` parameter.
- `FillContainer` parameter on `BbScrollArea` (default: `false`) — when true, automatically fills remaining vertical space in a flex column parent container using CSS `flex-1 min-h-0`, excluding sibling elements. Eliminates manual `Height`/`MaxHeight` calculations for header/content/footer layouts.
- `ResetScrollOnNavigation` parameter on `BbSidebarInset` (default: `true`) — automatically scrolls the inset content area to the top when the route changes via `NavigationManager.LocationChanged`. Improves SPA navigation UX.

### Changed

- `ToastService.Success()` now uses `ToastVariant.Success` (green accent + icon) instead of `ToastVariant.Default` (neutral). To restore old neutral behavior, use `ToastService.Show()` directly.
- `BbToastProvider` changed from `IDisposable` to `IAsyncDisposable`. Auto-dismiss timer now tracks elapsed time per toast (supporting pause-on-hover) instead of comparing `CreatedAt` timestamps.
- `BbSidebarInset` changed from inline `@code` to code-behind with `IAsyncDisposable` for `NavigationManager.LocationChanged` subscription and JS interop support.

### Performance

- `BbDropdownMenuContent`, `BbContextMenuContent`, and `BbMenubarContent` keyboard navigation moved from C# `@onkeydown` to JavaScript — arrow keys, Home, End, Enter, and Space are handled entirely in JS with zero C# interop, eliminating unnecessary round-trips and re-renders during menu navigation. Only Escape key and Menubar ArrowLeft/ArrowRight trigger C# callbacks.

### Fixed

- `autoUpdate` callback in `positioning.js` now guards against stale DOM elements — FloatingUI's `autoUpdate` listeners (scroll, resize, intersection) could fire after Blazor removes elements from the DOM but before cleanup runs, causing non-fatal "Reference element is not ready" errors. The callback now silently bails out via `isElementReady` check.

### Internal

- Created `InputValidationBehavior` internal composition class — centralizes EditContext validation logic (`IsInvalid`, `EffectiveAriaInvalid`, `EffectiveName`, `NotifyFieldChanged`, `Update`) shared by all input components. Refactored 8 components to use it: `BbInput`, `BbTextarea`, `BbCurrencyInput`, `BbNumericInput<TValue>`, `BbMaskedInput`, `BbInputGroupInput`, `BbInputGroupTextarea`, `BbInputField<TValue>`. No public API changes.
- Created `menu-keyboard.js` shared module in `BlazorBlueprint.Primitives` — handles keyboard events for all menu containers (`[role="menu"]`). Supports `"vertical"` mode (DropdownMenu, ContextMenu) and `"menubar"` mode (Menubar). Uses DOM-based item navigation via `[role="menuitem"]`/`[role="menuitemcheckbox"]`/`[role="menuitemradio"]` queries and `aria-disabled` filtering. Removed C# item registration/tracking (`RegisterMenuItem`, `UnregisterMenuItem`, `FocusedIndex`, `SetFocusedIndex`) from `BbContextMenu`, `BbMenubarMenu`, and `BbDropdownMenuContent`.

---

## 2026-02-16

### Changed

> **Breaking Change** — the following change requires updates to all component references.

- **BREAKING:** All Razor components across both `BlazorBlueprint.Components` (~300+ components) and `BlazorBlueprint.Primitives` (~65 components) now use a `Bb` prefix — e.g., `<Button>` → `<BbButton>`, `<Dialog>` → `<BbDialog>`, `<Select>` → `<BbSelect>`. This follows the convention used by other third-party libraries such as MudBlazor (`Mud` prefix) and Radzen (`Radzen` prefix) to prevent naming collisions with standard HTML elements, user-defined components, and third-party libraries. Non-component types (enums, context classes, services, helper classes, interfaces, event args) are unchanged. See [V3-MIGRATION-GUIDE.md](V3-MIGRATION-GUIDE.md#component-bb-prefix) for full migration instructions.
- **BREAKING:** `IPortalService` redesigned with Two-Layer Portal Architecture — portals are now split into `PortalCategory.Container` (Dialog, Sheet, AlertDialog) and `PortalCategory.Overlay` (Popover, Select, Dropdown, Tooltip, HoverCard) categories. `RegisterPortal(id, content)` replaced by `RegisterPortal(id, content, category)`; `GetPortals()` replaced by `GetPortals(PortalCategory)` returning insertion-ordered results; `OnPortalsChanged` replaced by `OnPortalsCategoryChanged`. Each category has its own host, so opening a tooltip no longer causes Dialog/Sheet portals to re-render. See [V3-MIGRATION-GUIDE.md](V3-MIGRATION-GUIDE.md#iportalservice-two-layer-portal-architecture) for full migration instructions.

### Added

- `ForceMount` parameter on `BbFloatingPortal` (default: `true`) — portal content stays registered in the DOM when closed. Content is hidden via CSS and repositioned on re-open. Eliminates re-mount overhead on every open/close cycle.
- `data-state` attribute (`"open"` / `"closed"`) on `BbFloatingPortal` content div — enables CSS animations based on open/closed state (e.g., `data-[state=closed]:animate-out`).
- `PortalCategory` enum (`Overlay`, `Container`) in `BlazorBlueprint.Primitives` — categorizes portal content for scoped rendering.
- `BbCategoryPortalHost` component — core category-scoped portal host that subscribes only to its own category's change events.
- `BbContainerPortalHost` component — thin wrapper rendering only Container portals (Dialog, Sheet, AlertDialog).
- `BbOverlayPortalHost` component — thin wrapper rendering only Overlay portals (Popover, Select, Dropdown, Tooltip, HoverCard).
- Insertion order tracking in `PortalService` via `Interlocked.Increment` — ensures parent portals render before children regardless of registration timing.
- `escape-keydown.js` module in `BlazorBlueprint.Primitives` — element-scoped Escape key detection shared by Dialog, Sheet, and Drawer.

### Removed

- Hidden cache-seeding render hack (`<div style="display:none">`) from Primitives `BbSelectContent` — items are always mounted in the portal via ForceMount.
- `displayTextCache` dictionary, `GetCachedDisplayText()`, and `ClearItems()` from `SelectContext` — no longer needed since items stay registered across open/close cycles with ForceMount.

### Performance

- `BbFloatingPortal` defaults to keeping content mounted (`ForceMount=true`). All floating overlay consumers (Select, Popover, Tooltip, HoverCard, DropdownMenu) benefit from zero re-mount overhead on open/close.
- `BbDialogContent`, `BbSheetContent`, and `BbDrawerContent` keyboard handling moved from C# `@onkeydown` to JavaScript — only Escape key triggers C# interop, eliminating unnecessary round-trips and re-renders for every other keystroke typed inside dialogs, sheets, and drawers.

---

## 2026-02-15

### Changed

> **Breaking Changes** — the following changes require updates to existing chart code.

- **BREAKING:** Chart components completely rebuilt on [Apache ECharts](https://echarts.apache.org/), replacing [Blazor-ApexCharts](https://github.com/apexcharts/Blazor-ApexCharts). The new implementation uses a Recharts-inspired declarative composition API where charts are composed from child components (`<XAxis>`, `<YAxis>`, `<ChartTooltip>`, `<Line>`, `<Bar>`, etc.) instead of configuring variants and `<ApexPointSeries>`. Every chart in consumer applications must be rewritten — there is no compatibility layer. See [V3-MIGRATION-GUIDE.md](V3-MIGRATION-GUIDE.md#chart-components-apexcharts--echarts) for full migration instructions.
- **BREAKING:** `ChartBase<TItem>` replaced by non-generic `ChartBase` — charts no longer require a `TItem` type parameter. Data is passed via `object? Data` and properties are referenced by name via `DataKey` strings using reflection-based extraction.
- **BREAKING:** `RadialChart` renamed to `RadialBarChart` — all references must be updated.
- **BREAKING:** All chart variant enums removed — `AreaChartVariant`, `BarChartVariant`, `LineChartVariant`, `PieChartVariant`, `RadarChartVariant`, `RadialChartVariant`. Chart behavior is now configured via child components and series parameters (e.g., `Curve="CurveType.Smooth"`, `Stacked="true"`, `Horizontal="true"`).
- **BREAKING:** `@using ApexCharts` no longer needed — remove from `_Imports.razor` and all `.razor` files.
- **BREAKING:** `Blazor-ApexCharts` NuGet dependency removed from `BlazorBlueprint.Components.csproj`.

### Added
- Declarative chart composition API with child component registration via `CascadingValue` and `IChartComponent.ApplyTo(EChartsOption)` pattern.
- `ChartBase.razor` / `ChartBase.razor.cs` — base chart component with JS interop lifecycle (`OnAfterRenderAsync`), child registration, `BuildOption()` orchestration, and `IAsyncDisposable` cleanup.
- `DataExtractor` — reflection-based data extraction with `ConcurrentDictionary` property accessor cache for extracting values from any `IEnumerable` via string property names.
- `IChartComponent`, `IChartSeries`, `IFillComponent` interfaces for the component registration pattern.
- Composable chart components: `XAxis` (category axis with `DataKey`, `Type`, `ShowGrid`, `LabelInside`, `BoundaryGap`, etc.), `YAxis` (value axis with `ShowGrid`, `GridColor`, `GridType`, etc.), `Grid` (chart area margins with `ContainLabel`), `ChartTooltip` (hover tooltip with `Indicator`, `BackgroundColor`, `BorderColor`, `TextColor`), `ChartLegend` (series legend with `Position`, `TextColor`, `ItemGap`).
- Series components inheriting from `SeriesBase` (shared `DataKey`, `Name`, `Color`, `Stacked`, `StackGroup`): `Line` (`Curve`, `StrokeWidth`, `ShowDots`, `DotSize`, `Dashed`), `Bar` (`BorderRadius`, `BarWidth`, `ShowLabel`, `LabelPosition`, `LabelFormatter`, `LabelColor`, `LabelFontSize`, `FillKey`), `Area` (`Curve`, `StrokeWidth`, `FillOpacity`, `ShowDots`), `Pie` (`NameKey`, `OuterRadius`, `InnerRadius`, `PaddingAngle`, `ShowLabels`, `LabelPosition`, `LabelFormatter`, `ShowLabelLine`, `ActiveIndex`), `Radar` (`FillOpacity`, `ShowDots`, `DotSize`, `StrokeWidth`), `RadialBar` (`NameKey`, `RoundCap`, `ShowLabels`, `ShowBackground`).
- `CenterLabel` component — center text for donut and radial charts with `Title`, `Value`, `Text`, `FontSize`, `FontWeight`.
- Fill system: `Fill`, `LinearGradient` (`Direction`), `GradientStop` (`Offset`, `Color`, `Opacity`) — nestable inside any series for custom gradient fills.
- Chart type components: `LineChart`, `BarChart` (`Horizontal`), `AreaChart`, `PieChart`, `RadarChart` (`Shape`, `IndicatorKey`, `Indicators`, `MaxValue`, `ShowAxisLines`, `ShowGridLines`, `GridFill`), `RadialBarChart` (`StartAngle`, `EndAngle`, `InnerRadius`, `OuterRadius`).
- ECharts model classes for JSON serialization: `EChartsOption`, `EChartsAxis`, `EChartsSeries`, `EChartsGrid`, `EChartsTooltip`, `EChartsLegend`, `EChartsGradient`, `EChartsRadar`.
- New enums: `AxisType` (Category, Value, Time, Log), `CurveType` (Linear, Smooth, Step, StepBefore, StepAfter), `GradientDirection` (Vertical, Horizontal), `LabelPosition` (Top, Bottom, Left, Right, Inside, InsideLeft, InsideRight, InsideTop, InsideBottom, Outside, Center, Middle), `LineStyleType` (Solid, Dashed, Dotted), `RadarShape` (Polygon, Circle).
- `echarts-renderer.js` — ECharts instance lifecycle management (initialize, update, dispose) with `ResizeObserver` per chart, SVG renderer, and theme change callback integration.
- `chart-theme.js` — recursive CSS `var()` resolution via `getComputedStyle()`, OKLCH-to-hex conversion using canvas 2D context, and `MutationObserver` on `<html>` for automatic dark/light theme detection and chart re-rendering.
- Apache ECharts v5.5.x bundled as static web asset (`wwwroot/lib/echarts/echarts.min.js`) — no CDN or external NuGet dependency.
- All 6 chart demo pages rewritten with the new composition API, including interactive variants with code examples.
- Code example `.txt` files for all chart variants (55 files across AreaChart, BarChart, LineChart, PieChart, RadarChart, RadialChart directories).
- Chart README updated to document the new ECharts-based architecture, composition pattern, and complete parameter reference.

### Removed
- `Blazor-ApexCharts` (v4.0.0) NuGet dependency.
- `AreaChartVariant`, `BarChartVariant`, `LineChartVariant`, `PieChartVariant`, `RadarChartVariant`, `RadialChartVariant` enum types.
- `ChartBase.cs` (generic `ChartBase<TItem>` abstract class).
- `RadialChart.razor` / `RadialChart.razor.cs` (renamed to `RadialBarChart`).
- Old chart type `.razor` / `.razor.cs` files (replaced by `.cs`-only implementations inheriting from `ChartBase`).
- ApexCharts tooltip and legend CSS overrides from `blazorblueprint-input.css`.

---

## 2026-02-14

### Fixed
- `DialogContent` and `SheetContent` used `Task.Run` for JS cleanup, bypassing the Blazor synchronization context — replaced with `InvokeAsync()` to stay on the sync context and prevent race conditions.
- `DropdownMenuContent`, `PopoverContent`, `SidebarProvider`, and `ResponsiveNavProvider` `async void` event handlers lacked exception guards — wrapped in `try/catch (ObjectDisposedException)` to prevent process crashes during disposal.
- `DialogContent`, `SheetContent`, `DropdownMenuContent`, `PopoverContent`, `FocusManager`, and `PositioningService` `DisposeAsync` methods missing `JSDisconnectedException` guards — added `try/catch` around all JS module disposal calls for Blazor Server circuit disconnect safety.
- `FocusManager.GetModuleAsync()` was not thread-safe — added `SemaphoreSlim` with double-check locking to prevent concurrent JS module imports (e.g., nested dialogs).
- `Slider`, `ResizablePanelGroup`, and `ContextMenu` `[JSInvokable]` methods lacked `_disposed` guards — added `_disposed` flag and early-return checks to prevent JS callbacks after component disposal.
- Broad `catch (Exception)` blocks in `DialogContent`, `SheetContent`, `DrawerContent`, `DropdownMenuContent`, `PopoverContent`, `Slider`, and `ResizablePanelGroup` replaced with specific `catch (JSDisconnectedException)` + `catch (InvalidOperationException)` for proper SSR/prerendering handling.
- `SidebarProvider` cookie deserialization failed with `InvokeAsync<bool?>` when JS returned `null` — changed to `InvokeAsync<JsonElement>` with `ValueKind` switch for robust handling of boolean and null JSON values.
- `FloatingPortal` `Console.WriteLine` calls replaced with structured `ILogger` using `LoggerMessage.Define` static fields — diagnostic messages now appear in the standard ASP.NET Core logging pipeline.
- `DialogOverlay` and `SheetOverlay` had unused `IJSRuntime` injection and empty `IAsyncDisposable` implementation — removed dead code.
- `ContextMenu` had unused `@inject IJSRuntime JS` — removed.

### Changed
- Drawer demo direction buttons changed from text labels to arrow icon buttons (`arrow-up`, `arrow-right`, `arrow-down`, `arrow-left`).

---

## 2026-02-13

### Added
- `Shortcut` parameter on `MenubarItem` — when set (e.g., `Shortcut="Ctrl+Shift+T"`), automatically registers a global keyboard shortcut via `IKeyboardShortcutService` that invokes the item's `OnClick` callback. The shortcut is registered on first render and disposed automatically. Eliminates manual `@inject IKeyboardShortcutService`, field management, `OnAfterRenderAsync` registration, and `DisposeAsync` boilerplate per shortcut.
- `Field` cascading `IsInvalid` to child inputs — `Field` now wraps `@ChildContent` with a named `CascadingValue` (`FieldIsInvalid`) that propagates its `IsInvalid` state to child input components. Supported inputs: `Input`, `Textarea`, `InputGroupInput`, `InputGroupTextarea`, `NumericInput`, `CurrencyInput`, `MaskedInput`. Eliminates the need to manually set `AriaInvalid` or `Class="border-destructive"` on each input inside an invalid `Field`. Priority chain: explicit `AriaInvalid` parameter (user override) > cascaded `FieldIsInvalid` from parent `Field` > `IsInvalid` from EditContext validation.
- `NavItemDefinition` model — data class with `Text`, `Href`, `Children`, `Description`, and `Match` properties for defining navigation items once and rendering them in both desktop and mobile contexts.
- `ResponsiveNavItems` component — renders both a desktop `NavigationMenu` (hidden on mobile via CSS) and a mobile `ResponsiveNavContent` (Sheet) from a single `NavItemDefinition` collection. Parameters: `Items`, `DesktopClass`, `MobileClass`, `MobileHeader`, `MobileFooter`, `Side`. Eliminates duplicate navigation link definitions between desktop and mobile views.

### Fixed
- `MenubarContent` changed from conditional rendering (`@if (Menu?.IsOpen == true)`) to always rendering `@ChildContent` with `display:none` when closed — ensures `MenubarItem` components exist in the DOM from page load so their `Shortcut` registrations fire in `OnAfterRenderAsync`. Previously, items inside conditionally rendered content never had `firstRender` trigger because they were not in the DOM until the menu opened.

---

## 2026-02-12

### Changed

> **Breaking Changes** — the following changes may require updates to existing code.

- **BREAKING:** All `BlazorBlueprint.Components.*` sub-namespaces (77 total) consolidated into the root `BlazorBlueprint.Components` namespace — consumers now need only `@using BlazorBlueprint.Components` instead of 10+ sub-namespace imports. Remove all `@using BlazorBlueprint.Components.Button`, `@using BlazorBlueprint.Components.Dialog`, etc. lines from `_Imports.razor` and individual `.razor` files. Replace `using BlazorBlueprint.Components.Extensions` with `using BlazorBlueprint.Components` in `.cs` files.
- **BREAKING:** 8 consumer-facing types moved from `BlazorBlueprint.Primitives.*` sub-namespaces to the root `BlazorBlueprint.Primitives` namespace — `AccordionType`, `SheetSide`, `SortDirection`, `PaginationState`, `PopoverSide`, `PopoverAlign`, `PopoverPlacement`, `PositioningStrategy`. Replace `@using BlazorBlueprint.Primitives.Services` with `@using BlazorBlueprint.Primitives` in consumer `_Imports.razor`.

### Added
- `Loading`, `LoadingText`, and `LoadingTemplate` parameters on `Button` — when `Loading` is true, the button is disabled and shows a spinner (or custom `LoadingTemplate`). `LoadingText` replaces the button label during loading. Eliminates the manual `@if` block + spinner icon + `disabled` + `StateHasChanged()` boilerplate that every loading button previously required.
- `Dismissible` and `OnDismiss` parameters on `Alert` — when `Dismissible` is true, a close button (X icon) is rendered in the top-right corner. `OnDismiss` callback is invoked on click. Eliminates ~6 lines of manual close button markup per dismissible alert.
- `ShowCharacterCount` parameter on `Textarea` — when true and `MaxLength` is set, displays a `{current}/{max}` counter below the textarea. When `MaxLength` is not set, displays only the current count. Eliminates the manual `@(text?.Length ?? 0)/200` expression pattern.
- `TrackClass` and `ThumbClass` parameters on `Slider` — additional CSS classes applied to the slider track and thumb via `ClassNames.cn()`. Replaces the `[&_.bg-primary]:bg-emerald-500 [&_div[class*='border-primary']]:border-emerald-500` CSS hack pattern for custom slider styling.
- `InputClass` parameter on `InputOTP` — additional CSS classes applied to each individual OTP input box via `ClassNames.cn()`. Replaces the `Class="[&_input]:border-primary/50 [&_input]:bg-primary/5"` arbitrary variant hack for styling OTP slots.
- `ShowLabel` and `LabelFormat` parameters on `Progress` — when `ShowLabel` is true, displays a percentage label next to the progress bar. `LabelFormat` (default: `"{0}%"`) allows custom format strings where `{0}` is replaced with the integer percentage. Eliminates the manual `<div>` label wrapper above every progress bar.
- `Width` and `Height` parameters on `Skeleton` — explicit sizing via inline styles (e.g., `Width="200px"` or `Width="50%"`). Provides an alternative to Tailwind arbitrary values like `Class="h-[200px] w-[250px]"` for dynamic or non-standard sizes.
- `MinHeight` parameter on `MarkdownEditor` — sets the minimum height for the textarea and preview areas via inline style (e.g., `MinHeight="200px"`). Replaces the `class="min-h-[200px]"` CSS hack that every usage previously required. When not set, defaults to `150px`.
- `DefaultValue` parameter on `ColorPicker` — initial color value for uncontrolled usage. When set and `Value` is not externally bound, the picker starts at the specified color. Eliminates the need for wrapper components or explicit `@bind-Value` just to set an initial color.
- `Label` parameter on `RadioGroupItem` — renders a wrapper `<div>` with an associated `<label>` element, automatic `for`/`id` wiring, and `peer-disabled` styling. Eliminates ~4 lines of boilerplate per radio option. When `Label` is null, rendering is unchanged (backwards compatible). Auto-generates an `Id` if none is provided.
- `ShowArrow` parameter on `TooltipContent` — when true, renders a CSS arrow (rotated square) pointing toward the trigger element, positioned automatically based on the tooltip's `Side` (Top, Bottom, Left, Right). Eliminates the `overflow-visible after:content-[''] after:absolute ...` CSS pseudo-element hack that the arrow pattern previously required.
- `Icon` parameter on `DropdownMenuItem`, `ContextMenuItem`, and `CommandItem` — renders an icon before the item content with automatic `mr-2 h-4 w-4` sizing. Eliminates the `class="mr-2 h-4 w-4"` repeated on every menu item icon.
- `AutoSeparator` parameter on `BreadcrumbList` — when true, automatically inserts `BreadcrumbSeparator` between each `BreadcrumbItem`. `SeparatorContent` allows a custom separator (e.g., `/` instead of the default chevron). Eliminates manual `<BreadcrumbSeparator />` placement between every item.
- `Title`, `Time`, and `Description` shorthand parameters on `TimelineItem` — when `Title` is set and `ChildContent` is null, auto-renders the full sub-component tree (`TimelineContent` > `TimelineHeader` > `TimelineTitle` + `TimelineDescription`). Eliminates 7 nested sub-components per item for simple entries.
- `CommandDialog` component — wraps `Dialog` + `Command` with a `Shortcut` parameter for automatic keyboard shortcut registration/disposal via `IKeyboardShortcutService`. Supports `@bind-Open`, `OnValueChange`, and `CloseOnSelect` (default: true). Eliminates ~20 lines of manual dialog/command/shortcut boilerplate.
- `TotalPages` and `CurrentPage` parameters on `Pagination` — when `TotalPages` is set and `ChildContent` is null, auto-renders the standard prev/pages/next layout with ellipsis for large page counts. Supports `@bind-CurrentPage` two-way binding. Eliminates ~17 lines of nested `PaginationContent`/`PaginationItem`/`PaginationLink` markup.
- `TriggerClass` parameter on `Menubar` — additional CSS classes cascaded to all `MenubarTrigger` components within the menubar. Eliminates the need to repeat the same `Class` on every trigger.
- Runtime warning when `<PortalHost />` is missing from layout — `PortalService` now logs a structured `LogLevel.Warning` on the first `RegisterPortal()` call if no `PortalHost` has been registered, telling the developer exactly what to add and where. Previously, portal-based components (Dialog, Select, Popover, Combobox, DropdownMenu, Tooltip, Sheet) silently failed with no diagnostic.
- `IPortalService.HasHost` property — indicates whether a `PortalHost` is currently registered.
- `IPortalService.RegisterHost()` / `UnregisterHost()` methods — called by `PortalHost` during initialization and disposal to track host presence.
- `AddBlazorBlueprintComponents()` extension method on `IServiceCollection` — single-call DI registration for all BlazorBlueprint services (Primitives + Components). Calls `AddBlazorBlueprintPrimitives()` internally and registers `ToastService` and `DialogService`.
- `SelectOption<TValue>` record — value/label pair for use with the `Options` parameter on `Select`, `Combobox`, and `MultiSelect`. Factory method `SelectOption.FromValue<TValue>()` derives display text from `ToString()`.
- `SelectOptionExtensions` with `GetText<TValue>()` and `GetTexts<TValue>()` extension methods — look up display text from a `SelectOption<TValue>` collection, eliminating the need for per-collection helper functions.
- `Options` parameter on `Select<TValue>` — auto-generates trigger, value display, and dropdown content from a collection of `SelectOption<TValue>`. Takes priority over `ChildContent`.
- `Options` parameter on `Combobox<TValue>` — auto-generates searchable dropdown items from a collection of `SelectOption<TValue>`. Takes priority over `ChildContent`.
- `Options` parameter on `MultiSelect<TValue>` — auto-generates checkbox items from a collection of `SelectOption<TValue>`. Takes priority over `ChildContent`.
- `ComboboxItem<TValue>` component — compositional mode child for `Combobox` that supports rich item rendering (icons, avatars) with `Value`, `Text`, `Disabled`, and `ChildContent` parameters.
- `MultiSelectItem<TValue>` component — compositional mode child for `MultiSelect` that supports rich item rendering with `Value`, `Text`, `Disabled`, and `ChildContent` parameters.
- `Placeholder` parameter on `Select<TValue>` — displayed when no value is selected in data-binding mode (`Options`).
- `Options` parameter on `FormFieldSelect<TValue>` — same data-binding mode as `Select`, auto-generates `SelectItem` children from the collection.
- `Options` parameter on `FormFieldCombobox<TValue>` — same data-binding mode as `Combobox`, auto-generates items from the collection.
- `ChildContent` parameter on `FormFieldCombobox<TValue>` — supports compositional mode with `ComboboxItem` children.
- `Options` parameter on `FormFieldMultiSelect<TValue>` — same data-binding mode as `MultiSelect`, auto-generates items from the collection.
- `ChildContent` parameter on `FormFieldMultiSelect<TValue>` — supports compositional mode with `MultiSelectItem` children.
- `SelectValue.ChildContent` now accepts `RenderFragment<string>` — the template receives the display text as `context`, enabling rich trigger display (e.g. icons, avatars alongside text).
- `ISelectDisplayContext` interface — non-generic display-only contract (`DisplayText` + `OnStateChanged`) for components that need selected text without knowing `TValue`.
- `PositioningStrategy` enum (`Absolute`, `Fixed`) — replaces string-based positioning strategy.
- `SwitchThumb` sub-component for primitive `Switch` — automatically syncs `data-state` attribute from the parent `Switch` via `CascadingParameter`. Eliminates the manual `data-state="@(isChecked ? "checked" : "unchecked")"` that consumers had to set on their thumb `<span>` element. The styled `Switch` component now uses `<SwitchThumb>` internally instead of a manual span.
- `CheckboxIndicator` sub-component for primitive `Checkbox` — automatically renders the appropriate check (polyline) or indeterminate (line) SVG icon based on the parent `Checkbox` state via `CascadingParameter`. Supports custom content via `ChildContent`, and configurable `Size` (default: 14) and `StrokeWidth` (default: 3) for the default icons. Eliminates the manual `@if (isChecked) { <svg>...</svg> }` pattern. The styled `Checkbox` component now uses `<CheckboxIndicator />` internally.
- `ItemClass` parameter on primitive `DropdownMenu` — default CSS classes cascaded to all `DropdownMenuItem` children via `DropdownMenuContext`. Items merge `ItemClass` with their own `class` attribute. Eliminates repeating the same styling classes on every menu item. The styled `DropdownMenu` component passes this through.
- `ItemClass` parameter on primitive `RadioGroup<TValue>` — default CSS classes cascaded to all `RadioGroupItem` children via `RadioGroupContext`. Items merge `ItemClass` with their own `class` attribute. Same pattern as `DropdownMenu`.
- `ItemClass` parameter on primitive `Select<TValue>` — default CSS classes cascaded to all `SelectItem` children via `SelectContext`. Items merge `ItemClass` with their own `class` attribute. Same pattern as `DropdownMenu` and `RadioGroup`.
- `Href` and `Target` parameters on primitive `DropdownMenuItem` — when `Href` is set, the item renders as an `<a>` element instead of a `<div>`, with proper `rel="noopener noreferrer"` when `Target="_blank"`. When disabled, `href` is nullified and `aria-disabled` is set. The styled `DropdownMenuItem` component passes these through.
- `CascadingTypeParameter` attribute added to primitive `Select<TValue>` — child components (`SelectTrigger`, `SelectContent`, `SelectItem`, etc.) can now infer `TValue` from the parent `Select`, eliminating the need to repeat `TValue="string"` on every child component.
- Persistent display text cache added to `SelectContext<TValue>` — the context now maintains a `Dictionary<TValue, string>` cache that survives open/close cycles. Previously, display text was lost when the dropdown closed (items unregister on close). `SelectContent` now performs a one-time hidden render on first mount to seed the cache, so pre-selected values show display text in the trigger without requiring the dropdown to be opened first.
- Primitive `SelectItem.TextValue` renamed to `SelectItem.Text` — aligns naming with the Components layer `SelectItem.Text` and the `Combobox`/`MultiSelect` `Text` pattern.
- `ShowDot` and `DotClass` parameters on `Avatar` — when `ShowDot` is true, renders a small circular indicator positioned bottom-right. Dot appearance is controlled via `DotClass` (e.g., `DotClass="bg-green-500"` for online, `"bg-yellow-500"` for away). Dot size scales with the `Size` parameter. Eliminates the manual wrapper div + absolute-positioned span workaround.
- `AvatarGroup` component — renders child `Avatar` components with overlapping negative spacing (`-space-x-3`) and automatically adds a `border-2 border-background` ring to each avatar via cascading context. Eliminates the manual `-space-x-4` + `ring-2 ring-background` pattern.
- `ShowDot`, `DotPosition`, and `DotClass` parameters on `Badge` — when `ShowDot` is true, renders a notification dot on the badge. `DotPosition` (`BadgeDotPosition` enum: `TopRight`, `TopLeft`, `BottomRight`, `BottomLeft`) controls placement. `DotClass` overrides the dot color (defaults to `bg-primary`). Eliminates ~9 manual utility classes for badge notification dots.
- `BadgeDotPosition` enum — `TopRight`, `TopLeft`, `BottomRight`, `BottomLeft` values for controlling notification dot placement on `Badge`.
- `DrawerItem` component — styled action button for use inside `DrawerContent`. Supports `Icon` render fragment, `ChildContent`, `Disabled`, `OnClick`, and `CloseOnClick` (default: true, auto-closes the parent Drawer). Renders as a `<button>` with hover/focus/disabled styling. Eliminates the raw `<button>` with manual CSS pattern for mobile action sheets.
- `CheckboxGroup<TValue>` component — parent component managing a collection of checkboxes with `@bind-Values` two-way binding. Supports `ShowSelectAll` for an automatic select-all checkbox with indeterminate state calculation, `SelectAllLabel` for custom label text, and `Disabled` to disable the entire group. Uses `CheckboxGroupContext<TValue>` for item registration and state management.
- `CheckboxGroupItem<TValue>` component — individual checkbox within a `CheckboxGroup`. Supports `Value` (required), `Label`, `Disabled` (per-item), `ChildContent` for custom rendering, and auto-generated `Id` for label wiring. Items register/unregister with the parent via cascading context. Implements `IDisposable` for cleanup.
- `DialogService` — programmatic confirm dialog service registered as scoped via `AddBlazorBlueprintComponents()`. `DialogService.Confirm(title, description?, options?)` returns `Task<bool>` — `true` if the user clicks confirm, `false` if cancelled. Eliminates ~15 lines of declarative `AlertDialog` markup, multiple `bool` state variables, and manual event handlers per confirmation flow.
- `DialogProvider` component — renders programmatic confirm dialogs from `DialogService`. Place once at the root layout (e.g., `MainLayout.razor`) alongside `<ToastProvider />` and `<PortalHost />`. Renders AlertDialog-equivalent overlay and content with `role="alertdialog"` and `aria-modal="true"`. No click-to-dismiss on overlay — user must click Cancel or Confirm.
- `ConfirmDialogOptions` class — customization options for `DialogService.Confirm()`: `ConfirmText` (default: "Continue"), `CancelText` (default: "Cancel"), `Destructive` (default: false — when true, the confirm button uses `ButtonVariant.Destructive`).
- `ConfirmDialogData` class — internal data model pairing a dialog request with its `TaskCompletionSource<bool>` for async resolution. Properties: `Id`, `Title`, `Description`, `Options`.

### Changed

> **Breaking Changes** — the following changes may require updates to existing code.

- **BREAKING:** `Combobox<TItem>` renamed to `Combobox<TValue>` — the type parameter now represents the selected value type directly, not an item model type. The `Items`, `ValueSelector`, and `DisplaySelector` parameters have been removed in favor of `Options` (data-driven) or `ChildContent` with `ComboboxItem` children (compositional). `Value` changed from `string?` to `TValue?`.
- **BREAKING:** `MultiSelect<TItem>` renamed to `MultiSelect<TValue>` — the type parameter now represents the selected value type directly. The `Items`, `ValueSelector`, and `DisplaySelector` parameters have been removed in favor of `Options` (data-driven) or `ChildContent` with `MultiSelectItem` children (compositional). `Values` changed from `IEnumerable<string>?` to `IEnumerable<TValue>?`.
- **BREAKING:** `FormFieldCombobox<TItem>` renamed to `FormFieldCombobox<TValue>` — mirrors the `Combobox` API changes. `Items`, `ValueSelector`, and `DisplaySelector` removed; use `Options` or `ChildContent` instead.
- **BREAKING:** `FormFieldMultiSelect<TItem>` renamed to `FormFieldMultiSelect<TValue>` — mirrors the `MultiSelect` API changes. `Items`, `ValueSelector`, and `DisplaySelector` removed; use `Options` or `ChildContent` instead.
- **BREAKING:** `PositioningOptions.Strategy` changed from `string` to `PositioningStrategy` enum — code passing `Strategy="fixed"` must use `Strategy="PositioningStrategy.Fixed"`.
- **BREAKING:** `SelectValue.ChildContent` type changed from `RenderFragment` to `RenderFragment<string>` — existing `ChildContent` usage that doesn't accept a `context` parameter will fail to compile.
- Checkbox visual proportions updated — size increased from `h-4 w-4` to `h-5 w-5`, SVG check/indeterminate icons reduced from 16px to 14px with `stroke-width` increased from 2 to 3, matching the primitive layer proportions for a bolder, more balanced appearance.
- RadioGroupItem visual style updated to filled circle design — selected state now uses `border-primary bg-primary` with a `bg-background` inner dot (matching the primitive layer), replacing the previous thin-border style with a `bg-current` dot.
- Checkbox demo "With Container Background" section replaced with "Checkbox Card" pattern using CSS `has-[[data-state=checked]]` selectors and `<label for>` click delegation instead of C# conditional classes and `@onclick` handlers.
- `DataTableColumn.Property` is no longer `[EditorRequired]` when `CellTemplate` is provided — action columns (edit/delete buttons) no longer need a dummy `Property` binding.
- `SelectItem` (both Primitives and Components layers) now renders `Text` (or `Value.ToString()`) as fallback when `ChildContent` is null — items no longer require explicit child content markup.
- `Select` primitives-layer `GetDisplayText()` gracefully handles `DisplayTextSelector` exceptions, falling back to `null` so the placeholder shows.
- `FormFieldBase` caches `Expression.Compile()` result — avoids redundant compilation on every render cycle.
- `InputField<TValue>` caches default converter instance and properly awaits `ValueChanged` and `OnErrorCleared` callbacks.
- `FloatingPortal` render timeout warning upgraded from `Console.WriteLine` to structured `ILogger<FloatingPortal>` using `LoggerMessage.Define` — messages now appear in the standard ASP.NET Core logging pipeline instead of raw console output.
- Styled `Checkbox` component now uses `<CheckboxIndicator />` internally instead of inline SVG conditional rendering — same visual output, but the indicator logic is now reusable at the primitive layer.
- Styled `Switch` component now uses `<SwitchThumb />` internally instead of a manual `<span data-state="...">` — same visual output, but the thumb automatically syncs `data-state` from the parent.

### Fixed
- Portal-based components (Dialog, Select, Popover, Combobox, DropdownMenu, Tooltip, Sheet) no longer silently fail when `<PortalHost />` is missing from the layout — a structured warning is now logged on the first portal registration attempt.
- Sidebar desktop flash on mobile — added `hidden md:flex` to prevent desktop sidebar from briefly rendering before JS breakpoint detection.
- DateRangePicker responsive CSS moved into `@layer utilities` with doubled-selector specificity to prevent cascade conflicts.
- `FormFieldSelect` now auto-generates `DisplayTextSelector` from `Options` collection, eliminating the need for a manual lookup function.
- MultiSelect search input was one keystroke behind — switched from `@oninput` to `@bind` with `@bind:event="oninput"` for immediate state synchronization.
- Combobox and MultiSelect compositional mode display text regression — item text registry is now preserved on portal unmount so trigger/badge display text remains correct while the dropdown is closed.

---

## 2026-02-11

### Added
- `Container` parameter on `DialogContent` and `SheetContent` — renders content inline instead of portaling to document body, enabling CSS containment scenarios (e.g., parent with `transform: translateZ(0)`)
- Per-panel `MinSize`/`MaxSize` constraints now enforced during Resizable drag operations (previously hardcoded to 10% minimum for all panels)

### Changed

> **Breaking Changes** — the following changes may require updates to existing code.

- **BREAKING:** Components-layer trigger and close components now default `AsChild` to `true` — `DialogTrigger`, `DialogClose`, `SheetTrigger`, `SheetClose`, `DropdownMenuTrigger`, `PopoverTrigger`, `TooltipTrigger`, `HoverCardTrigger`, `CollapsibleTrigger`. Existing usages that relied on the auto-generated wrapper `<button>` must add `AsChild="false"` explicitly.
- **BREAKING:** `ResizablePanel` flex-grow changed from `0` to `1` — panels now grow to fill available space instead of using fixed sizing.
- **BREAKING:** `SelectContext<TValue>.SetDisplayText()` method removed — display text is now assigned directly via `State.DisplayText`.

### Fixed
- File input crash: `<Input Type="InputType.File">` no longer throws `InvalidStateError` when a file is selected (#132)
- NumericInput corner clipping when `ShowButtons` is true — input and button borders now use proper rounded corners

---

## 2026-02-10

### Added
- `Strategy` and `ZIndex` parameters on `PopoverContent` — allows dropdowns to use `fixed` positioning and custom z-index to escape stacking contexts (e.g., inside Dialogs)
- Dialog demo expanded with Select, Combobox, and MultiSelect controls to demonstrate nested portal behavior
- Dialog demo added "DatePicker in Dialog" section to demonstrate popover-based components inside dialogs
- DataTable demo added "Dialog with Combobox in Cell Template" section as a nested portal regression test
- API surface snapshot tests for Components and Primitives assemblies using Verify — detects unintentional public API changes
- `run-tests.sh` script for running API surface tests locally
- API surface tests integrated into NuGet publish CI workflow
- `HorizontalEnd` and `VerticalEnd` values for `FieldOrientation` enum — enables label-after-control and label-below-control layouts for `FormFieldCheckbox`
- Indeterminate (select-all) demo sections on Checkbox and FormFieldCheckbox demo pages
- Label orientation demo sections (right, bottom) on FormFieldCheckbox demo page

### Changed
- Combobox selected item checkmark moved from left side to right side of the item text
- `PopoverContent` defaults changed from `Strategy="absolute"` / `ZIndex=50` to `Strategy="fixed"` / `ZIndex=9999` — popover-based components (DatePicker, ColorPicker, TimePicker) now render correctly above Dialog/Sheet/Drawer overlays without explicit overrides
- `PopoverTrigger` AsChild pattern now caches `TriggerContext` and only recreates when dependencies change, reducing unnecessary CascadingValue propagation
- `Button` re-registers element reference when `TriggerContext` changes (not just on first render), fixing stale references inside Dialog/Sheet
- `FormFieldCheckbox` defaults to `FieldOrientation.HorizontalEnd` (label on right), matching the standard inline checkbox pattern
- `FormFieldBase.IsInvalid` now also considers `ErrorText` parameter, not just EditContext validation errors
- `FormFieldInput.IsInvalid` delegates to `base.IsInvalid` instead of duplicating the `HasEditContextErrors` check
- `FieldLabel` adds `shrink-0` class to prevent label text from shrinking in horizontal layouts

### Fixed
- Sidebar desktop visibility — `hidden` class overriding `md:flex` due to CSS layer cascade conflicts (#116)
- SidebarRail desktop visibility — same `hidden sm:flex` pattern causing rail to be invisible on all screen sizes
- DateRangePicker responsive layout broken by CSS layer cascade — replaced Tailwind `sm:` responsive variants with un-layered CSS using `data-*` attribute selectors
- Popover-based components (DatePicker, ColorPicker, TimePicker) rendering behind Dialog/Sheet/Drawer overlays — both Primitives and Components layer defaults now use `fixed` positioning with `z-index: 9999`
- Portal render timeout warnings when floating content is nested inside other portals (e.g., Combobox inside Dialog) — `NotifyPortalRendered` now fires before processing deferred rerenders in `PortalHost`
- Combobox and MultiSelect dropdowns now render correctly above Dialog overlays using `fixed` positioning with elevated z-index
- `DialogPortal` not refreshing content on re-render — event handlers inside dialog content would update state but the UI wouldn't re-render until an unrelated browser event occurred (#118)
- `SheetPortal` using `UpdatePortalContent` instead of `RefreshPortal`, which unnecessarily replaced the RenderFragment delegate on every parameter update
- Disabled Verify auto-launching diff tool on test failure
- Resolved analyzer errors in test project
- Removed obsolete `[UsesVerify]` attribute from test classes
- Infinite render loop in `PortalHost` when opening dropdown controls (Select, Combobox, MultiSelect) inside a Dialog — replaced `_pendingRerender` flag with structural change detection to break the loop that froze WASM apps
- Combobox search input not receiving focus on second open after item selection — `Popover.OnParametersSet` now routes controlled state changes through `Context.Close()`/`Context.Open()` so `OpenChanged` fires correctly and downstream cleanup (focus reset, click-outside disposal) is triggered

---

## 2026-02-09

### Added
- Control-specific `FormField` wrapper components: `FormFieldCheckbox`, `FormFieldSwitch`, `FormFieldRadioGroup`, `FormFieldSelect`, `FormFieldCombobox`, and `FormFieldMultiSelect` — pre-configured FormField wrappers that simplify EditForm integration for non-text controls
- `FormFieldBase` base class providing shared EditForm/EditContext integration, validation, label, description, and error message rendering
- `FormFieldInput` component for typed text input with built-in form field layout (label, description, validation messages)
- Flag icon and avatar examples in `FormFieldSelect` demo
- Demo pages for each FormField wrapper in the Components sidebar section
- EditForm integration for `InputField` components with cascading `EditContext` support

### Fixed
- Build errors related to FormField component integration
- Added `string` type to `GetFriendlyTypeName` in FormField for proper type display
- DateRangePicker popover layout responsive on mobile devices (single-month view with horizontal preset scrolling)
- DateRangePicker day name headers now rotate correctly based on `FirstDayOfWeek` parameter
- DateRangePicker no longer highlights disabled dates within a selected range
- DateRangePicker selected day count excludes disabled dates
- DateRangePicker presets are ignored when all dates in the range are disabled

---

## 2026-02-08

### Added
- `CursorType` enum and `CursorExtensions.ToClass()` utility for mapping cursor types to Tailwind CSS classes
- Cursor and pointer-events conflict resolution groups in TailwindMerge
- Cursor behavior demo section on Button demo page
- `InputField<TValue>` component — generic typed input with automatic type conversion, formatting, validation, and parse error handling
- `InputConverter<TValue>` system with global, instance, and built-in default converter resolution for 15+ types
- `UpdateTiming` enum for Input and InputField — `Immediate` (default), `OnChange` (blur/Enter), and `Debounced` modes
- `DebounceInterval` parameter (default 500ms) for debounced value updates
- `UpdateTiming` and `DebounceInterval` parameters on `Textarea`, `InputGroupInput`, and `InputGroupTextarea` — matching the `Input` component pattern
- `DisableDebounce` and `DebounceInterval` parameters on `CommandInput`, `NumericInput<T>`, and `CurrencyInput` — debounce ON by default with selective immediate firing on blur/arrow keys
- Update Timing / Debounce demo sections with live value displays on Textarea, InputGroup, Command, NumericInput, and CurrencyInput demo pages
- InputField demo page with examples for all supported types and features
- `SetPosition()` and `ResetPosition()` methods on `ToastService` for runtime toast position control
- `TimelineConnectorFit` enum with `Spaced` and `Connected` options for controlling how connector lines fit between icons
- `ConnectorFit` parameter on `Timeline` component
- `ConnectorClass` parameter on `TimelineItem` for custom connector height overrides
- `Loading` parameter on `TimelineIcon` for isolated pulse animation
- Rich Content, Connector Fit, and Custom Connector Height demo sections for Timeline
- EditForm and EditContext integration for 14 input components (`Input`, `Textarea`, `InputGroupInput`, `InputGroupTextarea`, `NumericInput`, `CurrencyInput`, `MaskedInput`, `Select`, `Combobox`, `NativeSelect`, `DatePicker`, `TimePicker`, `ColorPicker`, `InputOTP`) — cascading `EditContext`, `ValueExpression`, field validation tracking, and merged `aria-invalid` state
- `Name` parameter on text input components with auto-fallback to `FieldIdentifier` for SSR form postback support
- Form Validation demo sections on 12 component demo pages showing EditForm with `DataAnnotationsValidator` and `ValidationMessage`

### Changed
- Button component now shows `cursor-pointer` by default and `disabled:cursor-not-allowed` when disabled, replacing `disabled:pointer-events-none` for better UX feedback
- Added Salary column with currency format (`C0`) to the DataTable demo Basic Table example
- Command palette keyboard shortcut changed from `Ctrl+K` to `Ctrl+I` across demo pages to avoid browser address bar conflict
- `CommandDemo` refactored to use `IKeyboardShortcutService` instead of raw JS interop for Ctrl+I shortcut registration

### Fixed
- DataTable global search now uses the column's `Format` string when converting values for search matching, consistent with how cell values are rendered
- Text input components (`Input`, `Textarea`, `InputGroupInput`, `InputGroupTextarea`) now normalize empty/whitespace strings to `null`, preventing inconsistent validation on nullable string properties (#99)
- Dropdown mispositioning when switching between multiple open MultiSelect components
- Stale portal content when interacting inside open popovers
- ContextMenu keyboard navigation breaking on repeated right-clicks on the same trigger
- Toast positioning demo now interactive with live position switching
- Timeline connector line no longer uses fixed height — dynamically stretches to match content height (#104)
- Timeline connector gap asymmetry between top and bottom icons resolved with symmetric spacing
- Timeline loading pulse animation no longer bleeds through icon ring onto connector line
- Timeline collapsible item chevrons now align consistently regardless of title length
- Menubar demo icon name corrected from `check-circle` to `circle-check`
- Timeline connector `z-index` changed from `z-10` to `z-[1]` to prevent overlap with overlays
- Timeline connector minimum height increased from `min-h-8` to `min-h-16` for better spacing

---

## 2026-02-07

### Added
- Timeline component with composable sub-components (Timeline, TimelineItem, TimelineIcon, TimelineConnector, TimelineContent, TimelineHeader, TimelineTitle, TimelineDescription, TimelineTime, TimelineEmpty)
  - Layout alignment options (Center, Left, Right, Alternate)
  - Icon variants (Solid, Outline) with status-based coloring
  - Connector line styles (Solid, Dashed, Dotted)
  - Loading state with pulse animation
  - Collapsible items support
  - Size presets (Small, Medium, Large)
- DataTableColumn `Format` parameter for custom formatting of cell values (e.g., date/number formats via `IFormattable`)
- DataTableColumn now supports nullable `TValue` types for columns with null values
- Custom variant/styling examples to Button, Badge, Alert, and Toast demo pages showing how to use the `Class` parameter for project-specific variants
- `Class` property on `ToastData` for custom toast styling via TailwindMerge
- SplitButton component — primary action button with a dropdown menu for secondary actions, composing Button + DropdownMenu with SplitButtonItem and SplitButtonSeparator sub-components

### Changed
- `blazorblueprint.css` removed from git tracking — now rebuilt automatically during build via MSBuild targets (Windows and Linux), eliminating merge conflicts across branches

### Fixed
- Infinite render loop when FloatingPortal is nested inside Dialog (e.g., Combobox in Dialog) — removed re-registration in `OnParametersSet` to match DialogPortal pattern
- Migrated all components to `ClassNames.cn()` for proper TailwindMerge support
- TailwindMerge incorrectly classified `border-dashed`, `border-dotted`, and `border-l-2` as border-color utilities, causing them to be dropped during class merging


---

## 2026-02-06

### Added
- Button component now supports `Href` and `Target` parameters, rendering as an `<a>` element for navigation links while preserving all visual styling
- ColorPicker alpha (A) input field alongside R/G/B for precise transparency control

### Changed
- Coding standards are now enforced at build time

### Fixed
- Resolved all build warnings and configured warnings-as-errors for stricter code quality
- ColorPicker alpha channel now displays clean 6-char hex with percentage instead of confusing 8-char hex format
- ColorPicker alpha precision no longer drifts on repeated interactions
- ColorPicker demo alpha preview CSS corrected for proper transparency rendering

---

## 2026-02-05

### Fixed
- Touch and drag interactions now work on mobile devices for Slider, RangeSlider, Resizable, and ColorPicker components (migrated from mouse events to Pointer Events)
- RangeSlider thumb z-index no longer appears above dialog overlays
- Removed duplicate sidebar.js script tags causing console errors in demos
- Fixed broken theme CSS and image asset paths in demo apps

---

## 2026-02-04

### Added
- Multi-hosting model demo support - demos now run in Auto, Server, and WebAssembly modes

---

## 2026-02-03

### Fixed
- ScrollArea overflow and viewport height constraints for proper scrolling
- Spinner custom color support restored
- Compiler warnings (CS1998, CS0414) resolved across components
- DropdownMenu components now use fully qualified names to avoid ambiguity

### Changed
- Renamed "BlazorBlueprint" to "Blazor Blueprint" in page titles and prose

---

## 2026-02-02

### Changed
- Project renamed from BlazorUI to BlazorBlueprint for v2.0.0
- Repository URL updated to blazorblueprintui/ui

### Fixed
- ApexCharts legend now uses theme-aware styles

---

## 2026-02-01

### Added
- Chart Components Library with 6 chart types built on Blazor-ApexCharts:
  - AreaChart (Default, Spline, Stacked, Stepline variants)
  - BarChart (Vertical, Horizontal, Stacked, StackedHorizontal, FullStacked, FullStackedHorizontal, Grouped variants)
  - LineChart (Default, Spline, Stepline, Dashed, Gradient variants)
  - PieChart (Pie, Donut, GradientDonut variants)
  - RadarChart (Default, PolygonFill, MultiSeries variants)
  - RadialChart (Default, SemiCircle, Gauge, Gradient variants)
- ChartContainer wrapper component for consistent card-like styling
- ChartConfig for series-to-label/color mapping with CSS variable integration

### Fixed
- Select dropdown scroll flash when opening with a selected item

### Docs
- Comprehensive theming guide (THEMING.md) covering CSS variables, dark mode, and customization
- Chart components README with architecture explanation and usage examples
- ApexCharts.js and Blazor-ApexCharts added to acknowledgments (MIT License)
- README updated with accurate component (65+) and primitive (15) counts

---

## 2026-01-31

### Added
- FloatingPortal component for unified floating content infrastructure (portal registration, positioning, lifecycle management)

### Changed
- PopoverContent, TooltipContent, HoverCardContent, and DropdownMenuContent primitives refactored to use FloatingPortal (~600 lines of duplicated code removed)

### Fixed
- Select dropdown positioning flash on open
- Controlled state support for all floating components (Popover, Tooltip, HoverCard, DropdownMenu)
- Tooltip positioning now correctly derives Side/Align from context Placement
- PopoverContent MatchTriggerWidth not working in Combobox and MultiSelect
- Duplicate margin in Select dropdown causing inconsistent spacing with other dropdowns

---

## 2026-01-30

### Added
- ResponsiveNav component set for mobile-friendly navigation:
  - ResponsiveNavProvider: Context provider with JS interop for mobile detection
  - ResponsiveNavTrigger: Hamburger menu button (visible only on mobile, below 768px)
  - ResponsiveNavContent: Mobile Sheet content wrapper with auto-close on navigate
- NavigationMenuDemo updated with responsive example showing search and notification icons

---

## 2026-01-29

### Added
- 9 new input and selection components:
  - ColorPicker: Visual color selection with hex/RGB input, preset colors, and optional alpha channel
  - CurrencyInput: Locale-aware currency input with 40+ currencies and automatic formatting
  - DateRangePicker: Date range selection with dual calendar view and preset ranges
  - FileUpload: Drag-and-drop file upload with image previews, validation, and progress tracking
  - MaskedInput: Input with structured formats (phone, SSN, credit card, custom masks)
  - NumericInput: Numeric input with increment/decrement buttons and min/max constraints
  - RangeSlider: Dual-handle slider for selecting value ranges
  - Rating: Star rating component with half-value support and custom icons
  - TimePicker: Time selection with 12/24-hour format and keyboard navigation

### Fixed
- TailwindMerge validation now supports CSS combinator characters (`>`, `+`, `~`)
- DataTable select-all checkbox no longer toggles before dropdown option is selected
- CurrencyInput and NumericInput focus ring moved to container level

### Performance
- DataTable selection optimized for large datasets

### Docs
- README updated with hero image and improved intro

---

## 2026-01-27

### Added
- DataTable select-all dropdown - when total items exceed current page, clicking select-all checkbox shows dropdown with options to "Select all on this page" or "Select all X items" across all pages
- EnableKeyboardNavigation parameter for Table and DataTable components
- CSS custom properties for typography (`--tracking-*` letter-spacing variables, `--font-sans/serif/mono` font family variables)
- New JS modules for better code organization: `element-utils.js`, `table-row-nav.js`, `virtualization-scroll.js`
- IDropdownManagerService interface for improved testability

### Changed
- Table Primitive refactored - removed unused FilteringState and ColumnFilter classes (filtering is now solely DataTable's responsibility)
- OnFilter callback signature changed from `EventCallback<(string?, Dictionary<string, string>)>` to `EventCallback<string?>` (BREAKING CHANGE)
- DataTable default PageSizes updated to include 5, InitialPageSize changed to 5

### Fixed
- **Security**: Replaced all `eval()` calls with proper JS modules across 6 components
- **Security**: Fixed cssText injection vulnerability in `match-trigger-width.js`
- **Security**: Added HtmlSanitizer to MarkdownEditor to prevent XSS attacks
- **Security**: Added input validation to DropdownManagerService, TailwindMerge, KeyboardShortcutService, Slider, and ResizablePanelGroup
- **Security**: Added CSS class pattern validation to reject dangerous patterns
- DataTable selection tracking and reference aliasing bug resolved
- DataTable checkbox double-toggle issue fixed with stopPropagation
- Table focus ring styles added for keyboard navigation visibility
- Table arrow keys no longer scroll page during row navigation
- Bare catch blocks replaced with specific exception types
- Null-forgiving operators replaced with null-coalescing fallbacks

### Performance
- PortalHost optimized with portal key change tracking to avoid unnecessary re-renders
- MultiSelect cached TriggerCssClass and added delegate caching for event handlers
- RichTextEditor added ShouldRender() optimization to prevent unnecessary renders
- RadioGroup cached enabled items list to avoid LINQ allocations
- DataTable optimized search filtering with extracted MatchesSearch() method
- Calendar cached JS module reference and implemented proper IAsyncDisposable
- TailwindMerge added ConcurrentDictionary cache for regex lookups, fixed double regex evaluation
- TableDataExtensions optimized pagination with GetRange() instead of Skip().Take()

---

## 2026-01-26

### Added
- CommandVirtualizedGroup component for efficient rendering of large item lists using Blazor's Virtualize
- EnableLazyLoading parameter for CommandVirtualizedGroup - loads items progressively as user scrolls
- Wrap-around keyboard navigation for virtualized groups (Up at first item jumps to last, Down at last jumps to first)
- ScrollToIndexAsync method for programmatic scrolling to any item in virtualized groups
- Global search feature in demo header with Ctrl+K keyboard shortcut
- OnKeyboardNavigationChanged event for coordinating focus state across components
- OnFocusChanged and OnSearchChanged targeted events for O(1) performance updates
- Calendar keyboard navigation (arrow keys, Home/End, PageUp/PageDown, Enter/Space to select)
- ShowMonthYearDropdowns parameter for Calendar - option to show simple text header instead of dropdowns

### Changed
- Command component now supports both regular CommandItem and virtualized groups
- Keyboard navigation unified across regular items and virtualized groups with proper wrapping
- CommandSearch updated to use lazy loading for all icon groups (Lucide, Heroicons, Feather)

### Fixed
- Command component performance improved from O(n) to O(1) re-renders on focus change
- CommandVirtualizedGroup virtualization fixed - removed overflow-hidden that clipped Virtualize spacer elements
- Hover/keyboard navigation conflict resolved - CSS hover suppressed during keyboard navigation
- CommandItem no longer causes page jump on keyboard navigation (affected Combobox)
- Scroll-into-view only triggers during keyboard navigation, not mouse hover
- Select dropdown scrollbar appearance fixed for Edge browser

---

## 2026-01-25

### Fixed
- Menubar now switches between menus with a single click (previously required double-click due to overlay blocking)

### Added
- DisplayTextSelector parameter for Select component - allows deriving display text from the selected value without waiting for dropdown items to render
- DayClass and CellClass parameters for Calendar component - allows custom styling of day buttons and cells

### Changed
- License changed from MIT to Apache 2.0 for BlazorBlueprint.Components and BlazorBlueprint.Primitives (icon libraries remain MIT)
- Added NOTICE file with attribution requirements for derivative works
- Combobox and MultiSelect focus mechanism replaced with event-based approach using OnContentReady
- MultiSelect search box styling now matches Combobox for visual consistency

### Fixed
- Calendar performance significantly improved through caching and elimination of redundant renders
- Select dropdown now scrolls to and centers the selected item when opened

---

## 2026-01-24

### Added
- KeyboardShortcutService for global keyboard shortcut management
- Functional keyboard shortcuts in DropdownMenu and Menubar demo pages
- KeyboardShortcut utility class for parsing shortcut strings (e.g., "Ctrl+Shift+N")

### Changed
- DropdownMenu demo updated to show functional shortcuts with visual feedback
- Menubar demo updated to show functional shortcuts with visual feedback

---

## 2026-01-23

### Changed
- RichTextEditor rewritten to use Quill.js v2 in headless mode with custom Blazor toolbar
- RichTextEditor now uses Blazor Blueprint components (NativeSelect, Dialog, Toggle, Button) instead of raw HTML elements
- Link insertion uses BlazorBlueprint Dialog instead of browser prompt for consistent UX

### Fixed
- RichTextEditor block format removal now preserves inline formatting (bold, italic, etc.)
- RichTextEditor uses Quill's getSemanticHTML() for normalized cross-browser HTML output
- RichTextEditor callback suppression prevents update loops during programmatic content changes

### Added
- RichTextEditor SetDeltaAsync/GetDeltaAsync methods for native Quill Delta format support
- RichTextEditor toolbar presets (None, Simple, Standard, Full, Custom)

---

## 2026-01-22

### Added
- Checkbox item support for DropdownMenu and Menubar components

### Changed
- DataTable now uses the enhanced Pagination component

### Fixed
- Select component shows display name instead of raw value
- Select component keyboard navigation reliability improved
- ContextMenu repositions correctly on right-click within trigger area

---

## 2026-01-21

### Added
- Calendar month/year selection grids for easier date navigation
- ShowOutsideDays parameter for Calendar component
- Alert redesign with semantic variants (default, destructive, warning, success, info)

### Fixed
- DatePicker nested portal click-outside issues resolved
- DatePicker month/year dropdown no longer closes unexpectedly during navigation

---

## 2026-01-20

### Added
- 26 new components achieving shadcn/ui parity:
  - **High priority**: Alert, AlertDialog, Progress, Spinner, Toast, Calendar, DatePicker, NavigationMenu
  - **Medium priority**: Breadcrumb, Carousel, ContextMenu, Drawer, Menubar, Pagination, ScrollArea, Slider, Toggle, ToggleGroup
  - **Low priority**: AspectRatio, Empty, InputOTP, Kbd, NativeSelect, Resizable, Typography

### Fixed
- Keyboard navigation improvements across components
- Various component bug fixes

---

## 2026-01-19

### Added
- AutoClose parameter for MultiSelect component

### Fixed
- MultiSelect rebuilt using primitives architecture
- MultiSelect click-outside behavior corrected

---

## 2026-01-15

### Changed
- Website moved to separate repository

### Fixed
- Nested portal infinite loop prevention

---

## 2025-12-16

### Fixed
- Portal synchronization improvements
- Scroll jump prevention in portal-based components

---

## 2025-12-10

### Changed
- Comprehensive README documentation updates

---

## 2025-12-08

### Added
- AsChild pattern for trigger and close components

### Fixed
- UI jump in portal-based components

---

## 2025-12-07

### Fixed
- MultiSelect stale callback bug
- Accordion and Collapsible animation issues

---

## 2025-12-06

### Added
- MultiSelect component

### Fixed
- Primitives package reference update

---

## 2025-11-16

### Added
- Icon libraries expansion (Heroicons, Feather, Lucide)
- Website infrastructure
- DevFlow workflow system
- LLM documentation structure

### Changed
- GitHub Actions updates

---

## 2025-11-15

### Added
- Initial release v1.0.0-beta.1

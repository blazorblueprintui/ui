# Blazor Blueprint v3 Migration Guide

This guide helps you upgrade from Blazor Blueprint **v2** to **v3**. Sections are grouped by type: breaking changes that require code updates come first, followed by new features you can adopt at your own pace.

---

## Migration Checklist

These are the **breaking changes** that require code updates. Address them in order.

| # | Breaking Change | Severity | Action Required |
|---|---|---|---|
| 1 | Component `Bb` prefix | **High** | Add `Bb` prefix to all component tags — `<Button>` → `<BbButton>`, `<Dialog>` → `<BbDialog>`, etc. |
| 2 | Namespace flattening | **High** | Remove `@using BlazorBlueprint.Components.*` sub-namespace imports; replace `using BlazorBlueprint.Components.Extensions` with `using BlazorBlueprint.Components` |
| 3 | `Combobox<TItem>` → `BbCombobox<TValue>` | **High** | Replace `Items`/`ValueSelector`/`DisplaySelector` with `Options` or `BbComboboxItem` children |
| 4 | `MultiSelect<TItem>` → `BbMultiSelect<TValue>` | **High** | Replace `Items`/`ValueSelector`/`DisplaySelector` with `Options` or `BbMultiSelectItem` children |
| 5 | `FormFieldCombobox<TItem>` → `BbFormFieldCombobox<TValue>` | **High** | Replace `Items`/`ValueSelector`/`DisplaySelector` with `Options` or `ChildContent` |
| 6 | `FormFieldMultiSelect<TItem>` → `BbFormFieldMultiSelect<TValue>` | **High** | Replace `Items`/`ValueSelector`/`DisplaySelector` with `Options` or `ChildContent` |
| 7 | Trigger/Close `AsChild` default → `true` | **High** | Add `AsChild="false"` or wrap content with a component |
| 8 | `PositioningOptions.Strategy` → enum | **High** | Replace string values with `PositioningStrategy` enum |
| 9 | `BbSelectValue.ChildContent` → `RenderFragment<string>` | **High** | Add `context` parameter to existing templates |
| 10 | `BbResizablePanel` flex-grow 0 → 1 | **Medium** | Review panel layouts for unintended stretching |
| 11 | `SelectContext.SetDisplayText()` removed | **Medium** | Use `State.DisplayText` directly |
| 12 | Primitive `BbSelectItem.TextValue` → `Text` | **Low** | Rename `TextValue` to `Text` on primitive `BbSelectItem` usage |
| 13 | Chart components: ApexCharts → ECharts | **High** | Replace all chart markup — new declarative composition API with `<BbXAxis>`, `<BbLine>`, `<BbBar>`, etc. |
| 14 | `UpdateTiming` default: `Immediate` → `OnChange` | **Medium** | Add `UpdateTiming="UpdateTiming.Immediate"` if per-keystroke updates are needed |
| 15 | Input components: `IDisposable` → `IAsyncDisposable` | **Low** | Only affects code that explicitly casts to `IDisposable` |
| 16 | `IPortalService` Two-Layer Portal Architecture | **High** | Update `RegisterPortal` calls to include `PortalCategory`; replace `OnPortalsChanged` with `OnPortalsCategoryChanged`; replace `GetPortals()` with `GetPortals(PortalCategory)` |
| 17 | `ToastService.Success()` visual change | **Low** | Now renders with `ToastVariant.Success` (green accent + icon) instead of `ToastVariant.Default` (neutral); use `ToastService.Show()` to restore neutral style |
| 18 | Localization: string parameters now nullable | **Low** | Some component string parameters changed from `string` to `string?` with defaults from `BbLocalizationOptions` — only affects code that relied on non-null parameter types |
| 19 | Calendar: `FirstDayOfWeek` default changed | **Medium** | Default changed from `DayOfWeek.Sunday` to `CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek` — set explicitly if Sunday is required |
| 20 | Calendar: month/day names from CultureInfo | **Low** | Hardcoded English month/day name arrays replaced with `CultureInfo.CurrentCulture.DateTimeFormat` — override with `MonthNames`/`DayNames` parameters if needed |
| 21 | DatePicker: `DateFormat` default changed | **Medium** | Default changed from `"MMMM d, yyyy"` to `"d"` (culture-aware short date) — set `DateFormat="MMMM d, yyyy"` explicitly to restore old behavior |
| 22 | `InputConverter` uses `CurrentCulture` | **Medium** | Numeric parsing/formatting now uses `CultureInfo.CurrentCulture` instead of `InvariantCulture` — set `InputConverter.GlobalCulture = CultureInfo.InvariantCulture` to restore old behavior |

---

## Table of Contents

- [Breaking Changes](#breaking-changes)
  - [Component Bb Prefix](#component-bb-prefix)
  - [Namespace Flattening](#namespace-flattening)
  - [Combobox Component](#combobox-component)
  - [MultiSelect Component](#multiselect-component)
  - [FormFieldCombobox Component](#formfieldcombobox-component)
  - [FormFieldMultiSelect Component](#formfieldmultiselect-component)
  - [Trigger and Close Components](#trigger-and-close-components)
  - [Positioning Strategy](#positioning-strategy)
  - [Select Component (Breaking)](#select-component-breaking)
  - [ResizablePanel](#resizablepanel)
  - [Chart Components: ApexCharts → ECharts](#chart-components-apexcharts--echarts)
  - [UpdateTiming Default Change](#updatetiming-default-change)
  - [Input Components: IDisposable → IAsyncDisposable](#input-components-idisposable--iasyncdisposable)
  - [IPortalService: Two-Layer Portal Architecture](#iportalservice-two-layer-portal-architecture)
  - [Localization Support](#localization-support)
- [Setup Changes](#setup-changes)
  - [Service Registration (DI)](#service-registration-di)
  - [PortalHost Runtime Warning](#portalhost-runtime-warning)
- [Select Ecosystem: New Options API](#select-ecosystem-new-options-api)
  - [SelectOption Record](#selectoption-record)
  - [SelectOptionExtensions](#selectoptionextensions)
  - [Select: Options Mode](#select-options-mode)
  - [FormFieldSelect: Options Mode](#formfieldselect-options-mode)
- [New Components](#new-components)
  - [CommandDialog](#commanddialog-component)
  - [AvatarGroup](#avatargroup-component)
  - [DrawerItem](#draweritem-component)
  - [CheckboxGroup](#checkboxgroup-component)
  - [DialogService](#dialogservice-programmatic-confirm-dialogs)
  - [ResponsiveNavItems](#responsivenav-shared-item-definition)
- [New Parameters](#new-parameters)
  - [RadioGroupItem: Label](#radiogroupitem-label)
  - [Button: Loading State](#button-loading-state)
  - [Alert: Dismissible](#alert-dismissible)
  - [Textarea: ShowCharacterCount](#textarea-showcharactercount)
  - [Slider: TrackClass and ThumbClass](#slider-trackclass-and-thumbclass)
  - [InputOTP: InputClass](#inputotp-inputclass)
  - [Progress: ShowLabel and LabelFormat](#progress-showlabel-and-labelformat)
  - [Skeleton: Width and Height](#skeleton-width-and-height)
  - [MarkdownEditor: MinHeight](#markdowneditor-minheight)
  - [ColorPicker: DefaultValue](#colorpicker-defaultvalue)
  - [TooltipContent: ShowArrow](#tooltipcontent-showarrow)
  - [Menu Items: Icon](#menu-items-icon)
  - [BreadcrumbList: AutoSeparator](#breadcrumblist-autoseparator)
  - [TimelineItem: Shorthand Parameters](#timelineitem-shorthand-parameters)
  - [Pagination: Shorthand Parameters](#pagination-shorthand-parameters)
  - [Menubar: TriggerClass](#menubar-triggerclass)
  - [MenubarItem: Shortcut](#menubaritem-shortcut)
  - [Avatar: ShowDot and DotClass](#avatar-showdot-and-dotclass)
  - [Badge: ShowDot, DotPosition, DotClass](#badge-showdot-dotposition-dotclass)
  - [DropdownMenuItem: Href and Target](#dropdownmenuitem-href-and-target)
  - [Field: Cascading IsInvalid](#field-cascading-isinvalid)
  - [DataTableColumn: Property No Longer EditorRequired](#datatablecolumn-property-no-longer-editorrequired)
  - [ScrollArea: FillContainer](#scrollarea-fillcontainer)
  - [SidebarInset: ResetScrollOnNavigation](#sidebarinset-resetscrollonnavigation)
  - [Toast: Semantic Variants, Size, Icons, Pause-on-Hover, Per-Toast Position](#toast-semantic-variants-size-icons-pause-on-hover-per-toast-position)
  - [Input Components: Auto-Generated IDs](#input-components-auto-generated-ids)
- [Primitive Layer Improvements](#primitive-layer-improvements)
  - [SwitchThumb Sub-Component](#switchthumb-sub-component)
  - [CheckboxIndicator Sub-Component](#checkboxindicator-sub-component)
  - [ItemClass on DropdownMenu, RadioGroup, Select](#itemclass-parent-level-item-styling)
  - [CascadingTypeParameter](#cascadingtypeparameter)
  - [FloatingPortal: ForceMount](#floatingportal-forcemount)
  - [ISelectDisplayContext Interface](#iselectdisplaycontext-interface)
- [Visual Changes](#visual-changes)
  - [Checkbox Proportions](#checkbox-proportions)
  - [RadioGroupItem Filled Circle Design](#radiogroupitem-filled-circle-design)
- [Bug Fixes](#bug-fixes)
- [Internal Improvements](#internal-improvements)

---

## Breaking Changes

> **Note:** All v3 code examples throughout this guide use the `Bb` prefix. When comparing v2 → v3 examples, the prefix change is part of the migration.

### Component Bb Prefix

**Severity: High**

All Razor components now use a `Bb` prefix. This applies to every component tag across both `BlazorBlueprint.Components` (~300+ components) and `BlazorBlueprint.Primitives` (~65 components). Non-component types — enums, context classes, services, helper classes, interfaces, and event args — are unchanged.

**Why:** Prefixed component names prevent naming collisions with standard HTML elements, user-defined components, and third-party libraries. This follows the same convention used by MudBlazor (`Mud` prefix) and Radzen (`Radzen` prefix).

**Before:**

```razor
<Dialog @bind-Open="_open">
    <DialogTrigger>
        <Button>Open</Button>
    </DialogTrigger>
    <DialogContent>
        <DialogHeader>
            <DialogTitle>Title</DialogTitle>
            <DialogDescription>Description</DialogDescription>
        </DialogHeader>
    </DialogContent>
</Dialog>
```

**After:**

```razor
<BbDialog @bind-Open="_open">
    <BbDialogTrigger>
        <BbButton>Open</BbButton>
    </BbDialogTrigger>
    <BbDialogContent>
        <BbDialogHeader>
            <BbDialogTitle>Title</BbDialogTitle>
            <BbDialogDescription>Description</BbDialogDescription>
        </BbDialogHeader>
    </BbDialogContent>
</BbDialog>
```

**What gets the `Bb` prefix** — all Razor component tags (anything used as an HTML-like element in `.razor` markup):

| v2 | v3 |
|---|---|
| `<Button>` | `<BbButton>` |
| `<Dialog>` | `<BbDialog>` |
| `<Select>` | `<BbSelect>` |
| `<BarChart>` | `<BbBarChart>` |
| `<XAxis>` | `<BbXAxis>` |
| `<CheckboxGroup>` | `<BbCheckboxGroup>` |
| `<PortalHost>` | `<BbPortalHost>` |
| `<ToastProvider>` | `<BbToastProvider>` |

**What does NOT get the `Bb` prefix:**

- **Enums:** `ButtonVariant`, `AlertVariant`, `SheetSide`, `BadgeDotPosition`, etc.
- **Context classes:** `TooltipContext`, `DialogContext`, `CollapsibleContext`, etc.
- **Services:** `ToastService`, `DialogService`, `IPortalService`, etc.
- **Helper classes:** `SelectOption<T>`, `DateRange`, `NavItemDefinition`, etc.
- **Interfaces:** `IChartComponent`, `IDropdownMenuItem`, etc.
- **Event args:** `TextChangeEventArgs`, `SelectionChangeEventArgs`, etc.
- **RenderFragment parameters:** `ChildContent`, `Icon`, `LoadingTemplate`, etc.

**Steps:**

1. **Add `Bb` prefix to every component tag** (opening and closing) in your `.razor` files:
   ```diff
   - <Button Variant="ButtonVariant.Primary">Click me</Button>
   + <BbButton Variant="ButtonVariant.Primary">Click me</BbButton>
   ```

2. **Update `_Imports.razor`** — no changes needed for the prefix itself; the same `@using BlazorBlueprint.Components` import covers all `Bb`-prefixed components.

3. **Update `typeof()` and `nameof()` references** if you reference component types in C# code:
   ```diff
   - builder.OpenComponent<Dialog>(0);
   + builder.OpenComponent<BbDialog>(0);
   ```

> **Tip:** Use your IDE's find-and-replace to bulk-update component tags. In most cases, searching for `<ComponentName` and replacing with `<BbComponentName` (and the same for closing tags `</ComponentName>` → `</BbComponentName>`) handles the migration.

---

### Namespace Flattening

**Severity: High**

All `BlazorBlueprint.Components.*` sub-namespaces (77 total) have been consolidated into the root `BlazorBlueprint.Components` namespace. Additionally, 8 consumer-facing types from `BlazorBlueprint.Primitives.*` sub-namespaces have been moved to the root `BlazorBlueprint.Primitives` namespace.

**Why:** Previously, consumers needed 10+ `@using` lines in `_Imports.razor` — one per component group. This was the most common friction point for new users.

**v2 `_Imports.razor`:**

```razor
@using BlazorBlueprint.Components
@using BlazorBlueprint.Components.Button
@using BlazorBlueprint.Components.Input
@using BlazorBlueprint.Components.Dialog
@using BlazorBlueprint.Components.Sheet
@using BlazorBlueprint.Components.Accordion
@using BlazorBlueprint.Components.Tabs
@using BlazorBlueprint.Components.Select
@using BlazorBlueprint.Components.Avatar
@using BlazorBlueprint.Components.Badge
@using BlazorBlueprint.Primitives.Services
```

**v3 `_Imports.razor`:**

```razor
@using BlazorBlueprint.Components
@using BlazorBlueprint.Primitives
```

Two imports give you access to all components and their enums (`ButtonVariant`, `InputType`, `AccordionType`, `SheetSide`, etc.).

**Steps:**

1. **Remove all `@using BlazorBlueprint.Components.*` sub-namespace imports** from your `_Imports.razor` and individual `.razor` files. The root `@using BlazorBlueprint.Components` now covers everything.

2. **Replace `@using BlazorBlueprint.Primitives.Services`** with `@using BlazorBlueprint.Primitives` in your `_Imports.razor`. The 8 types that Components' public API exposes (e.g., `AccordionType`, `SheetSide`, `SortDirection`, `PaginationState`, `PopoverSide`, `PopoverAlign`, `PopoverPlacement`, `PositioningStrategy`) are now in the root Primitives namespace.

3. **Update `using BlazorBlueprint.Components.Extensions`** in `.cs` files to `using BlazorBlueprint.Components` (e.g., for `AddBlazorBlueprintComponents()`).

4. **Remove fully-qualified component references** that used the old sub-namespaces:
   ```diff
   - <BlazorBlueprint.Components.Dialog.Dialog>
   + <BbDialog>
   ```

> **Note:** The `BlazorBlueprint.Primitives.*` sub-namespaces still exist — only 8 consumer-facing types were promoted to the root namespace. If you use Primitives directly, your existing `@using BlazorBlueprint.Primitives.Dialog` etc. imports continue to work.

---

### Combobox Component

**Severity: High**

`Combobox<TItem>` has been renamed to `BbCombobox<TValue>` with a completely redesigned API. The `Items`, `ValueSelector`, and `DisplaySelector` parameters have been removed. The component now supports two modes: **Options mode** (data-driven) and **Compositional mode** (using `BbComboboxItem` children). See [SelectOption Record](#selectoption-record) for details on the `SelectOption<TValue>` type used by the Options mode.

The `Value` type changed from `string?` to `TValue?`, making the component fully generic — it now supports `int`, `Guid`, `enum`, and any other value type, not just strings.

**Why:** The previous API required a separate item model type (`TItem`) with `ValueSelector`/`DisplaySelector` functions to extract string values and display text. This was inconsistent with `Select<TValue>` (which was already generic) and forced all values to be strings. The new API aligns all three selection components — Select, Combobox, and MultiSelect — on the same `SelectOption<TValue>` pattern.

**v2 (Items + selectors):**

```razor
<Combobox TItem="Framework"
          Items="frameworks"
          @bind-Value="selectedFramework"
          ValueSelector="@(f => f.Value)"
          DisplaySelector="@(f => f.Label)"
          Placeholder="Select framework..."
          SearchPlaceholder="Search..."
          EmptyMessage="No framework found." />

@code {
    private string? selectedFramework;
    private List<Framework> frameworks = new()
    {
        new("blazor", "Blazor"),
        new("react", "React"),
        new("vue", "Vue.js")
    };
    public record Framework(string Value, string Label);
}
```

**v3 — Options mode (recommended for simple cases):**

```razor
<BbCombobox TValue="string"
          Options="_frameworkOptions"
          @bind-Value="_selectedFramework"
          Placeholder="Select framework..."
          SearchPlaceholder="Search..."
          EmptyMessage="No framework found." />

@code {
    private string? _selectedFramework;
    private readonly SelectOption<string>[] _frameworkOptions =
    [
        new("blazor", "Blazor"),
        new("react", "React"),
        new("vue", "Vue.js")
    ];
}
```

**v3 — Compositional mode (for rich item rendering):**

```razor
<BbCombobox TValue="string" @bind-Value="_selectedFramework"
          Placeholder="Select framework..." SearchPlaceholder="Search...">
    <BbComboboxItem Value="@("blazor")" Text="Blazor">
        <span class="flex items-center gap-2">
            <img src="blazor-icon.svg" class="h-4 w-4" /> Blazor
        </span>
    </BbComboboxItem>
    <BbComboboxItem Value="@("react")" Text="React">
        <span class="flex items-center gap-2">
            <img src="react-icon.svg" class="h-4 w-4" /> React
        </span>
    </BbComboboxItem>
</BbCombobox>
```

**Key differences:**

| v2 | v3 |
|---|---|
| `TItem` (item model type) | `TValue` (value type) |
| `Items` (required) | `Options` or `ChildContent` |
| `ValueSelector` (required) | Not needed — value is on `SelectOption.Value` or `BbComboboxItem.Value` |
| `DisplaySelector` (required) | Not needed — text is on `SelectOption.Text` or `BbComboboxItem.Text` |
| `Value` is `string?` | `Value` is `TValue?` |
| `ValueChanged` is `EventCallback<string?>` | `ValueChanged` is `EventCallback<TValue?>` |
| `ValueExpression` is `Expression<Func<string?>>?` | `ValueExpression` is `Expression<Func<TValue?>>?` |

---

### MultiSelect Component

**Severity: High**

`MultiSelect<TItem>` has been renamed to `BbMultiSelect<TValue>` with the same API redesign as Combobox. The `Items`, `ValueSelector`, and `DisplaySelector` parameters have been removed. The component now supports **Options mode** and **Compositional mode** (using `BbMultiSelectItem` children).

`Values` changed from `IEnumerable<string>?` to `IEnumerable<TValue>?`, making the component fully generic.

**v2 (Items + selectors):**

```razor
<MultiSelect TItem="Framework"
             Items="frameworks"
             @bind-Values="selectedValues"
             ValueSelector="@(f => f.Value)"
             DisplaySelector="@(f => f.Label)"
             Placeholder="Select frameworks..."
             SearchPlaceholder="Search..." />

@code {
    private IEnumerable<string>? selectedValues;
    private List<Framework> frameworks = new()
    {
        new("blazor", "Blazor"),
        new("react", "React"),
        new("vue", "Vue.js")
    };
    public record Framework(string Value, string Label);
}
```

**v3 — Options mode (recommended for simple cases):**

```razor
<BbMultiSelect TValue="string"
             Options="_frameworkOptions"
             @bind-Values="_selectedValues"
             Placeholder="Select frameworks..."
             SearchPlaceholder="Search..." />

@code {
    private IEnumerable<string>? _selectedValues;
    private readonly SelectOption<string>[] _frameworkOptions =
    [
        new("blazor", "Blazor"),
        new("react", "React"),
        new("vue", "Vue.js")
    ];
}
```

**v3 — Compositional mode (for rich item rendering):**

```razor
<BbMultiSelect TValue="string" @bind-Values="_selectedValues"
             Placeholder="Select frameworks..." SearchPlaceholder="Search...">
    <BbMultiSelectItem Value="@("blazor")" Text="Blazor">
        <span class="flex items-center gap-2">
            <img src="blazor-icon.svg" class="h-4 w-4" /> Blazor
        </span>
    </BbMultiSelectItem>
    <BbMultiSelectItem Value="@("react")" Text="React">
        <span class="flex items-center gap-2">
            <img src="react-icon.svg" class="h-4 w-4" /> React
        </span>
    </BbMultiSelectItem>
</BbMultiSelect>
```

**Key differences:**

| v2 | v3 |
|---|---|
| `TItem` (item model type) | `TValue` (value type) |
| `Items` (required) | `Options` or `ChildContent` |
| `ValueSelector` (required) | Not needed — value is on `SelectOption.Value` or `BbMultiSelectItem.Value` |
| `DisplaySelector` (required) | Not needed — text is on `SelectOption.Text` or `BbMultiSelectItem.Text` |
| `Values` is `IEnumerable<string>?` | `Values` is `IEnumerable<TValue>?` |
| `ValuesChanged` is `EventCallback<IEnumerable<string>?>` | `ValuesChanged` is `EventCallback<IEnumerable<TValue>?>` |
| `ValuesExpression` is `Expression<Func<IEnumerable<string>?>>?` | `ValuesExpression` is `Expression<Func<IEnumerable<TValue>?>>?` |

---

### FormFieldCombobox Component

**Severity: High**

`FormFieldCombobox<TItem>` has been renamed to `BbFormFieldCombobox<TValue>`, mirroring the Combobox API changes. The `Items`, `ValueSelector`, and `DisplaySelector` parameters have been removed. Use `Options` for data-driven mode or `ChildContent` with `BbComboboxItem` children for compositional mode.

**v2:**

```razor
<FormFieldCombobox TItem="Country"
                   Items="countries"
                   @bind-Value="_country"
                   ValueSelector="@(c => c.Code)"
                   DisplaySelector="@(c => c.Name)"
                   Label="Country"
                   Placeholder="Select a country..." />
```

**v3 — Options mode:**

```razor
<BbFormFieldCombobox TValue="string"
                   Options="_countryOptions"
                   @bind-Value="_country"
                   Label="Country"
                   Placeholder="Select a country..." />

@code {
    private readonly SelectOption<string>[] _countryOptions =
    [
        new("us", "United States"),
        new("ca", "Canada")
    ];
}
```

**v3 — Compositional mode:**

```razor
<BbFormFieldCombobox TValue="string"
                   @bind-Value="_country"
                   Label="Country"
                   Placeholder="Select a country...">
    <BbComboboxItem Value="@("us")" Text="United States">
        <span class="flex items-center gap-2">United States</span>
    </BbComboboxItem>
    <BbComboboxItem Value="@("ca")" Text="Canada">
        <span class="flex items-center gap-2">Canada</span>
    </BbComboboxItem>
</BbFormFieldCombobox>
```

---

### FormFieldMultiSelect Component

**Severity: High**

`FormFieldMultiSelect<TItem>` has been renamed to `BbFormFieldMultiSelect<TValue>`, mirroring the MultiSelect API changes. The `Items`, `ValueSelector`, and `DisplaySelector` parameters have been removed. Use `Options` for data-driven mode or `ChildContent` with `BbMultiSelectItem` children for compositional mode.

**v2:**

```razor
<FormFieldMultiSelect TItem="Skill"
                      Items="skills"
                      @bind-Values="_selectedSkills"
                      ValueSelector="@(s => s.Id)"
                      DisplaySelector="@(s => s.Name)"
                      Label="Skills"
                      Placeholder="Select skills..." />
```

**v3 — Options mode:**

```razor
<BbFormFieldMultiSelect TValue="string"
                      Options="_skillOptions"
                      @bind-Values="_selectedSkills"
                      Label="Skills"
                      Placeholder="Select skills..." />

@code {
    private readonly SelectOption<string>[] _skillOptions =
    [
        new("csharp", "C#"),
        new("blazor", "Blazor"),
        new("js", "JavaScript")
    ];
}
```

**v3 — Compositional mode:**

```razor
<BbFormFieldMultiSelect TValue="string"
                      @bind-Values="_selectedSkills"
                      Label="Skills"
                      Placeholder="Select skills...">
    <BbMultiSelectItem Value="@("csharp")" Text="C#" />
    <BbMultiSelectItem Value="@("blazor")" Text="Blazor" />
    <BbMultiSelectItem Value="@("js")" Text="JavaScript" />
</BbFormFieldMultiSelect>
```

---

### Trigger and Close Components

**Severity: High**

All Components-layer trigger and close components now default `AsChild` to `true` instead of `false`.

**Why:** The `AsChild` pattern — where the component composes with a child element instead of rendering its own wrapper — is the intended usage in almost all real-world scenarios. The previous `false` default required an extra `<button>` wrapper that developers almost always replaced anyway.

**Affected components:** `BbDialogTrigger`, `BbDialogClose`, `BbSheetTrigger`, `BbSheetClose`, `BbDropdownMenuTrigger`, `BbPopoverTrigger`, `BbTooltipTrigger`, `BbHoverCardTrigger`, `BbCollapsibleTrigger`

**If you relied on the auto-generated `<button>` wrapper**, add `AsChild="false"`:

```diff
- <BbDialogTrigger>Open</BbDialogTrigger>
+ <BbDialogTrigger AsChild="false">Open</BbDialogTrigger>
```

**If you already provided a child component** (the recommended pattern), no changes needed:

```razor
@* This already works — no changes required *@
<BbDialogTrigger>
    <BbButton>Open Dialog</BbButton>
</BbDialogTrigger>
```

---

### Positioning Strategy

**Severity: High**

`PositioningOptions.Strategy` changed from `string` to the `PositioningStrategy` enum.

**Why:** Type safety — the string-based API allowed invalid values like `"sticky"` or typos like `"fxied"` that would silently fail at runtime.

```diff
- Strategy="fixed"
+ Strategy="PositioningStrategy.Fixed"
```

```diff
- Strategy="absolute"
+ Strategy="PositioningStrategy.Absolute"
```

**Affected components:** Any component that sets `Strategy` on `PopoverContent`, `TooltipContent`, `HoverCardContent`, `DropdownMenuContent`, `FloatingPortal`, or `PositioningOptions` directly.

---

### Select Component (Breaking)

#### `SelectValue.ChildContent` type change

**Severity: High**

`ChildContent` type changed from `RenderFragment` to `RenderFragment<string>`. The template now receives the display text as `context`, so you can combine it with custom rendering (icons, avatars, etc.).

```diff
  <BbSelectValue Placeholder="Select a country...">
-     <span class="flex items-center gap-2">United States</span>
+     <span class="flex items-center gap-2">@GetFlag(_country) @context</span>
  </BbSelectValue>
```

**If you had `ChildContent` that didn't use the display text**, you must accept the `context` parameter (even if you ignore it):

```diff
  <BbSelectValue Placeholder="Pick one">
-     <span>Custom display</span>
+     @* context is the display text — use it or ignore it *@
+     <span>Custom display for @context</span>
  </BbSelectValue>
```

#### Primitive `SelectItem.TextValue` → `Text`

**Severity: Low**

The primitive-layer `SelectItem.TextValue` parameter has been renamed to `Text` for consistency with the Components layer and Combobox/MultiSelect patterns. The `Text` parameter provides the display text shown in the trigger when an item is selected, and also serves as the default rendered content when `ChildContent` is null.

```diff
- <BbSelectItem Value="@("apple")" TextValue="Apple">Apple</BbSelectItem>
+ <BbSelectItem Value="@("apple")" Text="Apple" />
```

#### `SelectContext<TValue>.SetDisplayText()` removed

**Severity: Medium** — only affects custom components built on the Primitives layer.

```diff
- selectContext.SetDisplayText("My Display Text");
+ selectContext.State.DisplayText = "My Display Text";
+ selectContext.NotifyStateChanged();
```

---

### ResizablePanel

**Severity: Medium**

`ResizablePanel` flex-grow changed from `0` to `1`. Panels now grow to fill available space, which is the expected behavior in most layouts.

**Why:** The previous `flex: 0 0 {size}%` meant panels would leave gaps when their content was smaller than the allocated percentage.

If your layout depends on panels maintaining their exact percentage size without growing:

```diff
- <BbResizablePanel DefaultSize="30">...</BbResizablePanel>
+ <BbResizablePanel DefaultSize="30" style="flex-grow: 0;">...</BbResizablePanel>
```

In most cases, no changes are needed — the new behavior is more intuitive.

---

### Chart Components: ApexCharts → ECharts

**Severity: High**

All chart components have been rebuilt from scratch. The previous implementation wrapped [Blazor-ApexCharts](https://github.com/apexcharts/Blazor-ApexCharts) with variant enums and `<ApexPointSeries>` children. The new implementation uses [Apache ECharts](https://echarts.apache.org/) with a Recharts-inspired **declarative composition API** where you compose charts from child components like `<BbXAxis>`, `<BbYAxis>`, `<BbLine>`, `<BbBar>`, etc.

**Why:** The ApexCharts wrapper required verbose lambda expressions (`XValue`, `YValue`), a generic `TItem` type parameter on every chart, and `SeriesType` enums to define series. The new API is simpler — pass any `IEnumerable` as `Data` and reference properties by name via `DataKey` strings. Series are defined as child components, not via `<ApexPointSeries>`.

This is a **full replacement** — every chart in your application must be rewritten. There is no compatibility layer.

**v2 (ApexCharts):**

```razor
@using ApexCharts

<BarChart TItem="SalesData"
          Items="@data"
          Variant="BarChartVariant.Stacked"
          Height="350px">
    <ApexPointSeries TItem="SalesData"
                     Items="@data"
                     Name="Desktop"
                     SeriesType="SeriesType.Bar"
                     XValue="@(item => item.Month)"
                     YValue="@(item => (decimal)item.Desktop)" />
    <ApexPointSeries TItem="SalesData"
                     Items="@data"
                     Name="Mobile"
                     SeriesType="SeriesType.Bar"
                     XValue="@(item => item.Month)"
                     YValue="@(item => (decimal)item.Mobile)" />
</BarChart>

@code {
    public class SalesData
    {
        public string Month { get; set; } = "";
        public int Desktop { get; set; }
        public int Mobile { get; set; }
    }

    private List<SalesData> data =
    [
        new() { Month = "Jan", Desktop = 186, Mobile = 80 },
        new() { Month = "Feb", Desktop = 305, Mobile = 200 },
        new() { Month = "Mar", Desktop = 237, Mobile = 120 }
    ];
}
```

**v3 (ECharts):**

```razor
<BbBarChart Data="@data" Height="350px">
    <BbXAxis DataKey="month" />
    <BbYAxis />
    <BbChartTooltip />
    <BbChartLegend />
    <BbBar DataKey="desktop" Name="Desktop" Color="var(--chart-1)" Stacked="true" />
    <BbBar DataKey="mobile" Name="Mobile" Color="var(--chart-2)" Stacked="true" />
</BbBarChart>

@code {
    private record SalesData(string Month, int Desktop, int Mobile);

    private List<SalesData> data =
    [
        new("Jan", 186, 80),
        new("Feb", 305, 200),
        new("Mar", 237, 120)
    ];
}
```

**Key differences:**

| v2 | v3 |
|---|---|
| `TItem` type parameter on every chart | No type parameter — `Data` is `object?` |
| `Items` parameter | `Data` parameter (any `IEnumerable`) |
| `Variant` enum per chart type | Behavior controlled via child components and parameters |
| `<ApexPointSeries>` with lambdas | `<BbLine>`, `<BbBar>`, `<BbArea>`, `<BbPie>`, `<BbRadar>`, `<BbRadialBar>` with `DataKey` strings |
| `SeriesType` enum | Implicit — each series component knows its type |
| `@using ApexCharts` required | No additional `@using` needed (already in `BlazorBlueprint.Components`) |
| `Blazor-ApexCharts` NuGet dependency | Apache ECharts bundled as static asset (no external NuGet) |

#### Migration by chart type

**LineChart:**

```diff
- <LineChart TItem="TrendData" Items="@data" Variant="LineChartVariant.Spline" Height="300px">
-     <ApexPointSeries TItem="TrendData" Items="@data" Name="Desktop"
-                      SeriesType="SeriesType.Line" XValue="@(i => i.Month)"
-                      YValue="@(i => (decimal)i.Desktop)" />
- </LineChart>
+ <BbLineChart Data="@data" Height="300px">
+     <BbXAxis DataKey="month" />
+     <BbYAxis />
+     <BbChartTooltip />
+     <BbLine DataKey="desktop" Name="Desktop" Color="var(--chart-1)" Curve="CurveType.Smooth" />
+ </BbLineChart>
```

**AreaChart:**

```diff
- <AreaChart TItem="TrendData" Items="@data" Variant="AreaChartVariant.Stacked" Height="300px">
-     <ApexPointSeries TItem="TrendData" Items="@data" Name="Desktop"
-                      SeriesType="SeriesType.Area" XValue="@(i => i.Month)"
-                      YValue="@(i => (decimal)i.Desktop)" />
- </AreaChart>
+ <BbAreaChart Data="@data" Height="300px">
+     <BbXAxis DataKey="month" />
+     <BbYAxis />
+     <BbChartTooltip />
+     <BbArea DataKey="desktop" Name="Desktop" Color="var(--chart-1)" Stacked="true" />
+ </BbAreaChart>
```

**PieChart:**

```diff
- <PieChart TItem="BrowserShare" Items="@data" Variant="PieChartVariant.Donut"
-           CenterLabel="Total" CenterValue="1,234" Height="300px">
-     <ApexPointSeries TItem="BrowserShare" Items="@data" Name="Share"
-                      SeriesType="SeriesType.Donut" XValue="@(i => i.Browser)"
-                      YValue="@(i => (decimal)i.Visitors)" />
- </PieChart>
+ <BbPieChart Data="@data" Height="300px">
+     <BbChartTooltip />
+     <BbPie DataKey="visitors" NameKey="browser" InnerRadius="60">
+         <BbCenterLabel Title="Total" Value="1,234" />
+     </BbPie>
+ </BbPieChart>
```

**RadarChart:**

```diff
- <RadarChart TItem="SkillData" Items="@data" Variant="RadarChartVariant.PolygonFill" Height="300px">
-     <ApexPointSeries TItem="SkillData" Items="@data" Name="Score"
-                      SeriesType="SeriesType.Radar" XValue="@(i => i.Skill)"
-                      YValue="@(i => (decimal)i.Score)" />
- </RadarChart>
+ <BbRadarChart Data="@data" IndicatorKey="skill" Height="300px">
+     <BbChartTooltip />
+     <BbRadar DataKey="score" Name="Score" Color="var(--chart-1)" FillOpacity="0.6" />
+ </BbRadarChart>
```

**RadialChart → RadialBarChart (renamed):**

```diff
- <RadialChart TItem="ProgressData" Items="@data" Variant="RadialChartVariant.Default"
-              CenterLabel="Progress" CenterValue="75%" Height="300px">
-     <ApexPointSeries TItem="ProgressData" Items="@data" Name="Progress"
-                      SeriesType="SeriesType.RadialBar" XValue="@(i => i.Label)"
-                      YValue="@(i => (decimal)i.Value)" />
- </RadialChart>
+ <BbRadialBarChart Data="@data" Height="300px">
+     <BbChartTooltip />
+     <BbRadialBar DataKey="value" NameKey="label" RoundCap="true">
+         <BbCenterLabel Title="Progress" Value="75%" />
+     </BbRadialBar>
+ </BbRadialBarChart>
```

#### Removed types

The following types no longer exist and any references must be removed:

- `AreaChartVariant`, `BarChartVariant`, `LineChartVariant`, `PieChartVariant`, `RadarChartVariant`, `RadialChartVariant` — variant enums replaced by composable child components
- `RadialChart` — renamed to `BbRadialBarChart`
- `ChartBase<TItem>` — replaced by non-generic `ChartBase` (internal, not used directly)

#### New composable components

These are new child components placed inside chart types to configure behavior:

| Component | Purpose |
|---|---|
| `<BbXAxis>` | Category or value axis (bottom) |
| `<BbYAxis>` | Value axis (left) |
| `<BbGrid>` | Chart area margins |
| `<BbChartTooltip>` | Hover tooltip |
| `<BbChartLegend>` | Series legend |
| `<BbLine>` | Line series |
| `<BbBar>` | Bar series |
| `<BbArea>` | Area (filled line) series |
| `<BbPie>` | Pie/donut series |
| `<BbRadar>` | Radar series |
| `<BbRadialBar>` | Radial bar series |
| `<BbCenterLabel>` | Center text for donut/radial charts |
| `<BbFill>` + `<BbLinearGradient>` + `<BbGradientStop>` | Custom gradient fills on any series |

#### Steps

1. **Remove `@using ApexCharts`** from `_Imports.razor` and any `.razor` files
2. **Remove the `Blazor-ApexCharts` NuGet package** from your project (if referenced directly)
3. **Rewrite each chart** using the new composition API — see examples above for each chart type
4. **Replace variant-based behavior** with the equivalent child components and parameters:
   - Smooth lines → `Curve="CurveType.Smooth"` on `<BbLine>` or `<BbArea>`
   - Step lines → `Curve="CurveType.Step"` on `<BbLine>` or `<BbArea>`
   - Stacked series → `Stacked="true"` on series components
   - Horizontal bars → `Horizontal="true"` on `<BbBarChart>`
   - Donut → `InnerRadius` on `<BbPie>`
   - Circle radar → `Shape="RadarShape.Circle"` on `<BbRadarChart>`
5. **Add `<BbChartTooltip />`** inside each chart if you want hover tooltips (they are no longer enabled by default via a boolean parameter)
6. **Add `<BbChartLegend />`** inside each chart if you want a legend
7. **Replace `RadialChart`** references with `BbRadialBarChart`
8. **Remove any ApexCharts CSS overrides** — ECharts renders to SVG/Canvas, not DOM elements with CSS classes

> **Note:** The `ChartConfig` / `ChartSeriesConfig` / `ChartColor` types are unchanged — custom color mapping via `Config` parameter still works the same way.

---

### UpdateTiming Default Change

The default `UpdateTiming` for text input components has changed from `Immediate` to `OnChange`. This means `ValueChanged` now fires only on blur/Enter instead of every keystroke. Event handling has moved to JavaScript for optimal performance in Blazor Server/Auto mode.

**Affected components:** `BbInput`, `BbTextarea`, `BbInputGroupInput`, `BbInputGroupTextarea`, `BbInputField<TValue>`, `BbFormFieldInput<TValue>`

**v2:**
```razor
<!-- Default was Immediate — ValueChanged fires on every keystroke -->
<Input @bind-Value="name" />
```

**v3:**
```razor
<!-- Default is now OnChange — ValueChanged fires on blur/Enter -->
<BbInput @bind-Value="name" />

<!-- To restore per-keystroke behavior, set explicitly: -->
<BbInput @bind-Value="name" UpdateTiming="UpdateTiming.Immediate" />
```

---

### Input Components: IDisposable → IAsyncDisposable

The following components now implement `IAsyncDisposable` instead of `IDisposable` due to the JavaScript-first event architecture:

`BbInput`, `BbTextarea`, `BbInputGroupInput`, `BbInputGroupTextarea`, `BbInputField<TValue>`, `BbNumericInput<TValue>`, `BbCurrencyInput`

This only affects code that explicitly casts these components to `IDisposable`. Normal usage with `@implements IAsyncDisposable` or `await using` is unaffected.

---

### IPortalService: Two-Layer Portal Architecture

The portal system has been redesigned with a two-layer architecture that splits portals into **Container** (Dialog, Sheet, AlertDialog) and **Overlay** (Popover, Select, Dropdown, Tooltip, HoverCard) categories. Each category has its own host component, so opening a tooltip no longer causes Dialog and Sheet portals to re-render.

**New enum — `PortalCategory`:**

```csharp
namespace BlazorBlueprint.Primitives;

public enum PortalCategory
{
    Overlay,   // Popover, Select, Dropdown, Tooltip, HoverCard
    Container  // Dialog, Sheet, AlertDialog
}
```

**New host components:**

| Component | Description |
|---|---|
| `BbContainerPortalHost` | Renders only Container portals (Dialog, Sheet) |
| `BbOverlayPortalHost` | Renders only Overlay portals (Popover, Select, etc.) |
| `BbCategoryPortalHost` | Core host component that accepts a `PortalCategory` parameter |

`BbPortalHost` continues to work — it now composes both category hosts internally. No layout changes required.

**`IPortalService` breaking API changes:**

```diff
  public interface IPortalService
  {
-     void RegisterPortal(string id, RenderFragment content);
+     void RegisterPortal(string id, RenderFragment content, PortalCategory category);

-     IReadOnlyList<KeyValuePair<string, RenderFragment>> GetPortals();
+     IReadOnlyList<KeyValuePair<string, RenderFragment>> GetPortals(PortalCategory category);

-     event Action? OnPortalsChanged;
+     event Action<PortalCategory>? OnPortalsCategoryChanged;

      // Unchanged: HasHost, RegisterHost(), UnregisterHost(),
      // OnPortalRendered, NotifyPortalRendered(), UnregisterPortal(),
      // UpdatePortalContent(), RefreshPortal()
  }
```

**If you have a custom `IPortalService` implementation**, update all three members:

```diff
  public class MyPortalService : IPortalService
  {
-     public void RegisterPortal(string id, RenderFragment content) { ... }
+     public void RegisterPortal(string id, RenderFragment content, PortalCategory category) { ... }

-     public IReadOnlyList<KeyValuePair<string, RenderFragment>> GetPortals() { ... }
+     public IReadOnlyList<KeyValuePair<string, RenderFragment>> GetPortals(PortalCategory category) { ... }

-     public event Action? OnPortalsChanged;
+     public event Action<PortalCategory>? OnPortalsCategoryChanged;
  }
```

**If you call `RegisterPortal` directly** (e.g., in custom portal components), add the category parameter:

```diff
- PortalService.RegisterPortal(portalId, content);
+ PortalService.RegisterPortal(portalId, content, PortalCategory.Overlay);
```

**If you subscribe to `OnPortalsChanged`**, update the event handler:

```diff
- PortalService.OnPortalsChanged += HandlePortalsChanged;
+ PortalService.OnPortalsCategoryChanged += HandlePortalsCategoryChanged;

- private void HandlePortalsChanged() { ... }
+ private void HandlePortalsCategoryChanged(PortalCategory category) { ... }
```

**If you call `GetPortals()`**, add the category parameter:

```diff
- var portals = PortalService.GetPortals();
+ var containerPortals = PortalService.GetPortals(PortalCategory.Container);
+ var overlayPortals = PortalService.GetPortals(PortalCategory.Overlay);
```

**Optional: separate hosts for category-scoped re-rendering:**

```razor
@* Option A: Single host (renders both categories — existing usage, unchanged) *@
<BbPortalHost />

@* Option B: Separate hosts (each only re-renders for its own category) *@
<BbContainerPortalHost />
<BbOverlayPortalHost />
```

---

### Localization Support

v3 introduces a built-in localization system via `BbLocalizationOptions`. All hardcoded English strings are now configurable.

#### Breaking: String parameter nullability

Some component string parameters changed from `string` to `string?`. The component resolves the effective value from `BbLocalizationOptions` when the parameter is `null`. This only affects code that relied on the non-null type (e.g., assigning to a `string` variable without null check).

**Affected components:** `BbCombobox`, `BbMultiSelect`, `BbTagInput`, `BbDatePicker`, `BbDateRangePicker`, `BbDataView`

#### Breaking: Calendar `FirstDayOfWeek` default

The `FirstDayOfWeek` parameter changed from a hardcoded `DayOfWeek.Sunday` to `CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek`. This means the calendar respects the user's culture by default.

```razor
@* Restore old behavior *@
<BbCalendar FirstDayOfWeek="DayOfWeek.Sunday" />
```

#### Breaking: Calendar month/day names from CultureInfo

Hardcoded English month and day name arrays were replaced with `CultureInfo.CurrentCulture.DateTimeFormat`. Month and day names now automatically localize. Use optional `MonthNames` and `DayNames` parameters to override.

#### Breaking: DatePicker `DateFormat` default

Changed from `"MMMM d, yyyy"` (English-only) to `"d"` (culture-aware short date pattern).

```razor
@* Restore old behavior *@
<BbDatePicker DateFormat="MMMM d, yyyy" />
```

#### Breaking: `InputConverter` uses `CurrentCulture`

`InputConverter` now uses `CultureInfo.CurrentCulture` instead of `CultureInfo.InvariantCulture` for parsing and formatting numeric values. This means locale-formatted numbers (e.g., `1.234,56` in German) parse correctly by default.

```csharp
// Restore old behavior globally
InputConverter.GlobalCulture = CultureInfo.InvariantCulture;

// Or per-instance
var converter = new InputConverter<decimal> { Culture = CultureInfo.InvariantCulture };
```

#### Configuring localization

```csharp
// Option A: Static at startup
builder.Services.AddBlazorBlueprintComponents(opts => {
    opts.DataGrid.NoResultsFound = "Keine Ergebnisse gefunden";
    opts.Calendar.GoToPreviousMonth = "Zum vorherigen Monat";
});

// Option B: Dynamic with IStringLocalizer (register as scoped)
builder.Services.AddScoped(sp => {
    var loc = sp.GetRequiredService<IStringLocalizer<SharedResources>>();
    return new BbLocalizationOptions {
        DataGrid = { NoResultsFound = loc["NoResults"] }
    };
});
```

For `Func<>` labels (dynamic strings with parameters):
```csharp
opts.DataGrid.SelectAllOnPage = n => loc["SelectAll", n];
```

---

## Setup Changes

### Service Registration (DI)

A new `AddBlazorBlueprintComponents()` extension method on `IServiceCollection` replaces the need to manually register services separately.

**v2 (manual registration):**

```csharp
using BlazorBlueprint.Primitives.Extensions;
using BlazorBlueprint.Components.Toast;

services.AddBlazorBlueprintPrimitives();
services.AddScoped<ToastService>();
```

**v3 (single call):**

```csharp
using BlazorBlueprint.Components;

services.AddBlazorBlueprintComponents();
```

`AddBlazorBlueprintComponents()` calls `AddBlazorBlueprintPrimitives()` internally and registers all component-level services (`ToastService`, `DialogService`). This is the recommended approach going forward.

**If you only use the Primitives package** (without Components), `AddBlazorBlueprintPrimitives()` remains available and unchanged.

---

### PortalHost Runtime Warning

`PortalService` now tracks whether a `BbPortalHost` component has been registered in the layout. When a portal-based component (Dialog, Select, Popover, Combobox, DropdownMenu, Tooltip, Sheet, etc.) attempts to register a portal and no `BbPortalHost` is present, a structured `LogLevel.Warning` is emitted:

```
warn: BlazorBlueprint.Primitives.Services.PortalService[1]
      BlazorBlueprint: No <BbPortalHost /> detected. Portal-based components (Dialog, Select, Popover, etc.)
      require a <BbPortalHost /> in your layout to render. Add <BbPortalHost /> to your MainLayout.razor.
```

The warning fires once per service lifetime (scoped), so it does not spam the log on every portal operation. Previously, forgetting `<BbPortalHost />` caused portal-based components to silently fail — no dropdown appeared, no dialog opened, and no error was logged.

**`IPortalService` host members:**

```csharp
public interface IPortalService
{
    bool HasHost { get; }        // Whether a PortalHost is currently registered
    void RegisterHost();         // Called by PortalHost.OnInitialized
    void UnregisterHost();       // Called by PortalHost.DisposeAsync
    // ... see "Two-Layer Portal Architecture" above for full API
}
```

**If you have a custom `IPortalService` implementation**, you must add the host members (plus the category-scoped members documented in [Two-Layer Portal Architecture](#iportalservice-two-layer-portal-architecture)):

```diff
  public class MyPortalService : IPortalService
  {
+     public bool HasHost { get; private set; }
+     public void RegisterHost() => HasHost = true;
+     public void UnregisterHost() => HasHost = false;
      // ... existing implementation
  }
```

---

## Select Ecosystem: New Options API

v3 introduces a unified data-driven API for Select, Combobox, and MultiSelect based on the `SelectOption<TValue>` record. This section covers the shared building blocks.

### SelectOption Record

A new record type for defining value/label pairs:

```csharp
// Explicit value and display text
var options = new SelectOption<string>[]
{
    new("us", "United States"),
    new("ca", "Canada")
};

// Derive display text from ToString()
var options = fruits.Select(SelectOption.FromValue).ToArray();
```

Works with any value type — `string`, `int?`, `Guid?`, enums, etc. Use nullable types (`int?`, `Guid?`) when you need a placeholder to display.

### SelectOptionExtensions

Utility methods for looking up display text from `SelectOption<TValue>` collections:

```csharp
// Look up display text for a single value
string text = _countryOptions.GetText(selectedCountry);

// Look up display texts for multiple values
IEnumerable<string> texts = _skillOptions.GetTexts(selectedSkills);
```

These are particularly useful when displaying selected values outside of the component itself (e.g., in a summary panel or confirmation dialog).

### Select: Options Mode

`Select<TValue>` now supports an `Options` parameter that auto-generates the trigger, value display, and dropdown content from a collection of `SelectOption<TValue>`.

```razor
@* v3 — data-binding mode (recommended for simple cases) *@
<BbSelect TValue="string"
          @bind-Value="_fruit"
          Options="_fruitOptions"
          Placeholder="Select a fruit" />

@code {
    private string? _fruit;

    private readonly SelectOption<string>[] _fruitOptions =
    [
        new("apple", "Apple"),
        new("banana", "Banana"),
        new("cherry", "Cherry")
    ];
}
```

The compositional mode (using `BbSelectTrigger`, `BbSelectContent`, `BbSelectItem` children) is still fully supported for cases that need custom rendering.

### FormFieldSelect: Options Mode

`FormFieldSelect<TValue>` now supports the same `Options` data-binding mode as `Select`:

```razor
<BbFormFieldSelect TValue="string"
                   @bind-Value="_formModel.Country"
                   Label="Country"
                   Placeholder="Select a country..."
                   Options="_countryOptions" />

@code {
    private readonly SelectOption<string>[] _countryOptions =
    [
        new("us", "United States"),
        new("ca", "Canada"),
        new("gb", "United Kingdom")
    ];
}
```

When `Options` is provided, the component auto-generates `BbSelectItem` children and derives `DisplayTextSelector` from the collection. You no longer need a manual lookup function.

**v2 (manual approach):**

```razor
<FormFieldSelect TValue="string"
                 @bind-Value="_country"
                 Label="Country"
                 Placeholder="Select..."
                 DisplayTextSelector="GetCountryText">
    <SelectItem Value="@("us")">United States</SelectItem>
    <SelectItem Value="@("ca")">Canada</SelectItem>
</FormFieldSelect>

@code {
    private string GetCountryText(string value) => value switch
    {
        "us" => "United States",
        "ca" => "Canada",
        _ => value
    };
}
```

**v3 (Options approach):**

```razor
<BbFormFieldSelect TValue="string"
                   @bind-Value="_country"
                   Label="Country"
                   Placeholder="Select..."
                   Options="_countryOptions" />

@code {
    private readonly SelectOption<string>[] _countryOptions =
    [
        new("us", "United States"),
        new("ca", "Canada")
    ];
}
```

The compositional approach with `ChildContent` still works for cases that need custom item rendering (avatars, icons, multi-line layouts).

---

## New Components

All new components are purely additive — no existing code needs to change.

### CommandDialog Component

A new component that wraps `Dialog` + `Command` with built-in keyboard shortcut support. Eliminates ~20 lines of boilerplate previously required to create a command palette with a keyboard shortcut.

**v2 (manual dialog + command + shortcut registration):**

```razor
@inject IKeyboardShortcutService KeyboardShortcuts
@implements IDisposable

<Dialog @bind-Open="_isOpen">
    <DialogContent Class="overflow-hidden p-0 shadow-lg">
        <Command OnValueChange="HandleSelect">
            <CommandInput Placeholder="Type a command..." AutoFocus="true" />
            <CommandList>
                <CommandEmpty>No results found.</CommandEmpty>
                <CommandGroup Heading="Actions">
                    <CommandItem Value="profile">Profile</CommandItem>
                    <CommandItem Value="settings">Settings</CommandItem>
                </CommandGroup>
            </CommandList>
        </Command>
    </DialogContent>
</Dialog>

@code {
    private bool _isOpen;
    private IDisposable? _shortcutRegistration;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _shortcutRegistration = await KeyboardShortcuts.RegisterAsync("Ctrl+K", () =>
            {
                _isOpen = !_isOpen;
                StateHasChanged();
                return Task.CompletedTask;
            });
        }
    }

    private void HandleSelect(string value)
    {
        _isOpen = false;
        // handle selection...
    }

    public void Dispose() => _shortcutRegistration?.Dispose();
}
```

**v3 (CommandDialog):**

```razor
<BbCommandDialog @bind-Open="_isOpen" Shortcut="Ctrl+K" OnValueChange="HandleSelect">
    <BbCommandInput Placeholder="Type a command..." AutoFocus="true" />
    <BbCommandList>
        <BbCommandEmpty>No results found.</BbCommandEmpty>
        <BbCommandGroup Heading="Actions">
            <BbCommandItem Value="profile">Profile</BbCommandItem>
            <BbCommandItem Value="settings">Settings</BbCommandItem>
        </BbCommandGroup>
    </BbCommandList>
</BbCommandDialog>

@code {
    private bool _isOpen;
    private void HandleSelect(string value) { /* handle selection */ }
}
```

**Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Open` / `OpenChanged` | `bool` | `false` | Controls whether the dialog is open (supports `@bind-Open`) |
| `Shortcut` | `string?` | `null` | Keyboard shortcut to toggle the dialog (e.g., `"Ctrl+K"`, `"Ctrl+Shift+P"`) |
| `OnValueChange` | `EventCallback<string>` | — | Invoked when a command item is selected |
| `CloseOnSelect` | `bool` | `true` | Whether to close the dialog when a command item is selected |
| `Class` | `string?` | `null` | Additional CSS classes for the dialog content |

---

### AvatarGroup Component

A new component that renders child `Avatar` components with overlapping negative spacing and automatic border rings.

**v2 (manual overlapping layout):**

```razor
<div class="flex -space-x-4">
    <Avatar Class="ring-2 ring-background">
        <AvatarImage Src="/user1.jpg" Alt="User 1" />
        <AvatarFallback>U1</AvatarFallback>
    </Avatar>
    <Avatar Class="ring-2 ring-background">
        <AvatarImage Src="/user2.jpg" Alt="User 2" />
        <AvatarFallback>U2</AvatarFallback>
    </Avatar>
</div>
```

**v3 (AvatarGroup):**

```razor
<BbAvatarGroup>
    <BbAvatar>
        <BbAvatarImage Src="/user1.jpg" Alt="User 1" />
        <BbAvatarFallback>U1</BbAvatarFallback>
    </BbAvatar>
    <BbAvatar>
        <BbAvatarImage Src="/user2.jpg" Alt="User 2" />
        <BbAvatarFallback>U2</BbAvatarFallback>
    </BbAvatar>
</BbAvatarGroup>
```

When avatars are nested inside `BbAvatarGroup`, a `border-2 border-background` ring is automatically added to each avatar via cascading context. The group uses `flex -space-x-3` for the overlapping layout.

---

### DrawerItem Component

A styled action button component for use inside `BbDrawerContent`, designed for mobile-friendly action sheets. When `CloseOnClick` is true (default) and the item is inside a `BbDrawer`, clicking the item automatically closes the drawer after invoking the callback.

**v2 (manual action buttons):**

```razor
<DrawerContent>
    <DrawerHeader>
        <DrawerTitle>Actions</DrawerTitle>
    </DrawerHeader>
    <div class="px-2 pb-4 space-y-1">
        <button type="button"
                class="flex w-full items-center rounded-md px-4 py-3 text-sm font-medium
                       hover:bg-accent focus-visible:outline-none focus-visible:ring-2"
                @onclick="HandleShare">
            <LucideIcon Name="share" class="mr-3 h-5 w-5" />
            Share
        </button>
    </div>
</DrawerContent>
```

**v3 (DrawerItem):**

```razor
<BbDrawerContent>
    <BbDrawerHeader>
        <BbDrawerTitle>Actions</BbDrawerTitle>
    </BbDrawerHeader>
    <div class="px-2 pb-4 space-y-1">
        <BbDrawerItem OnClick="HandleShare">
            <Icon><LucideIcon Name="share" Size="20" /></Icon>
            <ChildContent>Share</ChildContent>
        </BbDrawerItem>
        <BbDrawerItem Class="text-destructive" OnClick="HandleDelete">
            <Icon><LucideIcon Name="trash-2" Size="20" /></Icon>
            <ChildContent>Delete</ChildContent>
        </BbDrawerItem>
    </div>
</BbDrawerContent>
```

**Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Icon` | `RenderFragment?` | `null` | Icon rendered before the content with `mr-3 h-5 w-5` sizing |
| `ChildContent` | `RenderFragment?` | `null` | The action label text |
| `Class` | `string?` | `null` | Additional CSS classes (e.g., `"text-destructive"` for danger actions) |
| `Disabled` | `bool` | `false` | Whether the item is disabled |
| `OnClick` | `EventCallback` | — | Callback invoked when the item is clicked |
| `CloseOnClick` | `bool` | `true` | Whether clicking the item closes the parent BbDrawer |

---

### CheckboxGroup Component

A new component pair for managing a collection of checkboxes with shared state and optional select-all support. `BbCheckboxGroup<TValue>` manages the selected values collection via `@bind-Values` and coordinates state across all child `BbCheckboxGroupItem<TValue>` components.

**v2 (manual state management):**

```razor
<div class="space-y-3">
    <div class="flex items-center gap-2 pb-2 border-b">
        <Checkbox Checked="@(_selectedFruits.Count == _allFruits.Length)"
                  Indeterminate="@(_selectedFruits.Count > 0 && _selectedFruits.Count < _allFruits.Length)"
                  CheckedChanged="@HandleSelectAll" />
        <span class="text-sm font-medium">Select all</span>
    </div>
    @foreach (var fruit in _allFruits)
    {
        <div class="flex items-center gap-2">
            <Checkbox Checked="@_selectedFruits.Contains(fruit)"
                      CheckedChanged="@(v => ToggleFruit(fruit, v))" />
            <label class="text-sm">@fruit</label>
        </div>
    }
</div>

@code {
    private readonly string[] _allFruits = ["Apple", "Banana", "Cherry"];
    private HashSet<string> _selectedFruits = new();

    private void HandleSelectAll(bool selectAll)
    {
        _selectedFruits = selectAll
            ? new HashSet<string>(_allFruits)
            : new HashSet<string>();
    }

    private void ToggleFruit(string fruit, bool isChecked)
    {
        if (isChecked) { _selectedFruits.Add(fruit); }
        else { _selectedFruits.Remove(fruit); }
    }
}
```

**v3 (CheckboxGroup):**

```razor
<BbCheckboxGroup TValue="string"
                 @bind-Values="_selectedFruits"
                 ShowSelectAll="true"
                 SelectAllLabel="Select all">
    <BbCheckboxGroupItem Value="@("Apple")" Label="Apple" />
    <BbCheckboxGroupItem Value="@("Banana")" Label="Banana" />
    <BbCheckboxGroupItem Value="@("Cherry")" Label="Cherry" />
</BbCheckboxGroup>

@code {
    private IReadOnlyCollection<string> _selectedFruits = Array.Empty<string>();
}
```

**BbCheckboxGroup Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Values` / `ValuesChanged` | `IReadOnlyCollection<TValue>` | `[]` | The currently selected values (supports `@bind-Values`) |
| `ShowSelectAll` | `bool` | `false` | Whether to show a select-all checkbox above the items |
| `SelectAllLabel` | `string` | `"Select all"` | Label text for the select-all checkbox |
| `Disabled` | `bool` | `false` | Whether all items in the group are disabled |
| `Class` | `string?` | `null` | Additional CSS classes for the group container |

**BbCheckboxGroupItem Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Value` | `TValue` | — | **(Required)** The value this item represents |
| `Label` | `string?` | `null` | Optional label displayed next to the checkbox |
| `Disabled` | `bool` | `false` | Whether this individual item is disabled |
| `Class` | `string?` | `null` | Additional CSS classes |
| `ChildContent` | `RenderFragment?` | `null` | Custom content instead of `Label` |

---

### DialogService (Programmatic Confirm Dialogs)

A new `DialogService` with a `Confirm()` method provides programmatic confirm dialogs as a single async call. This follows the same service/provider pattern as `ToastService`/`BbToastProvider`.

**v2 (manual AlertDialog for every confirmation flow):**

```razor
<AlertDialog @bind-Open="_showDeleteDialog">
    <AlertDialogTrigger>
        <Button Variant="ButtonVariant.Destructive">Delete Item</Button>
    </AlertDialogTrigger>
    <AlertDialogContent>
        <AlertDialogHeader>
            <AlertDialogTitle>Are you sure?</AlertDialogTitle>
            <AlertDialogDescription>This action cannot be undone.</AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction OnClick="HandleDelete">Delete</AlertDialogAction>
        </AlertDialogFooter>
    </AlertDialogContent>
</AlertDialog>

@code {
    private bool _showDeleteDialog;

    private async Task HandleDelete(MouseEventArgs e)
    {
        // perform delete
    }
}
```

**v3 (DialogService.Confirm):**

```razor
@inject DialogService DialogService

<BbButton Variant="ButtonVariant.Destructive" OnClick="HandleDelete">Delete Item</BbButton>

@code {
    private async Task HandleDelete(MouseEventArgs e)
    {
        var confirmed = await DialogService.Confirm(
            "Are you sure?",
            "This action cannot be undone.",
            new ConfirmDialogOptions { Destructive = true, ConfirmText = "Delete" });

        if (confirmed)
        {
            // perform delete
        }
    }
}
```

**Setup:**

1. **Service registration** — `DialogService` is automatically registered when you call `AddBlazorBlueprintComponents()`:

```csharp
services.AddBlazorBlueprintComponents(); // registers DialogService, ToastService, and all primitives
```

2. **Provider component** — add `<BbDialogProvider />` to your root layout alongside `<BbToastProvider />` and `<BbPortalHost />`:

```razor
@* MainLayout.razor *@
<BbToastProvider />
<BbDialogProvider />
<BbPortalHost />
```

**`ConfirmDialogOptions` parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ConfirmText` | `string` | `"Continue"` | Text for the confirm button |
| `CancelText` | `string` | `"Cancel"` | Text for the cancel button |
| `Destructive` | `bool` | `false` | When true, confirm button uses `ButtonVariant.Destructive` |

**When to use `AlertDialog` vs `DialogService`:**
- Use `DialogService.Confirm()` for simple yes/no confirmations — it's fewer lines and keeps UI logic in the handler method.
- Use the declarative `AlertDialog` component when you need custom content inside the dialog (form fields, rich layouts, multi-step flows).

---

### ResponsiveNav: Shared Item Definition

A new data-driven approach for responsive navigation that eliminates the need to define navigation links twice — once for desktop (`NavigationMenu`) and once for mobile (`ResponsiveNavContent`).

`NavItemDefinition` is a model class for defining navigation items:

```csharp
public class NavItemDefinition
{
    public required string Text { get; init; }
    public string? Href { get; init; }
    public IReadOnlyList<NavItemDefinition>? Children { get; init; }
    public string? Description { get; init; }
    public NavLinkMatch Match { get; init; } = NavLinkMatch.Prefix;
}
```

`ResponsiveNavItems` is a single component that renders **both** views from one `Items` collection:
- **Desktop** — `BbNavigationMenu` with `BbNavigationMenuList > BbNavigationMenuLink` for flat items, and `BbNavigationMenuTrigger + BbNavigationMenuContent` (grid with descriptions) for items with `Children`. Hidden on mobile via CSS.
- **Mobile** — `BbResponsiveNavContent` wrapping a `<nav>` with `NavLink` elements for flat items and category headers with indented child links for items with `Children`.

**v2 (duplicate navigation definitions):**

```razor
<ResponsiveNavProvider>
    <header class="flex items-center justify-between p-4 border-b">
        <a href="/" class="font-bold">My App</a>
        <ResponsiveNavTrigger />

        <!-- Desktop navigation -->
        <NavigationMenu Class="hidden md:flex">
            <NavigationMenuList>
                <NavigationMenuItem>
                    <NavigationMenuLink Href="/docs">Documentation</NavigationMenuLink>
                </NavigationMenuItem>
                <NavigationMenuItem>
                    <NavigationMenuLink Href="/primitives">Primitives</NavigationMenuLink>
                </NavigationMenuItem>
            </NavigationMenuList>
        </NavigationMenu>
    </header>

    <!-- Mobile navigation (same links, different markup) -->
    <ResponsiveNavContent>
        <nav class="flex flex-col space-y-4">
            <a href="/docs" class="text-lg font-medium hover:text-primary">Documentation</a>
            <a href="/primitives" class="text-lg font-medium hover:text-primary">Primitives</a>
        </nav>
    </ResponsiveNavContent>
</ResponsiveNavProvider>
```

**v3 (single data definition):**

```razor
<BbResponsiveNavProvider>
    <header class="flex items-center justify-between p-4 border-b">
        <a href="/" class="font-bold">My App</a>
        <BbResponsiveNavTrigger />
        <BbResponsiveNavItems Items="@navItems" />
    </header>
</BbResponsiveNavProvider>

@code {
    private static readonly NavItemDefinition[] navItems =
    [
        new() { Text = "Documentation", Href = "/docs" },
        new()
        {
            Text = "Components",
            Children =
            [
                new() { Text = "Button", Href = "/components/button",
                        Description = "Interactive buttons." },
                new() { Text = "Dialog", Href = "/components/dialog",
                        Description = "Modal dialogs." }
            ]
        },
        new() { Text = "Primitives", Href = "/primitives" }
    ];
}
```

**Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Items` | `IReadOnlyList<NavItemDefinition>` | `[]` | The shared navigation items |
| `DesktopClass` | `string?` | `null` | Additional CSS classes for the desktop `BbNavigationMenu` |
| `MobileClass` | `string?` | `null` | Additional CSS classes for the mobile `<nav>` element |
| `MobileHeader` | `RenderFragment?` | `null` | Optional header for the mobile sheet |
| `MobileFooter` | `RenderFragment?` | `null` | Optional footer for the mobile sheet |
| `Side` | `SheetSide` | `Left` | Which side the mobile sheet slides in from |

**Design notes:**
- One level of nesting is supported (matches the `BbNavigationMenu` dropdown pattern).
- Both approaches coexist — `BbResponsiveNavItems` is additive, not a replacement.
- Mobile uses Blazor `NavLink` (not raw `<a>`) for active-state highlighting.

---

## New Parameters

All parameters in this section are purely additive — existing code continues to work unchanged. Each one replaces a common boilerplate pattern with a single parameter.

### RadioGroupItem: Label

`RadioGroupItem` now accepts an optional `Label` string parameter that renders a wrapper `<div>` with an associated `<label>` element. The component handles `for`/`id` wiring automatically. When `Label` is null, the component renders identically to v2 (backwards compatible).

```diff
- <div class="flex items-center gap-3">
-     <RadioGroupItem Value="@("option1")" Id="r1" />
-     <label for="r1" class="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">
-         Option 1
-     </label>
- </div>
+ <BbRadioGroupItem Value="@("option1")" Label="Option 1" />
```

---

### Button: Loading State

`Button` now supports a built-in loading state with `Loading`, `LoadingText`, and `LoadingTemplate` parameters. When `Loading` is true, the button is disabled and shows a spinner.

```diff
- <Button Disabled="@_isLoading" OnClick="HandleSubmit">
-     @if (_isLoading)
-     {
-         <svg class="animate-spin h-4 w-4 mr-2">...</svg>
-         <span>Saving...</span>
-     }
-     else
-     {
-         <span>Save Changes</span>
-     }
- </Button>
+ <BbButton Loading="@_isLoading" LoadingText="Saving..." OnClick="HandleSubmit">
+     Save Changes
+ </BbButton>
```

For custom loading indicators (e.g., branded spinners, progress rings), use `LoadingTemplate`:

```razor
<BbButton Loading="@_isLoading" OnClick="HandleSubmit">
    <LoadingTemplate>
        <MyCustomSpinner />
    </LoadingTemplate>
    <ChildContent>Save Changes</ChildContent>
</BbButton>
```

---

### Alert: Dismissible

`Alert` now supports built-in dismiss functionality with `Dismissible` and `OnDismiss` parameters. When `Dismissible` is true, a close button (X icon) appears in the top-right corner.

```diff
  <BbAlert Variant="AlertVariant.Info"
+        Dismissible="true"
+        OnDismiss="@(() => _showAlert = false)">
      <BbAlertTitle>Info</BbAlertTitle>
      <BbAlertDescription>Important message here.</BbAlertDescription>
-     <button class="absolute right-4 top-4 ..." @onclick="@(() => _showAlert = false)">
-         <LucideIcon Name="x" Class="h-4 w-4" />
-         <span class="sr-only">Close</span>
-     </button>
  </BbAlert>
```

---

### Textarea: ShowCharacterCount

When `ShowCharacterCount` is true and `MaxLength` is set, a `{current}/{max}` counter is displayed below the textarea. When `MaxLength` is not set, only the current count is shown.

```diff
- <Textarea MaxLength="280" Placeholder="What's happening?"
-           Value="@_text" ValueChanged="@(v => _text = v)" />
- <p class="text-xs text-muted-foreground">
-     @(_text?.Length ?? 0) / 280 characters
- </p>
+ <BbTextarea ShowCharacterCount="true" MaxLength="280"
+           Placeholder="What's happening?" @bind-Value="_text" />
```

---

### Slider: TrackClass and ThumbClass

`Slider` now exposes `TrackClass` and `ThumbClass` parameters for styling the track and thumb sub-parts directly.

```diff
  <BbSlider @bind-Value="_value"
-         Class="[&_.bg-primary]:bg-emerald-500 [&_div[class*='border-primary']]:border-emerald-500" />
+         TrackClass="bg-emerald-200 dark:bg-emerald-900"
+         ThumbClass="border-emerald-500" />
```

---

### InputOTP: InputClass

`InputOTP` now exposes an `InputClass` parameter for styling individual OTP input boxes directly.

```diff
  <BbInputOTP Length="4" ShowSeparator="false"
-           Class="[&_input]:border-primary/50 [&_input]:bg-primary/5" />
+           InputClass="border-blue-500 focus:ring-blue-500" />
```

---

### Progress: ShowLabel and LabelFormat

When `ShowLabel` is true, a percentage label is rendered next to the progress bar. `LabelFormat` (default: `"{0}%"`) allows custom format strings.

```diff
- <div class="flex justify-between text-sm">
-     <span>Progress</span>
-     <span>60%</span>
- </div>
- <Progress Value="60" />
+ <BbProgress Value="60" ShowLabel="true" />
+ <BbProgress Value="3" Max="5" ShowLabel="true" LabelFormat="{0}% done" />
```

---

### Skeleton: Width and Height

`Skeleton` now supports explicit sizing via `Width` and `Height` parameters, applied as inline styles.

```diff
- <Skeleton Class="h-[200px] w-[250px]" />
+ <BbSkeleton Width="250px" Height="200px" />
```

Both approaches still work — `Width`/`Height` are inline styles while `Class` uses Tailwind utilities.

---

### MarkdownEditor: MinHeight

`MarkdownEditor` now accepts a `MinHeight` parameter that sets the minimum height of the textarea and preview areas via inline style. Defaults to `150px` when not set.

```diff
- <MarkdownEditor @bind-Value="_content" class="min-h-[200px]" />
+ <BbMarkdownEditor @bind-Value="_content" MinHeight="200px" />
```

---

### ColorPicker: DefaultValue

`ColorPicker` now supports uncontrolled usage with the `DefaultValue` parameter. When set, the picker initializes with the specified color without requiring external `@bind-Value` binding.

```diff
- <ColorPicker @bind-Value="_color" />
- @code { private string _color = "#8B5CF6"; }
+ <BbColorPicker DefaultValue="#8B5CF6" />
```

Use `DefaultValue` when you want the picker to start at a specific color but don't need to track the value externally. Use `@bind-Value` when you need two-way binding.

---

### TooltipContent: ShowArrow

`TooltipContent` now accepts a `ShowArrow` boolean parameter that renders a CSS arrow pointing toward the trigger element. The arrow is automatically positioned based on the tooltip's `Side`.

```diff
  <BbTooltipContent
-     Class="overflow-visible after:content-[''] after:absolute after:left-1/2
-         after:-translate-x-1/2 after:top-full after:border-[6px]
-         after:border-transparent after:border-t-primary">
+     ShowArrow="true">
      Tooltip with arrow
  </BbTooltipContent>
```

---

### Menu Items: Icon

`DropdownMenuItem`, `ContextMenuItem`, and `CommandItem` now accept an optional `Icon` render fragment that renders an icon before the item content with automatic sizing (`mr-2 h-4 w-4`).

```diff
  <BbDropdownMenuItem>
-     <LucideIcon Name="pencil" Class="mr-2 h-4 w-4" />
-     Edit
+     <Icon><LucideIcon Name="pencil" Size="16" /></Icon>
+     <ChildContent>Edit</ChildContent>
  </BbDropdownMenuItem>
```

---

### BreadcrumbList: AutoSeparator

When `AutoSeparator` is true, `BreadcrumbSeparator` components are automatically inserted between each `BreadcrumbItem`. The `SeparatorContent` parameter allows a custom separator.

```diff
- <BreadcrumbList>
+ <BbBreadcrumbList AutoSeparator="true">
      <BbBreadcrumbItem>
          <BbBreadcrumbLink Href="/">Home</BbBreadcrumbLink>
      </BbBreadcrumbItem>
-     <BreadcrumbSeparator />
      <BbBreadcrumbItem>
          <BbBreadcrumbLink Href="/components">Components</BbBreadcrumbLink>
      </BbBreadcrumbItem>
-     <BreadcrumbSeparator />
      <BbBreadcrumbItem>
          <BbBreadcrumbPage>Breadcrumb</BbBreadcrumbPage>
      </BbBreadcrumbItem>
  </BbBreadcrumbList>
```

For a custom separator:

```razor
<BbBreadcrumbList AutoSeparator="true">
    <SeparatorContent>/</SeparatorContent>
    <ChildContent>
        <BbBreadcrumbItem>
            <BbBreadcrumbLink Href="/">Home</BbBreadcrumbLink>
        </BbBreadcrumbItem>
        <BbBreadcrumbItem>
            <BbBreadcrumbPage>Current</BbBreadcrumbPage>
        </BbBreadcrumbItem>
    </ChildContent>
</BbBreadcrumbList>
```

---

### TimelineItem: Shorthand Parameters

`TimelineItem` now supports `Title`, `Time`, and `Description` shorthand parameters that auto-render the full sub-component tree when `ChildContent` is null.

```diff
- <TimelineItem>
-     <ChildContent>
-         <TimelineContent>
-             <TimelineHeader>
-                 <TimelineTitle>Design Phase</TimelineTitle>
-                 <TimelineTime>Mar 2024</TimelineTime>
-             </TimelineHeader>
-             <TimelineDescription>Completed wireframes.</TimelineDescription>
-         </TimelineContent>
-     </ChildContent>
- </TimelineItem>
+ <BbTimelineItem Title="Design Phase" Time="Mar 2024"
+               Description="Completed wireframes." />
```

The full compositional mode is still supported for cases that need custom rendering (icons, collapsible details, etc.).

---

### Pagination: Shorthand Parameters

When `TotalPages` is set and `ChildContent` is null, `Pagination` auto-renders the standard prev/pages/next layout with smart ellipsis for large page counts.

```diff
- <Pagination State="@_state" OnPageChanged="HandlePageChanged">
-     <PaginationContent>
-         <PaginationItem><PaginationPrevious /></PaginationItem>
-         @for (var i = 1; i <= _totalPages; i++)
-         {
-             var page = i;
-             <PaginationItem>
-                 <PaginationLink IsActive="@(_currentPage == page)" OnClick="@(() => GoToPage(page))">
-                     @page
-                 </PaginationLink>
-             </PaginationItem>
-         }
-         <PaginationItem><PaginationNext /></PaginationItem>
-     </PaginationContent>
- </Pagination>
+ <BbPagination TotalPages="@_totalPages" @bind-CurrentPage="_currentPage" />
```

The full compositional mode is still supported for custom layouts.

---

### Menubar: TriggerClass

`Menubar` now accepts a `TriggerClass` parameter that cascades additional CSS classes to all `MenubarTrigger` components within the menubar.

```diff
- <Menubar>
+ <BbMenubar TriggerClass="hover:bg-sidebar-accent hover:text-sidebar-accent-foreground">
      <BbMenubarMenu>
-         <MenubarTrigger Class="hover:bg-sidebar-accent hover:text-sidebar-accent-foreground">File</MenubarTrigger>
+         <BbMenubarTrigger>File</MenubarTrigger>
          <BbMenubarContent>...</BbMenubarContent>
      </BbMenubarMenu>
      <BbMenubarMenu>
-         <MenubarTrigger Class="hover:bg-sidebar-accent hover:text-sidebar-accent-foreground">Edit</MenubarTrigger>
+         <BbMenubarTrigger>Edit</MenubarTrigger>
          <BbMenubarContent>...</BbMenubarContent>
      </BbMenubarMenu>
  </BbMenubar>
```

`TriggerClass` is merged with each trigger's own `Class` via `ClassNames.cn()`, so individual triggers can still override or extend the shared styles.

---

### MenubarItem: Shortcut

`MenubarItem` now accepts a `Shortcut` string parameter (e.g., `"Ctrl+Shift+T"`, `"Ctrl+Z"`) that automatically registers a global keyboard shortcut via `IKeyboardShortcutService`. When the shortcut is pressed — even when the menu is closed — the item's `OnClick` callback is invoked. Registration and disposal are handled automatically.

```diff
- @inject IKeyboardShortcutService KeyboardShortcuts
- @implements IAsyncDisposable

  <BbMenubarItem
+     Shortcut="Ctrl+Shift+T"
      OnClick="@(() => HandleAction("New Tab"))">
      New Tab
      <BbMenubarShortcut>Ctrl+Shift+T</BbMenubarShortcut>
  </BbMenubarItem>

- @code {
-     private IDisposable? _shortcutNewTab;
-
-     protected override async Task OnAfterRenderAsync(bool firstRender)
-     {
-         if (firstRender)
-         {
-             _shortcutNewTab = await KeyboardShortcuts.RegisterAsync(
-                 "Ctrl+Shift+T", () => { HandleAction("New Tab"); return Task.CompletedTask; });
-         }
-     }
-
-     public ValueTask DisposeAsync()
-     {
-         _shortcutNewTab?.Dispose();
-         return ValueTask.CompletedTask;
-     }
- }
```

The `Shortcut` parameter handles registration, the `Disabled` guard, and disposal automatically. `MenubarShortcut` continues to render the visual shortcut hint — the two are independent (one registers the handler, the other displays the hint text).

---

### Avatar: ShowDot and DotClass

`Avatar` now supports a built-in dot indicator for status display. When `ShowDot` is true, a small circular dot is rendered at the bottom-right corner. The dot size scales automatically with the avatar's `Size` parameter. Appearance is controlled via `DotClass` (e.g., `"bg-green-500"` for online). Defaults to `"bg-primary"` when `DotClass` is not specified.

```diff
- <div class="relative inline-block">
-     <Avatar>
-         <AvatarImage Src="/avatar.jpg" Alt="User" />
-         <AvatarFallback>JD</AvatarFallback>
-     </Avatar>
-     <span class="absolute bottom-0 right-0 block h-2.5 w-2.5 rounded-full bg-green-500 ring-2 ring-background"></span>
- </div>
+ <BbAvatar ShowDot="true" DotClass="bg-green-500">
+     <BbAvatarImage Src="/avatar.jpg" Alt="User" />
+     <BbAvatarFallback>JD</BbAvatarFallback>
+ </BbAvatar>
```

---

### Badge: ShowDot, DotPosition, DotClass

`Badge` now supports a built-in notification dot. When `ShowDot` is true, a small circular dot is rendered on the badge. `DotPosition` (`BadgeDotPosition` enum) controls placement. `DotClass` overrides the dot color (defaults to `"bg-primary"`).

```diff
- <span class="relative inline-flex">
-     <Badge Variant="BadgeVariant.Secondary">Messages</Badge>
-     <span class="absolute -top-1 -right-1 block h-2 w-2 rounded-full bg-red-500 ring-2 ring-background"></span>
- </span>
+ <BbBadge Variant="BadgeVariant.Secondary" ShowDot="true" DotClass="bg-red-500">
+     Messages
+ </BbBadge>
```

**Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ShowDot` | `bool` | `false` | Whether to show the notification dot |
| `DotPosition` | `BadgeDotPosition` | `TopRight` | Position of the dot relative to the badge |
| `DotClass` | `string?` | `null` | Custom CSS classes for the dot (defaults to `bg-primary`) |

---

### DropdownMenuItem: Href and Target

Both the primitive and styled `DropdownMenuItem` now accept `Href` and `Target` parameters. When `Href` is set, the item renders as an `<a>` element instead of a `<div>`, with proper link semantics.

```razor
@* Internal navigation *@
<BbDropdownMenuItem Href="/settings">Settings</BbDropdownMenuItem>

@* External link in new tab *@
<BbDropdownMenuItem Href="https://github.com" Target="_blank">
    GitHub
</BbDropdownMenuItem>
```

When `Target="_blank"`, `rel="noopener noreferrer"` is automatically added. When `Disabled` is true and `Href` is set, the `href` attribute is nullified and `aria-disabled="true"` is applied.

---

### Field: Cascading IsInvalid

`Field` now cascades its `IsInvalid` state to child input components via a named `CascadingValue`. When `IsInvalid` is true on a `Field`, all child inputs automatically receive `aria-invalid="true"`.

**Affected inputs:** `Input`, `Textarea`, `InputGroupInput`, `InputGroupTextarea`, `NumericInput`, `CurrencyInput`, `MaskedInput`

**Priority chain:**

1. **Explicit `AriaInvalid` parameter** — highest priority, always respected as a user override
2. **Cascaded `FieldIsInvalid`** from parent `Field` — applies when `AriaInvalid` is not explicitly set
3. **`IsInvalid` from EditContext** — lowest priority, from form validation

```diff
  <BbField IsInvalid="true">
      <BbFieldLabel For="username">Username</BbFieldLabel>
      <BbFieldContent>
-         <Input Id="username" AriaInvalid="true" Class="border-destructive" />
+         <BbInput Id="username" />
          <BbFieldError>Username is already taken.</BbFieldError>
      </BbFieldContent>
  </BbField>
```

---

### DataTableColumn: Property No Longer EditorRequired

`DataTableColumn.Property` is no longer marked with `[EditorRequired]`. When `CellTemplate` is provided, `Property` can be omitted entirely. This is useful for action columns (edit/delete buttons) that don't map to a data property.

```diff
- <DataTableColumn TData="Person" TValue="object" Property="@(p => p)" Header="Actions">
+ <BbDataTableColumn TData="Person" TValue="object" Header="Actions">
      <CellTemplate Context="person">
          <BbButton Size="ButtonSize.Small" OnClick="@(() => Edit(person))">Edit</BbButton>
      </CellTemplate>
  </BbDataTableColumn>
```

When `Property` is null, the column is automatically excluded from sorting and global search.

---

### Toast: Semantic Variants, Size, Icons, Pause-on-Hover, Per-Toast Position

The toast component has been significantly enhanced with 5 new features:

**New `ToastVariant` values:** `Success`, `Info`, `Warning` join `Default` and `Destructive`. Each semantic variant uses the existing alert CSS custom properties for consistent theming.

**`ToastService.Success()` visual change:** Previously mapped to `ToastVariant.Default` (neutral). Now uses `ToastVariant.Success` (green accent with circle-check icon). To restore old neutral behavior, use `ToastService.Show()` directly.

```diff
- ToastService.Success("Saved.");  // was Default variant (neutral)
+ ToastService.Success("Saved.");  // now Success variant (green accent + icon)
+ ToastService.Info("New version available.");     // new
+ ToastService.Warning("Session expiring.");       // new
```

**`ToastSize` enum:** `Default` (standard padding) or `Compact` (reduced padding/font sizes for dialog-friendly contexts).

```csharp
ToastService.Show(new ToastData
{
    Description = "Compact toast.",
    Size = ToastSize.Compact,
    Variant = ToastVariant.Success
});
```

**Auto variant icons:** Semantic variants automatically display icons: Success → `circle-check`, Info → `info`, Warning → `triangle-alert`, Destructive → `circle-x`. Set `ShowIcon = false` to hide.

**Pause-on-hover:** `BbToastProvider.PauseOnHover` (default: `true`) pauses the auto-dismiss timer when the mouse hovers over a toast.

```razor
@* Enabled by default — no changes needed *@
<BbToastProvider Position="ToastPosition.BottomRight" />

@* To disable pause-on-hover: *@
<BbToastProvider Position="ToastPosition.BottomRight" PauseOnHover="false" />
```

**Per-toast position:** Individual toasts can override the provider's position via `ToastData.Position`. Toasts with custom positions render in separate positioned containers.

```csharp
ToastService.Show(new ToastData
{
    Description = "Appears in top-left corner.",
    Position = ToastPosition.TopLeft
});
```

**`BbToastProvider` lifecycle change:** Changed from `IDisposable` to `IAsyncDisposable`. This only affects consumers who programmatically create/dispose the provider (extremely rare).

---

### Input Components: Auto-Generated IDs

All input components now automatically generate a stable HTML `id` attribute when none is provided. This eliminates boilerplate when wiring `<label for="...">` to inputs.

**Affected components:** `BbInput`, `BbTextarea`, `BbNumericInput<TValue>`, `BbCurrencyInput`, `BbMaskedInput`, `BbInputField<TValue>`, `BbInputGroupInput`, `BbInputGroupTextarea`, `BbCheckbox`, `BbInputOTP`, `BbNativeSelect<TValue>`.

**Before (v2):**
```razor
@code { private string emailId = $"email-{Guid.NewGuid():N}"; }

<label for="@emailId">Email</label>
<BbInput Id="@emailId" Type="InputType.Email" @bind-Value="email" />
```

**After (v3):**
```razor
<BbInput @ref="emailInput" Type="InputType.Email" @bind-Value="email" />
@* id is auto-generated (e.g., "input-a3f7c2d4") — use browser DevTools to see it *@
```

When `Id` is not specified, the component generates an ID like `input-a3f7c2d4` (prefix varies by component type). The generated ID is stable for the component's lifetime. User-provided `Id` always takes precedence.

Additionally, `BbInputOTP` and `BbNativeSelect<TValue>` gain a new `Id` parameter that was previously missing entirely.

---

### ScrollArea: FillContainer

`BbScrollArea` gains a `FillContainer` parameter (default: `false`). When `true`, the scroll area automatically fills the remaining vertical space in a flex column parent container, excluding sibling elements like headers and footers. This eliminates manual `Height` or `MaxHeight` calculations for common layouts.

```razor
<div class="h-screen flex flex-col">
    <header class="h-16 border-b shrink-0">Header</header>
    <BbScrollArea FillContainer="true">
        @* Content automatically fills remaining space and scrolls *@
    </BbScrollArea>
    <footer class="h-12 border-t shrink-0">Footer</footer>
</div>
```

When `FillContainer` is `true`, it takes precedence over `Height` and `MaxHeight` parameters. The parent element must use `display: flex; flex-direction: column` with a constrained height (e.g., `h-screen`, `h-full`, or a fixed `h-[400px]`).

---

### SidebarInset: ResetScrollOnNavigation

`BbSidebarInset` gains a `ResetScrollOnNavigation` parameter (default: `true`). When `true`, the inset content area automatically scrolls to the top whenever the route changes, improving SPA navigation UX.

```razor
<BbSidebarInset>
    @* Scrolls to top on every navigation (default behavior) *@
</BbSidebarInset>
```

To disable automatic scroll reset:

```razor
<BbSidebarInset ResetScrollOnNavigation="false">
    @* Preserves scroll position across navigations *@
</BbSidebarInset>
```

---

## Primitive Layer Improvements

These changes are most relevant if you use `BlazorBlueprint.Primitives` directly to build custom components. The styled Components layer already uses these improvements internally.

### SwitchThumb Sub-Component

A new `SwitchThumb` sub-component automatically syncs the `data-state` attribute (`"checked"` or `"unchecked"`) from the parent `Switch` via a `CascadingParameter`.

```diff
  <BbSwitch @bind-Checked="isEnabled" class="relative h-6 w-11 rounded-full bg-input ...">
-     <span class="pointer-events-none block h-5 w-5 rounded-full bg-background shadow-lg ..."
-           data-state="@(isEnabled ? "checked" : "unchecked")" />
+     <BbSwitchThumb class="pointer-events-none block h-5 w-5 rounded-full bg-background shadow-lg ..." />
  </BbSwitch>
```

---

### CheckboxIndicator Sub-Component

A new `CheckboxIndicator` sub-component automatically renders the appropriate check (polyline) or indeterminate (line) SVG icon based on the parent `Checkbox` state.

```diff
  <BbCheckbox @bind-Checked="isChecked" class="h-5 w-5 rounded border ...">
-     @if (isChecked)
-     {
-         <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
-              fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round"
-              stroke-linejoin="round" class="text-current">
-             <polyline points="20 6 9 17 4 12"></polyline>
-         </svg>
-     }
+     <BbCheckboxIndicator />
  </BbCheckbox>
```

**Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Custom content to render instead of the default SVG icons |
| `Size` | `int` | `14` | Size of the default SVG icons in pixels |
| `StrokeWidth` | `int` | `3` | Stroke width of the default SVG icons |

---

### ItemClass: Parent-Level Item Styling

Three primitive components now accept an `ItemClass` parameter that cascades default CSS classes to all child items:

- `DropdownMenu` → `DropdownMenuItem`
- `RadioGroup<TValue>` → `RadioGroupItem<TValue>`
- `Select<TValue>` → `SelectItem<TValue>`

Items merge `ItemClass` with their own `class` attribute. Per-item `class` values are appended after `ItemClass`, so they can override or extend the shared styles.

```diff
- <DropdownMenu>
+ <BbDropdownMenu ItemClass="px-2 py-1.5 cursor-pointer rounded hover:bg-accent">
      <BbDropdownMenuContent>
-         <DropdownMenuItem class="px-2 py-1.5 cursor-pointer rounded hover:bg-accent">Cut</BbDropdownMenuItem>
-         <DropdownMenuItem class="px-2 py-1.5 cursor-pointer rounded hover:bg-accent">Copy</BbDropdownMenuItem>
-         <DropdownMenuItem class="px-2 py-1.5 cursor-pointer rounded hover:bg-accent">Paste</BbDropdownMenuItem>
+         <BbDropdownMenuItem>Cut</BbDropdownMenuItem>
+         <BbDropdownMenuItem>Copy</BbDropdownMenuItem>
+         <BbDropdownMenuItem>Paste</BbDropdownMenuItem>
      </BbDropdownMenuContent>
  </BbDropdownMenu>
```

---

### CascadingTypeParameter

The `[CascadingTypeParameter(nameof(TValue))]` attribute has been added to `Select<TValue>`, `CheckboxGroup<TValue>`, and `ToggleGroup<TValue>`. Child components can now infer `TValue` from the parent without explicitly specifying it.

**Select example:**

```diff
  <BbSelect TValue="string" @bind-Value="selectedFruit">
-     <SelectTrigger TValue="string" class="...">
-         <SelectValue TValue="string" Placeholder="Select a fruit" />
+     <BbSelectTrigger class="...">
+         <BbSelectValue Placeholder="Select a fruit" />
      </BbSelectTrigger>
-     <SelectContent TValue="string" class="...">
-         <SelectItem TValue="string" Value="apple" Text="Apple" />
-         <SelectItem TValue="string" Value="banana" Text="Banana" />
+     <BbSelectContent class="...">
+         <BbSelectItem Value="apple" Text="Apple" />
+         <BbSelectItem Value="banana" Text="Banana" />
      </BbSelectContent>
  </BbSelect>
```

**CheckboxGroup example:**

```diff
  <BbCheckboxGroup TValue="string" @bind-Values="_selected">
-     <CheckboxGroupItem TValue="string" Value="@("Apple")" Label="Apple" />
-     <CheckboxGroupItem TValue="string" Value="@("Banana")" Label="Banana" />
+     <BbCheckboxGroupItem Value="@("Apple")" Label="Apple" />
+     <BbCheckboxGroupItem Value="@("Banana")" Label="Banana" />
  </BbCheckboxGroup>
```

**ToggleGroup example:**

```diff
  <BbToggleGroup TValue="string" @bind-Value="_alignment" Type="ToggleGroupType.Single">
-     <ToggleGroupItem TValue="string" Value="@("left")">Left</BbToggleGroupItem>
-     <ToggleGroupItem TValue="string" Value="@("center")">Center</BbToggleGroupItem>
-     <ToggleGroupItem TValue="string" Value="@("right")">Right</BbToggleGroupItem>
+     <BbToggleGroupItem Value="@("left")">Left</BbToggleGroupItem>
+     <BbToggleGroupItem Value="@("center")">Center</BbToggleGroupItem>
+     <BbToggleGroupItem Value="@("right")">Right</BbToggleGroupItem>
  </BbToggleGroup>
```

Only the root parent component needs the type parameter — all children inherit it.

---

### FloatingPortal: ForceMount

`BbFloatingPortal` now keeps portal content mounted in the DOM when closed (`ForceMount` defaults to `true`). Content is hidden via CSS when closed and repositioned when re-opened. A `data-state` attribute (`"open"` / `"closed"`) on the portal content div enables CSS exit animations.

This affects all floating overlay components: Select, Popover, Tooltip, HoverCard, DropdownMenu. Items are always registered in the portal, so Select display text resolves immediately without workarounds.

**Benefits:**
- No re-mounting overhead on each open/close cycle
- Select items register immediately — display text resolves without cache hacks
- `data-state` attribute enables CSS exit animations (`data-[state=closed]:animate-out`)
- Smoother open/close UX

**Removed (internal, non-breaking):**
- Hidden cache-seeding render (`<div style="display:none">`) from `BbSelectContent` — items are always mounted in the portal via ForceMount.
- `displayTextCache` dictionary, `GetCachedDisplayText()`, and `ClearItems()` from `SelectContext` — items stay registered across open/close cycles, so persistent caching and re-registration are unnecessary.

No action required — this is a non-breaking internal improvement.

---

### ISelectDisplayContext Interface

A non-generic interface for components that need display text without knowing `TValue`:

```csharp
public interface ISelectDisplayContext
{
    string? DisplayText { get; }
    event Action? OnStateChanged;
}
```

`SelectContext<TValue>` implements this interface. `SelectValue` now uses it instead of dynamic typing.

---

## Visual Changes

These are visual-only updates with no API impact.

### Checkbox Proportions

The Checkbox component's visual proportions have been updated:

- Size increased from `h-4 w-4` (16px) to `h-5 w-5` (20px)
- SVG check and indeterminate icons reduced from 16px to 14px
- SVG `stroke-width` increased from 2 to 3

The checkbox will appear slightly larger and bolder. If you have custom CSS that depends on the checkbox being exactly 16px, you may need to adjust.

### RadioGroupItem Filled Circle Design

The RadioGroupItem's visual style has been updated to use a filled circle design:

- **Before:** Thin `border` with `border-primary` always applied, `bg-current` inner dot
- **After:** `border-2` with conditional `border-primary bg-primary` when checked (solid fill), `bg-background` inner dot (white on primary background)

The selected state now appears as a filled primary-color circle with a white dot in the center, providing better visual contrast. If you have custom CSS targeting the radio button's specific border or background classes, you may need to update those selectors.

---

## Bug Fixes

These are non-breaking fixes included in v3. No action required.

- **File input crash:** `<Input Type="InputType.File">` no longer throws `InvalidStateError` when a file is selected. If you had a workaround for this crash, you can remove it.
- **Sidebar mobile flash:** The desktop sidebar now uses `hidden md:flex` to prevent a brief flash on mobile screens. If you had custom CSS to hide the sidebar on mobile, you may be able to remove it.

---

## Internal Improvements

Non-breaking changes for informational purposes.

- **FormFieldBase expression caching:** `FormFieldBase` now caches the result of `Expression.Compile()`, avoiding redundant compilation on every render.
- **FormFieldBase `NotifyFieldChanged()`:** A new protected method for derived FormField components to notify `EditContext` that a bound value changed.
- **FloatingPortal structured logging:** Render timeout warnings upgraded from `Console.WriteLine` to structured `ILogger<FloatingPortal>` using `LoggerMessage.Define`.
- **Styled Checkbox** now uses `<BbCheckboxIndicator />` internally instead of inline SVG conditional rendering.
- **Styled Switch** now uses `<BbSwitchThumb />` internally instead of a manual `<span data-state="...">`.
- **Input validation centralization:** Created `InputValidationBehavior` internal composition class that encapsulates shared EditContext validation logic (`IsInvalid`, `EffectiveAriaInvalid`, `EffectiveName`, `NotifyFieldChanged`). Refactored 8 input components (`BbInput`, `BbTextarea`, `BbCurrencyInput`, `BbNumericInput<TValue>`, `BbMaskedInput`, `BbInputGroupInput`, `BbInputGroupTextarea`, `BbInputField<TValue>`) to delegate to this shared class, eliminating ~220 lines of duplicated code. No public API changes.
- **Menu keyboard navigation moved to JavaScript:** `BbDropdownMenuContent`, `BbContextMenuContent`, and `BbMenubarContent` keyboard handling moved from C# `@onkeydown` event handlers to a shared `menu-keyboard.js` module. Arrow keys, Home, End, Enter, and Space are handled entirely in JavaScript with zero C# interop — only Escape key and Menubar ArrowLeft/ArrowRight trigger C# callbacks. This eliminates unnecessary round-trips and re-renders during menu navigation. No public API changes.

---

## Summary

### Required actions (breaking changes)

1. **Add `Bb` prefix to all component tags** — `<Button>` → `<BbButton>`, `<Dialog>` → `<BbDialog>`, etc. across all `.razor` files
2. **Remove sub-namespace imports** — delete all `@using BlazorBlueprint.Components.*` lines; use `@using BlazorBlueprint.Components` + `@using BlazorBlueprint.Primitives` only
3. **Migrate `Combobox<TItem>`** to `BbCombobox<TValue>` — replace `Items`/`ValueSelector`/`DisplaySelector` with `Options` or `BbComboboxItem` children
4. **Migrate `MultiSelect<TItem>`** to `BbMultiSelect<TValue>` — replace `Items`/`ValueSelector`/`DisplaySelector` with `Options` or `BbMultiSelectItem` children
5. **Migrate `FormFieldCombobox<TItem>`** to `BbFormFieldCombobox<TValue>` — same API changes as Combobox
6. **Migrate `FormFieldMultiSelect<TItem>`** to `BbFormFieldMultiSelect<TValue>` — same API changes as MultiSelect
7. **Add `AsChild="false"`** to trigger/close components that relied on auto-generated buttons
8. **Replace positioning strategy strings** with `PositioningStrategy` enum values
9. **Update `BbSelectValue.ChildContent`** templates to accept the `context` parameter
10. **Review `BbResizablePanel` layouts** for panels that shouldn't grow to fill space
11. **Replace `SelectContext.SetDisplayText()`** with direct `State.DisplayText` assignment (primitives layer only)
12. **Rename `BbSelectItem.TextValue`** to `Text` on primitive `BbSelectItem` usage
13. **Rewrite all chart components** — ApexCharts replaced by ECharts with new declarative composition API; remove `@using ApexCharts` and `Blazor-ApexCharts` NuGet; rewrite every chart using `<BbXAxis>`, `<BbLine>`, `<BbBar>`, etc.; rename `RadialChart` to `BbRadialBarChart`
14. **Add `UpdateTiming="UpdateTiming.Immediate"`** to any `BbInput`, `BbTextarea`, `BbInputGroupInput`, `BbInputGroupTextarea`, `BbInputField`, or `BbFormFieldInput` that relied on per-keystroke `ValueChanged` updates (default changed from `Immediate` to `OnChange`)
15. **Update `IDisposable` casts** — `BbInput`, `BbTextarea`, `BbInputGroupInput`, `BbInputGroupTextarea`, `BbInputField`, `BbNumericInput`, `BbCurrencyInput` now implement `IAsyncDisposable` instead of `IDisposable`
16. **Update `IPortalService` usage** — `RegisterPortal` now requires a `PortalCategory` parameter; `GetPortals()` replaced by `GetPortals(PortalCategory)`; `OnPortalsChanged` replaced by `OnPortalsCategoryChanged`
17. **Review `ToastService.Success()` visual change** — now renders with `ToastVariant.Success` (green accent + icon) instead of neutral; use `ToastService.Show()` directly to restore old neutral style
18. **Review nullable string parameters** — `BbCombobox`, `BbMultiSelect`, `BbTagInput`, `BbDatePicker`, `BbDateRangePicker`, `BbDataView` changed some string params to `string?`
19. **Set `FirstDayOfWeek` explicitly** if you relied on Sunday default — now defaults to `CultureInfo.CurrentCulture`
20. **Set `DateFormat` explicitly** on `BbDatePicker` if you relied on `"MMMM d, yyyy"` — now defaults to `"d"`
21. **Review `InputConverter` culture behavior** — now uses `CurrentCulture` instead of `InvariantCulture`; set `InputConverter.GlobalCulture = CultureInfo.InvariantCulture` to restore

### Recommended actions (non-breaking)

18. **Replace manual DI registration** with `services.AddBlazorBlueprintComponents()`
19. **Add `<BbDialogProvider />`** to your root layout if using `DialogService`
20. **Consider separate portal hosts** — replace `<BbPortalHost />` with `<BbContainerPortalHost />` + `<BbOverlayPortalHost />` for category-scoped re-rendering

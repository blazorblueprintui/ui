---
title: "DataGrid Server-Side Infinite Scroll (Virtualized ItemsProvider)"
date: 2026-03-23
branch: feat/datagrid-virtual-scroll
status: complete
author: claude
tags: [datagrid, virtualization, items-provider, infinite-scroll]
estimated_tasks: 8
---

# DataGrid Server-Side Infinite Scroll

## Context

Currently `ItemsProvider` (server-side data) uses page-based pagination, and `Virtualize` (smooth scrolling) only works with client-side data. When `Virtualize=true` with `ItemsProvider`, the grid fetches **all** items from the server (`StartIndex=0, Count=null`) and virtualizes the DOM rendering — but the full dataset must still fit in memory.

This plan enables true server-side infinite scroll: Blazor's `<Virtualize ItemsProvider=...>` drives data requests on demand as the user scrolls, fetching only the visible window plus overscan.

> **CRITICAL RULE — Original Code Only**
>
> All implementations must be written from scratch, original to BlazorBlueprint. No code may be copied from any third-party codebase.

---

## Architecture

### Current Modes

| Condition | Behavior |
|---|---|
| `Items` set, `Virtualize=false` | Client-side data, paginated |
| `Items` set, `Virtualize=true` | Client-side data, virtualized DOM (all items in memory) |
| `ItemsProvider` set, `Virtualize=false` | Server-side data, paginated via `StartIndex`/`Count` |
| `ItemsProvider` set, `Virtualize=true` | **Currently:** Server fetches ALL items, virtualizes DOM only |

### New Mode

| Condition | Behavior |
|---|---|
| `ItemsProvider` set, `Virtualize=true` | **New:** `<Virtualize ItemsProvider=...>` drives server requests as user scrolls |

### Key Insight

Blazor's native `<Virtualize>` component already supports an `ItemsProvider` delegate that receives `ItemsProviderRequest` with `StartIndex` and `Count`. We create a **bridge method** that translates between Blazor's request type and BlazorBlueprint's `DataGridRequest`, injecting the current sort/filter/group state.

This is cleaner than a custom scroll-based approach because Blazor handles all viewport tracking, request batching, and DOM recycling.

---

## Implementation Steps

### Step 1: Add VirtualScrollHeight Parameter

**File:** `src/BlazorBlueprint.Components/Components/DataGrid/BbDataGrid.razor.cs`

Add parameter after `OverscanCount`:

```csharp
/// <summary>
/// CSS height for the scroll container when using virtualized ItemsProvider mode
/// (both <see cref="Virtualize"/> and <see cref="ItemsProvider"/> are set).
/// Required in this mode to give the Virtualize component a bounded scroll area.
/// Defaults to <c>"400px"</c>. Accepts any CSS length value.
/// </summary>
[Parameter]
public string VirtualScrollHeight { get; set; } = "400px";
```

**Estimated effort:** Trivial (5 min)

---

### Step 2: Add Computed Property for Virtual+Provider Mode

**File:** `src/BlazorBlueprint.Components/Components/DataGrid/BbDataGrid.razor.cs`

Add a helper property to detect the combined mode:

```csharp
/// <summary>
/// Whether the grid is in server-side virtual scroll mode (both Virtualize and ItemsProvider set).
/// </summary>
private bool IsVirtualizedProvider => Virtualize && ItemSize > 0 && ItemsProvider != null;
```

**Estimated effort:** Trivial (5 min)

---

### Step 3: Create the Virtualize ItemsProvider Bridge

**File:** `src/BlazorBlueprint.Components/Components/DataGrid/BbDataGrid.razor.cs`

Add a field for the Virtualize component reference and the bridge method:

```csharp
private Virtualize<TData>? _virtualizeRef;

/// <summary>
/// Bridge between Blazor's ItemsProviderRequest and BlazorBlueprint's DataGridRequest.
/// Called by <Virtualize> as the user scrolls.
/// </summary>
private async ValueTask<ItemsProviderResult<TData>> VirtualItemsProviderAsync(
    ItemsProviderRequest request)
{
    var aggregateColumns = _columns
        .Where(c => c.Aggregate != AggregateFunction.None)
        .Select(c => c.ColumnId)
        .ToList();

    var dataGridRequest = new DataGridRequest
    {
        SortDefinitions = _gridState.Sorting.Definitions,
        StartIndex = request.StartIndex,
        Count = request.Count,
        CancellationToken = request.CancellationToken,
        Filters = _gridState.Filtering.Filters,
        GroupDefinition = _gridState.Grouping.ActiveGroup,
        AggregateColumns = aggregateColumns.Count > 0 ? aggregateColumns : null
    };

    var result = await ItemsProvider!(dataGridRequest);

    // Update pagination total for display purposes (e.g., "Showing X of Y")
    _gridState.Pagination.TotalItems = result.TotalItemCount;

    return new ItemsProviderResult<TData>(result.Items, result.TotalItemCount);
}
```

**Key decisions:**
- `StartIndex` and `Count` come directly from Blazor's request — the Virtualize component manages windowing
- Sort/filter/group state is injected from the current grid state
- `TotalItemCount` flows back to update the pagination state (for info display)
- CancellationToken flows through so superseded requests are cancelled

**Estimated effort:** Small (30 min)

---

### Step 4: Modify LoadFromProviderAsync — Skip in Virtual+Provider Mode

**File:** `src/BlazorBlueprint.Components/Components/DataGrid/BbDataGrid.razor.cs`

In the existing `LoadFromProviderAsync()` method (line ~1966), add an early exit when in virtual+provider mode, since the Virtualize component drives data loading:

```csharp
private async Task LoadFromProviderAsync()
{
    // In virtualized provider mode, Virtualize drives data loading.
    // Just refresh the Virtualize component instead.
    if (IsVirtualizedProvider)
    {
        if (_virtualizeRef != null)
        {
            await _virtualizeRef.RefreshDataAsync();
        }
        return;
    }

    // ... existing pagination-based loading logic unchanged ...
}
```

This ensures that when sort/filter changes trigger `ProcessDataAsync()` → `LoadFromProviderAsync()`, the Virtualize component is told to re-query rather than doing a manual fetch.

**Estimated effort:** Small (15 min)

---

### Step 5: Update the Razor Template

**File:** `src/BlazorBlueprint.Components/Components/DataGrid/BbDataGrid.razor`

#### 5a. Scroll Container Height

Wrap the table container with a height-constrained div when in virtual+provider mode:

```razor
<div @ref="containerRef"
     class="@ClassNames.cn("relative w-full", Virtualize ? "" : "overflow-auto", TableContainerClass)"
     style="@(IsVirtualizedProvider ? $"height: {VirtualScrollHeight}; overflow-y: auto;" : null)">
```

#### 5b. Conditional Virtualize Rendering

Replace the existing Virtualize block (lines 278-282) to handle both modes:

```razor
@* Existing client-side virtualization with Items *@
else if (Virtualize && ItemSize > 0 && !IsVirtualizedProvider)
{
    <Virtualize Items="@_processedDataList" Context="item"
                ItemSize="@ItemSize" OverscanCount="@OverscanCount">
        @RenderDataRow(item)
    </Virtualize>
}
@* NEW: Server-side virtualization with ItemsProvider bridge *@
else if (IsVirtualizedProvider)
{
    <Virtualize @ref="_virtualizeRef"
                ItemsProvider="@VirtualItemsProviderAsync"
                Context="item"
                ItemSize="@ItemSize"
                OverscanCount="@OverscanCount">
        @RenderDataRow(item)
    </Virtualize>
}
```

#### 5c. Hide Pagination in Virtual+Provider Mode

Update the pagination condition (line 329):

```razor
@if (ShowPagination && !IsLoading && !IsVirtualizedProvider && _processedData.Any())
```

**Estimated effort:** Medium (45 min) — careful ordering of conditional blocks

---

### Step 6: Handle Sort/Filter Refresh

**File:** `src/BlazorBlueprint.Components/Components/DataGrid/BbDataGrid.razor.cs`

The existing `HandleSortChange` and filter handling already call `ProcessDataAsync()` → `LoadFromProviderAsync()`. With Step 4's change, this now calls `_virtualizeRef.RefreshDataAsync()` in virtual+provider mode, which re-queries the bridge from `StartIndex=0`. No additional changes needed.

However, verify that these methods work correctly:
- `HandleSortChange` — triggers data reload ✓
- Filter changes (via column filter UI) — triggers data reload ✓
- `HandlePageSizeChanged` — should be unreachable (pagination hidden) ✓

**Estimated effort:** Small (15 min) — verification and testing only

---

### Step 7: Handle Grouped/Hierarchy Mode

Grouped and hierarchy modes with server-side virtual scroll are **not supported** in this iteration. The `_groupedRenderItems` path stays on the existing client-side virtualization.

Add a guard in the bridge method:

```csharp
private async ValueTask<ItemsProviderResult<TData>> VirtualItemsProviderAsync(
    ItemsProviderRequest request)
{
    // Grouping is not supported with virtualized provider mode
    if (_groupByAccessor != null)
    {
        return new ItemsProviderResult<TData>(Array.Empty<TData>(), 0);
    }

    // ... bridge logic ...
}
```

**Estimated effort:** Trivial (5 min)

---

### Step 8: Demo Page and Testing

**File:** `demos/BlazorBlueprint.Demo.Shared/Pages/Components/DataGridDemo.razor`

Add a new section demonstrating server-side virtual scroll. Use a simulated async provider that adds artificial delay:

```razor
<BbDataGrid TData="Person" ItemsProvider="@VirtualProviderAsync"
            Virtualize="true" ItemSize="48"
            VirtualScrollHeight="400px">
    <Columns>
        <BbDataGridPropertyColumn TData="Person" TProp="string"
            Property="@(p => p.Name)" Sortable />
        ...
    </Columns>
</BbDataGrid>

@code {
    private async ValueTask<DataGridResult<Person>> VirtualProviderAsync(DataGridRequest request)
    {
        await Task.Delay(50); // Simulate network latency
        var allData = MockDataService.GeneratePersons(10000);

        // Apply sort
        var sorted = ApplySorting(allData, request.SortDefinitions);

        // Apply pagination window
        var page = sorted.Skip(request.StartIndex).Take(request.Count ?? 50).ToList();

        return new DataGridResult<Person>
        {
            Items = page,
            TotalItemCount = allData.Count
        };
    }
}
```

**Test scenarios:**
- Scroll through 10,000 items — only visible + overscan rows in DOM
- Sort a column — grid refreshes from top with new sort order
- Filter a column — grid refreshes with filtered count
- Verify pagination footer is hidden
- Verify `VirtualScrollHeight` constrains the container
- Verify empty state when provider returns 0 items
- Verify loading indicator during initial load

**Estimated effort:** Medium (1-2 hours)

---

## Execution Order

| Step | Description | Dependencies |
|------|-------------|-------------|
| 1 | Add `VirtualScrollHeight` parameter | None |
| 2 | Add `IsVirtualizedProvider` property | None |
| 3 | Create bridge method | Steps 1, 2 |
| 4 | Modify `LoadFromProviderAsync` | Step 2 |
| 5 | Update razor template | Steps 2, 3 |
| 6 | Verify sort/filter refresh | Steps 4, 5 |
| 7 | Guard grouped mode | Step 3 |
| 8 | Demo page and testing | Steps 1-7 |

---

## API Surface Changes

New public parameters on `BbDataGrid<TData>`:

| Parameter | Type | Default | Description |
|---|---|---|---|
| `VirtualScrollHeight` | `string` | `"400px"` | CSS height for scroll container in virtual+provider mode |

No changes to `DataGridRequest` or `DataGridResult` — existing `StartIndex`/`Count` fields are reused.

---

## Risk Assessment

| Risk | Mitigation |
|------|-----------|
| Blazor's `Virtualize` may make redundant requests during rapid scrolling | The `CancellationToken` flow through the bridge ensures superseded requests are cancelled. `OverscanCount` reduces request frequency. |
| Sort/filter change causes flicker as Virtualize re-queries from scratch | `RefreshDataAsync()` resets the scroll position and re-queries cleanly. Users expect a reset on filter change. |
| Grouped/hierarchy mode not supported | Guard with early return and clear documentation. Can be added later. |
| Large `TotalItemCount` causes memory issues in Virtualize component | Blazor's Virtualize handles this natively — it only tracks DOM for visible items, not all items. |
| Existing `Virtualize + Items` behavior must not change | The `IsVirtualizedProvider` condition is strict: requires `ItemsProvider != null`. Client-side virtualization is unchanged. |

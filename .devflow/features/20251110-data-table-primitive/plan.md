# Technical Implementation Plan: Data Table Primitive

**Feature:** Data Table Primitive
**Phase:** 1 - MVP (Core Primitive)
**Status:** Planning Complete
**Created:** 2025-11-10
**Architect:** Senior Software Architect

---

## Executive Summary

This plan outlines the technical implementation for a headless data table primitive for the BlazorUI UI library. Following TanStack Table's philosophy and the project's existing primitives architecture, we will create a flexible, type-safe table component that separates data logic from presentation. The primitive will provide sophisticated sorting, pagination, and selection capabilities while giving developers complete control over markup and styling.

The implementation will follow a headless approach, consistent with existing primitives like Combobox, using a context-based state management pattern with cascading values. This ensures maximum flexibility, reusability, and consistency with the BlazorUI architecture.

**Complexity:** High
**Estimated Task Count:** 18-22 tasks
**Risk Level:** Medium
**Architecture Impact:** Minor (follows existing patterns)
**Requires Review:** false

---

## 1. Architecture Decision

### Headless Primitive Approach

The table will be implemented as a **headless primitive**, providing logic and state management without prescriptive markup. This aligns with:

- **TanStack Table's philosophy** - Separation of concerns between logic and presentation
- **Existing BlazorUI primitives** - Follows the same pattern as Combobox, Dialog, etc.
- **Project constitution** - Component isolation and composition principles

### Architecture Justification

**Pros:**
- Complete markup control for developers
- Consistent with existing primitives library structure
- Enables multiple styled variants from single primitive
- Framework-agnostic (works across Server/WebAssembly/Hybrid)
- Smaller bundle size (no built-in styles)
- Easier to maintain and test

**Cons:**
- More verbose initial implementation for consumers
- Requires documentation of markup patterns
- Developers must handle styling themselves (mitigated by examples)

### Integration with Existing Architecture

```
/src/BlazorUI.Primitives/
  /Primitives/
    /Table/                    # New primitive
      Table.razor              # Main container component
      Table.razor.cs
      TableHeader.razor        # Header row container
      TableHeaderCell.razor    # Sortable header cell
      TableBody.razor          # Body container
      TableRow.razor          # Data row wrapper
      TableCell.razor         # Data cell wrapper
      TablePagination.razor   # Pagination controls
      /Models/
        TableContext.cs       # State management
        ColumnDefinition.cs   # Column configuration
        TableState.cs         # State container
        SortingState.cs       # Sort configuration
        PaginationState.cs    # Pagination state
        SelectionState.cs     # Row selection
      /Enums/
        SortDirection.cs
        SelectionMode.cs
```

---

## 2. API Design

### Core Table Component API

```csharp
// Table<TData> - Main primitive component
<Table TData="Person"
       Data="@people"
       Columns="@columnDefinitions"
       @bind-State="@tableState"
       OnStateChange="@HandleStateChange"
       SelectionMode="SelectionMode.Multiple"
       Context="row">

    <HeaderTemplate>
        <TableHeader>
            @foreach (var column in context.Columns)
            {
                <TableHeaderCell Column="@column" />
            }
        </TableHeader>
    </HeaderTemplate>

    <RowTemplate>
        <TableRow Item="@row" Selected="@row.IsSelected">
            @foreach (var column in context.Columns)
            {
                <TableCell Column="@column" Item="@row" />
            }
        </TableRow>
    </RowTemplate>

    <EmptyTemplate>
        <div>No data available</div>
    </EmptyTemplate>

</Table>
```

### Column Definition Pattern (C#-Idiomatic)

```csharp
// Inspired by TanStack but adapted for C# type system
public class ColumnDefinition<TData, TValue>
{
    // Required properties
    public string Id { get; set; }
    public string Header { get; set; }
    public Func<TData, TValue> Accessor { get; set; }

    // Optional properties
    public bool CanSort { get; set; } = true;
    public IComparer<TValue>? CustomComparer { get; set; }
    public RenderFragment<CellContext<TData, TValue>>? CellTemplate { get; set; }
    public RenderFragment<HeaderContext>? HeaderTemplate { get; set; }
    public string? Width { get; set; }
    public string? MinWidth { get; set; }
    public string? MaxWidth { get; set; }
    public bool Visible { get; set; } = true;
}

// Usage example
var columns = new[]
{
    new ColumnDefinition<Person, string>
    {
        Id = "name",
        Header = "Name",
        Accessor = p => p.Name,
        CanSort = true
    },
    new ColumnDefinition<Person, int>
    {
        Id = "age",
        Header = "Age",
        Accessor = p => p.Age,
        Width = "100px"
    },
    new ColumnDefinition<Person, DateTime>
    {
        Id = "birthdate",
        Header = "Birth Date",
        Accessor = p => p.BirthDate,
        CellTemplate = context => @<text>@context.Value.ToShortDateString()</text>
    }
};
```

### State Management Approach

Support both **controlled** and **uncontrolled** modes:

```csharp
// Uncontrolled (internal state)
<Table TData="Person" Data="@people" Columns="@columns" />

// Controlled (external state management)
<Table TData="Person"
       Data="@people"
       Columns="@columns"
       State="@tableState"
       OnStateChange="@((state) => tableState = state)" />

@code {
    private TableState<Person> tableState = new();

    // Can programmatically control state
    void SortByName() => tableState.Sorting.SetSort("name", SortDirection.Ascending);
    void ClearSelection() => tableState.Selection.Clear();
    void GoToPage(int page) => tableState.Pagination.CurrentPage = page;
}
```

---

## 3. Core Components & Classes

### Table<TData> Component

**Responsibility:** Main container that orchestrates state and renders child components

```csharp
public partial class Table<TData> : ComponentBase
{
    [Parameter] public IEnumerable<TData> Data { get; set; }
    [Parameter] public IEnumerable<IColumnDefinition<TData>> Columns { get; set; }
    [Parameter] public TableState<TData>? State { get; set; }
    [Parameter] public EventCallback<TableState<TData>> StateChanged { get; set; }
    [Parameter] public SelectionMode SelectionMode { get; set; }
    [Parameter] public RenderFragment<TableRenderContext<TData>>? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment<TData>? RowTemplate { get; set; }
    [Parameter] public RenderFragment? EmptyTemplate { get; set; }

    private TableContext<TData> context;
    private IEnumerable<TData> processedData;

    protected override void OnInitialized()
    {
        context = new TableContext<TData>(State ?? new TableState<TData>());
        ProcessData();
    }

    private void ProcessData()
    {
        processedData = Data
            .ApplySorting(context.State.Sorting)
            .ApplyPagination(context.State.Pagination);
    }
}
```

### TableContext<TData> Class

**Responsibility:** Manages shared state between table components via CascadingValue

```csharp
public class TableContext<TData> : PrimitiveContextWithEvents<TableState<TData>>
{
    public IEnumerable<IColumnDefinition<TData>> Columns { get; set; }
    public IEnumerable<TData> ProcessedData { get; set; }
    public SelectionMode SelectionMode { get; set; }

    // Event handlers
    public Action<string, SortDirection>? OnSort { get; set; }
    public Action<TData>? OnRowSelect { get; set; }
    public Action<int>? OnPageChange { get; set; }
    public Action<int>? OnPageSizeChange { get; set; }

    public TableContext(TableState<TData> state) : base(state, "table")
    {
    }

    public void ToggleSort(string columnId)
    {
        State.Sorting.ToggleSort(columnId);
        NotifyStateChanged();
    }

    public void ToggleRowSelection(TData item)
    {
        State.Selection.Toggle(item);
        NotifyStateChanged();
    }
}
```

### ColumnDefinition<TData, TValue> Class

**Responsibility:** Defines column configuration and behavior

```csharp
public interface IColumnDefinition<TData>
{
    string Id { get; }
    string Header { get; }
    bool CanSort { get; }
    bool Visible { get; }
    object? GetValue(TData item);
    int Compare(TData x, TData y);
}

public class ColumnDefinition<TData, TValue> : IColumnDefinition<TData>
{
    public string Id { get; set; }
    public string Header { get; set; }
    public Func<TData, TValue> Accessor { get; set; }
    public bool CanSort { get; set; } = true;
    public IComparer<TValue>? CustomComparer { get; set; }
    public bool Visible { get; set; } = true;
    public RenderFragment<CellContext<TData, TValue>>? CellTemplate { get; set; }

    public object? GetValue(TData item) => Accessor(item);

    public int Compare(TData x, TData y)
    {
        var xValue = Accessor(x);
        var yValue = Accessor(y);

        if (CustomComparer != null)
            return CustomComparer.Compare(xValue, yValue);

        return Comparer<TValue>.Default.Compare(xValue, yValue);
    }
}
```

### TableState<TData> Class

**Responsibility:** Container for all table state

```csharp
public class TableState<TData>
{
    public SortingState Sorting { get; set; } = new();
    public PaginationState Pagination { get; set; } = new();
    public SelectionState<TData> Selection { get; set; } = new();

    // Computed properties
    public bool HasActiveFilters => false; // Phase 2
    public int TotalSelected => Selection.SelectedItems.Count;
}
```

### SortingState Class

**Responsibility:** Manages sorting configuration

```csharp
public class SortingState
{
    public string? SortedColumn { get; set; }
    public SortDirection Direction { get; set; } = SortDirection.None;
    public List<SortDescriptor> MultiSort { get; set; } = new(); // Phase 2

    public void SetSort(string columnId, SortDirection direction)
    {
        SortedColumn = columnId;
        Direction = direction;
    }

    public void ToggleSort(string columnId)
    {
        if (SortedColumn == columnId)
        {
            Direction = Direction switch
            {
                SortDirection.None => SortDirection.Ascending,
                SortDirection.Ascending => SortDirection.Descending,
                SortDirection.Descending => SortDirection.None,
                _ => SortDirection.None
            };

            if (Direction == SortDirection.None)
                SortedColumn = null;
        }
        else
        {
            SortedColumn = columnId;
            Direction = SortDirection.Ascending;
        }
    }
}
```

### PaginationState Class

**Responsibility:** Manages pagination configuration

```csharp
public class PaginationState
{
    private int currentPage = 1;
    private int pageSize = 10;

    public int CurrentPage
    {
        get => currentPage;
        set => currentPage = Math.Max(1, value);
    }

    public int PageSize
    {
        get => pageSize;
        set => pageSize = Math.Max(1, value);
    }

    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public int StartIndex => (CurrentPage - 1) * PageSize;
    public int EndIndex => Math.Min(StartIndex + PageSize, TotalItems);

    public bool CanGoNext => CurrentPage < TotalPages;
    public bool CanGoPrevious => CurrentPage > 1;

    public void NextPage() => CurrentPage = Math.Min(CurrentPage + 1, TotalPages);
    public void PreviousPage() => CurrentPage = Math.Max(CurrentPage - 1, 1);
    public void GoToPage(int page) => CurrentPage = Math.Max(1, Math.Min(page, TotalPages));
}
```

### SelectionState<TData> Class

**Responsibility:** Tracks row selection

```csharp
public class SelectionState<TData>
{
    private readonly HashSet<TData> selectedItems = new();

    public IReadOnlyCollection<TData> SelectedItems => selectedItems;
    public SelectionMode Mode { get; set; } = SelectionMode.None;

    public bool IsSelected(TData item) => selectedItems.Contains(item);

    public void Select(TData item)
    {
        if (Mode == SelectionMode.Single)
            selectedItems.Clear();

        selectedItems.Add(item);
    }

    public void Deselect(TData item) => selectedItems.Remove(item);

    public void Toggle(TData item)
    {
        if (IsSelected(item))
            Deselect(item);
        else
            Select(item);
    }

    public void SelectAll(IEnumerable<TData> items)
    {
        foreach (var item in items)
            selectedItems.Add(item);
    }

    public void Clear() => selectedItems.Clear();
}
```

---

## 4. State Management Strategy

### State Flow Architecture

```
User Interaction → Component Event → Context Method → State Update → NotifyStateChanged → Re-render
```

### Controlled vs Uncontrolled Modes

```csharp
// Table component handles both modes
protected override void OnParametersSet()
{
    // Use provided state or internal state
    var state = State ?? internalState;

    if (context == null)
    {
        context = new TableContext<TData>(state);
    }
    else if (State != null && State != context.State)
    {
        // External state changed
        context.State = State;
    }

    ProcessData();
}

private async Task HandleStateChange()
{
    if (State != null)
    {
        // Controlled mode - notify parent
        await StateChanged.InvokeAsync(context.State);
    }

    // Always reprocess data
    ProcessData();
    StateHasChanged();
}
```

### Optimization with ShouldRender()

```csharp
public partial class TableRow<TData> : ComponentBase
{
    [CascadingParameter] public TableContext<TData> Context { get; set; }
    [Parameter] public TData Item { get; set; }

    private bool isSelected;
    private int renderCount = 0;

    protected override void OnParametersSet()
    {
        isSelected = Context.State.Selection.IsSelected(Item);
    }

    protected override bool ShouldRender()
    {
        // Only re-render if selection state changed
        var currentSelected = Context.State.Selection.IsSelected(Item);
        if (currentSelected != isSelected)
        {
            isSelected = currentSelected;
            return true;
        }

        // Skip unnecessary renders
        return renderCount == 0; // Only render once initially
    }
}
```

---

## 5. Rendering Pattern

### Developer Usage Pattern

```razor
@* Developers have complete control over markup *@
<Table TData="Product" Data="@products" Columns="@columns" Context="table">
    <div class="rounded-md border">
        <table class="w-full caption-bottom text-sm">
            <thead class="[&_tr]:border-b">
                <tr class="border-b transition-colors hover:bg-muted/50">
                    @if (table.SelectionMode != SelectionMode.None)
                    {
                        <th class="h-12 px-4">
                            <input type="checkbox" @onchange="@table.ToggleSelectAll" />
                        </th>
                    }
                    @foreach (var column in table.Columns)
                    {
                        <TableHeaderCell Column="@column" OnSort="@table.ToggleSort">
                            <span class="flex items-center gap-2">
                                @column.Header
                                @if (column.CanSort)
                                {
                                    <SortIndicator Column="@column" State="@table.State.Sorting" />
                                }
                            </span>
                        </TableHeaderCell>
                    }
                </tr>
            </thead>
            <tbody class="[&_tr:last-child]:border-0">
                @foreach (var item in table.ProcessedData)
                {
                    <tr class="border-b transition-colors hover:bg-muted/50 @(table.IsSelected(item) ? "bg-muted" : "")">
                        @if (table.SelectionMode != SelectionMode.None)
                        {
                            <td class="p-4">
                                <input type="checkbox"
                                       checked="@table.IsSelected(item)"
                                       @onchange="@(() => table.ToggleSelection(item))" />
                            </td>
                        }
                        @foreach (var column in table.Columns)
                        {
                            <TableCell Item="@item" Column="@column">
                                @if (column.CellTemplate != null)
                                {
                                    @column.CellTemplate(new CellContext<Product> { Item = item, Value = column.GetValue(item) })
                                }
                                else
                                {
                                    @column.GetValue(item)
                                }
                            </TableCell>
                        }
                    </tr>
                }
            </tbody>
        </table>
    </div>

    @if (table.State.Pagination.TotalPages > 1)
    {
        <TablePagination State="@table.State.Pagination" OnPageChange="@table.GoToPage" />
    }
</Table>
```

### RenderFragment Pattern

```csharp
// Table component provides render contexts
public class TableRenderContext<TData>
{
    public IEnumerable<IColumnDefinition<TData>> Columns { get; set; }
    public TableState<TData> State { get; set; }
    public IEnumerable<TData> ProcessedData { get; set; }
    public SelectionMode SelectionMode { get; set; }

    // Helper methods for template usage
    public bool IsSelected(TData item) => State.Selection.IsSelected(item);
    public void ToggleSelection(TData item) => State.Selection.Toggle(item);
    public void ToggleSort(string columnId) => State.Sorting.ToggleSort(columnId);
}
```

---

## 6. Data Flow & Transformations

### Processing Pipeline

```
Raw Data → Filtering (Phase 2) → Sorting → Pagination → Rendered Data
```

### Implementation

```csharp
public static class TableDataExtensions
{
    public static IEnumerable<TData> ApplySorting<TData>(
        this IEnumerable<TData> data,
        SortingState sorting,
        IEnumerable<IColumnDefinition<TData>> columns)
    {
        if (sorting.SortedColumn == null || sorting.Direction == SortDirection.None)
            return data;

        var column = columns.FirstOrDefault(c => c.Id == sorting.SortedColumn);
        if (column == null)
            return data;

        return sorting.Direction == SortDirection.Ascending
            ? data.OrderBy(item => column.GetValue(item))
            : data.OrderByDescending(item => column.GetValue(item));
    }

    public static IEnumerable<TData> ApplyPagination<TData>(
        this IEnumerable<TData> data,
        PaginationState pagination)
    {
        // Update total count before pagination
        pagination.TotalItems = data.Count();

        return data
            .Skip(pagination.StartIndex)
            .Take(pagination.PageSize);
    }
}
```

### Performance Considerations

```csharp
// Optimize for large datasets
private void ProcessData()
{
    // Use IQueryable when possible for database queries
    IQueryable<TData> query = Data.AsQueryable();

    // Apply transformations
    query = ApplySorting(query);

    // Get total count before pagination
    var totalCount = query.Count();
    context.State.Pagination.TotalItems = totalCount;

    // Apply pagination last
    processedData = query
        .Skip(context.State.Pagination.StartIndex)
        .Take(context.State.Pagination.PageSize)
        .ToList(); // Materialize only the visible page
}
```

---

## 7. Type Safety Approach

### Generic Constraints and Variance

```csharp
// Strong typing throughout the API
public partial class Table<TData> : ComponentBase where TData : class
{
    // Ensures reference types for selection tracking
}

// Column definitions with type safety
public class ColumnDefinition<TData, TValue> : IColumnDefinition<TData>
{
    public Func<TData, TValue> Accessor { get; set; }

    // Type-safe sorting with custom comparers
    public IComparer<TValue>? CustomComparer { get; set; }
}
```

### Expression Trees for Property Access (Optional Enhancement)

```csharp
// Alternative API for compile-time checked property access
public static class ColumnBuilder
{
    public static ColumnDefinition<TData, TValue> Create<TData, TValue>(
        Expression<Func<TData, TValue>> propertyExpression,
        string? header = null)
    {
        var memberExpression = (MemberExpression)propertyExpression.Body;
        var propertyName = memberExpression.Member.Name;
        var compiledAccessor = propertyExpression.Compile();

        return new ColumnDefinition<TData, TValue>
        {
            Id = propertyName.ToLower(),
            Header = header ?? propertyName,
            Accessor = compiledAccessor
        };
    }
}

// Usage
var columns = new[]
{
    ColumnBuilder.Create<Person, string>(p => p.Name),
    ColumnBuilder.Create<Person, int>(p => p.Age, "Age (years)"),
    ColumnBuilder.Create<Person, DateTime>(p => p.BirthDate)
};
```

---

## 8. Accessibility Foundation

### ARIA Attributes

```razor
<table role="table" aria-label="@AriaLabel" aria-rowcount="@TotalRows">
    <thead role="rowgroup">
        <tr role="row">
            @foreach (var column in columns)
            {
                <th role="columnheader"
                    aria-sort="@GetAriaSortValue(column)"
                    @onclick="@(() => ToggleSort(column.Id))"
                    tabindex="@(column.CanSort ? 0 : -1)">
                    @column.Header
                </th>
            }
        </tr>
    </thead>
    <tbody role="rowgroup">
        @foreach (var (item, index) in processedData.Select((item, i) => (item, i)))
        {
            <tr role="row"
                aria-rowindex="@(startIndex + index + 2)"
                aria-selected="@IsSelected(item)">
                @foreach (var column in columns)
                {
                    <td role="cell">@column.GetValue(item)</td>
                }
            </tr>
        }
    </tbody>
</table>

@code {
    string GetAriaSortValue(IColumnDefinition column)
    {
        if (sortingState.SortedColumn != column.Id)
            return "none";

        return sortingState.Direction switch
        {
            SortDirection.Ascending => "ascending",
            SortDirection.Descending => "descending",
            _ => "none"
        };
    }
}
```

### Keyboard Navigation

```csharp
public partial class TableHeaderCell<TData> : ComponentBase
{
    [Parameter] public IColumnDefinition<TData> Column { get; set; }
    [Parameter] public EventCallback<string> OnSort { get; set; }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (!Column.CanSort) return;

        switch (e.Key)
        {
            case "Enter":
            case " ":
                e.PreventDefault();
                await OnSort.InvokeAsync(Column.Id);
                break;
        }
    }
}
```

### Focus Management

```csharp
// Manage focus for keyboard navigation
public class TableKeyboardNavigator<TData>
{
    private ElementReference? currentFocusedElement;
    private int focusedRowIndex = -1;
    private int focusedColumnIndex = -1;

    public async Task HandleKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "ArrowUp":
                await MoveFocus(-1, 0);
                break;
            case "ArrowDown":
                await MoveFocus(1, 0);
                break;
            case "ArrowLeft":
                await MoveFocus(0, -1);
                break;
            case "ArrowRight":
                await MoveFocus(0, 1);
                break;
            case " ":
                if (e.CtrlKey)
                    await ToggleRowSelection();
                break;
        }
    }
}
```

---

## 9. Theming Integration

### No Built-in Styles

The primitive provides no CSS - developers have complete control:

```razor
@* Developer applies their own Tailwind classes *@
<Table TData="Product" Data="@products" Columns="@columns">
    <TableTemplate>
        <div class="rounded-lg border border-border">
            <table class="w-full">
                <thead class="bg-muted/50">
                    <!-- Custom styling -->
                </thead>
                <tbody class="divide-y divide-border">
                    <!-- Custom styling -->
                </tbody>
            </table>
        </div>
    </TableTemplate>
</Table>
```

### CSS Variable Integration Points

```css
/* Developers can use CSS variables for consistent theming */
.custom-table {
    --table-border: var(--border);
    --table-header-bg: hsl(var(--muted) / 0.5);
    --table-row-hover: hsl(var(--muted) / 0.3);
    --table-row-selected: hsl(var(--accent));
}

.custom-table thead {
    background-color: var(--table-header-bg);
}

.custom-table tbody tr:hover {
    background-color: var(--table-row-hover);
}

.custom-table tbody tr[aria-selected="true"] {
    background-color: var(--table-row-selected);
}
```

---

## 10. RTL Support

### Logical Properties Guidance

Documentation will guide developers to use logical properties:

```css
/* Good - RTL aware */
.table-cell {
    padding-inline-start: 1rem;
    padding-inline-end: 1rem;
    text-align: start;
}

/* Bad - Not RTL aware */
.table-cell {
    padding-left: 1rem;
    padding-right: 1rem;
    text-align: left;
}
```

### Sort Indicator Directionality

```razor
@* Sort indicator component *@
<span class="inline-flex items-center">
    @if (Direction == SortDirection.Ascending)
    {
        <svg class="h-4 w-4 @(IsRtl ? "rotate-180" : "")" />
    }
    else if (Direction == SortDirection.Descending)
    {
        <svg class="h-4 w-4 @(IsRtl ? "rotate-180" : "")" />
    }
</span>
```

---

## 11. File Structure

```
/src/BlazorUI.Primitives/
  /Primitives/
    /Table/
      Table.razor
      Table.razor.cs
      TableHeader.razor
      TableHeader.razor.cs
      TableHeaderCell.razor
      TableHeaderCell.razor.cs
      TableBody.razor
      TableBody.razor.cs
      TableRow.razor
      TableRow.razor.cs
      TableCell.razor
      TableCell.razor.cs
      TablePagination.razor
      TablePagination.razor.cs
      /Models/
        TableContext.cs
        ColumnDefinition.cs
        TableState.cs
        SortingState.cs
        PaginationState.cs
        SelectionState.cs
        TableRenderContext.cs
        CellContext.cs
      /Enums/
        SortDirection.cs
        SelectionMode.cs
      /Extensions/
        TableDataExtensions.cs
      /Utilities/
        TableKeyboardNavigator.cs
```

---

## 12. Technical Challenges & Solutions

### Challenge 1: State Management Complexity

**Problem:** Coordinating sorting, pagination, and selection state can lead to bugs.

**Solution:**
- Use immutable state updates
- Single source of truth via TableContext
- Clear state flow with NotifyStateChanged pattern
- Comprehensive state validation in setters

### Challenge 2: Generic Type Constraints in C#

**Problem:** C# generics are more restrictive than TypeScript, making dynamic column accessors challenging.

**Solution:**
- Use `IColumnDefinition<TData>` interface for type erasure
- Provide both `Func<TData, TValue>` and expression tree APIs
- Use object boxing for heterogeneous column collections
- Leverage covariance where appropriate

### Challenge 3: Performance with Blazor's Rendering

**Problem:** Large tables can cause performance issues with Blazor's change detection.

**Solution:**
- Implement `ShouldRender()` to minimize re-renders
- Use `@key` directive for row identity tracking
- Process data only when state changes
- Defer virtualization to Phase 3
- Provide pagination guidance (recommend 10-100 items per page)

### Challenge 4: Type-Safe Column Accessors

**Problem:** Need compile-time safety for property access while maintaining flexibility.

**Solution:**
- Primary API uses `Func<TData, TValue>` delegates
- Optional expression tree builder for property expressions
- Runtime validation with clear error messages
- IntelliSense-friendly API design

---

## 13. Testing Strategy

### Manual Testing Matrix

| Feature | Server | WebAssembly | Hybrid |
|---------|--------|-------------|--------|
| Basic Rendering | ✓ | ✓ | ✓ |
| Sorting | ✓ | ✓ | ✓ |
| Pagination | ✓ | ✓ | ✓ |
| Selection | ✓ | ✓ | ✓ |
| Keyboard Nav | ✓ | ✓ | ✓ |
| ARIA | ✓ | ✓ | ✓ |
| Large Dataset (1000+ rows) | ✓ | ✓ | ✓ |
| State Binding | ✓ | ✓ | ✓ |

### Test Scenarios

1. **Basic Functionality**
   - Table renders with data
   - Columns display correctly
   - Empty state shows when no data

2. **Sorting**
   - Click header to sort ascending
   - Click again for descending
   - Click third time to remove sort
   - Sort works with different data types

3. **Pagination**
   - Navigate between pages
   - Change page size
   - Boundary conditions (first/last page)
   - Total count updates correctly

4. **Selection**
   - Single selection mode
   - Multiple selection mode
   - Select all functionality
   - Clear selection

5. **Accessibility**
   - Tab navigation through headers
   - Enter/Space to sort
   - Screen reader announces sort state
   - ARIA attributes present

---

## 14. Demo Page Requirements

### Location
`/demo/BlazorUI.Demo/Pages/Primitives/TablePrimitive.razor`

### Examples to Include

1. **Basic Table**
   ```razor
   <h3>Basic Table</h3>
   <BasicTableExample />
   ```

2. **Sortable Table**
   ```razor
   <h3>Table with Sorting</h3>
   <SortableTableExample />
   ```

3. **Paginated Table**
   ```razor
   <h3>Table with Pagination</h3>
   <PaginatedTableExample />
   ```

4. **Selectable Table**
   ```razor
   <h3>Table with Row Selection</h3>
   <SelectableTableExample />
   ```

5. **Custom Styled Table**
   ```razor
   <h3>Custom Styled Table (shadcn style)</h3>
   <CustomStyledTableExample />
   ```

6. **Controlled State Example**
   ```razor
   <h3>Externally Controlled Table</h3>
   <ControlledTableExample />
   ```

---

## 15. Implementation Order

### Phase 1 MVP Implementation Steps

1. **Core Infrastructure** (2-3 days)
   - Create Table.razor and Table.razor.cs
   - Implement TableContext and base state classes
   - Set up CascadingValue pattern

2. **Column Definition System** (2 days)
   - Create ColumnDefinition classes
   - Implement IColumnDefinition interface
   - Add accessor and comparer logic

3. **State Management** (2-3 days)
   - Implement TableState container
   - Create SortingState with single-column sort
   - Create PaginationState with basic navigation
   - Create SelectionState with single/multi modes

4. **Sorting Implementation** (2 days)
   - Add sort toggle logic
   - Implement data sorting pipeline
   - Create sort indicators
   - Add keyboard support for headers

5. **Pagination Implementation** (2 days)
   - Create TablePagination component
   - Implement page navigation logic
   - Add page size selector
   - Calculate total pages and boundaries

6. **Selection Implementation** (2 days)
   - Add selection checkboxes
   - Implement select all functionality
   - Handle single vs multi-select modes
   - Add keyboard selection support

7. **Accessibility** (1-2 days)
   - Add comprehensive ARIA attributes
   - Implement keyboard navigation
   - Add focus management
   - Test with screen readers

8. **Demo Page** (1-2 days)
   - Create demo page structure
   - Build 6 example scenarios
   - Add sample data generators
   - Document usage patterns

9. **Documentation** (1 day)
   - Write XML comments for public APIs
   - Create README section
   - Document migration path for future styled components
   - Add usage examples

---

## 16. Architecture Impact Assessment

### Impact Level: **Minor**

The table primitive follows established patterns in the BlazorUI library:

- Uses same context/state management pattern as existing primitives
- Follows headless component philosophy
- Consistent with file organization structure
- No new dependencies or architectural patterns

### Updates to architecture.md

Add to "Component Categories" section:
```markdown
**7. Data Components**
- Table (primitive with sorting, pagination, selection)
```

Add to "Future Enhancements" Phase 3:
```markdown
- Table with sorting/filtering ✓ (primitive complete)
- Data table with virtualization (planned)
```

### No ADR Required

This implementation follows existing architectural patterns and doesn't introduce significant changes requiring an Architecture Decision Record.

---

## 17. Performance Considerations

### Rendering Optimization

```csharp
// Use ShouldRender to minimize updates
protected override bool ShouldRender()
{
    // Only render when state actually changes
    return stateVersion != lastRenderedVersion;
}
```

### Data Processing Efficiency

```csharp
// Process data only when needed
private int dataVersion = 0;
private int lastProcessedVersion = -1;

private void ProcessData()
{
    if (lastProcessedVersion == dataVersion)
        return; // Skip if already processed

    // Process data
    processedData = ApplyTransformations(Data);
    lastProcessedVersion = dataVersion;
}
```

### Memory Management

- Don't cache all pages in memory
- Use IQueryable for database-backed tables
- Dispose of event subscriptions properly
- Clear selection when data changes

### Pagination Limits Guidance

Recommended limits:
- **Default page size:** 10 items
- **Maximum page size:** 100 items
- **Large datasets:** Use server-side pagination (Phase 4)
- **Virtual scrolling:** For 1000+ items (Phase 3)

---

## 18. Documentation Requirements

### XML Comments Example

```csharp
/// <summary>
/// A headless table primitive that provides sorting, pagination, and selection capabilities.
/// </summary>
/// <typeparam name="TData">The type of data displayed in the table.</typeparam>
/// <remarks>
/// This is a headless component - it provides logic and state management but no styling.
/// Developers have complete control over the HTML structure and CSS classes.
/// </remarks>
/// <example>
/// <code>
/// &lt;Table TData="Person" Data="@people" Columns="@columns"&gt;
///     &lt;HeaderTemplate&gt;...&lt;/HeaderTemplate&gt;
///     &lt;RowTemplate&gt;...&lt;/RowTemplate&gt;
/// &lt;/Table&gt;
/// </code>
/// </example>
public partial class Table<TData> : ComponentBase where TData : class
```

### README Section

```markdown
## Table Primitive

A powerful, headless table component inspired by TanStack Table.

### Features
- Type-safe column definitions
- Single-column sorting
- Client-side pagination
- Row selection (single/multi)
- Complete markup control
- Keyboard navigation
- ARIA accessibility

### Basic Usage
[Include code example]

### Advanced Examples
See the demo page for complete examples including:
- Custom styling with Tailwind
- Controlled state management
- Complex data types
```

### Migration Path Documentation

```markdown
## Future: Styled Table Component

When the styled table component is released, migration will be simple:

### From Primitive (current):
```razor
<Table TData="Product" Data="@products">
  <HeaderTemplate><!-- custom markup --></HeaderTemplate>
</Table>
```

### To Styled Component (future):
```razor
<StyledTable TData="Product" Data="@products" />
```

The styled component will use the same primitive internally.
```

---

## Risk Assessment Summary

### Technical Risks

1. **Performance at scale** (Medium)
   - Mitigation: Clear pagination limits, virtualization in Phase 3

2. **State synchronization bugs** (Medium)
   - Mitigation: Immutable updates, comprehensive testing

3. **Browser compatibility** (Low)
   - Mitigation: Use standard HTML/CSS, test across browsers

4. **Type system limitations** (Low)
   - Mitigation: Well-designed interfaces, clear documentation

### Timeline Risks

1. **Scope creep** (Medium)
   - Mitigation: Strict Phase 1 scope, defer advanced features

2. **Testing overhead** (Low)
   - Mitigation: Manual testing matrix, automated tests in future

---

## Success Metrics

- Table renders correctly in all 3 Blazor hosting models
- Sorting, pagination, and selection work without bugs
- Demo page shows 6 different usage scenarios
- API is intuitive and well-documented
- Performance is acceptable for 100-500 rows
- Accessibility passes WCAG 2.1 AA standards

---

## Conclusion

This technical plan provides a comprehensive blueprint for implementing a powerful, flexible data table primitive that follows TanStack Table's philosophy while being idiomatic to C# and Blazor. The headless approach ensures maximum flexibility for developers while maintaining consistency with the BlazorUI architecture.

The implementation focuses on Phase 1 MVP features, with clear paths for future enhancements. By following existing patterns in the codebase and leveraging Blazor's component model effectively, we can deliver a high-quality table primitive that meets all specifications.

---

**Plan Status:** Complete and Ready for Task Breakdown
**Next Step:** Generate atomic tasks using Task Planner agent
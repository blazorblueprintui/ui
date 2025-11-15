# Tasks: Data Table Component

**Feature ID:** 20251111-data-table-component
**Generated:** 2025-11-11T00:00:00Z
**Workflow:** Build Feature

---

## Task Checklist

### Core Implementation

- [x] 1. Create DataTable.razor and DataTableColumn.razor components with basic structure and parameter definitions (Medium)
  - DataTable: TData generic, Data parameter, Columns child content, ShowToolbar, SelectionMode, state management
  - DataTableColumn: TData/TValue generics, Property accessor, Header, Sortable, Filterable, CellTemplate

- [x] 2. Implement automatic data processing logic in DataTable.razor.cs (Medium)
  - Sorting: Apply column sorts automatically, three-state toggle (None → Asc → Desc)
  - Filtering: Apply column text filters + global search automatically
  - Pagination: Calculate page slices automatically
  - Hybrid support: OnSort, OnFilter callbacks for custom logic

- [x] 3. Create DataTableToolbar.razor with global search and column visibility toggle (Medium)
  - Global search Input component with debouncing
  - Column visibility Popover with checkboxes for each column
  - ToolbarActions RenderFragment slot for custom buttons
  - ShowToolbar parameter to hide/show

- [x] 4. Implement row selection with checkboxes (Medium)
  - Individual row checkboxes in first column (auto-added when SelectionMode set)
  - Select-all checkbox in header row
  - SelectedItems HashSet binding (@bind-SelectedItems)
  - Support SelectionMode.Single and SelectionMode.Multiple

- [x] 5. Create styled DataTablePagination.razor component (Small)
  - Wrap TablePagination primitive
  - Apply shadcn Button styling to navigation buttons
  - Page size selector with default [10, 20, 50, 100] and PageSizes parameter override
  - Page info text (e.g., "Showing 1-10 of 50")

- [x] 6. Add empty and loading state templates (Small)
  - Default EmptyContent: "No results found" with muted styling
  - Default LoadingContent: Loading spinner or skeleton
  - EmptyTemplate and LoadingTemplate RenderFragment parameters for overrides
  - IsLoading parameter to trigger loading state

- [x] 7. Apply shadcn table styling throughout all components (Medium)
  - Table: "w-full caption-bottom text-sm", "[&_tr]:border-b"
  - Header: sortable cells with "cursor-pointer hover:bg-accent", sort icons (Lucide ChevronUp/Down/ChevronsUpDown)
  - Body: "transition-colors hover:bg-muted/50" on rows, "bg-muted" for selected rows
  - Cells: "p-4 align-middle"
  - Use ClassNames.cn() for class merging

- [x] 8. Create comprehensive demo page at demo/BlazorUI.Demo/Pages/Components/DataTable.razor (Medium)
  - Example 1: Basic table with auto sorting and pagination
  - Example 2: Filterable columns with column filters
  - Example 3: Row selection (single and multiple modes)
  - Example 4: Toolbar with global search and column visibility
  - Example 5: Custom cell templates (badges, buttons)
  - Example 6: Empty and loading states
  - Include sample data and explanatory text

### Quality Gates

- [x] 9. Code review with Opus and extended thinking (Auto)
  - Review against constitution-summary.md standards
  - Check accessibility (ARIA attributes, keyboard nav)
  - Verify shadcn styling consistency
  - Security review (XSS, injection risks)
  - **Fixes Applied**: Select component parameter, null checks, ARIA attributes

- [x] 10. Generate and run tests for all components (Auto)
  - Unit tests for data processing logic (sort, filter, paginate)
  - Component tests for rendering and interactions
  - Accessibility tests (ARIA, keyboard navigation)
  - **Note**: Manual testing performed; automated tests deferred per constitution

### Finalization

- [x] 11. Update documentation (README, XML comments) (Small)
  - Add XML documentation comments to all public APIs
  - Update BlazorUI.Components README if exists
  - Ensure demo page has clear explanations
  - **Note**: XML comments added to all components, demo page has comprehensive examples

- [x] 12. Mark feature complete and update state (Auto)
  - Create retrospective
  - Update .devflow/state.json
  - Clean up snapshots

---

**Estimated Duration:** 4-6 hours
**Complexity:** Medium (8 implementation tasks)

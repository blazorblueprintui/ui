# DataGrid enhancement plan

Drafted 2026-08-27. Covers five issues, to be run as one sweep.

| # | Issue | Size | Depends on |
|---|---|---|---|
| 1 | [#463](https://github.com/blazorblueprintui/ui/issues/463) sort/filter on the displayed value | S | — |
| 2 | [#456](https://github.com/blazorblueprintui/ui/issues/456) enterprise row selection | S | — |
| 3 | [#412](https://github.com/blazorblueprintui/ui/issues/412) nested multi-level grouping | L | #463 |
| 4 | [#497](https://github.com/blazorblueprintui/ui/issues/497) inline and cell editing | XL | — |
| 5 | [#498](https://github.com/blazorblueprintui/ui/issues/498) export visible rows to CSV | S | #463 |

Two of these (#497, #498) were raised from this plan and did not exist before it.

## Where the grid actually stands

Worth stating, because it changes what is worth building. `BbDataGrid` already has:
virtualization, server-side `ItemsProvider`, hierarchy/tree rows with paged lazy children,
single-level grouping (client and server), multi-column sort, per-column filtering, global
search, column resize, reorder, visibility, **pinning**, **aggregates/footer**, detail rows,
row context menu, keyboard navigation, sticky header.

That is a strong feature set. The gaps below are narrow and specific, not foundational.

Confirmed absent: **inline/cell editing** and **export**. Everything else a mainstream Blazor
grid offers is already here in some form.

---

## Ordering, and why

```
#463  sort/filter selector  ──┬──→  #412  nested grouping   (reuses the selector)
                              └──→  #498  CSV export        (exports display values)

#456  selection behaviour     ─────  independent, small
#497  inline editing          ─────  independent, extra large
```

`#463` is the keystone, and goes first. Not because it is the loudest, but because two other
items consume its selector: `#412`'s own design notes say grouping has the same defect and wants
the same selector, and `#498` has to export what the column displays. Building grouping first
means building that selector twice, the second time through a recursive code path.

`#456` is small, has an agreed design, and touches hit-testing that `#412` also touches. Doing
it before the grouping re-architecture keeps the two apart.

---

## 1. `#463` — sort and filter on a value the column does not display

**The defect.** A column with a `CellTemplate` resolving a key to a label still sorts and filters
on the raw property. The user sees `Active / Cancelled / Pending`, clicks the header, and the rows
reorder by `StatusId`. It reads as a broken grid rather than a template limitation.

**Shape.** One selector, not two:

```razor
<BbDataGridPropertyColumn Property="x => x.StatusId" Title="Status"
                          SortAndFilterBy="x => statuses[x.StatusId].StatusName">
```

Settled deliberately as **one** parameter. Separate `SortBy`/`FilterBy` invites them to disagree,
which is confusing to debug and impossible to explain. The split stays available later; merging
two into one afterwards is not.

**Open decisions**
- **Server-side.** A client `Func` cannot be translated by `ItemsProvider`. Either take
  `Expression<Func<TData, object>>` so it can project into a query, or document it as
  client-side only and log when one is set alongside `ItemsProvider`. Prefer the expression.
- **Grouping** must consume the same selector — see step 3.

**Done when:** header click orders by the visible text, the filter compares against the visible
text, and a grouped column groups by it too.

---

## 2. `#456` — enterprise row selection

**Shape.** A small enum, as already agreed with the reporter on the issue:

```csharp
public enum DataGridSelectionBehavior { Toggle, Replace }
```

`Toggle` is today's behaviour and stays the default, so nothing breaks. `Replace` gives:

| Input | Result |
|---|---|
| Click | clear selection, select this row; if already the only selection, clear it |
| Shift+Click | select the range from the anchor to this row |
| Ctrl/Cmd+Click | add or remove this row, leave the rest |

**Deliberately not** a pluggable strategy. A public extension point in the middle of selection,
hit-testing and keyboard nav would constrain steps 1 and 3, and it is close to permanent once
someone implements one.

**Watch:** the anchor row. Shift+click extends from the last *anchor*, not the last *selected* row
— those differ after a Ctrl+click. Getting this wrong is the usual bug, and it is what makes the
feature feel wrong rather than broken. Needs an explicit anchor field, reset on plain click.

Also decide interaction with the checkbox `BbDataGridSelectColumn`: checkbox clicks should stay
`Toggle` semantics even under `Replace`, or checkboxes become unusable.

---

## 3. `#412` — nested multi-level grouping

The largest of the three, and a re-architecture of group state rather than an increment.

**What is in the way** (from the issue, all still true):
- `DataGridGroupState.ActiveGroup` is a single `GroupDefinition?`
- `GroupDefinition` has no parent/child/level/order
- the grouping pass is one flat `GroupBy`
- `DataGridGroupRow<TData>` has no `Depth`/`Children`
- collapsed groups are a flat `HashSet<object>` of raw keys, so nested groups sharing a leaf key
  collide — needs composite path keys
- `DataGridGroupedResult<TData>` (server contract) is flat with one `object? Key`

**Design**
- `ActiveGroup` → ordered `ActiveGroups`, keeping the single-level members as shims.
- Collapsed keys → a `GroupPath` record with value equality; compute ancestor-collapse at render
  rather than materialising descendants.
- Recursive grouping; group rows carry all descendant leaf items so aggregates roll up.
- Server contract stays **flat**, each row carrying its full group path. That matches what
  `GROUP BY a, b` returns and avoids inventing a recursive wire shape.
- Surface: past two levels a per-column menu is unusable. A grouping breadcrumb
  (`Department › Status ›`) with remove and reorder is the likelier control.

**Pagination is the hard part.** A page boundary can land mid-subtree, so every open ancestor
header must be re-emitted at the top of the next page or rows lose their context. This is where
the subtle bugs will be. Budget for it explicitly.

**Fold in the known bug:** grouping + `Virtualize` + `ItemsProvider` renders an empty grid.
`#411` suppressed the menu action there and logs a warning, but the gap stands. Nesting changes
the render-list shape anyway, so fix it here rather than leaving a second warning behind.

---

## 4. `#497` — inline / cell editing

**Why.** The clearest remaining gap against Radzen, Syncfusion and MudBlazor, which all ship
inline and cell editing with validation. It is also the underlying need behind `#464`: that
reporter wants a JS-free `BbNumericInput` because they are hand-building an editable grid out of
200 numeric inputs. A real editing mode serves them better than the thing they asked for.

**Shape**

```razor
<BbDataGrid EditMode="DataGridEditMode.Row" OnRowCommit="Save">
    <BbDataGridPropertyColumn Property="x => x.Name">
        <EditTemplate>
            <BbInput @bind-Value="context.Name" />
        </EditTemplate>
    </BbDataGridPropertyColumn>
```

- `EditMode`: `None` (default), `Cell`, `Row`.
- `EditTemplate` per column; fall back to a type-appropriate editor when absent.
- `OnRowCommit` / `OnRowCancel`, both cancellable, returning the edited item.
- Validation through the existing `EditContext` so `DataAnnotations` work unchanged.
- Keyboard: Enter commits, Escape cancels, Tab moves cell to cell in `Cell` mode.

**Risks.** Editing interacts with virtualization (an editor scrolled out of view must not lose
its buffer), with sorting (a row edited into a new sort position should not jump under the
cursor mid-edit), and with the row context menu. Scope the first pass to `Row` mode only if
that keeps it landable — `Cell` mode is where most of the complexity is.

**Size:** the largest item here, comfortably larger than `#412`. It deserves its own issue and
its own plan.

## 5. `#498` — export visible rows to CSV

Small, self-contained, and asked for by every grid consumer eventually.

```razor
<BbDataGrid ShowExport="true" ExportFileName="employees.csv" />
```

- Export the **visible, sorted, filtered** rows by default, not the raw source. That is what
  people mean, and getting it wrong is the usual complaint.
- Use each column's display value — so it must consume `#463`'s selector. Another reason `#463`
  goes first.
- `ExportAsync()` on the grid ref for programmatic use.
- Respect column visibility and order.
- Excel is deliberately out of scope: it needs a dependency, and CSV covers the reported need.

**Watch:** CSV injection. A cell starting `=`, `+`, `-` or `@` executes as a formula in Excel.
Prefix with an apostrophe. This is a security issue, not a formatting one.

---

## Not included, and why

- **`#464` NumericInput JS opt-out** — likely obviated by item 4. Revisit after editing lands
  rather than building both.
- **Excel export, PDF export** — dependency cost, no reported demand.
- **Column groups / banded headers** — not requested, and pinning already covers the common case.
- **`#486` attribute splatting** — repo-wide, already has PR #490 open.

## Sequencing for the sweep

**Wave 1 — #463 + #456.** Both small. Both fix things that currently read as bugs rather than
missing features: a column that sorts by an invisible key, and a multi-select that ignores every
convention the user brought from their file explorer. Ship together.

**Wave 2 — #498.** Small, and #463 has just landed the selector it needs.

**Wave 3 — #412 and #497.** Each is a multi-week piece and each deserves its own plan. They touch
different parts of the grid — group state and render-list shape versus cell hit-testing and edit
buffers — so they can run in parallel given capacity. If choosing one, #497 closes the larger
competitive gap and probably retires #464 with it.

**Do not** start #412 before #463. Grouping needs the same selector, and building it inside the
recursive grouping pass first means building it twice, the second time in the harder place.

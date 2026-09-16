# V4 implementation checkpoint

Authorized scope: require .NET 10 (drop 8/9); DataGrid cell and batch editing; time-slot Scheduler with event editing, recurrence, resources and time zones; TreeSelect/Cascader; upload progress/cancel/retry.

- [x] .NET 10 solution, packaging and host verification; migration documentation.
- [x] Buffered cell/batch grid editing, validation, keyboard entry, save/reject/cancel, demos/tests.
- [x] Scheduler recurrence/time-zone model and time-slot UI with editor/resources, demos/tests.
- [x] TreeSelect and Cascader built on existing selection/overlay components, demos/tests.
- [x] Upload transport lifecycle and UI with progress/cancel/retry, demos/tests.
- [x] Public API snapshots, demo navigation/catalog, changelog and final verification.

Work began on the user's `v4` branch. The completed changes, research and September 15 notes are included in a feature branch for a pull request targeting `v4`, as requested. Package publishing is outside this request.

Initial SDK upgrade: net10.0 throughout the 12 main solution projects, SDK 10.0.400, ASP.NET component packages 10.0.12. Fix newer analyzer findings without weakening warning enforcement. Command's code moved into a .razor.cs file to avoid a new Razor preprocessor parsing failure. MSBuild requires an unsandboxed test/build invocation for local IPC sockets.

Grid design: preserve row editing. Cell/batch modes use an explicit `EditItemFactory` clone so edits are isolated; applications must deep-copy editable nested objects. Stage batch drafts by stable ItemKey, retain drafts on validation/server rejection, and apply changes only after successful commit. Include keyboard entry and focus restoration, and test source isolation. Inline InputGroup actions fit the existing cell width; an inert sizing copy of the pre-edit value keeps automatic column sizing stable. Demos cover text, number, date, select and checkbox editors, initial sorting, ordering after cell/batch commits and validation recovery. New demos use the shared header/example/accessibility/API structure and all appear in the component catalog.

Scheduler: separate `BbScheduler` component with day/week views, resource lanes, overlapping event placement, and independent event/display IANA zones. Ical.Net expands daily/weekly/monthly/yearly RRULEs. Single-occurrence edits use exclusions and independent overrides; series edits preserve the series identity. Skipped DST wall times are rejected; repeated start and end times have independent offset choices. Persistence callbacks receive a proposed cloned collection and can reject it without losing the draft.

Scheduler pointer gestures preview locally and commit once through the same persistence callback. Moving preserves elapsed duration (including overnight events) and other resource assignments; recurring gestures create occurrence exceptions. Top and bottom resize handles snap to SlotMinutes, and Escape cancels a drag. Delete requires an AlertDialog confirmation that distinguishes an event, one occurrence and an entire series. Single-zone mode hides zone selection and edits in the configured display zone without rewriting stored recurrence zones.

The event editor uses `BbDialog`, `BbSelect`, `BbDateTimePicker`, `BbInput`, `BbNumericInput`, `BbCheckbox`, `BbLabel` and `BbButton`. Display-zone and demonstration switches also use Bb components. Floating overlays fall back to their actual anchor for focus restoration when a composed trigger overrides the generated ID. Calendar focus waits locally for popup visibility; browser checks confirm active-day focus, arrow navigation and Escape returning to the date-time trigger inside the event dialog.

TreeSelect supports single/multiple values, leaf-only selection, search and form validation. Its chevron and search row match the standard Bb pickers. Parent checkboxes cascade to all descendants, including collapsed and filtered nodes; branch rollups remain checked or indeterminate when only leaf keys are bound. Search updates while typing and ignores repeated selection notifications produced by tree expansion. The filter remains stable during closing and resets on reopening. Floating overlays wait only for their own exit animation, avoiding a flash while longer child transitions finish. Cascader shares the standard picker styling and supports full-path search, leaf selection, optional branch selection, preselected paths, form validation, arrow-key opening/navigation, Home/End, Enter/Space activation and RTL navigation. Browser checks cover delayed child-column rendering, leaf selection, trigger focus restoration, search filtering and automatic trailing-edge scrolling on opening and branch changes.

Upload handlers receive a cancellation token, a fresh size-limited browser stream and cumulative progress reporting. Three attempts can run concurrently. Failed/canceled files remain available for retry from zero. Retained input elements protect browser file references across subsequent selections; rapid duplicate drop events cannot replace a selected input's file map. Removing files cancels their attempts. Transport exceptions remain available for diagnostics without exposing their details in the default UI.

## Verification

- Release solution build: all 12 projects, zero warnings/errors.
- .NET suite: 260 tests covering API snapshots, conventions, buffered editing, rendered component lifecycles, recurrence/DST, hierarchy indexing/selection and upload cancellation/retry.
- JavaScript suite: 37 tests covering existing drag controls, rapid upload input replacement, overlay exit/focus restoration, calendar focus after popup visibility, filtered tree keyboard navigation and Cascader keyboard interactions/cleanup.
- Release WebAssembly publish with trimming succeeds. WebAssembly AOT compilation was not tested; the optional `wasm-tools` workload is not installed.
- Server, standalone WASM and Auto hosts start. Server and Auto map .NET 10 static assets.
- Local NuGet packages contain net10.0 assemblies and matching Primitives dependencies; no packages were published.
- Browser checks cover grid validation, cell save, staged batch isolation/discard, scheduler resource overlap and Bb editor controls/save, hierarchy selection, and upload progress/failure/cancel/retry.
- Reviewed API snapshot changes before accepting the new surfaces.

Live demos: `/components/datagrid-editing`, `/components/scheduler`, `/components/tree-select`, `/components/cascader`, and the transport section of `/components/file-upload`. Each has permanent source examples and API documentation.

## Application responsibilities and boundaries

Applications provide deep-copy factories for editable grid DTOs and atomic backend persistence for batch saves. Upload handlers implement the real destination and honor cancellation; the demonstration only reads local files. All-day appointments and external calendar synchronization are not part of this change. The migration guide documents the .NET 10 minimum and matching Primitives package required by release builds.

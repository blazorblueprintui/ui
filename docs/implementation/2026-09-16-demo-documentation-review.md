# Component demo documentation review

Reviewed all 121 routed component demo pages on 16 September 2026. The component index, internal Button test route and non-routed example fragments are excluded from this count. Navigation still has one entry per demo page, with shared route aliases consolidated.

## Navigation and version labels

- Restored the main links to Home, Architecture, Primitives, Components, and the expandable sections to Primitives, Components, Charts, Icons, Recipes, Guides. Alphabetical sorting applies to the Components families and their children.
- Kept the wider sidebar, wrapping labels and related-page groups.
- Sidebar `v4` badges identify new demos. Existing components such as Sortable, Sidebar, Carousel and Context Menu are not labelled as new merely because they have additional APIs.
- Compared public APIs against the `components/v3.17.0` release baseline. API references can now show a version badge beside a new component heading or an individual new parameter, method or enum value. Existing members retain their previous status.
- Sortable's API reference marks KeyboardSorting, DragOverlayTemplate, KeyboardInstructions, CanDrop, CanMove and the new SortableHandle component as v4 additions.

## Documentation corrections

- Every reviewed page has examples first, then one standard Accessibility section with ARIA and keyboard guidance, followed by one API Reference section containing its API groups.
- Moved the SortableHandle reference into Sortable's final API Reference section and consolidated other stray or duplicate API cards.
- Added missing API references, including Button, Button Group, Card, Checkbox, Event Calendar, Typography, Command Library and the dedicated Data Grid hierarchy/styling pages.
- Checked documented members and types against the source. Corrected Tooltip.Placement to PopoverPlacement and FieldError.Errors to IEnumerable<string>?.
- Added standard accessibility panels for Date Input and Time Input, and corrected guidance for segmented editing, menu subcomponents, focus handling, selection, reduced motion and form-control keyboard behavior where applicable.

## Time Input

- Replaced the native AM/PM selectors with BbSelect both inline and inside the clock popup. Popup hour, minute and second choices also use BbSelect so their initial values display correctly.
- Kept the current value, localized period labels, read-only/disabled behavior and validation attributes. AM/PM changes preserve the minute value.
- Updated the permanent demo and code snippet to show 12-hour mode explicitly.

## Validation

- Full solution build: zero warnings and zero errors across all three demo hosts.
- Full .NET suite: 305 tests passed, including three new period-selection regression cases covering midnight, noon and an afternoon value.
- All 121 reviewed routes returned rendered HTML with one Accessibility section followed by one API Reference heading, named API groups and no examples or API cards outside their intended sections.
- Rendered navigation checks verified the original main-link order and new-component badge distinctions. The existing navigation check covers 120 distinct sidebar destinations; the separately routed Command Library demonstration is also included in the documentation review.
- All 10 static CSS generation and delivery checks passed.
- Chrome checks confirmed Date Input's standard documentation panels; Sortable's final section order and member-level badges; and Time Input's initial PM value, keyboard AM/PM selection, popup numeric values and synchronization between inline and popup controls.

This records a documentation, source and targeted browser review. It does not claim an assistive-technology certification or complete browser interaction coverage for every component.

## Subsequent organization and recurrence changes

The follow-up split the mobile and motion demos into focused component pages, bringing the reviewed total to 130. The homepage, sidebar and command search now share the same grouped list of 129 distinct demo destinations; Command Library remains separately routed. The shopping composition moved into Recipes, and the sidebar returned to its standard width with version badges aligned at each row's trailing edge. See [implementation progress](2026-09-16-parity-progress.md#focused-demos-recipes-and-recurrence-follow-up) for the final recurrence behavior and validation results.

# Browser interaction checks

These tests drive the real demos in Chromium and WebKit. Each test gets an isolated browser context; failures retain screenshots and traces in the ignored `test-results` directory. There are no automatic retries.

Build the solution while the demos are stopped, then start each host in a separate terminal from the repository root:

```sh
dotnet build BlazorBlueprint.sln
dotnet run --project demos/BlazorBlueprint.Demo.Server --no-build --launch-profile http
dotnet run --project demos/BlazorBlueprint.Demo.Wasm --no-build --launch-profile http
dotnet run --project demos/BlazorBlueprint.Demo.Auto --no-build --launch-profile http
```

The HTTP profiles use ports 7172, 5184 and 5185 respectively. The WebAssembly and Auto HTTPS profiles use 7173 and 7174. Override the test targets with `BB_SERVER_URL`, `BB_WASM_URL` and `BB_AUTO_URL` if needed.

Install the test dependencies and browsers, then run the suite:

```sh
cd tests/browser
npm ci
npx playwright install chromium webkit
npm test
```

To run one host/browser combination, use `npm test -- --project=wasm-chromium`. To use an installed Google Chrome instead of the bundled Chromium, set `BB_CHROMIUM_CHANNEL=chrome`.

If using a separately installed compatible WebKit, set `BB_WEBKIT_EXECUTABLE` to its launcher path. The bundled revision remains the default.

Coverage includes:

- Audit regressions: conditional step order, responsive tab editing, headless menu focus, EditForm field messages, and scroll completion/focus.
- Mobile DateRangePicker preset matching, manual/custom ranges, clearing, and desktop-to-mobile preset synchronization. The state-change fixture also covers externally loaded/reset ranges and custom preset lists.
- World-map data updates, country clicks, theme changes, mobile resizing, pan/zoom, and lazy geometry loading across all three hosts.
- Segmented input editing and validation, nested pickers and focus return.
- TreeSelect expansion, single selection and cascading checkbox selection.
- Nested menu keyboard navigation in LTR and RTL, radio selection and dismissal.
- Scoped theme inheritance into portals, dark mode and consumer class overrides.
- Sortable keyboard reordering and transfers between lists.
- Scheduler full-day scrolling, day/week switching, recurrence editing, backdrop draft retention and Today navigation.
- Mobile Drawer sizing and focus restoration, Select bottom-sheet focus containment, and the Mobile Shop recipe.
- Reduced-motion behavior and the interactive render-state provider.
- Interactive Auto using a Server circuit on the first visit and WebAssembly on a subsequent visit, followed by functional input interaction.

These are targeted interaction regressions, not a screen-reader certification or exhaustive coverage of every component.

## Parameter and lifecycle regressions

`npm run test:state-changes` builds and starts the small test fixture on port 7188, runs Chromium and WebKit, and stops the fixture afterward. It covers signature restoration across modes, Gantt/Pivot redraws and errors, keyboard Tab behavior, ListBox labels, changing chart callbacks, FileUpload resets, and escaped radar tooltips. Run it with demo hosts stopped so its build can update static assets safely. The browser overrides above also apply.

To test an already running fixture, set `BB_STATE_CHANGES_URL`. The fixture project is `fixtures/StateChanges/StateChanges.csproj`; it references the current source projects and is separate from the product demos.

## Accessibility audit

`npm run test:a11y` checks the Server demo in Chromium, so only that host needs to be running. It has two parts:

- `a11y-axe.checks.mjs` runs axe-core with the WCAG 2.0, 2.1 and 2.2 A and AA tags on every component, primitive and chart demo. The routes are read from the demo pages' `@page` directives, so new demos are included automatically. Each page is scanned inside `#main-content`; the header and sidebar are scanned once on their own.
- `a11y-keyboard.checks.mjs` drives Dialog, AlertDialog, Sheet, Popover, DropdownMenu, ContextMenu, Menubar, Select, Combobox, Tabs, Accordion, DatePicker, DataGrid and Tooltip with Tab, Shift+Tab, Enter, Space, Escape and the arrow keys. It checks that focus is visible, moves into overlays, stays inside modals and returns to the trigger on close. Each scenario runs with and without `prefers-reduced-motion: reduce`.

By default the audit runs in report mode: findings are written to `test-results/a11y/report.md` and `report.json`, and the tests pass. Set `BB_A11Y_MODE=enforce` to fail any test with a finding. A test still fails in report mode if a demo element it drives is missing.

```sh
npm run test:a11y
BB_A11Y_MODE=enforce npm run test:a11y
```

`BB_SERVER_URL` and `BB_CHROMIUM_CHANNEL` apply as above.

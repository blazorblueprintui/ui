## What's New in v3.7.3

### Bug Fixes

- **DataGrid Date/DateTime filtering** — Comparison operators (Equals, NotEquals, GreaterThan, LessThan, Between) now use whole-day semantics, treating the selected date as the entire day rather than an exact midnight timestamp. Fixes incorrect filtering when data contains non-midnight time components (#259).

### Improvements

- Bumped **BlazorBlueprint.Primitives** dependency to 3.7.3.

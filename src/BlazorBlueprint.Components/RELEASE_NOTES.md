## What's New in v3.9.3

### Bug Fixes

- **BbFormFieldSelect** — Fixed label click incorrectly opening the dropdown by removing the `For` attribute from the field label
- **BbTagInput** — Fixed stale UI after tag removal by tracking tag count changes in the render-check logic
- **Border radius** — Aligned `--radius-md` and `--radius-sm` calculations with shadcn/ui docs, using proportional scaling (`0.8` / `0.6`) instead of fixed pixel offsets

### Improvements

- Bumped **BlazorBlueprint.Primitives** dependency to v3.9.3

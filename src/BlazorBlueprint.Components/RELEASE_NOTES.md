## What's New in v3.10.1

### Bug Fixes

- **CSS cascade**: Scoped the `@layer bb` layer to BlazorBlueprint's hand-authored component CSS only; Tailwind's bulk output now imports into its native layers so emitted utilities tie with consumer utilities instead of beating them (#318).
- **BbSidebar**: Added a higher-specificity attribute-selector rule so the sidebar's mobile/desktop visibility is authoritative regardless of consumer stylesheet load order (#308).

### Improvements

- Bumped the `BlazorBlueprint.Primitives` dependency to 3.10.1.

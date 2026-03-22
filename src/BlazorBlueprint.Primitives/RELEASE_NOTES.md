## What's New in v3.7.3

### Bug Fixes

- **Filtering** — Date and DateTime filter comparisons now use whole-day semantics. Operators (Equals, NotEquals, GreaterThan, LessThan, Between) treat the selected date as representing the entire day rather than an exact midnight timestamp, fixing incorrect results when data contains non-midnight time components.

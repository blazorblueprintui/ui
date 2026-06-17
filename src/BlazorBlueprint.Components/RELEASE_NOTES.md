## What's New in v3.12.1

### New Features

- **BbCalendar**: Outside-month (previous/next) days are now selectable; choosing one navigates the displayed month to bring it into view, while genuinely disabled days (Min/MaxDate, DisabledDates) stay non-interactive.
- **BbCalendar**: Added an opt-in `AutoFocus` parameter and a public `FocusActiveDayAsync()` method to move keyboard focus to the active day so arrow-key navigation works immediately.

### Bug Fixes

- **BbDatePicker**: Focuses the active day when the popover opens, so arrow keys navigate the calendar right away instead of leaving focus on the trigger.

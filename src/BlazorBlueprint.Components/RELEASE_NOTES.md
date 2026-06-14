## What's New in v3.12.0

### Breaking Changes

- **BbSidebarMenuButton / BbSidebarMenuSubButton**: `IsActive` is now nullable (`bool?`) — update two-way bindings to a nullable field (#365).

### New Features

- **BbDataGridSelectColumn**: Added `SelectAllScope` (with new `DataGridSelectAllScope` enum) to make the header checkbox toggle only the current page instead of prompting across pages (#367).
- **BbMultiSelect**: Added `SingleLine` to keep the trigger at a fixed height, clipping overflowing tags while pinning the "+N more" indicator and chevron (#366).
- **BbAccordionTrigger**: Added `Icon` (`RenderFragment<bool>`) to replace the default chevron with a custom icon, with the open state passed as context (#347).
- **BbThemeSwitcher**: Added `Variant` to set the trigger button's visual variant (defaults to `Outline`) (#363).

### Bug Fixes

- **BbSidebarMenuButton / BbSidebarMenuSubButton**: An explicit `IsActive` now overrides automatic location matching, so `IsActive="false"` keeps an item inactive even when its href matches the URL (#365).
- **Dialog**: The close (X) button now honors `ShowClose` on programmatically opened dialogs (#364).
- **BbInputOTP**: `OnComplete` now fires when the final slot is filled by manual entry (#362).

### Improvements

- **BbCombobox / BbMultiSelect**: Aligned trigger heights to `h-10` to match other form controls (#366).
- **BbDataGrid**: "Select all on this page" from the multi-page menu is now an exclusive action that replaces the entire selection, including rows on other pages (#367).

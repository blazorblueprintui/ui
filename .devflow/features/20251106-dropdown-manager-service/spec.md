# Feature Specification: Dropdown Manager Service

**Created:** 2025-11-06T00:00:00Z
**Feature ID:** 20251106-dropdown-manager-service
**Workflow:** Build Feature (Streamlined)

---

## Description

Implement a global dropdown manager service to ensure only one dropdown component can be open at a time across the entire application. When a user opens a new dropdown (Select, Combobox, DropdownMenu, Popover), any previously open dropdown should automatically close. This fixes the bug where multiple Select dropdowns can be open simultaneously.

---

## Rationale

Currently, each dropdown component manages its own state independently without awareness of other open dropdowns. This creates a poor UX where users can open multiple Select dropdowns at once by clicking different triggers. A centralized dropdown manager ensures consistent "one dropdown at a time" behavior, which is standard UX practice in web applications.

---

## Acceptance Criteria

- ✓ Create a scoped DropdownManagerService in Blazix
- ✓ Service tracks currently open dropdown by unique identifier
- ✓ When a dropdown opens, service automatically closes any previously open dropdown
- ✓ SelectPrimitive integrates with the service
- ✓ Other dropdown primitives (Combobox, DropdownMenu, Popover) can integrate with the service
- ✓ Service registered in DI container for the demo app
- ✓ Multiple Select dropdowns on same page - only one can be open at a time
- ✓ No breaking changes to existing component APIs

---

## Files Affected

- `src/Blazix/Services/DropdownManagerService.cs` - New service to create
- `src/Blazix/Primitives/Select/SelectPrimitive.razor` - Integrate with service
- `demo/BlazorUI.Demo/Program.cs` - Register service in DI
- Potentially: Other dropdown primitives (Combobox, DropdownMenu, Popover, etc.)

---

## Dependencies

- Microsoft.Extensions.DependencyInjection (already available)
- No external packages needed

---

**Next Step:** Auto-generate tasks and begin implementation with `/execute`

# Feature Specification: Item Component

**Created:** 2025-11-13T04:50:00.000Z
**Feature ID:** 20251113-item-component
**Workflow:** Build Feature (Streamlined)

---

## Description

Create a complete Item component system for Blazor, inspired by shadcn/ui's Item component. The component will provide a flexible, composable structure for building list items with support for media (icons/images), content sections (title/description), and action areas. All sub-components will be included: Item, ItemGroup, ItemSeparator, ItemMedia, ItemContent, ItemTitle, ItemDescription, ItemActions, ItemHeader, and ItemFooter. The component will support variant styling (default, outline, muted), size options (default, sm), and polymorphic rendering via AsChild parameter.

---

## Rationale

List items are a fundamental UI pattern used across applications for notifications, settings menus, user lists, and more. A well-designed Item component provides a consistent, accessible pattern for these use cases while remaining flexible enough to adapt to different contexts through composition and variants.

---

## Acceptance Criteria

- [ ] All sub-components implemented (Item, ItemGroup, ItemSeparator, ItemMedia, ItemContent, ItemTitle, ItemDescription, ItemActions, ItemHeader, ItemFooter)
- [ ] Support for variant prop (default, outline, muted) on Item component
- [ ] Support for size prop (default, sm) on Item component
- [ ] AsChild parameter support for polymorphic rendering (e.g., render as `<a>` tag)
- [ ] Demo page created at `/components/item` with multiple examples:
  - Notification list example
  - Settings items example
  - User profile cards example
  - Mixed variants showcase
- [ ] Component added to Components index page (demo/BlazorUI.Demo/Pages/Components/Index.razor)
- [ ] Component added to left navigation menu in MainLayout.razor
- [ ] Consistent styling matching shadcn/ui design patterns
- [ ] Proper accessibility attributes (ARIA, semantic HTML)
- [ ] Dark mode support via CSS variables

---

## Files Affected

**New Files:**
- `src/BlazorUI.Components/Components/Item/Item.razor`
- `src/BlazorUI.Components/Components/Item/Item.razor.cs`
- `src/BlazorUI.Components/Components/Item/ItemGroup.razor`
- `src/BlazorUI.Components/Components/Item/ItemGroup.razor.cs`
- `src/BlazorUI.Components/Components/Item/ItemSeparator.razor`
- `src/BlazorUI.Components/Components/Item/ItemSeparator.razor.cs`
- `src/BlazorUI.Components/Components/Item/ItemMedia.razor`
- `src/BlazorUI.Components/Components/Item/ItemMedia.razor.cs`
- `src/BlazorUI.Components/Components/Item/ItemContent.razor`
- `src/BlazorUI.Components/Components/Item/ItemContent.razor.cs`
- `src/BlazorUI.Components/Components/Item/ItemTitle.razor`
- `src/BlazorUI.Components/Components/Item/ItemTitle.razor.cs`
- `src/BlazorUI.Components/Components/Item/ItemDescription.razor`
- `src/BlazorUI.Components/Components/Item/ItemDescription.razor.cs`
- `src/BlazorUI.Components/Components/Item/ItemActions.razor`
- `src/BlazorUI.Components/Components/Item/ItemActions.razor.cs`
- `src/BlazorUI.Components/Components/Item/ItemHeader.razor`
- `src/BlazorUI.Components/Components/Item/ItemHeader.razor.cs`
- `src/BlazorUI.Components/Components/Item/ItemFooter.razor`
- `src/BlazorUI.Components/Components/Item/ItemFooter.razor.cs`
- `demo/BlazorUI.Demo/Pages/Components/ItemDemo.razor`

**Modified Files:**
- `demo/BlazorUI.Demo/Pages/Components/Index.razor` (add Item component card)
- `demo/BlazorUI.Demo/Shared/MainLayout.razor` (add Item to components submenu)

---

## Dependencies

- TailwindMerge (existing) - for className composition
- Separator component (existing) - used by ItemSeparator
- AsChildSupport pattern (existing in other components) - for polymorphic rendering
- Lucide Icons (existing) - for demo examples
- Button, Avatar, Badge components (existing) - for demo examples

---

**Next Step:** Auto-generate tasks and begin implementation with `/execute`

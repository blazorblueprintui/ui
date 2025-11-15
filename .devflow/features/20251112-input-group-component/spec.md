# Feature Specification: Input Group Component

**Created:** 2025-11-12T00:00:00.000Z
**Feature ID:** 20251112-input-group-component
**Workflow:** Build Feature (Streamlined)

---

## Description

Create a comprehensive InputGroup component system based on Shadcn UI's design that allows displaying additional information, actions, and controls around input and textarea elements. The component will support multiple addon positions (inline-start, inline-end, block-start, block-end) and include specialized sub-components for different use cases including buttons, text, and status indicators.

---

## Rationale

The InputGroup component fills a critical gap in form UX by enabling rich input experiences with contextual actions and information. This matches the Shadcn UI pattern and provides Blazor developers with a flexible, composable system for building sophisticated form inputs with icons, validation states, character counters, action buttons, and more - all while maintaining accessibility and consistent styling.

---

## Acceptance Criteria

- [ ] **InputGroup Component** - Root container with border, focus states, and support for all alignment variants
- [ ] **InputGroupAddon Component** - Flexible addon positioning with inline-start, inline-end, block-start, block-end alignments
- [ ] **InputGroupInput Component** - Pre-styled input replacement that integrates seamlessly with addons
- [ ] **InputGroupTextarea Component** - Pre-styled textarea replacement with proper sizing and alignment
- [ ] **InputGroupButton Component** - Button variant optimized for use within input groups
- [ ] **InputGroupText Component** - Text content wrapper for labels, prefixes, suffixes
- [ ] **Alignment Enum** - InputGroupAlign enum with all four alignment options
- [ ] **Demo Page** - Comprehensive examples including:
  - Icons and text addons (search icons, email symbols, currency prefixes, URL schemes)
  - Action buttons (copy, send, search functionality)
  - Status indicators (loading spinners, validation states, character counters)
  - Form validation (error states with visual feedback)
- [ ] **Navigation Integration** - Add to components index page and navigation menu
- [ ] **Accessibility** - Focus management, ARIA attributes, keyboard navigation
- [ ] **Dark Mode** - Full theme support using CSS variables

---

## Files Affected

**New Files:**
- `src/BlazorUI.Components/Components/InputGroup/InputGroup.razor`
- `src/BlazorUI.Components/Components/InputGroup/InputGroup.razor.cs`
- `src/BlazorUI.Components/Components/InputGroup/InputGroupAddon.razor`
- `src/BlazorUI.Components/Components/InputGroup/InputGroupAddon.razor.cs`
- `src/BlazorUI.Components/Components/InputGroup/InputGroupInput.razor`
- `src/BlazorUI.Components/Components/InputGroup/InputGroupInput.razor.cs`
- `src/BlazorUI.Components/Components/InputGroup/InputGroupTextarea.razor`
- `src/BlazorUI.Components/Components/InputGroup/InputGroupTextarea.razor.cs`
- `src/BlazorUI.Components/Components/InputGroup/InputGroupButton.razor`
- `src/BlazorUI.Components/Components/InputGroup/InputGroupButton.razor.cs`
- `src/BlazorUI.Components/Components/InputGroup/InputGroupText.razor`
- `src/BlazorUI.Components/Components/InputGroup/InputGroupText.razor.cs`
- `src/BlazorUI.Components/Components/InputGroup/InputGroupAlign.cs`
- `demo/BlazorUI.Demo/Pages/Components/InputGroupDemo.razor`

**Modified Files:**
- `demo/BlazorUI.Demo/Pages/Components/Index.razor` (add InputGroup to component list)
- `demo/BlazorUI.Demo/Shared/NavMenu.razor` (add navigation link)

---

## Dependencies

- Existing Input component (for reference and styling consistency)
- Button component (for InputGroupButton styling)
- Label component (for form labels in demos)
- TailwindMerge utility (ClassNames.cn)
- Lucide icons (for demo examples)

---

**Next Step:** Auto-generate tasks and begin implementation with `/execute`

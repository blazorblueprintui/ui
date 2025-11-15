# Constitution Summary

> **Note:** This is a condensed version for quick reference during code reviews. Full details in constitution.md.

## Tech Stack
- **Language:** C# (.NET 8 LTS)
- **Framework:** Blazor (Server, WebAssembly, Hybrid)
- **Database:** None (UI library)

## Coding Standards
- **Style:** .NET SDK built-in analyzers + EditorConfig
- **Naming:** PascalCase for public members, camelCase for private (no underscore prefix)
- **Organization:** Feature-based by component: /Components/Button/Button.razor, organized by UI element

## Testing Requirements
- **Unit Tests:** Manual testing (not configured), N/A coverage
- **Integration Tests:** Manual testing across Blazor hosting models
- **When:** Defer to future implementation

## Security Essentials
- **Auth:** N/A - UI library, consuming apps handle authentication
- **Data:** Use Blazor's @ syntax for HTML encoding, avoid MarkupString
- **Critical:** Validate inputs, encode HTML with @, use Blazor event handlers

## Key Principles
Feature-based components with CSS Variables for theming (following shadcn). Parameter-driven customization with sensible defaults. Accessibility first (WCAG 2.1 AA, ARIA, keyboard navigation). Tailwind CSS integration via utility classes.

---
**For complete details:** See .devflow/constitution.md

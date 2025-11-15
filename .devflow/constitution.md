# Project Constitution

> **Purpose:** This document defines the unchanging principles and standards for this project. It serves as the "law book" that guides all technical decisions throughout development.

**Last Updated:** 2025-11-01

---

## 1. Project Mission & Domain Context

### Mission & Purpose
Create a UI component library for Blazor that is based on shadcn (https://ui.shadcn.com/). The library should be plug and play for the end user and should honor the UI design, style and functionality of shadcn, in Blazor.

### Target Users & Their Needs
Developers who want a consistent modern UI for their application. The library should provide pre-built, production-ready components that match the quality and aesthetic of shadcn's React components.

### Core Domain Concepts
**N/A** - This is a general-purpose UI component library that can be used across any domain. There are no specific business entities or domain concepts.

**Examples of where this library could be used:**
- E-commerce: Product catalogs, checkout flows, admin dashboards
- Healthcare: Patient management, appointment scheduling, medical records
- Finance: Account dashboards, transaction history, portfolio management
- Any web application requiring modern, accessible UI components

### Success Criteria
A plug-and-play library that, when used, mimics the look and feel of https://ui.shadcn.com/ components. Success means:
- Visual parity with shadcn components
- Easy integration into any Blazor project (Server, WebAssembly, or Hybrid)
- Minimal configuration required
- Consistent behavior across all hosting models
- Developer-friendly API and documentation

### Business Rules & Constraints
- **Easy to use:** Components should be intuitive with sensible defaults
- **Standard look and feel:** Maintain visual consistency with shadcn design system
- **Plug and play:** Minimize setup and configuration burden on developers
- **Framework agnostic:** Support all Blazor hosting models (Server, WebAssembly, Hybrid)

### Regulatory & Compliance Requirements
**None** - As a UI component library, there are no specific regulatory or compliance requirements. However, components should follow accessibility standards (WCAG) and web best practices.

---

## 2. Technology Stack

### Programming Language
**C# (.NET 8 LTS)**

### Framework
**Blazor** - Supporting all hosting models:
- Blazor Server
- Blazor WebAssembly
- Blazor Hybrid (MAUI)

Target framework: **.NET 8 (LTS)** for long-term support and stability.

### Database
**None** - This is a pure UI component library with no data persistence layer.

### Additional Technologies
- **Tailwind CSS** - For styling, following shadcn's utility-first approach
- **CSS Variables** - For theming and customization (shadcn's approach)
- **Razor components** - For component markup and logic
- **JavaScript Interop** - For advanced UI interactions when needed

---

## 3. Architecture Patterns

### Primary Architecture
**Feature-based Component Architecture**

Components are organized by feature/component type (Button, Input, Dialog, Card, etc.), with each component being self-contained and reusable.

**Rationale:** This approach aligns with how shadcn organizes components and makes it easy for developers to find and use specific UI elements. Each component is a discrete unit that can be imported independently, reducing bundle size and improving discoverability.

### Key Architectural Principles

1. **Component Isolation**
   - Each component should be self-contained with minimal dependencies
   - Components should not directly depend on other library components unless necessary
   - Use composition over inheritance

2. **Theming via CSS Variables**
   - Follow shadcn's approach: CSS custom properties for colors, spacing, etc.
   - Background/foreground pairing convention (e.g., `--primary` + `--primary-foreground`)
   - Support light/dark mode through CSS variable overrides
   - Allow developers to customize themes via their own CSS or Tailwind config

3. **Parameter-driven Customization**
   - Use Blazor component parameters for behavior and variants
   - Provide sensible defaults to minimize configuration
   - Support common variants (size, color, disabled state, etc.)

4. **Accessibility First**
   - All components should be keyboard navigable
   - Proper ARIA attributes for screen readers
   - Focus management and visual indicators
   - Follow WCAG 2.1 AA standards

5. **Tailwind Integration**
   - Use Tailwind utility classes for styling
   - Rely on consumer's `tailwind.config.js` for customization
   - Provide CSS variable tokens that integrate with Tailwind's theme system

6. **Zero Runtime Dependencies**
   - Minimize JavaScript dependencies
   - Use native Blazor capabilities where possible
   - Only use JS interop when absolutely necessary

---

## 4. Coding Standards

### Style Guide
Follow **Microsoft C# Coding Conventions** with modern .NET patterns.

### Linter
**.NET SDK built-in analyzers** - Rely on Roslyn analyzers included with .NET 8 SDK for code quality.

### Formatter
**EditorConfig** - Use `.editorconfig` file to enforce formatting rules across the codebase.

### Naming Conventions
- **Classes, Interfaces, Methods, Properties:** PascalCase (e.g., `ButtonComponent`, `IsDisabled`)
- **Private fields, local variables, parameters:** camelCase (e.g., `isActive`, `buttonText`)
- **No underscore prefix for private fields** (modern C# convention)
- **Constants:** PascalCase (e.g., `DefaultTimeout`)
- **Component files:** Match component name (e.g., `Button.razor`, `Dialog.razor`)

**Examples:**
```csharp
public class Button  // PascalCase for class
{
    private bool isDisabled;  // camelCase, no underscore

    public string Text { get; set; }  // PascalCase for property

    public void HandleClick(MouseEventArgs args)  // PascalCase for method
    {
        var clickCount = 0;  // camelCase for local variable
    }
}
```

### File Organization
**Feature-based structure:**
```
/Components
  /Button
    Button.razor
    Button.razor.cs
    Button.css (if needed)
  /Input
    Input.razor
    Input.razor.cs
  /Dialog
    Dialog.razor
    Dialog.razor.cs
/Shared
  /Base
    ComponentBase.cs (if needed for shared component logic)
/Themes
  shadcn-base.css
  variables.css
/wwwroot
  /styles
    components.css
```

**Component naming:**
- Razor component files: `ComponentName.razor`
- Code-behind (if used): `ComponentName.razor.cs`
- Keep related files together in component-specific folders

### Code Review Requirements
- All code changes should maintain visual parity with shadcn components
- Verify component works across all Blazor hosting models (Server, WebAssembly, Hybrid)
- Check for proper accessibility (ARIA, keyboard navigation)
- Ensure Tailwind classes are used correctly
- Verify CSS variables follow shadcn's naming convention
- Components should have sensible defaults requiring minimal configuration

---

## 5. Testing Requirements

### Testing Approach
**Manual testing** for initial development phase. Automated testing may be added later as the library matures.

### Unit Testing
- **Framework:** Not configured yet (consider bUnit + xUnit in the future)
- **Coverage Requirement:** N/A for now
- **When to Write:** Defer to future implementation

### Integration Testing
- **Framework:** Manual testing across Blazor Server, WebAssembly, and Hybrid
- **Scope:** Verify components render correctly and function as expected in different hosting models

### End-to-End Testing
- **Framework:** Manual browser testing
- **Coverage:** Test visual appearance, interactions, accessibility, and responsive behavior

### Test Organization
Manual test cases should verify:
- Visual parity with shadcn components
- Functionality (clicks, keyboard navigation, state changes)
- Accessibility (screen reader, keyboard-only navigation)
- Responsiveness across different screen sizes
- Dark mode support
- Cross-browser compatibility (Chrome, Firefox, Safari, Edge)

---

## 6. Security Standards

### Authentication
**N/A** - This is a UI component library. Authentication is the responsibility of the consuming application.

### Authorization
**N/A** - Authorization logic is the responsibility of the consuming application. Components may provide visual states (e.g., disabled buttons) based on authorization, but the logic is external.

### Data Protection
- Follow Blazor's built-in XSS protection mechanisms
- Use `@` syntax for automatic HTML encoding
- Avoid `MarkupString` unless absolutely necessary
- Validate and sanitize user inputs in form components

### Security Best Practices
1. **Input Validation:** Form components should validate user input (required fields, format checks, etc.)
2. **HTML Encoding:** Always encode user-provided content to prevent XSS
3. **Event Handler Safety:** Use Blazor's event handling system (avoid inline JavaScript)
4. **Dependency Scanning:** Regularly update NuGet packages to patch vulnerabilities
5. **CSP Compatibility:** Avoid inline styles/scripts that violate Content Security Policy

### Vulnerability Scanning
- Monitor .NET security advisories
- Keep .NET SDK and NuGet packages up to date
- Use `dotnet list package --vulnerable` to check for known vulnerabilities

---

## 7. Performance Standards

### Performance Requirements
**Standard component render performance** - Components should render efficiently without specific SLAs. Focus on:
- Minimal re-renders (use `ShouldRender()` judiciously)
- Efficient event handlers
- Lazy loading for heavy components (e.g., dialogs, modals)

### Optimization Guidelines
1. **Avoid unnecessary re-renders:** Use `ShouldRender()` to prevent redundant updates
2. **Virtualization for lists:** Use `Virtualize` component for long lists
3. **Code splitting:** Components should be independently loadable
4. **CSS optimization:** Use Tailwind's purge/optimization features
5. **JavaScript interop:** Minimize JS calls, batch when possible

### Monitoring and Metrics
- Monitor component render times during development (browser DevTools)
- Track bundle size (Blazor WebAssembly)
- Watch for memory leaks in long-running applications (Blazor Server)

---

## 8. Deployment Standards

### Deployment Strategy
**NuGet Package Distribution** - Library will be published as a NuGet package for easy consumption.

### CI/CD Pipeline
- **Build:** Automated build on commit (GitHub Actions or similar)
- **Testing:** Run manual test checklist before release
- **Versioning:** Follow Semantic Versioning (SemVer)
- **Publishing:** Automated NuGet package publishing on tag/release

### Environment Configuration
**No environment-specific configuration** - Library is environment-agnostic. Consuming applications handle their own configuration.

### Rollback Procedures
- Use NuGet package versioning for rollback
- Maintain changelog for each release
- Support LTS versions for critical fixes

---

## 9. Documentation Standards

### Code Documentation
- **XML comments** for all public APIs (classes, methods, properties)
- Document component parameters with `<summary>` tags
- Include usage examples in XML comments where helpful
- Keep comments up to date with code changes

**Example:**
```csharp
/// <summary>
/// A button component following shadcn design system.
/// </summary>
/// <param name="Variant">Visual style variant (default, outline, ghost, etc.)</param>
/// <param name="Size">Size variant (sm, md, lg)</param>
/// <param name="Disabled">Whether the button is disabled</param>
public class Button : ComponentBase
{
    // ...
}
```

### API Documentation
- Maintain comprehensive README with:
  - Installation instructions
  - Quick start guide
  - Component usage examples
  - Theming and customization guide
- Component-specific documentation for each UI element
- Link to shadcn docs for design reference

### Architecture Documentation
- Keep `.devflow/architecture.md` updated as the library evolves
- Document major architectural decisions in `.devflow/decisions/` (ADRs)
- Maintain a changelog for version history

---

## 10. Dependencies and Package Management

### Package Manager
**NuGet** - Standard .NET package manager

### Dependency Update Policy
- Keep .NET SDK on the LTS channel (.NET 8)
- Update dependencies regularly for security patches
- Test thoroughly before updating major versions
- Pin specific versions in `.csproj` to ensure reproducible builds

### Allowed Licenses
Prefer permissive open-source licenses:
- **MIT** (preferred)
- **Apache 2.0**
- **BSD**

Avoid:
- GPL (copyleft restrictions)
- Proprietary/commercial licenses

---

## 11. Error Handling and Logging

### Error Handling Strategy
- **User-facing errors:** Display friendly error messages in UI (e.g., invalid form input)
- **Developer errors:** Log to console during development
- **Runtime errors:** Use Blazor's error boundaries to gracefully handle component failures
- **Validation errors:** Provide clear feedback for form validation

### Logging Framework
**Console logging** during development. Consuming applications should use their own logging infrastructure (Serilog, NLog, etc.).

### Log Levels
- **Information:** Component lifecycle events (if needed for debugging)
- **Warning:** Deprecated API usage, non-critical issues
- **Error:** Component rendering failures, unhandled exceptions

### Error Reporting
- Errors should not crash the entire application (use error boundaries)
- Provide meaningful error messages to developers (console logs)
- Document common errors and solutions in library documentation

---

## 12. Version Control Standards

### Branch Strategy
- **main:** Production-ready code, protected branch
- **develop:** Integration branch for ongoing work
- **feature/component-name:** Feature branches for new components
- **fix/issue-description:** Bug fix branches

### Commit Message Format
Follow **Conventional Commits:**
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat:` New component or feature
- `fix:` Bug fix
- `docs:` Documentation changes
- `style:` Formatting, CSS changes
- `refactor:` Code restructuring without behavior change
- `test:` Adding tests
- `chore:` Maintenance tasks

**Example:**
```
feat(button): add button component with variants

- Implemented default, outline, ghost variants
- Added size options (sm, md, lg)
- Supports disabled state
- Includes hover and focus styles

Closes #123
```

### Pull Request Requirements
- Clear description of changes
- Reference related issues
- Verify component works in Blazor Server, WebAssembly, and Hybrid
- Check visual parity with shadcn
- Update documentation if API changes
- Manual testing completed

---

## 13. DevFlow Quality Gates

Configure automated quality checks during feature execution:

### Quality Gate Settings

```yaml
quality_gates:
  per_task_review: true       # Code review after each subtask implementation
  checkpoint_review: true     # Comprehensive review after phase completion
  testing: false              # Automated testing disabled (manual testing for now)
```

### Gate Descriptions

**Per-Task Review:**
- Runs after implementing each individual subtask
- Reviews: code quality, security, constitution compliance
- Model: Opus with extended thinking
- Scope: Narrow (single subtask changes)
- Blocks: Prevents proceeding until approved or max attempts reached

**Checkpoint Review:**
- Runs after completing all parent tasks in a phase section
- Reviews: integration, spec fulfillment, architecture, security, code quality
- Model: Opus with extended thinking
- Scope: Broad (entire phase with multiple parent tasks)
- **Iterative fixing:** Auto-fixes CRITICAL/HIGH issues, stops at MEDIUM/LOW
- **Severity-based:**
  - CRITICAL: Security vulnerabilities, data loss, breaking bugs (auto-fix)
  - HIGH: Spec violations, architecture breaks (auto-fix)
  - MEDIUM: Code quality, potential bugs (ask user)
  - LOW: Style, minor optimizations (ask user)
- **Max 5 review cycles** before user intervention
- **Context-aware:** Monitors tokens, compacts if needed

**Testing:**
- **Disabled for now** - Manual testing approach
- When enabled in future: Generates and validates unit/integration tests

### Disabling Quality Gates

Set any gate to `false` to disable (not recommended):

```yaml
quality_gates:
  per_task_review: true
  checkpoint_review: false   # Disables checkpoint review - faster but may miss phase-level issues
  testing: false             # Disabled until bUnit is configured
```

---

## Notes

- **This document is the single source of truth** for project standards
- All agents and developers must follow these guidelines
- Changes to this document require careful consideration and should be documented
- Reference this document when making technical decisions
- As the library matures, revisit testing and CI/CD sections

---

**Generated by DevFlow** | Last reviewed: 2025-11-01

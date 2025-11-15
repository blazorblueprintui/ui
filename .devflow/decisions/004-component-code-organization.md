# ADR-004: Component Code Organization Pattern

**Status:** Accepted
**Date:** 2025-11-01
**Context:** Button Component (20251101-button-component)

---

## Context

BlazorUI components need a consistent code organization pattern. Blazor supports multiple ways to structure components:
1. Single-file components (`.razor` with `@code` block)
2. Code-behind pattern (`.razor` + `.razor.cs`)
3. Partial classes with separate files

Since this is the first component, the chosen pattern will set the precedent for all future components.

Key considerations:
- Separation of concerns
- Readability and maintainability
- Testing capabilities
- IDE support
- Team collaboration

## Decision

**We will use the code-behind pattern (`.razor` + `.razor.cs`) for all components.**

## Rationale

### Why Code-Behind?

1. **Separation of Concerns**
   - Markup (`.razor`) separated from logic (`.razor.cs`)
   - Easier to focus on one aspect at a time
   - Visual structure immediately clear in `.razor` file
   - Business logic isolated in `.razor.cs` file

2. **Better Readability**
   - Shorter `.razor` files focus on UI structure
   - Logic doesn't clutter markup
   - Easier to understand component purpose at a glance
   - Parameters and methods clearly organized in C# file

3. **Improved Maintainability**
   - Changes to logic don't affect markup file
   - Easier to refactor C# code
   - Clear boundaries for code reviews
   - Less scrolling between markup and code

4. **Testability**
   - Logic in `.razor.cs` is more testable
   - Can unit test methods independently
   - Easier to mock dependencies
   - Separation makes test targets clear

5. **IDE Support**
   - Better IntelliSense in `.razor.cs`
   - Full C# language features
   - Better refactoring tools
   - Easier navigation between files

6. **Enterprise Standard**
   - Common pattern in large Blazor applications
   - Familiar to developers from WPF/XAML background
   - Used in Microsoft examples for complex components

7. **Consistency**
   - All components follow same structure
   - Easy to locate code vs markup
   - Predictable for contributors
   - Establishes clear convention

### How It Works

**Button.razor (Markup):**
```razor
@namespace BlazorUI.Components.Button
@inherits ComponentBase

<button
    type="@ButtonTypeString"
    class="@CssClass"
    disabled="@Disabled"
    @onclick="HandleClick">
    @ChildContent
</button>
```

**Button.razor.cs (Logic):**
```csharp
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Button;

public partial class Button : ComponentBase
{
    [Parameter] public ButtonVariant Variant { get; set; }
    [Parameter] public ButtonSize Size { get; set; }
    // ... more parameters

    private string CssClass => BuildCssClass();
    private string BuildCssClass() { /* logic */ }

    private async Task HandleClick(MouseEventArgs e) { /* logic */ }
}
```

### Trade-offs

**Disadvantages:**
- Two files per component instead of one
- Need to switch between files during development
- Slightly more boilerplate

**Mitigation:**
- Modern IDEs make file switching trivial (F7 in VS, Alt+F7 in Rider)
- File explorer groups files together (Button.razor, Button.razor.cs)
- Benefits outweigh the minor inconvenience
- Consistency more valuable than file count

## Alternatives Considered

### Alternative 1: Single-File Components

**Approach:** All code in `.razor` file with `@code` block.

**Example:**
```razor
@namespace BlazorUI.Components.Button

<button class="@CssClass">@ChildContent</button>

@code {
    [Parameter] public ButtonVariant Variant { get; set; }

    private string CssClass => BuildCssClass();

    private string BuildCssClass()
    {
        // 50+ lines of logic...
    }
}
```

**Pros:**
- Single file to maintain
- No file switching needed
- Simpler for very small components

**Cons:**
- Mixes markup and logic
- Harder to read for complex components
- IntelliSense less robust in `@code` blocks
- Difficult to test logic independently
- Button component has significant logic (CSS class building)

**Decision:** Rejected. Button has too much logic for single file.

### Alternative 2: Partial Classes (Multiple .cs Files)

**Approach:** Split logic across multiple partial classes.

**Example:**
- `Button.razor`
- `Button.razor.cs` (parameters)
- `Button.Logic.cs` (methods)
- `Button.Styles.cs` (CSS building)

**Pros:**
- Maximum separation of concerns
- Very organized for huge components

**Cons:**
- Overkill for most components
- Too many files to navigate
- Harder to understand component structure
- Over-engineered for current needs

**Decision:** Rejected. Unnecessary complexity.

### Alternative 3: Base Class Inheritance

**Approach:** Create `ShadcnComponentBase` with shared logic, inherit in components.

**Example:**
```csharp
public class ShadcnComponentBase : ComponentBase
{
    protected string BuildCssClasses(params string[] classes) { /* ... */ }
}

public partial class Button : ShadcnComponentBase
{
    // Component-specific code
}
```

**Pros:**
- Shared logic centralized
- DRY principle
- Common utilities reusable

**Cons:**
- Premature abstraction (only one component so far)
- Unclear what should be in base class
- Can add later if needed

**Decision:** Deferred. May add base class later if patterns emerge.

## Consequences

### Positive

- Clear separation between UI and logic
- Better IDE support and IntelliSense
- Easier to test component logic
- Improved readability for complex components
- Consistent pattern across all components

### Negative

- Two files per component
- Need to switch files during development
- Slightly more setup for new components

### Neutral

- File count increases but organization improves
- `.razor.cs` files group with `.razor` in file explorer
- Modern IDEs handle file switching well

## Implementation Guidelines

### File Organization

**Per-component folder:**
```
/Components
  /Button
    Button.razor          # Markup only
    Button.razor.cs       # Logic, parameters, methods
    ButtonVariant.cs      # Supporting types
    ButtonSize.cs         # Supporting types
    ButtonType.cs         # Supporting types
```

### Naming Conventions

- **Markup file:** `ComponentName.razor`
- **Code-behind:** `ComponentName.razor.cs`
- **Supporting types:** `TypeName.cs` (in same folder)

### Code-Behind Structure

```csharp
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorUI.Components.ComponentName;

/// <summary>
/// XML documentation for the component.
/// </summary>
public partial class ComponentName : ComponentBase
{
    // 1. Parameters (in logical order)
    [Parameter] public SomeType Prop1 { get; set; }
    [Parameter] public OtherType Prop2 { get; set; }

    // 2. Injected services (if needed)
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    // 3. Private fields
    private string someField;

    // 4. Properties
    private string SomeProperty => ComputeSomething();

    // 5. Lifecycle methods
    protected override void OnInitialized() { }

    // 6. Private methods
    private void HandleSomething() { }

    private string ComputeSomething() { }
}
```

### Markup Structure

```razor
@namespace BlazorUI.Components.ComponentName
@inherits ComponentBase

@* Keep markup focused on structure *@
<element
    attr1="@Prop1"
    attr2="@ComputedValue"
    @onclick="HandleClick">
    @ChildContent
</element>

@* Avoid complex logic in .razor file *@
@* Move logic to .razor.cs *@
```

### When to Use Single-File

Use single-file components only when:
- Component is trivially simple (< 10 lines of code total)
- No logic beyond basic parameter binding
- Unlikely to grow in complexity

**Example of acceptable single-file:**
```razor
@namespace BlazorUI.Components.Separator

<hr class="@CssClass" />

@code {
    [Parameter] public string? Class { get; set; }
    private string CssClass => $"border-t border-border {Class}".Trim();
}
```

## References

- [Blazor component structure](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/)
- [Code-behind pattern in Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/class-libraries)
- [WPF code-behind pattern](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/code-behind-and-xaml-in-wpf) (similar concept)

---

**Related ADRs:**
- None yet (first architectural pattern established)

**Future Considerations:**
- May introduce base class if shared patterns emerge across multiple components
- May introduce helper utilities in `/Shared` folder
- May introduce CSS class builder utility

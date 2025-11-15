# ADR-003: CSS Variables Theming Strategy

**Status:** Accepted
**Date:** 2025-11-01
**Context:** Button Component (20251101-button-component)

---

## Context

BlazorUI needs a theming system that allows developers to customize colors, spacing, and other design tokens. The system should:
- Match shadcn/ui's theming approach
- Support light and dark modes
- Enable runtime theme switching
- Allow customization without rebuilding

shadcn uses CSS custom properties (CSS variables) with HSL color format for its theming system.

## Decision

**We will use HSL-based CSS custom properties matching shadcn's exact approach for theming.**

## Rationale

### Why CSS Variables?

1. **Runtime Theme Switching**
   - No rebuild required to change themes
   - Toggle dark mode by adding `.dark` class
   - Instant theme changes via JavaScript
   - Users can customize themes on the fly

2. **shadcn Parity**
   - Exact same approach as shadcn/ui
   - Direct translation from React to Blazor
   - Maintains visual consistency
   - Easier to reference shadcn docs

3. **HSL Format Benefits**
   - Opacity adjustments: `bg-primary/90` works out-of-the-box
   - Easier color manipulation
   - Better for programmatic color generation
   - More intuitive than RGB

4. **Background/Foreground Pairing**
   - Consistent naming convention
   - Ensures proper contrast
   - Simpler for developers
   - Example: `--primary` + `--primary-foreground`

5. **Tailwind Integration**
   - Seamlessly integrates with Tailwind theme
   - Can use utilities like `bg-primary`, `text-primary-foreground`
   - No conflicts with Tailwind's built-in colors

### How It Works

**Define variables in CSS:**
```css
:root {
  --primary: 222.2 47.4% 11.2%;  /* HSL without wrapping */
  --primary-foreground: 210 40% 98%;
}

.dark {
  --primary: 210 40% 98%;
  --primary-foreground: 222.2 47.4% 11.2%;
}
```

**Use in Tailwind config:**
```javascript
theme: {
  extend: {
    colors: {
      primary: {
        DEFAULT: "hsl(var(--primary))",
        foreground: "hsl(var(--primary-foreground))",
      },
    },
  },
}
```

**Use in components:**
```razor
<button class="bg-primary text-primary-foreground hover:bg-primary/90">
```

**Toggle dark mode:**
```csharp
await JSRuntime.InvokeVoidAsync("document.documentElement.classList.toggle", "dark");
```

### Trade-offs

**Disadvantages:**
- Requires modern browsers (IE11 not supported)
- CSS variable syntax can be confusing initially
- Slightly more complex than static colors
- Need to understand HSL color format

**Mitigation:**
- Target modern browsers only (documented requirement)
- Provide clear examples in documentation
- shadcn uses same approach, so familiar to many developers

## Alternatives Considered

### Alternative 1: Tailwind Theme Only

**Approach:** Use Tailwind's default theme configuration without CSS variables.

**Pros:**
- Simpler setup
- Standard Tailwind approach
- No CSS variable complexity

**Cons:**
- No runtime theme switching
- Requires rebuild for color changes
- Can't toggle dark mode without rebuild
- Doesn't match shadcn's approach
- Less flexible for consumers

**Decision:** Rejected. Loses key benefit of runtime customization.

### Alternative 2: Sass Variables

**Approach:** Use Sass/SCSS variables for theming.

**Pros:**
- Powerful variable system
- Functions and mixins available
- Nested definitions

**Cons:**
- Requires Sass compilation
- No runtime theme switching
- Not how shadcn works
- More complex build process
- Variables not accessible to JavaScript

**Decision:** Rejected. Can't switch themes at runtime.

### Alternative 3: JavaScript Theme Objects

**Approach:** Define themes as JavaScript objects, inject styles dynamically.

**Pros:**
- Programmatic theme management
- Type-safe in TypeScript
- Easy to validate

**Cons:**
- Requires JavaScript for styling
- Performance overhead (runtime injection)
- Not standard CSS approach
- Breaks without JavaScript
- More complex than CSS variables

**Decision:** Rejected. Overcomplicates theming.

### Alternative 4: RGB CSS Variables

**Approach:** Use RGB format instead of HSL.

**Pros:**
- More familiar to some developers
- Standard in many design systems

**Cons:**
- Opacity adjustments harder: `rgba(var(--primary-rgb), 0.9)`
- Tailwind's opacity syntax doesn't work cleanly
- shadcn uses HSL
- Less intuitive for color manipulation

**Decision:** Rejected. HSL is superior for Tailwind integration.

## Consequences

### Positive

- Runtime theme switching works
- Dark mode toggle is instant
- Consumers can customize easily
- Exact shadcn parity
- Tailwind utilities work seamlessly
- Opacity modifiers work (`bg-primary/90`)

### Negative

- IE11 not supported
- Slightly steeper learning curve
- Need to understand HSL format
- More files to manage (CSS variables file)

### Neutral

- Requires JavaScript for dark mode toggle (acceptable)
- CSS variables can be overridden by consumers
- Themes can be distributed as separate CSS files

## Implementation Notes

**CSS Variables File: `shadcn-base.css`**

Location: `src/BlazorUI/wwwroot/styles/shadcn-base.css`

```css
@layer base {
  :root {
    --background: 0 0% 100%;
    --foreground: 222.2 84% 4.9%;
    --primary: 222.2 47.4% 11.2%;
    --primary-foreground: 210 40% 98%;
    --secondary: 210 40% 96.1%;
    --secondary-foreground: 222.2 47.4% 11.2%;
    --destructive: 0 84.2% 60.2%;
    --destructive-foreground: 210 40% 98%;
    --muted: 210 40% 96.1%;
    --muted-foreground: 215.4 16.3% 46.9%;
    --accent: 210 40% 96.1%;
    --accent-foreground: 222.2 47.4% 11.2%;
    --border: 214.3 31.8% 91.4%;
    --input: 214.3 31.8% 91.4%;
    --ring: 222.2 84% 4.9%;
    --radius: 0.5rem;
  }

  .dark {
    --background: 222.2 84% 4.9%;
    --foreground: 210 40% 98%;
    --primary: 210 40% 98%;
    --primary-foreground: 222.2 47.4% 11.2%;
    /* ... dark mode overrides */
  }
}
```

**Tailwind Config Integration:**
```javascript
module.exports = {
  darkMode: ["class"],
  theme: {
    extend: {
      colors: {
        border: "hsl(var(--border))",
        input: "hsl(var(--input))",
        ring: "hsl(var(--ring))",
        background: "hsl(var(--background))",
        foreground: "hsl(var(--foreground))",
        primary: {
          DEFAULT: "hsl(var(--primary))",
          foreground: "hsl(var(--primary-foreground))",
        },
        // ... more colors
      },
    },
  },
}
```

**Consumer Customization Example:**
```css
/* In consumer's app.css */
:root {
  --primary: 200 100% 50%;  /* Custom blue */
  --primary-foreground: 0 0% 100%;
}
```

**Dark Mode Toggle Component:**
```csharp
@inject IJSRuntime JSRuntime

<button @onclick="ToggleDarkMode">Toggle Dark Mode</button>

@code {
    private async Task ToggleDarkMode()
    {
        await JSRuntime.InvokeVoidAsync("eval",
            "document.documentElement.classList.toggle('dark')");
    }
}
```

## Browser Support

**Supported:**
- Chrome 49+
- Firefox 31+
- Safari 9.1+
- Edge 15+

**Not Supported:**
- IE11 (CSS custom properties not supported)

This is acceptable as IE11 is no longer maintained by Microsoft (as of June 2022).

## References

- [CSS Custom Properties (MDN)](https://developer.mozilla.org/en-US/docs/Web/CSS/Using_CSS_custom_properties)
- [shadcn Theming Docs](https://ui.shadcn.com/docs/theming)
- [Tailwind with CSS Variables](https://tailwindcss.com/docs/customizing-colors#using-css-variables)
- [HSL Color Format](https://developer.mozilla.org/en-US/docs/Web/CSS/color_value/hsl)

---

**Related ADRs:**
- ADR-001: Demo App Hosting Model
- ADR-002: Tailwind Integration

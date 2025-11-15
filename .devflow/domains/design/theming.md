# Theming System

**Domain:** Design System
**Last Updated:** 2025-11-01

---

## Overview

The theming system follows shadcn's CSS Variables approach for customization. This allows developers to easily customize colors, spacing, and other design tokens without modifying component code.

## CSS Variables Architecture

### Color System

All colors use HSL format with CSS custom properties:

```css
:root {
  /* Base colors */
  --background: 0 0% 100%;
  --foreground: 222.2 84% 4.9%;

  /* Primary */
  --primary: 222.2 47.4% 11.2%;
  --primary-foreground: 210 40% 98%;

  /* Secondary */
  --secondary: 210 40% 96.1%;
  --secondary-foreground: 222.2 47.4% 11.2%;

  /* Destructive (error/danger) */
  --destructive: 0 84.2% 60.2%;
  --destructive-foreground: 210 40% 98%;

  /* Muted (disabled, subtle) */
  --muted: 210 40% 96.1%;
  --muted-foreground: 215.4 16.3% 46.9%;

  /* Accent (hover states) */
  --accent: 210 40% 96.1%;
  --accent-foreground: 222.2 47.4% 11.2%;

  /* Border and input */
  --border: 214.3 31.8% 91.4%;
  --input: 214.3 31.8% 91.4%;
  --ring: 222.2 84% 4.9%;

  /* Radius */
  --radius: 0.5rem;
}
```

### Dark Mode

Dark mode overrides colors using the `.dark` class:

```css
.dark {
  --background: 222.2 84% 4.9%;
  --foreground: 210 40% 98%;

  --primary: 210 40% 98%;
  --primary-foreground: 222.2 47.4% 11.2%;

  --secondary: 217.2 32.6% 17.5%;
  --secondary-foreground: 210 40% 98%;

  /* ... other dark mode overrides ... */
}
```

## Background/Foreground Pairing Convention

shadcn uses a simple pairing convention:
- `--primary` → `--primary-foreground`
- `--secondary` → `--secondary-foreground`
- `--destructive` → `--destructive-foreground`

**Usage in components:**
```css
.btn-primary {
  background-color: hsl(var(--primary));
  color: hsl(var(--primary-foreground));
}
```

## Tailwind Integration

### tailwind.config.js Setup

Developers must configure Tailwind to use CSS variables:

```javascript
module.exports = {
  darkMode: ["class"],
  content: [
    './Components/**/*.{razor,html}',
    './Pages/**/*.{razor,html}',
  ],
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
        secondary: {
          DEFAULT: "hsl(var(--secondary))",
          foreground: "hsl(var(--secondary-foreground))",
        },
        destructive: {
          DEFAULT: "hsl(var(--destructive))",
          foreground: "hsl(var(--destructive-foreground))",
        },
        muted: {
          DEFAULT: "hsl(var(--muted))",
          foreground: "hsl(var(--muted-foreground))",
        },
        accent: {
          DEFAULT: "hsl(var(--accent))",
          foreground: "hsl(var(--accent-foreground))",
        },
      },
      borderRadius: {
        lg: "var(--radius)",
        md: "calc(var(--radius) - 2px)",
        sm: "calc(var(--radius) - 4px)",
      },
    },
  },
}
```

### Using Theme Colors in Components

```razor
@* Button.razor *@
<button class="bg-primary text-primary-foreground hover:bg-primary/90">
    Primary Button
</button>

<button class="bg-secondary text-secondary-foreground hover:bg-secondary/80">
    Secondary Button
</button>
```

## Pre-configured Color Schemes

Provide 5 base color palettes (matching shadcn):

1. **Neutral** (default)
2. **Stone**
3. **Zinc**
4. **Gray**
5. **Slate**

Each palette includes light and dark mode variants.

## Customization Guide

### For Library Consumers

**1. Create a CSS file with custom color variables:**

```css
/* app.css */
@import 'shadcnblazor/styles.css';

:root {
  /* Override primary color */
  --primary: 200 100% 50%;  /* Custom blue */
  --primary-foreground: 0 0% 100%;
}
```

**2. Or use a pre-configured theme:**

```css
@import 'shadcnblazor/themes/zinc.css';
```

**3. Toggle dark mode programmatically:**

```csharp
// Add/remove 'dark' class to document root
await JSRuntime.InvokeVoidAsync("document.documentElement.classList.toggle", "dark");
```

## Component Parameter Support

Some components may accept theme overrides via parameters:

```razor
<Button Variant="ButtonVariant.Custom" Class="bg-purple-500 text-white">
    Custom Color Button
</Button>
```

## Best Practices

1. **Always use CSS variables** for colors (not hardcoded values)
2. **Follow the pairing convention** (background + foreground)
3. **Test in both light and dark modes**
4. **Use HSL format** for color values (allows easier opacity adjustments)
5. **Provide sensible defaults** so components work out-of-the-box

## Implementation Checklist

- [ ] Define all CSS variables in `variables.css`
- [ ] Create dark mode overrides in `.dark` selector
- [ ] Configure Tailwind to use CSS variables
- [ ] Provide 5 pre-configured color palettes
- [ ] Document customization guide for consumers
- [ ] Test all components in light/dark mode
- [ ] Verify color contrast meets WCAG AA standards

## References

- [shadcn Theming Docs](https://ui.shadcn.com/docs/theming)
- [Tailwind CSS Customization](https://tailwindcss.com/docs/theme)
- [CSS Custom Properties (MDN)](https://developer.mozilla.org/en-US/docs/Web/CSS/Using_CSS_custom_properties)

---

**Note:** This theming system is foundational to all components. Any changes here affect the entire library.

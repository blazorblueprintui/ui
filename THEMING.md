# BlazorUI Theming Guide

BlazorUI uses CSS custom properties (variables) for theming, following the [shadcn/ui](https://ui.shadcn.com) design system. This allows complete customization of colors, typography, and sizing without modifying component code.

## Quick Start

1. Create a theme CSS file (e.g., `wwwroot/styles/themes/my-theme.css`)
2. Define CSS variables for `:root` (light mode) and `.dark` (dark mode)
3. Load your theme **before** `blazorui.css`:

```html
<link href="styles/themes/my-theme.css" rel="stylesheet" />
<link href="_content/BlazorUI.Components/blazorui.css" rel="stylesheet" />
```

## Color Format

BlazorUI uses the [OKLCH color space](https://developer.mozilla.org/en-US/docs/Web/CSS/color_value/oklch) for perceptually uniform colors:

```css
--primary: oklch(0.205 0 0);
/*              │     │ │
                │     │ └── Hue (0-360)
                │     └──── Chroma (0-0.4, saturation)
                └────────── Lightness (0-1) */
```

You can also use any valid CSS color format (hex, rgb, hsl).

---

## Theme Variables Reference

### Core Colors

These are the foundational colors used throughout the library.

| Variable | Description | Used By |
|----------|-------------|---------|
| `--background` | Page/app background | Body, containers |
| `--foreground` | Primary text color | Body text, headings |

```css
:root {
  --background: oklch(1 0 0);           /* White */
  --foreground: oklch(0.145 0 0);       /* Near black */
}

.dark {
  --background: oklch(0.145 0 0);       /* Near black */
  --foreground: oklch(0.985 0 0);       /* Near white */
}
```

---

### Semantic Colors

Colors with specific meaning, each with a foreground variant for text/icons.

| Variable | Description | Used By |
|----------|-------------|---------|
| `--primary` | Primary brand color | Primary buttons, links, focus rings |
| `--primary-foreground` | Text on primary backgrounds | Button text |
| `--secondary` | Secondary/subtle actions | Secondary buttons, badges |
| `--secondary-foreground` | Text on secondary backgrounds | |
| `--destructive` | Dangerous/delete actions | Delete buttons, error states |
| `--destructive-foreground` | Text on destructive backgrounds | |
| `--muted` | Subdued backgrounds | Disabled states, placeholders |
| `--muted-foreground` | Subdued text | Helper text, captions |
| `--accent` | Hover/focus highlights | Menu item hover, keyboard focus |
| `--accent-foreground` | Text on accent backgrounds | |

```css
:root {
  --primary: oklch(0.205 0 0);
  --primary-foreground: oklch(0.985 0 0);

  --secondary: oklch(0.97 0 0);
  --secondary-foreground: oklch(0.205 0 0);

  --destructive: oklch(0.577 0.245 27.325);
  --destructive-foreground: oklch(1 0 0);

  --muted: oklch(0.97 0 0);
  --muted-foreground: oklch(0.556 0 0);

  --accent: oklch(0.97 0 0);
  --accent-foreground: oklch(0.205 0 0);
}
```

---

### UI Element Colors

Colors for specific UI elements.

| Variable | Description | Used By |
|----------|-------------|---------|
| `--border` | Default border color | Cards, inputs, dividers |
| `--input` | Input field borders | Input, Select, Textarea |
| `--ring` | Focus ring color | Focus outlines, keyboard navigation |

```css
:root {
  --border: oklch(0.922 0 0);
  --input: oklch(0.922 0 0);
  --ring: oklch(0.708 0 0);
}
```

---

### Component Colors

Colors for specific component types.

#### Card

| Variable | Description |
|----------|-------------|
| `--card` | Card background |
| `--card-foreground` | Card text color |

```css
:root {
  --card: oklch(1 0 0);
  --card-foreground: oklch(0.145 0 0);
}
```

#### Popover

Used by Popover, DropdownMenu, Tooltip, Select, Combobox, and other floating elements.

| Variable | Description |
|----------|-------------|
| `--popover` | Popover background |
| `--popover-foreground` | Popover text color |

```css
:root {
  --popover: oklch(1 0 0);
  --popover-foreground: oklch(0.145 0 0);
}
```

---

### Alert Colors

Colors for the Alert component variants. **Library provides defaults** - override only if needed.

| Variable | Description |
|----------|-------------|
| `--alert-success` | Success icon/border color |
| `--alert-success-foreground` | Success text color |
| `--alert-success-bg` | Success background |
| `--alert-info` | Info icon/border color |
| `--alert-info-foreground` | Info text color |
| `--alert-info-bg` | Info background |
| `--alert-warning` | Warning icon/border color |
| `--alert-warning-foreground` | Warning text color |
| `--alert-warning-bg` | Warning background |
| `--alert-danger` | Danger icon/border color |
| `--alert-danger-foreground` | Danger text color |
| `--alert-danger-bg` | Danger background |

```css
:root {
  /* Success - Green */
  --alert-success: oklch(0.55 0.20 142);
  --alert-success-foreground: oklch(0.30 0.09 142);
  --alert-success-bg: oklch(0.993 0.003 142);

  /* Info - Blue */
  --alert-info: oklch(0.50 0.20 255);
  --alert-info-foreground: oklch(0.30 0.10 255);
  --alert-info-bg: oklch(0.993 0.003 255);

  /* Warning - Amber */
  --alert-warning: oklch(0.68 0.18 55);
  --alert-warning-foreground: oklch(0.35 0.10 55);
  --alert-warning-bg: oklch(0.995 0.003 55);

  /* Danger - Red */
  --alert-danger: oklch(0.55 0.22 27);
  --alert-danger-foreground: oklch(0.30 0.12 27);
  --alert-danger-bg: oklch(0.993 0.003 27);
}
```

---

### Sidebar Colors

Colors specifically for the Sidebar component.

| Variable | Description |
|----------|-------------|
| `--sidebar-background` | Sidebar background |
| `--sidebar-foreground` | Sidebar text |
| `--sidebar-primary` | Active/selected item |
| `--sidebar-primary-foreground` | Active item text |
| `--sidebar-accent` | Hover state |
| `--sidebar-accent-foreground` | Hover state text |
| `--sidebar-border` | Sidebar borders |
| `--sidebar-ring` | Focus ring in sidebar |

```css
:root {
  --sidebar-background: oklch(0.985 0 0);
  --sidebar-foreground: oklch(0.145 0 0);
  --sidebar-primary: oklch(0.205 0 0);
  --sidebar-primary-foreground: oklch(0.985 0 0);
  --sidebar-accent: oklch(0.97 0 0);
  --sidebar-accent-foreground: oklch(0.205 0 0);
  --sidebar-border: oklch(0.922 0 0);
  --sidebar-ring: oklch(0.708 0 0);
}
```

---

### Chart Colors

Colors for data visualization components.

| Variable | Description |
|----------|-------------|
| `--chart-1` | First chart color |
| `--chart-2` | Second chart color |
| `--chart-3` | Third chart color |
| `--chart-4` | Fourth chart color |
| `--chart-5` | Fifth chart color |

```css
:root {
  --chart-1: oklch(0.81 0.1 252);
  --chart-2: oklch(0.62 0.19 260);
  --chart-3: oklch(0.55 0.22 263);
  --chart-4: oklch(0.49 0.22 264);
  --chart-5: oklch(0.42 0.18 266);
}
```

---

### Sizing & Layout

#### Border Radius

| Variable | Description | Default |
|----------|-------------|---------|
| `--radius` | Base border radius | `0.5rem` |

Components use this as a base:
- `border-radius: var(--radius)` - Large (cards, dialogs)
- `border-radius: calc(var(--radius) - 2px)` - Medium (buttons, inputs)
- `border-radius: calc(var(--radius) - 4px)` - Small (badges, tags)

```css
:root {
  --radius: 0.5rem;    /* Rounded corners */
  /* --radius: 0;      /* Sharp corners */
  /* --radius: 1rem;   /* Very rounded */
}
```

#### Sidebar Dimensions

**Library provides defaults** - override only if needed.

| Variable | Description | Default |
|----------|-------------|---------|
| `--sidebar-width` | Expanded sidebar width | `16rem` |
| `--sidebar-width-mobile` | Mobile sidebar width | `18rem` |
| `--sidebar-width-icon` | Collapsed (icon-only) width | `3rem` |

```css
:root {
  --sidebar-width: 16rem;
  --sidebar-width-mobile: 18rem;
  --sidebar-width-icon: 3rem;
}
```

---

### Typography

| Variable | Description | Default Fallback |
|----------|-------------|------------------|
| `--font-sans` | Sans-serif font stack | `ui-sans-serif, system-ui, sans-serif` |
| `--font-serif` | Serif font stack | `ui-serif, Georgia, serif` |
| `--font-mono` | Monospace font stack | `ui-monospace, monospace` |

```css
:root {
  --font-sans: 'Inter', ui-sans-serif, system-ui, sans-serif;
  --font-mono: 'JetBrains Mono', ui-monospace, monospace;
}
```

---

## Complete Theme Example

Here's a complete theme file with all variables:

```css
/* my-theme.css */

:root {
  /* Core */
  --background: oklch(1 0 0);
  --foreground: oklch(0.145 0 0);

  /* Semantic */
  --primary: oklch(0.205 0 0);
  --primary-foreground: oklch(0.985 0 0);
  --secondary: oklch(0.97 0 0);
  --secondary-foreground: oklch(0.205 0 0);
  --destructive: oklch(0.577 0.245 27.325);
  --destructive-foreground: oklch(1 0 0);
  --muted: oklch(0.97 0 0);
  --muted-foreground: oklch(0.556 0 0);
  --accent: oklch(0.97 0 0);
  --accent-foreground: oklch(0.205 0 0);

  /* UI Elements */
  --border: oklch(0.922 0 0);
  --input: oklch(0.922 0 0);
  --ring: oklch(0.708 0 0);

  /* Components */
  --card: oklch(1 0 0);
  --card-foreground: oklch(0.145 0 0);
  --popover: oklch(1 0 0);
  --popover-foreground: oklch(0.145 0 0);

  /* Sizing */
  --radius: 0.5rem;
}

.dark {
  /* Core */
  --background: oklch(0.145 0 0);
  --foreground: oklch(0.985 0 0);

  /* Semantic */
  --primary: oklch(0.985 0 0);
  --primary-foreground: oklch(0.205 0 0);
  --secondary: oklch(0.269 0 0);
  --secondary-foreground: oklch(0.985 0 0);
  --destructive: oklch(0.396 0.141 25.723);
  --destructive-foreground: oklch(0.985 0 0);
  --muted: oklch(0.269 0 0);
  --muted-foreground: oklch(0.708 0 0);
  --accent: oklch(0.269 0 0);
  --accent-foreground: oklch(0.985 0 0);

  /* UI Elements */
  --border: oklch(0.269 0 0);
  --input: oklch(0.269 0 0);
  --ring: oklch(0.556 0 0);

  /* Components */
  --card: oklch(0.145 0 0);
  --card-foreground: oklch(0.985 0 0);
  --popover: oklch(0.145 0 0);
  --popover-foreground: oklch(0.985 0 0);
}
```

---

## Dark Mode

BlazorUI uses class-based dark mode. Add the `dark` class to the `<html>` element to enable dark mode:

```html
<html class="dark">
```

Toggle with JavaScript:
```javascript
document.documentElement.classList.toggle('dark');
```

Or with Blazor:
```csharp
await JS.InvokeVoidAsync("eval", "document.documentElement.classList.toggle('dark')");
```

---

## CSS Layer Priority

Theme variables defined outside `@layer` always override library defaults:

```
Priority (highest to lowest):
1. Your theme file (no @layer)     ← Your customizations win
2. @layer utilities
3. @layer components
4. @layer base                      ← Library defaults
```

This means you only need to define the variables you want to customize.

---

## Migrating from HSL

If you have an existing shadcn/ui theme using HSL format:

```css
/* Old HSL format */
--primary: 222.2 47.4% 11.2%;

/* New OKLCH format */
--primary: oklch(0.205 0.02 250);
```

Use a color converter tool or keep using HSL - both formats work:

```css
--primary: hsl(222.2 47.4% 11.2%);
```

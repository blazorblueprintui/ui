# Current Theme Token Mapping

**Document Date**: 2025-11-06
**Purpose**: Document existing theme structure before migration to single OKLCH theme pattern

---

## Current Architecture Overview

BlazorUI currently uses a **mixed theme system** with two different approaches:

1. **Base Theme** (`shadcn-base.css`) - HSL format, single theme with `:root` and `.dark`
2. **Custom Themes** (`themes/*.css`) - OKLCH format, multi-theme with `[data-theme="name"]` selectors

### File Structure

```
demo/BlazorUI.Demo/wwwroot/
├── styles/
│   ├── shadcn-base.css          # HSL-based default theme
│   ├── base.css                  # Additional base styles
│   └── themes/
│       ├── claude.css            # OKLCH (current active)
│       ├── ocean.css             # OKLCH
│       ├── sunset.css            # OKLCH
│       ├── clean-slate.css       # OKLCH
│       ├── dark-matter.css       # OKLCH
│       ├── tangerine.css         # OKLCH
│       ├── twitter.css           # OKLCH
│       └── default.css           # OKLCH
└── css/
    ├── app-input.css             # Tailwind v4 input file
    └── app.css                   # Generated CSS output
```

---

## Token Inventory

### 1. Color Tokens (Core)

#### shadcn-base.css (HSL Format)
```css
:root {
  --background: 0 0% 100%;               /* White */
  --foreground: 222.2 84% 4.9%;          /* Near black */
  --card: 0 0% 100%;
  --card-foreground: 222.2 84% 4.9%;
  --popover: 0 0% 100%;
  --popover-foreground: 222.2 84% 4.9%;
  --primary: 222.2 47.4% 11.2%;
  --primary-foreground: 210 40% 98%;
  --secondary: 210 40% 96.1%;
  --secondary-foreground: 222.2 47.4% 11.2%;
  --muted: 210 40% 96.1%;
  --muted-foreground: 215.4 16.3% 46.9%;
  --accent: 210 40% 96.1%;
  --accent-foreground: 222.2 47.4% 11.2%;
  --destructive: 0 84.2% 60.2%;
  --destructive-foreground: 210 40% 98%;
  --border: 214.3 31.8% 91.4%;
  --input: 214.3 31.8% 91.4%;
  --ring: 222.2 84% 4.9%;
}

.dark {
  --background: 222.2 84% 4.9%;          /* Near black */
  --foreground: 210 40% 98%;             /* Near white */
  /* ... similar pattern for all tokens */
}
```

#### claude.css (OKLCH Format)
```css
[data-theme="claude"] {
  --background: oklch(0.9818 0.0054 95.0986);
  --foreground: oklch(0.3438 0.0269 95.7226);
  --card: oklch(0.9818 0.0054 95.0986);
  --card-foreground: oklch(0.1908 0.0020 106.5859);
  --popover: oklch(1.0000 0 0);
  --popover-foreground: oklch(0.2671 0.0196 98.9390);
  --primary: oklch(0.6171 0.1375 39.0427);      /* Warm orange/amber */
  --primary-foreground: oklch(1.0000 0 0);
  --secondary: oklch(0.9245 0.0138 92.9892);
  --secondary-foreground: oklch(0.4334 0.0177 98.6048);
  --muted: oklch(0.9341 0.0153 90.2390);
  --muted-foreground: oklch(0.6059 0.0075 97.4233);
  --accent: oklch(0.9245 0.0138 92.9892);
  --accent-foreground: oklch(0.2671 0.0196 98.9390);
  --destructive: oklch(0.1908 0.0020 106.5859);
  --destructive-foreground: oklch(1.0000 0 0);
  --border: oklch(0.8847 0.0069 97.3627);
  --input: oklch(0.7621 0.0156 98.3528);
  --ring: oklch(0.6171 0.1375 39.0427);
}

[data-theme="claude"].dark {
  --background: oklch(0.2679 0.0036 106.6427);
  --foreground: oklch(0.8074 0.0142 93.0137);
  /* ... dark mode variants */
}
```

### 2. Chart Colors (OKLCH Themes Only)

Present in claude.css and other OKLCH themes, **missing from shadcn-base.css**:

```css
--chart-1: oklch(0.5583 0.1276 42.9956);
--chart-2: oklch(0.6898 0.1581 290.4107);
--chart-3: oklch(0.8816 0.0276 93.1280);
--chart-4: oklch(0.8822 0.0403 298.1792);
--chart-5: oklch(0.5608 0.1348 42.0584);
```

### 3. Sidebar Tokens

#### shadcn-base.css (HSL)
```css
--sidebar-background: 0 0% 98%;
--sidebar-foreground: 240 5.3% 26.1%;
--sidebar-primary: 240 5.9% 10%;
--sidebar-primary-foreground: 0 0% 98%;
--sidebar-accent: 240 4.8% 95.9%;
--sidebar-accent-foreground: 240 5.9% 10%;
--sidebar-border: 220 13% 91%;
--sidebar-ring: 217.2 91.2% 59.8%;

/* Dimensions */
--sidebar-width: 16rem;
--sidebar-width-mobile: 18rem;
--sidebar-width-icon: 3rem;
```

#### claude.css (OKLCH) - Uses Different Token Names
```css
--sidebar: oklch(0.9663 0.0080 98.8792);           /* vs --sidebar-background */
--sidebar-foreground: oklch(0.3590 0.0051 106.6524);
--sidebar-primary: oklch(0.6171 0.1375 39.0427);
--sidebar-primary-foreground: oklch(0.9881 0 0);
--sidebar-accent: oklch(0.9245 0.0138 92.9892);
--sidebar-accent-foreground: oklch(0.3250 0 0);
--sidebar-border: oklch(0.9401 0 0);
--sidebar-ring: oklch(0.7731 0 0);
```

**Inconsistency**: OKLCH themes use `--sidebar` while HSL uses `--sidebar-background`

### 4. Spacing and Radius

Present in all themes:
```css
--radius: 0.5rem;
```

**Missing** (need to be added):
- `--radius-sm`, `--radius-md`, `--radius-lg`, `--radius-xl` (calculated variants)
- Custom spacing tokens

### 5. Font Families

**Completely missing from all current theme files**

Should be added:
```css
--font-sans: "Inter", system-ui, sans-serif;
--font-serif: "Merriweather", serif;
--font-mono: "JetBrains Mono", monospace;
```

### 6. Shadow Scale

**Completely missing from all current theme files**

Should be added:
```css
--shadow-2xs: 0 1px 2px -1px oklch(0% 0 0 / 0.1);
--shadow-xs: 0 2px 4px -2px oklch(0% 0 0 / 0.1);
--shadow-sm: 0 4px 6px -4px oklch(0% 0 0 / 0.1);
--shadow: 0 10px 15px -3px oklch(0% 0 0 / 0.1);
--shadow-md: 0 20px 25px -5px oklch(0% 0 0 / 0.1);
--shadow-lg: 0 25px 50px -12px oklch(0% 0 0 / 0.25);
--shadow-xl: 0 35px 60px -15px oklch(0% 0 0 / 0.3);
--shadow-2xl: 0 50px 100px -20px oklch(0% 0 0 / 0.35);
```

---

## Tailwind Configuration

### Current app-input.css Structure

```css
/* Import Tailwind v4 configuration FIRST */
@config "../../tailwind.config.js";

/* Import Tailwind CSS */
@import 'tailwindcss';

/* Import shadcn base styles with CSS variables */
@import '../styles/shadcn-base.css';

/* Import OKLCH theme files */
@import '../styles/themes/claude.css';
@import '../styles/themes/ocean.css';
@import '../styles/themes/sunset.css';

/* Import base styles */
@import '../styles/base.css';
```

### Theme Switching Mechanism

Themes are switched via JavaScript:
```javascript
document.documentElement.setAttribute('data-theme', 'claude');
document.documentElement.classList.toggle('dark');
```

---

## Problems with Current Structure

### 1. Color Space Mismatch
- **Base theme**: HSL values wrapped in `hsl()` by Tailwind
- **Custom themes**: OKLCH values also wrapped in `hsl()` (invalid!)
- **Result**: `hsl(oklch(0.9818 0.0054 95.0986))` = broken CSS

### 2. Selector Inconsistency
- **Base theme**: Uses `:root` and `.dark` (standard Shadcn)
- **Custom themes**: Uses `[data-theme="name"]` and `[data-theme="name"].dark` (custom pattern)
- **Result**: Cannot use standard Shadcn themes from tweakcn.com

### 3. Token Name Inconsistency
- **Sidebar**: `--sidebar-background` (HSL) vs `--sidebar` (OKLCH)
- **Chart colors**: Present in OKLCH themes, missing in HSL base

### 4. Missing Tokens
- No font family tokens
- No shadow scale tokens
- No calculated radius variants

### 5. Maintenance Burden
- 8 theme files to maintain
- Duplicate token definitions
- Mixed color formats

---

## Migration Path

To achieve Shadcn compatibility, we need:

1. **Single theme file** with `:root` and `.dark` selectors only
2. **OKLCH color space** for all color tokens
3. **Complete token set**: colors, fonts, shadows, spacing
4. **Consistent naming**: Match standard Shadcn token names
5. **Remove** multi-theme switching logic
6. **Simplified** ThemeService for dark mode toggle only

---

## Reference: Standard Shadcn Theme Structure (Target)

```css
@layer base {
  :root {
    /* All tokens in OKLCH */
    --background: oklch(...);
    --foreground: oklch(...);
    /* ... */
    --font-sans: "Inter", system-ui, sans-serif;
    --shadow-xs: ...;
    --radius: 0.5rem;
  }

  .dark {
    /* Dark mode overrides */
    --background: oklch(...);
    --foreground: oklch(...);
    /* ... */
  }
}

@theme inline {
  /* Map to Tailwind utilities */
  --color-background: var(--background);
  --color-foreground: var(--foreground);
  /* ... */
}
```

---

## Backup Location

All current theme files have been backed up to:
```
.devflow/features/20251106-shadcn-theme-compatibility/theme-backup/
```

Files backed up:
- shadcn-base.css
- base.css
- themes/claude.css
- themes/ocean.css
- themes/sunset.css
- themes/clean-slate.css
- themes/dark-matter.css
- themes/tangerine.css
- themes/twitter.css
- themes/default.css

---

**Document Purpose**: Reference for migration guide and for understanding what existed before the refactor.

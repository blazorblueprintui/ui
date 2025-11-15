# Internationalization (i18n)

**Domain:** Infrastructure
**Last Updated:** 2025-11-01

---

## Overview

BlazorUI components support internationalization (i18n) through:
1. **RTL (Right-to-Left) layout support** for languages like Arabic, Hebrew
2. **Localization-ready architecture** for translatable strings
3. **Culture-aware formatting** (dates, numbers, currencies)

## RTL Support

### Implementation Strategy

Components should respond to the `dir` attribute on the document root:

```html
<!-- LTR (default) -->
<html dir="ltr">

<!-- RTL -->
<html dir="rtl">
```

### CSS Logical Properties

Use CSS logical properties instead of physical directions:

**Instead of:**
```css
.component {
  margin-left: 1rem;
  padding-right: 0.5rem;
  text-align: left;
}
```

**Use:**
```css
.component {
  margin-inline-start: 1rem;   /* left in LTR, right in RTL */
  padding-inline-end: 0.5rem;  /* right in LTR, left in RTL */
  text-align: start;           /* left in LTR, right in RTL */
}
```

### Tailwind RTL Plugin

For Tailwind-based styling, use the RTL plugin:

```javascript
// tailwind.config.js
module.exports = {
  plugins: [
    require('tailwindcss-rtl'),
  ],
}
```

**Usage in components:**
```html
<div class="ms-4">  <!-- margin-start: 1rem (left in LTR, right in RTL) -->
<div class="me-4">  <!-- margin-end: 1rem (right in LTR, left in RTL) -->
```

## Localization Architecture

### String Resources

Components with user-facing text should support localization:

**1. Component with localizable strings:**
```csharp
// Dialog.razor.cs
[Parameter] public string CloseLabel { get; set; } = "Close";
[Parameter] public string CancelLabel { get; set; } = "Cancel";
[Parameter] public string ConfirmLabel { get; set; } = "Confirm";
```

**2. Consumer can override with localized strings:**
```razor
<Dialog CloseLabel="@Localizer["Close"]"
        CancelLabel="@Localizer["Cancel"]">
    <!-- content -->
</Dialog>
```

### Minimal Built-in Strings

The library should minimize hard-coded strings. When necessary:

**Provide defaults but allow overrides:**
```csharp
public class Pagination
{
    [Parameter] public string NextLabel { get; set; } = "Next";
    [Parameter] public string PreviousLabel { get; set; } = "Previous";
    [Parameter] public string FirstLabel { get; set; } = "First";
    [Parameter] public string LastLabel { get; set; } = "Last";
}
```

## Culture-Aware Formatting

### Dates and Times

Use .NET's built-in culture support:

```csharp
// DatePicker component (future)
var formattedDate = selectedDate.ToString("d", CultureInfo.CurrentCulture);
```

### Numbers and Currencies

```csharp
var formattedNumber = value.ToString("N2", CultureInfo.CurrentCulture);
var formattedCurrency = price.ToString("C", CultureInfo.CurrentCulture);
```

## Blazor Localization Integration

### Consumer Setup

Developers using the library will configure localization in their app:

**1. Program.cs:**
```csharp
builder.Services.AddLocalization();
```

**2. App.razor or MainLayout.razor:**
```razor
@using System.Globalization
@inject IStringLocalizer<Resources> Localizer

@code {
    protected override void OnInitialized()
    {
        // Set culture based on user preference
        CultureInfo.CurrentCulture = new CultureInfo("ar-SA");  // Arabic
        CultureInfo.CurrentUICulture = new CultureInfo("ar-SA");
    }
}
```

**3. Set `dir` attribute:**
```razor
<html dir="@(CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? "rtl" : "ltr")">
```

## Component Guidelines

### 1. Use Logical Properties

✅ **Good:**
```css
.card {
  margin-inline-start: 1rem;
  border-inline-start: 1px solid var(--border);
  text-align: start;
}
```

❌ **Bad:**
```css
.card {
  margin-left: 1rem;
  border-left: 1px solid var(--border);
  text-align: left;
}
```

### 2. Avoid Hard-Coded Text

✅ **Good:**
```razor
<button>@CloseLabel</button>
```

❌ **Bad:**
```razor
<button>Close</button>
```

### 3. Test in RTL Mode

Every component should be tested with:
```html
<html dir="rtl" lang="ar">
```

Verify:
- Layout flips correctly
- Icons face the right direction
- Spacing and alignment work
- Text flows right-to-left

## Icon Directionality

Some icons need to flip in RTL:

```razor
@* Arrow icons should flip in RTL *@
<svg class="@(IsRtl ? "transform scale-x-[-1]" : "")">
    <!-- arrow icon -->
</svg>
```

**Icons that should flip:**
- Arrows (left/right)
- Navigation (back/forward)
- Chevrons (directional)

**Icons that should NOT flip:**
- Close (X)
- Search (magnifying glass)
- Checkmark

## Implementation Checklist

- [ ] Use CSS logical properties in all components
- [ ] Make user-facing strings parameterizable
- [ ] Test components in RTL mode (`dir="rtl"`)
- [ ] Use culture-aware formatting for dates/numbers
- [ ] Document RTL support in component demos
- [ ] Provide RTL examples in demo app
- [ ] Handle icon directionality correctly

## Example: RTL-Aware Button

```razor
@* Button.razor *@
<button
    class="inline-flex items-center justify-center rounded-md
           px-4 py-2 text-sm font-medium
           bg-primary text-primary-foreground">

    @if (Icon != null && IconPosition == IconPosition.Start)
    {
        <span class="me-2">@Icon</span>
    }

    @ChildContent

    @if (Icon != null && IconPosition == IconPosition.End)
    {
        <span class="ms-2">@Icon</span>
    }
</button>

@code {
    [Parameter] public RenderFragment? Icon { get; set; }
    [Parameter] public IconPosition IconPosition { get; set; } = IconPosition.Start;
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

**Note:** `me-2` (margin-end) and `ms-2` (margin-start) automatically flip in RTL.

## Future Enhancements

1. **Locale-specific components:**
   - Date pickers with localized calendars
   - Number inputs with culture-aware formatting

2. **Translation helpers:**
   - Resource file templates for common strings
   - CLI tool to extract translatable strings

3. **Bidirectional text:**
   - Proper handling of mixed LTR/RTL content
   - Unicode bidirectional algorithm support

## References

- [CSS Logical Properties (MDN)](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_Logical_Properties)
- [Tailwind CSS RTL](https://tailwindcss.com/docs/plugins#rtl-support)
- [Blazor Localization](https://learn.microsoft.com/en-us/aspnet/core/blazor/globalization-localization)
- [RTL Styling Guide](https://rtlstyling.com/)

---

**Note:** RTL support is a core requirement. All components must work seamlessly in both LTR and RTL modes.

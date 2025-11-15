# Combobox Primitive Implementation

## Headless Design Pattern

### Core Principle

The Combobox primitive follows **strict headless design principles**:

1. **Primitives provide ZERO styling** - They contain only functionality and accessibility features
2. **Styling belongs in consumer components** - Command components (or other wrappers) handle all visual presentation
3. **Data attributes for styling hooks** - Primitives expose state via `data-*` attributes, never CSS classes

### Why Headless?

- **Framework agnostic** - Can be used with any CSS framework (Tailwind, Bootstrap, custom CSS, etc.)
- **Maximum flexibility** - Consumers control 100% of visual presentation
- **Clear separation** - Behavior logic separate from styling logic
- **Easier testing** - Primitives can be tested without CSS dependencies
- **Better performance** - No unnecessary style recalculations when primitive state changes

### The Wrapping Pattern

Command components wrap primitives using this pattern:

```razor
<!-- WRONG: Passing classes to primitive -->
<ComboboxItem class="px-2 py-1.5 hover:bg-accent" Value="@Value">
    @ChildContent
</ComboboxItem>

<!-- CORRECT: Wrapping primitive in styled container -->
<div class="px-2 py-1.5 hover:bg-accent">
    <ComboboxItem Value="@Value">
        @ChildContent
    </ComboboxItem>
</div>
```

**Key Points:**
- Primitive receives ONLY functional parameters (Value, Disabled, OnSelect, etc.)
- Wrapper div receives ALL styling (classes, inline styles, etc.)
- This maintains a clean boundary between behavior and presentation

## Implementation Details

### Primitive Components

Located in `src/Blazix/Primitives/Combobox/`:

1. **ComboboxPrimitive.razor** - Root container with state management
   - Manages search query, selected value, filtering
   - Provides context to child components
   - NO styling, just semantic HTML

2. **ComboboxInput.razor** - Search input element
   - Renders pure `<input>` with ARIA attributes
   - Handles keyboard navigation via JavaScript
   - Exposes state via `data-*` attributes

3. **ComboboxContent.razor** - Listbox container
   - Renders semantic `<div role="listbox">`
   - Conditional rendering based on search/items
   - NO presentation classes

4. **ComboboxItem.razor** - Individual option
   - Renders `<div role="option">` with ARIA attributes
   - Uses `data-focused` and `data-disabled` for styling hooks
   - Selection handled via click events

5. **ComboboxEmpty.razor** - Empty state message
   - Conditional rendering when no items match
   - Pure semantic HTML

6. **ComboboxContext.cs** - Shared state management
   - Cascading parameter pattern
   - Controlled/uncontrolled mode support

### Styled Components

Located in `src/BlazorUI/Components/Command/`:

1. **Command.razor** - Wraps ComboboxPrimitive
2. **CommandInput.razor** - Wraps ComboboxInput
3. **CommandList.razor** - Wraps ComboboxContent
4. **CommandItem.razor** - Wraps ComboboxItem
5. **CommandEmpty.razor** - Wraps ComboboxEmpty

Each follows the wrapping pattern described above.

### JavaScript Integration

File: `src/Blazix/wwwroot/js/primitives/combobox.js`

**Headless Design in JavaScript:**
- Uses ONLY `data-focused` attribute for state
- Does NOT add/remove CSS classes
- Handles keyboard navigation (Arrow keys, Home, End, Enter, Escape)
- Manages focus and scroll behavior
- Integrates with Blazor via `dotNetRef.invokeMethodAsync()`

**Example:**
```javascript
// Correct - uses data attributes
focusedEl.setAttribute('data-focused', 'true');
focusedEl.setAttribute('aria-selected', 'true');

// Wrong - would couple to specific CSS classes
// focusedEl.classList.add('focused'); // ❌ REMOVED
```

CSS can then style based on the data attribute:
```css
[data-focused="true"] {
    background-color: var(--accent);
}
```

## Fixes Applied (2025-11-07)

### Issue: Headless Design Violations

**Problem:** Command components were passing CSS classes directly to primitives, violating headless principles.

**Root Cause:** Primitives accepted `class` attributes via `AdditionalAttributes`, allowing styled components to inject styling.

**Solution:** Refactored all Command components to use the wrapping pattern.

### Changes Made

#### 1. Command.razor
```diff
- <ComboboxPrimitive OnValueChange="@HandleValueChange" class="@CssClass">
+ <div class="@CssClass">
+     <ComboboxPrimitive OnValueChange="@HandleValueChange">
```

#### 2. CommandInput.razor
```diff
- <ComboboxInput class="flex h-11 w-full..." placeholder="@Placeholder" disabled="@Disabled" />
+ <div class="flex h-11 w-full...">
+     <ComboboxInput placeholder="@Placeholder" disabled="@Disabled" />
+ </div>
```

#### 3. CommandList.razor
```diff
- <ComboboxContent class="@CssClass">
+ <div class="@CssClass">
+     <ComboboxContent>
```

#### 4. CommandItem.razor
```diff
- <ComboboxItem Value="@Value" ... class="@CssClass">
+ <div class="@CssClass">
+     <ComboboxItem Value="@Value" ...>
```

#### 5. CommandEmpty.razor
```diff
- <ComboboxEmpty class="@CssClass">
+ <div class="@CssClass">
+     <ComboboxEmpty>
```

#### 6. combobox.js
```diff
- el.classList.remove('focused');
+ // HEADLESS DESIGN: Use data attributes only, not CSS classes

- focusedEl.classList.add('focused');
+ // HEADLESS DESIGN: Use data attributes only, not CSS classes
```

### Documentation Added

Added comments to all modified files explaining the headless design pattern:

```razor
@*
  Component - styled wrapper over Primitive
  HEADLESS DESIGN: This component wraps the unstyled primitive in a styled container.
  The primitive itself receives NO styling - only the wrapper div does.
*@
```

## Benefits Achieved

1. ✅ **True headless architecture** - Primitives completely decoupled from styling
2. ✅ **Framework flexibility** - Can use primitives with any CSS approach
3. ✅ **Clear boundaries** - Explicit separation between behavior and presentation
4. ✅ **Maintainability** - Styling changes don't affect primitive logic
5. ✅ **Testability** - Primitives can be tested without CSS concerns
6. ✅ **Performance** - No unnecessary class manipulations

## Future Considerations

### Additional Primitives

Other Command components don't have primitive equivalents:
- CommandGroup
- CommandSeparator
- CommandShortcut

**Decision:** These are pure presentation components and don't need primitives. They can remain as styled components.

### Styling Hooks

Current data attributes exposed for styling:
- `data-focused` - Item has keyboard focus
- `data-disabled` - Item is disabled
- `aria-selected` - Item is selected
- `aria-disabled` - Item is disabled (for accessibility)

### Alternative CSS Frameworks

The headless pattern enables easy integration with:
- **Tailwind CSS** - Current implementation
- **Bootstrap** - Could create Bootstrap-styled wrappers
- **Material UI** - Could create Material-styled wrappers
- **Custom CSS** - Full control over styling

Example for Bootstrap:
```razor
<div class="list-group-item @(Disabled ? "disabled" : "")">
    <ComboboxItem Value="@Value" Disabled="@Disabled">
        @ChildContent
    </ComboboxItem>
</div>
```

## Testing

### Manual Testing Checklist

- [ ] Keyboard navigation works (Arrow keys, Home, End, Enter, Escape)
- [ ] Visual focus states update correctly
- [ ] Disabled items cannot be selected
- [ ] Search filtering works
- [ ] Empty state displays when no matches
- [ ] Primitives render without styling (inspect DOM)
- [ ] Command components render with correct styles
- [ ] No console errors related to class manipulation

### Automated Testing

Consider adding tests for:
1. Primitive behavior (filtering, selection, keyboard nav)
2. Data attribute updates on state changes
3. ARIA attribute correctness
4. Component wrapping pattern validation

## References

- Radix UI Primitives: https://www.radix-ui.com/primitives
- Headless UI: https://headlessui.com/
- React Aria: https://react-spectrum.adobe.com/react-aria/

## Changelog

### 2025-11-07 - Headless Design Enforcement
- Refactored all Command components to use wrapping pattern
- Removed CSS class manipulation from JavaScript
- Added inline documentation explaining headless design
- Created this implementation guide

### 2025-11-06 - Initial Implementation
- Created Combobox primitives
- Migrated Command components to use primitives
- Implemented keyboard navigation
- Added filtering and search functionality

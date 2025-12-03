# Calendar Component

Accessible month/day grid for date selection. Foundation for date pickers and range pickers.

## Features

- **Date Selection**: Single date selection with `@bind-SelectedDate`
- **Month Navigation**: Navigate between months with arrows
- **Min/Max Dates**: Constrain selectable date range
- **Keyboard Navigation**: Full keyboard support
- **Accessibility**: ARIA grid roles and labels
- **Localization**: Culture-aware formatting

## Usage

### Basic Calendar

```razor
@using BlazorUI.Components.Calendar

<Calendar @bind-SelectedDate="selectedDate" />

<p>Selected: @selectedDate?.ToString("D")</p>

@code {
    private DateOnly? selectedDate;
}
```

### With Date Constraints

```razor
<Calendar 
    @bind-SelectedDate="selectedDate"
    MinDate="DateOnly.FromDateTime(DateTime.Today)"
    MaxDate="DateOnly.FromDateTime(DateTime.Today.AddMonths(3))" />

@code {
    private DateOnly? selectedDate;
}
```

### With Initial Month

```razor
<Calendar 
    @bind-SelectedDate="selectedDate"
    InitialMonth="new DateOnly(2024, 12, 1)" />

@code {
    private DateOnly? selectedDate;
}
```

### With Disabled Dates

```razor
<Calendar 
    @bind-SelectedDate="selectedDate"
    IsDateDisabled="IsWeekend" />

@code {
    private DateOnly? selectedDate;

    private bool IsWeekend(DateOnly date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || 
               date.DayOfWeek == DayOfWeek.Sunday;
    }
}
```

### With Custom Culture

```razor
@using System.Globalization

<Calendar 
    @bind-SelectedDate="selectedDate"
    Culture="new CultureInfo(\"fr-FR\")" />

@code {
    private DateOnly? selectedDate;
}
```

### In a Popover (DatePicker Pattern)

```razor
@using BlazorUI.Components.Popover
@using BlazorUI.Components.Calendar
@using BlazorUI.Components.Button

<Popover>
    <PopoverTrigger>
        <Button Variant="ButtonVariant.Outline" Class="w-[280px] justify-start text-left font-normal">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="mr-2">
                <rect width="18" height="18" x="3" y="4" rx="2" ry="2"/>
                <line x1="16" x2="16" y1="2" y2="6"/>
                <line x1="8" x2="8" y1="2" y2="6"/>
                <line x1="3" x2="21" y1="10" y2="10"/>
            </svg>
            @(selectedDate?.ToString("PPP") ?? "Pick a date")
        </Button>
    </PopoverTrigger>
    <PopoverContent Class="w-auto p-0">
        <Calendar @bind-SelectedDate="selectedDate" />
    </PopoverContent>
</Popover>

@code {
    private DateOnly? selectedDate;
}
```

## API Reference

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `SelectedDate` | `DateOnly?` | `null` | The currently selected date |
| `SelectedDateChanged` | `EventCallback<DateOnly?>` | - | Called when selection changes |
| `MinDate` | `DateOnly?` | `null` | Minimum selectable date |
| `MaxDate` | `DateOnly?` | `null` | Maximum selectable date |
| `Culture` | `CultureInfo` | `CurrentCulture` | Culture for formatting |
| `InitialMonth` | `DateOnly?` | `null` | Initial month to display |
| `IsDateDisabled` | `Func<DateOnly, bool>?` | `null` | Function to disable specific dates |
| `Class` | `string?` | `null` | Additional CSS classes |
| `AdditionalAttributes` | `Dictionary<string, object>?` | `null` | Additional HTML attributes |

### Keyboard Navigation

| Key | Action |
|-----|--------|
| `ArrowLeft` | Previous day |
| `ArrowRight` | Next day |
| `ArrowUp` | Same day, previous week |
| `ArrowDown` | Same day, next week |
| `Home` | First day of month |
| `End` | Last day of month |
| `PageUp` | Previous month |
| `PageDown` | Next month |
| `Enter/Space` | Select focused date |

### Accessibility

The calendar uses proper ARIA attributes:
- `role="application"` on the root
- `role="grid"` on the table
- `aria-label` for month/year
- `aria-selected` on selected date
- `aria-live="polite"` for month changes

# ScrollArea Component

A styled scrollable region with custom scrollbars using shadcn/Radix patterns.

## Features

- **Custom Scrollbars**: Styled scrollbar track and thumb
- **Orientation Support**: Vertical and horizontal scrolling
- **Configurable Behavior**: Auto, always visible, hover, or scroll-triggered
- **Tailwind Styled**: Uses theme tokens for consistent styling

## Usage

### Basic Vertical Scroll

```razor
@using BlazorUI.Components.ScrollArea

<ScrollArea Class="h-72 w-48 rounded-md border">
    <div class="p-4">
        @for (int i = 1; i <= 50; i++)
        {
            <div class="text-sm">Item @i</div>
        }
    </div>
</ScrollArea>
```

### Horizontal Scroll

```razor
<ScrollArea ShowVerticalScrollbar="false" ShowHorizontalScrollbar="true" Class="w-96 whitespace-nowrap rounded-md border">
    <div class="flex w-max space-x-4 p-4">
        @for (int i = 1; i <= 20; i++)
        {
            <div class="w-32 h-20 rounded-md bg-muted flex items-center justify-center">
                Card @i
            </div>
        }
    </div>
</ScrollArea>
```

### Both Scrollbars

```razor
<ScrollArea ShowVerticalScrollbar="true" ShowHorizontalScrollbar="true" Class="h-72 w-72 rounded-md border">
    <div class="w-[600px] p-4">
        @* Wide content here *@
    </div>
</ScrollArea>
```

### Card with Long Content

```razor
<div class="w-80 rounded-lg border">
    <div class="p-4 border-b">
        <h3 class="font-semibold">Notifications</h3>
    </div>
    <ScrollArea Class="h-64">
        <div class="p-4">
            @foreach (var notification in Notifications)
            {
                <div class="mb-4 last:mb-0">
                    <p class="text-sm font-medium">@notification.Title</p>
                    <p class="text-sm text-muted-foreground">@notification.Description</p>
                </div>
            }
        </div>
    </ScrollArea>
</div>
```

## API Reference

### ScrollArea Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | The content to render within the scrollable area |
| `Type` | `ScrollAreaType` | `Auto` | The type of scrollbar behavior |
| `ScrollHideDelay` | `int` | `600` | Controls visibility delay of scrollbars in milliseconds |
| `ShowVerticalScrollbar` | `bool` | `true` | Whether to show the vertical scrollbar |
| `ShowHorizontalScrollbar` | `bool` | `false` | Whether to show the horizontal scrollbar |
| `Class` | `string?` | `null` | Additional CSS classes to apply |
| `AdditionalAttributes` | `Dictionary<string, object>?` | `null` | Additional HTML attributes |

### ScrollAreaType Enum

| Value | Description |
|-------|-------------|
| `Auto` | Scrollbars visible when content overflows |
| `Always` | Scrollbars always visible |
| `Scroll` | Scrollbars appear only when scrolling |
| `Hover` | Scrollbars appear only on hover |

### ScrollBar Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Orientation` | `Orientation` | `Vertical` | The orientation of the scrollbar |
| `Class` | `string?` | `null` | Additional CSS classes to apply |
| `AdditionalAttributes` | `Dictionary<string, object>?` | `null` | Additional HTML attributes |

### Orientation Enum

| Value | Description |
|-------|-------------|
| `Vertical` | Vertical scrollbar |
| `Horizontal` | Horizontal scrollbar |

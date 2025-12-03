# Resizable Components

Split layout components with draggable handles for horizontal/vertical split layouts.

## Features

- **Flexible Layout**: Horizontal or vertical panel arrangements
- **Drag to Resize**: Handles between panels for resizing
- **Min/Max Constraints**: Optional size limits per panel
- **Collapsible Panels**: Optional collapse functionality
- **Layout Events**: Callbacks when sizes change

## Usage

### Basic Horizontal Split

```razor
@using BlazorUI.Components.Resizable

<ResizablePanelGroup Direction="ResizableDirection.Horizontal" Class="min-h-[200px] max-w-md rounded-lg border">
    <ResizablePanel DefaultSize="50">
        <div class="flex h-full items-center justify-center p-6">
            <span class="font-semibold">Left Panel</span>
        </div>
    </ResizablePanel>
    <ResizableHandle Index="0" />
    <ResizablePanel DefaultSize="50">
        <div class="flex h-full items-center justify-center p-6">
            <span class="font-semibold">Right Panel</span>
        </div>
    </ResizablePanel>
</ResizablePanelGroup>
```

### Vertical Split

```razor
<ResizablePanelGroup Direction="ResizableDirection.Vertical" Class="min-h-[400px] max-w-md rounded-lg border">
    <ResizablePanel DefaultSize="30">
        <div class="flex h-full items-center justify-center p-6">
            <span class="font-semibold">Top Panel</span>
        </div>
    </ResizablePanel>
    <ResizableHandle Index="0" />
    <ResizablePanel DefaultSize="70">
        <div class="flex h-full items-center justify-center p-6">
            <span class="font-semibold">Bottom Panel</span>
        </div>
    </ResizablePanel>
</ResizablePanelGroup>
```

### With Handle Grip

```razor
<ResizablePanelGroup Direction="ResizableDirection.Horizontal" Class="min-h-[200px] max-w-md rounded-lg border">
    <ResizablePanel DefaultSize="50">
        <div class="flex h-full items-center justify-center p-6">
            <span class="font-semibold">Panel 1</span>
        </div>
    </ResizablePanel>
    <ResizableHandle Index="0" WithHandle="true" />
    <ResizablePanel DefaultSize="50">
        <div class="flex h-full items-center justify-center p-6">
            <span class="font-semibold">Panel 2</span>
        </div>
    </ResizablePanel>
</ResizablePanelGroup>
```

### Three Panel Layout

```razor
<ResizablePanelGroup Direction="ResizableDirection.Horizontal" Class="min-h-[300px] rounded-lg border">
    <ResizablePanel DefaultSize="25" MinSize="15">
        <div class="flex h-full items-center justify-center p-4">
            <span class="font-semibold">Sidebar</span>
        </div>
    </ResizablePanel>
    <ResizableHandle Index="0" />
    <ResizablePanel DefaultSize="50">
        <div class="flex h-full items-center justify-center p-4">
            <span class="font-semibold">Main Content</span>
        </div>
    </ResizablePanel>
    <ResizableHandle Index="1" />
    <ResizablePanel DefaultSize="25" MinSize="15">
        <div class="flex h-full items-center justify-center p-4">
            <span class="font-semibold">Details</span>
        </div>
    </ResizablePanel>
</ResizablePanelGroup>
```

### Nested Layouts

```razor
<ResizablePanelGroup Direction="ResizableDirection.Horizontal" Class="min-h-[400px] rounded-lg border">
    <ResizablePanel DefaultSize="30">
        <div class="flex h-full items-center justify-center p-4">
            <span class="font-semibold">Left</span>
        </div>
    </ResizablePanel>
    <ResizableHandle Index="0" />
    <ResizablePanel DefaultSize="70">
        <ResizablePanelGroup Direction="ResizableDirection.Vertical">
            <ResizablePanel DefaultSize="50">
                <div class="flex h-full items-center justify-center p-4">
                    <span class="font-semibold">Top Right</span>
                </div>
            </ResizablePanel>
            <ResizableHandle Index="0" />
            <ResizablePanel DefaultSize="50">
                <div class="flex h-full items-center justify-center p-4">
                    <span class="font-semibold">Bottom Right</span>
                </div>
            </ResizablePanel>
        </ResizablePanelGroup>
    </ResizablePanel>
</ResizablePanelGroup>
```

## API Reference

### ResizablePanelGroup Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | The panels and handles to render |
| `Direction` | `ResizableDirection` | `Horizontal` | The layout direction |
| `DefaultSizes` | `double[]?` | `null` | Initial panel sizes as percentages |
| `OnLayoutChange` | `EventCallback<double[]>` | - | Called when panel sizes change |
| `Class` | `string?` | `null` | Additional CSS classes |
| `AdditionalAttributes` | `Dictionary<string, object>?` | `null` | Additional HTML attributes |

### ResizablePanel Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | The panel content |
| `DefaultSize` | `double?` | `null` | Default size as percentage |
| `MinSize` | `double?` | `null` | Minimum size as percentage |
| `MaxSize` | `double?` | `null` | Maximum size as percentage |
| `Collapsible` | `bool` | `false` | Whether the panel can be collapsed |
| `Class` | `string?` | `null` | Additional CSS classes |
| `AdditionalAttributes` | `Dictionary<string, object>?` | `null` | Additional HTML attributes |

### ResizableHandle Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Index` | `int` | `0` | The handle index (between panels) |
| `WithHandle` | `bool` | `false` | Whether to show a visual grip |
| `Class` | `string?` | `null` | Additional CSS classes |
| `AdditionalAttributes` | `Dictionary<string, object>?` | `null` | Additional HTML attributes |

### ResizableDirection Enum

| Value | Description |
|-------|-------------|
| `Horizontal` | Panels arranged side by side |
| `Vertical` | Panels stacked vertically |

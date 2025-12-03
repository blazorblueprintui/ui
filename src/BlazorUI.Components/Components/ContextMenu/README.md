# ContextMenu Component

Right-click context menu component distinct from DropdownMenu.

## Features

- **Right-click Activation**: Opens on contextmenu event
- **Position at Cursor**: Appears at click coordinates
- **Click Outside Close**: Closes when clicking outside
- **Keyboard Navigation**: Arrow keys, Enter, Escape support
- **ARIA Compliant**: Proper menu roles and states

## Usage

### Basic Context Menu

```razor
@using BlazorUI.Components.ContextMenu

<ContextMenu>
    <ContextMenuTrigger>
        <div class="flex h-[150px] w-[300px] items-center justify-center rounded-md border border-dashed text-sm">
            Right click here
        </div>
    </ContextMenuTrigger>
    <ContextMenuContent>
        <ContextMenuItem OnClick="@(() => Console.WriteLine("Back"))">
            Back
            <ContextMenuShortcut>⌘[</ContextMenuShortcut>
        </ContextMenuItem>
        <ContextMenuItem Disabled="true">
            Forward
            <ContextMenuShortcut>⌘]</ContextMenuShortcut>
        </ContextMenuItem>
        <ContextMenuItem>
            Reload
            <ContextMenuShortcut>⌘R</ContextMenuShortcut>
        </ContextMenuItem>
        <ContextMenuSeparator />
        <ContextMenuItem>
            Save As...
            <ContextMenuShortcut>⇧⌘S</ContextMenuShortcut>
        </ContextMenuItem>
        <ContextMenuItem>Print...</ContextMenuItem>
    </ContextMenuContent>
</ContextMenu>
```

### File Manager Context Menu

```razor
<ContextMenu>
    <ContextMenuTrigger>
        <div class="p-4 border rounded-md">
            <p class="font-medium">document.pdf</p>
            <p class="text-sm text-muted-foreground">2.4 MB</p>
        </div>
    </ContextMenuTrigger>
    <ContextMenuContent>
        <ContextMenuLabel>File Actions</ContextMenuLabel>
        <ContextMenuSeparator />
        <ContextMenuItem OnClick="@HandleOpen">
            Open
            <ContextMenuShortcut>Enter</ContextMenuShortcut>
        </ContextMenuItem>
        <ContextMenuItem OnClick="@HandleRename">
            Rename
            <ContextMenuShortcut>F2</ContextMenuShortcut>
        </ContextMenuItem>
        <ContextMenuItem OnClick="@HandleDuplicate">
            Duplicate
            <ContextMenuShortcut>⌘D</ContextMenuShortcut>
        </ContextMenuItem>
        <ContextMenuSeparator />
        <ContextMenuItem OnClick="@HandleDelete" Class="text-red-600 focus:text-red-600">
            Delete
            <ContextMenuShortcut>⌫</ContextMenuShortcut>
        </ContextMenuItem>
    </ContextMenuContent>
</ContextMenu>

@code {
    private void HandleOpen() { }
    private void HandleRename() { }
    private void HandleDuplicate() { }
    private void HandleDelete() { }
}
```

### DataTable Row Context Menu

```razor
<ContextMenu>
    <ContextMenuTrigger>
        <tr class="border-b hover:bg-muted/50">
            <td class="p-4">John Doe</td>
            <td class="p-4">john@example.com</td>
            <td class="p-4">Active</td>
        </tr>
    </ContextMenuTrigger>
    <ContextMenuContent>
        <ContextMenuItem OnClick="@HandleEdit">Edit</ContextMenuItem>
        <ContextMenuItem OnClick="@HandleView">View Details</ContextMenuItem>
        <ContextMenuSeparator />
        <ContextMenuItem OnClick="@HandleDeactivate">Deactivate</ContextMenuItem>
        <ContextMenuItem OnClick="@HandleDelete" Class="text-destructive">Delete</ContextMenuItem>
    </ContextMenuContent>
</ContextMenu>
```

## API Reference

### ContextMenu Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | The menu content |
| `Open` | `bool?` | `null` | Controls open state (controlled mode) |
| `OpenChanged` | `EventCallback<bool>` | - | Called when open state changes |
| `OnOpen` | `EventCallback<(double X, double Y)>` | - | Called when menu opens |
| `OnClose` | `EventCallback` | - | Called when menu closes |

### ContextMenuTrigger Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | The trigger element |
| `Disabled` | `bool` | `false` | Whether the trigger is disabled |

### ContextMenuContent Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | The menu items |
| `CloseOnEscape` | `bool` | `true` | Close on Escape key |
| `CloseOnClickOutside` | `bool` | `true` | Close on outside click |
| `ZIndex` | `int` | `50` | Z-index value |
| `Class` | `string?` | `null` | Additional CSS classes |

### ContextMenuItem Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | Item content |
| `Disabled` | `bool` | `false` | Whether the item is disabled |
| `OnClick` | `EventCallback` | - | Click callback |
| `Inset` | `bool` | `false` | Add left padding |
| `Class` | `string?` | `null` | Additional CSS classes |

### ContextMenuSeparator Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Class` | `string?` | `null` | Additional CSS classes |

### ContextMenuLabel Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | Label text |
| `Inset` | `bool` | `false` | Add left padding |
| `Class` | `string?` | `null` | Additional CSS classes |

### ContextMenuShortcut Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | Shortcut text |
| `Class` | `string?` | `null` | Additional CSS classes |

### Keyboard Navigation

| Key | Action |
|-----|--------|
| `ArrowDown` | Next item |
| `ArrowUp` | Previous item |
| `Home` | First item |
| `End` | Last item |
| `Enter/Space` | Activate item |
| `Escape` | Close menu |

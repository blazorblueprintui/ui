# Menubar Component

A horizontally-oriented menu bar that provides access to a consistent set of commands or actions. Each menu can be opened to reveal dropdown content with items, separators, and keyboard shortcuts.

## Features

- **Keyboard Navigation**: Arrow keys to navigate between menus and items
- **Roving Focus**: Tab to enter the menubar, arrow keys to move between triggers
- **Auto-Open**: When a menu is open, hovering/focusing other triggers opens those menus
- **Accessible**: Full ARIA support for screen readers
- **Controlled/Uncontrolled**: Supports both modes with `@bind-ActiveIndex`

## Usage

### Basic Example

```razor
@using BlazorUI.Components.Menubar

<Menubar>
    <MenubarMenu>
        <MenubarTrigger>File</MenubarTrigger>
        <MenubarContent>
            <MenubarItem OnClick="HandleNewFile">
                New File
                <MenubarShortcut>⌘N</MenubarShortcut>
            </MenubarItem>
            <MenubarItem OnClick="HandleOpen">
                Open
                <MenubarShortcut>⌘O</MenubarShortcut>
            </MenubarItem>
            <MenubarSeparator />
            <MenubarItem OnClick="HandleSave">
                Save
                <MenubarShortcut>⌘S</MenubarShortcut>
            </MenubarItem>
            <MenubarItem OnClick="HandleSaveAs">
                Save As...
                <MenubarShortcut>⇧⌘S</MenubarShortcut>
            </MenubarItem>
        </MenubarContent>
    </MenubarMenu>

    <MenubarMenu>
        <MenubarTrigger>Edit</MenubarTrigger>
        <MenubarContent>
            <MenubarItem OnClick="HandleUndo">
                Undo
                <MenubarShortcut>⌘Z</MenubarShortcut>
            </MenubarItem>
            <MenubarItem OnClick="HandleRedo">
                Redo
                <MenubarShortcut>⇧⌘Z</MenubarShortcut>
            </MenubarItem>
            <MenubarSeparator />
            <MenubarItem OnClick="HandleCut">
                Cut
                <MenubarShortcut>⌘X</MenubarShortcut>
            </MenubarItem>
            <MenubarItem OnClick="HandleCopy">
                Copy
                <MenubarShortcut>⌘C</MenubarShortcut>
            </MenubarItem>
            <MenubarItem OnClick="HandlePaste">
                Paste
                <MenubarShortcut>⌘V</MenubarShortcut>
            </MenubarItem>
        </MenubarContent>
    </MenubarMenu>

    <MenubarMenu>
        <MenubarTrigger>View</MenubarTrigger>
        <MenubarContent>
            <MenubarItem OnClick="HandleZoomIn">
                Zoom In
                <MenubarShortcut>⌘+</MenubarShortcut>
            </MenubarItem>
            <MenubarItem OnClick="HandleZoomOut">
                Zoom Out
                <MenubarShortcut>⌘-</MenubarShortcut>
            </MenubarItem>
            <MenubarSeparator />
            <MenubarItem OnClick="HandleResetZoom">
                Reset Zoom
                <MenubarShortcut>⌘0</MenubarShortcut>
            </MenubarItem>
        </MenubarContent>
    </MenubarMenu>
</Menubar>

@code {
    private void HandleNewFile() => Console.WriteLine("New file");
    private void HandleOpen() => Console.WriteLine("Open");
    private void HandleSave() => Console.WriteLine("Save");
    private void HandleSaveAs() => Console.WriteLine("Save as");
    private void HandleUndo() => Console.WriteLine("Undo");
    private void HandleRedo() => Console.WriteLine("Redo");
    private void HandleCut() => Console.WriteLine("Cut");
    private void HandleCopy() => Console.WriteLine("Copy");
    private void HandlePaste() => Console.WriteLine("Paste");
    private void HandleZoomIn() => Console.WriteLine("Zoom in");
    private void HandleZoomOut() => Console.WriteLine("Zoom out");
    private void HandleResetZoom() => Console.WriteLine("Reset zoom");
}
```

### Controlled Mode

```razor
<Menubar @bind-ActiveIndex="activeMenuIndex">
    <MenubarMenu>
        <MenubarTrigger>File</MenubarTrigger>
        <MenubarContent>
            <MenubarItem>New</MenubarItem>
            <MenubarItem>Open</MenubarItem>
        </MenubarContent>
    </MenubarMenu>
</Menubar>

@code {
    private int activeMenuIndex = -1; // -1 means no menu is open
}
```

### Disabled Items

```razor
<Menubar>
    <MenubarMenu>
        <MenubarTrigger>Edit</MenubarTrigger>
        <MenubarContent>
            <MenubarItem>Undo</MenubarItem>
            <MenubarItem Disabled="true">Redo</MenubarItem>
            <MenubarSeparator />
            <MenubarItem>Cut</MenubarItem>
        </MenubarContent>
    </MenubarMenu>
</Menubar>
```

## Component API

### Menubar

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ActiveIndex` | `int?` | `null` | Controls which menu is open (controlled mode) |
| `ActiveIndexChanged` | `EventCallback<int>` | - | Callback when active index changes |
| `DefaultActiveIndex` | `int` | `-1` | Default active index (uncontrolled mode) |
| `Loop` | `bool` | `true` | Enable keyboard loop navigation |
| `Class` | `string?` | `null` | Additional CSS classes |

### MenubarMenu

Container for grouping MenubarTrigger and MenubarContent.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Child content (trigger and content) |

### MenubarTrigger

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Disabled` | `bool` | `false` | Whether the trigger is disabled |
| `Class` | `string?` | `null` | Additional CSS classes |

### MenubarContent

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `CloseOnEscape` | `bool` | `true` | Close on Escape key |
| `CloseOnClickOutside` | `bool` | `true` | Close on click outside |
| `Side` | `PopoverSide` | `Bottom` | Positioning side |
| `Align` | `PopoverAlign` | `Start` | Positioning alignment |
| `Loop` | `bool` | `true` | Loop keyboard navigation |
| `Class` | `string?` | `null` | Additional CSS classes |

### MenubarItem

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Disabled` | `bool` | `false` | Whether the item is disabled |
| `OnClick` | `EventCallback<MouseEventArgs>` | - | Click handler |
| `CloseOnSelect` | `bool` | `true` | Close menu after selection |
| `Inset` | `bool` | `false` | Add left padding for alignment |
| `Class` | `string?` | `null` | Additional CSS classes |

### MenubarSeparator

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Class` | `string?` | `null` | Additional CSS classes |

### MenubarShortcut

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Shortcut text (e.g., "⌘K") |
| `Class` | `string?` | `null` | Additional CSS classes |

## Keyboard Navigation

| Key | Action |
|-----|--------|
| `Enter` / `Space` | Open/close focused menu |
| `ArrowDown` | Open menu / Move to next item |
| `ArrowUp` | Move to previous item |
| `ArrowRight` | Move to next menu trigger |
| `ArrowLeft` | Move to previous menu trigger |
| `Escape` | Close current menu |
| `Home` | Move to first item |
| `End` | Move to last item |

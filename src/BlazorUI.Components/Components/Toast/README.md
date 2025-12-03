# Toast Component

Sonner-style toast notifications with a provider, viewport, and programmatic service.

## Features

- **Programmatic API**: Use `IToastService` to show toasts from anywhere
- **Variants**: Default, Success, Warning, Error, Info
- **Auto-dismiss**: Configurable duration
- **Actions**: Optional action buttons
- **Stacking**: Multiple toasts stack nicely
- **Positions**: Multiple viewport positions

## Setup

Register the toast service in your `Program.cs`:

```csharp
builder.Services.AddSingleton<IToastService, ToastService>();
```

Add the `ToastProvider` to your `MainLayout.razor`:

```razor
@using BlazorUI.Components.Toast

<ToastProvider Position="ToastPosition.BottomRight">
    @Body
</ToastProvider>
```

## Usage

### Basic Usage

```razor
@using BlazorUI.Components.Toast
@inject IToastService Toasts

<button @onclick="ShowToast">Show Toast</button>

@code {
    private void ShowToast()
    {
        Toasts.Show("Settings saved", "Your preferences have been updated.");
    }
}
```

### Success Toast

```razor
<button @onclick="ShowSuccess">Success</button>

@code {
    private void ShowSuccess()
    {
        Toasts.Success("Success!", "Your changes have been saved.");
    }
}
```

### Error Toast

```razor
<button @onclick="ShowError">Error</button>

@code {
    private void ShowError()
    {
        Toasts.Error("Error", "Something went wrong. Please try again.");
    }
}
```

### Warning Toast

```razor
<button @onclick="ShowWarning">Warning</button>

@code {
    private void ShowWarning()
    {
        Toasts.Warning("Warning", "This action cannot be undone.");
    }
}
```

### Info Toast

```razor
<button @onclick="ShowInfo">Info</button>

@code {
    private void ShowInfo()
    {
        Toasts.Info("Did you know?", "You can customize toast duration.");
    }
}
```

### Toast with Action

```razor
<button @onclick="ShowWithAction">Toast with Action</button>

@code {
    private void ShowWithAction()
    {
        Toasts.Show(new ToastOptions
        {
            Title = "Event scheduled",
            Description = "Your meeting has been scheduled.",
            ActionLabel = "Undo",
            OnAction = () => Console.WriteLine("Undo clicked!"),
            Duration = TimeSpan.FromSeconds(10)
        });
    }
}
```

### Custom Duration

```razor
<button @onclick="ShowLongToast">Long Toast</button>

@code {
    private void ShowLongToast()
    {
        Toasts.Show("Long toast", "This will be visible for 10 seconds.", 
            ToastVariant.Default, TimeSpan.FromSeconds(10));
    }
}
```

### Different Positions

```razor
<ToastProvider Position="ToastPosition.TopRight">
    @Body
</ToastProvider>
```

## API Reference

### IToastService Methods

| Method | Description |
|--------|-------------|
| `Show(ToastOptions)` | Shows a toast with full options |
| `Show(title, description, variant, duration)` | Shows a simple toast |
| `Success(title, description)` | Shows a success toast |
| `Error(title, description)` | Shows an error toast |
| `Warning(title, description)` | Shows a warning toast |
| `Info(title, description)` | Shows an info toast |
| `Dismiss(id)` | Dismisses a specific toast |
| `DismissAll()` | Dismisses all toasts |

### ToastOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Id` | `string` | Auto-generated | Unique identifier |
| `Title` | `string?` | `null` | Toast title |
| `Description` | `string?` | `null` | Toast description |
| `Variant` | `ToastVariant` | `Default` | Visual variant |
| `Duration` | `TimeSpan?` | `5s` | Auto-dismiss duration |
| `ActionLabel` | `string?` | `null` | Action button label |
| `OnAction` | `Action?` | `null` | Action button callback |
| `OnDismiss` | `Action?` | `null` | Dismiss callback |

### ToastVariant Enum

| Value | Description |
|-------|-------------|
| `Default` | Neutral toast |
| `Success` | Green success toast |
| `Warning` | Yellow warning toast |
| `Destructive` | Red error toast |
| `Info` | Blue info toast |

### ToastPosition Enum

| Value | Description |
|-------|-------------|
| `TopLeft` | Top left corner |
| `TopCenter` | Top center |
| `TopRight` | Top right corner |
| `BottomLeft` | Bottom left corner |
| `BottomCenter` | Bottom center |
| `BottomRight` | Bottom right corner |

### ToastProvider Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | The app content |
| `Position` | `ToastPosition` | `BottomRight` | Viewport position |
| `MaxToasts` | `int` | `5` | Max visible toasts |

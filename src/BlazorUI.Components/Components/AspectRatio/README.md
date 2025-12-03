# AspectRatio Component

A wrapper component that maintains a fixed aspect ratio for its children. Commonly used for videos, images, avatars, and cards.

## Features

- **CSS aspect-ratio**: Uses the native CSS `aspect-ratio` property
- **Flexible Ratio**: Accepts any ratio as a double (width / height)
- **Responsive**: Works with responsive containers
- **Minimal**: Single responsibility component

## Usage

### Basic Example (16:9 Video)

```razor
@using BlazorUI.Components.AspectRatio

<AspectRatio Ratio="16.0/9.0">
    <video src="video.mp4" class="w-full h-full object-cover" />
</AspectRatio>
```

### Square Avatar

```razor
<AspectRatio Ratio="1" Class="w-24">
    <img src="avatar.jpg" alt="User avatar" class="w-full h-full object-cover rounded-full" />
</AspectRatio>
```

### 4:3 Image

```razor
<AspectRatio Ratio="4.0/3.0" Class="max-w-md">
    <img src="photo.jpg" alt="Photo" class="w-full h-full object-cover rounded-lg" />
</AspectRatio>
```

### Card with Background

```razor
<AspectRatio Ratio="21.0/9.0" Class="bg-muted rounded-lg overflow-hidden">
    <div class="absolute inset-0 flex items-center justify-center">
        <span class="text-muted-foreground">Content centered in 21:9 aspect ratio</span>
    </div>
</AspectRatio>
```

## API Reference

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | The content to render within the aspect ratio container |
| `Ratio` | `double` | `1.0` | The desired aspect ratio (width / height). Common values: 16/9 = 1.778, 4/3 = 1.333, 1/1 = 1 |
| `Class` | `string?` | `null` | Additional CSS classes to apply to the container |
| `AdditionalAttributes` | `Dictionary<string, object>?` | `null` | Additional HTML attributes to apply to the container |

### Common Aspect Ratios

| Ratio | Value | Use Case |
|-------|-------|----------|
| 1:1 | `1` | Square avatars, profile pictures |
| 4:3 | `4.0/3.0` | Traditional photos, presentations |
| 16:9 | `16.0/9.0` | Widescreen video, modern displays |
| 21:9 | `21.0/9.0` | Ultra-wide displays, cinematic content |
| 3:2 | `3.0/2.0` | Classic photography |

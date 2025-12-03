# InputOtp Component

A one-time password (OTP) input component with individual slots for each character. Commonly used for verification codes, two-factor authentication, and similar security features.

## Features

- **Auto-advance**: Automatically moves to the next slot when a digit is entered
- **Backspace Navigation**: Moves to previous slot when backspacing on empty slot
- **Paste Support**: Paste complete OTP codes that auto-fill all slots
- **Keyboard Navigation**: Arrow keys, Home, End for navigation
- **Validation**: Built-in pattern validation (digits by default)
- **Controlled/Uncontrolled**: Supports both modes with `@bind-Value`
- **Accessible**: Full ARIA support for screen readers

## Usage

### Basic Example (6 Digits)

```razor
@using BlazorUI.Components.InputOtp

<InputOtp Length="6" OnComplete="HandleComplete">
    <InputOtpGroup>
        <InputOtpSlot Index="0" />
        <InputOtpSlot Index="1" />
        <InputOtpSlot Index="2" />
    </InputOtpGroup>
    <InputOtpSeparator />
    <InputOtpGroup>
        <InputOtpSlot Index="3" />
        <InputOtpSlot Index="4" />
        <InputOtpSlot Index="5" />
    </InputOtpGroup>
</InputOtp>

@code {
    private void HandleComplete(string value)
    {
        Console.WriteLine($"OTP Complete: {value}");
    }
}
```

### Simple 4-Digit OTP

```razor
<InputOtp Length="4" @bind-Value="otpValue">
    <InputOtpGroup>
        <InputOtpSlot Index="0" />
        <InputOtpSlot Index="1" />
        <InputOtpSlot Index="2" />
        <InputOtpSlot Index="3" />
    </InputOtpGroup>
</InputOtp>

@code {
    private string otpValue = "";
}
```

### Controlled Mode with Validation

```razor
<InputOtp Length="6" 
          @bind-Value="otp" 
          OnValueChange="HandleOtpChange"
          OnComplete="VerifyOtp">
    <InputOtpGroup>
        @for (int i = 0; i < 6; i++)
        {
            var index = i;
            <InputOtpSlot Index="index" />
        }
    </InputOtpGroup>
</InputOtp>

<p>Current value: @otp</p>
<p>Is complete: @(otp.Length == 6 ? "Yes" : "No")</p>

@code {
    private string otp = "";

    private void HandleOtpChange(string value)
    {
        otp = value;
    }

    private async Task VerifyOtp(string value)
    {
        // Verify OTP with backend
        Console.WriteLine($"Verifying OTP: {value}");
    }
}
```

### Alphanumeric OTP

```razor
<InputOtp Length="6" Pattern="[A-Za-z0-9]">
    <InputOtpGroup>
        @for (int i = 0; i < 6; i++)
        {
            var index = i;
            <InputOtpSlot Index="index" />
        }
    </InputOtpGroup>
</InputOtp>
```

### Disabled State

```razor
<InputOtp Length="6" Disabled="true" Value="123456">
    <InputOtpGroup>
        @for (int i = 0; i < 6; i++)
        {
            var index = i;
            <InputOtpSlot Index="index" />
        }
    </InputOtpGroup>
</InputOtp>
```

## Component API

### InputOtp

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Length` | `int` | `6` | Number of OTP slots |
| `Value` | `string?` | `null` | Controlled OTP value |
| `ValueChanged` | `EventCallback<string>` | - | Value change callback |
| `DefaultValue` | `string` | `""` | Default value (uncontrolled) |
| `OnValueChange` | `EventCallback<string>` | - | Callback when value changes |
| `OnComplete` | `EventCallback<string>` | - | Callback when all slots filled |
| `Pattern` | `string` | `"[0-9]"` | Regex pattern for validation |
| `Disabled` | `bool` | `false` | Whether input is disabled |
| `AriaLabel` | `string` | `"One-time password"` | Accessible label |
| `Class` | `string?` | `null` | Additional CSS classes |

### InputOtpSlot

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Index` | `int` | Required | Slot index (0-based) |
| `Class` | `string?` | `null` | Additional CSS classes |

### InputOtpGroup

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Child slots |
| `Class` | `string?` | `null` | Additional CSS classes |

### InputOtpSeparator

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `"-"` | Custom separator content |
| `Class` | `string?` | `null` | Additional CSS classes |

## Keyboard Navigation

| Key | Action |
|-----|--------|
| `0-9` | Enter digit in current slot |
| `Backspace` | Clear current slot or move to previous |
| `Delete` | Clear current slot |
| `ArrowLeft` | Move to previous slot |
| `ArrowRight` | Move to next slot |
| `Home` | Move to first slot |
| `End` | Move to last slot |

## Paste Support

The component supports pasting OTP codes. When you paste:
- The pasted value fills slots starting from the first slot
- Invalid characters (per the Pattern) are filtered out
- Focus moves to the last filled slot

## Data Attributes

The slots expose data attributes for custom styling:

- `data-slot`: The slot index
- `data-active`: `"true"` when the slot is focused
- `data-filled`: `"true"` when the slot has a value

```css
/* Example custom styling */
[data-active="true"] {
    border-color: var(--ring);
}

[data-filled="true"] {
    background-color: var(--accent);
}
```

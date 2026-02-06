using System.Globalization;
using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorBlueprint.Components.Rating;

/// <summary>
/// A rating component with customizable icons and half-rating support.
/// </summary>
public partial class Rating : ComponentBase
{
    private readonly string _instanceId = Guid.NewGuid().ToString("N")[..8];
    private double _hoverValue;

    /// <summary>
    /// Gets or sets the current rating value.
    /// </summary>
    [Parameter]
    public double Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the value changes.
    /// </summary>
    [Parameter]
    public EventCallback<double> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the maximum rating value.
    /// </summary>
    [Parameter]
    public int Max { get; set; } = 5;

    /// <summary>
    /// Gets or sets whether half ratings are allowed.
    /// </summary>
    [Parameter]
    public bool AllowHalf { get; set; }

    /// <summary>
    /// Gets or sets whether clicking the current value clears the rating.
    /// </summary>
    [Parameter]
    public bool AllowClear { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the rating is read-only (display only).
    /// </summary>
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Gets or sets whether the rating is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the icon type.
    /// </summary>
    [Parameter]
    public RatingIcon Icon { get; set; } = RatingIcon.Star;

    /// <summary>
    /// Gets or sets the color for active (filled) icons.
    /// </summary>
    [Parameter]
    public string? ActiveColor { get; set; }

    /// <summary>
    /// Gets or sets the color for inactive (empty) icons.
    /// </summary>
    [Parameter]
    public string? InactiveColor { get; set; }

    /// <summary>
    /// Gets or sets a custom icon template.
    /// </summary>
    [Parameter]
    public RenderFragment<RatingIconContext>? IconTemplate { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the rating group.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the size of the icons.
    /// </summary>
    [Parameter]
    public RatingSize Size { get; set; } = RatingSize.Default;

    private string CssClass => ClassNames.cn(
        "inline-flex items-center gap-1",
        (Disabled || ReadOnly) ? "cursor-default" : "cursor-pointer",
        Disabled ? "opacity-50" : null,
        Class
    );

    private string IconClass => ClassNames.cn(
        Size switch
        {
            RatingSize.Small => "h-4 w-4",
            RatingSize.Large => "h-8 w-8",
            _ => "h-6 w-6"
        },
        "transition-transform",
        !(Disabled || ReadOnly) ? "hover:scale-110" : null
    );

    private string GetIconContainerClass(int index)
    {
        var isActive = GetFillPercentage(index) > 0;
        return ClassNames.cn(
            "relative inline-flex",
            !(Disabled || ReadOnly) ? "cursor-pointer" : null
        );
    }

    private double GetFillPercentage(int index)
    {
        var currentValue = _hoverValue > 0 ? _hoverValue : Value;

        if (currentValue >= index)
        {
            return 1;
        }

        if (AllowHalf && currentValue >= index - 0.5)
        {
            return 0.5;
        }

        return 0;
    }

    private async Task HandleClick(int index)
    {
        if (Disabled || ReadOnly)
        {
            return;
        }

        double newValue;

        if (AllowHalf)
        {
            // If clicking the same value, toggle between half and full, or clear
            if (Value == index)
            {
                newValue = AllowClear ? 0 : index;
            }
            else if (Value == index - 0.5)
            {
                newValue = index;
            }
            else
            {
                newValue = index;
            }
        }
        else
        {
            // Simple click behavior
            if (AllowClear && Value == index)
            {
                newValue = 0;
            }
            else
            {
                newValue = index;
            }
        }

        if (newValue != Value)
        {
            Value = newValue;
            await ValueChanged.InvokeAsync(newValue);
        }
    }

    private void HandleMouseMove(MouseEventArgs e, int index)
    {
        if (Disabled || ReadOnly)
        {
            return;
        }

        if (AllowHalf)
        {
            // Determine if hovering over the left or right half of the icon
            // This is a simplified approach - in practice you'd need JS to get exact position
            _hoverValue = index;
        }
        else
        {
            _hoverValue = index;
        }
    }

    private void HandleMouseLeave() => _hoverValue = 0;

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (Disabled || ReadOnly)
        {
            return;
        }

        var step = AllowHalf ? 0.5 : 1.0;
        var newValue = Value;

        switch (e.Key)
        {
            case "ArrowRight":
            case "ArrowUp":
                newValue = Math.Min(Max, Value + step);
                break;
            case "ArrowLeft":
            case "ArrowDown":
                newValue = Math.Max(0, Value - step);
                break;
            case "Home":
                newValue = 0;
                break;
            case "End":
                newValue = Max;
                break;
            case "1":
            case "2":
            case "3":
            case "4":
            case "5":
            case "6":
            case "7":
            case "8":
            case "9":
                var numValue = int.Parse(e.Key, CultureInfo.InvariantCulture);
                if (numValue <= Max)
                {
                    newValue = numValue;
                }
                break;
            case "0":
                if (AllowClear)
                {
                    newValue = 0;
                }
                break;
        }

        if (newValue != Value)
        {
            Value = newValue;
            await ValueChanged.InvokeAsync(newValue);
        }
    }
}

/// <summary>
/// Defines the size of the Rating component icons.
/// </summary>
public enum RatingSize
{
    /// <summary>
    /// Small size (16px).
    /// </summary>
    Small,

    /// <summary>
    /// Default size (24px).
    /// </summary>
    Default,

    /// <summary>
    /// Large size (32px).
    /// </summary>
    Large
}

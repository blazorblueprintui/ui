using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A callout component that displays a prominent message to users.
/// </summary>
/// <remarks>
/// <para>
/// The Alert component provides a way to display important messages that attract
/// user attention. It follows accessibility guidelines with proper ARIA attributes.
/// </para>
/// <para>
/// Features:
/// - 5 visual variants (Default, Success, Info, Warning, Danger)
/// - Optional left accent border with subtle tinted background
/// - Optional icon support
/// - Auto-dismiss with optional countdown bar and pause-on-hover
/// - Action button slot for inline actions (Undo, Retry, etc.)
/// - Semantic HTML with role="alert"
/// - Dark mode compatible via CSS variables
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Alert Variant="AlertVariant.Default"&gt;
///     &lt;AlertTitle&gt;Heads up!&lt;/AlertTitle&gt;
///     &lt;AlertDescription&gt;You can add components to your app.&lt;/AlertDescription&gt;
/// &lt;/Alert&gt;
/// </code>
/// </example>
public partial class BbAlert : ComponentBase, IAsyncDisposable
{
    /// <summary>
    /// Gets or sets the visual style variant of the alert.
    /// </summary>
    /// <remarks>
    /// Controls the color scheme and visual appearance using CSS custom properties.
    /// Default value is <see cref="AlertVariant.Default"/>.
    /// </remarks>
    [Parameter]
    public AlertVariant Variant { get; set; } = AlertVariant.Default;

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the alert.
    /// </summary>
    /// <remarks>
    /// Custom classes are appended after the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the alert.
    /// </summary>
    /// <remarks>
    /// Typically contains AlertTitle and AlertDescription components.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets an optional icon to display in the alert.
    /// </summary>
    /// <remarks>
    /// Can be any RenderFragment (SVG, icon font, image).
    /// Positioned absolutely in the top-left corner.
    /// </remarks>
    [Parameter]
    public RenderFragment? Icon { get; set; }

    /// <summary>
    /// Gets or sets whether to show a thick left accent border.
    /// </summary>
    /// <remarks>
    /// When true, displays a 4px left border in the variant's accent color.
    /// When false (default), displays a standard 1px border on all sides.
    /// </remarks>
    [Parameter]
    public bool AccentBorder { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the alert can be dismissed by the user.
    /// </summary>
    /// <remarks>
    /// When true, a close button (X icon) is rendered in the top-right corner.
    /// Clicking the button invokes the <see cref="OnDismiss"/> callback.
    /// </remarks>
    [Parameter]
    public bool Dismissible { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the alert is dismissed.
    /// </summary>
    /// <remarks>
    /// Invoked when the dismiss button is clicked or when the auto-dismiss timer expires.
    /// Use this to hide or remove the alert from the UI.
    /// </remarks>
    [Parameter]
    public EventCallback OnDismiss { get; set; }

    /// <summary>
    /// Gets or sets the duration in milliseconds after which the alert is automatically dismissed.
    /// </summary>
    /// <remarks>
    /// When null (default), the alert does not auto-dismiss. When set, a countdown timer starts
    /// after the component renders and invokes <see cref="OnDismiss"/> when it expires.
    /// </remarks>
    [Parameter]
    public int? AutoDismissAfter { get; set; }

    /// <summary>
    /// Gets or sets whether to pause the auto-dismiss countdown when the mouse hovers over the alert.
    /// </summary>
    /// <remarks>
    /// Only effective when <see cref="AutoDismissAfter"/> is set. Default is true.
    /// </remarks>
    [Parameter]
    public bool PauseOnHover { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show a visual countdown progress bar at the bottom of the alert.
    /// </summary>
    /// <remarks>
    /// Only visible when <see cref="AutoDismissAfter"/> is set. Default is false.
    /// </remarks>
    [Parameter]
    public bool ShowCountdown { get; set; } = false;

    /// <summary>
    /// Gets or sets optional action buttons to display below the alert content.
    /// </summary>
    /// <remarks>
    /// Actions render after the ChildContent and before the dismiss button.
    /// Use small, outline, or ghost buttons for best visual results.
    /// </remarks>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    private CancellationTokenSource? dismissCts;
    private double remainingPercent = 100.0;
    private bool isPaused;
    private bool dismissed;

    /// <summary>
    /// Gets the computed CSS classes for the alert element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        // Base alert styles
        "relative w-full rounded-lg border p-4 text-foreground",
        // Extra right padding for dismiss button
        Dismissible ? "pr-10" : null,
        // Extra bottom padding for countdown bar
        ShowCountdown && AutoDismissAfter.HasValue ? "pb-2" : null,
        // Accent border style (thick left border)
        AccentBorder ? "border-l-4" : null,
        Icon != null ? "[&>svg+div]:translate-y-[-3px] [&>svg]:absolute [&>svg]:left-4 [&>svg]:top-4 [&:has(svg)]:pl-11" : null,
        // Variant-specific styles (border color, background tint, icon color)
        Variant switch
        {
            AlertVariant.Default => "bg-muted/30 [&>svg]:text-muted-foreground",
            AlertVariant.Success => AccentBorder
                ? "border-l-alert-success bg-alert-success-bg border-alert-success/30 [&>svg]:text-alert-success"
                : "border-alert-success/30 bg-alert-success-bg [&>svg]:text-alert-success",
            AlertVariant.Info => AccentBorder
                ? "border-l-alert-info bg-alert-info-bg border-alert-info/30 [&>svg]:text-alert-info"
                : "border-alert-info/30 bg-alert-info-bg [&>svg]:text-alert-info",
            AlertVariant.Warning => AccentBorder
                ? "border-l-alert-warning bg-alert-warning-bg border-alert-warning/30 [&>svg]:text-alert-warning"
                : "border-alert-warning/30 bg-alert-warning-bg [&>svg]:text-alert-warning",
            AlertVariant.Danger => AccentBorder
                ? "border-l-alert-danger bg-alert-danger-bg border-alert-danger/30 [&>svg]:text-alert-danger"
                : "border-alert-danger/30 bg-alert-danger-bg [&>svg]:text-alert-danger",
            _ => "bg-muted/30 [&>svg]:text-muted-foreground"
        },
        // Custom classes (if provided)
        Class
    );

    protected override async Task OnParametersSetAsync()
    {
        if (AutoDismissAfter.HasValue && dismissCts == null && !dismissed)
        {
            await StartAutoDismissAsync();
        }
    }

    private async Task StartAutoDismissAsync()
    {
        dismissCts = new CancellationTokenSource();
        var token = dismissCts.Token;
        var totalMs = AutoDismissAfter!.Value;
        var elapsedMs = 0;
        const int tickMs = 100;

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(tickMs));

        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                if (isPaused)
                {
                    continue;
                }

                elapsedMs += tickMs;

                if (ShowCountdown)
                {
                    remainingPercent = Math.Max(0.0, 100.0 - (elapsedMs * 100.0 / totalMs));
                    StateHasChanged();
                }

                if (elapsedMs >= totalMs)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (!token.IsCancellationRequested)
        {
            dismissed = true;
            await HandleDismiss();
        }
    }

    private void HandleMouseEnter()
    {
        if (PauseOnHover && AutoDismissAfter.HasValue)
        {
            isPaused = true;
        }
    }

    private void HandleMouseLeave()
    {
        if (PauseOnHover && AutoDismissAfter.HasValue)
        {
            isPaused = false;
        }
    }

    /// <summary>
    /// Handles the dismiss button click or auto-dismiss expiration.
    /// </summary>
    private async Task HandleDismiss()
    {
        dismissed = true;
        dismissCts?.Cancel();

        if (OnDismiss.HasDelegate)
        {
            await OnDismiss.InvokeAsync();
        }
    }

    public ValueTask DisposeAsync()
    {
        dismissCts?.Cancel();
        dismissCts?.Dispose();
        dismissCts = null;
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}

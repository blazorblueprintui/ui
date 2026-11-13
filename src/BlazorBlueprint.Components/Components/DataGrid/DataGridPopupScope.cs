using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorBlueprint.Components;

/// <summary>
/// Holds the open state of one header popup (a column's filter popover or column menu), so that
/// opening or closing it re-renders only this scope rather than the whole grid.
/// </summary>
/// <remarks>
/// The open state used to live on the grid, and the popup's <c>OpenChanged</c> was a lambda written
/// in the grid's markup, which makes the grid the event's receiver. Every open and close therefore
/// re-rendered every row: opening a filter took 33 ms at 10 rows and 51 ms at 100 in a published
/// WebAssembly build (#655). Here the callback's receiver is this scope instead.
/// <para>
/// The popup must still receive its controlled value on every change, or its open state goes stale
/// and dismissing it stops reporting back (#437). This scope re-renders itself on each change, so
/// that still holds.
/// </para>
/// </remarks>
internal sealed class DataGridPopupScope : ComponentBase
{
    private readonly EventCallback<bool> openChanged;
    private bool open;
    private int lastCloseVersion;

    public DataGridPopupScope()
    {
        openChanged = EventCallback.Factory.Create<bool>(this, value => open = value);
    }

    /// <summary>
    /// The popup, given its current open state and the callback to report changes through.
    /// </summary>
    [Parameter]
    public RenderFragment<DataGridPopupState>? ChildContent { get; set; }

    /// <summary>
    /// Closes the popup whenever it changes. The grid bumps it after an action taken from a popup
    /// (applying a filter, grouping by the column) so the popup closes along with the grid's update.
    /// </summary>
    [Parameter]
    public int CloseVersion { get; set; }

    protected override void OnInitialized() => lastCloseVersion = CloseVersion;

    protected override void OnParametersSet()
    {
        if (CloseVersion != lastCloseVersion)
        {
            lastCloseVersion = CloseVersion;
            open = false;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder) =>
        builder.AddContent(0, ChildContent?.Invoke(new DataGridPopupState(open, openChanged)));
}

/// <summary>
/// The open state handed to a popup inside a <see cref="DataGridPopupScope"/>.
/// </summary>
/// <param name="Open">Whether the popup is open.</param>
/// <param name="OpenChanged">Reports a change; its receiver is the scope, not the grid.</param>
internal readonly record struct DataGridPopupState(bool Open, EventCallback<bool> OpenChanged);

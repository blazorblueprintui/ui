using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Primitives.Table;

/// <summary>
/// A headless pagination component that provides pagination controls for a table.
/// Developers have complete control over the markup and styling.
/// </summary>
/// <typeparam name="TData">The type of data displayed in the table.</typeparam>
public partial class TablePagination<TData> : ComponentBase, IDisposable
{
    private bool _disposed;
    private PaginationContext _context = default!;

    /// <summary>
    /// Gets or sets the pagination state to control and display.
    /// </summary>
    [Parameter]
    public PaginationState State { get; set; } = default!;

    /// <summary>
    /// Gets or sets the callback invoked when the page changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> OnPageChange { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the page size changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> OnPageSizeChange { get; set; }

    /// <summary>
    /// Gets or sets the available page sizes for the selector.
    /// Default: [10, 25, 50, 100]
    /// </summary>
    [Parameter]
    public int[] PageSizeOptions { get; set; } = [10, 25, 50, 100];

    /// <summary>
    /// Gets or sets whether to show the page size selector.
    /// Default: true
    /// </summary>
    [Parameter]
    public bool ShowPageSizeSelector { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show page info (e.g., "1-10 of 100 items").
    /// Default: true
    /// </summary>
    [Parameter]
    public bool ShowPageInfo { get; set; } = true;

    /// <summary>
    /// Gets or sets the custom pagination template.
    /// Provides full control over pagination markup.
    /// </summary>
    [Parameter]
    public RenderFragment<PaginationContext>? PaginationTemplate { get; set; }

    /// <summary>
    /// Gets or sets the child content (used when PaginationTemplate is not provided).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets the pagination context for the template.
    /// </summary>
    private PaginationContext Context => _context;

    /// <summary>
    /// Initializes the component and creates the pagination context.
    /// </summary>
    protected override void OnInitialized()
    {
        _context = new PaginationContext
        {
            State = State,
            PageSizeOptions = PageSizeOptions,
            ShowPageSizeSelector = ShowPageSizeSelector,
            ShowPageInfo = ShowPageInfo,
            GoToFirstPage = HandleFirstPage,
            GoToPreviousPage = HandlePreviousPage,
            GoToNextPage = HandleNextPage,
            GoToLastPage = HandleLastPage,
            GoToPage = HandleGoToPage,
            ChangePageSize = HandlePageSizeChange
        };
    }

    /// <summary>
    /// Handles navigation to the first page.
    /// </summary>
    private async Task HandleFirstPage()
    {
        if (State.CurrentPage == 1)
        {
            return;
        }

        State.FirstPage();
        await NotifyPageChange();
    }

    /// <summary>
    /// Handles navigation to the previous page.
    /// </summary>
    private async Task HandlePreviousPage()
    {
        if (!State.CanGoPrevious)
        {
            return;
        }

        State.PreviousPage();
        await NotifyPageChange();
    }

    /// <summary>
    /// Handles navigation to the next page.
    /// </summary>
    private async Task HandleNextPage()
    {
        if (!State.CanGoNext)
        {
            return;
        }

        State.NextPage();
        await NotifyPageChange();
    }

    /// <summary>
    /// Handles navigation to the last page.
    /// </summary>
    private async Task HandleLastPage()
    {
        if (State.CurrentPage == State.TotalPages)
        {
            return;
        }

        State.LastPage();
        await NotifyPageChange();
    }

    /// <summary>
    /// Handles navigation to a specific page.
    /// </summary>
    /// <param name="page">The page number to navigate to (1-based).</param>
    private async Task HandleGoToPage(int page)
    {
        if (State.CurrentPage == page)
        {
            return;
        }

        State.GoToPage(page);
        await NotifyPageChange();
    }

    /// <summary>
    /// Handles page size change.
    /// </summary>
    /// <param name="newPageSize">The new page size.</param>
    private async Task HandlePageSizeChange(int newPageSize)
    {
        if (State.PageSize == newPageSize)
        {
            return;
        }

        State.PageSize = newPageSize;

        if (OnPageSizeChange.HasDelegate)
        {
            await OnPageSizeChange.InvokeAsync(newPageSize);
        }

        await NotifyPageChange();
    }

    /// <summary>
    /// Notifies listeners that the page has changed.
    /// </summary>
    private async Task NotifyPageChange()
    {
        if (OnPageChange.HasDelegate)
        {
            await OnPageChange.InvokeAsync(State.CurrentPage);
        }

        if (!_disposed)
        {
            StateHasChanged();
        }
    }

    /// <summary>
    /// Disposes resources used by the component.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _disposed = true;
    }
}

/// <summary>
/// Context provided to the pagination template.
/// </summary>
public class PaginationContext
{
    /// <summary>
    /// Gets or sets the pagination state.
    /// </summary>
    public PaginationState State { get; set; } = default!;

    /// <summary>
    /// Gets or sets the available page size options.
    /// </summary>
    public int[] PageSizeOptions { get; set; } = [];

    /// <summary>
    /// Gets or sets whether to show the page size selector.
    /// </summary>
    public bool ShowPageSizeSelector { get; set; }

    /// <summary>
    /// Gets or sets whether to show page info.
    /// </summary>
    public bool ShowPageInfo { get; set; }

    /// <summary>
    /// Gets the action to navigate to the first page.
    /// </summary>
    public Func<Task> GoToFirstPage { get; set; } = default!;

    /// <summary>
    /// Gets the action to navigate to the previous page.
    /// </summary>
    public Func<Task> GoToPreviousPage { get; set; } = default!;

    /// <summary>
    /// Gets the action to navigate to the next page.
    /// </summary>
    public Func<Task> GoToNextPage { get; set; } = default!;

    /// <summary>
    /// Gets the action to navigate to the last page.
    /// </summary>
    public Func<Task> GoToLastPage { get; set; } = default!;

    /// <summary>
    /// Gets the action to navigate to a specific page.
    /// </summary>
    public Func<int, Task> GoToPage { get; set; } = default!;

    /// <summary>
    /// Gets the action to change the page size.
    /// </summary>
    public Func<int, Task> ChangePageSize { get; set; } = default!;

    /// <summary>
    /// Gets the page info text (e.g., "1-10 of 100 items").
    /// </summary>
    public string GetPageInfoText()
    {
        if (State.TotalItems == 0)
        {
            return "No items";
        }


        return $"{State.FirstItemIndex}-{State.LastItemIndex} of {State.TotalItems} items";
    }

    /// <summary>
    /// Gets the page range for displaying page numbers (e.g., for "1 2 3 ... 10" style pagination).
    /// </summary>
    /// <param name="maxPages">Maximum number of page buttons to show.</param>
    /// <returns>Array of page numbers to display.</returns>
    public int[] GetPageRange(int maxPages = 5)
    {
        if (State.TotalPages <= maxPages)
        {
            return Enumerable.Range(1, State.TotalPages).ToArray();
        }

        var halfMax = maxPages / 2;
        var startPage = Math.Max(1, State.CurrentPage - halfMax);
        var endPage = Math.Min(State.TotalPages, startPage + maxPages - 1);

        if (endPage - startPage < maxPages - 1)
        {
            startPage = Math.Max(1, endPage - maxPages + 1);
        }

        return Enumerable.Range(startPage, endPage - startPage + 1).ToArray();
    }
}

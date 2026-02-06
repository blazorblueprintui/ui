namespace BlazorBlueprint.Primitives.Table;

/// <summary>
/// Manages pagination configuration and state for a table.
/// Handles page navigation, page size, and boundary calculations.
/// </summary>
public class PaginationState
{
    private int currentPage = 1;
    private int pageSize = 10;
    private int totalItems;

    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// Automatically clamped to valid range (1 to TotalPages).
    /// </summary>
    public int CurrentPage
    {
        get => currentPage;
        set => currentPage = Math.Max(1, value);
    }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// Must be at least 1.
    /// </summary>
    public int PageSize
    {
        get => pageSize;
        set
        {
            pageSize = Math.Max(1, value);
            // Reset to page 1 when page size changes
            currentPage = 1;
        }
    }

    /// <summary>
    /// Gets or sets the total number of items in the dataset.
    /// Updated automatically during data processing.
    /// </summary>
    public int TotalItems
    {
        get => totalItems;
        set
        {
            totalItems = Math.Max(0, value);
            // Clamp current page to valid range
            if (currentPage > TotalPages && TotalPages > 0)
            {
                currentPage = TotalPages;
            }
        }
    }

    /// <summary>
    /// Gets the total number of pages based on total items and page size.
    /// </summary>
    public int TotalPages => TotalItems == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    /// <summary>
    /// Gets the zero-based start index for the current page.
    /// </summary>
    public int StartIndex => (CurrentPage - 1) * PageSize;

    /// <summary>
    /// Gets the zero-based end index for the current page (exclusive).
    /// </summary>
    public int EndIndex => Math.Min(StartIndex + PageSize, TotalItems);

    /// <summary>
    /// Gets whether navigation to the next page is possible.
    /// </summary>
    public bool CanGoNext => CurrentPage < TotalPages;

    /// <summary>
    /// Gets whether navigation to the previous page is possible.
    /// </summary>
    public bool CanGoPrevious => CurrentPage > 1;

    /// <summary>
    /// Gets the one-based index of the first item on the current page.
    /// </summary>
    public int FirstItemIndex => TotalItems == 0 ? 0 : StartIndex + 1;

    /// <summary>
    /// Gets the one-based index of the last item on the current page.
    /// </summary>
    public int LastItemIndex => EndIndex;

    /// <summary>
    /// Navigates to the next page if possible.
    /// </summary>
    public void NextPage()
    {
        if (CanGoNext)
        {
            CurrentPage++;
        }
    }

    /// <summary>
    /// Navigates to the previous page if possible.
    /// </summary>
    public void PreviousPage()
    {
        if (CanGoPrevious)
        {
            CurrentPage--;
        }
    }

    /// <summary>
    /// Navigates to the first page.
    /// </summary>
    public void FirstPage() => CurrentPage = 1;

    /// <summary>
    /// Navigates to the last page.
    /// </summary>
    public void LastPage()
    {
        if (TotalPages > 0)
        {
            CurrentPage = TotalPages;
        }
    }

    /// <summary>
    /// Navigates to a specific page.
    /// Page number is automatically clamped to valid range.
    /// </summary>
    /// <param name="page">The page number to navigate to (1-based).</param>
    public void GoToPage(int page) => CurrentPage = Math.Max(1, Math.Min(page, TotalPages > 0 ? TotalPages : 1));

    /// <summary>
    /// Resets pagination to the first page.
    /// Useful when data or filters change.
    /// </summary>
    public void Reset() => CurrentPage = 1;
}

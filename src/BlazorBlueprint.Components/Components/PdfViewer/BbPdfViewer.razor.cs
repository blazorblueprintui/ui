using System.Globalization;
using System.IO;
using BlazorBlueprint.Primitives.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

/// <summary>
/// A PDF viewer component built on PDF.js that follows the shadcn/ui design system.
/// The document is rendered into a canvas with a toolbar for page navigation, zoom
/// and fit-to-width, and exposes the same actions on the component reference.
/// </summary>
public partial class BbPdfViewer : ComponentBase, IAsyncDisposable
{
    // === Private Fields ===
    private ElementReference canvas;
    private IJSObjectReference? jsModule;
    private bool jsInitialized;
    private string? lastKnownUrl;

    private bool isLoading;
    private string? loadError;
    private int pageCount;
    private int currentPage;
    private double scale = 1.25;
    private string pageInput = "1";

    private const double ScaleEpsilon = 0.000001;

    // Snapshot of everything that shapes the rendered output, taken inside
    // ShouldRender so a parent re-render with unchanged values never forces
    // the viewer to redraw.
    private bool lastIsLoading;
    private string? lastLoadError;
    private int lastPageCount;
    private int lastCurrentPage;
    private double lastScale;
    private string? lastPageInput;
    private string? lastUrl;
    private string? lastId;
    private bool lastToolbar = true;
    private string? lastClass;
    private string? lastAriaLabel;
    private string? lastHeight;

    // === Parameters - Source ===

    /// <summary>
    /// Gets or sets the URL of the PDF document to display. Set a new value to
    /// replace the document; set <c>null</c> or empty to clear the viewer. Omit
    /// it entirely and call <see cref="LoadDataAsync(byte[])"/> instead to show
    /// a document from local bytes.
    /// </summary>
    [Parameter]
    public string? Url { get; set; }

    // === Parameters - Appearance ===

    /// <summary>
    /// Gets or sets whether the toolbar (page navigation, zoom, fit-to-width and
    /// download) is rendered. Default <c>true</c>.
    /// </summary>
    [Parameter]
    public bool Toolbar { get; set; } = true;

    /// <summary>
    /// Gets or sets the zoom scale used when a document first loads. Default 1.25.
    /// </summary>
    [Parameter]
    public double DefaultScale { get; set; } = 1.25;

    /// <summary>
    /// Gets or sets the smallest zoom scale the toolbar and methods allow. Default 0.5.
    /// </summary>
    [Parameter]
    public double MinScale { get; set; } = 0.5;

    /// <summary>
    /// Gets or sets the largest zoom scale the toolbox toolbar and methods allow. Default 3.0.
    /// </summary>
    [Parameter]
    public double MaxScale { get; set; } = 3.0;

    /// <summary>
    /// Gets or sets the height of the scrollable canvas area, as a CSS length.
    /// When null, the viewer fills its parent's height. Default "640px".
    /// </summary>
    [Parameter]
    public string? Height { get; set; } = "640px";

    /// <summary>
    /// Gets or sets additional CSS classes for the container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the HTML id attribute for the container.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the rendered page. When null, a localized
    /// default ("PDF document") is used.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    // === Parameters - Events ===

    /// <summary>
    /// Gets or sets the callback invoked after a document loads, with the number
    /// of pages in it.
    /// </summary>
    [Parameter]
    public EventCallback<int> OnDocumentLoaded { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the visible page changes.
    /// </summary>
    [Parameter]
    public EventCallback<PdfViewerPageChangeEventArgs> OnPageChanged { get; set; }

    // === Lifecycle Methods ===

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeJsAsync();
        }

        // Reload whenever the Url parameter has changed since the current document
        // was loaded. ShouldRender gates renders, so this only runs on real changes.
        if (jsInitialized && jsModule is not null
            && !string.Equals(Url, lastKnownUrl, StringComparison.Ordinal))
        {
            await LoadPdfAsync();
        }
    }

    private async Task InitializeJsAsync()
    {
        if (jsInitialized)
        {
            return;
        }

        try
        {
            jsModule = await JsModules.GetAsync(
                JS,
                "./_content/BlazorBlueprint.Components/js/pdfjs-interop.js");

            jsInitialized = true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"Failed to initialize PdfViewer JS: {ex.Message}");
        }
    }

    private async Task LoadPdfAsync()
    {
        if (jsModule is null)
        {
            return;
        }

        lastKnownUrl = Url;
        isLoading = true;
        loadError = null;
        pageCount = 0;
        currentPage = 0;
        scale = ClampScale(DefaultScale);
        pageInput = "1";
        StateHasChanged();

        if (string.IsNullOrWhiteSpace(Url))
        {
            await jsModule.InvokeVoidAsync("clear", canvas);
            isLoading = false;
            StateHasChanged();
            return;
        }

        try
        {
            var state = await jsModule.InvokeAsync<PdfViewerState>(
                "load", canvas, Url, BuildOptions());

            ApplyState(state);

            if (state is { Ok: true })
            {
                await OnDocumentLoaded.InvokeAsync(state.PageCount);
            }
        }
        catch (Exception ex) when (
            ex is JSDisconnectedException
            or JSException
            or TaskCanceledException
            or ObjectDisposedException
            or InvalidOperationException)
        {
            isLoading = false;
            loadError = Localizer["PdfViewer.LoadFailed"];
            StateHasChanged();
        }
    }

    private object BuildOptions() => new
    {
        initialScale = ClampScale(DefaultScale),
        minScale = Math.Max(MinScale, 0.1),
        maxScale = Math.Max(MaxScale, Math.Max(MinScale, 0.1))
    };

    // === State Helpers ===

    private void ApplyState(PdfViewerState? state)
    {
        isLoading = false;

        if (state is null || !state.Ok)
        {
            loadError = string.IsNullOrEmpty(state?.Error)
                ? Localizer["PdfViewer.LoadFailed"]
                : state!.Error;
            pageCount = 0;
            currentPage = 0;
            StateHasChanged();
            return;
        }

        loadError = null;
        pageCount = state.PageCount;
        currentPage = state.CurrentPage;
        scale = state.Scale;
        pageInput = pageCount > 0
            ? currentPage.ToString(CultureInfo.InvariantCulture)
            : "1";
        StateHasChanged();
    }

    private double ClampScale(double value)
    {
        var min = Math.Max(MinScale, 0.1);
        var max = Math.Max(MaxScale, min);
        return Math.Clamp(value, min, max);
    }

    // === Toolbar Actions ===

    private async Task NavigatePageAsync(string jsMethod)
    {
        if (jsModule is null || !jsInitialized)
        {
            return;
        }

        var before = currentPage;
        var state = await jsModule.InvokeAsync<PdfViewerState>(jsMethod, canvas);
        ApplyState(state);

        if (currentPage > 0 && currentPage != before)
        {
            await OnPageChanged.InvokeAsync(new PdfViewerPageChangeEventArgs
            {
                Page = currentPage,
                PageCount = pageCount
            });
        }
    }

    private async Task ZoomAsync(string jsMethod, object? argument = null)
    {
        if (jsModule is null || !jsInitialized)
        {
            return;
        }

        var state = argument is null
            ? await jsModule.InvokeAsync<PdfViewerState>(jsMethod, canvas)
            : await jsModule.InvokeAsync<PdfViewerState>(jsMethod, canvas, argument);
        ApplyState(state);
    }

    private async Task GoToPageInputAsync()
    {
        if (!int.TryParse(pageInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out var page))
        {
            pageInput = pageCount > 0
                ? currentPage.ToString(CultureInfo.InvariantCulture)
                : "1";
            return;
        }

        if (page == currentPage)
        {
            pageInput = currentPage.ToString(CultureInfo.InvariantCulture);
            return;
        }

        await GoToPageAsync(page);

        if (pageCount == 0)
        {
            pageInput = "1";
        }
    }

    // === Public API Methods ===

    /// <summary>Moves to the previous page, if any.</summary>
    public Task PreviousPageAsync() => NavigatePageAsync("previousPage");

    /// <summary>Moves to the next page, if any.</summary>
    public Task NextPageAsync() => NavigatePageAsync("nextPage");

    /// <summary>
    /// Jumps to a specific page (1-based). Out-of-range values are clamped to the
    /// document's range. No-op when no document is loaded.
    /// </summary>
    public async Task GoToPageAsync(int page)
    {
        if (jsModule is null || !jsInitialized || pageCount == 0)
        {
            return;
        }

        var before = currentPage;
        var state = await jsModule.InvokeAsync<PdfViewerState>("gotoPage", canvas, page);
        ApplyState(state);

        if (currentPage > 0 && currentPage != before)
        {
            await OnPageChanged.InvokeAsync(new PdfViewerPageChangeEventArgs
            {
                Page = currentPage,
                PageCount = pageCount
            });
        }
    }

    /// <summary>Zooms in by one step.</summary>
    public Task ZoomInAsync() => ZoomAsync("zoomIn");

    /// <summary>Zooms out by one step.</summary>
    public Task ZoomOutAsync() => ZoomAsync("zoomOut");

    /// <summary>
    /// Sets the zoom scale, clamped to <see cref="MinScale"/> and <see cref="MaxScale"/>.
    /// </summary>
    public Task SetScaleAsync(double scale) => ZoomAsync("setScale", scale);

    /// <summary>Fits the current page to the width of its scroll area.</summary>
    public Task FitToWidthAsync() => ZoomAsync("fitToWidth");

    /// <summary>
    /// Loads a PDF document from a byte array (for example bytes read from an
    /// <c>InputFile</c>), replacing whatever document was showing. The bytes are
    /// streamed to the browser, so no URL or web request is involved.
    /// </summary>
    /// <remarks>
    /// A subsequent <see cref="Url"/> change replaces the byte-loaded document.
    /// </remarks>
    public async Task LoadDataAsync(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        using var stream = new MemoryStream(data, writable: false);
        await LoadDataAsync(stream);
    }

    /// <summary>
    /// Loads a PDF document from a stream (for example
    /// <c>IBrowserFile.OpenReadStream()</c>), replacing whatever document was
    /// showing. The stream is streamed to the browser, so no URL or web request
    /// is involved. The stream is consumed and disposed by the transfer, so pass
    /// a fresh stream per load.
    /// </summary>
    /// <remarks>
    /// A subsequent <see cref="Url"/> change replaces the byte-loaded document.
    /// </remarks>
    public async Task LoadDataAsync(Stream data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (jsModule is null || !jsInitialized)
        {
            return;
        }

        // A programmatic load replaces the current document; keep the Url marker
        // in sync so an unchanged Url parameter does not reload it afterwards.
        lastKnownUrl = Url;
        isLoading = true;
        loadError = null;
        pageCount = 0;
        currentPage = 0;
        scale = ClampScale(DefaultScale);
        pageInput = "1";
        StateHasChanged();

        try
        {
            using (var streamReference = new DotNetStreamReference(data))
            {
                var state = await jsModule.InvokeAsync<PdfViewerState>(
                    "loadData", canvas, streamReference, BuildOptions());

                ApplyState(state);

                if (state is { Ok: true })
                {
                    await OnDocumentLoaded.InvokeAsync(state.PageCount);
                }
            }
        }
        catch (Exception ex) when (
            ex is JSDisconnectedException
            or JSException
            or TaskCanceledException
            or ObjectDisposedException
            or InvalidOperationException)
        {
            isLoading = false;
            loadError = Localizer["PdfViewer.LoadFailed"];
            StateHasChanged();
        }
    }

    /// <summary>
    /// Saves the open document as a PDF file. Byte-loaded documents are saved
    /// straight from memory; URL-loaded documents are saved as a blob so a
    /// cross-origin source is downloaded rather than navigated to. No-op before
    /// a document is loaded.
    /// </summary>
    /// <param name="fileName">
    /// The suggested file name. When null, it is derived from the source URL or
    /// defaults to "document.pdf".
    /// </param>
    public async Task DownloadAsync(string? fileName = null)
    {
        if (jsModule is null || !jsInitialized || pageCount == 0)
        {
            return;
        }

        await jsModule.InvokeVoidAsync("download", canvas, fileName);
    }

    /// <summary>Gets the currently visible page (1-based), or 0 before a document loads.</summary>
    public async Task<int> GetCurrentPageAsync()
    {
        if (jsModule is not null && jsInitialized)
        {
            return await jsModule.InvokeAsync<int>("getCurrentPage", canvas);
        }
        return currentPage;
    }

    /// <summary>Gets the number of pages in the loaded document, or 0 before it loads.</summary>
    public async Task<int> GetPageCountAsync()
    {
        if (jsModule is not null && jsInitialized)
        {
            return await jsModule.InvokeAsync<int>("getPageCount", canvas);
        }
        return pageCount;
    }

    /// <summary>Gets the current zoom scale.</summary>
    public async Task<double> GetScaleAsync()
    {
        if (jsModule is not null && jsInitialized)
        {
            return await jsModule.InvokeAsync<double>("getScale", canvas);
        }
        return scale;
    }

    // === Render Helpers ===

    private int PageCount => pageCount;
    private int CurrentPage => currentPage;
    private double CurrentScale => scale;
    private bool IsDocumentReady => pageCount > 0;

    private bool CanGoPrevious => IsDocumentReady && currentPage > 1;
    private bool CanGoNext => IsDocumentReady && currentPage < pageCount;
    private bool CanZoomOut => IsDocumentReady && scale > MinScale + ScaleEpsilon;
    private bool CanZoomIn => IsDocumentReady && scale < MaxScale - ScaleEpsilon;

    private string EffectiveAriaLabel => AriaLabel ?? Localizer["PdfViewer.AriaLabel"];

    /// <summary>
    /// Determines whether the component needs to re-render, comparing the current
    /// parameters and viewer state against what was last rendered. A document reload
    /// is driven separately by the Url comparison in <see cref="OnAfterRenderAsync"/>
    /// so ShouldRender can stay purely presentational.
    /// </summary>
    protected override bool ShouldRender()
    {
        var changed = lastIsLoading != isLoading
            || lastLoadError != loadError
            || lastPageCount != pageCount
            || lastCurrentPage != currentPage
            || Math.Abs(lastScale - scale) > ScaleEpsilon
            || lastPageInput != pageInput
            || lastUrl != Url
            || lastId != Id
            || lastToolbar != Toolbar
            || lastClass != Class
            || lastAriaLabel != AriaLabel
            || lastHeight != Height;

        if (changed)
        {
            lastIsLoading = isLoading;
            lastLoadError = loadError;
            lastPageCount = pageCount;
            lastCurrentPage = currentPage;
            lastScale = scale;
            lastPageInput = pageInput;
            lastUrl = Url;
            lastId = Id;
            lastToolbar = Toolbar;
            lastClass = Class;
            lastAriaLabel = AriaLabel;
            lastHeight = Height;
        }

        return changed;
    }

    // === CSS Classes ===

    private string ContainerCssClass => ClassNames.cn(
        "bb:flex bb:flex-col bb:rounded-md bb:border bb:border-input bb:bg-background",
        "bb:overflow-hidden",
        Class
    );

    private static string ToolbarCssClass => ClassNames.cn(
        "bb:flex bb:flex-wrap bb:items-center bb:gap-1 bb:px-3 bb:py-2 bb:border-b bb:border-input bb:bg-muted/40"
    );

    private static string ViewportCssClass => ClassNames.cn(
        "bb:relative bb:flex bb:flex-1 bb:items-start bb:justify-center bb:overflow-auto bb:bg-muted/30"
    );

    private static string CanvasCssClass => ClassNames.cn(
        "bb:block bb:mx-auto bb:bg-background bb:shadow-md"
    );

    private string ViewportStyle =>
        string.IsNullOrEmpty(Height) ? "" : $"height: {Height}";

    /// <summary>
    /// Viewer state returned from pdfjs-interop.js. Field names map to the
    /// camelCase properties of the object the interop layer returns.
    /// </summary>
    private sealed class PdfViewerState
    {
        public bool Ok { get; set; }
        public int PageCount { get; set; }
        public int CurrentPage { get; set; }
        public double Scale { get; set; }
        public string? Error { get; set; }
    }

    // === Dispose ===

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (jsModule is null || !jsInitialized)
        {
            return;
        }

        try
        {
            await jsModule.InvokeVoidAsync("dispose", canvas);
            await jsModule.DisposeAsync();
        }
        catch (Exception ex) when (
            ex is JSDisconnectedException
            or JSException
            or TaskCanceledException
            or ObjectDisposedException
            or InvalidOperationException)
        {
            // Expected during circuit disconnect/prerendering/disposal.
        }
    }
}
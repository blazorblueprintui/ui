using System.Globalization;
using System.IO;
using BlazorBlueprint.Primitives.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
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
    private Task? initTask;
    private string? lastKnownUrl;

    // Every load, from Url or LoadDataAsync, takes the next version and its own cancellation
    // source. A newer load or disposal cancels the older call and bumps the version, so a result
    // that still arrives late is recognised as stale and never replaces the newer document.
    private int loadVersion;
    private CancellationTokenSource? loadCts;

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
    private double lastMinScale;
    private double lastMaxScale;
    private double lastDefaultScale;
    private Dictionary<string, object>? lastAdditionalAttributes;

    [Inject]
    private ILogger<BbPdfViewer> Logger { get; set; } = null!;

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
            await EnsureModuleAsync();
        }

        // Reload whenever the Url parameter has changed since the current document
        // was loaded. ShouldRender gates renders, so this only runs on real changes.
        if (jsModule is not null && !string.Equals(Url, lastKnownUrl, StringComparison.Ordinal))
        {
            await LoadPdfAsync();
        }
    }

    /// <summary>
    /// Imports the interop module on first use and waits for it. A public method called from the
    /// consumer's own <c>OnAfterRenderAsync(firstRender)</c> can run before the viewer's import has
    /// finished; it waits here instead of silently doing nothing. A failed import is tried again.
    /// </summary>
    private async Task<bool> EnsureModuleAsync()
    {
        if (initTask is null || (initTask.IsCompleted && jsModule is null))
        {
            initTask = InitializeJsAsync();
        }

        await initTask;
        return jsModule is not null;
    }

    private async Task InitializeJsAsync()
    {
        try
        {
            jsModule = await JsModules.GetAsync(
                JS,
                "./_content/BlazorBlueprint.Components/js/pdfjs-interop.js");
        }
        catch (Exception ex)
        {
            PdfViewerLog.ModuleImportFailed(Logger, ex);
        }
    }

    private async Task LoadPdfAsync()
    {
        if (jsModule is null)
        {
            return;
        }

        lastKnownUrl = Url;
        var (version, cancellationToken) = BeginLoad();

        PdfViewerState? state;
        try
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                await jsModule.InvokeVoidAsync("clear", cancellationToken, canvas);
                if (version == loadVersion)
                {
                    isLoading = false;
                    StateHasChanged();
                }
                return;
            }

            // The explicit token also replaces Blazor Server's 60-second interop timeout: a large
            // document on a slow link is still loading, not failed.
            state = await jsModule.InvokeAsync<PdfViewerState>(
                "load", cancellationToken, canvas, Url, BuildOptions());
        }
        catch (Exception ex) when (IsInteropFailure(ex))
        {
            FailLoad(version, ex.Message);
            return;
        }

        await CompleteLoadAsync(version, state);
    }

    /// <summary>
    /// Starts a load: supersedes the one in flight and resets the visible state.
    /// </summary>
    private (int Version, CancellationToken CancellationToken) BeginLoad()
    {
        loadCts?.Cancel();
        loadCts?.Dispose();
        loadCts = new CancellationTokenSource();

        isLoading = true;
        loadError = null;
        pageCount = 0;
        currentPage = 0;
        scale = ClampScale(DefaultScale);
        pageInput = "1";
        StateHasChanged();

        return (++loadVersion, loadCts.Token);
    }

    private void FailLoad(int version, string reason)
    {
        if (version != loadVersion)
        {
            return;
        }

        PdfViewerLog.LoadFailed(Logger, reason);
        isLoading = false;
        loadError = Localizer["PdfViewer.LoadFailed"];
        StateHasChanged();
    }

    /// <summary>
    /// Applies a load's result unless a newer load or disposal has overtaken it. Raising
    /// <see cref="OnDocumentLoaded"/> happens outside the interop error handling, so an exception
    /// from the consumer's own handler reaches them rather than showing as a failed load.
    /// </summary>
    private async Task CompleteLoadAsync(int version, PdfViewerState? state)
    {
        if (version != loadVersion || state is { Superseded: true })
        {
            return;
        }

        ApplyLoadResult(state);

        if (state is { Ok: true })
        {
            await OnDocumentLoaded.InvokeAsync(state.PageCount);
        }
    }

    private static bool IsInteropFailure(Exception ex) =>
        ex is JSDisconnectedException
            or JSException
            or OperationCanceledException
            or ObjectDisposedException
            or InvalidOperationException;

    private object BuildOptions() => new
    {
        initialScale = ClampScale(DefaultScale),
        minScale = Math.Max(MinScale, 0.1),
        maxScale = Math.Max(MaxScale, Math.Max(MinScale, 0.1))
    };

    // === State Helpers ===

    private void ApplyLoadResult(PdfViewerState? state)
    {
        isLoading = false;

        if (state is null || !state.Ok)
        {
            // Users see the localized message. What PDF.js said can be raw English, and can carry
            // the whole URL, signed query string included, so it goes to the log instead.
            if (!string.IsNullOrEmpty(state?.Error))
            {
                PdfViewerLog.LoadFailed(Logger, state.Error);
            }

            loadError = Localizer["PdfViewer.LoadFailed"];
            pageCount = 0;
            currentPage = 0;
            StateHasChanged();
            return;
        }

        loadError = null;
        ApplyViewState(state);
    }

    private void ApplyViewState(PdfViewerState state)
    {
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

    /// <summary>
    /// Runs a page, zoom or fit action in JavaScript. A failure is logged rather than thrown: these
    /// run from toolbar clicks, and an exception escaping an event handler ends a Blazor Server
    /// circuit. A result from before a newer load, or with no document open, is not applied.
    /// </summary>
    private async Task<PdfViewerState?> InvokeActionAsync(string jsMethod, params object?[] arguments)
    {
        if (!await EnsureModuleAsync())
        {
            return null;
        }

        var version = loadVersion;
        PdfViewerState? state;
        try
        {
            state = await jsModule!.InvokeAsync<PdfViewerState>(jsMethod, [canvas, .. arguments]);
        }
        catch (Exception ex) when (IsInteropFailure(ex))
        {
            PdfViewerLog.ActionFailed(Logger, jsMethod, ex.Message);
            return null;
        }

        if (!string.IsNullOrEmpty(state?.Error))
        {
            PdfViewerLog.ActionFailed(Logger, jsMethod, state.Error);
        }

        if (version != loadVersion || state is not { Ok: true })
        {
            return null;
        }

        ApplyViewState(state);
        return state;
    }

    private async Task NavigatePageAsync(string jsMethod, params object?[] arguments)
    {
        var before = currentPage;
        if (await InvokeActionAsync(jsMethod, arguments) is null)
        {
            return;
        }

        if (currentPage > 0 && currentPage != before)
        {
            await OnPageChanged.InvokeAsync(new PdfViewerPageChangeEventArgs
            {
                Page = currentPage,
                PageCount = pageCount
            });
        }
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
    public Task GoToPageAsync(int page) =>
        pageCount == 0 ? Task.CompletedTask : NavigatePageAsync("gotoPage", page);

    /// <summary>Zooms in by one step.</summary>
    public Task ZoomInAsync() => InvokeActionAsync("zoomIn");

    /// <summary>Zooms out by one step.</summary>
    public Task ZoomOutAsync() => InvokeActionAsync("zoomOut");

    /// <summary>
    /// Sets the zoom scale, clamped to <see cref="MinScale"/> and <see cref="MaxScale"/>.
    /// </summary>
    public Task SetScaleAsync(double scale) => InvokeActionAsync("setScale", scale);

    /// <summary>Fits the current page to the width of its scroll area.</summary>
    public Task FitToWidthAsync() => InvokeActionAsync("fitToWidth");

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

        if (!await EnsureModuleAsync())
        {
            return;
        }

        // A programmatic load replaces the current document; keep the Url marker
        // in sync so an unchanged Url parameter does not reload it afterwards.
        lastKnownUrl = Url;
        var (version, cancellationToken) = BeginLoad();

        PdfViewerState? state;
        try
        {
            using var streamReference = new DotNetStreamReference(data);
            state = await jsModule!.InvokeAsync<PdfViewerState>(
                "loadData", cancellationToken, canvas, streamReference, BuildOptions());
        }
        catch (Exception ex) when (IsInteropFailure(ex))
        {
            FailLoad(version, ex.Message);
            return;
        }

        await CompleteLoadAsync(version, state);
    }

    /// <summary>
    /// Saves the open document as a PDF file, from the bytes the viewer already
    /// holds, so a URL-loaded document is not requested again and a cross-origin
    /// or expiring URL downloads the same as local bytes. No-op before a document
    /// is loaded.
    /// </summary>
    /// <param name="fileName">
    /// The suggested file name. When null, it is derived from the source URL or
    /// defaults to "document.pdf".
    /// </param>
    public async Task DownloadAsync(string? fileName = null)
    {
        if (pageCount == 0 || !await EnsureModuleAsync())
        {
            return;
        }

        try
        {
            var failure = await jsModule!.InvokeAsync<string?>("download", canvas, fileName);
            if (failure is not null)
            {
                PdfViewerLog.ActionFailed(Logger, "download", failure);
            }
        }
        catch (Exception ex) when (IsInteropFailure(ex))
        {
            PdfViewerLog.ActionFailed(Logger, "download", ex.Message);
        }
    }

    /// <summary>Gets the currently visible page (1-based), or 0 before a document loads.</summary>
    public Task<int> GetCurrentPageAsync() => QueryAsync("getCurrentPage", currentPage);

    /// <summary>Gets the number of pages in the loaded document, or 0 before it loads.</summary>
    public Task<int> GetPageCountAsync() => QueryAsync("getPageCount", pageCount);

    /// <summary>Gets the current zoom scale.</summary>
    public Task<double> GetScaleAsync() => QueryAsync("getScale", scale);

    private async Task<T> QueryAsync<T>(string jsMethod, T fallback)
    {
        if (!await EnsureModuleAsync())
        {
            return fallback;
        }

        try
        {
            return await jsModule!.InvokeAsync<T>(jsMethod, canvas);
        }
        catch (Exception ex) when (IsInteropFailure(ex))
        {
            return fallback;
        }
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
            || lastHeight != Height
            || !lastMinScale.Equals(MinScale)
            || !lastMaxScale.Equals(MaxScale)
            || !lastDefaultScale.Equals(DefaultScale)
            || !SameAttributes(lastAdditionalAttributes, AdditionalAttributes);

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
            lastMinScale = MinScale;
            lastMaxScale = MaxScale;
            lastDefaultScale = DefaultScale;
            lastAdditionalAttributes = AdditionalAttributes is null ? null : new(AdditionalAttributes);
        }

        return changed;
    }

    // Compared by content, not reference: a parent re-render hands over a new dictionary every time,
    // and that alone must not redraw the viewer.
    private static bool SameAttributes(Dictionary<string, object>? before, Dictionary<string, object>? now)
    {
        if (before is null || now is null)
        {
            return before is null && now is null;
        }

        return before.Count == now.Count
            && now.All(pair => before.TryGetValue(pair.Key, out var value) && Equals(value, pair.Value));
    }

    // === CSS Classes ===

    private string ContainerCssClass => ClassNames.cn(
        "bb:flex bb:flex-col bb:rounded-md bb:border bb:border-input bb:bg-background",
        "bb:overflow-hidden",
        // With no Height the viewer fills its parent, as documented; the viewport takes what the
        // toolbar leaves.
        string.IsNullOrEmpty(Height) ? "bb:h-full" : null,
        Class
    );

    private static string ToolbarCssClass => ClassNames.cn(
        "bb:flex bb:flex-wrap bb:items-center bb:gap-1 bb:px-3 bb:py-2 bb:border-b bb:border-input bb:bg-muted/40"
    );

    private static string ViewportCssClass => ClassNames.cn(
        "bb:relative bb:flex bb:min-h-0 bb:flex-1 bb:items-start bb:justify-center bb:overflow-auto bb:bg-muted/30"
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

        /// <summary>True when a newer load or disposal overtook this one in JavaScript.</summary>
        public bool Superseded { get; set; }
    }

    // === Dispose ===

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        // A load still in flight must not apply its result to a disposed viewer.
        loadVersion++;
        loadCts?.Cancel();
        loadCts?.Dispose();
        loadCts = null;

        if (jsModule is null)
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
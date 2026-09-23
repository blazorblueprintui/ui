using System.Globalization;
using System.IO;
using System.Reflection;
using BlazorBlueprint.Components;
using BlazorBlueprint.Tests.Performance;
using BlazorBlueprint.Tests.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Xunit;

namespace BlazorBlueprint.Tests.PdfViewer;

/// <summary>
/// The viewer's state lives in JavaScript: PDF.js renders into a canvas and the interop module is
/// the source of truth for page count, current page and zoom. These tests substitute a recording
/// <see cref="IJSRuntime"/> that returns a fabricated <c>PdfViewerState</c>, so the component's
/// lifecycle — when it loads and reloads a document, what it renders from the returned state, and
/// which events it raises — is exercised end to end without a browser.
/// </summary>
public class PdfViewerTests
{
    [Fact]
    public async Task WithoutADocumentTheToolbarRendersDisabled()
    {
        await RunAsync(async (renderer, viewer, js) =>
        {
            var markup = renderer.Markup();

            Assert.Contains("data-slot=\"pdf-viewer\"", markup, StringComparison.Ordinal);
            Assert.Contains("data-slot=\"toolbar\"", markup, StringComparison.Ordinal);
            Assert.Contains("<canvas", markup, StringComparison.Ordinal);

            // Page input shows "1" with no total; zoom shows the default 125%.
            Assert.Contains("value=\"1\"", markup, StringComparison.Ordinal);
            Assert.Contains("–", markup, StringComparison.Ordinal);
            Assert.Contains("125%", markup, StringComparison.Ordinal);

            // Every toolbar button carries aria-disabled="true"; the page input is tied to
            // pageCount == 0 so it is disabled too.
            Assert.Equal(6, CountOf(markup, "aria-disabled=\"true\""));
            Assert.Equal(0, ComponentProbe.Field<int>(viewer, "pageCount"));

            // Nothing was asked of the interop layer beyond importing the module.
            Assert.DoesNotContain(js.Calls, call => call.Identifier is "load" or "clear");
        });
    }

    [Fact]
    public async Task ToolbarFalseOmitsTheToolbarButKeepsTheCanvas()
    {
        await RunAsync(async (renderer, viewer, js) =>
        {
            var markup = renderer.Markup();

            Assert.DoesNotContain("data-slot=\"toolbar\"", markup, StringComparison.Ordinal);
            Assert.Contains("data-slot=\"pdf-viewer\"", markup, StringComparison.Ordinal);
            Assert.Contains("<canvas", markup, StringComparison.Ordinal);
        }, parameters => parameters[nameof(BbPdfViewer.Toolbar)] = false);
    }

    [Fact]
    public async Task RootParametersAndSplattedAttributesRenderOnTheContainer()
    {
        await RunAsync(async (renderer, viewer, js) =>
        {
            var markup = renderer.Markup();

            Assert.Contains("id=\"my-viewer\"", markup, StringComparison.Ordinal);
            Assert.Contains("data-testid=\"extra\"", markup, StringComparison.Ordinal);
            Assert.Contains("aria-label=\"My document\"", markup, StringComparison.Ordinal);
            Assert.Contains("my-custom-class", markup, StringComparison.Ordinal);
        }, parameters =>
        {
            parameters[nameof(BbPdfViewer.Id)] = "my-viewer";
            parameters[nameof(BbPdfViewer.AriaLabel)] = "My document";
            parameters[nameof(BbPdfViewer.Class)] = "my-custom-class";
            parameters["data-testid"] = "extra";
        });
    }

    [Fact]
    public async Task UrlChangeReloadsOnlyWhenTheSourceActuallyChanges()
    {
        await RunAsync(async (renderer, viewer, js) =>
        {
            // Mounted without a Url: only the module import has happened.
            Assert.DoesNotContain(js.Calls, call => call.Identifier is "load" or "clear");

            await SetUrlAsync(viewer, "a.pdf");
            Assert.Equal(1, js.Calls.Count(call => call.Identifier == "load"));
            Assert.Equal("a.pdf", js.Calls.Single(call => call.Identifier == "load").Args![1]);

            // Re-setting the same Url is not a reload.
            await SetUrlAsync(viewer, "a.pdf");
            Assert.Equal(1, js.Calls.Count(call => call.Identifier == "load"));

            // A genuinely different Url loads the new document.
            await SetUrlAsync(viewer, "b.pdf");
            Assert.Equal(2, js.Calls.Count(call => call.Identifier == "load"));
            Assert.Equal("b.pdf", js.Calls.Last(call => call.Identifier == "load").Args![1]);

            // Clearing the Url clears the canvas instead of loading anything.
            await SetUrlAsync(viewer, null);
            Assert.Equal(1, js.Calls.Count(call => call.Identifier == "clear"));
            Assert.Equal(2, js.Calls.Count(call => call.Identifier == "load"));

            // A null Url rendered again stays cleared.
            await SetUrlAsync(viewer, null);
            Assert.Equal(1, js.Calls.Count(call => call.Identifier == "clear"));
        }, setupJs: js => js.Results["load"] = _ => State(ok: true, pageCount: 2, currentPage: 1, scale: 1.25));
    }

    [Fact]
    public async Task SuccessfulLoadRendersTheDocumentAndRaisesOnDocumentLoaded()
    {
        var loaded = 0;
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                Assert.Equal(1, js.Calls.Count(call => call.Identifier == "load"));
                Assert.Equal(14, loaded);

                var markup = renderer.Markup();
                Assert.Contains("value=\"3\"", markup, StringComparison.Ordinal);
                Assert.Contains(">14</span>", markup, StringComparison.Ordinal);
                Assert.Contains("75%", markup, StringComparison.Ordinal);

                // 3 of 14 is neither first, last nor at a zoom bound: nothing may be disabled.
                Assert.Equal(0, CountOf(markup, "aria-disabled=\"true\""));
            },
            parameters => parameters[nameof(BbPdfViewer.Url)] = "a.pdf",
            setupJs: js => js.Results["load"] = _ => State(ok: true, pageCount: 14, currentPage: 3, scale: 0.75),
            documentLoaded: value => loaded = value);
    }

    [Fact]
    public async Task LoadOptionsCarryTheZoomBounds()
    {
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                var options = js.Calls.Single(call => call.Identifier == "load").Args![2]!;
                Assert.Equal("a.pdf", js.Calls.Single(call => call.Identifier == "load").Args![1]);
                Assert.Equal(2.0, Property(options, "initialScale"));
                Assert.Equal(1.0, Property(options, "minScale"));
                Assert.Equal(4.0, Property(options, "maxScale"));
            },
            parameters =>
            {
                parameters[nameof(BbPdfViewer.Url)] = "a.pdf";
                parameters[nameof(BbPdfViewer.DefaultScale)] = 2.0;
                parameters[nameof(BbPdfViewer.MinScale)] = 1.0;
                parameters[nameof(BbPdfViewer.MaxScale)] = 4.0;
            },
            setupJs: js => js.Results["load"] = _ => State(ok: true, pageCount: 2, currentPage: 1, scale: 2.0));
    }

    [Fact]
    public async Task NavigationRaisesOnPageChangedOnlyWhenThePageMoves()
    {
        var changed = new List<PdfViewerPageChangeEventArgs>();
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                Assert.Empty(changed);

                await viewer.GoToPageAsync(5);

                var args = Assert.Single(changed);
                Assert.Equal(5, args.Page);
                Assert.Equal(14, args.PageCount);
                Assert.Contains("value=\"5\"", renderer.Markup(), StringComparison.Ordinal);

                // Asking for the page that is already visible raises nothing.
                changed.Clear();
                await viewer.GoToPageAsync(5);
                Assert.Empty(changed);

                // Next/previous use the same event.
                await viewer.NextPageAsync();
                Assert.Equal(6, Assert.Single(changed).Page);
            },
            parameters =>
            {
                parameters[nameof(BbPdfViewer.Url)] = "a.pdf";
                parameters[nameof(BbPdfViewer.OnPageChanged)] =
                    EventCallback.Factory.Create<PdfViewerPageChangeEventArgs>(this, changed.Add);
            },
            setupJs: js =>
            {
                js.Results["load"] = _ => State(ok: true, pageCount: 14, currentPage: 3, scale: 0.75);
                js.Results["gotoPage"] = _ => State(ok: true, pageCount: 14, currentPage: 5, scale: 0.75);
                js.Results["nextPage"] = _ => State(ok: true, pageCount: 14, currentPage: 6, scale: 0.75);
            });
    }

    [Fact]
    public async Task FailedLoadShowsTheLocalizedErrorAndLeavesControlsDisabled()
    {
        await RunAsync(async (renderer, viewer, js) =>
        {
            // The stub returns null by default, so the load has no state to report.
            Assert.Contains("Unable to load the PDF document.", renderer.Markup(), StringComparison.Ordinal);
            // No document means every toolbar action, download included, is disabled.
            Assert.Equal(6, CountOf(renderer.Markup(), "aria-disabled=\"true\""));
            Assert.Equal(0, ComponentProbe.Field<int>(viewer, "pageCount"));
        }, parameters => parameters[nameof(BbPdfViewer.Url)] = "missing.pdf");
    }

    [Fact]
    public async Task LoadDataAsyncStreamsBytesToTheInteropModuleAndRaisesOnDocumentLoaded()
    {
        var loaded = 0;
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                await viewer.LoadDataAsync(new byte[] { 0x25, 0x50, 0x44, 0x46 });

                var call = Assert.Single(js.Calls, call => call.Identifier == "loadData");
                Assert.IsType<DotNetStreamReference>(call.Args![1]);
                Assert.Equal(1.25, Property(call.Args![2]!, "initialScale"));
                Assert.Equal(14, loaded);

                var markup = renderer.Markup();
                Assert.Contains("value=\"3\"", markup, StringComparison.Ordinal);
                Assert.Contains(">14</span>", markup, StringComparison.Ordinal);
                Assert.Equal(0, CountOf(markup, "aria-disabled=\"true\""));
            },
            setupJs: js => js.Results["loadData"] = _ => State(ok: true, pageCount: 14, currentPage: 3, scale: 1.25),
            documentLoaded: value => loaded = value);
    }

    [Fact]
    public async Task LoadDataAsyncAcceptsACallerOwnedStream()
    {
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                using var stream = new MemoryStream(new byte[] { 0x25, 0x50, 0x44, 0x46 });
                await viewer.LoadDataAsync(stream);

                // The stream is handed to the interop layer as a DotNetStreamReference.
                Assert.Single(js.Calls, call => call.Identifier == "loadData");
                Assert.Contains(">1</span>", renderer.Markup(), StringComparison.Ordinal);
            },
            setupJs: js => js.Results["loadData"] = _ => State(ok: true, pageCount: 1, currentPage: 1, scale: 1.0));
    }

    [Fact]
    public async Task DownloadButtonSavesTheOpenDocument()
    {
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                // Six toolbar buttons carry onclick handlers; download is the sixth.
                await renderer.DispatchAsync("onclick", new MouseEventArgs(), occurrence: 5);

                Assert.Equal(1, js.Calls.Count(call => call.Identifier == "download"));
            },
            parameters => parameters[nameof(BbPdfViewer.Url)] = "a.pdf",
            setupJs: js => js.Results["load"] = _ => State(ok: true, pageCount: 14, currentPage: 3, scale: 0.75));
    }

    [Fact]
    public async Task DownloadAsyncNoOpsWithoutADocument()
    {
        await RunAsync(async (renderer, viewer, js) =>
        {
            await viewer.DownloadAsync();
            Assert.DoesNotContain(js.Calls, call => call.Identifier == "download");
        });
    }

    [Fact]
    public async Task DisposalReleasesTheLoadedDocument()
    {
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                await ((IAsyncDisposable)viewer).DisposeAsync();
                Assert.Contains(js.Calls, call => call.Identifier == "dispose");
            },
            parameters => parameters[nameof(BbPdfViewer.Url)] = "a.pdf",
            setupJs: js => js.Results["load"] = _ => State(ok: true, pageCount: 1, currentPage: 1, scale: 1.0));
    }

    // ---------------------------------------------------------------------------------------

    private static async Task RunAsync(
        Func<ComponentTestRenderer, BbPdfViewer, RecordingJsRuntime, Task> body,
        Action<Dictionary<string, object?>>? parameters = null,
        Action<RecordingJsRuntime>? setupJs = null,
        Action<int>? documentLoaded = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        var js = new RecordingJsRuntime();
        services.AddSingleton<IJSRuntime>(js);
        services.AddBlazorBlueprintComponents();

        var viewerParameters = new Dictionary<string, object?> { [nameof(BbPdfViewer.Url)] = null };
        parameters?.Invoke(viewerParameters);
        if (documentLoaded is not null)
        {
            viewerParameters[nameof(BbPdfViewer.OnDocumentLoaded)] =
                EventCallback.Factory.Create<int>(js, documentLoaded);
        }

        setupJs?.Invoke(js);

        await using var provider = services.BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);

        var viewer = await renderer.Dispatcher.InvokeAsync(
            () => renderer.MountAsync<BbPdfViewer>(viewerParameters));

        await renderer.Dispatcher.InvokeAsync(() => body(renderer, viewer, js));
    }

    private static Task SetUrlAsync(BbPdfViewer viewer, string? url) =>
        viewer.SetParametersAsync(ParameterView.FromDictionary(
            new Dictionary<string, object?> { [nameof(BbPdfViewer.Url)] = url }));

    private static int CountOf(string markup, string value)
    {
        var count = 0;
        var at = 0;
        while ((at = markup.IndexOf(value, at, StringComparison.Ordinal)) >= 0)
        {
            count++;
            at += value.Length;
        }

        return count;
    }

    private static double Property(object target, string name) =>
        Convert.ToDouble(
            target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public)!.GetValue(target),
            CultureInfo.InvariantCulture);

    /// <summary>
    /// Builds an instance of the viewer's private <c>PdfViewerState</c> DTO, which is what the
    /// real interop layer deserialises from the JSON object pdfjs-interop.js returns.
    /// </summary>
    private static object State(bool ok, int pageCount, int currentPage, double scale)
    {
        var stateType = typeof(BbPdfViewer).GetNestedType(
            "PdfViewerState", BindingFlags.NonPublic) ??
            throw new InvalidOperationException("PdfViewerState nested type not found.");

        var state = Activator.CreateInstance(stateType)!;
        stateType.GetProperty("Ok")!.SetValue(state, ok);
        stateType.GetProperty("PageCount")!.SetValue(state, pageCount);
        stateType.GetProperty("CurrentPage")!.SetValue(state, currentPage);
        stateType.GetProperty("Scale")!.SetValue(state, scale);
        return state;
    }

    /// <summary>
    /// A JavaScript runtime that records every call and answers each interop identifier with a
    /// configured result, so tests can both verify what the component asked for and feed it the
    /// state it would get back from pdfjs-interop.js. Like <see cref="NoopJavaScript"/>, it stands
    /// in for the module reference itself when an import is requested.
    /// </summary>
    private sealed class RecordingJsRuntime : IJSRuntime, IJSObjectReference
    {
        public List<(string Identifier, object?[]? Args)> Calls { get; } = [];
        public Dictionary<string, Func<object?[]?, object?>> Results { get; } = new();

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Calls.Add((identifier, args));

            if (typeof(TValue) == typeof(IJSObjectReference))
            {
                return ValueTask.FromResult((TValue)(object)this);
            }

            if (Results.TryGetValue(identifier, out var factory) && factory(args) is { } result)
            {
                return ValueTask.FromResult((TValue)result);
            }

            return ValueTask.FromResult<TValue>(default!);
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
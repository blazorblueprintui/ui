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

    [Fact]
    public async Task ALoadOvertakenByANewerUrlNeverReplacesIt()
    {
        var slow = new TaskCompletionSource<object?>();
        var loaded = new List<int>();
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                await SetUrlAsync(viewer, "slow.pdf");
                await SetUrlAsync(viewer, "fast.pdf");
                Assert.Equal([1], loaded);

                // The slow document finishes after the fast one has been shown.
                slow.SetResult(State(ok: true, pageCount: 14, currentPage: 1, scale: 1.25));
                await Task.Delay(50);

                Assert.Equal(1, ComponentProbe.Field<int>(viewer, "pageCount"));
                Assert.Equal([1], loaded);
                Assert.Contains(">1</span>", renderer.Markup(), StringComparison.Ordinal);

                // The overtaken call was cancelled, so it no longer counts against the interop timeout.
                Assert.True(js.Calls.First(call => call.Identifier == "load").Token.IsCancellationRequested);
            },
            setupJs: js => js.Results["load"] = args => (string?)args![1] == "slow.pdf"
                ? slow.Task
                : State(ok: true, pageCount: 1, currentPage: 1, scale: 1.25),
            documentLoaded: loaded.Add);
    }

    [Fact]
    public async Task LoadDataAsyncOvertakesAUrlLoadInFlight()
    {
        var slow = new TaskCompletionSource<object?>();
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                await SetUrlAsync(viewer, "slow.pdf");
                await viewer.LoadDataAsync(new byte[] { 0x25, 0x50, 0x44, 0x46 });

                slow.SetResult(State(ok: true, pageCount: 14, currentPage: 1, scale: 1.25));
                await Task.Delay(50);

                Assert.Equal(2, ComponentProbe.Field<int>(viewer, "pageCount"));
            },
            setupJs: js =>
            {
                js.Results["load"] = _ => slow.Task;
                js.Results["loadData"] = _ => State(ok: true, pageCount: 2, currentPage: 1, scale: 1.25);
            });
    }

    [Fact]
    public async Task DisposalStopsALoadInFlightFromReportingIn()
    {
        var slow = new TaskCompletionSource<object?>();
        var loaded = new List<int>();
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                await SetUrlAsync(viewer, "slow.pdf");
                await ((IAsyncDisposable)viewer).DisposeAsync();

                slow.SetResult(State(ok: true, pageCount: 14, currentPage: 1, scale: 1.25));
                await Task.Delay(50);

                Assert.Empty(loaded);
                Assert.True(js.Calls.First(call => call.Identifier == "load").Token.IsCancellationRequested);
            },
            setupJs: js => js.Results["load"] = _ => slow.Task,
            documentLoaded: loaded.Add);
    }

    [Fact]
    public async Task AnExceptionFromTheConsumersLoadedHandlerIsNotReportedAsAFailedLoad()
    {
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => viewer.LoadDataAsync(new byte[] { 0x25, 0x50, 0x44, 0x46 }));

                Assert.DoesNotContain("Unable to load the PDF document.", renderer.Markup(), StringComparison.Ordinal);
            },
            setupJs: js => js.Results["loadData"] = _ => State(ok: true, pageCount: 2, currentPage: 1, scale: 1.25),
            documentLoaded: _ => throw new InvalidOperationException("The consumer's handler failed."));
    }

    [Fact]
    public async Task LoadDataAsyncWaitsForTheModuleImportInsteadOfDoingNothing()
    {
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                // What a consumer's own OnAfterRenderAsync(firstRender) sees: the viewer has
                // rendered, but its module import has not come back yet.
                var load = viewer.LoadDataAsync(new byte[] { 0x25, 0x50, 0x44, 0x46 });
                Assert.DoesNotContain(js.Calls, call => call.Identifier == "loadData");

                js.ImportGate!.SetResult();
                await load;

                Assert.Single(js.Calls, call => call.Identifier == "loadData");
                Assert.Equal(3, ComponentProbe.Field<int>(viewer, "pageCount"));
            },
            setupJs: js =>
            {
                js.ImportGate = new TaskCompletionSource();
                js.Results["loadData"] = _ => State(ok: true, pageCount: 3, currentPage: 1, scale: 1.25);
            });
    }

    [Fact]
    public async Task AFailedLoadShowsTheLocalizedMessageNotWhatPdfJsSaid()
    {
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                var markup = renderer.Markup();
                Assert.Contains("Unable to load the PDF document.", markup, StringComparison.Ordinal);
                Assert.DoesNotContain("sig=secret", markup, StringComparison.Ordinal);
            },
            parameters => parameters[nameof(BbPdfViewer.Url)] = "a.pdf",
            setupJs: js => js.Results["load"] = _ => State(
                ok: false, pageCount: 0, currentPage: 0, scale: 1.25,
                error: "Unexpected server response (404) while retrieving PDF \"https://files.example/a.pdf?sig=secret\"."));
    }

    [Fact]
    public async Task AToolbarActionThatFailsInJavaScriptDoesNotThrow()
    {
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                // On Blazor Server an exception escaping a click handler ends the circuit.
                await viewer.NextPageAsync();
                await renderer.DispatchAsync("onclick", new MouseEventArgs(), occurrence: 1);

                Assert.Equal(3, ComponentProbe.Field<int>(viewer, "currentPage"));
                Assert.DoesNotContain("Unable to load the PDF document.", renderer.Markup(), StringComparison.Ordinal);
            },
            parameters => parameters[nameof(BbPdfViewer.Url)] = "a.pdf",
            setupJs: js =>
            {
                js.Results["load"] = _ => State(ok: true, pageCount: 14, currentPage: 3, scale: 1.25);
                js.Results["nextPage"] = _ => throw new JSException("Worker was destroyed");
            });
    }

    [Fact]
    public async Task AnActionAnsweredWithNoDocumentLeavesTheViewerAlone()
    {
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                await viewer.ZoomInAsync();

                Assert.Equal(14, ComponentProbe.Field<int>(viewer, "pageCount"));
                Assert.DoesNotContain("Unable to load the PDF document.", renderer.Markup(), StringComparison.Ordinal);
            },
            parameters => parameters[nameof(BbPdfViewer.Url)] = "a.pdf",
            setupJs: js =>
            {
                js.Results["load"] = _ => State(ok: true, pageCount: 14, currentPage: 3, scale: 1.25);
                js.Results["zoomIn"] = _ => State(ok: false, pageCount: 0, currentPage: 0, scale: 0);
            });
    }

    [Fact]
    public async Task ChangingTheZoomBoundsRedrawsTheToolbar()
    {
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                Assert.Equal(0, CountOf(renderer.Markup(), "aria-disabled=\"true\""));

                // Zoom is already at the new maximum, so zoom-in must now render disabled.
                await viewer.SetParametersAsync(ParameterView.FromDictionary(
                    new Dictionary<string, object?>
                    {
                        [nameof(BbPdfViewer.Url)] = "a.pdf",
                        [nameof(BbPdfViewer.MaxScale)] = 1.25
                    }));

                Assert.Equal(1, CountOf(renderer.Markup(), "aria-disabled=\"true\""));
            },
            parameters => parameters[nameof(BbPdfViewer.Url)] = "a.pdf",
            setupJs: js => js.Results["load"] = _ => State(ok: true, pageCount: 14, currentPage: 3, scale: 1.25));
    }

    [Fact]
    public async Task WithNoHeightTheViewerFillsItsParent()
    {
        await RunAsync(
            async (renderer, viewer, js) =>
            {
                var markup = renderer.Markup();
                Assert.Contains("bb:h-full", markup, StringComparison.Ordinal);
                Assert.DoesNotContain("height: ", markup, StringComparison.Ordinal);
            },
            parameters => parameters[nameof(BbPdfViewer.Height)] = null);
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
    private static object State(bool ok, int pageCount, int currentPage, double scale, string? error = null)
    {
        var stateType = typeof(BbPdfViewer).GetNestedType(
            "PdfViewerState", BindingFlags.NonPublic) ??
            throw new InvalidOperationException("PdfViewerState nested type not found.");

        var state = Activator.CreateInstance(stateType)!;
        stateType.GetProperty("Ok")!.SetValue(state, ok);
        stateType.GetProperty("PageCount")!.SetValue(state, pageCount);
        stateType.GetProperty("CurrentPage")!.SetValue(state, currentPage);
        stateType.GetProperty("Scale")!.SetValue(state, scale);
        stateType.GetProperty("Error")!.SetValue(state, error);
        return state;
    }

    /// <summary>
    /// A JavaScript runtime that records every call and answers each interop identifier with a
    /// configured result, so tests can both verify what the component asked for and feed it the
    /// state it would get back from pdfjs-interop.js. Like <see cref="NoopJavaScript"/>, it stands
    /// in for the module reference itself when an import is requested. A result given as a
    /// <see cref="Task{TResult}"/> of object completes when the test says so, and ignores the call's
    /// cancellation token, the way a JavaScript promise already running finishes regardless.
    /// </summary>
    private sealed class RecordingJsRuntime : IJSRuntime, IJSObjectReference
    {
        public List<(string Identifier, object?[]? Args, CancellationToken Token)> Calls { get; } = [];
        public Dictionary<string, Func<object?[]?, object?>> Results { get; } = new();

        /// <summary>When set, the module import waits for it, like a slow circuit round trip.</summary>
        public TaskCompletionSource? ImportGate { get; set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Calls.Add((identifier, args, cancellationToken));

            if (typeof(TValue) == typeof(IJSObjectReference))
            {
                return ImportGate is { } gate
                    ? new ValueTask<TValue>(ImportAsync<TValue>(gate.Task))
                    : ValueTask.FromResult((TValue)(object)this);
            }

            if (Results.TryGetValue(identifier, out var factory) && factory(args) is { } result)
            {
                return result is Task<object?> pending
                    ? new ValueTask<TValue>(AwaitAsync<TValue>(pending))
                    : ValueTask.FromResult((TValue)result);
            }

            return ValueTask.FromResult<TValue>(default!);
        }

        private static async Task<TValue> AwaitAsync<TValue>(Task<object?> pending) => (TValue)(await pending)!;

        private async Task<TValue> ImportAsync<TValue>(Task gate)
        {
            await gate;
            return (TValue)(object)this;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
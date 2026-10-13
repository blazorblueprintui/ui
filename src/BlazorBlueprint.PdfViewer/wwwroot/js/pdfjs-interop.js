// Pdf.js interop for PdfViewer component
// Handles loading, page navigation, zoom, fit-to-width and lifecycle.
//
// State is kept per canvas in a WeakMap: a slot holding the document the canvas shows and the load
// in flight, if any. Every function that can change the visible state returns the same shape
// { ok, currentPage, pageCount, scale }, which .NET deserializes into PdfViewerState to keep its
// own fields in sync. None of them throws: a failure comes back in `error`, because an exception
// escaping an interop call from an event handler ends a Blazor Server circuit.

// The legacy build: the modern one calls Map.prototype.getOrInsertComputed without a polyfill,
// which only the newest browsers have.
import * as pdfjsLib from "../lib/pdfjs/pdf.min.mjs";

const workerUrl = new URL(
    "../lib/pdfjs/pdf.worker.min.mjs",
    import.meta.url
);

pdfjsLib.GlobalWorkerOptions.workerSrc = workerUrl.href;

// PDF.js decodes JPEG 2000, JBIG2 and CCITT images, and applies ICC colour profiles, only through
// these WebAssembly modules; without them scanned documents render blank or wrong.
const wasmUrl = new URL("../lib/pdfjs/wasm/", import.meta.url).href;

const SCALE_STEP = 0.25;
const FIT_WIDTH_MARGIN = 32;

// canvas -> { generation, loadingTask, viewer, overtaken }. The generation increases every time the
// canvas is given a new document or released, so a load that was overtaken can tell it is stale.
const slots = new WeakMap();

function slotFor(canvas) {
    let slot = slots.get(canvas);
    if (!slot) {
        slot = { generation: 0, loadingTask: null, viewer: null, overtaken: null };
        slots.set(canvas, slot);
    }
    return slot;
}

function currentViewer(canvas) {
    const slot = slots.get(canvas);
    return slot ? slot.viewer : null;
}

function clamp(value, min, max) {
    return Math.min(Math.max(value, min), max);
}

function defaultOptions() {
    return { initialScale: 1.25, minScale: 0.5, maxScale: 3 };
}

function normalizeOptions(options) {
    const minScale = Number(options && options.minScale) > 0 ? options.minScale : defaultOptions().minScale;
    const maxScale = Number(options && options.maxScale) > 0 ? options.maxScale : defaultOptions().maxScale;
    const initialScale = clamp(
        Number(options && options.initialScale) > 0 ? options.initialScale : defaultOptions().initialScale,
        minScale,
        maxScale
    );
    return { initialScale, minScale, maxScale };
}

function stateFor(canvas) {
    const viewer = currentViewer(canvas);
    return {
        ok: !!viewer,
        currentPage: viewer ? viewer.currentPage : 0,
        pageCount: viewer ? viewer.pageCount : 0,
        scale: viewer ? viewer.scale : 0
    };
}

function messageOf(err) {
    return err && err.message ? err.message : String(err);
}

function clearCanvas(canvas) {
    const context = canvas.getContext("2d");
    if (context) {
        context.clearRect(0, 0, canvas.width, canvas.height);
    }
    canvas.width = 0;
    canvas.height = 0;
    canvas.style.width = "";
    canvas.style.height = "";
}

async function destroyTask(loadingTask) {
    try {
        await loadingTask.destroy();
    } catch {
        // Already destroyed, or the load it belonged to failed.
    }
}

/**
 * Ends whatever the canvas is showing or loading. Bumping the generation makes any load still in
 * flight stale, and destroying its loading task terminates the worker and the document inside it.
 * PDFDocumentProxy has no destroy() in PDF.js 6: only the loading task frees them.
 * @param {HTMLCanvasElement} canvas
 * @returns {Promise<number>} The canvas's new generation.
 */
async function release(canvas) {
    const slot = slotFor(canvas);
    const generation = ++slot.generation;

    // Lets the load in flight return at once (see open).
    if (slot.overtaken) {
        slot.overtaken();
        slot.overtaken = null;
    }

    const { viewer, loadingTask } = slot;
    slot.viewer = null;
    slot.loadingTask = null;

    if (viewer) {
        try {
            viewer.renderTask && viewer.renderTask.cancel();
        } catch {
            // Ignore render cancellation errors.
        }
        viewer.renderTask = null;
        viewer.pageCache.clear();
        await destroyTask(viewer.loadingTask);
    }

    if (loadingTask) {
        await destroyTask(loadingTask);
    }

    return generation;
}

/**
 * Loads a document into the canvas and renders its first page, replacing whatever the canvas was
 * showing or loading. A load overtaken by a newer one, or by dispose, destroys its own document and
 * returns { superseded: true } instead of registering it.
 * @param {HTMLCanvasElement} canvas
 * @param {object} options
 * @param {() => Promise<object>} getSource Resolves to the getDocument source ({ url } or { data }).
 */
async function open(canvas, options, getSource) {
    const generation = await release(canvas);
    const slot = slotFor(canvas);
    const { initialScale, minScale, maxScale } = normalizeOptions(options);
    const superseded = { ok: false, superseded: true, currentPage: 0, pageCount: 0, scale: initialScale };

    if (slot.generation !== generation) {
        return superseded;
    }

    clearCanvas(canvas);

    // A loading task destroyed before its worker has answered never settles its promise, so a load
    // overtaken that early would wait forever, holding on to everything it references. The next
    // release() resolves this and the load returns at once; what it was awaiting is then ignored.
    const overtaken = new Promise(resolve => {
        slot.overtaken = resolve;
    }).then(() => superseded);

    return Promise.race([fetchAndRender(), overtaken]);

    async function fetchAndRender() {
        let loadingTask = null;
        try {
            const source = await getSource();
            if (slot.generation !== generation) {
                return superseded;
            }

            loadingTask = pdfjsLib.getDocument({ ...source, wasmUrl });
            slot.loadingTask = loadingTask;

            const pdf = await loadingTask.promise;
            if (slot.generation !== generation) {
                await destroyTask(loadingTask);
                return superseded;
            }

            const viewer = {
                loadingTask,
                pdf,
                url: source.url || null,
                pageCount: pdf.numPages,
                currentPage: 1,
                scale: initialScale,
                minScale,
                maxScale,
                pageCache: new Map(),
                renderTask: null
            };
            slot.loadingTask = null;
            slot.viewer = viewer;

            await renderPage(canvas, viewer, 1);
            if (slot.generation !== generation) {
                return superseded;
            }

            return {
                ok: true,
                currentPage: viewer.currentPage,
                pageCount: viewer.pageCount,
                scale: viewer.scale
            };
        } catch (err) {
            // A newer load or dispose destroying this one's task rejects here too; that is not an error.
            if (slot.generation !== generation) {
                return superseded;
            }

            slot.viewer = null;
            slot.loadingTask = null;
            if (loadingTask) {
                await destroyTask(loadingTask);
            }
            clearCanvas(canvas);
            return {
                ok: false,
                currentPage: 0,
                pageCount: 0,
                scale: initialScale,
                error: messageOf(err)
            };
        }
    }
}

/**
 * Loads a document from a URL.
 * @param {HTMLCanvasElement} canvas
 * @param {string} url
 * @param {{initialScale?: number, minScale?: number, maxScale?: number}} [options]
 * @returns {Promise<{ok: boolean, currentPage: number, pageCount: number, scale: number, error?: string, superseded?: boolean}>}
 */
export function load(canvas, url, options) {
    return open(canvas, options, async () => ({ url: resolveUrl(url) }));
}

// PDF.js resolves a relative URL against the page's location rather than <base href>, so a
// relative Url failed on every page below the app's root.
function resolveUrl(url) {
    try {
        return new URL(url, document.baseURI).href;
    } catch {
        return url;
    }
}

/**
 * Loads a document from an in-memory byte stream. The .NET side sends a DotNetStreamReference,
 * read here with arrayBuffer(), so PDF.js renders from the local bytes and there is no web request.
 * @param {HTMLCanvasElement} canvas
 * @param {{arrayBuffer: () => Promise<ArrayBuffer>}} streamReference
 * @param {{initialScale?: number, minScale?: number, maxScale?: number}} [options]
 * @returns {Promise<{ok: boolean, currentPage: number, pageCount: number, scale: number, error?: string, superseded?: boolean}>}
 */
export function loadData(canvas, streamReference, options) {
    return open(canvas, options, async () => ({ data: await streamReference.arrayBuffer() }));
}

async function getPage(viewer, pageNumber) {
    let page = viewer.pageCache.get(pageNumber);
    if (!page) {
        page = await viewer.pdf.getPage(pageNumber);
        viewer.pageCache.set(pageNumber, page);
    }
    return page;
}

/**
 * Renders a given page at the viewer's current scale.
 * The previous render task is cancelled first so rapid navigation cannot paint
 * a stale frame over the newest one, and resolved pages are cached so paging
 * back and forth does not re-fetch every time.
 * @param {HTMLCanvasElement} canvas
 * @param {object} viewer
 * @param {number} pageNumber
 */
async function renderPage(canvas, viewer, pageNumber) {
    const page = await getPage(viewer, pageNumber);
    if (currentViewer(canvas) !== viewer) {
        return;
    }

    if (viewer.renderTask) {
        viewer.renderTask.cancel();
        viewer.renderTask = null;
    }

    const viewport = page.getViewport({
        scale: viewer.scale
    });

    const devicePixelRatio = window.devicePixelRatio || 1;

    canvas.width = Math.floor(viewport.width * devicePixelRatio);
    canvas.height = Math.floor(viewport.height * devicePixelRatio);
    canvas.style.width = `${viewport.width}px`;
    canvas.style.height = `${viewport.height}px`;

    const context = canvas.getContext("2d");

    const transform =
        devicePixelRatio !== 1
            ? [
                devicePixelRatio,
                0,
                0,
                devicePixelRatio,
                0,
                0
            ]
            : null;

    const renderTask = page.render({
        canvasContext: context,
        viewport,
        transform
    });

    viewer.renderTask = renderTask;
    viewer.currentPage = pageNumber;

    try {
        await renderTask.promise;
    } catch (err) {
        if (err && err.name === "RenderingCancelledException") {
            return;
        }
        throw err;
    } finally {
        // Only clear our own task: a newer render may already have replaced it.
        if (viewer.renderTask === renderTask) {
            viewer.renderTask = null;
        }
    }
}

/**
 * Runs an action against the document the canvas shows and returns the resulting state. A failure
 * is returned in `error` rather than thrown. A document replaced or released mid-action rejects
 * with "Worker was destroyed"; that is not a failure of the document now showing, so it is dropped.
 * @param {HTMLCanvasElement} canvas
 * @param {(viewer: object) => Promise<void>} action
 */
async function act(canvas, action) {
    const viewer = currentViewer(canvas);
    if (!viewer) {
        return stateFor(canvas);
    }

    try {
        await action(viewer);
    } catch (err) {
        if (currentViewer(canvas) === viewer) {
            return { ...stateFor(canvas), error: messageOf(err) };
        }
    }

    return stateFor(canvas);
}

/**
 * Navigates to a page, clamped to the valid range. No-op when the target is
 * already visible. Returns the updated state.
 * @param {HTMLCanvasElement} canvas
 * @param {number} pageNumber
 */
function goToPage(canvas, pageNumber) {
    return act(canvas, async viewer => {
        const target = clamp(Math.floor(pageNumber), 1, viewer.pageCount);
        if (target !== viewer.currentPage) {
            await renderPage(canvas, viewer, target);

            // A new page starts at its top, not wherever the last one was scrolled to.
            if (canvas.parentElement) {
                canvas.parentElement.scrollTop = 0;
            }
        }
    });
}

/**
 * Moves to the next page, if any.
 * @param {HTMLCanvasElement} canvas
 */
export function nextPage(canvas) {
    const viewer = currentViewer(canvas);
    return goToPage(canvas, viewer ? viewer.currentPage + 1 : 1);
}

/**
 * Moves to the previous page, if any.
 * @param {HTMLCanvasElement} canvas
 */
export function previousPage(canvas) {
    const viewer = currentViewer(canvas);
    return goToPage(canvas, viewer ? viewer.currentPage - 1 : 1);
}

/**
 * Jumps to a specific page (1-based), clamped to the document's range.
 * @param {HTMLCanvasElement} canvas
 * @param {number} page
 */
export function gotoPage(canvas, page) {
    return goToPage(canvas, page);
}

/**
 * Sets the zoom scale for the current page.
 * @param {HTMLCanvasElement} canvas
 * @param {number} scale
 */
export function setScale(canvas, scale) {
    return act(canvas, async viewer => {
        const before = viewer.scale;
        viewer.scale = clamp(scale, viewer.minScale, viewer.maxScale);
        if (viewer.scale !== before) {
            await renderPage(canvas, viewer, viewer.currentPage);
        }
    });
}

/**
 * Zooms in one step.
 * @param {HTMLCanvasElement} canvas
 */
export function zoomIn(canvas) {
    return act(canvas, async viewer => {
        viewer.scale = clamp(viewer.scale + SCALE_STEP, viewer.minScale, viewer.maxScale);
        await renderPage(canvas, viewer, viewer.currentPage);
    });
}

/**
 * Zooms out one step.
 * @param {HTMLCanvasElement} canvas
 */
export function zoomOut(canvas) {
    return act(canvas, async viewer => {
        viewer.scale = clamp(viewer.scale - SCALE_STEP, viewer.minScale, viewer.maxScale);
        await renderPage(canvas, viewer, viewer.currentPage);
    });
}

/**
 * Fits the current page to the width of its scroll container, leaving a small
 * margin on each side. Returns the updated state.
 * @param {HTMLCanvasElement} canvas
 */
export function fitToWidth(canvas) {
    return act(canvas, async viewer => {
        const page = await getPage(viewer, viewer.currentPage);
        const baseViewport = page.getViewport({ scale: 1 });
        const container = canvas.parentElement;
        const available = (container ? container.clientWidth : 0) - FIT_WIDTH_MARGIN;
        if (available <= 0) {
            return;
        }

        viewer.scale = clamp(available / baseViewport.width, viewer.minScale, viewer.maxScale);
        await renderPage(canvas, viewer, viewer.currentPage);
    });
}

/**
 * Gets the current page number.
 * @param {HTMLCanvasElement} canvas
 * @returns {number}
 */
export function getCurrentPage(canvas) {
    return stateFor(canvas).currentPage;
}

/**
 * Gets the document's page count, or 0 when nothing is loaded.
 * @param {HTMLCanvasElement} canvas
 * @returns {number}
 */
export function getPageCount(canvas) {
    return stateFor(canvas).pageCount;
}

/**
 * Gets the current zoom scale.
 * @param {HTMLCanvasElement} canvas
 * @returns {number}
 */
export function getScale(canvas) {
    return stateFor(canvas).scale;
}

function defaultFileName(viewer) {
    if (viewer.url) {
        const path = viewer.url.split("#")[0].split("?")[0];
        const name = path.substring(path.lastIndexOf("/") + 1);
        if (name) {
            return name;
        }
    }
    return "document.pdf";
}

/**
 * Saves the open document as a PDF file, from the bytes PDF.js already holds. That works the same
 * for URL and byte-loaded documents: no second request, so nothing to fail on CORS or an expired
 * signed URL, and nothing read from the ArrayBuffer given to getDocument, which PDF.js transfers
 * to its worker and leaves empty.
 * @param {HTMLCanvasElement} canvas
 * @param {string|null} [fileName]
 * @returns {Promise<string|null>} null when the file was saved, otherwise why it was not.
 */
export async function download(canvas, fileName) {
    const viewer = currentViewer(canvas);
    if (!viewer) {
        return "No document is loaded.";
    }

    let data;
    try {
        data = await viewer.pdf.getData();
    } catch (err) {
        return messageOf(err);
    }

    const blob = new Blob([data], { type: "application/pdf" });
    const objectUrl = URL.createObjectURL(blob);

    const anchor = document.createElement("a");
    anchor.href = objectUrl;
    anchor.download = fileName || defaultFileName(viewer);
    anchor.style.display = "none";
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    setTimeout(() => URL.revokeObjectURL(objectUrl), 1000);

    return null;
}

/**
 * Clears the canvas and destroys the loaded document.
 * @param {HTMLCanvasElement} canvas
 */
export async function clear(canvas) {
    await release(canvas);
    clearCanvas(canvas);
}

/**
 * Destroys the loaded document, stops any load in flight and frees their workers. Safe to call when
 * nothing is loaded, and during circuit disposal.
 * @param {HTMLCanvasElement} canvas
 */
export async function dispose(canvas) {
    await release(canvas);
}

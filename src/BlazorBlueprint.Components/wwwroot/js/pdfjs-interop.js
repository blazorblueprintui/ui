// Pdf.js interop for PdfViewer component
// Handles loading, page navigation, zoom, fit-to-width and lifecycle.
//
// State is kept per canvas in a WeakMap, so each viewer owns its PDF document,
// its page cache and the in-flight render task. Every function that can change
// the visible state returns the same shape { ok, currentPage, pageCount, scale },
// which .NET deserializes into PdfViewerState to keep its own fields in sync.

import * as pdfjsLib from "../lib/pdfjs/pdf.mjs";

const workerUrl = new URL(
    "../lib/pdfjs/pdf.worker.mjs",
    import.meta.url
);

pdfjsLib.GlobalWorkerOptions.workerSrc = workerUrl.href;

const SCALE_STEP = 0.25;
const FIT_WIDTH_MARGIN = 32;

const viewers = new WeakMap();

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
    const viewer = viewers.get(canvas);
    return {
        ok: !!viewer,
        currentPage: viewer ? viewer.currentPage : 0,
        pageCount: viewer ? viewer.pageCount : 0,
        scale: viewer ? viewer.scale : 0
    };
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

/**
 * Loads a document into the canvas and renders its first page.
 * Replaces whatever the viewer was showing before (the previous PDF is destroyed).
 * @param {HTMLCanvasElement} canvas
 * @param {string} url
 * @param {{initialScale?: number, minScale?: number, maxScale?: number}} [options]
 * @returns {Promise<{ok: boolean, currentPage: number, pageCount: number, scale: number, error?: string}>}
 */
export async function load(canvas, url, options) {
    await dispose(canvas);

    const { initialScale, minScale, maxScale } = normalizeOptions(options);

    clearCanvas(canvas);

    try {
        const pdf = await pdfjsLib.getDocument({ url }).promise;

        const viewer = {
            pdf,
            url,
            data: null,
            pageCount: pdf.numPages,
            currentPage: 1,
            scale: initialScale,
            minScale,
            maxScale,
            pageCache: new Map(),
            renderTask: null
        };
        viewers.set(canvas, viewer);

        await renderPage(canvas, 1);

        return {
            ok: true,
            currentPage: viewer.currentPage,
            pageCount: viewer.pageCount,
            scale: viewer.scale
        };
    } catch (err) {
        clearCanvas(canvas);
        return {
            ok: false,
            currentPage: 0,
            pageCount: 0,
            scale: initialScale,
            error: err && err.message ? err.message : String(err)
        };
    }
}

/**
 * Loads a document from an in-memory byte stream. The .NET side sends a
 * DotNetStreamReference, whose `dotnetStream` property is a ReadableStream that
 * is consumed directly here, so PDF.js renders from the local bytes and there is
 * no second web request. Replaces whatever the viewer was showing before.
 * @param {HTMLCanvasElement} canvas
 * @param {{dotnetStream: ReadableStream}} streamReference
 * @param {{initialScale?: number, minScale?: number, maxScale?: number}} [options]
 * @returns {Promise<{ok: boolean, currentPage: number, pageCount: number, scale: number, error?: string}>}
 */
export async function loadData(canvas, streamReference, options) {
    await dispose(canvas);

    const { initialScale, minScale, maxScale } = normalizeOptions(options);

    clearCanvas(canvas);

    try {
        const data = await streamReference?.arrayBuffer();
        const pdf = await pdfjsLib.getDocument({ data }).promise;

        const viewer = {
            pdf,
            url: null,
            data,
            pageCount: pdf.numPages,
            currentPage: 1,
            scale: initialScale,
            minScale,
            maxScale,
            pageCache: new Map(),
            renderTask: null
        };
        viewers.set(canvas, viewer);

        await renderPage(canvas, 1);

        return {
            ok: true,
            currentPage: viewer.currentPage,
            pageCount: viewer.pageCount,
            scale: viewer.scale
        };
    } catch (err) {
        clearCanvas(canvas);
        return {
            ok: false,
            currentPage: 0,
            pageCount: 0,
            scale: initialScale,
            error: err && err.message ? err.message : String(err)
        };
    }
}

/**
 * Renders a given page at the viewer's current scale.
 * The previous render task is cancelled first so rapid navigation cannot paint
 * a stale frame over the newest one, and resolved pages are cached so paging
 * back and forth does not re-fetch every time.
 * @param {HTMLCanvasElement} canvas
 * @param {number} pageNumber
 */
async function renderPage(canvas, pageNumber) {
    const viewer = viewers.get(canvas);
    if (!viewer) {
        return;
    }

    let page = viewer.pageCache.get(pageNumber);
    if (!page) {
        page = await viewer.pdf.getPage(pageNumber);
        viewer.pageCache.set(pageNumber, page);
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
        viewer.renderTask = null;
    }
}

/**
 * Navigates to a page, clamped to the valid range. No-op when the target is
 * already visible. Returns the updated state.
 * @param {HTMLCanvasElement} canvas
 * @param {number} pageNumber
 */
function goToPage(canvas, pageNumber) {
    const viewer = viewers.get(canvas);
    if (!viewer) {
        return Promise.resolve(stateFor(canvas));
    }

    const view = stateFor(canvas);
    const target = clamp(Math.floor(pageNumber), 1, view.pageCount);
    if (target === viewer.currentPage) {
        return Promise.resolve(view);
    }

    return renderPage(canvas, target).then(() => stateFor(canvas));
}

/**
 * Moves to the next page, if any.
 * @param {HTMLCanvasElement} canvas
 */
export function nextPage(canvas) {
    const current = stateFor(canvas).currentPage;
    return goToPage(canvas, current ? current + 1 : 0);
}

/**
 * Moves to the previous page, if any.
 * @param {HTMLCanvasElement} canvas
 */
export function previousPage(canvas) {
    const current = stateFor(canvas).currentPage;
    return goToPage(canvas, current ? current - 1 : 0);
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
 * @returns {Promise<{currentPage: number, pageCount: number, scale: number}>}
 */
export async function setScale(canvas, scale) {
    const viewer = viewers.get(canvas);
    if (!viewer) {
        return stateFor(canvas);
    }
    const before = viewer.scale;
    viewer.scale = clamp(scale, viewer.minScale, viewer.maxScale);
    if (viewer.scale !== before) {
        await renderPage(canvas, viewer.currentPage);
    }
    return stateFor(canvas);
}

/**
 * Zooms in one step.
 * @param {HTMLCanvasElement} canvas
 */
export async function zoomIn(canvas) {
    const viewer = viewers.get(canvas);
    if (!viewer) {
        return stateFor(canvas);
    }
    viewer.scale = clamp(viewer.scale + SCALE_STEP, viewer.minScale, viewer.maxScale);
    await renderPage(canvas, viewer.currentPage);
    return stateFor(canvas);
}

/**
 * Zooms out one step.
 * @param {HTMLCanvasElement} canvas
 */
export async function zoomOut(canvas) {
    const viewer = viewers.get(canvas);
    if (!viewer) {
        return stateFor(canvas);
    }
    viewer.scale = clamp(viewer.scale - SCALE_STEP, viewer.minScale, viewer.maxScale);
    await renderPage(canvas, viewer.currentPage);
    return stateFor(canvas);
}

/**
 * Fits the current page to the width of its scroll container, leaving a small
 * margin on each side. Returns the updated state.
 * @param {HTMLCanvasElement} canvas
 */
export async function fitToWidth(canvas) {
    const viewer = viewers.get(canvas);
    if (!viewer) {
        return stateFor(canvas);
    }

    let page = viewer.pageCache.get(viewer.currentPage);
    if (!page) {
        page = await viewer.pdf.getPage(viewer.currentPage);
        viewer.pageCache.set(viewer.currentPage, page);
    }

    const baseViewport = page.getViewport({ scale: 1 });
    const container = canvas.parentElement;
    const available = (container ? container.clientWidth : 0) - FIT_WIDTH_MARGIN;
    if (available <= 0) {
        return stateFor(canvas);
    }

    viewer.scale = clamp(available / baseViewport.width, viewer.minScale, viewer.maxScale);
    await renderPage(canvas, viewer.currentPage);
    return stateFor(canvas);
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
 * Saves the open document as a PDF file. Documents loaded from bytes are down-
 * loaded straight from memory; URL-loaded documents are re-fetched and saved as
 * a blob, so a cross-origin page is downloaded instead of being navigated to.
 * If the source cannot be fetched (for example no CORS on the server), the URL
 * is opened in a new tab as a fallback.
 * @param {HTMLCanvasElement} canvas
 * @param {string|null} [fileName]
 */
export async function download(canvas, fileName) {
    const viewer = viewers.get(canvas);
    if (!viewer) {
        return;
    }

    let data = viewer.data;
    if (!data) {
        if (!viewer.url) {
            return;
        }
        try {
            const response = await fetch(viewer.url);
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }
            data = new Uint8Array(await response.arrayBuffer());
        } catch (err) {
            window.open(viewer.url, "_blank");
            return;
        }
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
}

/**
 * Clears the canvas and destroys the loaded document.
 * @param {HTMLCanvasElement} canvas
 */
export async function clear(canvas) {
    await dispose(canvas);
    clearCanvas(canvas);
}

/**
 * Destroys the loaded document and frees its resources. Safe to call when
 * nothing is loaded, and during circuit disposal.
 * @param {HTMLCanvasElement} canvas
 */
export async function dispose(canvas) {
    const viewer = viewers.get(canvas);
    if (!viewer) {
        return;
    }

    viewers.delete(canvas);

    try {
        viewer.renderTask && viewer.renderTask.cancel();
    } catch {
        // Ignore render cancellation errors.
    }
    viewer.renderTask = null;

    for (const page of viewer.pageCache.values()) {
        try {
            page.cleanup();
        } catch {
            // Ignore page cleanup errors.
        }
    }
    viewer.pageCache.clear();

    try {
        await viewer.pdf.destroy();
    } catch {
        // Ignore PDF.js cleanup errors.
    }
}
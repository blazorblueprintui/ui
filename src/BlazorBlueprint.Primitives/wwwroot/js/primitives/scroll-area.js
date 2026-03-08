// ScrollArea primitive - handles scroll tracking, thumb sizing, drag behavior
// and track click-to-jump for custom scrollbar implementation.

const instances = new Map();

/**
 * Gets a viewport element by its ID (helper for Blazor interop).
 * @param {string} viewportId - The viewport element ID.
 * @returns {HTMLElement|null}
 */
export function getViewportElement(viewportId) {
  return document.getElementById(viewportId);
}

/**
 * Initializes scroll area behavior for a scrollbar instance.
 * @param {string} instanceId - Unique instance identifier.
 * @param {HTMLElement} viewportEl - The scrollable viewport element.
 * @param {HTMLElement} scrollbarEl - The scrollbar track element.
 * @param {object} dotNetRef - Reference to the Blazor component.
 * @param {object} options - Configuration options.
 * @param {string} options.orientation - "vertical" or "horizontal".
 */
export function initialize(instanceId, viewportEl, scrollbarEl, dotNetRef, options) {
  if (!viewportEl || !scrollbarEl || !dotNetRef) {
    return;
  }

  const isVertical = options?.orientation !== "horizontal";
  const thumbEl = scrollbarEl.querySelector("[data-scrollarea-thumb]");

  const state = {
    viewportEl,
    scrollbarEl,
    thumbEl,
    dotNetRef,
    isVertical,
    isDragging: false,
    dragStartPos: 0,
    dragStartScroll: 0,
    pointerId: null,
  };

  const update = () => {
    const contentSize = isVertical ? viewportEl.scrollHeight : viewportEl.scrollWidth;
    const viewportSize = isVertical ? viewportEl.clientHeight : viewportEl.clientWidth;
    const scrollPos = isVertical ? viewportEl.scrollTop : viewportEl.scrollLeft;
    const maxScroll = contentSize - viewportSize;

    const hasOverflow = contentSize > viewportSize;
    const dataState = hasOverflow ? "visible" : "hidden";

    if (thumbEl) {
      if (hasOverflow) {
        const trackSize = isVertical ? scrollbarEl.clientHeight : scrollbarEl.clientWidth;
        const thumbSize = Math.max(20, (viewportSize / contentSize) * trackSize);
        const thumbPos = maxScroll > 0
          ? ((scrollPos / maxScroll) * (trackSize - thumbSize))
          : 0;

        if (isVertical) {
          thumbEl.style.height = `${thumbSize}px`;
          thumbEl.style.transform = `translate3d(0, ${thumbPos}px, 0)`;
        } else {
          thumbEl.style.width = `${thumbSize}px`;
          thumbEl.style.transform = `translate3d(${thumbPos}px, 0, 0)`;
        }
      }

      thumbEl.setAttribute("data-state", dataState);
    }

    const scrollPercent = maxScroll > 0 ? Math.round((scrollPos / maxScroll) * 100) : 0;

    try {
      dotNetRef.invokeMethodAsync("JsUpdateScrollState", scrollPercent, dataState).catch(() => {});
    } catch {
      // Component may be disposed
    }
  };

  // Scroll listener
  const handleScroll = () => {
    requestAnimationFrame(update);
  };

  // Track click to jump scroll position
  const handleTrackPointerDown = (e) => {
    if (e.target === thumbEl || (thumbEl && thumbEl.contains(e.target))) {
      return;
    }

    e.preventDefault();
    const rect = scrollbarEl.getBoundingClientRect();
    const contentSize = isVertical ? viewportEl.scrollHeight : viewportEl.scrollWidth;
    const viewportSize = isVertical ? viewportEl.clientHeight : viewportEl.clientWidth;
    const maxScroll = contentSize - viewportSize;

    if (maxScroll <= 0) {
      return;
    }

    const trackSize = isVertical ? rect.height : rect.width;
    const clickPos = isVertical ? (e.clientY - rect.top) : (e.clientX - rect.left);
    const ratio = clickPos / trackSize;

    viewportEl.scrollTo({
      [isVertical ? "top" : "left"]: ratio * maxScroll,
      behavior: "smooth",
    });
  };

  // Thumb drag handlers
  const handleThumbPointerDown = (e) => {
    if (state.isDragging) {
      return;
    }

    e.preventDefault();
    e.stopPropagation();
    state.isDragging = true;
    state.pointerId = e.pointerId;
    state.dragStartPos = isVertical ? e.clientY : e.clientX;
    state.dragStartScroll = isVertical ? viewportEl.scrollTop : viewportEl.scrollLeft;

    if (thumbEl) {
      thumbEl.setPointerCapture(e.pointerId);
    }

    document.body.style.userSelect = "none";
    document.body.style.cursor = "grabbing";
  };

  const handleThumbPointerMove = (e) => {
    if (!state.isDragging || e.pointerId !== state.pointerId) {
      return;
    }

    e.preventDefault();
    const currentPos = isVertical ? e.clientY : e.clientX;
    const delta = currentPos - state.dragStartPos;

    const contentSize = isVertical ? viewportEl.scrollHeight : viewportEl.scrollWidth;
    const viewportSize = isVertical ? viewportEl.clientHeight : viewportEl.clientWidth;
    const trackSize = isVertical ? scrollbarEl.clientHeight : scrollbarEl.clientWidth;
    const thumbSize = Math.max(20, (viewportSize / contentSize) * trackSize);

    const scrollableTrack = trackSize - thumbSize;
    const maxScroll = contentSize - viewportSize;

    if (scrollableTrack > 0) {
      const scrollDelta = (delta / scrollableTrack) * maxScroll;
      viewportEl.scrollTo({
        [isVertical ? "top" : "left"]: state.dragStartScroll + scrollDelta,
      });
    }
  };

  const handleThumbPointerUp = (e) => {
    if (e.pointerId !== state.pointerId) {
      return;
    }

    state.isDragging = false;
    state.pointerId = null;

    if (thumbEl) {
      thumbEl.releasePointerCapture(e.pointerId);
    }

    document.body.style.userSelect = "";
    document.body.style.cursor = "";
  };

  const handleThumbPointerCancel = (e) => {
    if (e.pointerId !== state.pointerId) {
      return;
    }

    state.isDragging = false;
    state.pointerId = null;
    document.body.style.userSelect = "";
    document.body.style.cursor = "";
  };

  // ResizeObserver to recalculate on size changes
  const resizeObserver = new ResizeObserver(() => {
    requestAnimationFrame(update);
  });
  resizeObserver.observe(viewportEl);
  if (viewportEl.firstElementChild) {
    resizeObserver.observe(viewportEl.firstElementChild);
  }

  // Attach listeners
  viewportEl.addEventListener("scroll", handleScroll, { passive: true });
  scrollbarEl.addEventListener("pointerdown", handleTrackPointerDown);

  if (thumbEl) {
    thumbEl.addEventListener("pointerdown", handleThumbPointerDown);
    thumbEl.addEventListener("pointermove", handleThumbPointerMove);
    thumbEl.addEventListener("pointerup", handleThumbPointerUp);
    thumbEl.addEventListener("pointercancel", handleThumbPointerCancel);
  }

  instances.set(instanceId, {
    state,
    resizeObserver,
    handleScroll,
    handleTrackPointerDown,
    handleThumbPointerDown,
    handleThumbPointerMove,
    handleThumbPointerUp,
    handleThumbPointerCancel,
  });

  // Initial update
  update();
}

/**
 * Disposes a scroll area instance and cleans up all listeners.
 * @param {string} instanceId - The instance to dispose.
 */
export function dispose(instanceId) {
  const stored = instances.get(instanceId);
  if (!stored) {
    return;
  }

  const { state, resizeObserver } = stored;

  state.viewportEl.removeEventListener("scroll", stored.handleScroll);
  state.scrollbarEl.removeEventListener("pointerdown", stored.handleTrackPointerDown);

  if (state.thumbEl) {
    state.thumbEl.removeEventListener("pointerdown", stored.handleThumbPointerDown);
    state.thumbEl.removeEventListener("pointermove", stored.handleThumbPointerMove);
    state.thumbEl.removeEventListener("pointerup", stored.handleThumbPointerUp);
    state.thumbEl.removeEventListener("pointercancel", stored.handleThumbPointerCancel);
  }

  resizeObserver.disconnect();
  instances.delete(instanceId);
}

// Slider drag handler for primitives
// Handles pointer drag events for smooth slider interaction

const sliderStates = new Map();

/**
 * Initializes drag handling for a slider element
 * @param {HTMLElement} trackElement - The slider track element
 * @param {object} dotNetRef - Reference to the Blazor component
 * @param {string} sliderId - Unique identifier for the slider
 * @param {object} options - Configuration options
 * @param {string} options.orientation - "horizontal" or "vertical"
 */
export function initialize(trackElement, dotNetRef, sliderId, options) {
  if (!trackElement || !dotNetRef) return;

  const isVertical = options?.orientation === "vertical";

  const state = {
    trackElement,
    dotNetRef,
    isDragging: false,
    pointerId: null,
  };

  const calculatePercentage = (e) => {
    const rect = trackElement.getBoundingClientRect();
    if (isVertical) {
      return Math.max(
        0,
        Math.min(1, 1 - (e.clientY - rect.top) / rect.height)
      );
    }
    return Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width));
  };

  const handlePointerMove = (e) => {
    if (!state.isDragging || e.pointerId !== state.pointerId) return;
    e.preventDefault();
    const percentage = calculatePercentage(e);
    dotNetRef
      .invokeMethodAsync("JsUpdateValueFromPercentage", percentage)
      .catch(() => {});
  };

  const handlePointerUp = (e) => {
    if (e.pointerId !== state.pointerId) return;
    if (state.isDragging) {
      state.isDragging = false;
      state.pointerId = null;
      trackElement.releasePointerCapture(e.pointerId);
      document.body.style.userSelect = "";
      document.body.style.cursor = "";
    }
  };

  const handlePointerCancel = (e) => {
    if (e.pointerId !== state.pointerId) return;
    state.isDragging = false;
    state.pointerId = null;
    document.body.style.userSelect = "";
    document.body.style.cursor = "";
  };

  const handleTrackPointerDown = (e) => {
    if (state.isDragging) return;
    e.preventDefault();
    state.isDragging = true;
    state.pointerId = e.pointerId;
    trackElement.setPointerCapture(e.pointerId);
    document.body.style.userSelect = "none";
    document.body.style.cursor = "grabbing";

    const percentage = calculatePercentage(e);
    dotNetRef
      .invokeMethodAsync("JsUpdateValueFromPercentage", percentage)
      .catch(() => {});
  };

  trackElement.addEventListener("pointerdown", handleTrackPointerDown);
  trackElement.addEventListener("pointermove", handlePointerMove);
  trackElement.addEventListener("pointerup", handlePointerUp);
  trackElement.addEventListener("pointercancel", handlePointerCancel);

  sliderStates.set(sliderId, {
    state,
    handleTrackPointerDown,
    handlePointerMove,
    handlePointerUp,
    handlePointerCancel,
    trackElement,
  });
}

/**
 * Removes slider drag handling
 * @param {string} sliderId - Unique identifier for the slider
 */
export function dispose(sliderId) {
  const stored = sliderStates.get(sliderId);
  if (stored) {
    stored.trackElement.removeEventListener(
      "pointerdown",
      stored.handleTrackPointerDown
    );
    stored.trackElement.removeEventListener(
      "pointermove",
      stored.handlePointerMove
    );
    stored.trackElement.removeEventListener(
      "pointerup",
      stored.handlePointerUp
    );
    stored.trackElement.removeEventListener(
      "pointercancel",
      stored.handlePointerCancel
    );
    sliderStates.delete(sliderId);
  }
}

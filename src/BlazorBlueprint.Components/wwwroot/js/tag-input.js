/**
 * Tag Input JavaScript interop module.
 * Handles trigger key preventDefault, paste event reading, and container click-to-focus.
 */

const instances = new Map();

/**
 * Initializes event handling for a tag input component.
 * @param {HTMLElement} containerEl - The outer container div.
 * @param {HTMLInputElement} inputEl - The inline text input element.
 * @param {DotNetObject} dotNetRef - Reference to the Blazor component.
 * @param {string} instanceId - Unique ID for this instance.
 * @param {object} config - Configuration object.
 * @param {number} config.triggers - Bitmask of TagInputTrigger flags.
 */
export function initialize(containerEl, inputEl, dotNetRef, instanceId, config) {
  if (!containerEl || !inputEl || !dotNetRef) {
    return;
  }

  const handleKeyDown = (e) => {
    const key = e.key;

    // Check if the key is a configured trigger
    const isTrigger =
      (key === 'Enter' && (config.triggers & 1)) ||
      (key === ',' && (config.triggers & 2)) ||
      (key === ' ' && (config.triggers & 4)) ||
      (key === 'Tab' && (config.triggers & 8)) ||
      (key === ';' && (config.triggers & 16));

    if (isTrigger) {
      e.preventDefault();
      dotNetRef.invokeMethodAsync('JsTriggerAdd').catch(() => {});
      return;
    }

    // Backspace on empty input removes the last tag
    if (key === 'Backspace' && inputEl.value === '') {
      dotNetRef.invokeMethodAsync('JsBackspace').catch(() => {});
      return;
    }

    // Arrow key navigation for suggestions
    if (key === 'ArrowDown') {
      e.preventDefault();
      dotNetRef.invokeMethodAsync('JsSuggestionNext').catch(() => {});
      return;
    }

    if (key === 'ArrowUp') {
      e.preventDefault();
      dotNetRef.invokeMethodAsync('JsSuggestionPrev').catch(() => {});
      return;
    }

    // Escape closes suggestions
    if (key === 'Escape') {
      dotNetRef.invokeMethodAsync('JsSuggestionClose').catch(() => {});
    }
  };

  const handlePaste = (e) => {
    const text = e.clipboardData ? e.clipboardData.getData('text') : '';
    if (!text) {
      return;
    }

    // Check if pasted text contains any delimiter that matches a configured trigger
    const hasDelimiter =
      ((config.triggers & 2) && text.includes(',')) ||
      ((config.triggers & 16) && text.includes(';')) ||
      ((config.triggers & 4) && text.includes(' '));

    if (hasDelimiter) {
      e.preventDefault();
      dotNetRef.invokeMethodAsync('JsPasteText', text).catch(() => {});
    }
  };

  const handleContainerClick = (e) => {
    // Don't steal focus from tag remove buttons
    if (e.target.closest('button')) {
      return;
    }
    inputEl.focus();
  };

  // Use capture phase for keydown to preventDefault before browser defaults
  inputEl.addEventListener('keydown', handleKeyDown, true);
  inputEl.addEventListener('paste', handlePaste);
  containerEl.addEventListener('click', handleContainerClick);

  instances.set(instanceId, {
    handleKeyDown,
    handlePaste,
    handleContainerClick,
    inputEl,
    containerEl
  });
}

/**
 * Focuses the input element programmatically.
 * @param {string} instanceId - The instance to focus.
 */
export function focusInput(instanceId) {
  const stored = instances.get(instanceId);
  if (stored) {
    stored.inputEl.focus();
  }
}

/**
 * Removes event handlers and cleans up state.
 * @param {string} instanceId - The instance to dispose.
 */
export function dispose(instanceId) {
  const stored = instances.get(instanceId);
  if (!stored) {
    return;
  }

  stored.inputEl.removeEventListener('keydown', stored.handleKeyDown, true);
  stored.inputEl.removeEventListener('paste', stored.handlePaste);
  stored.containerEl.removeEventListener('click', stored.handleContainerClick);
  instances.delete(instanceId);
}

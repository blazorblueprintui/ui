// Quill.js interop for RichTextEditor component
// Handles editor initialization, events, and content management

let editorStates = new Map();

/**
 * Initializes a Quill editor instance
 * @param {HTMLElement} element - The editor container element
 * @param {DotNetObject} dotNetRef - Reference to the Blazor component
 * @param {string} editorId - Unique identifier for the editor
 * @param {Object} options - Editor configuration options
 */
export function initializeEditor(element, dotNetRef, editorId, options) {
    if (!element || !dotNetRef) {
        console.error('initializeEditor: missing required parameters');
        return;
    }

    if (typeof Quill === 'undefined') {
        console.error('Quill is not loaded. Please include Quill.js in your page.');
        return;
    }

    const quillOptions = {
        theme: 'snow',
        placeholder: options.placeholder || '',
        readOnly: options.readOnly || false,
        modules: {
            toolbar: options.toolbar === false ? false : (options.toolbar || null)
        }
    };

    const quill = new Quill(element, quillOptions);

    // Debounced text-change handler
    let textChangeTimeout;
    quill.on('text-change', (delta, oldDelta, source) => {
        clearTimeout(textChangeTimeout);
        textChangeTimeout = setTimeout(() => {
            dotNetRef.invokeMethodAsync('OnTextChangeCallback', {
                delta: JSON.stringify(delta),
                oldDelta: JSON.stringify(oldDelta),
                source: source,
                html: quill.root.innerHTML,
                text: quill.getText(),
                length: quill.getLength()
            }).catch(err => console.error('Error in text-change:', err));
        }, 150);
    });

    // Selection-change for focus/blur detection
    quill.on('selection-change', (range, oldRange, source) => {
        dotNetRef.invokeMethodAsync('OnSelectionChangeCallback', {
            range: range,
            oldRange: oldRange,
            source: source
        }).catch(err => console.error('Error in selection-change:', err));
    });

    editorStates.set(editorId, { quill, dotNetRef, textChangeTimeout });
}

/**
 * Disposes of an editor instance
 * @param {string} editorId - Unique identifier for the editor
 */
export function disposeEditor(editorId) {
    const stored = editorStates.get(editorId);
    if (stored) {
        clearTimeout(stored.textChangeTimeout);
        editorStates.delete(editorId);
    }
}

/**
 * Sets the HTML content of the editor
 * @param {string} editorId - Unique identifier for the editor
 * @param {string} html - HTML content to set
 */
export function setHtml(editorId, html) {
    const stored = editorStates.get(editorId);
    if (stored) {
        stored.quill.root.innerHTML = html || '';
    }
}

/**
 * Gets the HTML content of the editor
 * @param {string} editorId - Unique identifier for the editor
 * @returns {string} HTML content
 */
export function getHtml(editorId) {
    const stored = editorStates.get(editorId);
    if (stored) {
        return stored.quill.root.innerHTML;
    }
    return '';
}

/**
 * Sets the editor contents using a Delta object
 * @param {string} editorId - Unique identifier for the editor
 * @param {string} delta - JSON string representation of the Delta
 */
export function setContents(editorId, delta) {
    const stored = editorStates.get(editorId);
    if (stored && delta) {
        stored.quill.setContents(JSON.parse(delta));
    }
}

/**
 * Gets the editor contents as a Delta object
 * @param {string} editorId - Unique identifier for the editor
 * @returns {string} JSON string representation of the Delta
 */
export function getContents(editorId) {
    const stored = editorStates.get(editorId);
    if (stored) {
        return JSON.stringify(stored.quill.getContents());
    }
    return '{}';
}

/**
 * Gets the plain text content of the editor
 * @param {string} editorId - Unique identifier for the editor
 * @returns {string} Plain text content
 */
export function getText(editorId) {
    const stored = editorStates.get(editorId);
    if (stored) {
        return stored.quill.getText();
    }
    return '';
}

/**
 * Gets the length of the editor content
 * @param {string} editorId - Unique identifier for the editor
 * @returns {number} Content length
 */
export function getLength(editorId) {
    const stored = editorStates.get(editorId);
    if (stored) {
        return stored.quill.getLength();
    }
    return 0;
}

/**
 * Gets the current selection range
 * @param {string} editorId - Unique identifier for the editor
 * @returns {Object|null} Selection range with index and length, or null
 */
export function getSelection(editorId) {
    const stored = editorStates.get(editorId);
    if (stored) {
        return stored.quill.getSelection();
    }
    return null;
}

/**
 * Sets the selection range
 * @param {string} editorId - Unique identifier for the editor
 * @param {number} index - Start index
 * @param {number} length - Selection length
 */
export function setSelection(editorId, index, length) {
    const stored = editorStates.get(editorId);
    if (stored) {
        stored.quill.setSelection(index, length);
    }
}

/**
 * Applies formatting to the current selection
 * @param {string} editorId - Unique identifier for the editor
 * @param {string} formatName - Name of the format
 * @param {*} value - Format value
 */
export function format(editorId, formatName, value) {
    const stored = editorStates.get(editorId);
    if (stored) {
        stored.quill.format(formatName, value);
    }
}

/**
 * Gets the formatting at the current selection
 * @param {string} editorId - Unique identifier for the editor
 * @returns {Object} Format object
 */
export function getFormat(editorId) {
    const stored = editorStates.get(editorId);
    if (stored) {
        return stored.quill.getFormat();
    }
    return {};
}

/**
 * Enables the editor
 * @param {string} editorId - Unique identifier for the editor
 */
export function enable(editorId) {
    const stored = editorStates.get(editorId);
    if (stored) {
        stored.quill.enable(true);
    }
}

/**
 * Disables the editor
 * @param {string} editorId - Unique identifier for the editor
 */
export function disable(editorId) {
    const stored = editorStates.get(editorId);
    if (stored) {
        stored.quill.enable(false);
    }
}

/**
 * Focuses the editor
 * @param {string} editorId - Unique identifier for the editor
 */
export function focus(editorId) {
    const stored = editorStates.get(editorId);
    if (stored) {
        stored.quill.focus();
    }
}

/**
 * Removes focus from the editor
 * @param {string} editorId - Unique identifier for the editor
 */
export function blur(editorId) {
    const stored = editorStates.get(editorId);
    if (stored) {
        stored.quill.blur();
    }
}

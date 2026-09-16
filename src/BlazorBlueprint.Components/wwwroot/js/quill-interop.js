// Quill.js interop for RichTextEditor component
// Handles editor initialization, events, and content management

let editorStates = new Map();
let attributorsRegistered = false;

/**
 * Quill's default align attributor writes ql-align-* classes, which only mean something
 * where Quill's stylesheet is loaded. The style attributor writes text-align inline, so
 * the HTML the consumer stores renders aligned anywhere. Colour and background already
 * default to inline styles. Registered once, before the first editor is created.
 */
function registerAttributors() {
    if (attributorsRegistered) {
        return;
    }
    attributorsRegistered = true;
    Quill.register(Quill.import('attributors/style/align'), true);
}

function historyState(quill) {
    return {
        canUndo: quill.history.stack.undo.length > 0,
        canRedo: quill.history.stack.redo.length > 0
    };
}

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

    registerAttributors();

    const quillOptions = {
        theme: null,  // Headless mode - we handle the toolbar ourselves
        placeholder: options.placeholder || '',
        readOnly: options.readOnly || false,
        modules: {
            toolbar: false,  // We build our own toolbar in Blazor
            table: true,     // Quill's built-in table module: rows, columns and whole tables
            // Programmatic Value updates must not be undoable: undo is for what the user typed.
            history: { userOnly: true },
            // Dropped and pasted images go through the same path as the toolbar button, so a
            // consumer's ImageUploader sees all of them.
            uploader: {
                handler: (range, files) => handleImageFiles(editorId, files, range)
            }
        },
        // Explicitly register all formats we support. 'table' pulls in its row, body and
        // container blots, so pasted <table> markup survives instead of flattening.
        formats: [
            'bold', 'italic', 'underline', 'strike', 'code',
            'color', 'background',
            'header', 'align',
            'list',
            'blockquote', 'code-block',
            'link', 'image',
            'indent',
            'table'
        ]
    };

    const quill = new Quill(element, quillOptions);

    // Quill writes checklist items out as <li data-list="checked"> but only reads
    // data-checked on the <ul> back in, so its own HTML loses the checked state on the
    // way round. Mark the items before the list matcher runs; applyFormat keeps a value
    // that is already set.
    const Delta = Quill.import('delta');
    quill.clipboard.addMatcher('li', (node, delta) => {
        const value = node.getAttribute('data-list');
        if (value !== 'checked' && value !== 'unchecked') {
            return delta;
        }
        return delta.compose(new Delta().retain(delta.length(), { list: value }));
    });

    // Debounced text-change handler
    let textChangeTimeout;
    const textChangeHandler = (delta, oldDelta, source) => {
        clearTimeout(textChangeTimeout);
        textChangeTimeout = setTimeout(() => {
            // Check if callbacks are suppressed (during programmatic updates)
            const state = editorStates.get(editorId);
            if (state && state.suppressCallbacks) {
                return;
            }

            dotNetRef.invokeMethodAsync('OnTextChangeCallback', {
                delta: JSON.stringify(delta),
                oldDelta: JSON.stringify(oldDelta),
                source: source,
                html: quill.getSemanticHTML(),
                text: quill.getText(),
                length: quill.getLength(),
                ...historyState(quill)
            }).catch(err => console.error('Error in text-change:', err));
        }, 150);
    };
    quill.on('text-change', textChangeHandler);

    // Selection-change for focus/blur detection and format tracking
    const selectionChangeHandler = (range, oldRange, source) => {
        const format = range ? quill.getFormat(range) : {};
        dotNetRef.invokeMethodAsync('OnSelectionChangeCallback', {
            range: range,
            oldRange: oldRange,
            source: source,
            format: format
        }).catch(err => console.error('Error in selection-change:', err));
    };
    quill.on('selection-change', selectionChangeHandler);

    editorStates.set(editorId, {
        quill,
        dotNetRef,
        textChangeTimeout,
        textChangeHandler,
        selectionChangeHandler,
        suppressCallbacks: false,
        hasImageUploader: !!options.hasImageUploader
    });
}

/**
 * Tells the editor whether .NET has an ImageUploader, so dropped and pasted images are
 * streamed to it instead of being inlined as data URLs.
 * @param {string} editorId - Unique identifier for the editor
 * @param {boolean} hasImageUploader
 */
export function setImageUploader(editorId, hasImageUploader) {
    const stored = editorStates.get(editorId);
    if (stored) {
        stored.hasImageUploader = !!hasImageUploader;
    }
}

/**
 * Disposes of an editor instance
 * @param {string} editorId - Unique identifier for the editor
 */
export function disposeEditor(editorId) {
    const stored = editorStates.get(editorId);
    if (stored) {
        // Clear pending debounce timeout
        clearTimeout(stored.textChangeTimeout);

        // Remove event handlers to prevent memory leaks
        stored.quill.off('text-change', stored.textChangeHandler);
        stored.quill.off('selection-change', stored.selectionChangeHandler);

        editorStates.delete(editorId);
    }
}

/**
 * Sets the HTML content of the editor
 * Uses Quill's clipboard module to properly convert HTML to Delta,
 * maintaining synchronization between DOM and internal state.
 * Suppresses callbacks to prevent update loops during programmatic updates.
 * @param {string} editorId - Unique identifier for the editor
 * @param {string} html - HTML content to set
 */
export function setHtml(editorId, html) {
    const stored = editorStates.get(editorId);
    if (stored) {
        const quill = stored.quill;

        // Suppress callbacks during programmatic update
        stored.suppressCallbacks = true;

        try {
            if (!html) {
                // Empty content - set empty Delta
                quill.setContents([{ insert: '\n' }], 'api');
            } else {
                // Convert HTML to Delta using Quill's clipboard module
                const delta = quill.clipboard.convert({ html: html });
                quill.setContents(delta, 'api');
            }
        } finally {
            // Re-enable callbacks after a short delay to allow any pending events to be suppressed
            setTimeout(() => {
                stored.suppressCallbacks = false;
            }, 200);
        }
    }
}

/**
 * Gets the HTML content of the editor using Quill's semantic HTML output,
 * which is normalized and consistent across browsers.
 * @param {string} editorId - Unique identifier for the editor
 * @returns {string} HTML content
 */
export function getHtml(editorId) {
    const stored = editorStates.get(editorId);
    if (stored) {
        return stored.quill.getSemanticHTML();
    }
    return '';
}

/**
 * Sets the editor contents using a Delta object
 * Suppresses callbacks to prevent update loops during programmatic updates.
 * @param {string} editorId - Unique identifier for the editor
 * @param {string} delta - JSON string representation of the Delta
 */
export function setContents(editorId, delta) {
    const stored = editorStates.get(editorId);
    if (stored && delta) {
        // Suppress callbacks during programmatic update
        stored.suppressCallbacks = true;

        try {
            stored.quill.setContents(JSON.parse(delta), 'api');
        } finally {
            // Re-enable callbacks after a short delay to allow any pending events to be suppressed
            setTimeout(() => {
                stored.suppressCallbacks = false;
            }, 200);
        }
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
 * Applies formatting and returns the updated format state
 * Used for all formats to ensure immediate state sync
 * @param {string} editorId - Unique identifier for the editor
 * @param {string} formatName - Name of the format
 * @param {*} value - Format value
 * @returns {Object} Updated format state
 */
export function formatAndGetState(editorId, formatName, value) {
    const stored = editorStates.get(editorId);
    if (stored) {
        const quill = stored.quill;
        const range = quill.getSelection();

        // Special handling for block format removal in Quill v2
        // Quill's format('code-block', false) and format('blockquote', false) don't work correctly
        // We need to preserve inline formats when removing block formats
        if ((formatName === 'code-block' || formatName === 'blockquote') && value === false && range) {
            const [line, offset] = quill.getLine(range.index);
            if (line) {
                const lineIndex = quill.getIndex(line);
                const lineLength = line.length();

                // Get the Delta for this line to preserve inline formats
                const lineDelta = quill.getContents(lineIndex, lineLength);

                // Collect inline formats from each operation in the line
                const inlineFormats = [];
                let currentIndex = lineIndex;

                for (const op of lineDelta.ops) {
                    if (op.insert && typeof op.insert === 'string' && op.attributes) {
                        // Filter to only inline formats (not block formats)
                        const inlineAttrs = {};
                        const inlineFormatNames = ['bold', 'italic', 'underline', 'strike', 'link', 'code', 'color', 'background'];
                        for (const key of inlineFormatNames) {
                            if (op.attributes[key] !== undefined) {
                                inlineAttrs[key] = op.attributes[key];
                            }
                        }
                        if (Object.keys(inlineAttrs).length > 0) {
                            inlineFormats.push({
                                index: currentIndex,
                                length: op.insert.length,
                                formats: inlineAttrs
                            });
                        }
                    }
                    if (op.insert) {
                        currentIndex += typeof op.insert === 'string' ? op.insert.length : 1;
                    }
                }

                // Remove all formatting from the line (this removes the block format)
                quill.removeFormat(lineIndex, lineLength, 'api');

                // Re-apply the inline formats we saved
                for (const fmt of inlineFormats) {
                    for (const [key, val] of Object.entries(fmt.formats)) {
                        quill.formatText(fmt.index, fmt.length, key, val, 'api');
                    }
                }
            }
        } else {
            quill.format(formatName, value);
        }

        // Get the updated format state immediately after applying
        const newRange = quill.getSelection();
        return newRange ? quill.getFormat(newRange) : quill.getFormat();
    }
    return {};
}

/**
 * Puts the caret back where it was before a toolbar control took focus.
 * Toolbar buttons are focusable, so by the time their click reaches .NET the editor has
 * lost focus and getSelection() is null. Quill keeps the last real range in
 * selection.savedRange; failing that, the end of the document.
 * @param {Quill} quill
 */
function restoreSelection(quill) {
    const range = quill.getSelection()
        ?? quill.selection.savedRange
        ?? { index: Math.max(quill.getLength() - 1, 0), length: 0 };
    quill.focus();
    quill.setSelection(range.index, range.length, 'silent');
}

/**
 * Inserts a table via Quill's built-in table module and returns the format state at the
 * new caret position, so .NET can enable the row/column actions.
 *
 * Quill's own insertTable splices the rows in at the caret index: the text before the
 * caret ends up inside the first cell, and when the caret sits directly after another
 * table Parchment merges the two into one. So the table always starts on its own line:
 * the current line is ended first, or an empty line that directly follows a table gets a
 * plain paragraph in front of it. Inside a table it does nothing — tables do not nest.
 * @param {string} editorId - Unique identifier for the editor
 * @param {number} rows
 * @param {number} columns
 * @returns {Object} Format object
 */
export function insertTable(editorId, rows, columns) {
    const stored = editorStates.get(editorId);
    if (!stored) {
        return {};
    }
    const quill = stored.quill;
    const table = quill.getModule('table');
    if (!table) {
        return {};
    }

    restoreSelection(quill);
    const range = quill.getSelection();
    if (!range) {
        return quill.getFormat();
    }
    if (quill.getFormat(range).table) {
        return quill.getFormat(range);
    }

    const [line] = quill.getLine(range.index);
    if (line) {
        const lineStart = quill.getIndex(line);
        const lineEnd = lineStart + line.length() - 1; // the line's own newline
        const isEmpty = line.length() === 1;
        const followsTable = line.prev != null && line.prev.statics.blotName === 'table-container';
        if (!isEmpty || followsTable) {
            const at = isEmpty ? lineStart : lineEnd;
            quill.insertText(at, '\n', 'user');
            // The new line inherits the block format of the one it split (heading, list,
            // quote); the table should be followed by a plain paragraph instead.
            quill.removeFormat(at + 1, 0, 'silent');
            quill.setSelection(at + 1, 0, 'silent');
        }
    }

    table.insertTable(rows, columns);
    return quill.getFormat();
}

const TABLE_ACTIONS = new Set([
    'insertRowAbove', 'insertRowBelow',
    'insertColumnLeft', 'insertColumnRight',
    'deleteRow', 'deleteColumn', 'deleteTable'
]);

/**
 * Runs a row, column or table action of Quill's built-in table module on the table
 * that contains the caret. No-op outside a table.
 * @param {string} editorId - Unique identifier for the editor
 * @param {string} action - One of TABLE_ACTIONS
 * @returns {Object} Format object at the caret afterwards
 */
export function tableAction(editorId, action) {
    const stored = editorStates.get(editorId);
    if (!stored || !TABLE_ACTIONS.has(action)) {
        return {};
    }
    const quill = stored.quill;
    const table = quill.getModule('table');
    if (!table) {
        return {};
    }
    restoreSelection(quill);
    table[action]();
    return quill.getFormat();
}

/**
 * Undoes the last user change.
 * @param {string} editorId - Unique identifier for the editor
 * @returns {{canUndo: boolean, canRedo: boolean}} History state afterwards
 */
export function undo(editorId) {
    const stored = editorStates.get(editorId);
    if (!stored) {
        return { canUndo: false, canRedo: false };
    }
    restoreSelection(stored.quill);
    stored.quill.history.undo();
    return historyState(stored.quill);
}

/**
 * Redoes the last undone change.
 * @param {string} editorId - Unique identifier for the editor
 * @returns {{canUndo: boolean, canRedo: boolean}} History state afterwards
 */
export function redo(editorId) {
    const stored = editorStates.get(editorId);
    if (!stored) {
        return { canUndo: false, canRedo: false };
    }
    restoreSelection(stored.quill);
    stored.quill.history.redo();
    return historyState(stored.quill);
}

function readAsDataUrl(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result);
        reader.onerror = () => reject(reader.error);
        reader.readAsDataURL(file);
    });
}

/**
 * Inserts image files at a position: through the consumer's ImageUploader when there is
 * one (it returns the URL to embed, or null to drop the file), otherwise as data URLs,
 * which is what Quill does on its own.
 * @param {string} editorId - Unique identifier for the editor
 * @param {FileList|File[]} files
 * @param {{index: number}|null} range - Where to insert; the caret when null
 */
async function handleImageFiles(editorId, files, range) {
    const stored = editorStates.get(editorId);
    if (!stored) {
        return;
    }
    const quill = stored.quill;
    const images = Array.from(files || []).filter(f => f && f.type && f.type.startsWith('image/'));
    if (images.length === 0) {
        return;
    }

    let index = range ? range.index : (quill.getSelection(true) ?? { index: quill.getLength() - 1 }).index;
    for (const file of images) {
        let url = null;
        if (stored.hasImageUploader) {
            try {
                url = await stored.dotNetRef.invokeMethodAsync(
                    'OnImageUploadCallback',
                    DotNet.createJSStreamReference(file),
                    file.name,
                    file.type,
                    file.size);
            } catch (err) {
                console.error('BlazorBlueprint: image upload failed', err);
                url = null;
            }
        } else {
            url = await readAsDataUrl(file);
        }
        if (!url) {
            continue;
        }
        quill.insertEmbed(index, 'image', url, 'user');
        index += 1;
    }
    quill.setSelection(index, 0, 'silent');
}

/**
 * Opens the browser's file picker for images and inserts the chosen files at the caret.
 * Must run within a user activation (a toolbar click), which the browser requires for a
 * file dialog.
 * @param {string} editorId - Unique identifier for the editor
 */
export function pickImage(editorId) {
    const stored = editorStates.get(editorId);
    if (!stored) {
        return;
    }
    const quill = stored.quill;
    restoreSelection(quill);
    const range = quill.getSelection() ?? { index: Math.max(quill.getLength() - 1, 0), length: 0 };

    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
    input.multiple = true;
    input.style.display = 'none';
    input.addEventListener('change', () => {
        handleImageFiles(editorId, input.files, range).finally(() => input.remove());
    });
    input.addEventListener('cancel', () => input.remove());
    document.body.appendChild(input);
    input.click();
}

/**
 * Inserts an image by URL at the caret.
 * @param {string} editorId - Unique identifier for the editor
 * @param {string} url
 */
export function insertImage(editorId, url) {
    const stored = editorStates.get(editorId);
    if (!stored || !url) {
        return;
    }
    const quill = stored.quill;
    restoreSelection(quill);
    const range = quill.getSelection() ?? { index: Math.max(quill.getLength() - 1, 0), length: 0 };
    quill.insertEmbed(range.index, 'image', url, 'user');
    quill.setSelection(range.index + 1, 0, 'silent');
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


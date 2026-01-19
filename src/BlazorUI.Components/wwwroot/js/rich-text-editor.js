/**
 * Rich Text Editor JavaScript module
 * Uses Selection API and Range API for modern, standards-compliant text formatting
 */

// Store references for cleanup using WeakMap
const editorMap = new WeakMap();

/**
 * Initialize the rich text editor
 * @param {HTMLElement} editorElement - The contenteditable element
 * @param {DotNetObjectReference} dotNetRef - Reference to Blazor component
 */
export function initialize(editorElement, dotNetRef) {
    if (!editorElement || editorMap.has(editorElement)) return;

    // Create data object first so handlers can reference it
    const data = {
        handlers: null,
        dotNetRef,
        history: [],           // Array of HTML snapshots for undo/redo
        historyIndex: -1,      // Current position in history
        maxHistory: 100        // Limit history size (increased for per-keystroke undo)
    };

    const handlers = {
        input: () => {
            // Notify Blazor and save state for undo
            notifyContentChanged(editorElement);
        },
        keydown: (e) => {
            // Intercept Ctrl+Z/Y to prevent browser's native undo and use our custom one
            if (e.ctrlKey || e.metaKey) {
                if (e.key === 'z' || e.key === 'Z') {
                    e.preventDefault();
                    if (e.shiftKey) {
                        redo(editorElement);
                    } else {
                        undo(editorElement);
                    }
                } else if (e.key === 'y' || e.key === 'Y') {
                    e.preventDefault();
                    redo(editorElement);
                }
            }
        },
        paste: (e) => {
            e.preventDefault();
            // Get HTML or plain text from clipboard
            let content = e.clipboardData.getData('text/html');
            if (!content) {
                content = e.clipboardData.getData('text/plain');
                // Convert plain text line breaks to <br>
                content = content.replace(/\n/g, '<br>');
            } else {
                content = sanitizeHtml(content);
            }
            insertHtmlAtCursor(content);
            // Notify Blazor and save state (via input handler)
            handlers.input();
        }
    };

    data.handlers = handlers;

    editorElement.addEventListener('input', handlers.input);
    editorElement.addEventListener('keydown', handlers.keydown);
    editorElement.addEventListener('paste', handlers.paste);

    // Store editor data
    editorMap.set(editorElement, data);

    // Save initial state
    saveState(editorElement);
}

/**
 * Toggle formatting on the selected text
 * @param {HTMLElement} editorElement - The contenteditable element
 * @param {string} tagName - The tag to toggle (strong, em, u, s)
 */
export function toggleFormat(editorElement, tagName) {
    const selection = window.getSelection();
    if (!selection.rangeCount) return;

    const range = selection.getRangeAt(0);

    // Check if selection is within the editor
    if (!editorElement.contains(range.commonAncestorContainer)) {
        editorElement.focus();
        return;
    }

    const upperTag = tagName.toUpperCase();

    // Check if already has this formatting
    const existingElement = getParentWithTag(range.commonAncestorContainer, upperTag);

    if (existingElement && editorElement.contains(existingElement)) {
        // Unwrap the element
        unwrapElement(existingElement);
    } else {
        // Apply formatting
        if (range.collapsed) {
            // No selection - insert empty formatted element for typing
            const element = document.createElement(tagName);
            element.innerHTML = '&#8203;'; // Zero-width space
            range.insertNode(element);
            // Move cursor inside
            range.setStart(element, 1);
            range.collapse(true);
            selection.removeAllRanges();
            selection.addRange(range);
        } else {
            // Wrap selection
            try {
                const element = document.createElement(tagName);
                range.surroundContents(element);
            } catch (e) {
                // surroundContents fails with partial selections
                // Fall back to extracting and reinserting
                const fragment = range.extractContents();
                const element = document.createElement(tagName);
                element.appendChild(fragment);
                range.insertNode(element);
            }
        }
    }

    notifyContentChanged(editorElement);
}

/**
 * Insert a list at the cursor position
 * @param {HTMLElement} editorElement - The contenteditable element
 * @param {boolean} ordered - True for ordered list, false for unordered
 */
export function insertList(editorElement, ordered) {
    const selection = window.getSelection();
    if (!selection.rangeCount) {
        editorElement.focus();
        return;
    }

    const range = selection.getRangeAt(0);

    // Check if selection is within the editor
    if (!editorElement.contains(range.commonAncestorContainer)) {
        editorElement.focus();
        return;
    }

    const listTag = ordered ? 'ol' : 'ul';

    // Check if already in a list of same type
    const existingList = getParentWithTag(range.commonAncestorContainer, listTag.toUpperCase());

    if (existingList && editorElement.contains(existingList)) {
        // Toggle off - unwrap list
        unwrapList(existingList);
    } else {
        // Check if in a list of other type - convert
        const otherListTag = ordered ? 'UL' : 'OL';
        const otherList = getParentWithTag(range.commonAncestorContainer, otherListTag);

        if (otherList && editorElement.contains(otherList)) {
            // Convert list type
            const newList = document.createElement(listTag);
            while (otherList.firstChild) {
                newList.appendChild(otherList.firstChild);
            }
            otherList.parentNode.replaceChild(newList, otherList);
        } else {
            // Create new list
            const list = document.createElement(listTag);

            if (range.collapsed) {
                const li = document.createElement('li');
                li.innerHTML = '&#8203;'; // Zero-width space for cursor
                list.appendChild(li);
            } else {
                // Extract selected content and split by lines
                const fragment = range.extractContents();
                const lines = extractLinesFromFragment(fragment);

                lines.forEach(lineContent => {
                    const li = document.createElement('li');
                    if (typeof lineContent === 'string') {
                        li.innerHTML = lineContent || '&#8203;';
                    } else {
                        li.appendChild(lineContent);
                    }
                    list.appendChild(li);
                });
            }

            range.insertNode(list);

            // Position cursor inside last list item
            const lastLi = list.lastElementChild;
            if (lastLi) {
                const textNode = lastLi.firstChild || lastLi;
                range.setStart(textNode, textNode.nodeType === Node.TEXT_NODE ? textNode.length : 0);
                range.collapse(true);
                selection.removeAllRanges();
                selection.addRange(range);
            }
        }
    }

    notifyContentChanged(editorElement);
}

/**
 * Set font size on selected text
 * @param {HTMLElement} editorElement - The contenteditable element
 * @param {string} size - The font size (e.g., "14px", "18px")
 */
export function setFontSize(editorElement, size) {
    const selection = window.getSelection();
    if (!selection.rangeCount) {
        editorElement.focus();
        return;
    }

    const range = selection.getRangeAt(0);

    if (!editorElement.contains(range.commonAncestorContainer)) {
        editorElement.focus();
        return;
    }

    if (range.collapsed) {
        // No selection - create span for future typing
        const span = document.createElement('span');
        span.style.fontSize = size;
        span.innerHTML = '&#8203;';
        range.insertNode(span);
        range.setStart(span, 1);
        range.collapse(true);
        selection.removeAllRanges();
        selection.addRange(range);
    } else {
        // Wrap selection in span with font size
        const span = document.createElement('span');
        span.style.fontSize = size;
        try {
            range.surroundContents(span);
        } catch (e) {
            const fragment = range.extractContents();
            span.appendChild(fragment);
            range.insertNode(span);
        }
    }

    notifyContentChanged(editorElement);
}

/**
 * Set font color on selected text
 * @param {HTMLElement} editorElement - The contenteditable element
 * @param {string} color - The color value (e.g., "#FF0000")
 */
export function setFontColor(editorElement, color) {
    const selection = window.getSelection();
    if (!selection.rangeCount) {
        editorElement.focus();
        return;
    }

    const range = selection.getRangeAt(0);

    if (!editorElement.contains(range.commonAncestorContainer)) {
        editorElement.focus();
        return;
    }

    if (range.collapsed) {
        // No selection - create span for future typing
        const span = document.createElement('span');
        span.style.color = color;
        span.innerHTML = '&#8203;';
        range.insertNode(span);
        range.setStart(span, 1);
        range.collapse(true);
        selection.removeAllRanges();
        selection.addRange(range);
    } else {
        // Wrap selection in span with color
        const span = document.createElement('span');
        span.style.color = color;
        try {
            range.surroundContents(span);
        } catch (e) {
            const fragment = range.extractContents();
            span.appendChild(fragment);
            range.insertNode(span);
        }
    }

    notifyContentChanged(editorElement);
}

/**
 * Set text alignment
 * @param {HTMLElement} editorElement - The contenteditable element
 * @param {string} alignment - The alignment (left, center, right, justify)
 */
export function setAlignment(editorElement, alignment) {
    const selection = window.getSelection();
    if (!selection.rangeCount) {
        editorElement.focus();
        return;
    }

    const range = selection.getRangeAt(0);

    if (!editorElement.contains(range.commonAncestorContainer)) {
        editorElement.focus();
        return;
    }

    // Find the block-level parent or create one
    let block = findBlockParent(range.commonAncestorContainer, editorElement);

    if (!block || block === editorElement) {
        // Wrap content in a div
        const div = document.createElement('div');
        div.style.textAlign = alignment;

        if (range.collapsed) {
            div.innerHTML = '&#8203;';
            range.insertNode(div);
            range.setStart(div, 0);
            range.collapse(true);
        } else {
            div.appendChild(range.extractContents());
            range.insertNode(div);
        }

        selection.removeAllRanges();
        selection.addRange(range);
    } else {
        // Apply alignment to existing block
        block.style.textAlign = alignment;
    }

    notifyContentChanged(editorElement);
}

/**
 * Get the HTML content
 * @param {HTMLElement} editorElement - The contenteditable element
 * @returns {string} The HTML content
 */
export function getContent(editorElement) {
    return editorElement ? editorElement.innerHTML : '';
}

/**
 * Set the HTML content
 * @param {HTMLElement} editorElement - The contenteditable element
 * @param {string} html - The HTML content to set
 */
export function setContent(editorElement, html) {
    if (editorElement) {
        editorElement.innerHTML = html || '';
    }
}

/**
 * Focus the editor
 * @param {HTMLElement} editorElement - The contenteditable element
 */
export function focus(editorElement) {
    if (editorElement) {
        editorElement.focus();
        // Move cursor to end
        const selection = window.getSelection();
        const range = document.createRange();
        range.selectNodeContents(editorElement);
        range.collapse(false);
        selection.removeAllRanges();
        selection.addRange(range);
    }
}

/**
 * Dispose and cleanup
 * @param {HTMLElement} editorElement - The contenteditable element
 */
export function dispose(editorElement) {
    if (!editorElement) return;

    const data = editorMap.get(editorElement);
    if (data) {
        editorElement.removeEventListener('input', data.handlers.input);
        editorElement.removeEventListener('keydown', data.handlers.keydown);
        editorElement.removeEventListener('paste', data.handlers.paste);
        editorMap.delete(editorElement);
    }
}

/**
 * Undo the last action
 * @param {HTMLElement} editorElement - The contenteditable element
 * @returns {boolean} True if undo was performed
 */
export function undo(editorElement) {
    const data = editorMap.get(editorElement);
    if (!data || data.historyIndex <= 0) {
        return false;
    }

    data.historyIndex--;
    const state = data.history[data.historyIndex];
    editorElement.innerHTML = state.html;
    restoreSelectionOffsets(editorElement, state.selection);
    // Only notify Blazor, don't save state (would corrupt history)
    notifyBlazor(editorElement);
    return true;
}

/**
 * Redo the last undone action
 * @param {HTMLElement} editorElement - The contenteditable element
 * @returns {boolean} True if redo was performed
 */
export function redo(editorElement) {
    const data = editorMap.get(editorElement);
    if (!data || data.historyIndex >= data.history.length - 1) return false;

    data.historyIndex++;
    const state = data.history[data.historyIndex];
    editorElement.innerHTML = state.html;
    restoreSelectionOffsets(editorElement, state.selection);
    // Only notify Blazor, don't save state (would corrupt history)
    notifyBlazor(editorElement);
    return true;
}

// ===== Helper Functions =====

/**
 * Get the current cursor/selection position as offsets within the editor
 * @param {HTMLElement} editorElement - The contenteditable element
 * @returns {{ start: number, end: number } | null}
 */
function getSelectionOffsets(editorElement) {
    const selection = window.getSelection();
    if (!selection.rangeCount) return null;

    const range = selection.getRangeAt(0);
    if (!editorElement.contains(range.commonAncestorContainer)) return null;

    // Calculate offset from start of editor
    const preRange = document.createRange();
    preRange.selectNodeContents(editorElement);
    preRange.setEnd(range.startContainer, range.startOffset);
    const start = preRange.toString().length;

    preRange.setEnd(range.endContainer, range.endOffset);
    const end = preRange.toString().length;

    return { start, end };
}

/**
 * Restore cursor/selection position from offsets
 * @param {HTMLElement} editorElement - The contenteditable element
 * @param {{ start: number, end: number }} offsets - The selection offsets to restore
 */
function restoreSelectionOffsets(editorElement, offsets) {
    if (!offsets) return;

    const selection = window.getSelection();
    const range = document.createRange();

    // Walk through text nodes to find the position
    let currentOffset = 0;
    let startNode = null, startOffset = 0;
    let endNode = null, endOffset = 0;

    const walker = document.createTreeWalker(
        editorElement,
        NodeFilter.SHOW_TEXT,
        null,
        false
    );

    let node;
    while ((node = walker.nextNode())) {
        const nodeLength = node.textContent.length;

        // Find start position
        if (!startNode && currentOffset + nodeLength >= offsets.start) {
            startNode = node;
            startOffset = offsets.start - currentOffset;
        }

        // Find end position
        if (!endNode && currentOffset + nodeLength >= offsets.end) {
            endNode = node;
            endOffset = offsets.end - currentOffset;
            break;
        }

        currentOffset += nodeLength;
    }

    // If we found valid positions, restore the selection
    if (startNode) {
        try {
            range.setStart(startNode, Math.min(startOffset, startNode.textContent.length));
            if (endNode) {
                range.setEnd(endNode, Math.min(endOffset, endNode.textContent.length));
            } else {
                range.collapse(true);
            }
            selection.removeAllRanges();
            selection.addRange(range);
        } catch (e) {
            // If restoration fails, just focus the editor
            editorElement.focus();
        }
    } else {
        // No text nodes - place cursor at start
        range.selectNodeContents(editorElement);
        range.collapse(true);
        selection.removeAllRanges();
        selection.addRange(range);
    }
}

/**
 * Save current state to history for undo/redo
 * @param {HTMLElement} editorElement - The contenteditable element
 */
function saveState(editorElement) {
    const data = editorMap.get(editorElement);
    if (!data) {
        return;
    }

    const html = editorElement.innerHTML;
    const selection = getSelectionOffsets(editorElement);

    // If we're not at the end of history, truncate forward history (discard redo states)
    if (data.historyIndex < data.history.length - 1) {
        data.history = data.history.slice(0, data.historyIndex + 1);
    }

    // Don't save duplicate consecutive states (compare HTML only)
    if (data.history.length > 0 && data.history[data.historyIndex].html === html) {
        return;
    }

    data.history.push({ html, selection });
    data.historyIndex = data.history.length - 1;

    // Limit history size
    if (data.history.length > data.maxHistory) {
        data.history.shift();
        data.historyIndex--;
    }
}

/**
 * Find parent element with specific tag name
 * @param {Node} node - Starting node
 * @param {string} tagName - Tag name to find (uppercase)
 * @returns {HTMLElement|null}
 */
function getParentWithTag(node, tagName) {
    while (node && node.nodeType !== Node.DOCUMENT_NODE) {
        if (node.nodeType === Node.ELEMENT_NODE && node.tagName === tagName) {
            return node;
        }
        node = node.parentNode;
    }
    return null;
}

/**
 * Find block-level parent element
 * @param {Node} node - Starting node
 * @param {HTMLElement} container - The container to stop at
 * @returns {HTMLElement|null}
 */
function findBlockParent(node, container) {
    const blockTags = ['P', 'DIV', 'H1', 'H2', 'H3', 'H4', 'H5', 'H6', 'LI', 'BLOCKQUOTE'];
    while (node && node !== container) {
        if (node.nodeType === Node.ELEMENT_NODE && blockTags.includes(node.tagName)) {
            return node;
        }
        node = node.parentNode;
    }
    return null;
}

/**
 * Unwrap an element, keeping its contents
 * @param {HTMLElement} element - The element to unwrap
 */
function unwrapElement(element) {
    const parent = element.parentNode;
    while (element.firstChild) {
        parent.insertBefore(element.firstChild, element);
    }
    parent.removeChild(element);
}

/**
 * Unwrap a list, converting items to paragraphs or text
 * @param {HTMLElement} list - The list element (ul/ol)
 */
function unwrapList(list) {
    const parent = list.parentNode;
    const items = Array.from(list.querySelectorAll('li'));

    items.forEach(item => {
        const div = document.createElement('div');
        while (item.firstChild) {
            div.appendChild(item.firstChild);
        }
        parent.insertBefore(div, list);
    });

    parent.removeChild(list);
}

/**
 * Extract lines from a document fragment, splitting by <br>, <div>, <p>, and newlines
 * @param {DocumentFragment} fragment - The fragment containing the selected content
 * @returns {Array} Array of line contents (strings or document fragments)
 */
function extractLinesFromFragment(fragment) {
    const lines = [];
    const temp = document.createElement('div');
    temp.appendChild(fragment.cloneNode(true));

    // Get the HTML and split by line-breaking elements
    let html = temp.innerHTML;

    // Replace line-breaking elements with newline markers
    // Order matters: handle <br> first, then block elements
    html = html.replace(/<br\s*\/?>/gi, '\n');

    // For block elements, add newline BEFORE opening tag (to separate from previous content)
    // and remove closing tags
    html = html.replace(/<div[^>]*>/gi, '\n');
    html = html.replace(/<\/div>/gi, '');
    html = html.replace(/<p[^>]*>/gi, '\n');
    html = html.replace(/<\/p>/gi, '');

    // Split by newlines
    const parts = html.split('\n');

    parts.forEach(part => {
        const trimmed = part.trim();
        if (trimmed) {
            lines.push(trimmed);
        }
    });

    // If no lines found, return the original content as single item
    if (lines.length === 0) {
        lines.push(temp.innerHTML || '&#8203;');
    }

    return lines;
}

/**
 * Insert HTML at current cursor position
 * @param {string} html - The HTML to insert
 */
function insertHtmlAtCursor(html) {
    const selection = window.getSelection();
    if (!selection.rangeCount) return;

    const range = selection.getRangeAt(0);
    range.deleteContents();

    const temp = document.createElement('div');
    temp.innerHTML = html;

    const fragment = document.createDocumentFragment();
    let lastNode;
    while (temp.firstChild) {
        lastNode = fragment.appendChild(temp.firstChild);
    }

    range.insertNode(fragment);

    if (lastNode) {
        range.setStartAfter(lastNode);
        range.collapse(true);
        selection.removeAllRanges();
        selection.addRange(range);
    }
}

/**
 * Sanitize HTML to prevent XSS
 * @param {string} html - The HTML to sanitize
 * @returns {string} Sanitized HTML
 */
function sanitizeHtml(html) {
    const temp = document.createElement('div');
    temp.innerHTML = html;

    // Remove script tags
    const scripts = temp.querySelectorAll('script');
    scripts.forEach(s => s.remove());

    // Remove style tags
    const styles = temp.querySelectorAll('style');
    styles.forEach(s => s.remove());

    // Remove event handlers and dangerous attributes
    const allElements = temp.querySelectorAll('*');
    allElements.forEach(el => {
        // Remove event handlers
        Array.from(el.attributes).forEach(attr => {
            if (attr.name.startsWith('on') || attr.name === 'href' && attr.value.startsWith('javascript:')) {
                el.removeAttribute(attr.name);
            }
        });
        // Remove iframe, embed, object
        if (['IFRAME', 'EMBED', 'OBJECT', 'FORM'].includes(el.tagName)) {
            el.remove();
        }
    });

    return temp.innerHTML;
}

/**
 * Notify Blazor of content changes (without saving state)
 * @param {HTMLElement} editorElement - The contenteditable element
 */
function notifyBlazor(editorElement) {
    const data = editorMap.get(editorElement);
    if (data && data.dotNetRef) {
        const html = editorElement.innerHTML;
        data.dotNetRef.invokeMethodAsync('OnContentChanged', html);
    }
}

/**
 * Notify Blazor of content changes and save state for undo
 * @param {HTMLElement} editorElement - The contenteditable element
 */
function notifyContentChanged(editorElement) {
    notifyBlazor(editorElement);
    saveState(editorElement);
}

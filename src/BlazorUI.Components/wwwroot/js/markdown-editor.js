/**
 * Markdown Editor JavaScript module
 * Handles textarea text selection, cursor positioning, text insertion, and undo/redo
 */

// Store references for editor data (history, dotNetRef, etc.)
const editorMap = new WeakMap();

/**
 * Scroll textarea to ensure cursor is visible
 * @param {HTMLTextAreaElement} textarea - The textarea element
 */
function scrollToCursor(textarea) {
    if (!textarea) return;

    const cursorPos = textarea.selectionEnd;
    const value = textarea.value;

    // Count lines up to cursor position
    const textUpToCursor = value.substring(0, cursorPos);
    const lines = textUpToCursor.split('\n');
    const lineNumber = lines.length;

    // Get computed styles for accurate line height calculation
    const style = window.getComputedStyle(textarea);
    const lineHeight = parseFloat(style.lineHeight) || parseFloat(style.fontSize) * 1.2;
    const paddingTop = parseFloat(style.paddingTop) || 0;
    const paddingBottom = parseFloat(style.paddingBottom) || 0;

    // Calculate cursor's vertical position
    const cursorTop = paddingTop + (lineNumber - 1) * lineHeight;
    const cursorBottom = cursorTop + lineHeight;

    // Get visible area
    const visibleTop = textarea.scrollTop;
    const visibleBottom = visibleTop + textarea.clientHeight - paddingBottom;

    // Scroll if cursor is outside visible area
    if (cursorBottom > visibleBottom) {
        // Cursor is below visible area - scroll down
        textarea.scrollTop = cursorBottom - textarea.clientHeight + paddingBottom + lineHeight;
    } else if (cursorTop < visibleTop) {
        // Cursor is above visible area - scroll up
        textarea.scrollTop = cursorTop - paddingTop;
    }
}

/**
 * Save current state to history for undo/redo
 * @param {HTMLTextAreaElement} textarea - The textarea element
 */
function saveState(textarea) {
    const data = editorMap.get(textarea);
    if (!data) return;

    const value = textarea.value;
    const selection = {
        start: textarea.selectionStart,
        end: textarea.selectionEnd
    };

    // If we're not at the end of history, truncate forward history (discard redo states)
    if (data.historyIndex < data.history.length - 1) {
        data.history = data.history.slice(0, data.historyIndex + 1);
    }

    // Don't save duplicate consecutive states (compare value only)
    if (data.history.length > 0 && data.history[data.historyIndex].value === value) {
        return;
    }

    data.history.push({ value, selection });
    data.historyIndex = data.history.length - 1;

    // Limit history size
    if (data.history.length > data.maxHistory) {
        data.history.shift();
        data.historyIndex--;
    }
}

/**
 * Notify Blazor of content changes (without saving state - for undo/redo)
 * @param {HTMLTextAreaElement} textarea - The textarea element
 */
function notifyBlazor(textarea) {
    const data = editorMap.get(textarea);
    if (data && data.dotNetRef) {
        data.dotNetRef.invokeMethodAsync('OnContentChanged', textarea.value);
    }
}

/**
 * Notify Blazor of content changes and save state for undo
 * @param {HTMLTextAreaElement} textarea - The textarea element
 */
function notifyContentChanged(textarea) {
    notifyBlazor(textarea);
    saveState(textarea);
}

/**
 * Undo the last action
 * @param {HTMLTextAreaElement} textarea - The textarea element
 * @returns {boolean} True if undo was performed
 */
export function undo(textarea) {
    const data = editorMap.get(textarea);
    if (!data || data.historyIndex <= 0) {
        return false;
    }

    data.historyIndex--;
    const state = data.history[data.historyIndex];
    textarea.value = state.value;
    textarea.setSelectionRange(state.selection.start, state.selection.end);
    // Only notify Blazor, don't save state (would corrupt history)
    notifyBlazor(textarea);
    return true;
}

/**
 * Redo the last undone action
 * @param {HTMLTextAreaElement} textarea - The textarea element
 * @returns {boolean} True if redo was performed
 */
export function redo(textarea) {
    const data = editorMap.get(textarea);
    if (!data || data.historyIndex >= data.history.length - 1) {
        return false;
    }

    data.historyIndex++;
    const state = data.history[data.historyIndex];
    textarea.value = state.value;
    textarea.setSelectionRange(state.selection.start, state.selection.end);
    // Only notify Blazor, don't save state (would corrupt history)
    notifyBlazor(textarea);
    return true;
}

/**
 * Get current selection information from textarea
 * @param {HTMLTextAreaElement} textarea - The textarea element
 * @returns {object} Selection info including start, end, and selected text
 */
export function getSelection(textarea) {
    return {
        selectionStart: textarea.selectionStart,
        selectionEnd: textarea.selectionEnd,
        selectedText: textarea.value.substring(textarea.selectionStart, textarea.selectionEnd)
    };
}

/**
 * Insert formatting around selected text or at cursor position
 * @param {HTMLTextAreaElement} textarea - The textarea element
 * @param {string} prefix - Text to insert before selection
 * @param {string} suffix - Text to insert after selection
 * @param {string} defaultText - Default text if nothing is selected
 * @returns {string} The new textarea value
 */
export function insertFormatting(textarea, prefix, suffix, defaultText) {
    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const selectedText = textarea.value.substring(start, end);
    const textToWrap = selectedText || defaultText;

    const before = textarea.value.substring(0, start);
    const after = textarea.value.substring(end);

    textarea.value = before + prefix + textToWrap + suffix + after;

    // Position cursor after the inserted text
    const newCursorPos = start + prefix.length + textToWrap.length + suffix.length;
    textarea.setSelectionRange(newCursorPos, newCursorPos);
    textarea.focus();

    return textarea.value;
}

/**
 * Insert text at the start of the current line(s)
 * Handles multi-line selections by prefixing each line
 * @param {HTMLTextAreaElement} textarea - The textarea element
 * @param {string} prefix - Text to insert at line start
 * @returns {string} The new textarea value
 */
export function insertLinePrefix(textarea, prefix) {
    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const value = textarea.value;

    // Find the start of the first selected line
    let lineStart = value.lastIndexOf('\n', start - 1) + 1;
    // Find the end of the last selected line
    let lineEnd = value.indexOf('\n', end);
    if (lineEnd === -1) lineEnd = value.length;

    // Get all text from line start to line end
    const selectedLinesText = value.substring(lineStart, lineEnd);
    const lines = selectedLinesText.split('\n');

    // Check if this is a single line operation
    if (lines.length === 1) {
        const currentLine = lines[0];

        // Handle heading replacement - remove existing heading prefix first
        const headingMatch = currentLine.match(/^#{1,6}\s*/);
        if (headingMatch && prefix.startsWith('#')) {
            const before = value.substring(0, lineStart);
            const after = value.substring(lineStart + headingMatch[0].length);
            textarea.value = before + prefix + after;
            const newCursorPos = lineStart + prefix.length;
            textarea.setSelectionRange(newCursorPos, newCursorPos);
            textarea.focus();
            return textarea.value;
        }

        // Check if this is a list prefix being toggled
        const listMatch = currentLine.match(/^(\d+\.\s|-\s)/);
        if (listMatch && (prefix === '- ' || prefix.match(/^\d+\.\s/))) {
            const before = value.substring(0, lineStart);
            const after = value.substring(lineStart + listMatch[0].length);
            // If same type, just remove (toggle off)
            if ((prefix === '- ' && listMatch[1] === '- ') ||
                (prefix.match(/^\d+\.\s/) && listMatch[1].match(/^\d+\.\s/))) {
                textarea.value = before + after;
                textarea.setSelectionRange(lineStart, lineStart);
                textarea.focus();
                return textarea.value;
            }
            // Different type, replace
            textarea.value = before + prefix + after;
            const newCursorPos = lineStart + prefix.length;
            textarea.setSelectionRange(newCursorPos, newCursorPos);
            textarea.focus();
            return textarea.value;
        }

        // Insert prefix at line start
        const before = value.substring(0, lineStart);
        const after = value.substring(lineStart);
        textarea.value = before + prefix + after;
        const newCursorPos = start + prefix.length;
        textarea.setSelectionRange(newCursorPos, end + prefix.length);
        textarea.focus();
        return textarea.value;
    }

    // Multi-line selection: apply prefix to each line
    const isNumberedList = prefix.match(/^\d+\.\s/);
    const isBulletList = prefix === '- ';

    const newLines = lines.map((line, index) => {
        // Check if line already has list prefix
        const existingBullet = line.match(/^-\s/);
        const existingNumber = line.match(/^\d+\.\s/);

        if (isBulletList) {
            if (existingBullet) {
                // Toggle off - remove bullet
                return line.substring(existingBullet[0].length);
            } else if (existingNumber) {
                // Replace numbered with bullet
                return '- ' + line.substring(existingNumber[0].length);
            } else {
                // Add bullet
                return '- ' + line;
            }
        } else if (isNumberedList) {
            if (existingNumber) {
                // Toggle off - remove number
                return line.substring(existingNumber[0].length);
            } else if (existingBullet) {
                // Replace bullet with number
                return `${index + 1}. ` + line.substring(existingBullet[0].length);
            } else {
                // Add number
                return `${index + 1}. ` + line;
            }
        } else {
            // For other prefixes (headings), just add prefix
            return prefix + line;
        }
    });

    const newText = newLines.join('\n');
    const before = value.substring(0, lineStart);
    const after = value.substring(lineEnd);

    textarea.value = before + newText + after;

    // Select the modified text
    const newEnd = lineStart + newText.length;
    textarea.setSelectionRange(lineStart, newEnd);
    textarea.focus();

    return textarea.value;
}

/**
 * Remove heading prefix from current line
 * @param {HTMLTextAreaElement} textarea - The textarea element
 * @returns {string} The new textarea value
 */
export function removeLinePrefix(textarea) {
    const start = textarea.selectionStart;
    const value = textarea.value;

    // Find the start of the current line
    let lineStart = value.lastIndexOf('\n', start - 1) + 1;
    const lineEnd = value.indexOf('\n', lineStart);
    const currentLine = value.substring(lineStart, lineEnd === -1 ? value.length : lineEnd);

    // Check for heading prefix
    const headingMatch = currentLine.match(/^#{1,6}\s*/);
    if (headingMatch) {
        const before = value.substring(0, lineStart);
        const after = value.substring(lineStart + headingMatch[0].length);
        textarea.value = before + after;
        const newCursorPos = Math.max(lineStart, start - headingMatch[0].length);
        textarea.setSelectionRange(newCursorPos, newCursorPos);
        textarea.focus();
        return textarea.value;
    }

    // Check for list prefix
    const listMatch = currentLine.match(/^(\d+\.\s|-\s)/);
    if (listMatch) {
        const before = value.substring(0, lineStart);
        const after = value.substring(lineStart + listMatch[0].length);
        textarea.value = before + after;
        const newCursorPos = Math.max(lineStart, start - listMatch[0].length);
        textarea.setSelectionRange(newCursorPos, newCursorPos);
        textarea.focus();
        return textarea.value;
    }

    return textarea.value;
}

/**
 * Focus the textarea element
 * @param {HTMLTextAreaElement} textarea - The textarea element
 */
export function focusTextarea(textarea) {
    if (textarea) {
        textarea.focus();
    }
}

// Store references for cleanup
const listenerMap = new WeakMap();
const inputListenerMap = new WeakMap();

/**
 * Initialize list continuation behavior and undo/redo on textarea
 * @param {HTMLTextAreaElement} textarea - The textarea element
 * @param {DotNetObjectReference} dotNetRef - Reference to Blazor component
 */
export function initializeListContinuation(textarea, dotNetRef) {
    if (!textarea || editorMap.has(textarea)) return;

    // Initialize editor data for history/undo
    const data = {
        dotNetRef,
        history: [],
        historyIndex: -1,
        maxHistory: 100
    };
    editorMap.set(textarea, data);

    // Save initial state
    saveState(textarea);

    const handler = (e) => {
        // Intercept Ctrl+Z/Y for undo/redo (prevent browser's native undo)
        if (e.ctrlKey || e.metaKey) {
            if (e.key === 'z' || e.key === 'Z') {
                e.preventDefault();
                if (e.shiftKey) {
                    redo(textarea);
                } else {
                    undo(textarea);
                }
                return;
            } else if (e.key === 'y' || e.key === 'Y') {
                e.preventDefault();
                redo(textarea);
                return;
            }
        }

        // Only handle Enter without Shift
        if (e.key !== 'Enter' || e.shiftKey) return;

        const start = textarea.selectionStart;
        const value = textarea.value;

        // Find the current line
        let lineStart = value.lastIndexOf('\n', start - 1) + 1;
        const currentLine = value.substring(lineStart, start);

        // Check for bullet list: "- " or "* "
        const bulletMatch = currentLine.match(/^(\s*)([-*])\s+(.*)$/);
        if (bulletMatch) {
            e.preventDefault();
            const indent = bulletMatch[1];
            const marker = bulletMatch[2];
            const content = bulletMatch[3];

            // If line is empty (just the marker), remove the marker and exit list
            if (content.trim() === '') {
                const before = value.substring(0, lineStart);
                const after = value.substring(start);
                textarea.value = before + after;
                textarea.setSelectionRange(lineStart, lineStart);
            } else {
                // Continue the list
                const before = value.substring(0, start);
                const after = value.substring(start);
                const newLine = `\n${indent}${marker} `;
                textarea.value = before + newLine + after;
                const newPos = start + newLine.length;
                textarea.setSelectionRange(newPos, newPos);
            }

            // Scroll to cursor and notify Blazor of the change
            scrollToCursor(textarea);
            textarea.dispatchEvent(new Event('input', { bubbles: true }));
            return;
        }

        // Check for numbered list: "1. ", "2. ", etc.
        const numberedMatch = currentLine.match(/^(\s*)(\d+)\.\s+(.*)$/);
        if (numberedMatch) {
            e.preventDefault();
            const indent = numberedMatch[1];
            const number = parseInt(numberedMatch[2], 10);
            const content = numberedMatch[3];

            // If line is empty (just the number), remove the marker and exit list
            if (content.trim() === '') {
                const before = value.substring(0, lineStart);
                const after = value.substring(start);
                textarea.value = before + after;
                textarea.setSelectionRange(lineStart, lineStart);
            } else {
                // Continue the list with incremented number
                const before = value.substring(0, start);
                const after = value.substring(start);
                const newLine = `\n${indent}${number + 1}. `;
                textarea.value = before + newLine + after;
                const newPos = start + newLine.length;
                textarea.setSelectionRange(newPos, newPos);
            }

            // Scroll to cursor and notify Blazor of the change
            scrollToCursor(textarea);
            textarea.dispatchEvent(new Event('input', { bubbles: true }));
            return;
        }

        // Not a list line, let default Enter behavior happen
    };

    textarea.addEventListener('keydown', handler);
    listenerMap.set(textarea, handler);

    // Add input listener for auto-scroll and state saving on typing
    const inputHandler = () => {
        // Use requestAnimationFrame to ensure scroll happens after DOM update
        requestAnimationFrame(() => {
            scrollToCursor(textarea);
            // Save state for undo after each input
            saveState(textarea);
        });
    };
    textarea.addEventListener('input', inputHandler);
    inputListenerMap.set(textarea, inputHandler);
}

/**
 * Cleanup list continuation listener and editor data
 * @param {HTMLTextAreaElement} textarea - The textarea element
 */
export function disposeListContinuation(textarea) {
    if (!textarea) return;
    const handler = listenerMap.get(textarea);
    if (handler) {
        textarea.removeEventListener('keydown', handler);
        listenerMap.delete(textarea);
    }
    const inputHandler = inputListenerMap.get(textarea);
    if (inputHandler) {
        textarea.removeEventListener('input', inputHandler);
        inputListenerMap.delete(textarea);
    }
    // Clean up editor data (history, dotNetRef)
    editorMap.delete(textarea);
}

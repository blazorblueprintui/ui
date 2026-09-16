/**
 * File Upload JavaScript interop module.
 * Handles drag-and-drop file transfers to Blazor's InputFile component.
 */

/**
 * Resets the value of a file input element.
 * @param {HTMLInputElement} inputFileElement - The InputFile element to reset.
 */
export function resetInput(inputFileElement) {
    if (inputFileElement) {
        inputFileElement.value = '';
    }
}

/**
 * Initializes the drop zone for file uploads.
 * @param {HTMLElement} dropZoneElement - The drop zone container element.
 * @param {HTMLInputElement} inputFileElement - The InputFile element.
 * @returns {Object} Cleanup object with dispose method.
 */
export function initializeDropZone(dropZoneElement, inputFileElement) {
    if (!dropZoneElement || !inputFileElement) {
        console.warn('FileUpload: Missing dropZone or inputFile element');
        return { dispose: () => {} };
    }

    const handleDragOver = (e) => {
        e.preventDefault();
        e.stopPropagation();
    };

    const handleDrop = (e) => {
        e.preventDefault();
        e.stopPropagation();

        if (!inputFileElement.disabled && e.dataTransfer?.files?.length > 0) {
            // Transfer files to the InputFile element
            inputFileElement.files = e.dataTransfer.files;

            // Dispatch change event to trigger Blazor's InputFile handler
            const event = new Event('change', { bubbles: true });
            inputFileElement.dispatchEvent(event);
        }
    };

    // Lock the input synchronously: a second selection on this same InputFile would replace
    // Blazor's browser-file map before the server has rendered the next input.
    const lockSelection = () => { inputFileElement.disabled = true; };
    inputFileElement.addEventListener('change', lockSelection, { capture: true });

    // Add event listeners
    dropZoneElement.addEventListener('dragover', handleDragOver);
    dropZoneElement.addEventListener('drop', handleDrop);

    // Return cleanup object
    return {
        dispose: () => {
            dropZoneElement.removeEventListener('dragover', handleDragOver);
            dropZoneElement.removeEventListener('drop', handleDrop);
            inputFileElement.removeEventListener('change', lockSelection, { capture: true });
        }
    };
}

/**
 * Lets the user paste files into the upload, as an alternative to drag-and-drop (#485).
 *
 * The listener is on `document`, not on the drop zone. A `paste` event fires at the focused
 * element only when that element is editable; a file input is not, so a listener on the zone
 * itself would never see one. Scoping is done by checking that focus is inside the zone, which
 * means a paste elsewhere on the page is ignored and two uploads on one page cannot both claim
 * the same paste.
 *
 * Pasted text is ignored — `clipboardData.files` is empty for it, so nothing happens and the
 * paste is left alone for whatever else might want it.
 *
 * Files are handed over the same way `initializeDropZone` hands over a drop: assigned to the
 * input and followed by a `change` event, so validation, limits and callbacks all run through
 * the one existing path rather than a second copy of them.
 *
 * @param {HTMLElement} dropZoneElement - The drop zone container element.
 * @param {HTMLInputElement} inputFileElement - The InputFile element.
 * @returns {Object} Cleanup object with dispose method.
 */
export function initializePaste(dropZoneElement, inputFileElement) {
    if (!dropZoneElement || !inputFileElement) {
        console.warn('FileUpload: Missing dropZone or inputFile element for paste');
        return { dispose: () => {} };
    }

    const handlePaste = (e) => {
        if (inputFileElement.disabled) {
            return;
        }

        // Only act when focus is inside this upload, so a paste anywhere else is left alone.
        if (!dropZoneElement.contains(document.activeElement)) {
            return;
        }

        const files = e.clipboardData?.files;
        if (!files || files.length === 0) {
            return;
        }

        e.preventDefault();

        // A file input only accepts a FileList, and multiple="false" must still receive one file.
        const transfer = new DataTransfer();
        const limit = inputFileElement.multiple ? files.length : 1;
        for (let i = 0; i < limit; i++) {
            transfer.items.add(files[i]);
        }

        inputFileElement.files = transfer.files;
        inputFileElement.dispatchEvent(new Event('change', { bubbles: true }));
    };

    document.addEventListener('paste', handlePaste);

    return {
        dispose: () => {
            document.removeEventListener('paste', handlePaste);
        }
    };
}

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

        if (e.dataTransfer?.files?.length > 0) {
            // Transfer files to the InputFile element
            inputFileElement.files = e.dataTransfer.files;

            // Dispatch change event to trigger Blazor's InputFile handler
            const event = new Event('change', { bubbles: true });
            inputFileElement.dispatchEvent(event);
        }
    };

    // Add event listeners
    dropZoneElement.addEventListener('dragover', handleDragOver);
    dropZoneElement.addEventListener('drop', handleDrop);

    // Return cleanup object
    return {
        dispose: () => {
            dropZoneElement.removeEventListener('dragover', handleDragOver);
            dropZoneElement.removeEventListener('drop', handleDrop);
        }
    };
}

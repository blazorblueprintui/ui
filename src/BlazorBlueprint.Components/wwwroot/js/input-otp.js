/**
 * InputOTP JavaScript interop module.
 * Reads clipboard data from paste events for .NET 8 compatibility,
 * since ClipboardEventArgs does not expose the pasted text in C#.
 */

const instances = new Map();

/**
 * Attaches a paste event handler to the OTP container element.
 * @param {HTMLElement} element - The OTP container element.
 * @param {DotNetObject} dotNetRef - Reference to the Blazor component.
 * @param {string} instanceId - Unique ID for this instance.
 */
export function initialize(element, dotNetRef, instanceId) {
    if (!element || !dotNetRef) return;

    const handlePaste = (event) => {
        const text = event.clipboardData?.getData('text') ?? '';
        event.preventDefault();
        dotNetRef.invokeMethodAsync('OnPasteJs', text).catch(err => {
            console.error('Error invoking OnPasteJs:', err);
        });
    };

    element.addEventListener('paste', handlePaste);
    instances.set(instanceId, { element, handlePaste });
}

/**
 * Removes the paste event handler and cleans up.
 * @param {string} instanceId - The instance to dispose.
 */
export function dispose(instanceId) {
    const instance = instances.get(instanceId);
    if (!instance) return;
    instance.element.removeEventListener('paste', instance.handlePaste);
    instances.delete(instanceId);
}

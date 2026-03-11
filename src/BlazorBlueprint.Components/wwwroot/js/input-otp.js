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
 * @param {string} otpMode - csharp enum InputOTPInputMode (Numbers, Letters, LettersAndNumbers).
 */
export function initialize(element, dotNetRef, instanceId, otpMode) {
    if (!element || !dotNetRef) return;

    const handlePaste = (event) => {
        const text = event.clipboardData?.getData('text') ?? '';
        event.preventDefault();
        dotNetRef.invokeMethodAsync('OnPasteJs', text).catch(err => {
            console.error('Error invoking OnPasteJs:', err);
        });
    };

    element.addEventListener('paste', handlePaste);

    const handleKeyDown = (event) => {
        const currentMode = instances.get(instanceId)?.otpMode;

        if (_isControlKey(event.key)) {
            return;
        } else if (event.isComposing) {
            return;
        } else if (event.ctrlKey || event.metaKey) {
            return;
        } else if (currentMode && !_isCharAllowed(event.key, currentMode)) {
            event.preventDefault();
        }
    };

    element.addEventListener('keydown', handleKeyDown);

    instances.set(instanceId, { element, handlePaste, handleKeyDown, otpMode });
}

/**
 * Updates the allowed character mode for an existing OTP instance.
 * @param {string} instanceId - The instance to update.
 * @param {string} otpMode - The new InputOTPInputMode value.
 */
export function updateOtpMode(instanceId, otpMode) {
    const instance = instances.get(instanceId);
    if (!instance) return;
    instance.otpMode = otpMode;
}

function _isControlKey(key) {
    return key.length !== 1;
}

function _isCharAllowed(char, otpMode) {
    if (otpMode === 'Numbers') {
        return /\d/.test(char);
    } else if (otpMode === 'Letters') {
        return /[a-zA-Z]/.test(char);
    } else if (otpMode === 'LettersAndNumbers') {
        return /[a-zA-Z0-9]/.test(char);
    }
    return true;
}

/**
 * Removes the paste event handler and cleans up.
 * @param {string} instanceId - The instance to dispose.
 */
export function dispose(instanceId) {
    const instance = instances.get(instanceId);
    if (!instance) return;
    instance.element.removeEventListener('paste', instance.handlePaste);
    instance.element.removeEventListener('keydown', instance.handleKeyDown);
    instances.delete(instanceId);
}

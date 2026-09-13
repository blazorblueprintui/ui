/**
 * Clipboard interop for BbCopyText.
 *
 * The whole file turns on one browser rule: a clipboard write needs **transient user activation**,
 * and that activation is spent by awaiting. Resolve the text first and write second, and the
 * write is refused — Safari most strictly. So the async path never awaits before writing; it
 * hands the *promise* to ClipboardItem and lets the browser hold the activation across it.
 */

/** Outcome codes, mirrored by CopyTextFailure on the C# side. */
const OK = 'ok';
const REFUSED = 'refused';
const NO_VALUE = 'noValue';
const UNSUPPORTED = 'unsupported';

/**
 * Whether this browser can take a promise inside a ClipboardItem.
 *
 * Feature detection only goes so far here: a browser can expose ClipboardItem and
 * clipboard.write yet still reject a promise value. That case surfaces as a refusal at write
 * time rather than being predicted, which is why the caller is told about failures.
 */
function supportsPromiseWrite() {
  return typeof ClipboardItem !== 'undefined'
    && typeof navigator?.clipboard?.write === 'function';
}

/**
 * Copies text that is already in hand.
 * @param {string} text
 * @returns {Promise<string>} One of the outcome codes above.
 */
export async function copyToClipboard(text) {
  if (typeof text !== 'string' || text.length === 0) {
    return NO_VALUE;
  }

  try {
    await navigator.clipboard.writeText(text);
    return OK;
  } catch {
    // Only for a page served over http, where the Clipboard API is absent entirely.
    // execCommand cannot rescue an expired activation, so using it as the catch-all for every
    // failure is what made this class of bug invisible.
    if (window.isSecureContext) {
      return REFUSED;
    }

    return copyViaExecCommand(text) ? OK : REFUSED;
  }
}

/**
 * Copies text that only a .NET callback can produce, without spending the user activation.
 *
 * `resolve()` is invoked **inside** the ClipboardItem rather than before the write, so the
 * browser keeps the activation alive for however long the callback takes.
 *
 * @param {object} dotNetRef - Reference to the BbCopyText instance.
 * @param {string} methodName - JSInvokable method returning the text.
 * @returns {Promise<string>} One of the outcome codes above.
 */
export async function copyFromAsyncSource(dotNetRef, methodName) {
  if (!dotNetRef || !methodName) {
    return NO_VALUE;
  }

  if (!supportsPromiseWrite()) {
    // No promise-aware path here. Resolving first spends the activation, so this can still be
    // refused — which is exactly why the result is reported rather than swallowed.
    let text;
    try {
      text = await dotNetRef.invokeMethodAsync(methodName);
    } catch {
      return NO_VALUE;
    }
    return copyToClipboard(text);
  }

  // Not awaited: the promise itself is the value, which is the entire point.
  let resolvedEmpty = false;
  const blobPromise = dotNetRef.invokeMethodAsync(methodName).then((text) => {
    if (typeof text !== 'string' || text.length === 0) {
      resolvedEmpty = true;
      // Rejecting here aborts the write rather than putting an empty string on the clipboard.
      throw new Error('BbCopyText: no value to copy');
    }
    return new Blob([text], { type: 'text/plain' });
  });

  try {
    await navigator.clipboard.write([new ClipboardItem({ 'text/plain': blobPromise })]);
    return OK;
  } catch {
    return resolvedEmpty ? NO_VALUE : REFUSED;
  }
}

/**
 * Last resort for an insecure context, where navigator.clipboard does not exist at all.
 * @param {string} text
 * @returns {boolean}
 */
function copyViaExecCommand(text) {
  const textarea = document.createElement('textarea');
  textarea.value = text;
  textarea.style.position = 'fixed';
  textarea.style.opacity = '0';
  document.body.appendChild(textarea);
  textarea.select();

  try {
    return document.execCommand('copy');
  } catch {
    return false;
  } finally {
    document.body.removeChild(textarea);
  }
}

export { UNSUPPORTED };

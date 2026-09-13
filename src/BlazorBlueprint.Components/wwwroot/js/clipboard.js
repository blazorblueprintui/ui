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

  // Not awaited: the promise itself is the value, which is the entire point on browsers that
  // accept it. The invocation is kept so the fallback below can reuse its result rather than
  // asking .NET twice — which for a consumer doing a round trip would mean two round trips.
  const valuePromise = dotNetRef.invokeMethodAsync(methodName);

  const blobPromise = valuePromise.then((text) => {
    if (typeof text !== 'string' || text.length === 0) {
      // Rejecting aborts the write rather than putting an empty string on the clipboard.
      throw new Error('BbCopyText: no value to copy');
    }
    return new Blob([text], { type: 'text/plain' });
  });

  try {
    await navigator.clipboard.write([new ClipboardItem({ 'text/plain': blobPromise })]);
    return OK;
  } catch {
    // Safari exposes ClipboardItem and clipboard.write but refuses a promise value here —
    // measured, not assumed. Falling back to resolve-then-write is what it does accept, and a
    // fast callback succeeds. A genuinely slow one may still be refused once the activation has
    // gone, which is reported rather than hidden.
    let text;
    try {
      text = await valuePromise;
    } catch {
      return NO_VALUE;
    }

    if (typeof text !== 'string' || text.length === 0) {
      return NO_VALUE;
    }

    try {
      await navigator.clipboard.writeText(text);
      return OK;
    } catch {
      return REFUSED;
    }
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

/**
 * Owns the copy gesture for a BbCopyText element.
 *
 * This exists because Safari refuses a clipboard write that is not made inside the user gesture,
 * and a Blazor click is not: it travels C# -> SignalR -> JS, and by the time JS runs the gesture
 * window has closed. A short write still slips through, which is why a literal Value copies fine
 * there, but a value that takes any real time does not — measured, twice, before this approach.
 *
 * Listening on the element puts the write back inside the gesture. The value is produced by
 * calling .NET from inside the ClipboardItem, so the callback can take as long as it likes.
 *
 * @param {HTMLElement} element - The BbCopyText root.
 * @param {object} dotNetRef - Reference to the component.
 * @returns {Object} Cleanup object with a dispose method.
 */
export function initializeCopy(element, dotNetRef) {
  if (!element || !dotNetRef) {
    return { dispose: () => {} };
  }

  const report = (outcome) => {
    dotNetRef.invokeMethodAsync('HandleCopyOutcome', outcome).catch(() => {});
  };

  const copy = () => {
    // Deliberately not async, and clipboard.write is called before anything is awaited. Awaiting
    // first is the whole bug.
    if (!supportsPromiseWrite()) {
      dotNetRef.invokeMethodAsync('ResolveCopyValue')
        .then((text) => copyToClipboard(text))
        .then(report)
        .catch(() => report(REFUSED));
      return;
    }

    const valuePromise = dotNetRef.invokeMethodAsync('ResolveCopyValue');

    const blobPromise = valuePromise.then((text) => {
      if (typeof text !== 'string' || text.length === 0) {
        throw new Error('BbCopyText: no value to copy');
      }
      return new Blob([text], { type: 'text/plain' });
    });

    navigator.clipboard
      .write([new ClipboardItem({ 'text/plain': blobPromise })])
      .then(() => report(OK))
      .catch(() => {
        // Distinguish "nothing to copy" from a genuine refusal, without asking .NET twice.
        valuePromise
          .then((text) => report(typeof text === 'string' && text.length > 0 ? REFUSED : NO_VALUE))
          .catch(() => report(NO_VALUE));
      });
  };

  const handleClick = () => copy();

  const handleKeyDown = (e) => {
    // A span with role="button" gets no native click from these, so they are wired explicitly.
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      copy();
    }
  };

  element.addEventListener('click', handleClick);
  element.addEventListener('keydown', handleKeyDown);

  return {
    dispose: () => {
      element.removeEventListener('click', handleClick);
      element.removeEventListener('keydown', handleKeyDown);
    }
  };
}

/**
 * EventSource JavaScript interop module.
 * Wraps the browser EventSource API for Server-Sent Events consumption.
 */

/**
 * Opens an EventSource connection and wires callbacks to a .NET object reference.
 * @param {string} url - The SSE endpoint URL.
 * @param {object} dotNetRef - DotNetObjectReference for invoking C# methods.
 * @param {string[]} eventTypes - Named event types to listen for.
 * @returns {object} Handle with dispose() to close the connection.
 */
export function connect(url, dotNetRef, eventTypes) {
  const source = new EventSource(url);

  source.onopen = () => {
    dotNetRef.invokeMethodAsync('OnJsOpen');
  };

  source.onerror = () => {
    const state = source.readyState === EventSource.CLOSED ? 'closed' : 'reconnecting';
    dotNetRef.invokeMethodAsync('OnJsError', state);
  };

  source.onmessage = (e) => {
    dotNetRef.invokeMethodAsync('OnJsDefaultMessage', e.data);
  };

  for (const eventType of eventTypes) {
    source.addEventListener(eventType, (e) => {
      dotNetRef.invokeMethodAsync('OnJsMessage', eventType, e.lastEventId, e.data);
    });
  }

  return {
    dispose: () => {
      source.close();
    }
  };
}

// Stack-based escape key detection for every overlay that has to watch Escape at the document.
//
// One document-level listener, one stack, and only the topmost overlay handles the key. Anything
// that registers its own document listener instead is invisible to this ordering, and Escape then
// dismisses every open overlay at once rather than peeling them one at a time — which is what a
// popover inside a dialog used to do.
//
// Overlays whose content holds focus (Select, menus) do not belong here. They handle Escape on
// their own container and stop it propagating, which keeps them off the stack while still taking
// precedence over whatever is underneath.

const stack = [];
let listening = false;

function handleKeyDown(e) {
  if (e.key === 'Escape' && stack.length > 0) {
    const top = stack[stack.length - 1];
    top.dotNetRef.invokeMethodAsync(top.methodName).catch(() => {});
  }
}

export function initialize(dotNetRef, instanceId, methodName = 'JsOnEscapeKey') {
  if (!dotNetRef) {
    return;
  }

  // Reopening must not leave the old entry behind, or the stack grows and the top stops being
  // the overlay actually on screen.
  dispose(instanceId);

  stack.push({ dotNetRef, instanceId, methodName });

  if (!listening) {
    document.addEventListener('keydown', handleKeyDown);
    listening = true;
  }
}

export function dispose(instanceId) {
  const index = stack.findIndex(s => s.instanceId === instanceId);
  if (index !== -1) {
    stack.splice(index, 1);
  }

  if (stack.length === 0 && listening) {
    document.removeEventListener('keydown', handleKeyDown);
    listening = false;
  }
}

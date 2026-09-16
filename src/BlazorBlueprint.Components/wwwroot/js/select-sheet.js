import * as select from '../../BlazorBlueprint.Primitives/js/primitives/select.js';
import { inheritTheme } from '../../BlazorBlueprint.Primitives/js/theme-scope.js';
const cleanups = new WeakMap();
export function initialize(listbox, reference, value, trigger) {
  dispose(listbox);
  cleanups.set(listbox, inheritTheme(trigger, listbox.closest('[role="dialog"]')));
  select.openListbox(listbox.id, value, true, reference);
  select.focusListbox(listbox.id);
}
export function dispose(listbox, trigger) {
  if (!listbox) return;
  select.cleanupKeyboardNavigation(listbox.id);
  cleanups.get(listbox)?.();
  cleanups.delete(listbox);
  trigger?.focus({ preventScroll: true });
}

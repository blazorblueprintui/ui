// Modules are shared by JsModules; this function owns no listeners or disposable state.
export function focusEditor(container, restoreId) {
    if (!container) return;
    if (restoreId) {
        const trigger = document.getElementById(restoreId);
        if (trigger && container.contains(trigger)) trigger.focus();
        return;
    }
    const editor = container.querySelector('[data-bb-cell-editor]');
    editor?.querySelector('input:not([disabled]), textarea:not([disabled]), select:not([disabled]), button:not([disabled]), [tabindex="0"]')?.focus();
}

/** Validate first, then notify the source before the target. This preserves shared-item handoffs. */
export async function performDrop(source, target, oldIndex, newIndex, targetId, isClone) {
  await Promise.resolve(); // Let Sortable finish restoring the source DOM first.
  if (source) {
    const allowed = await source.invokeMethodAsync('CanDropJS', oldIndex, newIndex, targetId, isClone);
    if (!allowed) return false;
    await source.invokeMethodAsync('OnRemoveJS', oldIndex, newIndex);
  }
  await target.invokeMethodAsync('OnAddJS', oldIndex, newIndex);
  return true;
}

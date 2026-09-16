/** Carries a local theme across a portal boundary without taking ownership of global theming. */
const names = [
  '--background', '--foreground', '--card', '--card-foreground', '--popover', '--popover-foreground',
  '--primary', '--primary-foreground', '--secondary', '--secondary-foreground', '--muted', '--muted-foreground',
  '--accent', '--accent-foreground', '--destructive', '--destructive-foreground', '--border', '--input', '--ring',
  '--radius', '--font-sans', '--font-mono', '--bb-spacing', '--bb-card-shadow', '--bb-menu-shadow', '--bb-menu-blur',
  '--bb-menu-opacity', '--bb-menu-background', '--bb-menu-foreground', '--bb-menu-accent', '--bb-menu-accent-foreground'
];
export function inheritTheme(reference, floating) {
  const scope = reference?.closest?.('[data-bb-theme-scope]');
  if (!scope || !floating) return () => {};
  const previousScope = floating.getAttribute('data-bb-theme-scope');
  floating.setAttribute('data-bb-theme-scope', '');
  const previous = new Map(names.map(name => [name, floating.style.getPropertyValue(name)]));
  const sync = () => {
    const styles = getComputedStyle(scope);
    for (const name of names) {
      const value = styles.getPropertyValue(name).trim();
      if (value) floating.style.setProperty(name, value);
      else floating.style.removeProperty(name);
    }
    floating.style.fontFamily = styles.fontFamily;
  };
  const previousFont = floating.style.fontFamily;
  sync();
  const observer = new MutationObserver(sync);
  // A containing theme or document palette can change while the overlay is open.
  for (let ancestor = scope; ancestor; ancestor = ancestor.parentElement) {
    observer.observe(ancestor, { attributes: true, attributeFilter: [
      'style', 'class', 'data-base-color', 'data-primary-color', 'data-bb-density', 'data-bb-font',
      'data-bb-surface', 'data-bb-menu-color', 'data-bb-menu-accent'
    ] });
  }
  return () => {
    observer.disconnect();
    if (previousScope === null) floating.removeAttribute('data-bb-theme-scope');
    else floating.setAttribute('data-bb-theme-scope', previousScope);
    for (const [name, value] of previous) {
      if (value) floating.style.setProperty(name, value);
      else floating.style.removeProperty(name);
    }
    floating.style.fontFamily = previousFont;
  };
}

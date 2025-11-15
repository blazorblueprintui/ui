# Task Breakdown: Shadcn Theme Compatibility

**Total Tasks**: 33 subtasks
**Estimated Time**: 24-32 hours
**Phases**: 6 major phases

---

## Phase 1: Foundation (5 tasks, 4-6 hours)

[x] 1. Project Setup and Documentation (effort: medium)
- [x] 1.1. Create ADR document for OKLCH adoption
- [x] 1.2. Backup existing theme files
- [x] 1.3. Document current theme token mapping

[x] 2. Install Tailwind v4 Dependencies (effort: low)
- [x] 2.1. Install tailwindcss@next and @tailwindcss/cli@next
- [x] 2.2. Update package.json with pinned versions [depends: 2.1]
- [x] 2.3. Verify installation success [depends: 2.2]

[x] 3. Create OKLCH Theme File (effort: high)
- [x] 3.1. Create app.css with :root section
- [x] 3.2. Add all color tokens in OKLCH [depends: 3.1]
- [x] 3.3. Add .dark section with dark tokens [depends: 3.2]
- [x] 3.4. Add font family tokens [depends: 3.2]
- [x] 3.5. Add shadow scale tokens [depends: 3.2]
- [x] 3.6. Add radius and spacing tokens [depends: 3.2]

[x] 4. Remove Old Theme Files (effort: low)
- [x] 4.1. Delete shadcn-base.css [depends: 3.6]
- [x] 4.2. Delete themes/default.css [depends: 3.6]
- [x] 4.3. Delete themes/ocean.css [depends: 3.6]
- [x] 4.4. Delete themes/claude.css [depends: 3.6]

[x] 5. Review Checkpoint: Foundation Complete

## Phase 2: Tailwind Configuration (4 tasks, 3-4 hours)

[x] 6. Update Tailwind Configuration for v4 (effort: medium)
- [x] 6.1. Modify tailwind.config.js to v4 format
- [x] 6.2. Remove HSL color wrappers [depends: 6.1]
- [x] 6.3. Set darkMode to class selector [depends: 6.1]

[x] 7. Create Tailwind v4 Input File (effort: medium)
- [x] 7.1. Create app-input.css with @import directive
- [x] 7.2. Add @theme inline block [depends: 7.1]
- [x] 7.3. Map color CSS variables [depends: 7.2]
- [x] 7.4. Map font CSS variables [depends: 7.2]
- [x] 7.5. Map shadow CSS variables [depends: 7.2]
- [x] 7.6. Map radius CSS variables [depends: 7.2]

[x] 8. Test Tailwind Utility Generation (effort: low)
- [x] 8.1. Build CSS output [depends: 7.6]
- [x] 8.2. Verify color utilities generated [depends: 8.1]
- [x] 8.3. Verify font utilities generated [depends: 8.1]
- [x] 8.4. Verify shadow utilities generated [depends: 8.1]

[x] 9. Review Checkpoint: Configuration Complete

## Phase 3: Component Refactoring - Simple (3 tasks, 4-5 hours)

[x] 10. Refactor Badge Component to cn() (effort: medium)
- [x] 10.1. Replace StringBuilder with ClassNames.cn()
- [x] 10.2. Update variant class mappings [depends: 10.1]
- [x] 10.3. Test all Badge variants [depends: 10.2]

[x] 11. Refactor Switch Component to cn() (effort: medium)
- [x] 11.1. Replace StringBuilder with ClassNames.cn()
- [x] 11.2. Update state class mappings [depends: 11.1]
- [x] 11.3. Test Switch in light mode [depends: 11.2]
- [x] 11.4. Test Switch in dark mode [depends: 11.2]

[x] 12. Review Checkpoint: Simple Components Complete

## Phase 4: Component Refactoring - Core (5 tasks, 6-8 hours)

[x] 13. Refactor Input Component to cn() (effort: medium)
- [x] 13.1. Replace StringBuilder with ClassNames.cn()
- [x] 13.2. Update size and state classes [depends: 13.1]
- [x] 13.3. Test Input focus states [depends: 13.2]

[x] 14. Refactor Button Component to cn() (effort: high)
- [x] 14.1. Replace StringBuilder with ClassNames.cn()
- [x] 14.2. Update variant switch statement [depends: 14.1]
- [x] 14.3. Update size switch statement [depends: 14.1]
- [x] 14.4. Test all Button variants [depends: 14.2, 14.3]
- [x] 14.5. Test Button in both modes [depends: 14.4]

[x] 15. Refactor AccordionTrigger to cn() (effort: medium)
- [x] 15.1. Replace manual concat with ClassNames.cn()
- [x] 15.2. Update state-based classes [depends: 15.1]
- [x] 15.3. Test Accordion interactions [depends: 15.2]

[x] 16. Refactor DialogContent to cn() (effort: medium)
- [x] 16.1. Replace manual concat with ClassNames.cn()
- [x] 16.2. Update overlay and content classes [depends: 16.1]
- [x] 16.3. Test Dialog in both modes [depends: 16.2]

[x] 17. Review Checkpoint: Core Components Complete

## Phase 5: Service Layer (4 tasks, 3-4 hours)

[x] 18. Simplify ThemeService for Dark Mode (effort: medium)
- [x] 18.1. Remove multi-theme switching logic
- [x] 18.2. Add dark mode toggle method [depends: 18.1]
- [x] 18.3. Implement localStorage persistence [depends: 18.2]
- [x] 18.4. Update JSInterop for .dark class [depends: 18.2]

[x] 19. Create DarkModeToggle Component (effort: low)
- [x] 19.1. Create component using Switch
- [x] 19.2. Connect to ThemeService [depends: 19.1, 18.4]
- [x] 19.3. Test toggle functionality [depends: 19.2]

[x] 20. Update Demo Pages (effort: low)
- [x] 20.1. Replace ThemeSelector with DarkModeToggle
- [x] 20.2. Remove theme switching UI
- [x] 20.3. Update demo page styling [depends: 20.1]

[x] 21. Review Checkpoint: Service Layer Complete

## Phase 6: Testing and Documentation (6 tasks, 6-9 hours)

[ ] 22. Theme Compatibility Testing (effort: medium)
- [ ] 22.1. Copy theme from tweakcn.com
- [ ] 22.2. Paste into app.css unmodified [depends: 22.1]
- [ ] 22.3. Verify all components render [depends: 22.2]
- [ ] 22.4. Test dark mode toggle [depends: 22.2]

[ ] 23. Component Visual Testing - Light Mode (effort: high)
- [ ] 23.1. Test Button all variants
- [ ] 23.2. Test Input with focus states
- [ ] 23.3. Test Badge all variants
- [ ] 23.4. Test Switch interactions
- [ ] 23.5. Test AccordionTrigger expanded/collapsed
- [ ] 23.6. Test DialogContent overlay

[ ] 24. Component Visual Testing - Dark Mode (effort: high)
- [ ] 24.1. Test all components in dark [depends: 23.6]
- [ ] 24.2. Verify contrast ratios acceptable [depends: 24.1]
- [ ] 24.3. Check focus ring visibility [depends: 24.1]

[ ] 25. Cross-Browser Testing (effort: medium)
- [ ] 25.1. Test in Chrome 111+ [depends: 24.3]
- [ ] 25.2. Test in Safari 15.4+ [depends: 24.3]
- [ ] 25.3. Test in Firefox 113+ [depends: 24.3]
- [ ] 25.4. Test in Edge Chromium [depends: 24.3]

[ ] 26. Create Migration Guide (effort: medium)
- [ ] 26.1. Document breaking changes
- [ ] 26.2. Create step-by-step upgrade instructions [depends: 26.1]
- [ ] 26.3. Document theme conversion process [depends: 26.1]
- [ ] 26.4. Add troubleshooting section [depends: 26.2]

[ ] 27. Update Documentation (effort: medium)
- [ ] 27.1. Update theming.md for OKLCH approach
- [ ] 27.2. Document Tailwind v4 requirements [depends: 27.1]
- [ ] 27.3. Add theme usage examples [depends: 27.1]
- [ ] 27.4. Update architecture.md with changes [depends: 27.1]

[ ] 28. Review Checkpoint: Testing and Documentation Complete

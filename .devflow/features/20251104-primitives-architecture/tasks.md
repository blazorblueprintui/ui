# Task Breakdown: Radix UI-Style Primitives Architecture

**Total Tasks**: 82 subtasks
**Estimated Time**: 100-130 hours
**Phases**: 6 major phases

---

## Phase 1: Infrastructure Foundation (15 subtasks, 18-24 hours)

[x] 1. Create Folder Structure (effort: low)
- [x] 1.1. Create Primitives/ directory structure
- [x] 1.2. Create Shared/Services/ directory structure
- [x] 1.3. Create Shared/Utilities/ directory structure
- [x] 1.4. Create Shared/Contexts/ directory structure
- [x] 1.5. Create wwwroot/js/primitives/ directory structure

[x] 2. Implement PortalService Infrastructure (effort: high)
- [x] 2.1. Create IPortalService interface
- [x] 2.2. Implement PortalService with registry [depends: 2.1]
- [x] 2.3. Create PortalHost component [depends: 2.2]
- [x] 2.4. Add DI registration [depends: 2.2]

[x] 3. Implement FocusManager Service (effort: high)
- [x] 3.1. Create IFocusManager interface
- [x] 3.2. Implement FocusManager with JS interop [depends: 3.1]
- [x] 3.3. Create focus-trap.js module [depends: 3.2]
- [x] 3.4. Add DI registration [depends: 3.2]

[x] 4. Implement PositioningService (effort: high)
- [x] 4.1. Create IPositioningService interface
- [x] 4.2. Implement PositioningService wrapper
- [x] 4.3. Create positioning.js with Floating UI [depends: 4.2]
- [x] 4.4. Add DI registration [depends: 4.2]

[x] 5. Implement IdGenerator Utility (effort: low)
- [x] 5.1. Create IdGenerator static class
- [x] 5.2. Add thread-safe counter implementation [depends: 5.1]

[x] 6. Implement UseControllableState Pattern (effort: medium)
- [x] 6.1. Create UseControllableState<T> class
- [x] 6.2. Add controlled/uncontrolled logic [depends: 6.1]

[x] 7. Implement AriaBuilder Utility (effort: medium)
- [x] 7.1. Create AriaBuilder class
- [x] 7.2. Add fluent API methods [depends: 7.1]

[x] 8. Implement KeyboardNavigator Base (effort: medium)
- [x] 8.1. Create KeyboardNavigator class
- [x] 8.2. Add arrow key handling [depends: 8.1]
- [x] 8.3. Add RTL support [depends: 8.2]

[x] 9. Create JavaScript Modules (effort: high)
- [x] 9.1. Create portal.js module
- [x] 9.2. Create click-outside.js module
- [x] 9.3. Setup JS module bundling

[x] 10. Create PrimitiveContext Base (effort: medium)
- [x] 10.1. Create PrimitiveContext<TState> abstract class
- [x] 10.2. Add state management pattern [depends: 10.1]

[x] 11. Register Services in DI (effort: low)
- [x] 11.1. Create ServiceCollectionExtensions
- [x] 11.2. Add all service registrations [depends: 11.1, 2.4, 3.4, 4.4]

[x] 12. Review Checkpoint: Infrastructure Foundation Complete

---

## Phase 2: Dialog Primitive Reference Implementation (18 subtasks, 24-32 hours)

[x] 13. Create Dialog Context (effort: medium)
- [x] 13.1. Create DialogContext.cs
- [x] 13.2. Add state properties [depends: 13.1]
- [x] 13.3. Add ID generation [depends: 13.2]

[x] 14. Create Dialog Root Component (effort: medium)
- [x] 14.1. Create Dialog.razor primitive
- [x] 14.2. Implement controlled/uncontrolled state [depends: 14.1, 13.3]
- [x] 14.3. Setup CascadingValue context [depends: 14.2]

[x] 15. Create DialogTrigger Component (effort: medium)
- [x] 15.1. Create DialogTrigger.razor
- [x] 15.2. Implement click handler [depends: 15.1]
- [x] 15.3. Add ARIA attributes [depends: 15.2]

[x] 16. Create DialogPortal Component (effort: high)
- [x] 16.1. Create DialogPortal.razor
- [x] 16.2. Integrate with PortalService [depends: 16.1]
- [x] 16.3. Handle disposal cleanup [depends: 16.2]

[x] 17. Create DialogOverlay Component (effort: medium)
- [x] 17.1. Create DialogOverlay.razor
- [x] 17.2. Add click-outside handler [depends: 17.1]
- [x] 17.3. Implement close on click [depends: 17.2]

[x] 18. Create DialogContent Component (effort: high)
- [x] 18.1. Create DialogContent.razor
- [x] 18.2. Implement focus trap [depends: 18.1]
- [x] 18.3. Add escape key handler [depends: 18.2]
- [x] 18.4. Implement body scroll lock [depends: 18.2]
- [x] 18.5. Add focus restoration [depends: 18.3]
- [x] 18.6. Add ARIA dialog attributes [depends: 18.2]

[x] 19. Create DialogTitle Component (effort: low)
- [x] 19.1. Create DialogTitle.razor
- [x] 19.2. Add aria-labelledby support [depends: 19.1]

[x] 20. Create DialogDescription Component (effort: low)
- [x] 20.1. Create DialogDescription.razor
- [x] 20.2. Add aria-describedby support [depends: 20.1]

[x] 21. Create DialogClose Component (effort: medium)
- [x] 21.1. Create DialogClose.razor
- [x] 21.2. Implement close handler [depends: 21.1]
- [x] 21.3. Add keyboard support [depends: 21.2]

[x] 22. Test Dialog Primitive with Playwright (effort: high)
- [x] 22.1. Test keyboard navigation sequence [depends: 18.6, 21.3]
- [x] 22.2. Test focus trap behavior [depends: 18.2]
- [x] 22.3. Verify ARIA attributes present [depends: 18.6]
- [x] 22.4. Test escape key close [depends: 18.3]

[x] 23. Create Styled Dialog Component (effort: medium)
- [x] 23.1. Create Components/Dialog/Dialog.razor wrapper
- [x] 23.2. Apply shadcn Tailwind classes [depends: 23.1]
- [x] 23.3. Create DialogHeader helper [depends: 23.1]
- [x] 23.4. Create DialogFooter helper [depends: 23.1]

[x] 24. Create Dialog Demo Pages (effort: medium)
- [x] 24.1. Create DialogPrimitive demo page
- [x] 24.2. Create Dialog styled demo page [depends: 23.2]
- [x] 24.3. Add usage examples [depends: 24.1, 24.2]

[x] 25. Review Checkpoint: Dialog Primitive Complete

---

## Phase 3: Refactor Existing Components (28 subtasks, 32-42 hours)

[x] 26. Refactor Popover to Primitive (effort: high)
- [x] 26.1. Create PopoverPrimitive.razor
- [x] 26.2. Create PopoverTrigger.razor [depends: 26.1]
- [x] 26.3. Create PopoverContent with positioning [depends: 26.2]
- [x] 26.4. Integrate PositioningService [depends: 26.3]
- [x] 26.5. Rebuild styled Popover component [depends: 26.4]
- [x] 26.6. Test with Playwright [depends: 26.5]
- [x] 26.7. Create primitive demo page [depends: 26.4]
- [x] 26.8. Create styled demo page [depends: 26.5]

[x] 27. Refactor Dropdown Menu to Primitive (effort: high)
- [x] 27.1. Create DropdownMenuPrimitive.razor
- [x] 27.2. Create DropdownMenuTrigger.razor [depends: 27.1]
- [x] 27.3. Create DropdownMenuContent [depends: 27.2]
- [x] 27.4. Implement keyboard navigation [depends: 27.3]
- [x] 27.5. Rebuild styled DropdownMenu component [depends: 27.4]
- [x] 27.6. Test keyboard nav with Playwright [depends: 27.4]
- [x] 27.7. Create primitive demo page [depends: 27.4]
- [x] 27.8. Create styled demo page [depends: 27.5]

[x] 28. Refactor Tooltip to Primitive (effort: medium)
- [x] 28.1. Create TooltipPrimitive.razor
- [x] 28.2. Create TooltipTrigger with hover [depends: 28.1]
- [x] 28.3. Create TooltipContent with positioning [depends: 28.2]
- [x] 28.4. Rebuild styled Tooltip component [depends: 28.3]
- [x] 28.5. Test with Playwright [depends: 28.4]
- [x] 28.6. Create demo pages [depends: 28.4]

[x] 29. Refactor Collapsible to Primitive (effort: medium)
- [x] 29.1. Create CollapsiblePrimitive.razor
- [x] 29.2. Create CollapsibleTrigger.razor [depends: 29.1]
- [x] 29.3. Create CollapsibleContent.razor [depends: 29.2]
- [x] 29.4. Rebuild styled Collapsible component [depends: 29.3]
- [x] 29.5. Test with Playwright [depends: 29.4]
- [x] 29.6. Create demo pages [depends: 29.4]

[x] 30. Refactor Checkbox to Primitive (effort: medium)
- [x] 30.1. Create CheckboxPrimitive.razor
- [x] 30.2. Implement indeterminate state [depends: 30.1]
- [x] 30.3. Add ARIA checkbox attributes [depends: 30.2]
- [x] 30.4. Rebuild styled Checkbox component [depends: 30.3]
- [x] 30.5. Test with Playwright [depends: 30.4]
- [x] 30.6. Create demo pages [depends: 30.4]

[x] 31. Refactor Radio Group to Primitive (effort: medium)
- [x] 31.1. Create RadioGroupPrimitive.razor
- [x] 31.2. Create RadioGroupItem.razor [depends: 31.1]
- [x] 31.3. Implement arrow key navigation [depends: 31.2]
- [x] 31.4. Add ARIA radio attributes [depends: 31.3]
- [x] 31.5. Rebuild styled RadioGroup component [depends: 31.4]
- [x] 31.6. Test keyboard nav with Playwright [depends: 31.4]
- [x] 31.7. Create demo pages [depends: 31.5]

[x] 32. Refactor Switch to Primitive (effort: low)
- [x] 32.1. Create SwitchPrimitive.razor
- [x] 32.2. Add ARIA switch role [depends: 32.1]
- [x] 32.3. Rebuild styled Switch component [depends: 32.2]
- [x] 32.4. Test with Playwright [depends: 32.3]
- [x] 32.5. Create demo pages [depends: 32.3]

[x] 33. Refactor Label to Primitive (effort: low)
- [x] 33.1. Create LabelPrimitive.razor
- [x] 33.2. Implement for attribute handling [depends: 33.1]
- [x] 33.3. Rebuild styled Label component [depends: 33.2]
- [x] 33.4. Create demo pages [depends: 33.3]

[x] 34. Refactor Select to Primitive (effort: high)
- [x] 34.1. Create SelectPrimitive.razor (+ SelectContext.cs, SelectItem.razor)
- [x] 34.2. Create SelectTrigger.razor [depends: 34.1]
- [x] 34.3. Create SelectContent dropdown [depends: 34.2]
- [x] 34.4. Implement keyboard navigation [depends: 34.3]
- [x] 34.5. Rebuild styled Select component [depends: 34.4]
- [x] 34.6. Test with Playwright [depends: 34.5]
- [x] 34.7. Create demo pages [depends: 34.5]

[x] 35. Review Checkpoint: Refactored Components Complete

---

## Phase 4: New Priority Primitives (11 subtasks, 14-18 hours)

[x] 36. Implement Tabs Primitive (effort: high)
- [x] 36.1. Create TabsPrimitive.razor
- [x] 36.2. Create TabsList.razor [depends: 36.1]
- [x] 36.3. Create TabsTrigger.razor [depends: 36.2]
- [x] 36.4. Create TabsContent.razor [depends: 36.2]
- [x] 36.5. Implement arrow key navigation [depends: 36.3]
- [x] 36.6. Add ARIA tabs attributes [depends: 36.5]
- [x] 36.7. Create styled Tabs component [depends: 36.6]
- [x] 36.8. Test with Playwright [depends: 36.6]
- [x] 36.9. Create demo pages [depends: 36.7]

[x] 37. Implement Accordion Primitive (effort: high)
- [x] 37.1. Create AccordionPrimitive.razor
- [x] 37.2. Create AccordionItem.razor [depends: 37.1]
- [x] 37.3. Create AccordionTrigger.razor [depends: 37.2]
- [x] 37.4. Create AccordionContent.razor [depends: 37.2]
- [x] 37.5. Implement single/multiple mode [depends: 37.3]
- [x] 37.6. Add keyboard navigation [depends: 37.5]
- [x] 37.7. Add ARIA accordion attributes [depends: 37.6]
- [x] 37.8. Create styled Accordion component [depends: 37.7]
- [x] 37.9. Test with Playwright [depends: 37.7]
- [x] 37.10. Create demo pages [depends: 37.8]

[x] 38. Implement Hover Card Primitive (effort: medium)
- [x] 38.1. Create HoverCardPrimitive.razor
- [x] 38.2. Create HoverCardTrigger with hover [depends: 38.1]
- [x] 38.3. Create HoverCardContent with positioning [depends: 38.2]
- [x] 38.4. Implement delay controls [depends: 38.3]
- [x] 38.5. Create styled HoverCard component [depends: 38.4]
- [x] 38.6. Test with Playwright [depends: 38.5]
- [x] 38.7. Create demo pages [depends: 38.5]

[x] 39. Review Checkpoint: New Primitives Complete

---

## Phase 5: Demo Site Restructure (10 subtasks, 12-16 hours)

[x] 40. Restructure Demo Folder Organization (effort: medium)
- [x] 40.1. Create Demo/Pages/Primitives/ directory
- [x] 40.2. Create Demo/Pages/Components/ directory
- [x] 40.3. Move existing demos to appropriate folders [depends: 40.1, 40.2]

[x] 41. Create Shared Demo Utilities (effort: medium)
- [x] 41.1. Create CodeBlock.razor component
- [x] 41.2. Create PropsTable.razor component
- [x] 41.3. Create ExampleSection.razor component
- [x] 41.4. Create KeyboardShortcuts.razor component

[x] 42. Update Demo Navigation (effort: medium)
- [x] 42.1. Update NavMenu.razor structure
- [x] 42.2. Add Primitives section [depends: 42.1]
- [x] 42.3. Add Components section [depends: 42.1]
- [x] 42.4. Add architecture overview link [depends: 42.1]

[x] 43. Update Landing Page (effort: low)
- [x] 43.1. Update Index.razor content
- [x] 43.2. Add primitives architecture overview [depends: 43.1]
- [x] 43.3. Add getting started guidance [depends: 43.1]

[x] 44. Update Getting Started Page (effort: medium)
- [x] 44.1. Update GettingStarted.razor
- [x] 44.2. Explain primitives vs components [depends: 44.1]
- [x] 44.3. Add usage examples [depends: 44.2]
- [x] 44.4. Document composition patterns [depends: 44.2]

[x] 45. Review Checkpoint: Demo Site Complete

---

## Phase 6: Testing and Documentation (10 subtasks, 12-18 hours)

[ ] 46. Perform Keyboard Navigation Audit (effort: high)
- [ ] 46.1. Test Tab/Shift+Tab across all primitives
- [ ] 46.2. Test Enter/Space activation
- [ ] 46.3. Test Escape key handling
- [ ] 46.4. Test arrow key navigation
- [ ] 46.5. Test Home/End keys

[ ] 47. Test Hosting Model Compatibility (effort: medium)
- [ ] 47.1. Test all primitives in Blazor Server
- [ ] 47.2. Test all primitives in Blazor WebAssembly [depends: 47.1]
- [ ] 47.3. Document any compatibility issues [depends: 47.2]

[ ] 48. Perform Screen Reader Testing (effort: high)
- [ ] 48.1. Test with NVDA screen reader
- [ ] 48.2. Verify role announcements [depends: 48.1]
- [ ] 48.3. Verify state change announcements [depends: 48.1]
- [ ] 48.4. Document accessibility compliance [depends: 48.3]

[ ] 49. Cross-Browser Testing (effort: medium)
- [ ] 49.1. Test in Chrome
- [ ] 49.2. Test in Firefox
- [ ] 49.3. Test in Safari
- [ ] 49.4. Test in Edge
- [ ] 49.5. Document browser compatibility [depends: 49.1, 49.2, 49.3, 49.4]

[ ] 50. Update Architecture Documentation (effort: medium)
- [ ] 50.1. Update architecture.md with primitives layer
- [ ] 50.2. Document two-tier architecture pattern [depends: 50.1]
- [ ] 50.3. Document service infrastructure [depends: 50.1]
- [ ] 50.4. Add folder structure diagrams [depends: 50.2]

[ ] 51. Document API Patterns (effort: medium)
- [ ] 51.1. Document controlled/uncontrolled pattern
- [ ] 51.2. Document composition patterns
- [ ] 51.3. Document CascadingValue usage
- [ ] 51.4. Create JavaScript interop guidelines

[ ] 52. Create Primitive READMEs (effort: medium)
- [ ] 52.1. Create Dialog README with API
- [ ] 52.2. Create Tabs README with API
- [ ] 52.3. Create Accordion README with API
- [ ] 52.4. Create README template for remaining [depends: 52.1]

[ ] 53. Review Checkpoint: Testing and Documentation Complete

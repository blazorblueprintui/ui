# ADR-001: Demo App Hosting Model

**Status:** Accepted
**Date:** 2025-11-01
**Context:** Button Component (20251101-button-component)

---

## Context

BlazorUI needs a demo application for developing and testing components. Blazor supports three hosting models: Server, WebAssembly, and Hybrid (MAUI). We need to choose which hosting model to use for the primary demo application.

Since this is the first component being built, there is no existing code or infrastructure. The demo app choice will affect:
- Development iteration speed
- Debugging experience
- Testing workflow
- Bundle size visibility

## Decision

**We will use Blazor Server as the primary hosting model for the demo application.**

## Rationale

### Why Blazor Server?

1. **Fastest Development Iteration**
   - Hot reload works instantly
   - No WebAssembly compilation wait time (30-60 seconds per build)
   - Changes reflect immediately in browser

2. **Easier Debugging**
   - Server-side execution allows standard .NET debugging
   - Full access to breakpoints and debug tools
   - Console output appears in terminal, not browser console

3. **Simpler Setup**
   - No need to configure WebAssembly-specific settings
   - Standard ASP.NET Core pipeline
   - Fewer build steps

4. **Better for Visual Iteration**
   - Manual testing requires frequent changes
   - Immediate feedback on styling and layout
   - Easier to compare with shadcn reference

5. **Can Add WebAssembly Later**
   - Not an either/or decision
   - Can create separate WASM demo project later
   - Useful for testing bundle size and WebAssembly-specific issues

### Trade-offs

**Disadvantages:**
- Requires server connection (SignalR)
- Won't surface WebAssembly-specific issues immediately
- Doesn't test download size and performance

**Mitigation:**
- Phase 4 of implementation plan includes WebAssembly testing
- Can create `BlazorUI.Demo.Wasm` project later
- Most component issues are platform-agnostic

## Alternatives Considered

### Alternative 1: Blazor WebAssembly

**Pros:**
- Tests bundle size from day one
- No server connection required
- Surfaces WebAssembly-specific issues early

**Cons:**
- 30-60 second compilation time on every change
- Slower development iteration
- More complex debugging (browser DevTools)
- Harder to test during active development

**Decision:** Rejected for initial demo. Can add as secondary demo later.

### Alternative 2: Blazor Hybrid (MAUI)

**Pros:**
- Tests native desktop/mobile scenarios
- Platform-specific testing

**Cons:**
- Overkill for web component library
- Adds complexity (MAUI setup, platform projects)
- Slower than Server for web development
- Not all developers have mobile emulators

**Decision:** Rejected. Hybrid testing can be deferred to later phase.

### Alternative 3: Multiple Demo Apps (Server + WASM)

**Pros:**
- Tests all scenarios from start
- Comprehensive platform coverage

**Cons:**
- More projects to maintain
- Duplicate demo pages
- Adds initial setup complexity
- Unnecessary for first component

**Decision:** Rejected for initial implementation. Start with Server, add WASM in Phase 4 if needed.

## Consequences

### Positive

- Fast development cycle for building components
- Easy visual testing and iteration
- Simple debugging experience
- Lower barrier to entry for contributors

### Negative

- Won't catch WebAssembly-specific issues immediately
- Need to manually test in WASM later
- Bundle size optimization deferred to Phase 4

### Neutral

- Demo app structure can serve as template for WASM demo
- Tailwind configuration will work identically in WASM
- Component code is platform-agnostic

## Implementation Notes

**Demo project setup:**
```bash
dotnet new blazorserver -n BlazorUI.Demo -f net8.0 -o demo/BlazorUI.Demo
```

**Development workflow:**
```bash
# Terminal 1: Tailwind watch
cd demo/BlazorUI.Demo
npm run watch:css

# Terminal 2: Blazor hot reload
dotnet watch run --project demo/BlazorUI.Demo
```

**Future WebAssembly demo (optional):**
```bash
dotnet new blazorwasm -n BlazorUI.Demo.Wasm -f net8.0 -o demo/BlazorUI.Demo.Wasm
```

## References

- [Blazor hosting models comparison](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models)
- [Blazor Server vs WebAssembly guide](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-model-configuration)
- shadcn uses Next.js (SSR), closest Blazor equivalent is Server

---

**Related ADRs:**
- ADR-002: Tailwind Integration
- ADR-003: CSS Variables Theming

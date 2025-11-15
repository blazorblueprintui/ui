# ADR-002: Tailwind CSS Integration Approach

**Status:** Accepted
**Date:** 2025-11-01
**Context:** Button Component (20251101-button-component)

---

## Context

BlazorUI components need styling that matches shadcn/ui's design system. shadcn uses Tailwind CSS with utility classes for styling. We need to decide how to integrate Tailwind CSS into our Blazor component library.

Key considerations:
- shadcn relies heavily on Tailwind utilities
- Components need to work in consuming applications
- Build process and tooling
- Developer experience

## Decision

**We will use Tailwind standalone CLI (no Node.js required), integrated into the demo app's build process.**

## Rationale

### Why Tailwind Standalone CLI?

1. **Pure .NET Workflow**
   - No Node.js dependency required
   - Single executable binary
   - Keeps development stack pure .NET
   - No npm/package.json needed

2. **Ease of Distribution**
   - ~10MB standalone executable
   - Can be committed to repo or downloaded on build
   - Works on Windows, macOS, Linux
   - No installation process - just download and run

3. **Full Tailwind Capabilities**
   - JIT mode for on-demand utility generation
   - Plugin support for extensions
   - Custom theme configuration
   - Purging/optimization built-in

4. **MSBuild Integration**
   - Easily integrated into `.csproj` files
   - Runs as part of `dotnet build`
   - No separate build tooling required
   - Standard .NET development experience

5. **Performance**
   - Native binary - faster than Node.js version
   - JIT compiles only used utilities
   - Small output bundle in production
   - Fast development builds

6. **Consumer Control**
   - Developers configure Tailwind in their apps
   - Can customize theme, colors, spacing
   - Not locked into library's Tailwind version

### How It Works

**Download Standalone CLI:**
```bash
# Windows
curl -sLO https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-windows-x64.exe

# macOS (Intel)
curl -sLO https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-macos-x64

# Linux
curl -sLO https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-linux-x64
```

**Build Process:**
```bash
# Development (watch mode)
./tailwindcss.exe -i input.css -o output.css --watch

# Production (minified)
./tailwindcss.exe -i input.css -o output.css --minify
```

**MSBuild Integration:**
```xml
<Target Name="BuildTailwindCSS" BeforeTargets="BeforeBuild">
  <Exec Command="$(MSBuildProjectDirectory)\tailwindcss.exe -i wwwroot/css/app-input.css -o wwwroot/css/app.css" />
</Target>
```

**In Consuming Apps:**
Developers will:
1. Download Tailwind standalone CLI
2. Configure `tailwind.config.js` to scan library components
3. Include library's CSS variables
4. Build Tailwind as part of their app (via MSBuild or manually)

### Trade-offs

**Disadvantages:**
- Binary file size (~10MB) in repository (if committed)
- Two processes during development (Tailwind + Blazor)
- Consumers must download CLI and configure Tailwind
- Not "truly" plug-and-play (requires setup)

**Mitigation:**
- Can download binary on build instead of committing
- Document setup clearly in README
- Provide example `tailwind.config.js` and MSBuild integration
- Tailwind setup is one-time per project
- Much simpler than requiring Node.js

## Alternatives Considered

### Alternative 1: Tailwind via npm/Node.js

**Approach:** Install Tailwind via npm, use Node.js tooling for build.

**Pros:**
- Official Tailwind documentation uses this approach
- More common in web development
- PostCSS plugins ecosystem available

**Cons:**
- Requires Node.js installation
- Adds npm/package.json to .NET project
- Not idiomatic for .NET developers
- Extra dependency to manage and document
- Consumers also need Node.js

**Decision:** Rejected. The standalone CLI provides the same features without Node.js dependency, keeping the stack pure .NET.

### Alternative 2: Pre-compiled CSS

**Approach:** Generate all Tailwind utilities, bundle as static CSS file.

**Pros:**
- No Tailwind setup required by consumers
- True plug-and-play (just reference CSS file)
- No Node.js dependency

**Cons:**
- Huge CSS bundle size (100KB+ uncompressed)
- Can't customize Tailwind theme
- Misses core benefit of utility-first CSS
- Not how shadcn works
- Doesn't support JIT features

**Decision:** Rejected. Goes against Tailwind and shadcn philosophy.

### Alternative 3: CSS-in-C# / Inline Styles

**Approach:** Generate styles programmatically in C# code.

**Pros:**
- No external build tools
- Pure Blazor/.NET solution
- Dynamic styling possible

**Cons:**
- Massive departure from shadcn
- Need to reimplement all Tailwind utilities
- Loses visual parity with shadcn
- Huge maintenance burden
- Developers can't use Tailwind knowledge

**Decision:** Rejected. Breaks from shadcn design system.

### Alternative 4: PostCSS Plugin for Blazor

**Approach:** Integrate PostCSS/Tailwind directly into .NET build pipeline.

**Pros:**
- Single build process
- Integrated with `dotnet build`
- No separate npm scripts

**Cons:**
- Complex setup
- Non-standard approach
- Harder to debug
- Less flexible than standard Tailwind CLI
- Breaks standard Tailwind workflows

**Decision:** Rejected. Adds complexity without significant benefit.

### Alternative 5: Tailwind CDN

**Approach:** Use Tailwind's Play CDN for styling.

**Pros:**
- Zero setup for consumers
- Instant Tailwind access
- No build step

**Cons:**
- Production use discouraged by Tailwind
- Slower performance (loads all utilities)
- Can't purge unused styles
- JIT mode limited
- Not suitable for libraries

**Decision:** Rejected. Not production-ready.

## Consequences

### Positive

- Full Tailwind feature set available
- Matches shadcn's approach exactly
- Consumers can customize themes
- Standard, well-documented workflow
- JIT provides optimal performance

### Negative

- Consumers must install Tailwind
- Requires Node.js for build
- Additional setup step for consumers
- Two watch processes during development

### Neutral

- Tailwind version can be updated independently
- Consumers control which Tailwind features they use
- Build process is familiar to React developers

## Implementation Notes

**Download Standalone CLI:**
```bash
cd demo/BlazorUI.Demo

# Windows
curl -sLO https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-windows-x64.exe
mv tailwindcss-windows-x64.exe tailwindcss.exe

# macOS Intel
curl -sLO https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-macos-x64
chmod +x tailwindcss-macos-x64
mv tailwindcss-macos-x64 tailwindcss

# Linux
curl -sLO https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-linux-x64
chmod +x tailwindcss-linux-x64
mv tailwindcss-linux-x64 tailwindcss
```

**Initialize Tailwind:**
```bash
./tailwindcss init  # or .\tailwindcss.exe init on Windows
```

**tailwind.config.js:**
```javascript
module.exports = {
  darkMode: ["class"],
  content: [
    './Pages/**/*.{razor,html}',
    './Shared/**/*.{razor,html}',
    '../../src/BlazorUI/**/*.{razor,html}',  // Scan library
  ],
  theme: {
    extend: {
      colors: {
        // CSS variable integration
        border: "hsl(var(--border))",
        primary: {
          DEFAULT: "hsl(var(--primary))",
          foreground: "hsl(var(--primary-foreground))",
        },
        // ... more colors
      },
    },
  },
}
```

**Development Commands:**
```bash
# Watch mode (development)
./tailwindcss.exe -i wwwroot/css/app-input.css -o wwwroot/css/app.css --watch

# Build (production)
./tailwindcss.exe -i wwwroot/css/app-input.css -o wwwroot/css/app.css --minify
```

**MSBuild Integration (Optional):**

Add to `BlazorUI.Demo.csproj`:
```xml
<!-- Download Tailwind CLI if not present -->
<Target Name="DownloadTailwindCLI" BeforeTargets="BeforeBuild" Condition="!Exists('$(MSBuildProjectDirectory)\tailwindcss.exe')">
  <Message Text="Downloading Tailwind CLI..." Importance="high" />
  <DownloadFile SourceUrl="https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-windows-x64.exe"
                DestinationFolder="$(MSBuildProjectDirectory)"
                DestinationFileName="tailwindcss.exe" />
</Target>

<!-- Build Tailwind CSS -->
<Target Name="BuildTailwindCSS" BeforeTargets="BeforeBuild" DependsOnTargets="DownloadTailwindCLI">
  <Message Text="Building Tailwind CSS..." Importance="high" />
  <Exec Command="$(MSBuildProjectDirectory)\tailwindcss.exe -i wwwroot/css/app-input.css -o wwwroot/css/app.css" />
</Target>
```

**Consumer Setup (README documentation):**
```markdown
### Prerequisites
- .NET 8 SDK
- Tailwind CSS standalone CLI (no Node.js required)

### Installation
1. Install BlazorUI: `dotnet add package BlazorUI`
2. Download Tailwind CLI:
   ```bash
   curl -sLO https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-windows-x64.exe
   ```
3. Configure `tailwind.config.js` (see example in documentation)
4. Build CSS: `./tailwindcss.exe -i input.css -o output.css`

### Optional: MSBuild Integration
Add Tailwind build to your `.csproj` file so CSS builds automatically with `dotnet build`.
See documentation for MSBuild integration example.
```

## References

- [Tailwind Standalone CLI](https://tailwindcss.com/blog/standalone-cli)
- [Tailwind CLI documentation](https://tailwindcss.com/docs/installation)
- [Tailwind JIT mode](https://tailwindcss.com/docs/just-in-time-mode)
- [shadcn Tailwind setup](https://ui.shadcn.com/docs/installation)
- [Blazor with Tailwind guide](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/css-isolation)
- [Tailwind GitHub Releases](https://github.com/tailwindlabs/tailwindcss/releases)

---

**Related ADRs:**
- ADR-001: Demo App Hosting Model
- ADR-003: CSS Variables Theming

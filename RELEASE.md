# Release Guide

This document describes how Blazor Blueprint packages are published to NuGet.

## Overview

Blazor Blueprint uses a **monorepo with independent package versioning**. Each of the seven packages can be released independently with its own version number:

- **BlazorBlueprint.Primitives** - Headless UI primitives
- **BlazorBlueprint.Components** - Styled components
- **BlazorBlueprint.Icons.Lucide** - Lucide icon library
- **BlazorBlueprint.Icons.Heroicons** - Heroicons library
- **BlazorBlueprint.Icons.Feather** - Feather icon library
- **BlazorBlueprint.Icons.FontAwesome** - Font Awesome Free icon library
- **BlazorBlueprint.PdfViewer** - PDF viewer built on PDF.js (add-on to Components)

## Prerequisites

Releases are **run locally by a maintainer** — there is no CI publishing pipeline. Pushing a tag does not publish anything on its own; the release scripts do the packing, the NuGet push, and the tagging together.

You need:

1. **The `devkit` checkout** — the release scripts live in the private `devkit` repo, checked out at `devkit/` in the repo root (it is gitignored). Maintainers only; external contributors cannot run releases.
2. **A NuGet API key** exported as `NUGET_API_KEY`:

   ```bash
   # ~/.bashrc.local
   export NUGET_API_KEY="your-key"
   ```

   Create the key at https://www.nuget.org/account/apikeys with "Push" permission, scoped to `BlazorBlueprint.*`.
3. **The appropriate release branch**, up to date with origin. Stable and icon releases run from `develop` and open the `develop` → `main` PR. Prereleases use `release-beta.sh --branch NAME`, refuse `main`, and do not open a merge PR.

## Quick Start

The scripts are interactive — they show you what changed, prompt for versions, and summarise before doing anything.

```bash
# Primitives, Components and/or PdfViewer
./devkit/scripts/release.sh

# Prerelease from the v4.1.0 branch
./devkit/scripts/release-beta.sh --branch v4.1.0

# Icon packages (Lucide, Heroicons, Feather, Font Awesome)
./devkit/scripts/release-icons.sh
```

Use `--dry-run` on any release script to walk through every step without executing any git, build, NuGet, or PR operation. It is the safest way to check what a release would do:

```bash
./devkit/scripts/release-icons.sh --dry-run
```

## How It Works

### `release.sh` — Primitives, Components and PdfViewer

Releases any one of these packages, or a combination, in a single run. The dependency chain is Primitives → Components → PdfViewer: each package is packed against the published NuGet version of the one before it, and the script polls NuGet for availability when a package depends on one released earlier in the same run. Creates the `develop` → `main` PR when done.

PdfViewer has its own version line (`pdfviewer/v*`, starting at 1.0.0) and is published without release notes.

Flags:

| Flag | Effect |
|------|--------|
| `--dry-run` | Walk through all steps without executing any |
| `--skip-notes` | Use existing `RELEASE_NOTES.md` as-is instead of regenerating |
| `--skip-tests` | Skip .NET and JavaScript tests (for re-releases where code has not changed) |
| `--clear-cache` | Clear the NuGet HTTP cache before building, when a freshly published package isn't resolving |

### `release-beta.sh` — Primitives, Components and PdfViewer prereleases

Runs from the branch named by `--branch` (default `v4`; pass `--branch v4.1.0` for this branch). Select a base such as `4.1.0` and an identifier such as `beta`; the script derives the next number from local tags. It refuses stable versions and releases from `main`.

The script runs the .NET and JavaScript suites, prepares release notes, tags and publishes Primitives, waits for NuGet availability, then updates Components to use that Primitives version and publishes Components. PdfViewer follows the same pattern after Components; until it has a stable release, its prereleases build towards 1.0.0. It pushes the release branch and package tags without opening a PR to `main`.

It supports the same flags as `release.sh`, plus `--branch NAME`. `--skip-tests` skips both the .NET and JavaScript suites. `--skip-notes` uses the existing package notes as written, so check their version headings before using it for publication. Browser checks run separately as described in [tests/browser/README.md](tests/browser/README.md).

For a local rehearsal that does not build, tag, push or publish:

```bash
./devkit/scripts/release-beta.sh --branch v4.1.0 --dry-run --skip-notes
```

### `release-icons.sh` — Icon packages

Detects which icon packages have changed since their last tagged release, prompts for version bumps, then builds, packs, pushes to NuGet, tags, and creates the `develop` → `main` PR.

Flags:

| Flag | Effect |
|------|--------|
| `--dry-run` | Walk through all steps without executing any |
| `--clear-cache` | Clear the NuGet HTTP cache before building |

### Git Tag Naming Convention

Tags follow the pattern `<package>/v<version>` and are created by the release scripts — you do not normally tag by hand.

| Package | Tag prefix |
|---------|-----------|
| Primitives | `primitives/v` |
| Components | `components/v` |
| Icons.Lucide | `icons-lucide/v` |
| Icons.Heroicons | `icons-heroicons/v` |
| Icons.Feather | `icons-feather/v` |
| Icons.FontAwesome | `icons-fontawesome/v` |
| PdfViewer | `pdfviewer/v` |

Examples: `primitives/v3.13.0`, `components/v3.13.0`, `icons-lucide/v2.0.1`, `icons-fontawesome/v2.0.0`

### MinVer Versioning

Each project uses [MinVer](https://github.com/adamralph/minver) to calculate the package version from git tags.

**Configuration** (in each `.csproj`):
```xml
<MinVerTagPrefix>primitives/v</MinVerTagPrefix>  <!-- or icons-lucide/v, etc. -->
<MinVerDefaultPreReleaseIdentifiers>beta.0</MinVerDefaultPreReleaseIdentifiers>
```

**How it works:**
- Tag `primitives/v1.0.0-beta.4` → Version `1.0.0-beta.4`
- Tag `icons-lucide/v1.0.3` → Version `1.0.3`
- Tag `components/v2.0.0` → Version `2.0.0`
- No matching tag → Version `0.0.0-beta.0.<commit-count>`

That last case matters: a package with no tag of its own can only ever produce a `0.0.0-beta.0.x` version, whatever the rest of the repo is versioned at. If a package has never been released, check for its tag first.

## Versioning Strategy

### Independent Versioning

Each package can have a different version number:

```
BlazorBlueprint.Primitives        3.13.0
BlazorBlueprint.Components        3.13.0
BlazorBlueprint.Icons.Lucide      2.0.1
BlazorBlueprint.Icons.Heroicons   2.0.0
BlazorBlueprint.Icons.Feather     2.0.0
BlazorBlueprint.Icons.FontAwesome 2.0.0
BlazorBlueprint.PdfViewer         1.0.0
```

This allows you to:
- Release bug fixes for one package without bumping others
- Evolve packages at different paces
- Clearly communicate changes to consumers

### Semantic Versioning

Follow [Semantic Versioning](https://semver.org/):

- **Major** (1.0.0 → 2.0.0): Breaking changes
- **Minor** (1.0.0 → 1.1.0): New features (backward compatible)
- **Patch** (1.0.0 → 1.0.1): Bug fixes (backward compatible)
- **Pre-release** (1.0.0-beta.1): Beta versions

## Release Checklist

Before releasing a package:

1. ✅ **On the correct branch, up to date** — `develop` for stable/icon releases, or the explicit `--branch` for prereleases
2. ✅ **All changes committed** - no uncommitted files
3. ✅ **Tests passing** — `./scripts/run-tests.sh` for .NET and JavaScript; run the browser suite separately before publication
4. ✅ **README updated** - document new features/changes
5. ✅ **`NUGET_API_KEY` exported** - required for every release, not just the first

## Monitoring Releases

Check what is currently live:

```bash
./devkit/scripts/nuget-versions.sh
```

Packages appear at:
- https://www.nuget.org/packages/BlazorBlueprint.Primitives
- https://www.nuget.org/packages/BlazorBlueprint.Components
- https://www.nuget.org/packages/BlazorBlueprint.Icons.Lucide
- https://www.nuget.org/packages/BlazorBlueprint.Icons.Heroicons
- https://www.nuget.org/packages/BlazorBlueprint.Icons.Feather
- https://www.nuget.org/packages/BlazorBlueprint.Icons.FontAwesome
- https://www.nuget.org/packages/BlazorBlueprint.PdfViewer

**Note:** It may take 5-10 minutes for packages to appear on NuGet.org after publishing.

## Troubleshooting

### Script says "uncommitted changes"

```bash
git status
git add .
git commit -m "Your commit message"
```

### Script says "Must be on the develop branch"

Releases run from `develop`; the script opens the `develop` → `main` PR itself.

```bash
git checkout develop
git pull origin develop
```

### `NUGET_API_KEY environment variable is not set`

Export it as described in [Prerequisites](#prerequisites). It is read from the environment on every run, so a new shell needs it too.

### Package version is `0.0.0-beta.0.x`

There is no git tag for that package's prefix, so MinVer has nothing to derive from. Check with:

```bash
git tag --list 'icons-fontawesome/v*'
```

### Tag already exists

Delete the tag if you need to recreate it:

```bash
# Example for primitives
git tag -d primitives/v1.0.0-beta.4
git push origin :refs/tags/primitives/v1.0.0-beta.4

# Example for icons
git tag -d icons-lucide/v1.0.3
git push origin :refs/tags/icons-lucide/v1.0.3
```

### Package version mismatch

This usually means the git tag doesn't match the MinVer configuration in the `.csproj`.

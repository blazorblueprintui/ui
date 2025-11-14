# Release Guide

This document describes the automated NuGet publishing system for BlazorUI.

## Overview

BlazorUI uses a **monorepo with independent package versioning**. Each of the three packages can be released independently with its own version number:

- **BlazorUI.Primitives** - Headless UI primitives
- **BlazorUI.Components** - Styled components
- **BlazorUI.Icons** - Lucide icon library

## Quick Start

To release a package, simply run the appropriate script:

```bash
./scripts/release-primitives.sh 1.0.0-beta.4
./scripts/release-components.sh 1.1.0-beta.2
./scripts/release-icons.sh 1.0.3
```

That's it! The rest is automated.

## How It Works

### 1. Release Scripts (`scripts/`)

Each package has a dedicated release script that:

1. **Validates** the version format (semantic versioning)
2. **Checks** for uncommitted changes (prevents dirty releases)
3. **Confirms** with you before proceeding
4. **Creates** a git tag (e.g., `primitives/v1.0.0-beta.4`)
5. **Pushes** the tag to GitHub

### 2. Git Tag Naming Convention

Tags follow the pattern: `<package>/v<version>`

Examples:
- `primitives/v1.0.0-beta.4`
- `components/v1.1.0-beta.2`
- `icons/v1.0.3`

### 3. MinVer Versioning

Each project uses [MinVer](https://github.com/adamralph/minver) to automatically calculate the package version from git tags.

**Configuration** (in each `.csproj` file):
```xml
<MinVerTagPrefix>primitives/v</MinVerTagPrefix>
<MinVerDefaultPreReleaseIdentifiers>beta.0</MinVerDefaultPreReleaseIdentifiers>
```

**How it works:**
- Tag `primitives/v1.0.0-beta.4` → Version `1.0.0-beta.4`
- Tag `components/v2.0.0` → Version `2.0.0`
- No matching tag → Version `0.0.0-beta.0.<commit-count>`

### 4. GitHub Actions Automation

When a tag is pushed, GitHub Actions automatically:

1. **Detects** which package to build (from tag prefix)
2. **Restores** dependencies
3. **Builds** the project (Release configuration)
4. **Packs** the NuGet package (MinVer sets version automatically)
5. **Verifies** the package version matches the tag
6. **Publishes** to NuGet.org

**Workflows:**
- `.github/workflows/nuget-publish.yml` - Triggered by tag pushes
- `.github/workflows/ci.yml` - Runs on all PRs and pushes to main

## Versioning Strategy

### Independent Versioning

Each package can have a different version number:

```
BlazorUI.Primitives 1.2.0
BlazorUI.Components 1.1.5
BlazorUI.Icons      1.0.3
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

### Beta Releases

For beta versions, use the format: `X.Y.Z-beta.N`

Examples:
- `1.0.0-beta.1` - First beta
- `1.0.0-beta.2` - Second beta
- `1.0.0` - Stable release

## Release Checklist

Before releasing a package:

1. ✅ **All changes committed** - No uncommitted files
2. ✅ **Tests passing** - Run `dotnet build` and verify
3. ✅ **README updated** - Document new features/changes
4. ✅ **Version decided** - Choose appropriate semantic version
5. ✅ **NUGET_API_KEY configured** - Required for first release

## Setting Up NuGet API Key

### First-Time Setup (Repository Owner)

1. **Get NuGet API key:**
   - Go to https://www.nuget.org/account/apikeys
   - Create a new API key with "Push" permissions
   - Scope it to the BlazorUI.* packages

2. **Add to GitHub Secrets:**
   - Go to repository Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `NUGET_API_KEY`
   - Value: (paste your NuGet API key)

3. **Test the workflow:**
   ```bash
   ./scripts/release-primitives.sh 1.0.0-beta.1
   ```

## Monitoring Releases

### GitHub Actions Dashboard

Monitor releases at: https://github.com/blazorui-net/ui/actions

Each release creates a workflow run showing:
- Build logs
- Pack output
- Publish status
- Direct link to NuGet package

### NuGet.org

Packages appear at:
- https://www.nuget.org/packages/BlazorUI.Primitives
- https://www.nuget.org/packages/BlazorUI.Components
- https://www.nuget.org/packages/BlazorUI.Icons

**Note:** It may take 5-10 minutes for packages to appear on NuGet.org after publishing.

## Troubleshooting

### Script says "uncommitted changes"

```bash
git status
git add .
git commit -m "Your commit message"
```

### Version format error

Ensure version follows semantic versioning:
- ✅ `1.0.0`
- ✅ `1.0.0-beta.1`
- ✅ `2.1.3-alpha.2`
- ❌ `v1.0.0` (no 'v' prefix)
- ❌ `1.0` (must have three parts)

### Tag already exists

Delete the tag if you need to recreate it:

```bash
git tag -d primitives/v1.0.0-beta.4
git push origin :refs/tags/primitives/v1.0.0-beta.4
```

### Package version mismatch

This usually means the git tag doesn't match the MinVer configuration.

Check:
1. Tag format: `primitives/v1.0.0-beta.4` (note the prefix)
2. MinVerTagPrefix in `.csproj`: `<MinVerTagPrefix>primitives/v</MinVerTagPrefix>`

### GitHub Actions failing

Check:
1. NUGET_API_KEY secret is configured
2. Build succeeds locally: `dotnet build -c Release`
3. Workflow logs for specific errors

## Manual Release (Fallback)

If automation fails, you can release manually:

```bash
# Build and pack
dotnet pack src/BlazorUI.Primitives/BlazorUI.Primitives.csproj -c Release -o ./packages

# Publish to NuGet
dotnet nuget push ./packages/BlazorUI.Primitives.*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

**Note:** Manual releases won't have the git tag versioning benefits.

## Best Practices

1. **Release often** - Small, frequent releases are better than large infrequent ones
2. **Test before releasing** - Always run `dotnet build` locally first
3. **Document changes** - Update README or CHANGELOG for significant changes
4. **Use pre-release versions** - Use `-beta.X` suffix until stable
5. **Coordinate dependencies** - If Components depends on new Primitives features, release Primitives first

## Development Workflow

### Working on a feature

```bash
git checkout -b feature/my-new-feature
# Make changes
dotnet build
git commit -m "Add new feature"
git push origin feature/my-new-feature
```

### Releasing the feature

```bash
git checkout main
git merge feature/my-new-feature
git push origin main

# Release with new version
./scripts/release-primitives.sh 1.1.0
```

## CI/CD Pipeline

### Continuous Integration (`.github/workflows/ci.yml`)

Runs on every PR and push to main:
- Builds all projects
- Runs tests (if any)
- Creates NuGet packages (as artifacts)
- Verifies package creation

**Purpose:** Ensure code quality and catch issues early

### Continuous Deployment (`.github/workflows/nuget-publish.yml`)

Triggered by tag pushes:
- Builds specific package
- Publishes to NuGet.org
- Only runs for tagged releases

**Purpose:** Automate releases and reduce human error

## Additional Resources

- [MinVer Documentation](https://github.com/adamralph/minver)
- [Semantic Versioning](https://semver.org/)
- [NuGet Package Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/publish-a-package)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)

---

**Questions or Issues?**

If you encounter problems with the release system, check:
1. This guide's Troubleshooting section
2. GitHub Actions workflow logs
3. Open an issue on the repository

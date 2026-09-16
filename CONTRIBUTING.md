# Contributing to Blazor Blueprint UI

Thank you for your interest in contributing to Blazor Blueprint UI! 🎉
We welcome all contributions — whether it's bug fixes, new features, documentation improvements, or tooling enhancements.

---

## 📋 Table of Contents

- [Getting Started](#getting-started)
- [Branching Strategy](#branching-strategy)
- [Submitting a Pull Request](#submitting-a-pull-request)
- [Commit Messages](#commit-messages)
- [Pull Request Checklist](#pull-request-checklist)
- [Code Style](#code-style)
- [Reporting Bugs](#reporting-bugs)
- [Suggesting Features](#suggesting-features)

---

## Getting Started

1. **Fork** this repository to your own GitHub account
2. **Clone** your fork locally:
   ```bash
   git clone https://github.com/<your-username>/ui.git
   cd ui
   ```
3. **Set the upstream remote** so you can pull in future changes:
   ```bash
   git remote add upstream https://github.com/blazorblueprintui/ui.git
   ```
4. **Download the Tailwind CSS standalone CLI:**

   Blazor Blueprint uses the [Tailwind CSS standalone CLI](https://tailwindcss.com/blog/standalone-cli) — no Node.js or npm required. The binary is not included in the repository and must be downloaded before your first build.

   Run the appropriate script from the `tools/` directory:

   **Linux / macOS:**
   ```bash
   cd tools
   ./install.sh
   ```

   **Windows (PowerShell):**
   ```powershell
   cd tools
   .\install.ps1
   ```
   
   The MSBuild target (`tools/tailwind.targets`) is imported by `BlazorBlueprint.Components` and runs automatically on every build — no manual Tailwind invocation needed.

5. **Install dependencies** and verify the project builds:
   ```bash
   dotnet build
   ```

6. **Run regression tests:**
   ```bash
   dotnet test tests/BlazorBlueprint.Tests
   node --test tests/js/*.test.mjs
   python3 scripts/check-static-css.py
   ```
   The JavaScript interaction tests use Node.js 20+ and its built-in test runner, with no npm dependencies. Node.js is only needed for these development tests, not for building or consuming the library. Performance checks count query rows, filter evaluations, and interop callbacks rather than relying on machine-specific timing.

   The CSS check requires Python 3 and a solution build. It verifies that each demo host's static asset metadata and compressed stylesheets match the generated CSS. With a freshly started demo, also check HTTP delivery:
   ```bash
   python3 scripts/check-static-css.py --base-url http://localhost:7172
   ```
   Stop the demo before rebuilding and restart it afterward; this project does not support hot reload. Tailwind must run before static asset discovery so that fingerprints and compressed files describe the current CSS.

When updating bundled JavaScript, CSS, or icon data, preserve upstream copyright headers and refresh the affected package's `THIRD-PARTY-NOTICES.txt` from the corresponding upstream release. Components and Primitives keep this file under `wwwroot`; icon packages keep it in the project directory. Record the upstream source and version when known, then inspect a locally built `.nupkg` to confirm that its license and notice files are included.

---

## Branching Strategy

> ⚠️ **Important:** Please **do not** target `main` with your pull requests.
> `main` reflects the latest released version of the NuGet package only.

| Branch | Purpose |
|---|---|
| `main` | Released, stable code — mirrors the published NuGet package |
| `develop` | Active development — all features and fixes accumulate here |
| `feature/*` | Individual feature branches, branched off `develop` |
| `fix/*` | Bug fix branches, branched off `develop` |

### For contributors:

- Always **branch off `develop`**:
  ```bash
  git fetch upstream
  git checkout -b feature/my-new-feature upstream/develop
  ```
- Always **target `develop`** as the base branch when opening a pull request
- `main` is only updated by maintainers when cutting a new release

---

## Submitting a Pull Request

1. Ensure your branch is based off the latest `develop`:
   ```bash
   git fetch upstream
   git rebase upstream/develop
   ```
2. Push your branch to your fork:
   ```bash
   git push origin feature/my-new-feature
   ```
3. Open a pull request from your fork's branch **targeting `blazorblueprintui/ui:develop`**
4. Fill in the pull request template completely
5. Wait for a review — we aim to respond within a few days

---

## Commit Messages

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(scope): short description

[optional body]
```

**Types:**
- `feat` — a new feature
- `fix` — a bug fix
- `docs` — documentation changes only
- `refactor` — code change that neither fixes a bug nor adds a feature
- `style` — formatting, missing semicolons, etc.
- `test` — adding or updating tests
- `chore` — build process or tooling changes

**Examples:**
```
feat(Combobox): add SearchQuery parameter for async filtering
fix(InputOTP): implement paste support via JS interop
docs: add CONTRIBUTING.md
```

---

## Pull Request Checklist

Before submitting, please ensure:

- [ ] Your branch is based off `develop` (not `main`)
- [ ] Your PR targets the `develop` branch
- [ ] The project builds without errors: `dotnet build`
- [ ] You have tested your changes on Blazor Server and/or WebAssembly
- [ ] Accessibility and keyboard navigation are unaffected (or improved)
- [ ] Dark mode still works correctly
- [ ] You have updated documentation/demo pages if adding a new feature
- [ ] Your commit messages follow the Conventional Commits format

---

## Code Style

- Follow the existing code patterns and conventions in the codebase
- Use meaningful variable and parameter names
- Add XML doc comments (`/// <summary>`) to public parameters and methods
- Keep components focused — one responsibility per component

---

## Reporting Bugs

Please [open an issue](https://github.com/blazorblueprintui/ui/issues/new) and include:

- A clear description of the bug
- Steps to reproduce
- Expected vs actual behaviour
- Blazor hosting model (Server / WebAssembly / Hybrid)
- Browser and OS
- A minimal reproduction if possible (link to a StackBlitz, GitHub repo, etc.)

---

## Suggesting Features

We love feature suggestions! [Open an issue](https://github.com/blazorblueprintui/ui/issues/new) with:

- A description of the problem you're trying to solve
- Your proposed solution or API design
- Any relevant examples from other component libraries

---

## Thank You ❤️

Every contribution, no matter how small, helps make Blazor Blueprint UI better for everyone.

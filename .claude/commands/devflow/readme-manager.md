---
allowed-tools: Read, Write, Glob, Grep, Task(readme-maintainer), Bash
argument-hint:
description: Update README.md to reflect current project state
model: sonnet
version: 1.0.0
---

> **Windows Users:** This command uses bash syntax. Ensure you have Git Bash installed and are running Claude Code from a Git Bash terminal, not PowerShell. [Installation guide](https://github.com/mathewtaylor/devflow#requirements)

# Update Project README

Analyze the current project state and ensure README.md is fully up-to-date.

## Current State

- README exists: !`test -f README.md && echo "yes" || echo "no"`
- DevFlow initialized: !`test -f .devflow/constitution.md && echo "yes" || echo "no"`

---

## Your Task

Invoke the **readme-maintainer agent** to:
1. Analyze the current project structure and features
2. Review README.md (or create if missing)
3. Identify outdated or missing sections
4. Update README with current project state

The agent will handle all analysis and updates automatically.

---

## Output

After the readme-maintainer agent completes:

```
✅ README.md updated successfully!

Changes made:
- {{List of sections updated}}
- {{New sections added}}
- {{Sections removed}}

Review: README.md
```

---
allowed-tools: Read, Task(architect), Task(state-manager), Bash(test:*), Bash(find:*), Bash(ls:*), Bash(xargs:*)
argument-hint: [feature-name]?
description: Generate technical implementation plan using Architect agent
model: sonnet
version: 1.0.0
---

> **Windows Users:** This command uses bash syntax. Ensure you have Git Bash installed and are running Claude Code from a Git Bash terminal, not PowerShell. [Installation guide](https://github.com/mathewtaylor/devflow#requirements)

# Generate Technical Plan

Create comprehensive technical plan for: **$1** (or active feature)

## Determine Target Feature

If $1 provided: Use it (exact match or partial match by name)
If not: Use active feature from state.json, or most recent pending/active feature

## Current State for Feature

- Feature exists: !`node .devflow/lib/cli.js query feature_exists "$1"`
- Has spec: !`node .devflow/lib/cli.js query has_spec "$1"`
- Has plan: !`node .devflow/lib/cli.js query has_plan "$1"`

## Context for Architect

@.devflow/constitution.md
@.devflow/architecture.md
@.devflow/domains/_index.md
@.devflow/features/{{featureKey}}/spec.md

## Load Relevant Domain Docs

Based on spec concerns and keywords, load matching domain documentation:
- Keywords: auth, login, signup → `@.devflow/domains/security/authentication.md`
- Keywords: permission, role, access → `@.devflow/domains/security/authorization.md`
- Keywords: tenant, organization → `@.devflow/domains/infrastructure/multi-tenancy.md`
- Keywords: cache, redis, performance → `@.devflow/domains/infrastructure/caching.md`
- Keywords: database, table, migration → `@.devflow/domains/data/database-conventions.md`

## Your Task

Invoke **Architect agent** with all loaded context.

The Architect will:
1. Analyze feature spec and existing architecture
2. Propose technical approach aligned with constitution
3. Design components, data models, APIs
4. Address cross-cutting concerns
5. Identify architecture impacts (minor/major)
6. Estimate complexity and risks
7. Create comprehensive plan.md
8. Create ADR if significant decision made

---

## Save Outputs

- **plan.md**: Write to `.devflow/features/{{featureKey}}/plan.md`
- **ADR** (if created): Write to `.devflow/decisions/NNNN-title.md` (next sequential number)

---

## Update State

Invoke State Manager to update:
```json
{
  "phase": "PLAN"
}
```

---

## Handle User Review

If Architect flags `requires_review: true` (major architecture changes):

```
⚠️ REVIEW REQUIRED

Major architecture changes proposed:
- {{change 1}}
- {{change 2}}

Plan: .devflow/features/{{featureKey}}/plan.md

Please review before proceeding.
```

---

## Output

```
✅ Technical plan generated!

Feature: {{Display Name}}
Plan: .devflow/features/{{featureKey}}/plan.md
{{ADR: .devflow/decisions/NNNN-title.md if created}}

Complexity: {{complexity}}
Estimated Tasks: {{count}}
Risk Level: {{level}}

Key Challenges:
- {{challenge 1}}
- {{challenge 2}}

Architecture Impact: {{minor/major}}
{{⚠️ Review required if major}}

Next: Run /tasks to break plan into executable tasks
```

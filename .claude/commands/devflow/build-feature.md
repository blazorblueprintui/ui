---
allowed-tools: Read, Write, AskUserQuestion, Task(state-manager, planner), Bash
argument-hint: ["brief description"]?
description: Streamlined workflow for small features (<2 hours) - combines spec + tasks + execute
model: sonnet
version: 1.0.0
---

> **Windows Users:** This command uses bash syntax. Ensure you have Git Bash installed and are running Claude Code from a Git Bash terminal, not PowerShell. [Installation guide](https://github.com/mathewtaylor/devflow#requirements)

# Build Feature (Streamlined Workflow)

Fast-track small features with minimal documentation overhead.

**Use when:**
- Feature is well-understood and < 2 hours
- Simple changes (bug fix, UI tweak, validation, minor enhancement)
- Want quality gates without heavy process

**Description:** $ARGUMENTS

## Pre-flight Check

**First, verify DevFlow is initialized:**

Check if `.devflow/constitution.md` exists:
- If **YES**: Continue with feature creation
- If **NO**: Stop and inform the user:

  ```
  ❌ DevFlow has not been initialized yet.

  Please run `/devflow:init` first to:
  • Create your project's constitution (principles and standards)
  • Generate architecture documentation
  • Set up the DevFlow state system

  After initialization, you can create features with `/devflow:build-feature`.
  ```

  **Do not proceed without initialization.**

**Second, verify constitution summary exists:**

Check if `.devflow/constitution-summary.md` exists:
- If **YES**: Continue with feature creation
- If **NO**: Stop and inform the user:

  ```
  ❌ Constitution summary not found.

  The constitution-summary.md file is required for efficient code reviews.

  Options:
  a) Run /devflow:init to generate it automatically
  b) Create it manually from constitution.md

  Cannot proceed without constitution summary.
  ```

  **Do not proceed without constitution summary.**

## Current State

- DevFlow initialized: !`test -f .devflow/constitution.md && echo "✓" || echo "✗"`
- Active feature: !`node .devflow/lib/cli.js query active_feature`
- Total features: !`node .devflow/lib/cli.js query feature_count`

## Project Context

@.devflow/constitution-summary.md

<!-- Note: Uses constitution-summary (200-300 tokens) instead of full constitution (2,500 tokens)
     for efficiency. Full constitution available at .devflow/constitution.md if needed. -->

---

## Step 1: Capture Feature Description

**If $ARGUMENTS provided:**
- Use as initial description
- Skip to clarification questions

**If no arguments:**
- Prompt: "Describe the feature you want to build (1-2 sentences):"
- Capture user input

---

## Step 2: Analyze and Confirm Understanding

Using the description, analyze:
- What needs to be changed
- Which files are likely affected
- What the success criteria should be

Present your understanding:
```
I understand you want to:
→ [Brief description of the change]
→ [Expected outcome]
→ [Files/components affected]

Is this correct? (y/n)
```

If no, ask user to clarify and repeat this step.

---

## Step 3: Ask 2-4 Targeted Clarification Questions

Based on gaps in the description, ask **2-4 specific questions**. Choose from:

**Common questions:**
- Which file(s) contain the code to modify? (if unclear)
- What should the error/success message say? (for user-facing changes)
- Should this happen on [event A], [event B], or both? (for behavior timing)
- Any existing library/pattern to follow? (for consistency)
- What should happen if [edge case]? (for error handling)
- What are the acceptance criteria? (if not clear)
- Any dependencies on other features/APIs? (for integrations)

**Keep questions:**
- Specific (not open-ended)
- Actionable (answers drive implementation)
- Minimal (2-4 only)

---

## Step 4: Generate Feature Folder Name

Create feature key: `yyyymmdd-feature-slug`
- Date: Today (yyyymmdd format)
- Slug: From description, lowercased, spaces→hyphens, special chars removed
- Example: "Add email validation" → "20251024-email-validation"

Check if active feature exists. If yes, inform user this will be pending.

---

## Step 5: Create Simplified Specification

Read template: `@../../../.devflow/templates/build-feature-spec.md.template`

Fill placeholders:
- `{{FEATURE_NAME}}` - Display name from description
- `{{CREATED_DATE}}` - ISO 8601 timestamp
- `{{FEATURE_KEY}}` - Generated feature key (yyyymmdd-slug)
- `{{DESCRIPTION}}` - 2-3 sentence description of what this feature does
- `{{RATIONALE}}` - 1-2 sentences on why this is needed
- `{{ACCEPTANCE_CRITERIA}}` - Bulleted list of success criteria
- `{{FILES_AFFECTED}}` - List of files to modify/create
- `{{DEPENDENCIES}}` - External deps, APIs, or "None"

Write to: `.devflow/features/{{featureKey}}/spec.md`

---

## Step 6: Auto-Generate Simple Tasks

Create flat task list (5-10 tasks maximum):

**Standard structure:**
```markdown
# Tasks: {{FEATURE_NAME}}

**Feature ID:** {{featureKey}}
**Generated:** {{ISO timestamp}}
**Workflow:** Build Feature

---

## Task Checklist

- [ ] 1. [Specific implementation step] (Medium)
- [ ] 2. [Another implementation step] (Small)
- [ ] 3. Code review (Opus) (Auto)
- [ ] 4. Generate and run tests (Auto)
- [ ] 5. Update documentation if needed (Small)
- [ ] 6. Mark feature complete (Auto)
```

**Guidelines:**
- Break implementation into 2-5 specific steps (not just "Implement feature")
- Include code review and testing tasks (always)
- Mark review/testing as "(Auto)" - these are quality gates
- Keep tasks small (< 1 hour each)
- No parent tasks (flat structure)
- Use complexity labels: (Small), (Medium), (Large - rare)

Write to: `.devflow/features/{{featureKey}}/tasks.md`

---

## Step 7: Update State

Invoke State Manager agent to add feature:
```json
{
  "{{featureKey}}": {
    "display_name": "{{Display Name}}",
    "status": "pending",
    "phase": "SPEC",
    "workflow_type": "build",
    "current_task": 0,
    "concerns": [],
    "created_at": "{{ISO timestamp}}",
    "completed_at": null,
    "snapshot": null
  }
}
```

**If active_feature is null:**
- Also set `active_feature: "{{featureKey}}"`
- Update status to "active"

---

## Step 8: Confirm Ready to Execute

Display summary:
```
✅ Build-feature setup complete!

Feature: {{Display Name}}
Workflow: Build (streamlined)
Location: .devflow/features/{{featureKey}}/
Tasks: {{count}} tasks
Quality gates: Code Review (Opus) + Testing

Files:
✓ spec.md (simplified specification)
✓ tasks.md ({{count}} tasks)

Ready to start implementation? (y/n)
```

**If yes:**
- Transition to EXECUTE phase via State Manager
- Begin executing tasks sequentially
- Follow same execution flow as `/execute` command:
  - For each task:
    1. Implement code
    2. Run code review gate (Opus + extended thinking)
    3. Generate and run tests
    4. Log to implementation.md (streamlined format)
    5. Update tasks.md checkbox
    6. Ask: Continue? (y/n/skip/pause)

**If no:**
- Keep phase=SPEC
- User can run `/execute` later to start

**Quality gates (same as full workflow):**
- Code review: Opus + extended thinking
- Testing: Generate tests + validate
- Auto-fix retries: Max 3 attempts
- User fallback if gates fail

---

## Step 9: On Completion

When all tasks complete:

1. **Create retrospective:**
   - Read template: `@../../../.devflow/templates/build-feature-retrospective.md.template`
   - Fill with brief bullets (3-5 per section)
   - Write to: `.devflow/features/{{featureKey}}/retrospective.md`

2. **Clean up snapshot** (if exists):
   - Delete `.devflow/features/{{featureKey}}/snapshot.md`

3. **Update state:**
   - Set phase=DONE
   - Set status=completed
   - Set completed_at timestamp
   - Set snapshot=null
   - Clear active_feature

4. **Output:**
```
✅ Feature complete!

Feature: {{Display Name}}
Duration: {{duration}}
Tasks completed: {{count}}
Files modified: {{list}}
Tests added: {{count}}

View:
- Implementation: .devflow/features/{{featureKey}}/implementation.md
- Retrospective: .devflow/features/{{featureKey}}/retrospective.md
```

---

## Pause Support

User can pause at any task by choosing "pause" option.

**On pause:**
1. Offer snapshot creation (same as full workflow)
2. Set status=paused in state.json
3. Keep current_task position
4. Clear active_feature

**On resume:**
- Run `/execute` (without arguments)
- Load snapshot if exists
- Resume from current_task

---

## Important Notes

- **No plan.md:** Planning happens inline during execution
- **Same quality gates:** No compromise on code review + testing
- **Streamlined docs:** Less verbose logging, brief retrospective
- **Flat tasks:** No hierarchical parent/subtask structure
- **Fast workflow:** Spec → Tasks → Execute in single command
- **Full flexibility:** Can pause/resume like full workflow

---

## When to Use Full Workflow Instead

Suggest full workflow (`/spec` → `/plan` → `/tasks` → `/execute`) if:
- Feature will take > 2-4 hours
- Significant architectural decisions needed
- Multiple components/modules affected
- Cross-cutting concerns to consider
- ADR documentation required

For those cases, recommend:
```
This seems complex. Consider using the full workflow:
/spec [feature-name]  # Comprehensive specification
/plan                 # Architect agent (Opus + thinking)
/tasks                # Detailed task breakdown
/execute              # Implementation with checkpoints
```

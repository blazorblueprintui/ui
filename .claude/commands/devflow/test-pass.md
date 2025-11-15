---
allowed-tools: Read, Edit, Task(state-manager), Bash
argument-hint: <criterion-number>
description: Mark acceptance criterion as passing
model: haiku
version: 1.0.0
---

> **Windows Users:** This command uses bash syntax. Ensure you have Git Bash installed and are running Claude Code from a Git Bash terminal, not PowerShell. [Installation guide](https://github.com/mathewtaylor/devflow#requirements)

# Mark Acceptance Criterion as Passing

Mark criterion **$1** as tested and passing for active feature.

## Current State

- Active feature: !`node .devflow/lib/cli.js query active_feature`

## Your Task

Mark an acceptance criterion as passing and update validation metrics.

---

## Step 1: Validate Input

**1. Check criterion number provided:**
- Must have $1 argument (criterion number)
- Must be a positive integer
- If missing or invalid: Error "Usage: /test-pass <criterion-number>"

**2. Check active feature:**
- Read state.json to get active_feature
- If null: Error "No active feature. Run /validate first."

**3. Verify validation phase:**
- Check feature.phase in state.json
- If not "VALIDATE": Error "Feature not in validation phase. Run /validate first."

---

## Step 2: Verify Criterion Exists

**1. Read validation.md:**

Load: `.devflow/features/{{featureKey}}/validation.md`

**2. Parse acceptance criteria checklist:**

Find section "## Acceptance Criteria Testing"

Extract checklist items:
```markdown
- [ ] 1. First criterion
- [ ] 2. Second criterion
- [x] 3. Third criterion (already checked)
```

**3. Validate criterion number:**
- Check if criterion $1 exists in list
- Count total criteria
- If $1 > total: Error "Criterion #$1 not found. Valid criteria: 1-{{total}}"
- If criterion already checked `[x]`: Info "Criterion #$1 already marked passing."

---

## Step 3: Mark Criterion in validation.md

**Use Edit tool to update validation.md:**

Find exact line: `- [ ] $1. {{description}}`
Replace with: `- [x] $1. {{description}}`

Example:
- Old: `- [ ] 3. User can log in with valid credentials`
- New: `- [x] 3. User can log in with valid credentials`

**If Edit fails:**
- Retry once with more context
- If still fails: Show error, suggest manual edit

---

## Step 4: Update State Metrics

**Calculate new metrics:**

Parse validation.md to count:
- Total criteria: count all `- [ ]` or `- [x]` lines
- Passed: count all `[x]` lines (including newly marked one)
- Pending: total - passed

**Invoke State Manager:**

Use Task tool:
```
Task tool invocation:
- subagent_type: "state-manager"
- description: "Update validation metrics"
- prompt: """
  Update feature {{featureKey}} validation:
  - Set criteria_passed: {{passed_count}}
  - Set criteria_pending: {{pending_count}}
  - Validate schema before writing

  Return success status.
  """
```

**Handle State Manager response:**

If `success: false`:
- Show error details
- Offer retry or continue without state update

If `success: true`:
- Continue to output

---

## Step 5: Display Progress

```
✓ Criterion #$1 marked as passing

Progress: {{passed}}/{{total}} criteria passed ({{percent}}%)

{{#if all_passed}}
🎉 All criteria passed!

Run /validate-complete to finish validation.
{{else}}
Remaining criteria to test:
{{#each pending_criteria}}
- [ ] {{number}}. {{description}}
{{/each}}
{{/if}}

{{#if open_issues}}
⚠️ Note: {{open_issues}} open issue(s) still need fixing
View status: /validate-status
{{/if}}
```

---

## Step 6: Check for Completion

**If all criteria passed AND no open issues:**
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ Validation Requirements Met
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

All acceptance criteria: ✓ Passed
Open issues: 0

Ready to complete validation!

Run: /validate-complete
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

---

## Notes

- **Simple validation**: Marks criterion checkbox and updates state
- **Progress tracking**: Shows what's left to test
- **Completion detection**: Automatically detects when ready to finalize
- **Issue awareness**: Reminds about open issues that need fixing

Continue testing criteria and marking them as you go!

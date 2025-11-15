---
allowed-tools: Read, Write, Task(state-manager), Bash
argument-hint: [feature-name]?
description: Start validation phase - test acceptance criteria and report bugs
model: sonnet
version: 1.0.0
---

> **Windows Users:** This command uses bash syntax. Ensure you have Git Bash installed and are running Claude Code from a Git Bash terminal, not PowerShell. [Installation guide](https://github.com/mathewtaylor/devflow#requirements)

# Start Feature Validation

Begin testing and validation for: **$1** (or active feature)

## Current State

- Active feature: !`node .devflow/lib/cli.js query active_feature`
- Feature exists: !`node .devflow/lib/cli.js query feature_exists "$1"`

## Your Task

Transition feature from EXECUTE → VALIDATE phase and set up validation tracking.

---

## Setup Phase

**1. Determine target feature:**
- If $1 provided: Find matching feature key in state.json
- If not: Use active_feature from state.json
- If none: Error "No active feature. Specify feature name or activate one."

**2. Validate feature state:**
- Feature must be in EXECUTE phase
- If phase is SPEC, PLAN, or TASKS: Error "Feature not ready for validation. Run /execute first."
- If phase is VALIDATE: Info "Feature already in validation. Use /test-fail to report bugs."
- If phase is DONE: Error "Feature already complete."

**3. Load context:**

@../../../.devflow/constitution.md
@../../../.devflow/features/{{featureKey}}/spec.md

Parse acceptance criteria from spec.md.

---

## Parse Acceptance Criteria

Read spec.md and extract the "## Acceptance Criteria" section.

**Expected format in spec.md:**
```markdown
## Acceptance Criteria
1. First criterion description
2. Second criterion description
3. Third criterion description
```

**Extract criteria:**
- Parse numbered list items (1., 2., 3., etc.)
- Store each criterion with its number and description
- Count total criteria

**If no criteria found:**
```
⚠️ Warning: No acceptance criteria found in spec.md

This feature has no defined success criteria. Validation will track
bugs but cannot verify acceptance criteria.

Continue with validation anyway? (y/n)
```

If no, abort. If yes, continue with 0 criteria.

---

## Create Validation Document

**1. Read template:**

@../../../.devflow/templates/validation.md.template

**2. Prepare template data:**
```javascript
FEATURE_NAME = feature.display_name
TIMESTAMP = new Date().toISOString()
TOTAL_CRITERIA = criteria.length
CRITERIA_CHECKLIST = criteria.map((c, i) => `- [ ] ${i+1}. ${c.description}`).join('\n')
```

**3. Replace placeholders and create file:**

Use Write tool to create: `.devflow/features/{{featureKey}}/validation.md`

Replace all template placeholders:
- `{{FEATURE_NAME}}` → display name
- `{{TIMESTAMP}}` → ISO timestamp
- `{{TOTAL_CRITERIA}}` → criteria count
- `{{CRITERIA_CHECKLIST}}` → formatted criteria list

---

## Initialize Validation State

**Invoke State Manager agent:**

Use Task tool to invoke state-manager agent:
```
Task tool invocation:
- subagent_type: "state-manager"
- description: "Initialize validation phase"
- prompt: """
  Update feature {{featureKey}}:
  - Set phase: "VALIDATE"
  - Add validation object:
    {
      "started_at": "{{ISO timestamp}}",
      "criteria_total": {{count}},
      "criteria_passed": 0,
      "criteria_failed": 0,
      "criteria_pending": {{count}},
      "issues": []
    }

  Validate schema before writing. Return success status.
  """
```

**Handle State Manager response:**

If `success: false`:
- Show error details
- Offer retry or manual fix options

If `success: true`:
- Continue to output display

---

## Display Validation Instructions

```
✅ Validation phase started!

Feature: {{display_name}}
Location: .devflow/features/{{featureKey}}/validation.md
Phase: VALIDATE
Acceptance Criteria: {{count}}

─────────────────────────────────────────────────────────────
📋 Testing Instructions
─────────────────────────────────────────────────────────────

**Your job:** Test the feature against its acceptance criteria.

**For each criterion:**
1. Test the functionality manually or with automated tests
2. If tests pass: /test-pass <criterion-number>
3. If bugs found: /test-fail "<error log or description>"

**Commands:**

/test-fail "error description or paste logs here"
  → Reports bugs/failures with intelligent analysis
  → Creates fix tasks automatically
  → Re-runs code review and testing on fixes

/test-pass <criterion-number>
  → Marks criterion as passing
  → Updates validation metrics

/validate-status
  → View validation progress
  → See open/fixed issues
  → Check criteria completion

/validate-complete
  → Finalize validation (all criteria must pass)
  → Transitions to DONE phase

─────────────────────────────────────────────────────────────

**Acceptance Criteria to Test:**

{{#each criteria}}
{{number}}. {{description}}
{{/each}}

Start testing and use /test-fail to report any bugs you find.
```

---

## Notes

- **Intelligent bug analysis**: /test-fail uses validation-analyzer agent (Opus + extended thinking) to deeply analyze errors
- **Quality gates**: All fixes go through code review and testing before being marked complete
- **Iterative validation**: Fix bugs, re-test, repeat until all criteria pass
- **Context preservation**: validation.md and state.json track all progress

Begin systematic feature validation with confidence!

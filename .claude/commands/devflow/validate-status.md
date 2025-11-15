---
allowed-tools: Read, Bash
argument-hint: [feature-name]?
description: View validation progress dashboard
model: haiku
version: 1.0.0
---

> **Windows Users:** This command uses bash syntax. Ensure you have Git Bash installed and are running Claude Code from a Git Bash terminal, not PowerShell. [Installation guide](https://github.com/mathewtaylor/devflow#requirements)

# Validation Status Dashboard

View validation progress for: **$1** (or active feature)

## Current State

- Active feature: !`node .devflow/lib/cli.js query active_feature`

## Your Task

Display comprehensive validation status with criteria, issues, and metrics.

---

## Step 1: Determine Target Feature

**1. Find feature:**
- If $1 provided: Find matching feature key in state.json
- If not: Use active_feature from state.json
- If none: Error "No feature in validation. Run /validate first."

**2. Verify validation phase:**
- Check feature.phase in state.json
- If not "VALIDATE": Error "Feature '{{display_name}}' is in {{phase}} phase, not VALIDATE."

---

## Step 2: Gather Validation Data

**1. Read state.json validation object:**
```javascript
{
  started_at: "timestamp",
  criteria_total: N,
  criteria_passed: N,
  criteria_failed: N,
  criteria_pending: N,
  issues: [...]
}
```

**2. Read validation.md:**

Load: `.devflow/features/{{featureKey}}/validation.md`

Parse:
- Acceptance criteria checklist (extract `[ ]` vs `[x]` status)
- Issues section (may have manual notes)

**3. Calculate metrics:**
- Time in validation: `now - started_at` (in minutes, hours, or days)
- Completion percentage: `(criteria_passed / criteria_total) * 100`
- Issues by status: count open, fixing, fixed, closed
- Issues by severity: count CRITICAL, HIGH, MEDIUM, LOW

---

## Step 3: Display Status Dashboard

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📊 Validation Status: {{display_name}}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

**Phase:** VALIDATE
**Status:** {{status_message}}
**Started:** {{started_at}} ({{duration}} ago)

─────────────────────────────────────────────────────────────
📋 Acceptance Criteria Progress
─────────────────────────────────────────────────────────────

{{progress_bar}} {{percent}}%

✓ Passed:  {{criteria_passed}}/{{criteria_total}}
✗ Failed:  {{criteria_failed}}/{{criteria_total}}
⏳ Pending: {{criteria_pending}}/{{criteria_total}}

{{#each criteria}}
{{checkbox}} {{number}}. {{description}}
{{/each}}

─────────────────────────────────────────────────────────────
🐛 Issues Summary
─────────────────────────────────────────────────────────────

Total Issues: {{issues.length}}

By Status:
  🔴 Open:   {{open_count}}
  🔧 Fixing: {{fixing_count}}
  ✅ Fixed:  {{fixed_count}}
  📋 Closed: {{closed_count}}

By Severity:
  🚨 CRITICAL: {{critical_count}}
  ⚠️  HIGH:     {{high_count}}
  ⚡ MEDIUM:   {{medium_count}}
  📝 LOW:      {{low_count}}

─────────────────────────────────────────────────────────────
📝 Issue Details
─────────────────────────────────────────────────────────────

{{#if no_issues}}
No issues found yet. Great work! ✨
{{else}}
{{#each issues}}
#{{id}} [{{severity}}] {{description}}
  Status: {{status}}
  {{#if criterion}}Affects: Criterion #{{criterion}}{{/if}}
  Found: {{found_at}}
  {{#if fixed_at}}Fixed: {{fixed_at}}{{/if}}
  {{#if tasks}}Tasks: {{tasks}}{{/if}}

{{/each}}
{{/if}}

─────────────────────────────────────────────────────────────
⏱️  Metrics
─────────────────────────────────────────────────────────────

Time in validation: {{duration}}
Issues found: {{issues.length}}
Issues fixed: {{fixed_count}}
Fix success rate: {{#if issues.length}}{{(fixed_count/issues.length*100).toFixed(0)}}%{{else}}N/A{{/if}}

─────────────────────────────────────────────────────────────
🎯 Next Steps
─────────────────────────────────────────────────────────────

{{#if all_criteria_passed_and_no_open_issues}}
✅ Ready to complete!

All criteria passed and no open issues.

Run: /validate-complete
{{else}}
{{#if pending_criteria}}
Test remaining criteria:
{{#each pending_criteria}}
  - Criterion #{{number}}: {{description}}
{{/each}}

When passing: /test-pass <criterion-number>
When bugs found: /test-fail "<error description>"
{{/if}}

{{#if open_issues}}
Fix open issues:
{{#each open_issues}}
  - Issue #{{id}} ({{severity}}): {{description}}
{{/each}}

Report bugs: /test-fail "<error description>"
{{/if}}
{{/if}}

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

---

## Status Message Logic

Determine appropriate status message:

**"🎉 Validation Complete"** if:
- criteria_passed === criteria_total
- open_issues === 0
- fixing_issues === 0

**"⚠️ Validation Blocked"** if:
- critical_issues > 0 AND status === "open"

**"🔧 Fixes in Progress"** if:
- fixing_issues > 0

**"📋 Testing in Progress"** (default):
- criteria_pending > 0

---

## Progress Bar Visual

Generate ASCII progress bar:
```javascript
const width = 30;
const filled = Math.floor((criteria_passed / criteria_total) * width);
const bar = '█'.repeat(filled) + '░'.repeat(width - filled);
```

Example:
```
███████████████████░░░░░░░░░░░ 63%
```

---

## Notes

- **Comprehensive overview**: All validation data in one place
- **Visual progress**: Easy to see what's done and what remains
- **Issue tracking**: Full visibility into bugs and fixes
- **Actionable guidance**: Clear next steps based on current state

Check validation status anytime during testing!

---
allowed-tools: Read, Write, Bash, Task(state-manager), AskUserQuestion
argument-hint: [feature-name]?
description: Complete validation phase and transition to DONE
model: sonnet
version: 1.0.0
---

> **Windows Users:** This command uses bash syntax. Ensure you have Git Bash installed and are running Claude Code from a Git Bash terminal, not PowerShell. [Installation guide](https://github.com/mathewtaylor/devflow#requirements)

# Complete Feature Validation

Finalize validation and mark feature complete: **$1** (or active feature)

## Current State

- Active feature: !`node .devflow/lib/cli.js query active_feature`

## Your Task

Validate all criteria passed, close validation phase, update architecture, generate retrospective, and mark feature complete.

---

## Step 1: Determine Target Feature

**1. Find feature:**
- If $1 provided: Find matching feature key in state.json
- If not: Use active_feature from state.json
- If none: Error "No feature in validation. Run /validate first."

**2. Verify validation phase:**
- Check feature.phase in state.json
- If not "VALIDATE": Error "Feature is in {{phase}} phase. Must be in VALIDATE to complete."

---

## Step 2: Validate Completion Requirements

**Read state.json validation object:**

Check requirements:
1. **All criteria passed:** `criteria_passed === criteria_total`
2. **No open issues:** Count issues with status "open" or "fixing" === 0

**If requirements not met:**

```
⚠️ Validation not ready to complete

{{#if criteria_pending > 0}}
✗ {{criteria_pending}} criteria still pending
  Run: /test-pass <criterion-number> for each passing criterion
{{/if}}

{{#if open_issues > 0}}
✗ {{open_issues}} open issue(s) must be fixed
  Run: /test-fail "<error>" to report and fix bugs
{{/if}}

{{#if fixing_issues > 0}}
✗ {{fixing_issues}} issue(s) currently being fixed
  Complete fixes before finalizing validation
{{/if}}

View status: /validate-status
```

**Ask user to confirm override (optional):**
```
Force complete validation anyway? (not recommended)
This will mark the feature complete despite unmet requirements.

(y/n):
```

If no: Abort
If yes: Continue with warnings logged

---

## Step 3: Display Completion Summary

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🎉 Validation Complete!
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Feature: {{display_name}}

✓ Criteria tested: {{criteria_passed}}/{{criteria_total}}
✓ Issues found: {{total_issues}}
✓ Issues fixed: {{fixed_issues}}
✓ Time in validation: {{duration}}

─────────────────────────────────────────────────────────────

Finalizing feature...
```

---

## Step 4: Load Context for Final Steps

@../../../.devflow/constitution.md
@../../../.devflow/architecture.md
@../../../.devflow/features/{{featureKey}}/spec.md
@../../../.devflow/features/{{featureKey}}/plan.md
@../../../.devflow/features/{{featureKey}}/implementation.md
@../../../.devflow/features/{{featureKey}}/validation.md

---

## Step 5: Update Architecture

**Ask user:**
```
Update architecture.md with changes from this feature? (y/n)

This analyzes implemented changes and updates living documentation.
```

**If yes:**

1. **Analyze feature changes:**
   - Read implementation.md for all files modified
   - Read validation.md for issues and patterns discovered
   - Identify:
     - New services or components
     - New API endpoints
     - Database schema changes
     - New architectural patterns used
     - Integration points added

2. **Categorize updates:**

   **Minor updates** (auto-apply without asking):
   - New API endpoints in existing services
   - New utility functions
   - Minor configuration changes
   - Documentation updates

   **Major updates** (show diff, require approval):
   - New architectural layers or patterns
   - New technology stack components
   - New cross-cutting concerns
   - New integration patterns
   - Significant pattern changes

3. **For major updates:**
   ```
   Proposed architecture.md updates:

   {{show_diff}}

   Apply these updates? (y/n/edit)
   ```

4. **Update architecture.md**
   - Apply approved changes
   - Preserve existing structure
   - Add timestamp comment

**If no:**
- Skip architecture update
- Log decision in retrospective

---

## Step 6: Generate Retrospective

**Calculate metrics:**
- Total implementation time (if trackable from timestamps)
- Tasks completed (read tasks.md)
- Tests added (count from implementation.md)
- Issues found and fixed during validation
- Review cycles

**Create `.devflow/features/{{featureKey}}/retrospective.md`:**

```markdown
# Feature Retrospective: {{display_name}}

**Completed:** {{ISO timestamp}}
**Feature ID:** {{featureKey}}
**Duration:** {{duration if trackable}}
**Implementation Tasks:** {{task_count}}
**Validation Issues:** {{issue_count}} found, {{fixed_count}} fixed

---

## Summary

{{one-paragraph summary of what was built}}

---

## What Went Well

{{analyze implementation.md and validation.md}}

Examples:
- Clear specification made implementation straightforward
- Code review caught security issue early (task 3.2)
- Test coverage prevented regressions
- Validation phase found edge case bugs before production

---

## Challenges Faced

{{analyze issues from validation.md, review failures from implementation.md}}

Examples:
- Authentication flow required 2 design iterations
- Database query optimization needed after performance testing
- Edge case with null user emails required validation fix (issue #3)

**How resolved:**
{{for each challenge, explain resolution}}

---

## Lessons Learned

{{extract patterns and insights}}

Examples:
- Always validate external API responses before processing
- Database indexes critical for queries with multiple joins
- User input validation should happen at API boundary
- Integration tests would have caught validation issue #2 earlier

---

## Technical Debt Created

{{identify shortcuts, TODOs, or compromises}}

{{#if any_debt}}
**Items to address in future:**
- {{debt_item_1}}
- {{debt_item_2}}

**Priority:** {{HIGH|MEDIUM|LOW}}
**Estimated effort:** {{hours}}
{{else}}
None identified. Implementation followed all standards and best practices.
{{/if}}

---

## Quality Metrics

**Code Review:**
- Tasks reviewed: {{tasks_with_review}}
- Issues found: {{review_issues}}
- Average attempts to pass: {{avg_review_attempts}}

**Testing:**
- Tests added: {{test_count}}
- Test failures during implementation: {{test_failures}}
- Coverage: {{coverage if available}}

**Validation:**
- Acceptance criteria: {{criteria_total}}
- Issues found: {{validation_issues}}
- Critical/High severity: {{critical_high_count}}
- Fix success rate: {{(fixed/total)*100}}%

---

## Recommendations for Future

{{analyze entire feature lifecycle}}

**Process improvements:**
- {{process_recommendation_1}}
- {{process_recommendation_2}}

**Technical improvements:**
- {{technical_recommendation_1}}
- {{technical_recommendation_2}}

**Documentation improvements:**
- {{documentation_recommendation_1}}

---

## Related Features

{{#if dependencies}}
**Depends on:**
{{list_dependencies}}
{{/if}}

{{#if enables}}
**Enables future features:**
{{potential_followup_features}}
{{/if}}

---

_Generated automatically by DevFlow. Edit to add personal notes._
```

---

## Step 7: Clean Up Snapshot

**If snapshot exists:**

1. Check state.json for `snapshot` field
2. If not null:
   - Construct path: `.devflow/features/{{featureKey}}/snapshot.md`
   - Delete file if exists: `rm .devflow/features/{{featureKey}}/snapshot.md`
   - Confirm: `✓ Snapshot cleared (feature complete)`

---

## Step 8: Mark Feature Complete

**Invoke State Manager:**

Use Task tool:
```
Task tool invocation:
- subagent_type: "state-manager"
- description: "Complete feature"
- prompt: """
  Mark feature {{featureKey}} complete:
  - Set phase: "DONE"
  - Set status: "completed"
  - Set completed_at: "{{ISO timestamp}}"
  - Set snapshot: null
  - Clear active_feature (set to null)
  - Validate schema before writing

  Return success status.
  """
```

**Handle State Manager response:**

If `success: false`:
- Show error
- Offer retry or manual fix

If `success: true`:
- Continue to celebration output

---

## Step 9: Celebration Output

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ Feature Complete: {{display_name}}!
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🎉 Congratulations! This feature has been:
   ✓ Specified
   ✓ Planned
   ✓ Implemented with quality gates
   ✓ Validated and tested
   ✓ Documented

─────────────────────────────────────────────────────────────
📁 Documentation
─────────────────────────────────────────────────────────────

Location: .devflow/features/{{featureKey}}/

✓ spec.md              - Requirements
✓ plan.md              - Technical design
✓ tasks.md             - {{total_tasks}} completed tasks
✓ implementation.md    - Execution log
✓ validation.md        - Testing and issues
✓ retrospective.md     - Lessons learned

─────────────────────────────────────────────────────────────
📊 Metrics
─────────────────────────────────────────────────────────────

Implementation: {{task_count}} tasks
Validation: {{criteria_total}} criteria tested
Issues: {{issue_count}} found, {{fixed_count}} fixed
{{#if duration}}Duration: {{duration}}{{/if}}
{{#if tests_added}}Tests added: {{tests_added}}{{/if}}

─────────────────────────────────────────────────────────────
🏗️  Architecture
─────────────────────────────────────────────────────────────

{{#if architecture_updated}}
✓ architecture.md updated with new patterns
{{else}}
○ architecture.md unchanged
{{/if}}

─────────────────────────────────────────────────────────────
🚀 Next Steps
─────────────────────────────────────────────────────────────

Ready to start a new feature:
- Run: /spec <feature-name>

Or work on another feature:
- View features: /status
- Resume paused feature: /execute <feature-name>

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Great work! 🎊
```

---

## Error Handling

**If requirements not met:**
- Show clear error with specific blockers
- Offer /validate-status to see details
- Allow force-complete with warnings

**If architecture update fails:**
- Log warning
- Continue with completion
- User can update manually later

**If retrospective generation fails:**
- Create basic retrospective template
- Log error for user review

**If state update fails:**
- Critical error - show details
- Offer retry or manual state.json editing
- Do not proceed until state is updated

---

## Notes

- **Validation enforcement**: All criteria must pass and no open issues (or user override)
- **Living documentation**: Architecture.md stays current with implementation
- **Learning capture**: Retrospective preserves insights for future features
- **Clean completion**: Snapshot cleared, state updated, feature marked done

Complete features with confidence and documented learnings!

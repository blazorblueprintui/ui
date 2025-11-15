---
allowed-tools: Read, Write, Edit, Bash, Grep, Glob, Task(validation-analyzer), Task(reviewer), Task(tester), Task(state-manager)
argument-hint: "<error description or paste logs>"
description: Report test failure or bug with intelligent root cause analysis and auto-fix
model: sonnet
version: 1.0.0
---

> **Windows Users:** This command uses bash syntax. Ensure you have Git Bash installed and are running Claude Code from a Git Bash terminal, not PowerShell. [Installation guide](https://github.com/mathewtaylor/devflow#requirements)

# Report Test Failure or Bug

Analyze and fix test failure for active feature.

**Input:** Paste error logs, stack traces, or describe the bug in your own words.

## Current State

- Active feature: !`node .devflow/lib/cli.js query active_feature`

## Your Task

Use validation-analyzer agent (Opus + extended thinking) to deeply analyze the error, create a resolution plan, and implement fixes with quality gates.

---

## Step 1: Validate Context

**1. Check active feature:**
- Read state.json to get active_feature
- If null: Error "No active feature. Run /validate first."

**2. Verify validation phase:**
- Check feature.phase in state.json
- If not "VALIDATE": Error "Feature not in validation phase. Run /validate first."

**3. Load feature context:**

@../../../.devflow/constitution.md
@../../../.devflow/architecture.md
@../../../.devflow/features/{{featureKey}}/spec.md
@../../../.devflow/features/{{featureKey}}/plan.md
@../../../.devflow/features/{{featureKey}}/tasks.md
@../../../.devflow/features/{{featureKey}}/implementation.md
@../../../.devflow/features/{{featureKey}}/validation.md

**Load relevant domain docs** based on feature's `concerns` array from state.json

---

## Step 2: Invoke Validation Analyzer

**Display analysis header:**
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🔍 Analyzing Error with Deep Reasoning
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Input received: {{first 200 chars of user input}}...

Loading feature context and analyzing with extended thinking...
```

**Use Task tool to invoke validation-analyzer agent:**

```
Task tool invocation:
- subagent_type: "validation-analyzer"
- description: "Analyze test failure"
- prompt: """
  Analyze this test failure or bug for feature "{{display_name}}".

  **User Input (raw error/description):**
  {{$ARGUMENTS}}

  **Feature Context:**
  - Feature: {{display_name}}
  - Phase: VALIDATE
  - Active testing acceptance criteria from spec.md

  **Context Files:**
  @spec.md (requirements and acceptance criteria)
  @plan.md (technical design)
  @tasks.md (what was implemented)
  @implementation.md (implementation log)
  @validation.md (current validation status)
  @constitution.md (standards)
  @architecture.md (system patterns)

  Perform deep root cause analysis using extended thinking.

  Return structured JSON with:
  - understanding (what was being tested)
  - error_analysis (type, location, message)
  - root_cause (what, why, code path, pattern)
  - severity (CRITICAL/HIGH/MEDIUM/LOW)
  - resolution_strategy (approach to fix)
  - fix_tasks (atomic, actionable tasks with file paths)

  See your agent prompt for full output format.
  """
```

**Parse agent response:**
- Extract JSON from response
- Validate all required fields present
- If parsing fails: Display error, ask user to try again with clearer description

---

## Step 3: Display Analysis Results

```
✓ Analysis complete

─────────────────────────────────────────────────────────────
📊 Root Cause Analysis
─────────────────────────────────────────────────────────────

**Understanding:**
Feature: {{understanding.feature}}
{{#if understanding.criterion_affected}}Criterion: #{{understanding.criterion_affected}}{{/if}}
Testing: {{understanding.user_was_testing}}

**Error:**
Type: {{error_analysis.error_type}}
Location: {{error_analysis.error_location}}
Message: {{error_analysis.error_message}}

**Root Cause:**
What: {{root_cause.what_went_wrong}}
Why: {{root_cause.why_it_went_wrong}}
Pattern: {{root_cause.pattern}}

**Severity:** {{severity}}

**Resolution Strategy:**
{{resolution_strategy}}

─────────────────────────────────────────────────────────────
🔧 Fix Plan ({{fix_tasks.length}} tasks)
─────────────────────────────────────────────────────────────

{{#each fix_tasks}}
{{id}}. {{description}}
    Files: {{files_to_modify}}
    Effort: {{complexity}} ({{estimated_minutes}} min)
{{/each}}

─────────────────────────────────────────────────────────────

Implement fixes now? (y/n/manual)
  y - Auto-implement with code review and testing
  n - Cancel (issue logged but not fixed)
  manual - Show me the plan, I'll fix it myself
```

**Handle user choice:**

- **y**: Continue to Step 4 (implement fixes)
- **n**: Skip to Step 6 (log issue only, mark as open)
- **manual**: Display detailed fix plan, skip implementation, mark issue for manual fix

---

## Step 4: Create Issue in State

**Generate issue ID:**
- Read state.json validation.issues array
- Find max issue.id (or 0 if empty)
- Next ID = max + 1

**Create issue object:**
```javascript
{
  "id": nextId,
  "description": `${error_analysis.error_type}: ${root_cause.what_went_wrong}`,
  "severity": severity,
  "status": "fixing",  // open | fixing | fixed | closed
  "criterion": understanding.criterion_affected || null,
  "tasks": fix_tasks.map(t => t.id),  // ["1.1", "1.2", "1.3"]
  "found_at": new Date().toISOString(),
  "fixed_at": null
}
```

**Invoke State Manager to add issue:**

Use Task tool:
```
Update feature {{featureKey}} validation:
- Add issue: {{issue_object}}
- Increment criteria_failed if criterion_affected is not null
- Validate schema before writing
```

---

## Step 5: Implement Fix Tasks

**For each fix task in fix_tasks array:**

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Fix Task {{task.id}}: {{task.description}}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Files to modify: {{task.files_to_modify}}
Complexity: {{task.complexity}}
```

### 5A. Implement the fix

Based on task description and suggested fix from analyzer:
- Read the files to modify
- Implement the fix following constitution standards
- Apply resolution strategy from analysis
- Log what you're doing

### 5B. Code Review Gate

**Skip review if:**
- Task is documentation only
- Task is configuration change

**For code fixes:**

Invoke reviewer agent (same pattern as /execute command):
```
Task tool invocation:
- subagent_type: "reviewer"
- description: "Review validation fix"
- prompt: """
  Review this fix for validation issue #{{issue.id}}.

  **Original Issue:**
  {{issue.description}}
  Severity: {{issue.severity}}

  **Fix Applied:**
  {{summary of changes made}}

  **Files Modified:**
  {{list files with changes}}

  **Context:**
  @constitution.md
  @spec.md (original requirements)

  Review for:
  - Does fix address root cause?
  - Are there side effects?
  - Code quality and standards
  - Security implications

  Return: APPROVED or CHANGES_REQUIRED with specific feedback
  """
```

**Handle review result:**

If `CHANGES_REQUIRED`:
```
⚠️ Code review found issues:

{{critical_issues}}
{{warnings}}

Fixing issues... (attempt {{attempt}}/3)
```
- Fix the issues
- Re-invoke reviewer
- Max 3 attempts, then ask user:
  ```
  Unable to pass review after 3 attempts.

  Options:
  a) Continue anyway (not recommended)
  b) Skip this fix task
  c) Pause, let me fix manually
  d) Show extended reasoning

  Choose:
  ```

If `APPROVED`:
```
✓ Code review passed
```

### 5C. Testing Gate

**Skip testing if:**
- Task is documentation
- Task is configuration
- Task is test file itself

**For code fixes:**

Invoke tester agent (same pattern as /execute command):
```
Task tool invocation:
- subagent_type: "tester"
- description: "Test validation fix"
- prompt: """
  Test this fix for validation issue #{{issue.id}}.

  **Original Bug:**
  {{issue.description}}

  **Fix Applied:**
  {{summary of changes}}

  **Files Modified:**
  {{list files}}

  Generate and run tests to verify:
  - Fix resolves the original error
  - No regressions introduced
  - Edge cases handled

  Return: PASS or FAIL with test results
  """
```

**Handle test result:**

If `FAIL`:
```
✗ Tests failed

{{failure_details}}

Fixing... (attempt {{attempt}}/3)
```
- Analyze failures
- Fix code or tests
- Re-run tests
- Max 3 attempts, then ask user

If `PASS`:
```
✓ Tests passed
```

### 5D. Log fix to validation.md

Append to validation.md:
```markdown
### Issue #{{issue.id}}: {{issue.description}}

**Found:** {{issue.found_at}}
**Severity:** {{issue.severity}}
{{#if issue.criterion}}**Affects Criterion:** {{issue.criterion}}{{/if}}

**Root Cause:**
{{root_cause.what_went_wrong}}

**Fix Tasks:**
{{#each fix_tasks}}
- [x] {{id}}. {{description}} ({{files_to_modify}})
{{/each}}

**Resolution:**
{{resolution_strategy}}

**Fixed:** {{timestamp}}
**Review:** ✓ Passed
**Tests:** ✓ Passed

---
```

---

## Step 6: Mark Issue Fixed

**Invoke State Manager:**

```
Update feature {{featureKey}} validation issue #{{issue.id}}:
- Set status: "fixed"
- Set fixed_at: "{{ISO timestamp}}"
- Decrement criteria_failed if criterion was affected
- Validate schema
```

---

## Step 7: Re-validation Prompt

```
✅ Issue #{{issue.id}} fixed and tested!

─────────────────────────────────────────────────────────────
🔄 Re-validation Required
─────────────────────────────────────────────────────────────

{{#if issue.criterion}}
Please re-test acceptance criterion #{{issue.criterion}}:
{{criterion_description}}

If passing now: /test-pass {{issue.criterion}}
If still failing: /test-fail "<new error description>"
{{else}}
Please re-test the affected functionality.

If working now: Issue is resolved ✓
If still failing: /test-fail "<new error description>"
{{/if}}

─────────────────────────────────────────────────────────────

View validation status: /validate-status
Report another bug: /test-fail "<description>"
```

---

## Step 8: Final Output

```
✅ Validation issue processed

Issue: #{{issue.id}}
Severity: {{severity}}
Status: {{#if implemented}}Fixed and tested{{else}}Logged (manual fix){{/if}}
Fix tasks: {{fix_tasks.length}} completed

{{#if issue.criterion}}
Re-test criterion #{{issue.criterion}} and use /test-pass when passing.
{{/if}}
```

---

## Error Handling

**If validation-analyzer fails:**
- Show error message
- Offer to retry with more specific input
- Suggest manual issue logging

**If State Manager fails:**
- Show error details
- Offer retry or manual state.json editing

**If fixes fail quality gates repeatedly:**
- Log issue with attempted fixes
- Mark issue status as "open" with notes
- Suggest manual intervention

---

## Notes

- **Intelligent analysis**: validation-analyzer uses Opus + extended thinking for deep root cause analysis
- **Any input format**: Stack traces, test output, manual descriptions all work
- **Quality gates**: Same code review + testing standards as /execute
- **Iterative fixing**: Re-run /test-fail if issue persists after fix
- **State tracking**: All issues logged in state.json and validation.md

Report bugs with confidence - the system will understand and fix them intelligently!

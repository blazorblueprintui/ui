---
# Note: Bash unrestricted - intentional for feature implementation flexibility
# This command needs to run tests, migrations, build tools, etc.
allowed-tools: Read, Write, Edit, Bash, Grep, Glob, Task(reviewer), Task(checkpoint-reviewer), Task(tester), Task(state-manager), Bash(node:*)
argument-hint: [feature-name]?
description: Execute feature tasks with automated code review and testing
model: sonnet
version: 1.0.0
---

> **Windows Users:** This command uses bash syntax. Ensure you have Git Bash installed and are running Claude Code from a Git Bash terminal, not PowerShell. [Installation guide](https://github.com/mathewtaylor/devflow#requirements)

# Execute Feature Implementation

Implement tasks for: **$1** (or active feature)

## Current State

- Active feature: !`node .devflow/lib/cli.js query active_feature`
- Feature exists: !`node .devflow/lib/cli.js query feature_exists "$1"`

## Your Task

**IMPORTANT CONSTRAINTS:**
- Follow the execution loop exactly as specified below
- Do NOT skip quality gates (code review, testing) unless explicitly allowed by user
- Use ONLY the allowed tools listed in frontmatter
- Pause/resume supported - save progress to state.json at each step
- Use three-state checkbox system in tasks.md

---

## Task Structure and Checkbox System

**Hierarchical Task Format:**

Tasks.md uses a hierarchical structure with parent tasks and subtasks:
```markdown
[ ] 1. Parent Task Title (effort: high)
- [ ] 1.1. First subtask
- [ ] 1.2. Second subtask
- [ ] 1.3. Third subtask

[ ] 2. Another Parent Task (effort: medium)
- [ ] 2.1. Subtask description
- [ ] 2.2. Another subtask
```

**Three-State Checkbox System:**
- `[ ]` = Not started
- `[-]` = In progress (parent task only)
- `[x]` = Complete

**Checkbox Rules:**
- **Parent tasks:** Mark `[-]` when starting first subtask, `[x]` when all subtasks complete
- **Subtasks:** Mark `[x]` immediately when subtask passes review/tests

**Execution Flow:**
1. Show confirmation ONCE per parent task
2. Execute subtasks sequentially (1.1 → 1.2 → 1.3 → 2.1)
3. Track current subtask in state.json (e.g., "1.2")
4. Mark checkboxes as work progresses
5. When resuming, continue from saved subtask

---

### Setup Phase

1. **Determine target feature:**
   - If $1 provided: Find matching feature key
   - If not: Use active_feature from state.json
   - If none: Show available features and error

2. **Validate feature:**
   - Must have tasks.md file
   - If missing: Error "Run /tasks first"

3. **Activate feature** (if not already active):
   - Invoke State Manager to set status="active"
   - Update active_feature in state.json

4. **Verify constitution summary exists:**
   - Check if `.devflow/constitution-summary.md` exists
   - If not found:
     ```
     ❌ Constitution summary not found

     The constitution-summary.md file is required for efficient code reviews.

     Options:
     a) Run /init to generate it automatically
     b) Create it manually from constitution.md

     Cannot proceed without constitution summary.
     ```
   - Stop execution

5. **Load context:**

@.devflow/constitution-summary.md
@.devflow/architecture.md
@.devflow/features/{{featureKey}}/spec.md
@.devflow/features/{{featureKey}}/plan.md
@.devflow/features/{{featureKey}}/tasks.md

<!-- Note: Uses constitution-summary (200-300 tokens) instead of full constitution (2,500 tokens)
     for efficiency during code reviews. Full constitution available at .devflow/constitution.md
     if detailed reference needed. -->

**Load relevant domain docs** based on feature's `concerns` array from state.json

---

### Execution Loop

Parse tasks.md to identify parent tasks (1, 2, 3) and their subtasks (1.1, 1.2, 1.3).

**For each parent task from current position to end:**

#### 1. Display Parent Task (Confirmation Point)

**Check if this is a Review Checkpoint parent task:**

If parent task title contains "Review Checkpoint" (e.g., "3. Review Checkpoint: Data Layer Complete"):
- **Skip normal subtask iteration** - this is a checkpoint, not implementation
- Go directly to **Section 3B: Execute Review Checkpoint** (below)

**For normal parent tasks:**

**Proactive Snapshot Suggestion (if applicable):**

If parent task has >5 subtasks AND snapshot doesn't already exist:
```
💡 Large parent task ahead ({{subtask_count}} subtasks).

Update snapshot before starting? (y/n)

This helps resume if the session is interrupted during this
lengthy parent task.
```

If yes: Follow Pause Handling snapshot creation steps (gather data, create snapshot.md, update state)
If no: Continue to parent task confirmation

**Parent Task Confirmation:**

Show this ONCE per parent task:

```
─────────────────────────────────────────
Parent Task {{parent_number}}: {{parent_title}} ({{effort}})
Subtasks: {{subtask_list}} ({{subtask_count}} total)
{{#if parent_in_progress}}Status: IN PROGRESS ({{completed_count}}/{{subtask_count}} complete){{/if}}
─────────────────────────────────────────

This will implement all subtasks under this parent task.
Continue? (y/n/skip/pause)
  y - Implement all subtasks in this parent
  n - Stop execution
  skip - Mark all subtasks complete without implementing (use cautiously)
  pause - Save progress and exit
```

**If continuing:**

- Mark parent task in tasks.md: `[ ]` → `[-]` (if not already `[-]`)
- Begin iterating through subtasks

#### 2. For Each Subtask Under Current Parent

**Display subtask:**
```
Starting subtask {{subtask_number}}: {{subtask_description}}
{{#if dependencies}}Dependencies: {{dependencies}}{{/if}}
```

**Check Dependencies:**

If subtask has `[depends: x.y]`:
- Verify those subtasks are marked `[x]` in tasks.md
- If not: Show which are incomplete, offer to:
  - Jump to incomplete dependency
  - Skip this subtask for now
  - Exit

#### 3. Implement Task

Based on task description:
- Write code following constitution standards
- Create new files or modify existing
- Follow patterns from architecture.md
- Apply relevant domain documentation rules
- Log what you're doing for implementation.md

---

### 3B. Execute Review Checkpoint (Review Checkpoint Parent Tasks Only)

**CRITICAL: This section only applies when parent task title contains "Review Checkpoint".**

**Check configuration:**

Read constitution.md for `quality_gates.checkpoint_review` setting:
- If `false`: Log "Checkpoint review disabled", mark parent task complete, skip to next parent
- If `true` or not specified (default): Continue with checkpoint review

**Collect checkpoint scope:**

1. **Find last checkpoint**: Search tasks.md backwards for previous "Review Checkpoint" parent task
   - If none found: Checkpoint scope starts from feature beginning
   - If found: Checkpoint scope starts from task after that checkpoint
2. **Identify reviewed parent tasks**: All parent tasks between last checkpoint and current checkpoint
   - Example: If last checkpoint was task 3, and current is task 7, review parent tasks 4, 5, 6
3. **Collect all files changed**:
   - Read implementation.md for "Files Modified" sections for each parent task in scope
   - Create comprehensive list of all created/modified files
4. **Extract requirements**:
   - From spec.md: Requirements for all reviewed parent tasks
   - Parse phase section name from checkpoint title (e.g., "Data Layer Complete" → Phase 1: Data Layer)
5. **Extract design**:
   - From plan.md: Technical design for all reviewed parent tasks

💡 **Tip:** For long checkpoint reviews, consider creating a snapshot first to preserve progress.

**Invoke Checkpoint Reviewer (Attempt 1):**

Use Task tool to invoke checkpoint-reviewer agent:

```
Task tool invocation:
- subagent_type: "checkpoint-reviewer"
- description: "Checkpoint review: {{phase_name}}"
- prompt: """
  Perform comprehensive checkpoint review with extended thinking.

  **Checkpoint:** {{checkpoint_parent_number}}. Review Checkpoint: {{phase_name}} Complete
  **Scope:** Parent tasks {{list, e.g., 1, 2}} ({{count}} tasks)
  **Since:** {{last_checkpoint_number or "feature start"}}

  **Files Changed in Checkpoint Scope:**
  {{comprehensive list of all files created/modified}}

  **Spec Requirements for This Phase:**
  {{extract requirements from spec.md for all reviewed parent tasks}}

  **Technical Plan for This Phase:**
  {{extract design from plan.md for all reviewed parent tasks}}

  **Context:**
  @constitution-summary.md
  @architecture.md

  **Checkpoint Review Focus:**
  1. Integration: Do all parent tasks in this phase work together?
  2. Spec alignment: Are ALL phase requirements met?
  3. Architecture compliance: Matches plan.md and architecture patterns?
  4. Security: Any vulnerabilities introduced across phase?
  5. Code quality: Bad practices, code smells, duplication across files?
  6. Cross-cutting concerns: Performance, maintainability, testability?

  **Severity Classification Required:**
  Categorize ALL issues by severity:
  - CRITICAL: Security vulnerabilities, data loss, breaking bugs
  - HIGH: Spec violations, architecture breaks, missing requirements
  - MEDIUM: Code quality, potential bugs, missing tests
  - LOW: Style, minor optimizations, suggestions

  Use extended thinking for deep cross-component analysis.

  Return structured feedback with status and severity-categorized issues.
  """
```

**Display checkpoint review header:**

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🔍 Checkpoint Review: {{phase_name}}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Scope: Parent tasks {{list}} ({{count}} tasks)
Files reviewed: {{file_count}} files
Since: {{last_checkpoint or "feature start"}}

Analyzing with extended thinking...
```

**Handle Checkpoint Review Result:**

**If status = CLEAN:**
```
✓ Checkpoint review passed

Phase: {{phase_name}}
Reviewed: {{parent_task_count}} parent tasks
Files: {{file_count}} files checked

✅ No issues found. Excellent work!
```

- Log success to implementation.md (see section 6)
- Mark Review Checkpoint parent task complete `[x]`
- Continue to next parent task

**If status = ISSUES_FOUND:**

Analyze severity distribution and handle based on what's present:

**CRITICAL or HIGH issues found (auto-fix these):**

```
⚠️ Checkpoint Review: Issues Require Fixes

Phase: {{phase_name}}

CRITICAL Issues ({{count}}):
{{for each: file:line | description | suggested fix}}

HIGH Issues ({{count}}):
{{for each: file:line | description | suggested fix}}

{{if medium}}MEDIUM Issues ({{count}}):
{{summary - will be addressed after critical/high}}{{/if}}

{{if low}}LOW Issues ({{count}}):
{{summary - will be reviewed after all fixes}}{{/if}}

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Auto-fixing CRITICAL and HIGH severity issues...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

**Fix critical and high issues sequentially:**

For each CRITICAL issue:
1. Display: `Fixing CRITICAL: {{issue_description}}`
2. Analyze issue and suggested fix from review
3. Implement fix (modify code, add validation, fix security issue, etc.)
4. Log fix to implementation.md

For each HIGH issue:
1. Display: `Fixing HIGH: {{issue_description}}`
2. Analyze issue and suggested fix from review
3. Implement fix (add missing feature, fix spec violation, align architecture, etc.)
4. Log fix to implementation.md

**Re-run checkpoint review (attempt 2-5):**

- Update attempt counter
- Invoke checkpoint-reviewer again with same scope (now includes fixes)
- If CLEAN: Proceed to check MEDIUM/LOW issues (if any remain)
- If CRITICAL/HIGH still present: Repeat auto-fix (max 5 total attempts)

**After 5 attempts with CRITICAL/HIGH still present:**

```
❌ Checkpoint review failed after 5 attempts

Phase: {{phase_name}} has persistent critical/high issues

Remaining CRITICAL issues ({{count}}):
{{list}}

Remaining HIGH issues ({{count}}):
{{list}}

This phase may have fundamental design problems that require manual intervention.

Options:
a) Continue anyway (NOT RECOMMENDED - known critical issues persist)
b) Pause execution for manual review (recommended)
c) Show me extended reasoning to understand the issues
d) Revert all changes in this phase (destructive)

Choose:
```

Handle user choice:
- **a) Continue**: Log all issues to implementation.md, mark checkpoint complete `[x]`, continue
- **b) Pause**: Save state, exit execution for manual intervention
- **c) Show reasoning**: Display extended reasoning from review, then ask again
- **d) Revert**: Use git to revert phase changes, mark checkpoint skipped

**Only MEDIUM or LOW issues remaining (all critical/high fixed):**

```
✓ Checkpoint Review: Critical Issues Resolved

Phase: {{phase_name}}
Attempt: {{attempt_count}} review cycle(s)

All CRITICAL and HIGH issues have been fixed.

Remaining issues:
{{if medium}}MEDIUM ({{count}}):
  {{for each: file:line | description | suggested fix}}
{{/if}}

{{if low}}LOW ({{count}}):
  {{for each: file:line | description | suggestion}}
{{/if}}

Options:
a) Auto-fix MEDIUM issues (recommended if medium issues present)
b) Show me issue details, I'll decide what to fix
c) Accept and continue (log issues but don't block)
d) Pause for manual review

Choose:
```

Handle user choice:

**a) Auto-fix MEDIUM:**
- Fix each MEDIUM issue sequentially
- Log fixes to implementation.md
- Re-run checkpoint review (attempt N+1)
- If LOW still remain after MEDIUM fixed: Ask about LOW issues
- If CLEAN: Mark checkpoint complete

**b) Show details:**
- Display full details for each MEDIUM and LOW issue
- For each issue, ask: `Fix this issue? (y/n/skip)`
  - y: Fix it now
  - n: Skip, mark as accepted
  - skip: Skip for now, will ask again
- After all decisions, re-run checkpoint review if any were fixed

**c) Accept and continue:**
- Log all MEDIUM/LOW issues to implementation.md as "Accepted Issues"
- Mark Review Checkpoint parent task complete `[x]`
- Continue to next parent task

**d) Pause:**
- Save current state
- Exit execution for manual review

**Update implementation.md after checkpoint:**

Append checkpoint review section:

```markdown
## Review Checkpoint: {{phase_name}} Complete

**Completed:** {{ISO timestamp}}
**Checkpoint:** Parent Task {{checkpoint_number}}
**Scope:** Parent tasks {{list, e.g., 1, 2}} ({{count}} tasks)
**Since:** {{last_checkpoint or "feature start"}}
**Files Reviewed:** {{file_count}} files
**Review Cycles:** {{attempt_count}}

### Initial Review Results

- **CRITICAL:** {{count}} issues
- **HIGH:** {{count}} issues
- **MEDIUM:** {{count}} issues
- **LOW:** {{count}} issues

### Fixes Applied

**CRITICAL Fixes ({{count}}):**
{{for each: issue description | files modified | resolution}}

**HIGH Fixes ({{count}}):**
{{for each: issue description | files modified | resolution}}

{{if medium_fixed}}**MEDIUM Fixes ({{count}}):**
{{for each: issue description | files modified | resolution}}
{{/if}}

### Final Status

{{if clean}}✅ All issues resolved - checkpoint CLEAN{{/if}}

{{if accepted_issues}}⚠️ Accepted Issues (not blocking):

**MEDIUM ({{count}}):**
{{list with rationale for accepting}}

**LOW ({{count}}):**
{{list with rationale for accepting}}
{{/if}}

{{if failed}}❌ Checkpoint FAILED after 5 attempts:
{{list persistent critical/high issues}}
User action: {{what user chose}}
{{/if}}

---
```

**Mark checkpoint complete:**

- Use Edit tool to mark Review Checkpoint parent task: `[ ]` → `[x]`
- Invoke State Manager to update current_task to next parent task number
- Log checkpoint completion

**Continue to next parent task:**

- If more parent tasks exist: Show next parent task confirmation
- If this was the last parent task: Go to Completion Flow

---

#### 4. Code Review (with Extended Thinking)

**Skip review if task is:**
- Documentation only
- Configuration changes
- Test file creation

**For code tasks:**

Invoke **Code Reviewer agent** (opus with extended thinking) with:
- Code changes made
- Constitution standards
- Task description and acceptance criteria
- Relevant spec.md sections

**Handle review result:**

If `CHANGES_REQUIRED`:
```
⚠️ Code review found issues:

Critical Issues:
- {{issue with location and fix suggestion}}

Warnings:
- {{warning with location}}

Fixing issues... (attempt {{attempt}}/3)
```
- Fix the issues
- Re-invoke reviewer
- Max 3 attempts, then ask user:
  ```
  Unable to pass review after 3 attempts.

  Remaining issues: {{list}}

  Options:
  a) Continue anyway (not recommended)
  b) Skip this task
  c) Pause execution
  d) Let me try to fix manually

  Choose:
  ```

If `APPROVED`:
```
✓ Code review passed
{{#if warnings or suggestions}}
  Warnings: {{count}}
  Suggestions: {{count}}
{{/if}}
```

#### 5. Testing

**Skip testing if task is:**
- Documentation
- Configuration
- Test file itself

**For code tasks:**

Invoke **Test Engineer agent** with:
- Implemented code
- Constitution testing requirements
- Task acceptance criteria

**Handle test result:**

If `FAIL`:
```
✗ Tests failed ({{failed_count}}/{{total_count}})

Failed tests:
- {{test_name}}: {{failure_message}}

Fixing... (attempt {{attempt}}/3)
```
- Analyze failures
- Fix code or tests
- Re-run tests
- Max 3 attempts, then ask user

If `PASS`:
```
✓ Tests passed ({{passed_count}}/{{total_count}})
  Coverage: {{coverage}}% (target: {{target}}%)
```

---

## Post-Implementation Steps (After Review + Tests Pass)

**CRITICAL: Follow this exact sequence for each subtask to maintain consistency.**

### Execution Order

After code review ✓ and tests ✓ pass for a subtask, complete these steps in order:

**STEP A: Update tasks.md checkboxes** (Section 6)
- Use Edit tool to mark subtask `[x]`
- Use Edit tool to update parent if needed (`[-]` or `[x]`)
- **If Edit fails**: Retry once with more context, then pause execution

**STEP B: Invoke State Manager** (Section 7)
- Use Task tool with state-manager agent
- Request current_task update to completed subtask number (e.g., "1.2")
- **If State Manager fails**: Show error, ask user how to proceed

**STEP C: Log to implementation.md** (Section 6)
- Append completed subtask details
- Include parent task context
- Record review and test results

**STEP D: Check remaining work** (Section 8)
- If more subtasks in current parent: Continue to next subtask
- If parent complete: Show completion message, move to next parent
- If all tasks complete: Go to Completion Flow

### Error Handling and Consistency

**State Consistency Policy:**
- Steps A and B must both succeed for consistent state
- If Step A succeeds but Step B fails: State is inconsistent (tasks.md ✓, state.json ✗)
- On resume: Derive position from BOTH tasks.md checkboxes AND state.json current_task
- Resolution: If mismatch, tasks.md is source of truth - update state.json to match

**Rollback is NOT supported:**
- Once checkboxes are marked, they stay marked
- If state update fails, fix state.json on next attempt
- This prevents losing progress if execution is interrupted

---

#### 6. Mark Subtask Complete

**CRITICAL: Use Edit tool to update tasks.md checkboxes**

**STEP 1: Mark current subtask complete**

Use Edit tool to update tasks.md:
- Find exact line: `- [ ] {{subtask_number}}. {{description}}`
- Replace with: `- [x] {{subtask_number}}. {{description}}`

Example:
- Old: `- [ ] 1.2. Create user service implementation`
- New: `- [x] 1.2. Create user service implementation`

**If Edit fails:** Retry once with more context (include parent line), then pause execution and alert user.

**STEP 2: Check and update parent task checkbox**

After marking subtask complete, read tasks.md to check parent status:

1. Count remaining `[ ]` subtasks under this parent
2. If **all subtasks are now `[x]`**:
   - Use Edit tool to find: `[-] {{parent_number}}. {{parent_title}}`
   - Replace with: `[x] {{parent_number}}. {{parent_title}}`
3. If **subtasks still remain**:
   - Parent stays as `[-]` (in progress) - no edit needed

**Example sequence:**
```markdown
Before (completing subtask 1.2):
[-] 1. Parent Task (effort: high)
- [x] 1.1. First subtask
- [ ] 1.2. Second subtask ← completing this one
- [ ] 1.3. Third subtask

After Edit (subtask 1.2):
[-] 1. Parent Task (effort: high)  ← stays [-] (still has 1.3)
- [x] 1.1. First subtask
- [x] 1.2. Second subtask
- [ ] 1.3. Third subtask

After completing 1.3:
[x] 1. Parent Task (effort: high)  ← Edit changes [-] to [x] (all done)
- [x] 1.1. First subtask
- [x] 1.2. Second subtask
- [x] 1.3. Third subtask
```

**Create/append to implementation.md:**

```markdown
## Subtask {{subtask_number}}: {{subtask_description}}
**Parent Task:** {{parent_number}}. {{parent_title}}
**Completed:** {{ISO timestamp}}
{{#if dependencies}}**Dependencies:** {{dependencies}}{{/if}}

### Implementation
{{summary of what was done}}

### Code Review
{{#if skipped}}
Skipped (non-code task)
{{else}}
✓ Approved
{{#if warnings}}Warnings: {{count}}{{/if}}
{{#if suggestions}}Suggestions: {{count}}{{/if}}
{{/if}}

### Testing
{{#if skipped}}
Skipped (non-code task)
{{else}}
✓ {{passed}}/{{total}} tests passed
✓ Coverage: {{coverage}}%
{{/if}}

### Files Modified
{{list of files created/modified}}

---
```

#### 7. Update State

**Use Task tool to invoke State Manager agent:**

```
Task tool invocation:
- subagent_type: "state-manager"
- description: "Update current task progress"
- prompt: "Update current_task to '{{subtask_number}}' for active feature. Use state-io.js utilities (readState, writeState, validateSchema). Return success status."
```

Example: For subtask 1.2:
```
Task(state-manager): "Update current_task to '1.2' for active feature. Validate schema before writing."
```

**Handle State Manager response:**

If response contains `success: false`:
- Show error details to user
- Options:
  - `retry` - Invoke State Manager again
  - `continue` - Continue without state update (not recommended)
  - `pause` - Pause execution for manual intervention
  - `manual` - Show user how to fix manually with Bash

If response contains `success: true`:
- Log confirmation: `✓ State updated: current_task = "{{subtask_number}}"`
- Continue to next step

**State after successful update:**
```json
{
  "current_task": "{{subtask_number}}",  // e.g., "1.2", "2.1"
  "phase": "EXECUTE",
  "status": "active"
}
```

Parent task number is derived by parsing current_task string ("1.2" → parent 1, subtask 2).

**If last subtask of parent task:**
- Display: `✓ Parent Task {{parent_number}} complete ({{subtask_count}}/{{subtask_count}} subtasks)`
- **Check context usage** (see Context Management below)
- Move to next parent task
- Show next parent task confirmation

**If last subtask of entire feature:**
- Transition to phase=DONE, status=completed
- Go to **Completion Flow**

---

### After Parent Task Completion

After completing each parent task:

1. Check if more parent tasks remain
2. If yes, show confirmation and proceed to next parent task
3. If no, proceed to Completion Flow

**💡 Context Management Tip:**

If you notice sluggish performance or Claude Code shows high token usage warnings:
- Consider creating a snapshot (offered on pause or before large parent tasks)
- Use `/compact` to create summary and continue fresh
- Both approaches preserve progress in state.json and tasks.md

---

#### 8. Continue Through Subtasks

Continue automatically to next subtask in same parent (no confirmation between subtasks).

When parent task complete or all feature tasks complete: Show appropriate completion message

---

### Pause Handling

When user chooses "pause":

**Step 1: Verify state saved**
- Current subtask is already saved in state.json (e.g., "1.2")
- Tasks.md shows current state with checkboxes:
  - Parent tasks: `[x]` complete, `[-]` in progress, `[ ]` not started
  - Subtasks: `[x]` complete, `[ ]` not done

**Step 2: Offer snapshot creation**
```
Create/update snapshot for easy resume? (y/n)

This creates a summary in snapshot.md to help resume after
Claude Code's /compact or session interruptions.
```

**Step 3: If yes, create snapshot:**

1. **Read template:** `.devflow/templates/snapshot.md.template`

2. **Gather snapshot data:**
   - **Feature metadata:**
     - FEATURE_NAME: display_name from state.json
     - FEATURE_KEY: featureKey
     - PHASE: phase from state.json
     - STATUS: status from state.json (should be "paused")
     - SNAPSHOT_TIMESTAMP: Current timestamp (ISO 8601)

   - **Progress metrics:**
     - Count tasks.md checkboxes: `[x]` vs total subtasks
     - COMPLETED_TASKS: count of `[x]` subtasks
     - TOTAL_TASKS: count of all subtasks (under all `- [ ]` or `- [-]` or `- [x]` parent tasks)
     - PROGRESS_PERCENT: (COMPLETED_TASKS / TOTAL_TASKS * 100).toFixed(0)

   - **Current position:**
     - CURRENT_TASK: current_task from state.json (e.g., "1.2")
     - CURRENT_PARENT: extract parent number (e.g., "1.2" → "1")
     - PARENT_TITLE: extract from tasks.md (parent task 1's title)
     - CURRENT_SUBTASK_DESCRIPTION: extract from tasks.md

   - **Phase progress:**
     - List all phase sections in tasks.md with completion status
     - Format: "Phase 1: Complete (5/5), Phase 2: In Progress (2/8), Phase 3: Not Started (0/12)"

   - **Recent work:**
     - Read implementation.md, extract last 3-5 completed subtask entries
     - Include: subtask number, title, completion timestamp
     - Format as bulleted list

   - **Files modified:**
     - Scan implementation.md for all "Files Modified:" sections
     - Deduplicate file paths
     - Format as bulleted list with file paths

   - **Review status:**
     - Find last checkpoint review in implementation.md
     - Extract: checkpoint name, status (APPROVED/ISSUES), issue count if any

   - **Issues:**
     - Scan recent checkpoint reviews for warnings/issues
     - Summarize: "No issues" or "3 MEDIUM issues accepted, see implementation.md"

   - **Next steps:**
     - NEXT_SUBTASK_DETAILS: Description of current_task subtask
     - REMAINING_SUBTASKS: List remaining `[ ]` subtasks in current parent
     - UPCOMING_PARENT_TASKS: List next 1-2 `[ ]` parent tasks

   - **Context notes:**
     - "Paused during Parent Task {{CURRENT_PARENT}} execution"
     - Add any warnings about approaching token limits if relevant

3. **Fill template and write file:**
   - Use Write tool to create `.devflow/features/{{featureKey}}/snapshot.md`
   - Replace all `{{PLACEHOLDER}}` values with gathered data
   - **Note:** This overwrites existing snapshot.md if present

4. **Update state.json:**
   - Invoke State Manager agent with:
     ```
     Update feature {{featureKey}}:
     - Set snapshot: "snapshot.md"
     ```

**Step 4: Confirm pause:**
```
⏸️ Execution paused

Feature: {{display_name}}
Progress: {{completed_tasks}}/{{total_tasks}} subtasks complete
Current: Subtask {{current_task}} (in Parent Task {{current_parent}})
{{#if snapshot_created}}
✓ Snapshot saved: .devflow/features/{{featureKey}}/snapshot.md
{{/if}}

Resume: /execute (without arguments)
```

**If no (skip snapshot):**
- Invoke State Manager to set `snapshot: null` if it was previously set
- Display pause confirmation without snapshot message

---

### Completion Flow

When all tasks are marked complete:

```
🎉 All implementation tasks complete!

Summary:
- Feature: {{display_name}}
- Tasks completed: {{total_tasks}}
- Files modified: {{file_count}}
- Tests added: {{test_count}}
- Time: {{duration if trackable}}
```

**1. Clean Up Snapshot**

If snapshot exists:
1. **Check state.json** for snapshot value
2. **If not null:**
   - Construct path: `.devflow/features/{{featureKey}}/snapshot.md`
   - If file exists, delete it using Bash: `rm .devflow/features/{{featureKey}}/snapshot.md`
   - Confirm: `✓ Snapshot cleared (implementation complete)`

**2. Transition to Validation Phase**

Invoke State Manager to:
- Set phase=VALIDATE (not DONE - validation phase comes next)
- Keep status=active
- Keep feature as active_feature (validation continues on same feature)

**3. Implementation Complete!**

```
✅ Implementation complete: {{display_name}}!

Documentation:
- Spec: features/{{key}}/spec.md
- Plan: features/{{key}}/plan.md
- Tasks: features/{{key}}/tasks.md ({{total}} ✓)
- Implementation: features/{{key}}/implementation.md

─────────────────────────────────────────────────────────────
🔍 Next: Validation Phase
─────────────────────────────────────────────────────────────

Implementation is done, but the feature needs validation testing
before it can be marked complete.

**Start validation:**
  /validate

This will:
  - Parse acceptance criteria from spec.md
  - Create validation.md tracking document
  - Set up intelligent bug reporting

**During validation:**
  - Test each acceptance criterion
  - Report bugs: /test-fail "<error logs or description>"
  - Mark passing: /test-pass <criterion-number>
  - Check progress: /validate-status

**When all tests pass:**
  - Finalize: /validate-complete
  - Generates retrospective and marks feature DONE

─────────────────────────────────────────────────────────────

Start validation now: /validate
```

---

## Error Recovery

### Review Rejection After 3 Attempts
- Show user the persistent issues
- Offer to continue anyway, skip, pause, or get manual help
- Log the decision

### Test Failure After 3 Attempts
- Show failing tests
- Offer to continue anyway (tests marked incomplete), skip, pause, or get manual help
- Log the decision

### Task Implementation Failure
If an error occurs during implementation:
```
❌ Error implementing task: {{error message}}

Options:
a) Retry
b) Skip this task
c) Pause execution
d) Manual intervention needed

Choose:
```

---

## Resume Behavior

When `/execute` runs without arguments and active_feature exists:

**Step 1: Check for snapshot**

Read state.json to get snapshot value:
- If `snapshot` is not null (e.g., "snapshot.md")
- Construct full path: `.devflow/features/{{featureKey}}/snapshot.md`
- Check if file exists and is readable

**Step 2: Load and display snapshot (if exists)**

If snapshot file exists:
1. **Read snapshot.md**
2. **Extract key information:**
   - Progress percentage
   - Last completed subtask
   - Files modified (count)
   - Issues summary
3. **Display snapshot summary:**
   ```
   📸 Loading snapshot...

   Last session summary:
   ─────────────────────────────────────────
   Progress: {{completed}}/{{total}} subtasks ({{percent}}%)
   Last completed: Subtask {{last_number}} - {{last_title}}
   Files modified: {{file_count}} files
   {{#if issues}}Issues noted: {{issue_summary}}{{/if}}
   ─────────────────────────────────────────
   ```

**Step 3: Parse current position**

Parse current_task from state.json (e.g., "1.2"):
- Extract parent task number by parsing string (e.g., "1.2" → parent is 1)
- Extract subtask number (e.g., "1.2" is subtask 2 of parent 1)
- Determine which subtasks in parent are complete by reading tasks.md checkboxes

**Step 4: Display resume information**
```
Resuming feature: {{display_name}}
Progress: {{completed_subtask_count}}/{{total_subtask_count}} subtasks completed

Parent Task {{parent_number}}: {{parent_title}} ({{status}})
├─ Completed: {{list of completed subtasks with [x]}}
└─ Next: Subtask {{current_task}} - {{subtask_description}}

Remaining in this parent: {{remaining_subtasks}}

Continue from where you left off? (y/n)
```

**Step 5: If continuing**
- If resuming mid-parent (parent marked `[-]`): Continue with subtasks, no new confirmation
- If resuming at new parent (parent marked `[ ]`): Show parent task confirmation prompt
- Snapshot remains in place (not deleted on resume)
- Will be updated on next pause or deleted on feature completion

---

## Notes

- **Token management**: If context gets large (>50K tokens), offer to create snapshot and continue with fresh context
- **Git integration**: If git available, use `git diff` to show exact changes for code review
- **Incremental saves**: implementation.md updates after each task, never lose progress
- **Flexible workflow**: Can skip non-critical tasks, pause anytime, jump to dependencies

Execute with confidence - every task is reviewed and tested before proceeding.

---
allowed-tools: Read, Task(ideas)
argument-hint: [idea text] | complete <number> | clear
description: Capture ideas quickly without leaving the terminal
model: haiku
version: 1.0.0
---

# Capture Ideas

Quick idea capture system to stay in flow while coding. Ideas are stored in `.devflow/ideas.md` as a numbered checklist.

## Arguments

**Input:** $ARGUMENTS

---

## Your Task

Invoke the **ideas agent** to handle this request based on the arguments:

### Usage Modes

**1. Add New Idea** (text provided)
```
/devflow:idea "Add user authentication flow"
```
→ Appends idea to `.devflow/ideas.md` with next number

**2. List Ideas** (no arguments)
```
/devflow:idea
```
→ Shows all current ideas with their status

**3. Mark Complete** (complete command)
```
/devflow:idea complete 3
```
→ Marks idea #3 as done (changes `[ ]` to `[x]`)

**4. Clear Completed** (clear command)
```
/devflow:idea clear
```
→ Removes all completed ideas, renumbers remaining

---

## Implementation

The ideas agent will:
- Read/create `.devflow/ideas.md`
- Manage numbered checkbox list format
- Handle all file operations
- Return success/error status

Keep the response concise - users want quick capture without interruption.

---

## Output Format

**After add:**
```
✓ Idea #N captured in .devflow/ideas.md
```

**After list:**
```
Current Ideas (X total, Y pending):
[ ] 1. First idea
[x] 2. Completed idea
[ ] 3. Another idea
```

**After complete:**
```
✓ Marked idea #N as complete
```

**After clear:**
```
✓ Removed X completed ideas, Y remaining
```

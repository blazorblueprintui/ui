---
# Note: Bash unrestricted - safe because all bash commands are read-only detection
# (checking for files, git repos, package managers). File writes use Write tool.
allowed-tools: Read, Write, Glob, Grep, AskUserQuestion, Task(architect), Bash
argument-hint:
description: Initialize DevFlow - create constitution and architecture documentation
model: sonnet
version: 1.0.0
---

> **Windows Users:** This command uses bash syntax. Ensure you have Git Bash installed and are running Claude Code from a Git Bash terminal, not PowerShell. [Installation guide](https://github.com/mathewtaylor/devflow#requirements)

# Initialize DevFlow

Create personalized constitution and architecture documentation for this project.

## Current State

- DevFlow initialized: !`test -f .devflow/constitution.md && echo "yes" || echo "no"`
- Existing code detected: !`node .devflow/lib/cli.js query code_detected`
- Package files found: !`node .devflow/lib/cli.js query package_files`

## Your Task

If already initialized, ask user whether to reinitialize (create backups of existing files).

Run an interactive constitution wizard to gather:

**Project Mission & Domain Context:**
1. What problem does this project solve? What is its mission or purpose?
2. Who are the primary users/customers and what are their key needs?
3. What are the core domain concepts or business entities?
   - Examples: In e-commerce: products, orders, customers, inventory
   - In healthcare: patients, providers, appointments, medical records
   - In finance: accounts, transactions, portfolios, risk assessments
4. What does success look like for this project? (business goals, metrics, outcomes)
5. What are the core business rules or constraints?
6. [Optional] Any regulatory/compliance requirements?
   - Examples: HIPAA, GDPR, SOC2, PCI-DSS, FERPA, etc.
   - Skip if not applicable

**Tech Stack:**
7. Primary language (JavaScript, TypeScript, C#, Python, Java, Go, etc.)
8. Framework (React, Next.js, ASP.NET Core, Django, Spring Boot, etc.)
9. Database (PostgreSQL, MySQL, SQL Server, MongoDB, etc.)
10. Additional technologies (Redis, Docker, etc.)

**Architecture:**
11. Based on tech stack, suggest patterns:
   - **.NET:** Clean Architecture + CQRS, Vertical Slice, Traditional Layered
   - **Node.js:** MVC, Clean Architecture, Feature-based
   - **React:** Atomic Design, Feature-based, Pages + Components
   - *Ask follow-up questions based on selection (e.g., MediatR? FluentValidation?)*

**Standards:**
12. Linter (ESLint, Pylint, etc.)
13. Formatter (Prettier, Black, etc.)
14. Naming conventions

**Testing:**
15. Unit test framework
16. Coverage requirement
17. Integration testing approach

**Security:**
18. Authentication (JWT, OAuth, session cookies, etc.)
19. Authorization (RBAC, ABAC, custom)

**Performance:**
20. Performance requirements or SLAs

**Cross-Cutting Concerns:**
21. Which patterns apply? (multi-select)
    - Multi-tenancy
    - RBAC/ABAC
    - Caching
    - Message queues
    - Audit logging
    - API versioning
    - Rate limiting

Generate `@../../../.devflow/constitution.md` from template, filling all {{PLACEHOLDERS}} with gathered info.

---

## Generate Constitution Summary

**CRITICAL:** After creating constitution.md, immediately generate the constitution summary for efficient code reviews.

**Step 1: Read constitution-summary template**

Read: `@../../../.devflow/templates/constitution-summary.md.template`

**Step 2: Generate brief versions of complex fields**

For the following fields, create 1-2 sentence summaries from the full constitution data:

- **NAMING_CONVENTIONS_BRIEF**: Extract core naming rules (1-2 lines)
  - Example: "PascalCase for classes, camelCase for methods, UPPER_CASE for constants"

- **FILE_ORGANIZATION_BRIEF**: Summarize folder structure (1-2 lines)
  - Example: "Feature-based organization: /features/[name]/{components,services,tests}"

- **AUTHENTICATION_STANDARD_BRIEF**: Core auth method (1 line)
  - Example: "JWT tokens with HttpOnly cookies, 15min access + 7day refresh"

- **DATA_PROTECTION_STANDARD_BRIEF**: Key protection rules (1 line)
  - Example: "AES-256 for PII, bcrypt for passwords, TLS 1.3 in transit"

- **SECURITY_BEST_PRACTICES_BRIEF**: Top 3 security rules (1-2 lines)
  - Example: "Input validation on all endpoints, parameterized queries, principle of least privilege"

- **KEY_ARCHITECTURAL_PRINCIPLES_BRIEF**: Top architectural rules (2-3 lines)
  - Example: "Clean Architecture with CQRS. Dependencies flow inward. Domain logic isolated from infrastructure."

**Step 3: Fill template with gathered data**

Replace placeholders in constitution-summary template:

**Direct mappings (same as constitution.md):**
- {{PROGRAMMING_LANGUAGE}}
- {{FRAMEWORK}}
- {{DATABASE}}
- {{LINTER}}
- {{FORMATTER}}
- {{UNIT_TEST_FRAMEWORK}}
- {{COVERAGE_REQUIREMENT}}
- {{INTEGRATION_TEST_FRAMEWORK}}
- {{WHEN_TO_WRITE_UNIT_TESTS}}

**Brief versions (created in Step 2):**
- {{NAMING_CONVENTIONS_BRIEF}}
- {{FILE_ORGANIZATION_BRIEF}}
- {{AUTHENTICATION_STANDARD_BRIEF}}
- {{DATA_PROTECTION_STANDARD_BRIEF}}
- {{SECURITY_BEST_PRACTICES_BRIEF}}
- {{KEY_ARCHITECTURAL_PRINCIPLES_BRIEF}}

**Step 4: Write constitution-summary.md**

Write to: `.devflow/constitution-summary.md`

Result should be 200-300 tokens vs 2,500 tokens for full constitution.

**Why this matters:**
- execute.md loads constitution-summary for every code review (10-20+ times per feature)
- Saves ~2,200 tokens per review
- 10-task feature: ~28,600 tokens saved
- 20-task feature: ~55,000 tokens saved

---

## Architecture Setup

### If Existing Project (code detected):

1. Scan codebase using Glob/Grep:
   - Project structure, tech stack, frameworks
   - Database (ORM files, migrations)
   - API patterns, services, components

2. Present findings to user:
   ```
   Found:
   ✓ ASP.NET Core 8.0, EF Core, SQL Server
   ✓ /Controllers - 12 controllers
   ✓ /Services - 8 services
   ✓ Repository pattern detected

   Is this accurate? (y/n)
   ```

3. Get feedback, generate `@../../../.devflow/architecture.md`

4. Ask user to review: "Check .devflow/architecture.md and refine. Press Enter when ready."

### If New Project (no code):

1. Suggest architecture based on selected patterns
2. Show proposed folder structure and key libraries
3. Ask user: Use this/Different approach/Describe own
4. Generate architecture blueprint in `@../../../.devflow/architecture.md`

---

## Domain Documentation

For each selected cross-cutting concern, create template in `.devflow/domains/`:
- `security/authentication.md`
- `security/authorization.md`
- `infrastructure/multi-tenancy.md`
- `infrastructure/caching.md`
- etc.

Update `@../../../.devflow/domains/_index.md` with one-line summaries.

---

## Initialize State

Preserve existing state.json if valid, or create fresh if missing/corrupted.

```bash
node .devflow/lib/cli.js update init-or-migrate-state
```

**Behavior:**
- If state.json exists and is valid: **Preserve it** (keeps all features and active_feature)
- If state.json is corrupted or missing: Create fresh empty state
- Validates schema to ensure state integrity

**Result:**
- Existing features preserved during reinit (no data loss)
- Fresh state created on first init
- Constitution/architecture can be updated without affecting feature progress

---

## Documentation Discovery Notification

Check for existing documentation files:

- Markdown count: !`node .devflow/lib/cli.js query markdown_count "README.md,CONTRIBUTING.md,CHANGELOG.md,LICENSE.md"`

**If markdown count > 5:**

Display notification:
```
📚 Documentation Detected

Found {{count}} markdown files in your project with technical documentation.

You can consolidate them into DevFlow's domain structure:
  /consolidate-docs

This will:
  • Analyze existing documentation
  • Extract technical information (auth, database, API, etc.)
  • Organize into structured .devflow/domains/
  • Provide archival recommendations for old docs

Run this now or later. DevFlow works great either way!
```

**If markdown count ≤ 5:**
- Skip notification (too few files to warrant consolidation)

---

## CLAUDE.md Integration

Automatically integrate DevFlow reference with project's CLAUDE.md file.

### Check for existing CLAUDE.md

- CLAUDE.md exists: !`test -f CLAUDE.md && echo "yes" || echo "no"`

### Integration Logic

**If CLAUDE.md EXISTS:**

1. Read existing CLAUDE.md content
2. Check if DevFlow section already present (search for either):
   - `## DevFlow` (heading)
   - `@.devflow/instructions.md` (reference to instructions)

3. **If DevFlow section NOT present:**
   - Append delimiter: `\n---\n\n`
   - Append minimal reference section:
     ```markdown
     ## DevFlow

     This project uses **DevFlow** for structured feature development.

     For complete DevFlow instructions, see: @.devflow/instructions.md

     Last updated: YYYY-MM-DD
     ```
   - Replace `YYYY-MM-DD` with current date: `$(date +%Y-%m-%d)`
   - Write back to CLAUDE.md
   - Track result: "✓ DevFlow reference added to CLAUDE.md"

4. **If DevFlow section ALREADY present:**
   - Find section boundaries:
     - Start: Line containing `## DevFlow`
     - End: Next `---` separator OR next `##` heading OR end of file
   - Replace entire DevFlow section with updated minimal reference (same as step 3)
   - Replace `YYYY-MM-DD` with current date
   - Write back to CLAUDE.md
   - Track result: "✓ DevFlow reference updated in CLAUDE.md"

**If CLAUDE.md DOES NOT EXIST:**

1. Create new CLAUDE.md with minimal reference section:
   ```markdown
   # CLAUDE.md

   This file provides guidance to Claude Code when working with this repository.

   ---

   ## DevFlow

   This project uses **DevFlow** for structured feature development.

   For complete DevFlow instructions, see: @.devflow/instructions.md

   Last updated: YYYY-MM-DD
   ```
2. Replace `YYYY-MM-DD` with current date: `$(date +%Y-%m-%d)`
3. Write to `CLAUDE.md` in project root
4. Track result: "✓ Created CLAUDE.md with DevFlow reference"

### Implementation Notes

- Use current date: `$(date +%Y-%m-%d)`
- Detection searches for `## DevFlow` heading or `@.devflow/instructions.md` reference
- When appending to existing file, use clean delimiter: `\n---\n\n`
- DevFlow section is minimal (5 lines) - full instructions live in `.devflow/instructions.md`
- User's CLAUDE.md stays clean; updates never touch user content

---

## Output

```
✅ DevFlow initialized successfully!

Files created:
- .devflow/constitution.md
- .devflow/constitution-summary.md (for efficient code reviews)
- .devflow/architecture.md
- .devflow/state.json
- .devflow/domains/_index.md
{{domain docs created}}

CLAUDE.md: {{status}}
  (Status: "✓ DevFlow section added" | "✓ DevFlow section updated" | "✓ Created with DevFlow instructions")

{{#if docs_detected}}
📚 Optional: Run /consolidate-docs to organize existing documentation
{{/if}}

Next: Run /spec [feature-name] to create your first feature
Example: /spec user-authentication
```

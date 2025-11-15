# DevFlow Instructions for Claude Code

This project uses **DevFlow** - an agentic workflow system for structured feature development.

---

## Context Management

Claude Code displays token usage in system warnings throughout your session. Watch for warnings like:

```
Token usage: XX,XXX/200,000; remaining YY,YYY
```

**Best Practices:**
- Monitor Claude Code's system warnings regularly
- Create snapshots before long operations (`/execute` with many tasks)
- Use `/compact` when context feels heavy or sluggish
- Break large features into smaller, manageable pieces

**DevFlow helps through:**
- Smart context loading (only loads relevant documentation)
- Snapshot creation on pause for easy resume
- Minimal documentation overhead
- Tier-based document loading (core, feature-specific, on-demand)

---

## DevFlow Overview

DevFlow provides:
- **Structured workflow:** Spec → Plan → Tasks → Execute with intelligent agents
- **Living documentation:** Constitution, architecture, and cross-cutting concerns auto-maintained
- **Smart context loading:** Only loads relevant documentation to optimize token usage
- **Feature tracking:** Comprehensive state management for multi-feature projects

---

## Available Commands

### Core Workflow
- `/init` - Initialize DevFlow (create constitution + architecture documentation)
- `/spec [feature-name]` - Create feature specification through interactive wizard
- `/plan` - Generate technical implementation plan (invokes Architect agent)
- `/tasks` - Break plan into atomic, executable tasks (invokes Task Planner agent)
- `/execute [feature-name]?` - Execute tasks with automated reviews and testing

### Utility Commands
- `/status` - Show current progress, active features, and context usage
- `/think [question]` - Deep analysis with extended thinking for complex decisions

---

## Documentation Structure

```
.devflow/
├── constitution.md        # Project mission, domain, principles & standards (full version ~3,000 tokens)
├── constitution-summary.md # Condensed version for code reviews (~200-300 tokens, excludes mission/domain)
├── architecture.md        # Current system state (ALWAYS READ FIRST)
├── state.json            # Current progress and feature tracking
│
├── domains/              # Cross-cutting concerns (load on-demand)
│   ├── _index.md         # Quick reference (always load)
│   ├── security/
│   │   ├── authentication.md
│   │   ├── authorization.md
│   │   └── encryption.md
│   ├── infrastructure/
│   │   ├── multi-tenancy.md
│   │   ├── caching.md
│   │   ├── logging.md
│   │   └── error-handling.md
│   ├── data/
│   │   ├── database-conventions.md
│   │   ├── migrations.md
│   │   └── audit-trails.md
│   └── integration/
│       ├── third-party-apis.md
│       └── message-queues.md
│
├── features/             # Per-feature documentation
│   └── yyyymmdd-feature-name/
│       ├── spec.md
│       ├── plan.md
│       ├── tasks.md
│       ├── implementation.md
│       └── retrospective.md
│
├── decisions/            # Architecture Decision Records (ADRs)
└── snapshots/            # Context snapshots for resume functionality
```

---

## Context Loading Strategy

### Always Load (Tier 1 - Core Context)
1. **constitution-summary.md** (~200-300 tokens) - Condensed technical standards for code reviews
   - **Note:** Full constitution.md (~3,000 tokens) used only for initial planning and specific queries
   - Full version includes: project mission, domain context, business rules, and technical standards
   - Summary excludes mission/domain, focuses only on coding standards
   - This optimization saves ~2,700 tokens per code review (10-20+ reviews per feature)
2. **architecture.md** (~1,500 tokens) - Current system structure
3. **domains/_index.md** (~500 tokens) - Quick reference for cross-cutting concerns

**Total Tier 1:** ~2,200-2,300 tokens (down from ~4,500)

### Feature-Specific (Tier 2 - Dynamic Loading)
When working on a feature, load:
1. Current feature's **spec.md**, **plan.md**, **tasks.md** (~2000 tokens)
2. Relevant source code files (~5000 tokens)

**Total Tier 2:** ~7000 tokens

### On-Demand Concerns (Tier 3 - Smart Loading)
Load full concern documentation from `domains/` when:
- **Keywords detected:** Feature mentions "auth", "permissions", "tenant", "cache", etc.
- **Explicit tags:** User tagged concerns during `/spec`
- **Agent intelligence:** Architect/Planner agents identify relevant concerns

**Per Concern:** ~1000 tokens each

**Total context budget:** ~15,000-20,000 tokens (well within limits for most commands)

---

## Working with DevFlow

### Starting a New Feature

1. Run `/spec feature-name` to create specification
   - Answer interactive questions
   - Tag relevant cross-cutting concerns
   - Result: `.devflow/features/yyyymmdd-feature-name/spec.md`

2. Run `/plan` to generate technical design
   - Architect agent analyzes spec + constitution + architecture
   - Proposes approach, patterns, libraries
   - Result: `plan.md` in feature folder

3. Run `/tasks` to break down into atomic tasks
   - Task Planner agent creates executable checklist
   - Each task <2hrs, with dependencies and complexity
   - Result: `tasks.md` with checkboxes

4. Run `/execute` to implement
   - Execute tasks sequentially
   - Auto-review code (Code Reviewer agent)
   - Auto-generate tests (Test Engineer agent)
   - Update documentation (Documentation agent)
   - Result: Completed feature + updated architecture

### When User Asks General Questions

1. **Check constitution.md** for:
   - Project mission and purpose (why the project exists)
   - Domain concepts and business entities
   - Business rules and constraints
   - Success criteria and goals
   - Regulatory/compliance requirements
   - Technical principles and coding standards
2. **Check architecture.md** for system structure and components
3. **Check domains/_index.md** for relevant cross-cutting concerns
4. **Load full concern docs** only when detailed info needed

### Reading State

Always check `.devflow/state.json` to understand:
- Current progress and active features
- Feature phases and task counts
- Paused features that can be resumed

Example:
```json
{
  "initialized": true,
  "active_feature": "20251020-user-auth",
  "features": {
    "20251020-user-auth": {
      "display_name": "User Authentication",
      "status": "active",
      "phase": "EXECUTE",
      "current_task": 6,
      "concerns": ["authentication", "authorization"]
    }
  }
}
```

---

## Feature Naming Convention

Features use format: `yyyymmdd-feature-name`

Examples:
- `20251020-user-authentication`
- `20251022-payment-integration`
- `20251025-email-notifications`

**Benefits:**
- Chronological ordering
- Unique identifiers (date prevents conflicts)
- Concise names (2-4 words)

---

## Important Behaviors

### Context-Aware Execution
- Monitor Claude Code's system warnings for token usage
- Suggest snapshot creation for long-running features
- Use `/compact` if context feels sluggish
- Break large features into smaller phases when appropriate

### Guided Flexibility
- Workflow is **Spec → Plan → Tasks → Execute**
- Users CAN skip steps (e.g., go straight to `/execute`)
- Warn them about skipped steps, but **allow** it
- Example: "⚠️ Warning: No plan found. Consider running `/plan` first. Continue anyway? (y/n)"

### Single Active Feature
- Only ONE feature can be `status: "active"` at a time
- If user tries to activate a new feature while one is active, prompt to pause/complete the current one

### Context Optimization
- Use `.devflowignore` to exclude irrelevant files (node_modules, build outputs, etc.)
- Load domain docs only when triggered by keywords or tags
- Monitor token usage and suggest creating snapshots if approaching limits

---

## If DevFlow Not Initialized

If `.devflow/constitution.md` doesn't exist, suggest:

```
❌ DevFlow has not been initialized in this project.

Run /init to set up DevFlow's documentation and workflow system.
```

---

## Sub-Agents

DevFlow uses specialized agents in `.claude/agents/`:

- **state-manager** - Validates transitions, manages feature state
- **architect** - Deep architectural analysis and technical planning (Opus + extended thinking)
- **planner** - Breaks plans into atomic, executable tasks
- **reviewer** - Reviews code for quality, standards, security (Opus + extended thinking)
- **tester** - Generates and runs tests
- **git-operations-manager** - Handles all git operations (commits, pushes, branches, worktrees)

Agents are invoked automatically by slash commands.

---

## Best Practices

1. **Check context space FIRST** before any DevFlow command
2. **Use constitution-summary.md for code reviews** - Full constitution only for planning/architecture/business questions
   - Summary: 200-300 tokens (used by `/execute` and `/build-feature`) - excludes mission/domain context
   - Full: 3,000 tokens (used by `/plan`, `/tasks`, manual reference) - includes mission, domain, business rules
   - Saves ~2,700 tokens per review × 10-20 reviews per feature
3. **Check state.json** to understand current progress
4. **Load concern docs intelligently** based on feature needs
5. **Update architecture.md** after significant features
6. **Create ADRs** for major architectural decisions
7. **Use snapshots** for long-running features to manage context
8. **Monitor token usage** via Claude Code's system warnings

---

## Getting Help

- Run `/help` for Claude Code general help
- Check `.devflow/constitution.md` for project-specific standards
- Review `.devflow/features/` for examples of completed features

---

**DevFlow Version:** 1.0.0

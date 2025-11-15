---
# Note: Bash unrestricted - safe because all bash commands are read-only checks
# (finding files, testing existence, counting). File writes use Write tool.
allowed-tools: Read, Write, Glob, Grep, Bash
argument-hint:
description: Consolidate existing documentation into DevFlow domain structure
model: sonnet
version: 1.0.0
---

> **Windows Users:** This command uses bash syntax. Ensure you have Git Bash installed and are running Claude Code from a Git Bash terminal, not PowerShell. [Installation guide](https://github.com/mathewtaylor/devflow#requirements)

# Consolidate Documentation

Scan existing project documentation and consolidate into DevFlow's structured domain system.

## Prerequisites Check

- DevFlow initialized: !`test -f .devflow/constitution.md && echo "yes" || echo "no"`
- Documentation files: !`node .devflow/lib/cli.js query doc_count`

## Your Task

### Step 1: Validate Prerequisites

**If DevFlow not initialized:**
```
❌ DevFlow not initialized

DevFlow must be initialized before consolidating documentation.

Run: /init
```

**If no documentation found (count ≤ 3):**
```
📚 No significant documentation to consolidate

Found only {{count}} markdown files, which is too few to consolidate.

DevFlow domains are already set up. You can:
- Manually create domain files in .devflow/domains/
- Use domain files as you build features with /spec
```

---

### Step 2: Discovery

Scan for all markdown documentation files (excluding standard project files).

**Search command:**
```bash
find . -type f -name "*.md" \
  -not -path "*/node_modules/*" \
  -not -path "*/.git/*" \
  -not -path "*/dist/*" \
  -not -path "*/build/*" \
  -not -path "*/.devflow/*" \
  -not -name "CLAUDE.md" \
  -not -name "README.md" \
  -not -name "CONTRIBUTING.md" \
  -not -name "CHANGELOG.md" \
  -not -name "LICENSE.md" \
  -not -name "CODE_OF_CONDUCT.md"
```

Store results for analysis.

---

### Step 3: Content Analysis

For each discovered file:

1. **Read file content**
2. **Detect domain mapping** using keyword matching
3. **Score relevance** to each domain category

**Domain Keywords (case-insensitive):**

```javascript
const domainKeywords = {
  'security/authentication': [
    'login', 'signup', 'auth', 'token', 'jwt', 'oauth',
    'session', 'password', 'credential', 'signin', 'sso'
  ],
  'security/authorization': [
    'permission', 'role', 'rbac', 'abac', 'access',
    'policy', 'claim', 'scope', 'acl', 'authorization'
  ],
  'security/encryption': [
    'encrypt', 'decrypt', 'hash', 'crypto', 'ssl',
    'tls', 'certificate', 'key', 'cipher'
  ],
  'data/database-conventions': [
    'table', 'column', 'schema', 'entity', 'model',
    'orm', 'query', 'database', 'sql', 'nosql'
  ],
  'data/migrations': [
    'migration', 'schema change', 'version', 'rollback',
    'upgrade', 'alter table', 'seed'
  ],
  'data/audit-trails': [
    'audit', 'tracking', 'history', 'changelog',
    'event log', 'audit log', 'version history'
  ],
  'infrastructure/caching': [
    'cache', 'redis', 'memcached', 'performance',
    'ttl', 'invalidation', 'cache key', 'memoization'
  ],
  'infrastructure/logging': [
    'log', 'logger', 'monitoring', 'observability',
    'trace', 'debug', 'info', 'error', 'winston', 'serilog'
  ],
  'infrastructure/error-handling': [
    'error', 'exception', 'try-catch', 'failure',
    'retry', 'fallback', 'error handler'
  ],
  'infrastructure/multi-tenancy': [
    'tenant', 'organization', 'workspace', 'isolation',
    'multi-tenant', 'tenant id', 'org'
  ],
  'integration/third-party-apis': [
    'api', 'endpoint', 'rest', 'graphql', 'webhook',
    'integration', 'http', 'request', 'response', 'route'
  ],
  'integration/message-queues': [
    'queue', 'message', 'pub-sub', 'rabbitmq', 'kafka',
    'event', 'message bus', 'subscriber', 'publisher'
  ]
};
```

**Matching algorithm:**
- Count keyword occurrences in file (case-insensitive)
- If count ≥ 3 for a domain → map file to that domain
- Files can map to multiple domains (e.g., API authentication doc maps to both security/authentication and integration/third-party-apis)

---

### Step 4: Present Findings

Show comprehensive report to user:

```
📚 Documentation Discovery Report

Scanned: {{total_files_found}} markdown files
Analyzed: {{analyzed_count}} files

Detected domain mappings:

┌─────────────────────────────────────────────┐
│ Security                                     │
├─────────────────────────────────────────────┤
{{#if security_auth_files}}
│ • Authentication ({{count}} files)           │
{{#each auth_files}}
│   - {{file_path}}                            │
{{/each}}
│                                              │
{{/if}}
{{#if security_authz_files}}
│ • Authorization ({{count}} files)            │
{{#each authz_files}}
│   - {{file_path}}                            │
{{/each}}
{{/if}}
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│ Data                                         │
├─────────────────────────────────────────────┤
{{#if data_db_files}}
│ • Database Conventions ({{count}} files)     │
{{#each db_files}}
│   - {{file_path}}                            │
{{/each}}
{{/if}}
└─────────────────────────────────────────────┘

{{Similar blocks for other categories}}

Files kept as-is (general project info):
  • README.md (project overview - preserved)
  • CONTRIBUTING.md (contributor guide - preserved)
  • CHANGELOG.md (version history - preserved)
  • LICENSE.md (legal - preserved)

Total domains detected: {{domain_count}}
Total files to consolidate: {{consolidation_count}}

Consolidate documentation into DevFlow domains? (y/n)
```

---

### Step 5: Consolidation (If User Approves)

For each domain with mapped files:

#### 5.1 Check Existing Domain File

```bash
test -f .devflow/domains/{{category}}/{{domain}}.md
```

**If exists:**
```
Domain file already exists: {{category}}/{{domain}}.md

Options:
  a) Merge - Add consolidated content to existing file
  b) Skip - Keep existing file unchanged
  c) Replace - Replace with consolidated content

Choose (a/b/c):
```

**If user chooses 'b' (skip):** Move to next domain

#### 5.2 Consolidate Content

For each file mapped to this domain:

1. **Read file content**
2. **Extract relevant sections:**
   - Look for headings related to domain keywords
   - Extract content under those headings
   - Preserve code blocks and formatting
   - Note file path and section names

3. **Build consolidated content:**
   - Combine all extractions
   - Deduplicate identical content
   - Organize by type (rules, patterns, scenarios)
   - Add source attribution

#### 5.3 Create Domain File

Use `.devflow/templates/domains/concern.md.template` as base:

1. **Fill template variables:**
   - `{{CONCERN_NAME}}` → Domain name (e.g., "Authentication")
   - `{{CATEGORY}}` → Category (e.g., "Security")
   - `{{DATE}}` → Current date (YYYY-MM-DD)
   - `{{STATUS}}` → "Active"
   - `{{CONSOLIDATED}}` → true
   - `{{SOURCE_FILES}}` → List of source files
   - `{{OVERVIEW}}` → Extracted overview/description
   - `{{RULES}}` → Extracted rules and guidelines
   - `{{PATTERNS}}` → Extracted implementation patterns
   - `{{ANTI_PATTERNS}}` → Extracted anti-patterns
   - `{{SCENARIOS}}` → Extracted scenarios and examples
   - `{{SOURCE_ATTRIBUTION}}` → Detailed source breakdown
   - `{{KEYWORDS}}` → Domain keywords for auto-loading

2. **Add source attribution section:**
   ```markdown
   ## Source Attribution

   ### From {{file1}}
   - {{extracted_topic_1}}
   - {{extracted_topic_2}}

   ### From {{file2}}
   - {{extracted_topic_1}}
   - {{extracted_topic_2}}
   ```

3. **Write to `.devflow/domains/{{category}}/{{domain}}.md`**

#### 5.4 Show Progress

```
Consolidating documentation...

[1/3] Security
  ✓ Analyzing 3 files for authentication...
  ✓ Extracting from docs/auth.md
  ✓ Extracting from wiki/login-flow.md
  ✓ Extracting from API.md (Authentication section)
  → Created .devflow/domains/security/authentication.md (3 sources)

  ✓ Analyzing 2 files for authorization...
  ✓ Extracting from docs/permissions.md
  ✓ Extracting from ARCHITECTURE.md (RBAC section)
  → Created .devflow/domains/security/authorization.md (2 sources)

[2/3] Data
  ✓ Analyzing 2 files for database conventions...
  → Created .devflow/domains/data/database-conventions.md (2 sources)

[3/3] Integration
  ✓ Analyzing 4 files for API design...
  → Created .devflow/domains/integration/third-party-apis.md (4 sources)
```

#### 5.5 Update domains/_index.md

For each created domain:

1. **Extract one-line summary** from domain file (first paragraph of Overview)
2. **Add entry** to appropriate category in `_index.md`
3. **Update keywords table** with auto-loading triggers

---

### Step 6: Final Report

```
✅ Documentation consolidated successfully!

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Summary
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Created/Updated Domain Files:

Security:
  • .devflow/domains/security/authentication.md
    Sources: docs/auth.md, wiki/login-flow.md, API.md

  • .devflow/domains/security/authorization.md
    Sources: docs/permissions.md, ARCHITECTURE.md

Data:
  • .devflow/domains/data/database-conventions.md
    Sources: docs/database.md, docs/schema-design.md

  • .devflow/domains/data/migrations.md
    Sources: CONTRIBUTING.md (Migrations section)

Integration:
  • .devflow/domains/integration/third-party-apis.md
    Sources: docs/api-design.md, docs/endpoints.md, docs/webhooks.md, API.md

Updated:
  • .devflow/domains/_index.md (added {{count}} domain summaries)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Files Analyzed
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Kept as-is (general project info):
  ✓ README.md (project overview)
  ✓ CONTRIBUTING.md (contributor guide)
  ✓ CHANGELOG.md (version history)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Archival Recommendations
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

These files have been consolidated and can be safely archived:

⚠️  docs/auth.md
    → Consolidated into security/authentication.md

⚠️  docs/permissions.md
    → Consolidated into security/authorization.md

⚠️  docs/database.md
    → Consolidated into data/database-conventions.md

⚠️  docs/schema-design.md
    → Consolidated into data/database-conventions.md

⚠️  docs/api-design.md
    → Consolidated into integration/third-party-apis.md

⚠️  docs/endpoints.md
    → Consolidated into integration/third-party-apis.md

⚠️  docs/webhooks.md
    → Consolidated into integration/third-party-apis.md

⚠️  wiki/login-flow.md
    → Consolidated into security/authentication.md

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Archive Commands
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

To archive these files (recommended):

# Create archive directory with timestamp
mkdir -p archive/$(date +%Y%m%d)

# Move consolidated files to archive
mv docs/auth.md archive/$(date +%Y%m%d)/
mv docs/permissions.md archive/$(date +%Y%m%d)/
mv docs/database.md archive/$(date +%Y%m%d)/
mv docs/schema-design.md archive/$(date +%Y%m%d)/
mv docs/api-design.md archive/$(date +%Y%m%d)/
mv docs/endpoints.md archive/$(date +%Y%m%d)/
mv docs/webhooks.md archive/$(date +%Y%m%d)/
mv wiki/login-flow.md archive/$(date +%Y%m%d)/

# Optional: Remove empty directories
rmdir docs/ wiki/ 2>/dev/null || true

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Next Steps
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

1. Review consolidated files in .devflow/domains/
2. Verify all information was captured correctly
3. Test a feature with /spec to see domain auto-loading
4. Archive old documentation using commands above
5. Update README.md to reference .devflow/domains/ for technical docs

Example README update:
```
## Documentation

Technical documentation is organized in `.devflow/domains/`:
- Security: Authentication, Authorization
- Data: Database Conventions, Migrations
- Integration: API Design, Third-Party APIs

See [.devflow/domains/_index.md](.devflow/domains/_index.md) for full index.
```

Your documentation is now centralized and ready for DevFlow! 🎉
```

---

## Error Handling

**If file read fails:**
```
⚠️ Could not read {{file_path}}: {{error}}
Skipping this file...
```

**If domain creation fails:**
```
❌ Failed to create {{domain}}: {{error}}

This domain was skipped. You can manually create it later in:
.devflow/domains/{{category}}/{{domain}}.md
```

**If no domains detected:**
```
📚 No technical documentation detected

The markdown files found don't contain detectable patterns for:
- Security (authentication, authorization)
- Data (database, migrations)
- Infrastructure (caching, logging)
- Integration (APIs, message queues)

You can still manually create domain files in .devflow/domains/
using the template at .devflow/templates/domains/concern.md.template
```

---

## Notes

- **Non-destructive:** Original files are never modified
- **Repeatable:** Can be run multiple times (will offer merge/skip options)
- **Selective:** Only consolidates files with clear domain mapping
- **Transparent:** Full source attribution in consolidated files
- **Safe:** Clear recommendations on what can be archived

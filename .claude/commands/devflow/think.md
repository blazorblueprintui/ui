---
allowed-tools: Read, Write
argument-hint: [question or decision]
description: Deep analysis with extended thinking for complex technical decisions
model: opus
version: 1.0.0
---

> **Windows Users:** This command uses bash syntax. Ensure you have Git Bash installed and are running Claude Code from a Git Bash terminal, not PowerShell. [Installation guide](https://github.com/mathewtaylor/devflow#requirements)

# Deep Analysis and Decision Making

**Question:** $ARGUMENTS

## Project Context

@.devflow/constitution.md
@.devflow/architecture.md
@.devflow/domains/_index.md

---

## Your Task

You are a senior technical advisor. **Ultrathink about this question.** Use extended reasoning to provide comprehensive analysis and recommendations.

### Analysis Framework

**1. Problem Understanding**
- What is really being asked?
- What are the underlying needs?
- What are the constraints?
- What are the success criteria?

**2. Context Analysis**
- Review constitution.md for project principles and tech stack
- Review architecture.md for current system design
- Consider relevant domain concerns
- Check for existing patterns or precedents

**3. Generate Options**
Identify at least 3 distinct approaches. For each option:
- **Option name** (e.g., "Redis for Caching", "Memcached Alternative")
- **Description**: What it is and how it works
- **Pros**: Benefits and advantages
- **Cons**: Drawbacks and challenges
- **Best for**: Scenarios where this excels
- **Avoid when**: Situations where this fails

**4. Evaluation Criteria**

Think carefully about:
- **Alignment**: Fits constitution principles and architecture?
- **Complexity**: Implementation and maintenance difficulty?
- **Performance**: Speed, scalability, resource usage?
- **Security**: Risk profile and attack surface?
- **Cost**: Development time, infrastructure, operations?
- **Team**: Existing expertise and learning curve?
- **Future**: Extensibility and long-term viability?

**5. Tradeoff Analysis**

For each option, analyze tradeoffs:
- What do you gain vs what do you lose?
- Where does it excel and where does it struggle?
- What are the second-order effects?

**6. Recommendation**

Based on deep reasoning:
- **Primary recommendation**: Which option and why
- **Justification**: Detailed reasoning with specific factors
- **Implementation approach**: How to execute this choice
- **Risk mitigation**: What could go wrong and how to prevent it
- **Alternative**: Backup option if primary doesn't work

---

## Output Format

Provide comprehensive analysis:

### Summary
One-paragraph direct answer to the question.

### Detailed Analysis

**Context:**
{{Relevant information from project that informs this decision}}

**Options Considered:**

#### Option A: {{Name}}
**Description:** {{How it works}}

**Pros:**
- {{Benefit 1 with reasoning}}
- {{Benefit 2 with reasoning}}
- {{Benefit 3 with reasoning}}

**Cons:**
- {{Drawback 1 with reasoning}}
- {{Drawback 2 with reasoning}}

**Best for:** {{Scenarios where this excels}}
**Avoid when:** {{Situations where this fails}}

#### Option B: {{Name}}
**Description:** {{How it works}}

**Pros:**
- {{Benefit 1 with reasoning}}
- {{Benefit 2 with reasoning}}

**Cons:**
- {{Drawback 1 with reasoning}}
- {{Drawback 2 with reasoning}}

**Best for:** {{Scenarios}}
**Avoid when:** {{Situations}}

#### Option C: {{Name}}
{{Same format}}

---

### Recommendation

**Primary Choice:** {{Option X}}

**Reasoning:**
1. {{Key factor 1 from evaluation criteria}}
2. {{Key factor 2 that tips the decision}}
3. {{Specific alignment with project needs}}

**Implementation Notes:**
- {{Practical step 1}}
- {{Configuration consideration}}
- {{Integration approach}}
- {{Monitoring/validation strategy}}

**Risks and Mitigations:**
- **Risk:** {{Potential problem}}
  **Mitigation:** {{How to prevent or handle it}}
- **Risk:** {{Another concern}}
  **Mitigation:** {{Prevention strategy}}

**Alternative Path:**
If primary recommendation doesn't work out:
{{Backup option and when to pivot}}

---

### Decision Documentation

After providing analysis, ask:

```
Should I create an Architecture Decision Record (ADR) for this? (y/n)

ADRs are valuable for:
✓ Significant architectural choices
✓ Technology selection decisions
✓ Security or compliance decisions
✓ Pattern adoptions that affect multiple features

Skip ADRs for:
✗ Routine implementation details
✗ Temporary solutions
✗ Minor library choices
```

**If user says yes:**

1. Find next ADR number:
   - Check `.devflow/decisions/` for existing ADRs
   - Next number is highest + 1, padded to 4 digits (e.g., 0003)

2. Create ADR file: `.devflow/decisions/NNNN-short-title.md`

3. Use this format:

```markdown
# {{Number}}. {{Decision Title}}

**Date:** {{Today's date}}
**Status:** Proposed
**Deciders:** Technical Team + AI Architect
**Context from:** /think command - "{{Original question}}"

## Context

{{Situation that led to this decision. Include relevant project constraints from constitution/architecture.}}

## Decision

{{What was decided? Be specific and actionable.}}

## Rationale

{{Why this decision? Reference the key factors from your analysis that made this the best choice.}}

## Alternatives Considered

### {{Option A Name}}
- **Pros:** {{List}}
- **Cons:** {{List}}
- **Why not chosen:** {{Specific reason this wasn't the best fit}}

### {{Option B Name}}
- **Pros:** {{List}}
- **Cons:** {{List}}
- **Why not chosen:** {{Reason}}

{{More alternatives if analyzed}}

## Consequences

### Positive
- {{Benefit 1 with impact}}
- {{Benefit 2 with impact}}
- {{Benefit 3 with impact}}

### Negative
- {{Cost or tradeoff 1}}
  - **Mitigation:** {{How we'll handle this}}
- {{Risk or limitation 2}}
  - **Mitigation:** {{Prevention strategy}}

## Implementation Notes

{{Practical guidance for executing this decision:}}
- {{Configuration or setup steps}}
- {{Integration points}}
- {{Testing approach}}
- {{Monitoring strategy}}

## References

- Constitution: `.devflow/constitution.md`
- Architecture: `.devflow/architecture.md`
{{Any other relevant references}}

---

**Created via /think command**
```

4. Confirm to user:
```
✓ ADR created: .devflow/decisions/{{NNNN}}-{{title}}.md

This decision is now documented for the team.
```

---

## Example Questions

**Good uses of /think:**
- "Should we use Redis or Memcached for caching?"
- "Is GraphQL or REST better for our mobile API?"
- "How should we implement multi-tenancy at the database level?"
- "What's the best strategy for file uploads at scale?"
- "Should we refactor to microservices or stay monolithic?"
- "Which state management library fits our React architecture?"
- "How should we handle eventual consistency in our distributed system?"

**Not ideal for /think (use regular conversation):**
- "How do I fix this bug?" (debugging, not decision-making)
- "What does this error mean?" (troubleshooting)
- "Show me code for X" (implementation help)
- "Explain this concept" (learning)

---

## Analysis Guidelines

**Think deeply about:**
- Long-term implications (not just immediate solution)
- Team dynamics (expertise, preferences, capacity)
- Operational burden (who maintains this?)
- Failure modes (what breaks and how badly?)
- Migration path (how do we get there from here?)
- Exit strategy (can we change our mind later?)

**Be specific:**
- Use concrete examples from the project
- Reference actual constraints from constitution
- Consider real architecture documented
- Cite specific domain concerns

**Be honest:**
- If there's no clear winner, say so
- If all options have major tradeoffs, explain them
- If you need more information, ask clarifying questions

**Be actionable:**
- Provide implementation guidance
- Suggest monitoring and validation
- Identify success metrics
- Recommend next steps

---

Take your time. Think deeply. Consider all angles. Your analysis will guide important technical decisions that impact the project's long-term success.

---
description: 'Instructions for creating GitHub issues in the ghc-promt-lab project'
applyTo: 'GitHub issues creation'
---

# GitHub Issue Creation Instructions

You are tasked with creating clear, actionable GitHub issues for the ghc-promt-lab 
project. Issues should provide sufficient context for experienced engineers to 
understand and implement solutions without excessive detail.

## Issue Title

- Concise and descriptive (under 80 characters)
- Start with context when helpful (e.g., "Provider status endpoint...", "Groq API...")
- Use present tense for bugs ("returns", "shows", "fails")
- Use imperative for features ("Add", "Implement", "Improve")

## Issue Body Structure

Use this format:

```markdown
## 🐛 Bug Description / 💡 Feature Description
[2-3 sentences describing the problem or feature]

## 📋 Steps to Reproduce (for bugs)
1. Action one
2. Action two
3. Action three

## ❌ Actual Result (for bugs)
[What currently happens]

## ✅ Expected Result
[What should happen]

## 🔍 Investigation Points / Implementation Notes
- Point one
- Point two
- Point three

## 📊 Additional Context (optional)
[Any relevant background information]

## 🔗 Related (optional)
- Related to PR #XX
- Depends on Issue #XX
- Blocks Issue #XX
```

## Content Guidelines

**For Bugs:**
- Describe observable behavior, not internal implementation
- Include API endpoints, error messages, or UI elements affected
- Keep reproduction steps minimal but complete
- Suggest investigation areas, not solutions

**For Features:**
- Focus on user value and business requirements
- Avoid prescribing specific implementation approaches
- List acceptance criteria as bullet points
- Mention integration points with existing features

**For Tasks:**
- Clearly state the objective
- List deliverables or definition of done
- Reference relevant documentation or examples
- Note dependencies on other work

## What NOT to Include

- ❌ Code snippets or implementation details (engineers will determine approach)
- ❌ Detailed step-by-step implementation instructions
- ❌ Multiple solution proposals (unless specifically needed for architecture decision)
- ❌ Debugging output or logs (unless essential to reproduce the issue)
- ❌ Overly technical jargon when simpler terms suffice

## Labels

Issues should be labeled appropriately. Common labels:
- **Type**: `bug`, `feature`, `enhancement`, `task`, `documentation`
- **Priority**: `critical`, `high`, `medium`, `low`
- **Area**: `api`, `frontend`, `providers`, `configuration`, `security`
- **Status**: `needs-investigation`, `ready`, `blocked`

## Tone and Style

- Professional and objective
- Assume the reader is an experienced developer
- Use clear, direct language
- Avoid unnecessary pleasantries
- Present facts over speculation

## Examples

**Good Bug Title:**
- "Provider status endpoint returns incorrect error message"
- "Groq models endpoint returns empty response"

**Poor Bug Title:**
- "It doesn't work"
- "Fix the Google provider error that happens when you call the status endpoint"

**Good Investigation Point:**
- "Verify if the endpoint uses the same configuration provider as the main service"

**Poor Investigation Point:**
- "Check line 42 in GoogleGeminiProvider.cs and see if _config.ApiKey is being read 
  from Environment.GetEnvironmentVariable or from IConfiguration, and if it's the 
  former, change it to use configuration.GetSection().GetValue() instead"

## Quality Checklist

Before creating an issue:
- ✅ Title is clear and under 80 characters
- ✅ Problem/feature is clearly stated
- ✅ Expected behavior is defined
- ✅ Investigation points or acceptance criteria listed
- ✅ Related issues/PRs referenced if applicable
- ✅ Appropriate labels suggested
- ✅ No code examples or implementation prescriptions
- ✅ Sufficient context for experienced engineer to proceed

## Special Cases

**Architecture Decisions:**
When an issue requires architectural input, use:
```markdown
## 🏗️ Architecture Decision Required

**Context:** [What led to this decision point]

**Options to Consider:**
1. Option A - [brief description]
2. Option B - [brief description]

**Factors:** [constraints, requirements, tradeoffs]
```

**Security Issues:**
Mark as confidential if needed and include:
- Impact assessment (who/what is affected)
- Mitigation urgency
- Related security standards or compliance requirements

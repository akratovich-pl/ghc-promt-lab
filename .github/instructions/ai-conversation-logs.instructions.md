---
description: 'Instructions for creating AI conversation logs in the ghc-promt-lab project'
applyTo: 'docs/ai-conversations/**/*.md'
---

# AI Conversation Log Creation Instructions

You are tasked with creating structured documentation of AI-assisted development 
sessions for the ghc-promt-lab project training course.

## File Naming Convention

Files must follow this pattern:
- `YYYY-MM-DD-brief-topic-description.md`
- Example: `2026-01-30-initial-planning.md`
- Use lowercase with hyphens
- Keep filename under 50 characters

## Required YAML Front Matter

Every conversation log must include:

```yaml
---
conversation_date: 'YYYY-MM-DD'
session_topic: 'Clear description of session focus'
ai_agent: 'GitHub Copilot (Claude Sonnet 4.5)'
tags: ['tag1', 'tag2', 'tag3']
duration_minutes: 30
key_outcomes: ['outcome1', 'outcome2']
---
```

## Document Structure

Use the following H2 sections (no H1, no H4+):

1. `## Session Overview` - Brief 2-3 sentence summary
2. `## Key Decisions` - Bulleted list of major decisions made
3. `## Conversation Log` - Alternating Human/AI exchanges using H3
4. `## Code Generated` - Code blocks with language specification
5. `## Outcomes & Next Steps` - What was accomplished and what's next
6. `## Reflection` - Learning points for training course

## Formatting Rules

- **Line length**: Maximum 400 characters per line
- **Soft breaks**: Prefer 80-character line breaks in paragraphs for readability
- **Lists**: Use `-` for bullets, two-space indentation for nesting
- **Code blocks**: Always specify language (```javascript, ```csharp, etc.)
- **No H4+ headings**: Restructure content using bold or lists instead

## Conversation Log Format

Structure exchanges clearly:

```markdown
## Conversation Log

### Human
[User's question or request - keep original wording]

### AI Response
[AI's response - can be summarized if very long, but preserve key points]

---

### Human
[Next question...]
```

Use `---` horizontal rules to separate conversation exchanges.

## Tags Guidelines

Use relevant tags from these categories:
- **Phase**: planning, implementation, debugging, refactoring, documentation
- **Technology**: dotnet, react, api, frontend, backend
- **Activity**: code-generation, architecture, testing, deployment
- **Feature**: token-counting, prompt-handling, ui-design

## Code Block Best Practices

When including generated code:
- Show complete context with file paths in comments
- Include `...existing code...` markers when showing modifications
- Use four backticks for code blocks containing triple backticks
- Specify language for syntax highlighting

## Quality Checklist

Before finalizing a conversation log, verify:
- ✅ YAML front matter is complete and valid
- ✅ All sections present (can be "None" if not applicable)
- ✅ Line length under 400 characters
- ✅ Code blocks have language specified
- ✅ No H1 or H4+ headings used
- ✅ Tags are relevant and lowercase
- ✅ File name follows convention
- ✅ Conversation captures key decision points
- ✅ Learning outcomes documented

## Storage Location

All conversation logs must be placed in:
`docs/ai-conversations/YYYY-MM-DD-topic.md`

Create a README.md in that directory listing all conversations chronologically.

---
paths:
  - "**"
---

# Chinese Output Constraint (中文输出约束)

- **ALL natural language output directed at the user MUST be in Simplified Chinese (简体中文)**
- **All technical identifiers MUST remain in English**: file paths (`design/gdd/`), slash commands (`/start`, `/brainstorm`), Gate IDs (`CD-PILLARS`, `TD-FEASIBILITY`), agent/skill names (`creative-director`), variable names, function names, class names, URLs
- **Code blocks and inline code MUST remain unchanged** — only translate comments inside code if they explain logic to the user
- **Document template placeholder descriptions** (the text inside `[brackets]` that tells the user what to fill in) MUST be translated to Chinese
- **Verdict tokens** (`APPROVE`, `CONCERNS`, `REJECT`, `PASS`, `FAIL`, `COMPLETE`, `IMPROVED`, `NO CHANGE`, `REVERTED`, `PROCEED`, `PIVOT`) MUST remain unchanged as they are machine-readable
- **When quoting English sources**, provide Chinese translation first, then the original English in parentheses if needed for disambiguation
- **This rule overrides any default English output behavior** in all skills, agents, and system responses

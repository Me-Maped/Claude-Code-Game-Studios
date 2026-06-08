<p align="center">
  <h1 align="center">Claude Code Game Studios</h1>
  <p align="center">
    把一个 Claude Code session 变成完整的 game development studio。
    <br />
    49 agents。73 skills。一个协调一致的 AI team。
  </p>
</p>

<p align="center">
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT License"></a>
  <a href=".claude/agents"><img src="https://img.shields.io/badge/agents-49-blueviolet" alt="49 Agents"></a>
  <a href=".claude/skills"><img src="https://img.shields.io/badge/skills-73-green" alt="73 Skills"></a>
  <a href=".claude/hooks"><img src="https://img.shields.io/badge/hooks-12-orange" alt="12 Hooks"></a>
  <a href=".claude/rules"><img src="https://img.shields.io/badge/rules-11-red" alt="11 Rules"></a>
  <a href="https://docs.anthropic.com/en/docs/claude-code"><img src="https://img.shields.io/badge/built%20for-Claude%20Code-f5f5f5?logo=anthropic" alt="Built for Claude Code"></a>
  <a href="https://www.buymeacoffee.com/donchitos3"><img src="https://img.shields.io/badge/Buy%20Me%20a%20Coffee-Support%20this%20project-FFDD00?logo=buymeacoffee&logoColor=black" alt="Buy Me a Coffee"></a>
  <a href="https://github.com/sponsors/Donchitos"><img src="https://img.shields.io/badge/GitHub%20Sponsors-Support%20this%20project-ea4aaa?logo=githubsponsors&logoColor=white" alt="GitHub Sponsors"></a>
</p>

---

## 为什么存在

用 AI 单人做游戏很强大，但单个 chat session 没有结构。没有人会阻止你 hardcode magic numbers、跳过 design docs，或写 spaghetti code。没有 QA pass，没有 design review，也没有人问“这真的符合游戏 vision 吗？”

**Claude Code Game Studios** 通过给 AI session 加上真实 studio 的结构来解决这个问题。你得到的不是一个 general-purpose assistant，而是 49 个 organized into a studio hierarchy 的 specialized agents：directors 守护 vision，department leads 负责各自 domains，specialists 执行 hands-on work。每个 agent 都有明确 responsibilities、escalation paths 和 quality gates。

结果是：每个 decision 仍由你做出，但现在你有一支会提出正确问题、尽早抓住错误，并从 first brainstorm 到 launch 都保持项目有序的 team。

---

## 目录

- [What's Included](#whats-included)
- [Studio Hierarchy](#studio-hierarchy)
- [Slash Commands](#slash-commands)
- [Getting Started](#getting-started)
- [Upgrading](#upgrading)
- [Project Structure](#project-structure)
- [How It Works](#how-it-works)
- [Design Philosophy](#design-philosophy)
- [Customization](#customization)
- [Platform Support](#platform-support)
- [Community](#community)
- [Supporting This Project](#supporting-this-project)
- [License](#license)

---

## What's Included

| Category | Count | Description |
|----------|-------|-------------|
| **Agents** | 49 | 覆盖 design、programming、art、audio、narrative、QA 和 production 的 specialized subagents |
| **Skills** | 73 | 覆盖每个 workflow phase 的 slash commands（`/start`, `/design-system`, `/create-epics`, `/create-stories`, `/dev-story`, `/story-done` 等） |
| **Hooks** | 12 | 在 commits、pushes、asset changes、session lifecycle、agent audit trail 和 gap detection 中自动 validation |
| **Rules** | 11 | 编辑 gameplay、engine、AI、UI、network code 等位置时自动执行的 path-scoped coding standards |
| **Templates** | 41 | 用于 GDDs、UX specs、ADRs、sprint plans、HUD design、accessibility 等的 document templates |

## Studio Hierarchy

Agents 分为三个 tiers，匹配真实 studio 的运作方式：

```
Tier 1 — Directors (Opus)
  creative-director    technical-director    producer

Tier 2 — Department Leads (Sonnet)
  game-designer        lead-programmer       art-director
  audio-director       narrative-director    qa-lead
  release-manager      localization-lead

Tier 3 — Specialists (Sonnet/Haiku)
  gameplay-programmer  engine-programmer     ai-programmer
  network-programmer   tools-programmer      ui-programmer
  systems-designer     level-designer        economy-designer
  technical-artist     sound-designer        writer
  world-builder        ux-designer           prototyper
  performance-analyst  devops-engineer       analytics-engineer
  security-engineer    qa-tester             accessibility-specialist
  live-ops-designer    community-manager
```

### Engine Specialists

template 包含三大主流 engines 的 agent sets。使用与你项目匹配的一组：

| Engine | Lead Agent | Sub-Specialists |
|--------|-----------|-----------------|
| **Godot 4** | `godot-specialist` | GDScript, Shaders, GDExtension |
| **Unity** | `unity-specialist` | DOTS/ECS, Shaders/VFX, Addressables, UI Toolkit |
| **Unreal Engine 5** | `unreal-specialist` | GAS, Blueprints, Replication, UMG/CommonUI |

## Slash Commands

在 Claude Code 中输入 `/` 可访问全部 73 个 skills：

**Onboarding & Navigation**
`/start` `/help` `/project-stage-detect` `/setup-engine` `/adopt`

**Game Design**
`/brainstorm` `/map-systems` `/design-system` `/quick-design` `/review-all-gdds` `/propagate-design-change`

**Art & Assets**
`/art-bible` `/asset-spec` `/asset-audit`

**UX & Interface Design**
`/ux-design` `/ux-review`

**Architecture**
`/create-architecture` `/architecture-decision` `/architecture-review` `/create-control-manifest`

**Stories & Sprints**
`/create-epics` `/create-stories` `/dev-story` `/sprint-plan` `/sprint-status` `/story-readiness` `/story-done` `/estimate`

**Reviews & Analysis**
`/design-review` `/code-review` `/balance-check` `/content-audit` `/scope-check` `/perf-profile` `/tech-debt` `/gate-check` `/consistency-check` `/security-audit`

**QA & Testing**
`/qa-plan` `/smoke-check` `/soak-test` `/regression-suite` `/test-setup` `/test-helpers` `/test-evidence-review` `/test-flakiness` `/skill-test` `/skill-improve`

**Production**
`/milestone-review` `/retrospective` `/bug-report` `/bug-triage` `/reverse-document` `/playtest-report`

**Release**
`/release-checklist` `/launch-checklist` `/changelog` `/patch-notes` `/hotfix` `/day-one-patch`

**Creative & Content**
`/prototype` `/onboard` `/localize`

**Team Orchestration**（在单个 feature 上协调多个 agents）
`/team-combat` `/team-narrative` `/team-ui` `/team-release` `/team-polish` `/team-audio` `/team-level` `/team-live-ops` `/team-qa`

## Getting Started

### Prerequisites

- [Git](https://git-scm.com/)
- [Claude Code](https://docs.anthropic.com/en/docs/claude-code)（`npm install -g @anthropic-ai/claude-code`）
- **Recommended**: [jq](https://jqlang.github.io/jq/)（用于 hook validation）和 Python 3（用于 JSON validation）

如果 optional tools 缺失，所有 hooks 都会 graceful fail；不会破坏任何东西，只是失去对应 validation。

### Setup

1. **Clone 或作为 template 使用**：
   ```bash
   git clone https://github.com/Donchitos/Claude-Code-Game-Studios.git my-game
   cd my-game
   ```

2. **打开 Claude Code** 并启动 session：
   ```bash
   claude
   ```

3. **运行 `/start`** — system 会询问你当前状态（no idea、vague concept、clear design、existing work），并引导你进入正确 workflow。不会做 assumptions。

   如果你已经知道需要什么，也可以直接跳到 specific skill：
   - `/brainstorm` — 从零探索 game ideas
   - `/setup-engine godot 4.6` — 如果已确定 engine，则配置它
   - `/project-stage-detect` — 分析 existing project

## Upgrading

已经在使用旧版本 template？参见 [UPGRADING.md](UPGRADING.md)，其中有逐步 migration instructions、版本间 changed 内容 breakdown，以及哪些 files 可以安全 overwrite、哪些需要 manual merge。

## Project Structure

```
CLAUDE.md                           # Master configuration
.claude/
  settings.json                     # Hooks, permissions, safety rules
  agents/                           # 49 agent definitions (markdown + YAML frontmatter)
  skills/                           # 73 slash commands (subdirectory per skill)
  hooks/                            # 12 hook scripts (bash, cross-platform)
  rules/                            # 11 path-scoped coding standards
  statusline.sh                     # Status line script (context%, model, stage, epic breadcrumb)
  docs/
    workflow-catalog.yaml           # 7-phase pipeline definition (read by /help)
    templates/                      # 41 document templates
src/                                # Game source code
assets/                             # Art, audio, VFX, shaders, data files
design/                             # GDDs, narrative docs, level designs
docs/                               # Technical documentation and ADRs
tests/                              # Test suites (unit, integration, performance, playtest)
tools/                              # Build and pipeline tools
prototypes/                         # Throwaway prototypes (isolated from src/)
production/                         # Sprint plans, milestones, release tracking
```

## How It Works

### Agent Coordination

Agents 遵循结构化 delegation model：

1. **Vertical delegation** — directors delegate 给 leads，leads delegate 给 specialists
2. **Horizontal consultation** — same-tier agents 可以互相 consult，但不能做 binding cross-domain decisions
3. **Conflict resolution** — disagreements 会向上 escalate 到 shared parent（design 找 `creative-director`，technical 找 `technical-director`）
4. **Change propagation** — cross-department changes 由 `producer` 协调
5. **Domain boundaries** — 没有 explicit delegation 时，agents 不会修改自己 domain 之外的 files

### Collaborative, Not Autonomous

这**不是** auto-pilot system。每个 agent 都遵循严格 collaboration protocol：

1. **Ask** — agents 在提出 solutions 前先问问题
2. **Present options** — agents 展示 2-4 个带 pros/cons 的 options
3. **You decide** — user 始终做最终决定
4. **Draft** — agents 在 finalizing 前展示 work
5. **Approve** — 没有你的 sign-off，不写入任何内容

你保持控制权。agents 提供 structure 和 expertise，不提供 autonomy。

### Automated Safety

**Hooks** 会在每个 session 自动运行：

| Hook | Trigger | What It Does |
|------|---------|--------------|
| `validate-commit.sh` | PreToolUse (Bash) | 检查 hardcoded values、TODO format、JSON validity、design doc sections；如果 command 不是 `git commit` 则提前 exit |
| `validate-push.sh` | PreToolUse (Bash) | push 到 protected branches 时 warn；如果 command 不是 `git push` 则提前 exit |
| `validate-assets.sh` | PostToolUse (Write/Edit) | 验证 naming conventions 和 JSON structure；如果 file 不在 `assets/` 中则提前 exit |
| `session-start.sh` | Session open | 显示 current branch 和 recent commits，帮助定位 context |
| `detect-gaps.sh` | Session open | 检测 fresh projects（建议 `/start`）以及 code 或 prototypes 存在时缺失的 design docs |
| `pre-compact.sh` | Before compaction | 保存 session progress notes |
| `post-compact.sh` | After compaction | 提醒 Claude 从 `active.md` restore session state |
| `notify.sh` | Notification event | 通过 PowerShell 显示 Windows toast notification |
| `session-stop.sh` | Session close | 将 `active.md` archive 到 session log 并记录 git activity |
| `log-agent.sh` | Agent spawned | Audit trail start，记录 subagent invocation |
| `log-agent-stop.sh` | Agent stops | Audit trail stop，完成 subagent record |
| `validate-skill-change.sh` | PostToolUse (Write/Edit) | 在任何 `.claude/skills/` change 后建议运行 `/skill-test` |

> **Note**: `validate-commit.sh`、`validate-assets.sh` 和 `validate-skill-change.sh` 会在每次 Bash/Write tool call 上触发，并在 command 或 file path 不相关时立即退出（exit 0）。这是正常 hook behavior，不是 performance concern。

`settings.json` 中的 **Permission rules** 会自动 allow safe operations（git status、test runs），并 block dangerous ones（force push、`rm -rf`、读取 `.env` files）。

### Path-Scoped Rules

Coding standards 会基于 file location 自动执行：

| Path | Enforces |
|------|----------|
| `src/gameplay/**` | Data-driven values, delta time usage, no UI references |
| `src/core/**` | Zero allocations in hot paths, thread safety, API stability |
| `src/ai/**` | Performance budgets, debuggability, data-driven parameters |
| `src/networking/**` | Server-authoritative, versioned messages, security |
| `src/ui/**` | No game state ownership, localization-ready, accessibility |
| `design/gdd/**` | Required 8 sections, formula format, edge cases |
| `tests/**` | Test naming, coverage requirements, fixture patterns |
| `prototypes/**` | Relaxed standards, README required, hypothesis documented |

## Design Philosophy

这个 template 建立在 professional game development practices 之上：

- **MDA Framework** — 用于 game design 的 Mechanics, Dynamics, Aesthetics analysis
- **Self-Determination Theory** — 用于 player motivation 的 Autonomy, Competence, Relatedness
- **Flow State Design** — 用于 player engagement 的 challenge-skill balance
- **Bartle Player Types** — Audience targeting 和 validation
- **Verification-Driven Development** — Tests first，再 implementation

## Customization

这是一个 **template**，不是 locked framework。所有内容都应该按需 customize：

- **Add/remove agents** — 删除不需要的 agent files，为你的 domains 添加新的
- **Edit agent prompts** — 调整 agent behavior，添加 project-specific knowledge
- **Modify skills** — 调整 workflows 以匹配 team process
- **Add rules** — 为 project directory structure 创建新的 path-scoped rules
- **Tune hooks** — 调整 validation strictness，添加 new checks
- **Pick your engine** — 使用 Godot、Unity 或 Unreal agent set（或不用）
- **Set review intensity** — `full`（all director gates）、`lean`（仅 phase gates）或 `solo`（none）。在 `/start` 中设置，或编辑 `production/review-mode.txt`。也可在任意 skill 上使用 `--review solo` 做 per-run override。

## Platform Support

主要 development 和 testing 环境是 **Windows 10** with Git Bash。所有 hooks 都使用 POSIX-compatible patterns（`grep -E`，不是 `grep -P`），并为 missing tools 提供 fallbacks，因此应可在 macOS 和 Linux 上运行。`notify.sh` hook 使用 PowerShell 显示 Windows toast notifications，在其他平台上是 no-op；macOS/Linux 的 desktop notifications 尚未接线。Cross-platform testing 正在进行；如遇 platform-specific breakage，请提交 issues。

## Community

- **Discussions** — [GitHub Discussions](https://github.com/Donchitos/Claude-Code-Game-Studios/discussions)，用于 questions、ideas 和展示你的作品
- **Issues** — [Bug reports and feature requests](https://github.com/Donchitos/Claude-Code-Game-Studios/issues)

---

## Supporting This Project

Claude Code Game Studios 是 free and open source。如果它为你节省时间，或帮助你 ship game，可以考虑支持持续开发：

<p>
  <a href="https://www.buymeacoffee.com/donchitos3"><img src="https://img.shields.io/badge/Buy%20Me%20a%20Coffee-FFDD00?style=for-the-badge&logo=buy-me-a-coffee&logoColor=black" alt="Buy Me a Coffee"></a>
  &nbsp;
  <a href="https://github.com/sponsors/Donchitos"><img src="https://img.shields.io/badge/GitHub%20Sponsors-ea4aaa?style=for-the-badge&logo=githubsponsors&logoColor=white" alt="GitHub Sponsors"></a>
</p>

- **[Buy Me a Coffee](https://www.buymeacoffee.com/donchitos3)** — one-time support
- **[GitHub Sponsors](https://github.com/sponsors/Donchitos)** — recurring support through GitHub

Sponsorships 会帮助投入维护 skills、添加 new agents、跟进 Claude Code 和 engine API changes，以及响应 community issues 所需的时间。

---

*Built for Claude Code. Maintained and extended — 欢迎通过 [GitHub Discussions](https://github.com/Donchitos/Claude-Code-Game-Studios/discussions) 贡献。*

## License

MIT License。详情见 [LICENSE](LICENSE)。

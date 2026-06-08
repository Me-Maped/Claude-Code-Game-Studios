# Claude Code Game Studios -- 完整 Workflow Guide

> **如何使用 Agent Architecture 从零开始做出并发布一款游戏。**
>
> 本 guide 会带你走完使用 49-agent system、73 个 slash commands 和 12 个 automated hooks 进行游戏开发的每个阶段。它假设你已经安装 Claude Code，并且正在 project root 下工作。
>
> pipeline 一共有 7 个 phase。每个 phase 都有正式的 gate（`/gate-check`），必须通过后才能进入下一阶段。权威 phase sequence 定义在 `.claude/docs/workflow-catalog.yaml` 中，并由 `/help` 读取。

---

## 目录

1. [Quick Start](#quick-start)
2. [Phase 1: Concept](#phase-1-concept)
3. [Phase 2: Systems Design](#phase-2-systems-design)
4. [Phase 3: Technical Setup](#phase-3-technical-setup)
5. [Phase 4: Pre-Production](#phase-4-pre-production)
6. [Phase 5: Production](#phase-5-production)
7. [Phase 6: Polish](#phase-6-polish)
8. [Phase 7: Release](#phase-7-release)
9. [Cross-Cutting Concerns](#cross-cutting-concerns)
10. [Appendix A: Agent Quick-Reference](#appendix-a-agent-quick-reference)
11. [Appendix B: Slash Command Quick-Reference](#appendix-b-slash-command-quick-reference)
12. [Appendix C: Common Workflows](#appendix-c-common-workflows)

---

## Quick Start

### 你需要准备什么

开始前，请确认你具备：

- **Claude Code** 已安装并可正常运行
- **Git**，Windows 使用 Git Bash，Mac/Linux 使用标准 terminal
- **jq**（可选但推荐；如果缺失，hooks 会回退到 `grep`）
- **Python 3**（可选；部分 hooks 会用它做 JSON validation）

### Step 1: Clone and Open

```bash
git clone <repo-url> my-game
cd my-game
```

### Step 2: Run /start

如果这是你的第一次 session：

```
/start
```

这个 guided onboarding 会询问你当前处于什么状态，并把你路由到正确的 phase：

- **Path A** -- 还没有想法：路由到 `/brainstorm`
- **Path B** -- 有模糊想法：带 seed 路由到 `/brainstorm`
- **Path C** -- 已有清晰 concept：路由到 `/setup-engine` 和 `/map-systems`
- **Path D1** -- 现有项目，artifact 较少：走正常 flow
- **Path D2** -- 现有项目，已有 GDDs/ADRs：先运行 `/project-stage-detect`，再运行 `/adopt` 做 brownfield migration

### Step 3: 验证 Hooks 是否正常工作

启动一个新的 Claude Code session。你应该看到来自 `session-start.sh` hook 的输出：

```
=== Claude Code Game Studios -- Session Context ===
Branch: main
Recent commits:
  abc1234 Initial commit
===================================
```

如果你看到了这些内容，说明 hooks 正常工作。如果没有，请检查 `.claude/settings.json`，确认 hook paths 对你的 OS 是正确的。

### Step 4: 随时请求帮助

任何时候都可以运行：

```
/help
```

它会从 `production/stage.txt` 读取当前 phase，检查已有 artifacts，然后明确告诉你下一步应该做什么。它会区分 REQUIRED next steps 和 OPTIONAL opportunities。

### Step 5: 创建目录结构

目录会按需创建。系统期望以下 layout：

```
src/                  # 游戏源代码
  core/               # Engine/framework code
  gameplay/           # Gameplay systems
  ai/                 # AI systems
  networking/         # Multiplayer code
  ui/                 # UI code
  tools/              # Dev tools
assets/               # Game assets
  art/                # Sprites, models, textures
  audio/              # Music, SFX
  vfx/                # Particle effects
  shaders/            # Shader files
  data/               # JSON config/balance data
design/               # Design documents
  gdd/                # Game design documents
  narrative/          # Story, lore, dialogue
  levels/             # Level design documents
  balance/            # Balance spreadsheets and data
  ux/                 # UX specifications
docs/                 # Technical documentation
  architecture/       # Architecture Decision Records
  api/                # API documentation
  postmortems/        # Post-mortems
tests/                # Test suites
prototypes/           # Throwaway prototypes
production/           # Sprint plans, milestones, releases
  sprints/
  milestones/
  releases/
  epics/              # Epic and story files (from /create-epics + /create-stories)
  playtests/          # Playtest reports
  session-state/      # Ephemeral session state (gitignored)
  session-logs/       # Session audit trail (gitignored)
```

> **Tip:** 第一天不需要创建所有目录。到达需要它们的 phase 时再创建即可。关键是创建时遵循这个 structure，因为 **rules system** 会基于 file paths 强制执行 standards。`src/gameplay/` 中的 code 会应用 gameplay rules，`src/ai/` 中的 code 会应用 AI rules，依此类推。

---

## Phase 1: Concept

### 本 Phase 会发生什么

你会从“没有想法”或“模糊想法”推进到一个结构化的 game concept document，其中包含明确的 pillars 和 player journey。这里要弄清楚你在做**什么**以及**为什么**做。

### Phase 1 Pipeline

```
/brainstorm  -->  game-concept.md  -->  /design-review  -->  /setup-engine
     |                                        |                    |
     v                                        v                    v
  10 concepts     Concept doc with       Validation          Engine pinned in
  MDA analysis    pillars, MDA,          of concept          technical-preferences.md
  Player motiv.   core loop, USP         document
                                                                   |
                                                                   v
                                                             /prototype
                                                       (concept prototype — 1-3 days)
                                                        PROCEED ↓     PIVOT → /brainstorm
                                                                   |
                                                                   v (PROCEED)
                                                             /map-systems
                                                                   |
                                                                   v
                                                            systems-index.md
                                                            (all systems, deps,
                                                             priority tiers)
```

### Step 1.1: 使用 /brainstorm 进行 Brainstorm

这是起点。运行 brainstorm skill：

```
/brainstorm
```

或者带上 genre hint：

```
/brainstorm roguelike deckbuilder
```

**会发生什么：** brainstorm skill 会使用专业 studio techniques，引导你完成一个协作式 6-phase ideation process：

1. 询问你的兴趣、主题和 constraints
2. 生成 10 个 concept seeds，并附带 MDA（Mechanics, Dynamics, Aesthetics）analysis
3. 你挑选 2-3 个最喜欢的方向做 deep analysis
4. 执行 player motivation mapping 和 audience targeting
5. 你选择最终 concept
6. 将其整理成 `design/gdd/game-concept.md`

concept document 包含：

- Elevator pitch（一句话）
- Core fantasy（玩家想象自己在做什么）
- MDA breakdown
- Target audience（Bartle types, demographics）
- Core loop diagram
- Unique selling proposition
- Comparable titles and differentiation
- Game pillars（3-5 个不可妥协的 design values）
- Anti-pillars（游戏刻意避免的事项）

### Step 1.2: Review the Concept（可选但推荐）

```
/design-review design/gdd/game-concept.md
```

在继续前验证 structure 和 completeness。

### Step 1.3: 选择 Engine

```
/setup-engine
```

或者指定 engine：

```
/setup-engine godot 4.6
```

**/setup-engine 会做什么：**

- 填充 `.claude/docs/technical-preferences.md`，包含 naming conventions、performance budgets 和 engine-specific defaults
- 检测 knowledge gaps（engine version 新于 LLM training data）并建议交叉参考 `docs/engine-reference/`
- 在 `docs/engine-reference/` 中创建 version-pinned reference docs

**为什么这很重要：** 一旦设置了 engine，系统就知道应该使用哪些 engine-specialist agents。如果选择 Godot，`godot-specialist`、`godot-gdscript-specialist`、`godot-shader-specialist` 等 agents 会成为你的常用专家。

### Step 1.4: 将 Concept 拆解为 Systems

在编写各个 GDDs 之前，先枚举游戏需要的所有 systems：

```
/map-systems
```

这会创建 `design/gdd/systems-index.md`，一个 master tracking document，用于：

- 列出游戏需要的每个 system（combat、movement、UI 等）
- 映射 systems 之间的 dependencies
- 分配 priority tiers（MVP, Vertical Slice, Alpha, Full Vision）
- 确定 design order（Foundation > Core > Feature > Presentation > Polish）

这是进入 Phase 2 前的**必需**步骤。来自 155 份 game postmortems 的研究确认，跳过 systems enumeration 会让 production 成本增加 5-10 倍。

### Phase 1 Gate

```
/gate-check concept
```

**通过要求：**

- Engine 已在 `technical-preferences.md` 中配置
- `design/gdd/game-concept.md` 存在并包含 pillars
- `design/gdd/systems-index.md` 存在并包含 dependency ordering

**Verdict:** PASS / CONCERNS / FAIL。CONCERNS 在风险被承认后可以通过；FAIL 会阻止进入下一阶段。

---

## Phase 2: Systems Design

### 本 Phase 会发生什么

你会创建所有定义游戏如何运作的 design documents。此时还不写 code，这一阶段是纯 design。systems index 中识别出的每个 system 都会有自己的 GDD，并按 section 编写、单独 review，最后所有 GDDs 会一起做 consistency cross-check。

### Phase 2 Pipeline

```
/map-systems next  -->  /design-system  -->  /design-review
       |                     |                     |
       v                     v                     v
  Picks next system    Section-by-section     Validates 8
  from systems-index   GDD authoring          required sections
                       (incremental writes)   APPROVED/NEEDS REVISION
       |
       |  (repeat for each MVP system)
       v
/review-all-gdds
       |
       v
  Cross-GDD consistency + design theory review
  PASS / CONCERNS / FAIL
```

### Step 2.1: 编写 System GDDs

按照 dependency order 使用 guided workflow 设计每个 system：

```
/map-systems next
```

它会选择最高优先级且尚未设计的 system，并交给 `/design-system`，后者会逐 section 引导你创建该 system 的 GDD。

你也可以直接设计指定 system：

```
/design-system combat-system
```

**/design-system 会做什么：**

1. 读取你的 game concept、systems index，以及任何 upstream/downstream GDDs
2. 运行 Technical Feasibility Pre-Check（domain mapping + feasibility brief）
3. 逐一引导你完成 8 个 required GDD sections
4. 每个 section 遵循：Context > Questions > Options > Decision > Draft > Approval > Write
5. 每个 section 在 approval 后立即写入文件（即使 crash 也能保留）
6. 标记与现有 approved GDDs 的冲突
7. 按 category 路由到 specialist agents（systems-designer 负责 math，economy-designer 负责 economy，narrative-director 负责 story systems）

**8 个 required GDD sections：**

| # | Section | 内容 |
|---|---------|------|
| 1 | **Overview** | system 的一段式 summary |
| 2 | **Player Fantasy** | 玩家使用该 system 时想象/感受到什么 |
| 3 | **Detailed Rules** | 无歧义的 mechanical rules |
| 4 | **Formulas** | 每个 calculation，包含 variable definitions 和 ranges |
| 5 | **Edge Cases** | 异常情况如何处理，需要明确解决 |
| 6 | **Dependencies** | 与哪些其他 systems 相连（bidirectional） |
| 7 | **Tuning Knobs** | designers 可安全修改哪些 values，并给出 safe ranges |
| 8 | **Acceptance Criteria** | 如何测试它有效，需要具体且可衡量 |

另有 **Game Feel** section：feel reference、input responsiveness（ms/frames）、animation feel targets（startup/active/recovery）、impact moments、weight profile。

### Step 2.2: Review 每个 GDD

在开始下一个 system 前，验证当前 GDD：

```
/design-review design/gdd/combat-system.md
```

检查 8 个 sections 的 completeness、formula clarity、edge case resolution、bidirectional dependencies 和 testable acceptance criteria。

**Verdict:** APPROVED / NEEDS REVISION / MAJOR REVISION。只有 APPROVED GDDs 应该继续推进。

### Step 2.3: 不需要完整 GDDs 的小改动

对于 tuning changes、小 additions，或不值得创建完整 GDD 的 tweaks：

```
/quick-design "add 10% damage bonus for flanking attacks"
```

它会在 `design/quick-specs/` 中创建 lightweight spec，而不是完整的 8-section GDD。用于 tuning、number changes 和 small additions。

### Step 2.4: Cross-GDD Consistency Review

所有 MVP system GDDs 单独 approved 后：

```
/review-all-gdds
```

它会同时读取 ALL GDDs，并运行两个 analysis phases：

**Phase 1 -- Cross-GDD Consistency:**
- Dependency bidirectionality（A references B，B 是否 reference A？）
- Systems 之间的 rule contradictions
- 指向 renamed 或 removed systems 的 stale references
- Ownership conflicts（两个 systems 声称拥有同一 responsibility）
- Formula range compatibility（System A 的 output 是否适配 System B 的 input？）
- Acceptance criteria cross-check

**Phase 2 -- Design Theory（Game Design Holism）:**
- Competing progression loops（两个 systems 是否争夺同一 reward space？）
- Cognitive load（一次超过 4 个 active systems？）
- Dominant strategies（某个 approach 让其他 approach 都失去意义）
- Economic loop analysis（sources and sinks 是否 balanced？）
- Systems 之间 difficulty curve consistency
- Pillar alignment 和 anti-pillar violations
- Player fantasy coherence

**Output:** `design/gdd/gdd-cross-review-[date].md`，包含 verdict。

### Step 2.5: Narrative Design（如适用）

如果你的游戏包含 story、lore 或 dialogue，就在此时构建：

1. **World-building** -- 使用 `world-builder` 定义 factions、history、geography 和 world rules
2. **Story structure** -- 使用 `narrative-director` 设计 story arcs、character arcs 和 narrative beats
3. **Character sheets** -- 使用 `narrative-character-sheet.md` template

### Phase 2 Gate

```
/gate-check systems-design
```

**通过要求：**

- `systems-index.md` 中所有 MVP systems 均为 `Status: Approved`
- 每个 MVP system 都有 reviewed GDD
- Cross-GDD review report 存在（`design/gdd/gdd-cross-review-*.md`），且 verdict 为 PASS 或 CONCERNS（不是 FAIL）

---

## Phase 3: Technical Setup

### 本 Phase 会发生什么

你会做出关键 technical decisions，将其记录为 Architecture Decision Records（ADRs），通过 review 验证，并生成一份 control manifest，为 programmers 提供扁平、可执行的 rules。你也会建立 UX foundations。

### Phase 3 Pipeline

```
/create-architecture  -->  /architecture-decision (x N)  -->  /architecture-review
        |                          |                                   |
        v                          v                                   v
  Master architecture       Per-decision ADRs              Validates completeness,
  document covering         in docs/architecture/          dependency ordering,
  all systems               adr-*.md                       engine compatibility
                                                                      |
                                                                      v
                                                         /create-control-manifest
                                                                      |
                                                                      v
                                                         Flat programmer rules
                                                         docs/architecture/
                                                         control-manifest.md
        Also in this phase:
        -------------------
        /ux-design  -->  /ux-review
        Accessibility requirements doc
        Interaction pattern library
```

### Step 3.1: Master Architecture Document

```
/create-architecture
```

创建 overarching architecture document：`docs/architecture/architecture.md`，覆盖 system boundaries、data flow 和 integration points。

### Step 3.2: Architecture Decision Records（ADRs）

对每个重要 technical decision：

```
/architecture-decision "State Machine vs Behavior Tree for NPC AI"
```

**会发生什么：** skill 会引导你创建 ADR，包含：
- Context 和 decision drivers
- 所有 options，附 pros/cons 和 engine compatibility
- Chosen option 及 rationale
- Consequences（positive, negative, risks）
- Dependencies（Depends On, Enables, Blocks, Ordering Note）
- GDD Requirements Addressed（通过 TR-ID 链接）

ADRs 的 lifecycle：Proposed > Accepted > Superseded/Deprecated。

**gate check 前至少需要 3 个 Foundation-layer ADRs。**

**Retrofitting existing ADRs:** 如果你已有来自 brownfield project 的 ADRs：

```
/architecture-decision retrofit docs/architecture/adr-005.md
```

它会检测缺少哪些 template sections，并只补充缺失部分，不覆盖现有内容。

### Step 3.3: Architecture Review

```
/architecture-review
```

整体验证所有 ADRs：
- ADR dependencies 的 topological sort（检测 cycles）
- Engine compatibility verification
- GDD Revision Flags（基于 ADR choices 标记需要更新的 GDD sections）
- TR-ID registry maintenance（`docs/architecture/tr-registry.yaml`）

### Step 3.4: Control Manifest

```
/create-control-manifest
```

读取所有 Accepted ADRs，生成扁平的 programmer rules sheet：

```
docs/architecture/control-manifest.md
```

它包含按 code layer 组织的 Required patterns、Forbidden patterns 和 Guardrails。后续创建的 stories 会嵌入 manifest version date，以便检测 staleness。

### Step 3.5: Accessibility Requirements

使用 template 创建 `design/accessibility-requirements.md`。选择一个 tier（Basic / Standard / Comprehensive / Exemplary），并填写 4-axis feature matrix（visual, motor, cognitive, auditory）。

这个 document 在 Phase 3 必需，因为 Phase 4 编写的 UX specs 会 reference 该 tier；它是 design prerequisite，而不是 UX deliverable。

### Phase 3 Gate

```
/gate-check technical-setup
```

**通过要求：**

- `docs/architecture/architecture.md` 存在
- 至少 3 个 ADRs 存在且为 Accepted
- Architecture review report 存在
- `docs/architecture/control-manifest.md` 存在
- `design/accessibility-requirements.md` 存在

---

## Phase 4: Pre-Production

### 本 Phase 会发生什么

你会为关键 screens 创建 UX specs，prototype 风险较高的 mechanics，将 design documents 转换为可实现的 stories，规划第一个 sprint，并构建一个证明 core loop 有趣的 Vertical Slice。

### Phase 4 Pipeline

```
/ux-design  -->  /vertical-slice  -->  /create-epics  -->  /create-stories  -->  /sprint-plan
    |                   |                   |                   |                       |
    v                   v                   v                   v                       v
  UX specs       Production-quality   Epic files in       Story files in          First sprint with
  design/ux/     end-to-end build     production/         production/             prioritized stories
                 in prototypes/       epics/*/EPIC.md     epics/*/story-*.md      production/sprints/
                 PROCEED/PIVOT/KILL   (one per module)    (one per behaviour)     sprint-*.md
    |                                                          |
    v                                                          v
 /ux-review                                             /story-readiness
 (validates specs                                       (validates each story
  before epics)                                          before pickup)
                                                               |
                                                               v
                                                           /dev-story
                                                         (implements the story,
                                                          routes to right agent)
```

### Step 4.1: 关键 Screens 的 UX Specs

在编写 epics 前，先创建 UX specs，让 story authors 知道有哪些 screens，以及它们必须支持哪些 player interactions。

**UX Specs:**

```
/ux-design main-menu
/ux-design core-gameplay-hud
```

三种 modes：screen/flow、HUD、interaction patterns。输出到 `design/ux/`。每个 spec 包含：player need、layout zones、states、interaction map、data requirements、events fired、accessibility、localization。

它会读取 Phase 3 写好的 `accessibility-requirements.md`，以及 `technical-preferences.md` 中的 input method config，用于驱动 accessibility 和 input coverage checks；无需每个 screen 重新指定。

> **Tip:** `/design-system` 会为每个带 UI requirements 的 system 输出 📌 UX Flag。用这些 flags 作为需要哪些 screen specs 的 checklist。

**Interaction Pattern Library:**

```
/ux-design interaction-patterns
```

创建 `design/ux/interaction-patterns.md`，包含 16 个 standard controls，加上 game-specific patterns（inventory slot、ability icon、HUD bar、dialogue box 等），并定义 animation 和 sound standards。

**UX Review:**

```
/ux-review all
```

验证 UX specs 是否符合 GDD alignment 和 accessibility tier compliance。输出 APPROVED / NEEDS REVISION / MAJOR REVISION NEEDED verdict。

### Step 4.2: 构建 Vertical Slice

Vertical Slice 是 production-quality proof，用来证明在投入完整 Production 前，你可以端到端构建完整 game loop。

```
/vertical-slice
```

**它证明什么：** 玩家从零开始，能否在几分钟内、无需 developer guidance 地体验到 core fantasy？

**它构建什么：** 一个接近 production-quality 的 playable build，覆盖至少一个完整的 [start → challenge → resolution] cycle。使用真实 architecture layers、真实 naming conventions、无 hardcoded values；但不要求 final art 或 audio。它不是 concept prototype 那样的一次性 throwaway；它证明 production pipeline feasibility。

**关于 concept prototyping：** 如果你在 Phase 1（Concept）运行过 `/prototype`，说明你已经验证 core idea 有趣。现在 Vertical Slice 验证的是你能否正确地构建它。两者回答不同问题。如果你跳过了 concept prototype，那么在投入完整 slice 前，先运行一次 prototype 是合理的。

**Verdict:** Vertical Slice 会输出 PROCEED / PIVOT / KILL verdict。
- **PROCEED** → 进入 Step 4.3（epics and stories）
- **PIVOT** → 使用 `/design-system [mechanic]` 修订受影响的 GDDs，然后重新运行 `/vertical-slice`
- **KILL** → 带着学到的内容回到 `/brainstorm`

### Step 4.3: 从 Design Artifacts 创建 Epics 和 Stories

```
/create-epics layer: foundation
/create-stories [epic-slug]   # repeat for each epic
/create-epics layer: core
/create-stories [epic-slug]   # repeat for each core epic
```

`/create-epics` 会读取 GDDs、ADRs 和 architecture 来定义 epic scope，每个 architectural module 一个 epic。然后 `/create-stories` 将每个 epic 拆成 `production/epics/[slug]/` 中的可实现 story files。每个 story 嵌入：
- GDD requirement references（TR-IDs，不引用原文，保持新鲜）
- ADR references（只来自 Accepted ADRs；Proposed ADRs 会导致 `Status: Blocked`）
- Control manifest version date（用于 staleness detection）
- Engine-specific implementation notes
- 来自 GDD 的 acceptance criteria

stories 存在后，运行 `/dev-story [story-path]` 来实现某个 story，它会自动路由到正确的 programmer agent。

### Step 4.4: Pickup 前验证 Stories

```
/story-readiness production/epics/combat/story-combat-damage-calc.md
```

检查：Design completeness、Architecture coverage、Scope clarity、Definition of Done。Verdict: READY / NEEDS WORK / BLOCKED。

### Step 4.5: Effort Estimation

```
/estimate production/epics/combat/story-combat-damage-calc.md
```

提供 effort estimates 和 risk assessment。

### Step 4.6: 规划第一个 Sprint

```
/sprint-plan new
```

**会发生什么：** `producer` agent 会协作进行 sprint planning：
- 询问 sprint goal 和 available time
- 将目标拆成 Must Have / Should Have / Nice to Have tasks
- 识别 risks 和 blockers
- 创建 `production/sprints/sprint-01.md`
- 填充 `production/sprint-status.yaml`（machine-readable story tracking）

### Step 4.7: Vertical Slice（Hard Gate）

进入 Production 前，必须构建并 playtest 一个 Vertical Slice：

- 一个完整端到端 core loop，可从开始玩到结束
- Representative quality（不能全部是 placeholder）
- 至少 3 次 unguided sessions
- 已编写 playtest report（`/playtest-report`）

这是 **hard gate**；如果没有人类 unguided 地玩过 build，`/gate-check` 会自动 FAIL。

### Phase 4 Gate

```
/gate-check pre-production
```

**通过要求：**

- `design/ux/` 中至少 1 个 UX spec 已 reviewed
- UX review 已完成（APPROVED，或 NEEDS REVISION 且 documented risks）
- 至少 1 个带 README 的 prototype
- `production/epics/[epic-slug]/` 中存在 story files
- 至少 1 个 sprint plan 存在
- 至少 1 个 playtest report 存在（Vertical Slice 已进行 3+ sessions）

---

## Phase 5: Production

### 本 Phase 会发生什么

这是核心 production loop。你会以 sprints（通常 1-2 周）为单位工作，逐 story 实现 features、跟踪进度，并通过结构化 completion review 关闭 stories。这个 phase 会重复，直到游戏 content-complete。

### Phase 5 Pipeline（Per Sprint）

```
/sprint-plan new  -->  /story-readiness  -->  implement  -->  /story-done
       |                     |                    |                |
       v                     v                    v                v
  Sprint created       Story validated      Code written     8-phase review:
  sprint-status.yaml   READY verdict        Tests pass       verify criteria,
  populated                                                  check deviations,
                                                             update story status
       |
       |  (repeat per story until sprint complete)
       v
  /sprint-status  (quick 30-line snapshot anytime)
  /scope-check    (if scope is growing)
  /retrospective  (at sprint end)
```

### Step 5.1: Story Lifecycle

Production phase 围绕 **story lifecycle** 展开：

```
/story-readiness  -->  implement  -->  /story-done  -->  next story
```

**1. Story Readiness:** pickup 某个 story 前，先验证：

```
/story-readiness production/epics/combat/story-combat-damage-calc.md
```

它检查 design completeness、architecture coverage、ADR status（如果 ADR 仍是 Proposed 则 block）、control manifest version（过期则 warn）和 scope clarity。Verdict: READY / NEEDS WORK / BLOCKED。

**2. Implementation:** 与合适的 agents 协作：

- `gameplay-programmer` 负责 gameplay systems
- `engine-programmer` 负责 core engine work
- `ai-programmer` 负责 AI behavior
- `network-programmer` 负责 multiplayer
- `ui-programmer` 负责 UI code
- `tools-programmer` 负责 dev tools

所有 agents 都遵循 collaborative protocol：读取 design doc，提出 clarifying questions，展示 architectural options，获得你的 approval，然后 implement。

**3. Story Completion:** story 完成后：

```
/story-done production/epics/combat/story-combat-damage-calc.md
```

它会运行 8-phase completion review：
1. 找到并读取 story file
2. 加载 referenced GDD、ADRs 和 control manifest
3. 验证 acceptance criteria（auto-checkable、manual、deferred）
4. 检查 GDD/ADR deviations（BLOCKING / ADVISORY / OUT OF SCOPE）
5. 提示进行 code review
6. 生成 completion report（COMPLETE / COMPLETE WITH NOTES / BLOCKED）
7. 将 story `Status: Complete` 与 completion notes 一起更新
8. 展示下一个 ready story

review 中发现的 tech debt 会记录到 `docs/tech-debt-register.md`。

### Step 5.2: Sprint Tracking

随时检查进度：

```
/sprint-status
```

它会从 `production/sprint-status.yaml` 读取并输出 30 行 quick snapshot。

如果 scope 正在增长：

```
/scope-check production/sprints/sprint-03.md
```

它会将当前 scope 与 original plan 比较，标记 scope increase，并建议 cut。

### Step 5.3: Content Tracking

```
/content-audit
```

将 GDD-specified content 与已实现内容对比，尽早发现 content gaps。

### Step 5.4: Design Change Propagation

当 GDD 在 stories 创建后发生变化：

```
/propagate-design-change design/gdd/combat-system.md
```

它会 git-diff 该 GDD，找出受影响 ADRs，生成 impact report，并引导你做 Superseded/update/keep decisions。

### Step 5.5: Multi-System Features（Team Orchestration）

对于跨多个 domains 的 features，使用 team skills：

```
/team-combat "healing ability with HoT and cleanse"
/team-narrative "Act 2 story content"
/team-ui "inventory screen redesign"
/team-level "forest dungeon level"
/team-audio "combat audio pass"
```

每个 team skill 会协调一个 6-phase collaborative workflow：
1. **Design** -- game-designer 提问并展示 options
2. **Architecture** -- lead-programmer 提出 code structure
3. **Parallel Implementation** -- specialists 同时工作
4. **Integration** -- gameplay-programmer 将所有内容接线整合
5. **Validation** -- qa-tester 按 acceptance criteria 运行验证
6. **Report** -- coordinator 总结 status

orchestration 是自动化的，但**决策点仍由你掌控**。

### Step 5.6: Sprint Review 和 Next Sprint

sprint 结束时：

```
/retrospective
```

分析 planned vs. completed、velocity、blockers 和 actionable improvements。

然后规划下一个 sprint：

```
/sprint-plan new
```

### Step 5.7: Milestone Reviews

在 milestone checkpoints：

```
/milestone-review "alpha"
```

生成 feature completeness、quality metrics、risk assessment 和 go/no-go recommendation。

### Phase 5 Gate

```
/gate-check production
```

**通过要求：**

- 所有 MVP stories complete
- Playtesting：3 个 sessions，覆盖 new player、mid-game 和 difficulty curve
- Fun hypothesis validated
- playtest data 中没有 confusion loops

---

## Phase 6: Polish

### 本 Phase 会发生什么

你的游戏已经 feature-complete。现在要把它做好。这个 phase 聚焦 performance、balance、accessibility、audio、visual polish 和 playtesting。

### Phase 6 Pipeline

```
/perf-profile  -->  /balance-check  -->  /asset-audit  -->  /playtest-report (x3)
       |                  |                    |                    |
       v                  v                    v                    v
  Profile CPU/GPU    Analyze formulas     Verify naming,      Cover: new player,
  memory, optimize   and data for         formats, sizes      mid-game, difficulty
  bottlenecks        broken progressions                      curve

  /tech-debt  -->  /team-polish
       |                |
       v                v
  Track and        Coordinated pass:
  prioritize       performance + art +
  debt items       audio + UX + QA
```

### Step 6.1: Performance Profiling

```
/perf-profile
```

引导你完成结构化 performance profiling：
- 建立 targets（FPS, memory, platform）
- 按 impact 排序识别 bottlenecks
- 生成 actionable optimization tasks，包含 code locations 和 expected gains

### Step 6.2: Balance Analysis

```
/balance-check assets/data/combat_damage.json
```

分析 balance data 中的 statistical outliers、broken progression curves、degenerate strategies 和 economy imbalances。

### Step 6.3: Asset Audit

```
/asset-audit
```

跨所有 assets 验证 naming conventions、file format standards 和 size budgets。

### Step 6.4: Playtesting（要求 3 Sessions）

```
/playtest-report
```

生成结构化 playtest reports。需要 3 个 sessions，覆盖：
- New player experience
- Mid-game systems
- Difficulty curve

### Step 6.5: Technical Debt Assessment

```
/tech-debt
```

扫描 TODO/FIXME/HACK comments、code duplication、过度复杂的 functions、missing tests 和 outdated dependencies。每个 item 都会分类并排序优先级。

### Step 6.6: Coordinated Polish Pass

```
/team-polish "combat system"
```

并行协调 4 个 specialists：
1. Performance optimization（performance-analyst）
2. Visual polish（technical-artist）
3. Audio polish（sound-designer）
4. Feel/juice（gameplay-programmer + technical-artist）

你设定 priorities；team 在每一步获得你的 approval 后执行。

### Step 6.7: Localization 和 Accessibility

```
/localize src/
```

扫描 hardcoded strings、会破坏 translation 的 concatenation、未考虑 expansion 的 text，以及 missing locale files。

Accessibility 会按 Phase 3 accessibility requirements document 中承诺的 tier 进行 audit。

### Phase 6 Gate

```
/gate-check polish
```

**通过要求：**

- 至少 3 个 playtest reports 存在
- Coordinated polish pass 已完成（`/team-polish`）
- 没有 blocking performance issues
- Accessibility tier requirements 已满足

---

## Phase 7: Release

### 本 Phase 会发生什么

你的游戏已经 polished、tested 并 ready。现在发布它。

### Phase 7 Pipeline

```
/release-checklist  -->  /launch-checklist  -->  /team-release
        |                       |                      |
        v                       v                      v
  Pre-release             Full cross-department    Coordinate:
  validation across       validation (Go/No-Go     build, QA sign-off,
  code, content,          per department)           deployment, launch
  store, legal
                    Also: /changelog, /patch-notes, /hotfix
```

### Step 7.1: Release Checklist

```
/release-checklist v1.0.0
```

生成全面的 pre-release checklist，覆盖：
- Build verification（所有 platforms 都能 compile 并 run）
- Certification requirements（platform-specific）
- Store metadata（descriptions, screenshots, trailers）
- Legal compliance（EULA, privacy policy, ratings）
- Save game compatibility
- Analytics verification

### Step 7.2: Launch Readiness（Full Validation）

```
/launch-checklist
```

完整 cross-department validation：

| Department | 检查内容 |
|-----------|----------|
| **Engineering** | Build stability, crash rates, memory leaks, load times |
| **Design** | Feature completeness, tutorial flow, difficulty curve |
| **Art** | Asset quality, missing textures, LOD levels |
| **Audio** | Missing sounds, mixing levels, spatial audio |
| **QA** | Open bug count by severity, regression suite pass rate |
| **Narrative** | Dialogue completeness, lore consistency, typos |
| **Localization** | All strings translated, no truncation, locale testing |
| **Accessibility** | Compliance checklist, assistive feature testing |
| **Store** | Metadata complete, screenshots approved, pricing set |
| **Marketing** | Press kit ready, launch trailer, social media scheduled |
| **Community** | Patch notes draft, FAQ prepared, support channels ready |
| **Infrastructure** | Servers scaled, CDN configured, monitoring active |
| **Legal** | EULA finalized, privacy policy, COPPA/GDPR compliance |

每个 item 都会得到 **Go / No-Go** status。全部为 Go 才能发布。

### Step 7.3: 生成 Player-Facing Content

```
/patch-notes v1.0.0
```

从 git history 和 sprint data 生成 player-friendly patch notes，将 developer language 转换为 player language。

```
/changelog v1.0.0
```

生成 internal changelog（更 technical，供团队使用）。

### Step 7.4: Coordinate the Release

```
/team-release
```

协调 release-manager、QA 和 DevOps 完成：
1. Pre-release validation
2. Build management
3. Final QA sign-off
4. Deployment preparation
5. Go/No-Go decision

### Step 7.5: Ship

当 push 到 `main` 或 `develop` 时，`validate-push` hook 会 warn。这是刻意设计的，release pushes 应该是慎重行为：

```bash
git tag v1.0.0
git push origin main --tags
```

### Step 7.6: Post-Launch

critical production bugs 使用 **Hotfix workflow**：

```
/hotfix "Players losing save data when inventory exceeds 99 items"
```

它会跳过正常 sprint processes，但保留完整 audit trail：
1. 创建 hotfix branch
2. 实现 fix
3. 确保 backport 到 development branch
4. 记录 incident

launch 稳定后做 **Post-mortem**：

```
Ask Claude to create a post-mortem using the template at
.claude/docs/templates/post-mortem.md
```

---

## Cross-Cutting Concerns

这些 topics 适用于所有 phases。

### Director Review Modes

Director gates 是 specialist agents，会在关键 workflow steps review 你的工作。默认会在每个 checkpoint 运行。你可以控制 review 强度。

**在 `/start` 期间设置一次 review intensity。** 保存到 `production/review-mode.txt`。

| Mode | 运行内容 | 最适合 |
|------|----------|--------|
| `full` | 每一步都运行所有 director gates | New projects，学习系统 |
| `lean` | 只在 phase transitions（`/gate-check`）运行 directors | Experienced devs |
| `solo` | 不运行 director reviews | Game jams、prototypes、maximum speed |

**单次运行 override**，不改变 global setting：

```
/brainstorm space horror --review full
/architecture-decision --review solo
```

`--review` flag 适用于所有使用 gate 的 skills。可以随时直接编辑 `production/review-mode.txt`，或重新运行 `/start` 来改变 global mode。

完整 gate definitions 和 check pattern：`.claude/docs/director-gates.md`

---

### Collaboration Protocol

这个 system 是**用户驱动的协作式**，不是 autonomous。

**Pattern:** Question > Options > Decision > Draft > Approval

每次 agent interaction 都遵循：
1. Agent 提出 clarifying questions
2. Agent 展示 2-4 个 options，附 trade-offs 和 reasoning
3. 你做 decision
4. Agent 基于你的 decision 起草
5. 你 review 并 refine
6. 写入前，Agent 询问 "May I write this to [filepath]?"

完整 protocol 和 examples 见 `docs/COLLABORATIVE-DESIGN-PRINCIPLE.md`。

### AskUserQuestion Tool

Agents 使用 `AskUserQuestion` tool 进行结构化 option presentation。pattern 是 Explain then Capture：先在 conversation text 中给出完整 analysis，再用干净的 UI picker 收集 decision。用于 design choices、architecture decisions 和 strategic questions。不要用于 open-ended discovery questions 或简单 yes/no confirmations。

### Agent Coordination（3-Tier Hierarchy）

```
Tier 1 (Directors):    creative-director, technical-director, producer
                                          |
Tier 2 (Leads):        game-designer, lead-programmer, art-director,
                       audio-director, narrative-director, qa-lead,
                       release-manager, localization-lead
                                          |
Tier 3 (Specialists):  gameplay-programmer, engine-programmer,
                       ai-programmer, network-programmer, ui-programmer,
                       tools-programmer, systems-designer, level-designer,
                       economy-designer, world-builder, writer,
                       technical-artist, sound-designer, ux-designer,
                       qa-tester, performance-analyst, devops-engineer,
                       analytics-engineer, accessibility-specialist,
                       live-ops-designer, prototyper, security-engineer,
                       community-manager, godot-specialist,
                       godot-gdscript-specialist, godot-shader-specialist,
                       godot-csharp-specialist, godot-gdextension-specialist,
                       unity-specialist, unity-dots-specialist,
                       unity-shader-specialist, unity-addressables-specialist,
                       unity-ui-specialist, unreal-specialist,
                       ue-blueprint-specialist, ue-gas-specialist,
                       ue-replication-specialist, ue-umg-specialist
```

**Coordination rules:**
- Vertical delegation：Directors > Leads > Specialists。复杂 decisions 不要跳过层级。
- Horizontal consultation：同 tier agents 可以互相 consult，但不得在自己 domain 之外做 binding decisions。
- Conflict resolution：Design conflicts 交给 `creative-director`。Technical conflicts 交给 `technical-director`。Scope conflicts 交给 `producer`。
- 不允许 unilateral cross-domain changes。

### Automated Hooks（Safety Net）

system 有 12 个自动运行的 hooks：

| Hook | Trigger | 作用 |
|------|---------|------|
| `session-start.sh` | Session start | 显示 branch、recent commits，并检测 active.md 以便 recovery |
| `detect-gaps.sh` | Session start | 检测 fresh projects（no engine, no concept）并建议 `/start` |
| `pre-compact.sh` | Before compaction | 将 session state dump 到 conversation，用于 auto-recovery |
| `post-compact.sh` | After compaction | 提醒 Claude 从 `active.md` restore session state |
| `notify.sh` | Notification event | 通过 PowerShell 显示 Windows toast notification |
| `validate-commit.sh` | Before commit | 检查 design doc references、valid JSON、no hardcoded values |
| `validate-push.sh` | Before push | 对 push 到 main/develop 发出 warning |
| `validate-assets.sh` | Before commit | 检查 asset naming 和 size |
| `validate-skill-change.sh` | Skill file written | `.claude/skills/` 变更后建议运行 `/skill-test` |
| `log-agent.sh` | Agent start | 记录 agent invocations，用于 audit trail |
| `log-agent-stop.sh` | Agent stop | 完成 agent audit trail（start + stop） |
| `session-stop.sh` | Session end | 最终 session logging |

### Context Resilience

**Session state file:** `production/session-state/active.md` 是 living checkpoint。每个重要 milestone 后更新它。任何 disruption（compaction、crash、`/clear`）后，先读取这个文件。

**Incremental writing:** 创建 multi-section documents 时，每个 section 在 approval 后立即写入文件。这意味着已完成 sections 能在 crashes 和 context compactions 后保留。关于已写入 sections 的先前讨论可以安全 compact。

**Automatic recovery:** `session-start.sh` hook 会自动检测并 preview `active.md`。`pre-compact.sh` hook 会在 compaction 前将 state dump 到 conversation。

**Sprint status tracking:** `production/sprint-status.yaml` 是 machine-readable story tracker。由 `/sprint-plan`（init）和 `/story-done`（status updates）写入。由 `/sprint-status`、`/help` 和 `/story-done`（next story）读取。它消除了脆弱的 markdown scanning。

### Brownfield Adoption

对于已经有一些 artifacts 的 existing projects：

```
/adopt
```

或 targeted：

```
/adopt gdds
/adopt adrs
/adopt stories
/adopt infra
```

它会审计 existing artifacts 的 **format**（不是 existence），将 gaps 分类为 BLOCKING/HIGH/MEDIUM/LOW，构建有序 migration plan，并写入 `docs/adoption-plan-[date].md`。核心原则：MIGRATION not REPLACEMENT；它永远不会重新生成现有工作，只填补 gaps。

各个 skills 也支持 retrofit mode：

```
/design-system retrofit design/gdd/combat-system.md
/architecture-decision retrofit docs/architecture/adr-005.md
```

它们会检测哪些 sections 已存在、哪些缺失，并只填补 gaps。

### Gate System

Phase gates 是正式 checkpoints。使用 transition name 运行 `/gate-check`：

```
/gate-check concept              # Concept -> Systems Design
/gate-check systems-design       # Systems Design -> Technical Setup
/gate-check technical-setup      # Technical Setup -> Pre-Production
/gate-check pre-production       # Pre-Production -> Production
/gate-check production           # Production -> Polish
/gate-check polish               # Polish -> Release
```

**Verdicts:**
- **PASS** -- 所有 requirements met，进入下一 phase
- **CONCERNS** -- requirements met 但有 acknowledged risks，可以通过
- **FAIL** -- requirements not met，会用具体 remediation 阻止进入下一阶段

gate 通过时，`production/stage.txt` 才会更新，这会控制 status line 和 `/help` behavior。

### Reverse Documentation

对于没有 design docs 的现有 code（brownfield adoption 后常见）：

```
/reverse-document src/gameplay/combat/
```

读取 existing code，并从中生成 GDD-format design documentation。

---

## Appendix A: Agent Quick-Reference

### “我需要做 X，应该用哪个 agent？”

| 我需要... | Agent | Tier |
|-----------|-------|------|
| 想出一个 game idea | `/brainstorm` skill | -- |
| 设计 game mechanic | `game-designer` | 2 |
| 设计具体 formulas/numbers | `systems-designer` | 3 |
| 设计 game level | `level-designer` | 3 |
| 设计 loot tables / economy | `economy-designer` | 3 |
| 构建 world lore | `world-builder` | 3 |
| 编写 dialogue | `writer` | 3 |
| 规划 story | `narrative-director` | 2 |
| 规划 sprint | `producer` | 1 |
| 做 creative decision | `creative-director` | 1 |
| 做 technical decision | `technical-director` | 1 |
| 实现 gameplay code | `gameplay-programmer` | 3 |
| 实现 core engine systems | `engine-programmer` | 3 |
| 实现 AI behavior | `ai-programmer` | 3 |
| 实现 multiplayer | `network-programmer` | 3 |
| 实现 UI | `ui-programmer` | 3 |
| 构建 dev tools | `tools-programmer` | 3 |
| Review code architecture | `lead-programmer` | 2 |
| 创建 shaders / VFX | `technical-artist` | 3 |
| 定义 visual style | `art-director` | 2 |
| 定义 audio style | `audio-director` | 2 |
| 设计 sound effects | `sound-designer` | 3 |
| 设计 UX flows | `ux-designer` | 3 |
| 编写 test cases | `qa-tester` | 3 |
| 规划 test strategy | `qa-lead` | 2 |
| Profile performance | `performance-analyst` | 3 |
| 设置 CI/CD | `devops-engineer` | 3 |
| 设计 analytics | `analytics-engineer` | 3 |
| 检查 accessibility | `accessibility-specialist` | 3 |
| 规划 live operations | `live-ops-designer` | 3 |
| 管理 release | `release-manager` | 2 |
| 管理 localization | `localization-lead` | 2 |
| 快速 prototype | `prototyper` | 3 |
| Audit security | `security-engineer` | 3 |
| 与 players 沟通 | `community-manager` | 3 |
| Godot-specific help | `godot-specialist` | 3 |
| GDScript-specific help | `godot-gdscript-specialist` | 3 |
| Godot shader help | `godot-shader-specialist` | 3 |
| GDExtension modules | `godot-gdextension-specialist` | 3 |
| Unity-specific help | `unity-specialist` | 3 |
| Unity DOTS/ECS | `unity-dots-specialist` | 3 |
| Unity shaders/VFX | `unity-shader-specialist` | 3 |
| Unity Addressables | `unity-addressables-specialist` | 3 |
| Unity UI Toolkit | `unity-ui-specialist` | 3 |
| Unreal-specific help | `unreal-specialist` | 3 |
| Unreal GAS | `ue-gas-specialist` | 3 |
| Unreal Blueprints | `ue-blueprint-specialist` | 3 |
| Unreal replication | `ue-replication-specialist` | 3 |
| Unreal UMG/CommonUI | `ue-umg-specialist` | 3 |

### Agent Hierarchy

```
                    creative-director / technical-director / producer
                                         |
          ---------------------------------------------------------------
          |            |           |           |          |        |       |
    game-designer  lead-prog  art-dir  audio-dir  narr-dir  qa-lead  release-mgr
          |            |           |           |          |        |        |
     specialists  programmers  tech-art  snd-design  writer   qa-tester  devops
     (systems,    (gameplay,             (sound)     (world-  (perf,     (analytics,
      economy,     engine,                           builder)  access.)   security)
      level)       ai, net,
                   ui, tools)
```

**Escalation rule:** 如果两个 agents 意见不一致，向上升级。Design conflicts 交给 `creative-director`。Technical conflicts 交给 `technical-director`。Scope conflicts 交给 `producer`。

---

## Appendix B: Slash Command Quick-Reference

### 按 Category 列出的全部 73 个 Commands

#### Onboarding and Navigation（6）

| Command | Purpose | Phase |
|---------|---------|-------|
| `/start` | Guided onboarding，路由到正确 workflow | Any（first session） |
| `/help` | Context-aware “下一步做什么？” | Any |
| `/project-stage-detect` | 完整 project audit，用于确定 current phase | Any |
| `/setup-engine` | 配置 engine、pin version、设置 preferences | 1 |
| `/adopt` | Brownfield audit 和 migration plan | Any（existing projects） |
| `/skill-improve` | 通过 test-fix-retest loop 改进 skill | Any |

#### Game Design（6）

| Command | Purpose | Phase |
|---------|---------|-------|
| `/brainstorm` | 带 MDA analysis 的 collaborative ideation | 1 |
| `/map-systems` | 将 concept 拆解为 systems index | 1-2 |
| `/design-system` | Guided section-by-section GDD authoring | 2 |
| `/quick-design` | 小改动的 lightweight spec | 2+ |
| `/review-all-gdds` | Cross-GDD consistency 和 design theory review | 2 |
| `/propagate-design-change` | 查找受 GDD changes 影响的 ADRs/stories | 5 |

#### UX and Interface（2）

| Command | Purpose | Phase |
|---------|---------|-------|
| `/ux-design` | 编写 UX specs（screen/flow, HUD, patterns） | 4 |
| `/ux-review` | 验证 UX specs 的 accessibility 和 GDD alignment | 4 |

#### Architecture（4）

| Command | Purpose | Phase |
|---------|---------|-------|
| `/create-architecture` | Master architecture document | 3 |
| `/architecture-decision` | 创建或 retrofit 一个 ADR | 3 |
| `/architecture-review` | 验证所有 ADRs 和 dependency ordering | 3 |
| `/create-control-manifest` | 从 Accepted ADRs 生成 flat programmer rules | 3 |

#### Stories and Sprints（8）

| Command | Purpose | Phase |
|---------|---------|-------|
| `/create-epics` | 将 GDDs + ADRs 转换为 epics（每个 module 一个） | 4 |
| `/create-stories` | 将单个 epic 拆成 story files | 4 |
| `/dev-story` | 实现 story，并路由到正确 programmer agent | 5 |
| `/sprint-plan` | 创建或管理 sprint plans | 4-5 |
| `/sprint-status` | 30 行 quick sprint snapshot | 5 |
| `/story-readiness` | 验证 story 已可实现 | 4-5 |
| `/story-done` | 8-phase story completion review | 5 |
| `/estimate` | 带 risk assessment 的 effort estimation | 4-5 |

#### Reviews and Analysis（13）

| Command | Purpose | Phase |
|---------|---------|-------|
| `/design-review` | 按 8-section standard 验证 GDD | 1-2 |
| `/code-review` | Architectural code review | 5+ |
| `/balance-check` | Game balance formula analysis | 5-6 |
| `/asset-audit` | Asset naming、format、size verification | 6 |
| `/asset-spec` | Per-asset visual specs 和 AI generation prompts | 5-6 |
| `/content-audit` | GDD-specified content vs. implemented | 5 |
| `/consistency-check` | Cross-GDD entity 和 formula inconsistency scan | 2+ |
| `/scope-check` | Scope creep detection | 5 |
| `/perf-profile` | Performance profiling workflow | 6 |
| `/tech-debt` | Tech debt scanning 和 prioritization | 6 |
| `/gate-check` | 带 PASS/CONCERNS/FAIL 的 formal phase gate | All transitions |
| `/reverse-document` | 从 existing code 生成 design docs | Any |
| `/security-audit` | Security vulnerability audit（save, network, input） | 6-7 |

#### QA and Testing（9）

| Command | Purpose | Phase |
|---------|---------|-------|
| `/qa-plan` | 为 sprint 或 feature 生成 QA test plan | 5 |
| `/smoke-check` | QA hand-off 前的 critical path smoke test gate | 5-6 |
| `/soak-test` | Extended play sessions 的 soak test protocol | 6 |
| `/regression-suite` | 映射 test coverage，找出缺少 regression tests 的 fixed bugs | 5-6 |
| `/test-setup` | Scaffold test framework 和 CI/CD pipeline | 4 |
| `/test-helpers` | 生成 engine-specific test helper libraries | 4-5 |
| `/test-evidence-review` | Test files 和 manual evidence 的 quality review | 5 |
| `/test-flakiness` | 从 CI logs 检测 non-deterministic tests | 5-6 |
| `/skill-test` | 验证 skill files 的 structural 和 behavioral correctness | Any |

#### Production Management（6）

| Command | Purpose | Phase |
|---------|---------|-------|
| `/milestone-review` | Milestone progress 和 go/no-go | 5 |
| `/retrospective` | Sprint retrospective analysis | 5 |
| `/bug-report` | 创建 structured bug report | 5+ |
| `/bug-triage` | 重新评估 open bugs 的 priority、severity 和 owner | 5+ |
| `/playtest-report` | Structured playtest session report | 4-6 |
| `/onboard` | Onboard 新 team member | Any |

#### Release（6）

| Command | Purpose | Phase |
|---------|---------|-------|
| `/release-checklist` | Pre-release validation | 7 |
| `/launch-checklist` | 完整 cross-department launch readiness | 7 |
| `/changelog` | Auto-generate internal changelog | 7 |
| `/patch-notes` | Player-facing patch notes | 7 |
| `/hotfix` | Emergency fix workflow | 7+ |
| `/day-one-patch` | 针对 gold master 后发现问题的 scoped patch | 7+ |

#### Creative（4）

| Command | Purpose | Phase |
|---------|---------|-------|
| `/prototype` | Concept prototype，GDDs 前验证 core idea | 1 |
| `/art-bible` | Guided Art Bible authoring，visual identity spec | 1-2 |
| `/vertical-slice` | Production 前的 production-quality end-to-end build | 4 |
| `/localize` | String extraction 和 validation | 6-7 |

#### Team Orchestration（9）

| Command | Purpose | Phase |
|---------|---------|-------|
| `/team-combat` | Combat feature：从 design 到 implementation | 5 |
| `/team-narrative` | Narrative content：从 structure 到 dialogue | 5 |
| `/team-ui` | UI feature：从 UX spec 到 polished implementation | 5 |
| `/team-level` | Level：从 layout 到 dressed encounters | 5 |
| `/team-audio` | Audio：从 direction 到 implemented events | 5-6 |
| `/team-polish` | Coordinated polish：perf + art + audio + QA | 6 |
| `/team-release` | Release coordination：build + QA + deployment | 7 |
| `/team-live-ops` | Live-ops planning：seasonal events、battle pass、retention | 7+ |
| `/team-qa` | Full QA cycle：strategy、execution、coverage、sign-off | 6-7 |

---

## Appendix C: Common Workflows

### Workflow 1: “刚开始，没有 game idea”

```
1. /start（根据你当前状态路由）
2. /brainstorm（collaborative ideation，选择一个 concept）
3. /setup-engine（pin engine 和 version）
4. 对 concept doc 运行 /design-review（可选，推荐）
5. /map-systems（将 concept 拆解为带 deps 和 priorities 的 systems）
6. /gate-check concept（验证你已准备进入 Systems Design）
7. 对每个 system 运行 /design-system（guided GDD authoring）
```

### Workflow 2: “已有 designs，想开始 coding”

```
1. 对每个 GDD 运行 /design-review（确认足够稳固）
2. /review-all-gdds（Cross-GDD consistency）
3. /gate-check systems-design
4. /create-architecture + /architecture-decision（每个 major decision 一次）
5. /architecture-review
6. /create-control-manifest
7. /gate-check technical-setup
8. /create-epics layer: foundation + /create-stories [slug]（定义 epics，拆成 stories）
9. /sprint-plan new
10. /story-readiness -> implement -> /story-done（story lifecycle）
```

### Workflow 3: “Production 中途需要添加复杂 feature”

```
1. /design-system 或 /quick-design（取决于 scope）
2. /design-review 进行验证
3. 如果修改 existing GDDs，运行 /propagate-design-change
4. /estimate 评估 effort 和 risk
5. /team-combat、/team-narrative、/team-ui 等（合适的 team skill）
6. 完成后运行 /story-done
7. 如果影响 game balance，运行 /balance-check
```

### Workflow 4: “Production 中出问题了”

```
1. /hotfix "description of the issue"
2. 在 hotfix branch 上实现 fix
3. 对 fix 运行 /code-review
4. 运行 tests
5. 对 hotfix build 运行 /release-checklist
6. Deploy 并 backport
```

### Workflow 5: “已有项目，想使用这个 system”

```
1. /start（选择 Path D -- existing work）
2. /project-stage-detect（确定 current phase）
3. /adopt（审计 existing artifacts，构建 migration plan）
4. /design-system retrofit [path]（填补 GDD gaps）
5. /architecture-decision retrofit [path]（填补 ADR gaps）
6. 在合适 transition 运行 /gate-check
```

### Workflow 6: “开始新的 sprint”

```
1. /retrospective（review last sprint）
2. /sprint-plan new（创建 next sprint）
3. /scope-check（确保 scope 可控）
4. pickup 前对每个 story 运行 /story-readiness
5. Implement stories
6. 每个完成的 story 运行 /story-done
7. 用 /sprint-status 快速检查进度
```

### Workflow 7: “Shipping the game”

```
1. /gate-check polish（验证 Polish phase 已完成）
2. /tech-debt（决定 launch 时可接受的内容）
3. /localize（final localization pass）
4. /release-checklist v1.0.0
5. /launch-checklist（full cross-department validation）
6. /team-release（coordinate the release）
7. /patch-notes 和 /changelog
8. Ship!
9. 如果 post-launch 出现问题，运行 /hotfix
10. launch 稳定后做 Post-mortem
```

### Workflow 8: “迷路了，不知道下一步做什么”

```
1. /help（读取你的 phase，检查 artifacts，告诉你下一步）
2. 如果 /help 没帮上忙：/project-stage-detect（full audit）
3. 如果 stage 看起来不对：在你认为所处的 transition 运行 /gate-check
```

---

## Tips for Getting the Most Out of the System

1. **始终先 design，再 implement。** agent system 的前提是写 code 前已经存在 design document。Agents 会持续 reference GDDs。

2. **为 cross-cutting features 使用 team skills。** 不要试图自己手动协调 4 个 agents；让 `/team-combat`、`/team-narrative` 等处理 orchestration。

3. **信任 rules system。** 当 rule 标记你的 code 中有问题时，修复它。rules 编码了来之不易的 game development wisdom（data-driven values、delta time、accessibility 等）。

4. **主动 compact。** 在约 65-70% context usage 时，compact 或 `/clear`。pre-compact hook 会保存你的进度。不要等到触及 limit。

5. **使用正确 tier 的 agent。** 不要让 `creative-director` 写 shader。不要让 `qa-tester` 做 design decisions。hierarchy 的存在有其原因。

6. **不确定时运行 /help。** 它会读取真实 project state，并告诉你唯一最重要的 next step。

7. **把 designs 交给 programmers 前运行 `/design-review`。** 这会尽早发现 incomplete specs，减少返工。

8. **每个 major feature 后运行 `/code-review`。** 在 architectural issues 扩散前抓住它们。

9. **先 prototype 风险 mechanics。** 对不成立的 mechanic 来说，一天 prototype 可以省下一周 production。

10. **保持 sprint plans 诚实。** 定期使用 `/scope-check`。Scope creep 是 indie games 的头号杀手。

11. **用 ADRs 记录 decisions。** 未来的你会感谢现在的你记录了事情为什么以这种方式构建。

12. **严格使用 story lifecycle。** pickup 前 `/story-readiness`，完成后 `/story-done`。这能尽早发现 deviations，并保持 pipeline 可靠。

13. **尽早且经常写入文件。** incremental section writing 意味着 design decisions 能在 crashes 和 compactions 后保留。文件才是记忆，不是 conversation。

# Repository Guidelines

## Codex Runtime Rules

本仓库面向 Claude Code Game Studios，但 Codex 在本仓库工作时应遵循同一套协作约束。所有面向用户的自然语言输出使用简体中文。保留技术标识符英文原样，包括文件路径、命令（如 `/start`、`/brainstorm`）、Gate ID（如 `CD-PILLARS`、`TD-FEASIBILITY`）、agent/skill 名称、变量名、函数名、类名、URL 与代码块内容。

## Project Structure

核心配置在 `CLAUDE.md`。Claude 相关能力位于 `.claude/`：`agents/` 存放 agent 定义，`skills/<name>/SKILL.md` 存放 slash command 技能，`hooks/` 存放自动化脚本，`rules/` 存放路径级规则，`docs/` 存放协作与标准文档。游戏源代码放在 `src/`，资源放在 `assets/`，设计文档放在 `design/`，技术文档放在 `docs/`，测试放在 `tests/`，生产管理资料放在 `production/`，工具脚本放在 `tools/`。

## Engine & Technical Context

引擎、语言、构建系统和资源管线在初始状态下未配置。修改实现前先检查 `.claude/docs/technical-preferences.md`；若仍为 `[TO BE CONFIGURED]`，不要假设引擎或语言。Godot、Unity、Unreal 相关工作应匹配对应 specialist 与版本参考，例如 `docs/engine-reference/godot/VERSION.md`。

## Collaboration Protocol

本仓库要求用户驱动协作，而非自主执行。复杂或会写文件的任务遵循：Question -> Options -> Decision -> Draft -> Approval -> Write。写入文件前说明将修改的路径和目的；多文件变更先给出变更集摘要。不要创建 commit，除非用户明确要求。不要跨 agent 或目录职责修改文件，除非用户明确授权。

## Coding Standards

游戏代码的 public API 需要 doc comments。系统级实现应对应 `docs/architecture/` 下的 ADR。玩法数值必须数据驱动，避免硬编码。public methods 应可单元测试，优先依赖注入而非 singleton。UI 变更需要截图验证；玩法逻辑优先写测试验证。

## Skills, Agents, and Hooks

新增 skill 必须使用 `.claude/skills/<name>/SKILL.md` 结构，并包含 YAML frontmatter：`name`、`description`、`argument-hint`、`allowed-tools`、`model`。只读检查用 `haiku`，多文档综合或阶段 gate 用 `opus`，其他默认 `sonnet`。新增 agent 必须包含 Collaboration Protocol。Hook 脚本必须跨平台，使用 `grep -E`，不要使用 `grep -P`，缺少 `jq` 或 Python 时应快速 `exit 0`。

## Testing & Verification

测试规则参考 `.claude/docs/coding-standards.md`。测试文件命名使用 `[system]_[feature]_test.[ext]`，测试函数命名使用 `test_[scenario]_[expected]`。单元测试必须确定、隔离、无外部 API/数据库/文件 I/O 依赖。改动 skill 时在 Claude Code 中调用对应 slash command 验证；改动 hook 时触发对应事件验证；改动 reference 内容时同步更新 `agent-roster`、`skills-reference`、`hooks-reference` 或 `rules-reference`。

## Commits and PRs

提交信息使用 Conventional Commits：`feat:`、`fix:`、`chore:`、`docs:`、`test:`、`refactor:`。正文中引用相关 story 或 task ID，例如 `Story: EPIC-001-S02`。PR 需要说明变更、类型、测试结果，并满足 `.github/PULL_REQUEST_TEMPLATE.md`。不要把使用本框架生成的游戏 GDD、项目概念、关卡设计或游戏资产提交到本框架仓库。

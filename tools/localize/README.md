# CCGS 中文本地化脚本

将 Claude Code Game Studios 仓库的英文技术文档（Agents、Skills、Rules、Hooks、Docs 等）自动翻译为中文。

## 特性

- **增量更新**：基于 SHA256 缓存，只翻译新增或修改的文件
- **智能分块**：长文件自动按 Markdown 标题分块，避免超出 API 输出限制
- **批量合并**：小文件合并到一次 API 调用，降低成本
- **内容保护**：自动保留代码块、文件路径、命令、Gate ID、URL 等技术标识符
- **YAML frontmatter 识别**：只翻译 `description` 字段，保留其他键值
- **成本预估**：翻译前先估算 token 消耗和费用

## 快速开始

### 1. 安装依赖

```bash
cd tools/localize
pip install -r requirements.txt
```

### 2. 配置 API Key

复制示例文件并填入你的 Anthropic API Key：

```bash
cp .env.example .env
# 编辑 .env，将 ANTHROPIC_API_KEY 替换为你的真实 Key
```

或者通过环境变量设置：

```bash
export ANTHROPIC_API_KEY=sk-xxx
```

### 3. 预估成本（推荐先执行）

```bash
python localize.py --estimate-cost
```

### 4. 执行翻译

```bash
# 只翻译变更的文件（首次运行会翻译全部）
python localize.py

# 强制重新翻译所有文件
python localize.py --force

# 只翻译指定目录
python localize.py --paths .claude/skills .claude/agents

# 使用更便宜的模型
python localize.py --model claude-haiku-4-5-20251001
```

## 命令行参数

| 参数 | 说明 |
|------|------|
| `--api-key KEY` | Anthropic API Key |
| `--model MODEL` | 使用的 Claude 模型，默认 `claude-sonnet-4-6` |
| `--paths PATH [PATH ...]` | 指定要扫描的路径，可覆盖默认值 |
| `--dry-run` | 只列出待翻译文件，不调用 API |
| `--force` | 强制重新翻译所有文件（忽略缓存） |
| `--estimate-cost` | 估算翻译成本 |
| `--no-batch` | 禁用批量合并，每个文件单独调用 API |
| `--repo-root PATH` | 手动指定仓库根目录 |

## 更新上游后重新本地化

当上游仓库更新后，按以下步骤同步并重新本地化：

```bash
# 1. 拉取上游更新
git fetch upstream
git merge upstream/main

# 2. 运行本地化脚本（只翻译变更/新增的文件）
cd tools/localize
python localize.py

# 3. 检查翻译结果，提交
cd ../..
git add -A
git commit -m "chore: localize updated content to Chinese"
```

## 成本参考（首次全量翻译）

以当前仓库规模（约 400 个文件，320 万字符）估算：

| 模型 | 预估费用 |
|------|---------|
| claude-haiku-4-5 | ~$2.8 USD |
| claude-sonnet-4-6 | ~$10.7 USD |
| claude-opus-4-7 | ~$53.5 USD |

**增量更新成本极低**：通常每次只涉及几个文件，费用可忽略不计。

## 工作原理

1. **扫描**：递归扫描仓库中的 `.md`、`.sh`、`.txt` 文件
2. **缓存对比**：计算每个文件的 SHA256，与 `.localization_cache.json` 对比
3. **内容保护**：用正则表达式提取并保护不需要翻译的内容（代码块、路径、命令等），替换为占位符
4. **API 翻译**：调用 Claude API 翻译，要求保留占位符
5. **还原**：将占位符还原为原始内容
6. **写回**：覆盖原文件（因为是 fork 的本地化仓库，直接覆盖符合预期）

## 注意事项

- 脚本会直接**覆盖**原文件。如果你需要保留英文版本，请先创建分支或备份。
- 建议在干净的工作区运行（`git status` 无未提交修改），以便区分脚本的修改和你自己的修改。
- 翻译质量取决于所选模型。Sonnet 性价比最高，Haiku 适合预算敏感场景，Opus 质量最好但最贵。
- `.claude/settings.json` 默认被排除，因为其中多为命令模式，无需翻译。

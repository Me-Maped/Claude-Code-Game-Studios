#!/usr/bin/env python3
"""
CCGS 中文本地化脚本 (Chinese Localization Script)

对 Claude Code Game Studios 仓库进行中文本地化翻译。
支持增量更新：通过 SHA256 缓存只翻译新增或修改的文件。
支持智能分块：长文件自动按标题分块，避免超出 API 输出限制。
支持批量合并：小文件合并到一次 API 调用，降低成本。

用法:
    cd tools/localize
    pip install -r requirements.txt
    # 设置 API Key (三种方式任选其一):
    #   1. export ANTHROPIC_API_KEY=sk-xxx
    #   2. 在项目根目录创建 .env 文件: ANTHROPIC_API_KEY=sk-xxx
    #   3. 命令行传入: --api-key sk-xxx

    # 预估翻译成本（不调用 API）:
    python localize.py --estimate-cost

    # 查看哪些文件需要翻译（不实际调用 API）:
    python localize.py --dry-run

    # 执行翻译（默认只处理变更文件）:
    python localize.py

    # 强制重新翻译所有文件:
    python localize.py --force

    # 只翻译指定目录（可用多次）:
    python localize.py --paths .claude/skills/brainstorm .claude/agents/creative-director.md

    # 使用指定模型（默认 claude-sonnet-4-6）:
    python localize.py --model claude-haiku-4-5-20251001
"""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import sys
import time
from dataclasses import dataclass, field
from pathlib import Path
from typing import Optional

# ---------------------------------------------------------------------------
# 依赖检测
# ---------------------------------------------------------------------------
try:
    from anthropic import Anthropic, APIError, RateLimitError
except ImportError:
    Anthropic = None  # type: ignore
    APIError = Exception  # type: ignore
    RateLimitError = Exception  # type: ignore

try:
    import yaml
except ImportError:
    yaml = None  # type: ignore

try:
    from dotenv import load_dotenv
except ImportError:
    def load_dotenv(*_, **__):
        pass  # type: ignore

# ---------------------------------------------------------------------------
# 常量与默认配置
# ---------------------------------------------------------------------------
DEFAULT_MODEL = "claude-sonnet-4-6"
MAX_RETRIES = 3
RETRY_DELAY = 5  # seconds
CACHE_FILE = ".localization_cache.json"

# 默认扫描路径（相对仓库根目录）
DEFAULT_PATHS = [
    "CLAUDE.md",
    "README.md",
    "CONTRIBUTING.md",
    "SECURITY.md",
    "UPGRADING.md",
    ".claude/agents",
    ".claude/skills",
    ".claude/rules",
    ".claude/hooks",
    ".claude/docs",
    "docs",
    "design",
    "production",
    "CCGS Skill Testing Framework",
]

# 需要排除的文件/目录模式
EXCLUDE_PATTERNS = [
    re.compile(r".*\.(png|jpg|jpeg|gif|ico|ttf|woff2?|mp3|wav|mp4|zip|exe|dll|so|dylib)$", re.I),
    re.compile(r"^\.git"),
    re.compile(r"node_modules"),
    re.compile(r"__pycache__"),
    re.compile(r"\.pyc$"),
    re.compile(r"\.claude/settings\.json$"),
]

# 分块阈值（按字符数估算）
# 英文约 4 字符/token，中文输出约 2.5 字符/token
# 单次输出上限 8192 tokens ≈ 20000 字符
CHUNK_SIZE_THRESHOLD = 15000  # 字符数
BATCH_MAX_CHARS = 12000       # 合并小文件时的字符上限

# ---------------------------------------------------------------------------
# System Prompt
# ---------------------------------------------------------------------------
SYSTEM_PROMPT = """你是一名专业的技术文档中译专家。你的任务是将游戏开发框架的技术文档从英文翻译为地道、专业的中文。

## 翻译原则
1. **保留所有技术标识符**：文件名、路径、命令、变量名、函数名、类名、Gate ID、配置键等必须原样保留，不得翻译。
2. **保留代码**：所有代码块、内联代码、命令行示例必须原样保留。
3. **保留 URL**：所有链接地址必须原样保留。
4. **保留英文术语**：在中文技术文档中已广泛使用的英文术语可保留英文，必要时在首次出现时加括号注明中文，例如：MVP（最小可行产品）、GDD（游戏设计文档）、ADR（架构决策记录）。
5. **专业准确**：使用游戏开发和软件工程领域的标准中文术语。
6. **语气风格**：保持原文的专业、清晰、指令性语气。将第二人称 "you" 翻译为 "你"。
7. **Markdown 格式**：严格保留所有 Markdown 语法（标题级别、表格、列表、加粗、斜体、引用块等）。
8. **占位符保护**：输入中形如 `__PROTECTED_N__` 的占位符必须原样保留，它们是代码、路径等受保护内容的位置标记。
9. **YAML frontmatter**：如果内容中包含 YAML frontmatter（`---` 包围的键值对），只翻译 `description` 字段的值，其他键和值必须原样保留。
10. **保留特定标记**：保留所有 verdict 标记（APPROVE, CONCERNS, REJECT, PASS, FAIL, PROCEED, PIVOT 等）以及像 COMPLETE, IMPROVED, NO CHANGE, REVERTED 这样的状态标记。

## 不翻译的示例
- `/brainstorm`, `/setup-engine`, `/design-system`
- `design/gdd/game-concept.md`, `src/gameplay/`
- `CD-PILLARS`, `AD-CONCEPT-VISUAL`, `TD-FEASIBILITY`
- `creative-director`, `lead-programmer`
- `claude-sonnet-4-6`, `claude-opus-4-7`
- `AskUserQuestion`, `Write`, `Edit`, `Read`, `Task`
- `TODO(name)`, `FIXME`
- `git commit`, `git push`
- `https://docs.godotengine.org/...`
- 代码块中的 `var damage: float = 10.0`

请直接输出翻译后的完整内容，不要添加任何解释、总结或 "以下是翻译" 之类的额外文字。"""

BATCH_SYSTEM_PROMPT = SYSTEM_PROMPT + """

## 批量翻译格式
本次输入包含多个文件，用 `===== FILE: relative/path =====` 分隔。
请对每个文件分别翻译，输出格式与输入完全一致：保留分隔符和文件路径，只翻译内容部分。
"""


# ---------------------------------------------------------------------------
# 数据模型
# ---------------------------------------------------------------------------
@dataclass
class TranslationUnit:
    """单个待翻译单元"""
    file_path: Path
    rel_path: str
    original_text: str
    sha256: str
    is_new: bool = True
    chunks: list[str] = field(default_factory=list)


@dataclass
class CacheEntry:
    sha256: str
    mtime: float
    translated_at: str


class LocalizationCache:
    def __init__(self, cache_path: Path):
        self.cache_path = cache_path
        self.entries: dict[str, CacheEntry] = {}
        self._load()

    def _load(self) -> None:
        if self.cache_path.exists():
            try:
                data = json.loads(self.cache_path.read_text(encoding="utf-8"))
                self.entries = {
                    k: CacheEntry(**v) for k, v in data.get("entries", {}).items()
                }
            except (json.JSONDecodeError, TypeError):
                self.entries = {}

    def save(self) -> None:
        data = {
            "entries": {
                k: {"sha256": v.sha256, "mtime": v.mtime, "translated_at": v.translated_at}
                for k, v in self.entries.items()
            }
        }
        self.cache_path.write_text(json.dumps(data, indent=2, ensure_ascii=False), encoding="utf-8")

    def needs_translation(self, rel_path: str, sha256: str) -> bool:
        entry = self.entries.get(rel_path)
        if entry is None:
            return True
        return entry.sha256 != sha256

    def update(self, rel_path: str, sha256: str, mtime: float) -> None:
        from datetime import datetime, timezone
        self.entries[rel_path] = CacheEntry(
            sha256=sha256,
            mtime=mtime,
            translated_at=datetime.now(timezone.utc).isoformat(),
        )


# ---------------------------------------------------------------------------
# Token / 字符估算
# ---------------------------------------------------------------------------
def estimate_tokens(text: str) -> int:
    """粗略估算 token 数（英文约 4 字符/token）。"""
    return max(1, len(text) // 4)


def estimate_cost(units: list[TranslationUnit], model: str) -> dict:
    """估算翻译总成本。"""
    total_chars = sum(len(u.original_text) for u in units)
    total_input_tokens = sum(estimate_tokens(u.original_text) for u in units)
    # 中文输出通常比英文输入短（中文字符更紧凑），假设比例为 0.7
    total_output_tokens = int(total_input_tokens * 0.7)

    # 定价 (USD per million tokens) — 2025-05 参考价
    pricing = {
        "claude-sonnet-4-6": {"input": 3.0, "output": 15.0},
        "claude-opus-4-7": {"input": 15.0, "output": 75.0},
        "claude-haiku-4-5-20251001": {"input": 0.8, "output": 4.0},
    }
    p = pricing.get(model, pricing["claude-sonnet-4-6"])

    input_cost = (total_input_tokens / 1_000_000) * p["input"]
    output_cost = (total_output_tokens / 1_000_000) * p["output"]

    return {
        "files": len(units),
        "total_chars": total_chars,
        "input_tokens": total_input_tokens,
        "output_tokens": total_output_tokens,
        "input_cost_usd": input_cost,
        "output_cost_usd": output_cost,
        "total_cost_usd": input_cost + output_cost,
        "model": model,
    }


# ---------------------------------------------------------------------------
# 保护器：提取并替换不需要翻译的内容
# ---------------------------------------------------------------------------
class ContentProtector:
    """将不需要翻译的内容替换为占位符，翻译后再还原。"""

    def __init__(self):
        self.protected: list[str] = []
        self._counter = 0

    def _add(self, text: str) -> str:
        placeholder = f"__PROTECTED_{self._counter}__"
        self.protected.append(text)
        self._counter += 1
        return placeholder

    def protect(self, text: str) -> str:
        """对文本进行保护处理，返回替换后的文本。"""
        self.protected = []
        self._counter = 0

        # 1. 保护 fenced code blocks (```...```)
        text = re.sub(
            r"```[\s\S]*?```",
            lambda m: self._add(m.group(0)),
            text,
        )

        # 2. 保护行内代码 (`...`)
        text = re.sub(r"`[^`\n]+`", lambda m: self._add(m.group(0)), text)

        # 3. 保护 URL
        text = re.sub(
            r"https?://[^\s\)\]\>\"\']+",
            lambda m: self._add(m.group(0)),
            text,
        )

        # 4. 保护文件路径
        # 匹配 common path patterns: ./foo/bar, foo/bar/baz.md, .claude/skills/...
        def maybe_protect_path(m: re.Match) -> str:
            s = m.group(0)
            if "/" in s or s.startswith("./") or s.startswith("../"):
                return self._add(s)
            return s

        text = re.sub(
            r"(?:\.{1,2}/)?(?:[\w\-]+/)+[\w\-]+(?:\.[\w\-]+)?(?:/|(?=\s|$|[,;:!?\"'\)]))?",
            maybe_protect_path,
            text,
        )

        # 5. 保护 Skill 命令 /word-word
        text = re.sub(
            r"(?<![\w/])/[a-z][a-z0-9\-]*(?:\s+[a-z0-9\-\[\]]+)?",
            lambda m: self._add(m.group(0)),
            text,
        )

        # 6. 保护 Gate ID (如 CD-PILLARS, TD-FEASIBILITY)
        text = re.sub(
            r"\b[A-Z]{2,}(?:-[A-Z]+){1,4}\b",
            lambda m: self._add(m.group(0)),
            text,
        )

        # 7. 保护反引号中的 agent/skill 名称
        text = re.sub(
            r"`[a-z][a-z0-9\-]*(?:-[a-z][a-z0-9\-]*)+`",
            lambda m: self._add(m.group(0)),
            text,
        )

        # 8. 保护环境变量
        text = re.sub(
            r"\$[A-Z_][A-Z0-9_]*",
            lambda m: self._add(m.group(0)),
            text,
        )

        return text

    def restore(self, text: str) -> str:
        """将占位符还原为原始内容。"""
        for idx, original in enumerate(self.protected):
            placeholder = f"__PROTECTED_{idx}__"
            text = text.replace(placeholder, original)
        return text


# ---------------------------------------------------------------------------
# 文件内容分块
# ---------------------------------------------------------------------------
def split_frontmatter(text: str) -> tuple[Optional[str], str]:
    """分离 YAML frontmatter 和正文。"""
    if not text.startswith("---"):
        return None, text

    match = re.search(r"\n---\s*(?:\n|$)", text[3:])
    if not match:
        return None, text

    end = 3 + match.end()
    frontmatter = text[3:match.start() + 3]
    body = text[end:]
    return frontmatter, body


def chunk_markdown(body: str, threshold: int = CHUNK_SIZE_THRESHOLD) -> list[str]:
    """按二级/三级标题将 markdown 正文分块。"""
    if len(body) <= threshold:
        return [body]

    # 按 ^## 或 ^### 分割，保留标题
    parts = re.split(r"(?=\n##\s)", body)
    if len(parts) <= 1:
        parts = re.split(r"(?=\n###\s)", body)

    chunks: list[str] = []
    current = ""
    for part in parts:
        if not part.strip():
            continue
        if len(current) + len(part) > threshold and current:
            chunks.append(current)
            current = part
        else:
            current += part
    if current.strip():
        chunks.append(current)

    # 如果某个 chunk 仍然过大，尝试按段落分割
    final_chunks: list[str] = []
    for chunk in chunks:
        if len(chunk) > threshold * 1.5:
            paragraphs = chunk.split("\n\n")
            current = ""
            for para in paragraphs:
                if len(current) + len(para) > threshold and current:
                    final_chunks.append(current)
                    current = para + "\n\n"
                else:
                    current += para + "\n\n"
            if current.strip():
                final_chunks.append(current)
        else:
            final_chunks.append(chunk)

    return final_chunks


# ---------------------------------------------------------------------------
# API 翻译
# ---------------------------------------------------------------------------
def _call_api(text: str, client: Anthropic, model: str, system: str) -> str:
    """底层 API 调用，带重试。"""
    for attempt in range(1, MAX_RETRIES + 1):
        try:
            response = client.messages.create(
                model=model,
                max_tokens=8192,
                temperature=0.1,
                system=system,
                messages=[
                    {
                        "role": "user",
                        "content": text,
                    }
                ],
            )
            return response.content[0].text  # type: ignore
        except RateLimitError:
            if attempt < MAX_RETRIES:
                print(f"    [RATE LIMIT] Waiting {RETRY_DELAY}s... ({attempt}/{MAX_RETRIES})")
                time.sleep(RETRY_DELAY * attempt)
            else:
                raise
        except APIError as e:
            if attempt < MAX_RETRIES:
                print(f"    [API ERROR] {e} — retrying ({attempt}/{MAX_RETRIES})")
                time.sleep(RETRY_DELAY)
            else:
                raise
    return text  # fallback — should never reach here


def translate_text(text: str, client: Anthropic, model: str) -> str:
    """翻译单段文本。"""
    protector = ContentProtector()
    protected_text = protector.protect(text)
    prompt = f"请将以下技术文档内容翻译为中文。保留所有占位符 `__PROTECTED_N__` 不变。\n\n{protected_text}"
    translated = _call_api(prompt, client, model, SYSTEM_PROMPT)
    return protector.restore(translated)


def translate_frontmatter(frontmatter: str, client: Anthropic, model: str) -> Optional[str]:
    """只翻译 frontmatter 中的 description 字段。"""
    if yaml is None:
        print("    [WARN] PyYAML not installed, skipping frontmatter translation.")
        return None

    try:
        data = yaml.safe_load(frontmatter)
    except yaml.YAMLError:
        return None

    if not isinstance(data, dict):
        return None

    changed = False
    for key in ("description",):
        if key in data and isinstance(data[key], str):
            translated = translate_text(data[key], client, model)
            if translated and translated != data[key]:
                data[key] = translated
                changed = True

    if not changed:
        return None

    dumped = yaml.dump(data, sort_keys=False, allow_unicode=True, width=4096)
    return dumped


def translate_chunks(chunks: list[str], client: Anthropic, model: str) -> list[str]:
    """翻译多个文本块。"""
    return [translate_text(chunk, client, model) for chunk in chunks]


def group_batches(units: list[TranslationUnit], max_chars: int = BATCH_MAX_CHARS) -> list[list[TranslationUnit]]:
    """将 units 按字符数上限分组，避免单次 API 调用超出上下文限制。"""
    batches: list[list[TranslationUnit]] = []
    current_batch: list[TranslationUnit] = []
    current_chars = 0
    for unit in units:
        unit_chars = len(unit.original_text) + len(unit.rel_path) + 30  # 分隔符开销
        if current_batch and current_chars + unit_chars > max_chars:
            batches.append(current_batch)
            current_batch = [unit]
            current_chars = unit_chars
        else:
            current_batch.append(unit)
            current_chars += unit_chars
    if current_batch:
        batches.append(current_batch)
    return batches


def translate_batch(units: list[TranslationUnit], client: Anthropic, model: str) -> dict[str, str]:
    """批量翻译多个小文件（合并为一次 API 调用），返回 {rel_path: translated}。"""
    builder: list[str] = []
    for unit in units:
        builder.append(f"===== FILE: {unit.rel_path} =====")
        builder.append(unit.original_text)

    combined = "\n".join(builder)
    protector = ContentProtector()
    protected = protector.protect(combined)
    prompt = (
        "请将以下多个文件的内容分别翻译为中文。每个文件以 `===== FILE: path =====` 开头。"
        "保留所有占位符 `__PROTECTED_N__` 不变。输出格式与输入完全一致，保留分隔符和文件路径。\n\n"
        + protected
    )
    translated_combined = _call_api(prompt, client, model, BATCH_SYSTEM_PROMPT)
    translated_combined = protector.restore(translated_combined)

    # 解析回各个文件
    pattern = re.compile(r"===== FILE: (.+?) =====")
    matches = list(pattern.finditer(translated_combined))
    results: dict[str, str] = {}

    for i, match in enumerate(matches):
        rel_path = match.group(1).strip()
        start = match.end()
        end = matches[i + 1].start() if i + 1 < len(matches) else len(translated_combined)
        content = translated_combined[start:end].strip("\n")
        results[rel_path] = content

    return results


# ---------------------------------------------------------------------------
# 文件级翻译
# ---------------------------------------------------------------------------
def translate_unit(unit: TranslationUnit, client: Anthropic, model: str) -> str:
    """翻译单个文件，自动处理 frontmatter、分块、合并。"""
    text = unit.original_text

    # 1. 分离 frontmatter
    frontmatter, body = split_frontmatter(text)
    new_frontmatter = None
    if frontmatter is not None:
        new_frontmatter = translate_frontmatter(frontmatter, client, model)

    # 2. 处理 shell 脚本
    if unit.file_path.suffix == ".sh":
        translated_body = translate_shell_script(body, client, model)
        if new_frontmatter is not None:
            return f"---\n{new_frontmatter}---\n{translated_body}"
        return translated_body

    # 3. Markdown 正文分块翻译
    if body.strip():
        chunks = chunk_markdown(body)
        if len(chunks) == 1:
            translated_body = translate_text(chunks[0], client, model)
        else:
            print(f"    [CHUNK] 分为 {len(chunks)} 块翻译")
            translated_chunks = translate_chunks(chunks, client, model)
            translated_body = "".join(translated_chunks)
    else:
        translated_body = body

    # 4. 组装
    if new_frontmatter is not None:
        text = f"---\n{new_frontmatter}---\n{translated_body}"
    elif frontmatter is not None:
        text = f"---\n{frontmatter}---\n{translated_body}"
    else:
        text = translated_body

    return text


def translate_shell_script(text: str, client: Anthropic, model: str) -> str:
    """翻译 shell 脚本：保留命令，只翻译注释和字符串字面量。"""
    lines = text.splitlines(keepends=True)
    translated_lines: list[str] = []

    for line in lines:
        stripped = line.strip()
        # 纯注释行
        if stripped.startswith("#"):
            translated = translate_text(line, client, model)
            translated_lines.append(translated)
            continue

        # echo 语句中的字符串
        # 匹配 echo "..." 或 echo '...'
        new_line = line
        for quote in ('"', "'"):
            pattern = re.compile(rf"\becho\s+{quote}([^{quote}]*){quote}")
            for match in pattern.finditer(line):
                original_str = match.group(1)
                if not original_str.strip():
                    continue
                translated_str = translate_text(original_str, client, model)
                new_line = new_line.replace(
                    f'echo {quote}{original_str}{quote}',
                    f'echo {quote}{translated_str}{quote}',
                    1,
                )
        translated_lines.append(new_line)

    return "".join(translated_lines)


# ---------------------------------------------------------------------------
# 文件扫描
# ---------------------------------------------------------------------------
def should_translate(path: Path) -> bool:
    """判断文件是否需要翻译。"""
    name = path.name
    s = str(path).replace("\\", "/")
    for pattern in EXCLUDE_PATTERNS:
        if pattern.search(name) or pattern.search(s):
            return False
    return path.suffix.lower() in (".md", ".sh", ".txt")


def scan_files(repo_root: Path, paths: list[str]) -> list[Path]:
    """扫描需要翻译的文件。"""
    files: set[Path] = set()
    for p in paths:
        target = repo_root / p
        if not target.exists():
            print(f"[WARN] Path not found: {target}")
            continue
        if target.is_file():
            if should_translate(target):
                files.add(target.resolve())
        else:
            for child in target.rglob("*"):
                if child.is_file() and should_translate(child):
                    files.add(child.resolve())
    return sorted(files)


# ---------------------------------------------------------------------------
# 主流程
# ---------------------------------------------------------------------------
def main() -> int:
    parser = argparse.ArgumentParser(
        description="CCGS 中文本地化脚本",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    parser.add_argument(
        "--api-key",
        default=os.environ.get("ANTHROPIC_API_KEY"),
        help="Anthropic API Key (默认读取 ANTHROPIC_API_KEY 环境变量)",
    )
    parser.add_argument(
        "--model",
        default=DEFAULT_MODEL,
        help=f"使用的 Claude 模型 (默认: {DEFAULT_MODEL})",
    )
    parser.add_argument(
        "--paths",
        nargs="+",
        default=None,
        help=f"指定要扫描的路径 (默认: {' '.join(DEFAULT_PATHS)})",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="只列出需要翻译的文件，不调用 API",
    )
    parser.add_argument(
        "--force",
        action="store_true",
        help="强制重新翻译所有文件（忽略缓存）",
    )
    parser.add_argument(
        "--repo-root",
        type=Path,
        default=None,
        help="仓库根目录 (默认: 脚本所在目录的上两级)",
    )
    parser.add_argument(
        "--estimate-cost",
        action="store_true",
        help="估算翻译全部待处理文件的成本，不调用 API",
    )
    parser.add_argument(
        "--no-batch",
        action="store_true",
        help="禁用批量合并，每个文件单独调用 API",
    )
    args = parser.parse_args()

    # 确定仓库根目录
    if args.repo_root is None:
        repo_root = Path(__file__).resolve().parents[2]
    else:
        repo_root = args.repo_root.resolve()

    print(f"[INFO] Repository root: {repo_root}")

    # 加载 .env
    env_file = repo_root / ".env"
    if env_file.exists():
        load_dotenv(env_file)
        if not args.api_key:
            args.api_key = os.environ.get("ANTHROPIC_API_KEY")

    if not args.dry_run and not args.estimate_cost and not args.api_key:
        print("[ERROR] API Key not provided. Use --api-key or set ANTHROPIC_API_KEY.")
        return 1

    if not args.dry_run and not args.estimate_cost and Anthropic is None:
        print("[ERROR] anthropic package not installed. Run: pip install -r requirements.txt")
        return 1

    if not args.dry_run and not args.estimate_cost:
        client = Anthropic(api_key=args.api_key)
    else:
        client = None  # type: ignore

    # 扫描文件
    scan_paths = args.paths if args.paths else DEFAULT_PATHS
    files = scan_files(repo_root, scan_paths)
    print(f"[INFO] Scanned {len(files)} files")

    # 加载缓存
    cache_path = repo_root / CACHE_FILE
    cache = LocalizationCache(cache_path)

    # 构建翻译单元
    units: list[TranslationUnit] = []
    for fpath in files:
        rel = str(fpath.relative_to(repo_root)).replace("\\", "/")
        content = fpath.read_text(encoding="utf-8")
        sha = hashlib.sha256(content.encode("utf-8")).hexdigest()
        needs = args.force or cache.needs_translation(rel, sha)
        units.append(TranslationUnit(
            file_path=fpath,
            rel_path=rel,
            original_text=content,
            sha256=sha,
            is_new=needs,
        ))

    todo = [u for u in units if u.is_new]
    skipped = [u for u in units if not u.is_new]

    print(f"[INFO] To translate: {len(todo)} | Up-to-date: {len(skipped)}")

    # 成本预估
    cost_info = estimate_cost(todo, args.model)
    print(
        f"[INFO] Estimated: ~{cost_info['input_tokens']:,} input tokens, "
        f"~{cost_info['output_tokens']:,} output tokens, "
        f"~${cost_info['total_cost_usd']:.2f} USD ({args.model})"
    )

    if args.estimate_cost:
        print("\n[Estimate Cost] Details:")
        for k, v in cost_info.items():
            if isinstance(v, float):
                print(f"  {k}: {v:.4f}")
            else:
                print(f"  {k}: {v}")
        return 0

    if args.dry_run:
        print("\n[Dry Run] Files to translate:\n")
        for u in todo:
            print(f"  - {u.rel_path} ({len(u.original_text)} chars)")
        return 0

    if not todo:
        print("[INFO] All files are up-to-date. Nothing to translate.")
        return 0

    # 分类：小文件批量，大文件单文件分块
    batch_units: list[TranslationUnit] = []
    single_units: list[TranslationUnit] = []

    for u in todo:
        if args.no_batch:
            single_units.append(u)
        elif len(u.original_text) <= BATCH_MAX_CHARS and u.file_path.suffix != ".sh":
            batch_units.append(u)
        else:
            single_units.append(u)

    print(f"[INFO] Batch units: {len(batch_units)} | Single units: {len(single_units)}")

    # 执行翻译
    success = 0
    failed = 0

    # 1. 批量翻译小文件
    if batch_units and not args.no_batch:
        batches = group_batches(batch_units)
        print(f"\n[Batch] Translating {len(batch_units)} small files in {len(batches)} API call(s)...")
        for bidx, batch in enumerate(batches, 1):
            print(f"  Batch {bidx}/{len(batches)} ({len(batch)} files)...")
            try:
                results = translate_batch(batch, client, args.model)
                for unit in batch:
                    translated = results.get(unit.rel_path, "")
                    if not translated:
                        print(f"    [WARN] Empty result for {unit.rel_path}, skipping")
                        failed += 1
                        continue
                    unit.file_path.write_text(translated, encoding="utf-8")
                    cache.update(unit.rel_path, unit.sha256, unit.file_path.stat().st_mtime)
                    cache.save()
                    success += 1
                    print(f"    [OK] {unit.rel_path}")
            except Exception as e:
                print(f"    [FAIL] Batch {bidx} failed: {e}")
                for _ in batch:
                    failed += 1

    # 2. 单文件翻译（大文件或 shell 脚本）
    for idx, unit in enumerate(single_units, 1):
        print(f"\n[{idx}/{len(single_units)}] Translating: {unit.rel_path}")
        try:
            translated = translate_unit(unit, client, args.model)
            unit.file_path.write_text(translated, encoding="utf-8")
            cache.update(unit.rel_path, unit.sha256, unit.file_path.stat().st_mtime)
            cache.save()
            success += 1
            print(f"    [OK] Saved")
        except Exception as e:
            failed += 1
            print(f"    [FAIL] {type(e).__name__}: {e}")

    print(f"\n{'=' * 50}")
    print(f"Done: success={success}, failed={failed}, skipped={len(skipped)}")
    print(f"Cache: {cache_path}")

    return 0 if failed == 0 else 1


if __name__ == "__main__":
    sys.exit(main())

#!/usr/bin/env python3
"""Validate local Markdown link targets in the tracked Toolbox documentation.

This is a deliberately dependency-free file-target checker, not a CommonMark parser:
it does not resolve GitHub heading anchors, generated external sites, or reference links.
"""
from __future__ import annotations

import re
import sys
from pathlib import Path
from urllib.parse import unquote, urlsplit

ROOT = Path(__file__).resolve().parents[2]
LINK = re.compile(r"(?<!\\)!?\[[^\]\n]*\]\(\s*(?:<(?P<angle>[^>\n]+)>|(?P<bare>[^\s)]+))")
FENCE = re.compile(r"^\s{0,3}(`{3,}|~{3,})")
INLINE_CODE = re.compile(r"(`+)[^\n]*?\1")


def inspect_file(path: Path) -> tuple[int, list[str]]:
    errors: list[str] = []
    checked = 0
    inside_fence: tuple[str, int] | None = None

    for number, original in enumerate(path.read_text(encoding="utf-8-sig").splitlines(), 1):
        fence = FENCE.match(original)
        if fence:
            marker = fence.group(1)
            if inside_fence is None:
                inside_fence = (marker[0], len(marker))
            elif marker[0] == inside_fence[0] and len(marker) >= inside_fence[1]:
                inside_fence = None
            continue
        if inside_fence is not None:
            continue

        # Do not interpret illustrative Markdown inside inline code spans.
        line = INLINE_CODE.sub(lambda match: " " * len(match.group(0)), original)
        for match in LINK.finditer(line):
            target = (match.group("angle") or match.group("bare") or "").strip()
            if not target or target.startswith("#"):
                continue
            parsed = urlsplit(target)
            if parsed.scheme or target.startswith("//"):
                continue
            checked += 1
            relative = unquote(parsed.path).replace("\\", "/")
            if not relative:
                continue
            if relative.startswith("/"):
                errors.append(f"{path.relative_to(ROOT)}:{number}: absolute link is not portable: {target}")
                continue

            expected = (path.parent / relative).resolve()
            if not expected.is_relative_to(ROOT) or not expected.exists():
                errors.append(f"{path.relative_to(ROOT)}:{number}: missing local target {target}")
    return checked, errors


def main() -> int:
    total, errors, count = 0, [], 0
    for file in sorted(ROOT.rglob("*.md")):
        if any(part in {".git", ".vs", "bin", "obj", ".idea", "node_modules"} for part in file.parts):
            continue
        checked, issues = inspect_file(file)
        total += checked
        count += 1
        errors.extend(issues)

    print(f"Markdown files checked: {count}")
    print(f"Local link targets checked: {total}")
    if errors:
        print(f"Broken local links: {len(errors)}", file=sys.stderr)
        for problem in errors:
            print(problem, file=sys.stderr)
        return 1
    print("All checked local Markdown file targets exist.")
    return 0


if __name__ == "__main__":
    sys.exit(main())

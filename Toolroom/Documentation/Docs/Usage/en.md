# Repository Markdown file-target checks

Run `python3 Toolroom/Documentation/Check-Markdown-Links.py` from any directory (the checker resolves the repository root from its source path). It scans Markdown files throughout the repository, excluding common build/tool directories.

It reads inline Markdown links and image references outside fenced and simple inline code spans. Relative paths are URL-decoded and checked against the checked-out filesystem. External `https:`, `mailto:`, etc. links are ignored. Absolute filesystem links are reported as nonportable.

**Limits:** this lightweight check does not validate heading fragments, remote URL availability, reference-style link definitions, all nested Markdown syntaxes, or embedded HTML anchors. It is useful for detecting broken local topic links when moving/adding project READMEs. CI should complement human review and build/test jobs.

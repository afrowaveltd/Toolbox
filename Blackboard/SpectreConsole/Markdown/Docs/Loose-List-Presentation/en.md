# Blank-separated paragraphs in terminal lists

**Added 2026-10-31.** The ExchangeHouse Markdown parser supports a deliberately bounded loose-list subset: blank lines may separate explicitly indented paragraphs within a single list item. It retains their ownership and exact original source position.

The Spectre renderer consumes this structure without another parser. It displays a blank line, then aligns the next paragraph under the existing item's *visible text* using marker width and configured `ListIndent`. An existing number or bullet is never repeated for that paragraph. Inline text retains the normal body/link/emphasis styles and remains safe Spectre `Text`.

Sample:

```markdown
- Setup
  First paragraph.

  Second **paragraph**.
- Done
```

With the default theme, the second paragraph appears below the first after an empty line, both under `Setup`. A nested ordered list also respects its original numeral and optional custom indentation.

Only blanks with a following indented paragraph are owned by the list item; trailing blanks and blank-before-marker boundaries do not become loose content. **No full CommonMark loose-list container, lazy continuation or nested fenced-block support is claimed.**

[Parser and ownership semantics](../../../../../ExchangeHouse/Markdown/Docs/Loose-List-Paragraphs/en.md) · [Renderer implementation status](../../IMPLEMENTATION_STATUS.md).

# Proposal: standalone terminal Markdown viewer

**Status:** Idea / design proposal only, captured 2026-11-02. **Not implemented.**
**Trigger:** Revisit after the ExchangeHouse Markdown parser and the Blackboard Spectre renderer have reached a mutually agreed stable feature/test baseline. This document does not claim CommonMark completeness or a product release.

## User goal

Read a real Markdown document or repository README directly in a terminal, including Windows Terminal, Debian and remote SSH sessions, without opening a browser or manually downloading the README. The app must work offline for local files, display source safely, and support the **entire document** rather than truncating larger help files.

The provisional command name `mdview` is **not a finalized product name**.

## Proposed entry points

```text
mdview
mdview README.md
mdview ./Docs/Help/en.md
mdview github:afrowaveltd/Toolbox
mdview https://example.org/manual.md
mdview --source github --repository afrowaveltd/Toolbox
```

With **no arguments**, open an interactive menu:
1. Local file: reuse Spectre FileBrowser, directory navigation and `*.md;*.markdown` filtering.
2. HTTP(S) Markdown: enter an explicit URL; HTTPS is preferred. Validate redirects and content before rendering.
3. GitHub repository: enter `owner/repository`, `github:owner/repository` or a recognized GitHub repository URL.

An explicit path/URL/repository selection bypasses the source menu. Back/exit returns to the menu instead of exiting unexpectedly.

## Source adapters; one document renderer

A future `IMarkdownDocumentSource` (name provisional) should return a validated document, origin, optional revision and a base URI/path for navigation. Proposed providers:

- **Local:** local filesystem, using the present FileBrowser and strict full-document Unicode decoding; never modify the selected file.
- **HTTP(S):** bounded-buffer network download and strict text validation, with timeout, cancellation, redirect policy, status/content-type handling, and warnings for insecure HTTP. Do not execute code or interpret fetched content as terminal markup.
- **GitHub README:** normalize the supplied owner/repository rather than performing fuzzy global search. GitHub REST provides `GET https://api.github.com/repos/{owner}/{repo}/readme` and a raw Markdown media type (`Accept: application/vnd.github.raw+json`), using the default branch when `ref` is omitted. It finds GitHub's preferred README rather than guessing `main`, `master`, `README.md`, or `README.MD`. Follow reported path/ref for linked documents. Optional branch/tag/commit selection and subdirectory READMEs can follow in later versions.

GitHub's unauthenticated public REST API has a rate limit; use conditional requests (ETag/Last-Modified) and an optional disk cache to avoid unnecessary downloads. Add opt-in credentials for private repositories later, with secure secret storage and no token leakage in logs, output or links.

Current external references:
- [GitHub API: Get a repository README](https://docs.github.com/en/rest/repos/contents#get-a-repository-readme)
- [GitHub README conventions](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes)
- [GitHub REST API rate limits](https://docs.github.com/en/rest/using-the-rest-api/rate-limits-for-the-rest-api)

The viewer, not ExchangeHouse, should own fetching, caching, security and navigation. Rendering remains delegated to Blackboard's Markdown Spectre component with optional TextMate language highlighter. Do not introduce GitHub or HTTP dependencies into the parsing library.

## Document navigation and accessibility

The first useful version should show headings, lists, quotes, code, inline links, link destinations and meaningful alt text for images. Follow local relative file links or GitHub repository-relative links in their correct context; resolve `./`, `../`, repository-root paths, fragments and documented branch/ref without silently switching to a different revision.

Desired later conveniences: an outline of headings, `/` text search, `n` for next match, Back/Forward history, optional theme selection, bookmarks, and a clear source/revision indicator. These are *proposed capabilities*, not parser claims.

Terminal-only image fallbacks should be text/alt information; image fetching or sixel/kitty graphics are separate optional enhancements. All output remains safe literal terminal text and code fences are **never executed**.

## Large documents and resource safety

**Do not use an arbitrary 64 KiB content cutoff** or silently display only the first N lines of a help file. Continue full-document support, including lengthy localized help and TextScape documentation. Inform the user when a file may take time to display. Local documents must work offline.

The existing Showcase reader validates a complete document with a small decoding buffer but materializes both its full string and Markdown model in RAM. Before promising unlimited practical size, investigate a line/block index, disk-backed temporary cache, paged presentation and bounded resident memory (possibly leveraging Toolbox's future storage tools). Network input also needs sane protections against unexpectedly large/decompression-bomb responses, but resource safeguards must be explicit rather than silently truncating valid Markdown.

A `404`, authentication error, rate limit, invalid encoding, timeout or unavailable network must produce an actionable message and preserve the user interface. Cached content should display its source and fetch date to distinguish stale/offline documents.

## Suggested delivery slices (after parser completion)

1. **Local standalone viewer:** use existing complete local Markdown loading + FileBrowser and a reusable navigation/keyboard host. Test Windows, Debian, SSH, UTF-8/16/32, long documents and code escape safety.
2. **GitHub README provider:** `owner/repo` input, official preferred README API, default ref, correct errors and safe cache; test both sample repositories and fake HTTP handlers.
3. **General HTTP(S) provider:** explicit URL, safe redirects, streaming validation, retries/timeouts, offline cache, test servers and error paths.
4. **Linked document navigation:** relative links, anchors, history, search, outline and future language selection for `Docs/**/en.md`.

Actual naming, assembly placement, public API, package policy and exact keybindings are open questions. **No new project should be created solely because of this proposal.**

## Existing building blocks

- [ExchangeHouse Markdown parser](../../../../../ExchangeHouse/Markdown/README.md)
- [Blackboard renderer README](../../README.md)
- [Blackboard Showcase](../../../../Samples/Blackboard.Showcase/README.md), which already opens whole local Markdown files in read-only mode.

[Markdown implementation status](../../IMPLEMENTATION_STATUS.md).

# Placeholder-safe HTML translation with context samples

Status: design refinement, 2026-10-22; **not implemented**.

## Motivation

Dita's LibreTranslate implementation is an experimental reference, not code to clone blindly. UnderstandMe should preserve placeholder identities and positions during translation while providing representative sample values to improve contextual grammatical translation. LibreTranslate supports `format=html`. The LibreTranslate community has documented using `<span translate="no">…</span>` to protect nontranslated spans; this still requires local verification of resulting markup.

Example template: `Hello {userName}, how are you?`, hint/sample: `Bolaji`.

Illustrative protected backend text:
```html
Hello <span translate="no" data-um-id="p0">Bolaji</span>, how are you?
```
Send with `format=html`. The editor/translator preserves the internal mapping `p0 -> userName` within the request, **never trusting returned attributes as the sole source of identity**. On return, parse markup with an HTML-aware parser; verify expected span markers (identity, occurrence count, nesting, duplication, removal), then reconstruct the template (e.g. `Ahoj {userName}, jak se máš?`), validate named placeholder syntax and store only the template, **not** the sample-value-rendered text.

The original proposed `name="userName"` is useful conceptually, but use an internal request marker instead, to avoid exposing source placeholder keys unnecessarily and to make repeated placeholders and potential name collisions unambiguous. This example uses a sample `Bolaji` to supply context but `translate="no"` prevents its translation. Do not assume a `span` without `translate="no"` protects content.

## Safety and fallback

- HTML-encode sample values (including `&`, `<`, quotes and Unicode). Use a proper HTML serializer/parser, not regex stripping.
- Track every placeholder occurrence, including repeated identifiers, in request-local metadata and verify exact match counts before persistence. Reject unknown, missing, duplicated or structurally corrupted marker elements.
- Validate the reconstructed target template through the existing placeholder validator; only publish through conditional source-revision/current-target checks.
- Context hints and sample values may be personal (e.g. names). Provider routing MUST respect request privacy/consent; never log or persist samples in shared dictionaries or content telemetry.
- `translate="no"` protects the sample but does not guarantee translation quality or precise grammatical inflection. Test language families and potentially support explicit typed grammatical hints. The HTML-aware strategy is provider-capability-dependent; other providers may require alternative placeholder protection.
- Handle source text that is originally HTML separately: do not insert an invalid fragment or allow source markup to impersonate internal markers. Use robust tokenization and marker namespace reservation.
- Failed markup verification yields an invalid attempt/retry-with-alternate-policy outcome, not silent deletion of placeholders or storage of broken translations.
- Cache identity must include source template and translation-relevant hint policy (to avoid invalid reuse when contextual hints alter translation); never use the concrete sample as a shared entry key.

## Tests needed

Round trip for one, many and repeated placeholders; reordered placeholder tokens; multiple sample values; HTML encoding and malicious markup injection; nested HTML source; missing/modified/reordered/duplicate markers; source-placeholder mismatch; target placeholder validation; source revision races; right-to-left and grammatical gender/context; providers without HTML support; P0 ephemeral/private caching and source-value redaction.

API shape, hint syntax and provider adapters are to be finalized during implementation. All public code requires XML comments, with targeted explanatory `//` comments around mapping, trust checks and reconstruction.

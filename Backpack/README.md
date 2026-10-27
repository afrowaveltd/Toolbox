# Backpack — AJIS 2.0 in Afrowave.Toolbox

> **Status: DESIGN / knowledge capture, not a working AJIS 2 implementation.**
> Last design capture: 2026-10-27. API names, wire format and syntax examples are provisional.

Backpack is the proposed **.NET 11** successor to AJIS 1.x, developed within [Afrowave.Toolbox](https://github.com/afrowaveltd/Toolbox). It is a new, independent project: do not copy AJIS 1 code wholesale, alter the AJIS 1 repository or claim AJIS 2 capabilities are implemented.

**Philosophy:** A small JSON-familiar surface, a few composable rules, powerful optional capabilities, and no mandatory complexity for simple documents. AJIS is a portable smart data backpack, not a scripting language.

## Engineering rule

Use the [Toolbox-wide design philosophy](../Docs/Design-Philosophy/en.md): do not reject an idea simply because it is ambitious; first capture what it enables, then look for common mechanisms that make it simpler. Keep a JSON-simple path for ordinary users.

**Language targets:** .NET 11 first; **vanilla JavaScript and TypeScript are important planned first-class implementations**, not incidental ports. The AJIS document contract stays common, while the JS API should feel like JavaScript (including model-local callback functions), not a literal C#-style DI translation. Our [JSLinq](https://github.com/afrowaveltd/JSLinq) and existing JavaScript SignalR work are potential complementary integrations, **not required dependencies** of the AJIS core.

## Start here

- [AJIS 1 baseline and real sources](Docs/AJIS1_BASELINE.md) — what is implemented, what was designed, and where to look.
- [Architecture and invariants](Docs/ARCHITECTURE.md) — minimal framing, pointers, unified directives, streaming, binary layout, storage, agent workspaces, handlers.
- [Requirements / AJIS 1 → Backpack gaps](Docs/REQUIREMENTS_AND_GAPS.md) — specific follow-up work.
- [Decision log / questions](Docs/DECISIONS_AND_OPEN_QUESTIONS.md) — established directions vs open proposals.
- [Czech conversation capture](Docs/IDEAS_2026-10-27.cs.md) — preserve the rationale and examples from the design session.
- [Use cases](Docs/USE_CASES.md) — Hardware Revival, cross-environment data.
- [JSLinq / JS AjisFS integration](Docs/JSLINQ_AJISFS_INTEGRATION.md) — source-based assessment of in-memory LINQ, async streams, disk-index pushdown and resource cleanup.
- [Compatibility tests to write](Docs/COMPATIBILITY_TESTS.md).

**Earlier discussion:** [../Ajis/DESIGN_NOTES.md](../Ajis/DESIGN_NOTES.md), captured September 2026, is retained untouched. It includes ideas on profiles, large-data processing, transport variants, security, A2FS, tooling and testing. Those ideas remain input to the review, not automatically accepted normative requirements. Note that it assumes a future separate A2 repository; the current decision is **to develop .NET Backpack inside Toolbox**.

## Existing implementations to compare

- [Ajis.Dotnet (AJIS 1 family)](https://github.com/afrowaveltd/Ajis.Dotnet), default branch `master`.
- [AJIS_ATP (C implementation and format work)](https://github.com/afrowaveltd/AJIS_ATP), default branch `main`.
- [Toolbox](https://github.com/afrowaveltd/Toolbox), default branch `master`.

## Scaffold

`Afrowave.Toolbox.Backpack.csproj` is intentionally **empty and not packable**. No public parser, serializer, directives or pointer APIs have been implemented or promised. It targets `net11.0` to leave room for C# 15 discriminated unions after the SDK/toolchain is available. Since the rest of Toolbox targets `net10.0`, a full solution build also needs the .NET 11 SDK. GitHub Actions is configured to install both the stable .NET 10 SDK and the .NET 11 preview SDK while 11 is prerelease. Packaging and eventual implementation are separate decisions.

## Next work

1. Verify the AJIS 1 behavior against actual tests, not only historical roadmap prose.
2. Reduce the design to a minimal formal grammar, unambiguous streaming fixtures and versioned wire contract.
3. Establish conformance tests for **round-trip preservation** and JSON-compatible basic usage.
4. Design C# objects and serializers from the grammar (not vice versa).
5. Implement incrementally with a small executable slice and test each invariant.

Do not promote examples from the discussion to normative grammar without a specific decision and test case.

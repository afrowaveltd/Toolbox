# Afrowave design philosophy: explore broadly, simplify deliberately

> Cross-project engineering principle. Not a mandate to implement every idea.

## 1. First explore, then reduce

- **Ideas are welcome, even if they initially sound difficult, ambitious, unusual or expensive.** Do not reject useful possibilities solely because they seem hard.
- Collect user needs, scenarios, and candidate approaches **before** locking the implementation.
- Then look for overlap. Can two capabilities share the same mechanism? Can a new capability be expressed by composing rules that already exist?
- **Reduce independent rules, not useful capabilities.** Remove redundancy after finding it; do not achieve "simplicity" by discarding requirements prematurely.
- A complicated feature is not necessarily a complicated format. A powerful implementation can have a tiny, understandable surface.
- Exploration does not imply automatic adoption: distinguish brainstorming, candidate, chosen design, validated implementation, and released contract.

## 2. Smallest useful operation first

A simple consumer should need only a simple call; optional modules, DI, global initialization, transport infrastructure or runtime assumptions must not be prerequisites for basic functionality. Each part should do only its own job.

## 3. Language-native experience over forced API uniformity

Cross-language products should share **data semantics, portable document contracts and conformance fixtures**, not necessarily the same class names or chaining style. Expose idiomatic APIs for each ecosystem and preserve a minimal dependency-free entry point where viable.

## 4. Compatibility by impact, not dogma

Preserve old behavior when inexpensive. Allow a justified major-version break where it simplifies the architecture. Provide documentation for localized configuration changes, and investigate adapters/migration tooling when a change would force broad consumer rewrites.

## 5. How to capture new ideas

For every significant feature, record the underlying problem, examples, possible common mechanisms, interactions with existing rules, unknowns, tests, and what could be **removed** from the design. Keep ideas visible while they are still unsettled.

**Example:** [Backpack / AJIS 2](../../Backpack/README.md) moved from separate document-wide `#` and object-wide `$` directive families to one proposed `#` rule that attaches to the following element, including the document root. That is the desired kind of simplification: fewer constructs, no lost expressive power.

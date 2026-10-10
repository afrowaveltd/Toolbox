# Translation Pipes, Chains and Pluggable Workers

Status: design proposal and agreed functional direction; not implemented.

## Different concepts

- **Queue**: physical waiting work by priority, owned by QueueMaster.
- **Buffer**: a small FIFO reservation of items owned by an endpoint/worker.
- **Pipe**: selection/execution policies (priority, persistence, caching, retries, provider preference, privacy, review, retention).
- **Chain**: sequence of compatible processing steps, potentially with branch/alternative results.
- **Worker**: one operation that accepts a compatible input and returns one or more results, or an error; it need not understand the whole Chain.

Policies should be easy to configure with flags for combinations, enums for exclusive states, and scalar limits for durations/size. New community workers should be registered via interfaces/capabilities, not by extending a central list of hardcoded implementation names. Global pipe profiles may be overridden per target and, subject to policy constraints, per job.

## Translation providers

A provider describes a family (LibreTranslate, Google, Microsoft, Ollama, OpenAI, Human). An endpoint is one configured instance with its own URI/credentials, capabilities, languages, buffer size, concurrency and `IsReady`. Providers/endpoints and Translation Targets are addable/removable via configuration; new requests follow updated selection rules. Do not route unsupported pairs to an endpoint.

Translation alternatives may be produced by multiple providers in parallel for an editor, or sequentially for cost-sensitive automatic fallback. A failure can retry or choose another compatible endpoint. QueueMaster does not need to know what semantic operation the worker performs.

## Configurable chain examples

- `FastUI`: short machine translation → placeholder/format validation → provisional cache.
- `DictionarySync`: discover new keys → translate configured languages → validate → write authoritative dictionaries → `Update_Cache`.
- `ArticleAI`: logical split → translate chunks → reassemble → mandatory AI fidelity/language review → final validation → result.
- `ManualReview`: human/AI alternatives → manual selection/edit → auditable publication.

An AI pass can combine translation, tone and constraints in one composed prompt; two-pass translation + final review; or separate translation, style revision and semantic validation. The web-host editor should let a user build/reorder/configure these chains. Individual steps choose prompts/models. Prompt templates and fragments are versioned/configurable with inspectable composed final prompts.

## Source fidelity and structure

Human comments may be explicit, emotional or sarcastic. Preserve source meaning, wording intensity and tone unless the target's explicit editorial policy authorizes changes. Documentation may allow professional polishing. Distinguish faithful translation from optional editorial rewriting. An AI semantic check compares proposed output to the **original**, not a previous generated translation.

Split long articles/documents on meaningful boundaries, keep stable part IDs and order, translate chunks, and assemble them. Document-format adapters must protect Markdown/HTML syntax, links, code and protected tokens. Structured outputs from Ollama/AI can be validated against agreed schemas; parsable JSON alone does not establish semantic correctness.

Results are typed objects and may hold multiple variants from multiple models or branches. Any preserved compatible result may serve as input to another step or to a persistent rollback/resume workflow. A required final check cannot be bypassed by marking an earlier step completed.

Related: [Editor and Audit](../Editor-and-Audit/en.md), [Notes](../../Notes.md).


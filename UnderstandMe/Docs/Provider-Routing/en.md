# Translation Provider Selection, Cost and Escalation

Status: agreed architectural direction and design questions, 2026-10-10. This is not implemented and does not fix public interfaces or configuration property names.

## Goal

UnderstandMe supports multiple translation providers rather than requiring one translation engine. The preferred first provider is chosen by **policy and workload**, not hard-coded in QueueMaster. Self-hosted LibreTranslate is a first-class economical default for suitable workloads. Other installations may prefer Google, Microsoft, an AI provider, self-hosted AfroLT or a human worker. Installing any optional backend does not make it a core dependency.

## Separate the different priorities

- **Job priority** belongs to QueueMaster: P0 real-time through P4 background, controlling *when* work is dispatched.
- **Provider order/preference** belongs to the Translation Pipe's provider-selection policy: deciding *which eligible engine* should handle a particular translation step.
- **Endpoint capacity/readiness** belongs to each configured provider endpoint: it describes what that instance can currently accept, not the global queue order.

A high-priority job does **not** necessarily require an expensive provider. A low-priority background job may explicitly request a costly AI review. Provider selection must not create a second global queue.

## Providers and endpoints

A provider family (LibreTranslate, Google, Microsoft, AfroLT, Ollama/AI, Human, or a community implementation) may have multiple configured endpoints. Each endpoint registers:

- stable provider/endpoint ID, version and capabilities;
- available source/target language pairs, format support (plain text, HTML etc.), batch/size limits, placeholder or protected-content requirements;
- readiness/health, concurrency, request/throughput limits and estimated latency;
- configured operating-cost profile (currency, applicable billing unit, per-unit estimate where known, and optionally local running costs), and request/budget limits;
- credential/privacy restrictions and local-only versus remote designation.

Actual costs depend on the backend and billing basis; do not claim one provider is universally cheaper or that a local service has zero cost. Configured costs may be estimated/unknown. Keep provider capability detection and cost estimation separate from client-specific policy decisions. The engine contract returns typed results/failures through existing Toolbox conventions.

## Request policy and configuration precedence

Define reusable **ProviderSelectionPolicy** profiles and attach one to a Translation Pipe, target, chain step or eligible request. Configuration hierarchy is: system defaults -> named Pipe profile -> Translation Target constraints/preferences -> allowed individual request overrides. **Hard requirements are never bypassed** by more specific preferences, including privacy, supported formats/languages, permitted provider families, maximum spending and mandatory validation/review.

Profiles are independent of other modules' internals and may select by:

- a **fixed preference order** (e.g. LibreTranslate first; Google second);
- **lowest estimated eligible cost**; or
- explicitly prioritized requirements (quality/latency/availability), optionally restricted to an administrator-defined provider list.

The provider rank/order is deliberate and configurable. A primary paid Google endpoint may be selected even when LibreTranslate is available, if that is what the policy requests.

Suggested conceptual profiles, not mandatory names:

| Profile | Start with | Possible escalation |
| --- | --- | --- |
| Economy | Lowest-cost compatible provider, typically local LibreTranslate in a self-hosted deployment | Next eligible provider only when enabled, justified and within budget |
| PreferredGoogle | Configured Google endpoint | Another eligible endpoint according to policy |
| LocalOnly | Compatible local LibreTranslate or AfroLT endpoint | Other local providers only; no external service |
| ReviewFirst | Configured translation provider plus a separately required validation/review Chain step | Target-specific alternate provider or human review |

## Eligibility, fallback and quality

The selection policy first **filters for eligible endpoints** based on languages, request format, capabilities, ready state, privacy/LocalOnly limits, explicit allowlists, budget and other hard constraints. Then it chooses among eligible candidates by priority/order or cost/latency preference. Never submit unsupported languages or a format to a provider merely because it ranks first.

**Eskalation/fallback is not automatic with every request.** It happens only if authorized by the Pipe/target/job and triggered by a concrete reason such as:

- transport timeout, transient provider failure, overload or unavailable endpoint;
- unsupported capability discovered before dispatch or an explicit provider-reported limitation;
- validation failure (protected placeholders/HTML structure, output language, requested format, or a separately configured semantic review);
- a deliberate policy requiring multiple candidates, higher-tier review or manual selection.

Do not infer quality from a provider's name alone; a fluent-looking result is not proof of fidelity. If the translation is structurally valid but wrong in meaning, detecting that requires an explicit evaluator/reviewer; the system must not promise objective automatic quality scoring without such a step. Failed mandatory checks must not silently publish unverified translations.

Escalation also needs a policy for **which failures are retryable**, per-endpoint backoff and maximum attempts/provider switches. Chargeable requests can already have been processed even if their response times out, so retries may incur costs; prefer idempotent job IDs where provider capabilities permit, guard against retry storms and enforce total expense/time caps. Preserve useful candidate results and failure reasons for optional editor/audit workflows without requiring that every real-time translation be persisted.

## Architecture ownership

- QueueMaster: priority queues and atomic job dispatch/ownership; receives eligible work and dispatches to compatible workers without hard-coded vendor decisions.
- Translation Pipe / provider selection: evaluates constraints, provider order and fallback/escalation policy **for each translation step**.
- Provider adapters and endpoints: implement translation and report supported capabilities, capacity, results and failures, without knowing about all other backends.
- Translation Chain: decides processing stages such as translation, format check, semantic review and approval; a review step can use a different provider from the initial translation.
- ResultProcessor: restore protected syntax and validate final output according to the Chain and target contract; it does not itself reassign jobs.
- Observability: emits provider selection, fallback reasons, costs/estimates, attempts and outcomes as typed events to optional SystemMonitor without storing API keys or full text. Operational/audit persistence follows the job's own policy, not SignalR delivery.

## Before implementation

Specify the minimum provider capability contract, policy precedence and selection algorithm; define capability/price units (per character/token/request/local resource), and test language eligibility, unsupported HTML, LocalOnly constraints, absent/unhealthy endpoints, budget ceilings, retries after ambiguous timeouts, mandatory validation failure, provider-order overrides and fallback exhaustion. Test that QueueMaster job priority does not get conflated with provider rank.

See [Translation Chains](../Translation-Chains/en.md), [Architecture](../Architecture/en.md), [AfroLT future design](../AfroLT-Future-Design/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).

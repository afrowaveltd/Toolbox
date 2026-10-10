# Architecture and Responsibility Boundaries

Status: planned design, 2026-10-10. This file is intentionally about responsibilities rather than a frozen class/folder structure.

## Principle

A library is a collection of independently replaceable workers and interfaces, not one hardcoded Afutrans workflow. Each component owns one concern, declares its inputs/outputs and capabilities, and uses contracts rather than concrete implementations. The composition root of an application chooses the pieces. Avoid circular implementation-project references.

## Core modules

| Module | Owns | Must not own |
| --- | --- | --- |
| QueueMaster | Global priority queues, job ownership, demand-driven dispatch, persistence/recovery contracts | Translation algorithm, UI, source parsing |
| DataAccess | Reading/writing external data and capabilities | Identifying phrases to translate, priority logic |
| TextSource | Format-aware extraction, segment metadata and request preparation | Internal queue implementation |
| Translation workers | Processing an eligible item and returning an outcome | Assignment of other workers, storage routing |
| Chain/steps | Step ordering and compatibility for an individual workflow | Ownership of all system queues |
| ResultProcessor | Structure/placeholder restoration, assembly, result routing | Choosing next translator |
| Editor | Dictionary inspection, manual edits, optional improvement requests | Alternative queue infrastructure |
| Messenger | Event transport and status projection | Commands, dispatch, prioritization |
| Localization | Standard .NET localization, dictionary lookup and provisional cache | Mandatory dependence on Afutrans service |

A human can be a translator worker, but the Editor additionally acts as an independent dictionary-management client. A new provider registers capabilities; QueueMaster does not need enum cases naming every possible community implementation.

## Provider activation and minimal human-only installation

**Human translation is the only mandatory translation provider capability**: the core must accept human work, support a manual translator worker and receive typed human-created results. A system with no external machine or AI translator is fully valid. Human translation does not require permanent availability or mandatory human review of all machine-produced results; those are workflow choices.

LibreTranslate, AfroLT, Google, Microsoft, Ollama/AI and any other automatic provider are **optional modules/plugins** individually activated by the host. No automatic backend package, model or paid API credentials are required by the core, and a provider-priority list does not auto-enable vendors. Default preferences apply to the subset actually installed and enabled. An operator who wishes to avoid paid translation can simply omit paid/AI integrations altogether.

The default known-client profile adds no restrictions beyond the host's enabled-provider set and global hard limits. An optional public Afutrans service has a **separately enabled and administrator-configurable** profile: it may offer only idle free/local capacity, deliberately expose all enabled providers including paid/AI capacity, or any narrower/human-only choice. The software must neither force paid access nor forbid an explicit administrator-approved public offering. [Client Access Policies](../Client-Access-Policies/en.md), [Provider Routing](../Provider-Routing/en.md).

## Human translators process durable waiting work

The mandatory Human provider is a **regular QueueMaster-compatible worker**, not a separate engine or an always-online service. The host may enqueue/persist human-intended work when no translator is connected; opening a translator UI later retrieves authoritative pending/in-progress totals by priority/language and allows atomic exclusive claim, editing and completion. No new physical per-human queue is introduced; the existing five priority queues and persistent work ownership remain authoritative. A missed monitoring event or disconnected UI cannot cause accepted work to disappear.

There is **no automatic human continuation** of an ephemeral RealTime request. An explicitly authorized manual translation of the content must be submitted separately as a durable **non-P0** request; private chat text is never exposed to Human merely because RealTime fails. Human results are validated and delivered through the same typed contracts as other providers. See [Human Translation Workflow](../Human-Translation-Workflow/en.md).

## P0 RealTime boundary: no Human and no private-chat exposure

Although **Human is the only required provider capability**, the Human worker belongs **only to durable explicitly authorized non-RealTime P1-P4 translation**. `P0 RealTime` is an automatic-only execution path. If a host has no activated automatic translator, P0 is **inactive**: return the original untranslated text almost immediately, without queuing or invoking Human. If a configured machine translator fails, no Human fallback is allowed, including for `Mandatory` requests.

RealTime may carry private chat messages. Its source content must **not be persisted as a Human job, placed in a manual editor/inbox, or copied for later human review** merely because automatic translation is unavailable. An actual human task exists only when separately submitted as an authorized and durable non-P0 manual request. This is a strict selection/privacy constraint, not just a preference or a postponed worker choice. See [Human Translation Workflow](../Human-Translation-Workflow/en.md) and [Request Settings](../Request-Settings-and-Live-Translation/en.md).

## Privacy is an independent request and result-storage dimension

Each translation request carries a `Privacy` classification that can be made stricter by Client/Target/host policy. It controls **visibility**, while `Persist` and cache/lifetime options control **retention**. A `Private` chat/forum translation may be persisted for history reuse but must be available **only to current authorized viewers** of its source and may not automatically appear in Human translator queues, manual audit, public dictionary keys, shared translation memory or raw SystemMonitor data. Any standalone authorized Human translation must be explicitly initiated as a separate non-P0 operation. These exclusions apply to any queue priority, not just P0.

**P0 is still volatile and automatic-only**: saving its verified translated result in a separate scoped content store does **not** retain the P0 work item. The host owns source-message permission checks on **every** private translation lookup, with correct tenant, conversation, message revision, culture, retention and deletion behavior. Provider adapters accept private text only when authorized by the effective external-processor disclosure policy. [Privacy and Scoped Storage](../Translation-Privacy-and-Scoped-Storage/en.md).

## Message translation stays host-owned

**Application localization** and **chat/message translation** share processing contracts but not their storage model. `KEY=VALUE`/dictionary-less keys are reserved for application/web UI resource dictionaries. For chat/other user-generated text, the Splitter assigns **temporary `0..N` segment IDs**, the engine returns translated segments, and **the consuming host assembles the entire message**. Chunk IDs are neither stored nor treated as dictionary keys. The chat app owns its message history, translated content reuse, authorized recipient views and retention; UnderstandMe does **not** mandate centralized chat persistence.

A recipient can optionally **edit their own translated view**. The application applies the correction only in the current view when caching is disabled, or saves a **recipient-specific override** at their opt-in when permitted. Cache does not automatically mean publication to other users or shared memory; broader sharing, if supported, is a separate authorized action. Initial automatically translated text remains private to its allowed recipients; no generic Human auditor receives it. The dictionary Editor's required revision audit applies to UI dictionaries, not automatically to personal chat edits. [Chat Translation Flow](../Chat-Translation-Flow/en.md).

## Semantic Splitter and document-specific adapters

One reusable **format-aware Splitter** creates meaningful, request-local chunks; a short text need not split. For plain prose, **paragraphs are preferred** if reliably present, with sentence boundaries for longer unformatted continuous text. Markdown uses semantic text-bearing syntax nodes; HTML uses translatable element/text content while preserving nested inline markup. Each new format may register its own splitter without changing QueueMaster/provider interfaces. A provider size ceiling may require safer further subdivision; not arbitrary first-pass chopping.

`Settings` can select format and paragraph/sentence policy. `RequestId` and `SegmentId` remain **transient**, not saved localization `KEY=VALUE` keys. The host, not the engine, assembles the final document/message and owns presentation/storage. [Semantic Splitter](../Semantic-Splitter/en.md).

## Composition

The host registers DataAccess, format adapters, backend endpoints, result processors, optional Chain steps, history/audit, Messenger transports and UI extensions through interfaces. Translation Targets supply connection and policy profiles. Features can be omitted entirely.

`Translation Pipe` describes policies (persistence, priority, retries, privacy, caching and selection). `Translation Chain` describes the processing steps. They are related, but not identical, and neither requires one physical queue per target/type.

The architecture must also allow asynchronous requests for additional translation work from a chain step or editor, preserving correlation to the originating job while remaining within QueueMaster's public API.

## Policy-first request admission with functional best effort

The host **orchestrator** is the authority at the request boundary: before enqueueing, it validates the caller's translation intent, applies higher-priority system/administrator policies, preserves the original request, and creates a normalized effective Settings snapshot. Caller-specified endpoint, Google API or otherwise, is a **preference**, not an enforceable choice. If forbidden, over-quota or unavailable, the orchestrator must seek an eligible authorized alternative rather than rejecting solely on the requested provider. If none can satisfy language, format, privacy and hard account limits, return a specific typed rejection or later failure with explanatory reasons.

The orchestrator can adjust permitted priority/flag/provider preferences on admission and must disclose material changes to the client. QueueMaster accepts approved work, schedules it and respects administrative pauses; provider routing evaluates candidates and current readiness/account caps at dispatch. The actual provider is reported after execution, not merely from an admission plan. A restrictive billing/operational cap takes precedence even over previously accepted jobs, without rewriting their original request or historical policy snapshot.

These rules apply to every transport and frontend without requiring a specific module to know the concrete vendor implementation. [Request Admission and Normalization](../Request-Admission-and-Normalization/en.md).

## Per-client translation policies and optional public capacity

Add a **ClientPolicy** boundary to the application orchestrator: trusted `ClientContext` (registered/paid, internal or public anonymous) supplies provider/endpoint/account access, per-client quota and request priority/rate/concurrency limits. The original client request does not self-authorize paid engines. By default ordinary registered/internal clients can use all provider types allowed by global administration; optional named profiles can narrow this to free/local engines or extend it to purchased paid-provider entitlements.

Policy is an **intersection** of global admin limits, client plan and request's hard needs, rather than a second QueueMaster scheduler. A shared paid-provider usage account retains its own hard cap independent of individual client-plan quota. For example, the optional public Afutrans API may publish spare local LibreTranslate capacity to other users, while clients lacking paid access can never spend the host's Google allowance. Public usage receives configurable rate/concurrency/quota caps and can be scheduled behind reserved internal capacity without changing the queue's single ownership model.

Authentication for billable translation access is distinct from the intentionally lite GUID-only monitoring peer identification. All business transports enforce the same client policy through common application handlers; an anonymous public profile can be configured separately. [Client Access Policies](../Client-Access-Policies/en.md).

## Provider routing is not queue priority

Keep the **translator provider selection policy** outside the QueueMaster's five job-priority queues. The QueueMaster retains ownership of job scheduling and atomic dispatch; a Pipe-level selector uses provider/endpoint capability declarations, configured order, per-request constraints and cost caps to determine compatible candidates and permitted escalation. Neither a worker adapter nor the global queue hard-codes LibreTranslate, Google, Microsoft or AfroLT as a mandatory dependency.

One installation may prioritize local LibreTranslate and escalate only when necessary; another can prioritize a paid provider and use the others as fallback. Selection is configurable globally, by Translation Target/Chain step and, if allowed, for an individual request. Strict LocalOnly, formats/languages and spending constraints are not waived by overrides. Validation and review remain explicit Chain/ResultProcessor responsibilities. [Provider routing](../Provider-Routing/en.md).

## Request Settings, live segments and stable provider policies

A typed effective `Settings` snapshot is resolved for each new logical request. It separates the exclusive QueueMaster P0–P4 job priority from combinable execution flags, e.g. `Mandatory` and opt-in `SystemPersistent` translation-memory reads/writes. QueueMaster schedules; Translation Pipe/provider policy chooses compatible available endpoints according to cost/privacy/language and operator-configured priority, including overrides by language pair. Default routing tiers are local/free -> equally ranked paid APIs -> AI, all configurable. A RealTime Mandatory request favors an **available acceptable** backend without waiting for a busy preferred endpoint, whereas non-Mandatory RealTime can return original source immediately and update later. No required provider may violate hard constraints.

A format-aware splitter yields ordered `(SegmentId, Text)` pairs; an independent `RequestId` scopes browser live updates. A client can replace individual verified paragraphs while preserving Markdown/HTML format boundaries using safe DOM text updates or validated rendered HTML, without using the outbound-only administrative SystemMonitor for translated user content.

Configuration changes apply to **new requests**. Existing queued/in-flight work retains its captured effective Settings, but an administrator's orchestration pause/stop supersedes dispatch irrespective of Mandatory. The optional approved bounded SystemPersistent memory is checked before provider calls; write/promotion is separate from reading and temporary cache. Capability tests consider model version changes, without maintaining automatic long-lived language-quality rankings. [Request Settings and Live Translation](../Request-Settings-and-Live-Translation/en.md).

## Provider billing and UI-independent live results

**Billing belongs to a provider account's commercial plan, not a single hard-coded vendor price.** A provider adapter declares metering/cost capabilities; configured account/endpoint billing Settings represent purchased monthly bundles, renewal, included usage, allowed overage price and an administrator-controlled spending cap. A durable UsageLedger/QuotaAccount tracks usage and concurrent reservations (potentially shared across hosts) independently from best-effort SystemMonitor. At each chargeable dispatch, provider routing checks current account caps, even though existing requests retain their original selection preferences. An administrator alone approves overage or a new package; the system calculates, enforces and reports. [Provider Billing](../Provider-Billing-and-Quotas/en.md).

Likewise, the **splitter and completion contract is presentation-neutral**: segment identity, source content/format metadata, RequestId, verified translated segment and revision. Blazor, JavaScript, Avalonia or other clients choose their own way to change displayed text or re-render a component. The translation core does not prescribe DOM spans or JavaScript functions. [Request Settings and Live Translation](../Request-Settings-and-Live-Translation/en.md).

## Hierarchical SystemMonitor

Each module emits events independently. A host maps `/SystemMonitor` for events from all locally registered modules, and `/AfroLT/SystemMonitor`, `/Afutrans/SystemMonitor`, `/UnderstandMe/SystemMonitor` for each module's own events. All use one typed envelope with sender identity. The host fans out a single event to global and module feeds, without Hub-to-Hub forwarding. Remote instances only aggregate through an optional bridge. Monitoring is outbound-only, best-effort, never a storage or command channel. See [SystemMonitor](../SystemMonitor/en.md).

### Optional remote event aggregation

A monitoring host may also load an optional **SignalR client receiver** connected to multiple trusted remote `/SystemMonitor` endpoints. Received events enter the same local event router as self-generated events; its root `/SystemMonitor` combines local and imported monitoring streams, as in VPS1 and VPS2 feeding VPS3. The receiver does not add callable methods to the outbound-only Hub and does not make individual modules depend on one another.

Preserve original event/source IDs and generic message contracts. Federation is explicitly configured and uses bounded nonblocking queues, per-connection reconnect/health status, provenance and loop/deduplication protection. It remains best-effort, never a durable job or incident delivery mechanism. See [SystemMonitor](../SystemMonitor/en.md).

### Lite SystemMonitor peer identification

SystemMonitor's basic pairing is **identification/filtering**, not full application authentication. Each node has a known GUID `NodeId`. Remote receiver configurations list the expected node and its HTTPS endpoint; the source announces its identity, offered module sources and typed event contracts in an initial `MonitorHello`. An unknown or mismatched node is ignored, and in-process listeners require no credentials. A connecting monitor client may identify itself by GUID during connection establishment if the serving host uses a client allowlist.

No default shared high-entropy key, OAuth/JWT provider or central identity service is needed in this limited anonymous, outbound-only monitoring scenario. GUIDs are not secrets and do not cryptographically authenticate a source; trusted endpoint configuration and normal TLS certificate checks remain important. The monitoring Hub has no client-callable publishing function. Further authorization can be an optional extension if the threat model changes. See [SystemMonitor identification](../SystemMonitor/en.md).

### Pending monitor registrations and administrator approval

An unknown monitoring node may request enrollment through a separate small HTTPS control endpoint and identify itself by public `ClientId` GUID, its remote endpoint and offered modules. **Enrollment is never automatic.** The receiving monitor host persists the pending request, then optionally broadcasts a typed `MonitorRegistrationRequested` event. An administrator can approve/deny immediately if listening or later from the stored requests, using a separate authorized API/UI. On approval the recipient issues a **new private random `ClientKey` GUID**, binds it to `ClientId` and permitted sources, and makes it retrievable only by the original applicant through a separate enrollment claim channel. The approved client can subsequently present both IDs in HTTPS control request headers.

The event stream must not carry this private key; SignalR monitoring remains purely outbound and best-effort. Enabling a source after approval uses an independently configured receiver SignalR client and normal HTTPS source checks. The issued credential secures recipient-side control/enrollment and is not automatically authentication for the reverse-direction signal stream. See [SystemMonitor enrollment](../SystemMonitor/en.md).

### Risk-triggered challenge for recognized monitor nodes

An already approved monitor client is normally recognized by its public `ClientId` and familiar connection metadata (expected host, observed network profile and declared sources); its private, recipient-issued `ClientKey` **need not be sent on every routine monitoring reconnection**. If the peer's network/source/declared modules change unexpectedly, the recipient requests the private GUID through a separate HTTPS connection-verification step and admits the new connection only after a match. An existing live SignalR connection retains its associated node identity.

This is a **lightweight familiarity heuristic**, not identity proof: headers and IP addresses can be imitated, and NAT/proxy addresses can be shared or change. Installations needing stronger guarantees can challenge each new connection. The SystemMonitor Hub remains outgoing-only; key challenges and any other control exchanges happen outside its application methods. [Adaptive SystemMonitor challenge](../SystemMonitor/en.md).

### Administrator-driven monitoring filters and incident handling

The aggregate monitoring dashboard offers clear independent interactive view filters by source/VPS, module/instance, severity, event type, correlation/job ID and text, with saved presets. A user's display filter never changes which module is trusted or which other monitor clients can see events.

The router checks incoming event validity separately. On a suspicious or malformed event, it **records a durable, safe incident and drops that event** without changing the sender's registration. Only a human administrator can later mute a source in their view, manually revoke/block a registered origin or stop a managed receiver. Repeated incidents can be counted/deduplicated to prevent logging floods. Legitimate new enrollment requests are kept as Pending, not automatically classified as malicious. See [SystemMonitor design](../SystemMonitor/en.md).

## Design-stage constraints

- No folder layout, package split, DI registrations or interface method signatures are final.
- .NET 10-compatible base contracts and optional .NET 11 compile-time API surfaces are being evaluated.
- Default to `System.Text.Json` where serializing JSON is actually required; do not constrain other formats to JSON representations.
- Register all accepted edits through controlled write/audit boundaries, regardless of UI.
- Afutrans is one host. Its English-key rule is not forced on every UnderstandMe consumer.

See [working notes](../../Notes.md), [QueueMaster](../QueueMaster-and-Reliability/en.md) and [open questions](../Open-Questions/en.md).


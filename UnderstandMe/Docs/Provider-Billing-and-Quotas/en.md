# Provider Pricing, Packages, Usage Tracking and Spending Policies

Status: architectural direction agreed on 2026-10-10; not implemented. Names and structures below are conceptual and must be finalized before coding.

## Purpose

Each translation provider must declare its **pricing/metering capabilities** and each configured provider account or endpoint must be able to carry the operator's **actual commercial plan**. Policies for included usage, paid overage and budget caps are configurable, and **only an administrator** authorizes spending beyond the agreed plan or a purchase of an additional package. Billing is independent of best-effort monitoring and of translated text rendering.

Example scenario (illustrative, not a claim about any real provider): a subscription includes **1,000,000 words each month**. Each chargeable translation reduces the remaining allowance. The admin can choose a hard cutoff, permit additional per-word charges under a cap, or pause paid usage until an additional package is manually purchased/registered.

## General provider price model versus account plan

- **ProviderPricingCapabilities** (declared by a provider adapter): supported charging units (source words, source characters, target characters, tokens, requests, pages or other provider-defined units), counting rules or available authoritative usage reports, supported package/quota shapes and whether charged usage can be reconciled with actual provider receipts. These fields belong to the provider's **capability/price description**, alongside supported languages and translation formats.
- **ProviderAccountPlan / EndpointBillingSettings** (configured for each actual subscription/account): currency, plan identifier, included units, billing period and renewal anchor/time zone, current purchased bundles, negotiated rate, overage unit price, enabled/disabled state, and approved policy when the allowance runs out. Do not store one universal Google/Microsoft price as if every subscriber has the same rate.
- **UsageLedger / QuotaAccount** (durable operational state): committed or estimated consumption by account, period and charge unit, current reservations for in-flight dispatches, applicable bundles and accepted overage; separate projected price from reconciled actual charge.
- **RequestBillingPolicy** (part of effective request Settings): permitted provider accounts, whether paid usage is allowed, maximum estimated cost and fallback behavior. System- and account-level hard ceilings cannot be bypassed by an individual request's preferences or `Mandatory` flag.

The provider/endpoint model exposes the price information that routing needs, but **the provider adapter should not own the whole usage accounting ledger**. This keeps the commercial meter, QueueMaster, Translator and billing policy loosely coupled.

## Handling included packages

A model can represent a recurring subscription, prepaid block, fixed usage tier, purchased add-on bundle or another provider-supported shape. Charging units must be explicit. Example:

```text
Account:           paid-translator-A
Plan:              monthly-package
Meter:             SourceWords (according to that provider's counting rules)
Included:          1,000,000 units / billing period
Period:            monthly, provider-specific renewal anchor
Consumed:          740,000 units
Remaining:         260,000 units (less any uncommitted reservations)
Overage:           €0.00002 / additional unit [illustration only]
Action on exhaustion: Stop | PermitOverageWithinCap | AwaitAdminPackage
```

A bundle purchase is an **administrator-controlled operation**. After an admin actually buys a package from the provider, the admin or a trusted provider integration can record/confirm its allowance in billing configuration; the core must not assume that updating a local number has purchased real external capacity. Package expiration, rollover and multiple parallel bundles are modelled only where supported by the external tariff.

## Admission, reservation and settlement

Before submitting a chargeable request to a paid endpoint, calculate its expected metered units and estimated cost. The system checks remaining allowance and hard/overage permissions **at dispatch**, not merely when a job was enqueued. For concurrent workers, reserve expected units against one authoritative shared account ledger **atomically** so two simultaneous jobs cannot both consume the same final quota. If an estimate cannot bound potential charge (e.g. unpredictable token output), use explicit conservative reservations/provider-enforced limits or deny requests under a strict hard cap.

On response, record the provider's actual reported billable units/cost when available and release/adjust the reservation. Otherwise settle using the declared estimator and label the figures **estimated** until reconciled. A timeout or transport failure may still incur a charge; do not assume failed responses are free or automatically refund the reservation. For multiple machines sharing one paid API account, their quota coordination needs **one authoritative shared ledger or reservation service**; merely broadcasting SystemMonitor events is not sufficient.

Accounting writes that protect a spending limit must be durable and idempotent by attempt/account/period where feasible. An engine's result does not become correct or incorrect merely because billing settlement was delayed; expose a separate unsettled/unknown usage state and avoid silent overspending.

## Exhaustion policies: administrator sets, system enforces

- **Hard stop:** never dispatch a new chargeable attempt if it would exceed the allowed remaining units/budget. Route to another configured and eligible endpoint, wait for quota renewal/manual intervention, or return a defined unavailable/source-fallback result according to the request Settings.
- **Explicit overage allowed:** use the provider's configured per-unit price and cap. Track included consumption and estimated/actual overage separately. If the overage cap is exhausted, stop further overage automatically; this does not give the system permission to raise the cap.
- **Await additional package:** stop eligible paid dispatch for that account, surface usage and an admin action to record a legitimately purchased add-on package. No autonomous purchase or unapproved increase in spend.

The admin may change these settings manually. A `RealTime | Mandatory` request can choose another acceptable endpoint **without waiting for its preferred busy backend**, but may not bypass hard usage/price caps or trigger unapproved purchases.

Changes in price/allowance apply to **new dispatch admission checks immediately**, even for a request created under an older Settings snapshot; this is a global operational guard, just like an orchestrator pause. The request's original provider *preference* and behavior flags remain unchanged. Do not silently reinterpret historical queue priority or provider order when an administrator changes pricing. Administrative spend constraints take precedence over accepting new paid charges.

## Orchestrator policy checks and graceful alternative selection

A client cannot force a paid vendor by specifying its name in a translation request. Before QueueMaster accepts the normalized request, the **orchestrator applies current administrator permissions and effective provider ranking**. If Google (or another preferred account) is forbidden or has no approved available quota, the orchestrator looks for another compatible **authorized** provider by default, rather than rejecting an otherwise translatable request. The client is informed when the actual provider differs and receives a structured reason.

An account hard limit is an **absolute constraint on further billable dispatch**, not a command to stop all translation. After the allowed included units are exhausted, no new chargeable call may be sent through that account without prior explicit overage authorization; the orchestrator may use another provider, wait when permitted, or only then report rejection/unavailable with a reason when all eligible routes are exhausted. `Mandatory` and an explicit vendor request do not grant an overage exception. A previously accepted job still has its account's remaining quota checked **at each chargeable dispatch**, and may require a different eligible endpoint when the current hard cap is reached. Never silently charge for an alternative paid vendor if request/account policy disallows spending.

Billing and engine selection must share one coherent eligibility decision, without giving QueueMaster responsibility for the cost model. Admission responses identify policy substitutions; completion/segment results identify the **actual** provider and charge estimate/receipt. See [Request Admission and Normalization](../Request-Admission-and-Normalization/en.md).

## Cost reporting and monitoring

The administrator should be able to see per billing account/endpoint and period:

- included quota, consumed/estimated units, reserved units, remaining quota, renewal timing and purchased add-on packages;
- estimated versus reconciled actual costs, currency, overage price and authorized overage cap;
- usage by Translation Target, provider, language pair and optionally request/job when reliable metering supports it;
- notifications as thresholds approach or a request is rejected for lack of permitted allowance.

Persist billing evidence outside SignalR. A typed `ProviderQuotaChanged` or `ProviderBudgetThresholdReached` event may be broadcast to **SystemMonitor**, but live monitoring is best-effort and cannot be the billing ledger. Never include credentials, raw translated source text or a payment instrument in these events.

## Non-goals and open details

This is **not** a full payment system, automatic webshop or invoice reconciliation product. Do not assume provider prices are always public or static. Pricing may change independently of model version. Remaining specifics to agree on: precise `Words` counting per provider, billing period time zones and renewal, reservation policy if actual consumption exceeds estimate, cancellation/retry/partial batch charges, package rollover/stacking, currency exchange (when displaying consolidated multi-currency totals), history retention, shared-account multi-server ledger, and exact admin UI flows.

Tests: quota boundary with parallel requests, no negative remaining in hard mode, automatic rejection of unapproved overage, manually authorized overage calculation and cap, package extension only after approval, renewal timing, timeout with unknown charge, retry idempotency, global hard budget overriding request flags, failover to free endpoint, and accurate estimated/actual labeling.

Related: [Provider Routing](../Provider-Routing/en.md), [Request Settings and Live Updates](../Request-Settings-and-Live-Translation/en.md), [QueueMaster](../QueueMaster-and-Reliability/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).

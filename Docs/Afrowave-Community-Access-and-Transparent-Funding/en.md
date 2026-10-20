# Afrowave Community: Free-First Service and Fully Transparent Operating Funds

**Status: confirmed guiding principles for future hosted services, 2026-10-20. Documentation only.** Canonical detailed Afutrans allocation/ledger design: [Afutrans Community-First No-Profit and Transparent Funding](https://github.com/afrowaveltd/Afutrans/blob/main/Docs/Community-First-No-Profit-and-Transparent-Funding/en.md). These values apply to the wider Afrowave community, not as a mandatory dependency or billing package inside open .NET Essentials/WhenItFails/UnderstandMe libraries.

## Community mission and equality

- Afrowave should try to provide **everything practically possible free of charge**, from the first day and as a permanent design goal. No artificially restricted quota or credit mechanism built merely to manufacture scarcity and drive upgrades. A person who cannot pay should receive the **same rights within CommunityFree** as someone able to donate money.
- No personal enrichment from voluntary community funding, no secret premium margin, no payment to get ahead of others on a shared community worker. Operator may protect necessary first-party identity/realtime capacity; paid dedicated resources must be separately funded or a customer's own and **must not take reserved free community resources**.
- No obligatory donation, registration of a paid plan, AI subscription or live Afrowave Id/Gateway connection for ordinary local/offline UnderstandMe. Each host still decides which public methods to expose, and privileged admin/target operations must remain authorized.
- Prefer **fair slowing, transparent queue backlog and durable deferred requests** over monthly translation-credit exhaustion. Avoid artificial per-user lifetime/period count limits as default *or desired final model*. But storage, real compute, abuse protection, security isolation and paid-provider hard budgets have finite constraints; enforce truthful backpressure to prevent losing accepted jobs or bankrupting the operation. Such measures are operational safety, not monetization.
- People may voluntarily give computing resources (e.g., donated AI or LibreTranslate worker capacity) as well as money, with distinct owner/content privacy permissions. Surplus realtime workers may help the community **nonguaranteedly** while protected first-party service remains ready.

## All contributed funds must become accounted service value

- Donations are strictly **optional** and go to actual documented community service costs: one-time top-up of paid AI/token/provider capacity; additional VPS/LibreTranslate instances; domain/network/storage/backups/monitoring/hosting and other necessary service operations. Donors may voluntarily earmark funds to a particular purpose, or contribute to a disclosed general community operations fund.
- A dedicated managed VPS/service rented for an actual documented hosting cost is a distinct **expense-backed service choice**, not paid privilege over CommunityFree. An application/customer connecting its own LibreTranslate, Google/Microsoft subscription or Ollama/AI backend owes no Afrowave VPS rental simply for using its own engine.
- Never imply that processing fees, taxes, FX differences or unpaid/pending invoices are already hardware capacity. Public reporting should show them transparently; **zero hidden profit and no personal withdrawals**. Unspent earmarked funds remain explicitly recorded as balances, not booked as costs or taken as private earnings.

## A transparent public and private ledger, reconciled to the last cent

Track contributions and costs in the proper currency with precise minor units/decimal accounting; stable IDs and audit history. Distinguish:

1. Amount actually received (gross and net) and external payment fees, tax, FX and any refunds.
2. Donor-designated purpose or operator-approved general community cost center and allocated/reserved money.
3. Actual provider payment/VPS invoice and timestamp versus merely planned expenditure.
4. Real provisioned compute capacity (deployed VPS active period, newly available worker, verified credited AI token balance) versus a purchase request still pending.
5. Any unspent, purpose-restricted and general balance or unused prepaid provider capacity (credits purchased are not the same as tokens already consumed).

Public dashboard must list per-period receipts, expenses, balances, service costs and meaningful capacity delivered; show privacy-safe redacted source evidence. Each contributor can privately see what happened to their own amount, deductions, purpose, allocation, paid purchases and still-unspent balance. Append correction/reversal entries instead of silently altering past entries. Public accounting is **not** permission to disclose donor identities, payer details, API tokens, customer texts or private queue data.

Example conservation check in each currency (with explicitly evidenced currency exchange when needed): `opening cash + accepted receipts - actual fees/taxes - actual paid expenses - refunds +/- audited adjustments = closing cash`, while prepaid credits and already committed obligations are separately tracked. If totals cannot be reconciled, display the discrepancy for review rather than claim 100% was spent correctly.

## Communication instead of monetization pressure

Explain what is free, how shared capacity is allocated, why there can be a wait, what the next processing state is and how voluntary donations or resource sharing **actually** improve services. Do not use artificial counters, manipulative urgency, a false unlimited promise or a surprise paywall. Donated VPS/AI credits are shown as actual usable capacity when provisioned; a contribution alone does not prove extra capacity already exists.

## Architectural separation and future checks

- Id optionally verifies app/service/customer identity and grants; it must not become the payment processor or secret store.
- Generic Gateway optionally authorizes and forwards external requests; it does not unilaterally determine target storage access.
- Afutrans manages permitted work, user/resource offers and host policy; UnderstandMe remains the independent library with real P0/P1–P4, QueueMaster and private dictionaryless startup.
- A future public finance/operating-cost dashboard and community donation ledger are distinct optional application/backend capabilities, not prerequisite packages for core .NET tools.
- Tests must verify: no donor jumps ahead in CommunityFree; no arbitrary exhausted-credit paywall; honest overload response; contribution purpose restriction; full cent-precise revenue/fee/invoice/balance reconciliation and private donor receipt; real provider credits/VPS provisioning; zero personal withdrawals and protection of identities/provider keys.

**Implementation status: planned product ethics and architecture only.** No collection of donations, real payment/accounting ledger, financial evidence, live dashboard, VPS procurement or tests created by this documentation change.

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

## Multi-purpose contributions, accumulated monthly reserves and implementation timing (2026-10-20)

Afrowave contributors should be free to **split a single contribution into several earmarked purposes**, such as **USD 10 gross, USD 5 for AI provider credits and USD 5 for server costs**. Proposed accounting binds each purpose line to the original receipt, records agreed treatment of payment-processing/FX costs upfront, tracks actual expense/provisioned benefit and leaves any unused amount in its original restricted purpose pool. Never silently reduce a promised net allocation or reassign a reserve.

**Accumulating funds is a legitimate outcome.** If several donors collectively contribute more toward community VPS running costs than the present month's invoices require, **carry the remainder forward for later months**. Show distinct actual monthly costs, paid periods, future-month restricted reserve/commitments and unspent cash; do not book unspent funds as purchases or claim a planned future server is already operating. Permit pooling within the same donor-approved purpose, with privacy-preserving public aggregates and donor-specific accurate receipts.

**When to build:** record this as future product governance **only**. The user expressly postpones any financial watchdog, donation/payment page, bookkeeping implementation or solicitation of contributions **until useful Afrowave services actually operate and there is a clear practical reason to fund them**. UnderstandMe, other Toolbox libraries and their offline operation remain independent of finance modules. Canonical details: [Afutrans funding policy](https://github.com/afrowaveltd/Afutrans/blob/main/Docs/Community-First-No-Profit-and-Transparent-Funding/en.md). **Documentation, not functionality.**

## Public financial clarity: exact total charges without invoice-style tax tables (2026-10-20)

**Current community reporting decision:** public transparency must be genuinely understandable at a glance. Show: gross donated sum; **the actual total payment/transfer fee as one amount** if charged; net sum available; donor-designated AI/server/general purposes; actual **tax-inclusive whole invoice price** per funded VPS/provider/service; future-month reserved and other unspent balances. Explain what service or provider credit was actually delivered. This should communicate **exact cost**, not every line of underlying VAT/tax/FX bookkeeping.

**Illustrative, not pricing:** Received USD 10.00, transfer fee USD 0.40, net funds USD 9.60, split into AI USD 4.80 and server USD 4.80 under a previously disclosed proportional fee policy. Other explicit fee-payer treatments may preserve a net USD 5 + 5 split. A server whose total invoice is USD 6.00 appears publicly as **'VPS operation — USD 6.00 (total paid, including tax)'**, not many technical VAT/rate line items.

**Under the hood:** preserve complete transaction, legally required tax/FX and invoice evidence for exact reconciliation and compliance. Public summaries and any donor-specific receipt are merely a clean view of the same verifiable ledger. Where the user wants additional evidence, make the suitably redacted original invoice accessible by choice; do not force a cryptic electricity-bill-style breakdown as the main interface. Never conceal a *real* separate transfer charge or make the published total disagree with cash/provider statements.

**Implementation timing remains deferred:** no funding page, payment gateway or financial watchdog until live useful community services and a meaningful reason to accept donations. Detailed proposal: [Afutrans public funding charter](https://github.com/afrowaveltd/Afutrans/blob/main/Docs/Community-First-No-Profit-and-Transparent-Funding/en.md). **Documentation only.**

## Planned directly verifiable bank-sourced public financial feed (2026-10-20)

Once real useful Afrowave services justify voluntary contributions, the owner intends to seek a **dedicated bank account with an officially authorized public transaction/balance API or transparent-account feed**. The planned public community page will show actual bank-source **incoming transfers, outgoing transfers and current account balance**, independently verifiable where the bank offers public source links, alongside simple Afrowave accounting and a visible timestamp of the last successfully fetched bank state.

An API feed is not intrinsically instant: distinguish last successful sync and booked/pending payment status according to the bank's actual semantics. An authorized **read-only server-side mirror**, using webhooks or bounded polling, must not expose bank secrets or any payment initiation function. Treat 'public transparent bank account' and 'PSD2/Open Banking access to a private account' as different products/permissions; do not assume a private account's owner credentials can be made publicly available.

The bank feed documents transfers/balance, while the separate purpose ledger documents donor-selected splits, unspent future-month server reserves, fees, invoices and actually provisioned AI/VPS value. Reconcile independently, show any discrepancy honestly, and do not confuse bank expenditure with proven service installation or unused provider token credits. Maintain **simple money in / money out / current bank balance** public UX instead of exposing VAT-tax tables.

**Privacy requirement:** evaluate whether the selected bank's public product reveals transfer counterparty names, numbers, references and payer-authored messages; inform users *before* they donate what becomes publicly visible from the bank itself. Publicly show transaction amounts/balances without automatically leaking private donor details or falsely claiming that application-side redaction can hide information already disclosed by the bank's transparent service.

**Timing unchanged:** this is **only an optional future bank-selection criterion**. Do not implement financial monitoring, banking integration, public payments or fundraising until useful running services create a clear need for donations. Detailed service design: [Afutrans public bank-backed transparency](https://github.com/afrowaveltd/Afutrans/blob/main/Docs/Community-First-No-Profit-and-Transparent-Funding/en.md).


# Afrowave Id: replicated identity authority, fault tolerance and trust boundaries

**Status: architecture requirements / proposal, 2026-10-20. Not implemented or operationally tested.** Context: [Id rebuild review](../Id-Rebuild-Review/en.md), historic [`AfrowaveWebPublic/Id`](https://github.com/afrowaveltd/AfrowaveWebPublic/tree/master/Id). This is an availability/security design record, not a commitment that a deployed Id cluster already exists.

## Why Id is a critical service

Id is the authoritative **logical** identity/credential, first-party authentication, application/brand registry, system enforcement and service-entitlement service for Afrowave. Applications retain **authority for their own internal application roles, content and business data**; Id does not take ownership of these to become an all-powerful tenant database.

Even temporary unavailability of Id could prevent **new login, account recovery, app/service registration, MFA/revocation checks and grant changes**. Existing consumer applications should be able to keep serving authorized, already-established local sessions within their explicit expiry and revocation-freshness rules rather than synchronously contacting Id for every page. This is bounded continuity, **not** ignoring revoked credentials indefinitely.

## Minimum replicated deployment

- **At least two independent stateless (or externally-stateful) Id application/API instances** on separate servers/failure domains. Put health-checked ingress / reverse proxy / load balancer in front; ingress itself must not be a single unrecoverable machine. DNS/service discovery and certificates should support node replacement and routing without user intervention.
- **Replicated durable identity database** on different machines with an authoritative write leader (or a rigorously specified consensus-based database). A simple pair of writable independent database copies is unsafe: a network partition can produce split-brain identities, role assignments, revoked access and tokens.
- **A third independent quorum/fencing witness or voter** is the recommended minimum to discriminate leader loss versus network partition when employing a consensus-driven automatic failover mechanism. A two-node voting system without a third vote cannot safely promise both continued writes after one node's failure **and** no split brain. Do not falsely market two synchronized SQL servers as automatic HA.
- Preferred production layout: application A and database A in failure domain/site A, application B and database B in failure domain/site B, plus **independent witness/quorum** in failure domain/site C or equivalent arrangement with tested database-orchestrator fencing. Witness placement/network guarantees matter. A 3-voter quorum is **not** a guarantee of full site resilience if critical services, ingress, power, or quorum voters share a failure domain.
- Database technology/driver determines actual options. Choose explicit replication/failover semantics, transaction guarantees, fencing, write admission and leadership checks for the chosen SQL engine; do not introduce an ad-hoc in-process bidirectional data sync that can lose accepted credentials.

### Consistency over divergent identity writes

- Critical mutations—password/MFA secret change, account locks, session/token revocation, system privileges, brand/application ownership and service grants—must have **one authoritative ordered commit history**. When a partition removes write quorum, reject/queue affected changes with transparent status; never let two isolated leaders both commit.
- Set and test clear **RPO/RTO objectives** for critical writes and for ordinary reads/sign-ins. Prefer synchronous/quorum acknowledgement within the primary write group where zero acknowledged-data-loss is required; cross-region asynchronous recovery copies may have nonzero RPO and must not independently accept writes until formally promoted/fenced.
- A worldwide footprint may later use local edge/replica **read-only** directory/JWKS/cache services and regionally distributed API endpoints, but new authoritative writes must obey the same globally agreed consistency model. A third country improves geographic redundancy but does not remove latency/partition trade-offs.
- Enforce unique username/identity/app identifiers and single reserved `IsAuthenticator` via durable constraints and transactional bootstrap; retries after failover must not create two owner accounts, two Id authorities or duplicated privileges.

## Session, key and cache resilience

- All Id nodes must share a **consistent protected cryptographic key lifecycle**: signing/verification keys, key IDs (`kid`), rotation windows, distributed data-protection keys and revocation status. Keys/credentials belong in replicated restricted secret/key storage, **not** hardcoded per-node or shared via ordinary public app databases.
- Authorize tokens by issuer, audience, expiry, signature and applicable revocation policy. Clients may use cached, time-bounded verified Id public keys and local sessions; sensitive control-plane actions can require a fresher check than an ordinary page. Short token lifetimes and deliberate cache TTLs bound the effect of Id outage and later revocation.
- Session/cookie state must be portable across nodes through securely shared data-protection keys and, where required, an appropriate resilient state/session store; Id's first-party auth cookie remains Id-scoped. Registered applications **own their own first-party session cookies**; do not forward Id cookies or credentials to unrelated app domains.
- App/target readiness should not depend on Afutrans or a particular online translation provider. Id has validated local UI dictionaries and a complete language selector available before serving authentication pages.

## Not just replication: actual recovery

- Keep **encrypted off-cluster backups** with tested point-in-time recovery, retention and restoration credentials separate from live replica credentials. Replication faithfully reproduces accidental deletion or compromised administrative writes; **it is not a backup**.
- Rehearse failure modes: loss of one app node, database leader, witness, network partition, entire site, ingress/DNS, key store, expired TLS certificate, unavailable upstream identity integrations (there are no required external login providers), and concurrent installer/privilege changes.
- Operate minimally scoped monitoring/alerts on authentication readiness, replication lag, quorum health, request errors, signed-key rotation, recovery objectives, clocks/time synchronization, capacity, failed backups and credential abuse. No behavioral profiling or third-party tracking is needed.
- Use separately scoped automated deployment and emergency/manual recovery roles, two-person or controlled break-glass procedures for high-impact changes, immutable or separately protected audit of privileged operations, and regular software/security maintenance.
- Publish truthful service health and communicate outage progress without leaking personal data or technical secrets.

## Tenant-controlled insecure connectivity without compromising Id

- Registered application owners may publish their own demo website over HTTP or choose insecure classic FTP/HTTP target connectors when the selected Afutrans deployment explicitly supports those **owner-opted-in** modes. Show explicit warnings; do not police their own private infrastructure by forbidding every risky protocol.
- The hosted service and Id **cannot inherit** that insecurity. Run untrusted connector jobs in isolated, quota-limited environments with target-only credentials and egress controls; avoid SSRF, remote code execution and cross-tenant data leakage. Id should never store connected tenants' FTP passwords/SQL connection strings merely for service registration.
- **Central Id login/authorization responses remain encrypted.** OAuth 2.0 Security BCP [RFC 9700](https://www.rfc-editor.org/rfc/rfc9700) states that authorization responses MUST NOT use unencrypted network connections; non-loopback internet HTTP redirect URIs for this purpose are forbidden. A public plain-HTTP static/demo site may be listed but cannot receive Id authorization codes/tokens through unencrypted callbacks; it can be upgraded to HTTPS for SSO or run without that sign-in capability. Local native-loopback exceptions are protocol-defined separately.
- Choice of application transport security does not grant rights to use other tenants' resources or access Id's internal control plane. Isolate credentials, network, signing keys and administrative APIs; realistic isolation reduces but cannot guarantee zero compromise risk.

## Operational acceptance tests and readiness gates (future)

1. Stop Id API node A: API node B serves sign-in and already-authorized client operations through the healthy ingress; no duplicate login or loss of stored MFA.
2. Stop the database write leader: the explicitly chosen database HA layer elects/fences one valid leader only with quorum; acknowledged writes meet configured RPO. No two Ids independently issue conflicting mutations.
3. Split the network between two database replicas: the minority partition cannot write or mint new authoritative privileges; dependent API calls fail clearly until leadership/quorum is restored.
4. Kill the witness too: document the exact reduced-availability behavior; **no promise** that every two-out-of-three failure maintains writable service.
5. Rotate signing keys during/after leader failover: all surviving nodes verify compatible sessions until policy expiry; emergency revocation rules propagate with bounded delay.
6. Bring down every Id endpoint: already authorized application-owned sessions continue only under configured expiry and revocation freshness; **new central sign-in cannot succeed while all authority is down**.
7. Simulate accidental deletion/credential compromise: restore from independent tested backup rather than trusting replicas.
8. Developer publishes a plain HTTP public demo: directory discovery works, but unsafe Id credential/token callbacks are rejected; FTP connector compromise cannot access other targets, key material or Id metadata.
9. Bootstrap interrupted mid-admin/app/role creation retries exactly once with a reserved Id app and no unintended system grants.

## Next design decisions

Select the initial Id database implementation and clustering/orchestration stack, independent hosting/failure domains, automated routing/fencing mechanism, backup strategy, operational RPO/RTO/SLO targets, revocation freshness/cache intervals, key custody and monitoring. Do not claim zero downtime before failover drills and region-loss exercises prove the chosen architecture.

**Implementation status:** no Id replica, failover orchestration, distributed key store, off-cluster backup or compatibility tests were added with this document. Work remains design-stage.

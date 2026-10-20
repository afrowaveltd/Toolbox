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

## Preferred-primary Id1/Id2, SignalR Uplink, rejoin and SMTP alerts (2026-10-20)

### Preserve the author's original design

- **Id1** is the preferred normal primary; **Id2** is a redundant synchronized server, with room for more. A distinct always-connected **SignalR Uplink Hub** reports node health, cluster status, sync/rejoin requests and committed-change positions. Each Id node integrates with a separately registered SMTP service application to send problem notifications.
- Once Id1 genuinely fails, an authorized and sufficiently current Id2 takes over and durably commits further changes. A returning node must contact/discover the **current legitimate leader**, reconcile, validate and only then advertise healthy public service endpoints. If Id2 was promoted, recovering Id1 must initially rejoin **as its follower**.

### Preferred primary is not a permanently authorized master

| Provisional setting | Meaning |
| --- | --- |
| `NodeId` | Persistent identity for this Id installation/node |
| `PreferredPrimary` (author's `IsMaster`) | Desired normal primary; **configuration preference, not authority to write** |
| `LeaderId`, `LeaderEpoch` | Quorum-authorized leader and monotonic generation; stale leadership cannot perform privileged writes |
| `Role` | Observed dynamic `Recovering`, `Follower`, `Leader`, `Unavailable` |
| `ReplicationPosition` | Database replication offset/transaction-log position used for catch-up verification |
| `UplinkConnected` | Informational peer-connectivity state, **not** proof that peer is dead or alive |
| `ReadReady`, `WriteReady`, `PublicReady` | Separate health/readiness gates for API ingress |

After Id2 becomes a legitimate leader, Id1's preferred-primary flag **must not** force an immediate takeover. A planned, verified **switchover** can return leadership to Id1 later, once caught up and ready, with a new epoch/fencing. Frequent automatic failbacks risk repeated service interruption.

### Separate SignalR control plane from authoritative replication

- Keep SignalR Uplink: authenticated bidirectional heartbeats, topology/health, `LeaderChanged`, `ReplicationProgress`, `SyncRequested`, `SyncCompleted`, `NodeReady`, and change-available notifications. Reconnect/replay/deduplication is required. Hub commands never grant themselves leader status.
- **Do not elect a leader merely because the other node's SignalR uplink disconnects.** Both Ids can remain reachable by public clients despite their mutual network split. An independent quorum/leader lease and fencing mechanism must authorize only one writer; the minority rejects high-impact mutations rather than creating a second identity history.
- Use a proven database **transaction-log replication mechanism** for identity, MFA, sessions/revocation and app/service-grant records. SignalR event messages do not guarantee durable ordering or delivery and must not be the only record of changes.
- For ancillary node-local projections/settings that require special sync, use durable revisions or a transactional outbox with monotonic sequence, idempotent replay and snapshot fallback. Acknowledged writes are committed to an explicit authoritative data history, never a loose pair of local JSON stores.

### Two-node plus witness limitations

- A suggested minimum cluster has **two independent Id/data servers and a third independent quorum/fencing voter**. A voter is not a third copy of the identity database. Choose a mature HA design appropriate for the selected database. PostgreSQL with Patroni and a suitable 3-voter DCS is an example, not a decided implementation.
- If Id1 is lost and Id2 holds quorum and an eligible up-to-date replica, it can receive a new leader epoch and serve writes. If the partitioned Id1 stays alive, it must be fenced and stop accepting authoritative writes.
- **Durability and availability must be negotiated explicitly:** with only two data-bearing nodes, synchronous confirmation to the replica can stop writes when Id2 is unavailable; permitting asynchronous Id1 writes during that outage risks losing recently acknowledged changes if Id1 then dies. A third voting-only witness does not store those changes. If zero acknowledged-write loss plus continued write availability on loss of one data replica is required, evaluate **three data-bearing nodes or equivalent durable data quorum**, not only two plus witness.
- Document service RPO, RTO and tolerated replica lag. Never claim perfect write availability and zero loss under every failure; use a temporary read-only/error state rather than split-brain corruption when quorum is unavailable.
- Patroni references: https://github.com/patroni/patroni/blob/master/docs/faq.rst ; https://patroni.readthedocs.io/en/latest/ha_multi_dc.html ; https://patroni.readthedocs.io/en/latest/replication_modes.html .

### Rejoin state machine (proposed)

1. `Bootstrapping`: process starts non-public; load protected node identity and trusted cluster membership.
2. `DiscoverLeader`: use Uplink where available, but **verify** authoritative leader identity/epoch through quorum rather than static Id1 preference.
3. `CatchUp`: replay trusted database replication log to current position, or use controlled snapshot/rewind when logs were lost or timelines diverged. No independent writes while catching up.
4. `Validate`: check consistency, schema, signing keys, revocation status freshness, DB position and local settings.
5. `FollowerReady`: only safe allowed read/auth endpoints are exposed as determined by policy; writes route to current leader and no stale new privileges are minted.
6. `LeaderReady`: requires actual authorized promotion, fencing, current durable state and key readiness. Ingress sends write traffic only here.
7. `Degraded` / `ReadOnly`: no quorum or unverified leader; cease authoritative writes and report explicit status.

The original idea that Id2 must contact Id1 before it serves clients becomes a stronger invariant: **a rejoining Id node must synchronize with the _currently authorized leader_ before it advertises readiness**. Id1 might no longer be that leader. Never discard legitimate post-failover Id2 changes to restore Id1's older snapshot.

### SMTP and monitoring

- Use the planned Afrowave SMTP **service application** to deliver node/cluster failure alerts. Queue incidents in an outbox, limit and deduplicate retries; **mail availability never blocks Id login, readiness, elections or transactions**.
- A process that dies cannot send its own failure email. Peers should report suspected failure, but a lightweight **independent external monitor** must check cluster ingress, quorum and SMTP failure so a common-mode outage is visible.
- Alert on `UplinkLost`, `ReplicaLag`, `QuorumLost`, `LeaderChanged`, `Failover`, `RejoinFailed`, `SigningKeyUnavailable`, `CertificateExpiry` and `IngressUnhealthy`. Notifications are not a consensus vote.

### Future fault-injection tests

- Id1 dies -> quorum-authorized Id2 takes over, Id1 returns as follower then optionally planned switchover to preferred primary.
- Uplink is cut but both instances receive client requests -> at most one writer; minority cannot independently create users or system roles.
- Id2 stops while Id1 runs -> synchronous strict vs asynchronous/nonzero-RPO operational behavior is transparent and tested.
- Id1 returns with a divergent history or missing transaction logs -> controlled rewind/full resync and validation before readiness.
- Reordered/duplicate/lost SignalR events -> durable source-of-truth replication or outbox recovers idempotently.
- SMTP service fails or both Id services die -> no login failure caused by alerts; external monitor still detects the problem.

**Status: architecture documentation only.** No new Id Uplink Hub, HA manager, SMTP alert worker, data replica or tests implemented here. Do not infer that the historic Id repo already contains production-ready replication.

## Observer: independent supervisor, optional SignalR relay and quorum witness (2026-10-20)

**Author clarification:** add a separately hosted `Observer` service as the third cluster participant and a broader Afrowave service health receiver (`IAmHere` heartbeats). Observer is potentially both a **third voter/witness** and **SignalR Uplink forwarder** when Id1 and Id2 cannot communicate directly but can both reach Observer. These are independent responsibilities with distinct authentication/permissions.

- **Id1–Id2 link fails, Observer sees both:** relay control/status where useful and prefer keeping the present authorized leader. The relay may provide an alternate control route but SignalR does not replace transaction-log replication.
- **Id1–Observer link fails, Id1–Id2 remain connected:** two of three true voters can maintain quorum; report Observer degradation.
- **Id1 loses Id2 and Observer while Id2+Observer remain connected:** minority Id1 must be fenced from writes; Id2+Observer may elect/preserve a leader under the selected quorum stack and durability policy.
- **Observer fails, Id1+Id2 remain linked:** no obligatory Observer path for normal service; safe cluster quorum may remain available. A third observer alone stores no identity data.
- Every ordinary app can report liveness but must **not** automatically become an Id election voter. Observer cannot access/edit passwords, Id-managed privileges or tenant content simply because it monitors the platform; active probes and independent outside-cluster monitoring remain necessary.

The full failure matrix, model contracts, trust boundaries and test plan are in [Id Observer and Uplink](../Id-Observer-and-Uplink/en.md). **Documented design; no runtime implementation or tests.**

## Three-state external health and multi-Id join procedure (2026-10-20)

**User-requested public status vocabulary:** `Ready`, `Degraded`, `Fail`. Keep `Leader`/`Follower` role, quorum lease/epoch, write authority and replication state separate. A `Ready` follower is not independently authorized to commit identities; `Degraded` covers incomplete network/catch-up/quorum/maintenance and may offer safely scoped reads; `Fail` covers unavailable critical functionality (with reason `Unreachable` when health probes only know that a node is not accessible).

Id2 should be able to accept **full writes when Id1 fails**, provided Id2 has the elected leader lease, trusted key material, suitable replica freshness and a data replication/durability policy that actually permits commits. Observer's status change **does not itself revoke an isolated Id1's writes**; the database leader fencing, consensus epoch and lease expiration must enforce that independently. Losing just one peer link when an authorized quorum remains available should report connectivity degradation, not automatically demote a healthy primary.

More than two Id nodes are supported conceptually: securely register a node, bootstrap/catch up as nonwritable `Degraded`, validate replica and key versions, become `Ready` follower, then allow leadership when eligible. Voting membership is independent of number of API/replica nodes: majority `floor(N/2)+1`, and adding a 4th voter to a 3-voter group does not improve single-failure tolerance (3/4 votes required). Preserve quorum during controlled sequential reconfiguration. See [Observer and Uplink](../Id-Observer-and-Uplink/en.md) for state matrix and acceptance tests. **Design only.**


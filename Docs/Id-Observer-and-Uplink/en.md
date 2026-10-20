# Afrowave Observer: Service Supervision, Relay and Id Cluster Witness

**Status: architecture proposal following author clarification (2026-10-20); no implementation.** The original idea is a separately deployed Observer service to which Afrowave services can report availability, with a special role in coordinating the critical Id1/Id2 cluster. This supplements [Id HA and replication](../Id-High-Availability-and-Replication/en.md), not replaces its consistency requirements.

## Responsibilities must remain distinct

1. **Supervisor:** registered Afrowave services/instances send minimal authenticated `IAmHere`/heartbeat and status snapshots. Observer checks last-seen, independent probes and declared dependencies, reports degradation and requests alerts via the optional SMTP service. No personal content, private dictionary data or browsing analytics belongs in a heartbeat.
2. **Uplink relay:** if Id1 and Id2 cannot communicate directly but both can reach Observer, a dedicated authenticated SignalR/Uplink relay can forward *allowlisted*, addressed, bounded, versioned control messages, status and committed-revision notifications. The relay cannot fabricate or edit a cluster vote, identity mutation or signature. Prefer end-to-end mutual authenticated message envelopes, replay/idempotency checks, rate/bandwidth caps, loop prevention and traceable hop IDs.
3. **Cluster witness:** Observer may run or host a properly isolated voter/witness for the Id cluster's **real consensus/lease/fencing subsystem**. Merely observing `IAmHere`, or trusting a SignalR `Connected` flag, does not grant leadership. Use an established quorum implementation such as etcd/Patroni where suitable, without inventing consensus over a custom Hub protocol.

These may be hosted together initially, but must expose separate contracts/permissions and not share a catch-all administrative principal. `Observer` is a coordination authority **only in the specified cluster mechanism**: it does *not* supersede Id for users, passwords, permissions, brand ownership, applications or private content.

## Network topology and failure matrix

Logical topology (bidirectional paths, not assuming every pair can always communicate):

```text
             Observer (separate failure domain)
              /                     \
             /                       \
          Id1 <---- direct Uplink ----> Id2
    Preferred Primary                  Standby / elected leader
```

| Reachability/failure | Supervision/relay outcome | Safety rule |
| --- | --- | --- |
| Id1 and Id2 not directly reachable; **both reach Observer** | Observer can relay control/Uplink events or provide a configured alternate network route; alert on the direct-link fault | Keep the currently valid leader if lease/quorum is still maintained; **do not elect just because direct Uplink failed** |
| Id1 and Id2 communicate; **Observer unreachable** | Peers report degradation and keep direct Uplink; alternate relay unavailable | Id1+Id2 can form **2 of 3** voter quorum if the consensus system actually sees their votes; data replication/durability rules still apply |
| Id1 loses **both** Id2 and Observer, while Id2+Observer communicate | Id2+Observer have a majority of voter connectivity | Old Id1 must lose/stop write authority through fencing; Id2 may promote only after freshness and quorum checks |
| Id2 unavailable; Id1+Observer healthy | Id1 can keep or acquire voter majority; issue alert about missing standby | **Two data-bearing replicas with witness only** may have blocked strict synchronous writes or nonzero async RPO; do not promise both continuous writes and zero data loss |
| Observer sees **neither** Id | May be Observer or shared network outage; Observer cannot reliably diagnose root cause alone | Out-of-cluster monitor must probe independent routes; avoid self-certifying health |
| Observer sees Id1 only and Id2 sees clients but not Observer/Id1 | The minority Id2 must not claim leadership from client reachability | Prefer read-only/fail-closed over conflicting user/MFA/role grants |

A third voter **must not become a mandatory hub for normal traffic**: direct Id1–Id2 Uplink, direct service-to-Id access and replicated database mechanisms should remain available without the Observer relay. Otherwise Observer becomes a single point of failure.

## Relay is not automatic database replication

- Business or cluster-control SignalR forwarding carries small signed/authenticated envelopes (`FromNode`, `ToNode`, `MessageId`, `LeaderEpoch`, `Sequence`, `Type`, bounded `Payload`, TTL). The receiver verifies source and epoch, deduplicates IDs, rejects unknown/unauthorized commands and fetches durable state by revision when needed.
- Identity changes, passwords, MFA, app grants and token revocations are **authoritative transactional DB writes**. The database uses its own robust replication/WAL/commit semantics; neither SignalR nor the Observer's memory queue is an identity source of truth.
- If a database replication channel also needs an alternate route because the direct private network breaks, use a properly authenticated encrypted network overlay/tunnel/proxy suited to continuous database traffic. **Forwarding Hub method calls is not a transparent TCP/database transport.**
- Observer relay must be resource-limited and resilient to reconnect, duplicate/out-of-order delivery, flooding and misconfigured app peers. A crashed relay does not roll back committed Id data.

## Simple service 'I am here' registration without making every app a voter

- Each app instance authenticates with a registered **service/instance identity** and sends minimum state: `ServiceId`, `InstanceId`, current health/readiness, revision/version if applicable, and a heartbeat time. Receive timestamp should be assigned by the Observer; avoid trusting client wall clocks as absolute proof of liveness.
- Optional metadata includes self-declared capability, region/failure domain and safe health check URL, always validated against registration policy. A heartbeat is a **hint**, not proof of availability to every real user: use active probes/ingress tests and independent monitoring.
- **Only specifically enrolled Id-cluster members/witness processes get an election vote.** Arbitrary community applications reporting availability must never increase the quorum or influence Id's leadership.
- Observer may supervise the SMTP service itself. Incident messages use a durable, bounded outbox and deduplication; an outside-cluster health monitor is needed to detect complete Observer or Id outages.
- Observer should not acquire owners' service credentials or require all apps to upload their configuration and user data. Keep an explicit minimal retention window for heartbeat history.

## Readiness, recovery and leader preference

- When a previously isolated Id node reconnects (directly or relayed), it contacts/verifies the **current authorized leader**, not automatically `Id1` regardless of elections; verifies current epoch and catch-up position before serving as a normal public Id endpoint.
- Preferred Id1 remains a *placement priority*, not a permanent writable master. If Id2 was promoted, Id1 initially rejoins as follower. An optional planned switchover can restore preference after safe catch-up.
- A node that loses quorum or a required fencing lease cannot persist authoritative mutations, even if its HTTP endpoint still responds or Observer has a different view of heartbeat health. HTTP public readiness vs read-only readiness vs write readiness are separate signals.
- A relay path can heal a network-control partition and spare a needless failover. It is not a substitute for a database leader lease, and the fact that Observer sees both peers does not mean the peers can directly see or replicate data to one another.

## Contract candidates and tests (not frozen APIs)

Names are illustrative: `IServiceHealthReporter`, `IObserverHealthRegistry`, `IUplinkRelay`, `IClusterLeadershipReader`, `IClusterWitnessAdapter`, `IIncidentPublisher`. Reuse Messenger/SystemMonitor transports only where ownership/security semantics fit; Id privileged cluster control is **not** an anonymous GUID-only SystemMonitor stream. Keep distinct contracts for read health, relay messaging and quorum decisions.

Future tests:

1. Break Id1–Id2 direct route but leave both Observer links working: control messages relay with mutual authentication and deduplication; legitimate leader remains leader.
2. Kill Observer while Id1–Id2 link survives: cluster quorum permits safe continued operation if actual voting subsystem stays healthy, while outside monitor alerts.
3. Isolate Id1 from both; Id2 and Observer connected: only Id2 can regain leadership through quorum/fencing and adequate replicated data, Id1 cannot write conflicting records even if reachable to public clients.
4. Reconnect a formerly preferred Id1 after Id2 failover: catch-up before public readiness; no automatic overwrite of Id2's committed work.
5. Feed forged heartbeats, spoofed app GUID, replayed SignalR messages, false leader epochs and messages through open relay: no new cluster votes or unauthorized writes.
6. Force database-replication path failure while SignalR relay works: system must report replica lag/commit inability and cannot equate healthy relay with safe database replication.
7. Id, Observer or SMTP separately/collectively fail: outbox/independent checker deliver useful alerts without turning an alert dependency into a login outage.

**Design-stage only.** No Observer, relay, quorum integration, deployed SMTP health report or related tests exists in the new Id project at this time.

## Three external states, fenced authority and scalable cluster membership (2026-10-20)

**Confirmed conceptual API preference:** the externally visible node/service health state remains a deliberately small three-value enumeration: `Ready`, `Degraded`, `Fail`. Keep leader/follower membership, replication progress and quorum eligibility **orthogonal** instead of overloading one enum.

| State | Meaning | Public and write behavior |
| --- | --- | --- |
| `Ready` | Node satisfies the readiness requirements **for its current role** and has an accepted, sufficiently current view of cluster state | A ready `Leader` can take authorized writes. A ready `Follower` may serve permitted reads or proxy requests to the leader; it cannot independently write identities. |
| `Degraded` | A live node has incomplete network reachability, stale replication, lacks a valid leadership lease/quorum, is joining/rejoining, or is under maintenance | May provide approved safe read/health endpoints, **never** unauthorized identity writes; release from degraded state only after resync + validation + applicable lease/role requirements |
| `Fail` | Critical service operations are unavailable or the supervisor cannot establish the required readiness; a stopped/crashed process may be observed externally as `Unreachable` | Remove from operational routing; no authoritative writes. `Unreachable` is a **reason/observation**, not proof of the physical root cause. |

Additional independent values: `NodeRole` (`Leader`, `Follower`, optional `Candidate`), `CanWrite` (derived from leader lease/epoch, fencing and DB authority), `CanServeReads`, `QuorumAvailable`, `ReplicationPosition`, `SyncStatus`, `LastObservedAt`, `StateReason`, `MembershipVersion`, `PublicEndpointReachable`. Internal rejoin phases `Discover`, `CatchUp`, `Validate` map to externally `Degraded`. `Ready` describes a healthy follower **or** leader and does not grant writes merely because the state name is `Ready`.

### Automatic degradation must physically revoke write authority

- The author wants Id1 automatically moved to `Degraded` and Id2 to continue **fully, including new writes**, if Id1 disappears, while Id1 remains degraded until again online **and caught up**. Implement this by actual database/consensus lease loss, fencing/epoch rejection and health-checked ingress routing. Observer can trigger health reports or ask the HA orchestrator to act, but cannot revoke an **unreachable** primary safely by merely sending a SignalR demotion command.
- The old leader must cease writes **even if Observer cannot reach it**. A leader that loses the ability to renew a required quorum lease or use the fenced storage must demote; the authoritative store rejects stale leader epochs. Id2 can promote only after consensus, suitable replica freshness, write-fencing and key readiness.
- If Id1 loses **only one** connectivity path (Observer or Id2) while the cluster still has a legitimate majority and working replication route, do not degrade or fail over needlessly. Report `LinkDegraded` as a warning/condition, possibly enable Observer relay. Conversely, public ingress can mark an API instance degraded/unready even while its internal DB leader is alive, and arrange a **safe leader handoff** if service-level availability requires it.
- In an ordinary two-Id + voting Observer setup, Id2+Observer could authorize leadership with 2/3 votes if Id1 isolated; Id1+Id2 still possess 2/3 if only Observer fails. The **voter system**, not a health dashboard, must count these votes. Detection has bounded timeouts/leases and no guarantee of instant transition.
- Two data-bearing nodes plus an observer-only voter do **not** ensure zero acknowledged write loss and write availability under a data-node loss. A strict synchronous configuration may temporarily refuse commits, or an asynchronous policy accepts an explicitly measured RPO. For both stronger durability and available writes after one failure, consider a properly configured third data-bearing replica/consensus system.

### Grow beyond two Id servers without creating a new problem

- **Application/replica membership** is independent of **consensus voters**. Register a new Id instance with strong node identity and explicit administrative authorization. First run a bootstrap/catch-up in `Degraded` with no public/write readiness, then validate schema, replication position, keys, membership epoch and health, then switch to `Ready` as `Follower`. Promotion to `Leader` is a distinct later decision.
- Voter count `N` requires a strict majority: `quorum = floor(N / 2) + 1`. A 3-voter configuration needs 2; a 4-voter configuration needs 3 (still tolerates only one failure); a 5-voter configuration needs 3 (tolerates two). Do **not** automatically give every newly registered Id app, Observer instance or service heartbeat a vote; preserve deliberate membership control and suitable odd voting-group sizes.
- Adding a fourth data-bearing Id instance may improve read throughput/data replication resilience **without immediately changing the voting set**. Adding and removing voters uses quorum-approved, sequential, restart-safe membership changes, preferably a supported `learner`/nonvoting catch-up stage where the selected cluster stack provides it. Do not add two members at once or count an unready peer toward quorum accidentally.
- Scaling read/API Id nodes is straightforward through the common contracts; scaling strongly consistent write authority still has one leader or equivalent ordered transactional mechanism, even with more than two Id services.
- Observer remains a replaceable **Supervisor + optional Relay + explicitly enrolled Witness**. Do not rely on a single Observer machine for every application's availability or for normal Id traffic; its loss should be survivable while other voting members remain quorate.

### Scenarios to test

1. Id1 becomes unreachable, Id2 and Observer are quorate: Id2 safely becomes write leader and handles logins, MFA, registrations and authorized grants after the HA database layer confirms durability/fencing; Id1 is externally `Fail`/`Unreachable` until observed again.
2. Id1 returns: `Degraded`, discover current leader, catch up and validate; **only then `Ready`** as follower. No automatic return to preferred-primary leadership.
3. Id1 loses only Observer: Id1+Id2 remain quorate; no forced failover. Observer returns and topology/health warning clears.
4. Id1 loses only Id2 direct Uplink: Observer can relay control traffic if permitted; database write capability still follows the HA store's independent state.
5. Id1 isolated from both quorum peers while still answering HTTP: its authoritative writes stop and fail closed; no two conflicting Id account revisions.
6. Add Id3: first `Degraded`/nonvoting catch-up, then `Ready` follower and only deliberately change voter membership if required; no quorum regression when an added node is initially offline.
7. Failover with a stale Id2 replica and only two physical data copies demonstrates chosen RPO/durability policy rather than silently losing acknowledged writes.

References: [etcd failures](https://etcd.io/docs/v3.5/op-guide/failures/), [etcd runtime reconfiguration](https://etcd.io/docs/v3.5/op-guide/runtime-configuration/), [Patroni failover and DCS leases](https://patroni.readthedocs.io/en/latest/dcs_failsafe_mode.html). Specific database/consensus implementation remains undecided.

**Status:** requirements and future acceptance tests only. No C# status enum/observer endpoint, quorum/fencing mechanism, replication, API routing or new Id tests implemented.

## Multi-hop trusted connectivity: Id1 → Id2 → Observer (2026-10-20)

**Author-preferred response to one missing connection: repair the route before moving the leader.** Concrete case: Id1 is `Leader`; its direct Uplink to Id2 is healthy; direct Id1–Observer is broken; Id2–Observer is healthy. Choose **option 3**: Id2 acts as a trusted intermediate relay so Id1 and Observer can exchange approved control traffic. Do **not** unnecessarily promote Id2 because it alone has a direct Observer link, and do **not** merely record the failure when a safe alternative path is available.

```text
                X direct path broken
       Id1  - - - - - - - - > Observer
        |                           ^
        | direct, trusted           | direct, trusted
        v                           |
       Id2 -------------------------+

         Id1 -> Id2 -> Observer
```

### Logical goal

> If a node can reach at least one trusted, authorized cluster peer, it can ask that peer to relay communication to other explicitly authorized cluster destinations.

This is **scoped transitive reachability**, not an unrestricted network gateway and not proof every destination in the world is reachable. Whether the intermediate peer can actually reach the destination, is authorized to relay the requested protocol and has sufficient capacity remains to be checked. Route selection should **prefer direct paths**, then authorized, bounded fallbacks; discover and discard stale paths promptly.

### Separation of health, link quality, role and authority

- Id1 may remain globally **`Ready` as `Leader`** while a *particular direct link* is marked `Degraded` with an incident. A direct link's failure is not by itself a node-wide `Degraded` health transition or grounds to change leadership, provided application/DB readiness and authoritative quorum stay healthy.
- Id2 may be `Ready` as `Follower` and additionally act as a relay for an Id1–Observer control channel. **Relay ≠ leader:** intermediary traffic does not give Id2 permission to mutate Id1/Observer data or assume cluster authority.
- Observer should display `Id1 direct: unreachable` and `Id1 via Id2: reachable`, not collapse these into a false statement that direct connectivity is healthy. Track route provenance, timestamps, hop count, and live downstream delivery acknowledgements.
- If the relay fails too, fall back to actual quorum/fencing decisions based on authenticated voting-system membership and database write readiness. A missing direct Observer link does not revoke writes while Id1+Id2 hold a legitimate majority.

### Relay mechanism choices and limitations

**Control-plane Relay via SignalR:** intermediary forwards an end-to-end authenticated envelope with `SourceNode`, `DestinationNode`, `MessageId`, `LeaderEpoch`, `Sequence`, `TimeToLive`, `HopLimit`, `Trace/RouteId` and bounded payload. Observer must verify that the original source is really Id1; **authentication to Id2 alone must not cause Observer to treat Id2 as Id1**. Id2 is an authorized forwarder, not a delegate allowed to forge leader heartbeats or new votes. Use no arbitrary hub method invocation or forwarding of Id bearer tokens. Maintain dedupe/replay protection, TTL, loop prevention, access policy and rate limits.

**Transport connectivity via secured overlay (e.g. WireGuard):** if the goal is transparent Id1-to-Observer TCP traffic, database replication, existing TLS clients or a consensus/DCS implementation whose voting connections require normal network sockets, a routed, authenticated overlay or deliberately scoped proxy is more suitable than reimplementing generic TCP forwarding as SignalR messages. End-to-end TLS/mTLS remains in place through the intermediate hop. Do not confuse a relayed SignalR health message with a functioning database replication or quorum-voting channel.

**Quorum:** the example Id1+Id2 link already offers 2 of 3 voting members **if those actual voting peers remain connected in the chosen consensus implementation**. Id1 can retain lease/leadership without direct Observer connectivity. A relayed Observer vote counts only if the real consensus stack **securely supports that route**; the Observer's application-level heartbeat dashboard cannot manufacture a vote. When the network partition isolates a prior leader from quorum, fencing still prevents simultaneous writers.

**Trust scope:** only explicitly enrolled internal cluster nodes may relay privileged Id control traffic. Ordinary user-created Afrowave apps sending `IAmHere` do not become routers or cluster voters. For future general service-mesh use, distinguish service discovery from relay permission; forbid access to other tenants, Id keys, internal-only administrative endpoints and unapproved destinations. Each hop is monitored and subject to quotas, expiry and audit.

### Suggested route health representation (not a frozen API)

| Item | Meaning |
| --- | --- |
| `NodeHealth` | `Ready`, `Degraded`, or `Fail`: overall usable service status |
| `NodeRole` | `Leader`, `Follower`: actual leader/replica responsibilities |
| `LinkHealth(Id1,Observer)` | `DirectUnavailable`, with distinct reason/last confirmed direct probe |
| `Route(Id1,Observer)` | `ViaId2`, independently authenticated, with bounded hop count |
| `RouteHealth` | `ReadyViaRelay` after Id1 and Observer successfully acknowledge a test/control exchange |
| `CanWrite(Id1)` | Derived **only** from actual leader lease/quorum, fencing and DB state; unaffected by relay appearance alone |

### Integration tests to implement later

1. Cut only Id1–Observer direct path while Id1–Id2 and Id2–Observer remain up. Detect failure; establish authorized relay; Id1 stays legitimate `Leader`/`Ready`; report direct-link incident and relay route, no failover.
2. Restore direct link: prefer direct communication again after stable health confirmation, retire relay automatically without dropping authorized in-flight messages or duplicating change events.
3. Break Id2–Observer during relay: retire stale route, report that Id1's Observer path is now lost; proceed according to **real** cluster quorum/write availability rather than falsely assuming observer votes traverse an application Hub.
4. Spoof source `Id1` through Id2, inject wrong epoch, replay message or create circular Id1–Id2 forwarding: reject unauthorized, stale or looping traffic.
5. Cut direct database replication but keep SignalR messages relayable: replication is still marked unhealthy and write durability policy is enforced; SignalR success alone never makes database healthy.
6. Register an ordinary Afrowave service and send valid `IAmHere`: service may be monitored but gains no cluster voting/privileged Id routing access.

**Status:** architecture/documentation only; no SignalR relay router, mesh, overlay provisioning, routing table or tests are implemented.

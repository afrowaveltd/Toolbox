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


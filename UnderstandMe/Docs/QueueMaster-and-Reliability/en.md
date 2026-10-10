# QueueMaster, Priorities and Reliability

Status: agreed operational concept; concrete synchronization and SQL schema are not yet designed.

## Work ownership

QueueMaster is the only owner of five central priority queues. Producers submit logical jobs, batches or items through its API. Translators do **not** dequeue central work directly; their individual small FIFO buffers request the next compatible item when capacity appears.

Provisional priority names: P0 `RealTime`, P1 `Interactive`, P2 `High`, P3 `Normal`, P4 `Background`.

An endpoint buffer is intentionally small (five items is an initial tuning candidate). Every free slot can request **one** item. QueueMaster chooses the highest-priority waiting item compatible with that endpoint. Within an endpoint buffer, work remains strict FIFO; there is no priority-based skipping, preemption or buffer rearrangement. This bounds queued-ahead work without a complex load-balancing algorithm. Faster translators naturally request replenishment more often.

## Notifications

`QueueEmpty` represents whether an individual central queue has no items. An enqueue that **atomically changes a given queue from zero items to one** triggers the common `AddedToQueue` event; enqueues to already nonempty queues do not. Idle buffers can wake and request work. A periodic check (initial suggestion ~500 ms) provides a safety net. A buffer that becomes free while compatible work already exists also requests more.

QueueMaster (not the event) selects the priority and ensures one owner per item. A partially empty buffer can be replenished one slot at a time; default size and thresholds remain subject to tests.

## Endpoints, failures and recovery

An endpoint has `IsReady` as a single boolean readiness state. Capabilities (source/target languages, HTML, batch/alternatives, model/context, privacy) and available capacity are separate. An unsupported language is not a server failure.

Temporary endpoint outages are expected. Retry cadence/count is configurable; a previous LibreTranslate experiment used 2-second retries up to 10 attempts. During transient unavailability, a small buffer can remain reserved briefly or eligible unprocessed work can be reassigned when another compatible endpoint requests capacity. Avoid both duplicate publishes and starvation; exact leasing/retry/hand-off details will be designed and tested later. Periodically check unavailable endpoints and allow them to rejoin automatically.

## Persistence

P0 RealTime requests are volatile/fire-and-forget, without durable job history. Normal/long-running tasks may use `Persist` so an acknowledged request survives restarts. Default standalone operational database: SQLite; larger deployments can select supported EF Core database providers. This is **not** the same as the source/target dictionary storage.

A durable acceptance ACK means the request has been committed to durable work storage, not merely enqueued in memory, and not completed. A transient live request has its own non-durable response semantics. Track item ownership and completed checkpoints so interrupted work can be resumed safely. Outbox/Inbox with event IDs and acknowledgements supports at-least-once delivery plus idempotent effects.

Operational state must recover even if the process stops after a DB commit and before inserting an item into an in-memory queue. Build in-memory queues from durable state when needed. SignalR reconnection, local events, and ~3-hour source reconciliations cover different problems and should remain independent.

## Lifecycle

Keep `Persist` (recoverable job execution), `Preserve` (optional step outputs/history), and `LifeTime` (cleanup policy) separate. `FinalizeAndFlush` can delete disposable working state only after the receiver acknowledges acceptance and the relevant policies permit it. Mandatory manual-edit audit is not disposable with a job.

Related: [Translation Chains](../Translation-Chains/en.md), [Notes](../../Notes.md).


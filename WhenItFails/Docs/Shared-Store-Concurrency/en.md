# Shared-store runtime concurrency

Status: **pre-1.0 ownership audit; five focused regression tests await local verification**.

## Scope of the existing activation gate

The default `ErrorCatalogRuntime` serializes `InitializeAsync` and
`ResetToDefaultsAsync` on **one runtime instance** with a cancellable
instance-local `SemaphoreSlim`. It holds the gate until that
operation finishes publishing its context and recording status.

A second runtime with the **same** `IErrorCatalogContextStore` has a
different gate. A direct caller of `store.Set(context)` bypasses all
runtime gates. Reading, descriptor resolution and snapshots also do
not take the activation gate.

## Observed boundary scenarios

`SharedStoreRuntimePublicationBoundaryTests` records five scenarios
with the real publication-aware `ErrorCatalogContextStore` and
controlled initializer doubles:

1. When runtime B publishes a different context after A has completed,
   A's `GetCompletedActivation()` rejects its now-stale publication,
   while B's observation matches the newer store generation.
2. When runtime A has published its context and is paused before it
   returns from initialization, runtime B can complete its own
   activation. A can then finish writing its legacy status **after**
   B's publication, but cannot claim a matching completed activation
   observation for A's displaced context. B's completed observation
   remains valid.
3. A direct external `Set` of a **different** context between A's
   publication and status recording leaves A's legacy status present
   but prevents a matched completed activation observation.
4. A strict initialization failure in another runtime, without a
   publication, leaves the first runtime's completed observation and
   the store publication unchanged.
5. A second runtime can record a `PreviousContextRecovery` status
   for a context already published by runtime A **without** advancing
   the store generation. Each runtime has its own local
   `ActivationSequence` and may truthfully report a different status
   for the same `(StoreId, Generation)` pair.

The tests capture current behavior; they **do not** establish a
cross-runtime ordering guarantee. A runtime's `GetStatus()` is
instance-local diagnostic state and is not a synchronized
description of every later external publication to its shared store.

## Important same-reference ownership gap

The current optional completed-observation recording checks that the
store's latest publication contains the **same context reference** as
the operation's payload. This is not sufficient to prove ownership.

An external writer can publish that *same instance* after a runtime
operation's `Set` and before its `RecordStatus`. The latest
publication then has a **different generation** but the same object
reference. The runtime cannot identify the publication it originally
owned solely from that reference and can associate its status with
the later external publication. This is an unresolved strict
activation-identity gap. The instance-local activation gate does not
prevent it.

Avoid describing `GetCompletedActivation()` as a globally atomic
context/status transaction or its generation as the uniquely
identified publication *owned by* the runtime operation under external
writers. The publication-aware combined snapshot, separately called
`GetStatus()` and completed-observation reader are not automatically
one coherent multi-view snapshot.

## Next design decision

A stronger contract needs **publication ownership**, not only a lock
around `ErrorCatalogRuntime`:

- A store operation that returns the **specific publication record**
  created by the successful write, ideally via an additive optional
  publisher capability.
- Propagation of that record from the owning initializer or
  runtime operation to the completed status, including rules for
  flexible previous-context recovery (which does **not** republish).
- A strategy for external `Set` callers and multiple runtime
  instances sharing the store; legacy/custom stores lacking the
  capability must receive a clearly defined weaker result rather
  than a fabricated ownership token.
- Read-side semantics for stale-but-previously-completed observations,
  and a separately designed coherent combined data/status capture
  if the consumer requires both from the same selected publication.

Do not treat an independently generated GUID, a hash of the context
reference, a timestamp, or merely double-reading `Generation` as
proof of who performed a write. None establishes ownership under the
same-reference republish scenario.

No production API or JSON schema is modified by this audit step.

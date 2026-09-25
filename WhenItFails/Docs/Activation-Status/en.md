# Completed runtime activation status observations

Status: **additive pre-1.0 candidate; eight focused tests included in the maintainer-confirmed 1314/1314 GREEN suite**.

## Why this is separate from a context generation

The default store increments `Generation` on every successful
`Set(context)`, including a reset or automatic fallback. A flexible
initialization failure can retain the **same** context and publication
while the runtime records a **new recovery status**. Therefore the
store generation alone cannot identify a completed runtime status event.

The default `ErrorCatalogRuntime` now also implements the optional
`IErrorCatalogRuntimeActivationReader`:

```csharp
using Afrowave.Toolbox.WhenItFails.Interfaces;

if (runtime is IErrorCatalogRuntimeActivationReader reader)
{
    var response = reader.GetCompletedActivation();

    if (response.IsSuccess && response.Data is { } observation)
    {
        Console.WriteLine(
            $"{observation.StoreId}/{observation.Generation}, " +
            $"status #{observation.ActivationSequence}: " +
            $"{observation.Status.State}");
    }
}
```

The returned `ErrorCatalogActivationStatusSnapshot` is sealed with
four getter-only properties: `StoreId`, `Generation`,
`ActivationSequence` and `Status`. It exposes **no live catalog
context**. The public status object has init-only properties and is the
status instance recorded for the selected completed observation.
The outer Essentials `Response<T>` retains its normal mutable
envelope.

`ActivationSequence` starts at 1 for the first **matched completed
status observation within one runtime instance** and increments for
each later matching completion. This number is not persisted and is not
a substitute for the store-scoped `(StoreId, Generation)` pair.

## Recording and reading an observation

After constructing and validating a runtime status, `RecordStatus`
retains its original legacy status publication. Separately, the runtime
reads the optional store publication record and creates a completed
observation **only if the published context is the exact context
reference of that activation payload**. The completed observation
associates the selected publication record and the status object in
one private record; its public projection exposes only identity and
status. It does not infer a generation from a timestamp or fabricate
one for a store without the optional reader.

`GetCompletedActivation()` checks that the selected recorded status is
still the current legacy status and that the selected publication
record is still the one returned by the store at the time of its check.
If no completed match exists it returns Invalid with
`WIF_ACTIVATION_STATUS_UNAVAILABLE`. A changed status or publication
returns `WIF_ACTIVATION_STATUS_PENDING` or
`WIF_ACTIVATION_PUBLICATION_CHANGED` respectively. A legacy custom
store without the optional publication reader returns NotSupported
with `WIF_ACTIVATION_STATUS_NOT_SUPPORTED` rather than a synthetic ID.

The optional observation is deliberately **best effort**, and any
failure of its private capture must not change the established
initialization/reset/recovery result or existing `GetStatus()`
behavior. A context publication performed externally without a
corresponding runtime status completion invalidates the previously
recorded observation (even if the same mutable context reference is
re-published). Successful project initialization and reset/fallback
normally advance both counters; retaining the previous context
advances the runtime observation sequence **without** advancing the
context generation. Failed strict reinitialization and failed explicit
reset leave the previous completed observation intact.

## Serialized activation on the default runtime

On a single default `ErrorCatalogRuntime` instance, `InitializeAsync`
and `ResetToDefaultsAsync` use the same cancellable asynchronous gate.
An operation holds it from entry to the underlying initializer/provider
through publication, recovery and status recording. A later operation
enters only after the first finishes, fails or propagates cancellation;
waiting cancellation does not invoke the queued initializer/provider.
This is an **instance-local**, non-reentrant activation gate. Do not call
an awaited activation method recursively from within the initializer or
built-in provider of the same runtime instance.

Context resolution, status reads, optional activation reads and ordinary
snapshot readers do not take the gate. Publication can still precede
status recording, and a reader can observe that intermediate state.
Custom runtimes, external store writers and separate runtime instances
sharing a store are not serialized by this gate.

## Consistency limits

The method returns a **selected, previously recorded association**.
It does not make the store and runtime status one atomic publication
and does not lock either resource. A direct store writer can change
the publication immediately **after** the consistency check. A caller
must not treat a separately acquired `GetStatus()` or
`GetPublishedCombinedSnapshot()` result as belonging to this
observation without an additional, explicitly coordinated read.

The default `ErrorCatalogRuntime` now serializes its own `InitializeAsync`
and `ResetToDefaultsAsync` operations through one asynchronous activation
gate, including publication and status recording. This prevents overlapping
activations *on the same runtime instance* from overtaking one another.
The optional status **reader** itself does not acquire the gate, and direct
writers using the injected context store or other runtime instances that
share that store remain outside this serialization boundary. In particular,
an external writer can publish the *same context reference* between this
runtime's `Set` and status recording. Reference equality alone cannot
establish which actor owned that publication. Strict activation-event
identity across all writers therefore remains out of scope until the store
has an explicit publication ownership policy. This optional observation
is an incremental contract, **not** the final atomic context-plus-status
snapshot.

The default runtime still supports the existing nine-method
`IErrorCatalogRuntime` interface and `GetStatus()` semantics.
A custom runtime is not required to implement the optional reader.
No catalog JSON schema, published package version, or serialization
wire contract is changed by this feature.

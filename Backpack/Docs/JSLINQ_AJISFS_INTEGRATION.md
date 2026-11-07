# JSLinq ↔ Backpack JavaScript / AjisFS integration review

**Reviewed:** 2026-10-27 — [afrowaveltd/JSLinq](https://github.com/afrowaveltd/JSLinq) `main`. **Status:** proposal, not an implemented integration. JSLinq repository at review had `@afrowave/jslinq` version `0.1.0`.

## Evidence in JSLinq code

- [`src/index.ts`](https://github.com/afrowaveltd/JSLinq/blob/main/src/index.ts) exposes `JSLinq.from<T>(Iterable<T>)`, `range`, `repeat`, `empty`.
- [`src/enumerable.ts`](https://github.com/afrowaveltd/JSLinq/blob/main/src/enumerable.ts): sync `Iterable<T>` / `[Symbol.iterator]` implementations for `where`, `select`, `take`, `skip`, `selectMany`, `distinct`, `groupBy`, `orderBy`, aggregates and element operators. `where/select/take` evaluate lazily. `groupBy` builds a `Map` of arrays **eagerly**; `orderBy` gathers all input into an array before sorting, and `distinct` maintains an ever-growing seen-key `Set`.
- TypeScript source, ESM package and browser compatibility build via Babel; [`test/`](https://github.com/afrowaveltd/JSLinq/tree/main/test) contains phase1–phase9 test files. Build/test claims here are from source inspection; **tests were not executed** during this review.
- Current `JSLinq.from` accepts **sync** `Iterable`, not `AsyncIterable`; no `fromAsync` or async LINQ class found in inspected source.
- In the inspected sync operator implementations, custom iterators implement `next()` but generally do not propagate `return()`/cleanup to the wrapped iterator. **Before AjisFS/file/network source integration, implement reliable early-termination disposal** and cancellation (`take`, `first`, exceptions, user break).
- `where(x => ...)` takes an arbitrary JS function. A disk query engine **cannot reliably infer indexed constraints from the predicate body**, so JSLinq alone does not provide indexed query pushdown.

## Integration boundaries (proposed)

```text
AJIS text or native binary format
  → AJIS parser/framer (document semantics, pointers, directives)
  → AjisFS storage/index resolver (reads only indexed/relevant objects)
  → async record sequence  [AsyncIterable<T>]
  → optional JSLinq async/JS query operators
  → projections/aggregates/UI/SignalR
```

**Responsibilities:**

1. **Backpack core:** decoding/framing/semantic model — JSLinq should not be a required dependency.
2. **AjisFS:** minimal storage-native primitives useful to callers: seek/read by data-area offset, record scan, index lookup, projection where efficient, bounded buffering and optional temporary disk workspace. AjisFS owns persistence, indexing and transaction mechanics **but does not independently implement JSLinq operators** such as `where`, `groupBy`, `orderBy` or a second complete query planner. Offset is relative to the start of the native AJIS binary data area after the header, **not LBA**.
3. **JSLinq:** owns the query vocabulary and execution semantics — filter, projection, grouping, sort, aggregation, joins, pagination — building on normal `Iterable` and future `AsyncIterable` streams. For large datasets its operators may optionally use a storage capability/index/scratch provider exposed by AjisFS, while preserving in-memory operation without AjisFS.
4. **Optional bridge:** an adapter such as `@afrowave/ajis-jslinq` can connect the JSLinq query plan and AjisFS storage capabilities so an indexed restriction or an index-ordered scan is pushed to the storage side. Neither JSLinq nor ordinary AJIS parsing must depend on AjisFS. AjisFS should not become a duplicate database query engine or silently materialize entire files.

## Design clarification: AjisFS goes only halfway (2026-10-27)

**Direction from the architecture discussion:** AjisFS implements the small set of **storage capabilities** which make JSLinq better, but deliberately **does not implement operations that belong to JSLinq**. Never build two independently maintained LINQ engines.

- **AjisFS answers:** “Where are those records physically, which index is available, how can I read only the required bytes, how can I enumerate them incrementally, can I use scratch space?”
- **JSLinq answers:** “Which filter, ordering, projection, grouping, join or aggregation does the user want, and how should those operators compose?”
- **An optional bridge** translates *structured/index-friendly parts* of a JSLinq query request into AjisFS capabilities. It does not parse arbitrary JavaScript lambda bodies as SQL.
- **Only implement storage primitives on demand**, when a concrete JSLinq use case benefits. Do not preemptively add a generic SQL-like planner, JSLinq-specific query duplication or a large parallel abstraction stack to AjisFS.
- **Disk-heavy algorithms** such as external sorting or grouping can be orchestrated by JSLinq or its provider using scratch/buffer capabilities AjisFS exposes. Deciding the minimal practical contract is a **separate design step**, not a promise that those algorithms already exist.

Illustrative separation (not public API):

```text
JSLinq query (intent/operations)
      |
 optional AJIS query adapter
      |
 AjisFS primitives: index | scan | read | offsets | scratch
      |
        AJIS file
```

This approach keeps AJIS simple, JSLinq independently useful and AjisFS focused on what it uniquely knows about the physical data.

### Simple already-supported use case

```js
import { JSLinq } from '@afrowave/jslinq';
const people = [{ name: 'Ada', country: 'CZ' }, { name: 'Ben', country: 'GB' }];
const names = JSLinq.from(people)
  .where(person => person.country === 'CZ')
  .select(person => person.name)
  .toArray();
```

### Future indexed streaming use case (pseudocode, **not implemented**)

```js
const fs = await AjisFS.open('people.ajis');
// A declarative predicate lets AjisFS use a country index *before* producing records:
const source = fs.query('users', { country: 'CZ' }); // proposed AsyncIterable
const names = await JSLinq.fromAsync(source)          // proposed JSLinq extension
  .where(user => user.age >= 18)
  .select(user => user.name)
  .take(50)
  .toArray();
```

AjisFS exposes indexed access and I/O; an **optional adapter** selects those capabilities from structured query constraints; JSLinq owns remaining filter/projection and other query semantics. If conditions must be completely optimized, consider a provider with field-aware expressions rather than introspecting JS lambda source.

## JavaScript binary work is viable

- JS `Number` bitwise operators coerce to **signed 32-bit**: do not use them for 64-bit disk offsets/flags. Use `BigInt` (and BigInt bit operations) when 64-bit exactness is needed.
- `DataView`, `Uint8Array` and Node `Buffer` already read/write bytes and fixed-width integers, including 64-bit integers as `BigInt` on modern runtimes.
- A `Number` represents *integer byte offsets* exactly up to `Number.MAX_SAFE_INTEGER` (~9 PB). 150 GB offsets are representable; this does **not** remove format-level 64-bit bounds or the need for BigInt when reading full unsigned 64-bit wire values.
- Start with correct portable TS/JS byte decoding and benchmark. Optional Node native/WASM acceleration can be considered only for measured bottlenecks; it is not required by the format.

## Suggested next test milestones

1. `JSLinq.from(AjisReader.records())` working over a synchronous small record stream; verify no entire-file buffering.
2. `fromAsync` or an independent optional async adapter, with bounded backpressure, `AbortSignal`/cancellation, and guaranteed iterator cleanup.
3. Indexed AJIS file query: compare `index lookup → async rows → local where/select` versus full scan; verify I/O counts and RAM.
4. Test huge `orderBy`/`groupBy` with JSLinq-managed algorithms and an optional AjisFS-provided scratch/index interface; **do not reimplement LINQ operators inside AjisFS**.
5. Portability fixtures for Node and browser; same AJIS documents and semantics, different storage interfaces.
6. Link JSLinq and JS SignalR as **optional integrations**, never mandatory dependencies for simple AJIS parsing.

**Design principle:** Reuse JSLinq as the JS developer-facing *query vocabulary*, and build the minimal missing async/index bridge instead of creating a competing query library inside Backpack.

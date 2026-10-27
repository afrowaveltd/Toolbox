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
2. **AjisFS:** index lookup, seek/offset addressing, persistent iterators, bounded-memory buffering, paging, external sort/aggregation, disk spill and transaction rules. Offset for native binary object data is relative to the start of payload area after header, **not LBA**.
3. **JSLinq:** developer-friendly *in-memory* and *streaming* query operations, ideally expanded with `AsyncIterable` and async terminal operations.
4. **Optional bridge:** an adapter such as `@afrowave/ajis-jslinq`, or an AJIS query adapter built on `JSLinq`. A full AjisFS implementation must not silently switch all queries to `Array.from(file)`.

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

AjisFS handles index/predicate pushdown and I/O; JSLinq handles the residual local filter and projection. If conditions must be completely optimized, consider a provider with field-aware expressions rather than introspecting JS lambda source.

## JavaScript binary work is viable

- JS `Number` bitwise operators coerce to **signed 32-bit**: do not use them for 64-bit disk offsets/flags. Use `BigInt` (and BigInt bit operations) when 64-bit exactness is needed.
- `DataView`, `Uint8Array` and Node `Buffer` already read/write bytes and fixed-width integers, including 64-bit integers as `BigInt` on modern runtimes.
- A `Number` represents *integer byte offsets* exactly up to `Number.MAX_SAFE_INTEGER` (~9 PB). 150 GB offsets are representable; this does **not** remove format-level 64-bit bounds or the need for BigInt when reading full unsigned 64-bit wire values.
- Start with correct portable TS/JS byte decoding and benchmark. Optional Node native/WASM acceleration can be considered only for measured bottlenecks; it is not required by the format.

## Suggested next test milestones

1. `JSLinq.from(AjisReader.records())` working over a synchronous small record stream; verify no entire-file buffering.
2. `fromAsync` or an independent optional async adapter, with bounded backpressure, `AbortSignal`/cancellation, and guaranteed iterator cleanup.
3. Indexed AJIS file query: compare `index lookup → async rows → local where/select` versus full scan; verify I/O counts and RAM.
4. Test huge `orderBy`/`groupBy` with disk-backed AjisFS strategy, rather than assuming ordinary JSLinq can stream them with fixed RAM.
5. Portability fixtures for Node and browser; same AJIS documents and semantics, different storage interfaces.
6. Link JSLinq and JS SignalR as **optional integrations**, never mandatory dependencies for simple AJIS parsing.

**Design principle:** Reuse JSLinq as the JS developer-facing *query vocabulary*, and build the minimal missing async/index bridge instead of creating a competing query library inside Backpack.

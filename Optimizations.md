# Optimizations

Candidate performance work, with the measurement that motivates each. Ratios are against the hand-coded loop
in the matching benchmark on .NET 11 RC2, short job, in-process (see `CLAUDE.md` for the run recipe and its
caveat). The mechanism behind most of these is described in `ValueLinq/Docs/BatchFromLocalCopy.md`.

## Current standing against ZLinq and System.Linq

One sitting, Handcoded, Cistern, ZLinq and Linq per class in one process. Cistern leads every row; Cistern and ZLinq
allocate nothing. Cistern's weakest rows, as ratios to hand-coded:

| Benchmark | Cistern | ZLinq | Linq |
|---|---|---|---|
| `WhereWhereSum` Array / List, 1000 and 100000 | 1.95–2.11 | 4.83–5.33 | 3.83–5.11 |
| `SelectManyWhereSum` row length 10 (100 and 10000 rows) | 2.44–2.52 | 2.82–2.85 | 9.89–10.08 |
| `SelectSelectSum` Array / List, 1000 and 100000 | 1.45–1.51 | 5.79–6.53 | 13.59–15.69 |
| Length 10 on List (the six Coverage sums) | 1.69–2.18 | 2.11–2.92 | 4.17–11.05 |

Elsewhere on Array and List from length 1000 Cistern is 1.13–1.25 (ZLinq 3.35–5.62); on Enumerable 1.04–1.22.

The ref-push redesign (chain passed by `ref`, state in cells) was built and measured against this; its outcome and
where the implementation is kept are in `RefPushRollout.md`.

## A type-level "always answers `AsSpanSource`" marker

`SelectMany.Push` reaches its chain as `ref (_common == null ? ref _next : ref _common.Chain)`, a byref of unknown
location, so the chain stays in memory and its write-backs take a write barrier. Hard-wiring the skip for
`SystemArray.Node` inner types (`typeof(NodeU) == typeof(SystemArray.Node<U>)`) took `SelectManyWhereSum` at row length 10 from
about 2.0× to 1.17×, the largest gain measured on that benchmark. The general form is a marker on inner node types
that always answer `AsSpanSource` (array, list, memory), so `_common` is never consulted for them.

## Fuse consecutive `Where`s and consecutive `Select`s

`WhereWhereSum` and `SelectSelectSum` are the weakest flat chains. A forwarder two levels above the leaf cannot have
the leaf promoted by its batch copy (see `ValueLinq/Docs/BatchFromLocalCopy.md`, "Constraints"); one
forwarder holding both delegates keeps a single level, and keeps each delegate separate so Dynamic PGO can still
guard and inline each. Not yet confirmed by a listing.

## Copy mutable state only

A member on `IPushEnumerator` that copies only the mutable state, so a batch writes back the accumulator rather
than the whole chain. Built as a hard-wired hack for `SelectManyWhereSum`'s chain it measured 1.36 / 1.13 / 1.24 /
1.23 at row lengths 2 / 10 / 100 / 1000 against 1.55 / 1.16 / 1.29 / 1.32 without.

## Other weak spots from the full Coverage run

`SkipTakeSum` on array/list (~2.6× ZLinq although `Optimizations.SkipTake` slices; cause unknown); `Cast`,
`OfType`, `Aggregate` and `Zip` slow on `List<T>` only; `SelectToList`, unknown-length `ToArray`, `ToDictionary`
presizing and `OrderBy` allocation; `Except`/`Distinct` allocation; fixed per-query cost at length 10, worst on lists.

## Batch paths for the remaining forwarders

`SelectFoward`, `WhereFoward`, and the `SkipWhile`/`TakeWhile` forwarders with their indexed variants answer the span
batch request. `SelectIdxFoward`, `WhereIdxFoward`, the `Select_In`/`Where_In` variants, `ValueSelect`/`ValueWhere`,
`Skip` and `Take` return `Unavailable`, so any chain through them falls to the per-element loop with the
memory-carried accumulator.
Same local-copy pattern, a few lines each; `Skip` and `Take` can slice the span instead of counting elements.

## Indexed `SelectMany` wrappers still close over the selector

The `Func<TSource, int, IEnumerable<TResult>>` overloads (`Transforms.tt`, built by `NodeImpl.SelectManyEnumerable`)
wrap the selector in a lambda, so each query allocates a closure and a delegate (80 B), as do the result-selector
overloads' pairing of each source element with its inner sequence. The non-indexed overloads use `ValueSelectNode` with
the `EnumerableNodeSelector` functor; the indexed ones need an indexed `ValueSelect` node first.

## Results

Landed changes with their before and after, same job and machine as above, Cistern against hand-coded.

- `ListPopulator` classes replaced by `CollectionsMarshal.SetCount` plus `AsSpan`, and `NodesEnd` overriding the
  `INodes` default member: `Synthetic.WhereToList` at 10 and 64 unchanged at 46 ns and 128 ns. Robustness, not
  speed.
- Non-indexed `SelectMany` wrappers on `ValueSelectNode` with the `EnumerableNodeSelector` functor instead of a
  capturing lambda: `Synthetic.SelectManyWhereSum` allocation 80 B to 0 B per query, the one-row case 25 ns to
  19 ns, 10k rows of length 10 from 249 µs to 217 µs.
- `IPushEnumerator.TryProcessBatch<TObject, TRequest>` with its `GetSpan` delegate replaced by
  `TryProcessSpan(ReadOnlySpan<T>, BatchOrigin)`: `Synthetic.SelectManyWhereSum` and `Synthetic.Pipeline` within
  noise of before (1.42× and 1.23× at 10k rows; 1.34× and 0.96×). Deletes the request dispatch, `UseSpan` and
  `Chunk<T>`; no speed change.

## Known floors, not optimizations

- `Sum` on integers uses a `checked` add; it costs about 30% on a one-cycle loop. `SIMDOptions.Fastest` is the
  existing opt-out, so this is a semantics choice, not a defect.
- `SelectMany` over inner sequences that expose no span (`Synthetic.SelectManyEnumerableSum`, `Queue<int>` rows) is
  3.4×, level with ZLinq at 3.7× and ahead of System.Linq at 5.0×. The whole gap is the boxed `IEnumerator<T>` per
  row (40 B, the same in all three libraries) and interface dispatch per element; the shared
  `ErasedChain.PushSink` is one object per query, so holding the chain by `ref` instead would not move it.
- `Sum`, `Min`, `Max` and `Average` reach their SIMD span paths only when attached directly to a container;
  anything between them and the source makes the chain scalar. A batch path on the intervening forwarder (above)
  restores the register loop but not the vectorisation.

# Batches run from a local copy

Why a forwarder's batch method copies its downstream to a local before the loop, what that costs, and how to see it.
Comments at the sites point here by section.

**Verified against: .NET 11.0.0-rc.2.26504.105 (x64, Windows), Release, Tier1 listings.** Everything here is a
statement about that JIT's struct promotion and register allocation. A JIT change can move any of it, in either
direction, with no build or test saying so; the Tier1 listing under "Seeing it" is the check, and this line is updated
with the version each time it is re-run.

## The problem

A push chain is a nested struct: `Where.Push<T, Select.Push<T, U, Sum<U, long>>>`, the `Sum` at the bottom. The
container pushes into it through a `ref`, and every forwarder reaches its downstream through a field, `_next`. The JIT
keeps the fields of a struct it reaches by reference in memory, so the `Sum`'s accumulator is a memory location, and
each element costs it a load, an add and a store: a store-to-load round trip of several cycles where a register add
costs one.

## The fix

Copy the downstream to a local, run the whole span against the copy, write the copy back:

```csharp
// through the field: _next is reached by ref, so its accumulator lives in memory
foreach (var item in span)
{
    if (!_next.ProcessNext(_selector(item)))
        break;
}

// from a local copy: the JIT promotes the copy's fields, so the accumulator lives in a register
var next = _next;
var selector = _selector;
foreach (var item in span)
{
    if (!next.ProcessNext(selector(item)))
        break;
}
_next = next;
```

The local is a plain struct local, so the JIT promotes its fields to registers for the loop. On the JIT named at the
top, a nested struct's fields are promoted as a flat one's are: a `Where`'s copy of a `Select` over a `Sum` keeps the
sum in a register. Earlier JITs promoted only the outer struct's fields, which left a two-level copy in memory; that
is the kind of fact the version line exists for. The copy in and out costs a struct copy per batch, repaid over the
span's elements.

## Constraints

- The copy must not be live into a `try`/`finally` or a `catch`. Anything live across an exception edge is pinned to
  the stack, which puts the accumulator back in memory for the whole loop. So the batch loop has no handler, and an
  exception from a user delegate mid-batch discards that batch's partial state: `_next` keeps what it held before the
  batch. That is the documented behaviour of a chain that throws.
- The copy is of the whole downstream chain, so its size grows with depth. The leaf aggregator is what the promotion
  is for; a deep chain stays in memory either way.

## Where it applies

- `Select.Push.ProcessBatch` (`Nodes/Select.cs`): the span run against the copied downstream.
- `Where.Push.ProcessElements` (`Nodes/Where.cs`): every batch, each survivor handed to the copied downstream as the
  predicate passes it. A `Where` does not collect its survivors into a buffer to offer downstream as a batch: the
  store and reload per survivor cost more than a batch saves the receiver, up to twice the time of the one pass at
  90% selectivity (`Parity/B2` B14).
- `SelectManyResult.Push` (`Nodes/SelectManyResult.cs`): the span of each inner sequence that exposes one, from a copy of the
  downstream only when the downstream's type carries the `IInternal__Aggregator__NotForManualUse` marker, which
  `[Aggregator]` writes on every leaf aggregator, and against it by `ref` otherwise.
- `SkipWhile`, `SkipWhileIdx`, `TakeWhile`, `TakeWhileIdx`: their `Push.ProcessElements`, for the part of a span the
  predicate has finished with.
- `SelectMany.Push` (`Nodes/SelectMany.cs`) keeps its downstream chain by value for the same reason and offers each
  inner span to it as a batch; it moves the chain into a heap `ErasedChain.PushSink` only for inner sequences that cannot expose
  a span, because an inner node's push copies the enumerator it is given.
- `IInternal__Aggregator__NotForManualUse` (`Chain/AggregatorMarkers.cs`): the push enumerator with no downstream, which a batch copy promotes whole.

## Seeing it

The proof is the Tier1 listing of the batch loop. A harness that runs one chain enough times to reach Tier1, with the
JIT asked for the method's disassembly:

```csharp
static long WhereSelectSum(int[] data) => data.Where(x => (x & 1) == 0).Select(x => x * 2).Sum();
// called a few million times over an int[1000] from Main
```

```
set DOTNET_JitDisasm=*ProcessBatch* *ProcessElements*
set DOTNET_JitStdOutFile=listing.txt
```

In the listing, the loop body is the delegate calls and then the accumulate. Promoted, the accumulate is an add
between registers; not promoted, it is a read-modify-write through the stack (`add qword ptr [rsp+..], reg`) or a
load and a store around the add. The `(Tier1)` header on the listing is what matters; Tier0 keeps everything in
memory whatever the code does.

## Benchmarks

The Coverage benchmarks that run these paths over an array, each compared against its hand-coded loop (never against
`System.Linq`): `SelectSum`, `WhereSum`, `WhereSelectSum`, `SelectWhereSum`, `WhereWhereSum`, `SelectSelectSum`,
`SkipWhileSum`, `TakeWhileSum`, and `Synthetic/SelectManyWhereSum` for the SelectMany path. The per-element cost is
what the local copy changes, so the lengths that show it are the long ones (1,000 and 100,000 in the default
`Lengths`); at length 10 the fixed cost of entering the chain dominates. See `CLAUDE.md`, Benchmarks, for the run line.

Two Synthetic benchmarks measure the aggregator marker's choice in `SelectManyResult.Push.ProcessNext`, each
against the other path forced there:

- `SelectManyResultSelectorSum`, the result-selector SelectMany straight into a `Sum`, which the marker runs from a
  copy: at 100 rows 6.6 µs against 19.2 µs by `ref` (hand-coded 5.1 µs); at 10,000 rows the two are level, 1.92 ms
  against 0.52 ms hand-coded.
- `SelectManyResultSelectorWhereSum`, a `Where` between them, which the marker leaves by `ref`: at 10,000 rows
  1.43–1.47 ms against 1.60–1.80 ms with the copy forced; at 100 rows the copy is 5–10% ahead.

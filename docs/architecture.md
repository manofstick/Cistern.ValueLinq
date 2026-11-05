# Architecture

[← Overview](../README.md) · [History](history.md) · [Source generators](generators.md)

- [Nodes](#nodes)
- [The JIT as the optimiser](#the-jit-as-the-optimiser)
- [Push](#push)
- [Pull](#pull)
- [Requests](#requests)
- [`IEnumerable<T>` boundaries](#ienumerablet-boundaries)
- [Generated code](#generated-code)

## Nodes

Every operator is an `INode<T>` struct holding its source node and its configuration (a selector, a count, a
comparer). A query is a nesting of these structs, wrapped in `ValueEnumerable<TSource, TNode, TEnumerator>`, which
also names the type of the chain's enumerator.

```mermaid
flowchart TB
    subgraph VE["ValueEnumerable#lt;int, TNode, TEnumerator#gt;"]
        direction TB
        subgraph SEL["Select.Node#lt;int, int, …#gt; — _selector"]
            subgraph WHE["Where.Node#lt;int, …#gt; — _predicate"]
                ARR["SystemArray.Node#lt;int#gt; — int[]"]
            end
        end
    end
```

Each node file under [`ValueLinq/Nodes/`](../ValueLinq/Nodes) holds one operator: its `Node`, its push forwarder
`Push`, and its pull enumerator `Pull`. Sources (arrays, lists, spans, `Range`, `IEnumerable<T>`, …) are under
[`ValueLinq/Containers/`](../ValueLinq/Containers). Aggregators are under
[`ValueLinq/Aggregation/`](../ValueLinq/Aggregation).

A query runs in one of two modes. An aggregation **pushes**: the source drives a loop and hands each element down the
chain. `foreach` **pulls**: the consumer asks the chain for each element in turn. Both run from the same nodes.

## The JIT as the optimiser

The design hands as much as possible to RyuJIT:

- **Specialisation.** A generic method instantiated over a struct is compiled separately for that struct. Every
  chain therefore gets its own code, in which each call between operators is a direct call to a known method, and so
  a candidate for inlining.
- **Constants per type.** Properties the chain asks about are `static abstract` members: `AllowBatchesWhen`
  (which batches a taker accepts), `AllowInitialise`, and `RebuildChanges` (whether a pull chain can be reshaped).
  `typeof(T) == typeof(int)` tests work the same way. In a specialised method each is a constant, and the branch it
  guards is removed.
- **Dynamic PGO.** Delegates (`Func<T, U>`) are devirtualised by guarded devirtualisation when one target dominates.
  The push loop is written inside each source and is not wrapped in exception handlers, so PGO instruments and
  optimises it as an ordinary loop. Several layout decisions exist only to keep this. For example, a `finally` is
  used rather than a `catch`, and initialisation is inline. They are recorded in the
  [project notes](../CLAUDE.md).
- **Register promotion.** A forwarder running a span copies its downstream to a local, runs the span against the
  copy and writes it back, so the aggregator's accumulator stays in a register rather than making a store-to-load
  round trip through the chain per element. See
  [`BatchFromLocalCopy.md`](../ValueLinq/Docs/BatchFromLocalCopy.md).
- **Value delegates.** Where a delegate's indirect call is the cost, a struct implementing `IFunc<T, U>` can be passed
  in its place. It is called through its constraint and inlined like the rest of the chain.

Nothing is cached between calls. Each call pays for its own decisions, so an optimisation must pay on a single call
rather than on a benchmark loop's repetition.

## Push

### Filling the chain

An aggregation is an `IPushEnumerator<T>` struct. Each operator wraps it in its forwarder struct, so the chain is a
nesting of forwarders with the aggregator innermost. It is not built on the way down. `CreateViaPush` descends the
nodes, each passing its source a *filler* (a `ref struct` holding its configuration). At the source the chain is
declared once, as a single local, and filled in place through the fillers, each forwarder writing its own fields. The
aggregator is constructed directly in its slot.

```mermaid
sequenceDiagram
    participant Agg as Sum()
    participant Sel as Select.Node
    participant Whe as Where.Node
    participant Src as SystemArray.Node
    Agg->>Sel: CreateViaPush(filler: Sum)
    Sel->>Whe: CreateViaPush(filler: Select → Sum)
    Whe->>Src: CreateViaPush(filler: Where → Select → Sum)
    Note over Src: declare chain local<br/>Where.Push#lt;Select.Push#lt;Sum#gt;#gt;
    Src->>Src: Fill · Initialise · Started
    loop each element / span
        Src->>Src: chain.ProcessNext(x)
    end
    Src->>Src: release source · GetResult · Dispose
    Src-->>Agg: result
```

Building the aggregator elsewhere and copying it in is avoided. The copy is read back as one wide load over narrower
stores, which defeats store forwarding and measured at 6–10 ns a push.

### Batches

A source with contiguous storage hands its span to the chain whole, as a `Batch<T>`, to a taker that accepts it.
Both sides are `BatchKinds` flags tested before the batch is built, so a batch is never offered and declined. A leaf
aggregator receiving a span runs a vectorised loop where one applies (`Sum`, `Min`, `Max`, `Contains`, …).

```mermaid
flowchart LR
    L["List#lt;int#gt;"] -->|"span (Batch)"| S["Select.Push<br/>runs span from local copy"]
    S -->|"ProcessNext × n"| A["Sum"]
    L2["int[]"] -->|"span (Batch)"| A2["Sum<br/>vectorised"]
```

An `ICollection<T>` is handed over *cold*, its elements not yet copied out. A `ToArray` or `ToList` builder takes it
through the collection's own `CopyTo` into its storage, and an aggregator that reads every element takes it as a span
copied into a rented buffer.

### The push protocol

Every source and every operator that pushes another sequence obeys one protocol. The test suite checks it for each
source, run to the end, stopped early and thrown out of.

```mermaid
stateDiagram-v2
    [*] --> Filled: Fill
    Filled --> Started: Initialise (Begin, from the source end)
    Started --> Running: ProcessNext / ProcessBatch
    Running --> Running: true
    Running --> Released: false, or source ends
    Running --> Released: exception
    Released --> Result: GetResult (only if no exception)
    Result --> Disposed: Dispose
    Released --> Disposed: Dispose
    Filled --> Disposed: Dispose (never started)
    Disposed --> [*]
```

- Nothing goes in after a `false`.
- The source (its enumerator, a rented buffer) is released before anything else is called on the chain.
- `GetResult` is called at most once. `Dispose` is called exactly once, last, on every path.
- Setup that runs user code (Except's set, FullJoin's lookup) is a forwarder's `Begin`, run in `Initialise`.

## Pull

### The typed chain

Each node type has one pull enumerator, an `IPullEnumerator<T>` struct that builds itself from the node and creates
its source's enumerator in turn. `ValueEnumerable` carries the enumerator chain's type, so every extension method
names it in its return type:

```csharp
ValueEnumerable<U,
    Select.Node<T, U, TPrior>,
    Select.Pull<T, U, TPrior, TPriorEnumerator>>
```

`GetEnumerator` hands `foreach` the whole chain by value, and each `MoveNext` is a direct call. The node knows nothing
of its enumerator.

```mermaid
flowchart LR
    subgraph nodes["Nodes (what)"]
        direction LR
        N1["SystemArray.Node"] --> N2["Where.Node"] --> N3["Select.Node"]
    end
    subgraph enums["Enumerators (how to pull)"]
        direction LR
        E1["SystemArray enumerator"] --> E2["Where.Pull"] --> E3["Select.Pull"]
    end
    N3 -. "Create(in node)" .-> E3
    E3 -. "Create(in Where.Node)" .-> E2
    E2 -. "Create(in SystemArray.Node)" .-> E1
```

Where a chain's type cannot be named (behind `IEnumerable<T>`, or over a sequence typed only at run time) it is held as
`PullEnumerator<T>`, the one heap enumerator, at one virtual call per element.

### Rebuilding

When a chain is boxed into `PullEnumerator<T>`, it is first rebuilt over what its source actually holds. An
`IEnumerable<T>` that is really an array becomes an array source, a `List<T>` a list source, and so on, so the one
virtual call is at the top of the chain rather than per element at the bottom.

`IPullEnumerator.Rebuild` walks the chain downwards. Each enumerator hands its source an `IChainBuilder`, which
rebuilds its node over whatever node and enumerator type come back up. `RebuildChanges`, a constant per chain type,
skips the walk when nothing below can change.

```mermaid
flowchart TB
    T["Take.Pull"] -->|"Rebuild(builder: Take)"| S["Skip.Pull"]
    S -->|"Rebuild(builder: Skip → Take)"| O["OrderBy.Pull"]
    O -->|"Rebuild(builder: …)"| E["SystemIEnumerable source"]
    E ==>|"is int[] → SystemArray.Node"| O
    O ==>|"MergeSkipTake? yes → BufferedWindow.Node"| S
    S ==>|"merged"| T
    T ==>|"PullEnumerator#lt;int#gt; over BufferedWindow"| R(("foreach"))
```

The walk also merges a `Skip` or `Take` into a buffering node below it (OrderBy, Reverse, Shuffle). Skip's and
Take's chain builders ask the enumerator that comes back up for `MergeSkipTake`. A buffering node answers with a
`BufferedWindow.Node`, a window whose type stays nameable, and only the window's elements are put in order.

`PreferRebuilt` is true from a `Skip` or `Take` over such an enumerator. This is the one case where `GetEnumerator`
hands `foreach` the rebuilt chain's heap enumerator rather than the typed chain, because ordering only the window
outweighs the allocation. For every other chain the allocation costs more than the rebuild saves.

The contract between an enumerator and whoever pulls it is in [`PullProtocol.md`](../ValueLinq/Docs/PullProtocol.md).

## Requests

Nodes optimise each other through requests. A request is a struct from
[`ValueLinq/Optimizations/`](../ValueLinq/Optimizations), passed down through `INode.TryPushOptimization`. A node
answers the requests it can and declines the rest, and the asker then falls back to the general path.

```mermaid
sequenceDiagram
    participant TA as ToArray()
    participant Sel as Select.Node
    participant Skp as Skip.Node
    participant Lst as SystemList.Node
    TA->>Sel: CopyTo (dry run): how many?
    Sel->>Skp: AsSpanSource?
    Skp->>Lst: AsSpanSource (re-sliced past n)
    Lst-->>Skp: list's span, sliced
    Skp-->>Sel: span
    Sel-->>TA: count = span.Length
    TA->>TA: allocate exact array
    TA->>Sel: CopyTo (real)
    Sel->>Sel: destination[i] = f(span[i])
```

| Request | Asked by | Answered by, for example |
|---|---|---|
| `Count` | `Count()`, `TryGetNonEnumeratedCount` | collections, `Range`, `Skip`/`Take` over a counted source |
| `CopyTo` | `ToArray`, `ToList` | a node made of parts, each part copying into its slice |
| `SkipTake` | `Skip`, `Take` | a list or array (a slice), `Select` (the source's slice, projected) |
| `ElementAt` | `First`, `Last`, `ElementAt` | an indexable source; `Select` runs its selector on the found element only |
| `AsSpanSource` | anything that can run a span | arrays, lists, spans, memory |
| `MaterialiseArray` / `MaterialiseList` | `ToArray`, `ToList` | `Concat`, copying counted parts straight into place |
| `Contains`, `Any`, `LastMatch`, `Reverse`, … | the matching aggregates | the nodes that can answer without enumerating |

The rules that keep this safe:

- A **dry run** (`Count`, `CopyTo` without a destination) is answered only without enumerating and without running
  user code.
- A request asked for real is never declined once its dry run has been answered, and a node that declines has run
  nothing.
- `Select` declines a real `Count` and is pushed, running its selector once per element, because `System.Linq` does.

Each request is an interface marked `[Optimization]`, and a node declares the interfaces it answers. The dispatch is
generated (see [Source generators](generators.md#requests)): one guard per interface on the request's type,
calling the method directly. Each guard is a type test that folds to a constant in the specialised code.

## `IEnumerable<T>` boundaries

A Cistern chain handed out as `IEnumerable<T>` and consumed by another Cistern chain is recognised through the
internal `IPullEnumerable<T>`. Each chain is compiled once, and the boundary costs one virtual call per element:

- **pulled**, through the inner chain's own `PullEnumerator<T>`;
- **pushed**, through an `ErasedChain.PushSink<T>` holding the outer chain. A span from the inner chain's source
  crosses the boundary whole.

```mermaid
flowchart LR
    subgraph inner["inner chain (compiled once)"]
        I1["source"] --> I2["…"]
    end
    subgraph outer["outer chain (compiled once)"]
        O1["…"] --> O2["aggregator"]
    end
    I2 -->|"PushSink: 1 virtual call / element<br/>or / span"| O1
```

`Fuse()` instead compiles the outer chain through the inner chain's node, producing one fully specialised loop at the
cost of a new instantiation for every pairing.

### Handing a chain out: `ToEnumerable`

`ToEnumerable()` hands a chain out as one heap `IEnumerable<T>`. The chain is rebuilt once at the call, over whatever
its `IEnumerable<T>` sources hold, so neither its enumerators nor its pushes walk it again.

Where the chain's `CopyTo` dry run answers, meaning its length is known without enumerating, the object handed out is
a read-only `ICollection<T>` and `IReadOnlyCollection<T>`:

- `Count` is the dry run's answer, and nothing runs;
- `CopyTo(T[], int)` is the chain's `CopyTo` request, so each part writes straight into the destination (a `Select`
  over an array projects into it, a `Concat` copies each part into its slice);
- `Contains` is the chain's `Contains`.

A consumer that knows nothing of Cistern, such as `new List<T>(e)`, `List<T>.AddRange` or `System.Linq`'s `ToArray`,
sees an `ICollection<T>`. It sizes its storage exactly and fills it with one `CopyTo`,
rather than enumerating and growing.

```mermaid
flowchart LR
    Q["array.Select(f).ToEnumerable()"] -->|"CopyTo dry run: 10,000"| C["ChainCollection (ICollection#lt;T#gt;)"]
    C -->|"Count → 10,000"| L["new List#lt;T#gt;(e)"]
    L -->|"CopyTo(items, 0)"| C
    C -->|"destination[i] = f(array[i])"| L
```

`ToEnumerable(EnumerableOptions.Fuse)` hands the chain out as a node that is not an `IPullEnumerable<T>`, so any
Cistern chain pushing through it is fused with it.

## Generated code

The repetitive parts of every operator are written by Roslyn source generators: constructors, fillers, chain
builders, request dispatch, `Dispose`, `GetResult`, and the marker interfaces. The hand-written file carries the
algorithm (`ProcessNext`, `MoveNext`, `Current`, and the request answers), and nothing else. See
[Source generators](generators.md).

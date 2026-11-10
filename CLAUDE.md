# Cistern.ValueLinq

A LINQ implementation built on value-type pipelines: operators are structs composed at compile time, so a query
allocates nothing unless it has to. `using Cistern.ValueLinq;` in place of `System.Linq` is the whole migration.
Candidate performance work is listed in `Optimizations.md`.

Every project targets `net11.0` only, except `Cistern.ValueLinq.Generators` (netstandard2.0), which carries its own
`System.Index`/`Range` and `IsExternalInit` polyfills rather than a package.

## Tests

- `Tests/Linqs.Tests.csproj` is the corefx LINQ suite, imported: `src/libraries/System.Linq/tests` of dotnet/runtime at
  `281b6ea21a54509f3368b79ae87e6d85adbe710c`, plus the `Common/tests` files it links (under `Tests/Common/`), moved
  from `System.Linq.Tests` to `Linqs.Tests` so `System.Linq`'s operators are not in scope (its interface types are
  written qualified). `Tests/TestUtilities/` stands in for the runtime repo's test packages. No test of our own goes in
  it. A skipped test carries a `CISTERN.VALUELINQ` reason; one that cannot compile against Cistern's types has its body
  under `#if !CISTERN_VALUELINQ`, and one that only needs a different static type keeps the upstream line in the
  `#else` of an `#if CISTERN_VALUELINQ`.
- `Cistern.ValueLinq.Tests` is for what the corefx suite does not cover (Cistern-only surface, push-chain state and
  disposal). It shares no code with the imported suite.
- `Cistern.Profiling` is a console host for driving a test body under a profiler.

## Architecture

Every operator is an `INode<T>` struct wrapping its source node. Of the chain contracts only `IPullEnumerator` and
`INodeEnumerator` are public, because the constraints of the public `ValueEnumerable` and `ValueEnumerator` name them;
`IPullEnumerator`'s members are internal. A query runs in one of two modes.

### Push

Aggregations (`Sum`, `ToList`, `First`, ...) push: the aggregator is an `IPushEnumerator<T>` struct, each operator wraps
it in its forwarder struct (`Select.Push`, ...), and the source container loops its elements into the chain.

The chain is not built on the way down. `CreateViaPush` takes an `IPushFiller<TPushEnumerator>`; a wrapping node passes
its source a filler of its own (a `readonly ref struct` holding its state and an `ErasedRef` to the filler above), and
where the descent ends the chain is declared once and filled through them (`PushFiller.Fill`), each forwarder writing
its own fields. The innermost slot is either built in place through the push enumerator's `ICreate.Create` (a partial
struct marked `[Create]`, or `[Aggregator]` for a leaf aggregator, declaring exactly one constructor, from which the
generator writes `Args` and `Create`), or takes a chain built already, `Downstream<T>` (the downstream a node pushes its
parts into). An aggregator must not be built elsewhere and handed to `Downstream`: copied into its slot, it is read
back as one wide load over the narrower stores that built it, which defeats store forwarding and costs 6–10 ns a push.
A node that pushes through another node in its place passes its filler on unchanged.

A container with a span hands it whole, as a `Batch<T>` through `ProcessBatch`, to a taker whose
`IPushEnumerator.AllowBatchesWhen` (a constant per type) takes it, and otherwise loops element by element. Both sides
are `BatchKinds` flags: a batch's `Requires` is what a taker must accept, and every test is
`AllowBatchesWhen.Takes(Requires)`, made before a batch is built and by a forwarder handing one on, so a batch is never
offered and declined. An `ICollection<T>` is handed over cold, its elements not yet copied out: `ColdCopy` through the
collection's own `CopyTo` into the taker's storage (the ToArray and ToList builders), `ColdSpan` through `GetSpan`, which
copies into a rented buffer (the aggregators that read every element, and a forwarder that hands every element on over
one of them). A cold batch offers `ColdSpan` as an alternative only for a collection otherwise read through its boxed
`IEnumerator<T>`: over `HashSet`, `Queue`, `Stack` and `LinkedList` of a value type, whose struct enumerator is called
directly, the copy is a second pass and measured slower. Over a reference type those struct enumerators run as shared
code and measured at twice the boxed enumerator's time pushed, so the push takes them only for a value type (pulled
they are faster, and kept). A chain behind an erased sink is asked at run time (`PushSink.Takes`).

### Pull

`foreach` and `IEnumerable<T>` pull. Each node type has one pull enumerator, an `IPullEnumerator<T>` struct that builds
itself from the node through `INodeEnumerator<TNode, TSelf>.Create`, creating its source's enumerator in turn; the node
knows nothing of it. `ValueEnumerable<TSource, TNode, TEnumerator>` carries the chain's enumerator type, so each
extension method names it in its return type, and `GetEnumerator` hands `foreach` the whole chain by value. What an
enumerator may expect of whoever pulls it, and owes in return, is in `ValueLinq/Docs/PullProtocol.md`.

Where a chain's type cannot be named it is held as a `PullEnumerator<T>`, the one heap enumerator, pulled with one
virtual `TryGetNext` per element: what `IEnumerable<T>.GetEnumerator` hands out, and the handle on a sequence typed only
at run time (`SystemIEnumerable.Node`, `CisternCast.Node`, `CisternOfType.Node` and the parts of a flattened chain of
`IEnumerable<T>` concats, pulled through `ErasedEnumerator`). A wrapper over a caller's `IEnumerator<T>` reads its
`Current` once per element.

`PullEnumerator.Create` boxes a chain over an `IEnumerable<T>` rebuilt over what the source holds at run time (an array,
a `List<T>`, any other `IList<T>` read by index, a collection's struct enumerator, else the source's `IEnumerator<T>`;
a Cistern chain stays as built), so the one virtual call per element is at the top rather than per source element at
the bottom. `IPullEnumerator.Rebuild` walks the chain down, each enumerator handing its source an `IChainBuilder` that
rebuilds its node over whatever node and enumerator type come back up; `RebuildChanges`, a constant per chain type,
skips the walk when nothing below can be replaced. Both are `static abstract`, so every new enumerator implements them:
a source builds itself as it is, an operator over one source passes the walk through, and an operator over two decides
which sides it rebuilds (a Concat of two `IEnumerable<T>`s rebuilds neither, so `TryCollectNodes` still flattens a chain
of them).

The walk also merges a Skip or Take into a buffering node below it (OrderBy, Reverse, Shuffle, the `IBuffered<T>`
nodes), the pull counterpart of the `SkipTake` request: Skip's and Take's chain builders ask the enumerator that comes
back up for `MergeSkipTake` (`IPullEnumerator.TryPullOptimization`, answered through `[PullOptimization]` interfaces),
which a buffering node's enumerator answers by building a `BufferedWindow.Node`, so the window's type stays nameable
and nothing is erased. `PreferRebuilt`, true from a Skip or Take over such an enumerator, is the one case where
`GetEnumerator` hands `foreach` the rebuilt chain's heap enumerator rather than the typed chain: sorting only the window
outweighs the allocation, which for every other chain costs more than the rebuild saves.

### Requests

Cross-node requests go through `INode.TryPushOptimization` with a request struct from `ValueLinq/Optimizations/`. A node
answers the ones it can and returns false otherwise; the asker falls back to the general path. Each request has an
interface marked `[PushOptimization]` (`IOptimizationCount`, ...) of one method named for the request, taking it by `in`
and the result by `out` (`CVL0001` on any other arguments). A node declares the interfaces it answers and writes each
method, non-public; the generator writes `TryPushOptimization`, one guard per interface on the request's type calling
the method directly (a call through the interface copies the node or, in shared code, costs a second dictionary
lookup), reading and writing through `Optimizations.Request`'s unchecked reinterpretations. The result's type is not
guarded, only asserted in Debug: a guard on it would not fold where the result is shared code, and reading the generic
context for it keeps the dispatch from being inlined into its asker.

A node that hands the requests it does not answer to another node (Skip and Take to their source re-sliced, a container
to the node over its memory) sets `Forwards = true` and writes a non-public `Forward<TRequest, TRequestResult>`; a method
that declines into it calls `Forward` itself. `[Operator]` and `[Materialized]` nodes get the dispatch generated and may
not write `TryPushOptimization`; any other node takes `[Answerer]`, whose dispatch is public, so a container built in
place over another's parts is called directly. A type whose whole answer is a forward chosen at run time writes
`TryPushOptimization` itself.

- `ToArray` and `ToList` size their result through `CopyTo`'s dry run (no destination: the exact count, running
  nothing) and then fill it through `CopyTo`, which a node made of parts answers by having each part copy into its
  slice; a container with a span answers neither, as both fall back to `AsSpanSource`. Where that declines, a Concat
  whose leading parts are collections or spans, or whose trailing parts are counted, answers `MaterialiseArray` or
  `MaterialiseList`: the parts between are pushed into an aggregator whose `GetResult` sizes the result and copies each
  counted part straight into its place.
- `Count`'s dry run, which `TryGetNonEnumeratedCount` asks, is declined by any node that would have to enumerate or run
  user code. Asked for real, a Select declines `Count` and is pushed, running its selector once per element, only
  because corefx's `SelectSideEffectsExecutedOnCount` and `RunSelectorDuringCount` pin System.Linq doing so. That is a
  straitjacket with no principle behind it: System.Linq runs a selector for nothing else it does not have to, so
  `Count` is the one aggregate where the selector's side effects are a promise, and it keeps every `Count` over a
  Select from answering without the Select.
- `Map` and `Filter` are `Select` and `Where` over a function the caller promises is pure, pushed through the same
  forwarders, and answer what those must not: Map's `Count` and `Any` from its source alone, any `ElementAt` found in
  the source and only it projected, a `SkipTake` its source cannot slice by a Skip moved below it, and `Reverse` as the
  projection over the reversed source; Filter's from-end `ElementAt` scanning from the end, and `Reverse` likewise.
  `Filter` over an `OrderBy` (also through a `Reverse` above or below it) or a `Shuffle` is typed as that node over the
  Filter, by overloads on `ValueEnumerable` whose receiver names the reordering node. Over a `Reverse` alone it stays
  above: whether sinking pays depends on what reads the result (an element access stops at the first match from the
  end; only a read of every element gains from the smaller buffer), which the type cannot know.
- A request asked for real is never declined once its dry run has answered, and a node that declines has run nothing:
  one whose answer may run user code (OrderBy's selection) under a node made of parts is asked only once that node has
  settled everything that could make it decline.
- `AsSpanSource` with `Stable` set is for an asker that holds the source across calls returning to its caller (a pull
  enumerator between `MoveNext`s); a `List<T>` declines it, to be enumerated and throw on a change as its enumerator
  does.
- A `SkipTake` reaching an `IEnumerable<T>` that cannot be sliced is answered with a `SystemEnumerableSkipTake`, which
  skips and takes at the enumerator, so the Skip and Take nodes above it leave the push. Skip and Take are separate
  nodes, folding only into their own kind.

### The push protocol

What a push does with the chain it fills, checked by `Cistern.ValueLinq.Tests/PushProtocol.cs` over every source and
the operators that push or pull other sequences, run to the end, stopped early and thrown out of:

1. A chain is started by `PushFiller.Fill`: filled (a forwarder's `Start` only sets its state), then initialised, then
   reported started through its fillers (`IPushFiller.Started`). Setup that may run user code (Except's set, FullJoin's
   lookup) is the forwarder's `Begin`, run by `Initialise` when `TPushEnumerator.AllowInitialise`, from the source's
   end, so an aggregator's (the pooled ones rent their presized buffer there) runs last. Each forwarder's part runs
   once, so a chain filled again from a `Downstream` is unchanged by the second call. Initialisation is inline and
   unprotected: what it builds is what Dynamic PGO optimises the loop over, and neither a handler around it nor building
   it out of line keeps that. So until a chain has started, nothing filling built holds anything to release, and a node
   that holds something its chain will own (an enumerator it opened, as Zip's second; a chain it hands on through
   `Downstream`, as Prepend's, Fork's, and `SystemIEnumerable`'s to a Cistern node) puts a `Guard` before the filler it
   passes down and releases it in a `finally` when the guard says the chain never started. A `finally`, never a
   `catch`: a `catch` around the push, into which the source's loop is inlined, loses what PGO does for the loop
   (measured at 40% on Distinct).
2. Elements go in until the source ends or the chain answers false, and nothing more after a false.
3. The source (its enumerator, a rented buffer, a cold batch) is released when the loop ends, normally or by exception,
   before anything else is called on the chain.
4. `GetResult` is called at most once, only after a run that did not throw, and after 3. It may run user code.
5. `Dispose` is called exactly once on every path, last, and is safe on a chain never initialised.
6. A chain reached through `PushForward` into a sink gets 2 and 3 only: the sink's owner initialises the chain before
   pushing any part into it, and takes the result and disposes it once every part has finished its own 3.
7. A forwarder's `GetResult` finishes its own part first and then passes it on: it releases a source of its own, or
   pushes what it held back into `_next` unless that halted, opening and releasing a source of its own for it where it
   has to (FullJoin's inner sequence, when its outer was empty). Its `Dispose` releases a source of its own not yet
   released and disposes `_next` in a `finally`; its `Initialise` runs its own `Begin`, then `_next`'s.

Each container writes its loop by hand: a helper running it would take the chain by `ref`, which costs the loop's
codegen. Nothing on the filling path handles exceptions: a handler in `PushFiller.Fill` keeps it from being inlined,
which measured about 15 ns a push.

### Chain options and `IEnumerable<T>` boundaries

A node passes on the `ChainOptions` its `CreateViaPush` was given, and an operator that pushes a second sequence (a
Concat part, a SelectMany inner sequence) passes it its own options. An `Options.Node` (what `Fuse()` makes) overrides
them for the part of the chain below it.

A Cistern chain reached as `IEnumerable<T>` is met through the internal `IPullEnumerable<T>` that `ValueEnumerable`
implements, so each chain is compiled once and the boundary costs a virtual call per element: pull through the inner
chain's own `PullEnumerator<T>`, push through an `ErasedChain.PushSink<T>` holding the outer chain, unless
`ChainOptions.Fuse` is set. Fused, the outer chain pushes through the inner chain's node, a new instantiation for every
pairing of inner and outer chain, which a program composing many chains through `IEnumerable<T>` (the corefx suite's
`CreateSources` most of all) multiplies into tens of thousands of JIT-compiled methods. The sink's forwarder takes every
span batch, so an inner container's span crosses whole; a cold batch crosses only once the sink's `Takes` has said the
outer chain takes it. `ToEnumerable(EnumerableOptions)` hands a chain out as one heap `IEnumerable<T>` over the chain
rebuilt once at the call (an `ICollection<T>` where `CopyTo`'s dry run answers); with `Fuse`, as an `INode` that is not
an `IPullEnumerable`, so whatever pushes through it is fused with it.

### Generated operators

The attributes are in `ValueLinq/Operator.cs`, and `Cistern.ValueLinq.Generators` writes the plumbing they ask for;
build with `-p:EmitCompilerGeneratedFiles=true` to read it under `obj/`. Every file in `ValueLinq/Nodes/` is one
`public static partial class` named for its operator and holding all of it: `Node`, `Push` and `Pull` first, the
operator's other types after, named through the class from outside (`Nodes.GroupBy.Lookup<,>`).

- A regular single-source operator holds `[Operator] Node<...>`, declaring its source field and its configuration
  fields (`[Propagate]` on those `Push` and `Pull` get a copy of; `ByRef = true` for a struct wider than a reference);
  `Push<...>`, declaring its state fields and `ProcessNext` (and `AllowBatchesWhen` and `ProcessBatch` where it runs
  batches itself); and `Pull<...>`, declaring its state fields, `MoveNext` and `Current`. Everything else is generated,
  and anything the hand-written side declares other than `TryPushOptimization` displaces its generated version (a node
  writing its own `CreateViaPush` may have no `Push`; a `Pull` writing `RebuildChanges` writes `PreferRebuilt` with
  it). A `partial void Start()` on `Push` or `Pull` sets a state field's non-default start value; it runs as the chain
  is filled, so on `Push` it runs no user code, and setup that does is a `Begin` method (push protocol rule 1). A `Pull`
  needing type parameters beyond the node's and its source enumerator declares its own constraints; a `Push` field of
  type `ChainOptions` is filled from the push's options.
- An operator that materialises its source into another node and runs as that node from then on (Reverse into a
  `PooledBuffer.Node<T>`, a buffer rented for the one enumeration) holds `[Materialized] Node<...>`, declaring its
  fields and `Materialize(in ChainOptions)`, and `Pull<...>`, declaring one field, that node's enumerator. Its `Rebuild`
  is the identity, as the chain below was run to materialise. Its request methods answer what it can without
  materialising.
- A hand-written forwarder (a partial struct named `...Push` with a `Fill` returning `ref` to its downstream) takes
  `[Forwarder]`; any other pull enumerator takes `[Puller]` and writes its state, `MoveNext`, `Current` and, if it
  enumerates a node, a private static `Create(in TNode node)`. `IPullEnumerator` has no default members, so an
  enumerator none of the three attributes covers fails to compile.

The chain interfaces' members name their type parameters `TryPushOptimization<TRequest, TRequestResult>`,
`CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>`, `GetResult<TPushResult>`, `Rebuild<TOwnNode, TBuilt,
TBuilder>`, `TryPullOptimization<TRequest, TOwnNode, TBuilt, TBuilder>` and `Build<TNode, TEnumerator>`, and
`Forward<TRequest, TRequestResult>` likewise; every implementation names them the same, and a node's, forwarder's or
enumerator's own type parameters are never `TOwnNode`, `TBuilt` or `TBuilder` and never start with `TPush` or
`TRequest` (an `[Operator]` node's are not `TNode` or `TEnumerator` either). `CVL0001` otherwise. `Push` and `Pull`
carry `[StructLayout(LayoutKind.Auto)]`, as their fields are split across two declarations.

### Why forwarders run batches from a local copy

A forwarder's batch method copies its downstream to a local, runs the span against the copy and writes it back, so the
leaf aggregator's accumulator is register-resident instead of a store-to-load round trip per element through the by-ref
chain. The mechanism, its constraints, why a `Where` hands its survivors on one at a time, and the JIT version it was
verified on are in `ValueLinq/Docs/BatchFromLocalCopy.md`. A new aggregator is marked `[Aggregator]`, not `[Create]`, or
a `SelectMany` with a result selector runs each inner span against it by `ref` rather than from a local copy. An
aggregator writing a `Dispose` is run in place by an `IEnumerable<T>` source's loop, as what it releases may be in a
copy an exception discards; one writing none gets an empty one generated (a hand-written empty `Dispose` warns,
`CVL0002`). The markers the attribute writes may not be listed by hand (`CVL0001`).

### Generated wrappers

The extension methods over the source types are T4 output (`ValueLinq/EnumerableWrappers/*.tt`; `Collections.tth`
lists the collection types), generated into `obj/t4/` by the build and not checked in. `dotnet-t4` compiles a template
with the SDK of the default .NET install, which must therefore exist. An overload that needs hand-written logic is
removed from the template's table and written in the matching `Enumerable.*.cs` partial. `ValueEnumerable`'s extension
methods are hand-written in `ValueEnumerable/ValueEnumerable.cs`; `ValueEnumerable/ExtensionFunctions.tt` is commented
out and generates nothing.

## Benchmarks

Each benchmark lives in its own folder as partial files, one per library (`Benchmark.cs` holds data, params and
`SanityCheck`; `Benchmark.Handcoded.cs` is the baseline; `Benchmark.CisternValueLinq.cs`, `Benchmark.Linq.cs`,
`Benchmark.ZLinq.cs`, ...), so each file carries only its own library's `using`. A new class is registered in
`Program.cs`'s sanity list, which runs before BenchmarkDotNet. Comparison libraries behind `#if LINQAF` /
`CISTERNLINQ` / `STRUCTLINQ` / `HYPERLINQ` are switched by the csproj's `DefineConstants`.

Run the benchmark exe from a directory inside the repository: BenchmarkDotNet finds the project by searching the tree
above the working directory. Keep each case in a process of its own (no `--inProcess` for a measurement): two Cistern
methods driving the same operator chain through different lambdas would corrupt each other's delegate guards, and a
parameter value that takes one branch (`Take(0)`) would train the code another value is measured on.
`CISTERN_LENGTHS` (comma-separated) replaces the Coverage benchmarks' lengths, 10, 1000 and 100000 by default.

A case can be bimodal across processes of the same binary: SkipTakeSum's length-0 Cistern rows sit at one of two times
several nanoseconds apart, decided per process. One process's row is never read as a change on its own; a difference
counts once it reappears in a rerun of the same case.

`Benchmark/Reports/Report.cs` runs the Parity or Toub suite and writes it up as HTML; its usage is in its header. It
stops on anything it has no place for, so a Toub entry added, renamed or removed needs its line in `ToubLabels`, and a
new library in either suite needs its columns in the report.

## Conventions

- Source files are UTF-8 with BOM.
- A comment says only what a reader could not work out from the code or from a library used as intended.
- Benchmarks and sanity checks compare against the hand-coded loop, never against `System.Linq`, except under
  `Benchmark/Parity/`, whose classes measure Cistern against System.Linq's own optimizations (one class per section of
  an audit of System.Linq's source, one method per entry and library: System.Linq, Cistern, ZLinq where its API covers
  the entry, and a naive hand-coded version written against the source's declared type, without System.Linq's runtime
  type tests) and check every version's result, and Cistern's selector call counts, against System.Linq's; and
  `Benchmark/Toub/`, one class per .NET release importing the LINQ benchmarks of that release's performance post at the
  post's sizes and data, laid out and checked as a Parity class.
- An optimisation pays on a single call. The library holds no state shared between calls (no static cache, no
  last-seen type, no remembered answer) and nothing tuned to the same input arriving again, which only a benchmark
  loop rewards.
- `[module: SkipLocalsInit]` is set for the library.
- The null-forgiving `!` is acceptable. Prefer annotations that do not change codegen (`?`, `[MaybeNullWhen]`,
  constraints); where only a code change would silence a nullable warning (an extra local, an extra copy per element in
  a hot loop), use `!`. No `#nullable disable` or pragmas.

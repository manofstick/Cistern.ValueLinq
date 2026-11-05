# The pull protocol

What an `IPullEnumerator<T>` may expect of whoever pulls it, and what it owes in return. Every enumerator in a chain is
both: it is pulled by the one above and pulls the one below.

## Creation

An enumerator is built from its node by `INodeEnumerator<TNode, TSelf>.Create`, which creates the source's enumerator
in turn. A node that yields nothing whatever its source holds (a Take of nothing, an empty range, a Skip past a source
of known length) does not create its source's enumerator, so a buffering node below is never run; its `Dispose` then
has nothing to release.

## MoveNext and Current

- `Current` is read at most once after each `MoveNext` that returned true, and never otherwise.
- So `Current` may do the work: a transforming enumerator computes its element on read (Select runs its selector in
  `Current`), and the value passes up the chain in a register rather than through a field per level.
- An enumerator that needs its source's element more than once (Where tests it and then hands it on; TakeWhile,
  Except, the first element past a SkipWhile) reads `Current` once and keeps the value in a field.
- An enumerator that discards elements (Skip, a TakeRange's leading skip) still reads their `Current`, so a selector
  below runs, and throws, for them as it does in push mode over a source that cannot slice itself (in push a Select
  over one that can answers the `SkipTake` by slicing it, and runs its selector only for the elements kept).
- Nothing calls `MoveNext` again once it has returned false: the holders of a chain (`ValueEnumerator`,
  `PullEnumerator`) stop there, and an enumerator that loops over its source stops at its source's first false.

## Dispose

`Dispose` is called once, by the chain's holder, and each enumerator disposes the enumerator it created below it.
`PullEnumerator` disposes the chain as soon as `MoveNext` returns false, as System.Linq's iterators dispose their
source there, and does not touch it again; its own `Dispose` is then a no-op. Both holders' `Current` is default after
the end.

`ValueEnumerator`, which `foreach` holds by value, disposes the chain in its `Dispose` only, which `foreach` calls
straight after the last `MoveNext`. Disposing at the end of `MoveNext` puts a call on the loop's exit path that the JIT
does not inline there (the path is rarely run, and a `Dispose` holding a `try`/`finally`, as Zip's does, is not small);
the call takes the enumerator's address, which keeps the whole chain's state in memory, so every step of the loop loads
and stores each counter instead of keeping it in a register. Measured on net11 RC2, a `foreach` over a `Select` of an
array ran in less than half the time without it.

## TryPushRest

A push that has to drain an enumerator into a sink (a Cistern chain met as `IEnumerable<T>`, a Concat part) asks
`TryPushRest` first. An enumerator with
parts it has not opened (a Concat's) pushes them into the sink and returns true; otherwise it returns false, having
done nothing, and the caller pulls what is left. The call is through the interface, so a struct that does not implement
it is boxed once per drain.

## The static members

These are constants or calls per enumerator type, used without an instance:

- `Rebuild`: the walk, the pull counterpart of `CreateViaPush`, that every enumerator implements. Each enumerator hands
  its source a chain builder that rebuilds its own node over whatever comes back up; an `IEnumerable<T>` source comes
  back as the container it holds at run time.
- `RebuildChanges`: the rebuild would produce a different chain, so `PullEnumerator.Create` walks rather than boxing
  the chain as it is.
- `PreferRebuilt`: `foreach` enumerates the chain `Rebuild` produces, boxed, rather than the typed chain.
- `TryPullOptimization`: the pull counterpart of `TryPushOptimization`, a request asked of an enumerator type during
  the rebuild with a request struct from `ValueLinq/Optimizations/`, answered or declined. An enumerator declares the
  `[PullOptimization]` interfaces of the requests it answers and writes each one's static method; the generator writes
  the dispatch. The request goes to the enumerator rather than the node because only the enumerator names its node's
  type with every constraint the answer needs.

`IPullEnumerator` has no default members: the generator gives every enumerator the ones it does not write. An
`[Operator]` or `[Materialized]` node's `Pull` gets them with the rest of its plumbing; every other enumerator carries
`[Puller]`. It writes a `Current` and, where it enumerates a node, a private static `Create(in TNode node)`, and declares
neither interface: `[Puller]` declares `IPullEnumerator<T>` over `Current`'s type and `INodeEnumerator<TNode, Self>` over
`Create`'s node, with the explicit `Create` forwarding to the enumerator's own. It also writes `RebuildChanges` and
`PreferRebuilt` false, a `Rebuild` handing back its own node (a throw for an
enumerator with no `INodeEnumerator`, which nothing rebuilds), an empty `Dispose`, a declining `TryPushRest`, and the
`TryPullOptimization` dispatch. A `TryPushRest` is never left to an interface default because calling a default
interface member on a struct boxes it.
- `MergeSkipTake`, the one pull request: a Skip's or Take's chain builder asks the enumerator below it to build the
  window of its node that the skip and take leave, in place of the Skip or Take rebuilt over it. OrderBy, Reverse,
  Shuffle and `BufferedWindow` answer it, the last composing a second Skip or Take into its window.

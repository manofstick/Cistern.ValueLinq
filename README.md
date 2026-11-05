# Cistern.ValueLinq

LINQ to Objects, built from structs.

The architecture was designed in [2020](docs/history.md). It has now been filled in, with the assistance of Claude, to
cover the whole `System.Linq` surface. Improvements to the JIT since then let it run [faster](#benchmarks) than
`System.Linq` and ZLinq on most of the queries measured. Its architecture, value types throughout, still lets a chain
be optimised at run time: nodes rewrite each other, and a typed chain is reshaped where a different shape pays.

The design pays in full only where the element types are value types. For a reference type the JIT compiles one
shared implementation over the canonical type (`__Canon`) and looks up the exact types at run time. Calls through
shared code are inlined far less often, and inlining is what the performance here rests on. Sharing gains little in
return, since each chain is a type of its own and two chains rarely share an instantiation. A future JIT might allow
unshared code to be requested for chosen types, or let PGO specialise shared code where it runs hot. Neither exists yet.

Nearly all of the corefx `System.Linq` test suite runs and passes against it ([skipped tests](#skipped-tests)). That includes `System.Linq`'s
inconsistencies: whether a selector is run for its side effects or treated as free of them varies from operator to
operator, and Cistern follows each case.

At the least it is a curiosity. It should also serve the JIT team as a demanding benchmark of generic specialisation
over value types.

```csharp
using Cistern.ValueLinq;   // in place of using System.Linq;
```

That `using` is the whole migration. The operators keep their names, signatures and results. What changes is the type
each query is built from.

[History](docs/history.md) · [Architecture](docs/architecture.md) · [Source generators](docs/generators.md) ·
[Benchmarks](#benchmarks)

---

## What it is

Every operator is a generic struct that wraps the struct of the operator before it. A query such as

```csharp
array.Where(x => x > 0).Select(x => x * 2).Sum()
```

is, before `Sum` runs, a single value of the type

```
Select.Node<int, int, Where.Node<int, SystemArray.Node<int>>>
```

The JIT compiles a specialisation of each method for each struct instantiation. The whole chain is therefore known
when the loop is compiled: the calls between operators are direct, and the inliner can collapse them into one loop.
Generics do the work that a hand-written loop does by knowing the source type.

## Properties

- **Compatibility.** The `System.Linq` test suite from dotnet/runtime is imported in full (`Tests/`) and runs against
  Cistern. Behaviour follows `System.Linq` where it is observable, including where that behaviour is not uniform:
  - `Count()` over a `Select` runs the selector once per element, as `System.Linq` does, even though no other
    aggregate runs a selector it does not need.
  - A `List<T>` is read as a span where `System.Linq` reads it as a span. It is enumerated, and throws when modified
    mid-enumeration, where `System.Linq` enumerates it.
  - Selectors run for the elements `System.Linq` runs them for: not for elements a `Skip` passes over in a sliceable
    source, and not for elements other than the one `First`, `Last` or `ElementAt` returns.

  A test skipped for Cistern carries a `CISTERN.VALUELINQ` reason. The skips are summarised under [Skipped tests](#skipped-tests).
- **Allocation.** A chain is a value on the stack. An aggregation over an array, a `List<T>`, a span or a `Range`
  allocates nothing beyond its result. Working buffers that do not outlive the call (the `ToArray` and `ToList` builders,
  a collection copied out to be read as a span) are rented from the shared pool.
- **Two execution models, both typed.** Aggregations push. `foreach` pulls. Each is compiled from the same nodes, and
  neither erases the chain's type.
- **Cross-operator optimisation.** Nodes answer requests from the nodes above them (`Count`, `CopyTo`, `SkipTake`,
  `ElementAt`, …), so `Skip(n)` over a `List<T>` becomes a slice, `Select(f).First()` runs `f` once, and a
  `Concat` of collections copies each part straight into the result.
- **`IEnumerable<T>` boundaries.** A Cistern chain handed out as `IEnumerable<T>` and consumed by another Cistern
  chain is recognised. It is pulled or pushed through its own compiled chain rather than through `IEnumerator<T>` per
  element. `ToEnumerable()` hands a chain out as an `ICollection<T>` where its length is known without enumerating, so
  a consumer such as `new List<T>(e)` sizes its storage exactly and fills it with one `CopyTo`.

## Push and pull

### Push

An aggregation (`Sum`, `ToList`, `First`, `Max`, …) builds a chain of forwarder structs ending in the aggregator,
then hands it to the source. The source runs its own loop, so an array, a list and a `HashSet<T>` each get the loop
best suited to them. A source with contiguous storage hands its span to the chain whole, and the aggregator can then
run a vectorised loop.

```mermaid
flowchart LR
    S["int[]<br/>source loop"] -->|"ProcessNext(x)"| W["Where.Push"]
    W -->|"ProcessNext(x)"| P["Select.Push"]
    P -->|"ProcessNext(y)"| A["Sum<br/>aggregator"]
    A -.->|"GetResult()"| R(("result"))
```

### Pull

`foreach` pulls. Each operator has a struct enumerator, and `ValueEnumerable<TSource, TNode, TEnumerator>` carries the
whole enumerator chain's type, so `GetEnumerator` returns the complete chain by value. `MoveNext` is a direct call into
it. No interface dispatch is involved.

```mermaid
flowchart RL
    C["foreach"] -->|"MoveNext()"| P["Select.Pull"]
    P -->|"MoveNext()"| W["Where.Pull"]
    W -->|"MoveNext()"| S["SystemArray enumerator"]
```

### Overriding the static type where it pays

A statically typed pull chain cannot change shape at run time, but some queries are much cheaper in a different shape.
`OrderBy(k).Skip(100).Take(10)` only needs the ten elements of the window, not the whole sort. On the pull side,
`Skip` and `Take` ask the ordering below them whether it can absorb them. The ordering answers by building a
*buffered window* node, which puts only the window's elements in order. `foreach` is then handed this rebuilt chain in place of
the typed one.

```mermaid
flowchart LR
    subgraph typed["As written"]
        direction LR
        A1["int[]"] --> O1["OrderBy"] --> S1["Skip(100)"] --> T1["Take(10)"]
    end
    subgraph rebuilt["As enumerated"]
        direction LR
        A2["int[]"] --> W2["BufferedWindow<br/>orders only elements 100–109"]
    end
    typed ==>|"rebuilt at GetEnumerator"| rebuilt
```

The push side does the same through its `SkipTake` request.

## Benchmarks

Full results: [Parity suite](https://manofstick.github.io/Cistern.ValueLinq/Benchmark/Reports/Results/parity-report.html) · [Toub suite](https://manofstick.github.io/Cistern.ValueLinq/Benchmark/Reports/Results/toub-report.html)

The following are from the Parity suite (`Benchmark/Parity/`), which compares each query against `System.Linq` and
[ZLinq](https://github.com/Cysharp/ZLinq) and checks that every version returns the same result. The multiple after each System.Linq and ZLinq time is that library's time divided by Cistern's. Sources hold
10,000 elements. The suite ran on .NET 11 RC2, out of process with one process per case, BenchmarkDotNet
`--job short`.

| Query | Cistern | System.Linq | ZLinq | Allocated (C / L / Z) |
|---|--:|--:|--:|--:|
| `object[]`.`OfType<string>().Select(s => s.Length).Sum()` | **20.0 µs** | 62.5 µs (3.1×) | 35.8 µs (1.8×) | 0 B / 104 B / 0 B |
| `List<int>`.`Skip(n/4).Take(n/2).Select(f).Sum()` | **2.77 µs** | 15.1 µs (5.5×) | 5.54 µs (2.0×) | 0 B / 152 B / 0 B |
| eight `List<int>`s `.Concat(…)` ×7 `.Sum()` | **0.76 µs** | 26.0 µs (34×) | 32.2 µs (42×) | 32 B / 760 B / 0 B |
| `int[]`.`Append(…)` ×4 `.Prepend(…)` ×4 `.Sum()` | **1.01 µs** | 34.0 µs (34×) | 17.6 µs (17×) | 1,008 B / 976 B / 0 B |
| `IEnumerable<int>`.`TakeLast(10).ToArray()` | **24.0 µs** | 73.5 µs (3.1×) | 53.8 µs (2.2×) | 224 B / 376 B / 104 B |
| iterator `.Concat(Collection<int>).Concat(Collection<int>).ToArray()` | **3.20 µs** | 48.2 µs (15×) | 93.4 µs (29×) | 78.9 KB / 78.4 KB / 78.2 KB |
| `int[]`.`OrderBy(k).Select(f).First()` | **7.35 µs** | 10.3 µs (1.4×) | 846 µs (115×) | 0 B / 144 B / 128 B |
| `int[]`.`OrderBy(k).Skip(100).Take(10).ToArray()` | **93.8 µs** | 138 µs (1.5×) | 126 µs (1.3×) | 200 B / 117.6 KB / 224 B |
| `foreach` over `int[]`.`OrderBy(k).Skip(100).Take(100)` | **29.2 µs** | 34.3 µs (1.2×) | 33.4 µs (1.1×) | 536 B / 117.5 KB / 160 B |

The table shows where Cistern is clearly ahead, and not every result is like this. Across all 379 Parity entries:

| Cistern against | ahead (≤ 0.9×) | level | behind 1.1–2× | behind ≥ 2× |
|---|--:|--:|--:|--:|
| System.Linq | 194 | 121 | 46 | 18 |
| ZLinq | 223 | 79 | 53 | 18 |

### Where Cistern is behind

The weak point is the very small query: an answer that takes constant time (`Count`, `First`, `ElementAt` and the
like), or one over an empty source. Of the 126 Parity entries that Cistern runs in under 100 ns, System.Linq or ZLinq
is faster on nearly half (58). Cistern recovers the source's type and builds its chain before answering, which costs
a few nanoseconds that System.Linq's single type test, or ZLinq's constant-folded answer, does not.

| Query | Cistern | System.Linq | ZLinq |
|---|--:|--:|--:|
| `IEnumerable<int>` over a `HashSet<int>`, `.Count()` | 4.6 ns | **0.8 ns** (0.18×) | 4.7 ns (1.0×) |
| `List<int>`.`Concat(List<int>).Count()` | 11.0 ns | 9.5 ns (0.87×) | **1.3 ns** (0.12×) |

At microsecond scale the differences are smaller, apart from `Shuffle`, where the method differs:

| Query | Cistern | System.Linq | ZLinq | |
|---|--:|--:|--:|---|
| `int[]`.`ToLookup(k).Count` | 62.4 µs | 55.8 µs (0.89×) | **43.6 µs** (0.70×) | |
| iterator `.Reverse().ToArray()` | 16.6 µs | **13.0 µs** (0.79×) | 26.0 µs (1.6×) | |
| `List<int>`.`Where(f).ToArray()` | 27.0 µs | **22.1 µs** (0.82×) | 29.7 µs (1.1×) | |
| `int[]`.`Shuffle().ToArray()` | 87.1 µs | 93.7 µs (1.1×) | **19.5 µs** (0.22×) | ZLinq has its own generator and a batched Fisher–Yates ([dotnet/runtime#111015](https://github.com/dotnet/runtime/pull/111015), unbiased, not adopted by .NET). System.Linq and Cistern use `Random.Shared`. |

The full results are kept as HTML in [`Benchmark/Reports/Results/`](Benchmark/Reports/Results) and published
through the links above. [`Benchmark/Reports/`](Benchmark/Reports/README.md) describes how to reproduce them. The Toub suite
(`Benchmark/Toub/`) imports the LINQ benchmarks from each .NET release's performance post since .NET Core 2.0, at
the post's sizes.

## Further reading

- [History](docs/history.md): from F#'s `Seq` through Cistern.Linq to value types.
- [Architecture](docs/architecture.md): nodes, the push and pull chains, requests, and the role of the JIT.
- [Source generators](docs/generators.md): the attributes and what they write.
- [`ValueLinq/Docs/PullProtocol.md`](ValueLinq/Docs/PullProtocol.md) and
  [`ValueLinq/Docs/BatchFromLocalCopy.md`](ValueLinq/Docs/BatchFromLocalCopy.md): two contracts in detail.

## Skipped tests

Of 52,238 test cases in the imported suite, 52,101 pass, none fail, and 137 are skipped.[^tests]
Apart from nine edge cases of behaviour, the skips test type identity, APIs newer than RC2, or areas outside
Cistern's scope.

| Reason | Skipped |
|---|--:|
| Type identity: a result that cannot be cast to an `IEnumerator<T>`, is not an `IOrderedEnumerable<T>`, `IList<T>` or `Lookup`, or is not the same instance as its source or a shared singleton | 69 |
| A `System.Linq` API added after .NET 11 RC2 | 38 |
| Behaviour not reproduced: when `GetEnumerator` reaches the source, `ElementAt`'s exception type over an `IList<T>`, how far `Last` over a `Reverse` enumerates, and similar | 9 |
| Debugger display (`EnumerableDebugView`), not yet done | 6 |
| Fails against `System.Linq` on .NET 11 RC2 as well | 3 |
| `System.Linq.Queryable`, which is out of scope | 2 |
| Upstream conditions not met in an ordinary run (stress mode, debugger proxy support, and the like) | 10 |

[^tests]: Counted from `dotnet test Tests/Linqs.Tests.csproj -c Release` on .NET 11 RC2, in which each row of a
    theory is a test case; the suite has 1,889 test methods. The first six rows are the 127 tests skipped with a
    `CISTERN.VALUELINQ` reason, the last row upstream's own conditional skips. Separately, 99 passages of test code that
    cannot compile against Cistern's types are excluded under `#if !CISTERN_VALUELINQ`, and 55 compile against a
    different static type under `#if CISTERN_VALUELINQ`.

## Licence

MIT. Portions derived from dotnet/runtime; see [THIRD-PARTY-NOTICES.TXT](THIRD-PARTY-NOTICES.TXT).

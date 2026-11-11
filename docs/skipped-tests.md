# Skipped tests

[← Overview](../README.md) · [History](history.md) · [Architecture](architecture.md) ·
[Source generators](generators.md)

Of 52,238 test cases in the imported suite, 52,135 pass, none fail, and 103 are skipped.[^tests] Nine of the skips are
edge cases of behaviour that Cistern doesn't reproduce. The rest test type identity, an API that isn't in .NET 11, or
something outside Cistern's scope.

| Reason | Skipped |
|---|--:|
| Type identity: a result that cannot be cast to an `IEnumerator<T>`, is not an `IOrderedEnumerable<T>`, `IList<T>` or `Lookup`, or is not the same instance as its source or a shared singleton | 69 |
| Behaviour not reproduced: when `GetEnumerator` reaches the source, `ElementAt`'s exception type over an `IList<T>`, how far `Last` over a `Reverse` enumerates, and similar | 9 |
| Debugger display (`EnumerableDebugView`), not yet done | 6 |
| `Array.CreateFilled`, not in .NET 11 | 4 |
| Fails against `System.Linq` on .NET 11 as well | 3 |
| `System.Linq.Queryable`, which is out of scope | 2 |
| Upstream conditions not met in an ordinary run (stress mode, debugger proxy support, and the like) | 10 |

[^tests]: Counted from `dotnet test Tests/Linqs.Tests.csproj -c Release` on .NET 11.0.0, in which each row of a
    theory is a test case; the suite has 1,889 test methods. The first six rows are the 93 tests skipped with a
    `CISTERN.VALUELINQ` reason, the last row upstream's own conditional skips. Separately, 61 passages of test code that
    cannot compile against Cistern's types are excluded under `#if !CISTERN_VALUELINQ`, 55 compile against a
    different static type under `#if CISTERN_VALUELINQ`, and the 4 using `Array.CreateFilled` are under
    `#if NET12_0_OR_GREATER`.

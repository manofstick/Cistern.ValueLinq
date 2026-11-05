# Toub benchmarks

The LINQ benchmarks from Stephen Toub's performance posts, one class per release: .NET Core 2.0 and 3.0, and .NET 5
through .NET 11. Each entry is the post's query at the post's sizes and data. It is written once per library (System.Linq,
Cistern, ZLinq where its API covers it, and hand-coded), and every version's result is checked against System.Linq's.
Some entries carry extra Cistern variants, such as the source reached as its own type rather than as `IEnumerable<T>`,
or the chain built inline.

Run one entry from `Benchmark/`, with the net11 SDK on `PATH` and in `DOTNET_ROOT`:

```
bin/Release/net11.0/Cistern.Benchmarks --filter '*Toub.Net11.N01_*' --job short --iterationCount 20 --warmupCount 5
```

To run the whole suite and write it up as HTML, use [`Reports/Report.cs`](../Reports/README.md).

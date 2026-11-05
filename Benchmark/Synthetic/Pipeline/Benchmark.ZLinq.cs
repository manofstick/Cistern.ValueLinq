using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class Pipeline
    {
        [Benchmark]
        public double ZLinq() =>
            _trades
            .AsValueEnumerable()
            .Where(t => t.Qty >= 10)
            .Select(t => t.Price * t.Qty)
            .Where(notional => notional > 1000.0)
            .Skip(_skip)
            .Take(_take)
            .Aggregate(0.0, (acc, notional) => acc + notional);
    }
}

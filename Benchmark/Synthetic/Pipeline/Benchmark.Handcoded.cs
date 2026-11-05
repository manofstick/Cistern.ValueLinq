using BenchmarkDotNet.Attributes;

namespace Cistern.Benchmarks.Synthetic
{
    partial class Pipeline
    {
        [Benchmark(Baseline = true)]
        public double Handcoded()
        {
            var acc = 0.0;
            var seen = 0;
            var taken = 0;
            foreach (var t in _trades)
            {
                if (t.Qty < 10)
                    continue;

                var notional = t.Price * t.Qty;
                if (!(notional > 1000.0))
                    continue;

                if (seen++ < _skip)
                    continue;

                if (taken++ >= _take)
                    break;

                acc += notional;
            }
            return acc;
        }
    }
}

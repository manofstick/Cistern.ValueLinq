using BenchmarkDotNet.Attributes;
using NetFabric.Hyperlinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.ImmutableArray
{
#if HYPERLINQ_tmp
    partial class WhereSelectToList
    {
        [Benchmark]
        public List<double> HyperLinq() => _double.AsSpan().Where(x => x > 0).Select(x => x + 1).ToList();
    }
#endif
}

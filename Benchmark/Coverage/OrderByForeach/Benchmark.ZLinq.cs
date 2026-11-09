using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class OrderByForeach
    {
        [Benchmark]
        public long ZLinq()
        {
            var hash = 0L;
            var query =
                _data
                .AsValueEnumerable()
                .OrderBy(x => x);
            foreach (var x in query)
                hash = hash * 31 + x;
            return hash;
        }
    }
}

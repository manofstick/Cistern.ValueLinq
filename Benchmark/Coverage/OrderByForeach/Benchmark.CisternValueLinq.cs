using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class OrderByForeach
    {
        [Benchmark]
        public long CisternValueLinq()
        {
            var hash = 0L;
            var query =
                _data
                .OrderBy(x => x);
            foreach (var x in query)
                hash = hash * 31 + x;
            return hash;
        }
    }
}

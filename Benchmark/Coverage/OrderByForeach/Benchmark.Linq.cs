using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class OrderByForeach
    {
        [Benchmark]
        public long Linq()
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

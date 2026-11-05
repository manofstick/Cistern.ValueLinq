using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Coverage
{
    partial class WhereSelectForeach
    {
        [Benchmark]
        public long Linq()
        {
            var sum = 0L;
            var query =
                _data
                .Where(x => x % 3 == 0)
                .Select(x => (long)x * x);
            foreach (var x in query)
                sum += x;
            return sum;
        }
    }
}

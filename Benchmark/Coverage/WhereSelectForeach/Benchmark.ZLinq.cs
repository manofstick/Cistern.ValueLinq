using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Coverage
{
    partial class WhereSelectForeach
    {
        [Benchmark]
        public long ZLinq()
        {
            var sum = 0L;
            var query =
                _data
                .AsValueEnumerable()
                .Where(x => x % 3 == 0)
                .Select(x => (long)x * x);
            foreach (var x in query)
                sum += x;
            return sum;
        }
    }
}

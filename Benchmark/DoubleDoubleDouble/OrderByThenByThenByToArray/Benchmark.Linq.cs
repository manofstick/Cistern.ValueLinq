using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.DoubleDoubleDouble
{
    partial class OrderByThenByThenByToArray
    {
        [Benchmark(Baseline = true)]
        public (double, double, double)[] Linq()
        {
            return
                _doubledoubledoubles
                .OrderBy(x => x.x)
                .ThenBy(x => x.y)
                .ThenBy(x => x.z)
                .ToArray();
        }
    }
}

using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.DoubleDoubleDouble
{
    partial class OrderByThenByThenByToArray
    {
        [Benchmark]
        public (double, double, double)[] CisternValueLinq()
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

using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.DoubleDoubleDouble
{
#if LINQAFx
    partial class OrderByThenByThenByToArray
    {
        [Benchmark]
        public (double, double, double)[] LinqAF()
        {
            return
                _doubledoubledoubles
                .OrderBy(x => x.x)
                .ThenBy(x => x.y)
                .ThenBy(x => x.z)
                .ToArray();
        }
    }
#endif
}

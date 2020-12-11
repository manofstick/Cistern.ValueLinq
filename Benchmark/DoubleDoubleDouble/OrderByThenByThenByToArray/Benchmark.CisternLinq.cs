using BenchmarkDotNet.Attributes;
using Cistern.Linq;

namespace Cistern.Benchmarks.DoubleDoubleDouble
{
#if CISTERNLINQx
    partial class OrderByThenByThenByToArray
    {
        [Benchmark]
        public (double x, double y, double z)[] CisternLinq()
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

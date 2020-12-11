using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.DummyData
{
#if LINQAFx
    partial class OrderByToArray
    {
        [Benchmark]
        public DataSetA.Data[] LinqAF()
        {
            return
                _data
                .OrderBy(x => x.DOB)
                .ToArray();
        }
    }
#endif
}

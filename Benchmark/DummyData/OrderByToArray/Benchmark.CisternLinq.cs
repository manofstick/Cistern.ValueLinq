using BenchmarkDotNet.Attributes;
using Cistern.Linq;

namespace Cistern.Benchmarks.DummyData
{
#if CISTERNLINQx
    partial class OrderByToArray
    {
        [Benchmark]
        public DataSetA.Data[] CisternLinq()
        {
            return
                _data
                .OrderBy(x => x.DOB)
                .ToArray();
        }
    }
#endif
}

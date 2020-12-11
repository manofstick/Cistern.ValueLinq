using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.DummyData
{
    partial class OrderByToArray
    {
        [Benchmark(Baseline = true)]
        public DataSetA.Data[] Linq()
        {
            return
                _data
                .OrderBy(x => x.DOB)
                .ToArray();
        }
    }
}

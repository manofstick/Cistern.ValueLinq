using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.DummyData
{
    partial class OrderByToArray
    {
        [Benchmark]
        public DataSetA.Data[] CisternValueLinq()
        {
            return
                _data
                .OrderBy(x => x.DOB)
                .ToArray();
        }
    }
}

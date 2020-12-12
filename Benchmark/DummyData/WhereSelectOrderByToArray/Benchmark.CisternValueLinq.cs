using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.DummyData
{
    partial class WhereSelectOrderByToArray
    {
        [Benchmark]
        public string[] CisternValueLinq()
        {
            return
                _data
                .Where(x => !x.Country.StartsWith('S'))
                .Select(x => x.Name)
                .OrderBy(x => x)
                .ToArray();
        }
    }
}

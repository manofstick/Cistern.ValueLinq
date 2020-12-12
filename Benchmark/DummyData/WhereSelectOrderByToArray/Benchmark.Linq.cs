using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.DummyData
{
    partial class WhereSelectOrderByToArray
    {
        [Benchmark(Baseline = true)]
        public string[] Linq()
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

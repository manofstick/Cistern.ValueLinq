using BenchmarkDotNet.Attributes;
using LinqAF;

namespace Cistern.Benchmarks.DummyData
{
#if LINQAF
    partial class WhereSelectOrderByToArray
    {
        [Benchmark]
        public string[] LinqAF()
        {
            return
                _data
                .Where(x => !x.Country.StartsWith('S'))
                .Select(x => x.Name)
                .OrderBy(x => x)
                .ToArray();
        }
    }
#endif
}

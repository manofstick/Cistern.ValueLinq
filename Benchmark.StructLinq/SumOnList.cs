using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using StructLinq;

namespace Cistern.Benchmarks.StructLinq
{
    [MemoryDiagnoser]
    public partial class SumOnList
    {
        private readonly List<int> list;
        public SumOnList()
        {
            list = Enumerable.Range(-1, 10000).ToList();
        }

        [Benchmark(Baseline = true)]
        public int Linq()
        {
            return list.Sum();
        }

        [Benchmark]
        public int StructLinq()
        {
            return list.ToStructEnumerable().Sum(x => x);
        }
    }

}
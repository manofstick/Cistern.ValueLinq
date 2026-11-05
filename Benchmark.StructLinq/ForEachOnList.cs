using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using StructLinq;

namespace Cistern.Benchmarks.StructLinq
{
    [MemoryDiagnoser]
    public partial class ForEachOnList
    {
        private readonly List<int> list;
        public ForEachOnList()
        {
            list = Enumerable.Range(-1, 10000).ToList();
        }

        [Benchmark(Baseline = true)]
        public int Default()
        {
            var sum = 0;
            foreach (var i in list)
            {
                sum += i;
            }

            return sum;
        }

        [Benchmark]
        public int StructLinq()
        {
            var sum = 0;
            foreach (var i in list.ToStructEnumerable())
            {
                sum += i;
            }
            return sum;
        }

    }
}
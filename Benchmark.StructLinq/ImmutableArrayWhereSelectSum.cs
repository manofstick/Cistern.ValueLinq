using System.Collections.Immutable;
using System.Linq;
using BenchmarkDotNet.Attributes;
using StructLinq;

namespace Cistern.Benchmarks.StructLinq
{
    [MemoryDiagnoser]
    public partial class ImmutableArrayWhereSelectSum
    {
        private const int Count = 10000;
        private readonly ImmutableArray<int> array;

        public ImmutableArrayWhereSelectSum()
        {
            array = Enumerable.Range(0, Count).ToImmutableArray();
        }

       
        [Benchmark(Baseline = true)]
        public int LINQ() => array
            .Where(x => (x & 1) == 0)
            .Select(x => x * 2)
            .Sum();

        [Benchmark]
        public int StructLinq() => array
            .ToStructEnumerable()
            .Where(x => (x & 1) == 0, x => x)
            .Select(x => x * 2, x => x)
            .Sum(x=>x);

        [Benchmark]
        public int StructLinqFunction()
        {
            var where = new WherePredicate();
            var select = new SelectFunction();
            return array
                .ToStructEnumerable()
                .Where(ref @where, x => x)
                .Select(ref @select, x => x, x => x)
                .Sum(x => x);
        }
    }
}
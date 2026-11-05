using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Synthetic
{
    partial class ForeachChains
    {
        [BenchmarkCategory("ArraySelect"), Benchmark]
        public long CisternValueLinq_ArraySelect()
        {
            var sum = 0L;
            foreach (var x in _array.Select(x => (long)x * x))
                sum += x;
            return sum;
        }

        [BenchmarkCategory("ArrayWhereSelect"), Benchmark]
        public long CisternValueLinq_ArrayWhereSelect()
        {
            var sum = 0L;
            foreach (var x in _array.Where(x => x % 3 == 0).Select(x => (long)x * x))
                sum += x;
            return sum;
        }

        [BenchmarkCategory("ListWhereSelect"), Benchmark]
        public long CisternValueLinq_ListWhereSelect()
        {
            var sum = 0L;
            foreach (var x in _list.Where(x => x % 3 == 0).Select(x => (long)x * x))
                sum += x;
            return sum;
        }

        [BenchmarkCategory("ArraySkipTake"), Benchmark]
        public long CisternValueLinq_ArraySkipTake()
        {
            var sum = 0L;
            foreach (var x in _array.Skip(Length / 4).Take(Length / 2))
                sum += x;
            return sum;
        }

        [BenchmarkCategory("EnumerableWhereSelect"), Benchmark]
        public long CisternValueLinq_EnumerableWhereSelect()
        {
            var sum = 0L;
            foreach (var x in _enumerable.Where(x => x % 3 == 0).Select(x => (long)x * x))
                sum += x;
            return sum;
        }

        [BenchmarkCategory("EnumerableWhereSelectAsEnumerable"), Benchmark]
        public long CisternValueLinq_EnumerableWhereSelectAsEnumerable()
        {
            var sum = 0L;
            IEnumerable<long> query = _enumerable.Where(x => x % 3 == 0).Select(x => (long)x * x);
            foreach (var x in query)
                sum += x;
            return sum;
        }
    }
}

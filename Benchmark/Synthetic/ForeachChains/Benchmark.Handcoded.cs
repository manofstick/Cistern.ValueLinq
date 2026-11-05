using BenchmarkDotNet.Attributes;
using System;

namespace Cistern.Benchmarks.Synthetic
{
    partial class ForeachChains
    {
        [BenchmarkCategory("ArraySelect"), Benchmark(Baseline = true)]
        public long Handcoded_ArraySelect()
        {
            var sum = 0L;
            foreach (var x in _array)
                sum += (long)x * x;
            return sum;
        }

        [BenchmarkCategory("ArrayWhereSelect"), Benchmark(Baseline = true)]
        public long Handcoded_ArrayWhereSelect() => WhereSelect(_array);

        [BenchmarkCategory("ListWhereSelect"), Benchmark(Baseline = true)]
        public long Handcoded_ListWhereSelect()
        {
            var sum = 0L;
            foreach (var x in _list)
            {
                if (x % 3 == 0)
                    sum += (long)x * x;
            }
            return sum;
        }

        [BenchmarkCategory("ArraySkipTake"), Benchmark(Baseline = true)]
        public long Handcoded_ArraySkipTake()
        {
            var sum = 0L;
            var start = Length / 4;
            var end = Math.Min(_array.Length, start + Length / 2);
            for (var i = start; i < end; ++i)
                sum += _array[i];
            return sum;
        }

        // the source is an array held as IEnumerable<int>: the loop a programmer would write over the array in hand
        [BenchmarkCategory("EnumerableWhereSelect"), Benchmark(Baseline = true)]
        public long Handcoded_EnumerableWhereSelect() => WhereSelect(_array);

        [BenchmarkCategory("EnumerableWhereSelectAsEnumerable"), Benchmark(Baseline = true)]
        public long Handcoded_EnumerableWhereSelectAsEnumerable() => WhereSelect(_array);

        private static long WhereSelect(ReadOnlySpan<int> source)
        {
            var sum = 0L;
            foreach (var x in source)
            {
                if (x % 3 == 0)
                    sum += (long)x * x;
            }
            return sum;
        }
    }
}

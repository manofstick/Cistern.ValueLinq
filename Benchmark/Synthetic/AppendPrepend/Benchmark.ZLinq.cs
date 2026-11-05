using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class AppendPrepend
    {
        [Benchmark, BenchmarkCategory("Sum")]
        public int ZLinq_Sum() =>
            _data
            .AsValueEnumerable()
            .Prepend(Prepended)
            .Append(Appended)
            .Sum();

        [Benchmark, BenchmarkCategory("Foreach")]
        public int ZLinq_Foreach()
        {
            var sum = 0;
            foreach (var x in _data.AsValueEnumerable().Prepend(Prepended).Append(Appended))
                sum += x;
            return sum;
        }

        [Benchmark, BenchmarkCategory("ToArray")]
        public int[] ZLinq_ToArray() =>
            _data
            .AsValueEnumerable()
            .Prepend(Prepended)
            .Append(Appended)
            .ToArray();

        [Benchmark, BenchmarkCategory("ReverseSum")]
        public int ZLinq_ReverseSum() =>
            _data
            .AsValueEnumerable()
            .Prepend(Prepended)
            .Append(Appended)
            .Reverse()
            .Sum();

        [Benchmark, BenchmarkCategory("ReverseToArray")]
        public int[] ZLinq_ReverseToArray() =>
            _data
            .AsValueEnumerable()
            .Prepend(Prepended)
            .Append(Appended)
            .Reverse()
            .ToArray();

        [Benchmark, BenchmarkCategory("ToList")]
        public List<int> ZLinq_ToList() =>
            _data
            .AsValueEnumerable()
            .Prepend(Prepended)
            .Append(Appended)
            .ToList();
    }
}

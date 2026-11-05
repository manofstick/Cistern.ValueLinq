using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class AppendPrepend
    {
        [Benchmark, BenchmarkCategory("Sum")]
        public int Linq_Sum() =>
            _data
            .Prepend(Prepended)
            .Append(Appended)
            .Sum();

        [Benchmark, BenchmarkCategory("Foreach")]
        public int Linq_Foreach()
        {
            var sum = 0;
            foreach (var x in _data.Prepend(Prepended).Append(Appended))
                sum += x;
            return sum;
        }

        [Benchmark, BenchmarkCategory("ToArray")]
        public int[] Linq_ToArray() =>
            _data
            .Prepend(Prepended)
            .Append(Appended)
            .ToArray();

        [Benchmark, BenchmarkCategory("ReverseSum")]
        public int Linq_ReverseSum() =>
            _data
            .Prepend(Prepended)
            .Append(Appended)
            .Reverse()
            .Sum();

        [Benchmark, BenchmarkCategory("ReverseToArray")]
        public int[] Linq_ReverseToArray() =>
            _data
            .Prepend(Prepended)
            .Append(Appended)
            .Reverse()
            .ToArray();

        [Benchmark, BenchmarkCategory("ToList")]
        public List<int> Linq_ToList() =>
            _data
            .Prepend(Prepended)
            .Append(Appended)
            .ToList();
    }
}

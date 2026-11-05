using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Synthetic
{
    partial class AppendPrepend
    {
        [Benchmark, BenchmarkCategory("Sum")]
        public int CisternValueLinq_Sum() =>
            _data
            .Prepend(Prepended)
            .Append(Appended)
            .Sum();

        [Benchmark, BenchmarkCategory("Foreach")]
        public int CisternValueLinq_Foreach()
        {
            var sum = 0;
            foreach (var x in _data.Prepend(Prepended).Append(Appended))
                sum += x;
            return sum;
        }

        [Benchmark, BenchmarkCategory("ToArray")]
        public int[] CisternValueLinq_ToArray() =>
            _data
            .Prepend(Prepended)
            .Append(Appended)
            .ToArray();

        [Benchmark, BenchmarkCategory("ReverseSum")]
        public int CisternValueLinq_ReverseSum() =>
            _data
            .Prepend(Prepended)
            .Append(Appended)
            .Reverse()
            .Sum();

        [Benchmark, BenchmarkCategory("ReverseToArray")]
        public int[] CisternValueLinq_ReverseToArray() =>
            _data
            .Prepend(Prepended)
            .Append(Appended)
            .Reverse()
            .ToArray();

        [Benchmark, BenchmarkCategory("ToList")]
        public List<int> CisternValueLinq_ToList() =>
            _data
            .Prepend(Prepended)
            .Append(Appended)
            .ToList();
    }
}

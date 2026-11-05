using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Synthetic
{
    partial class AppendPrepend
    {
        [Benchmark(Baseline = true), BenchmarkCategory("Sum")]
        public int Handcoded_Sum()
        {
            var sum = Prepended;
            foreach (var x in _data)
                sum += x;
            return sum + Appended;
        }

        [Benchmark(Baseline = true), BenchmarkCategory("Foreach")]
        public int Handcoded_Foreach()
        {
            var sum = Prepended;
            foreach (var x in _data)
                sum += x;
            return sum + Appended;
        }

        [Benchmark(Baseline = true), BenchmarkCategory("ToArray")]
        public int[] Handcoded_ToArray()
        {
            var result = new int[_data.Length + 2];
            result[0] = Prepended;
            Array.Copy(_data, 0, result, 1, _data.Length);
            result[^1] = Appended;
            return result;
        }

        [Benchmark(Baseline = true), BenchmarkCategory("ReverseSum")]
        public int Handcoded_ReverseSum()
        {
            var sum = Appended;
            for (var i = _data.Length - 1; i >= 0; --i)
                sum += _data[i];
            return sum + Prepended;
        }

        [Benchmark(Baseline = true), BenchmarkCategory("ReverseToArray")]
        public int[] Handcoded_ReverseToArray()
        {
            var result = new int[_data.Length + 2];
            result[0] = Appended;
            for (var i = 0; i < _data.Length; ++i)
                result[i + 1] = _data[_data.Length - 1 - i];
            result[^1] = Prepended;
            return result;
        }

        [Benchmark(Baseline = true), BenchmarkCategory("ToList")]
        public List<int> Handcoded_ToList()
        {
            var result = new List<int>(_data.Length + 2) { Prepended };
            result.AddRange(_data);
            result.Add(Appended);
            return result;
        }
    }
}

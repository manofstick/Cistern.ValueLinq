﻿using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Double.Sum
{
    partial class Benchmark
    {
        [Benchmark(Baseline = true)]
        public double Linq() => _double.Sum();

        [Benchmark]
        public double Linq_Foreach()
        {
            var total = 0.0;
            foreach (var item in _double)
                total += item;
            return total;
        }
    }
}
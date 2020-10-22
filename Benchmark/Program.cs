﻿using BenchmarkDotNet.Running;
using System;

namespace Cistern.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // For some sanity checking
            DoubleDoubleDouble.SelectWhereAggregate.Benchmark.SanityCheck();
            DoubleDoubleDouble.WhereSelectAggregate.Benchmark.SanityCheck();
            Double.ToList.Benchmark.SanityCheck();
            Double.SelectToList.Benchmark.SanityCheck();
            Double.SelectSelectToList.Benchmark.SanityCheck();
            Double.WhereToList.Benchmark.SanityCheck();
            Double.SelectWhereToList.Benchmark.SanityCheck();
            Double.WhereSelectToList.Benchmark.SanityCheck();
            Double.WhereSelectIToList.Benchmark.SanityCheck();
            Double.WhereWhereToList.Benchmark.SanityCheck();
            Double.Sum.Benchmark.SanityCheck();
            Double.SelectSum.Benchmark.SanityCheck();

            var summary = BenchmarkRunner.Run<Double.Sum.Benchmark>();
        }
    }
}